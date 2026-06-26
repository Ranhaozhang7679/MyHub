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
    /// 工件原点示教结果(对应源端 AutoCaliProfile.TeachWorkOriginProfile)。
    /// 工件坐标系结果(CoordTransForm)+ 三点示教 + 原点类型 + 原点偏移计算。IXMLParser 落盘往返(P5-5)。
    /// </summary>
    [Serializable]
    public class TeachWorkOriginResult : IXMLParser
    {
        public TeachWorkOriginResult()
        {
            this.RltTool2Work = new CoordTransForm();
            this.OriginPosi = new PositionXYZRxRyRz();
            this.LongSidePosi = new PositionXYZRxRyRz();
            this.DiagonalPosi = new PositionXYZRxRyRz();
            this.OrgPosiType = OriginPosiType.DiagCenter;
        }
        public TeachWorkOriginResult(TeachWorkOriginResult other) : this()
        {
            this.CopyFrom(other);
        }
        public void CopyFrom(TeachWorkOriginResult other)
        {
            this.RltTool2Work.CopyFrom(other.RltTool2Work);
            this.OriginPosi.CopyFrom(other.OriginPosi);
            this.LongSidePosi.CopyFrom(other.LongSidePosi);
            this.DiagonalPosi.CopyFrom(other.DiagonalPosi);
            this.OrgPosiType = other.OrgPosiType;
        }

        /// <summary>工件坐标系结果</summary>
        public CoordTransForm RltTool2Work { get; set; }
        /// <summary>第一个示教位置</summary>
        public PositionXYZRxRyRz OriginPosi { get; set; }
        /// <summary>长边终点示教位置</summary>
        public PositionXYZRxRyRz LongSidePosi { get; set; }
        /// <summary>对角线示教位置</summary>
        public PositionXYZRxRyRz DiagonalPosi { get; set; }
        /// <summary>原点位置类型</summary>
        public OriginPosiType OrgPosiType { get; set; }

        /// <summary>原点位置类型</summary>
        public enum OriginPosiType
        {
            /// <summary>起点</summary>
            OriginPosi,
            /// <summary>长边中心</summary>
            LongCenter,
            /// <summary>对角线中心</summary>
            DiagCenter,
        }

        /// <summary>根据示教位置与原点类型计算原点偏移(原样迁自源端)</summary>
        public PositionXYZRz CalculateOriginOffset()
        {
            switch (OrgPosiType)
            {
                case OriginPosiType.OriginPosi:
                    return new PositionXYZRz()
                    {
                        X = OriginPosi.X,
                        Y = OriginPosi.Y,
                        Z = OriginPosi.Z,
                        RZ = Math.Atan2(LongSidePosi.Y - OriginPosi.Y, LongSidePosi.X - OriginPosi.X),
                    };
                case OriginPosiType.LongCenter:
                    return new PositionXYZRz()
                    {
                        X = (OriginPosi.X + LongSidePosi.X) / 2,
                        Y = (OriginPosi.Y + LongSidePosi.Y) / 2,
                        Z = (OriginPosi.Z + LongSidePosi.Z) / 2,
                        RZ = Math.Atan2(LongSidePosi.Y - OriginPosi.Y, LongSidePosi.X - OriginPosi.X),
                    };
                case OriginPosiType.DiagCenter:
                    return new PositionXYZRz()
                    {
                        X = (OriginPosi.X + DiagonalPosi.X) / 2,
                        Y = (OriginPosi.Y + DiagonalPosi.Y) / 2,
                        Z = (OriginPosi.Z + DiagonalPosi.Z) / 2,
                        RZ = Math.Atan2(LongSidePosi.Y - OriginPosi.Y, LongSidePosi.X - OriginPosi.X),
                    };
                default: return new PositionXYZRz();
            }
        }

        public XElement ExportXml()
        {
            XElement xRoot = new XElement("TeachWorkOriginResult");
            xRoot.SetAttributeValue("OrgPosiType", OrgPosiType.ToString());
            xRoot.Add(RltTool2Work.ExportXml());
            xRoot.Add(CalibrationXml.ToXml(OriginPosi, "OriginPosi"));
            xRoot.Add(CalibrationXml.ToXml(LongSidePosi, "LongSidePosi"));
            xRoot.Add(CalibrationXml.ToXml(DiagonalPosi, "DiagonalPosi"));
            return xRoot;
        }

        public void ParserXml(XElement xElement)
        {
            xElement.GetAttribute("OrgPosiType", v =>
            {
                if (Enum.TryParse(v, out OriginPosiType t)) OrgPosiType = t;
            });
            var xTrans = xElement.Element("CoordTransForm");
            if (xTrans != null) RltTool2Work.ParserXml(xTrans);
            CalibrationXml.FromXml(xElement.Element("OriginPosi"), OriginPosi);
            CalibrationXml.FromXml(xElement.Element("LongSidePosi"), LongSidePosi);
            CalibrationXml.FromXml(xElement.Element("DiagonalPosi"), DiagonalPosi);
        }
    }
}
