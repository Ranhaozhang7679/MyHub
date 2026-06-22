using Luster.Motion.DataStruct.Checkpoint;
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
    /// 上下游互锁挂 <c>HandoverNode</c>：经 <see cref="AttachTo(IHandoverSnapshotProvider)"/>
    /// 挂载交握快照提供者，<see cref="CreateSnapshot(IInputSnapshot)"/> 把交握 InterLock/DoorLock
    /// 投影成 <see cref="SafetyInputKind.UpstreamInterlock"/>/<see cref="SafetyInputKind.DownstreamInterlock"/>
    /// 维度，与物理 IO 快照复合后喂 <see cref="Evaluate(IInputSnapshot)"/>。
    /// </remarks>
    public class InterlockMatrix
    {
        private readonly List<InterlockRule> _rules = new List<InterlockRule>();
        private readonly List<IHandoverSnapshotProvider> _handovers = new List<IHandoverSnapshotProvider>();

        public InterlockMatrix(IEnumerable<InterlockRule> rules = null)
        {
            if (rules != null) _rules.AddRange(rules);
        }

        public IReadOnlyList<InterlockRule> Rules => _rules;

        public void Add(InterlockRule rule) => _rules.Add(rule);

        public void Clear()
        {
            _rules.Clear();
            _handovers.Clear();
        }

        /// <summary>
        /// 挂载交握快照提供者（ADR-C seam，上下游互锁随交握状态动态生效）。
        /// <para><c>HandoverNode</c> 实现数 <see cref="IHandoverSnapshotProvider"/>，挂载后
        /// <see cref="CreateSnapshot(IInputSnapshot)"/> 会把其 InterLock/DoorLock 位投影到
        /// Upstream/DownstreamInterlock 维度。</para>
        /// </summary>
        public void AttachTo(IHandoverSnapshotProvider handover)
        {
            if (handover != null && !_handovers.Contains(handover))
            {
                _handovers.Add(handover);
            }
        }

        /// <summary>
        /// 构造复合输入快照：物理 IO 快照 + 挂载的 Handover 互锁维度。
        /// <para>TES-28 Recovery 调用：<c>matrix.Evaluate(matrix.CreateSnapshot(physicalSnapshot))</c>。
        /// 未挂载 Handover 时返回原物理快照。</para>
        /// </summary>
        public IInputSnapshot CreateSnapshot(IInputSnapshot physicalSnapshot)
        {
            if (_handovers.Count == 0) return physicalSnapshot;
            return new CompositeInputSnapshot(physicalSnapshot, _handovers.ToList());
        }

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

    /// <summary>
    /// 复合输入快照：物理 IO 快照 + 挂载的 Handover 互锁维度（ADR-C）。
    /// <para>UpstreamInterlock/DownstreamInterlock 维度投影自挂载的
    /// <see cref="IHandoverSnapshotProvider"/> 的 InterLock/DoorLock 位；
    /// 其余维度委托物理快照。</para>
    /// </summary>
    internal sealed class CompositeInputSnapshot : IInputSnapshot
    {
        private readonly IInputSnapshot _physical;
        private readonly IReadOnlyList<IHandoverSnapshotProvider> _handovers;

        public CompositeInputSnapshot(IInputSnapshot physical, IReadOnlyList<IHandoverSnapshotProvider> handovers)
        {
            _physical = physical;
            _handovers = handovers ?? new List<IHandoverSnapshotProvider>();
        }

        public bool IsTriggered(SafetyInputKind kind, string target)
        {
            // 上下游互锁维度：从挂载的 Handover 快照投影
            if (kind == SafetyInputKind.UpstreamInterlock || kind == SafetyInputKind.DownstreamInterlock)
            {
                foreach (var h in _handovers)
                {
                    var snap = h.GetSnapshot();
                    if (snap == null) continue;
                    // 角色匹配：Feed=上游,Leave=下游
                    bool isUpstream = string.Equals(snap.Role, "Feed", System.StringComparison.OrdinalIgnoreCase);
                    bool isDownstream = string.Equals(snap.Role, "Leave", System.StringComparison.OrdinalIgnoreCase);
                    if (kind == SafetyInputKind.UpstreamInterlock && isUpstream)
                    {
                        // 上游互锁触发 = InterLock 信号 ON 或 对端门锁未到位
                        if (snap.Signals.InterLock || !snap.Signals.DoorLock) return true;
                    }
                    if (kind == SafetyInputKind.DownstreamInterlock && isDownstream)
                    {
                        if (snap.Signals.InterLock || !snap.Signals.DoorLock) return true;
                    }
                }
                return false;
            }

            // 其余维度委托物理快照
            return _physical != null && _physical.IsTriggered(kind, target);
        }
    }
}
