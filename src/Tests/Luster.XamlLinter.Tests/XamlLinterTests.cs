using Luster.XamlLinter;
using System.Linq;
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

        [Fact]
        public void HardcodedColor_Hex_ReportsMedium()
        {
            string xaml = @"<UserControl xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
                <TextBlock Background=""#1ba1e2""/>
            </UserControl>";
            var report = XamlLinter.Lint(xaml, "V");
            Assert.Contains(report.Issues, i => i.Rule == "hardcoded-color" && i.Severity == "medium");
        }

        [Fact]
        public void HardcodedColor_StaticResource_NoIssue()
        {
            string xaml = @"<UserControl xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
                <TextBlock Background=""{StaticResource PrimaryBrush}""/>
            </UserControl>";
            var report = XamlLinter.Lint(xaml, "V");
            Assert.DoesNotContain(report.Issues, i => i.Rule == "hardcoded-color");
        }

        [Fact]
        public void HardcodedSize_PixelValue_ReportsMedium()
        {
            string xaml = @"<UserControl xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
                <TextBlock Height=""30"" Width=""80""/>
            </UserControl>";
            var report = XamlLinter.Lint(xaml, "V");
            Assert.Contains(report.Issues, i => i.Rule == "hardcoded-size");
        }

        [Fact]
        public void HardcodedSize_Binding_NoIssue()
        {
            string xaml = @"<UserControl xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
                <TextBlock Height=""{Binding H}""/>
            </UserControl>";
            var report = XamlLinter.Lint(xaml, "V");
            Assert.DoesNotContain(report.Issues, i => i.Rule == "hardcoded-size");
        }

        [Fact]
        public void DesignAttribute_Skipped()
        {
            // d:DesignHeight 是设计时属性,不报 hardcoded-size
            string xaml = @"<UserControl xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
                xmlns:d=""http://schemas.microsoft.com/expression/blend/2008""
                xmlns:mc=""http://schemas.openxmlformats.org/markup-compatibility/2006""
                mc:Ignorable=""d""
                d:DesignHeight=""450"">
                <Grid></Grid>
            </UserControl>";
            var report = XamlLinter.Lint(xaml, "V");
            Assert.DoesNotContain(report.Issues, i => i.Rule == "hardcoded-size");
        }

        [Fact]
        public void HardcodedSize_MultiValue_Reports()
        {
            // 契约 §2.4 原例:Padding="5,2" / Margin="5,2,5,2" / CornerRadius="4,2,4,2" 也是写死值,应报 hardcoded-size
            string xaml = @"<UserControl xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
                <Border Padding=""5,2"" Margin=""5,2,5,2"" CornerRadius=""4,2,4,2""/>
            </UserControl>";
            var report = XamlLinter.Lint(xaml, "V");
            // 三处写死尺寸都应报(Padding/Margin/CornerRadius)
            Assert.Contains(report.Issues, i => i.Rule == "hardcoded-size");
            Assert.True(report.Issues.Where(i => i.Rule == "hardcoded-size").Count() >= 3,
                $"应至少报 3 个 hardcoded-size(Padding/Margin/CornerRadius),实际 {report.Issues.Where(i => i.Rule == "hardcoded-size").Count()}");
        }

        [Fact]
        public void InlineStyle_Reported()
        {
            string xaml = @"<UserControl xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
                <UserControl.Resources>
                    <Style TargetType=""Button""/>
                </UserControl.Resources>
                <Grid></Grid>
            </UserControl>";
            var report = XamlLinter.Lint(xaml, "V");
            Assert.Contains(report.Issues, i => i.Rule == "inline-style");
        }

        [Fact]
        public void FontSize_OutOfTier_ReportsLow()
        {
            string xaml = @"<UserControl xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
                <TextBlock FontSize=""16""/>
            </UserControl>";
            var report = XamlLinter.Lint(xaml, "V");
            Assert.Contains(report.Issues, i => i.Rule == "font-size-tier" && i.Severity == "low");
        }

        [Fact]
        public void FontSize_InTier_NoIssue()
        {
            string xaml = @"<UserControl xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
                <TextBlock FontSize=""14""/>
            </UserControl>";
            var report = XamlLinter.Lint(xaml, "V");
            Assert.DoesNotContain(report.Issues, i => i.Rule == "font-size-tier");
        }

        [Fact]
        public void Issue_HasLineNumber()
        {
            // 裸控件应报出真实行号(Location 非 L0)
            string xaml = @"<UserControl xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
                <Grid>
                    <Button Content=""ok""/>
                </Grid>
            </UserControl>";
            var report = XamlLinter.Lint(xaml, "V");
            var issue = Assert.Single(report.Issues);
            // Location 应为 "L<行号>",行号 > 0(Button 在第 4 行附近)
            Assert.StartsWith("L", issue.Location);
            var lineStr = issue.Location.Substring(1);
            int line = int.Parse(lineStr);
            Assert.True(line > 0, $"行号应 > 0,实际 {line}");
        }
    }
}
