using Luster.XamlLinter;
using Xunit;

namespace Luster.XamlLinter.Tests
{
    public class XamlLinterTests
    {
        [Fact]
        public void Lint_EmptyGrid_NoIssue()
        {
            string xaml = @"<UserControl xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
                <Grid></Grid>
            </UserControl>";
            var report = XamlLinter.Lint(xaml, "EmptyView");
            Assert.Equal("EmptyView", report.View);
            Assert.Empty(report.Issues);
        }

        [Fact]
        public void BareControl_BareButton_ReportsHigh()
        {
            string xaml = @"<UserControl xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
                <Button Content=""ok""/>
            </UserControl>";
            var report = XamlLinter.Lint(xaml, "V");
            var issue = Assert.Single(report.Issues);
            Assert.Equal("high", issue.Severity);
            Assert.Equal("bare-control", issue.Rule);
        }

        [Fact]
        public void BareControl_HcButton_NoIssue()
        {
            string xaml = @"<UserControl xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
                xmlns:hc=""https://handyorg.github.io/handycontrol"">
                <hc:Button Content=""ok""/>
            </UserControl>";
            var report = XamlLinter.Lint(xaml, "V");
            Assert.Empty(report.Issues);
        }

        [Fact]
        public void CommentContainsButton_NotReported()
        {
            // 注释里的 <Button> 不应被报为裸控件(XamlXmlReader 节点流天然过滤注释)
            string xaml = @"<UserControl xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
                <!-- <Button> 这里是注释 -->
                <Grid></Grid>
            </UserControl>";
            var report = XamlLinter.Lint(xaml, "V");
            Assert.Empty(report.Issues);
        }
    }
}
