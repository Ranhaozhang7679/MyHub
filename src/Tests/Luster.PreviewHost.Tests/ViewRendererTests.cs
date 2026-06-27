using System;
using System.Threading;
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

        private static RenderRequest ThemedRequest() =>
            new RenderRequest
            {
                ViewTypeName = typeof(ThemedSampleView).AssemblyQualifiedName,
                DesignVmTypeName = "Luster.PreviewHost.Fixtures.ThemedSampleDesignVm",
                Width = 400,
                Height = 300
            };

        /// <summary>
        /// 在 STA 线程中执行动作,确保 ViewRenderer.Render 走"同线程渲染"路径
        /// (修复1:当前线程为 STA 时直接渲染,不开新线程)。
        /// 同时在该 STA 线程内加载 App.xaml 主题(模拟 Program.Main),
        /// 使引用主题资源的 View 不致因 Application.Current 为 null 而失败。
        /// </summary>
        private static void RunInStaWithTheme(Action action)
        {
            Exception err = null;
            var thread = new Thread(() =>
            {
                try
                {
                    // 加载主题:仅当当前进程未创建 Application 时才 new App(),避免重复实例化抛异常
                    if (System.Windows.Application.Current == null)
                    {
                        var app = new App();
                        app.InitializeComponent();
                    }
                    action();
                }
                catch (Exception ex) { err = ex; }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (err != null)
                throw err;
        }

        [Fact]
        public void Render_WithDesignVm_SucceedsAndProducesPng()
        {
            RunInStaWithTheme(() =>
            {
                var result = ViewRenderer.Render(SampleRequest());
                Assert.True(result.Success, result.Error ?? "");
                Assert.NotNull(result.PngBytes);
                Assert.True(result.PngBytes.Length > 0);
                Assert.True(result.DesignDataPresent);
            });
        }

        [Fact]
        public void Render_WithoutDesignVm_SucceedsButMarksMissing()
        {
            RunInStaWithTheme(() =>
            {
                var req = SampleRequest(designVm: null);
                var result = ViewRenderer.Render(req);
                Assert.True(result.Success, result.Error ?? "");
                Assert.False(result.DesignDataPresent);
            });
        }

        [Fact]
        public void Render_NonExistentView_ReturnsFailure()
        {
            RunInStaWithTheme(() =>
            {
                var req = SampleRequest();
                req.ViewTypeName = "Does.Not.Exist, NoAssembly";
                var result = ViewRenderer.Render(req);
                Assert.False(result.Success);
                Assert.False(string.IsNullOrEmpty(result.Error));
            });
        }

        [Fact]
        public void Render_ThemedSampleView_SucceedsAndRendersContent()
        {
            RunInStaWithTheme(() =>
            {
                var result = ViewRenderer.Render(ThemedRequest());
                Assert.True(result.Success, result.Error ?? "");
                Assert.NotNull(result.PngBytes);
                Assert.True(result.PngBytes.Length > 0);
                // 引用主题资源(PrimaryBrush + HandyControl Button)的 View 能成功渲染且
                // 产物非近空白:字节数应明显大于空 View 截图(约数百字节阈值)。
                Assert.True(result.PngBytes.Length > 1000,
                    "ThemedSampleView PNG 字节数过小(" + result.PngBytes.Length + "),疑似近空白渲染");
            });
        }
    }
}
