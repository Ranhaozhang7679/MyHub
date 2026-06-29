using System;
using System.Collections.Generic;
using System.Reflection;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Module.Motion.Safety;
using Luster.Module.Motion.Safety.Functions;
using Luster.TaskFlow.Motion;
using Moq;
using Xunit;

namespace Luster.Module.Motion.Regression.Baseline.Tests
{
    /// <summary>
    /// ⑥ 互锁矩阵回归 (TES-165 P9-D / Suite=Safety)。
    /// 锁定 CheckInterlock.DoExcute 互锁求值 + 告警路由契约：
    /// IO 经 IInputSnapshot 注入（不直接读 IO），InterlockMatrix 经 SafetyModule 静态注册表查找。
    /// 任一规则触发即上报最高严重度并返回 false（errMsg 含规则 ID）；无触发则放行返回 true。
    /// 无硬件依赖，必 GREEN。
    /// 注：方法名用 ASCII，规避 xunit.runner.visualstudio 3.1.5 对中文方法名的发现不稳定问题。
    /// </summary>
    /// <remarks>
    /// MyOwner 注入方式：CheckInterlock 继承 MotionFunction，其 Owner.set 会触发 InitParameters/Init，
    /// 在 Moq IMotionModule 代理上访问 MyOwner.Parameters（默认 null）会导致测试宿主进程崩溃
    /// （xunit 静默丢弃用例，既不报 Passed 也不报 Failed）。故用反射直接设 motionModule 私有字段，
    /// 绕过 setter 的初始化逻辑——DoExcute 仅依赖 MyOwner.OnAlarm，Mock 默认实现即可满足。
    /// </remarks>
    [Trait("Category", "Regression")]
    [Trait("Suite", "Safety")]
    public class SafetyRegressionTests
    {
        // 互锁触发即阻断：单条 EStop 规则触发 → DoExcute 返回 false + errMsg 含规则 ID
        [Fact]
        public void CheckInterlock_TriggeredRuleBlocksAndReports()
        {
            var matrix = new InterlockMatrix(new[]
            {
                Rule("EMG", RecoveryPolicy.Abort, (SafetyInputKind.EStop, "e1"))
            });
            // 快照返回 EStop 触发
            var snapshot = new FuncSnapshot((k, t) => k == SafetyInputKind.EStop);

            using (var reg = new StaticRegistry("ICW_TEST_TRIG", matrix, "ICW_TEST_SNAP", snapshot))
            {
                var owner = new Mock<IMotionModule>();
                var check = new CheckInterlock { MatrixName = reg.MatrixName, SnapshotFactoryName = reg.SnapshotName };
                SetOwner(check, owner.Object);

                bool ok = check.DoExcute(out string errMsg);

                // 触发即阻断
                Assert.False(ok, "互锁触发应阻断流程返回 false");
                // errMsg 含规则 ID（CheckInterlock.cs:69 errMsg = $"互锁触发：{top.RuleId}"）
                Assert.Contains("EMG", errMsg);
                Assert.Contains("互锁触发", errMsg);
            }
        }

        // 最高严重度上报正确：多规则同时触发，Evaluate 按 Recovery 降序 → top 为 Abort
        [Fact]
        public void CheckInterlock_HighestSeverityReported()
        {
            var matrix = new InterlockMatrix(new[]
            {
                Rule("WARN", RecoveryPolicy.Manual, (SafetyInputKind.DoorLock, "门锁A")),
                Rule("SCRAP", RecoveryPolicy.Scrap, (SafetyInputKind.UpstreamInterlock, "上游A")),
                Rule("ABORT", RecoveryPolicy.Abort, (SafetyInputKind.EStop, "e1"))
            });
            // 快照返回全部触发
            var snapshot = new FuncSnapshot((k, t) => true);

            using (var reg = new StaticRegistry("ICW_TEST_SEV", matrix, "ICW_TEST_SEV_SNAP", snapshot))
            {
                var owner = new Mock<IMotionModule>();
                var check = new CheckInterlock { MatrixName = reg.MatrixName, SnapshotFactoryName = reg.SnapshotName };
                SetOwner(check, owner.Object);

                bool ok = check.DoExcute(out string errMsg);

                Assert.False(ok);
                // top 应为 ABORT（RecoveryPolicy.Abort 严重度最高，Evaluate 已降序排序）
                Assert.Contains("ABORT", errMsg);
            }
        }

