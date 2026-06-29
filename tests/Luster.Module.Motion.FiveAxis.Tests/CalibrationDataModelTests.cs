using FluentAssertions;
using Luster.Motion.FiveAxis.Coordinate;
using Luster.Motion.FiveAxis.Data;
using Luster.Motion.FiveAxis.Data.Calibration;
using Luster.Motion.FiveAxis.Kinematics;
using Luster.Motion.FiveAxis.Position;
using Luster.Motion.FiveAxis.Service;
using NUnit.Framework;

namespace Luster.Module.Motion.FiveAxis.Tests
{
    /// <summary>
    /// P5-5 标定/配置数据模型 IXMLParser 往返 + CalibratedCoord5Axis 包装验收。
    /// 验收点:FiveAxisPara/标定结果数据模型 ExportXml↔ParserXml 往返一致(含 Coord5Axis 6 参数 + Position 点列表 + LinearConverter + CoordTransForm)。
    /// </summary>
    [TestFixture]
    public class CalibrationDataModelTests
    {
        // 构造一个各字段均带可识别非默认值的 FiveAxisCaliProfile(含 Coord5Axis 矩阵结构参数 + 点列表 + 线性映射 + 坐标系转换)
        private static FiveAxisCaliProfile BuildSampleProfile()
        {
            var p = new FiveAxisCaliProfile
            {
                BallSampleSpan = 3.5,
                BallRadius = 15.8705,
                CaliDelay = 250,
                LaserValidOffset = 5.5
            };

            p.LaserCali.LaserId = 2;
            p.LaserCali.LaserStandard = 12.34;
            p.LaserCali.LaserMap.Map1 = new LinearPointMap(1.1, 2.2);
            p.LaserCali.LaserMap.Map2 = new LinearPointMap(3.3, 4.4);
            p.LaserCali.LaserPosi = new PositionXYZ(10, 20, 30);
            p.LaserCali.CameraPosi = new PositionXYZ(11, 21, 31);

            p.RoughCali.FirstPosi = new PositionXYZRxRyRz { X = 1, Y = 2, Z = 3, RX = 5, RY = 0, RZ = 5 };
            p.RoughCali.Rx = 5; p.RoughCali.Rz = 6;
            p.RoughCali.ResultFirstPosi = new PositionXYZ(100.5, 200.25, 300.125);
            p.RoughCali.ResultRxPosi = new PositionXYZ(101.5, 201.25, 301.125);
            p.RoughCali.ResultRzPosi = new PositionXYZ(102.5, 202.25, 302.125);
            p.RoughCali.Rough5Para = new Coord5Axis
            {
                ACenter = new PositionXYZ(0, 1.5, 2.5),
                ADir = new PositionXYZ(1, 0, 0),
                ACirPulses = 360000,
                CCenter = new PositionXYZ(3.5, 4.5, 0),
                CDir = new PositionXYZ(0, 0, 1),
                CCirPulses = 720000,
            };

            p.AccurateCali.FirstPosi = new PositionXYZRxRyRz { X = 9, Y = 8, Z = 7, RX = 1, RY = 2, RZ = 3 };
            p.AccurateCali.RxSpan = 2.5; p.AccurateCali.RxFCount = 3; p.AccurateCali.RxBCount = 2;
            p.AccurateCali.RzSpan = 1.5; p.AccurateCali.RzFCount = 4; p.AccurateCali.RzBCount = 1;
            p.AccurateCali.ZeroRx = 0.123;
            p.AccurateCali.ResultFirstPosi = new PositionXYZRxRyRz { X = 50, Y = 60, Z = 70, RX = 0, RY = 0, RZ = 0 };
            p.AccurateCali.ResultRxPosiLis.Add(new PositionXYZRxRyRz { X = 51, Y = 61, Z = 71, RX = 2.5, RY = 0, RZ = 0 });
            p.AccurateCali.ResultRxPosiLis.Add(new PositionXYZRxRyRz { X = 52, Y = 62, Z = 72, RX = -2.5, RY = 0, RZ = 0 });
            p.AccurateCali.ResultRzPosiLis.Add(new PositionXYZRxRyRz { X = 53, Y = 63, Z = 73, RX = 0, RY = 0, RZ = 1.5 });
            p.AccurateCali.Accurate5Para = new Coord5Axis
            {
                ACenter = new PositionXYZ(0, 1.55, 2.55),
                ADir = new PositionXYZ(1, 0, 0),
                ACirPulses = 360001,
                CCenter = new PositionXYZ(3.55, 4.55, 0),
                CDir = new PositionXYZ(0, 0, 1),
                CCirPulses = 720001,
            };

            p.UniformityCheck.CheckBoardPosi = new PositionXYZRxRyRz { X = 1, Y = 1, Z = 1, RX = 0, RY = 0, RZ = 0 };
            p.UniformityCheck.CheckPoints.Add(new PositionXYZRxRyRz { X = 5, Y = 6, Z = 7, RX = 0, RY = 0, RZ = 1 });

            p.WorkOriginCali.OrgPosiType = TeachWorkOriginResult.OriginPosiType.LongCenter;
            p.WorkOriginCali.OriginPosi = new PositionXYZRxRyRz { X = 0, Y = 0, Z = 0, RX = 0, RY = 0, RZ = 0 };
            p.WorkOriginCali.LongSidePosi = new PositionXYZRxRyRz { X = 100, Y = 0, Z = 0, RX = 0, RY = 0, RZ = 0 };
            p.WorkOriginCali.DiagonalPosi = new PositionXYZRxRyRz { X = 100, Y = 50, Z = 0, RX = 0, RY = 0, RZ = 0 };
            p.WorkOriginCali.RltTool2Work.Trans = new PositionXYZRxRyRz { X = 1.1, Y = 2.2, Z = 3.3, RX = 4.4, RY = 5.5, RZ = 6.6 };

            return p;
        }

