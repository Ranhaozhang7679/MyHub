using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.SimDevice.MotionCard.LC;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Enums;
using Luster.TaskFlow.Motion.Interfaces;
using System;
using System.Threading;

namespace Luster.Module.Motion.Device.Functions
{
    /// <summary>
    /// 大寰音圈电机 Function
    /// 品牌:大寰(DH Robotics) SAC-N2 驱动器 + DLAR-20-40 ZR 执行器
    /// 通信:EtherCAT(CIA402协议)
    /// 力控:开环力控(CSP位置模式 + 电流限制)
    /// 压力反馈:0x6077h(电流反馈间接推算)
    /// 回零:非标回零(Ec6000_HomeMove)
    /// </summary>
    public class DHRoboticsVCM : MotionFunction, IPauseFunction, IStopFunction, INote
    {
        #region 参数定义

        // ===== 公共参数 =====
        [NotEmpty]
        [Parameter("轴设备选择", 0, CN = "轴名称", EditorType = typeof(VAxis))]
        public VDevice DeviceParam { get; set; }

        [Parameter("动作类型", 1, CN = "动作类型", DefaultV = VCMActionType.ServoOn)]
        public VCMActionType ActionType { get; set; }

        // ===== 目标位置参数 =====
        [Parameter("目标位置(mm)", 2, CN = "目标位置")]
        public double TargetPosition { get; set; }

        [Parameter("位置上限(mm)", 3, CN = "位置上限")]
        public double PositionUpperLimit { get; set; }

        [Parameter("位置下限(mm)", 4, CN = "位置下限")]
        public double PositionLowerLimit { get; set; }

        // ===== 运动参数 =====
        [Parameter("运动速度(mm/s)", 5, CN = "运动速度", DefaultV = 50.0)]
        public double MoveSpeed { get; set; }

        [Parameter("加速度(mm/s²)", 6, CN = "加速度", DefaultV = 1000.0)]
        public double MoveAcc { get; set; }

        [Parameter("减速度(mm/s²)", 7, CN = "减速度", DefaultV = 1000.0)]
        public double MoveDec { get; set; }

