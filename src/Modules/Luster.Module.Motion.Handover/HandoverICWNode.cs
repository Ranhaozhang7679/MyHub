#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       HandoverICWNode
* 文 件 名:       HandoverICWNode.cs
* 创建时间:       2026/06/22
* 作    者:       全栈工程师
* 版    权:       Luster Technology Co.,Ltd.
* 说    明:       TES-37-4 / TES-47 ICW 交握节点(Server 侧,Req/Resp 寄存器对)
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.VDevice;
using Luster.TaskFlow.Common.Attributes;
using System;
using System.ComponentModel;

namespace Luster.Module.Motion.Handover
{
    /// <summary>
    /// ICW 交握节点:作 Server 侧,移植源端
    /// <c>SP-2025140\Plugin.CommonPlugin\Handover\HandoverICWModbusTcpServer.cs</c>
    /// + <c>Task\Machine\CheckStationTask.cs:1036-1200</c> 的 ICW 入料/检测 Req/Resp 路径。
    /// <para>ADR-TES-37 决策 D2 / §3.2:角色 = ICW(Server),对应源端 <c>HandoverICWModbusTcpServer</c>。
    /// 与 Feed/Leave 的逐位握手 <b>根本不同</b>:ICW 走 Request/Response 寄存器对 +
    /// <see cref="ICWSyncResultCodeType"/> 枚举驱动(非逐位),故本节点不复用基类 15+13 步位握手状态机,
    /// 而是重写 <see cref="DoExcute"/> 驱动 Req/Resp 步进机。</para>
    /// <para>ADR-TES-37 决策 D1/D5:Server 侧经 <see cref="VModbusServer"/> 虚拟设备承载(TES-45 已落地),
    /// 本节点经 <c>GetVDevice&lt;VModbusServer&gt;</c> 引用,不再自持 Hsl Server 实例。</para>
    /// <para>ADR-TES-37 决策 D3:实际读写走字符串地址(对齐源端 <c>CommWriteUshort/CommReadUshort</c>
    /// 字符串地址重载 + <c>GetActualAddress</c> 运行路径)。</para>
    /// <para><b>不侵入红线</b>:不改 <c>MotionFunction</c> / 既有节点契约,只新增。</para>
    /// <para><b>已知集成 seam(TES-37-7 处置)</b>:基类 <see cref="HandoverNode.CommDevice"/>
    /// 带 <c>[NotEmpty]</c> 校验且为 <c>VCommuncation</c> 编辑器,ICW 走 Server 侧(VModbusServer)不使用它;
    /// 工程配置时需给 <c>CommDevice</c> 占位或由 TES-37-7 集成层放宽该校验,不影响本节点 Req/Resp 运行逻辑。</para>
    /// </summary>
    public class HandoverICWNode : HandoverNode
    {
        #region 步号常量(对齐源端 ICW_LoadStart / ICW_LoadEnd / ICW_CheckStart / ICW_CheckEnd 路径)

        /// <summary>清响应 + 写配方/模式(对齐源端 ClearStatus + WriteProductInfo)</summary>
        private const int StepClearAndWriteInfo = 0;

        /// <summary>写请求位 Request=1(对齐源端 WriteStartStatus)</summary>
        private const int StepWriteStart = 1;

        /// <summary>轮询响应位 Response(对齐源端 ReadEndStatus 轮询循环)</summary>
        private const int StepWaitResponse = 2;

        /// <summary>收尾:清响应(对齐源端流程结束清零)</summary>
        private const int StepFinishClear = 98;

        /// <summary>→ StepDone(对齐源端 break outer return true)</summary>
        private const int StepToDone = 99;

        #endregion

        #region 参数

        /// <summary>
        /// ICW Server 虚拟设备(持 Hsl <see cref="HslCommunication.ModBus.ModbusTcpServer"/> + DataPool)。
        /// <para>ADR-TES-37 §3.2:经 <c>GetVDevice&lt;VModbusServer&gt;</c> 引用 TES-45 落地的 VModbusServer。</para>
        /// </summary>
        [NotEmpty]
        [Parameter("ICW服务设备", 10, CN = "ICW服务设备", EditorType = typeof(VModbusServer))]
        public VDevice IcwServerDevice { get; set; }

        /// <summary>
        /// ICW Req/Resp 寄存器对地址表 + 心跳 + 配方/模式轮询地址。
        /// </summary>
        [Parameter("ICW地址表", 11, CN = "ICW地址表")]
        public IcwHandoverAddress IcwAddress { get; set; } = new IcwHandoverAddress();

