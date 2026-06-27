using System;
using System.Threading.Tasks;
using Luster.VisualReviewer;
using Xunit;

namespace Luster.VisualReviewer.Tests
{
    public interface IVisualReviewClient
    {
        Task<ReviewReport> ReviewAsync(byte[] png, string contract, string viewName);
    }

    public class VisualReviewClientTests
    {
        private sealed class StubClient : IVisualReviewClient
        {
            public string? ReturnedJson;
            public Exception? ToThrow;
            public Task<ReviewReport> ReviewAsync(byte[] png, string contract, string viewName)
            {
                // 模拟 VisualReviewClient.Review 的降级语义:底层抛异常时降级为 Degraded 报告,不外抛
                try
                {
                    if (ToThrow != null) throw ToThrow;
                    return Task.FromResult(VisualReviewClient.ParseReport(ReturnedJson!, viewName));
                }
                catch (Exception ex)
                {
                    return Task.FromResult(new ReviewReport
                    {
                        View = viewName,
                        Degraded = true,
                        Summary = "视觉模型不可达: " + ex.Message,
                        Score = -1
                    });
                }
            }
        }

        [Fact]
        public async Task ParseReport_ValidJson_PopulatesFields()
        {
            var json = @"{""summary"":""布局清晰"",""score"":8,""issues"":[{""severity"":""high"",""category"":""overlap"",""description"":""控件重叠"",""location"":""右下""}]}";
            var stub = new StubClient { ReturnedJson = json };
            var report = await stub.ReviewAsync(new byte[0], "契约", "YyyView");
            Assert.Equal("YyyView", report.View);
            Assert.Equal(8, report.Score);
            Assert.Single(report.Issues);
            Assert.Equal("high", report.Issues[0].Severity);
        }

        [Fact]
        public async Task Review_NetworkFailure_Degrades()
        {
            var stub = new StubClient { ToThrow = new InvalidOperationException("网络不可达") };
            var report = await stub.ReviewAsync(new byte[0], "契约", "YyyView");
            Assert.True(report.Degraded);
            Assert.Empty(report.Issues);
        }
    }
}
