#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LExpress
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Common.Data
* 文 件 名:       LExpress.cs
* 创建时间:       2022/2/11 10:42:14
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      21cddad5-b935-4817-8c4b-2d1b4ddf442c
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/2/11 10:42:14
* 修 改 人:		  L05123
************************************************************************************/

#endregion

using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Luster.Common.DataStruct.DataModels
{
    /// <summary>
    /// 表达式
    /// </summary>
    public class LExpress : IXMLParser, IEmptyObj
    {
        /// <summary>
        /// 表达式名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 表达式
        /// </summary>
        public string ExText { get; set; }

        /// <summary>
        /// Desc 对应别名
        /// Key 对应模块GUID和Key
        /// </summary>
        public List<KeyValue> KeyWords { get; set; }

        /// <summary>
        /// 变量
        /// </summary>
        public List<LVariable> Variables { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public LExpress()
        {
            ExText = string.Empty;
            Name = string.Empty;
            KeyWords = new List<KeyValue>();
            Variables = new List<LVariable>();
        }

        /// <summary>
        /// 表达式导出
        /// </summary>
        /// <returns></returns>
        public XElement ExportXml()
        {
            XElement xRoot = new XElement("Express");
            xRoot.SetAttributeValue("Name", Name);
            xRoot.SetAttributeValue("Txt", ExText);

            foreach (var item in KeyWords)
            {
                XElement xItem = new XElement("Keyword");
                xItem.SetAttributeValue("Desc", item.Desc);
                xItem.SetAttributeValue("Key", item.Key);
                xItem.SetAttributeValue("Value", item.Value);
                xRoot.Add(xItem);
            }

            if (Variables.Count > 0)
            {
                foreach (var item in Variables)
                {
                    xRoot.Add(item.ExportXml());
                }
            }

            return xRoot;
        }

        /// <summary>
        /// 表达式解析
        /// </summary>
        /// <param name="xElement"></param>
        public void ParserXml(XElement xElement)
        {
            xElement.GetAttribute("Name", (name) => Name = name);
            xElement.GetAttribute("Txt", (txt) => ExText = txt);

            foreach (var xKey in xElement.Elements("Keyword"))
            {
                KeyValue keyValue = new KeyValue();
                xKey.GetAttribute("Desc", desc => keyValue.Desc = desc);
                xKey.GetAttribute("Key", key => keyValue.Key = key);
                xKey.GetAttribute("Value", val => keyValue.Value = val);
                KeyWords.Add(keyValue);
            }

            foreach (var item in xElement.Elements("Variable"))
            {
                LVariable v = new LVariable();
                v.ParserXml(item);
                Variables.Add(v);
            }
        }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(ExText);
        }
    }
}