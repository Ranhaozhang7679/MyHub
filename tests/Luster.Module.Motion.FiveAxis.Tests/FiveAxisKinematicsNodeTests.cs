using FluentAssertions;
using Luster.Module.Motion.FiveAxis.Functions;
using Luster.Motion.FiveAxis.Kinematics;
using Luster.Motion.FiveAxis.Position;
using NUnit.Framework;

namespace Luster.Module.Motion.FiveAxis.Tests
{
    /// <summary>
    /// FiveAxisKinematicsNode 节点封装验收(P5-2):
    /// 节点 DoExcute 的输出 TargetX/Y/Z 与直接调 Coord5Axis.PointO2D/PointD2O 一致,
    /// TargetU=RX、TargetV=RZ(3+2 构型旋转轴目标 = 旋转角命令)。
    /// </summary>
    [TestFixture]
    public class FiveAxisKinematicsNodeTests
    {
        private static FiveAxisKinematicsNode CreateNode()
        => new FiveAxisKinematicsNode
        {
            ACenterX = 0.1, ACenterY = 0.2, ACenterZ = 0.3,
            ADirX = 0, ADirY = 0, ADirZ = 1,
            ACirPulses = 360000,
            CCenterX = -0.1, CCenterY = 0, CCenterZ = 0.05,
            CDirX = 0, CDirY = 0, CDirZ = 1,
            CCirPulses = 360000,
        };

        private static Coord5Axis CreateCoord(FiveAxisKinematicsNode n)
        => new Coord5Axis
        {
            ACenter = new PositionXYZ(n.ACenterX, n.ACenterY, n.ACenterZ),
            ADir = new PositionXYZ(n.ADirX, n.ADirY, n.ADirZ),
            ACirPulses = n.ACirPulses,
            CCenter = new PositionXYZ(n.CCenterX, n.CCenterY, n.CCenterZ),
            CDir = new PositionXYZ(n.CDirX, n.CDirY, n.CDirZ),
            CCirPulses = n.CCirPulses,
        };

        [Test]
        [Category("Regression")]
        [Category("SourceAlignment")]
        public void DoExcute_Org2Dest_MatchesCoord5AxisPointO2D()
        {
            var node = CreateNode();
            node.RX = 30; node.RZ = 45;
            node.InputX = 10; node.InputY = 20; node.InputZ = 30;
            node.Mode = FiveAxisKinematicsNode.KinematicsMode.Org2Dest;

            var ok = node.DoExcute(out var errMsg);
            ok.Should().BeTrue();
            errMsg.Should().BeEmpty();

            var expected = CreateCoord(node).PointO2D(node.RX, node.RZ,
                new PositionXYZ(node.InputX, node.InputY, node.InputZ));

            node.TargetX.Should().BeApproximately(expected.X, 1e-9);
            node.TargetY.Should().BeApproximately(expected.Y, 1e-9);
            node.TargetZ.Should().BeApproximately(expected.Z, 1e-9);
            node.TargetU.Should().Be(node.RX);
            node.TargetV.Should().Be(node.RZ);
        }

        [Test]
        [Category("Regression")]
        [Category("SourceAlignment")]
        public void DoExcute_Dest2Org_MatchesCoord5AxisPointD2O()
        {
            var node = CreateNode();
            node.RX = -90; node.RZ = 123.4;
            node.InputX = -50.5; node.InputY = 100.25; node.InputZ = -77.7;
            node.Mode = FiveAxisKinematicsNode.KinematicsMode.Dest2Org;

            node.DoExcute(out _);

            var expected = CreateCoord(node).PointD2O(node.RX, node.RZ,
                new PositionXYZ(node.InputX, node.InputY, node.InputZ));

            node.TargetX.Should().BeApproximately(expected.X, 1e-9);
            node.TargetY.Should().BeApproximately(expected.Y, 1e-9);
            node.TargetZ.Should().BeApproximately(expected.Z, 1e-9);
            node.TargetU.Should().Be(node.RX);
            node.TargetV.Should().Be(node.RZ);
        }
    }
}
