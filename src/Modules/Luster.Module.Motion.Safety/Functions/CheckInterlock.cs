using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;

namespace Luster.Module.Motion.Safety.Functions
{
    /// <summary>
    /// 上下游互锁检查节点（TES-38，InterlockMatrix 子项，ADR-C seam）。
    /// 通过 <see cref="IInputSnapshot"/> 求值 <see cref="InterlockMatrix"/>，
    /// 任一规则触发即上报 <see cref="InterlockRule.AlarmCode"/> + <see cref="RecoveryPolicy"/> 并返回 false。
    /// </summary>
    /// <remarks>
    /// 不直接读 IO——IO 投影由 <see cref="IInputSnapshot"/> 实现（默认 <see cref="InputSnapshotAdapter"/>）。
    /// 上下游互锁(UpstreamInterlock/DownstreamInterlock)维度等 TES-37 <c>HandoverNode</c> 落地后在 snapshot 适配器中接入。
    /// <para>检查模式对齐源端 <c>CheckNormalAction</c>：</para>
    /// <para>· <see cref="InterlockCheckMode.One"/>：单次快照求值，触发即报警阻断。</para>
    /// <para>· <see cref="InterlockCheckMode.Wait"/>：连续轮询，在 <see cref="TimeOut"/> 内等待互锁解除；超时仍触发则报警。</para>
    /// </remarks>
    public class CheckInterlock : MotionFunction
    {
        /// <summary>互锁矩阵配置名（指向已加载的 InterlockMatrix）</summary>
        [Parameter("互锁矩阵配置名（启动时由 SafetyModule 注册）", 0, CN = "互锁矩阵名", DefaultV = "Default")]
        public string MatrixName { get; set; } = "Default";

        /// <summary>输入快照工厂名（宿主注入，缺省走 IOInput 适配器）</summary>
        [Parameter("输入快照工厂名", 1, CN = "快照工厂", DefaultV = "IOInput")]
        public string SnapshotFactoryName { get; set; } = "IOInput";

        /// <summary>互锁检查模式：单次检查 / 连续检查(等待解除)</summary>
        [Parameter("互锁检查模式", 2, CN = "检查模式", DefaultV = InterlockCheckMode.One)]
        public InterlockCheckMode CheckMode { get; set; } = InterlockCheckMode.One;

        /// <summary>连续检查超时时间(ms)，-1 表示不超时（仅 Wait 模式生效）</summary>
        [Parameter("连续检查超时时间(ms)，-1表示不超时", 3, CN = "超时时间", DefaultV = -1)]
        public int TimeOut { get; set; } = -1;

        public CheckInterlock()
        {
            this.Tips = "上下游互锁检查";
            this.Icon = "\xe729";
        }

        public override string[] NoteParams { get; set; } = new[] { nameof(MatrixName) };

        public override bool DoExcute(out string errMsg)
        {
            errMsg = string.Empty;
            var matrix = SafetyModule.LookupMatrix(MatrixName);
            if (matrix == null)
            {
                // 矩阵未配置不阻断流程（仅提示），避免配置缺失导致全线卡死
                OnAlarm(AlarmType.InfoTip,
                    $"互锁矩阵 {MatrixName} 未加载，跳过互锁检查", "INTERLOCK_SKIP");
                return base.DoExcute(out errMsg);
            }

            var snapshot = SafetyModule.LookupSnapshot(SnapshotFactoryName, MyOwner);
            if (snapshot == null)
            {
                OnAlarm(AlarmType.WarningTip,
                    $"互锁快照工厂 {SnapshotFactoryName} 未注册", "INTERLOCK_NO_SNAPSHOT");
                errMsg = "互锁快照工厂缺失";
                return false;
            }

            // 单次检查：当前快照求值
            if (CheckMode == InterlockCheckMode.One)
            {
                return EvaluateAndReport(matrix, snapshot, out errMsg);
            }

            // 连续检查：在超时时间内轮询等待互锁解除
            bool resolved = false;
            try
            {
                WaitFunc(() => matrix.Evaluate(snapshot).Count == 0,
                    $"等待:{MyOwner.Alias} 互锁条件解除!", timeout: TimeOut);
                resolved = true;
            }
            catch
            {
                // 超时仍未解除，按当前触发项报警
                resolved = false;
            }

            if (resolved)
            {
                return base.DoExcute(out errMsg);
            }
            return EvaluateAndReport(matrix, snapshot, out errMsg);
        }

        /// <summary>
        /// 求值矩阵并上报最高严重度触发项；无触发返回 true。
        /// </summary>
        private bool EvaluateAndReport(InterlockMatrix matrix, IInputSnapshot snapshot, out string errMsg)
        {
            errMsg = string.Empty;
            IReadOnlyList<InterlockRule> triggered = matrix.Evaluate(snapshot);
            if (triggered.Count == 0)
            {
                return base.DoExcute(out errMsg);
            }

            // 上报最高严重度触发项（Evaluate 已按 Recovery 严重度降序）
            var top = triggered[0];
            OnAlarm(AlarmType.WarningTip,
                $"[互锁]{top.RuleId} code={top.AlarmCode} recovery={top.Recovery}",
                string.IsNullOrEmpty(top.AlarmCode) ? top.RuleId : top.AlarmCode);
            errMsg = $"互锁触发：{top.RuleId}";
            return false;
        }
    }
}
