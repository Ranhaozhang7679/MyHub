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

namespace DC.Authorization.WPF.Views
{
    /// <summary>
    /// CreateAccountView.xaml 的交互逻辑
    /// </summary>
    public partial class CreateAccountView : UserControl
    {
        public CreateAccountView()
        {
            InitializeComponent();
        }

        private void PwdBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.CreateAccountViewModel vm)
                vm.Password = ((PasswordBox)sender).Password;
        }

        private void ConfirmPwdBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.CreateAccountViewModel vm)
                vm.CheckedPassword = ((PasswordBox)sender).Password;
        }
    }
}
