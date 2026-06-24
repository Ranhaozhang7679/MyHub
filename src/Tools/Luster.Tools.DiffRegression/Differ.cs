using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Luster.Tools.DiffRegression.Differ
{
    /// <summary>正逆解矩阵 diff：元素级绝对误差 ≤ threshold（默认 1e-6）。</summary>
    /// <remarks>
    /// Coord5Axis 输出 4x4 齐次变换矩阵；源端 vs 迁移后逐元素比对，保证 diff ≤1e-6 可还原性。
    /// 输入支持 CSV（每行一行矩阵元素）或 JSON（数组的数组）。
    /// </remarks>
    internal sealed class MatrixDiffer : IDiffer
    {
        public List<DiffItem> Diff(string baselinePath, string actualPath, double threshold)
        {
            double[][] b = FileLoader.LoadMatrix(baselinePath);
            double[][] a = FileLoader.LoadMatrix(actualPath);
            var items = new List<DiffItem>();

            int rows = Math.Max(b.Length, a.Length);
            for (int r = 0; r < rows; r++)
            {
                if (r >= b.Length)
                {
                    items.Add(RowMismatchItem(r, "baseline 缺行"));
                    continue;
                }
                if (r >= a.Length)
                {
                    items.Add(RowMismatchItem(r, "actual 缺行"));
                    continue;
                }
                int cols = Math.Max(b[r].Length, a[r].Length);
                for (int c = 0; c < cols; c++)
                {
                    string key = $"[{r},{c}]";
                    if (c >= b[r].Length || c >= a[r].Length)
                    {
                        items.Add(new DiffItem
                        {
                            Key = key,
                            Baseline = c < b[r].Length ? b[r][c].ToString("G17", CultureInfo.InvariantCulture) : "(missing)",
                            Actual = c < a[r].Length ? a[r][c].ToString("G17", CultureInfo.InvariantCulture) : "(missing)",
                            Error = null,
                            Pass = false,
                            Note = "列数不一致"
                        });
                        continue;
                    }
                    double bv = b[r][c];
                    double av = a[r][c];
                    double err = Math.Abs(av - bv);
                    items.Add(new DiffItem
                    {
                        Key = key,
                        Baseline = bv.ToString("G17", CultureInfo.InvariantCulture),
                        Actual = av.ToString("G17", CultureInfo.InvariantCulture),
                        Error = err,
                        Pass = err <= threshold,
                        Note = null
                    });
                }
            }
            return items;
        }

        private static DiffItem RowMismatchItem(int r, string note)
        {
            return new DiffItem { Key = $"[row {r}]", Baseline = "(missing)", Actual = "(missing)", Error = null, Pass = false, Note = note };
        }
    }

    /// <summary>key-value diff：标定参数（绝对误差）与 CT 节拍（相对误差）。</summary>
    internal enum KeyValueKind { Absolute, Relative }

    internal sealed class KeyValueDiffer : IDiffer
    {
        private readonly KeyValueKind _kind;

        public KeyValueDiffer(KeyValueKind kind) { _kind = kind; }

        public List<DiffItem> Diff(string baselinePath, string actualPath, double threshold)
        {
            var b = FileLoader.LoadKeyValue(baselinePath).ToDictionary(kv => kv.Key, kv => kv.Value);
            var a = FileLoader.LoadKeyValue(actualPath).ToDictionary(kv => kv.Key, kv => kv.Value);
            var items = new List<DiffItem>();
            var keys = new HashSet<string>(b.Keys);
            foreach (var k in a.Keys) keys.Add(k);

            foreach (string key in keys)
            {
                bool hasB = b.TryGetValue(key, out string bv);
                bool hasA = a.TryGetValue(key, out string av);
                if (!hasB || !hasA)
                {
                    items.Add(new DiffItem
                    {
                        Key = key,
                        Baseline = hasB ? bv : "(missing)",
                        Actual = hasA ? av : "(missing)",
                        Error = null,
                        Pass = false,
                        Note = "key 仅存在于一侧"
                    });
                    continue;
                }

                if (Num.TryParse(bv, out double bnum) && Num.TryParse(av, out double anum))
                {
                    double err = _kind == KeyValueKind.Relative ? Num.RelativeError(bnum, anum) : Math.Abs(anum - bnum);
                    items.Add(new DiffItem
                    {
                        Key = key,
                        Baseline = bnum.ToString("G17", CultureInfo.InvariantCulture),
                        Actual = anum.ToString("G17", CultureInfo.InvariantCulture),
                        Error = err,
                        Pass = err <= threshold,
                        Note = _kind == KeyValueKind.Relative ? "relative" : "absolute"
                    });
                }
                else
                {
                    // 非数值字段：精确匹配
                    bool eq = string.Equals(bv, av, StringComparison.Ordinal);
                    items.Add(new DiffItem
                    {
                        Key = key,
                        Baseline = bv,
                        Actual = av,
                        Error = null,
                        Pass = eq,
                        Note = "string-exact"
                    });
                }
            }
            return items;
        }
    }

    /// <summary>检测判定 diff：分类字段（OK/NG、缺陷类）精确匹配，数值字段绝对误差。</summary>
    /// <remarks>
    /// 检测结果以记录集给出（CSV 带表头或 JSON 对象数组），每条记录按 key（如工件号）对齐。
    /// 数值字段按 threshold 绝对误差比对；非数值字段要求完全一致。
    /// </remarks>
    internal sealed class DetectDiffer : IDiffer
    {
        public List<DiffItem> Diff(string baselinePath, string actualPath, double threshold)
        {
            var b = FileLoader.LoadRecords(baselinePath);
            var a = FileLoader.LoadRecords(actualPath);
            var items = new List<DiffItem>();

            // 用第一条记录的字段并集作为比对字段；记录数不一致直接报缺失
            int count = Math.Max(b.Count, a.Count);
            var fieldSet = new HashSet<string>();
            foreach (var rec in b) foreach (var k in rec.Keys) fieldSet.Add(k);
            foreach (var rec in a) foreach (var k in rec.Keys) fieldSet.Add(k);

            for (int i = 0; i < count; i++)
            {
                if (i >= b.Count)
                {
                    items.Add(new DiffItem { Key = $"record[{i}]", Baseline = "(missing)", Actual = "(present)", Error = null, Pass = false, Note = "baseline 缺记录" });
                    continue;
                }
                if (i >= a.Count)
                {
                    items.Add(new DiffItem { Key = $"record[{i}]", Baseline = "(present)", Actual = "(missing)", Error = null, Pass = false, Note = "actual 缺记录" });
                    continue;
                }
                foreach (string field in fieldSet)
                {
                    string bv = b[i].TryGetValue(field, out string bval) ? bval : "(missing)";
                    string av = a[i].TryGetValue(field, out string aval) ? aval : "(missing)";
                    string key = $"record[{i}].{field}";

                    if (bv == "(missing)" || av == "(missing)")
                    {
                        items.Add(new DiffItem { Key = key, Baseline = bv, Actual = av, Error = null, Pass = false, Note = "字段仅存在于一侧" });
                        continue;
                    }

                    if (Num.TryParse(bv, out double bnum) && Num.TryParse(av, out double anum))
                    {
                        double err = Math.Abs(anum - bnum);
                        items.Add(new DiffItem { Key = key, Baseline = bnum.ToString("G17", CultureInfo.InvariantCulture), Actual = anum.ToString("G17", CultureInfo.InvariantCulture), Error = err, Pass = err <= threshold, Note = "absolute" });
                    }
                    else
                    {
                        bool eq = string.Equals(bv, av, StringComparison.Ordinal);
                        items.Add(new DiffItem { Key = key, Baseline = bv, Actual = av, Error = null, Pass = eq, Note = "string-exact" });
                    }
                }
            }
            return items;
        }
    }
}
