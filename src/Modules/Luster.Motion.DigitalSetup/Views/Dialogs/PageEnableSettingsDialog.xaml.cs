using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Luster.Motion.DigitalSetup.Views.Dialogs
{
    /// <summary>
    /// PageEnableSettingsDialog.xaml 的交互逻辑
    /// </summary>
    public partial class PageEnableSettingsDialog : UserControl
    {
        public PageEnableSettingsDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 阻止CheckBox点击事件冒泡到Expander头部，避免影响折叠/展开
        /// </summary>
        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }
    }

    /// <summary>
    /// 反向布尔值到可见性转换器（true = Collapsed, false = Visible）
    /// </summary>
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility != Visibility.Visible;
            }
            return false;
        }
    }
}
