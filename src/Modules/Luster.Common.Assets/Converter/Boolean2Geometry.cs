#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       String2Geometry
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.Assets.Converter
* 文 件 名:       String2Geometry.cs
* 创建时间:       2021/11/19 21:11:37
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      806881c4-d100-44b4-a263-18bb30dc3b42
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/11/19 21:11:37
* 修 改 人:		  luster
************************************************************************************/

#endregion

using HandyControl.Tools;
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
    public class Boolean2Geometry : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
            {
                return null;
            }

            // 第一个值是真，第二个值对应假
            string[] vals = parameter.ToString().Split('_');

            if (bool.TryParse(value.ToString(), out bool b))
            {
                if (b)
                {
                    return ResourceHelper.GetResource<Geometry>($"{vals[0]}Geometry");
                }
                else
                {
                    return ResourceHelper.GetResource<Geometry>($"{vals[1]}Geometry");
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}