        // ===== 软着陆压力参数 =====
        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("目标压力设定值", 8, CN = "目标压力")]
        public double TargetPressure { get; set; }

        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("压力上限(防撞保护)", 9, CN = "压力上限")]
        public double PressureUpperLimit { get; set; }

        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("压力下限", 10, CN = "压力下限")]
        public double PressureLowerLimit { get; set; }

        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("压入速度(mm/s)", 11, CN = "压入速度")]
        public double PressSpeed { get; set; }

        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("软着陆超时(秒)", 12, CN = "软着陆超时", DefaultV = 10)]
        public int SoftLandingTimeout { get; set; }

        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("压力标定系数K(压力=K×电流+B)", 13, CN = "标定系数K", DefaultV = 1.0)]
        public double PressureCalibrationK { get; set; }

        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("压力标定偏移B", 14, CN = "标定偏移B", DefaultV = 0.0)]
        public double PressureCalibrationB { get; set; }

        // ===== 多段运动参数 =====
        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("启用多段运动", 15, CN = "多段运动", DefaultV = false)]
        public bool EnableMultiSegment { get; set; }

        [DependOn("EnableMultiSegment", true)]
        [Parameter("中间点位(mm),快速定位到此位置后再软着陆", 16, CN = "中间点位")]
        public double MiddlePosition { get; set; }

        // ===== 回零参数 =====
        [DependOn("ActionType", VCMActionType.Home)]
        [Parameter("回零模式代码", 17, CN = "回零模式", DefaultV = (short)0)]
        public short HomeMode { get; set; }

        [DependOn("ActionType", VCMActionType.Home)]
        [Parameter("回零高速(mm/s)", 18, CN = "回零高速", DefaultV = 50.0)]
        public double HomeSpeed { get; set; }

        [DependOn("ActionType", VCMActionType.Home)]
        [Parameter("回零低速(mm/s)", 19, CN = "回零低速", DefaultV = 10.0)]
        public double HomeLowSpeed { get; set; }

        [DependOn("ActionType", VCMActionType.Home)]
        [Parameter("回零加速度(mm/s²)", 20, CN = "回零加速度", DefaultV = 1000.0)]
        public double HomeAcc { get; set; }

        [DependOn("ActionType", VCMActionType.Home)]
        [Parameter("回零超时(秒)", 21, CN = "回零超时", DefaultV = 60)]
        public int HomeTimeout { get; set; }

        // ===== 输出参数 =====
        [Parameter("执行结果", 30, CN = "执行结果", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool OutResult { get; set; }

        [Parameter("实际位置(mm)", 31, CN = "实际位置", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public double OutPosition { get; set; }

        [Parameter("实际压力", 32, CN = "实际压力", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public double OutPressure { get; set; }

        [Parameter("失败原因", 33, CN = "失败原因", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string OutFailReason { get; set; }

        #endregion

        public override string[] NoteParams => new string[] { nameof(DeviceParam), nameof(ActionType) };

        private VAxis _axis;
        private volatile bool _isBreak;

        public DHRoboticsVCM()
        {
            Tips = "大寰音圈电机(SAC-N2)";
            Icon = "\xe678";
        }

        public override bool DoExcute(out string errMsg)
        {
            errMsg = "";
            OutResult = false;
            OutFailReason = "";
            _isBreak = false;

            GetVDevice<VAxis>(DeviceParam, out _axis);
            if (_axis == null)
            {
                errMsg = $"设备:{DeviceParam.Name}未找到";
                OutFailReason = errMsg;
                return false;
            }

            try
            {
                switch (ActionType)
                {
                    case VCMActionType.ServoOn:
                        ExecuteServoOn();
                        break;
                    case VCMActionType.Reset:
                        ExecuteReset();
                        break;
                    case VCMActionType.ServoOff:
                        ExecuteServoOff();
                        break;
                    case VCMActionType.Home:
                        ExecuteHome();
                        break;
                    case VCMActionType.HardLanding:
                        ExecuteHardLanding();
                        break;
                    case VCMActionType.SoftLanding:
                        ExecuteSoftLanding();
                        break;
                    default:
                        errMsg = $"不支持的动作类型: {ActionType}";
                        OutFailReason = errMsg;
                        return false;
                }
            }
            catch (Exception ex)
            {
                errMsg = $"执行异常: {ex.Message}";
                OutFailReason = errMsg;
                OutResult = false;
                return false;
            }

            return base.DoExcute(out errMsg);
        }

        #region CIA402 辅助方法

        private int ReadStatusWord()
        {
            _axis.SDORead(0x6041, 0, 2, out int status, 1);
            return status;
        }

        private void WriteControlWord(int value)
        {
            _axis.SDOWrite(0x6040, 0, value, 2);
        }

        private bool WaitForStatus(int expectedMasked, int mask = 0x006F, int timeoutMs = 5000)
        {
            int elapsed = 0;
            while (elapsed < timeoutMs)
            {
                int status = ReadStatusWord();
                if ((status & mask) == expectedMasked)
                    return true;
                Thread.Sleep(10);
                elapsed += 10;
            }
            return false;
        }

        private bool IsFaultState()
        {
            int status = ReadStatusWord();
            return (status & 0x0008) != 0 || (status & 0x0004) != 0;
        }

        private bool ClearFault()
        {
            WriteControlWord(0x0000);
            Thread.Sleep(50);
            WriteControlWord(0x0080);
            Thread.Sleep(100);
            return !IsFaultState();
        }

        #endregion

        #region 使能 / 复位 / 失能

        private void ExecuteServoOn()
        {
            for (int attempt = 0; attempt < 2; attempt++)
            {
                if (IsFaultState())
                {
                    if (!ClearFault())
                    {
                        if (attempt == 0) continue;
                        OutResult = false;
                        OutFailReason = "使能失败: 报警清除失败";
                        return;
                    }
                }

                WriteControlWord(0x0006);
                if (!WaitForStatus(0x0021))
                {
                    if (attempt == 0) continue;
                    OutResult = false;
                    OutFailReason = $"使能失败: 无法进入 Ready to switch on, 状态字: 0x{ReadStatusWord():X4}";
                    return;
                }

                WriteControlWord(0x0007);
                if (!WaitForStatus(0x0023))
                {
                    if (attempt == 0) continue;
                    OutResult = false;
                    OutFailReason = $"使能失败: 无法进入 Switched on, 状态字: 0x{ReadStatusWord():X4}";
                    return;
                }

                WriteControlWord(0x000F);
                if (!WaitForStatus(0x0027))
                {
                    if (attempt == 0) continue;
                    OutResult = false;
                    OutFailReason = $"使能失败: 无法进入 Operation enabled, 状态字: 0x{ReadStatusWord():X4}";
                    return;
                }

                OutResult = true;
                return;
            }
        }

        private void ExecuteReset()
        {
            if (!IsFaultState())
            {
                OutResult = true;
                return;
            }
            OutResult = ClearFault();
            if (!OutResult)
                OutFailReason = "复位失败: 报警清除失败";
        }

        private void ExecuteServoOff()
        {
            WriteControlWord(0x0000);
            Thread.Sleep(50);
            OutResult = true;
        }

        #endregion

        #region 回零(大寰非标回零)

        /// <summary>
        /// 回零流程(大寰非标回零)
        /// 调用 Ec6000_HomeMove.M60_HomMove 启动回零
        /// 调用 M60_WaitHoming 等待回零完成
        /// 错误码: -11=超时, -12=模式切换失败
        /// </summary>
        private void ExecuteHome()
        {
            short axis = (short)_axis.AxisNo;
            short card = 0;
            short homsts = 0;

            short ret = Ec6000_HomeMove.M60_HomMove(
                axis,
                (short)HomeMode,
                ref homsts,
                0,
                (uint)(HomeSpeed * 1000),
                (uint)(HomeLowSpeed * 1000),
                (uint)(HomeAcc * 1000),
                card);

            if (ret != 0)
            {
                OutResult = false;
                OutFailReason = $"回零启动失败, 错误码: {ret}";
                return;
            }

            ret = Ec6000_HomeMove.M60_WaitHoming(
                axis,
                (short)(HomeTimeout * 1000),
                ref homsts,
                card);

            if (ret == -11)
            {
                OutResult = false;
                OutFailReason = $"回零超时({HomeTimeout}秒)";
                return;
            }

            if (ret == -12)
            {
                OutResult = false;
                OutFailReason = "回零完成但模式切换失败(未能切回CSP模式)";
                return;
            }

            if (ret != 0)
            {
                OutResult = false;
                OutFailReason = $"回零失败, 错误码: {ret}, 回零状态: {homsts}";
                return;
            }

            OutResult = true;
        }

        #endregion

        #region 硬着陆

        private void ExecuteHardLanding()
        {
            _axis.MoveAbs(TargetPosition, MoveSpeed, MoveAcc, MoveDec);
            _axis.CheckMotionDone();

            double actualPos = _axis.GetCurrentPos();
            OutPosition = actualPos;

            if (actualPos >= PositionLowerLimit && actualPos <= PositionUpperLimit)
            {
                OutResult = true;
            }
            else
            {
                OutResult = false;
                OutFailReason = $"位置超限: 实际{actualPos:F3}mm, 范围[{PositionLowerLimit:F3}, {PositionUpperLimit:F3}]mm";
            }
        }

        #endregion

        #region 软着陆(大寰:通过电流反馈推算压力)

        /// <summary>
        /// 读取压力反馈值(大寰:0x6077h 电流反馈间接推算)
        /// </summary>
        private double ReadPressure()
        {
            _axis.SDORead(0x6077, 0, 2, out int currentValue, 1);
            return currentValue * PressureCalibrationK + PressureCalibrationB;
        }

        private int PressureToCurrentLimit(double pressure)
        {
            if (Math.Abs(PressureCalibrationK) < 0.0001)
                return (int)pressure;
            return (int)((pressure - PressureCalibrationB) / PressureCalibrationK);
        }

        private int ReadCurrentLimit()
        {
            _axis.SDORead(0x5018, 0, 2, out int value, 1);
            return value;
        }

        private void ExecuteSoftLanding()
        {
            int defaultCurrentLimit = ReadCurrentLimit();

            try
            {
                if (EnableMultiSegment)
                {
                    _axis.MoveAbs(MiddlePosition, MoveSpeed, MoveAcc, MoveDec);
                    _axis.CheckMotionDone();
                    if (_isBreak) return;
                }

                int currentLimit = PressureToCurrentLimit(TargetPressure);
                _axis.SDOWrite(0x5018, 0, currentLimit, 2);
                Thread.Sleep(50);

                _axis.MoveAbs(TargetPosition, PressSpeed, MoveAcc, MoveDec);

                int elapsed = 0;
                int timeoutMs = SoftLandingTimeout * 1000;

                while (elapsed < timeoutMs)
                {
                    if (_isBreak) return;

                    double position = _axis.GetCurrentPos();
                    double pressure = ReadPressure();

                    if (position >= PositionLowerLimit && position <= PositionUpperLimit
                        && pressure >= PressureLowerLimit && pressure <= PressureUpperLimit)
                    {
                        _axis.Stop();
                        Thread.Sleep(50);
                        OutPosition = _axis.GetCurrentPos();
                        OutPressure = ReadPressure();
                        OutResult = true;
                        return;
                    }

                    if (position >= PositionUpperLimit)
                    {
                        _axis.Stop();
                        Thread.Sleep(50);
                        OutPosition = _axis.GetCurrentPos();
                        OutPressure = ReadPressure();
                        OutResult = false;
                        OutFailReason = $"到达位置上限({PositionUpperLimit}mm)防撞停止, 当前压力:{pressure:F2}, 范围[{PressureLowerLimit:F2}, {PressureUpperLimit:F2}]";
                        return;
                    }

                    Thread.Sleep(5);
                    elapsed += 5;
                }

                _axis.Stop();
                Thread.Sleep(50);
                OutPosition = _axis.GetCurrentPos();
                OutPressure = ReadPressure();
                OutResult = false;
                OutFailReason = $"软着陆超时({SoftLandingTimeout}秒), 位置:{OutPosition:F3}mm, 压力:{OutPressure:F2}";
            }
            finally
            {
                _axis.SDOWrite(0x5018, 0, defaultCurrentLimit, 2);
            }
        }

        #endregion

        #region 停止/暂停

        public override void Stop()
        {
            _isBreak = true;
            if (_axis != null)
            {
                _axis.Stop();
            }
        }

        public override bool IsNeedPause => true;

        #endregion
    }
}
