using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Luster.Tools.DiffRegression.Differ
{
    /// <summary>比对结果项：单个 key/字段的误差明细。</summary>
    public sealed class DiffItem
    {
        public string Key { get; set; }
        public string Baseline { get; set; }
        public string Actual { get; set; }
        /// <summary>误差值（绝对或相对，依 mode）；分类字段为 null。</summary>
        public double? Error { get; set; }
        public bool Pass { get; set; }
        public string Note { get; set; }
    }

    /// <summary>结构化比对报告，供测试工程师报告引用。</summary>
    public sealed class DiffReport
    {
        public string Mode { get; set; }
        public string Baseline { get; set; }
        public string Actual { get; set; }
        public double Threshold { get; set; }
        public int Total { get; set; }
        public int Passed { get; set; }
        public int Failed { get; set; }
        public double MaxError { get; set; }
        public string Result { get; set; }
        public List<DiffItem> Items { get; set; } = new List<DiffItem>();

        public string ToJson(Formatting formatting)
        {
            return JsonConvert.SerializeObject(this, formatting);
        }
    }

    /// <summary>统一执行入口：按 mode 选 Differ，产出报告。</summary>
    public static class DiffRunner
    {
        public static DiffReport Run(DiffMode mode, string baselinePath, string actualPath, double threshold)
        {
            if (!File.Exists(baselinePath))
            {
                throw new FileNotFoundException("基线文件不存在: " + baselinePath, baselinePath);
            }
            if (!File.Exists(actualPath))
            {
                throw new FileNotFoundException("实际输出文件不存在: " + actualPath, actualPath);
            }

            IDiffer differ;
            switch (mode)
            {
                case DiffMode.Matrix: differ = new MatrixDiffer(); break;
                case DiffMode.Cali: differ = new KeyValueDiffer(KeyValueKind.Absolute); break;
                case DiffMode.Ct: differ = new KeyValueDiffer(KeyValueKind.Relative); break;
                case DiffMode.Detect: differ = new DetectDiffer(); break;
                default: throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }

            List<DiffItem> items = differ.Diff(baselinePath, actualPath, threshold);

            var report = new DiffReport
            {
                Mode = mode.ToString().ToLowerInvariant(),
                Baseline = baselinePath,
                Actual = actualPath,
                Threshold = threshold,
                Total = items.Count,
                Passed = items.Count(i => i.Pass),
                Failed = items.Count(i => !i.Pass),
                MaxError = items.Where(i => i.Error.HasValue).Select(i => i.Error.Value).DefaultIfEmpty(0).Max(),
                Items = items
            };
            report.Result = report.Failed == 0 ? "PASS" : "FAIL";
            return report;
        }
    }

    internal interface IDiffer
    {
        List<DiffItem> Diff(string baselinePath, string actualPath, double threshold);
    }

    /// <summary>数值解析工具。</summary>
    internal static class Num
    {
        public static bool TryParse(string s, out double v)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                v = 0;
                return false;
            }
            return double.TryParse(s.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out v);
        }

        /// <summary>相对误差：|a-b| / max(|baseline|, eps)。</summary>
        public static double RelativeError(double baseline, double actual)
        {
            double denom = Math.Max(Math.Abs(baseline), 1e-9);
            return Math.Abs(actual - baseline) / denom;
        }
    }
}
