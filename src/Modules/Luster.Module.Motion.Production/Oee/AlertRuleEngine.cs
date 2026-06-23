using System;
using System.Collections.Generic;

namespace Luster.Module.Motion.Production.Oee
{
    /// <summary>
    /// 红黄预警规则引擎（TES-33 P8-E）。
    /// 简单阈值驱动：CT/良率/OEE/温度等指标超阈值 → 红/黄预警。
    /// 对齐源端报警分级 + lmv AlarmType（WarningTip 黄 / DeviceError 红）。
    /// </summary>
    public static class AlertRuleEngine
    {
        /// <summary>
        /// 评估单条指标阈值规则。
        /// </summary>
        /// <param name="metric">指标名（如 "CT"/"Yield"/"OEE"）</param>
        /// <param name="value">当前值</param>
        /// <param name="yellowThreshold">黄线阈值</param>
        /// <param name="redThreshold">红线阈值</param>
        /// <param name="compareMode">比较模式：Above=值≥阈值触发（CT），Below=值≤阈值触发（良率/OEE）</param>
        public static AlertLevel EvaluateMetric(string metric, double value,
            double yellowThreshold, double redThreshold, AlertCompareMode compareMode)
        {
            bool red, yellow;
            if (compareMode == AlertCompareMode.Above)
            {
                // 值越高越危险（CT）：≥红线红，≥黄线黄
                red = value >= redThreshold;
                yellow = value >= yellowThreshold;
            }
            else
            {
                // 值越低越危险（良率/OEE）：≤红线红，≤黄线黄
                red = value <= redThreshold;
                yellow = value <= yellowThreshold;
            }

            if (red) return AlertLevel.Red;
            if (yellow) return AlertLevel.Yellow;
            return AlertLevel.Green;
        }

        /// <summary>
        /// 批量评估规则，返回触发的预警列表（Green 不返回）。
        /// </summary>
        public static IReadOnlyList<AlertRecord> EvaluateAll(IReadOnlyList<AlertRule> rules,
            IReadOnlyDictionary<string, double> metricValues)
        {
            var alerts = new List<AlertRecord>();
            if (rules == null || metricValues == null) return alerts;

            foreach (var rule in rules)
            {
                if (!rule.Enable) continue;
                if (!metricValues.TryGetValue(rule.Metric, out double value)) continue;

                var level = EvaluateMetric(rule.Metric, value, rule.YellowThreshold, rule.RedThreshold, rule.CompareMode);
                if (level != AlertLevel.Green)
                {
                    alerts.Add(new AlertRecord(level, rule.Metric, value, rule.RedThreshold, rule.YellowThreshold,
                        BuildMessage(rule, value, level)));
                }
            }
            // 红优先于黄
            alerts.Sort((a, b) => b.Level.CompareTo(a.Level));
            return alerts;
        }

        private static string BuildMessage(AlertRule rule, double value, AlertLevel level)
        {
            string levelText = level == AlertLevel.Red ? "红线" : "黄线";
            string trend = rule.CompareMode == AlertCompareMode.Above ? "超" : "低于";
            return $"[{levelText}]{rule.Metric}={value:F2}{trend}阈值(黄{rule.YellowThreshold}/红{rule.RedThreshold})";
        }

        /// <summary>预置 P8-E 阈值规则（对齐用户硬指标：UPH≥1800、良率、CT）</summary>
        public static IReadOnlyList<AlertRule> DefaultRules()
        {
            return new List<AlertRule>
            {
                // CT 越高越危险（Above）：黄 3s 红 5s（示例阈值，现场标定）
                new AlertRule("CT", 3.0, 5.0, AlertCompareMode.Above),
                // 良率越低越危险（Below）：黄 98% 红 95%
                new AlertRule("Yield", 0.98, 0.95, AlertCompareMode.Below),
                // OEE 越低越危险：黄 60% 红 40%
                new AlertRule("OEE", 0.60, 0.40, AlertCompareMode.Below),
            };
        }
    }

    /// <summary>预警等级</summary>
    public enum AlertLevel
    {
        Green = 0,
        Yellow = 1,
        Red = 2
    }

    /// <summary>比较模式</summary>
    public enum AlertCompareMode
    {
        /// <summary>值≥阈值触发（CT 等越高越危险）</summary>
        Above = 0,
        /// <summary>值≤阈值触发（良率/OEE 等越低越危险）</summary>
        Below = 1
    }

    /// <summary>预警规则</summary>
    public sealed class AlertRule
    {
        public string Metric { get; }
        public double YellowThreshold { get; }
        public double RedThreshold { get; }
        public AlertCompareMode CompareMode { get; }
        public bool Enable { get; set; } = true;

        public AlertRule(string metric, double yellowThreshold, double redThreshold, AlertCompareMode compareMode)
        {
            Metric = metric ?? string.Empty;
            YellowThreshold = yellowThreshold;
            RedThreshold = redThreshold;
            CompareMode = compareMode;
        }
    }

    /// <summary>预警记录</summary>
    public sealed class AlertRecord
    {
        public AlertLevel Level { get; }
        public string Metric { get; }
        public double Value { get; }
        public double RedThreshold { get; }
        public double YellowThreshold { get; }
        public string Message { get; }

        public AlertRecord(AlertLevel level, string metric, double value,
            double redThreshold, double yellowThreshold, string message)
        {
            Level = level; Metric = metric; Value = value;
            RedThreshold = redThreshold; YellowThreshold = yellowThreshold; Message = message;
        }
    }
}
