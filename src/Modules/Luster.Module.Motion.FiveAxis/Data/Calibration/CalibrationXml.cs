using Luster.Motion.FiveAxis.Kinematics;
using Luster.Motion.FiveAxis.Position;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;

namespace Luster.Motion.FiveAxis.Coordinate
{
    /// <summary>
    /// 标定/配置数据模型 XML 往返辅助(P5-5)。
    /// 为已迁入的 PositionXYZ / PositionXYZRxRyRz / Coord5Axis(P5-2 产出,本体算法不动)提供
    /// XElement round-trip,使标定结果数据模型(RoughCaliResult 等)实现 IXMLParser 时无需修改 P5-2 文件。
    /// double 一律用 XmlConvert(CultureInfoInvariant)序列化,保证跨区域往返一致。
    /// </summary>
    internal static class CalibrationXml
    {
        // ---- PositionXYZ ----

        /// <summary>导出 PositionXYZ(X/Y/Z 属性)</summary>
        public static XElement ToXml(PositionXYZ p, string elementName)
        {
            var x = new XElement(elementName);
            x.SetAttributeValue("X", XmlConvert.ToString(p.X));
            x.SetAttributeValue("Y", XmlConvert.ToString(p.Y));
            x.SetAttributeValue("Z", XmlConvert.ToString(p.Z));
            return x;
        }

        /// <summary>解析 PositionXYZ</summary>
        public static void FromXml(XElement x, PositionXYZ p)
        {
            if (x == null) return;
            p.X = GetDouble(x, "X", p.X);
            p.Y = GetDouble(x, "Y", p.Y);
            p.Z = GetDouble(x, "Z", p.Z);
        }

        // ---- PositionXYZRxRyRz ----

        /// <summary>导出 PositionXYZRxRyRz(X/Y/Z/RX/RY/RZ 属性)</summary>
        public static XElement ToXml(PositionXYZRxRyRz p, string elementName)
        {
            var x = ToXml((PositionXYZ)p, elementName);
            x.SetAttributeValue("RX", XmlConvert.ToString(p.RX));
            x.SetAttributeValue("RY", XmlConvert.ToString(p.RY));
            x.SetAttributeValue("RZ", XmlConvert.ToString(p.RZ));
            return x;
        }

        /// <summary>解析 PositionXYZRxRyRz</summary>
        public static void FromXml(XElement x, PositionXYZRxRyRz p)
        {
            if (x == null) return;
            FromXml(x, (PositionXYZ)p);
            p.RX = GetDouble(x, "RX", p.RX);
            p.RY = GetDouble(x, "RY", p.RY);
            p.RZ = GetDouble(x, "RZ", p.RZ);
        }

        // ---- Coord5Axis ----

        /// <summary>
        /// 导出 Coord5Axis(6 个结构参数:ACenter/ADir/CCenter/CDir 为 PositionXYZ,ACirPulses/CCirPulses 为 double)。
        /// 注意:Coord5Axis 内的 MathNet Matrix&lt;double&gt; 仅在方法内局部计算,无字段,无需序列化。
        /// </summary>
        public static XElement ToXml(Coord5Axis c, string elementName)
        {
            var x = new XElement(elementName);
            x.SetAttributeValue("ACirPulses", XmlConvert.ToString(c.ACirPulses));
            x.SetAttributeValue("CCirPulses", XmlConvert.ToString(c.CCirPulses));
            x.Add(ToXml(c.ACenter, "ACenter"));
            x.Add(ToXml(c.ADir, "ADir"));
            x.Add(ToXml(c.CCenter, "CCenter"));
            x.Add(ToXml(c.CDir, "CDir"));
            return x;
        }

        /// <summary>解析 Coord5Axis</summary>
        public static void FromXml(XElement x, Coord5Axis c)
        {
            if (x == null) return;
            c.ACirPulses = GetDouble(x, "ACirPulses", c.ACirPulses);
            c.CCirPulses = GetDouble(x, "CCirPulses", c.CCirPulses);
            FromXml(x.Element("ACenter"), c.ACenter);
            FromXml(x.Element("ADir"), c.ADir);
            FromXml(x.Element("CCenter"), c.CCenter);
            FromXml(x.Element("CDir"), c.CDir);
        }

        // ---- 通用 ----

        private static double GetDouble(XElement x, string attr, double fallback)
        {
            var a = x.Attribute(attr);
            if (a == null) return fallback;
            return XmlConvert.ToDouble(a.Value);
        }
    }
}
