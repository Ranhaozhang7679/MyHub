using System;
using System.Collections.Generic;
using System.Linq;

namespace Luster.Module.Motion.Production.Trace
{
    /// <summary>
    /// <see cref="IProductionTraceService"/> 默认实现（TES-33 P8-B）。
    /// 按 SN 聚合 <see cref="IProductionTraceRepository"/> 各维度数据为 <see cref="ProductTraceRecord"/>。
    /// </summary>
    public class ProductionTraceService : IProductionTraceService
    {
        private readonly IProductionTraceRepository _repo;

        public ProductionTraceService(IProductionTraceRepository repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        /// <inheritdoc/>
        public ProductTraceRecord Trace(string sn)
        {
            if (string.IsNullOrEmpty(sn)) return null;
            if (_repo == null) return null;

            var parameters = _repo.GetParameters(sn) ?? new Dictionary<string, object>();
            var images = _repo.GetImagePaths(sn) ?? new List<string>();
            var result = _repo.GetResult(sn);
            double ct = _repo.GetCycleTime(sn);
            var alarms = _repo.GetAlarmCodes(sn) ?? new List<string>();
            var times = _repo.GetStationTime(sn);
            string recipe = _repo.GetRecipeName(sn) ?? string.Empty;

            return new ProductTraceRecord(
                sn, result.IsOK, result.NGCode,
                parameters, images, ct, alarms,
                times.Enter, times.Out, recipe);
        }

        /// <inheritdoc/>
        public IReadOnlyList<ProductTraceRecord> TraceBatch(IReadOnlyList<string> sns)
        {
            if (sns == null) return new List<ProductTraceRecord>();
            var list = new List<ProductTraceRecord>(sns.Count);
            foreach (var sn in sns)
            {
                var r = Trace(sn);
                if (r != null) list.Add(r);
            }
            return list;
        }

        /// <inheritdoc/>
        public ProductionSummary BuildSummary(IReadOnlyList<ProductTraceRecord> records)
        {
            return ProductionSummaryBuilder.Build(records);
        }
    }

    /// <summary>生产摘要构建器（纯逻辑，便于单测）</summary>
    public static class ProductionSummaryBuilder
    {
        /// <summary>从追溯记录列表构建生产摘要（源端生产摘要/日报）</summary>
        public static ProductionSummary Build(IReadOnlyList<ProductTraceRecord> records)
        {
            if (records == null || records.Count == 0)
            {
                return new ProductionSummary(0, 0, 0, 0, 0, 0);
            }

            int total = records.Count;
            int ok = records.Count(r => r.IsOK);
            int ng = total - ok;
            double yield = total > 0 ? (double)ok / total : 0;
            double avgCt = records.Average(r => r.CycleTimeSec);
            // UPH 估算：3600 / 平均CT（单产品节拍）
            int uph = avgCt > 0 ? (int)Math.Round(3600.0 / avgCt) : 0;

            return new ProductionSummary(total, ok, ng, yield, avgCt, uph);
        }
    }
}
