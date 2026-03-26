#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LVariable
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.DataStruct.DataModels
* 文 件 名:       LVariable.cs
* 创建时间:       2022/9/8 10:34:05
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      61ae0402-2858-4da0-9166-a430473c8554
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/8 10:34:05
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
    /// 变量
    /// </summary>
    public class LVariable : IXMLParser
    {
        /// <summary>
        /// 对应的模块ID
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// 对应的Key
        /// </summary>
        public string ParamKey { get; set; }

        /// <summary>
        /// 最终显示名称
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// 用于缓存对象
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// 导出
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public XElement ExportXml()
        {
            XElement xVar = new XElement("Variable");
            xVar.SetAttributeValue("ID", ID);
            xVar.SetAttributeValue("ParamKey", ParamKey);
            xVar.SetAttributeValue("Alias", Alias);
            return xVar;
        }

        /// <summary>
        /// 对象解析
        /// </summary>
        /// <param name="xElement"></param>
        public void ParserXml(XElement xElement)
        {
            xElement.GetAttribute("ID", id => ID = Guid.Parse(id));
            xElement.GetAttribute("ParamKey", key => ParamKey = key);
            xElement.GetAttribute("Alias", alias => Alias = alias);
        }


        public override string ToString()
        {
            return Alias;
        }
    }
}