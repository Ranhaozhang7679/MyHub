using System.Xml;

namespace Luster.XamlLinter
{
    /// <summary>
    /// XAML 静态检查核心:用 XamlXmlReader 遍历 XAML 节点流,按 RuleConfig 规则匹配。
    /// 纯内存方法 Lint(string),不读文件,便于测试。
    /// </summary>
    public static class XamlLinter
    {
        public static LintReport Lint(string xamlContent, string viewName)
        {
            var report = new LintReport { View = viewName, Xaml = "(inline)" };
            using (var reader = new System.Xaml.XamlXmlReader(XmlReader.Create(
                new System.IO.StringReader(xamlContent))))
            {
                while (reader.Read())
                {
                    // 节点遍历在 Task 4 实现
                }
            }
            report.IssueCount = report.Issues.Count;
            report.Summary = report.IssueCount == 0
                ? "未发现源码级问题"
                : $"发现 {report.IssueCount} 个源码级问题";
            return report;
        }
    }
}
