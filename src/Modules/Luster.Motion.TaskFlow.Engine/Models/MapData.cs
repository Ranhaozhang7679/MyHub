#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       MapData
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.TaskFlow.Engine.Models
* 文 件 名:       MapData.cs
* 创建时间:       2022/7/20 15:20:38
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      64d3ef07-2f3a-42db-a506-41a1c88e31a2
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/20 15:20:38
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using Luster.TaskFlow.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.Motion.TaskFlow.Engine.Models
{
    /// <summary>
    /// 映射对象
    /// </summary>
    public class MapData : IXMLParser
    {
        /// <summary>
        /// 映射别名
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// 映射ID
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// 映射对应的Key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 数据类型
        /// </summary>
        public Type DataType { get; set; }

        /// <summary>
        /// 数据源
        /// </summary>
        public string DataSource { get; set; }

        /// <summary>
        /// 监控项
        /// </summary>
        public bool IsMonitor { get; set; }

        /// <summary>
        /// 标准值
        /// </summary>
        public double StandardValue { get; set; }

        /// <summary>
        /// 负公差
        /// </summary>
        public double NegativestandardDeviation { get; set; }

        /// <summary>
        /// 正公差
        /// </summary>
        public double PositivestandardDeviation { get; set; }

        /// <summary>
        /// 补偿值
        /// </summary>
        public double CompensationValue { get; set; }
        /// <summary>
        /// Ng原因
        /// </summary>
        public string NgReason { get; set; }


        /// <summary>
        /// 对应的参数
        /// </summary>
        public ParameterAttribute Parameter { get; set; }

        /// <summary>
        /// 导出
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public XElement ExportXml()
        {
            XElement xMap = new XElement("Map");
            xMap.SetAttributeValue("Alias", Alias);
            xMap.SetAttributeValue("ID", ID);
            xMap.SetAttributeValue("Key", Key);
            xMap.SetAttributeValue("DataSource", DataSource);
            xMap.SetAttributeValue("IsMonitor", IsMonitor);
            xMap.SetAttributeValue("StandardValue", StandardValue);
            xMap.SetAttributeValue("NegativestandardDeviation", NegativestandardDeviation);
            xMap.SetAttributeValue("PositivestandardDeviation", PositivestandardDeviation);
            xMap.SetAttributeValue("CompensationValue", CompensationValue);
            xMap.SetAttributeValue("NgReason", NgReason);
            var dataType = ParameterAttribute.GetDataTypeByType(DataType);
            xMap.SetAttributeValue("DataType", dataType);

            return xMap;
        }

        /// <summary>
        /// 解析
        /// </summary>
        /// <param name="xElement"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void ParserXml(XElement xElement)
        {
            xElement.GetAttribute("Alias", alias => Alias = alias);
            xElement.GetAttribute("ID", id => ID = Guid.Parse(id));
            xElement.GetAttribute("Key", key => Key = key);
            xElement.GetAttribute("DataSource", d => DataSource = d);
            xElement.GetAttribute("IsMonitor", d => IsMonitor = bool.Parse(d));
            xElement.GetAttribute("StandardValue", standvalue => StandardValue = Convert.ToDouble(standvalue));
            xElement.GetAttribute("NegativestandardDeviation", maxlimit => NegativestandardDeviation = Convert.ToDouble(maxlimit));
            xElement.GetAttribute("PositivestandardDeviation", minlimit => PositivestandardDeviation = Convert.ToDouble(minlimit));
            xElement.GetAttribute("CompensationValue", compensationValue => CompensationValue = Convert.ToDouble(compensationValue));
            xElement.GetAttribute("NgReason", ngreason => NgReason = ngreason);
            xElement.GetAttribute("DataType", dataType =>
            {
                var type = ParameterAttribute.GetTypeByDataType(dataType);
                if (type != null)
                {
                    DataType = type;
                }
            });
        }
    }
}