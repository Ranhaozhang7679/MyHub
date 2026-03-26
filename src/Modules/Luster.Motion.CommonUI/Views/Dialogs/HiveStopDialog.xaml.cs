using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
    public partial class HiveStopDialog : UserControl
    {
        public HiveStopDialog()
        {
            InitializeComponent();
        }
        //2025-6-16 处理RequestNavigate事件以打开超链接对应的指定路径
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            // 使用 Process.Start 打开指定路径
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri)
            {
                UseShellExecute = true
            });

            // 标记事件已处理
            e.Handled = true;
        }
    }
}
