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

namespace Luster.Common.Assets.Views
{
    /// <summary>
    /// MsgTipDialogContent.xaml 的交互逻辑
    /// </summary>
    public partial class MessageDialog : UserControl
    {
        public MessageDialog()
        {
            InitializeComponent();

            this.Loaded -= MessageDialog_Loaded;
            this.Loaded += MessageDialog_Loaded;
        }

        private void MessageDialog_Loaded(object sender, RoutedEventArgs e)
        {
            BtnYes.Focus();
            // 获取宿主该 UserControl 的真实弹窗 Window (通常是 Luster.Common.Assets.Views.DialogWindow)
            System.Windows.Window win = System.Windows.Window.GetWindow(this); // 修复命名空间冲突
            if (win != null)
            {
                win.Topmost = true;
            }
        }
    }
}