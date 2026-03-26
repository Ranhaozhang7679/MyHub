#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ProductStringConverter
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.SubSystem.Converter
* 文 件 名:       ProductStringConverter.cs
* 创建时间:       2022/9/7 13:07:41
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      5c1b473d-703f-4501-9673-c5d5cef592cb
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/7 13:07:41
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Luster.Motion.CommonUI.Converter
{
    public class ProductStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is ProductItemModel p)
            {
                if (p.IsToss)
                {
                    return "Toss";
                }
                else
                {
                    if (p.Result)
                    {
                        return "OK";
                    }
                    else
                    {
                        return "NG";
                    }
                }
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
