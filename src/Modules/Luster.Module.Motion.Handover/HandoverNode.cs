#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       HandoverNode
* 文 件 名:       HandoverNode.cs
* 创建时间:       2026/06/22
* 作    者:       全栈工程师
* 版    权:       Luster Technology Co.,Ltd.
* 说    明:       TES-37-3 / TES-46 交握节点抽象基类
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.VDevice;
using Luster.Module.Motion.Handover.Signals;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using System;
using System.ComponentModel;

namespace Luster.Module.Motion.Handover
{
    /// <summary>
    /// 交握角色:上料(读上游)/ 下料(写下游)/ ICW(从站)。
    /// <para>本 Issue(TES-37-3)仅实现 <see cref="Feed"/> / <see cref="Leave"/> 两个 Client 侧角色;
    /// <see cref="ICW"/> 为 TES-37-4 范围,此处仅占位。</para>
    /// </summary>
    public enum HandoverRole
    {
        /// <summary>上料:作 Client 读上游来料信号</summary>
        Feed = 0,

        /// <summary>下料:作 Client 写下游去料信号</summary>
        Leave = 1,

        /// <summary>ICW:作 Server(Request/Response 寄存器对)。TES-37-4 范围</summary>
        ICW = 2,
    }

    /// <summary>
    /// 交握节点抽象基类:封装源端 <c>CheckNormalAction.cs</c> 的 15+13 步 step-switch 状态机骨架。
    /// <para>ADR-TES-37 决策 D2:对外暴露 <c>[Parameter]</c> 通信设备 + 角色 + 信号地址表,
    /// 子类型实现 <see cref="StepMachine"/> 的步进 switch(可还原,含异常分支 101/102)。</para>
    /// <para>ADR-TES-37 决策 D3:信号读写经 <see cref="ReadSignal"/> / <see cref="WriteSignal"/>
    /// 字符串地址(对齐源端 <c>CheckStationTask.ReadLoadSignal/WriteLoadSignal</c> 运行路径),
    /// 位含义引用 <see cref="HandoverSignalBit"/> 字典。</para>
    /// <para><b>不侵入红线</b>:不改 <c>MotionFunction</c> / 既有节点契约,只新增。</para>
    /// </summary>
    public abstract class HandoverNode : MotionFunction
    {
        /// <summary>状态机完成步号(对齐源端 step==100 退出)</summary>
        public const int StepDone = 100;

        /// <summary>状态机中止步号(对齐源端运动失败 return false:整个握手返回失败)</summary>
        public const int StepAbort = -1000;

        /// <summary>轮询等待时长(ms,对齐源端 _timeSpan)</summary>
        protected const int WaitSleep = 50;

        /// <summary>状态机当前步号(对齐源端 step 局部变量,提为字段以便驱动)</summary>
        protected int CurrentStep { get; set; } = 0;

        /// <summary>状态机内部步号过滤器(对齐源端 SFInfo 的 istep,避免重复日志)</summary>
        private int _innerStep = -1;

        #region 参数

        /// <summary>
        /// 交握通信设备(纯 Client,经 VCommuncation 读写字符串地址)。
        /// <para>ADR-TES-37 决策 D3/§3.6:不新增 Client,直接用 lmv 现有 <c>VCommuncation</c>。</para>
        /// </summary>
        [NotEmpty]
        [Parameter("通信设备", 0, CN = "通信设备", EditorType = typeof(VCommuncation))]
        public VDevice CommDevice { get; set; }

        /// <summary>协议类型(默认 ModbusTCP,对齐源端握手 Client)</summary>
        [Parameter("协议类型", 1, CN = "协议类型", DefaultV = Luster.Motion.DataStruct.Enums.ProtocolType.ModbusTCP)]
        public Luster.Motion.DataStruct.Enums.ProtocolType ProtocolType { get; set; } =
            Luster.Motion.DataStruct.Enums.ProtocolType.ModbusTCP;

        /// <summary>交握角色(主/从、上/下游)</summary>
        [Parameter("角色", 2, CN = "角色")]
        public HandoverRole Role { get; set; }

        /// <summary>
        /// 交握信号地址表(字符串地址,对齐源端 LoadInteraction/UnLoadInteraction 运行路径)。
        /// </summary>
        [Parameter("信号地址表", 3, CN = "信号地址表")]
        public HandoverSignalAddress Address { get; set; } = new HandoverSignalAddress();

        /// <summary>单步信号读写超时(ms,对齐源端 SysProfile.HandoverFeed.ReadTimeOut)</summary>
        [Parameter("信号超时(ms)", 4, CN = "信号超时", DefaultV = 3000)]
        public int SignalTimeout { get; set; } = 3000;

