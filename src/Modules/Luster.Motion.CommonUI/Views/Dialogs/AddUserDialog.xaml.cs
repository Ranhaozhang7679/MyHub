using Luster.Motion.CommonUI.ViewModel.Dialogs;
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
    /// AddUserDialog.xaml 的交互逻辑
    /// </summary>
    public partial class AddUserDialog : UserControl
    {
        public AddUserDialog()
        {
            InitializeComponent();
        }


        private void rePassword_PasswordChanged(object sender, RoutedEventArgs e)
        {

            if (DataContext is AddUserDialogVM vModel)
            {
                vModel.RepeatePassword = rePassword.Password;
            }
        }

        private void password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is AddUserDialogVM vModel) 
            {
                vModel.Password = password.Password;
            }
        }

        private void oldPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is AddUserDialogVM vModel)
            {
                vModel.OriginalPassword = oldPassword.Password;
            }
        }
    }
}
