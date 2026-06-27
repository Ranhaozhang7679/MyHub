using Luster.PreviewHost;
using Luster.PreviewHost.Fixtures;
using Xunit;

namespace Luster.PreviewHost.Tests
{
    public class ViewRendererTests
    {
        // 用本程序集内的夹具 View,无需外部 assembly
        private static RenderRequest SampleRequest(string designVm = "Luster.PreviewHost.Fixtures.SampleDesignVm") =>
            new RenderRequest
            {
                ViewTypeName = typeof(SampleView).AssemblyQualifiedName,
                DesignVmTypeName = designVm,
                Width = 400,
                Height = 300
            };

        [Fact]
        public void Render_WithDesignVm_SucceedsAndProducesPng()
        {
            var result = ViewRenderer.Render(SampleRequest());
            Assert.True(result.Success, result.Error ?? "");
            Assert.NotNull(result.PngBytes);
            Assert.True(result.PngBytes.Length > 0);
            Assert.True(result.DesignDataPresent);
        }

        [Fact]
        public void Render_WithoutDesignVm_SucceedsButMarksMissing()
        {
            var req = SampleRequest(designVm: null);
            var result = ViewRenderer.Render(req);
            Assert.True(result.Success, result.Error ?? "");
            Assert.False(result.DesignDataPresent);
        }

        [Fact]
        public void Render_NonExistentView_ReturnsFailure()
        {
            var req = SampleRequest();
            req.ViewTypeName = "Does.Not.Exist, NoAssembly";
            var result = ViewRenderer.Render(req);
            Assert.False(result.Success);
            Assert.False(string.IsNullOrEmpty(result.Error));
        }
    }
}
