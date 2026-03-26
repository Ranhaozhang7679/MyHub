using System;
using System.Collections.Generic;
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
    /// HiveAlarmDialog.xaml 的交互逻辑
    /// </summary>
    public partial class HiveAlarmDialog : UserControl
    {
        public HiveAlarmDialog()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            tbStartTime.Focus();
            //var win = Window.GetWindow(this);
            //if (win != null)
            //{
            //    win.Owner = Application.Current.MainWindow;
            //}

            //// 查找承载 HiveStartDialog2Part2 的窗口
            //Window hiveStartWindow = null;
            //foreach (Window w in Application.Current.Windows)
            //{
            //    if (w.Content is Luster.Motion.CommonUI.Views.Dialogs.HiveStartDialog2Part2)
            //    {
            //        hiveStartWindow = w;
            //        break;
            //    }
            //}

            //// 如果找到了，设置 HiveAlarmDialog 的窗口 Owner 为 HiveStartDialog2Part2 的窗口
            //if (win != null && hiveStartWindow != null)
            //{
            //    hiveStartWindow.Owner = win;
            //    // 让 HiveStartDialog2Part2 窗口激活（置顶）
            //    hiveStartWindow.Activate();
            //}
        }
    }
}
