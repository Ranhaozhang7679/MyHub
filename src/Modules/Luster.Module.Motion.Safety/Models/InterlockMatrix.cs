using Luster.Motion.DataStruct.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Luster.Module.Motion.Safety.Models
{
    /// <summary>
    /// 安全联锁矩阵：聚合多条 <see cref="InterlockRule"/>，统一求值并产出触发的报警契约。
    /// 对应 TES-38「安全联锁矩阵」+ 源端散点互锁（急停/安全门/门锁/轴限位/伺服/上下游互锁）。
    /// 上下游互锁子项（InterlockConditionType.HandshakeBit）等 TES-37 的 HandoverNode 出来后挂载。
    /// </summary>
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
        /// 求值所有规则，返回触发的报警契约列表（按 Severity 降序）。
        /// </summary>
        /// <param name="resolver">条件求值回调，由宿主注入设备/IO/轴/握手位的真实读取逻辑</param>
        public List<AlarmSchema> Evaluate(Func<InterlockCondition, bool> resolver)
        {
            var triggered = new List<AlarmSchema>();
            foreach (var rule in _rules)
            {
                if (rule.Evaluate(resolver))
                {
                    triggered.Add(rule.Alarm);
                }
            }
            return triggered
                .OrderByDescending(a => a.Severity)
                .ToList();
        }

        /// <summary>是否存在致命级（Fatal）触发</summary>
        public bool HasFatal(Func<InterlockCondition, bool> resolver)
            => Evaluate(resolver).Any(a => a.Severity == AlarmSeverity.Fatal);
    }
}
