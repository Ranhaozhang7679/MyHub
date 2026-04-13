#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       PathConverter
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.Assets.FloatingInfo.Models
* 文 件 名:       PathConverter.cs
* 创建时间:       2026/04/02
* 作    者:       Luster
* 版    权:       <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 唯一标识：      a1b2c3d4-e5f6-7890-abcd-ef123456789b
* 创建年份:      2026
************************************************************************************/

#endregion

using System;
using System.IO;

namespace Luster.Common.Assets.FloatingInfo.Models
{
    /// <summary>
    /// 路径转换工具类，用于处理绝对路径和相对路径之间的转换
    /// </summary>
    public static class PathConverter
    {
        /// <summary>
        /// 检测是否为网络路径（UNC路径）
        /// </summary>
        /// <param name="path">要检测的路径</param>
        /// <returns>如果是UNC路径返回true，否则返回false</returns>
        public static bool IsUncPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            // 检查是否为UNC路径格式（\Server\Share\...）
            return path.StartsWith(@"\\", StringComparison.Ordinal);
        }

        /// <summary>
        /// 仅当路径在基准目录下时才转换为相对路径，否则保持绝对路径不变。
        /// 适用于路径可能位于配方目录之外（如CCD图片、中转数据）的场景。
        /// </summary>
        /// <param name="absolutePath">绝对路径</param>
        /// <param name="basePath">基准路径（通常是配方目录）</param>
        /// <returns>如果在基准目录下则返回相对路径，否则返回原绝对路径</returns>
        public static string ToRelativePathIfUnderBase(string absolutePath, string basePath)
        {
            if (string.IsNullOrEmpty(absolutePath) || string.IsNullOrEmpty(basePath))
                return absolutePath;

            if (IsUncPath(absolutePath))
                return absolutePath;

            if (!Path.IsPathRooted(absolutePath))
                return absolutePath;

            try
            {
                // 规范化路径用于比较
                string normPath = Path.GetFullPath(absolutePath);
                string normBase = Path.GetFullPath(basePath)
                    .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                    + Path.DirectorySeparatorChar;

                // 仅当路径在基准目录下时才转换
                if (!normPath.StartsWith(normBase, StringComparison.OrdinalIgnoreCase))
                    return absolutePath;

                return ToRelativePath(absolutePath, basePath);
            }
            catch
            {
                return absolutePath;
            }
        }

        /// <summary>
        /// 将绝对路径转换为相对路径
        /// </summary>
        /// <param name="absolutePath">绝对路径</param>
        /// <param name="basePath">基准路径</param>
        /// <returns>相对路径，如果转换失败返回原路径</returns>
        public static string ToRelativePath(string absolutePath, string basePath)
        {
            if (string.IsNullOrEmpty(absolutePath))
            {
                return absolutePath;
            }

            // 网络路径不进行转换
            if (IsUncPath(absolutePath))
            {
                return absolutePath;
            }

            try
            {
                // 尝试转换为相对路径
                var relativePath = GetRelativePath(basePath, absolutePath);
                return NormalizeSeparators(relativePath);
            }
            catch
            {
                // 转换失败时返回原路径
                return absolutePath;
            }
        }

        /// <summary>
        /// 将相对路径解析为绝对路径
        /// </summary>
        /// <param name="relativePath">相对路径</param>
        /// <param name="basePath">基准路径</param>
        /// <returns>绝对路径，如果解析失败返回原路径</returns>
        public static string ToAbsolutePath(string relativePath, string basePath)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                return relativePath;
            }

            // 如果已经是绝对路径或网络路径，直接返回
            if (Path.IsPathRooted(relativePath) || IsUncPath(relativePath))
            {
                return relativePath;
            }

            try
            {
                // 组合基准路径和相对路径
                var absolutePath = Path.Combine(basePath, relativePath);
                // 规范化路径
                return Path.GetFullPath(absolutePath);
            }
            catch
            {
                // 解析失败时返回原路径
                return relativePath;
            }
        }

        /// <summary>
        /// 标准化路径分隔符为正斜杠
        /// </summary>
        /// <param name="path">要标准化的路径</param>
        /// <returns>使用正斜杠的路径</returns>
        public static string NormalizeSeparators(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            // 将反斜杠替换为正斜杠
            return path.Replace("\\", "/");
        }

        /// <summary>
        /// 获取相对路径（兼容.NET Framework）
        /// </summary>
        /// <param name="relativeTo">基准路径</param>
        /// <param name="path">绝对路径</param>
        /// <returns>相对路径</returns>
        private static string GetRelativePath(string relativeTo, string path)
        {
            try
            {
                // .NET Framework兼容性处理
                // 使用Uri方式计算相对路径
                var fromUri = new Uri(relativeTo.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar);
                var toUri = new Uri(path);

                // 如果路径不在同一个根目录下，返回原路径
                if (fromUri.Scheme != toUri.Scheme)
                {
                    return path;
                }

                var relativeUri = fromUri.MakeRelativeUri(toUri);
                var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

                return relativePath.Replace('/', Path.DirectorySeparatorChar);
            }
            catch
            {
                return path;
            }
        }
    }
}
