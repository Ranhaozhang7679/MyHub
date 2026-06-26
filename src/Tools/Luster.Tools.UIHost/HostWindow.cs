using System;
using System.Windows;
using Luster.Motion.EditorUI.Views;

namespace Luster.Tools.UIHost
{
    /// <summary>
    /// 最小 WPF 宿主：用于 TES-139 截图验证 FiveAxisManualControl 控件渲染效果。
    /// 启动后展示控件并设置标题，供 PowerShell 按 Title 截图。
    /// </summary>
    public class HostWindow : Window
    {
        public HostWindow()
        {
            Title = "TES-139 FiveAxisManualControl";
            Width = 360;
            Height = 440;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Content = new FiveAxisManualControl();
            Loaded += HostWindow_Loaded;
        }

        private void HostWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 渲染完成后写一个标记文件，供截图脚本确认窗口已就绪
            try
            {
                System.IO.File.WriteAllText("_uiready.txt", "ready");
            }
            catch { }
        }
    }
}