        // 深比较两个 Coord5Axis 的 6 个结构参数
        private static void CoordShouldEqual(Coord5Axis a, Coord5Axis b)
        {
            b.ACirPulses.Should().BeApproximately(a.ACirPulses, 1e-9, "ACirPulses");
            b.CCirPulses.Should().BeApproximately(a.CCirPulses, 1e-9, "CCirPulses");
            PositionShouldEqual(a.ACenter, b.ACenter, "ACenter");
            PositionShouldEqual(a.ADir, b.ADir, "ADir");
            PositionShouldEqual(a.CCenter, b.CCenter, "CCenter");
            PositionShouldEqual(a.CDir, b.CDir, "CDir");
        }

        private static void PositionShouldEqual(PositionXYZ a, PositionXYZ b, string label)
        {
            b.X.Should().BeApproximately(a.X, 1e-9, label + ".X");
            b.Y.Should().BeApproximately(a.Y, 1e-9, label + ".Y");
            b.Z.Should().BeApproximately(a.Z, 1e-9, label + ".Z");
        }

        [Test]
        [Category("Regression")]
        [Category("SourceAlignment")]
        public void FiveAxisCaliProfile_RoundTrip_KeepsAllFields()
        {
            var src = BuildSampleProfile();

            var x = src.ExportXml();
            var back = new FiveAxisCaliProfile();
            back.ParserXml(x);

            // 标量
            back.BallSampleSpan.Should().BeApproximately(src.BallSampleSpan, 1e-9);
            back.BallRadius.Should().BeApproximately(src.BallRadius, 1e-9);
            back.CaliDelay.Should().Be(src.CaliDelay);
            back.LaserValidOffset.Should().BeApproximately(src.LaserValidOffset, 1e-9);

            // 激光标定
            back.LaserCali.LaserId.Should().Be(src.LaserCali.LaserId);
            back.LaserCali.LaserStandard.Should().BeApproximately(src.LaserCali.LaserStandard, 1e-9);
            back.LaserCali.LaserMap.Map1.DirectValue.Should().BeApproximately(src.LaserCali.LaserMap.Map1.DirectValue, 1e-9);
            back.LaserCali.LaserMap.Map1.UnitValue.Should().BeApproximately(src.LaserCali.LaserMap.Map1.UnitValue, 1e-9);
            back.LaserCali.LaserMap.Map2.UnitValue.Should().BeApproximately(src.LaserCali.LaserMap.Map2.UnitValue, 1e-9);
            PositionShouldEqual(src.LaserCali.LaserPosi, back.LaserCali.LaserPosi, "LaserPosi");
            PositionShouldEqual(src.LaserCali.CameraPosi, back.LaserCali.CameraPosi, "CameraPosi");

            // 粗标(含 Coord5Axis)
            back.RoughCali.Rx.Should().BeApproximately(src.RoughCali.Rx, 1e-9);
            PositionShouldEqual(src.RoughCali.ResultFirstPosi, back.RoughCali.ResultFirstPosi, "Rough.ResultFirstPosi");
            CoordShouldEqual(src.RoughCali.Rough5Para, back.RoughCali.Rough5Para);

            // 精标(含点列表 + Coord5Axis)
            back.AccurateCali.RxSpan.Should().BeApproximately(src.AccurateCali.RxSpan, 1e-9);
            back.AccurateCali.RxFCount.Should().Be(src.AccurateCali.RxFCount);
            back.AccurateCali.RzBCount.Should().Be(src.AccurateCali.RzBCount);
            back.AccurateCali.ZeroRx.Should().BeApproximately(src.AccurateCali.ZeroRx, 1e-9);
            back.AccurateCali.ResultRxPosiLis.Count.Should().Be(src.AccurateCali.ResultRxPosiLis.Count, "RxLis count");
            back.AccurateCali.ResultRzPosiLis.Count.Should().Be(src.AccurateCali.ResultRzPosiLis.Count, "RzLis count");
            PositionShouldEqual(src.AccurateCali.ResultRxPosiLis[1], back.AccurateCali.ResultRxPosiLis[1], "RxLis[1]");
            CoordShouldEqual(src.AccurateCali.Accurate5Para, back.AccurateCali.Accurate5Para);

            // 一致性点检(含点列表)
            back.UniformityCheck.CheckPoints.Count.Should().Be(src.UniformityCheck.CheckPoints.Count);
            PositionShouldEqual(src.UniformityCheck.CheckPoints[0], back.UniformityCheck.CheckPoints[0], "Uniformity[0]");

            // 工件原点(含 CoordTransForm + 枚举)
            back.WorkOriginCali.OrgPosiType.Should().Be(src.WorkOriginCali.OrgPosiType);
            back.WorkOriginCali.RltTool2Work.Trans.X.Should().BeApproximately(src.WorkOriginCali.RltTool2Work.Trans.X, 1e-9);
            back.WorkOriginCali.RltTool2Work.Trans.RZ.Should().BeApproximately(src.WorkOriginCali.RltTool2Work.Trans.RZ, 1e-9);

            // 往返稳定性:再导出一次,XML 文本应一致(属性/元素顺序确定)
            var x2 = back.ExportXml();
            x2.ToString().Should().Be(x.ToString(), "round-trip XML stability");
        }

