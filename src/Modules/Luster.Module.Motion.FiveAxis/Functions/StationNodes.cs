using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Real;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Common.Logics;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Logic;
using Luster.TaskFlow.Motion.Modules;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Luster.Module.Motion.FiveAxis.Functions
{
    // ====================================================================
    // P5-6 检测站骨架 + AOI#1 三段式编排节点(P2-D + P2-E)。
    //
    // 范围:五轴检测站 Function 骨架(FiveAxisStation)+ RTCP 帧建立/退出节点
    //      (RtcpFrameEnter/RtcpFrameExit,即源端 BuildFrame/ExitFrame)+ 站间交握
    //      状态机节点(HandoverNode,源端 HandoverFeedFromUpArmToStage 15 步 /
    //      HandoverLeaveToDownArmToStage 13 步的信号交握抽离)。
    //
    // 范式合规:全部为 MotionFunction 节点,经 FiveAxisModule/FiveAxisStationModule
    //   AddFunction 注册,可由 XML recipe 编排(TES-29 MotionModule+MotionFunction+recipe
    //   范式),无硬编码 Action。源端 CheckNormalAction.cs 三段式(prepare/work/complete)
    //   映射为站模块 Children 下的三个 Group 容器(见 Recipes/AOI1_ThreeSegment.recipe.xml)。
    //
    // 异常结构化:源端空 catch(CheckNormalAction.cs:296 / Check5AxisStationBase.cs:1238)
    //   迁移为结构化报警——节点失败抛 DeviceTimeoutException/DeviceException 由
    //   MotionModule.DoFunction 统一转 AlarmInfo(OnAlarm);卡侧资源(RTCP 帧)用
    //   try/catch 保证退出不抛空(RtcpFrameExit 幂等)。急停:WaitFunc 内部检查
    //   IsBreak/BrokenOff,急停时跳出等待(对应源端段间急停门 + onBreakStoped)。
    //
    // R1 非侵入:仅新增节点,不改 IMotionCard/Luster.Prism/Luster.TaskFlow.Common 主干。
    // ====================================================================

    /// <summary>
    /// 五轴检测站 Function(P2-D 站骨架)。
    /// 派生自 <see cref="StationFunction"/>,实现 <see cref="IFreeStation"/>:自由工站,
    /// 不检查有料状态,由 recipe 全局变量控制有无料(与源端 Check5AxisStationBase 对齐)。
    /// DoExcute 更新当前在制 SN(TryPeek)→ 递归运行 Children[0](三段式 Group 链)→ 统计工站节拍。
    /// 三段式(prepare/work/complete)由 recipe 在本站 Children 下挂三个 Group 容器表达,
    /// 站骨架本身不硬编码段顺序,段间急停门由 MotionRunEngine.IsBreak/BrokenOff 保证。
    /// </summary>
    public class FiveAxisStation : StationFunction, IFreeStation
    {
        /// <summary>是否启用工站</summary>
        [Parameter("是否启用工站", 0, CN = "工站启用", DefaultV = true, CanRef = ParamRef.Ref)]
        public bool IsEnabled { get; set; }

        /// <summary>启动后跳转至第一步(全局变量名)</summary>
        [Parameter("启动后跳转至第一步", 0, CN = "工站跳转启用", DefaultV = false, CanRef = ParamRef.Ref)]
        public bool IsReturnEnabled { get; set; }

        [DependOn("IsReturnEnabled", true)]
        [Parameter("启动后跳转至第一步", 1, CN = "工站跳转", EditorType = typeof(IGlobal))]
        public string IsReturn { get; set; }

        /// <summary>所属模块分区</summary>
        [Parameter("所属模块", 1, CN = "所属模块", DefaultV = "System")]
        public string Module { get; set; }

        /// <summary>本站要料(OUT)</summary>
        [Parameter("本站要料", 11, CN = "本站要料", ParamType = ParamType.OUT, DefaultV = false)]
        public bool ThisGet { get; set; }

        /// <summary>本站有料(OUT)</summary>
        [Parameter("本站有料", 12, CN = "本站有料", ParamType = ParamType.OUT, DefaultV = false)]
        public bool ThisHave { get; set; }

        public FiveAxisStation()
        {
            this.Icon = "\xe696";
            this.Tips = "五轴检测站(AOI#1):自由工站,三段式 prepare/work/complete 由 recipe Group 链表达";
        }

        public override bool DoExcute(out string errMsg)
        {
            bool isSuccess = false;
            errMsg = string.Empty;

            // 工站是否启用
            if (!IsEnabled)
            {
                MyOwner.OnLog(LogType.Debug, $"Station : {MyOwner.Alias} is disable!");
                return true;
            }

            // 更新当前在制 SN(对应源端 SetDataSource / 在籍产品)
            if (TryPeek(out var dataID))
            {
                MyOwner.DataID = dataID;
            }
            else
            {
                MyOwner.DataID = "";
            }

            // 运行子模块(三段式 Group 链头,recipe 编排)
            if (MyOwner.Children.Count > 0)
            {
                var startModule = MyOwner.Children[0];
                motionRunEngine.Run(startModule, ref isSuccess);
                if (!isSuccess)
                {
                    errMsg = motionRunEngine.ErrorMessage;
                    return false;
                }
            }

            OnStationTime();
            return string.IsNullOrEmpty(errMsg);
        }

        public bool GetIsEnabled()
        {
            var p = MyOwner.Parameters[nameof(IsEnabled)];
            if (p != null && bool.TryParse(p.GetValue(out var errMsg)?.ToString(), out var v))
            {
                return v;
            }
            return true;
        }

        public string GetReturnValName() => IsReturn;

        #region 模块跳转(IFreeStation)

        private IMotionModule goModule = null;

        public bool IsGoTo(IMotionModule curModule)
        {
            bool isGoTo = goModule != null;
            if (isGoTo && curModule == goModule)
            {
                goModule = null;
                MyOwner.OnLog(LogType.Info, $"工站跳转结束->{curModule.Alias}");
                isGoTo = false;
            }
            return isGoTo;
        }

        public void GoTo(IMotionModule goToModule) => goModule = goToModule;

        public override void ClearDatas()
        {
            base.ClearDatas();
            goModule = null;
        }
        #endregion
    }

    /// <summary>
    /// RTCP 帧建立节点(P2-E work 段开头,即源端 BuildFrame / 建立 RTCP 坐标系)。
    /// 调 <see cref="IFiveAxisRTCP.ConfigureRtcp"/> 建立卡侧五轴 RTCP 坐标系 +
    /// <see cref="IFiveAxisRTCP.SetRtcpEnabled"/>(true) 进入刀尖跟随。
    /// 失败抛 <see cref="DeviceException"/> 转 AlarmInfo(结构化报警),不静默吞错。
    /// 与 <see cref="RtcpFrameExit"/> 成对:work 段首 Enter,complete 段(及急停安全路径)Exit。
    /// </summary>
    /// <remarks>
    /// RTCP 帧建立参数取自标定结果(P5-5 CalibratedCoord5Axis):旋转中心 + 刀具偏置 +
    /// 虚拟轴/真实轴映射。坐标系数由卡端分配。真机 ≤0.02mm 精度待人类现场验证(TES carve-out)。
    /// </remarks>
    public class RtcpFrameEnter : MotionFunction
    {
        /// <summary>多轴仿真设备(经其 GetDevice() 拿 IMotionCard + IFiveAxisRTCP)</summary>
        [NotEmpty]
        [Parameter("多轴设备", 1, CN = "多轴设备", EditorType = typeof(VAxisM), CanRef = ParamRef.None)]
        public VDevice AxisDevice { get; set; }

        /// <summary>RTCP 坐标系号(卡端分配)</summary>
        [Parameter("坐标系号", 2, Group = "RTCP配置", CN = "坐标系", DefaultV = 0)]
        public virtual int CoordinateSystem { get; set; }

        /// <summary>虚拟轴号(逗号分隔,如 "0,1,2,3,4")</summary>
        [Parameter("虚拟轴号", 3, Group = "RTCP配置", CN = "虚拟轴", DefaultV = "0,1,2,3,4")]
        public virtual string VirtualAxisIds { get; set; }

        /// <summary>真实轴号(逗号分隔)</summary>
        [Parameter("真实轴号", 4, Group = "RTCP配置", CN = "真实轴", DefaultV = "0,1,2,3,4")]
        public virtual string RealAxisIds { get; set; }

        /// <summary>旋转中心 X</summary>
        [Parameter("旋转中心X", 5, Group = "RTCP配置", CN = "旋转中心X", DefaultV = 0.0)]
        public virtual double RotationCenterX { get; set; }

        /// <summary>旋转中心 Y</summary>
        [Parameter("旋转中心Y", 6, Group = "RTCP配置", CN = "旋转中心Y", DefaultV = 0.0)]
        public virtual double RotationCenterY { get; set; }

        /// <summary>旋转中心 Z</summary>
        [Parameter("旋转中心Z", 7, Group = "RTCP配置", CN = "旋转中心Z", DefaultV = 0.0)]
        public virtual double RotationCenterZ { get; set; }

        /// <summary>刀具偏置 X</summary>
        [Parameter("刀具偏置X", 8, Group = "RTCP配置", CN = "刀具偏置X", DefaultV = 0.0)]
        public virtual double ToolOffsetX { get; set; }

        /// <summary>刀具偏置 Y</summary>
        [Parameter("刀具偏置Y", 9, Group = "RTCP配置", CN = "刀具偏置Y", DefaultV = 0.0)]
        public virtual double ToolOffsetY { get; set; }

        /// <summary>刀具偏置 Z</summary>
        [Parameter("刀具偏置Z", 10, Group = "RTCP配置", CN = "刀具偏置Z", DefaultV = 0.0)]
        public virtual double ToolOffsetZ { get; set; }

        /// <summary>执行成功(OUT)</summary>
        [Parameter("执行成功", 50, Group = "输出", CN = "成功", ParamType = ParamType.OUT)]
        public virtual bool Success { get; set; }

        /// <summary>RTCP 已启用(OUT)</summary>
        [Parameter("RTCP已启用", 51, Group = "输出", CN = "RTCP启用", ParamType = ParamType.OUT)]
        public virtual bool FrameEnabled { get; set; }

        public RtcpFrameEnter()
        {
            this.Tips = "RTCP帧建立(BuildFrame):建立卡侧五轴RTCP坐标系+进入刀尖跟随";
            this.Icon = "\xe6a1";
        }

        public override bool DoExcute(out string errMsg)
        {
            errMsg = string.Empty;
            Success = false;
            FrameEnabled = false;

            GetVDevice<VAxisM>(AxisDevice, out var vAxisM);
            if (vAxisM == null)
            {
                errMsg = "未获取到多轴设备";
                return false;
            }

            var rtcp = (vAxisM.GetDevice() as IMotionCard) as IFiveAxisRTCP;
            if (rtcp == null)
            {
                // 虚拟/无 RTCP 能力板卡:结构化报警(不静默吞错),由上层决定是否忽略(Empty 模式)。
                errMsg = "当前板卡不支持 RTCP(IFiveAxisRTCP 未实现)";
                return false;
            }

            var config = new FiveAxisRtcpConfig
            {
                CoordinateSystem = CoordinateSystem,
                VirtualAxisIds = ParseIntList(VirtualAxisIds),
                RealAxisIds = ParseIntList(RealAxisIds),
                RotationCenterX = RotationCenterX,
                RotationCenterY = RotationCenterY,
                RotationCenterZ = RotationCenterZ,
                ToolOffsetX = ToolOffsetX,
                ToolOffsetY = ToolOffsetY,
                ToolOffsetZ = ToolOffsetZ,
            };

            // 配置 RTCP 坐标系(建立帧)
            if (!rtcp.ConfigureRtcp(config))
            {
                errMsg = "ConfigureRtcp 失败(RTCP 帧建立失败)";
                return false;
            }

            // 进入刀尖跟随(使能 RTCP)
            if (!rtcp.SetRtcpEnabled(true))
            {
                errMsg = "SetRtcpEnabled(true) 失败(RTCP 进入失败)";
                return false;
            }

            Success = true;
            FrameEnabled = rtcp.RtcpEnabled;
            return true;
        }

        /// <summary>解析逗号分隔的轴号列表</summary>
        private static List<int> ParseIntList(string text)
        {
            var result = new List<int>();
            if (string.IsNullOrWhiteSpace(text)) return result;
            foreach (var part in text.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (int.TryParse(part.Trim(), out var v)) result.Add(v);
            }
            return result;
        }
    }

    /// <summary>
    /// RTCP 帧退出节点(P2-E work 段末 / complete 段,即源端 ExitFrame)。
    /// 调 <see cref="IFiveAxisRTCP.SetRtcpEnabled"/>(false) 退出刀尖跟随。
    /// 幂等 + try/catch:无 RTCP 能力板卡(虚拟)直接返回成功;退出异常转日志不抛
    /// (保证急停/异常路径下 complete 段 ExitFrame 不二次抛错阻断清理,对应 M-13 精神)。
    /// </summary>
    public class RtcpFrameExit : MotionFunction
    {
        /// <summary>多轴仿真设备</summary>
        [NotEmpty]
        [Parameter("多轴设备", 1, CN = "多轴设备", EditorType = typeof(VAxisM), CanRef = ParamRef.None)]
        public VDevice AxisDevice { get; set; }

        /// <summary>执行成功(OUT)</summary>
        [Parameter("执行成功", 50, Group = "输出", CN = "成功", ParamType = ParamType.OUT)]
        public virtual bool Success { get; set; }

        public RtcpFrameExit()
        {
            this.Tips = "RTCP帧退出(ExitFrame):退出卡侧五轴RTCP刀尖跟随(幂等)";
            this.Icon = "\xe6a2";
        }

        public override bool DoExcute(out string errMsg)
        {
            errMsg = string.Empty;
            Success = true; // 幂等:默认成功(无帧可退也算成功,保证清理路径不阻断)

            GetVDevice<VAxisM>(AxisDevice, out var vAxisM);
            if (vAxisM == null)
            {
                // 无设备(虚拟/空跑):幂等成功,不阻断 complete 段清理。
                return true;
            }

            var rtcp = (vAxisM.GetDevice() as IMotionCard) as IFiveAxisRTCP;
            if (rtcp == null)
            {
                // 无 RTCP 能力板卡:无帧可退,幂等成功。
                return true;
            }

            // try/catch 结构化:退出异常转日志,不抛(急停路径下避免二次阻断)。
            try
            {
                if (rtcp.RtcpEnabled)
                {
                    Success = rtcp.SetRtcpEnabled(false);
                    if (!Success)
                    {
                        MyOwner?.OnLog(LogType.Warning, "RTCP 退出失败(SetRtcpEnabled(false) 返回 false)");
                    }
                }
            }
            catch (Exception ex)
            {
                // 结构化记录(替代源端空 catch),不抛——ExitFrame 是清理动作,失败不阻断后续清理。
                MyOwner?.OnLog(LogType.Error, $"RTCP 退出异常:{ex.Message}");
                Success = false;
            }

            return true; // 即使退出失败也返回 true,保证 complete 段后续清理(下料/数据上传)继续
        }
    }

    /// <summary>
    /// 站间交握状态机节点(P2-E,源端 HandoverFeedFromUpArmToStage 15 步 / HandoverLeaveToDownArmToStage 13 步抽离)。
    /// 抽独立交握节点,不混入运动链——本节点只做站间信号交握状态机(互锁建立 → 送料 → transfer → 真空 → 清信号),
    /// 源端 15/13 步中夹带的定位移动(移上料位/下料位/慢速撤离)拆为独立 Move 节点置于 recipe,由本节点前/后调用。
    /// 异常结构化:信号等待超时抛 <see cref="DeviceTimeoutException"/>(由 MotionModule.DoFunction 转 AlarmType.Timeout),
    /// 对应源端交握异常撤离(step 101/102);急停由 <see cref="MotionFunction.WaitFunc"/> 内部 IsBreak/BrokenOff 检查保证。
    /// 虚拟模式下 VIO 仿真电平,交握链可端到端跑通(P5-7 验)。
    /// </summary>
    public class HandoverNode : MotionFunction
    {
        /// <summary>交握方向</summary>
        public enum HandoverDirection
        {
            [System.ComponentModel.Description("上游来料(15步,AOI#1⇅擦拭)")]
            Feed,

            [System.ComponentModel.Description("下游出料(13步,AOI#1⇅AOI#2)")]
            Leave,
        }

        /// <summary>交握方向</summary>
        [Parameter("交握方向", 0, Group = "方向", CN = "方向", DefaultV = HandoverDirection.Feed)]
        public virtual HandoverDirection Direction { get; set; }

        // ---- 交握信号(每个信号绑定一个 VIO 点,recipe 配置 Index/InputOutput) ----

        /// <summary>对方就绪信号(RecReady,输入)</summary>
        [Parameter("对方就绪(RecReady)", 10, Group = "信号", CN = "对方就绪", EditorType = typeof(VIO), CanRef = ParamRef.None)]
        public VDevice RecReady { get; set; }

        /// <summary>本侧互锁输出(SendInterLock,输出)</summary>
        [Parameter("本侧互锁(SendInterLock)", 11, Group = "信号", CN = "本侧互锁", EditorType = typeof(VIO), CanRef = ParamRef.None)]
        public VDevice SendInterLock { get; set; }

        /// <summary>对方互锁信号(RecInterLock,输入)</summary>
        [Parameter("对方互锁(RecInterLock)", 12, Group = "信号", CN = "对方互锁", EditorType = typeof(VIO), CanRef = ParamRef.None)]
        public VDevice RecInterLock { get; set; }

        /// <summary>本侧送料中(Sending,输出)</summary>
        [Parameter("本侧送料中(Sending)", 13, Group = "信号", CN = "本侧送料", EditorType = typeof(VIO), CanRef = ParamRef.None)]
        public VDevice Sending { get; set; }

        /// <summary>对方送料中(Recing,输入)</summary>
        [Parameter("对方送料中(Recing)", 14, Group = "信号", CN = "对方送料", EditorType = typeof(VIO), CanRef = ParamRef.None)]
        public VDevice Recing { get; set; }

        /// <summary>本侧 transfer(SendTransfer,输出)</summary>
        [Parameter("本侧转交(SendTransfer)", 15, Group = "信号", CN = "本侧转交", EditorType = typeof(VIO), CanRef = ParamRef.None)]
        public VDevice SendTransfer { get; set; }

        /// <summary>对方 transfer(RecTransfer,输入)</summary>
        [Parameter("对方转交(RecTransfer)", 16, Group = "信号", CN = "对方转交", EditorType = typeof(VIO), CanRef = ParamRef.None)]
        public VDevice RecTransfer { get; set; }

        /// <summary>真空使能(VacuumOn,输出,Feed 时开真空/Leave 时破真空)</summary>
        [Parameter("真空使能(VacuumOn)", 17, Group = "信号", CN = "真空", EditorType = typeof(VIO), CanRef = ParamRef.None)]
        public VDevice VacuumOn { get; set; }

        /// <summary>单步信号等待超时(ms,-1 无限等待;超时即交握异常撤离)</summary>
        [Parameter("信号超时(ms)", 20, Group = "超时", CN = "超时", DefaultV = 30000)]
        public virtual int SignalTimeoutMs { get; set; }

        /// <summary>执行成功(OUT)</summary>
        [Parameter("执行成功", 50, Group = "输出", CN = "成功", ParamType = ParamType.OUT)]
        public virtual bool Success { get; set; }

        public HandoverNode()
        {
            this.Tips = "站间交握状态机(上游15步/下游13步信号交握抽离,不含定位运动)";
            this.Icon = "\xe6a3";
        }

        public override bool DoExcute(out string errMsg)
        {
            errMsg = string.Empty;
            Success = false;

            // 解析信号 VIO(未配置的信号跳过——支持简化的交握子集)
            TryGetIO(RecReady, out var recReady);
            TryGetIO(SendInterLock, out var sendInterLock);
            TryGetIO(RecInterLock, out var recInterLock);
            TryGetIO(Sending, out var sending);
            TryGetIO(Recing, out var recing);
            TryGetIO(SendTransfer, out var sendTransfer);
            TryGetIO(RecTransfer, out var recTransfer);
            TryGetIO(VacuumOn, out var vacuum);

            try
            {
                if (Direction == HandoverDirection.Feed)
                {
                    RunFeedHandover(recReady, sendInterLock, recInterLock, sending, recing, sendTransfer, recTransfer, vacuum);
                }
                else
                {
                    RunLeaveHandover(recReady, sendInterLock, recInterLock, sending, recing, sendTransfer, recTransfer, vacuum);
                }

                Success = true;
                return true;
            }
            catch (DeviceTimeoutException ex)
            {
                // 信号等待超时 = 交握异常撤离(源端 step 101/102):转结构化报警,不静默吞错。
                errMsg = $"交握异常(方向={Direction}):{ex.Message}";
                MyOwner?.OnAlarm(AlarmType.FailError, errMsg);
                return false;
            }
            catch (Exception ex)
            {
                errMsg = $"交握执行异常(方向={Direction}):{ex.Message}";
                MyOwner?.OnAlarm(AlarmType.DeviceError, errMsg);
                return false;
            }
        }

        /// <summary>上游来料交握(15 步信号序列,定位移动由 recipe 独立 Move 节点承担)。</summary>
        private void RunFeedHandover(VIO recReady, VIO sendInterLock, VIO recInterLock,
            VIO sending, VIO recing, VIO sendTransfer, VIO recTransfer, VIO vacuum)
        {
            // step -1/0:等对方就绪 → 清本侧信号
            WaitSignal(recReady, true, "等待上游就绪(RecReady)");
            SetSignal(sendInterLock, false);
            SetSignal(sendTransfer, false);

            // step 3/4:写本侧互锁 → 等对方互锁(互锁建立)
            SetSignal(sendInterLock, true);
            WaitSignal(recInterLock, true, "等待上游互锁(RecInterLock)");

            // step 6/7:开真空(取产品在籍)→ 写送料中 → 等对方送料中
            SetSignal(vacuum, true);
            SetSignal(sending, true);
            WaitSignal(recing, true, "等待上游送料中(Recing)");

            // step 9/10:等对方 transfer 完成(上游把产品送过来)
            WaitSignal(recTransfer, true, "等待上游转交(RecTransfer)");

            // step 12/13:写本侧 transfer → 等对方 transfer 撤销
            SetSignal(sendTransfer, true);
            WaitSignal(recTransfer, false, "等待上游转交撤销(RecTransfer OFF)");

            // step 14:清信号(交握完成,产品已入站)
            SetSignal(sendInterLock, false);
            SetSignal(sending, false);
            SetSignal(sendTransfer, false);
        }

        /// <summary>下游出料交握(13 步信号序列,定位移动由 recipe 独立 Move 节点承担)。</summary>
        private void RunLeaveHandover(VIO recReady, VIO sendInterLock, VIO recInterLock,
            VIO sending, VIO recing, VIO sendTransfer, VIO recTransfer, VIO vacuum)
        {
            // step 0/1:等对方就绪(下游要料)
            WaitSignal(recReady, true, "等待下游就绪(RecReady)");

            // step 3/4:等对方互锁 → 写本侧互锁
            WaitSignal(recInterLock, true, "等待下游互锁(RecInterLock)");
            SetSignal(sendInterLock, true);

            // step 6/7:写送料中 → 等对方送料中
            SetSignal(sending, true);
            WaitSignal(recing, true, "等待下游送料中(Recing)");

            // step 8:破真空 + 清本站数据(产品已交出)
            SetSignal(vacuum, false);

            // step 9/10:写本侧 transfer → 等对方 transfer
            SetSignal(sendTransfer, true);
            WaitSignal(recTransfer, true, "等待下游转交(RecTransfer)");

            // step 11/12:清信号(交握完成,产品已出站)
            SetSignal(sendInterLock, false);
            SetSignal(sending, false);
            SetSignal(sendTransfer, false);
        }

        /// <summary>等待信号达到期望电平(急停感知:WaitFunc 内部检查 IsBreak/BrokenOff;超时抛 DeviceTimeoutException)。</summary>
        private void WaitSignal(VIO io, bool expected, string statusMsg)
        {
            if (io == null) return; // 信号未配置:跳过(支持简化交握子集)
            WaitFunc(() => io.GetDigital() == expected, statusMsg, 20, SignalTimeoutMs);
        }

        /// <summary>写信号电平(输出)。</summary>
        private static void SetSignal(VIO io, bool isOn)
        {
            if (io == null) return;
            io.SetDigital(isOn);
        }

        /// <summary>解析 VDevice → VIO(未绑定返回 false,不抛)。</summary>
        private void TryGetIO(VDevice device, out VIO io)
        {
            if (device == null)
            {
                io = null;
                return;
            }
            GetVDevice<VIO>(device, out io);
        }
    }
}
