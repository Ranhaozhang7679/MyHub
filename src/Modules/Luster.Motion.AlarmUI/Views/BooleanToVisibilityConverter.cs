using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Luster.Motion.AlarmUI.Views
{
    /// <summary>
    /// Convert bool to Visibility. If ConverterParameter is "True", logic is inverted.
    /// true -> Visible (default), false -> Collapsed
    /// </summary>
    public sealed class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool invert = string.Equals(parameter?.ToString(), "True", StringComparison.OrdinalIgnoreCase);
            bool b = false;
            if (value is bool bb) b = bb;
            else bool.TryParse(value?.ToString(), out b);

            if (invert) b = !b;
            return b ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool invert = string.Equals(parameter?.ToString(), "True", StringComparison.OrdinalIgnoreCase);
            bool b = value is Visibility v && v == Visibility.Visible;
            if (invert) b = !b;
            return b;
        }
    }
}