        /// <summary>配方/测试模式值(对齐源端 测试模式 寄存器写入值;留空地址则跳过)</summary>
        [Parameter("配方值", 12, CN = "配方值", DefaultV = (ushort)0)]
        public ushort RecipeValue { get; set; } = 0;

        /// <summary>模式值(对齐源端 扫描数量 寄存器写入值;留空地址则跳过)</summary>
        [Parameter("模式值", 13, CN = "模式值", DefaultV = (ushort)0)]
        public ushort ModeValue { get; set; } = 0;

        /// <summary>响应轮询超时(ms,对齐源端 SysProfile.ICWSetting.ReadTimeOut)</summary>
        [Parameter("响应超时(ms)", 14, CN = "响应超时", DefaultV = 30000)]
        public int ResponseTimeout { get; set; } = 30000;

        /// <summary>轮询间隔(ms,对齐源端 SysProfile.ICWSetting.ReadFrequency)</summary>
        [Parameter("轮询间隔(ms)", 15, CN = "轮询间隔", DefaultV = 200)]
        public int PollInterval { get; set; } = 200;

        /// <summary>心跳写入周期(ms,对齐源端 socketProfile.BeatTime;0=不写心跳)</summary>
        [Parameter("心跳周期(ms)", 16, CN = "心跳周期", DefaultV = 3000)]
        public int HeartbeatInterval { get; set; } = 3000;

        #endregion

        /// <summary>
        /// 构造函数:固定角色为 ICW。
        /// </summary>
        public HandoverICWNode()
        {
            this.Role = HandoverRole.ICW;
            this.Tips = "ICW 交握(Server,Req/Resp 寄存器对 + ICWSyncResultCodeType)";
            this.Icon = "\xe692";
        }

        #region 子类契约

        /// <summary>
        /// ICW 步进状态机:不使用基类 15+13 步位握手,改用 Req/Resp 步进。
        /// <para>本方法仅供 <see cref="DoExcute"/> 内部驱动,不对外暴露(基类抽象为 protected)。</para>
        /// </summary>
        protected override int StepMachine(int step)
        {
            var addr = IcwAddress;
            switch (step)
            {
                case StepClearAndWriteInfo:
                    // 源端 :1043 ClearStatus(ResponseAddress) + :1048 WriteProductInfo(配方/模式)
                    StepInfo(step, "[ICW] 清响应寄存器 + 写配方/模式");
                    if (!ClearResponse()) { SyncWait(WaitSleep); break; }
                    WriteRecipeAndMode();
                    step++;
                    break;

                case StepWriteStart:
                    // 源端 :1053 WriteStartStatus(RequestAddress=1)
                    StepInfo(step, "[ICW] 写请求位 Request=1");
                    if (!WriteStart()) { SyncWait(WaitSleep); break; }
                    _responseStart = DateTimeNow();
                    step++;
                    break;

                case StepWaitResponse:
                    // 源端 :1083-1112 ICW_LoadEnd 轮询 ReadEndStatus
                    WriteHeartbeatIfDue();

                    if (!ReadEndStatus(out var valid))
                    {
                        // 读失败(源端 :1090 WarningPause 后 break inner → valid=None → outer continue 重试)
                        WarningPause("[ICW] 从 ICW 读取响应失败,重试");
                        step = StepClearAndWriteInfo;
                        break;
                    }

                    if (valid == ICWSyncResultCodeType.Success)
                    {
                        StepInfo(step, "[ICW] 响应 Success");
                        step = StepFinishClear;
                        break;
                    }

                    if (valid == ICWSyncResultCodeType.None)
                    {
                        // 未完成:超时则重试(源端 :1104 timeout → valid 仍 None → outer continue),
                        // 否则按轮询间隔等待
                        if ((DateTimeNow() - _responseStart).TotalMilliseconds > ResponseTimeout)
                        {
                            WarningPause("[ICW] 读取响应超时,重试");
                            step = StepClearAndWriteInfo;
                        }
                        else
                        {
                            SyncWait(PollInterval);
                        }
                        break;
                    }

                    // 非 None / 非 Success 的异常码
                    if (valid == ICWSyncResultCodeType.检测软件异常)
                    {
                        // 源端 :1111 检测软件异常 → outer continue(重试整轮)
                        WarningPause(valid.ToString());
                        step = StepClearAndWriteInfo;
                    }
                    else
                    {
                        // 源端 :1111 其余异常码 → outer break(退出,return true)
                        WarningPause(valid.ToString());
                        step = StepFinishClear;
                    }
                    break;

                case StepFinishClear:
                    // 收尾清响应(对齐源端流程结束清零)
                    StepInfo(step, "[ICW] 收尾清响应");
                    ClearResponse();
                    step = StepToDone;
                    break;

                case StepToDone:
                    step = HandoverNode.StepDone;
                    break;

                default:
                    MyOwner.OnLog(LogType.Error, $"HandoverICWNode 步骤异常 Step={step}");
                    return HandoverNode.StepAbort;
            }

            return step;
        }

