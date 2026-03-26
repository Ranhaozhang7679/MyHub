using Luster.Common.DataStruct;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Luster.Common.Assets.Converter
{
    /// <summary>
    /// 角色可见性，控制UI是否可见
    /// </summary>
    public class RoleVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return Visibility.Visible;

            int curVal = (int)(SystemRole)value;
            if (Enum.TryParse<SystemRole>(parameter?.ToString(), out var needValue))
            {
                return curVal <= (int)needValue ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 角色启用和禁用，控制UI是否能够使用
    /// </summary>
    public class RoleEnabledCoverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return true;

            int curVal = (int)(SystemRole)value;

            if (Enum.TryParse<SystemRole>(parameter?.ToString(), out var needValue))
            {
                return curVal <= (int)needValue ;
            }
            else
            {
                return true;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
