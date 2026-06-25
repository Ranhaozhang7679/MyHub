using Luster.Module.Motion.TestToolchain.Simulation;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.TaskFlow.Engine.Models;
using Xunit;

namespace Luster.Module.Motion.TestToolchainTests
{
    /// <summary>
    /// TES-34 P9-C 单测：OpticalSimulator 脉宽→灰度算法 + SimulationProfile 几何结构 +
    /// VirtualModeMapping 虚拟模式映射。
    /// </summary>
    [Trait("Category", "SmokeTest")]
    public class SimulationTests
    {
        #region OpticalSimulator

        [Fact]
        public void OpticalSimulator_零脉宽返回全零()
        {
            var sim = new OpticalSimulator();
            var rgb = sim.SimulateCamera(0, 0, 0, 1f, 1f, 1f);
            Assert.Equal(0, rgb.R);
            Assert.Equal(0, rgb.G);
            Assert.Equal(0, rgb.B);
        }

        [Fact]
        public void OpticalSimulator_满脉宽钳位255()
        {
            // 100 脉宽 × 权重 1：经 gamma 1.2 + 串扰矩阵 + 颜色校正矩阵，G/B 超 255 钳位
            var sim = new OpticalSimulator();
            var rgb = sim.SimulateCamera(100, 100, 100, 1f, 1f, 1f);
            Assert.Equal(255, rgb.G);
            Assert.Equal(255, rgb.B);
            Assert.InRange(rgb.R, 240, 255);
        }

        [Fact]
        public void OpticalSimulator_确定性_同输入同输出()
        {
            // 噪声项源端已注释，本类无噪声，输出确定
            var sim = new OpticalSimulator();
            var a = sim.SimulateCamera(50, 60, 70, 0.8f, 0.9f, 1.0f);
            var b = sim.SimulateCamera(50, 60, 70, 0.8f, 0.9f, 1.0f);
            Assert.Equal(a.R, b.R);
            Assert.Equal(a.G, b.G);
            Assert.Equal(a.B, b.B);
        }

        [Fact]
        public void OpticalSimulator_亮度权重影响输出()
        {
            var sim = new OpticalSimulator();
            var full = sim.SimulateCamera(100, 100, 100, 1f, 1f, 1f);
            var half = sim.SimulateCamera(100, 100, 100, 0.5f, 0.5f, 0.5f);
            // 权重减半 → 光强减半 → 输出更低
            Assert.True(half.R <= full.R);
            Assert.True(half.G <= full.G);
        }

        #endregion

        #region SimulationProfile

        [Fact]
        public void SimulationProfile_默认零点()
        {
            var sp = new SimulationProfile();
            Assert.Equal(0, sp.FloorShape.X);
            Assert.Equal(0, sp.FloorShape.Y);
            Assert.Equal(0, sp.ARotateCenter.Z);
        }

        [Fact]
        public void SimulationProfile_CopyFrom逐字段复制()
        {
            var src = new SimulationProfile
            {
                FloorShape = new SimPoint3(1, 2, 3),
                ARotateCenter = new SimPoint3(4, 5, 6)
            };
            var dst = new SimulationProfile();
            dst.CopyFrom(src);
            Assert.Equal(1, dst.FloorShape.X);
            Assert.Equal(3, dst.FloorShape.Z);
            Assert.Equal(6, dst.ARotateCenter.Z);
        }

        #endregion

        #region VirtualModeMapping

        [Fact]
        public void VirtualModeMapping_默认Virtual为虚拟模式()
        {
            // SystemConfig.RunMode 默认 DeviceMode.Virtual（对齐源端 PluginComponent 启动置 VIRTUAL_MODE=true）
            var config = new SystemConfig();
            Assert.True(VirtualModeMapping.IsVirtualMode(config));
        }

        [Fact]
        public void VirtualModeMapping_Real模式非虚拟()
        {
            var config = new SystemConfig { RunMode = DeviceMode.Real };
            Assert.False(VirtualModeMapping.IsVirtualMode(config));
        }

        [Fact]
        public void VirtualModeMapping_null配置返回false()
        {
            Assert.False(VirtualModeMapping.IsVirtualMode(null));
        }

        [Fact]
        public void SetVirtualMode_切换Virtual与Real往返有效()
        {
            // 验收点：VIRTUAL_MODE 全局开关可正常切换（对应源端 ProfilePluginManager.VIRTUAL_MODE = true/false）
            var config = new SystemConfig();
            VirtualModeMapping.SetVirtualMode(config, true);
            Assert.True(VirtualModeMapping.IsVirtualMode(config));
            VirtualModeMapping.SetVirtualMode(config, false);
            Assert.False(VirtualModeMapping.IsVirtualMode(config));
        }

        #endregion
    }
}
