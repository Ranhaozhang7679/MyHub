using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Luster.Motion.DigitalSetup.Converters
{
    public class StatusToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = value as string;
            if (!string.IsNullOrEmpty(status))
            {
                return status == "OK" ?
                    new SolidColorBrush(Color.FromRgb(200, 255, 200)) :  // 浅绿色
                    new SolidColorBrush(Color.FromRgb(255, 200, 200));   // 浅红色
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}