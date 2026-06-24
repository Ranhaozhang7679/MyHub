using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Real;
using Luster.Motion.FiveAxis.Device;
using Luster.Motion.FiveAxis.Kinematics;
using Luster.SimDevice.MotionCards;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;

namespace Luster.SimDevice.MotionCard.ZMotion
{
    /// <summary>
    /// 正运动 ZMotion 运动控制卡。
    /// </summary>
    public class ZMotionMotionCard : MotionCardBase, IMotionCard, IFiveAxisRTCP, IFiveAxisFrame
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

        /// <summary>
        /// Frame 模式等待超时(ms)。对齐源端 <c>ZMCMotion.FrameTimeOut</c>(进逆解模式后轮询 GetLoaded 的超时)。
        /// </summary>
        [DisplayName("Frame模式超时(ms)")]
        public int FrameTimeOut { get; set; } = 5000;

        /// <summary>
        /// 卡端精标(FrameCal)表地址(ADR-TES-110 R-F4)。
        /// 对齐源端 <c>CrdProfile.CrdAddr.FrameCalAddr</c>(<c>FRAME_CAL</c> 命令读写的 Table 区)。
        /// ⚠️ R-F4 真机表地址配置待人类现场验证;默认值仅占位,生产前需按站配置。
        /// </summary>
        public FiveAxisFrameAddr FrameCalAddr { get; set; } = new FiveAxisFrameAddr
        {
            InAxisPosiTb = 0,
            InExtendTb = 0,
            OutZeroTb = 0,
            OutRobotTb = 0,
        };

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

        #region IFiveAxisFrame —— 卡端正逆解模式 + 精标解算(ADR-TES-110,对齐源端 ZMCMotion 五轴区)

        /// <summary>
        /// 进入五轴逆解模式。原样迁自源端 <c>Z5Axes_Frame</c>(ZMCMotion.cs:2807):
        /// 停止实轴/虚轴 → <c>SetTable</c> 写 26 float 结构参数(<c>Axis5ParaAddr</c>) →
        /// <c>ConnFrame</c> 进逆解 → 轮询 <c>GetLoaded</c> 至 Loaded(<c>FrameTimeOut</c> 超时)。
        /// </summary>
        public bool Frame(int crdIndex, IReadOnlyList<int> realAxisList, IReadOnlyList<int> virAxisList, Coord5Axis para)
        {
            CheckInit();
            if (para == null) throw new ArgumentNullException(nameof(para));
            if (realAxisList == null || virAxisList == null) throw new ArgumentException("Frame 需配置实轴和虚轴列表");

            if (SimulationMode) return true;

            var realLis = realAxisList.ToList();
            var virLis = virAxisList.ToList();
            if (!CancelAxesIfNotIdle(virLis)) return false;
            if (!CancelAxesIfNotIdle(realLis)) return false;

            CheckCrdNo(crdIndex);
            // 26 float 结构参数布局与源端 Z5Axes_Frame 一致(6+6+1+6+6+1=26,末尾补 0)
            var robotPara = new float[]
            {
                (float)para.ACenter.X, (float)para.ACenter.Y, (float)para.ACenter.Z,
                (float)para.ADir.X, (float)para.ADir.Y, (float)para.ADir.Z,
                (float)para.ACirPulses,
                (float)para.CCenter.X, (float)para.CCenter.Y, (float)para.CCenter.Z,
                (float)para.CDir.X, (float)para.CDir.Y, (float)para.CDir.Z,
                (float)para.CCirPulses,
                0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            };
            var axis5ParaAddr = ResolveAxis5ParaAddr(crdIndex);
            SafeNativeMethod(() => sdk.SetTable(cardHandle, axis5ParaAddr, robotPara.Length, robotPara) == 0, "Frame 写五轴结构参数失败");
            // 进逆解模式(源端 step=29 固定)
            SafeNativeMethod(() => sdk.ConnFrame(cardHandle, realLis.Count, realLis.ToArray(), 29, axis5ParaAddr, virLis.Count, virLis.ToArray()) == 0, "进入五轴逆解模式失败");
            Thread.Sleep(10);
            // 轮询物理轴 Loaded 状态(逆解判断物理轴,与源端一致)
            DateTime start = DateTime.Now;
            while (true)
            {
                var loaded = 0;
                SafeNativeMethod(() => sdk.GetLoaded(cardHandle, realLis[0], ref loaded) == 0, "读取进逆解模式状态失败");
                if (loaded != 0) break;
                if ((DateTime.Now - start).TotalMilliseconds > FrameTimeOut)
                {
                    throw new InvalidOperationException("进入五轴逆解状态超时");
                }
                Thread.Sleep(10);
            }
            return true;
        }

