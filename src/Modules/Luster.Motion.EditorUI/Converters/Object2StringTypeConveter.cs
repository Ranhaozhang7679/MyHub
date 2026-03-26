#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       StationCheckConverter
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.EditorUI.Converters
* 文 件 名:       StationCheckConverter.cs
* 创建时间:       2022/9/3 13:38:37
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      ed648edf-01d5-41bd-aefd-41f0dfdf2376
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/3 13:38:37
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Luster.Motion.EditorUI.Converters
{
    public class Object2StringTypeConveter : IMultiValueConverter
    {
        /// <summary>
        /// 数据类型
        /// </summary>
        private Type vType = null;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            vType = values[1] as Type;
            return values[0]?.ToString();
        }


        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            if (vType != null && value != null)
            {
                return new object[] { value.ConvertTo(vType), vType };
            }
            return null;
        }
    }
}