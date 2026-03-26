using Luster.Common.DataStruct;
using Luster.Motion.CommonUI;
using Luster.Motion.SubSystem.ViewModel;
using Luster.Motion.TaskFlow.Engine.Models;
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
    /// LoginContent.xaml 的交互逻辑
    /// </summary>
    public partial class LoginContent : UserControl
    {
        public LoginContent()
        {
            InitializeComponent();

#if DEBUG
           // password.Password = "123456";
#endif

            this.Loaded -= LoginContent_Loaded;
            this.Loaded += LoginContent_Loaded;
        }

        private void LoginContent_Loaded(object sender, RoutedEventArgs e)
        {
            // 角色配置
            if (GlobalProperty.AutoLogin == SystemRole.Admin.ToString())
            {
                combUser.SelectedIndex = 0;
                //password.Password = "Luster@1996";
            }
        }

        /// <summary>
        /// 获取密码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginContentVM vModel)
            {
                vModel.PWText = password.Password;
                password.Password = string.Empty;
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HandyControl.Controls.ComboBox comboBox = sender as HandyControl.Controls.ComboBox;

            if (comboBox != null && password != null)
            {
                //    if (comboBox.SelectedIndex == 2)
                //    {
                //        password.Password = "123456";
                //    }
                //    else
                //    {
                //        password.Password = "";
                //    }
            }
        }
    }
}
