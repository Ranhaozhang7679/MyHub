using Luster.Module.Motion.TestToolchain.Simulation;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.TaskFlow.Engine.Models;
using Xunit;

namespace Luster.Module.Motion.Regression.Baseline.Tests
{
    /// <summary>
    /// ② 模式切换回归 (TES-165 P9-D / Suite=ModeSwitch)。
    /// 锁定 SystemConfig.RunMode 默认 Virtual + VirtualModeMapping 往返切换语义，
    /// 复刻 SimulationTests 核心断言作为回归基线，防止 P9-C 产物被改坏。
    /// 纯逻辑，无硬件依赖，必 GREEN。
    /// 注：方法名用 ASCII，规避 xunit.runner.visualstudio 3.1.5 对中文方法名的发现不稳定问题。
    /// </summary>
    [Trait("Category", "Regression")]
    [Trait("Suite", "ModeSwitch")]
    public class ModeSwitchRegressionTests
    {
        // 对齐源端 PluginComponent 启动置 VIRTUAL_MODE=true（离线仿真基线）
        [Fact]
        public void SystemConfig_DefaultRunModeIsVirtual()
        {
            var config = new SystemConfig();
            Assert.Equal(DeviceMode.Virtual, config.RunMode);
        }

        [Fact]
        public void VirtualModeMapping_DefaultVirtualIsVirtualMode()
        {
            Assert.True(VirtualModeMapping.IsVirtualMode(new SystemConfig()));
        }

        [Fact]
        public void VirtualModeMapping_NullConfigReturnsFalse()
        {
            Assert.False(VirtualModeMapping.IsVirtualMode(null));
        }

        [Fact]
        public void SetVirtualMode_RealModeNotVirtual()
        {
            var config = new SystemConfig { RunMode = DeviceMode.Real };
            Assert.False(VirtualModeMapping.IsVirtualMode(config));
        }

        // 验收点：VIRTUAL_MODE 全局开关可正常切换（对应源端 ProfilePluginManager.VIRTUAL_MODE = true/false）
        [Fact]
        public void SetVirtualMode_VirtualRealRoundTripEffective()
        {
            var config = new SystemConfig();

            VirtualModeMapping.SetVirtualMode(config, true);
            Assert.True(VirtualModeMapping.IsVirtualMode(config));
            Assert.Equal(DeviceMode.Virtual, config.RunMode);

            VirtualModeMapping.SetVirtualMode(config, false);
            Assert.False(VirtualModeMapping.IsVirtualMode(config));
            Assert.Equal(DeviceMode.Real, config.RunMode);

            // 再切回 Virtual，验证往返不残留
            VirtualModeMapping.SetVirtualMode(config, true);
            Assert.True(VirtualModeMapping.IsVirtualMode(config));
            Assert.Equal(DeviceMode.Virtual, config.RunMode);
        }

        // null 容错：对齐 IsVirtualMode 的 null 语义，不抛异常
        [Fact]
        public void SetVirtualMode_NullConfigSafeNoop()
        {
            VirtualModeMapping.SetVirtualMode(null, true);
            Assert.False(VirtualModeMapping.IsVirtualMode(null));
        }
    }
}
