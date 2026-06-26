using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Luster.Motion.DataStruct.DataModels
{
    /// <summary>
    /// 安全联锁矩阵（ADR-C seam，TES-38 产出 / TES-28 P0-G 恢复闭环唯一输入）。
    /// 聚合多条 <see cref="InterlockRule"/>，<see cref="Evaluate(IInputSnapshot)"/> 返回触发的规则集。
    /// </summary>
    /// <remarks>
    /// TES-28 P0-G 恢复状态机不直接读 IO，只消费 <see cref="Evaluate"/> 返回的规则列表
    /// （P0-G↔P7-C seam，架构师 Q1 决策落地）。
    /// "上下游互锁挂 <c>HandoverNode</c>"子项等 TES-37 的 <c>HandoverNode</c> 落地后接入：
    /// 届时 <see cref="IInputSnapshot"/> 实现把 <c>UpstreamInterlock</c>/<c>DownstreamInterlock</c>
    /// 维度投影自 <c>HandoverNode.GetSnapshot()</c>。
    /// </remarks>
    public class InterlockMatrix
    {
        private readonly List<InterlockRule> _rules = new List<InterlockRule>();

        public InterlockMatrix(IEnumerable<InterlockRule> rules = null)
        {
            if (rules != null) _rules.AddRange(rules);
        }

        public IReadOnlyList<InterlockRule> Rules => _rules;

        public void Add(InterlockRule rule) => _rules.Add(rule);

        public void Clear() => _rules.Clear();

        /// <summary>
        /// 评估当前输入快照，返回触发的规则集（按 <see cref="InterlockRule.Recovery"/> 严重度降序）。
        /// 这是 TES-28 P0-G 恢复闭环状态机的唯一输入。
        /// </summary>
        public IReadOnlyList<InterlockRule> Evaluate(IInputSnapshot inputs)
        {
            var triggered = new List<InterlockRule>();
            foreach (var rule in _rules)
            {
                if (rule.Evaluate(inputs))
                {
                    triggered.Add(rule);
                }
            }
            // 按恢复策略严重度降序：Abort > Scrap > Clean > Resume > Home > Retry > Skip > Manual > None
            return triggered
                .OrderByDescending(r => RecoverySeverity(r.Recovery))
                .ThenBy(r => r.RuleId)
                .ToList();
        }

        /// <summary>是否存在致命级（Abort 急停）触发</summary>
        public bool HasFatal(IInputSnapshot inputs)
            => Evaluate(inputs).Any(r => r.Recovery == RecoveryPolicy.Abort);

        /// <summary>恢复策略 → 严重度权重（用于排序，非契约）</summary>
        private static int RecoverySeverity(RecoveryPolicy policy)
        {
            switch (policy)
            {
                case RecoveryPolicy.Abort: return 100;
                case RecoveryPolicy.Scrap: return 80;
                case RecoveryPolicy.Clean: return 70;
                case RecoveryPolicy.Resume: return 60;
                case RecoveryPolicy.Home: return 50;
                case RecoveryPolicy.Retry: return 40;
                case RecoveryPolicy.Skip: return 30;
                case RecoveryPolicy.Manual: return 20;
                default: return 0;
            }
        }
    }
}
