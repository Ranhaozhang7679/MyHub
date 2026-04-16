using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Enums;
using Luster.TaskFlow.Motion.Interfaces;
using System;
using System.Threading;

namespace Luster.Module.Motion.Device.Functions
{
    /// <summary>
    /// 钧舵音圈电机 Function
    /// 品牌：钧舵 GSFDmini 伺服驱动器
    /// 通信：EtherCAT（CIA402协议）
    /// 力控：驱动器内置力位控制(0x2016h触发, 0x201Ah状态机)
    /// 压力反馈：0x201Bh（模拟量 -10V~10V）
    /// </summary>
    public class JunRudderVCM : MotionFunction, IPauseFunction, IStopFunction, INote
    {
        #region 参数定义

        // ===== 公共参数 =====
        [NotEmpty]
        [Parameter("轴设备选择", 0, CN = "轴名称", EditorType = typeof(VAxis))]
        public VDevice DeviceParam { get; set; }

        [Parameter("动作类型", 1, CN = "动作类型", DefaultV = VCMActionType.ServoOn)]
        public VCMActionType ActionType { get; set; }

        // ===== 目标位置参数（硬着陆 / 软着陆共用） =====
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

        // ===== 软着陆(手动力控)参数 =====
        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("扭矩正向限制(额定电流1/1000)", 8, CN = "扭矩限制", DefaultV = 100)]
        public int TorquePositiveLimit { get; set; }

        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("压入速度(mm/s)", 9, CN = "压入速度", DefaultV = 5.0)]
        public double PressSpeed { get; set; }

        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("快进位置(mm)", 10, CN = "快进位置")]
        public double FastForwardPosition { get; set; }

        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("目标位置(mm)", 11, CN = "目标位置")]
        public double TargetPressPosition { get; set; }

        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("最大行程限制(mm)", 12, CN = "最大行程限制", DefaultV = 20.0)]
        public double MaxStrokeLimit { get; set; }

        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("软着陆超时(秒)", 13, CN = "软着陆超时", DefaultV = 10)]
        public int SoftLandingTimeout { get; set; }

        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("电流匹配容差", 14, CN = "电流容差", DefaultV = 10)]
        public int SoftLandingCurrentTolerance { get; set; }

        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("停止速度阈值(mm/s)", 15, CN = "速度阈值", DefaultV = 0.5)]
        public double SoftLandingSpeedThreshold { get; set; }

        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("压力标定系数K", 16, CN = "标定系数K", DefaultV = 1.0)]
        public double PressureCalibrationK { get; set; }

        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("压力标定偏移B", 24, CN = "标定偏移B", DefaultV = 0.0)]
        public double PressureCalibrationB { get; set; }

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

        /// <summary>
        /// 自动注释参数
        /// </summary>
        public override string[] NoteParams => new string[] { nameof(DeviceParam), nameof(ActionType) };

        /// <summary>
        /// 当前轴实例（供 Stop/Pause 使用）
        /// </summary>
        private VAxis _axis;

        /// <summary>
        /// 停止标志
        /// </summary>
        private volatile bool _isBreak;

        /// <summary>
        /// 构造函数
        /// </summary>
        public JunRudderVCM()
        {
            Tips = "钧舵音圈电机（GSFDmini）";
            Icon = "\xe678";
        }

        /// <summary>
        /// 主执行入口
        /// </summary>
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

        /// <summary>
        /// 读取状态字 0x6041h
        /// </summary>
        private int ReadStatusWord()
        {
            _axis.SDORead(0x6041, 0, 2, out int status, 1);
            return status;
        }

        /// <summary>
        /// 写控制字 0x6040h
        /// </summary>
        private void WriteControlWord(int value)
        {
            _axis.SDOWrite(0x6040, 0, value, 2);
        }

        /// <summary>
        /// 等待状态字达到预期值（按位掩码匹配）
        /// </summary>
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

        /// <summary>
        /// 检查是否处于故障状态（bit3=1 或 bit2=1）
        /// </summary>
        private bool IsFaultState()
        {
            int status = ReadStatusWord();
            return (status & 0x0008) != 0 || (status & 0x0004) != 0;
        }

        /// <summary>
        /// 清除故障（写 0x0000 → 写 0x0080 上升沿）
        /// </summary>
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

        /// <summary>
        /// 使能流程（CIA402标准），失败时重试1次
        /// </summary>
        private void ExecuteServoOn()
        {
            _axis.ServOn(true);
            OutResult = true;
        }

        /// <summary>
        /// 复位（仅清除报警，不执行上使能）
        /// </summary>
        private void ExecuteReset()
        {
            _axis.ResetStatus();
            OutResult = true;
        }

        /// <summary>
        /// 失能（下使能）
        /// </summary>
        private void ExecuteServoOff()
        {
            _axis.ServOn(false);
            OutResult = true;
        }

        #endregion

        #region 回零

        /// <summary>
        /// 回零流程（标准CIA402）
        /// </summary>
        private void ExecuteHome()
        {
            _axis.Home();
            _axis.CheckHomeDone(HomeTimeout);
            OutResult = true;
        }

        #endregion

        #region 硬着陆

        /// <summary>
        /// 硬着陆（普通点位运动）
        /// </summary>
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

        #region 软着陆(手动力控)

        /// <summary>
        /// 读取压力反馈值(0x201Bh 模拟量, -10V~10V → -32768~32767)
        /// </summary>
        private double ReadPressure()
        {
            _axis.SDORead(0x201B, 0, 2, out int rawValue, 1);
            return rawValue * PressureCalibrationK + PressureCalibrationB;
        }

        /// <summary>
        /// 软着陆(手动力控, 到达压力/位置后停止保持不动)
        /// 扭矩限制: 0x60E0(CIA402标准, 额定电流1/1000)
        /// 电流反馈: 0x6077(额定电流1/1000)
        /// 判定: |0x6077 - 0x60E0| ≤ 容差 AND 速度 ≤ 阈值 → 已压到并稳定 → Stop
        /// </summary>
        private void ExecuteSoftLanding()
        {
            // 1. 备份默认扭矩限制
            _axis.SDORead(0x60E0, 0, 2, out int defaultTorqueLimit, 1);

            try
            {
                // 2. 可选: 快速接近快进位置
                if (FastForwardPosition != 0)
                {
                    _axis.MoveAbs(FastForwardPosition, MoveSpeed, MoveAcc, MoveDec);
                    _axis.CheckMotionDone();
                    if (_isBreak) return;
                }

                // 3. 设置扭矩限制
                _axis.SDOWrite(0x60E0, 0, TorquePositiveLimit, 2);
                Thread.Sleep(50);

                // 4. 慢速压入到目标位置
                _axis.MoveAbs(TargetPressPosition, PressSpeed, MoveAcc, MoveDec);

                int elapsed = 0;
                int timeoutMs = SoftLandingTimeout * 1000;
                double lastPos = _axis.GetCurrentPos();

                while (elapsed < timeoutMs)
                {
                    if (_isBreak) return;

                    Thread.Sleep(10);
                    elapsed += 10;

                    double position = _axis.GetCurrentPos();
                    _axis.SDORead(0x6077, 0, 2, out int rawTorque, 1);
                    double speed = Math.Abs(position - lastPos) * 100; // mm/10ms → mm/s
                    lastPos = position;

                    // 判定: 实际电流接近限制值 AND 速度低于阈值 → 已压到并稳定
                    bool currentMatch = Math.Abs(rawTorque - TorquePositiveLimit) <= SoftLandingCurrentTolerance;
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

                    // 防撞: 超过最大行程限制
                    if (position >= MaxStrokeLimit)
                    {
                        _axis.Stop();
                        Thread.Sleep(50);
                        OutPosition = _axis.GetCurrentPos();
                        OutPressure = ReadPressure();
                        OutResult = false;
                        OutFailReason = $"到达最大行程限制({MaxStrokeLimit}mm)";
                        return;
                    }
                }

                // 超时
                _axis.Stop();
                Thread.Sleep(50);
                OutPosition = _axis.GetCurrentPos();
                OutPressure = ReadPressure();
                OutResult = false;
                OutFailReason = $"软着陆超时({SoftLandingTimeout}秒)";
            }
            finally
            {
                // 恢复默认扭矩限制
                _axis.SDOWrite(0x60E0, 0, defaultTorqueLimit, 2);
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
