using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Luster.Motion.DigitalSetup.Datas;

namespace Luster.Motion.DigitalSetup.Converters
{
    /// <summary>
    /// 将 CheckStatus 枚举转换为对应的颜色画笔
    /// </summary>
    public class CheckStatusToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CheckStatus status)
            {
                return status switch
                {
                    CheckStatus.CheckedOK => new SolidColorBrush(Color.FromRgb(76, 175, 80)),    // 绿色
                    CheckStatus.CheckedFail => new SolidColorBrush(Color.FromRgb(244, 67, 54)),  // 红色
                    CheckStatus.NotChecked => new SolidColorBrush(Color.FromRgb(158, 158, 158)), // 灰色
                    _ => new SolidColorBrush(Color.FromRgb(158, 158, 158))                        // 默认灰色
                };
            }
            return new SolidColorBrush(Color.FromRgb(158, 158, 158));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
