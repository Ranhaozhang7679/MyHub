using FluentAssertions;
using Luster.Motion.DataStruct.Real;
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
    /// TES-118 标定 diff 回归 actual 采集器。
    ///
    /// 用与手算基线（Diff/Baseline/cali_*_baseline.csv，来源见 BASELINE-Provenance.md）完全相同的输入，
    /// 调迁移端 FiveAxisCalibrationService 产出实际输出，落盘到 Diff/Actual/ 供 Luster.Tools.DiffRegression --mode cali 比对。
    ///
    /// 范围：仅采集 actual（test 代码 + 测试数据），不建 diff 工具、不改实现代码。
    /// 覆盖：粗标(RoughCalibrate)/激光(LaserCalibrate)/工件原点(CalibrateWorkOrigin)三阶段纯 C# 算法。
    /// 精标(AccurateCalibrate)卡端 FrameCal ⚠️ 待人类现场(R-F4)，不在此采集。
    /// </summary>
    [TestFixture]
    public class CalibrationDiffActualTests
    {
        private static string DiffDir => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Diff");
        private static string ActualDir => Path.Combine(DiffDir, "Actual");
        private static string BaselineDir => Path.Combine(DiffDir, "Baseline");

        [OneTimeSetUp]
        public void EnsureDirs()
        {
            Directory.CreateDirectory(ActualDir);
            Directory.CreateDirectory(BaselineDir);
            // 基线 CSV 由 csproj Content 拷贝到 Baseline/；兜底：从源码目录拷
            CopyBaselineFromSourceIfNeeded();
        }

        /// <summary>粗标 actual：与基线同输入，落盘 Rough5Para 全字段。</summary>
        [Test]
        public void RoughCalibrate_DumpActual_ForDiff()
        {
            var rough = new RoughCaliResult
            {
                FirstPosi = new PositionXYZRxRyRz { X = 0, Y = 0, Z = 0, RX = 0, RY = 0, RZ = 0 },
                Rx = 90,
                Rz = 90,
                ResultFirstPosi = new PositionXYZ(50, 30, 5),
                ResultRxPosi = new PositionXYZ(0, 10, 25),
                ResultRzPosi = new PositionXYZ(10, 30, 0),
            };
            rough.GeneratePosi(); // RxPosi.RX=90, RzPosi.RZ=90

            bool ok = new FiveAxisCalibrationService().RoughCalibrate(rough, mrxPulses: 360000, mrzPulses: 720000);
            ok.Should().BeTrue("粗标应成功");

            var p = rough.Rough5Para;
            var lines = new[]
            {
                $"ACenter.X,{Fmt(p.ACenter.X)}",
                $"ACenter.Y,{Fmt(p.ACenter.Y)}",
                $"ACenter.Z,{Fmt(p.ACenter.Z)}",
                $"ADir.X,{Fmt(p.ADir.X)}",
                $"ADir.Y,{Fmt(p.ADir.Y)}",
                $"ADir.Z,{Fmt(p.ADir.Z)}",
                $"ACirPulses,{Fmt(p.ACirPulses)}",
                $"CCenter.X,{Fmt(p.CCenter.X)}",
                $"CCenter.Y,{Fmt(p.CCenter.Y)}",
                $"CCenter.Z,{Fmt(p.CCenter.Z)}",
                $"CDir.X,{Fmt(p.CDir.X)}",
                $"CDir.Y,{Fmt(p.CDir.Y)}",
                $"CDir.Z,{Fmt(p.CDir.Z)}",
                $"CCirPulses,{Fmt(p.CCirPulses)}",
            };
            File.WriteAllLines(Path.Combine(ActualDir, "cali_rough_actual.csv"), lines);
            File.Exists(Path.Combine(BaselineDir, "cali_rough_baseline.csv")).Should().BeTrue(
                "基线文件应就位（csproj Content 拷贝）");
        }

        /// <summary>激光标定 actual：落 LaserMap/LaserPosi/CameraPosi + 派生 k/b/CameraOffset。</summary>
        [Test]
        public void LaserCalibrate_DumpActual_ForDiff()
        {
            var laser = new LaserCaliResult();
            var laserPosi = new PositionXYZ(10, 20, 30);
            var cameraPosi = new PositionXYZ(12, 22, 31);

            bool ok = new FiveAxisCalibrationService().LaserCalibrate(laser,
                laser1: 1.0, z1: 100.0, laser2: 5.0, z2: 500.0,
                laserStandard: 12.34, laserPosi: laserPosi, cameraPosi: cameraPosi);
            ok.Should().BeTrue("激光标定应成功");

            // k/b 由 Service 写入的 Map1/Map2 派生（两点定标 y=kx+b）——与基线手算独立对照
            double k = (laser.LaserMap.Map2.UnitValue - laser.LaserMap.Map1.UnitValue)
                       / (laser.LaserMap.Map2.DirectValue - laser.LaserMap.Map1.DirectValue);
            double b = laser.LaserMap.Map1.UnitValue - k * laser.LaserMap.Map1.DirectValue;
            var offset = (laser.CameraPosi - laser.LaserPosi) as PositionXYZ;

            var lines = new[]
            {
                $"LaserStandard,{Fmt(laser.LaserStandard)}",
                $"Map1.DirectValue,{Fmt(laser.LaserMap.Map1.DirectValue)}",
                $"Map1.UnitValue,{Fmt(laser.LaserMap.Map1.UnitValue)}",
                $"Map2.DirectValue,{Fmt(laser.LaserMap.Map2.DirectValue)}",
                $"Map2.UnitValue,{Fmt(laser.LaserMap.Map2.UnitValue)}",
                $"LaserMap.k,{Fmt(k)}",
                $"LaserMap.b,{Fmt(b)}",
                $"LaserPosi.X,{Fmt(laser.LaserPosi.X)}",
                $"LaserPosi.Y,{Fmt(laser.LaserPosi.Y)}",
                $"LaserPosi.Z,{Fmt(laser.LaserPosi.Z)}",
                $"CameraPosi.X,{Fmt(laser.CameraPosi.X)}",
                $"CameraPosi.Y,{Fmt(laser.CameraPosi.Y)}",
                $"CameraPosi.Z,{Fmt(laser.CameraPosi.Z)}",
                $"CameraOffset.X,{Fmt(offset.X)}",
                $"CameraOffset.Y,{Fmt(offset.Y)}",
                $"CameraOffset.Z,{Fmt(offset.Z)}",
            };
            File.WriteAllLines(Path.Combine(ActualDir, "cali_laser_actual.csv"), lines);
        }

        /// <summary>工件原点 actual：落 RltTool2Work.Trans。</summary>
        [Test]
        public void CalibrateWorkOrigin_DumpActual_ForDiff()
        {
            var origin = new TeachWorkOriginResult
            {
                OrgPosiType = TeachWorkOriginResult.OriginPosiType.OriginPosi,
                OriginPosi = new PositionXYZRxRyRz { X = 1, Y = 2, Z = 3, RX = 0, RY = 0, RZ = 0 },
                LongSidePosi = new PositionXYZRxRyRz { X = 4, Y = 6, Z = 9, RX = 0, RY = 0, RZ = 0 },
            };

            bool ok = new FiveAxisCalibrationService().CalibrateWorkOrigin(origin);
            ok.Should().BeTrue("工件原点示教应成功");

            var t = origin.RltTool2Work.Trans;
            var lines = new[]
            {
                $"Trans.X,{Fmt(t.X)}",
                $"Trans.Y,{Fmt(t.Y)}",
                $"Trans.Z,{Fmt(t.Z)}",
                $"Trans.RZ,{Fmt(t.RZ)}",
            };
            File.WriteAllLines(Path.Combine(ActualDir, "cali_origin_actual.csv"), lines);
        }

        // —— 工具方法 ——

        private static string Fmt(double v) => v.ToString("G17", CultureInfo.InvariantCulture);

        private void CopyBaselineFromSourceIfNeeded()
        {
            // 兜底：IDE/本地直跑时 Content 未拷贝，从源码目录补
            var src = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..",
                "tests", "Luster.Module.Motion.FiveAxis.Tests", "Diff", "Baseline");
            if (!Directory.Exists(src)) return;
            foreach (var f in Directory.GetFiles(src, "*.csv"))
            {
                var dst = Path.Combine(BaselineDir, Path.GetFileName(f));
                if (!File.Exists(dst)) File.Copy(f, dst);
            }
        }
    }
}
