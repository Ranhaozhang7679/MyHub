#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       EnumExtension
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.Extensions
* 文 件 名:       EnumExtension.cs
* 创建时间:       2021/12/2 9:43:45
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      d17d886b-9663-45d0-9223-c38ed07bc6e8
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2021
* 修改时间:		  2021/12/2 9:43:45
* 修 改 人:		  L05123
************************************************************************************/

#endregion

using Luster.Common.DataStruct.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.DataStruct.Extensions
{
    public static class EnumExtension
    {
        /// <summary>
        /// 获取当前枚举变量的描述属性值
        /// </summary>
        /// <param name="value"></param>
        /// <returns>指定属性描述值</returns>
        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attr = field.GetCustomAttribute(typeof(DescriptionAttribute));
            var descrip = field.Name;
            if (attr != null)
            {
                descrip = (attr as DescriptionAttribute).Description;
            }
            return descrip;
        }

        /// <summary>
        /// 获取当前枚举变量类型的所有属性描述值
        /// </summary>
        /// <param name="value"></param>
        /// <returns>所有属性描述值</returns>
        public static List<string> GetDescriptions(this Enum value)
        {
            Type type = value.GetType();
            var fields = type.GetFields().Where(info => info.FieldType == type);
            List<string> descrips = new List<string>();
            foreach (var field in fields)
            {
                var attr = field.GetCustomAttribute(typeof(DescriptionAttribute));
                var descrip = field.Name;
                if (attr != null)
                {
                    descrip = (attr as DescriptionAttribute).Description;
                }
                descrips.Add(descrip);
            }
            return descrips;
        }
    }
}