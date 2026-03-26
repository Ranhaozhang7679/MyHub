#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       Boolean2ColorCoverter
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.Assets.Converter
* 文 件 名:       Boolean2ColorCoverter.cs
* 创建时间:       2022/1/13 9:07:29
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      68a284a4-a70f-479a-81e8-9bb86277c5a1
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/1/13 9:07:29
* 修 改 人:		  L05123
************************************************************************************/

#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Luster.Common.Assets.Converter
{
    /// <summary>
    /// true 是绿色 false 是红色
    /// </summary>
    public class Boolean2PassColorCoverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (bool.TryParse(value?.ToString(), out var isTrue))
            {
                return (isTrue ? Brushes.Green : Brushes.Red);
            }

            return Brushes.Red;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return true;
        }
    }

    /// <summary>
    /// true是红色 false是绿色
    /// </summary>
    public class Boolean2RePassColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (bool.TryParse(value?.ToString(), out var isTrue))
            {
                return (isTrue ? Brushes.Red : Brushes.Green);
            }

            return Brushes.Green;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return true;
        }
    }
}