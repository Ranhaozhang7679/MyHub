using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.Enums;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Common.Models;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.Module.Motion.DataProc.Functions
{
    /// <summary>
    /// 复制文件
    /// </summary>
    public class CopyFile : MotionFunction
    {

        [NotEmpty]
        [Parameter("要拷贝的文件夹", 1, CN = "源图片路径", CanRef = ParamRef.Ref)]
        public string SourcePath { get; set; }

        [NotEmpty]
        [Parameter("目标文件夹", 2, CN = "目标图片路径", CanRef = ParamRef.Ref)]
        public string DstPath { get; set; }

        [Parameter("拷贝结果", 10, CN = "拷贝文件结果", ParamType = ParamType.OUT)]
        public bool Result { get; set; }

        /// <summary>
        /// 匹配条件：文件名满足此条件才会被复制（支持 * 和 ? 通配符；为空表示不限制）
        /// </summary>
        [Parameter("匹配条件", 3, CN = "匹配条件", CanRef = ParamRef.Ref)]
        public string MatchPattern { get; set; }

        /// <summary>
        /// 筛选条件：文件名满足此条件将不被复制（支持 * 和 ? 通配符；为空表示不排除）
        /// </summary>
        [Parameter("筛选条件", 4, CN = "筛选条件", CanRef = ParamRef.Ref)]
        public string ExcludePattern { get; set; }

        /// <summary>
        /// 后缀名：仅复制指定后缀的文件，多个用逗号/分号分隔，例如 ".jpg,.png"；为空表示不限制
        /// </summary>
        [Parameter("后缀名", 5, CN = "后缀名", CanRef = ParamRef.Ref)]
        public string FileExtensions { get; set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        public CopyFile()
        {
            this.Icon = "\xe629";
            this.Tips = "复制文件";
        }

        /// <summary>
        /// 文件夹拷贝
        /// </summary>
        /// <param name="strSourceFolder">源文件夹</param>
        /// <param name="strDestFolder">目标文件夹</param>
        public static void CopyFolder(string strSourceFolder, string strDestFolder)
        {
            var ctx = BuildContext(null, null, null);
            int copied = 0, skipped = 0;
            CopyFolderCore(strSourceFolder, strDestFolder, ctx, null, 0, ref copied, ref skipped);
        }

        /// <summary>
        /// 文件夹拷贝（带过滤条件与日志回调）
        /// </summary>
        /// <param name="strSourceFolder">源文件夹</param>
        /// <param name="strDestFolder">目标文件夹</param>
        /// <param name="matchPattern">匹配条件（满足才复制），为空不限制</param>
        /// <param name="excludePattern">筛选条件（满足不复制），为空不排除</param>
        /// <param name="extensions">后缀名限制，多个用逗号/分号分隔，为空不限制</param>
        /// <param name="logger">日志回调，为 null 则不输出日志</param>
        /// <param name="sampleInterval">进度采样间隔，每处理 N 个文件输出一次累计进度；≤0 关闭进度日志</param>
        /// <returns>(copied, skipped) 复制与跳过的文件数</returns>
        public static (int copied, int skipped) CopyFolder(string strSourceFolder, string strDestFolder,
            string matchPattern, string excludePattern, string extensions,
            Action<LogType, string> logger = null, int sampleInterval = 100)
        {
            // 整棵目录树共享同一个 context，所有预编译/预解析只做一次
            var ctx = BuildContext(matchPattern, excludePattern, extensions);
            int copied = 0, skipped = 0;
            CopyFolderCore(strSourceFolder, strDestFolder, ctx, logger, sampleInterval, ref copied, ref skipped);
            return (copied, skipped);
        }

        /// <summary>
        /// 文件夹拷贝内部实现：使用预编译的 FilterContext 进行过滤判定，并通过 ref 累加全局计数用于采样日志
        /// </summary>
        private static void CopyFolderCore(string strSourceFolder, string strDestFolder,
            FilterContext ctx, Action<LogType, string> logger, int sampleInterval,
            ref int copied, ref int skipped)
        {
            if (!Directory.Exists(strDestFolder))
            {
                Directory.CreateDirectory(strDestFolder);
                logger?.Invoke(LogType.Info, $"创建目标文件夹：{strDestFolder}");
            }

            string[] entries = Directory.GetFileSystemEntries(strSourceFolder);

            foreach (string entry in entries)
            {
                // 直接通过 File.GetAttributes 获取属性，避免 FileInfo 的额外分配
                FileAttributes attrs = File.GetAttributes(entry);
                string name = Path.GetFileName(entry);
                string destPath = Path.Combine(strDestFolder, name);

                if ((attrs & FileAttributes.Directory) != 0)
                {
                    // 目录始终递归进入，过滤仅作用于文件
                    CopyFolderCore(entry, destPath, ctx, logger, sampleInterval, ref copied, ref skipped);
                }
                else
                {
                    string reason;
                    if (!ShouldCopyFile(name, ctx, out reason))
                    {
                        skipped++;
                    }
                    else
                    {
                        File.Copy(entry, destPath, true);
                        copied++;
                    }

                    // 采样进度日志：每 N 个文件输出一次累计统计（sampleInterval ≤ 0 关闭）
                    int total = copied + skipped;
                    if (sampleInterval > 0 && (total % sampleInterval) == 0)
                    {
                        logger?.Invoke(LogType.Info, $"进度：已处理 {total} 个（复制 {copied}，跳过 {skipped}）");
                    }
                }
            }
        }

        /// <summary>
        /// 构建过滤上下文（整棵目录树共享，避免递归重复解析）
        /// </summary>
        private static FilterContext BuildContext(string matchPattern, string excludePattern, string extensions)
        {
            var ctx = new FilterContext();

            if (!string.IsNullOrEmpty(extensions))
            {
                ctx.Extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (string ext in extensions.Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string e = ext.Trim();
                    if (string.IsNullOrEmpty(e)) continue;
                    if (!e.StartsWith(".")) e = "." + e;
                    ctx.Extensions.Add(e);
                }
            }

            if (!string.IsNullOrEmpty(matchPattern))
            {
                ctx.MatchRegex = WildcardToRegex(matchPattern);
                ctx.MatchPattern = matchPattern;
            }

            if (!string.IsNullOrEmpty(excludePattern))
            {
                ctx.ExcludeRegex = WildcardToRegex(excludePattern);
                ctx.ExcludePattern = excludePattern;
            }

            return ctx;
        }

        /// <summary>
        /// 判断文件是否满足复制条件，并通过 reason 返回跳过原因
        /// </summary>
        private static bool ShouldCopyFile(string fileName, FilterContext ctx, out string reason)
        {
            reason = null;

            // 1. 后缀过滤（HashSet O(1)）
            if (ctx.Extensions != null && ctx.Extensions.Count > 0)
            {
                string fileExt = Path.GetExtension(fileName);
                if (!ctx.Extensions.Contains(fileExt))
                {
                    reason = $"后缀不匹配（{fileExt}）";
                    return false;
                }
            }

            // 2. 匹配条件（满足才复制）
            if (ctx.MatchRegex != null && !ctx.MatchRegex.IsMatch(fileName))
            {
                reason = $"未命中匹配条件（{ctx.MatchPattern}）";
                return false;
            }

            // 3. 筛选条件（满足则不复制）
            if (ctx.ExcludeRegex != null && ctx.ExcludeRegex.IsMatch(fileName))
            {
                reason = $"命中筛选条件（{ctx.ExcludePattern}）";
                return false;
            }

            return true;
        }

        /// <summary>
        /// 将通配符（* 和 ?）一次性编译为正则；不含通配符则按子串匹配（前后补 .*）
        /// </summary>
        private static Regex WildcardToRegex(string pattern)
        {
            bool hasWildcard = pattern.IndexOf('*') >= 0 || pattern.IndexOf('?') >= 0;
            string body = hasWildcard
                ? "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$"
                : ".*" + Regex.Escape(pattern) + ".*";
            return new Regex(body, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        /// <summary>
        /// 过滤上下文：缓存预编译正则与后缀集合，整次拷贝共享
        /// </summary>
        private class FilterContext
        {
            public HashSet<string> Extensions;
            public Regex MatchRegex;
            public Regex ExcludeRegex;
            public string MatchPattern;
            public string ExcludePattern;
        }
        /// <summary>
        /// 运动
        /// </summary>
        /// <param name="errMsg">errMsg</param>
        /// <returns></returns>
        public override bool DoExcute(out string errMsg)
        {
            try
            {
                DirectoryInfo a = new DirectoryInfo(SourcePath);
                if (!a.Exists)
                {
                    Result = false;
                    MyOwner.OnLog(LogType.Error, $"源文件夹不存在：{SourcePath}");
                    errMsg = "源文件夹不存在";
                    return false;
                }

                // 构建过滤信息用于日志
                List<string> filters = new List<string>();
                if (!string.IsNullOrEmpty(FileExtensions)) filters.Add($"后缀[{FileExtensions}]");
                if (!string.IsNullOrEmpty(MatchPattern)) filters.Add($"匹配[{MatchPattern}]");
                if (!string.IsNullOrEmpty(ExcludePattern)) filters.Add($"排除[{ExcludePattern}]");
                string filterInfo = filters.Count > 0 ? "，过滤：" + string.Join("，", filters) : "";

                MyOwner.OnLog(LogType.Info, $"开始复制：{SourcePath} → {DstPath}{filterInfo}");

                // 目标文件夹不存在时由 CopyFolder 内部自动创建
                var (copied, skipped) = CopyFolder(SourcePath, DstPath,
                    MatchPattern, ExcludePattern, FileExtensions,
                    (type, msg) => MyOwner.OnLog(type, msg));

                MyOwner.OnLog(LogType.Info, $"复制完成：共复制 {copied} 个文件，跳过 {skipped} 个文件");
                Result = true;
                errMsg = null;
                return true;
            }
            catch (Exception ex)
            {
                Result = false;
                MyOwner.OnLog(LogType.Error, $"复制文件发生错误：{ex.Message}");
                errMsg = ex.Message;
                return false;
            }
        }

    }
}
