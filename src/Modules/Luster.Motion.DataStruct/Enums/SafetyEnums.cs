using System.ComponentModel;

namespace Luster.Motion.DataStruct.Enums
{
    /// <summary>
    /// 报警严重级别（ADR-A 共享枚举，TES-37/38/28 三方共用）。
    /// 定义在框架层 <c>Luster.Motion.DataStruct</c>，业务模块不得各自定义私有副本。
    /// </summary>
    public enum AlarmSeverity
    {
        [Description("信息")]
        Info = 0,
        [Description("警告")]
        Warning = 1,
        [Description("错误")]
        Error = 2,
        [Description("致命")]
        Fatal = 3
    }

    /// <summary>
    /// 报警类别（ADR-A 共享枚举）。
    /// </summary>
    public enum AlarmCategory
    {
        [Description("安全")]
        Safety = 0,
        [Description("设备")]
        Device = 1,
        [Description("运动")]
        Motion = 2,
        [Description("通信")]
        Communication = 3,
        [Description("视觉")]
        Camera = 4,
        [Description("互锁")]
        Interlock = 5,
        [Description("人工介入")]
        Manual = 6
    }

    /// <summary>
    /// 恢复策略（ADR-A ↔ ADR-C 耦合点，TES-38 填具体内容喂 TES-28 P0-G 恢复状态机）。
    /// 对齐 TES-28 启动恢复三策略：清机(Clean)/续跑(Resume)/报废(Scrap)。
    /// </summary>
    public enum RecoveryPolicy
    {
        [Description("无恢复动作")]
        None = 0,
        [Description("重试")]
        Retry = 1,
        [Description("跳过")]
        Skip = 2,
        [Description("清机")]
        Clean = 3,
        [Description("续跑")]
        Resume = 4,
        [Description("报废")]
        Scrap = 5,
        [Description("回零恢复")]
        Home = 6,
        [Description("人工确认")]
        Manual = 7,
        [Description("急停停机")]
        Abort = 8
    }

    /// <summary>
    /// 锁存策略（急停/安全门/伺服报警必 Latch，人工确认前不可自动清除）。
    /// </summary>
    public enum LatchPolicy
    {
        [Description("不锁存")]
        None = 0,
        [Description("锁存待复位")]
        Latch = 1,
        [Description("延时自动清除")]
        AutoClear = 2
    }

    /// <summary>
    /// 抑制策略（关键异常不可静默失败，默认 None = 不允许抑制）。
    /// </summary>
    public enum SuppressPolicy
    {
        [Description("不允许抑制")]
        None = 0,
        [Description("抑制一次")]
        SuppressOnce = 1,
        [Description("时长抑制")]
        SuppressDuration = 2
    }

    /// <summary>
    /// 安全输入维度（ADR-C 共享枚举，对齐源端 + P0-G 闭环状态机输入）。
    /// <see cref="IInputSnapshot.IsTriggered"/> 按此维度查询当前是否触发。
    /// </summary>
    public enum SafetyInputKind
    {
        [Description("急停")]
        EStop,
        [Description("安全门")]
        DoorSafety,
        [Description("门锁")]
        DoorLock,
        [Description("正限位")]
        AxisLimitPos,
        [Description("负限位")]
        AxisLimitNeg,
        [Description("伺服报警")]
        ServoAlarm,
        [Description("刹车")]
        Brake,
        [Description("上游互锁")]
        UpstreamInterlock,
        [Description("下游互锁")]
        DownstreamInterlock
    }

    /// <summary>
    /// 互锁规则输入条件聚合模式（ADR-C seam，TES-38）。
    /// 对齐源端 <c>BuildSafeCondition</c> 的 All(全部满足) 与任一满足(Any) 两种判定语义。
    /// </summary>
    public enum InterlockJudgeMode
    {
        /// <summary>全部满足(AND)：所有 Inputs 条件成立才触发规则</summary>
        [Description("全部满足")]
        All = 0,

        /// <summary>任一满足(OR)：任一 Input 条件成立即触发规则</summary>
        [Description("任一满足")]
        Any = 1
    }

    /// <summary>
    /// 互锁检查模式（TES-38）。
    /// 对齐源端 <c>CheckNormalAction</c> 的单次快照判定与连续等待判定。
    /// </summary>
    public enum InterlockCheckMode
    {
        /// <summary>单次检查：当前快照求值，触发即报警阻断</summary>
        [Description("单次检查")]
        One = 0,

        /// <summary>连续检查：在超时时间内轮询等待条件解除，超时则报警</summary>
        [Description("连续检查")]
        Wait = 1
    }
}
