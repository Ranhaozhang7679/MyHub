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
                // 状态机:StartMember 记录当前属性名/命名空间/行号,Value 据此判断写死值。
                // currentMemberName == null 表示当前属性应跳过(d: 设计时属性 / 非 XAML 标准属性)。
                string currentMemberName = null;
                string currentMemberNs = null;
                int currentLine = 0;

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
                    else if (reader.NodeType == System.Xaml.XamlNodeType.StartMember)
                    {
                        currentMemberName = reader.Member.Name;
                        // 依赖属性:DeclaringType 非 null,取其 ns;附加属性/普通属性退回 Member 自身 ns
                        currentMemberNs = reader.Member.DeclaringType != null
                            ? reader.Member.DeclaringType.PreferredXamlNamespace
                            : reader.Member.PreferredXamlNamespace;
                        currentLine = LineOf(reader);
                        // d: 设计时属性(d:DesignHeight 等)跳过,不报 hardcoded-*
                        if (currentMemberNs == RuleConfig.DesignNs)
                        {
                            currentMemberName = null;
                        }
                    }
                    else if (reader.NodeType == System.Xaml.XamlNodeType.Value && currentMemberName != null)
                    {
                        string val = (reader.Value ?? "").ToString().Trim();
                        // 标记扩展 {StaticResource}/{Binding}/{DynamicResource}/{x:Static} 不报
                        if (val.StartsWith("{")) { /* skip */ }
                        else if (RuleConfig.ColorProperties.Contains(currentMemberName) && IsHexColor(val))
                        {
                            report.Issues.Add(new LintIssue
                            {
                                Severity = "medium",
                                Rule = "hardcoded-color",
                                Description = $"{currentMemberName} 写死 {val},应 {{StaticResource}} 引用主题色键",
                                Location = "L" + currentLine
                            });
                        }
                        else if (RuleConfig.SizeProperties.Contains(currentMemberName) && IsNumericValue(val))
                        {
                            report.Issues.Add(new LintIssue
                            {
                                Severity = "medium",
                                Rule = "hardcoded-size",
                                Description = $"{currentMemberName} 写死 {val},应引用 Sizes.xaml Key",
                                Location = "L" + currentLine
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

        /// <summary>判断是否为 #hex 颜色值(支持 #RGB/#RRGGBB/#AARRGGBB)</summary>
        private static bool IsHexColor(string val)
            => System.Text.RegularExpressions.Regex.IsMatch(val, @"^#([0-9a-fA-F]{3}|[0-9a-fA-F]{6}|[0-9a-fA-F]{8})$");

        /// <summary>判断是否为裸数值或逗号分隔多值(像素值,如 30 / 1.5 / "5,2" / "5,2,5,2"),非 Auto/NaN/标记扩展</summary>
        private static bool IsNumericValue(string val)
            => System.Text.RegularExpressions.Regex.IsMatch(val, @"^-?[0-9]+(\.[0-9]+)?(,-?[0-9]+(\.[0-9]+)?)*$");
    }
}
