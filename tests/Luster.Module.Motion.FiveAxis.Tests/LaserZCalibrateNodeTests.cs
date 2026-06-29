using FluentAssertions;
using Luster.Module.Motion.FiveAxis.Functions;
using Luster.Motion.DataStruct.Real;
using Luster.Motion.FiveAxis.Data.Calibration;
using Luster.Motion.FiveAxis.Position;
using NUnit.Framework;

namespace Luster.Module.Motion.FiveAxis.Tests
{
    /// <summary>
    /// P5-4 激光 Z 单点标定流程验收(TES-99)。
    /// 验收点:LaserZCalibrateNode 两点 (激光读数, Z 高度) → FiveAxisCalibrationService.LaserCalibrate
    /// → LinearConverter 激光读数↔Z 高度往返一致(与源端 Form5Cali.laserCaliApply 语义对齐)。
    /// 纯 C#,不依赖真机/渲染,软件层可验。
    /// </summary>
    [TestFixture]
    public class LaserZCalibrateNodeTests
    {
        /// <summary>
        /// 两点定标:激光读数 10→Z=1.0,激光读数 20→Z=2.0(线性 1:0.1,k=0.1,b=0)。
        /// 断言 k/b 正确,且 LaserMap 往返一致(DirectValueToUnit(laser) == z)。
        /// </summary>
        [Test]
        [Category("Regression")]
        [Category("SourceAlignment")]
        public void DoExcute_TwoPoints_ProducesCorrectLinearMap()
        {
            var node = new LaserZCalibrateNode
            {
                Laser1 = 10, Z1 = 1.0,
                Laser2 = 20, Z2 = 2.0,
                LaserStandard = 1.5,
                LaserPosiX = 100, LaserPosiY = 50, LaserPosiZ = 1,
                CameraPosiX = 101, CameraPosiY = 50, CameraPosiZ = 1,
            };

            bool ok = node.DoExcute(out var errMsg);

            ok.Should().BeTrue(errMsg);
            errMsg.Should().BeEmpty();

            // k=(z2-z1)/(laser2-laser1)=1/10=0.1,b=z1-k*laser1=1-0.1*10=0
            node.CalibratedK.Should().BeApproximately(0.1, 1e-9);
            node.CalibratedB.Should().BeApproximately(0.0, 1e-9);

            // LaserMap 往返一致:激光读数 → Z 高度(源端 sampleHandle 用此换算)
            var map = node.CalibratedResult.LaserMap;
            map.DirectValueToUnit(10).Should().BeApproximately(1.0, 1e-9);
            map.DirectValueToUnit(20).Should().BeApproximately(2.0, 1e-9);
            map.DirectValueToUnit(15).Should().BeApproximately(1.5, 1e-9);  // 中点插值

            // 标准值 + 示教位置回填
            node.CalibratedResult.LaserStandard.Should().Be(1.5);
            node.CalibratedResult.LaserPosi.X.Should().Be(100);
            node.CalibratedResult.CameraPosi.X.Should().Be(101);
        }

        /// <summary>
        /// 负斜率场景:激光读数增大 → Z 减小(常见于激光器朝下测距)。
        /// 断言 k 为负且往返一致。
        /// </summary>
        [Test]
        [Category("Regression")]
        [Category("SourceAlignment")]
        public void DoExcute_NegativeSlope_RoundTripsCorrectly()
        {
            var node = new LaserZCalibrateNode
            {
                Laser1 = 100, Z1 = 5.0,
                Laser2 = 200, Z2 = -5.0,
            };

            node.DoExcute(out _).Should().BeTrue();

            // k=(-5-5)/(200-100) = -0.1
            node.CalibratedK.Should().BeApproximately(-0.1, 1e-9);
            node.CalibratedResult.LaserMap.DirectValueToUnit(100).Should().BeApproximately(5.0, 1e-9);
            node.CalibratedResult.LaserMap.DirectValueToUnit(200).Should().BeApproximately(-5.0, 1e-9);
        }

        /// <summary>
        /// 两点激光读数相同 → 无法定标(LinearConverter 分母为 0),返回失败 + 明确错误。
        /// </summary>
        [Test]
        [Category("Regression")]
        [Category("SourceAlignment")]
        public void DoExcute_IdenticalLaserReadings_Fails()
        {
            var node = new LaserZCalibrateNode
            {
                Laser1 = 10, Z1 = 1.0,
                Laser2 = 10, Z2 = 2.0,  // 相同激光读数,不同 Z(异常工况)
            };

            bool ok = node.DoExcute(out var errMsg);

            ok.Should().BeFalse();
            errMsg.Should().NotBeEmpty();
        }

        /// <summary>
        /// ILaserController 接口契约(P3-A):定义单点测距 GetDistance,区别于线扫 ILineLaser。
        /// </summary>
        [Test]
        [Category("Regression")]
        [Category("SourceAlignment")]
        public void ILaserController_DefinesSinglePointDistanceContract()
        {
            var t = typeof(ILaserController);
            t.Should().NotBeNull();
            t.GetMethod("GetDistance").Should().NotBeNull("ILaserController 必须定义 GetDistance 单点测距方法");
            t.GetProperty("IsConnected").Should().NotBeNull("ILaserController 必须定义 IsConnected 连接状态");
        }
    }
}
