using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Module.Motion.Safety.Models;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Motion;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Luster.Module.Motion.Safety.Functions
{
    /// <summary>
    /// 安全联锁检查节点（TES-38）。
    /// 复用平台 <see cref="VIO.GetDigitalIn()"/> 读急停/安全门/门锁，
    /// 复用 <see cref="VAxis.GetAxisStatus(bool)"/> 读轴限位/伺服报警/急停/使能位，
    /// 对齐源端 <c>MachineControlBase.onTaskDoing</c> + <c>ZMCMotion.CheckXxxStatus</c> 位语义。
    /// 任一启用项触发即 <see cref="MotionFunction.OnAlarm"/> 并返回 false，阻断流程。
    /// </summary>
    /// <remarks>
    /// 急停/安全门在源端为常闭（NC）：输入 false = 触发。本节点用 <see cref="EmergencyNc"/>
    /// / <see cref="DoorNc"/> 极性参数适配现场接线差异，默认常闭。
    /// 硬件安全动作（刹车/伺服断电）标 ⚠️ 待人类现场验证，本节点只做状态读取与报警上报。
    /// </remarks>
    public class CheckSafety : MotionFunction
    {
        /// <summary>急停按钮 IO（常闭，false=按下）</summary>
        [Parameter("急停按钮输入 IO，常闭触点（输入为 false 视为急停按下）", 0, CN = "急停IO", EditorType = typeof(VIO))]
        public VDevice EmergencyStop { get; set; }

        /// <summary>安全门 IO</summary>
        [Parameter("安全门输入 IO", 1, CN = "安全门IO", EditorType = typeof(VIO))]
        public VDevice SafetyDoor { get; set; }

        /// <summary>门锁反馈 IO</summary>
        [Parameter("门锁到位反馈 IO", 2, CN = "门锁IO", EditorType = typeof(VIO))]
        public VDevice DoorLock { get; set; }

        /// <summary>待检轴（限位/伺服报警/急停位检查）</summary>
        [Parameter("待检查的轴（读 AxisStatus 位）", 3, CN = "检查轴", EditorType = typeof(VAxis))]
        public VDevice Axis { get; set; }

        [Parameter("是否检查急停", 4, CN = "检查急停", DefaultV = true)]
        public bool CheckEmergency { get; set; } = true;

        [Parameter("是否检查安全门", 5, CN = "检查安全门", DefaultV = true)]
        public bool CheckDoor { get; set; } = true;

        [Parameter("是否检查门锁", 6, CN = "检查门锁", DefaultV = true)]
        public bool CheckLock { get; set; } = true;

        [Parameter("是否检查轴限位/伺服报警", 7, CN = "检查轴状态", DefaultV = true)]
        public bool CheckAxisStatus { get; set; } = true;

        [Parameter("急停极性：true=常闭（false 触发），false=常开（true 触发）", 8, CN = "急停常闭", DefaultV = true)]
        public bool EmergencyNc { get; set; } = true;

        [Parameter("安全门极性：true=常闭（false 触发，门打开），false=常开", 9, CN = "门常闭", DefaultV = true)]
        public bool DoorNc { get; set; } = true;

        public CheckSafety()
        {
            this.Tips = "安全联锁检查";
            this.Icon = "\xe728";
        }

        public override string[] NoteParams { get; set; } = new[] { nameof(CheckEmergency), nameof(CheckDoor), nameof(CheckLock), nameof(CheckAxisStatus) };

        public override bool DoExcute(out string errMsg)
        {
            errMsg = string.Empty;

            // 1. 急停
            if (CheckEmergency && EmergencyStop != null)
            {
                GetVDevice<VIO>(EmergencyStop, out var emg);
                if (emg != null)
                {
                    bool pressed = EmergencyNc ? !emg.GetDigitalIn() : emg.GetDigitalIn();
                    if (pressed)
                    {
                        RaiseSafetyAlarm(AlarmSeverity.Fatal, AlarmCategory.Safety, AlarmType.DeviceError,
                            "急停已按下", "EMG", RecoveryPolicy.Abort);
                        errMsg = "急停触发";
                        return false;
                    }
                }
            }

            // 2. 安全门
            if (CheckDoor && SafetyDoor != null)
            {
                GetVDevice<VIO>(SafetyDoor, out var door);
                if (door != null)
                {
                    bool opened = DoorNc ? !door.GetDigitalIn() : door.GetDigitalIn();
                    if (opened)
                    {
                        RaiseSafetyAlarm(AlarmSeverity.Fatal, AlarmCategory.Safety, AlarmType.WarningTip,
                            "安全门未关闭", "SAFETY_DOOR", RecoveryPolicy.Abort);
                        errMsg = "安全门打开";
                        return false;
                    }
                }
            }

            // 3. 门锁
            if (CheckLock && DoorLock != null)
            {
                GetVDevice<VIO>(DoorLock, out var lockIo);
                if (lockIo != null && !lockIo.GetDigitalIn())
                {
                    RaiseSafetyAlarm(AlarmSeverity.Warning, AlarmCategory.Safety, AlarmType.WarningTip,
                        "门锁未到位", "DOOR_LOCK", RecoveryPolicy.Manual);
                    errMsg = "门锁未到位";
                    return false;
                }
            }

            // 4. 轴状态：限位 / 伺服报警 / 急停位
            if (CheckAxisStatus && Axis != null)
            {
                GetVDevice<VAxis>(Axis, out var axis);
                if (axis != null)
                {
                    var status = axis.GetAxisStatus(false) ?? new Dictionary<AxisStatus, bool>();
                    if (status.TryGetValue(AxisStatus.Emg, out var emgAxis) && emgAxis)
                    {
                        RaiseSafetyAlarm(AlarmSeverity.Fatal, AlarmCategory.Device, AlarmType.DeviceError,
                            $"轴 {axis.Name} 急停", "AXIS_EMG", RecoveryPolicy.Abort);
                        errMsg = "轴急停";
                        return false;
                    }
                    if (status.TryGetValue(AxisStatus.Pel, out var pel) && pel)
                    {
                        RaiseSafetyAlarm(AlarmSeverity.Fatal, AlarmCategory.Device, AlarmType.DeviceError,
                            $"轴 {axis.Name} 正限位触发", "AXIS_PEL", RecoveryPolicy.Abort);
                        errMsg = "轴正限位";
                        return false;
                    }
                    if (status.TryGetValue(AxisStatus.Mel, out var mel) && mel)
                    {
                        RaiseSafetyAlarm(AlarmSeverity.Fatal, AlarmCategory.Device, AlarmType.DeviceError,
                            $"轴 {axis.Name} 负限位触发", "AXIS_MEL", RecoveryPolicy.Abort);
                        errMsg = "轴负限位";
                        return false;
                    }
                    // 伺服报警：Alarm + Error1..4 任一置位
                    if (status.TryGetValue(AxisStatus.Alarm, out var alarm) && alarm)
                    {
                        RaiseSafetyAlarm(AlarmSeverity.Fatal, AlarmCategory.Device, AlarmType.DeviceError,
                            $"轴 {axis.Name} 伺服报警", "AXIS_ALARM", RecoveryPolicy.Abort);
                        errMsg = "轴伺服报警";
                        return false;
                    }
                }
            }

            return base.DoExcute(out errMsg);
        }

        /// <summary>
        /// 构造 <see cref="AlarmSchema"/> 并走平台既有 <see cref="MotionFunction.OnAlarm"/> 上报。
        /// 完整 AlarmSchema 持久化（写入 TbAlarminfos + traceId 绑定）由 TES-28 异常恢复接入。
        /// </summary>
        private void RaiseSafetyAlarm(AlarmSeverity severity, AlarmCategory category,
            AlarmType platformType, string message, string code, RecoveryPolicy recovery)
        {
            var schema = new AlarmSchema
            {
                Severity = severity,
                Category = category,
                Source = MyOwner?.Alias ?? "Safety",
                Code = code,
                Message = message,
                RecoveryPolicy = recovery,
                LatchPolicy = LatchPolicy.Latch,
                SuppressPolicy = SuppressPolicy.None,
                PlatformAlarmType = platformType,
                PlatformAlarmProc = severity >= AlarmSeverity.Fatal ? AlarmProc.Stop : AlarmProc.Check
            };
            // 关键异常不可静默失败：severity>=Error 强制上报
            OnAlarm(schema.PlatformAlarmType, $"[{schema.Severity}]{schema.Message}({schema.Code})", schema.ToAlarmCode());
        }
    }
}
