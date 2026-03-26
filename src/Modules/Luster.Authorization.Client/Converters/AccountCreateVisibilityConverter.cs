using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace DC.Authorization.WPF.Converters
{
    public class AccountCreateVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
             bool isEdit = bool.Parse(value.ToString());
            if (isEdit)
            {
                return Visibility.Collapsed;
            }
            else { return Visibility.Visible; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
