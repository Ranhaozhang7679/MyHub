using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Interfaces;

namespace Luster.Motion.DataStruct.Checkpoint
{
    /// <summary>
    /// checkpoint 持久化存储（ADR-B seam，TES-28 实现消费）。
    /// 落盘点 = 关键 phase 转换（产品维度事务边界），原子写（临时文件 → 替换）。
    /// </summary>
    public interface ICheckpointStore
    {
        /// <summary>加载指定工位的 checkpoint；不存在返回 null</summary>
        RunCheckpoint Load(string stationId);

        /// <summary>保存 checkpoint（原子写：写临时文件 → 替换）</summary>
        void Save(RunCheckpoint checkpoint);

        /// <summary>清除指定工位的 checkpoint（恢复成功后调用）</summary>
        void Clear(string stationId);
    }

    /// <summary>
    /// 异常恢复服务（ADR-B/C seam，TES-28 P0-G 恢复闭环入口）。
    /// 三策略由 UI 选择后调用 <see cref="Recover"/>，内部按
    /// <see cref="InterlockMatrix"/>.Evaluate(<see cref="IInputSnapshot"/>) + 实物校验决定续跑点。
    /// </summary>
    /// <remarks>
    /// 铁律：急停/安全门 latched alarm 在恢复向导完成前不可自动清除
    /// （对齐源端 <c>MachineManager.Restore()</c> 缺陷修复）。
    /// 实物校验（真空/轴位/ICW）硬件动作 ⚠️ 待人类现场验证，软件层校验逻辑可单测。
    /// </remarks>
    public interface IRecoveryService
    {
        /// <summary>
        /// 执行恢复三策略。
        /// </summary>
        /// <param name="checkpoint">断电前落盘的 checkpoint（null 表示无 checkpoint，按 ClearMachine 处理）</param>
        /// <param name="strategy">用户选择的恢复策略</param>
        /// <param name="interlock">安全联锁矩阵（ADR-C，恢复前求值判安全）</param>
        /// <param name="snapshot">当前安全输入快照（喂 InterlockMatrix.Evaluate）</param>
        RecoveryResult Recover(RunCheckpoint checkpoint, RecoveryStrategy strategy,
            InterlockMatrix interlock, IInputSnapshot snapshot);
    }
}
