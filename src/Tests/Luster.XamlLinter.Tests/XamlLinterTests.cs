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
    }
}
