using FluentAssertions;
using Luster.Motion.FiveAxis.Data.Calibration;
using Luster.Motion.FiveAxis.Position;
using Luster.Motion.FiveAxis.Service;
using NUnit.Framework;
using System;
using System.Globalization;
using System.IO;

namespace Luster.Module.Motion.FiveAxis.Tests
{
    /// <summary>
    /// TES-118 标定 cali diff fixture 生成器(测试工程师系统级 diff 回归)。
    ///
    /// 目的:为 Luster.Tools.DiffRegression(cali mode)产出 baseline + actual CSV。
    /// - baseline = 独立 ground truth(已知几何/数学解析,不依赖迁移后代码)
    /// - actual   = 迁移后 FiveAxisCalibrationService 纯算法路径输出(粗标/激光/原点示教)
    ///
    /// 覆盖范围(虚拟侧可跑的纯 C# 算法标定):
    /// - 粗标 RoughCalibrate:已知旋转中心(Rx=90°,中心 (Y=10,Z=5))→ ACenter/ADir/ACirPulses
    /// - 激光标定 LaserCalibrate:两点线性拟合(k=100,b=0)+ 位置落位 + LinearConverter 双向换算
    /// - 原点示教 CalibrateWorkOrigin:三点示教 → Trans(平移 + atan2 长边方向角)
    ///
    /// 不覆盖(明确 carve-out,本 fixture 不产出):
    /// - 精标 AccurateCalibrate:需卡端 IFiveAxisFrame.FrameCal(ADR-TES-110),Frame==null 时 return false
    /// - 检测/CT:见 TES-118 报告(R3 视觉体系未迁移 / 真机 carve-out)
    /// </summary>
    [TestFixture]
    public class CaliDiffFixtureTests
    {
        private static string OutDir =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DiffFixtures");

        private static void WriteCsv(string path, params (string k, double v)[] rows)
        {
            Directory.CreateDirectory(OutDir);
            using (var sw = new StreamWriter(path))
            {
                foreach (var r in rows)
                {
                    sw.WriteLine($"{r.k},{r.v.ToString("G17", CultureInfo.InvariantCulture)}");
                }
            }
        }

        [Test]
        public void Emit_RoughCalibrate_Fixtures()
        {
            // 已知几何:A 轴沿 X,绕 (Y=10,Z=5) 旋转 90°:(30,5)->(10,25)
            var rough = new RoughCaliResult
            {
                FirstPosi = new PositionXYZRxRyRz { X = 100, Y = 0, Z = 0, RX = 0, RY = 0, RZ = 0 },
                Rx = 90,
                Rz = 0,
                ResultFirstPosi = new PositionXYZ(100, 30, 5),
                ResultRxPosi = new PositionXYZ(0, 10, 25),
                ResultRzPosi = new PositionXYZ(0, 0, 0),
            };
            rough.GeneratePosi();

            bool ok = new FiveAxisCalibrationService().RoughCalibrate(rough, mrxPulses: 360000, mrzPulses: 720000);
            ok.Should().BeTrue("粗标已知中心应成功");

            // baseline = 独立 ground truth(数学解析,非迁移后代码派生)
            WriteCsv(Path.Combine(OutDir, "baseline_rough.csv"),
                ("ACenter.X", 0), ("ACenter.Y", 10), ("ACenter.Z", 5),
                ("ADir.X", 1), ("ADir.Y", 0), ("ADir.Z", 0),
                ("ACirPulses", 360000), ("CCirPulses", 720000));

            // actual = 迁移后 Service 输出
            var p = rough.Rough5Para;
            WriteCsv(Path.Combine(OutDir, "actual_rough.csv"),
                ("ACenter.X", p.ACenter.X), ("ACenter.Y", p.ACenter.Y), ("ACenter.Z", p.ACenter.Z),
                ("ADir.X", p.ADir.X), ("ADir.Y", p.ADir.Y), ("ADir.Z", p.ADir.Z),
                ("ACirPulses", p.ACirPulses), ("CCirPulses", p.CCirPulses));

            // sanity 断言(独立 ground truth)
            p.ACenter.Y.Should().BeApproximately(10, 1e-9);
            p.ACenter.Z.Should().BeApproximately(5, 1e-9);
            p.ACirPulses.Should().Be(360000);
            TestContext.WriteLine($"[fixture] 粗标 CSV 已产出 -> {OutDir}");
        }

