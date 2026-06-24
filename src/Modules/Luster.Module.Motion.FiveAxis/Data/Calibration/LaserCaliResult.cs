using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using Luster.Motion.FiveAxis.Coordinate;
using Luster.Motion.FiveAxis.Position;
using System;
using System.Xml;
using System.Xml.Linq;

namespace Luster.Motion.FiveAxis.Data.Calibration
{
    /// <summary>
    /// 激光标定结果(对应源端 AutoCaliProfile.LaserCaliProfile)。
    /// 激光读数↔Z 高度线性映射(LinearConverter)+ 激光/相机示教位置。IXMLParser 落盘往返(P5-5)。
    /// </summary>
    [Serializable]
    public class LaserCaliResult : IXMLParser
    {
        public LaserCaliResult()
        {
            this.LaserId = 1;
            this.LaserStandard = 0;
            this.LaserMap = new LinearConverter();
            this.LaserPosi = new PositionXYZ();
            this.CameraPosi = new PositionXYZ();
        }
        public LaserCaliResult(LaserCaliResult other) : this()
        {
            this.CopyFrom(other);
        }
        public void CopyFrom(LaserCaliResult obj)
        {
            this.LaserId = obj.LaserId;
            this.LaserStandard = obj.LaserStandard;
            this.LaserMap.CopyFrom(obj.LaserMap);
            this.LaserPosi.CopyFrom(obj.LaserPosi);
            this.CameraPosi.CopyFrom(obj.CameraPosi);
        }

        /// <summary>激光 ID</summary>
        public byte LaserId { get; set; }
        /// <summary>标准测量值</summary>
        public double LaserStandard { get; set; }
        /// <summary>激光与 Z 轴线性关系(直接值:激光测量值,当量值:Z 轴高度)</summary>
        public LinearConverter LaserMap { get; set; }
        /// <summary>激光示教位置(激光与相机对同一点的激光位置)</summary>
        public PositionXYZ LaserPosi { get; set; }
        /// <summary>相机示教位置(激光与相机对同一点的相机位置)</summary>
        public PositionXYZ CameraPosi { get; set; }

        public XElement ExportXml()
        {
            XElement xRoot = new XElement("LaserCaliResult");
            xRoot.SetAttributeValue("LaserId", XmlConvert.ToString(LaserId));
            xRoot.SetAttributeValue("LaserStandard", XmlConvert.ToString(LaserStandard));
            xRoot.Add(LaserMap.ExportXml());
            xRoot.Add(CalibrationXml.ToXml(LaserPosi, "LaserPosi"));
            xRoot.Add(CalibrationXml.ToXml(CameraPosi, "CameraPosi"));
            return xRoot;
        }

        public void ParserXml(XElement xElement)
        {
            xElement.GetAttribute("LaserId", v => LaserId = (byte)XmlConvert.ToInt32(v));
            xElement.GetAttribute("LaserStandard", v => LaserStandard = XmlConvert.ToDouble(v));
            var xMap = xElement.Element("LinearConverter");
            if (xMap != null) LaserMap.ParserXml(xMap);
            CalibrationXml.FromXml(xElement.Element("LaserPosi"), LaserPosi);
            CalibrationXml.FromXml(xElement.Element("CameraPosi"), CameraPosi);
        }
    }
}
