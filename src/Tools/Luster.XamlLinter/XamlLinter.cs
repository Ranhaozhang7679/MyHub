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
                    if (reader.NodeType == System.Xaml.XamlNodeType.StartObject)
                    {
                        var xt = reader.Type;
                        // 裸控件:默认 presentation ns + LocalName 在禁裸清单
                        if (xt.PreferredXamlNamespace == RuleConfig.PresentationNs
                            && RuleConfig.BareControlNames.Contains(xt.Name))
                        {
                            int line = LineOf(reader);
                            report.Issues.Add(new LintIssue
                            {
                                Severity = "high",
                                Rule = "bare-control",
                                Description = $"裸 <{xt.Name}> 应改 hc:{xt.Name} 或 Luster.Controls.Wpf 封装",
                                Location = "L" + line
                            });
                        }
                    }
                }
            }
            report.IssueCount = report.Issues.Count;
            report.Summary = report.IssueCount == 0
                ? "未发现源码级问题"
                : $"发现 {report.IssueCount} 个源码级问题";
            return report;
        }

        private static int LineOf(System.Xaml.XamlReader reader)
        {
            var li = reader as System.Xaml.IXamlLineInfo;
            return (li != null && li.HasLineInfo) ? li.LineNumber : 0;
        }
    }
}
