using FluentAssertions;
using Luster.Module.Motion.Device.Functions;
using Luster.Motion.FiveAxis.Kinematics;
using Luster.Motion.FiveAxis.Position;
using NUnit.Framework;

namespace Luster.Module.Motion.Device.Tests
{
    /// <summary>
    /// TrajectoryListTransformNode 节点封装验收(P2-E,TES-190):
    /// 节点 DoExcute 的 OutputPoints 逐点与直接调 Coord5Axis.PoseO2D/PoseD2O/PoseTool2Work/PoseWork2Tool 一致;
    /// 空输入返回 true 且输出为空;Tool2Work 模式旋转角取反。
    /// 范式对齐 FiveAxisKinematicsNodeTests(对象初始化器喂参 -> DoExcute -> 算法本体直出对拍)。
    /// </summary>
    [TestFixture]
    public class TrajectoryListTransformNodeTests
    {
        private static TrajectoryListTransformNode CreateNode()
        => new TrajectoryListTransformNode
        {
            ACenterX = 0.1, ACenterY = 0.2, ACenterZ = 0.3,
            ADirX = 0, ADirY = 0, ADirZ = 1,
            ACirPulses = 360000,
            CCenterX = -0.1, CCenterY = 0, CCenterZ = 0.05,
            CDirX = 0, CDirY = 0, CDirZ = 1,
            CCirPulses = 360000,
        };

        private static Coord5Axis CreateCoord(TrajectoryListTransformNode n)
        => new Coord5Axis
        {
            ACenter = new PositionXYZ(n.ACenterX, n.ACenterY, n.ACenterZ),
            ADir = new PositionXYZ(n.ADirX, n.ADirY, n.ADirZ),
            ACirPulses = n.ACirPulses,
            CCenter = new PositionXYZ(n.CCenterX, n.CCenterY, n.CCenterZ),
            CDir = new PositionXYZ(n.CDirX, n.CDirY, n.CDirZ),
            CCirPulses = n.CCirPulses,
        };

        private static TrajectoryPointList MakeInputs() => new TrajectoryPointList
        {
            new PositionXYZRxRyRz { X = 10,  Y = 20,  Z = 30,  RX = 30,  RY = 0,  RZ = 45 },
            new PositionXYZRxRyRz { X = -5,  Y = 100, Z = -7,  RX = -90, RY = 10, RZ = 123.4 },
            new PositionXYZRxRyRz { X = 0,   Y = 0,   Z = 0,   RX = 0,   RY = 0,  RZ = 0 },
        };

        [Test]
        public void DoExcute_Org2Dest_MatchesCoord5Axis_PoseO2D_PerPoint()
        {
            var node = CreateNode();
            node.InputPoints = MakeInputs();
            node.Mode = TrajectoryListTransformNode.TransformMode.Org2Dest;

            var ok = node.DoExcute(out var errMsg);
            ok.Should().BeTrue();
            errMsg.Should().BeEmpty();

            var coord = CreateCoord(node);
            node.OutputPoints.Count.Should().Be(node.InputPoints.Count);

            for (int i = 0; i < node.InputPoints.Count; i++)
            {
                var expected = coord.PoseO2D(node.InputPoints[i]);
                var actual = node.OutputPoints[i];
                actual.X.Should().BeApproximately(expected.X, 1e-9, $"point[{i}].X");
                actual.Y.Should().BeApproximately(expected.Y, 1e-9, $"point[{i}].Y");
                actual.Z.Should().BeApproximately(expected.Z, 1e-9, $"point[{i}].Z");
                actual.RX.Should().BeApproximately(expected.RX, 1e-9, $"point[{i}].RX");
                actual.RY.Should().BeApproximately(expected.RY, 1e-9, $"point[{i}].RY");
                actual.RZ.Should().BeApproximately(expected.RZ, 1e-9, $"point[{i}].RZ");
            }
        }

        [Test]
        public void DoExcute_Dest2Org_MatchesCoord5Axis_PoseD2O_PerPoint()
        {
            var node = CreateNode();
            node.InputPoints = MakeInputs();
            node.Mode = TrajectoryListTransformNode.TransformMode.Dest2Org;

            node.DoExcute(out _).Should().BeTrue();

            var coord = CreateCoord(node);
            node.OutputPoints.Count.Should().Be(node.InputPoints.Count);

            for (int i = 0; i < node.InputPoints.Count; i++)
            {
                var expected = coord.PoseD2O(node.InputPoints[i]);
                var actual = node.OutputPoints[i];
                actual.X.Should().BeApproximately(expected.X, 1e-9);
                actual.Y.Should().BeApproximately(expected.Y, 1e-9);
                actual.Z.Should().BeApproximately(expected.Z, 1e-9);
            }
        }

        [Test]
        public void DoExcute_EmptyInput_ReturnsTrue_EmptyOutput()
        {
            var node = CreateNode();
            node.InputPoints = new TrajectoryPointList();
            node.Mode = TrajectoryListTransformNode.TransformMode.Org2Dest;

            var ok = node.DoExcute(out var errMsg);
            ok.Should().BeTrue();
            errMsg.Should().BeEmpty();
            node.OutputPoints.Should().NotBeNull();
            node.OutputPoints.Should().BeEmpty();
        }

        [Test]
        public void DoExcute_Tool2Work_NegatesRotationAngles()
        {
            var node = CreateNode();
            node.InputPoints = MakeInputs();
            node.Mode = TrajectoryListTransformNode.TransformMode.Tool2Work;

            node.DoExcute(out _).Should().BeTrue();

            var coord = CreateCoord(node);
            node.OutputPoints.Count.Should().Be(node.InputPoints.Count);

            for (int i = 0; i < node.InputPoints.Count; i++)
            {
                var expected = coord.PoseTool2Work(node.InputPoints[i]);
                var actual = node.OutputPoints[i];
                // PoseTool2Work 仅翻转 RX/RY/RZ 符号,X/Y/Z 不变
                actual.RX.Should().BeApproximately(expected.RX, 1e-9);
                actual.RY.Should().BeApproximately(expected.RY, 1e-9);
                actual.RZ.Should().BeApproximately(expected.RZ, 1e-9);
                actual.RX.Should().BeApproximately(-node.InputPoints[i].RX, 1e-9);
                actual.RY.Should().BeApproximately(-node.InputPoints[i].RY, 1e-9);
                actual.RZ.Should().BeApproximately(-node.InputPoints[i].RZ, 1e-9);
            }
        }
    }
}
