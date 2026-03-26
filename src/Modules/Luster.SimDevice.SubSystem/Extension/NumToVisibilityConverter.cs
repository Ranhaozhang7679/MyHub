#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       NumToVisiblityConverter
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.SubSystem.Extension
* 文 件 名:       NumToVisiblityConverter.cs
* 创建时间:       2022/7/16 22:53:46
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      79a24156-de80-438b-a6f1-897b85736d2f
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/16 22:53:46
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Luster.SimDevice.SubSystem.Extension
{
    public class NumToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (int.TryParse(value.ToString(), out int num) && int.TryParse(parameter?.ToString(), out var vNum))
            {
                if (num >= vNum)
                {
                    return System.Windows.Visibility.Visible;
                }
            }

            return System.Windows.Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}