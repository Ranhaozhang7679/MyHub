#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       TypeExtension
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Common.Extensions
* 文 件 名:       TypeExtension
* 创建时间:       2021/10/31 14:15:32
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      92ed6a0c-740d-4bb3-a998-df35ae7f4532
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/10/31 14:15:32
* 修 改 人:		  luster
************************************************************************************/

#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.DataStruct.Extensions
{
    public static class TypeExtension
    {
        public static bool IsNumeric(this Type dataType)
        {
            if (dataType == null)
                throw new ArgumentNullException("dataType");

            return (dataType == typeof(int)
                    || dataType == typeof(double)
                    || dataType == typeof(long)
                    || dataType == typeof(short)
                    || dataType == typeof(float)
                    || dataType == typeof(Int16)
                    || dataType == typeof(Int32)
                    || dataType == typeof(Int64)
                    || dataType == typeof(uint)
                    || dataType == typeof(UInt16)
                    || dataType == typeof(UInt32)
                    || dataType == typeof(UInt64)
                    || dataType == typeof(sbyte)
                    || dataType == typeof(Single)
                    || dataType == typeof(Boolean)
                   );
        }

        /// <summary>
        /// 构建数据源
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public static List<KeyValue> EnumToDataSource(this Type enumType)
        {
            List<KeyValue> textItems = new List<KeyValue>();
            foreach (var item in Enum.GetValues(enumType))
            {
                var field = item.GetType().GetField(item.ToString());
                var attr = field.GetCustomAttribute(typeof(IgnoreAttribute));
                if (attr != null) continue;
                textItems.Add(new KeyValue()
                {
                    Desc = item.GetDescription(),
                    Value = item,
                    Key = item.ToString()
                });
            };

            return textItems;
        }

        /// <summary>
        /// 是否实现泛型类
        /// </summary>
        /// <param name="type"></param>
        /// <param name="generic"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool HasImplementedRawGeneric(this Type type, Type generic)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            if (generic == null) throw new ArgumentNullException(nameof(generic));

            // 测试接口。
            var isTheRawGenericType = type.GetInterfaces().Any(IsTheRawGenericType);

            if (isTheRawGenericType) return true;

            // 测试类型。
            while (type != null && type != typeof(object))
            {

                isTheRawGenericType = IsTheRawGenericType(type);

                if (isTheRawGenericType) return true;

                type = type.BaseType;
            }

            // 没有找到任何匹配的接口或类型。
            return false;

            // 测试某个类型是否是指定的原始接口。

            bool IsTheRawGenericType(Type test)

            => generic == (test.IsGenericType ? test.GetGenericTypeDefinition() : test);
        }

        public static Type GetArrayGenericType(this Type type)
        {
            Type resType = null;

            if (type.HasImplementedRawGeneric(typeof(LArray<>)))
            {
                resType = type.BaseType.GenericTypeArguments[0];
            }

            return resType;
        }
    }
}