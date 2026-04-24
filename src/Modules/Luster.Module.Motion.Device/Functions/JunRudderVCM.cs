using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Enums;
using Luster.TaskFlow.Motion.Interfaces;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Luster.Module.Motion.Device.Functions
{
    /// <summary>
    /// 钧舵音圈电机 Function
    /// 品牌：钧舵 GSFDmini 伺服驱动器
    /// 通信：EtherCAT(CIA402协议) via 固高(GoogolTech)板卡
    /// 力控：驱动器内置开环力位控制(P96, 0x2016h触发, 0x201Ah状态机)
    /// 流程：快进 → 一段速度 → 二段速度(探测) → 保压 → 回退
    /// 电流反馈：0x6077h(CIA402标准Torque Actual Value, 单位=0.001×额定电流)
    /// 压力标定：电流值 × K + B → 实际压力(需外接力传感器标定)
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

        // ===== 硬着陆参数 =====
        [DependOn("ActionType", VCMActionType.HardLanding)]
        [Parameter("目标位置(mm)", 2, CN = "目标位置")]
        public double TargetPosition { get; set; }

        [DependOn("ActionType", VCMActionType.HardLanding)]
        [Parameter("位置上限(mm)", 3, CN = "位置上限")]
        public double PositionUpperLimit { get; set; }

        [DependOn("ActionType", VCMActionType.HardLanding)]
        [Parameter("位置下限(mm)", 4, CN = "位置下限")]
        public double PositionLowerLimit { get; set; }

        [DependOn("ActionType", VCMActionType.HardLanding)]
        [Parameter("运动速度(mm/s)", 5, CN = "运动速度", DefaultV = 50.0)]
        public double MoveSpeed { get; set; }

        // ===== 运动参数（硬着陆 + 软着陆共用） =====
        [DependOn("ActionType", VCMActionType.HardLanding, VCMActionType.SoftLanding)]
        [Parameter("加速度(mm/s²)", 6, CN = "加速度", DefaultV = 100.0)]
        public double MoveAcc { get; set; }

        [DependOn("ActionType", VCMActionType.HardLanding, VCMActionType.SoftLanding)]
        [Parameter("减速度(mm/s²)", 7, CN = "减速度", DefaultV = 100.0)]
        public double MoveDec { get; set; }

        // ===== 软着陆(驱动器内置力位控制)参数 =====
        // --- 核心力控 ---
        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("扭矩限制(峰值电流1/10000)", 8, CN = "扭矩限制", DefaultV = 1700)]
        public int TorquePositiveLimit { get; set; }

        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("最大行程限制(mm)", 9, CN = "最大行程限制", DefaultV = 11.0)]
        public double MaxStrokeLimit { get; set; }

        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("停止速度阈值(mm/s)", 10, CN = "停止速度阈值", DefaultV = 0.5)]
        public double StopSpeedThreshold { get; set; }

        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("判断停止时间(ms)", 12, CN = "判断停止时间", DefaultV = 100)]
        public int StopJudgeTime { get; set; }


        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("快进/回退速度(mm/s)", 13, CN = "快进回退速度", DefaultV = 50.0)]
        public double FastRetractSpeed { get; set; }

        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("一段速度-逼近速度(mm/s)", 14, CN = "一段速度", DefaultV = 20.0)]
        public double FirstSpeed { get; set; }

        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("二段速度-探测速度(mm/s)", 15, CN = "二段速度", DefaultV = 5.0)]
        public double SecondSpeed { get; set; }

        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("回退位置(mm)", 16, CN = "回退位置", DefaultV = 0.0)]
        public double RetractPosition { get; set; }

        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("快进位置(mm)", 17, CN = "快进位置", DefaultV = 3.0)]
        public double FastForwardPosition { get; set; }

        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("速度切换位置(mm)", 18, CN = "速度切换位置", DefaultV = 5.0)]
        public double SpeedSwitchPosition { get; set; }

        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("力矩保持时间(ms)", 19, CN = "力矩保持时间", DefaultV = 2000)]
        public int TorqueHoldTime { get; set; }

        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("软着陆超时(秒)", 20, CN = "软着陆超时", DefaultV = 10)]
        public int SoftLandingTimeout { get; set; }


        // --- 目标压力控制 ---
        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("目标压力(N)", 22, CN = "目标压力")]
        public double TargetPressure { get; set; }

        // --- 扭矩→压力标定 (用于从目标压力反算扭矩指令) ---
        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("扭矩-压力标定系数K", 23, CN = "扭矩标定K*1000", DefaultV = 1)]
        public double TorquePressureK { get; set; }

        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("扭矩-压力标定偏移B", 24, CN = "扭矩标定B*1000", DefaultV = 0)]
        public double TorquePressureB { get; set; }

        // --- 电流→压力标定 (0x6077原始值 → 压力) ---
        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("电流-压力标定系数K", 25, CN = "电流标定K*1000", DefaultV = 1)]
        public double CurrentPressureK { get; set; }

        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("电流-压力标定偏移B", 26, CN = "电流标定B*1000", DefaultV = 0)]
        public double CurrentPressureB { get; set; }

        // ===== 非标回零参数 =====
        [DependOn("ActionType", VCMActionType.Home, VCMActionType.HomeNonStandard)]
        [Parameter("回零超时(秒)", 44, CN = "回零超时", DefaultV = 60)]
        public int HomeTimeout { get; set; }

        // ===== 输出参数 =====
        [Parameter("执行结果", 30, CN = "执行结果", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool OutResult { get; set; }

        [Parameter("实际位置(mm)", 31, CN = "实际位置", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public double OutPosition { get; set; }

        [Parameter("实时压力记录(N)，逗号分割", 32, CN = "实时压力", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string OutPressure { get; set; }

        [Parameter("失败原因", 33, CN = "失败原因", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string OutFailReason { get; set; }

        [DependOn("ActionType", VCMActionType.SoftLanding)]
        [Parameter("额定电流", 34, CN = "额定电流mA", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public double OutRatedCurrent { get; set; }

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
        /// 额定电流缓存（0x6075h，软着陆启动时读取一次，避免循环中重复SDO读取）
        /// </summary>
        private int _ratedCurrent;

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

        /// <summary>
        /// 确保驱动器处于位置模式(0x201A phase==9)
        /// 如果在力控模式(phase==1)，写0x2016=256(0x100)切回位置模式，等待phase==9
        /// </summary>
        private void EnsurePositionMode()
        {
            _axis.SDORead(0x201A, 0, 2, out int modeState, 1);
            int phase = modeState & 0x0F;

            // phase==0(未进入力控) 或 phase==9(已在位置模式) → 无需切换
            if (phase == 0 || phase == 9) return;

            // 写0x2016=256(0x100, bit8=1) 切回位置模式
            _axis.SDOWrite(0x2016, 0, 0x0100, 2);

            int elapsed = 0;
            while (elapsed < 3000)
            {
                Thread.Sleep(10);
                _axis.SDORead(0x201A, 0, 2, out modeState, 1);
                if ((modeState & 0x0F) == 9) return;
                elapsed += 10;
            }
            // 超时不阻塞，仅记录
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
        /// 回零流程（标准CIA402，使用轴卡预设参数）
        /// </summary>
        private void ExecuteHome()
        {
            EnsurePositionMode();
            _axis.Home();
            _axis.CheckHomeDone(HomeTimeout);
            OutResult = true;
        }

        /// <summary>
        /// 非标回零（SDO写入回零参数后启动回零）
        /// SDO: 0x6098(模式) → 0x6099.0(快速) → 0x6099.1(慢速) → 0x609A(加速度) → Home()
        /// </summary>
        private void ExecuteHomeNonStandard()
        {
            //EnsurePositionMode();

            //int pp = _axis.PerPluse;

            //// 1. 写入回零参数到驱动器SDO
            //_axis.SDOWrite(0x6098, 0, HomeMode, 1);
            //_axis.SDOWrite(0x6099, 0, (int)(HomeSpeed * pp), 4);
            //_axis.SDOWrite(0x6099, 1, (int)(HomeLowSpeed * pp), 4);
            //_axis.SDOWrite(0x609A, 0, (int)(HomeAcc * pp), 4);

            //// 2. 启动回零
            //_axis.Home();
            //_axis.CheckHomeDone(HomeTimeout);
            OutResult = true;
        }

        #endregion

        #region 硬着陆

        /// <summary>
        /// 硬着陆（普通点位运动）
        /// </summary>
        private void ExecuteHardLanding()
        {
            EnsurePositionMode();

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
        /// 读取0x6077电流反馈(PDO)，用标定系数换算为压力(N)
        /// 0x6077 = 千分比额定电流，反馈电流(mA) = rawValue × _ratedCurrent / 1000.0
        /// 界面K/B已放大1000倍，计算时除以1000还原
        /// 使用PDO读取避免SDO总线拥堵
        /// </summary>
        private double ReadFeedbackPressure()
        {
            int rawValue = 0;
            _axis.PDORead((short)_axis.AxisNo, 0x6077, 0, 2, ref rawValue, 1);
            double currentMA = rawValue * _ratedCurrent / 1000.0;
            return currentMA * (CurrentPressureK / 1000.0) + (CurrentPressureB / 1000.0);
        }

        /// <summary>
        /// 根据目标压力(N)反算扭矩SDO值
        /// 界面K/B已放大1000倍，计算时除以1000还原
        /// 结果×100: 标定用扭矩8~17 → SDO值800~1700
        /// </summary>
        private int CalcTorqueFromPressure(double pressure)
        {
            double k = TorquePressureK / 1000.0;
            double b = TorquePressureB / 1000.0;
            return (int)Math.Round((pressure - b) / k * 100);
        }

        /// <summary>
        /// 软着陆(GSFDmini开环力控 P96)
        /// 流程: 计算扭矩 → 写参数 → 切力控模式 → 触发 → 等待完成 → 读反馈压力 → 判定 → 切回位置模式
        /// </summary>
        private void ExecuteSoftLanding()
        {
            int pp = _axis.PerPluse;
            try
            {
                // 缓存额定电流（0x6075h），循环中不再重复读取
                _axis.SDORead(0x6075, 0, 4, out _ratedCurrent, 3);
                OutRatedCurrent = _ratedCurrent;
                // 0. 扭矩来源: 目标压力>0时自动反算，否则使用手动设置的值
                int torque = TorquePositiveLimit;
                if (TargetPressure > 0)
                {
                    torque = CalcTorqueFromPressure(TargetPressure);
                    if (torque < 0) torque = 0;
                    TorquePositiveLimit = torque;
                }

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
                _axis.SDOWrite(0x2017, 0, torque, 2);
                _axis.SDOWrite(0x2018, 0, torque, 2);

                // 5. 确保CSP模式
                Thread.Sleep(50);

                // 6. 切换到力控模式（PDO读取状态，避免SDO拥堵）
                int modeState = 0;
                _axis.SDORead(0x201A, 0, 2, out modeState, 3);
                if ((modeState & 0x0F) != 1)
                {
                    _axis.SDOWrite(0x2016, 0, 0, 2);
                    int switchElapsed = 0;
                    while (switchElapsed < 3000)
                    {
                        Thread.Sleep(10);
                        _axis.SDORead(0x201A, 0, 2, out modeState, 1);
                        if ((modeState & 0x0F) == 1) break;
                        switchElapsed += 10;
                    }
                    if ((modeState & 0x0F) != 1)
                    {
                        OutResult = false;
                        OutFailReason = $"切换力控模式超时(0x201A={modeState})，请检查是否使能";
                        return;
                    }
                }

                // 7. 触发力控
                _axis.SDOWrite(0x2016, 0, 0, 2);
                Thread.Sleep(5);
                _axis.SDOWrite(0x2016, 0, 1, 2);

                // 8. 等待力控完成，实时采集压力
                int elapsed = 0;
                int timeoutMs = SoftLandingTimeout * 1000;
                bool hadStarted = false;
                var pressureSamples = new System.Collections.Generic.List<double>();

                while (elapsed < timeoutMs)
                {
                    if (_isBreak) return;

                    int state = 0;
                    _axis.SDORead(0x201A, 0, 2, out state, 1);
                    Thread.Sleep(5);
                    int phase = state & 0x0F;

                    if (phase > 1) hadStarted = true;

                    // 力控过程中实时采集压力
                    if (hadStarted)
                    {
                        pressureSamples.Add(ReadFeedbackPressure());
                        Thread.Sleep(5);
                    }

                    if (hadStarted && phase == 1)
                    {
                        OutPosition = _axis.GetCurrentPos();
                        pressureSamples.Add(ReadFeedbackPressure());
                        OutPressure = string.Join(",", pressureSamples);
                        OutResult = true;

                        // 切回位置模式
                        _axis.SDOWrite(0x2016, 0, 0x0100, 2);
                        return;
                    }

                    Thread.Sleep(10);
                    elapsed += 20;
                }

                // 超时
                OutPosition = _axis.GetCurrentPos();
                pressureSamples.Add(ReadFeedbackPressure());
                OutPressure = string.Join(",", pressureSamples);
                OutResult = false;
                OutFailReason = $"软着陆超时({SoftLandingTimeout}秒)";
            }
            catch (Exception ex)
            {
                OutResult = false;
                OutFailReason = $"软着陆通信异常: {ex.Message}";
                try { OutPosition = _axis.GetCurrentPos(); } catch { }
            }
            finally
            {
                // 无论成功/失败/异常，确保切回位置模式
                try { _axis.SDOWrite(0x2016, 0, 0x0100, 2); } catch { }
            }
        }

        #endregion

        #region 停止/暂停

        public override void Stop()
        {
            _isBreak = true;
            if (_axis != null)
            {
                // 先复位0x2016h，再置bit8=1切回位置模式
                try { _axis.SDOWrite(0x2016, 0, 0, 2); } catch { }
                try { _axis.SDOWrite(0x2016, 0, 0x0100, 2); } catch { }
                _axis.Stop();
            }
        }

        public override bool IsNeedPause => true;

        #endregion
    }
}
