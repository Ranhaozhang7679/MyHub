using Luster.Common.DataStruct.Interfaces;
using Luster.Motion.FiveAxis.Coordinate;
using Luster.Motion.FiveAxis.Kinematics;
using System;
using System.Xml.Linq;

namespace Luster.Motion.FiveAxis.Data
{
    /// <summary>
    /// 五轴运行时结构参数数据模型(对应源端 Check5AxisBaseProfile.FiveAxisPara: Coord5Axis)。
    /// 继承 Coord5Axis(P5-2 迁入,本体算法不动)+ 增补 IXMLParser:6 个结构参数(ACenter/ADir/ACirPulses/CCenter/CDir/CCirPulses)XElement 往返,
    /// 供运行配置随 recipe XML 落盘/加载。子类化增补接口,不修改 Coord5Axis 源文件(R1 非侵入 P5-2 产出)。
    /// </summary>
    [Serializable]
    public class FiveAxisPara : Coord5Axis, IXMLParser
    {
        public FiveAxisPara() : base() { }
        public FiveAxisPara(Coord5Axis other) : base(other) { }

        public XElement ExportXml()
        {
            return CalibrationXml.ToXml(this, "FiveAxisPara");
        }

        public void ParserXml(XElement xElement)
        {
            CalibrationXml.FromXml(xElement, this);
        }
    }
}
