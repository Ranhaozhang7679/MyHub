using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Luster.PreviewHost
{
    public sealed class RenderRequest
    {
        public string ViewTypeName;       // AssemblyQualifiedName 或 "Full.Type, Asm"
        public string AssemblyPath;       // 可选:外部程序集路径,优先加载
        public string DesignVmTypeName;   // 可选:mock VM 类型全名
        public int Width = 1920;
        public int Height = 1080;
    }

    public sealed class RenderResult
    {
        public bool Success;
        public string Error;
        public byte[] PngBytes;
        public bool DesignDataPresent;
    }

    /// <summary>实例化 View + 设计时 VM,渲染到固定尺寸并截图为 PNG</summary>
    public static class ViewRenderer
    {
        public static RenderResult Render(RenderRequest req)
        {
            var result = new RenderResult();
            Exception workerError = null;
            // WPF 要求 STA 线程
            var thread = new Thread(() =>
            {
                try { RenderCore(req, result); }
                catch (Exception ex) { workerError = ex; }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (workerError != null)
            {
                result.Success = false;
                result.Error = workerError.GetType().Name + ": " + workerError.Message;
            }
            return result;
        }

        private static void RenderCore(RenderRequest req, RenderResult result)
        {
            // 1. 可选加载外部程序集
            if (!string.IsNullOrEmpty(req.AssemblyPath) && File.Exists(req.AssemblyPath))
                Assembly.LoadFrom(req.AssemblyPath);

            // 2. 实例化 View
            var viewType = Type.GetType(req.ViewTypeName);
            if (viewType == null)
            {
                result.Success = false;
                result.Error = "找不到 View 类型: " + req.ViewTypeName;
                return;
            }
            var view = Activator.CreateInstance(viewType) as FrameworkElement;
            if (view == null)
            {
                result.Success = false;
                result.Error = "View 类型不可实例化为 FrameworkElement: " + viewType.FullName;
                return;
            }

            // 3. 实例化设计时 VM(可选)
            object dc = null;
            if (!string.IsNullOrEmpty(req.DesignVmTypeName))
            {
                var vmType = Type.GetType(req.DesignVmTypeName);
                if (vmType != null)
                {
                    dc = Activator.CreateInstance(vmType);
                    result.DesignDataPresent = dc != null;
                }
            }
            view.DataContext = dc;

            // 4. 测量排列到固定尺寸
            view.Measure(new Size(req.Width, req.Height));
            view.Arrange(new Rect(0, 0, req.Width, req.Height));
            view.UpdateLayout();

            // 5. 截图
            var dpi = 96;
            var rtb = new RenderTargetBitmap(req.Width, req.Height, dpi, dpi, PixelFormats.Pbgra32);
            rtb.Render(view);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            using (var ms = new MemoryStream())
            {
                encoder.Save(ms);
                result.PngBytes = ms.ToArray();
            }
            result.Success = true;
        }
    }
}
