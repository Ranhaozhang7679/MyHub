using System;
using System.IO;

namespace Luster.Module.Motion.Production.Trace
{
    /// <summary>
    /// 检测图片归档服务（TES-33 P8-B）。
    /// 检测图片按 SN/时间归档到磁盘，路径对齐 <c>ProductInfo.ImagePath</c> / <c>TbAOIImage.Img_Raw</c>。
    /// </summary>
    /// <remarks>
    /// 真实相机采图链路依赖 P8-D 视觉，软件层先打通"保存/浏览/路径对齐"逻辑：
    /// 本服务接收原始图片字节 + SN，按 <c>{baseDir}/{date}/{SN}_{timestamp}.{ext}</c> 归档，
    /// 返回路径供 <c>ProductInfo.ImagePath</c> / <c>TbAOIImage</c> 持久化。真实采图 ⚠️ 待 P8-D + 现场。
    /// </remarks>
    public class ImageArchiveService
    {
        private readonly string _baseDir;

        public ImageArchiveService(string baseDir = null)
        {
            _baseDir = string.IsNullOrEmpty(baseDir)
                ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AOIImages")
                : baseDir;
        }

        /// <summary>归档原始图片，返回归档路径（null=失败）</summary>
        /// <param name="sn">产品 SN</param>
        /// <param name="imageData">图片字节</param>
        /// <param name="extension">扩展名（如 .jpg）</param>
        /// <param name="capturedAt">采图时间（可空，默认现在）</param>
        public string Archive(string sn, byte[] imageData, string extension = ".jpg", DateTime? capturedAt = null)
        {
            if (string.IsNullOrEmpty(sn) || imageData == null || imageData.Length == 0) return null;

            DateTime time = capturedAt ?? DateTime.Now;
            string dateDir = Path.Combine(_baseDir, time.ToString("yyyyMMdd"));
            try
            {
                if (!Directory.Exists(dateDir)) Directory.CreateDirectory(dateDir);
                // 文件名净化：SN 可能含非法字符
                string safeSn = string.Concat(sn.Split(Path.GetInvalidFileNameChars()));
                string fileName = $"{safeSn}_{time:HHmmss_fff}{extension}";
                string fullPath = Path.Combine(dateDir, fileName);

                File.WriteAllBytes(fullPath, imageData);
                return fullPath;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>查询某 SN 的图片目录（浏览用，返回该 SN 所有归档文件路径）</summary>
        public string[] ListImages(string sn)
        {
            if (string.IsNullOrEmpty(sn)) return new string[0];
            string safeSn = string.Concat(sn.Split(Path.GetInvalidFileNameChars()));
            try
            {
                var result = new System.Collections.Generic.List<string>();
                if (!Directory.Exists(_baseDir)) return result.ToArray();
                foreach (var dateDir in Directory.GetDirectories(_baseDir))
                {
                    foreach (var f in Directory.GetFiles(dateDir, $"{safeSn}_*"))
                    {
                        result.Add(f);
                    }
                }
                return result.ToArray();
            }
            catch
            {
                return new string[0];
            }
        }

        /// <summary>构造归档路径（纯逻辑，便于单测，不实际写盘）</summary>
        public static string BuildArchivePath(string baseDir, string sn, DateTime capturedAt, string extension)
        {
            string safeSn = string.IsNullOrEmpty(sn) ? "unknown" : string.Concat(sn.Split(Path.GetInvalidFileNameChars()));
            string ext = string.IsNullOrEmpty(extension) ? ".jpg" : extension;
            return Path.Combine(baseDir ?? "", capturedAt.ToString("yyyyMMdd"),
                $"{safeSn}_{capturedAt:HHmmss_fff}{ext}");
        }
    }
}
