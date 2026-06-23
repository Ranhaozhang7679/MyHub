using System.Collections.Generic;

namespace Luster.Module.Motion.Production.Trace
{
    /// <summary>
    /// 生产追溯数据访问（TES-33 P8-B，抽象便于 mock 测，真实 DB 实现留集成）。
    /// </summary>
    public interface IProductionTraceRepository
    {
        /// <summary>取某 SN 的产品参数（来自 TbProductInfo.Data / ProductInfo.Data）</summary>
        IReadOnlyDictionary<string, object> GetParameters(string sn);

        /// <summary>取某 SN 的检测图片路径（来自 TbAOIImage + ProductInfo.ImagePath）</summary>
        IReadOnlyList<string> GetImagePaths(string sn);

        /// <summary>取某 SN 的检测结果（true=OK）+ NG 码（来自 ProductStat / TbProductYeild）</summary>
        (bool IsOK, string NGCode) GetResult(string sn);

        /// <summary>取某 SN 的 CT（秒）</summary>
        double GetCycleTime(string sn);

        /// <summary>取某 SN 关联的报警码列表（来自 TbAlarm）</summary>
        IReadOnlyList<string> GetAlarmCodes(string sn);

        /// <summary>取某 SN 的进/出站时间</summary>
        (System.DateTime Enter, System.DateTime Out) GetStationTime(string sn);

        /// <summary>取某 SN 检测时的配方名</summary>
        string GetRecipeName(string sn);
    }

    /// <summary>
    /// 生产追溯服务（TES-33 P8-B，端到端追溯链）。
    /// 按 SN 聚合参数→结果→图片→CT→报警，返回 <see cref="ProductTraceRecord"/>。
    /// </summary>
    public interface IProductionTraceService
    {
        /// <summary>查询单个 SN 的端到端追溯记录</summary>
        ProductTraceRecord Trace(string sn);

        /// <summary>批量查询（生产摘要/日报用）</summary>
        IReadOnlyList<ProductTraceRecord> TraceBatch(IReadOnlyList<string> sns);

        /// <summary>生成生产摘要（OK/NG/良率/平均CT）</summary>
        ProductionSummary BuildSummary(IReadOnlyList<ProductTraceRecord> records);
    }

    /// <summary>生产摘要（源端生产摘要/日报）</summary>
    public sealed class ProductionSummary
    {
        public int Total { get; }
        public int OKCount { get; }
        public int NGCount { get; }
        /// <summary>良率（0~1）</summary>
        public double Yield { get; }
        /// <summary>平均 CT（秒）</summary>
        public double AvgCycleTimeSec { get; }
        /// <summary>UPH（按平均 CT 估算）</summary>
        public int UPH { get; }

        public ProductionSummary(int total, int okCount, int ngCount, double yield, double avgCycleTimeSec, int uph)
        {
            Total = total; OKCount = okCount; NGCount = ngCount;
            Yield = yield; AvgCycleTimeSec = avgCycleTimeSec; UPH = uph;
        }
    }
}