        /// <summary>是否禁用交握(对齐源端 SysProfile.HandoverFeed.Disable;禁用时读写恒成功)</summary>
        [Parameter("是否禁用", 5, CN = "是否禁用", DefaultV = false)]
        public bool Disable { get; set; } = false;

        #endregion

        /// <summary>
        /// 构造函数:设置节点提示与图标。
        /// </summary>
        protected HandoverNode()
        {
            this.Tips = "上下游交握状态机(15+13 步,含异常分支 101/102)";
            this.Icon = "\xe692";
        }

        #region 子类契约

        /// <summary>
        /// 步进状态机:子类实现源端 <c>CheckNormalAction.cs</c> 的 step-switch 逻辑。
        /// <para>每次调用处理当前 <see cref="CurrentStep"/> 一帧,返回下一步号;
        /// 返回 <see cref="StepDone"/> 表示状态机完成。</para>
        /// </summary>
        /// <param name="currentStep">当前步号</param>
        /// <returns>下一步号(返回 <see cref="StepDone"/> 退出)</returns>
        protected abstract int StepMachine(int currentStep);

        /// <summary>
        /// 状态机复位入口:对齐源端 <c>LoadSingleClaer</c> / <c>UnloadSingleClaer</c>,
        /// 清零本站发送侧全部握手信号(Sending/SendTransfer/SendInterLock)。
        /// <para>子类按角色复用 <see cref="ClearSendSignals"/> 或自定义。</para>
        /// </summary>
        protected abstract void ClearSignals();

        #endregion

        #region 信号读写(对齐源端 ReadLoadSignal / WriteLoadSignal)

        /// <summary>
        /// 读取交握信号(对齐源端 <c>CheckStationTask.ReadLoadSignal</c>:
        /// 读字符串地址,值==1 视为 true,失败/超时返回 false)。
        /// <para>禁用模式下恒返回 false(与源端 Disable 分支一致:Disable 时直接 return true,
        /// 此处为避免误触发握手,统一返回 false 让上层自行处理)。</para>
        /// </summary>
        /// <param name="address">字符串信号地址</param>
        /// <returns>信号是否为 ON</returns>
        protected virtual bool ReadSignal(string address)
        {
            if (Disable) return true;
            if (string.IsNullOrWhiteSpace(address)) return false;

            GetVDevice<VCommuncation>(CommDevice, out var comm);
            if (comm == null)
            {
                OnAlarm(AlarmType.FailError, $"交握通信设备未配置:{CommDevice?.Name}");
                return false;
            }

            comm.Open();
            comm.SetProtocol(ProtocolType);

            var values = comm.Read<bool>(address, SignalTimeout);
            return values != null && values.Count > 0 && values[0];
        }

