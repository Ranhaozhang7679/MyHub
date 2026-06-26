using Luster.Common.DataStruct.Interfaces;
using System;
using System.Xml.Linq;

namespace Luster.Motion.FiveAxis.Data.Calibration
{
    /// <summary>
    /// 线性转换器(两点定标:y=kx+b)。原样迁自源端 CommonMachineModelLibrary.Model.Settings.LinearConverter,
    /// UI 耦合剥离 + 增补 IXMLParser(P5-5)。供激光 Z 标定(激光读数↔Z 高度)落盘往返。
    /// </summary>
    [Serializable]
    public class LinearConverter : IXMLParser
    {
        public LinearConverter()
        {
            this.Map1 = new LinearPointMap(0, 0);
            this.Map2 = new LinearPointMap(1, 1);
        }
        public LinearConverter(LinearConverter other) : this()
        {
            this.CopyFrom(other);
        }
        public void CopyFrom(LinearConverter other)
        {
            this.Map1.CopyFrom(other.Map1);
            this.Map2.CopyFrom(other.Map2);
        }

        /// <summary>映射1</summary>
        public LinearPointMap Map1 { get; set; }
        /// <summary>映射2</summary>
        public LinearPointMap Map2 { get; set; }

        /// <summary>
        /// 当量为 y,模拟量为 x 有 y=kx+b,[0]为 k,[1]为 b
        /// </summary>
        private double[] GetConvertFactor()
        {
            double k = (Map2.UnitValue - Map1.UnitValue) / (Map2.DirectValue - Map1.DirectValue);
            double b = Map1.UnitValue - k * Map1.DirectValue;
            return new double[] { k, b };
        }

        /// <summary>当量转直接值 x=(y-b)/k</summary>
        public double UnitToDirectValue(double unit)
        {
            var factor = GetConvertFactor();
            return (unit - factor[1]) / factor[0];
        }

        /// <summary>直接值转当量 y=k*x+b</summary>
        public double DirectValueToUnit(double direct)
        {
            var factor = GetConvertFactor();
            return factor[0] * direct + factor[1];
        }

        public XElement ExportXml()
        {
            XElement xRoot = new XElement("LinearConverter");
            // Map1/Map2 同为 LinearPointMap,用包装节点名区分
            var xMap1 = Map1.ExportXml(); xMap1.Name = "Map1";
            var xMap2 = Map2.ExportXml(); xMap2.Name = "Map2";
            xRoot.Add(xMap1);
            xRoot.Add(xMap2);
            return xRoot;
        }

        public void ParserXml(XElement xElement)
        {
            var xMap1 = xElement?.Element("Map1");
            if (xMap1 != null) Map1.ParserXml(xMap1);
            var xMap2 = xElement?.Element("Map2");
            if (xMap2 != null) Map2.ParserXml(xMap2);
        }

        public override string ToString()
        {
            return this.GetType().Name;
        }
    }
}
