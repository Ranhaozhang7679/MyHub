using Luster.Common.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Luster.Module.Motion.Business.Functions
{
    /// <summary>
    /// 排线SN CSV文件操作辅助类。
    /// 所有"读-改-写"复合操作均在 <see cref="_fileLock"/> 内原子完成，
    /// 消除单进程多工站并发读取同一未使用SN的竞态。
    /// 返回值统一为业务结果码：1=成功 3=文件不存在 4=无数据 5=SN不存在 6=写入失败 8=SN已存在 9=未知异常 10=已使用不可归还。
    /// </summary>
    /// <remarks>
    /// 不依赖 CSVTool.OpenCSV 读取（其 line.Split(',') 后按 data[i] 索引无边界保护，
    /// 遇空行/字段不足行会 IndexOutOfRangeException），此处自实现健壮读取。
    /// 写入仍用 CSVTool.SaveCSV，但 SaveAllRecords 采用"临时文件+File.Replace"原子替换，
    /// 避免截断覆写中途崩溃导致全部SN数据丢失。
    /// </remarks>
    public static class CableSNFileHelper
    {
        private static readonly object _fileLock = new object();
        private static readonly Encoding _encoding = Encoding.UTF8;

        /// <summary>未使用（可被读取预占）</summary>
        public const string StateUnused = "否";
        /// <summary>预占（已读取分配，等待标记确认，不可再被读取）</summary>
        public const string StateOccupied = "预占";
        /// <summary>已使用</summary>
        public const string StateUsed = "是";

        /// <summary>
        /// 规范化文件路径：将正斜杠替换为反斜杠，兼容 CSVTool 内部使用 LastIndexOf('\\')
        /// </summary>
        private static string NormalizePath(string filePath)
        {
            return filePath?.Replace('/', '\\');
        }

        /// <summary>
        /// 确保目录和文件存在，若文件不存在则创建带列头的空文件。
        /// 仅写列头不追加尾部换行，避免制造空行导致读取越界。
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
                    // 仅写列头，不加尾部换行，避免空数据行
                    File.WriteAllText(filePath, "时间,排线SN,是否使用", _encoding);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 健壮读取CSV全部记录。
        /// 跳过空行与字段数不足3的行，避免 CSVTool.OpenCSV 的 data[i] 越界崩溃。
        /// 列顺序须与 SaveCSV 写入一致：Time,SN,IsUsed。
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

                var list = new List<CableSNRecord>();
                var lines = File.ReadAllLines(filePath, _encoding);

                // 跳过第0行表头，从第1行开始解析数据
                for (int i = 1; i < lines.Length; i++)
                {
                    var line = lines[i];
                    if (string.IsNullOrWhiteSpace(line))
                        continue; // 跳过空行

                    var data = line.Split(',');
                    if (data.Length < 3)
                        continue; // 字段不足，跳过损坏行

                    list.Add(new CableSNRecord
                    {
                        Time = data[0],
                        SN = data[1],
                        IsUsed = data[2]
                    });
                }

                return list;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 原子回写全部记录：先写临时文件，写完后用 File.Replace 原子替换原文件。
        /// 即使写入中途进程崩溃/掉电，原文件仍保持完整（旧内容或新内容二选一），不会出现截断的中间态。
        /// </summary>
        /// <param name="filePath">CSV文件完整路径</param>
        /// <param name="records">记录列表</param>
        /// <returns>是否成功</returns>
        public static bool SaveAllRecords(string filePath, List<CableSNRecord> records)
        {
            string tmp = null;
            try
            {
                filePath = NormalizePath(filePath);
                tmp = filePath + ".tmp";

                // 写入临时文件（CSVTool.SaveCSV 内部会确保目录存在）
                CSVTool.SaveCSV(records, tmp, _encoding);

                // 原子替换：原文件存在则 Replace，不存在（首次）则 Move
                if (File.Exists(filePath))
                    File.Replace(tmp, filePath, null);
                else
                    File.Move(tmp, filePath);

                return true;
            }
            catch
            {
                // 清理可能残留的临时文件
                if (tmp != null && File.Exists(tmp))
                {
                    try { File.Delete(tmp); } catch { /* 忽略清理失败 */ }
                }
                return false;
            }
        }

        /// <summary>
        /// 保存SN：若不存在则新增(未使用)，已存在则不重复写入。
        /// 锁内原子：读-判重-追加-写。
        /// </summary>
        /// <returns>1=新增成功 3=文件处理失败 6=写入失败 8=SN已存在 9=未知异常</returns>
        public static int SaveSN(string filePath, string sn)
        {
            lock (_fileLock)
            {
                try
                {
                    if (!EnsureFileExists(filePath))
                        return 3;

                    var records = ReadAllRecords(filePath) ?? new List<CableSNRecord>();
                    if (records.Any(r => r.SN == sn))
                        return 8;

                    records.Add(new CableSNRecord
                    {
                        Time = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                        SN = sn,
                        IsUsed = StateUnused
                    });

                    return SaveAllRecords(filePath, records) ? 1 : 6;
                }
                catch
                {
                    return 9;
                }
            }
        }

        /// <summary>
        /// 读取最早未使用SN并原子预占（未使用→预占），消除多工站并发重复读取。
        /// 锁内原子：读-找最早未使用-改预占-写。
        /// 不刷新 Time：保持入库时间，维持 FIFO 排序语义（预占→归还循环后仍按入库顺序）。
        /// </summary>
        /// <param name="filePath">CSV文件完整路径</param>
        /// <param name="sn">输出：被预占的SN；非成功时为空</param>
        /// <returns>1=成功(sn已输出) 3=文件不存在 4=无数据 6=写入失败 7=无可用SN(均已使用/预占) 9=未知异常</returns>
        public static int OccupyEarliestSN(string filePath, out string sn)
        {
            sn = string.Empty;
            lock (_fileLock)
            {
                try
                {
                    filePath = NormalizePath(filePath);
                    if (!File.Exists(filePath))
                        return 3;

                    var records = ReadAllRecords(filePath);
                    if (records == null || records.Count == 0)
                        return 4;

                    var target = records.OrderBy(r => r.Time).FirstOrDefault(r => r.IsUsed == StateUnused);
                    if (target == null)
                        return 7;

                    target.IsUsed = StateOccupied;

                    if (!SaveAllRecords(filePath, records))
                        return 6;

                    sn = target.SN;
                    return 1;
                }
                catch
                {
                    sn = string.Empty;
                    return 9;
                }
            }
        }

        /// <summary>
        /// 标记指定SN为已使用（未使用/预占→已使用）；已是已使用则幂等成功。
        /// 锁内原子：读-找-改-写。刷新Time记录使用时刻（该SN不再回未使用态，不影响FIFO）。
        /// </summary>
        /// <param name="filePath">CSV文件完整路径</param>
        /// <param name="sn">排线SN码</param>
        /// <param name="alreadyUsed">输出：标记前该SN是否已为已使用（供调用方区分日志）</param>
        /// <returns>1=成功(含幂等) 3=文件不存在 4=无数据 5=SN不存在 6=写入失败 9=未知异常</returns>
        public static int MarkUsedSN(string filePath, string sn, out bool alreadyUsed)
        {
            alreadyUsed = false;
            lock (_fileLock)
            {
                try
                {
                    filePath = NormalizePath(filePath);
                    if (!File.Exists(filePath))
                        return 3;

                    var records = ReadAllRecords(filePath);
                    if (records == null || records.Count == 0)
                        return 4;

                    var target = records.FirstOrDefault(r => r.SN == sn);
                    if (target == null)
                        return 5;

                    if (target.IsUsed == StateUsed)
                    {
                        alreadyUsed = true;
                        return 1; // 幂等
                    }

                    target.IsUsed = StateUsed;
                    target.Time = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

                    return SaveAllRecords(filePath, records) ? 1 : 6;
                }
                catch
                {
                    return 9;
                }
            }
        }

        /// <summary>
        /// 归还预占的SN（预占/未使用→未使用）；若SN已为已使用则失败(码10)。
        /// 锁内原子：读-找-判态-改-写。用于配方异常分支回收预占。
        /// 不刷新 Time：归还的SN保持原始入库时间，确保后续读取仍按入库FIFO顺序分配。
        /// </summary>
        /// <param name="filePath">CSV文件完整路径</param>
        /// <param name="sn">排线SN码</param>
        /// <returns>1=归还成功 3=文件不存在 4=无数据 5=SN不存在 6=写入失败 9=未知异常 10=已使用不可归还</returns>
        public static int ReleaseSN(string filePath, string sn)
        {
            lock (_fileLock)
            {
                try
                {
                    filePath = NormalizePath(filePath);
                    if (!File.Exists(filePath))
                        return 3;

                    var records = ReadAllRecords(filePath);
                    if (records == null || records.Count == 0)
                        return 4;

                    var target = records.FirstOrDefault(r => r.SN == sn);
                    if (target == null)
                        return 5;

                    if (target.IsUsed == StateUsed)
                        return 10; // 已使用不可归还

                    target.IsUsed = StateUnused;

                    return SaveAllRecords(filePath, records) ? 1 : 6;
                }
                catch
                {
                    return 9;
                }
            }
        }
    }
}
