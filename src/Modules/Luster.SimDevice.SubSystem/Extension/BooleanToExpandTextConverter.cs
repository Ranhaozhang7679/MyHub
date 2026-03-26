using System;
using System.Globalization;
using System.Windows.Data;

namespace Luster.SimDevice.SubSystem.Extension
{
    public class BooleanToExpandTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isExpanded)
            {
                return isExpanded ? "收起" : "展开";
            }
            return "展开";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}