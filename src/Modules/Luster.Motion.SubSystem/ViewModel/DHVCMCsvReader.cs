using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;

namespace Luster.Motion.SubSystem.ViewModel
{
    /// <summary>
    /// 大寰音圈电机 SaveFile() 落盘的 CSV 解析器。
    /// CSV 格式（DHRoboticsVCM.SaveFile 写入）：
    ///   表头: No,Time,Press,Position
    ///   数据: 序号, 时间ms, 压力kgf(已除1000), 位置mm
    /// 文件按行 append 写入（FileShare.ReadWrite），可能在写入过程中被读取，
    /// 因此 Read 内置 Retry，避开"文件被占用"和"只写了表头"的中间状态。
    /// </summary>
    public static class DHVCMCsvReader
    {
        private const int MaxRetry = 6;
        private const int RetryDelayMs = 50;

        /// <summary>
        /// 解析 CSV 文件，返回 Time/Press/Position 三组等长数组。
        /// 解析失败、文件不存在、写入未完成均返回空数组（不抛异常）。
        /// </summary>
        public static SampleBatch Read(string csvPath)
        {
            if (string.IsNullOrEmpty(csvPath) || !File.Exists(csvPath))
            {
                return SampleBatch.Empty;
            }

            for (int attempt = 0; attempt < MaxRetry; attempt++)
            {
                try
                {
                    var batch = TryReadOnce(csvPath);
                    if (batch.TimeMs.Length > 0)
                    {
                        return batch;
                    }
                }
                catch (IOException)
                {
                    // 文件被写入端占用，等下一次 Retry
                }
                catch (Exception)
                {
                    return SampleBatch.Empty;
                }

                Thread.Sleep(RetryDelayMs);
            }

            return SampleBatch.Empty;
        }

        private static SampleBatch TryReadOnce(string csvPath)
        {
            var timeList = new List<double>();
            var pressList = new List<double>();
            var posList = new List<double>();

            using (var fs = new FileStream(csvPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(fs))
            {
                string line;
                bool firstLine = true;
                while ((line = sr.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var parts = line.Split(',');
                    if (parts.Length < 4)
                    {
                        // 列数不足（可能是表头之外的注释行），跳过
                        firstLine = false;
                        continue;
                    }

                    // 表头行: No,Time,Press,Position —— 首行第 1 列非数字则视为表头跳过
                    if (firstLine)
                    {
                        firstLine = false;
                        if (!double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                        {
                            continue;
                        }
                    }

                    // 列顺序: [0]=No, [1]=Time(ms), [2]=Press(kgf), [3]=Position(mm)
                    if (!double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double t) ||
                        !double.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out double p) ||
                        !double.TryParse(parts[3], NumberStyles.Any, CultureInfo.InvariantCulture, out double pos))
                    {
                        continue;
                    }

                    timeList.Add(t);
                    pressList.Add(p);
                    posList.Add(pos);
                }
            }

            return new SampleBatch(
                timeList.ToArray(),
                pressList.ToArray(),
                posList.ToArray());
        }
    }

    /// <summary>
    /// 一组采样数据。三组数组等长；空数组表示解析失败或文件无数据。
    /// </summary>
    public sealed class SampleBatch
    {
        public double[] TimeMs { get; }
        public double[] PressKgf { get; }
        public double[] PositionMm { get; }

        public SampleBatch(double[] timeMs, double[] pressKgf, double[] positionMm)
        {
            TimeMs = timeMs ?? Array.Empty<double>();
            PressKgf = pressKgf ?? Array.Empty<double>();
            PositionMm = positionMm ?? Array.Empty<double>();
        }

        public static SampleBatch Empty { get; } = new SampleBatch(
            Array.Empty<double>(), Array.Empty<double>(), Array.Empty<double>());
    }
}
