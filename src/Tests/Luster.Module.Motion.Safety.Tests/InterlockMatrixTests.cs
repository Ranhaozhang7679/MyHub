using Luster.Module.Motion.Safety.Models;
using Luster.Motion.DataStruct.Enums;
using System;
using System.Linq;
using Xunit;

namespace Luster.Module.Motion.Safety.Tests
{
    public class InterlockMatrixTests
    {
        [Fact]
        public void Evaluate_全部条件成立才触发()
        {
            var rule = new InterlockRule
            {
                Name = "上下游互锁",
                Conditions = new System.Collections.Generic.List<InterlockCondition>
                {
                    new InterlockCondition { Target = "上游Ready", Expected = bool.TrueString },
                    new InterlockCondition { Target = "门锁到位", Expected = bool.TrueString }
                },
                Alarm = new AlarmSchema { Code = "INTERLOCK_1", Severity = AlarmSeverity.Warning }
            };
            var matrix = new InterlockMatrix(new[] { rule });

            // 只有一个条件成立 → 不触发
            var r1 = matrix.Evaluate(c => c.Target == "上游Ready");
            Assert.Empty(r1);

            // 两个条件都成立 → 触发
            var r2 = matrix.Evaluate(c => true);
            Assert.Single(r2);
            Assert.Equal("INTERLOCK_1", r2[0].Code);
        }

        [Fact]
        public void Evaluate_按Severity降序排序()
        {
            var matrix = new InterlockMatrix(new[]
            {
                new InterlockRule
                {
                    Conditions = new System.Collections.Generic.List<InterlockCondition>
                    { new InterlockCondition { Target = "a" } },
                    Alarm = new AlarmSchema { Code = "WARN", Severity = AlarmSeverity.Warning }
                },
                new InterlockRule
                {
                    Conditions = new System.Collections.Generic.List<InterlockCondition>
                    { new InterlockCondition { Target = "b" } },
                    Alarm = new AlarmSchema { Code = "FATAL", Severity = AlarmSeverity.Fatal }
                },
                new InterlockRule
                {
                    Conditions = new System.Collections.Generic.List<InterlockCondition>
                    { new InterlockCondition { Target = "c" } },
                    Alarm = new AlarmSchema { Code = "ERR", Severity = AlarmSeverity.Error }
                }
            });

            var triggered = matrix.Evaluate(c => true);
            Assert.Equal(new[] { "FATAL", "ERR", "WARN" }, triggered.Select(a => a.Code).ToArray());
        }

        [Fact]
        public void HasFatal_致命触发返回true()
        {
            var matrix = new InterlockMatrix(new[]
            {
                new InterlockRule
                {
                    Conditions = new System.Collections.Generic.List<InterlockCondition>
                    { new InterlockCondition { Target = "急停" } },
                    Alarm = new AlarmSchema { Code = "EMG", Severity = AlarmSeverity.Fatal }
                }
            });

            Assert.True(matrix.HasFatal(c => true));
            Assert.False(matrix.HasFatal(c => false));
        }

        [Fact]
        public void Evaluate_禁用规则不触发()
        {
            var matrix = new InterlockMatrix(new[]
            {
                new InterlockRule
                {
                    Enable = false,
                    Conditions = new System.Collections.Generic.List<InterlockCondition>
                    { new InterlockCondition { Target = "a" } },
                    Alarm = new AlarmSchema { Code = "DISABLED" }
                }
            });

            Assert.Empty(matrix.Evaluate(c => true));
        }

        [Fact]
        public void Add_Clear_动态增删规则()
        {
            var matrix = new InterlockMatrix();
            Assert.Empty(matrix.Rules);

            matrix.Add(new InterlockRule
            {
                Conditions = new System.Collections.Generic.List<InterlockCondition>
                { new InterlockCondition { Target = "a" } },
                Alarm = new AlarmSchema { Code = "X" }
            });
            Assert.Single(matrix.Rules);

            matrix.Clear();
            Assert.Empty(matrix.Rules);
        }
    }
}
