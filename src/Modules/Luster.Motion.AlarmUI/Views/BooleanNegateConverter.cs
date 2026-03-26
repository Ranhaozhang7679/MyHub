using System;
using System.Globalization;
using System.Windows.Data;

namespace Luster.Motion.AlarmUI.Views
{
    /// <summary>
    /// Boolean negate converter for bindings like RadioButton.IsChecked.
    /// </summary>
    public sealed class BooleanNegateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                return !b;
            }

            if (bool.TryParse(value?.ToString(), out var parsed))
            {
                return !parsed;
            }

            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                return !b;
            }

            if (bool.TryParse(value?.ToString(), out var parsed))
            {
                return !parsed;
            }

            return false;
        }
    }
}