        [Test]
        public void Emit_LaserCalibrate_Fixtures()
        {
            var laser = new LaserCaliResult();
            var laserPosi = new PositionXYZ(10, 20, 30);
            var cameraPosi = new PositionXYZ(12, 22, 31);

            bool ok = new FiveAxisCalibrationService().LaserCalibrate(laser,
                laser1: 1.0, z1: 100.0, laser2: 5.0, z2: 500.0,
                laserStandard: 12.34, laserPosi: laserPosi, cameraPosi: cameraPosi);
            ok.Should().BeTrue("激光标定应成功");

            // baseline = 独立解析:k=(500-100)/(5-1)=100, b=0
            //   DirectValueToUnit(d)=k*d+b ; UnitToDirectValue(u)=(u-b)/k
            WriteCsv(Path.Combine(OutDir, "baseline_laser.csv"),
                ("LaserStandard", 12.34),
                ("Map1.DirectValue", 1.0), ("Map1.UnitValue", 100.0),
                ("Map2.DirectValue", 5.0), ("Map2.UnitValue", 500.0),
                ("LaserPosi.X", 10), ("LaserPosi.Y", 20), ("LaserPosi.Z", 30),
                ("CameraPosi.X", 12), ("CameraPosi.Y", 22), ("CameraPosi.Z", 31),
                ("DirectValueToUnit_1", 100.0),
                ("DirectValueToUnit_5", 500.0),
                ("UnitToDirectValue_300", 3.0));

            // actual = 迁移后 Service 输出 + LinearConverter 双向换算
            WriteCsv(Path.Combine(OutDir, "actual_laser.csv"),
                ("LaserStandard", laser.LaserStandard),
                ("Map1.DirectValue", laser.LaserMap.Map1.DirectValue),
                ("Map1.UnitValue", laser.LaserMap.Map1.UnitValue),
                ("Map2.DirectValue", laser.LaserMap.Map2.DirectValue),
                ("Map2.UnitValue", laser.LaserMap.Map2.UnitValue),
                ("LaserPosi.X", laser.LaserPosi.X), ("LaserPosi.Y", laser.LaserPosi.Y), ("LaserPosi.Z", laser.LaserPosi.Z),
                ("CameraPosi.X", laser.CameraPosi.X), ("CameraPosi.Y", laser.CameraPosi.Y), ("CameraPosi.Z", laser.CameraPosi.Z),
                ("DirectValueToUnit_1", laser.LaserMap.DirectValueToUnit(1.0)),
                ("DirectValueToUnit_5", laser.LaserMap.DirectValueToUnit(5.0)),
                ("UnitToDirectValue_300", laser.LaserMap.UnitToDirectValue(300.0)));

            laser.LaserMap.DirectValueToUnit(1.0).Should().BeApproximately(100.0, 1e-9);
            TestContext.WriteLine($"[fixture] 激光 CSV 已产出 -> {OutDir}");
        }

        [Test]
        public void Emit_CalibrateWorkOrigin_Fixtures()
        {
            var origin = new TeachWorkOriginResult
            {
                OrgPosiType = TeachWorkOriginResult.OriginPosiType.OriginPosi,
                OriginPosi = new PositionXYZRxRyRz { X = 1, Y = 2, Z = 3, RX = 0, RY = 0, RZ = 0 },
                LongSidePosi = new PositionXYZRxRyRz { X = 4, Y = 6, Z = 9, RX = 0, RY = 0, RZ = 0 },
            };

            bool ok = new FiveAxisCalibrationService().CalibrateWorkOrigin(origin);
            ok.Should().BeTrue("原点示教应成功");

            // baseline = 独立解析:Trans=(1,2,3), RZ=atan2(6-2, 4-1)=atan2(4,3)
            WriteCsv(Path.Combine(OutDir, "baseline_origin.csv"),
                ("Trans.X", 1), ("Trans.Y", 2), ("Trans.Z", 3),
                ("Trans.RZ", Math.Atan2(4, 3)));

            // actual = 迁移后 Service 输出
            var t = origin.RltTool2Work.Trans;
            WriteCsv(Path.Combine(OutDir, "actual_origin.csv"),
                ("Trans.X", t.X), ("Trans.Y", t.Y), ("Trans.Z", t.Z),
                ("Trans.RZ", t.RZ));

            t.RZ.Should().BeApproximately(Math.Atan2(4, 3), 1e-12);
            TestContext.WriteLine($"[fixture] 原点示教 CSV 已产出 -> {OutDir}");
        }
    }
}
