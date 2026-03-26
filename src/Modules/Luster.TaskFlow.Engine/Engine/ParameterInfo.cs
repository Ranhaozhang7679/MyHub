#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ParameterInfo
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Engine.Engine
* 文 件 名:       ParameterInfo.cs
* 创建时间:       2022/2/7 14:57:12
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      92526413-ce40-4ae5-bcfc-0f9d76bb940d
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/2/7 14:57:12
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Extensions;
using Luster.TaskFlow.Common.Attributes;
using Luster.Common.DataStruct.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Luster.TaskFlow.Common.Module;

namespace Luster.TaskFlow.Engine
{
    /// <summary>
    /// 输入和输出参数
    /// </summary>
    public class ParameterInfo : IXMLParser
    {
        /// <summary>
        /// 模块ID
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// 参数名称
        /// </summary>
        public string ParamName { get; set; }

        /// <summary>
        /// 参数Key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 是否是输入参数，如果不是输入参数，那么就是输出参数
        /// </summary>
        public bool IsInput { get; set; }

        /// <summary>
        /// 参数名称
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// 参数类型
        /// </summary>
        public Type ParamType { get; set; }

        /// <summary>
        /// 参数值
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// 导出
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public XElement ExportXml()
        {
            XElement xItem = new XElement("ParamItem");
            xItem.SetAttributeValue("ModuleID", ID);
            xItem.SetAttributeValue("ParamType", ParameterAttribute.GetDataTypeByType(ParamType));
            xItem.SetAttributeValue("IsInput", IsInput);
            xItem.SetAttributeValue("ParamName", ParamName);
            xItem.SetAttributeValue("Key", Key);
            xItem.SetAttributeValue("Label", Label);
            return xItem;
        }

        /// <summary>
        /// 解析
        /// </summary>
        /// <param name="xElement"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void ParserXml(XElement xElement)
        {
            xElement.GetAttribute("ModuleID", (id) => ID = id);
            xElement.GetAttribute("ParamType", (pType) => ParamType = ParameterAttribute.GetTypeByDataType(pType));
            xElement.GetAttribute("IsInput", (isInput) => IsInput = bool.Parse(isInput));
            xElement.GetAttribute("ParamName", (pName) => ParamName = pName);
            xElement.GetAttribute("Key", (key) => Key = key);
            xElement.GetAttribute("Label", (label) => Label = label);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pAttr"></param>
        /// <returns></returns>
        public bool IsEqual(ParameterAttribute pAttr)
        {
            return pAttr.Name == ParamName && pAttr.Owner.ID.ToString() == ID;
        }

        /// <summary>
        /// 转换为参数
        /// </summary>
        /// <returns></returns>
        public ParameterAttribute ToParameter(IModule module, int sort)
        {
            return ParameterAttribute.CreateByType(Label, ParamType, module, Label, sort, IsInput ? Common.Enums.ParamType.IN : Common.Enums.ParamType.OUT);
        }
    }
}
