#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LRelyOn
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Common.Data
* 文 件 名:       LRelyOn.cs
* 创建时间:       2022/1/21 13:34:13
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      d41ce420-c422-4647-bbf7-a0971005f844
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/1/21 13:34:13
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.Extensions;
using Luster.Common.Models;
using Luster.TaskFlow.DataStruct.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.TaskFlow.Common.Data
{
    public class LRelyType : IXMLParser, IEmptyObj
    {
        /// <summary>
        /// 依赖参数名称
        /// </summary>
        public string ParamName { get; set; }

        /// <summary>
        /// 依赖具体属性名称
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// 依赖类型
        /// </summary>
        public string PropertyType { get; set; }

        /// <summary>
        /// 数据源
        /// </summary>
        public List<KeyValue> DataSource { get; set; }

        /// <summary>
        /// 导出
        /// </summary>
        /// <returns></returns>
        public XElement ExportXml()
        {
            XElement xRely = new XElement("LRelyType");
            xRely.SetAttributeValue("ParamName", ParamName);
            xRely.SetAttributeValue("PropertyName", PropertyName);
            xRely.SetAttributeValue("PropertyType", PropertyType);

            return xRely;
        }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(PropertyName);
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="xElement"></param>
        public void ParserXml(XElement xElement)
        {
            xElement.GetAttribute("PropertyName", (strP) => PropertyName = strP);
            xElement.GetAttribute("ParamName", (strP) => ParamName = strP);
            xElement.GetAttribute("PropertyType", (strP) => PropertyType = strP);
        }
    }
}
