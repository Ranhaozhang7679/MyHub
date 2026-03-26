using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Luster.Motion.DigitalSetup.Converters
{
    public class ChartHeightConverter : IValueConverter
    {
        // 当IsChartVisible为true时返回实际高度，否则返回0
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isVisible = value is bool b && b;
            // 可以通过parameter传递需要的高度，默认200
            double height = 200;
            if (parameter != null && double.TryParse(parameter.ToString(), out double paramHeight))
                height = paramHeight;
            return isVisible ? height : 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 通常不需要反向转换
            return Binding.DoNothing;
        }
    }
}