        /// <summary>
        /// 清零 ICW 响应寄存器(对齐源端 ClearStatus(ResponseAddress)=0)。
        /// </summary>
        protected override void ClearSignals()
        {
            ClearResponse();
        }

        #endregion

        #region ICW 寄存器读写(对齐源端 CommWriteUshort/CommReadUshort 字符串地址重载)

        /// <summary>响应轮询起始时间(非序列化)</summary>
        [Ignore] private DateTime _responseStart;

        /// <summary>上次心跳写入时间(非序列化)</summary>
        [Ignore] private DateTime _lastHeartbeat;

        /// <summary>
        /// 写单个保持寄存器(对齐源端 <c>HandoverICWModbusTcpServer.CommWriteUshort(string, ushort)</c>)。
        /// <para>默认经 <see cref="VModbusServer"/> 读写;单测用子类重写注入寄存器字典模拟。</para>
        /// </summary>
        /// <param name="address">字符串寄存器地址</param>
        /// <param name="value">ushort 值</param>
        /// <returns>是否写入成功</returns>
        protected virtual bool WriteIcwUshort(string address, ushort value)
        {
            if (Disable) return true;
            if (string.IsNullOrWhiteSpace(address)) return false;

            if (!TryGetServer(out var server)) return false;
            try
            {
                server.WriteRegister(address, value);
                return true;
            }
            catch (Exception ex)
            {
                MyOwner.OnLog(LogType.Error, $"[ICW] 写寄存器 {address}={value} 异常:{ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 读单个保持寄存器(对齐源端 <c>HandoverICWModbusTcpServer.CommReadUshort(string, out ushort)</c>)。
        /// </summary>
        /// <param name="address">字符串寄存器地址</param>
        /// <param name="value">读出的 ushort 值</param>
        /// <returns>是否读取成功</returns>
        protected virtual bool ReadIcwUshort(string address, out ushort value)
        {
            value = 0;
            if (Disable) return true;
            if (string.IsNullOrWhiteSpace(address)) return false;

            if (!TryGetServer(out var server)) return false;
            try
            {
                value = server.ReadRegister(address);
                return true;
            }
            catch (Exception ex)
            {
                MyOwner.OnLog(LogType.Error, $"[ICW] 读寄存器 {address} 异常:{ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取 VModbusServer 实例(经 <c>GetVDevice&lt;VModbusServer&gt;</c>,对齐 ADR §3.2)。
        /// </summary>
        protected bool TryGetServer(out VModbusServer server)
        {
            GetVDevice<VModbusServer>(IcwServerDevice, out server);
            if (server == null)
            {
                OnAlarm(AlarmType.FailError, $"ICW 服务设备未配置:{IcwServerDevice?.Name}");
                return false;
            }
            return true;
        }

        /// <summary>清响应寄存器(Response=0,对齐源端 ClearStatus)</summary>
        protected virtual bool ClearResponse()
        {
            return WriteIcwUshort(IcwAddress?.ResponseAddress, 0);
        }

        /// <summary>写请求位(Request=1,对齐源端 WriteStartStatus)</summary>
        protected virtual bool WriteStart()
        {
            return WriteIcwUshort(IcwAddress?.RequestAddress, 1);
        }

        /// <summary>
        /// 写配方/模式(对齐源端 ICW 通讯配置 CSV:测试模式/扫描数量 寄存器)。
        /// <para>地址留空则跳过(可选写)。</para>
        /// </summary>
        protected virtual void WriteRecipeAndMode()
        {
            if (!string.IsNullOrWhiteSpace(IcwAddress?.RecipeAddress))
            {
                WriteIcwUshort(IcwAddress.RecipeAddress, RecipeValue);
            }
            if (!string.IsNullOrWhiteSpace(IcwAddress?.ModeAddress))
            {
                WriteIcwUshort(IcwAddress.ModeAddress, ModeValue);
            }
        }

        /// <summary>
        /// 写心跳(对齐源端 <c>HandoverICWModbusTcpServer.WriteHeartBeat</c>:写 0 到 Heart 地址)。
        /// <para>按 <see cref="HeartbeatInterval"/> 周期写入;周期为 0 或地址空则不写。</para>
        /// </summary>
        protected virtual bool WriteHeartbeat()
        {
            return WriteIcwUshort(IcwAddress?.HeartbeatAddress, 0);
        }

        /// <summary>按周期写心跳(轮询间隙调用,对齐源端定时器行为)</summary>
        private void WriteHeartbeatIfDue()
        {
            if (HeartbeatInterval <= 0 || string.IsNullOrWhiteSpace(IcwAddress?.HeartbeatAddress)) return;
            if ((DateTimeNow() - _lastHeartbeat).TotalMilliseconds >= HeartbeatInterval)
            {
                _lastHeartbeat = DateTimeNow();
                WriteHeartbeat();
            }
        }

        /// <summary>
        /// 读响应码(对齐源端 <c>CheckStationTask.ReadEndStatus</c>:读 Response 寄存器,强转枚举)。
        /// </summary>
        /// <param name="valid">读出的 ICW 同步结果码</param>
        /// <returns>是否读取成功(失败时 valid=None)</returns>
        protected virtual bool ReadEndStatus(out ICWSyncResultCodeType valid)
        {
            valid = ICWSyncResultCodeType.None;
            if (!ReadIcwUshort(IcwAddress?.ResponseAddress, out ushort raw)) return false;
            valid = (ICWSyncResultCodeType)raw;
            return true;
        }

        #endregion

        #region DoExcute 驱动(ICW 不用基类 15+13 步位握手自检,重写驱动 Req/Resp)

        /// <summary>
        /// 驱动 ICW Req/Resp 状态机:循环调用 <see cref="StepMachine"/> 直到完成或中止。
        /// <para>重写基类:ICW 不使用 <see cref="HandoverNode.Address"/> 位握手地址表,
        /// 改用 <see cref="IcwAddress"/> Req/Resp 寄存器对自检。</para>
        /// </summary>
        public override bool DoExcute(out string errMsg)
        {
            errMsg = string.Empty;

            // 启动前自检:ICW Req/Resp 寄存器对须配置 + Server 设备须就位
            if (IcwAddress == null || !IcwAddress.IsConfigured)
            {
                errMsg = "ICW Req/Resp 寄存器对未完整配置(Request/Response 缺一不可)";
                OnAlarm(AlarmType.FailError, errMsg);
                return false;
            }
            if (IcwServerDevice == null)
            {
                errMsg = "ICW 服务设备未配置";
                OnAlarm(AlarmType.FailError, errMsg);
                return false;
            }

            CurrentStep = 0;
            _responseStart = DateTimeNow();
            _lastHeartbeat = DateTimeNow();

            try
            {
                // 对齐源端 while (mStation.CheckAccessError())
                while (CheckAccessError())
                {
                    CurrentStep = StepMachine(CurrentStep);

                    if (CurrentStep == HandoverNode.StepDone)
                    {
                        break;
                    }

                    // 中止
                    if (CurrentStep == HandoverNode.StepAbort)
                    {
                        errMsg = $"ICW 交握中止 STEP={CurrentStep}";
                        ClearSignals();
                        return false;
                    }
                }

                // 对齐源端 if (!mStation.CheckAccessWithPause()) return false;
                if (!CheckAccessWithPause())
                {
                    errMsg = "ICW 交握被暂停或中止";
                    return false;
                }

                if (CurrentStep != HandoverNode.StepDone)
                {
                    errMsg = $"ICW 交握未完成,步骤异常 STEP={CurrentStep}";
                    ClearSignals();
                    MyOwner.OnLog(LogType.Error, $"HandoverICWNode 步骤异常 Step={CurrentStep}");
                    return false;
                }

                // ICW 走 Req/Resp 寄存器对(用 IcwAddress),不复用基类 15+13 步位握手
                // 状态机(用 Address)。此处 StepDone 后直接返回成功,不委托 base.DoExcute ——
                // 否则会重入 HandoverNode.DoExcute 的 Address.IsConfigured 自检(8 位握手地址,
                // ICW 不配置 → 永远 return false,致 Req/Resp 主路径不可用)。
                // ICW 的启动自检已在本方法开头对 IcwAddress + IcwServerDevice 完成。
                return true;
            }
            catch (Exception ex)
            {
                errMsg = $"ICW 交握状态机异常:{ex.Message}";
                MyOwner.OnLog(LogType.Error, errMsg);
                ClearSignals();
                return false;
            }
        }

        #endregion

        #region 辅助

        /// <summary>
        /// 当前时间(对齐源端 <c>DateTime.Now</c>;提为虚方法便于单测注入固定时钟)。
        /// </summary>
        protected virtual DateTime DateTimeNow() => DateTime.Now;

        #endregion
    }
}
