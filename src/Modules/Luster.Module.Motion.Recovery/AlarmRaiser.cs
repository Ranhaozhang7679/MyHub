using Luster.Module.Motion.Safety.Models;
using Luster.Motion.DataStruct.Enums;
using System;
using System.Collections.Generic;

namespace Luster.Module.Motion.Recovery
{
    /// <summary>
    /// 关键异常不可静默失败 helper（ADR-A P0-H batch 1，TES-28）。
    /// 把源端散点的空 catch / 吞异常改造为结构化 <see cref="AlarmSchema"/> 发布。
    /// </summary>
    /// <remarks>
    /// P0-H 本期必治理范围（架构师决策）：
    /// 相机丢帧、通信断链(Handover/Modbus/McNet)、急停/安全门、IO 读写异常、
    /// 轨迹越界、伺服/限位报警。关键异常 <see cref="SuppressPolicy.None"/>（不可静默），
    /// 急停/限位/伺服报警 <see cref="LatchPolicy.Latch"/>。
    /// 纯 UI/日志/非关键外观类 catch 不在本期。
    /// </remarks>
    public static class AlarmRaiser
    {
        /// <summary>关键异常码 → 默认 AlarmSchema（覆盖 P0-H 必治理项）</summary>
        private static readonly Dictionary<string, AlarmSchema> _criticalAlarms = new Dictionary<string, AlarmSchema>
        {
            ["CAM_FRAME_LOST"] = Make(AlarmSeverity.Error, AlarmCategory.Camera, AlarmType.DeviceError,
                "CAM_FRAME_LOST", "相机丢帧", RecoveryPolicy.Retry, LatchPolicy.AutoClear),
            ["COMM_HANDOVER_BROKEN"] = Make(AlarmSeverity.Error, AlarmCategory.Communication, AlarmType.DeviceError,
                "COMM_HANDOVER_BROKEN", "Handover 通信断链", RecoveryPolicy.Manual, LatchPolicy.Latch),
            ["COMM_MODBUS_TIMEOUT"] = Make(AlarmSeverity.Error, AlarmCategory.Communication, AlarmType.Timeout,
                "COMM_MODBUS_TIMEOUT", "Modbus 通信超时", RecoveryPolicy.Retry, LatchPolicy.AutoClear),
            ["COMM_MCNET_BROKEN"] = Make(AlarmSeverity.Error, AlarmCategory.Communication, AlarmType.DeviceError,
                "COMM_MCNET_BROKEN", "McNet 通信断链", RecoveryPolicy.Manual, LatchPolicy.Latch),
            ["EMG_PRESSED"] = Make(AlarmSeverity.Fatal, AlarmCategory.Safety, AlarmType.DeviceError,
                "EMG_PRESSED", "急停触发", RecoveryPolicy.Abort, LatchPolicy.Latch),
            ["SAFETY_DOOR_OPEN"] = Make(AlarmSeverity.Fatal, AlarmCategory.Safety, AlarmType.WarningTip,
                "SAFETY_DOOR_OPEN", "安全门打开", RecoveryPolicy.Abort, LatchPolicy.Latch),
            ["IO_READ_WRITE_FAIL"] = Make(AlarmSeverity.Error, AlarmCategory.Device, AlarmType.DeviceError,
                "IO_READ_WRITE_FAIL", "IO 读写异常", RecoveryPolicy.Retry, LatchPolicy.AutoClear),
            ["TRAJ_OUT_OF_BOUNDS"] = Make(AlarmSeverity.Error, AlarmCategory.Motion, AlarmType.FailError,
                "TRAJ_OUT_OF_BOUNDS", "轨迹越界", RecoveryPolicy.Abort, LatchPolicy.Latch),
            ["SERVO_ALARM"] = Make(AlarmSeverity.Fatal, AlarmCategory.Device, AlarmType.DeviceError,
                "SERVO_ALARM", "伺服报警", RecoveryPolicy.Abort, LatchPolicy.Latch),
            ["AXIS_LIMIT"] = Make(AlarmSeverity.Fatal, AlarmCategory.Device, AlarmType.DeviceError,
                "AXIS_LIMIT", "轴限位触发", RecoveryPolicy.Abort, LatchPolicy.Latch),
        };

        /// <summary>
        /// 关键异常码是否注册（不可静默项）。未注册码视为非关键，可按普通异常处理。
        /// </summary>
        public static bool IsCritical(string code) => _criticalAlarms.ContainsKey(code);

        /// <summary>取关键异常的 AlarmSchema（不存在返回 null）</summary>
        public static AlarmSchema Get(string code)
            => code != null && _criticalAlarms.TryGetValue(code, out var s) ? s : null;

        /// <summary>
        /// 把捕获的异常包装为 <see cref="AlarmSchema"/>。
        /// 关键异常（IsCritical）强制 <see cref="SuppressPolicy.None"/> 不可静默；
        /// 调用方必须据此走 OnAlarm 上报，不允许吞掉。
        /// </summary>
        public static AlarmSchema Wrap(Exception ex, string code, string source, string traceId = "")
        {
            var schema = Get(code) ?? Make(AlarmSeverity.Warning, AlarmCategory.Manual, AlarmType.WarningTip,
                code, ex?.Message ?? "未知异常", RecoveryPolicy.Manual, LatchPolicy.AutoClear);
            // 关键异常不可静默：覆盖 Source/TraceId，保留策略
            return new AlarmSchema
            {
                Severity = schema.Severity,
                Category = schema.Category,
                Source = source ?? schema.Source,
                Code = schema.Code,
                Message = schema.Message,
                RecoveryPolicy = schema.RecoveryPolicy,
                LatchPolicy = schema.LatchPolicy,
                // 关键异常强制不可静默；非关键也默认 None（P0-H 默认不抑制）
                SuppressPolicy = SuppressPolicy.None,
                TraceId = traceId ?? "",
                PlatformAlarmType = schema.PlatformAlarmType,
                PlatformAlarmProc = schema.PlatformAlarmProc,
                Description = ex?.ToString() ?? ""
            };
        }

        private static AlarmSchema Make(AlarmSeverity sev, AlarmCategory cat, AlarmType platType,
            string code, string message, RecoveryPolicy recovery, LatchPolicy latch)
        {
            return new AlarmSchema
            {
                Severity = sev,
                Category = cat,
                Code = code,
                Message = message,
                RecoveryPolicy = recovery,
                LatchPolicy = latch,
                SuppressPolicy = SuppressPolicy.None, // 关键异常默认不可静默
                PlatformAlarmType = platType,
                PlatformAlarmProc = sev >= AlarmSeverity.Fatal ? AlarmProc.Stop : AlarmProc.Check
            };
        }
    }
}
