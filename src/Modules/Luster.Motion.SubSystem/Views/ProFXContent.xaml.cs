using Luster.Motion.SubSystem.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Luster.Motion.SubSystem.Views
{
    /// <summary>
    /// UserDefineMainContent.xaml 的交互逻辑
    /// </summary>
    public partial class ProFXContent : UserControl
    {
        public ProFXContent()
        {
            InitializeComponent();
            btnOpen.Checked += BtnOpen_Checked;
            btnOpen.Unchecked += BtnOpen_Unchecked;

            BtnAutoMode.Checked += BtnAutoMode_Checked;
            BtnAutoMode.Unchecked += BtnAutoMode_Unchecked;
            //DataContext = new ProFXContentVM();

        }

        private void BtnAutoMode_Unchecked(object sender, RoutedEventArgs e)
        {
            BtnAutoMode.Content = "手动模式";
        }

        private void BtnAutoMode_Checked(object sender, RoutedEventArgs e)
        {
            BtnAutoMode.Content = "自动模式";
        }

        private void BtnTray_Unchecked(object sender, RoutedEventArgs e)
        {
            btnTray.Content = $"不带Tray盘空跑";
        }

        private void BtnTray_Checked(object sender, RoutedEventArgs e)
        {
            btnTray.Content = $"带Tray盘空跑";
        }

        private void BtnOpen_Unchecked(object sender, RoutedEventArgs e)
        {
            btnOpen.Content = $"一键开门";
        }

        private void BtnOpen_Checked(object sender, RoutedEventArgs e)
        {
            btnOpen.Content = $"一键关门";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null )
            {
                // 获取按钮名称并传递到 ViewModel
                var viewModel = DataContext as ProFXContentVM;
                if (viewModel != null)
                {
                    viewModel.OnButtonClicked(button.Content.ToString());
                }
            }
        }
    }
}
