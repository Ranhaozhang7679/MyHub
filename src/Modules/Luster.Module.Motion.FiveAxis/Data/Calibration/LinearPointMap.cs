using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using System;
using System.Xml;
using System.Xml.Linq;

namespace Luster.Motion.FiveAxis.Data.Calibration
{
    /// <summary>
    /// 线性映射点(直接值↔当量值)。原样迁自源端 CommonMachineModelLibrary.Model.Settings.LinearPointMap,
    /// UI 耦合剥离(去 FieldToPropertyTypeDescriptor 基类 + WinForm 特性),增补 IXMLParser(P5-5)。
    /// 供 LinearConverter 两点定标(激光读数↔Z 高度)落盘往返。
    /// </summary>
    [Serializable]
    public class LinearPointMap : IXMLParser
    {
        public LinearPointMap() : this(0, 0)
        {
        }
        public LinearPointMap(double direct, double unit)
        {
            this.DirectValue = direct;
            this.UnitValue = unit;
        }
        public LinearPointMap(LinearPointMap other)
        {
            this.CopyFrom(other);
        }
        public void CopyFrom(LinearPointMap other)
        {
            this.DirectValue = other.DirectValue;
            this.UnitValue = other.UnitValue;
        }

        /// <summary>直接值</summary>
        public double DirectValue { get; set; }
        /// <summary>当量值</summary>
        public double UnitValue { get; set; }

        public XElement ExportXml()
        {
            XElement xRoot = new XElement("LinearPointMap");
            xRoot.SetAttributeValue("DirectValue", XmlConvert.ToString(DirectValue));
            xRoot.SetAttributeValue("UnitValue", XmlConvert.ToString(UnitValue));
            return xRoot;
        }

        public void ParserXml(XElement xElement)
        {
            xElement.GetAttribute("DirectValue", v => DirectValue = XmlConvert.ToDouble(v));
            xElement.GetAttribute("UnitValue", v => UnitValue = XmlConvert.ToDouble(v));
        }

        public override string ToString()
        {
            return $"{DirectValue},{UnitValue}";
        }
    }
}