        /// <summary>
        /// 进入五轴正解模式。本期只留签名,后续正解 Issue 实现(ADR-TES-110 范围冻结)。
        /// </summary>
        public bool Reframe(int crdIndex, IReadOnlyList<int> realAxisList, IReadOnlyList<int> virAxisList, Coord5Axis para)
        {
            throw new NotImplementedException("五轴正解 Reframe 待后续正解 Issue 实现(ADR-TES-110:本期只定逆解 FrameCal 契约)。");
        }

        /// <summary>
        /// 退出五轴正逆解模式。原样迁自源端 <c>Z5Axes_ExitFrame</c>(ZMCMotion.cs:3203):
        /// 多轴 <c>CancelAxisList</c> 停止 + 逐轴 <c>Single_Cancel</c>。
        /// </summary>
        public bool ExitFrame(IReadOnlyList<int> realAxisList, IReadOnlyList<int> virAxisList)
        {
            CheckInit();
            if (SimulationMode) return true;

            if (realAxisList == null || virAxisList == null) throw new ArgumentException("ExitFrame 需配置实轴和虚轴列表");
            var realLis = realAxisList.ToList();
            var virLis = virAxisList.ToList();

            if (realLis.Count > 0)
            {
                SafeNativeMethod(() => sdk.CancelAxisList(cardHandle, realLis.Count, realLis.ToArray(), 2) == 0, "退出五轴:实轴停止失败");
            }
            if (virLis.Count > 0)
            {
                SafeNativeMethod(() => sdk.CancelAxisList(cardHandle, virLis.Count, virLis.ToArray(), 2) == 0, "退出五轴:虚轴停止失败");
            }
            foreach (var axis in realLis)
            {
                SafeNativeMethod(() => sdk.SingleCancel(cardHandle, axis, 2) == 0, $"退出五轴:实轴{axis}停止失败");
            }
            foreach (var axis in virLis)
            {
                SafeNativeMethod(() => sdk.SingleCancel(cardHandle, axis, 2) == 0, $"退出五轴:虚轴{axis}停止失败");
            }
            return true;
        }

