using System;

namespace Luster.Module.Motion.Production.Oee
{
    /// <summary>
    /// OEE 三因子聚合（TES-33 P8-E）。
    /// OEE = Availability(可用率) × Performance(表现指数) × Quality(质量指数)。
    /// 复用 lmv 既有 <c>ImpactData</c>（RunTime/PMTime/IdleTime/StopTime/TossingTime/ActualRunTime）+
    /// <c>ProductStat</c>（OK/NG/Total）做聚合，不改既有件。
    /// </summary>
    /// <remarks>
    /// 因子定义（对齐 OEE 工业标准 + 源端 ImpactData 语义）：
    /// - Availability = RunTime / (RunTime + StopTime + IdleTime + PMTime)（计划生产时间内实际运行占比）
    /// - Performance = ActualRunTime / RunTime（实际运行时间 vs 理论运行，含 TossingTime 损耗）
    /// - Quality = OKCount / TotalCount（合格率）
    /// - OEE = A × P × Q
    /// UPH ≥1800 实测硬指标 ⚠️ 待现场，软件层按 3600/avgCT 估算（batch2 已有）。
    /// </remarks>
    public static class OeeCalculator
    {
        /// <summary>
        /// 计算 OEE 三因子 + 综合 OEE。
        /// </summary>
        /// <param name="runTime">运行时间（秒）</param>
        /// <param name="stopTime">停机时间（秒）</param>
        /// <param name="idleTime">待机时间（秒）</param>
        /// <param name="pmTime">保养时间（秒）</param>
        /// <param name="actualRunTime">实际运行时间（秒，扣除 TossingTime 损耗）</param>
        /// <param name="okCount">合格数</param>
        /// <param name="totalCount">总数</param>
        public static OeeResult Calculate(double runTime, double stopTime, double idleTime, double pmTime,
            double actualRunTime, int okCount, int totalCount)
        {
            // Availability: 计划生产时间 = Run + Stop + Idle + PM
            double plannedProductiveTime = runTime + stopTime + idleTime + pmTime;
            double availability = plannedProductiveTime > 0 ? runTime / plannedProductiveTime : 0;

            // Performance: 实际运行 / 运行（含损耗）
            double performance = runTime > 0 ? actualRunTime / runTime : 0;
            if (performance > 1) performance = 1; // 实际不应超理论

            // Quality: 合格率
            double quality = totalCount > 0 ? (double)okCount / totalCount : 0;

            double oee = availability * performance * quality;

            return new OeeResult(availability, performance, quality, oee,
                runTime, stopTime, idleTime, pmTime, actualRunTime, okCount, totalCount);
        }

        /// <summary>OEE 百分比展示（0~100）</summary>
        public static double ToPercent(double ratio) => Math.Round(ratio * 100, 1);
    }

    /// <summary>OEE 计算结果（值对象）</summary>
    public sealed class OeeResult
    {
        /// <summary>可用率（0~1）</summary>
        public double Availability { get; }
        /// <summary>表现指数（0~1）</summary>
        public double Performance { get; }
        /// <summary>质量指数（0~1）</summary>
        public double Quality { get; }
        /// <summary>综合 OEE（0~1）</summary>
        public double Oee { get; }

        public double RunTime { get; }
        public double StopTime { get; }
        public double IdleTime { get; }
        public double PmTime { get; }
        public double ActualRunTime { get; }
        public int OKCount { get; }
        public int TotalCount { get; }

        public OeeResult(double availability, double performance, double quality, double oee,
            double runTime, double stopTime, double idleTime, double pmTime, double actualRunTime,
            int okCount, int totalCount)
        {
            Availability = availability; Performance = performance; Quality = quality; Oee = oee;
            RunTime = runTime; StopTime = stopTime; IdleTime = idleTime; PmTime = pmTime;
            ActualRunTime = actualRunTime; OKCount = okCount; TotalCount = totalCount;
        }

        /// <summary>看板摘要</summary>
        public string ToDashboard()
        {
            return $"OEE={OeeCalculator.ToPercent(Oee)}% " +
                   $"(A={OeeCalculator.ToPercent(Availability)}% P={OeeCalculator.ToPercent(Performance)}% Q={OeeCalculator.ToPercent(Quality)}%) " +
                   $"OK={OKCount}/{TotalCount}";
        }
    }
}
