using FluentAssertions;
using Luster.Motion.FiveAxis.Kinematics;
using Luster.Motion.FiveAxis.Position;
using MathNet.Numerics.LinearAlgebra;
using NUnit.Framework;
using System.Linq;

namespace Luster.Module.Motion.FiveAxis.Tests
{
    /// <summary>
    /// 源端对齐 diff(TES-116 验收项):Coord5Axis 正逆解迁移后输出 vs 源端 SP-2025140 期望,误差 ≤1e-6。
    ///
    /// 背景:Coord5Axis 算法本体"路 X 原样迁 MathNet"(矩阵运算字节级一致,见 Coord5Axis.cs 头注 +
    /// 知识库 P5端到端集成范围评估.md)。源端文件 SP-2025140/.../MathNetExtend/Model/Coordinate/Coord5Axis.cs
    /// GetOrg2DestMatrix(:69)/GetDest2OrgMatrix(:97)与迁移端方法体一致,故"运行迁移端 == 运行源端"。
    ///
    /// 本测试用【从源端算法独立推导】的精确期望矩阵(E1-E4,源端默认 ctor 配置)钉死迁移端输出,
    /// 作为源端对齐 golden 值:误差 ≤1e-6(实际精确角度下 ~1e-16,容差吸收 MathNet 版本浮点差)。
    /// 非线性非零角场景由 Coord5AxisTests 的正逆解往返/逆矩阵不变式覆盖(此处不重复)。
    ///
    /// 源端默认 ctor 配置:ACenter=(1,0,0),ADir=(0,0,0),CCenter=(0,0,0),CDir=(0,0,1)。
    /// ADir=(0,0,0) 退化 → xyz2rcp 得 h=p=0 → ra_c=ra_b=I;CDir=(0,0,1) → 旋转轴沿 Z,无重定向;
    /// ACenter=(1,0,0) 位于 X 轴上,X 平移与 Rx 旋转可交换 → 算法坍缩为:
    ///   GetOrg2DestMatrix(rx,rz) = Rz(-radz) · Rx(-radx)
    /// (radx/radz 为度转弧度;getWorld2WorkMatrix 对六参数取负,故旋转角为 -radx/-radz)。
    /// </summary>
    [TestFixture]
    public class Coord5AxisSourceAlignmentTests
    {
        // 源端默认 ctor(与 SP-2025140 Coord5Axis 默认值一致)
        private static Coord5Axis CreateDefault() => new Coord5Axis();

        private static Matrix<double> Expected(params double[] rowMajor4x4)
            => Matrix<double>.Build.DenseOfRowMajor(4, 4, rowMajor4x4);

        private static void AssertMatrix(Matrix<double> actual, Matrix<double> expected, string label)
        {
            var maxAbs = (actual - expected).Enumerate().Max(x => System.Math.Abs(x));
            Assert.That(maxAbs, Is.LessThanOrEqualTo(1e-6),
                $"{label}:迁移端输出应与源端推导期望一致(≤1e-6),实际最大误差={maxAbs}");
        }

        /// <summary>E1:零旋转 → 单位阵(正逆解均为 I)。</summary>
        [Test]
        public void E1_IdentityMatrix_AtZeroAngles()
        {
            var coord = CreateDefault();
            AssertMatrix(coord.GetOrg2DestMatrix(0, 0),
                Expected(1, 0, 0, 0,
                         0, 1, 0, 0,
                         0, 0, 1, 0,
                         0, 0, 0, 1), "E1 Org2Dest(0,0)");
            AssertMatrix(coord.GetDest2OrgMatrix(0, 0),
                Expected(1, 0, 0, 0,
                         0, 1, 0, 0,
                         0, 0, 1, 0,
                         0, 0, 0, 1), "E1 Dest2Org(0,0)");
        }

