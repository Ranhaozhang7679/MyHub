#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ObjectExtension
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Common.Extensions
* 文 件 名:       ObjectExtension
* 创建时间:       2021/10/31 16:15:48
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      2cf17f66-5189-4015-b4df-138caee9d8b5
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/10/31 16:15:48
* 修 改 人:		  luster
************************************************************************************/

#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.Common.DataStruct.Extensions
{
    public static class ObjectExtension
    {
        public static object ConvertTo(this object value, Type dstType)
        {
            if (value != null)
            {
                var sourceType = value.GetType();

                var destinationConverter = TypeDescriptor.GetConverter(dstType);
                if (destinationConverter != null && destinationConverter.CanConvertFrom(value.GetType()))
                    return destinationConverter.ConvertFrom(value);

                var sourceConverter = TypeDescriptor.GetConverter(sourceType);
                if (sourceConverter != null && sourceConverter.CanConvertTo(dstType))
                    return sourceConverter.ConvertTo(value, dstType);

                if (dstType.IsEnum && value is int)
                    return Enum.ToObject(dstType, (int)value);

                if (!dstType.IsInstanceOfType(value))
                    return Convert.ChangeType(value, dstType);
            }

            return value;
        }


        public static string GetDescription(this object value)
        {
            if (value == null)
            {
                return String.Empty;
            }

            if (value.GetType().IsEnum)
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

            return value?.ToString();
        }

        /// <summary>
        /// 将对象转换为Xml对象
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="rootName"></param>
        /// <returns></returns>
        public static XElement ToXml(this object obj, string rootName = "", params string[] igonreNames)
        {
            Type type = obj.GetType();
            if (string.IsNullOrEmpty(rootName))
            {
                rootName = type.Name;
            }

            XElement xRoot = new XElement(rootName);
            foreach (var item in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                // 1.忽略没有Set方法的属性
                if (item.GetSetMethod() == null) continue;

                // 2.忽略Ignore有属性
                var igAttr = item.GetCustomAttribute<IgnoreAttribute>();
                if (igAttr != null) continue;

                // 3.忽略特定的属性名称
                string propName = item.Name;
                if (igonreNames.Contains(propName)) continue;

                XElement xItem = new XElement(propName);
                var xVal = item.GetValue(obj, null);
                var pType = item.PropertyType;
                if (pType.IsEnum)
                {
                    xItem.SetValue(xVal.ToString());
                }
                else if (typeof(IXMLParser).IsAssignableFrom(pType))
                {
                    if (xVal != null && xVal is IXMLParser xParser)
                    {
                        var newXml = xParser.ExportXml();
                        foreach (var xChild in newXml.Elements())
                        {
                            xItem.Add(xChild);
                        }

                        xItem.SetAttributeValue("RealType", xVal.GetType().Name);
                        foreach (var xAttr in newXml.Attributes())
                        {
                            xItem.SetAttributeValue(xAttr.Name, xAttr.Value);
                        };
                    }
                }
                else
                {
                    if (xVal != null)
                        xItem.SetValue(xVal.ToString());
                }

                xRoot.Add(xItem);
            }

            return xRoot;
        }

        /// <summary>
        /// 通过Xml来解析对象
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="xRoot"></param>
        public static void FromXml(this object obj, XElement xRoot)
        {
            var type = obj.GetType();

            foreach (var xItem in xRoot.Elements())
            {
                var prop = type.GetProperty(xItem.Name.ToString());
                if (prop == null || prop.GetSetMethod() == null) continue;

                // 实现忽略特性的，也不进行对象解析
                if (prop.GetCustomAttribute<IgnoreAttribute>() != null) continue;

                var pType = prop.PropertyType;
                if (pType.IsEnum)
                {
                    try
                    {
                        prop.SetValue(obj, Enum.Parse(pType, xItem.Value));
                    }
                    catch
                    {
                    }
                }
                else if (typeof(IXMLParser).IsAssignableFrom(pType))
                {
                    var cType = pType;

                    // 如果是接口或者抽象类
                    if (cType.IsInterface || cType.IsAbstract)
                    {
                        // 获取真实类型
                        string realType = "";
                        xItem.GetAttribute("RealType", (rType) => realType = rType);
                        if (string.IsNullOrEmpty(realType)) continue;
                        cType = realType.ConvertToType();
                    }

                    if (cType == null) continue;

                    // 通过真实类型
                    var newObj = Activator.CreateInstance(cType) as IXMLParser;
                    if (newObj != null)
                    {
                        newObj.ParserXml(xItem);
                        prop.SetValue(obj, newObj);
                    }
                }
                else if (pType == typeof(Guid))
                {
                    prop.SetValue(obj, Guid.Parse(xItem.Value));
                }
                else
                {
                    try
                    {
                        prop.SetValue(obj, Convert.ChangeType(xItem.Value, pType));
                    }
                    catch (Exception)
                    {
                        // 忽略转化失败异常
                    }

                }
            }
        }

        /// <summary>
        /// 将对象转换为指定类型对象
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object ConvertToType(this object obj, Type pType)
        {
            if (pType.IsEnum)
            {
                return Enum.Parse(pType, obj?.ToString());
            }
            else if (pType == typeof(Guid))
            {
                return Guid.Parse(obj?.ToString());
            }
            else if (pType.IsNumeric())
            {
                return Convert.ChangeType(obj, pType);
            }
            else if (pType == typeof(string))
            {
                return obj.ToString();
            }
            else if (pType == typeof(bool))
            {
                if (bool.TryParse(obj?.ToString(), out var isOk))
                {
                    return isOk;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return obj;
            }
        }

        /// <summary>
        /// 通用泛型转换
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="input">输入值</param>
        public static T To<T>(this object input)
        {
            if (input == null)
                return default(T);
            if (input is string && string.IsNullOrWhiteSpace(input.ToString()))
                return default(T);
            Type type = typeof(T);
            var typeName = type.Name.ToLower();
            try
            {
                if (typeName == "string")
                    return (T)(object)input.ToString();
                if (typeName == "guid")
                    return (T)(object)new Guid(input.ToString());
                if (type.IsEnum)
                    return input.Parse<T>();
                if (input is IConvertible)
                    return (T)System.Convert.ChangeType(input, type);
                return (T)input;
            }
            catch
            {
                return default(T);
            }
        }

        public static TEnum Parse<TEnum>(this object member)
        {
            string value = member.SafeString();
            if (string.IsNullOrWhiteSpace(value))
            {
                if (typeof(TEnum).IsGenericType)
                    return default(TEnum);
                throw new ArgumentNullException(nameof(member));
            }
            return (TEnum)System.Enum.Parse(typeof(TEnum), value, true);
        }
    }
}