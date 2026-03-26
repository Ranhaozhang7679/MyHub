#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ParamItem
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.DataStruct.Attributes
* 文 件 名:       ParamItem.cs
* 创建时间:       2022/4/18 16:39:18
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      01dfa270-95db-4903-b96a-99ec68b952cc
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/18 16:39:18
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Luster.Common.DataStruct.Attributes
{
    /// <summary>
    /// 属性对象
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class PropItemAttribute : Attribute
    {
        /// <summary>
        /// 隶属对象
        /// </summary>
        public object Owner { get; set; }

        private object _val;
        /// <summary>
        /// 隶属对象
        /// </summary>
        public object Value
        {
            get => _val; set
            {
                SetPropVal(value);
            }
        }

        /// <summary>
        /// 设置属性值
        /// </summary>
        private void SetPropVal(object val)
        {
            if (Owner != null && Property != null)
            {
                if (Type == typeof(LItems) && Value != null)
                {
                    var lItem = Value as LItems;
                    lItem.Value = val;
                    Property.SetValue(Owner, lItem);
                    _val = lItem;
                }
                else
                {
                    Property.SetValue(Owner, val.ConvertToType(Type));
                    _val = val;
                }
            }
        }

        /// <summary>
        /// 属性排序
        /// </summary>
        public int Sort { get; set; } = 0;

        /// <summary>
        /// 分组
        /// </summary>
        public string Group { get; set; } = string.Empty;

        /// <summary>
        /// 属性名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// 编辑框类型
        /// </summary>
        public Type EditorType { get; set; }

        /// <summary>
        /// 属性信息
        /// </summary>
        public PropertyInfo Property { get; private set; }

        /// <summary>
        /// 依赖对象
        /// </summary>
        public List<KeyValue> DependOns { get; set; }

        /// <summary>
        /// 显示名称
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// 提示信息
        /// </summary>
        public string Tips { get; set; } = string.Empty;

        /// <summary>
        /// 是否只读
        /// </summary>
        public bool IsReadOnly { get; set; } = false;

        /// <summary>
        /// 如果是double类型支持设置小数点尾数
        /// </summary>
        public int DecimalPlaces { get; set; }

        /// <summary>
        /// 默认值
        /// </summary>
        public object DefaultV { get; set; }

        /// <summary>
        /// 最小值
        /// </summary>
        public double MinV { get; set; } = -999999;

        /// <summary>
        /// 最大值
        /// </summary>
        public double MaxV { get; set; } = 999999999;

        /// <summary>
        /// 对应Collection
        /// </summary>
        public List<PropItemAttribute> OwnerCollection { get; set; }

        /// <summary>
        /// 通过Key获取PropItem特性值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual PropItemAttribute GetPropItemByKey(string key)
        {
            if (ContainKey(key))
                return OwnerCollection.FirstOrDefault(u => u.Name == key);
            throw new KeyNotFoundException(key);
        }

        /// <summary>
        /// 是否包含对应的名称
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual bool ContainKey(string key)
        {
            return OwnerCollection != null && OwnerCollection.Any(u => u.Name == key);
        }

        /// <summary>
        /// 通过属性来构建PropItem对象
        /// </summary>
        /// <param name="propInfo"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static PropItemAttribute CreateByProperty(PropertyInfo propInfo, object obj)
        {
            PropItemAttribute paramAttr = propInfo.GetCustomAttribute<PropItemAttribute>();
            if (paramAttr == null) return null;

            // 通过属性就行赋值
            paramAttr.Name = propInfo.Name;
            paramAttr.Type = propInfo.PropertyType;
            paramAttr.Property = propInfo;
            paramAttr.Owner = obj;
            if (string.IsNullOrEmpty(paramAttr.DisplayName))
            {
                paramAttr.DisplayName = propInfo.Name;
                var disAttr = propInfo.GetCustomAttribute<DisplayNameAttribute>();
                if (disAttr != null)
                {
                    paramAttr.DisplayName = disAttr.DisplayName;
                }
            }

            // 设置默认分组
            if (string.IsNullOrEmpty(paramAttr.Group))
                paramAttr.Group = paramAttr.Group;

            // 如果是double类型,并且没有设置默认的小数位数，默认保留两位小数
            if (propInfo.PropertyType == typeof(double) && paramAttr.DecimalPlaces == 0)
            {
                paramAttr.DecimalPlaces = 2;
            }
            else if (propInfo.PropertyType == typeof(int))
            {
                // 默认是0
                paramAttr.DecimalPlaces = 0;
            }

            // 默认值
            if (paramAttr.DefaultV != null)
            {
                paramAttr.Value = paramAttr.DefaultV;
            }
            else
            {
                object v = propInfo.GetValue(obj);
                if (v != null)
                    paramAttr.Value = v;
            }

            // 获取依赖的参数
            var depends = propInfo.GetCustomAttributes<DependOnAttribute>();
            if (depends != null)
            {
                paramAttr.DependOns = new List<KeyValue>();
                foreach (var item in depends)
                {
                    foreach (var subVal in item.DependValue)
                    {
                        paramAttr.DependOns.Add(new KeyValue()
                        {
                            Key = item.DependProp,
                            Value = subVal,
                        });
                    }
                }
            }


            return paramAttr;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="sort">排序</param>
        public PropItemAttribute(int sort)
        {
            Sort = sort;
        }
    }
}