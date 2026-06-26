using Luster.Motion.DataStruct.Enums;

namespace Luster.Module.Motion.Safety.Models
{
    /// <summary>
    /// 统一报警数据契约（TES-38 产出，对齐架构师 ADR-A 绑定契约的 <c>AlarmRecord</c>）。
    /// 字段：severity / category / source / code / message / recoveryPolicy / latchPolicy /
    /// suppressPolicy / traceId + 平台既有 <see cref="AlarmType"/> / <see cref="AlarmProc"/> 映射。
    /// </summary>
    /// <remarks>
    /// 架构师 ADR-A 决策：扩展现有 <see cref="AlarmType"/>/<see cref="AlarmProc"/>/<c>TbAlarm</c>，
    /// 不新建并列报警体系。本类是 TES-38 填充报警内容、绑 <see cref="RecoveryPolicy"/> 的中间形态，
    /// 最终由 TES-28 异常恢复写入 <c>TbAlarm</c>/<c>TbAlarminfos</c>。
    /// 共享枚举（<see cref="RecoveryPolicy"/> 等）定义在框架层 <c>Luster.Motion.DataStruct.Enums</c>。
    /// </remarks>
    public class AlarmSchema
    {
        /// <summary>报警严重级别</summary>
        public AlarmSeverity Severity { get; set; } = AlarmSeverity.Warning;

        /// <summary>报警类别（安全/设备/运动/通信/视觉/互锁/人工介入）</summary>
        public AlarmCategory Category { get; set; } = AlarmCategory.Safety;

        /// <summary>报警来源（设备名/站点名/模块名）</summary>
        public string Source { get; set; } = string.Empty;

        /// <summary>报警码（与源端 ErrCode.csv 的 Code 列对齐）</summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>报警中文名（ErrCode.csv 的 Name 列）</summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>英文报警名（ErrCode.csv 的 EName 列）</summary>
        public string EnglishName { get; set; } = string.Empty;

        /// <summary>报警描述（ErrCode.csv 的 Description 列）</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>恢复建议（ErrCode.csv 的 Suggestion 列）</summary>
        public string Suggestion { get; set; } = string.Empty;

        /// <summary>恢复策略：决定 TES-28 异常恢复三策略（清机/续跑/报废）如何路由（ADR-A↔ADR-C 耦合点）</summary>
        public RecoveryPolicy RecoveryPolicy { get; set; } = RecoveryPolicy.Manual;

        /// <summary>锁存策略：是否需要人工复位才能清除（急停/安全门/伺服报警必锁存）</summary>
        public LatchPolicy LatchPolicy { get; set; } = LatchPolicy.Latch;

        /// <summary>抑制策略：关键异常不可静默失败（默认 None = 不允许抑制）</summary>
        public SuppressPolicy SuppressPolicy { get; set; } = SuppressPolicy.None;

        /// <summary>追溯 ID，与单产品 SN / 工站 traceId 绑定（喂 P0-B 追溯写入状态）</summary>
        public string TraceId { get; set; } = string.Empty;

        /// <summary>映射到平台既有 AlarmType（喂给 AlarmInfo 走既有 UI 链路）</summary>
        public AlarmType PlatformAlarmType { get; set; } = AlarmType.WarningTip;

        /// <summary>映射到平台既有 AlarmProc（处置矩阵）</summary>
        public AlarmProc PlatformAlarmProc { get; set; } = AlarmProc.Stop;

        /// <summary>报警持续时间阈值（秒），超过则升级或触发 OEE 停机统计</summary>
        public int AlarmLongTimeSec { get; set; } = 0;

        /// <summary>
        /// 生成平台既有 <see cref="AlarmInfo"/> 的报警码（Code@Source 形式，便于 ErrorManager 解析）。
        /// </summary>
        public string ToAlarmCode() => string.IsNullOrEmpty(Source) ? Code : $"{Code}@{Source}";
    }
}
