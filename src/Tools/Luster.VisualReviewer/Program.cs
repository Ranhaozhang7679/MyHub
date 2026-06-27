using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Luster.VisualReviewer
{
    /// <summary>
    /// VisualReviewer 命令行入口:
    /// VisualReviewer.exe --screenshot &lt;png&gt; --report &lt;out.json&gt; [--view &lt;名&gt;]
    /// 评阅截图 → 写 JSON 报告 → 追加工作区 View 级 + 根级 index.md
    /// 退出码:0=成功,1=参数错误,2=视觉模型不可达(降级,仍落盘截图+报告)
    /// </summary>
    internal static class Program
    {
        private static int Main(string[] args)
        {
            // 参数解析:成对 --key value
            var map = new Dictionary<string, string>();
            for (int i = 0; i + 1 < args.Length; i += 2)
                map[args[i].TrimStart('-').ToLowerInvariant()] = args[i + 1];

            if (!map.ContainsKey("screenshot") || !map.ContainsKey("report"))
            {
                Console.Error.WriteLine(
                    "用法: VisualReviewer --screenshot <png> --report <out.json> " +
                    "[--view <名>]");
                return 1;
            }

            string shot = map["screenshot"];
            string reportPath = map["report"];
            string viewName = map.TryGetValue("view", out var v) ? v : Path.GetFileNameWithoutExtension(shot);

            // 安全修正:API key 仅从环境变量读取,缺失不 fallback 硬编码 key;
            // 空串会让 VisualReviewClient.CallModel 守卫抛异常 → Review 降级为 Degraded 报告(退出码 2)。
            string apiKey = Environment.GetEnvironmentVariable("SILICONFLOW_API_KEY") ?? "";

            try
            {
                byte[] png = File.ReadAllBytes(shot);

                var client = new VisualReviewClient(apiKey);
                var report = client.Review(png, viewName);
                report.Screenshot = shot;
                // 读 PreviewHost 落的 sidecar <png>.meta.json,据此设 DesignData(present/missing)。
                // 无 sidecar(旧截图/手制截图)时保持 "present"(向后兼容,不阻塞评阅)。
                report.DesignData = ReadDesignDataPresent(shot) ? "present" : "missing";

                // 落盘 JSON 报告(确保目录存在)
                Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(reportPath)));
                File.WriteAllText(reportPath,
                    JsonConvert.SerializeObject(report, Formatting.Indented));

                // 工作区索引(workspace/wpf-preview 在仓库根,相对当前工作目录)
                string wsRoot = Path.Combine(Directory.GetCurrentDirectory(), "workspace", "wpf-preview");
                WorkspaceIndexer.AppendView(wsRoot, viewName, report);
                WorkspaceIndexer.AppendRoot(wsRoot, viewName, report);

                if (report.Degraded)
                {
                    if (string.IsNullOrEmpty(apiKey))
                        Console.Error.WriteLine("未设置 SILICONFLOW_API_KEY 环境变量,已降级;截图与报告已落盘。");
                    else
                        Console.Error.WriteLine("视觉模型不可达,截图与报告已落盘,报告标 Degraded。");
                    return 2;
                }

                Console.WriteLine($"评阅完成: {viewName} 评分 {report.Score},问题 {report.Issues.Count} 项");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("VisualReviewer 异常: " + ex.GetType().Name + ": " + ex.Message);
                return 1;
            }
        }

        /// <summary>读 PreviewHost 落的 sidecar &lt;png&gt;.meta.json 的 DesignDataPresent;
        /// 无 sidecar 或解析失败返回 true(向后兼容,默认按有数据评阅,不阻塞)。</summary>
        private static bool ReadDesignDataPresent(string screenshotPath)
        {
            try
            {
                string metaPath = screenshotPath + ".meta.json";
                if (!File.Exists(metaPath)) return true;
                var jo = Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText(metaPath));
                return (bool)jo["DesignDataPresent"];
            }
            catch
            {
                return true;
            }
        }
    }
}
