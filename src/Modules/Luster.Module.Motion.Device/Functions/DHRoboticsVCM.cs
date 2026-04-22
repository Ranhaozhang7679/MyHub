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
    /// 力控:两段速软着陆(快进PP → 慢速接触PT → 保压 → 返回PB)
    /// 压力反馈:0x6077h(电流反馈间接推算)
    /// 回零:非标回零(Ec6000_HomeMove)
    /// </summary>
    public class DHRoboticsVCM : MotionFunction, IPauseFunction, IStopFunction, INote
    {
        public enum SlaveID : byte
        {
            NUM1 = 0,
            NUM2 = 1,
        }

        #region 参数定义

        // ===== 公共参数 =====
        [NotEmpty]
        [Parameter("轴设备选择", 0, CN = "轴名称", EditorType = typeof(VAxis))]
        public VDevice DeviceParam { get; set; }

        [NotEmpty]
        [Parameter("轴ID选择", 1, CN = "轴ID", DefaultV = SlaveID.NUM1)]
        public SlaveID SlaveNum { get; set; }

        [Parameter("动作类型", 2, CN = "动作类型", DefaultV = VCMActionType.ServoOn)]
        public VCMActionType ActionType { get; set; }

        // ===== 硬着陆参数 =====
        [DependOn("ActionType", VCMActionType.HardLanding)]
        [Parameter("目标位置(mm)", 3, CN = "目标位置")]
        public VAxisPos TargetPosition { get; set; }

        [DependOn("ActionType", VCMActionType.HardLanding)]
        [Parameter("位置上限(mm)", 4, CN = "位置上限")]
        public double PositionUpperLimit { get; set; }

        [DependOn("ActionType", VCMActionType.HardLanding)]
        [Parameter("位置下限(mm)", 5, CN = "位置下限")]
        public double PositionLowerLimit { get; set; }

        [DependOn("ActionType", VCMActionType.HardLanding)]
        [Parameter("运动速度(mm/s)", 6, CN = "运动速度", DefaultV = 50.0)]
        public double MoveSpeed { get; set; }

        // 加速度/减速度: 硬着陆和软着陆共用
        [DependOn("ActionType", VCMActionType.HardLanding)]
        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("加速度(mm/s²)", 7, CN = "加速度", DefaultV = 1000.0)]
        public double MoveAcc { get; set; }

        [DependOn("ActionType", VCMActionType.HardLanding)]
        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("减速度(mm/s²)", 8, CN = "减速度", DefaultV = 1000.0)]
        public double MoveDec { get; set; }

        // ===== 软着陆参数(参考DH Control Demo) =====
        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("快进位置PP(mm)", 9, CN = "快进位置")]
        public VAxisPos PPPosition { get; set; }

        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("快进速度(mm/s)", 10, CN = "快进速度", DefaultV = 50.0)]
        public double PPVelocity { get; set; }

        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("接触位置PT(mm)", 11, CN = "接触位置")]
        public VAxisPos PTPosition { get; set; }

        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("接触速度(mm/s)", 12, CN = "接触速度", DefaultV = 5.0)]
        public double PTVelocity { get; set; }

        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("扭矩限制(千分比)", 13, CN = "扭矩限制", DefaultV = 500)]
        public int TorqueLimit { get; set; }

        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("保压时间(ms)", 14, CN = "保压时间", DefaultV = 100)]
        public int InstallTime { get; set; }

        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("返回位置PB(mm)", 15, CN = "返回位置", DefaultV = 0.0)]
        public VAxisPos PBPosition { get; set; }

        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("软着陆超时(秒)", 16, CN = "软着陆超时", DefaultV = 10)]
        public int SoftLandingTimeout { get; set; }

        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("力矩到达容差(千分比)", 17, CN = "力矩容差", DefaultV = 20)]
        public int TorqueTolerance { get; set; }

        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("速度判定阈值(mm/s)", 18, CN = "速度阈值", DefaultV = 1.0)]
        public double SpeedThreshold { get; set; }

        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("压力标定系数K(压力=K×电流+B)", 19, CN = "标定系数K", DefaultV = 1.0)]
        public double PressureCalibrationK { get; set; }

        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("压力标定偏移B", 20, CN = "标定偏移B", DefaultV = 0.0)]
        public double PressureCalibrationB { get; set; }

        // ===== 回零参数 =====
        [DependOn("ActionType", VCMActionType.Home)]
        [DependOn("ActionType", VCMActionType.HomeNonStandard)]
        [Parameter("回零超时(秒)", 21, CN = "回零超时", DefaultV = 60)]
        public int HomeTimeout { get; set; }

        // ===== 非标回零参数 =====
        [DependOn("ActionType", VCMActionType.HomeNonStandard)]
        [Parameter("回零模式代码(不支持负数,负数请输入255+负数,如-3输入252)", 22, CN = "回零模式", DefaultV = (short)0)]
        public short HomeMode { get; set; }

        [DependOn("ActionType", VCMActionType.HomeNonStandard)]
        [Parameter("回零高速(mm/s)", 23, CN = "回零高速", DefaultV = 50.0)]
        public double HomeSpeed { get; set; }

        [DependOn("ActionType", VCMActionType.HomeNonStandard)]
        [Parameter("回零低速(mm/s)", 24, CN = "回零低速", DefaultV = 10.0)]
        public double HomeLowSpeed { get; set; }

        [DependOn("ActionType", VCMActionType.HomeNonStandard)]
        [Parameter("回零加速度(mm/s²)", 25, CN = "回零加速度", DefaultV = 1000.0)]
        public double HomeAcc { get; set; }

        [DependOn("ActionType", VCMActionType.HomeNonStandard)]
        [Parameter("碰撞回零电流阈值(千分比)", 26, CN = "碰撞电流阈值", DefaultV = 500)]
        public int HomeCollisionCurrent { get; set; }

        [DependOn("ActionType", VCMActionType.HomeNonStandard)]
        [Parameter("碰撞电流检测时间(ms)", 27, CN = "电流检测时间", DefaultV = 100)]
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

        // SAC-N2双轴控制器: 轴二地址偏移 +0x800
        private const int AxisOffset = 0x800;

        /// <summary>
        /// 根据SlaveNum返回轴专属的SDO地址(轴二+0x800)
        /// </summary>
        private short Addr(short baseAddr)
        {
            return SlaveNum == SlaveID.NUM2 ? (short)(baseAddr + AxisOffset) : baseAddr;
        }

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

            uint velHi = (uint)(HomeSpeed * _axis.PerPluse);
            uint velLo = (uint)(HomeLowSpeed * _axis.PerPluse);
            uint accUint = (uint)(HomeAcc * _axis.PerPluse);

            // 1. 设置回零参数(模式暂用34占位)
            short ret = ecat_motion.M_SetHomingPrm(axis, 34, 0, velHi, velLo, accUint, 0, card);
            if (ret != 0)
            {
                OutResult = false;
                OutFailReason = $"非标回零: 设置回零参数失败, 错误码: {ret}";
                return;
            }

            // 2. SDO写入正确的回零模式、堵转电流、堵转时间(先写参数再切模式)
            _axis.SDOWrite(Addr(0x6098), 0, HomeMode, 1);
            _axis.SDOWrite(Addr(0x5000), 6, HomeCollisionTime, 2);
            _axis.SDOWrite(Addr(0x5000), 5, HomeCollisionCurrent, 2);

            // 3. 切换至回零模式(Mode=6)
            ret = ecat_motion.M_SetHomingMode(axis, 6, card);
            Thread.Sleep(50);
            if (ret != 0)
            {
                OutResult = false;
                OutFailReason = $"非标回零: 切换回零模式失败, 错误码: {ret}";
                return;
            }

            // 4. 启动回零
            ret = ecat_motion.M_HomingStart(axis, card);
            if (ret != 0)
            {
                OutResult = false;
                OutFailReason = $"非标回零: 启动回零失败, 错误码: {ret}";
                return;
            }

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
            _axis.MoveAbs(TargetPosition[0].Position, MoveSpeed, MoveAcc, MoveDec);
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

        #region 软着陆(两段速, 参考DH Control Demo)

        /// <summary>
        /// 读取0x6077原始电流值(千分比)
        /// </summary>
        private int ReadRawCurrent()
        {
            _axis.SDORead(Addr(0x6077), 0, 2, out int currentValue, 1);
            return currentValue;
        }

        /// <summary>
        /// 写入扭矩限制(SDO 0x5018/0x5818)
        /// </summary>
        private void WriteTorqueLimit(int value)
        {
            _axis.SDOWrite(Addr(0x5018), 0, value, 2);
        }

        /// <summary>
        /// 读取当前扭矩限制
        /// </summary>
        private int ReadTorqueLimit()
        {
            _axis.SDORead(Addr(0x5018), 0, 2, out int value, 1);
            return value;
        }

        /// <summary>
        /// 读取压力反馈值(0x6077电流 × K + B)
        /// </summary>
        private double ReadPressure()
        {
            return ReadRawCurrent() * PressureCalibrationK + PressureCalibrationB;
        }

        /// <summary>
        /// 软着陆(参考DH Control Demo的SoftLand_ServoExternal)
        /// 流程: 力矩最大 → 快进PP → 设目标力矩 → 慢速PT → 等力矩到达 → 保压 → 返回PB → 解除力矩
        /// </summary>
        private void ExecuteSoftLanding()
        {
            const int MaxTorque = 3000;
            int defaultTorqueLimit = ReadTorqueLimit();

            try
            {
                // Step 0: 设定扭矩限制为最大
                WriteTorqueLimit(MaxTorque);
                Thread.Sleep(20);

                // Step 10: 快速段 - 快速接近产品上方(PP位置)
                _axis.MoveAbs(PPPosition[0].Position, PPVelocity, MoveAcc, MoveDec);
                _axis.CheckMotionDone();
                if (_isBreak) return;

                // Step 20: 设定扭矩限制为目标值
                WriteTorqueLimit(TorqueLimit);
                Thread.Sleep(20);

                // Step 30: 慢速段 - 低速接触产品(PT位置)
                _axis.MoveAbs(PTPosition[0].Position, PTVelocity, MoveAcc, MoveDec);

                // 等待力矩到达(接触判定)
                double lastPos = _axis.GetCurrentPos();
                int elapsed = 0;
                int timeoutMs = SoftLandingTimeout * 1000;
                bool torqueReached = false;

                while (elapsed < timeoutMs)
                {
                    if (_isBreak) return;

                    Thread.Sleep(10);
                    elapsed += 10;

                    int rawCurrent = ReadRawCurrent();
                    double position = _axis.GetCurrentPos();
                    double speed = Math.Abs(position - lastPos) * 100; // mm/10ms → mm/s
                    lastPos = position;

                    if (Math.Abs(rawCurrent - TorqueLimit) <= TorqueTolerance && speed <= SpeedThreshold)
                    {
                        torqueReached = true;
                        break;
                    }
                }

                if (!torqueReached)
                {
                    _axis.Stop();
                    Thread.Sleep(50);
                    OutPosition = _axis.GetCurrentPos();
                    OutPressure = ReadPressure();
                    OutResult = false;
                    OutFailReason = $"软着陆力矩未到达, 超时({SoftLandingTimeout}秒)";
                    return;
                }

                // Step 40: 保压
                Thread.Sleep(InstallTime);
                double installPos = _axis.GetCurrentPos();
                //在这读取压力
                OutPressure = ReadRawCurrent() * PressureCalibrationK + PressureCalibrationB;

                // Step 50: 返回段 - 快速返回PB位置
                _axis.MoveAbs(PBPosition[0].Position, PPVelocity, MoveAcc, MoveDec);

                // 等离开接触面后再解除力矩限制(避免突然释放导致过冲)
                int waitElapsed = 0;
                while (waitElapsed < 3000)
                {
                    if (_isBreak) return;
                    Thread.Sleep(10);
                    waitElapsed += 10;
                    if (Math.Abs(_axis.GetCurrentPos() - installPos) > 0.5) break;
                }

                WriteTorqueLimit(MaxTorque);
                _axis.CheckMotionDone();
                if (_isBreak) return;

                // Step 100: 完成
                OutPosition = _axis.GetCurrentPos();
                //OutPressure = ReadRawCurrent() * PressureCalibrationK + PressureCalibrationB;
                OutResult = true;
            }
            finally
            {
                WriteTorqueLimit(defaultTorqueLimit);
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

        public override void OnNotifyPropertyUIChanged(ParameterAttribute parameter, object newV)
        {
            base.OnNotifyPropertyUIChanged(parameter, newV);

            if (newV is VAxisPos vPos)
            {
                vPos.UpdateAxis(MyOwner.DeviceEngine);
                BuildDynamicAxisPos(vPos, 5);
                BuildDynamicAxisPos(vPos, 20, TaskFlow.Common.Enums.ParamType.OUT, false);
            }
        }
    }
}
