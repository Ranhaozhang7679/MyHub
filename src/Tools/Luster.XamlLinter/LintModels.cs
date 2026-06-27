using System.Collections.Generic;

namespace Luster.XamlLinter
{
    /// <summary>XAML 静态检查发现的单条源码级问题</summary>
    public sealed class LintIssue
    {
        /// <summary>严重度: high / medium / low</summary>
        public string Severity;

        /// <summary>规则名: bare-control / hardcoded-color / hardcoded-size / inline-style / font-size-tier</summary>
        public string Rule;

        /// <summary>问题描述(含修复建议)</summary>
        public string Description;

        /// <summary>位置: 行号,格式 "L&lt;行号&gt;"</summary>
        public string Location;
    }

    /// <summary>XAML 静态检查报告</summary>
    public sealed class LintReport
    {
        public string View;
        public string Xaml;
        public string Summary;
        public int IssueCount;
        public List<LintIssue> Issues = new List<LintIssue>();
    }
}
