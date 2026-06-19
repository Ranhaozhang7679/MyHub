using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Luster.Motion.ReportUI.ViewModel
{
    /// <summary>
    /// 解析现场 CSV 文件名中的时间戳，用于按时间段筛选曲线文件。
    /// 文件名格式：<SN>_<YYYY-M-D_HH-MM-SS-FFF>_<类型>.csv
    /// 示例：G9NHTBW5B1N0000WH0+8HTF_2026-6-18_17-16-45-781_TimeTorAngPre.csv
    /// </summary>
    public static class FileNameTimestampParser
    {
        private static readonly Regex TimestampRegex = new Regex(
            @"_(\d{4})-(\d{1,2})-(\d{1,2})_(\d{2})-(\d{2})-(\d{2})-(\d{3})_",
            RegexOptions.Compiled);

        /// <summary>
        /// 解析文件名中的时间戳；失败返回 DateTime.MinValue。
        /// </summary>
        public static DateTime TryParse(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return DateTime.MinValue;
            string fileName = Path.GetFileName(filePath);
            var m = TimestampRegex.Match(fileName);
            if (!m.Success) return DateTime.MinValue;

            if (!int.TryParse(m.Groups[1].Value, out int year) ||
                !int.TryParse(m.Groups[2].Value, out int month) ||
                !int.TryParse(m.Groups[3].Value, out int day) ||
                !int.TryParse(m.Groups[4].Value, out int hour) ||
                !int.TryParse(m.Groups[5].Value, out int minute) ||
                !int.TryParse(m.Groups[6].Value, out int second) ||
                !int.TryParse(m.Groups[7].Value, out int ms))
            {
                return DateTime.MinValue;
            }

            if (month < 1 || month > 12 || day < 1 || day > 31 ||
                hour < 0 || hour > 23 || minute < 0 || minute > 59 ||
                second < 0 || second > 59 || ms < 0 || ms > 999)
            {
                return DateTime.MinValue;
            }

            try
            {
                return new DateTime(year, month, day, hour, minute, second, ms);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        /// <summary>
        /// 判断文件名时间是否落在 [start, end] 区间。
        /// 解析失败的文件返回 true（保留），避免漏掉非标准命名的数据。
        /// start/end 任一为 DateTime.MinValue 表示该侧不限制。
        /// </summary>
        public static bool IsInTimeRange(string filePath, DateTime start, DateTime end)
        {
            var dt = TryParse(filePath);
            if (dt == DateTime.MinValue) return true;

            if (start != DateTime.MinValue && dt < start) return false;
            if (end != DateTime.MinValue && dt > end) return false;
            return true;
        }
    }
}
