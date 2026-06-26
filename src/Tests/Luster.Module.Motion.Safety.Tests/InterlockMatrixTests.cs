using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using System;
using System.Linq;
using Xunit;

namespace Luster.Module.Motion.Safety.Tests
{
    public class InterlockMatrixTests
    {
        /// <summary>用 lambda 构造可编程 IInputSnapshot，便于单测</summary>
        private sealed class FuncSnapshot : IInputSnapshot
        {
            private readonly Func<SafetyInputKind, string, bool> _func;
            public FuncSnapshot(Func<SafetyInputKind, string, bool> func) { _func = func; }
            public bool IsTriggered(SafetyInputKind kind, string target = null) => _func(kind, target);
        }

        private static InterlockRule Rule(string id, RecoveryPolicy recovery, params (SafetyInputKind, string)[] inputs)
        {
            return new InterlockRule
            {
                RuleId = id,
                AlarmCode = "CODE_" + id,
                Recovery = recovery,
                Inputs = inputs.Select(t => new InterlockInput { Kind = t.Item1, Target = t.Item2, Expected = true }).ToArray()
            };
        }

        [Fact]
        public void Evaluate_全部条件成立才触发()
        {
            var matrix = new InterlockMatrix(new[]
            {
                Rule("R1", RecoveryPolicy.Manual,
                    (SafetyInputKind.UpstreamInterlock, "上游A"), (SafetyInputKind.DoorLock, "门锁A"))
            });

            // 只有一个条件触发 → 不触发
            var r1 = matrix.Evaluate(new FuncSnapshot((k, t) => k == SafetyInputKind.UpstreamInterlock));
            Assert.Empty(r1);

            // 两个条件都触发 → 触发
            var r2 = matrix.Evaluate(new FuncSnapshot((k, t) => true));
            Assert.Single(r2);
            Assert.Equal("R1", r2[0].RuleId);
        }

        [Fact]
        public void Evaluate_按Recovery严重度降序排序()
        {
            var matrix = new InterlockMatrix(new[]
            {
                Rule("WARN", RecoveryPolicy.Manual, (SafetyInputKind.EStop, "e1")),
                Rule("ABORT", RecoveryPolicy.Abort, (SafetyInputKind.EStop, "e2")),
                Rule("SCRAP", RecoveryPolicy.Scrap, (SafetyInputKind.EStop, "e3"))
            });

            var triggered = matrix.Evaluate(new FuncSnapshot((k, t) => true));
            Assert.Equal(new[] { "ABORT", "SCRAP", "WARN" }, triggered.Select(r => r.RuleId).ToArray());
        }

        [Fact]
        public void HasFatal_Abort触发返回true()
        {
            var matrix = new InterlockMatrix(new[]
            {
                Rule("EMG", RecoveryPolicy.Abort, (SafetyInputKind.EStop, "e1"))
            });

            Assert.True(matrix.HasFatal(new FuncSnapshot((k, t) => true)));
            Assert.False(matrix.HasFatal(new FuncSnapshot((k, t) => false)));
        }

        [Fact]
        public void Evaluate_禁用规则不触发()
        {
            var rule = Rule("OFF", RecoveryPolicy.Manual, (SafetyInputKind.EStop, "e1"));
            rule.Enable = false;
            var matrix = new InterlockMatrix(new[] { rule });

            Assert.Empty(matrix.Evaluate(new FuncSnapshot((k, t) => true)));
        }

        [Fact]
        public void Evaluate_ExpectedFalse条件成立()
        {
            // Expected=false：该维度未触发时条件成立（例如"门锁到位"=DoorLock 不触发）
            var rule = new InterlockRule
            {
                RuleId = "DOOR_OK",
                Inputs = new[] { new InterlockInput { Kind = SafetyInputKind.DoorLock, Target = "门锁", Expected = false } }
            };
            var matrix = new InterlockMatrix(new[] { rule });

            // DoorLock 未触发(false) → Expected=false 匹配 → 规则触发
            Assert.Single(matrix.Evaluate(new FuncSnapshot((k, t) => false)));
            // DoorLock 触发(true) → Expected=false 不匹配 → 不触发
            Assert.Empty(matrix.Evaluate(new FuncSnapshot((k, t) => true)));
        }

        [Fact]
        public void Add_Clear_动态增删规则()
        {
            var matrix = new InterlockMatrix();
            Assert.Empty(matrix.Rules);

            matrix.Add(Rule("X", RecoveryPolicy.Manual, (SafetyInputKind.EStop, "e")));
            Assert.Single(matrix.Rules);

            matrix.Clear();
            Assert.Empty(matrix.Rules);
        }
    }
}
