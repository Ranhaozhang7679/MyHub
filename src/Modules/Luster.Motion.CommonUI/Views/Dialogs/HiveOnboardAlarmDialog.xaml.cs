using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Luster.Motion.CommonUI.Views.Dialogs
{
    /// <summary>
    /// HiveStopDialog.xaml 的交互逻辑
    /// </summary>
    public partial class HiveOnboardAlarmDialog : UserControl
    {
        public HiveOnboardAlarmDialog()
        {
            InitializeComponent();
            Loaded += OnFirstLoaded;
        }

        private void OnFirstLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnFirstLoaded;   // 只执行一次

            // 向上找到外层窗口，关闭软件时，关闭该弹窗
            if (Window.GetWindow(this) is Window w && w.Owner == null)
                w.Owner = System.Windows.Application.Current.MainWindow;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // 找到承载本控件的窗口
            var win = Window.GetWindow(this);
            if (win != null)
            {
                win.Topmost = true;          // 强制置顶
                // 居中主窗口,没作用
                win.Owner = Application.Current.MainWindow;
            }
        }

        private CancellationTokenSource _cts;   // 用来在窗口关闭时停掉循环
        private async void OnLoadedAsync(object sender, RoutedEventArgs e)
        {
            var win = Window.GetWindow(this);
            if (win == null) return;

            _cts = new CancellationTokenSource();
            win.Closed += (_, __) => _cts.Cancel();   // 窗口关了立刻停循环

            try
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    // 3. 再置顶（如果窗口还活着）
                    if (win.IsLoaded)
                    {
                        //win.Owner = null;
                        win.Topmost = true;                       
                        win.CenterToOwner();
                        // 1. 先取消置顶
                        win.Topmost = false;
                    }
                    // 2. 等 30 秒
                    await Task.Delay(TimeSpan.FromSeconds(30), _cts.Token);
 
                }
            }
            catch (OperationCanceledException)
            {
                // 正常关闭， swallow
            }
        }
    }
}
