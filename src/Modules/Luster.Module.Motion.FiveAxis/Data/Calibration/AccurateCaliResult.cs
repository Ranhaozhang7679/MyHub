using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using Luster.Motion.FiveAxis.Coordinate;
using Luster.Motion.FiveAxis.Kinematics;
using Luster.Motion.FiveAxis.Position;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace Luster.Motion.FiveAxis.Data.Calibration
{
    /// <summary>
    /// 五轴精确标定结果(对应源端 AutoCaliProfile.Accurate5AxisAutoCaliProfile)。
    /// Rx/Rz 间隔+正反数量 + 球心拟合结果点列表(List&lt;PositionXYZRxRyRz&gt;)+ 精确五轴参数(Coord5Axis)。IXMLParser 落盘往返(P5-5)。
    /// </summary>
    [Serializable]
    public class AccurateCaliResult : IXMLParser
    {
        public AccurateCaliResult()
        {
            this.FirstPosi = new PositionXYZRxRyRz();
            this.RxSpan = 0;
            this.RxFCount = 0;
            this.RxBCount = 0;
            this.RzSpan = 0;
            this.RzFCount = 0;
            this.RzBCount = 0;
            this.ResultFirstPosi = new PositionXYZRxRyRz();
            this.ResultRxPosiLis = new List<PositionXYZRxRyRz>();
            this.ResultRzPosiLis = new List<PositionXYZRxRyRz>();
            this.Accurate5Para = new Coord5Axis();
            this.ZeroRx = 0;
        }
        public AccurateCaliResult(AccurateCaliResult other) : this()
        {
            this.CopyFrom(other);
        }
        public void CopyFrom(AccurateCaliResult other)
        {
            this.FirstPosi.CopyFrom(other.FirstPosi);
            this.RxSpan = other.RxSpan;
            this.RxFCount = other.RxFCount;
            this.RxBCount = other.RxBCount;
            this.RzSpan = other.RzSpan;
            this.RzFCount = other.RzFCount;
            this.RzBCount = other.RzBCount;
            this.ResultFirstPosi.CopyFrom(other.ResultFirstPosi);
            this.ResultRxPosiLis.Clear();
            foreach (var item in other.ResultRxPosiLis) this.ResultRxPosiLis.Add(new PositionXYZRxRyRz(item));
            this.ResultRzPosiLis.Clear();
            foreach (var item in other.ResultRzPosiLis) this.ResultRzPosiLis.Add(new PositionXYZRxRyRz(item));
            this.Accurate5Para.CopyFrom(other.Accurate5Para);
            this.ZeroRx = other.ZeroRx;
        }

        /// <summary>第一个示教位置</summary>
        public PositionXYZRxRyRz FirstPosi { get; set; }
        /// <summary>Rx 间隔</summary>
        public double RxSpan { get; set; }
        /// <summary>Rx 正向数量</summary>
        public int RxFCount { get; set; }
        /// <summary>Rx 反向数量</summary>
        public int RxBCount { get; set; }
        /// <summary>Rz 间隔</summary>
        public double RzSpan { get; set; }
        /// <summary>Rz 正向数量</summary>
        public int RzFCount { get; set; }
        /// <summary>Rz 反向数量</summary>
        public int RzBCount { get; set; }
        /// <summary>起始点结果位置</summary>
        public PositionXYZRxRyRz ResultFirstPosi { get; set; }
        /// <summary>Rx 结果点位列表(球心拟合点)</summary>
        public List<PositionXYZRxRyRz> ResultRxPosiLis { get; set; }
        /// <summary>Rz 结果点位列表(球心拟合点)</summary>
        public List<PositionXYZRxRyRz> ResultRzPosiLis { get; set; }
        /// <summary>精确五轴参数</summary>
        public Coord5Axis Accurate5Para { get; set; }
        /// <summary>Rx 零点位置</summary>
        public double ZeroRx { get; set; }

        /// <summary>根据精确五轴参数与球半径,生成 Rx 采样目标点列表(原样迁自源端)</summary>
        public List<PositionXYZRxRyRz> GetRxPosiLis(Coord5Axis para, double radius)
        {
            var ballCenterPosi = new PositionXYZRxRyRz(this.ResultFirstPosi);
            var dfirst = para.PoseO2D(ballCenterPosi);
            List<PositionXYZRxRyRz> oLis = new List<PositionXYZRxRyRz>();
            for (int i = 0; i < this.RxBCount; i++)
            {
                double offsetRx = -(i + 1) * this.RxSpan;
                PositionXYZRxRyRz dtemp = new PositionXYZRxRyRz(dfirst);
                dtemp.RX += offsetRx;
                var tmpCenter = para.PoseD2O(dtemp);
                tmpCenter.Z += radius;
                oLis.Add(tmpCenter);
            }
            for (int i = 0; i < this.RxFCount; i++)
            {
                double offsetRx = (i + 1) * this.RxSpan;
                PositionXYZRxRyRz dtemp = new PositionXYZRxRyRz(dfirst);
                dtemp.RX += offsetRx;
                var tmpCenter = para.PoseD2O(dtemp);
                tmpCenter.Z += radius;
                oLis.Add(tmpCenter);
            }
            return oLis;
        }

        /// <summary>根据精确五轴参数与球半径,生成 Rz 采样目标点列表(原样迁自源端)</summary>
        public List<PositionXYZRxRyRz> GetRzPosiLis(Coord5Axis para, double radius)
        {
            var ballCenterPosi = new PositionXYZRxRyRz(this.ResultFirstPosi);
            var dfirst = para.PoseO2D(ballCenterPosi);
            List<PositionXYZRxRyRz> oLis = new List<PositionXYZRxRyRz>();
            for (int i = 0; i < this.RzBCount; i++)
            {
                double offsetRz = -(i + 1) * this.RzSpan;
                PositionXYZRxRyRz dtemp = new PositionXYZRxRyRz(dfirst);
                dtemp.RZ += offsetRz;
                var tmpCenter = para.PoseD2O(dtemp);
                tmpCenter.Z += radius;
                oLis.Add(tmpCenter);
            }
            for (int i = 0; i < this.RzFCount; i++)
            {
                double offsetRz = (i + 1) * this.RzSpan;
                PositionXYZRxRyRz dtemp = new PositionXYZRxRyRz(dfirst);
                dtemp.RZ += offsetRz;
                var tmpCenter = para.PoseD2O(dtemp);
                tmpCenter.Z += radius;
                oLis.Add(tmpCenter);
            }
            return oLis;
        }

        public XElement ExportXml()
        {
            XElement xRoot = new XElement("AccurateCaliResult");
            xRoot.SetAttributeValue("RxSpan", XmlConvert.ToString(RxSpan));
            xRoot.SetAttributeValue("RxFCount", XmlConvert.ToString(RxFCount));
            xRoot.SetAttributeValue("RxBCount", XmlConvert.ToString(RxBCount));
            xRoot.SetAttributeValue("RzSpan", XmlConvert.ToString(RzSpan));
            xRoot.SetAttributeValue("RzFCount", XmlConvert.ToString(RzFCount));
            xRoot.SetAttributeValue("RzBCount", XmlConvert.ToString(RzBCount));
            xRoot.SetAttributeValue("ZeroRx", XmlConvert.ToString(ZeroRx));
            xRoot.Add(CalibrationXml.ToXml(FirstPosi, "FirstPosi"));
            xRoot.Add(CalibrationXml.ToXml(ResultFirstPosi, "ResultFirstPosi"));
            xRoot.Add(ToXml(ResultRxPosiLis, "ResultRxPosiLis"));
            xRoot.Add(ToXml(ResultRzPosiLis, "ResultRzPosiLis"));
            xRoot.Add(CalibrationXml.ToXml(Accurate5Para, "Accurate5Para"));
            return xRoot;
        }

        public void ParserXml(XElement xElement)
        {
            xElement.GetAttribute("RxSpan", v => RxSpan = XmlConvert.ToDouble(v));
            xElement.GetAttribute("RxFCount", v => RxFCount = XmlConvert.ToInt32(v));
            xElement.GetAttribute("RxBCount", v => RxBCount = XmlConvert.ToInt32(v));
            xElement.GetAttribute("RzSpan", v => RzSpan = XmlConvert.ToDouble(v));
            xElement.GetAttribute("RzFCount", v => RzFCount = XmlConvert.ToInt32(v));
            xElement.GetAttribute("RzBCount", v => RzBCount = XmlConvert.ToInt32(v));
            xElement.GetAttribute("ZeroRx", v => ZeroRx = XmlConvert.ToDouble(v));
            CalibrationXml.FromXml(xElement.Element("FirstPosi"), FirstPosi);
            CalibrationXml.FromXml(xElement.Element("ResultFirstPosi"), ResultFirstPosi);
            FromXml(xElement.Element("ResultRxPosiLis"), ResultRxPosiLis);
            FromXml(xElement.Element("ResultRzPosiLis"), ResultRzPosiLis);
            CalibrationXml.FromXml(xElement.Element("Accurate5Para"), Accurate5Para);
        }

        private static XElement ToXml(List<PositionXYZRxRyRz> lis, string name)
        {
            var x = new XElement(name);
            foreach (var p in lis) x.Add(CalibrationXml.ToXml(p, "Point"));
            return x;
        }

        private static void FromXml(XElement x, List<PositionXYZRxRyRz> lis)
        {
            lis.Clear();
            if (x == null) return;
            foreach (var xp in x.Elements("Point"))
            {
                var p = new PositionXYZRxRyRz();
                CalibrationXml.FromXml(xp, p);
                lis.Add(p);
            }
        }
    }
}
