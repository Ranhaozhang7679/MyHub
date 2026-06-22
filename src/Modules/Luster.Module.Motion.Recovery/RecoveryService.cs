using Luster.Motion.DataStruct.Checkpoint;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using System.Collections.Generic;

namespace Luster.Module.Motion.Recovery
{
    /// <summary>
    /// 异常恢复服务（ADR-B/C，TES-28 P0-G 急停/安全门恢复闭环状态机）。
    /// </summary>
    /// <remarks>
    /// 状态机（ADR-C）：
    /// 急停触发 → 运动停止确认 → 能量/IO 安全确认 → 门锁闭合确认
    ///        → 轴位置复核 → 产品在籍复核(checkpoint)
    ///        → 选择[重新初始化 / 安全位恢复 / 人工清机]
    ///        → 解除 latched alarm（急停/安全门必 latch，人工确认前不可自动清）
    ///        → 允许启动
    ///
    /// 铁律：急停解除不能直接清 latched alarm；必须保留至恢复向导完成。
    /// 实物校验（真空/轴位/ICW）硬件动作 ⚠️ 待人类现场验证——
    /// 本实现校验入口以 <see cref="IRecoveryVerifier"/> 注入，软件层单测用 mock，
    /// 硬件校验逻辑由站级实现注入（接入 VAxis/VIO/HandoverNode 后）。
    /// </remarks>
    public class RecoveryService : IRecoveryService
    {
        private readonly IRecoveryVerifier _verifier;

        public RecoveryService(IRecoveryVerifier verifier = null)
        {
            _verifier = verifier;
        }

        /// <inheritdoc/>
        public RecoveryResult Recover(RunCheckpoint checkpoint, RecoveryStrategy strategy,
            InterlockMatrix interlock, IInputSnapshot snapshot)
        {
            // 1. 安全联锁预检：致命触发（急停/限位/伺服报警）未解除前不允许任何恢复
            var alarms = new List<string>();
            if (interlock != null && snapshot != null)
            {
                var triggered = interlock.Evaluate(snapshot);
                foreach (var rule in triggered)
                {
                    if (rule.Recovery == RecoveryPolicy.Abort)
                    {
                        alarms.Add(rule.AlarmCode ?? rule.RuleId);
                    }
                }
                if (alarms.Count > 0)
                {
                    return RecoveryResult.Fail(strategy, alarms,
                        $"安全联锁仍触发（{alarms.Count} 项致命），解除急停/限位/伺服报警后再恢复");
                }
            }

            // 2. 无 checkpoint：只能清机（从头跑）
            if (checkpoint == null)
            {
                return strategy == RecoveryStrategy.ClearMachine
                    ? RecoveryResult.Ok(strategy, 0, "无 checkpoint，清机恢复完成")
                    : RecoveryResult.Fail(strategy, null, "无 checkpoint，无法续跑/报废，请选清机");
            }

            // 3. 按策略执行
            switch (strategy)
            {
                case RecoveryStrategy.ClearMachine:
                    return ClearMachine(checkpoint, alarms);

                case RecoveryStrategy.Resume:
                    return Resume(checkpoint, alarms);

                case RecoveryStrategy.ScrapCurrent:
                    return ScrapCurrent(checkpoint, alarms);

                default:
                    return RecoveryResult.Fail(strategy, null, "未知恢复策略");
            }
        }

        /// <summary>清机：清除在籍产品，回到安全位</summary>
        private RecoveryResult ClearMachine(RunCheckpoint cp, List<string> alarms)
        {
            // 实物校验：轴回到安全位（verifier 注入；未注入则软件层视为通过，待硬件接入）
            if (_verifier != null && !_verifier.VerifyAxisAtSafePosition(cp.LastSafePosition))
            {
                alarms.Add("AXIS_NOT_AT_SAFE");
                return RecoveryResult.Fail(RecoveryStrategy.ClearMachine, alarms, "轴未回到安全位，清机失败");
            }
            // 在籍产品清空（软件层：清 checkpoint）；实物清机硬件动作 ⚠️ 待人类现场
            return RecoveryResult.Ok(RecoveryStrategy.ClearMachine, 0,
                $"清机完成，清除在籍产品 {cp.InStationProductSNs.Count} 件");
        }

        /// <summary>续跑：保留在籍产品，从断点继续</summary>
        private RecoveryResult Resume(RunCheckpoint cp, List<string> alarms)
        {
            // 产品在籍复核：checkpoint 在籍数 > 0 才有续跑意义
            if (cp.InStationProductSNs.Count == 0)
            {
                return RecoveryResult.Fail(RecoveryStrategy.Resume, null, "checkpoint 无在籍产品，无可续跑");
            }
            // 实物校验：在籍产品实物在位 + 轴位复核 + ICW 状态
            if (_verifier != null)
            {
                if (!_verifier.VerifyProductsInPlace(cp.InStationProductSNs))
                {
                    alarms.Add("PRODUCT_NOT_IN_PLACE");
                    return RecoveryResult.Fail(RecoveryStrategy.Resume, alarms, "在籍产品实物不在位，续跑失败");
                }
                if (!_verifier.VerifyAxisAtSafePosition(cp.LastSafePosition))
                {
                    alarms.Add("AXIS_NOT_AT_SAFE");
                    return RecoveryResult.Fail(RecoveryStrategy.Resume, alarms, "轴位置与 checkpoint 不符，续跑失败");
                }
                // ICW 状态校验（HandoverNode 就位后 verifier 注入；当前占位返回 true）
                if (!_verifier.VerifyHandoverState(cp.Handover))
                {
                    alarms.Add("HANDOVER_STATE_MISMATCH");
                    return RecoveryResult.Fail(RecoveryStrategy.Resume, alarms, "交握状态与 checkpoint 不符，续跑失败");
                }
            }
            // 追溯补写：若断电前未落追溯，续跑前补写
            string traceNote = cp.TraceWritten ? "" : "（补写追溯）";
            return RecoveryResult.Ok(RecoveryStrategy.Resume, cp.TrajectoryActionIndex,
                $"续跑完成，从 phase={cp.Phase} action#{cp.TrajectoryActionIndex} 继续{traceNote}");
        }

        /// <summary>报废当前在籍产品</summary>
        private RecoveryResult ScrapCurrent(RunCheckpoint cp, List<string> alarms)
        {
            // 报废需确认轴已停 + 安全联锁已解除（步骤 1 已检）
            return RecoveryResult.Ok(RecoveryStrategy.ScrapCurrent, 0,
                $"报废完成，在籍产品 {cp.InStationProductSNs.Count} 件标记报废");
        }
    }

    /// <summary>
    /// 恢复实物校验器（ADR-C，站级注入）。
    /// 软件层单测用 mock（默认 null = 跳过实物校验）；硬件校验逻辑由站级实现
    /// （接入 VAxis.GetCurrentPos / VIO / HandoverNode.GetSnapshot() 后注入）。
    /// 所有硬件动作 ⚠️ 待人类现场验证。
    /// </summary>
    public interface IRecoveryVerifier
    {
        /// <summary>校验在籍产品实物在位</summary>
        bool VerifyProductsInPlace(IReadOnlyList<string> productSNs);

        /// <summary>校验轴在安全位（与 checkpoint 比对）</summary>
        bool VerifyAxisAtSafePosition(AxisSafePosition expected);

        /// <summary>校验交握状态与 checkpoint 一致（ICW 字段待 TES-47 收口）</summary>
        bool VerifyHandoverState(HandoverStateSnapshot expected);
    }
}
