using Luster.Motion.SubSystem.ViewModel;
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
using System.Windows.Shapes;

namespace Luster.Motion.SubSystem.Views
{
    /// <summary>
    /// ProTestBottonContent.xaml 的交互逻辑
    /// </summary>
    public partial class ProTestBottonContent : UserControl
    {
        public ProTestBottonContent()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                // 获取按钮名称并传递到 ViewModel
                var viewModel = DataContext as ProTestBottonContentVM;
                if (viewModel != null)
                {
                    viewModel.OnButtonClicked(button.Content.ToString());
                }
            }
        }
    }
}
