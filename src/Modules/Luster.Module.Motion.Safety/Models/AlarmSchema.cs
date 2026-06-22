using Luster.Motion.DataStruct.Enums;
using System;

namespace Luster.Module.Motion.Safety.Models
{
    /// <summary>
    /// 统一报警数据契约（TES-28 ADR-C 输入 / TES-38 报警矩阵绑定）。
    /// 对齐 TES-28 验收字段：severity / category / source / code / message /
    /// recoveryPolicy / latchPolicy / suppressPolicy / traceId。
    /// 同时携带与平台既有 <see cref="AlarmType"/> / <see cref="AlarmProc"/> 的映射，
    /// 便于复用既有 AlarmEvent → AlarmContentVM → TbAlarminfos 链路。
    /// </summary>
    public class AlarmSchema
    {
        /// <summary>报警严重级别</summary>
        public AlarmSeverity Severity { get; set; } = AlarmSeverity.Warning;

        /// <summary>报警类别（安全/设备/运动/通信/互锁/人工介入）</summary>
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

        /// <summary>恢复策略：决定 TES-28 异常恢复三策略（清机/续跑/报废）如何路由</summary>
        public RecoveryPolicy RecoveryPolicy { get; set; } = RecoveryPolicy.Manual;

        /// <summary>锁存策略：是否需要人工复位才能清除</summary>
        public LatchPolicy LatchPolicy { get; set; } = LatchPolicy.Latch;

        /// <summary>抑制策略：关键异常不可静默失败（默认 None = 不允许抑制）</summary>
        public SuppressPolicy SuppressPolicy { get; set; } = SuppressPolicy.None;

        /// <summary>追溯 ID，与单产品 SN / 工站 traceId 绑定</summary>
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

    /// <summary>报警严重级别</summary>
    public enum AlarmSeverity
    {
        /// <summary>信息提示</summary>
        Info = 0,
        /// <summary>警告（可继续，需关注）</summary>
        Warning = 1,
        /// <summary>错误（必须停机/暂停）</summary>
        Error = 2,
        /// <summary>致命（急停级，安全回路动作）</summary>
        Fatal = 3
    }

    /// <summary>报警类别</summary>
    public enum AlarmCategory
    {
        /// <summary>安全类（急停/安全门/门锁/光幕）</summary>
        Safety = 0,
        /// <summary>设备类（轴限位/伺服报警/刹车/IO 异常）</summary>
        Device = 1,
        /// <summary>运动类（到位/回零/超时）</summary>
        Motion = 2,
        /// <summary>通信类（Modbus/PLC/ICW 断链/超时）</summary>
        Communication = 3,
        /// <summary>互锁类（上下游交接互锁）</summary>
        Interlock = 4,
        /// <summary>人工介入类</summary>
        Manual = 5
    }

    /// <summary>
    /// 恢复策略，对齐 TES-28 启动恢复三策略。
    /// </summary>
    public enum RecoveryPolicy
    {
        /// <summary>无恢复动作</summary>
        None = 0,
        /// <summary>重试当前动作</summary>
        Retry = 1,
        /// <summary>跳过当前产品继续</summary>
        Skip = 2,
        /// <summary>清机：清除在籍产品，回到安全位</summary>
        Clean = 3,
        /// <summary>续跑：保留在籍产品，从断点继续</summary>
        Resume = 4,
        /// <summary>报废当前在籍产品</summary>
        Scrap = 5,
        /// <summary>回零后恢复</summary>
        Home = 6,
        /// <summary>人工介入（不自动恢复）</summary>
        Manual = 7,
        /// <summary>急停停机，需人工复位</summary>
        Abort = 8
    }

    /// <summary>锁存策略</summary>
    public enum LatchPolicy
    {
        /// <summary>不锁存，条件消失即清除</summary>
        None = 0,
        /// <summary>锁存，需人工复位（急停/安全门/伺服报警必锁存）</summary>
        Latch = 1,
        /// <summary>条件消失后延时自动清除</summary>
        AutoClear = 2
    }

    /// <summary>抑制策略（关键异常不可静默失败）</summary>
    public enum SuppressPolicy
    {
        /// <summary>不允许抑制（默认，关键异常必须上报）</summary>
        None = 0,
        /// <summary>允许抑制一次（调试用）</summary>
        SuppressOnce = 1,
        /// <summary>允许在指定时长内抑制</summary>
        SuppressDuration = 2
    }
}
