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
        [Parameter("电流匹配容差(0x6077与0x5018的偏差)", 24, CN = "电流容差", DefaultV = 10)]
        public int SoftLandingCurrentTolerance { get; set; }

        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("速度判定阈值(mm/s)", 25, CN = "速度阈值", DefaultV = 0.5)]
        public double SoftLandingSpeedThreshold { get; set; }

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
        [DependOn("ActionType", VCMActionType.HomeNonStandard)]
        [Parameter("回零超时(秒)", 17, CN = "回零超时", DefaultV = 60)]
        public int HomeTimeout { get; set; }

        // ===== 非标回零参数 =====
        [DependOn("ActionType", VCMActionType.HomeNonStandard)]
        [Parameter("回零模式代码", 18, CN = "回零模式", DefaultV = (short)0)]
        public short HomeMode { get; set; }

        [DependOn("ActionType", VCMActionType.HomeNonStandard)]
        [Parameter("回零高速(mm/s)", 19, CN = "回零高速", DefaultV = 50.0)]
        public double HomeSpeed { get; set; }

        [DependOn("ActionType", VCMActionType.HomeNonStandard)]
        [Parameter("回零低速(mm/s)", 20, CN = "回零低速", DefaultV = 10.0)]
        public double HomeLowSpeed { get; set; }

        [DependOn("ActionType", VCMActionType.HomeNonStandard)]
        [Parameter("回零加速度(mm/s²)", 21, CN = "回零加速度", DefaultV = 1000.0)]
        public double HomeAcc { get; set; }

        [DependOn("ActionType", VCMActionType.HomeNonStandard)]
        [Parameter("碰撞回零电流阈值(千分比)", 22, CN = "碰撞电流阈值", DefaultV = 500)]
        public int HomeCollisionCurrent { get; set; }

        [DependOn("ActionType", VCMActionType.HomeNonStandard)]
        [Parameter("碰撞电流检测时间(ms)", 23, CN = "电流检测时间", DefaultV = 100)]
        public int HomeCollisionTime { get; set; }

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
                    case VCMActionType.HomeNonStandard:
                        ExecuteHomeNonStandard();
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
            _axis.ServOn(true);
            OutResult = true;
        }

        private void ExecuteReset()
        {
            _axis.ResetStatus();
            OutResult = true;
        }

        private void ExecuteServoOff()
        {
            _axis.ServOn(false);
            OutResult = true;
        }

        #endregion

        #region 回零

        /// <summary>
        /// 常规回零(LCMotionCard.Home)
        /// 使用轴卡标准回零方法
        /// </summary>
        private void ExecuteHome()
        {
            _axis.Home();
            _axis.CheckHomeDone(HomeTimeout);
            OutResult = true;
        }

        /// <summary>
        /// 非标回零(大寰文档P34推荐方式)
        /// 执行顺序: 设参数(模式34占位) → 切回零模式 → SDO写正确模式/堵转电流/时间 → 启动回零
        /// </summary>
        private void ExecuteHomeNonStandard()
        {
            short axis = (short)_axis.AxisNo;
            short card = 0;

            // 1. 速度/加速度单位转换 (mm/s → pls/s)
            uint velHi = (uint)(HomeSpeed * _axis.PerPluse);
            uint velLo = (uint)(HomeLowSpeed * _axis.PerPluse);
            uint accUint = (uint)(HomeAcc * _axis.PerPluse);

            // 2. 设定回零参数(模式暂用34占位)
            short ret = ecat_motion.M_SetHomingPrm(axis, 34, 0, velHi, velLo, accUint, 0, card);
            if (ret != 0)
            {
                OutResult = false;
                OutFailReason = $"非标回零: 设置回零参数失败, 错误码: {ret}";
                return;
            }

            // 3. 切换至回零模式(Mode=6)
            ret = ecat_motion.M_SetHomingMode(axis, 6, card);
            Thread.Sleep(50);
            if (ret != 0)
            {
                OutResult = false;
                OutFailReason = $"非标回零: 切换回零模式失败, 错误码: {ret}";
                return;
            }

            // 4. SDO写入正确的回零模式、堵转电流、堵转时间
            _axis.SDOWrite(0x6098, 0, HomeMode, 1);
            _axis.SDOWrite(0x5000, 5, HomeCollisionCurrent, 2);
            _axis.SDOWrite(0x5000, 6, HomeCollisionTime, 2);

            // 5. 启动回零
            ret = ecat_motion.M_HomingStart(axis, card);
            if (ret != 0)
            {
                OutResult = false;
                OutFailReason = $"非标回零: 启动回零失败, 错误码: {ret}";
                return;
            }

            // 等待回零完成
            short homsts = 0;
            int timeoutMs = Math.Min(HomeTimeout * 1000, short.MaxValue);
            ret = Ec6000_HomeMove.M60_WaitHoming(axis, (short)timeoutMs, ref homsts, card);

            if (ret == -11)
            {
                OutResult = false;
                OutFailReason = $"非标回零超时({HomeTimeout}秒)";
                return;
            }

            if (ret == -12)
            {
                OutResult = false;
                OutFailReason = "非标回零完成但模式切换失败(未能切回CSP模式)";
                return;
            }

            if (ret != 0)
            {
                OutResult = false;
                OutFailReason = $"非标回零失败, 错误码: {ret}, 回零状态: {homsts}";
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
            return ReadRawCurrent() * PressureCalibrationK + PressureCalibrationB;
        }

        /// <summary>
        /// 读取0x6077原始电流值(千分比)
        /// </summary>
        private int ReadRawCurrent()
        {
            _axis.SDORead(0x6077, 0, 2, out int currentValue, 1);
            return currentValue;
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
                double lastPos = _axis.GetCurrentPos();

                while (elapsed < timeoutMs)
                {
                    if (_isBreak) return;

                    Thread.Sleep(10);
                    elapsed += 10;

                    double position = _axis.GetCurrentPos();
                    int rawCurrent = ReadRawCurrent();
                    double speed = Math.Abs(position - lastPos) * 100; // mm/10ms → mm/s
                    lastPos = position;

                    // 文档P50判定: 0x6077 ∈ (0x5018 ± tolerance) AND 速度 ≤ 阈值
                    bool currentMatch = Math.Abs(rawCurrent - currentLimit) <= SoftLandingCurrentTolerance;
                    bool speedLow = speed <= SoftLandingSpeedThreshold;

                    if (currentMatch && speedLow)
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
                        OutFailReason = $"到达位置上限({PositionUpperLimit}mm)防撞停止, 电流:{rawCurrent}, 限制:{currentLimit}";
                        return;
                    }
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