        [Test]
        [Category("Regression")]
        [Category("SourceAlignment")]
        public void FiveAxisPara_RoundTrip_MatchesCoord5AxisFields()
        {
            var src = new FiveAxisPara
            {
                ACenter = new PositionXYZ(0.1, 0.2, 0.3),
                ADir = new PositionXYZ(0, 0, 1),
                ACirPulses = 360000,
                CCenter = new PositionXYZ(-0.1, 0, 0.05),
                CDir = new PositionXYZ(0, 0, 1),
                CCirPulses = 360000,
            };

            var x = src.ExportXml();
            var back = new FiveAxisPara();
            back.ParserXml(x);

            CoordShouldEqual(src, back);
            // FiveAxisPara 仍是 Coord5Axis,正逆解可用
            var m = back.GetOrg2DestMatrix(30, 45);
            m.RowCount.Should().Be(4);
            m.ColumnCount.Should().Be(4);
        }

        [Test]
        [Category("Regression")]
        [Category("SourceAlignment")]
        public void CoordTransForm_RoundTrip_AndKinematicsConsistency()
        {
            var src = new CoordTransForm
            {
                Trans = new PositionXYZRxRyRz { X = 10, Y = 20, Z = 30, RX = 5, RY = 0, RZ = 15 }
            };

            var x = src.ExportXml();
            var back = new CoordTransForm();
            back.ParserXml(x);

            back.Trans.X.Should().BeApproximately(src.Trans.X, 1e-9);
            back.Trans.RZ.Should().BeApproximately(src.Trans.RZ, 1e-9);

            // PoseD2O ∘ PoseO2D 往返还原(平移+旋转可逆)
            var pose = new PositionXYZRxRyRz { X = 1, Y = 2, Z = 3, RX = 10, RY = 20, RZ = 30 };
            var round = back.PoseD2O(back.PoseO2D(pose));
            round.X.Should().BeApproximately(pose.X, 1e-6, "Pose round X");
            round.Y.Should().BeApproximately(pose.Y, 1e-6, "Pose round Y");
            round.Z.Should().BeApproximately(pose.Z, 1e-6, "Pose round Z");
            round.RX.Should().BeApproximately(pose.RX, 1e-6, "Pose round RX");
            round.RZ.Should().BeApproximately(pose.RZ, 1e-6, "Pose round RZ");
        }