        /// <summary>E2:仅 A 轴 rx=90° → Rz(0)·Rx(-90°) = Rx(-90°)。
        /// Rx(-90°) = [[1,0,0],[0,0,1],[0,-1,0]](cos(-90)=0,sin(-90)=-1)。</summary>
        [Test]
        public void E2_RxMinus90_AtRx90()
        {
            var coord = CreateDefault();
            AssertMatrix(coord.GetOrg2DestMatrix(90, 0),
                Expected(1, 0, 0, 0,
                         0, 0, 1, 0,
                         0, -1, 0, 0,
                         0, 0, 0, 1), "E2 Org2Dest(90,0)");
            // 逆 = Rx(90°) = [[1,0,0],[0,0,-1],[0,1,0]]
            AssertMatrix(coord.GetDest2OrgMatrix(90, 0),
                Expected(1, 0, 0, 0,
                         0, 0, -1, 0,
                         0, 1, 0, 0,
                         0, 0, 0, 1), "E2 Dest2Org(90,0)");
        }

        /// <summary>E3:仅 C 轴 rz=90° → Rz(-90°)·Rx(0) = Rz(-90°)。
        /// Rz(-90°) = [[0,1,0],[-1,0,0],[0,0,1]]。</summary>
        [Test]
        public void E3_RzMinus90_AtRz90()
        {
            var coord = CreateDefault();
            AssertMatrix(coord.GetOrg2DestMatrix(0, 90),
                Expected(0, 1, 0, 0,
                         -1, 0, 0, 0,
                         0, 0, 1, 0,
                         0, 0, 0, 1), "E3 Org2Dest(0,90)");
            AssertMatrix(coord.GetDest2OrgMatrix(0, 90),
                Expected(0, -1, 0, 0,
                         1, 0, 0, 0,
                         0, 0, 1, 0,
                         0, 0, 0, 1), "E3 Dest2Org(0,90)");
        }

        /// <summary>E4:rx=rz=90° → Rz(-90°)·Rx(-90°) = [[0,0,1],[-1,0,0],[0,-1,0]]。</summary>
        [Test]
        public void E4_ComposedRxRz_AtRx90Rz90()
        {
            var coord = CreateDefault();
            AssertMatrix(coord.GetOrg2DestMatrix(90, 90),
                Expected(0, 0, 1, 0,
                         -1, 0, 0, 0,
                         0, -1, 0, 0,
                         0, 0, 0, 1), "E4 Org2Dest(90,90)");
            // 逆 = Rx(90°)·Rz(90°) = [[0,-1,0],[0,0,-1],[1,0,0]]
            AssertMatrix(coord.GetDest2OrgMatrix(90, 90),
                Expected(0, -1, 0, 0,
                         0, 0, -1, 0,
                         1, 0, 0, 0,
                         0, 0, 0, 1), "E4 Dest2Org(90,90)");
        }

        /// <summary>E5:点级正逆解对齐(默认配置)。PointO2D = M·[o;1]。
        /// E2:PointO2D(90,0,(0,1,0)) = Rx(-90°)·(0,1,0,1) = (0,0,-1)。
        /// E4:PointO2D(90,90,(1,0,0)) = Rz(-90°)·Rx(-90°)·(1,0,0,1) = (0,-1,0)。
        /// </summary>
        [Test]
        public void E5_PointO2D_AlignsWithSourceDerivedValues()
        {
            var coord = CreateDefault();

            var p2 = coord.PointO2D(90, 0, new PositionXYZ(0, 1, 0));
            p2.X.Should().BeApproximately(0, 1e-6);
            p2.Y.Should().BeApproximately(0, 1e-6);
            p2.Z.Should().BeApproximately(-1, 1e-6);

            var p4 = coord.PointO2D(90, 90, new PositionXYZ(1, 0, 0));
            p4.X.Should().BeApproximately(0, 1e-6);
            p4.Y.Should().BeApproximately(-1, 1e-6);
            p4.Z.Should().BeApproximately(0, 1e-6);

            // 逆解还原:PointD2O(PointO2D(p)) ≈ p(源端正逆解回差 ≤0.001mm 硬指标)
            var origin = new PositionXYZ(12.5, -7.3, 33.1);
            var roundTrip = coord.PointD2O(45, 30, coord.PointO2D(45, 30, origin));
            roundTrip.X.Should().BeApproximately(origin.X, 1e-6);
            roundTrip.Y.Should().BeApproximately(origin.Y, 1e-6);
            roundTrip.Z.Should().BeApproximately(origin.Z, 1e-6);
        }
    }
}
