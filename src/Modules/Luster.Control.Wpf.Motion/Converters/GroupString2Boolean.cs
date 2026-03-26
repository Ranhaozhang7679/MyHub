#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       GroupString2Boolean
* 机器名称:       L05123-NB
* 命名空间:       Luster.Control.Wpf.Motion.Converters
* 文 件 名:       GroupString2Boolean.cs
* 创建时间:       2022/6/6 21:36:39
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      5906def7-fb0b-4a70-9ae7-fe2f346f13f9
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/6 21:36:39
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

namespace Luster.Control.Wpf.Motion.Converters
{
    public class GroupString2Boolean : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return false;

            string name = value.ToString();
            if (name.LastIndexOf("Parameter") >= 0 || name.Contains("Z"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return false;

            return true;
        }
    }
}