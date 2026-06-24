using Luster.Module.Motion.TestToolchain.Simulation;
using Xunit;

namespace Luster.Module.Motion.TestToolchain.Tests
{
    /// <summary>
    /// TES-68 P9-C wiring 项 3:MockScanPathGenerator 单测。
    /// </summary>
    public class MockScanPathGeneratorTests
    {
        [Fact]
        public void GenerateScanPath_底板Y轴A轴各steps点()
        {
            var gen = new MockScanPathGenerator();
            var profile = new SimulationProfile
            {
                FloorShape = new SimPoint3 { X = 100, Y = 0, Z = 0 },
                FloorOffsetPosi = new SimPoint3 { X = 0, Y = 0, Z = 10 },
                YShape = new SimPoint3 { X = 0, Y = 50, Z = 0 },
                YOffsetPosi = new SimPoint3 { X = 0, Y = 0, Z = 10 },
                AShape = new SimPoint3 { X = 30, Y = 0, Z = 0 },
                AOffsetPosi = new SimPoint3 { X = 0, Y = 0, Z = 0 },
                ARotateCenter = new SimPoint3 { X = 50, Y = 0, Z = 0 }
            };

            var path = gen.GenerateScanPath(profile, steps: 10);

            Assert.Equal(30, path.Count);  // 10×3
        }

        [Fact]
        public void GenerateScanPath_底板扫描线插值正确()
        {
            var gen = new MockScanPathGenerator();
            var profile = new SimulationProfile
            {
                FloorShape = new SimPoint3 { X = 100, Y = 0, Z = 0 },
                FloorOffsetPosi = new SimPoint3 { X = 0, Y = 0, Z = 10 }
            };

            var path = gen.GenerateScanPath(profile, steps: 5);

            // 底板 5 点:X 从 0 插值到 100,Z=10
            Assert.Equal(0, path[0].X);
            Assert.Equal(100, path[4].X);
            Assert.Equal(10, path[0].Z);
            Assert.Equal(50, path[2].X);  // 中点
        }

        [Fact]
        public void GenerateScanPath_nullProfile返回空()
        {
            var gen = new MockScanPathGenerator();
            var path = gen.GenerateScanPath(null, 10);
            Assert.Empty(path);
        }

        [Fact]
        public void GenerateScanPath_零步返回空()
        {
            var gen = new MockScanPathGenerator();
            var path = gen.GenerateScanPath(new SimulationProfile(), 0);
            Assert.Empty(path);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 3)]
        [InlineData(10, 30)]
        [InlineData(100, 300)]
        public void ExpectedPointCount_三段各steps点(int steps, int expected)
        {
            Assert.Equal(expected, MockScanPathGenerator.ExpectedPointCount(steps));
        }

        [Fact]
        public void GenerateScanPath_消费7几何字段全覆盖()
        {
            // 验证 SimulationProfile 7 字段都被消费(非空 profile 生成非空路径)
            var gen = new MockScanPathGenerator();
            var profile = new SimulationProfile
            {
                FloorShape = new SimPoint3 { X = 1, Y = 1, Z = 1 },
                FloorOffsetPosi = new SimPoint3 { X = 1, Y = 1, Z = 1 },
                YShape = new SimPoint3 { X = 1, Y = 1, Z = 1 },
                YOffsetPosi = new SimPoint3 { X = 1, Y = 1, Z = 1 },
                AShape = new SimPoint3 { X = 1, Y = 1, Z = 1 },
                AOffsetPosi = new SimPoint3 { X = 1, Y = 1, Z = 1 },
                ARotateCenter = new SimPoint3 { X = 1, Y = 1, Z = 1 }
            };
            var path = gen.GenerateScanPath(profile, 5);
            Assert.Equal(15, path.Count);
            // 点位应非全零(消费了几何)
            Assert.NotEqual(0, path[0].X);
        }
    }
}
