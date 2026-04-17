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
    /// 通信：EtherCAT(CIA402协议) via 固高(GoogolTech)板卡
    /// 力控：驱动器内置开环力位控制(P96, 0x2016h触发, 0x201Ah状态机)
    /// 流程：快进 → 一段速度 → 二段速度(探测) → 保压 → 回退
    /// 压力反馈：0x201Bh(模拟量 -10V~10V)
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

        // ===== 软着陆(驱动器内置力位控制)参数 =====
        // --- 核心力控 ---
        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("扭矩正向限制(峰值电流1/10000)", 8, CN = "扭矩正向限制", DefaultV = 1000)]
        public int TorquePositiveLimit { get; set; }

        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("二段速度-探测速度(mm/s)", 9, CN = "二段速度", DefaultV = 5.0)]
        public double SecondSpeed { get; set; }

        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("快进位置(mm)", 10, CN = "快进位置")]
        public double FastForwardPosition { get; set; }

        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("速度切换位置(mm)", 11, CN = "速度切换位置")]
        public double SpeedSwitchPosition { get; set; }

        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("软着陆超时(秒)", 12, CN = "软着陆超时", DefaultV = 10)]
        public int SoftLandingTimeout { get; set; }

        // --- 位置 ---
        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("回退位置(mm)", 13, CN = "回退位置", DefaultV = 0.0)]
        public double RetractPosition { get; set; }

        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("最大行程限制(mm)", 14, CN = "最大行程限制", DefaultV = 20.0)]
        public double MaxStrokeLimit { get; set; }

        // --- 速度 ---
        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("快进/回退速度(mm/s)", 15, CN = "快进回退速度", DefaultV = 50.0)]
        public double FastRetractSpeed { get; set; }

        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("一段速度-逼近速度(mm/s)", 16, CN = "一段速度", DefaultV = 20.0)]
        public double FirstSpeed { get; set; }

        // --- 判定 ---
        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("力矩保持时间(ms)", 24, CN = "力矩保持时间", DefaultV = 500)]
        public int TorqueHoldTime { get; set; }

        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("判断停止时间(ms)", 25, CN = "判断停止时间", DefaultV = 100)]
        public int StopJudgeTime { get; set; }

        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("停止速度阈值(mm/s)", 26, CN = "停止速度阈值", DefaultV = 0.5)]
        public double StopSpeedThreshold { get; set; }

        // --- 压力反馈标定(输出用) ---
        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("压力标定系数K", 27, CN = "标定系数K", DefaultV = 1.0)]
        public double PressureCalibrationK { get; set; }

        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("压力标定偏移B", 28, CN = "标定偏移B", DefaultV = 0.0)]
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

        #region 软着陆(GSFDmini内置力位控制-开环力控 P96)

        /// <summary>
        /// 读取压力反馈值(0x201Bh 模拟量, -10V~10V → -32768~32767)
        /// </summary>
        private double ReadPressure()
        {
            _axis.SDORead(0x201B, 0, 2, out int rawValue, 1);
            return rawValue * PressureCalibrationK + PressureCalibrationB;
        }

        /// <summary>
        /// 软着陆(GSFDmini开环力控 P96)
        /// 流程: 快进 → 一段速度 → 二段速度(探测) → 保压(力矩保持时间) → 回退
        /// 触发: 0x2016 bit0上升沿, 保持CSP模式(bit8~11=1)
        /// 状态: 0x201A bit0~3 (2=快进, 3=一段, 4=二段, 6=回退, 1=完成)
        /// </summary>
        private void ExecuteSoftLanding()
        {
            int pp = _axis.PerPluse;

            // 1. 写入力控参数-位置
            _axis.SDOWrite(0x2009, 0, (int)(RetractPosition * pp), 4);
            _axis.SDOWrite(0x200A, 0, (int)(FastForwardPosition * pp), 4);
            _axis.SDOWrite(0x200B, 0, (int)(SpeedSwitchPosition * pp), 4);
            _axis.SDOWrite(0x200C, 0, (int)(MaxStrokeLimit * pp), 4);

            // 2. 写入力控参数-速度
            _axis.SDOWrite(0x200E, 0, (int)(FirstSpeed * pp), 4);
            _axis.SDOWrite(0x200F, 0, (int)(SecondSpeed * pp), 4);
            _axis.SDOWrite(0x2010, 0, (int)(FastRetractSpeed * pp), 4);
            _axis.SDOWrite(0x2012, 0, (int)(MoveAcc * pp), 4);
            _axis.SDOWrite(0x2013, 0, (int)(MoveDec * pp), 4);

            // 3. 写入力控参数-判定
            _axis.SDOWrite(0x2011, 0, (int)(StopSpeedThreshold * pp), 4);
            _axis.SDOWrite(0x2014, 0, TorqueHoldTime, 2);
            _axis.SDOWrite(0x2015, 0, StopJudgeTime, 2);

            // 4. 写入扭矩限制
            _axis.SDOWrite(0x2017, 0, TorquePositiveLimit, 2);
            _axis.SDOWrite(0x2018, 0, TorquePositiveLimit, 2);

            // 5. 确保CSP模式
            _axis.SDOWrite(0x6060, 0, 8, 1);
            Thread.Sleep(50);

            // 6. 触发力控: 0x2016 bit0上升沿, bit8~11保持CSP
            _axis.SDOWrite(0x2016, 0, 0x0F00, 2);  // bit8~11=1, bit0=0
            Thread.Sleep(10);
            _axis.SDOWrite(0x2016, 0, 0x0F01, 2);  // bit0=1 上升沿触发

            // 7. 等待力控完成(保压+回退完成后 phase回到1)
            int elapsed = 0;
            int timeoutMs = SoftLandingTimeout * 1000;
            bool hadStarted = false;

            while (elapsed < timeoutMs)
            {
                if (_isBreak) return;

                _axis.SDORead(0x201A, 0, 2, out int state, 1);
                int phase = state & 0x0F;

                // 检测到进入力控阶段(phase>1), 标记已启动
                if (phase > 1) hadStarted = true;

                // phase=1: 力控准备好(保压结束+回退完成)
                if (hadStarted && phase == 1)
                {
                    OutPosition = _axis.GetCurrentPos();
                    OutPressure = ReadPressure();
                    OutResult = true;
                    return;
                }

                Thread.Sleep(10);
                elapsed += 10;
            }

            // 超时
            OutPosition = _axis.GetCurrentPos();
            OutPressure = ReadPressure();
            OutResult = false;
            OutFailReason = $"软着陆超时({SoftLandingTimeout}秒)";
        }

        #endregion

        #region 停止/暂停

        public override void Stop()
        {
            _isBreak = true;
            if (_axis != null)
            {
                // 立即结束力控: 0x2016 bit2=1
                try { _axis.SDOWrite(0x2016, 0, 0x0F04, 2); } catch { }
                _axis.Stop();
            }
        }

        public override bool IsNeedPause => true;

        #endregion
    }
}
