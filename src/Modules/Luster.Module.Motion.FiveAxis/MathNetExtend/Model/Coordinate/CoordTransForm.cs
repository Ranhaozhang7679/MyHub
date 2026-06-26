using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using Luster.Motion.FiveAxis.Position;
using Luster.Motion.FiveAxis.Utils;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Xml.Linq;

namespace Luster.Motion.FiveAxis.Coordinate
{
    /// <summary>
    /// 坐标系转换参数(旋转为角度)。
    /// 原样迁自源端 SP-5140 MathNetExtend.Model.Coordinate.CoordTransForm(UI 耦合剥离:去 FieldToPropertyTypeDescriptor 基类 + WinForm 特性,改普通类)。
    /// 算法本体(GetOrg2DestMatrix/PointO2D/PoseO2D 等)与源端一致,内含 MathNet Matrix&lt;double&gt; 仅作方法内局部计算(无字段),序列化只保 Trans(PositionXYZRxRyRz)。
    /// P5-5 增补 IXMLParser:供 TeachWorkOriginResult.RltTool2Work 落盘/加载往返。
    /// </summary>
    [Serializable]
    public class CoordTransForm : IXMLParser
    {
        public CoordTransForm()
        {
            this.Trans = new PositionXYZRxRyRz();
        }
        public CoordTransForm(CoordTransForm other) : this()
        {
            this.CopyFrom(other);
        }
        public void CopyFrom(CoordTransForm other)
        {
            this.Trans.CopyFrom(other.Trans);
        }

        /// <summary>坐标系转换参数(旋转为角度)</summary>
        public PositionXYZRxRyRz Trans { get; set; }

        public Matrix<double> GetDest2OrgMatrix()
        {
            return GetOrg2DestMatrix().Inverse();
        }
        public Matrix<double> GetOrg2DestMatrix()
        {
            return Coordinate3dHelper.getWorld2WorkMatrix(Trans.X, Trans.Y, Trans.Z, AngleHelper.AngleToRad(Trans.RX), AngleHelper.AngleToRad(Trans.RY), AngleHelper.AngleToRad(Trans.RZ));
        }
        public PositionXYZ PointO2D(PositionXYZ o)
        {
            var vec = GetOrg2DestMatrix() * Coordinate3dHelper.ToVector(o, true);
            return Coordinate3dHelper.ToPosition(vec);
        }
        public PositionXYZ PointD2O(PositionXYZ d)
        {
            var vec = GetDest2OrgMatrix() * Coordinate3dHelper.ToVector(d, true);
            return Coordinate3dHelper.ToPosition(vec);
        }
        public PositionXYZ RotateO2D(PositionXYZ oVec)
        {
            return PointO2D(oVec) - PointO2D(new PositionXYZ()) as PositionXYZ;
        }
        public PositionXYZ RotateD2O(PositionXYZ dVec)
        {
            return PointD2O(dVec) - PointD2O(new PositionXYZ()) as PositionXYZ;
        }
        public PositionXYZRxRyRz PoseO2D(PositionXYZRxRyRz o)
        {
            var dp = PointO2D(o);
            return new PositionXYZRxRyRz()
            {
                X = dp.X,
                Y = dp.Y,
                Z = dp.Z,
                RX = o.RX - Trans.RX,
                RY = o.RY - Trans.RY,
                RZ = o.RZ - Trans.RZ,
            };
        }
        public PositionXYZRxRyRz PoseD2O(PositionXYZRxRyRz d)
        {
            var op = PointD2O(d);
            return new PositionXYZRxRyRz()
            {
                X = op.X,
                Y = op.Y,
                Z = op.Z,
                RX = d.RX + Trans.RX,
                RY = d.RY + Trans.RY,
                RZ = d.RZ + Trans.RZ,
            };
        }

        /// <summary>导出 Xml(只保 Trans)</summary>
        public XElement ExportXml()
        {
            XElement xRoot = new XElement("CoordTransForm");
            xRoot.Add(CalibrationXml.ToXml(Trans, "Trans"));
            return xRoot;
        }

        /// <summary>解析 Xml</summary>
        public void ParserXml(XElement xElement)
        {
            var xTrans = xElement?.Element("Trans");
            if (xTrans != null) CalibrationXml.FromXml(xTrans, Trans);
        }
    }
}
