using FluentAssertions;
using Luster.Motion.DataStruct.Real;
using Luster.Motion.FiveAxis.Data.Calibration;
using Luster.Motion.FiveAxis.Kinematics;
using Luster.Motion.FiveAxis.Position;
using Luster.Motion.FiveAxis.Service;
using Luster.Motion.FiveAxis.Utils;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Luster.Module.Motion.FiveAxis.Tests
{
    /// <summary>
    /// P5-5b 五轴标定 Service 算法验收(TES-111)。
    /// 验收点:粗标/激光/原点三阶段纯 C# 算法 —— 输入示教点/采点 → 输出参数,断言关键值与源端 Form5Cali 语义一致;
    /// 精标(AccurateCalibrate)阻塞于卡端 FrameCal ADR,断言抛 NotSupportedException(不编造卡端算法)。
    /// </summary>
    [TestFixture]
    public class FiveAxisCalibrationServiceTests
    {
        private FiveAxisCalibrationService _service;

        [SetUp]
        public void SetUp()
        {
            _service = new FiveAxisCalibrationService();
        }

        #region 粗标 RoughCalibrate

        /// <summary>
        /// 粗标已知旋转中心:A 轴(YZ 平面)绕中心 (Y=10, Z=5) 旋转 90°,
        /// (30,5)→(10,25),断言 ACenter = (0, 10, 5)。验证 AngleHelper.CalculateRoateCenter 端到端几何正确。
        /// </summary>
        [Test]
        public void RoughCalibrate_KnownCenterA_RecoversCenter()
        {
            var rough = new RoughCaliResult
            {
                FirstPosi = new PositionXYZRxRyRz { X = 100, Y = 0, Z = 0, RX = 0, RY = 0, RZ = 0 },
                Rx = 90,
                Rz = 0,
                ResultFirstPosi = new PositionXYZ(100, 30, 5),   // A 轴取 (Y,Z)=(30,5)
                ResultRxPosi = new PositionXYZ(0, 10, 25),        // 旋转 90° 后 (Y,Z)=(10,25)
                ResultRzPosi = new PositionXYZ(0, 0, 0),
            };
            rough.GeneratePosi(); // 填 RxPosi.RX = FirstPosi.RX + Rx = 90

            bool ok = _service.RoughCalibrate(rough, mrxPulses: 360000, mrzPulses: 720000);

            ok.Should().BeTrue();
            // A 轴方向沿 X,中心 X=0,(Y,Z)=旋转中心(10,5)
            rough.Rough5Para.ACenter.X.Should().Be(0);
            rough.Rough5Para.ACenter.Y.Should().BeApproximately(10, 1e-9);
            rough.Rough5Para.ACenter.Z.Should().BeApproximately(5, 1e-9);
            rough.Rough5Para.ADir.X.Should().Be(1);
            rough.Rough5Para.ADir.Y.Should().Be(0);
            rough.Rough5Para.ADir.Z.Should().Be(0);
            rough.Rough5Para.ACirPulses.Should().Be(360000);
            rough.Rough5Para.CCirPulses.Should().Be(720000);
        }

        /// <summary>
        /// 粗标布线验收:A/C 轴中心分别等于 AngleHelper.CalculateRoateCenter 直接计算值,
        /// ADir=(1,0,0)/CDir=(0,0,1)/ACenter.X=0/CCenter.Z=0 结构不变,CirPulses 正确赋值。
        /// </summary>
        [Test]
        public void RoughCalibrate_Wiring_MatchesCalculateRoateCenter()
        {
            var rough = new RoughCaliResult
            {
                FirstPosi = new PositionXYZRxRyRz { X = 1, Y = 2, Z = 3, RX = 10, RY = 0, RZ = 20 },
                Rx = 45,
                Rz = 30,
                ResultFirstPosi = new PositionXYZ(100, 200, 300),
                ResultRxPosi = new PositionXYZ(110, 210, 310),
                ResultRzPosi = new PositionXYZ(120, 220, 320),
            };
            rough.GeneratePosi(); // RxPosi.RX = 10+45=55, RzPosi.RZ = 20+30=50

            _service.RoughCalibrate(rough, mrxPulses: 3600, mrzPulses: 7200);

            // 期望中心:与源端 btnRoughCalculate 取分量完全一致
            var expCenterA = AngleHelper.CalculateRoateCenter(
                new PositionXYRz { X = rough.ResultFirstPosi.Y, Y = rough.ResultFirstPosi.Z, RZ = rough.FirstPosi.RX },
                new PositionXYRz { X = rough.ResultRxPosi.Y, Y = rough.ResultRxPosi.Z, RZ = rough.RxPosi.RX });
            var expCenterC = AngleHelper.CalculateRoateCenter(
                new PositionXYRz { X = rough.ResultFirstPosi.X, Y = rough.ResultFirstPosi.Y, RZ = rough.FirstPosi.RZ },
                new PositionXYRz { X = rough.ResultRzPosi.X, Y = rough.ResultRzPosi.Y, RZ = rough.RzPosi.RZ });

            rough.Rough5Para.ACenter.Should().BeEquivalentTo(new PositionXYZ(0, expCenterA.X, expCenterA.Y));
            rough.Rough5Para.ADir.Should().BeEquivalentTo(new PositionXYZ(1, 0, 0));
            rough.Rough5Para.CCenter.Should().BeEquivalentTo(new PositionXYZ(expCenterC.X, expCenterC.Y, 0));
            rough.Rough5Para.CDir.Should().BeEquivalentTo(new PositionXYZ(0, 0, 1));
            rough.Rough5Para.ACirPulses.Should().Be(3600);
            rough.Rough5Para.CCirPulses.Should().Be(7200);
        }

        [Test]
        public void RoughCalibrate_NullArg_Throws()
        {
            Action act = () => _service.RoughCalibrate(null, 1, 1);
            act.Should().Throw<ArgumentNullException>();
        }

        #endregion

        #region 激光标定 LaserCalibrate

        /// <summary>
        /// 激光标定:两点激光读数+Z 高度填 LinearConverter,标准值/示教位置落位,
        /// LinearConverter 双向换算还原采点(直接值↔当量)。
        /// </summary>
        [Test]
        public void LaserCalibrate_FillsMapAndPositions()
        {
            var laser = new LaserCaliResult();
            var laserPosi = new PositionXYZ(10, 20, 30);
            var cameraPosi = new PositionXYZ(12, 22, 31);

            bool ok = _service.LaserCalibrate(laser,
                laser1: 1.0, z1: 100.0,
                laser2: 5.0, z2: 500.0,
                laserStandard: 12.34,
                laserPosi: laserPosi,
                cameraPosi: cameraPosi);

            ok.Should().BeTrue();
            laser.LaserStandard.Should().Be(12.34);
            laser.LaserMap.Map1.DirectValue.Should().Be(1.0);
            laser.LaserMap.Map1.UnitValue.Should().Be(100.0);
            laser.LaserMap.Map2.DirectValue.Should().Be(5.0);
            laser.LaserMap.Map2.UnitValue.Should().Be(500.0);
            laser.LaserPosi.Should().BeEquivalentTo(laserPosi);
            laser.CameraPosi.Should().BeEquivalentTo(cameraPosi);

            // LinearConverter: y = kx + b, k=(500-100)/(5-1)=100, b=100-100*1=0
            laser.LaserMap.DirectValueToUnit(1.0).Should().BeApproximately(100.0, 1e-9);
            laser.LaserMap.DirectValueToUnit(5.0).Should().BeApproximately(500.0, 1e-9);
            laser.LaserMap.UnitToDirectValue(300.0).Should().BeApproximately(3.0, 1e-9);
        }

        /// <summary>
        /// CameraOffset = CameraPosi - LaserPosi,由下游 CalibratedCoord5Axis 派生(源端 laserCaliApply 语义)。
        /// </summary>
        [Test]
        public void LaserCalibrate_CameraOffsetDerivableByCalibratedCoord5Axis()
        {
            var laser = new LaserCaliResult();
            var laserPosi = new PositionXYZ(10, 20, 30);
            var cameraPosi = new PositionXYZ(12, 22, 31);

            _service.LaserCalibrate(laser, 1, 1, 2, 2, 0, laserPosi, cameraPosi);

            var cameraOffset = (laser.CameraPosi - laser.LaserPosi) as PositionXYZ;
            var accurate5Para = new Coord5Axis();
            var calibrated = new CalibratedCoord5Axis(accurate5Para, cameraOffset);

            calibrated.CameraOffset.Should().BeEquivalentTo(new PositionXYZ(2, 2, 1));
        }

        [Test]
        public void LaserCalibrate_NullArg_Throws()
        {
            Action act = () => _service.LaserCalibrate(null, 0, 0, 0, 0, 0, new PositionXYZ(), new PositionXYZ());
            act.Should().Throw<ArgumentNullException>();
        }

        #endregion

        #region 工件原点示教 CalibrateWorkOrigin

        /// <summary>
        /// 原点示教(OriginPosiType.OriginPosi):Trans = (OriginPosi.X/Y/Z, atan2(长边向量))。
        /// 对应源端 btnWorkCalculateFromTeach(Form5Cali.cs:1757-1774)。
        /// </summary>
        [Test]
        public void CalibrateWorkOrigin_OriginPosi_WritesTrans()
        {
            var origin = new TeachWorkOriginResult
            {
                OrgPosiType = TeachWorkOriginResult.OriginPosiType.OriginPosi,
                OriginPosi = new PositionXYZRxRyRz { X = 1, Y = 2, Z = 3, RX = 0, RY = 0, RZ = 0 },
                LongSidePosi = new PositionXYZRxRyRz { X = 4, Y = 6, Z = 9, RX = 0, RY = 0, RZ = 0 },
            };

            bool ok = _service.CalibrateWorkOrigin(origin);

            ok.Should().BeTrue();
            origin.RltTool2Work.Trans.X.Should().Be(1);
            origin.RltTool2Work.Trans.Y.Should().Be(2);
            origin.RltTool2Work.Trans.Z.Should().Be(3);
            // atan2(6-2, 4-1) = atan2(4, 3)
            origin.RltTool2Work.Trans.RZ.Should().BeApproximately(Math.Atan2(4, 3), 1e-12);
        }

        /// <summary>
        /// 原点示教(对角线中心):Trans 取 Origin/Diagonal 中点 + 长边方向角。
        /// </summary>
        [Test]
        public void CalibrateWorkOrigin_DiagCenter_WritesMidpoint()
        {
            var origin = new TeachWorkOriginResult
            {
                OrgPosiType = TeachWorkOriginResult.OriginPosiType.DiagCenter,
                OriginPosi = new PositionXYZRxRyRz { X = 0, Y = 0, Z = 0, RX = 0, RY = 0, RZ = 0 },
                LongSidePosi = new PositionXYZRxRyRz { X = 10, Y = 0, Z = 0, RX = 0, RY = 0, RZ = 0 },
                DiagonalPosi = new PositionXYZRxRyRz { X = 20, Y = 40, Z = 60, RX = 0, RY = 0, RZ = 0 },
            };

            _service.CalibrateWorkOrigin(origin);

            origin.RltTool2Work.Trans.X.Should().Be(10);
            origin.RltTool2Work.Trans.Y.Should().Be(20);
            origin.RltTool2Work.Trans.Z.Should().Be(30);
            origin.RltTool2Work.Trans.RZ.Should().BeApproximately(Math.Atan2(0, 10), 1e-12);
        }

        [Test]
        public void CalibrateWorkOrigin_NullArg_Throws()
        {
            Action act = () => _service.CalibrateWorkOrigin(null);
            act.Should().Throw<ArgumentNullException>();
        }

        #endregion

        #region 精标 AccurateCalibrate(ADR-TES-110 卡端 FrameCal 编排)

        /// <summary>
        /// 精标编排:成功路径严格按 ExitFrame→Frame→FrameCal→ExitFrame(必退)调用,回填 Accurate5Para + ZeroRx + CirPulses。
        /// 用 RecordingFrame(实现 IFiveAxisFrame)记录调用顺序,断言 R-F2 try/finally 保证最终 ExitFrame。
        /// </summary>
        [Test]
        public void AccurateCalibrate_Success_CallsLifecycleInOrder_AndFillsResult()
        {
            var accurate = new AccurateCaliResult();
            accurate.ResultFirstPosi = new PositionXYZRxRyRz { X = 1, Y = 2, Z = 3, RX = 0.1, RY = 0, RZ = 0.2 };
            accurate.ResultRxPosiLis.Add(new PositionXYZRxRyRz { X = 4, Y = 5, Z = 6, RX = 1, RY = 0, RZ = 0 });
            accurate.ResultRzPosiLis.Add(new PositionXYZRxRyRz { X = 7, Y = 8, Z = 9, RX = 0, RY = 0, RZ = 1 });

            var rough5Para = new Coord5Axis
            {
                ACenter = new PositionXYZ(0, 10, 5),
                ADir = new PositionXYZ(1, 0, 0),
                CCenter = new PositionXYZ(3, 4, 0),
                CDir = new PositionXYZ(0, 0, 1),
            };
            var frame = new RecordingFrame(success: true);
            var ctx = new AccurateCalibrateContext
            {
                Frame = frame,
                CrdIndex = 1,
                RealAxisList = new List<int> { 1, 2, 3, 4, 5 },
                VirAxisList = new List<int> { 101, 102, 103, 104, 105 },
                Rough5Para = rough5Para,
                MrxPulses = 360000,
                MrzPulses = 720000,
            };

            bool ok = _service.AccurateCalibrate(accurate, ctx);

            ok.Should().BeTrue();
            // 严格顺序:ExitFrame(清残留) → Frame(粗标) → FrameCal → ExitFrame(必退)
            frame.Calls.Should().Equal("ExitFrame", "Frame", "FrameCal", "ExitFrame");
            // FrameCal 实轴取前 3 轴(源端 Take(3))
            frame.LastFrameCalRealAxes.Should().Equal(1, 2, 3);
            // 采样点 = FirstPosi + RxLis + RzLis,共 3 点,每点 5 轴脉冲
            frame.LastFrameCalAxisPosi.Should().HaveCount(3);
            frame.LastFrameCalAxisPosi[0].Should().Equal(1.0, 2.0, 3.0, 0.1, 0.2); // To5AxisLis {X,Y,Z,RX,RZ}
            // 回填:ZeroRx + Accurate5Para + CirPulses
            accurate.ZeroRx.Should().Be(RecordingFrame.StubAZero);
            accurate.Accurate5Para.ACirPulses.Should().Be(360000);
            accurate.Accurate5Para.CCirPulses.Should().Be(720000);
            accurate.Accurate5Para.ACenter.X.Should().Be(RecordingFrame.StubPara.ACenterX);
        }

        /// <summary>
        /// R-F2 缓解验证:FrameCal 失败时,finally 仍保证 ExitFrame 调用(必退),返回 false,不回填结果。
        /// </summary>
        [Test]
        public void AccurateCalibrate_FrameCalFails_StillExitFrameInFinally_ReturnsFalse()
        {
            var accurate = new AccurateCaliResult();
            accurate.ResultFirstPosi = new PositionXYZRxRyRz { X = 1, Y = 2, Z = 3 };
            var frame = new RecordingFrame(success: false); // FrameCal 返回 false
            var ctx = new AccurateCalibrateContext
            {
                Frame = frame,
                CrdIndex = 1,
                RealAxisList = new List<int> { 1, 2, 3, 4, 5 },
                VirAxisList = new List<int> { 101, 102, 103, 104, 105 },
                Rough5Para = new Coord5Axis(),
            };

            bool ok = _service.AccurateCalibrate(accurate, ctx);

            ok.Should().BeFalse();
            // 即使 FrameCal 失败,也必须 ExitFrame→Frame→FrameCal(失败)→ExitFrame(必退)
            frame.Calls.Should().Equal("ExitFrame", "Frame", "FrameCal", "ExitFrame");
            accurate.Accurate5Para.ACirPulses.Should().Be(0); // 未回填
        }

        /// <summary>
        /// 非五轴卡(ctx.Frame 为 null)优雅退出返回 false,不抛异常(ADR 骨架 motionCard is not IFiveAxisFrame return false)。
        /// </summary>
        [Test]
        public void AccurateCalibrate_NullFrame_ReturnsFalseGracefully()
        {
            var accurate = new AccurateCaliResult();
            var ctx = new AccurateCalibrateContext { Frame = null };

            bool ok = _service.AccurateCalibrate(accurate, ctx);

            ok.Should().BeFalse();
        }

        /// <summary>记录 IFiveAxisFrame 调用顺序的测试替身,成功/失败可配。</summary>
        private class RecordingFrame : IFiveAxisFrame
        {
            public const double StubAZero = 1.234;
            public static readonly FiveAxisFramePara StubPara = new FiveAxisFramePara
            {
                ACenterX = 0, ACenterY = 10, ACenterZ = 5,
                ADirX = 1, ADirY = 0, ADirZ = 0,
                CCenterX = 3, CCenterY = 4, CCenterZ = 0,
                CDirX = 0, CDirY = 0, CDirZ = 1,
            };

            private readonly bool _success;
            public readonly List<string> Calls = new List<string>();
            public List<int> LastFrameCalRealAxes;
            public List<double[]> LastFrameCalAxisPosi;

            public RecordingFrame(bool success) { _success = success; }

            public bool ConfigureFrameTableAddr(int crdIndex, FiveAxisFrameTableAddr addr) { Calls.Add("ConfigureFrameTableAddr"); return true; }
            public bool Frame(int crdIndex, IReadOnlyList<int> realAxisList, IReadOnlyList<int> virAxisList, FiveAxisFramePara para) { Calls.Add("Frame"); return true; }
            public bool Reframe(int crdIndex, IReadOnlyList<int> realAxisList, IReadOnlyList<int> virAxisList, FiveAxisFramePara para) { throw new NotSupportedException(); }
            public bool ExitFrame(IReadOnlyList<int> realAxisList, IReadOnlyList<int> virAxisList) { Calls.Add("ExitFrame"); return true; }
            public bool FrameCal(int crdIndex, IReadOnlyList<int> realAxisList, IReadOnlyList<double[]> axisPosi, out double aZero, out FiveAxisFramePara para)
            {
                Calls.Add("FrameCal");
                LastFrameCalRealAxes = new List<int>(realAxisList);
                LastFrameCalAxisPosi = new List<double[]>(axisPosi);
                aZero = StubAZero;
                para = new FiveAxisFramePara();
                para.ACenterX = StubPara.ACenterX; para.ACenterY = StubPara.ACenterY; para.ACenterZ = StubPara.ACenterZ;
                para.ADirX = StubPara.ADirX; para.ADirY = StubPara.ADirY; para.ADirZ = StubPara.ADirZ;
                para.ACirPulses = StubPara.ACirPulses;
                para.CCenterX = StubPara.CCenterX; para.CCenterY = StubPara.CCenterY; para.CCenterZ = StubPara.CCenterZ;
                para.CDirX = StubPara.CDirX; para.CDirY = StubPara.CDirY; para.CDirZ = StubPara.CDirZ;
                para.CCirPulses = StubPara.CCirPulses;
                return _success;
            }
        }

        #endregion
    }
}
