#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       EnumLangConverter
* 机器名称:       L05123-02
* 命名空间:       Luster.Motion.CommonUI.Converter
* 文 件 名:       EnumLangConverter.cs
* 创建时间:       2022/12/19 11:20:43
* 作    者:       刘克志
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      895d24b9-9b0f-4cea-87de-2f54d0ee5dd6
* 登录用户:       刘克志
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/12/19 11:20:43
* 修 改 人:		  刘克志
************************************************************************************/
#endregion

using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Luster.Motion.CommonUI.Converter
{
    public class EnumLangConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            List<KeyValue> enums = value as List<KeyValue>;
            List<KeyValue> result = new List<KeyValue>();
            if (enums != null)
            {
                if (AppConfig.Lang == "en")
                {
                    enums.ForEach(u =>
                    {
                        u.Desc = u.Value.ToString();
                    });
                }
                else
                {
                    enums.ForEach(u =>
                    {
                        u.Desc = u.Value.GetDescription();
                    });
                }

                enums.ForEach(u => result.Add(u));
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}