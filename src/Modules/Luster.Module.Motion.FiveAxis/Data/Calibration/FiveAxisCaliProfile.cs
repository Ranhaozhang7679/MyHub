using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using System;
using System.Xml;
using System.Xml.Linq;

namespace Luster.Motion.FiveAxis.Data.Calibration
{
    /// <summary>
    /// 五轴自动标定配置/结果根模型(对应源端 AutoCaliProfile)。
    /// 聚合四阶段标定结果:激光标定(LaserCali)/ 五轴粗略标定(RoughCali)/ 五轴精确标定(AccurateCali)/ 工件原点示教(WorkOriginCali)+ 一致性点检(UniformityCheck)。
    /// 序列化机制:平台 IXMLParser(XElement round-trip),替代源端 [Serializable]+FieldToPropertyTypeDescriptor 反射 + WinForm PropertyGrid 特性。
    /// 三处 Coord5Axis 实例(Rough5Para/Accurate5Para/运行时 FiveAxisPara)语义保真:Coord5Axis 本体由 P5-2 迁入,此处只保其 6 个结构参数序列化往返(内含 PositionXYZ,无 Matrix 字段)。
    /// </summary>
    [Serializable]
    public class FiveAxisCaliProfile : IXMLParser
    {
        public FiveAxisCaliProfile()
        {
            this.BallSampleSpan = 3;
            this.BallRadius = 31.741 / 2;
            this.CaliDelay = 200;
            this.LaserValidOffset = 5;
            this.UniformityCheck = new UniformityCheckResult();
            this.LaserCali = new LaserCaliResult();
            this.RoughCali = new RoughCaliResult();
            this.AccurateCali = new AccurateCaliResult();
            this.WorkOriginCali = new TeachWorkOriginResult();
        }
        public FiveAxisCaliProfile(FiveAxisCaliProfile other) : this()
        {
            this.CopyFrom(other);
        }
        public void CopyFrom(FiveAxisCaliProfile obj)
        {
            this.BallSampleSpan = obj.BallSampleSpan;
            this.BallRadius = obj.BallRadius;
            this.CaliDelay = obj.CaliDelay;
            this.LaserValidOffset = obj.LaserValidOffset;
            this.UniformityCheck.CopyFrom(obj.UniformityCheck);
            this.LaserCali.CopyFrom(obj.LaserCali);
            this.RoughCali.CopyFrom(obj.RoughCali);
            this.AccurateCali.CopyFrom(obj.AccurateCali);
            this.WorkOriginCali.CopyFrom(obj.WorkOriginCali);
        }

        /// <summary>球采样间距(5 点采样)</summary>
        public double BallSampleSpan { get; set; }
        /// <summary>标准球半径</summary>
        public double BallRadius { get; set; }
        /// <summary>标定延时(ms)</summary>
        public int CaliDelay { get; set; }
        /// <summary>激光测量值限制</summary>
        public double LaserValidOffset { get; set; }
        /// <summary>一致性点检</summary>
        public UniformityCheckResult UniformityCheck { get; set; }
        /// <summary>激光标定</summary>
        public LaserCaliResult LaserCali { get; set; }
        /// <summary>五轴粗略标定</summary>
        public RoughCaliResult RoughCali { get; set; }
        /// <summary>五轴精确标定</summary>
        public AccurateCaliResult AccurateCali { get; set; }
        /// <summary>工件原点示教</summary>
        public TeachWorkOriginResult WorkOriginCali { get; set; }

        public XElement ExportXml()
        {
            XElement xRoot = new XElement("FiveAxisCaliProfile");
            xRoot.SetAttributeValue("BallSampleSpan", XmlConvert.ToString(BallSampleSpan));
            xRoot.SetAttributeValue("BallRadius", XmlConvert.ToString(BallRadius));
            xRoot.SetAttributeValue("CaliDelay", XmlConvert.ToString(CaliDelay));
            xRoot.SetAttributeValue("LaserValidOffset", XmlConvert.ToString(LaserValidOffset));
            xRoot.Add(UniformityCheck.ExportXml());
            xRoot.Add(LaserCali.ExportXml());
            xRoot.Add(RoughCali.ExportXml());
            xRoot.Add(AccurateCali.ExportXml());
            xRoot.Add(WorkOriginCali.ExportXml());
            return xRoot;
        }

        public void ParserXml(XElement xElement)
        {
            xElement.GetAttribute("BallSampleSpan", v => BallSampleSpan = XmlConvert.ToDouble(v));
            xElement.GetAttribute("BallRadius", v => BallRadius = XmlConvert.ToDouble(v));
            xElement.GetAttribute("CaliDelay", v => CaliDelay = XmlConvert.ToInt32(v));
            xElement.GetAttribute("LaserValidOffset", v => LaserValidOffset = XmlConvert.ToDouble(v));
            UniformityCheck.ParserXml(xElement.Element("UniformityCheckResult"));
            LaserCali.ParserXml(xElement.Element("LaserCaliResult"));
            RoughCali.ParserXml(xElement.Element("RoughCaliResult"));
            AccurateCali.ParserXml(xElement.Element("AccurateCaliResult"));
            WorkOriginCali.ParserXml(xElement.Element("TeachWorkOriginResult"));
        }
    }
}
