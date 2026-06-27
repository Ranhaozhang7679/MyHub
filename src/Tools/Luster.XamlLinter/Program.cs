using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Luster.XamlLinter
{
    /// <summary>
    /// XamlLinter 命令行入口:
    /// Luster.XamlLinter.exe --xaml &lt;path&gt; --report &lt;out.json&gt; [--view &lt;名&gt;]
    /// 读 XAML → 静态检查 → 写 JSON 报告
    /// 退出码:0=检查完成(可能有 issue,不阻塞);1=参数错误/XAML 解析失败
    /// </summary>
    internal static class Program
    {
        private static int Main(string[] args)
        {
            var map = new Dictionary<string, string>();
            for (int i = 0; i + 1 < args.Length; i += 2)
                map[args[i].TrimStart('-').ToLowerInvariant()] = args[i + 1];

            if (!map.ContainsKey("xaml") || !map.ContainsKey("report"))
            {
                Console.Error.WriteLine(
                    "用法: Luster.XamlLinter --xaml <View.xaml> --report <out.json> [--view <名>]");
                return 1;
            }

            string xamlPath = map["xaml"];
            string reportPath = map["report"];
            string viewName = map.TryGetValue("view", out var v) ? v : Path.GetFileNameWithoutExtension(xamlPath);

            try
            {
                string xamlContent = File.ReadAllText(xamlPath);
                var report = XamlLinter.Lint(xamlContent, viewName);
                report.Xaml = xamlPath;

                Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(reportPath)));
                File.WriteAllText(reportPath,
                    JsonConvert.SerializeObject(report, Formatting.Indented));

                Console.WriteLine($"静态检查完成: {viewName} 问题 {report.IssueCount} 项");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Luster.XamlLinter 异常: " + ex.GetType().Name + ": " + ex.Message);
                return 1;
            }
        }
    }
}
