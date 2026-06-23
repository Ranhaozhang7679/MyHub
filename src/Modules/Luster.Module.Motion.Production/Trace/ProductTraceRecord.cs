using System;
using System.Collections.Generic;

namespace Luster.Module.Motion.Production.Trace
{
    /// <summary>
    /// 端到端追溯记录（TES-33 P8-B 值对象）。
    /// 聚合 SN → 产品参数 → 检测结果 → 图片 → CT → 报警，端到端可查。
    /// </summary>
    /// <remarks>
    /// 数据源：lmv 既有 <c>ProductStat</c>（结果/CT）+ <c>ProductInfo</c>（参数/图片路径/时间）
    /// + <c>TbAOIImage</c>（图片）+ 报警（<c>TbAlarm</c>）。本类为聚合视图，不改既有表。
    /// </remarks>
    public sealed class ProductTraceRecord
    {
        /// <summary>产品 SN（追溯链主键）</summary>
        public string SN { get; }

        /// <summary>检测结果（true=OK）</summary>
        public bool IsOK { get; }

        /// <summary>NG 报警码（OK 时为空）</summary>
        public string NGCode { get; }

        /// <summary>产品参数（key→value，来自 ProductInfo.Data）</summary>
        public IReadOnlyDictionary<string, object> Parameters { get; }

        /// <summary>检测图片路径列表（来自 TbAOIImage.Img_Raw/Img_Zoom + ProductInfo.ImagePath）</summary>
        public IReadOnlyList<string> ImagePaths { get; }

        /// <summary>节拍 CT（秒，来自 ProductStat.RealCT / ProductInfo.GetCT）</summary>
        public double CycleTimeSec { get; }

        /// <summary>报警码列表（该 SN 关联的报警）</summary>
        public IReadOnlyList<string> AlarmCodes { get; }

        /// <summary>进站时间</summary>
        public DateTime EnterTime { get; }

        /// <summary>出站时间</summary>
        public DateTime OutTime { get; }

        /// <summary>配方名（产品检测时激活配方）</summary>
        public string RecipeName { get; }

        public ProductTraceRecord(string sn, bool isOK, string ngCode,
            IReadOnlyDictionary<string, object> parameters,
            IReadOnlyList<string> imagePaths,
            double cycleTimeSec,
            IReadOnlyList<string> alarmCodes,
            DateTime enterTime, DateTime outTime,
            string recipeName)
        {
            SN = sn ?? string.Empty;
            IsOK = isOK;
            NGCode = ngCode ?? string.Empty;
            Parameters = parameters ?? new Dictionary<string, object>();
            ImagePaths = imagePaths ?? new List<string>();
            CycleTimeSec = cycleTimeSec;
            AlarmCodes = alarmCodes ?? new List<string>();
            EnterTime = enterTime;
            OutTime = outTime;
            RecipeName = recipeName ?? string.Empty;
        }

        /// <summary>追溯链摘要（用于日志/审计/日报）</summary>
        public string ToSummary()
        {
            return $"SN={SN} 结果={(IsOK ? "OK" : "NG:" + NGCode)} CT={CycleTimeSec:F1}s " +
                   $"图片={ImagePaths.Count} 报警={AlarmCodes.Count} 配方={RecipeName} " +
                   $"进站={EnterTime:HH:mm:ss} 出站={OutTime:HH:mm:ss}";
        }
    }
}
