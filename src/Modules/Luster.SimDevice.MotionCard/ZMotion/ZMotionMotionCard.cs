using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Real;
using Luster.SimDevice.MotionCards;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Luster.SimDevice.MotionCard.ZMotion
{
    /// <summary>
    /// 正运动 ZMotion 运动控制卡。
    /// </summary>
    public class ZMotionMotionCard : MotionCardBase, IMotionCard, IFiveAxisRTCP, IFiveAxisContiInterp, IFiveAxisLatch
    {
        private readonly IZMotionSdk sdk;
        private readonly Dictionary<int, double> currentPositions = new Dictionary<int, double>();
        private readonly Dictionary<int, bool> homeDone = new Dictionary<int, bool>();
        private readonly Dictionary<int, bool> servoStatus = new Dictionary<int, bool>();
        private readonly Dictionary<int, bool> digitalInputs = new Dictionary<int, bool>();
        private readonly Dictionary<int, bool> digitalOutputs = new Dictionary<int, bool>();
        private readonly Dictionary<int, double> analogInputs = new Dictionary<int, double>();
        private readonly Dictionary<int, double> analogOutputs = new Dictionary<int, double>();
        private readonly Dictionary<string, int> sdoValues = new Dictionary<string, int>();
        private readonly Dictionary<string, int> pdoValues = new Dictionary<string, int>();
        private IntPtr cardHandle = IntPtr.Zero;
        private bool interpolationDone = true;

        // ===== P5-3 连续插补 + 高速锁存状态 =====
        // 连续插补器数据表起始地址(对应源端 GetCrdProfile().CrdAddr.MoveOpAddr)。lmv 侧不维护完整 CrdProfile,
        // 用一个固定数据表基地址承载 ReadContiOutFlag/锁存计数回读(与源端 ZMCMotion GetTable 调用同构)。
        private const int ContiMoveOpTableBase = 0;
        // 锁存数据表布局:[0]=已锁存计数,[1..]=锁存位置序列(对齐源端 GetHighLatchedCount/GetHighLatchedValue)。
        private const int LatchCountTableBase = 100;
        // 虚拟分支:锁存触发点回放队列(按注入点位递增),key=latchIndex。
        private readonly Dictionary<int, Queue<double>> virtualLatchQueues = new Dictionary<int, Queue<double>>();
        // 虚拟分支:连续插补输出标志回读计数(按注入点位递增,ADR v2 确定性桩值)。
        private int virtualContiOutFlagIndex;

        public ZMotionMotionCard() : this(new ZMotionSdk())
        {
        }

        internal ZMotionMotionCard(IZMotionSdk sdk)
        {
            this.sdk = sdk;
            Ip = "192.168.0.10";
            AxisCount = 8;
            DigitalInCount = 0;
            DigitalOutCount = 0;
            AnalogInCount = 0;
            AnalogOutCount = 0;
            LogPath = "D:\\LocalLog";
        }

        public override string Brand => "正运动";

        [DisplayName("IP地址")]
        public string Ip { get; set; }

        [DisplayName("模拟模式")]
        public bool SimulationMode { get; set; }

        [DisplayName("轴数量")]
        public int AxisCount { get; set; }

        [DisplayName("数字输入数量")]
        public int DigitalInCount { get; set; }

        [DisplayName("数字输出数量")]
        public int DigitalOutCount { get; set; }

        [DisplayName("模拟输入数量")]
        public int AnalogInCount { get; set; }

        [DisplayName("模拟输出数量")]
        public int AnalogOutCount { get; set; }

        [DisplayName("日志模式")]
        public int LogMode { get; set; }

        [DisplayName("日志路径")]
        public string LogPath { get; set; }

        public FiveAxisRtcpConfig RtcpConfig { get; private set; }

        public bool RtcpEnabled { get; private set; }

        public override void InitApi()
        {
            if (HasInit())
            {
                OnLog(LogType.Info, $"{Brand}_{ID} has initialize!");
                return;
            }

            if (SimulationMode)
            {
                OnStatusChanged(DeviceStatus.Online);
                return;
            }

            SafeNativeMethod(() =>
            {
                var result = sdk.OpenEth(Ip, out cardHandle);
                if (result != 0)
                {
                    OnLog(LogType.Error, $"正运动卡连接失败,错误码:{result}");
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(LogPath))
                {
                    result = sdk.SetTraceFile(LogMode, LogPath);
                    if (result != 0)
                    {
                        OnLog(LogType.Error, $"正运动日志设置失败,错误码:{result}");
                        return false;
                    }
                }

                return true;
            }, "正运动卡初始化失败");
            OnStatusChanged(DeviceStatus.Online);
        }

        public void ScanAxis(out uint axisNum)
        {
            CheckInit();
            axisNum = (uint)Math.Max(0, AxisCount);
        }

        public void ScanDigitalIO(out uint digitalIn, out ushort digitalOut)
        {
            CheckInit();
            digitalIn = (uint)Math.Max(0, DigitalInCount);
            digitalOut = (ushort)Math.Max(0, DigitalOutCount);
        }

        public void ScanAnglog(out ushort anglogIn, out ushort anglogOut)
        {
            CheckInit();
            anglogIn = (ushort)Math.Max(0, AnalogInCount);
            anglogOut = (ushort)Math.Max(0, AnalogOutCount);
        }

        public bool CheckMotionDone(int precision, int axisNo = 0, double targetPulse = 0)
        {
            CheckInit();
            if (axisNo == -1)
            {
                if (SimulationMode) return interpolationDone;
                var idle = 0;
                SafeNativeMethod(() => sdk.GetIfIdle(cardHandle, 0, ref idle) == 0, "读取插补状态失败");
                return idle != 0;
            }

            CheckAxisNo(axisNo);
            if (SimulationMode)
            {
                // 模拟模式下假设目标位置已被 Move/MoveLine 写入 currentPositions，运动即视为完成。
                return true;
            }

            var axisIdle = 0;
            SafeNativeMethod(() => sdk.GetIfIdle(cardHandle, axisNo, ref axisIdle) == 0, $"读取轴{axisNo}运动状态失败");
            return axisIdle != 0;
        }

        public double GetCurrentPos(int axisNo, double perPulse)
        {
            CheckInit();
            CheckAxisNo(axisNo);
            if (SimulationMode) return GetAxisPosition(axisNo);

            float pulse = 0;
            SafeNativeMethod(() => sdk.GetDpos(cardHandle, axisNo, ref pulse) == 0, $"读取轴{axisNo}当前位置失败");
            return perPulse == 0 ? pulse : pulse / perPulse;
        }

        public override void SetCurrentPos(int axisNo, double perPulse, double position)
        {
            CheckInit();
            CheckAxisNo(axisNo);
            SetAxisPosition(axisNo, position);
            if (!SimulationMode)
            {
                SafeNativeMethod(() => sdk.SetDpos(cardHandle, axisNo, (float)ToPulse(position, perPulse)) == 0, $"设置轴{axisNo}当前位置失败");
            }
        }

        public void Home(int axisNo, HomeMode homeMode, double high, double low, double perPlus, double homeAcc, double Offset, AxisPML axisPML)
        {
            CheckInit();
            CheckAxisNo(axisNo);
            if (SimulationMode)
            {
                SetAxisPosition(axisNo, Offset);
                homeDone[axisNo] = true;
                return;
            }

            SafeNativeMethod(() => sdk.SingleDatum(cardHandle, axisNo, (int)homeMode) == 0, $"轴:{axisNo} 回零失败");
        }

        public void HomeCancel(int axisNo)
        {
            Stop(axisNo);
        }

        public override bool CheckHomeDone(int axisNo = 0)
        {
            CheckInit();
            CheckAxisNo(axisNo);
            if (SimulationMode) return homeDone.TryGetValue(axisNo, out var done) && done;
            return CheckMotionDone(0, axisNo);
        }

        public void Jog(double vel, double acc, double dec, double perPlus, double slineTime, int axisNo, AxisPML axisPML)
        {
            CheckInit();
            CheckAxisNo(axisNo);
            if (SimulationMode)
            {
                SetAxisPosition(axisNo, GetAxisPosition(axisNo) + Math.Sign(vel));
                return;
            }

            SafeNativeMethod(() => sdk.SingleVMove(cardHandle, axisNo, vel >= 0 ? 1 : -1) == 0, $"轴:{axisNo} Jog运动失败");
        }

        public void Move(double pos, double vel, double acc, double dec, double perPlus, double slineTime, bool isAbsMove, int axisNo, AxisPML axisPML)
        {
            CheckInit();
            CheckAxisNo(axisNo);
            var target = isAbsMove ? pos : GetAxisPosition(axisNo) + pos;
            SetAxisPosition(axisNo, target);
            if (SimulationMode) return;

            var axes = new[] { axisNo };
            var positions = new[] { (float)target };
            SafeNativeMethod(() => (isAbsMove ? sdk.MoveAbs(cardHandle, 1, axes, positions) : sdk.Move(cardHandle, 1, axes, positions)) == 0,
                $"轴:{axisNo} 运动失败");
        }

        public void Stop(int axisNo, bool isAll = false)
        {
            CheckInit();
            if (SimulationMode) return;

            if (isAll)
            {
                for (var i = 1; i <= AxisCount; i++)
                {
                    var axis = i;
                    SafeNativeMethod(() => sdk.SingleCancel(cardHandle, axis, 2) == 0, $"轴:{axis} 停止失败");
                }
                return;
            }

            CheckAxisNo(axisNo);
            SafeNativeMethod(() => sdk.SingleCancel(cardHandle, axisNo, 2) == 0, $"轴:{axisNo} 停止失败");
        }

        public void MoveLine(List<int> axisId, List<double> pos, List<double> perPlusArr, List<double> vel, List<double> acc)
        {
            CheckAxisArgs(axisId, pos, nameof(MoveLine));
            interpolationDone = false;
            for (var i = 0; i < axisId.Count; i++)
            {
                SetAxisPosition(axisId[i], pos[i]);
            }
            interpolationDone = true;
            if (SimulationMode) return;

            var axes = axisId.ToArray();
            var positions = pos.Select(item => (float)item).ToArray();
            SafeNativeMethod(() => sdk.MoveAbsSp(cardHandle, axes.Length, axes, positions) == 0, "直线插补失败");
        }

        public void MoveCircle(List<int> axisId, List<double> pos, List<double> perPlusArr, List<double> vel, List<double> acc, double radius, short dir)
        {
            CheckAxisArgs(axisId, pos, nameof(MoveCircle));
            interpolationDone = false;
            for (var i = 0; i < axisId.Count; i++)
            {
                SetAxisPosition(axisId[i], pos[i]);
            }
            interpolationDone = true;
            if (SimulationMode) return;

            if (axisId.Count < 2 || pos.Count < 2)
            {
                throw new ArgumentException("圆弧插补至少需要两个轴和两个目标位置");
            }

            var axes = axisId.Take(2).ToArray();
            SafeNativeMethod(() => sdk.MoveCircAbsSp(cardHandle, axes.Length, axes, (float)pos[0], (float)pos[1], (float)radius, 0, dir) == 0, "圆弧插补失败");
        }

        public Dictionary<AxisStatus, bool> GetAxisStatus(int axisNo, bool IsThrowException = true)
        {
            CheckInit();
            CheckAxisNo(axisNo, IsThrowException);
            return new Dictionary<AxisStatus, bool>
            {
                { AxisStatus.Alarm, false },
                { AxisStatus.Normal, true },
                { AxisStatus.Moving, false },
                { AxisStatus.Pel, false },
                { AxisStatus.Mel, false },
                { AxisStatus.Org, homeDone.TryGetValue(axisNo, out var done) && done },
                { AxisStatus.Emg, false },
                { AxisStatus.SvOn, servoStatus.TryGetValue(axisNo, out var on) && on },
                { AxisStatus.OnPos, true },
                { AxisStatus.IsHome, homeDone.TryGetValue(axisNo, out done) && done },
            };
        }

        public void ServOn(int axisNo, bool isOn)
        {
            CheckInit();
            CheckAxisNo(axisNo);
            servoStatus[axisNo] = isOn;
            if (!SimulationMode)
            {
                SafeNativeMethod(() => sdk.SetAxisEnable(cardHandle, axisNo, isOn ? 1 : 0) == 0, $"轴:{axisNo} 使能设置失败");
            }
        }

        public void ResetState(int axisNo)
        {
            CheckInit();
            CheckAxisNo(axisNo);
        }

        public override void ClearEmg()
        {
        }

        public bool GetDigitalIn(int index)
        {
            CheckInit();
            return digitalInputs.TryGetValue(index, out var value) && value;
        }

        public bool GetDigitalOut(int index)
        {
            CheckInit();
            return digitalOutputs.TryGetValue(index, out var value) && value;
        }

        public void SetDigitalOut(int index, bool digitalOut)
        {
            CheckInit();
            digitalOutputs[index] = digitalOut;
        }

        public double GetAnalogIn(int index)
        {
            CheckInit();
            return analogInputs.TryGetValue(index, out var value) ? value : 0;
        }

        public double GetAnalogOut(int index)
        {
            CheckInit();
            return analogOutputs.TryGetValue(index, out var value) ? value : 0;
        }

        public void SetAnalogOut(int index, double analogVal)
        {
            CheckInit();
            analogOutputs[index] = analogVal;
        }

        public void SDORead(short slave, short index, short subindex, short data_size, out int value, short count)
        {
            CheckInit();
            var key = BuildKey(slave, index, subindex, data_size);
            if (SimulationMode)
            {
                value = sdoValues.TryGetValue(key, out var saved) ? saved : 0;
                return;
            }

            var readValue = 0;
            SafeNativeMethod(() => sdk.SDORead(cardHandle, (uint)slave, (uint)index, (uint)subindex, (uint)data_size, ref readValue) == 0,
                $"SDO读取失败,slave={slave},index={index},subindex={subindex}");
            value = readValue;
        }

        public void SDOWrite(short slave, short index, short subindex, int data, short data_size)
        {
            CheckInit();
            var key = BuildKey(slave, index, subindex, data_size);
            sdoValues[key] = data;
            if (!SimulationMode)
            {
                SafeNativeMethod(() => sdk.SDOWrite(cardHandle, (uint)slave, (uint)index, (uint)subindex, (uint)data_size, data) == 0,
                    $"SDO写入失败,slave={slave},index={index},subindex={subindex}");
            }
        }

        public void PDORead(short axis, short index, short subindex, short data_size, ref int value, short count)
        {
            CheckInit();
            var key = BuildKey(axis, index, subindex, data_size);
            if (SimulationMode)
            {
                value = pdoValues.TryGetValue(key, out var saved) ? saved : 0;
                return;
            }

            var response = string.Empty;
            SafeNativeMethod(() => sdk.DirectCommand(cardHandle, $"?NODE_PDOBUFF(0,{axis},{index},{subindex},{data_size})", out response, 2048) == 0,
                $"PDO读取失败,axis={axis},index={index},subindex={subindex}");
            int.TryParse(response, out value);
        }

        public void PDOWrite(short axis, short index, short subindex, int data, short data_size)
        {
            CheckInit();
            var key = BuildKey(axis, index, subindex, data_size);
            pdoValues[key] = data;
            if (!SimulationMode)
            {
                var response = string.Empty;
                SafeNativeMethod(() => sdk.DirectCommand(cardHandle, $"NODE_PDOBUFF(0,{axis},{index},{subindex},{data_size}) = {data}", out response, 0) == 0,
                    $"PDO写入失败,axis={axis},index={index},subindex={subindex}");
            }
        }

        public void AxisContinuousMove(int axisNo, double acc, double dec, double perPulse, List<double> pos, List<double> vel)
        {
            CheckInit();
            CheckAxisNo(axisNo);
            if (pos == null || pos.Count == 0)
            {
                throw new ArgumentException("连续运动点位不能为空", nameof(pos));
            }

            SetAxisPosition(axisNo, pos[pos.Count - 1]);
            if (SimulationMode) return;

            var axes = new[] { axisNo };
            foreach (var point in pos)
            {
                var positions = new[] { (float)point };
                SafeNativeMethod(() => sdk.MoveAbsSp(cardHandle, 1, axes, positions) == 0, $"轴:{axisNo} 连续运动失败");
            }
        }

        public bool ConfigureRtcp(FiveAxisRtcpConfig config)
        {
            CheckInit();
            ValidateRtcpConfig(config);
            RtcpConfig = config;
            RtcpEnabled = false;
            if (SimulationMode) return true;

            var response = string.Empty;
            var vir = string.Join(",", config.VirtualAxisIds);
            var real = string.Join(",", config.RealAxisIds);
            var command = $"Z5Axes_Frame({config.CoordinateSystem},{vir},{real},{config.RotationCenterX},{config.RotationCenterY},{config.RotationCenterZ})";
            SafeNativeMethod(() => sdk.DirectCommand(cardHandle, command, out response, 0) == 0, "配置五轴RTCP失败");
            return true;
        }

        public bool SetRtcpEnabled(bool enabled)
        {
            CheckInit();
            if (RtcpConfig == null)
            {
                throw new InvalidOperationException("请先配置 RTCP 坐标系");
            }

            RtcpEnabled = enabled;
            if (SimulationMode) return true;

            var response = string.Empty;
            var vir = string.Join(",", RtcpConfig.VirtualAxisIds);
            var real = string.Join(",", RtcpConfig.RealAxisIds);
            var command = enabled ? $"Z5Axes_FrameRotate({vir},0,0,0,0,0,0)" : $"Z5Axes_ExitFrame({vir},{real})";
            SafeNativeMethod(() => sdk.DirectCommand(cardHandle, command, out response, 0) == 0, enabled ? "开启RTCP失败" : "关闭RTCP失败");
            return true;
        }

        #region IFiveAxisContiInterp —— 连续插补旁路(P5-3,对齐源端 ZMCMotion IoperateCrd 实现)

        /// <summary>
        /// 打开连续插补模式(源端 OpenCrdConti,底层 SetMerge=1)。
        /// </summary>
        public bool CrdContiOpen(int crd, int[] axisList, CrdMode mode)
        {
            CheckInit();
            if (axisList == null || axisList.Length == 0)
            {
                throw new ArgumentException("连续插补轴列表不能为空", nameof(axisList));
            }

            virtualContiOutFlagIndex = 0;
            if (SimulationMode) return true;

            SafeNativeMethod(() => sdk.SetMerge(cardHandle, crd, 1) == 0, $"开启连续插补失败,crd={crd}");
            return true;
        }

        public bool CrdContiStart(int crd)
        {
            CheckInit();
            // 源端 StartCrdConti 仅校验环境(SetMerge 已在 Open 配置),lmv 同构。
            return true;
        }

        /// <summary>
        /// 追加直线插补(源端 AddContiLine,底层 SetMovemark + MoveAbsSp/MoveSp)。
        /// </summary>
        public bool CrdContiAddLine(int crd, double[] endPos, ContiMoveMode mode)
        {
            CheckInit();
            if (endPos == null || endPos.Length == 0)
            {
                throw new ArgumentException("连续插补终点位置不能为空", nameof(endPos));
            }

            // 记录最后一个轴目标到虚拟位置(虚拟模式下轨迹推进可见)
            SetAxisPosition(crd, endPos[endPos.Length - 1]);
            if (SimulationMode) return true;

            SafeNativeMethod(() => sdk.SetMovemark(cardHandle, crd, 0) == 0, $"设置运动标记失败,crd={crd}");
            var axes = Enumerable.Range(0, endPos.Length).Select(_ => crd).ToArray();
            var positions = endPos.Select(item => (float)item).ToArray();
            SafeNativeMethod(() =>
                (mode == ContiMoveMode.Absolute ? sdk.MoveAbsSp(cardHandle, axes.Length, axes, positions) : sdk.MoveSp(cardHandle, axes.Length, axes, positions)) == 0,
                $"追加连续插补直线失败,crd={crd}");
            return true;
        }

        /// <summary>
        /// 追加延时(源端 AddContiDelay,底层 SetMovemark + MoveDelay)。
        /// </summary>
        public bool CrdContiAddDelay(int crd, int delayMs, int markIndex)
        {
            CheckInit();
            if (SimulationMode) return true;

            SafeNativeMethod(() => sdk.SetMovemark(cardHandle, crd, markIndex) == 0, $"设置运动标记失败,crd={crd},mark={markIndex}");
            SafeNativeMethod(() => sdk.MoveDelay(cardHandle, crd, delayMs) == 0, $"追加连续插补延时失败,crd={crd},ms={delayMs}");
            return true;
        }

        /// <summary>
        /// 追加同步输出(源端 AddContiOutput + AddContiOutFlag,底层 MoveOp + MoveTable)。
        /// </summary>
        public bool CrdContiAddOutput(int crd, int ioIndex, bool level, int markIndex)
        {
            CheckInit();
            if (SimulationMode)
            {
                virtualContiOutFlagIndex = markIndex;
                return true;
            }

            SafeNativeMethod(() => sdk.MoveOp(cardHandle, crd, ioIndex, level ? 1 : 0) == 0, $"追加连续插补输出失败,crd={crd},io={ioIndex}");
            SafeNativeMethod(() => sdk.MoveTable(cardHandle, (uint)crd, (uint)ContiMoveOpTableBase, markIndex) == 0, $"追加输出标志表项失败,crd={crd},mark={markIndex}");
            return true;
        }

        /// <summary>
        /// 回读比较输出触发标志(源端 ReadContiOutFlag,底层 GetTable)。
        /// </summary>
        public bool ReadContiOutFlag(int crd, ref int index)
        {
            CheckInit();
            if (SimulationMode)
            {
                // 虚拟分支确定性桩值:按注入点位递增(ADR v2),让 LatchedOffset 计算链可跑通。
                index = virtualContiOutFlagIndex;
                return true;
            }

            var vs = new float[1];
            SafeNativeMethod(() => sdk.GetTable(cardHandle, ContiMoveOpTableBase, 1, vs) == 0, $"回读输出标志失败,crd={crd}");
            index = (int)vs[0];
            return true;
        }

        /// <summary>
        /// 查询插补器剩余缓冲空间(源端 GetContiRemainSpace,底层 GetRemain_Buffer)。
        /// </summary>
        public bool GetContiRemainSpace(int crd, out int space)
        {
            CheckInit();
            if (SimulationMode)
            {
                // 虚拟分支确定性桩值:返回充足(ADR v2),背压检查链不阻塞。
                space = 4096;
                return true;
            }

            space = 0;
            var remain = 0;
            SafeNativeMethod(() => sdk.GetRemainBuffer(cardHandle, crd, ref remain) == 0, $"查询插补剩余缓冲失败,crd={crd}");
            space = remain;
            return true;
        }

        /// <summary>
        /// 等待连续插补完成(底层 GetIfIdle 轮询)。
        /// </summary>
        public bool WaitCrdDone(int crd, int timeoutMs)
        {
            CheckInit();
            if (SimulationMode) return true;

            var deadline = timeoutMs <= 0 ? long.MaxValue : Environment.TickCount + timeoutMs;
            while (Environment.TickCount < deadline)
            {
                var idle = 0;
                if (sdk.GetIfIdle(cardHandle, crd, ref idle) == 0 && idle != 0)
                {
                    return true;
                }
                System.Threading.Thread.Sleep(5);
            }
            return false;
        }

        /// <summary>
        /// 停止连续插补(源端 StopCrdConti,底层 Single_Cancel)。
        /// ⚠️ 节点级 try/finally 必须调用(M-13 finally 契约)。
        /// </summary>
        public bool CrdContiStop(int crd)
        {
            CheckInit();
            if (SimulationMode) return true;

            SafeNativeMethod(() => sdk.SingleCancel(cardHandle, crd, 2) == 0, $"停止连续插补失败,crd={crd}");
            return true;
        }

        /// <summary>
        /// 关闭连续插补模式(源端 CloseCrdConti,底层 SetMerge=0)。
        /// ⚠️ 节点级 try/finally 必须调用(M-13 finally 契约)。
        /// </summary>
        public bool CrdContiClose(int crd)
        {
            CheckInit();
            if (SimulationMode) return true;

            SafeNativeMethod(() => sdk.SetMerge(cardHandle, crd, 0) == 0, $"关闭连续插补失败,crd={crd}");
            return true;
        }

        /// <summary>
        /// 配置速度前瞻/平滑参数(源端 CrdSetSmoothProfile,底层 SetCornerMode/SetZsmooth/SetDecelAngle/SetStopAngle)。
        /// </summary>
        public bool SetSmoothProfile(int crd, SmoothProfile profile)
        {
            CheckInit();
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            if (SimulationMode) return true;

            SafeNativeMethod(() => sdk.SetCornerMode(cardHandle, crd, profile.CornerMode) == 0, $"设置拐角模式失败,crd={crd}");
            SafeNativeMethod(() => sdk.SetZsmooth(cardHandle, crd, (float)profile.CornerRadius) == 0, $"设置拐角半径失败,crd={crd}");
            SafeNativeMethod(() => sdk.SetDecelAngle(cardHandle, crd, (float)AngleToRad(profile.DecelAngle)) == 0, $"设置减速角度失败,crd={crd}");
            SafeNativeMethod(() => sdk.SetStopAngle(cardHandle, crd, (float)AngleToRad(profile.StopAngle)) == 0, $"设置停止角度失败,crd={crd}");
            return true;
        }

        #endregion

        #region IFiveAxisLatch —— 高速锁存旁路(P5-3,对齐源端 ZMCMotion IoperateHighLatcher 实现)

        /// <summary>
        /// 启动高速锁存(源端 ResetHighLatcher,底层 REGIST 指令配置触发源/边沿/缓存)。
        /// </summary>
        public bool StartLatch(int axis, LatchTrigger trigger)
        {
            CheckInit();
            if (trigger == null)
            {
                throw new ArgumentNullException(nameof(trigger));
            }

            if (SimulationMode)
            {
                // 虚拟分支:清空回放队列,准备接收注入点位。
                var key = trigger.LatchIndex;
                virtualLatchQueues[key] = new Queue<double>();
                return true;
            }

            // 源端 REGIST 指令:mode = ContiMode?100:0 + (FallingEdge?4:3);BASE(axis) REGIST(mode,addr,maxlen,source)
            var mode = (trigger.ContinuousMode ? 100 : 0) + (trigger.TriggerEdge == LatchTriggerEdge.FallingEdge ? 4 : 3);
            var addr = LatchCountTableBase + trigger.LatchIndex * trigger.MaxLength;
            var command = $"BASE({axis}) REGIST({mode},{addr},{trigger.MaxLength},{trigger.SourceIndex})";
            var response = string.Empty;
            SafeNativeMethod(() => sdk.DirectCommand(cardHandle, command, out response, 0) == 0, $"启动高速锁存失败,axis={axis},cmd={command}");
            return true;
        }

        /// <summary>
        /// 批量等待锁存到位(v2 主路径,源端 WaitLatched(axis,count,out value))。
        /// timeoutMs 为整批超时(与源端 RunAction 循环对齐)。
        /// </summary>
        public bool WaitLatched(int axis, int count, int timeoutMs, out double[] latchedPos)
        {
            CheckInit();
            latchedPos = new double[count];
            if (count <= 0) return true;

            if (SimulationMode)
            {
                // 虚拟分支:按注入点位回放(ADR v2)。队列不足时以当前轴位置补齐,保证链路不阻塞。
                var queue = virtualLatchQueues.Values.FirstOrDefault();
                for (var i = 0; i < count; i++)
                {
                    latchedPos[i] = queue != null && queue.Count > 0 ? queue.Dequeue() : GetAxisPosition(axis);
                }
                return true;
            }

            // 真机:轮询已锁存计数,达到 count 后批量读位置(源端 GetHighLatchedCount + GetHighLatchedValue 同构)。
            var deadline = timeoutMs <= 0 ? long.MaxValue : Environment.TickCount + timeoutMs;
            while (Environment.TickCount < deadline)
            {
                var countBuf = new float[1];
                if (sdk.GetTable(cardHandle, LatchCountTableBase, 1, countBuf) != 0) return false;
                if ((int)countBuf[0] >= count)
                {
                    var valueBuf = new float[count];
                    SafeNativeMethod(() => sdk.GetTable(cardHandle, LatchCountTableBase + 1, count, valueBuf) == 0, $"读取锁存值失败,axis={axis}");
                    for (var i = 0; i < count; i++) latchedPos[i] = valueBuf[i];
                    return true;
                }
                System.Threading.Thread.Sleep(5);
            }
            return false;
        }

        /// <summary>
        /// 单值便利重载(转调批量 count=1)。
        /// </summary>
        public bool WaitLatched(int axis, int timeoutMs, out double latchedPos)
        {
            var ok = WaitLatched(axis, 1, timeoutMs, out var arr);
            latchedPos = ok && arr.Length > 0 ? arr[0] : 0;
            return ok;
        }

        /// <summary>
        /// 读取单点锁存位置(源端 GetHighLatchedValue count=1)。
        /// </summary>
        public bool ReadLatch(int axis, out double latchedPos)
        {
            CheckInit();
            if (SimulationMode)
            {
                var queue = virtualLatchQueues.Values.FirstOrDefault();
                latchedPos = queue != null && queue.Count > 0 ? queue.Dequeue() : GetAxisPosition(axis);
                return true;
            }

            var valueBuf = new float[1];
            SafeNativeMethod(() => sdk.GetTable(cardHandle, LatchCountTableBase + 1, 1, valueBuf) == 0, $"读取锁存值失败,axis={axis}");
            latchedPos = valueBuf[0];
            return true;
        }

        /// <summary>
        /// 清除锁存缓存(源端 ResetHighLatcher 重置)。
        /// ⚠️ 节点级 try/finally 必须调用(M-13 finally 契约)。
        /// </summary>
        public bool ClearLatch(int axis)
        {
            CheckInit();
            if (SimulationMode)
            {
                foreach (var queue in virtualLatchQueues.Values) queue.Clear();
                return true;
            }

            // 真机:重置锁存计数表项为 0(与源端 ResetHighLatcher 通过 REGIST 重置同效)。
            SafeNativeMethod(() => sdk.MoveTable(cardHandle, (uint)axis, (uint)LatchCountTableBase, 0) == 0, $"清除锁存缓存失败,axis={axis}");
            return true;
        }

        /// <summary>
        /// 虚拟分支注入锁存点位(供 P5-4 虚拟端到端链回放飞拍触发点)。
        /// 真机模式此方法无效(锁存值由卡端硬件捕获)。
        /// </summary>
        public void InjectVirtualLatchPoints(int latchIndex, IEnumerable<double> points)
        {
            if (!SimulationMode || points == null) return;
            if (!virtualLatchQueues.TryGetValue(latchIndex, out var queue))
            {
                queue = new Queue<double>();
                virtualLatchQueues[latchIndex] = queue;
            }
            foreach (var p in points) queue.Enqueue(p);
        }

        #endregion

        protected override void Dispose(bool isDispose)
        {
            if (cardHandle != IntPtr.Zero)
            {
                sdk.Close(cardHandle);
                cardHandle = IntPtr.Zero;
            }
            base.Dispose(isDispose);
        }

        private void CheckAxisArgs(List<int> axisId, List<double> pos, string method)
        {
            CheckInit();
            if (axisId == null || pos == null || axisId.Count == 0 || axisId.Count != pos.Count)
            {
                throw new ArgumentException($"{method} 的轴号与点位数量不匹配");
            }

            foreach (var axis in axisId)
            {
                CheckAxisNo(axis);
            }
        }

        private void CheckAxisNo(int axisNo, bool throwException = true)
        {
            if (axisNo >= 1 && axisNo <= AxisCount) return;
            if (throwException)
            {
                throw new ArgumentOutOfRangeException(nameof(axisNo), $"轴号 {axisNo} 超出允许范围[1,{AxisCount}]");
            }
        }

        private double GetAxisPosition(int axisNo)
        {
            return currentPositions.TryGetValue(axisNo, out var position) ? position : 0;
        }

        private void SetAxisPosition(int axisNo, double position)
        {
            currentPositions[axisNo] = position;
        }

        private static double ToPulse(double position, double perPulse)
        {
            return position * (perPulse == 0 ? 1 : perPulse);
        }

        // 度→弧度(对齐源端 MathNetExtend AngleHelper.AngleToRad,平滑参数角度阈值用)。
        private static double AngleToRad(double angle)
        {
            return angle * Math.PI / 180.0;
        }

        private static string BuildKey(short axis, short index, short subindex, short dataSize)
        {
            return $"{axis}:{index}:{subindex}:{dataSize}";
        }

        private static void ValidateRtcpConfig(FiveAxisRtcpConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (config.VirtualAxisIds == null || config.RealAxisIds == null || config.VirtualAxisIds.Count != 5 || config.RealAxisIds.Count != 5)
            {
                throw new ArgumentException("RTCP 需配置 5 个虚轴和 5 个实轴");
            }
        }
    }
}
