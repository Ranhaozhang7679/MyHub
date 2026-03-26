#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LData
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.DataStruct.DataModels
* 文 件 名:       LData.cs
* 创建时间:       2022/4/19 14:57:45
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      44ddc0b8-6b76-4917-aaed-ade5954695e5
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/19 14:57:45
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.Common.DataStruct.DataModels
{
    /// <summary>
    /// 包含数据源的对象
    /// </summary>
    public class LItems
    {
        /// <summary>
        /// 数据源对象
        /// </summary>
        public List<KeyValue> ItemSource { get; set; }

        /// <summary>
        /// 选择的值
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// 对应的名称
        /// </summary>
        public string DisplayPath { get; set; }

        /// <summary>
        /// 对应的值字段
        /// </summary>
        public string ValuePath { get; set; }

        /// <summary>
        /// 设置数据源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="displayPath"></param>
        /// <param name="valuePath"></param>
        public void SetItemSource<T>(List<T> items, string displayPath = "", string valuePath = "")
        {
            var type = typeof(T);
            ItemSource = new List<KeyValue>();
            if (type.IsNumeric() || type == typeof(string))
            {
                foreach (var item in items)
                {
                    ItemSource.Add(new KeyValue()
                    {
                        Key = item.ToString(),
                        Value = item,
                        Desc = item.ToString()
                    });
                }
            }
            else
            {
                foreach (var item in items)
                {
                    string desc = string.Empty;
                    if (!string.IsNullOrEmpty(displayPath))
                    {
                        var displayProp = type.GetProperty(displayPath);
                        if (displayProp != null)
                        {
                            desc = displayProp.GetValue(item, null).ToString();
                        }
                    }

                    object value = item;
                    if (!string.IsNullOrEmpty(valuePath))
                    {
                        var valueProp = type.GetProperty(valuePath);
                        if (valueProp != null)
                        {
                            value = valueProp.GetValue(item, null);
                        }
                    }

                    ItemSource.Add(new KeyValue()
                    {
                        Key = value?.ToString(),
                        Value = value,
                        Desc = desc
                    });
                }
            }
        }

        /// <summary>
        /// 重新Equiles判断
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            if (Value == null) return false;

            return obj.Equals(Value);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        /// <summary>
        /// 重写ToString方法
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (Value == null) return string.Empty;

            return Value.ToString();
        }
    }
}