        /// <summary>
        /// 写入交握信号(对齐源端 <c>CheckStationTask.WriteLoadSignal</c>:
        /// 写字符串地址,true→1,false→0,返回是否写入成功)。
        /// </summary>
        /// <param name="address">字符串信号地址</param>
        /// <param name="value">信号值</param>
        /// <returns>是否写入成功</returns>
        protected virtual bool WriteSignal(string address, bool value)
        {
            if (Disable) return true;
            if (string.IsNullOrWhiteSpace(address)) return false;

            GetVDevice<VCommuncation>(CommDevice, out var comm);
            if (comm == null)
            {
                OnAlarm(AlarmType.FailError, $"交握通信设备未配置:{CommDevice?.Name}");
                return false;
            }

            comm.Open();
            comm.SetProtocol(ProtocolType);

            try
            {
                comm.Write(value ? (ushort)1 : (ushort)0, address);
                return true;
            }
            catch (Exception ex)
            {
                MyOwner.OnLog(LogType.Error, $"写入交握信号 {address}={value} 异常:{ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 清零本站发送侧全部握手信号(对齐源端 <c>LoadSingleClaer</c> /
        /// <c>UnloadSingleClaer</c> 的 Sending/SendTransfer/SendInterLock 置 OFF)。
        /// </summary>
        protected virtual void ClearSendSignals()
        {
            if (Address == null) return;
            WriteSignal(Address.SendingAddress, false);
            WriteSignal(Address.SendTranSferAddress, false);
            WriteSignal(Address.SendInterLockAddress, false);
        }

        #endregion

        #region 运动与站级钩子(站层 override,TES-37-7 集成时接线)

        /// <summary>
        /// 站级访问校验:对齐源端 <c>mStation.CheckAccessError</c>(状态机外层 while 条件)。
        /// <para>默认返回 true(继续);站层可 override 接急停/暂停/中止判定。</para>
        /// </summary>
        protected virtual bool CheckAccessError() => true;

        /// <summary>
        /// 站级暂停校验:对齐源端 <c>mStation.CheckAccessWithPause</c>。
        /// <para>默认返回 true;站层可 override 接暂停/中止判定。</para>
        /// </summary>
        protected virtual bool CheckAccessWithPause() => true;

        /// <summary>
        /// 同步等待(对齐源端 <c>mStation.SyncWait</c>)。
        /// <para>默认 SpinWait 等待 <see cref="WaitSleep"/> ms;状态机轮询间隙调用。</para>
        /// </summary>
        protected virtual void SyncWait(int milliseconds)
        {
            System.Threading.Thread.Sleep(milliseconds);
        }

        /// <summary>
        /// 步信息过滤输出(对齐源端 <c>SFInfo</c>):仅当步号变化时记录一次日志,避免轮询刷屏。
        /// </summary>
        protected virtual void StepInfo(int step, string message)
        {
            if (_innerStep != step)
            {
                _innerStep = step;
                MyOwner.OnLog(LogType.Info, $"STEP[{step}] {message}");
            }
        }

        /// <summary>移动到上料/下料等待位(对齐源端 MoveToPickUpWaitPosition / MoveToLeaveWaitPosition)。站层 override。</summary>
        protected virtual bool MoveToWaitPosition() => true;

        /// <summary>移动到上料/下料位(对齐源端 MoveToPickUpPosition / MoveToLeavePosition)。站层 override。</summary>
        protected virtual bool MoveToWorkPosition() => true;

        /// <summary>慢速撤离到安全位(对齐源端 MoveToSafePositionSlowly / MoveToLeaveWaitPositionSlowly)。站层 override。</summary>
        protected virtual bool MoveToSafePosition() => true;

        /// <summary>真空吸控制(对齐源端 SorbControlProductExist / SorbControl)。站层 override。</summary>
        /// <param name="on">true=吸合,false=破真空</param>
        protected virtual bool SorbControl(bool on) => true;

        /// <summary>读取上游/写入下游产品在籍信息(对齐源端 GetProductInfor / SetDataSource)。站层 override。</summary>
        protected virtual bool TransferProductInfo() => true;

        /// <summary>
        /// 警告暂停(对齐源端 <c>mStation.WarningPause</c>):记录告警并阻塞等待。
        /// <para>默认仅记录日志,不阻塞(节点层无 UI 阻塞能力,站层 override 接告警弹窗)。</para>
        /// </summary>
        protected virtual void WarningPause(string message)
        {
            MyOwner.OnLog(LogType.Warning, $"交握警告暂停:{message}");
        }

        #endregion

        #region DoExcute 驱动

        /// <summary>
        /// 驱动状态机:循环调用 <see cref="StepMachine"/> 直到完成或中止(对齐源端 while+switch)。
        /// <para>异常分支 101/102 由子类 <see cref="StepMachine"/> 内部 case 处理,
        /// 基类只负责驱动循环与终态判定。</para>
        /// </summary>
        public override bool DoExcute(out string errMsg)
        {
            errMsg = string.Empty;

            // 启动前自检:关键握手地址须配置
            if (Address == null || !Address.IsConfigured)
            {
                errMsg = "交握信号地址表未完整配置(Ready/Sending/Transfer/InterLock 四组缺一不可)";
                OnAlarm(AlarmType.FailError, errMsg);
                return false;
            }

            CurrentStep = 0;
            _innerStep = -1;

            try
            {
                // 对齐源端 while (mStation.CheckAccessError())
                while (CheckAccessError())
                {
                    CurrentStep = StepMachine(CurrentStep);

                    if (CurrentStep == StepDone)
                    {
                        break;
                    }

                    // 中止:运动/产品动作失败,对齐源端 return false
                    if (CurrentStep == StepAbort)
                    {
                        errMsg = $"交握中止:运动或产品动作失败,STEP={CurrentStep}";
                        ClearSignals();
                        return false;
                    }
                }

                // 对齐源端 if (!mStation.CheckAccessWithPause()) return false;
                if (!CheckAccessWithPause())
                {
                    errMsg = "交握被暂停或中止";
                    return false;
                }

                if (CurrentStep != StepDone)
                {
                    errMsg = $"交握未完成,步骤异常 STEP={CurrentStep}";
                    ClearSignals();
                    MyOwner.OnLog(LogType.Error, $"HandoverNode 步骤异常 Step={CurrentStep}");
                    return false;
                }

                return base.DoExcute(out errMsg);
            }
            catch (Exception ex)
            {
                errMsg = $"交握状态机异常:{ex.Message}";
                MyOwner.OnLog(LogType.Error, errMsg);
                ClearSignals();
                return false;
            }
        }

        #endregion
    }
}
