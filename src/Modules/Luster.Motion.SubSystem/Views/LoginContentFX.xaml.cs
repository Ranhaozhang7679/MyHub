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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Luster.Motion.SubSystem.Views.Login
{
    /// <summary>
    /// LoginContentFX.xaml 的交互逻辑
    /// </summary>
    public partial class LoginContentFX : UserControl
    {
        public LoginContentFX()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginContentFXVM vModel)
            {
                vModel.FXPassword = password.Password;
                password.Password = string.Empty;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            tbCardID.Focus();
        }
    }
}
