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
    public class ZMotionMotionCard : MotionCardBase, IMotionCard, IFiveAxisRTCP
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