        [Test]
        [Category("Regression")]
        [Category("SourceAlignment")]
        public void CalibratedCoord5Axis_EffectiveParaIsTransFormedAndDelegates()
        {
            var acc = new Coord5Axis
            {
                ACenter = new PositionXYZ(0.1, 0.2, 0.3),
                ADir = new PositionXYZ(0, 0, 1),
                ACirPulses = 360000,
                CCenter = new PositionXYZ(-0.1, 0, 0.05),
                CDir = new PositionXYZ(0, 0, 1),
                CCirPulses = 360000,
            };
            var offset = new PositionXYZ(1, 2, 3);

            var calib = new CalibratedCoord5Axis(acc, offset);

            // EffectivePara = Accurate5Para.TransForm(CameraOffset)(源端 laserCaliApply 语义)
            var expected = acc.TransForm(offset);
            CoordShouldEqual(expected, calib.EffectivePara);

            // 委托正逆解:PointD2O(PointO2D(p)) 往返还原
            var p = new PositionXYZ(5, -6, 7.5);
            var round = calib.PointD2O(30, 45, calib.PointO2D(30, 45, p));
            PositionShouldEqual(p, round, "delegate round");
        }

        [Test]
        [Category("Regression")]
        [Category("SourceAlignment")]
        public void AccurateCaliResult_GetRxPosiLis_ReturnsExpectedCount()
        {
            var acc = new AccurateCaliResult
            {
                RxSpan = 2.5, RxFCount = 3, RxBCount = 2,
                ResultFirstPosi = new PositionXYZRxRyRz { X = 0, Y = 0, Z = 0, RX = 0, RY = 0, RZ = 0 },
            };
            var rough = new Coord5Axis
            {
                ACenter = new PositionXYZ(0, 0, 0),
                ADir = new PositionXYZ(1, 0, 0),
                CCenter = new PositionXYZ(0, 0, 0),
                CDir = new PositionXYZ(0, 0, 1),
            };

            var lis = acc.GetRxPosiLis(rough, 15.0);
            // RxBCount + RxFCount
            lis.Count.Should().Be(2 + 3);
        }

        [Test]
        [Category("Regression")]
        [Category("SourceAlignment")]
        public void LinearConverter_Convert_RoundTripMath()
        {
            var conv = new LinearConverter();
            conv.Map1 = new LinearPointMap(0, 0);
            conv.Map2 = new LinearPointMap(10, 100); // y = 10x

            // 直接值→当量→直接值 往返
            double direct = 7.5;
            double unit = conv.DirectValueToUnit(direct);
            conv.UnitToDirectValue(unit).Should().BeApproximately(direct, 1e-6);

            // 序列化往返后线性关系不变
            var x = conv.ExportXml();
            var back = new LinearConverter();
            back.ParserXml(x);
            back.DirectValueToUnit(direct).Should().BeApproximately(unit, 1e-9);
        }
    }
}
