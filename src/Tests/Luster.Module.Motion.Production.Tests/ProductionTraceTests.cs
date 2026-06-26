using Luster.Module.Motion.Production.Trace;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Luster.Module.Motion.ProductionTests
{
    /// <summary>
    /// TES-33 P8-B:生产追溯/审计/图片 单测。
    /// </summary>
    public class ProductionTraceTests : IDisposable
    {
        private readonly string _tmpDir;
        public ProductionTraceTests()
        {
            _tmpDir = Path.Combine(Path.GetTempPath(), "trace_test_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tmpDir);
        }
        public void Dispose() { try { Directory.Delete(_tmpDir, true); } catch { } }

        #region ProductionSummaryBuilder

        [Fact]
        public void BuildSummary_空列表返回零摘要()
        {
            var s = ProductionSummaryBuilder.Build(null);
            Assert.Equal(0, s.Total);
            Assert.Equal(0, s.UPH);

            var s2 = ProductionSummaryBuilder.Build(new List<ProductTraceRecord>());
            Assert.Equal(0, s2.Total);
        }

        [Fact]
        public void BuildSummary_OKNG良率CT计算()
        {
            var records = new List<ProductTraceRecord>
            {
                MakeRecord("SN1", true, 10.0),
                MakeRecord("SN2", true, 20.0),
                MakeRecord("SN3", false, 30.0),
            };
            var s = ProductionSummaryBuilder.Build(records);
            Assert.Equal(3, s.Total);
            Assert.Equal(2, s.OKCount);
            Assert.Equal(1, s.NGCount);
            Assert.Equal(2.0/3.0, s.Yield, 3);
            Assert.Equal(20.0, s.AvgCycleTimeSec, 3); // (10+20+30)/3
        }

        [Fact]
        public void BuildSummary_UPH按平均CT估算()
        {
            var records = new List<ProductTraceRecord>
            {
                MakeRecord("SN1", true, 6.0),  // 3600/6 = 600
            };
            var s = ProductionSummaryBuilder.Build(records);
            Assert.Equal(600, s.UPH);
        }

        [Fact]
        public void BuildSummary_全NG良率为0()
        {
            var records = new List<ProductTraceRecord>
            {
                MakeRecord("SN1", false, 5.0),
                MakeRecord("SN2", false, 5.0),
            };
            var s = ProductionSummaryBuilder.Build(records);
            Assert.Equal(0, s.Yield);
            Assert.Equal(2, s.NGCount);
        }

        #endregion

        #region ProductionTraceService（stub repo）

        [Fact]
        public void Trace_按SN聚合各维度()
        {
            var repo = new StubRepo
            {
                Parameters = new Dictionary<string, object> { ["X"] = 1.0, ["Y"] = 2.0 },
                Images = new List<string> { "img1.jpg", "img2.jpg" },
                IsOK = true,
                NGCode = "",
                CT = 12.5,
                Alarms = new List<string>(),
                Enter = new DateTime(2026, 6, 23, 10, 0, 0),
                Out = new DateTime(2026, 6, 23, 10, 0, 12),
                Recipe = "RecipeA"
            };
            var svc = new ProductionTraceService(repo);

            var rec = svc.Trace("SN001");

            Assert.NotNull(rec);
            Assert.Equal("SN001", rec.SN);
            Assert.True(rec.IsOK);
            Assert.Equal(2, rec.Parameters.Count);
            Assert.Equal(2, rec.ImagePaths.Count);
            Assert.Equal(12.5, rec.CycleTimeSec);
            Assert.Equal("RecipeA", rec.RecipeName);
        }

        [Fact]
        public void Trace_NG产品带NGCode和报警()
        {
            var repo = new StubRepo
            {
                IsOK = false, NGCode = "SCRATCH",
                Alarms = new List<string> { "CAM_FRAME_LOST", "AXIS_LIMIT" },
                CT = 8.0
            };
            var svc = new ProductionTraceService(repo);

            var rec = svc.Trace("SN002");

            Assert.False(rec.IsOK);
            Assert.Equal("SCRATCH", rec.NGCode);
            Assert.Equal(2, rec.AlarmCodes.Count);
        }

        [Fact]
        public void TraceBatch_批量查询()
        {
            var repo = new StubRepo { IsOK = true, CT = 5.0 };
            var svc = new ProductionTraceService(repo);

            var records = svc.TraceBatch(new List<string> { "SN1", "SN2", "SN3" });

            Assert.Equal(3, records.Count);
        }

        [Fact]
        public void TraceBatch_空SN跳过()
        {
            var repo = new StubRepo { IsOK = true };
            var svc = new ProductionTraceService(repo);

            var records = svc.TraceBatch(new List<string> { "SN1", "", null });

            Assert.Single(records);
        }

        [Fact]
        public void BuildSummary_通过Service聚合()
        {
            var repo = new StubRepo { IsOK = true, CT = 10.0 };
            var svc = new ProductionTraceService(repo);
            var records = svc.TraceBatch(new List<string> { "SN1", "SN2" });

            var summary = svc.BuildSummary(records);

            Assert.Equal(2, summary.Total);
            Assert.Equal(2, summary.OKCount);
        }

        #endregion

        #region ImageArchiveService

        [Fact]
        public void Archive_写入磁盘并返回路径()
        {
            var svc = new ImageArchiveService(_tmpDir);
            byte[] data = new byte[] { 1, 2, 3, 4 };

            string path = svc.Archive("SN001", data, ".jpg", new DateTime(2026, 6, 23, 10, 30, 0));

            Assert.NotNull(path);
            Assert.True(File.Exists(path));
            Assert.Contains("SN001", path);
            Assert.Contains("20260623", path);
        }

        [Fact]
        public void Archive_空数据返回null()
        {
            var svc = new ImageArchiveService(_tmpDir);
            Assert.Null(svc.Archive("SN1", null));
            Assert.Null(svc.Archive("SN1", new byte[0]));
            Assert.Null(svc.Archive("", new byte[] { 1 }));
        }

        [Fact]
        public void ListImages_按SN查归档图片()
        {
            var svc = new ImageArchiveService(_tmpDir);
            svc.Archive("SN001", new byte[] { 1 }, ".jpg", new DateTime(2026, 6, 23, 10, 0, 0));
            svc.Archive("SN001", new byte[] { 2 }, ".jpg", new DateTime(2026, 6, 23, 11, 0, 0));
            svc.Archive("SN002", new byte[] { 3 }, ".jpg", new DateTime(2026, 6, 23, 10, 0, 0));

            string[] imgs = svc.ListImages("SN001");

            Assert.Equal(2, imgs.Length);
        }

        [Fact]
        public void BuildArchivePath_纯逻辑路径构造()
        {
            string path = ImageArchiveService.BuildArchivePath(
                @"C:\AOI", "SN001", new DateTime(2026, 6, 23, 10, 30, 45, 123), ".jpg");

            Assert.Contains("20260623", path);
            Assert.Contains("SN001", path);
            Assert.EndsWith(".jpg", path);
        }

        [Fact]
        public void BuildArchivePath_SN含非法字符被净化()
        {
            string path = ImageArchiveService.BuildArchivePath(
                "C:\\AOI", "SN/001:BAD", DateTime.Now, ".jpg");
            Assert.DoesNotContain("/", path.Substring(path.LastIndexOf("SN")));
            Assert.DoesNotContain(":", path.Substring(path.LastIndexOf("SN")));
        }

        #endregion

        #region ProfileAuditor.HasDifference

        [Fact]
        public void HasDifference_引用相等无差异()
        {
            var obj = new { X = 1 };
            Assert.False(ProfileAuditor.HasDifference(obj, obj));
        }

        [Fact]
        public void HasDifference_均为null无差异()
        {
            Assert.False(ProfileAuditor.HasDifference<object>(null, null));
        }

        [Fact]
        public void HasDifference_一方null有差异()
        {
            Assert.True(ProfileAuditor.HasDifference<object>(null, new { X = 1 }));
            Assert.True(ProfileAuditor.HasDifference(new { X = 1 }, null));
        }

        [Fact]
        public void HasDifference_值类型相等无差异()
        {
            Assert.False(ProfileAuditor.HasDifference(10, 10));
        }

        [Fact]
        public void HasDifference_值类型不等有差异()
        {
            Assert.True(ProfileAuditor.HasDifference(10, 20));
        }

        [Fact]
        public void HasDifference_字符串不等有差异()
        {
            Assert.True(ProfileAuditor.HasDifference("RecipeA", "RecipeB"));
            Assert.False(ProfileAuditor.HasDifference("RecipeA", "RecipeA"));
        }

        [Fact]
        public void ProfileNames_对齐源端关键Profile()
        {
            Assert.Equal("AutoCaliProfile", ProfileAuditor.ProfileNames.AutoCaliProfile);
            Assert.Equal("Check5AxisBaseProfile", ProfileAuditor.ProfileNames.Check5AxisBaseProfile);
        }

        #endregion

        #region ProductTraceRecord.ToSummary

        [Fact]
        public void ToSummary_端到端追溯摘要()
        {
            var rec = new ProductTraceRecord(
                "SN001", true, "",
                new Dictionary<string, object> { ["X"] = 1 },
                new List<string> { "img1.jpg", "img2.jpg" },
                12.5,
                new List<string>(),
                new DateTime(2026, 6, 23, 10, 0, 0),
                new DateTime(2026, 6, 23, 10, 0, 12),
                "RecipeA");

            string summary = rec.ToSummary();

            Assert.Contains("SN=SN001", summary);
            Assert.Contains("结果=OK", summary);
            Assert.Contains("CT=12.5s", summary);
            Assert.Contains("图片=2", summary);
            Assert.Contains("配方=RecipeA", summary);
        }

        #endregion

        private static ProductTraceRecord MakeRecord(string sn, bool isOK, double ct)
        {
            return new ProductTraceRecord(sn, isOK, isOK ? "" : "NG",
                new Dictionary<string, object>(), new List<string>(),
                ct, new List<string>(),
                DateTime.Now, DateTime.Now, "RecipeA");
        }

        /// <summary>最小 IProductionTraceRepository stub</summary>
        private class StubRepo : IProductionTraceRepository
        {
            public IReadOnlyDictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
            public IReadOnlyList<string> Images { get; set; } = new List<string>();
            public bool IsOK { get; set; } = true;
            public string NGCode { get; set; } = "";
            public double CT { get; set; } = 0;
            public IReadOnlyList<string> Alarms { get; set; } = new List<string>();
            public DateTime Enter { get; set; } = DateTime.Now;
            public DateTime Out { get; set; } = DateTime.Now;
            public string Recipe { get; set; } = "";

            public IReadOnlyDictionary<string, object> GetParameters(string sn) => Parameters;
            public IReadOnlyList<string> GetImagePaths(string sn) => Images;
            public (bool IsOK, string NGCode) GetResult(string sn) => (IsOK, NGCode);
            public double GetCycleTime(string sn) => CT;
            public IReadOnlyList<string> GetAlarmCodes(string sn) => Alarms;
            public (DateTime Enter, DateTime Out) GetStationTime(string sn) => (Enter, Out);
            public string GetRecipeName(string sn) => Recipe;
        }
    }
}
