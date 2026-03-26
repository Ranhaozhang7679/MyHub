using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using HandyControl.Tools;

namespace Luster.Common.Assets.Converter
{
    public class String2BrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            BrushConverter brushConverter = new BrushConverter();
            if (value == null)
            {
                return new SolidColorBrush(Colors.Black);
            }

            return (Brush)brushConverter.ConvertFromString(value?.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return "Red";
        }
    }
}