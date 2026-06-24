using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Luster.Tools.DiffRegression.Differ
{
    /// <summary>CSV/JSON 统一加载器。按扩展名分流，CSV 用内建解析器（处理引号/逗号）。</summary>
    internal static class FileLoader
    {
        public static bool IsJson(string path)
        {
            return path.EndsWith(".json", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>读取二维数值网格：CSV 行列，或 JSON 数组的数组 / 扁平数组。</summary>
        public static double[][] LoadMatrix(string path)
        {
            if (IsJson(path))
            {
                JToken token = JToken.Parse(File.ReadAllText(path));
                if (token is JArray arr)
                {
                    // 数组的数组：[[...],[...]]
                    if (arr.Count > 0 && arr[0] is JArray)
                    {
                        return arr.Select(row => row.Select(c => (double)Convert.ToDouble(c, CultureInfo.InvariantCulture)).ToArray()).ToArray();
                    }
                    // 扁平数组：按行自动 reshape 为单行
                    double[] flat = arr.Select(c => (double)Convert.ToDouble(c, CultureInfo.InvariantCulture)).ToArray();
                    return new[] { flat };
                }
                throw new FormatException("JSON 矩阵应为数组（[[...],[...]] 或 [...]）。");
            }

            // CSV: 每行按逗号切分为数值
            var rows = new List<double[]>();
            foreach (string line in File.ReadAllLines(path))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                string[] cells = Csv.SplitLine(line);
                rows.Add(cells.Select(c =>
                {
                    if (!Num.TryParse(c, out double v))
                    {
                        throw new FormatException($"CSV 单元格非数值: '{c}'");
                    }
                    return v;
                }).ToArray());
            }
            return rows.ToArray();
        }

        /// <summary>读取 key-value：CSV 两列(key,value)，或 JSON 对象。</summary>
        public static IEnumerable<KeyValuePair<string, string>> LoadKeyValue(string path)
        {
            if (IsJson(path))
            {
                JObject obj = JObject.Parse(File.ReadAllText(path));
                foreach (JProperty p in obj.Properties())
                {
                    yield return new KeyValuePair<string, string>(p.Name, p.Value?.ToString() ?? string.Empty);
                }
                yield break;
            }

            foreach (string line in File.ReadAllLines(path))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                string[] cells = Csv.SplitLine(line);
                if (cells.Length < 2) continue;
                yield return new KeyValuePair<string, string>(cells[0].Trim(), cells[1].Trim());
            }
        }

        /// <summary>读取记录集：CSV 带表头，或 JSON 对象数组。返回每条记录的字段字典。</summary>
        public static List<Dictionary<string, string>> LoadRecords(string path)
        {
            if (IsJson(path))
            {
                JToken token = JToken.Parse(File.ReadAllText(path));
                if (token is JArray arr)
                {
                    return arr.OfType<JObject>()
                        .Select(o => o.Properties().ToDictionary(p => p.Name, p => p.Value?.ToString() ?? string.Empty))
                        .ToList();
                }
                throw new FormatException("JSON 检测记录应为对象数组。");
            }

            // CSV 带表头
            var result = new List<Dictionary<string, string>>();
            string[] lines = File.ReadAllLines(path);
            if (lines.Length == 0) return result;
            string[] header = Csv.SplitLine(lines[0]).Select(h => h.Trim()).ToArray();
            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;
                string[] cells = Csv.SplitLine(lines[i]);
                var rec = new Dictionary<string, string>();
                for (int c = 0; c < header.Length; c++)
                {
                    rec[header[c]] = c < cells.Length ? cells[c].Trim() : string.Empty;
                }
                result.Add(rec);
            }
            return result;
        }
    }

    /// <summary>简易 CSV 行解析器：处理双引号包裹与转义。</summary>
    internal static class Csv
    {
        public static string[] SplitLine(string line)
        {
            var fields = new List<string>();
            int i = 0;
            while (i < line.Length)
            {
                if (line[i] == '"')
                {
                    // 引号字段
                    i++;
                    int start = i;
                    var sb = new System.Text.StringBuilder();
                    while (i < line.Length)
                    {
                        if (line[i] == '"')
                        {
                            if (i + 1 < line.Length && line[i + 1] == '"')
                            {
                                sb.Append('"');
                                i += 2;
                            }
                            else
                            {
                                i++;
                                break;
                            }
                        }
                        else
                        {
                            sb.Append(line[i]);
                            i++;
                        }
                    }
                    fields.Add(sb.ToString());
                    // 跳过到下一个逗号
                    while (i < line.Length && line[i] != ',') i++;
                    if (i < line.Length && line[i] == ',') i++;
                }
                else
                {
                    int comma = line.IndexOf(',', i);
                    if (comma < 0)
                    {
                        fields.Add(line.Substring(i).Trim());
                        break;
                    }
                    fields.Add(line.Substring(i, comma - i).Trim());
                    i = comma + 1;
                }
            }
            return fields.ToArray();
        }
    }
}
