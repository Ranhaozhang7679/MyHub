using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using Luster.Motion.FiveAxis.Coordinate;
using Luster.Motion.FiveAxis.Kinematics;
using Luster.Motion.FiveAxis.Position;
using System;
using System.Xml;
using System.Xml.Linq;

namespace Luster.Motion.FiveAxis.Data.Calibration
{
    /// <summary>
    /// 五轴粗略标定结果(对应源端 AutoCaliProfile.Rough5AxisAutoCaliProfile)。
    /// 三点示教(FirstPosi/RxPosi/RzPosi)+ Rx/Rz 角度 + 示教结果 + 粗略五轴参数(Coord5Axis)。IXMLParser 落盘往返(P5-5)。
    /// </summary>
    [Serializable]
    public class RoughCaliResult : IXMLParser
    {
        public RoughCaliResult()
        {
            this.FirstPosi = new PositionXYZRxRyRz();
            this.RxPosi = new PositionXYZRxRyRz();
            this.RzPosi = new PositionXYZRxRyRz();
            this.Rx = 5;
            this.Rz = 5;
            this.ResultFirstPosi = new PositionXYZ();
            this.ResultRxPosi = new PositionXYZ();
            this.ResultRzPosi = new PositionXYZ();
            this.Rough5Para = new Coord5Axis();
        }
        public RoughCaliResult(RoughCaliResult other) : this()
        {
            this.CopyFrom(other);
        }
        public void CopyFrom(RoughCaliResult obj)
        {
            this.FirstPosi.CopyFrom(obj.FirstPosi);
            this.RxPosi.CopyFrom(obj.RxPosi);
            this.RzPosi.CopyFrom(obj.RzPosi);
            this.Rx = obj.Rx;
            this.Rz = obj.Rz;
            this.ResultFirstPosi.CopyFrom(obj.ResultFirstPosi);
            this.ResultRxPosi.CopyFrom(obj.ResultRxPosi);
            this.ResultRzPosi.CopyFrom(obj.ResultRzPosi);
            this.Rough5Para.CopyFrom(obj.Rough5Para);
        }

        /// <summary>第一个示教位置</summary>
        public PositionXYZRxRyRz FirstPosi { get; set; }
        /// <summary>Rx 标定示教位置</summary>
        public PositionXYZRxRyRz RxPosi { get; set; }
        /// <summary>Rz 标定示教位置</summary>
        public PositionXYZRxRyRz RzPosi { get; set; }
        /// <summary>Rx 旋转角度</summary>
        public double Rx { get; set; }
        /// <summary>Rz 旋转角度</summary>
        public double Rz { get; set; }
        /// <summary>第一个示教结果</summary>
        public PositionXYZ ResultFirstPosi { get; set; }
        /// <summary>Rx 示教结果</summary>
        public PositionXYZ ResultRxPosi { get; set; }
        /// <summary>Rz 示教结果</summary>
        public PositionXYZ ResultRzPosi { get; set; }
        /// <summary>五轴粗略参数</summary>
        public Coord5Axis Rough5Para { get; set; }

        /// <summary>根据第一个位置得到 Rx,Rz 标定示教位置</summary>
        public void GeneratePosi()
        {
            this.RxPosi.CopyFrom(FirstPosi);
            this.RxPosi.RX += this.Rx;
            this.RzPosi.CopyFrom(FirstPosi);
            this.RzPosi.RZ += this.Rz;
        }

        public XElement ExportXml()
        {
            XElement xRoot = new XElement("RoughCaliResult");
            xRoot.SetAttributeValue("Rx", XmlConvert.ToString(Rx));
            xRoot.SetAttributeValue("Rz", XmlConvert.ToString(Rz));
            xRoot.Add(CalibrationXml.ToXml(FirstPosi, "FirstPosi"));
            xRoot.Add(CalibrationXml.ToXml(RxPosi, "RxPosi"));
            xRoot.Add(CalibrationXml.ToXml(RzPosi, "RzPosi"));
            xRoot.Add(CalibrationXml.ToXml(ResultFirstPosi, "ResultFirstPosi"));
            xRoot.Add(CalibrationXml.ToXml(ResultRxPosi, "ResultRxPosi"));
            xRoot.Add(CalibrationXml.ToXml(ResultRzPosi, "ResultRzPosi"));
            xRoot.Add(CalibrationXml.ToXml(Rough5Para, "Rough5Para"));
            return xRoot;
        }

        public void ParserXml(XElement xElement)
        {
            xElement.GetAttribute("Rx", v => Rx = XmlConvert.ToDouble(v));
            xElement.GetAttribute("Rz", v => Rz = XmlConvert.ToDouble(v));
            CalibrationXml.FromXml(xElement.Element("FirstPosi"), FirstPosi);
            CalibrationXml.FromXml(xElement.Element("RxPosi"), RxPosi);
            CalibrationXml.FromXml(xElement.Element("RzPosi"), RzPosi);
            CalibrationXml.FromXml(xElement.Element("ResultFirstPosi"), ResultFirstPosi);
            CalibrationXml.FromXml(xElement.Element("ResultRxPosi"), ResultRxPosi);
            CalibrationXml.FromXml(xElement.Element("ResultRzPosi"), ResultRzPosi);
            CalibrationXml.FromXml(xElement.Element("Rough5Para"), Rough5Para);
        }
    }
}
