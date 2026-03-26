using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Luster.Control.Wpf.Motion.Editors
{
    public class ReadOnlyTextEditor : ParamEditorBase
    {
        public override FrameworkElement CreateElement(ParamItem propertyItem)
        {
            return new TextBox()
            {
                IsEnabled = false,
            };
        }

        public override DependencyProperty GetDependencyProperty()
        {
            return System.Windows.Controls.TextBox.TextProperty;
        }

        protected override IValueConverter GetConverter(ParamItem propertyItem)
        {
            if (propertyItem.PropertyType == typeof(DateTime))
            {
                return new DateTimeConverter();
            }
            else
            {
                return base.GetConverter(propertyItem);
            }
        }
    }

    class DateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;
            if (value is DateTime dt)
            {
                return dt.ToString("yyyy-MM-dd HH:mm:ss:fff");
            }
            else
            {
                return value.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}