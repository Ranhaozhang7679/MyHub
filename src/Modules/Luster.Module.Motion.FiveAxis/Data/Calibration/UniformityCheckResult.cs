using Luster.Common.DataStruct.Interfaces;
using Luster.Motion.FiveAxis.Coordinate;
using Luster.Motion.FiveAxis.Position;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Luster.Motion.FiveAxis.Data.Calibration
{
    /// <summary>
    /// 一致性点检结果(对应源端 AutoCaliProfile.UniformityCheckProfile)。
    /// 棋盘格/A 轴边缘/治具翻转成像位置 + 多点校验列表。IXMLParser 落盘往返(P5-5)。
    /// </summary>
    [Serializable]
    public class UniformityCheckResult : IXMLParser
    {
        public UniformityCheckResult()
        {
            this.CheckBoardPosi = new PositionXYZRxRyRz();
            this.APicPosi = new PositionXYZRxRyRz();
            this.A90Posi = new PositionXYZRxRyRz();
            this.A0Posi = new PositionXYZRxRyRz();
            this.CheckPoints = new List<PositionXYZRxRyRz>();
        }
        public UniformityCheckResult(UniformityCheckResult other) : this()
        {
            this.CopyFrom(other);
        }
        public void CopyFrom(UniformityCheckResult obj)
        {
            this.CheckBoardPosi.CopyFrom(obj.CheckBoardPosi);
            this.APicPosi.CopyFrom(obj.APicPosi);
            this.A90Posi.CopyFrom(obj.A90Posi);
            this.A0Posi.CopyFrom(obj.A0Posi);
            this.CheckPoints.Clear();
            foreach (var item in obj.CheckPoints) this.CheckPoints.Add(new PositionXYZRxRyRz(item));
        }

        /// <summary>棋盘格成像位置(像素当量/相机成像一致性)</summary>
        public PositionXYZRxRyRz CheckBoardPosi { get; set; }
        /// <summary>A 轴边缘成像位置(相机角度一致性)</summary>
        public PositionXYZRxRyRz APicPosi { get; set; }
        /// <summary>翻转 90 度治具边缘成像位置(治具高度一致性)</summary>
        public PositionXYZRxRyRz A90Posi { get; set; }
        /// <summary>翻转 0 度治具边缘成像位置(治具旋转角度一致性)</summary>
        public PositionXYZRxRyRz A0Posi { get; set; }
        /// <summary>一致性校验列表(多点校验治具一致性)</summary>
        public List<PositionXYZRxRyRz> CheckPoints { get; set; }

        public XElement ExportXml()
        {
            XElement xRoot = new XElement("UniformityCheckResult");
            xRoot.Add(CalibrationXml.ToXml(CheckBoardPosi, "CheckBoardPosi"));
            xRoot.Add(CalibrationXml.ToXml(APicPosi, "APicPosi"));
            xRoot.Add(CalibrationXml.ToXml(A90Posi, "A90Posi"));
            xRoot.Add(CalibrationXml.ToXml(A0Posi, "A0Posi"));
            var xPts = new XElement("CheckPoints");
            foreach (var p in CheckPoints) xPts.Add(CalibrationXml.ToXml(p, "Point"));
            xRoot.Add(xPts);
            return xRoot;
        }

        public void ParserXml(XElement xElement)
        {
            CalibrationXml.FromXml(xElement?.Element("CheckBoardPosi"), CheckBoardPosi);
            CalibrationXml.FromXml(xElement?.Element("APicPosi"), APicPosi);
            CalibrationXml.FromXml(xElement?.Element("A90Posi"), A90Posi);
            CalibrationXml.FromXml(xElement?.Element("A0Posi"), A0Posi);
            CheckPoints.Clear();
            var xPts = xElement?.Element("CheckPoints");
            if (xPts != null)
            {
                foreach (var xp in xPts.Elements("Point"))
                {
                    var p = new PositionXYZRxRyRz();
                    CalibrationXml.FromXml(xp, p);
                    CheckPoints.Add(p);
                }
            }
        }
    }
}