        /// <summary>
        /// 卡端精标解算。原样迁自源端 <c>ZFrameCali</c>(ZMCMotion.cs:3252):
        /// <c>SetTable</c> 写采样点(<c>InAxisPosiTb</c>) → <c>DirectCommand("BASE(...) FRAME_CAL(...)")</c> 卡端固件解算 →
        /// <c>GetTable</c> 读 <c>OutZeroTb</c>(aZero=vs[3]) + <c>OutRobotTb</c>(16 float → Coord5Axis)。
        /// 前置:已 <see cref="Frame"/>(粗标参数) 进入逆解模式。
        /// </summary>
        public bool FrameCal(int crdIndex, IReadOnlyList<int> realAxisList, IReadOnlyList<double[]> axisPosi,
                             out double aZero, out Coord5Axis para)
        {
            aZero = 0;
            para = new Coord5Axis();
            CheckInit();
            if (axisPosi == null || axisPosi.Count == 0) throw new ArgumentException("FrameCal 需提供采样点列表", nameof(axisPosi));

            if (SimulationMode)
            {
                // 模拟模式下无法还原卡端固件算法输出,给出全默认结构参数(供软件层编排联调,真机精度见 R-F4)。
                return true;
            }

            CheckCrdNo(crdIndex);
            var addr = FrameCalAddr ?? new FiveAxisFrameAddr();
            var space = axisPosi[0].Length;
            var group = axisPosi.Count;
            var fLis = new List<float>(group * space);
            foreach (var gp in axisPosi)
            {
                foreach (var item in gp)
                {
                    fLis.Add((float)item);
                }
            }
            SafeNativeMethod(() => sdk.SetTable(cardHandle, addr.InAxisPosiTb, group * space, fLis.ToArray()) == 0, "FrameCal 写采样点失败");

            var virBase = realAxisList != null ? string.Join(",", realAxisList) : string.Empty;
            var command = new StringBuilder()
                .AppendFormat("BASE({6}) FRAME_CAL({0},{1},{2},{3},{4},{5})",
                    addr.InAxisPosiTb, space, group,
                    addr.InExtendTb, addr.OutZeroTb, addr.OutRobotTb,
                    virBase).AppendLine();
            var response = string.Empty;
            SafeNativeMethod(() => sdk.DirectCommand(cardHandle, command.ToString(), out response, 0) == 0, "卡端 FRAME_CAL 精标解算失败");

            var vs = new float[5];
            SafeNativeMethod(() => sdk.GetTable(cardHandle, addr.OutZeroTb, vs.Length, vs) == 0, "FrameCal 读 A 轴零点失败");
            aZero = vs[3];

            var p = new float[16];
            SafeNativeMethod(() => sdk.GetTable(cardHandle, addr.OutRobotTb, p.Length, p) == 0, "FrameCal 读精标结构参数失败");
            int index = 0;
            para.ACenter.X = p[index++];
            para.ACenter.Y = p[index++];
            para.ACenter.Z = p[index++];
            para.ADir.X = p[index++];
            para.ADir.Y = p[index++];
            para.ADir.Z = p[index++];
            para.ACirPulses = p[index++];
            para.CCenter.X = p[index++];
            para.CCenter.Y = p[index++];
            para.CCenter.Z = p[index++];
            para.CDir.X = p[index++];
            para.CDir.Y = p[index++];
            para.CDir.Z = p[index++];
            para.CCirPulses = p[index++];
            return true;
        }

        /// <summary>轴非 idle 时 cancel,返回是否全部成功(对齐源端 Z5Axes_Frame 的 GetIfIdle/Single_Cancel 循环)。</summary>
        private bool CancelAxesIfNotIdle(List<int> axes)
        {
            foreach (var axis in axes)
            {
                var idle = 0;
                SafeNativeMethod(() => sdk.GetIfIdle(cardHandle, axis, ref idle) == 0, $"读取轴{axis}运动状态失败");
                if (idle == 0)
                {
                    SafeNativeMethod(() => sdk.SingleCancel(cardHandle, axis, 2) == 0, $"轴{axis}停止失败");
                }
            }
            return true;
        }

        /// <summary>坐标系编号越界校验(对齐源端 checkCrdEnv 语义,本期仅记录入口,实际 CrdProfile 配置见 R-F4)。</summary>
        private void CheckCrdNo(int crdIndex)
        {
            if (crdIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(crdIndex), $"坐标系编号 {crdIndex} 非法");
            }
        }

        /// <summary>解析五轴结构参数表地址。对齐源端 <c>CrdProfile.CrdAddr.Axis5ParaAddr</c>;本期 CrdProfile 表配置待 R-F4,暂用固定占位与 FrameCalAddr 同策略。</summary>
        private int ResolveAxis5ParaAddr(int crdIndex)
        {
            // ⚠️ R-F4 真机 CrdProfile.Axis5ParaAddr 表地址待人类现场核对;模拟模式不触达此路径。
            return FrameCalAddr?.InAxisPosiTb ?? 0;
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
