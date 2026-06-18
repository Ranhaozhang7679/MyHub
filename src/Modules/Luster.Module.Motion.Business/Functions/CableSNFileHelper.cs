using Luster.Common.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Luster.Module.Motion.Business.Functions
{
    /// <summary>
    /// 排线SN CSV文件操作辅助类
    /// </summary>
    public static class CableSNFileHelper
    {
        private static readonly object _fileLock = new object();
        private static readonly Encoding _encoding = Encoding.UTF8;

        /// <summary>
        /// 规范化文件路径：将正斜杠替换为反斜杠，兼容 CSVTool 内部使用 LastIndexOf('\\')
        /// </summary>
        private static string NormalizePath(string filePath)
        {
            return filePath?.Replace('/', '\\');
        }

        /// <summary>
        /// 确保目录和文件存在，若文件不存在则创建带列头的空文件
        /// </summary>
        /// <param name="filePath">CSV文件完整路径</param>
        /// <returns>是否成功</returns>
        public static bool EnsureFileExists(string filePath)
        {
            try
            {
                filePath = NormalizePath(filePath);
                var dir = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                if (!File.Exists(filePath))
                {
                    // 写入列头
                    File.WriteAllText(filePath, "时间,排线SN,是否使用" + Environment.NewLine, _encoding);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 读取CSV文件中的全部记录
        /// </summary>
        /// <param name="filePath">CSV文件完整路径</param>
        /// <returns>记录列表；文件不存在时返回null</returns>
        public static List<CableSNRecord> ReadAllRecords(string filePath)
        {
            try
            {
                filePath = NormalizePath(filePath);
                if (!File.Exists(filePath))
                    return null;

                return CSVTool.OpenCSV<CableSNRecord>(filePath);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 将全部记录回写到CSV文件
        /// </summary>
        /// <param name="filePath">CSV文件完整路径</param>
        /// <param name="records">记录列表</param>
        /// <returns>是否成功</returns>
        public static bool SaveAllRecords(string filePath, List<CableSNRecord> records)
        {
            try
            {
                filePath = NormalizePath(filePath);
                CSVTool.SaveCSV(records, filePath, _encoding);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 追加一条记录到CSV文件末尾
        /// </summary>
        /// <param name="filePath">CSV文件完整路径</param>
        /// <param name="sn">排线SN码</param>
        /// <param name="isUsed">是否使用（"是"/"否"）</param>
        /// <returns>是否成功</returns>
        public static bool AppendRecord(string filePath, string sn, string isUsed = "否")
        {
            lock (_fileLock)
            {
                try
                {
                    if (!EnsureFileExists(filePath))
                        return false;

                    // 读取现有记录
                    var records = ReadAllRecords(filePath) ?? new List<CableSNRecord>();

                    // 追加新记录
                    records.Add(new CableSNRecord
                    {
                        Time = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                        SN = sn,
                        IsUsed = isUsed
                    });

                    // 回写全部
                    return SaveAllRecords(filePath, records);
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}