        // 无触发不告警：规则均未满足 → DoExcute 放行返回 true，errMsg 为空
        [Fact]
        public void CheckInterlock_NoTriggerPassesWithoutAlarm()
        {
            var matrix = new InterlockMatrix(new[]
            {
                Rule("R1", RecoveryPolicy.Manual, (SafetyInputKind.EStop, "e1"))
            });
            // 快照返回全部未触发
            var snapshot = new FuncSnapshot((k, t) => false);

            using (var reg = new StaticRegistry("ICW_TEST_PASS", matrix, "ICW_TEST_PASS_SNAP", snapshot))
            {
                var owner = new Mock<IMotionModule>();
                var check = new CheckInterlock { MatrixName = reg.MatrixName, SnapshotFactoryName = reg.SnapshotName };
                SetOwner(check, owner.Object);

                bool ok = check.DoExcute(out string errMsg);

                // 无触发即放行
                Assert.True(ok, "无互锁触发应放行返回 true");
                Assert.Equal(string.Empty, errMsg);
            }
        }

        #region 测试辅助

        /// <summary>
        /// 用反射直接设 MotionFunction.motionModule 私有字段，绕过 Owner.set 的 InitParameters/Init
        /// （后者在 Moq 代理上访问 MyOwner.Parameters 会导致测试宿主崩溃，详见类注释）。
        /// </summary>
        private static void SetOwner(MotionFunction function, IMotionModule owner)
        {
            var field = typeof(MotionFunction).GetField("motionModule",
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(field);
            field.SetValue(function, owner);
        }

        /// <summary>构造 InterlockRule（AlarmCode=CODE_+id，所有输入 Expected=true）</summary>
        private static InterlockRule Rule(string id, RecoveryPolicy recovery, params (SafetyInputKind, string)[] inputs)
        {
            var rule = new InterlockRule
            {
                RuleId = id,
                AlarmCode = "CODE_" + id,
                Recovery = recovery,
            };
            var list = new List<InterlockInput>();
            foreach (var (kind, target) in inputs)
            {
                list.Add(new InterlockInput { Kind = kind, Target = target, Expected = true });
            }
            rule.Inputs = list.ToArray();
            return rule;
        }

        /// <summary>用 lambda 构造可编程 IInputSnapshot，便于单测</summary>
        private sealed class FuncSnapshot : IInputSnapshot
        {
            private readonly Func<SafetyInputKind, string, bool> _func;
            public FuncSnapshot(Func<SafetyInputKind, string, bool> func) { _func = func; }
            public bool IsTriggered(SafetyInputKind kind, string target = null) => _func(kind, target);
        }

        /// <summary>
        /// 静态注册表作用域：构造时注册 fake matrix + snapshot factory，Dispose 时还原 matrix
        /// （SafetyModule._matrices / _snapshotFactories 是全局静态，用例间必须清理避免污染）。
        /// 用唯一名隔离：每个用例用独立 matrixName/snapshotName，snapshot factory 不还原也不冲突。
        /// </summary>
        private sealed class StaticRegistry : IDisposable
        {
            public string MatrixName { get; }
            public string SnapshotName { get; }
            private readonly bool _matrixExisted;
            private readonly InterlockMatrix _oldMatrix;

            public StaticRegistry(string matrixName, InterlockMatrix matrix,
                string snapshotName, IInputSnapshot constantSnapshot)
            {
                MatrixName = matrixName;
                SnapshotName = snapshotName;
                // 记录旧 matrix 以便还原（同名覆盖时）
                _oldMatrix = SafetyModule.LookupMatrix(matrixName);
                _matrixExisted = _oldMatrix != null;

                SafetyModule.RegisterMatrix(matrixName, matrix);
                // 快照工厂返回固定实例（不依赖 IMotionModule 设备读取）
                SafetyModule.RegisterSnapshotFactory(snapshotName, _ => constantSnapshot);
            }

            public void Dispose()
            {
                // 还原 matrix（若原本不存在则不还原，由唯一名隔离保证不污染其他用例）
                if (_matrixExisted)
                {
                    SafetyModule.RegisterMatrix(MatrixName, _oldMatrix);
                }
            }
        }

        #endregion
    }
}
