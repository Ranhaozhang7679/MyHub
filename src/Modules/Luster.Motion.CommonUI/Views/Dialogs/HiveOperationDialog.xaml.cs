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
    /// HiveOperationDialog.xaml 的交互逻辑
    /// </summary>
    public partial class HiveOperationDialog : UserControl
    {
        public HiveOperationDialog()
        {
            InitializeComponent();
            this.Loaded += AlarmDialog_Loaded;
        }
        private void AlarmDialog_Loaded(object sender, RoutedEventArgs e)
        {
            // 获取宿主该 UserControl 的真实弹窗 Window (通常是 Luster.Common.Assets.Views.DialogWindow)
            Window win = Window.GetWindow(this);
            if (win != null)
            {
                //win.Topmost = true;
                win.Owner = Application.Current?.MainWindow;
            }
        }
    }
}
