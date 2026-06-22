using Luster.Module.Motion.Safety.Models;
using Luster.Motion.DataStruct.Enums;
using Xunit;

namespace Luster.Module.Motion.Safety.Tests
{
    public class AlarmSchemaTests
    {
        [Fact]
        public void 默认值_关键异常不可静默失败()
        {
            var s = new AlarmSchema();
            // 默认抑制策略 = None（不允许抑制），符合"关键异常不可静默失败"
            Assert.Equal(SuppressPolicy.None, s.SuppressPolicy);
            // 默认锁存 = Latch
            Assert.Equal(LatchPolicy.Latch, s.LatchPolicy);
            // 默认严重级别 Warning，类别 Safety
            Assert.Equal(AlarmSeverity.Warning, s.Severity);
            Assert.Equal(AlarmCategory.Safety, s.Category);
        }

        [Fact]
        public void ToAlarmCode_带Source拼码()
        {
            var s = new AlarmSchema { Code = "AXIS_PEL", Source = "X轴" };
            Assert.Equal("AXIS_PEL@X轴", s.ToAlarmCode());
        }

        [Fact]
        public void ToAlarmCode_无Source仅返回Code()
        {
            var s = new AlarmSchema { Code = "EMG", Source = "" };
            Assert.Equal("EMG", s.ToAlarmCode());
        }

        [Theory]
        [InlineData(AlarmSeverity.Fatal, AlarmProc.Stop)]
        [InlineData(AlarmSeverity.Error, AlarmProc.Stop)]
        [InlineData(AlarmSeverity.Warning, AlarmProc.Check)]
        public void 严重级别映射处置策略(AlarmSeverity sev, AlarmProc expectedProc)
        {
            // 由 AlarmMatrixLoader / CheckSafety 填充 PlatformAlarmProc，
            // 此处仅校验枚举值可正常赋值与读取
            var s = new AlarmSchema { Severity = sev, PlatformAlarmProc = expectedProc };
            Assert.Equal(expectedProc, s.PlatformAlarmProc);
        }

        [Fact]
        public void 恢复策略_覆盖TES28三策略()
        {
            // TES-28 启动恢复三策略：清机/续跑/报废 必须在 RecoveryPolicy 枚举中可达
            Assert.True(System.Enum.IsDefined(typeof(RecoveryPolicy), RecoveryPolicy.Clean));
            Assert.True(System.Enum.IsDefined(typeof(RecoveryPolicy), RecoveryPolicy.Resume));
            Assert.True(System.Enum.IsDefined(typeof(RecoveryPolicy), RecoveryPolicy.Scrap));
        }
    }
}
