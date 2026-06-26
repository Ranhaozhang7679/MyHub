using Luster.Motion.FiveAxis.Position;
using Luster.Motion.FiveAxis.Utils;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Luster.Motion.FiveAxis.Kinematics
{
    /// <summary>
    /// 五轴运动学核心(3+2 构型:X/Y/Z/U/V,V=旋转 R)。
    /// 原样迁自源端 SP-2025140 MathNetExtend.Model.Coordinate.Coord5Axis(路 X 决策:保留 Matrix&lt;double&gt; 实现,不重写自研矩阵)。
    /// UI 耦合剥离(非算法改动):去 FieldToPropertyTypeDescriptor 基类 + WinForm PropertyGrid 特性,
    /// 矩阵运算本体零改动,可还原性 diff &lt;=1e-6 不受影响。
    /// ACenter/ADir/CCenter/CDir/CirPulses 等字段对应源端 FiveAxisPara 数据模型(P2-C),节点层挂 lmv [Parameter] 替代原 WinForm 特性。
    /// </summary>
    [Serializable]
    public class Coord5Axis
    {
        public Coord5Axis()
        {
            this.ACenter = new PositionXYZ(1, 0, 0);
            this.ADir = new PositionXYZ();
            this.ACirPulses = 0;

            this.CCenter = new PositionXYZ();
            this.CDir = new PositionXYZ(0, 0, 1);
            this.CCirPulses = 0;
        }
        public Coord5Axis(Coord5Axis other) : this()
        {
            this.CopyFrom(other);
        }
        public void CopyFrom(Coord5Axis other)
        {
            this.ACenter.CopyFrom(other.ACenter);
            this.ADir.CopyFrom(other.ADir);
            this.ACirPulses = other.ACirPulses;

            this.CCenter.CopyFrom(other.CCenter);
            this.CDir.CopyFrom(other.CDir);
            this.CCirPulses = other.CCirPulses;
        }
        /// <summary>A 轴旋转中心到原点的偏移</summary>
        public PositionXYZ ACenter { get; set; }
        /// <summary>A 轴旋转中线的方向矢量</summary>
        public PositionXYZ ADir { get; set; }
        /// <summary>A 一圈脉冲数</summary>
        public double ACirPulses { get; set; }

        /// <summary>C 轴旋转中心到原点的偏移</summary>
        public PositionXYZ CCenter { get; set; }
        /// <summary>C 轴旋转中线的方向矢量</summary>
        public PositionXYZ CDir { get; set; }
        /// <summary>C 一圈脉冲数</summary>
        public double CCirPulses { get; set; }

        /// <summary>
        /// 原始空间到目标空间
        /// </summary>
        /// <param name="rx"></param>
        /// <param name="rz"></param>
        /// <returns></returns>
        public Matrix<double> GetOrg2DestMatrix(double rx, double rz)
        {
            double radx = AngleHelper.AngleToRad(rx);
            double radz = AngleHelper.AngleToRad(rz);
            Vector<double> rcp = Coordinate3dHelper.xyz2rcp(Coordinate3dHelper.ToVector(ADir));
            double h = rcp.At(1);
            double p = -rcp.At(2);
            var offsetA = Coordinate3dHelper.getWorld2WorkMatrix(ACenter.X, ACenter.Y, ACenter.Z, 0, 0, 0);
            var ra_c = Coordinate3dHelper.getWorld2WorkMatrix(0, 0, 0, 0, 0, h);
            var ra_b = Coordinate3dHelper.getWorld2WorkMatrix(0, 0, 0, 0, p, 0);
            var transRx = Coordinate3dHelper.getWorld2WorkMatrix(0, 0, 0, radx, 0, 0);
            var cur_c2a = ra_b * ra_c * offsetA * Coordinate3dHelper.ToVector(CCenter, true);
            var cur_cdir = DenseVector.OfArray((ra_b * ra_c * Coordinate3dHelper.ToVector(CDir, true)).Take(3).ToArray());
            var offsetC = Coordinate3dHelper.getWorld2WorkMatrix(cur_c2a.At(0), cur_c2a.At(1), cur_c2a.At(2), 0, 0, 0);
            Vector<double> rap = Coordinate3dHelper.xyz2rap(cur_cdir);
            double a = rap.At(1);
            p = rap.At(2);
            var rc_b = Coordinate3dHelper.getWorld2WorkMatrix(0, 0, 0, 0, p, 0);
            var rc_a = Coordinate3dHelper.getWorld2WorkMatrix(0, 0, 0, a, 0, 0);
            var transRz = Coordinate3dHelper.getWorld2WorkMatrix(0, 0, 0, 0, 0, radz);
            return transRz * rc_b * rc_a * offsetC * transRx * ra_b * ra_c * offsetA;
        }
        /// <summary>
        /// 目标空间到原始空间
        /// </summary>
        /// <param name="rx"></param>
        /// <param name="rz"></param>
        /// <returns></returns>
        public Matrix<double> GetDest2OrgMatrix(double rx, double rz)
        {
            return GetOrg2DestMatrix(rx, rz).Inverse();
        }
        /// <summary>
        /// 世界空间转工具空间
        /// </summary>
        /// <param name="rx"></param>
        /// <param name="rz"></param>
        /// <param name="o">世界空间（轴）</param>
        /// <returns></returns>
        public PositionXYZ PointO2D(double rx, double rz, PositionXYZ o)
        {
            var vec = GetOrg2DestMatrix(rx, rz) * Coordinate3dHelper.ToVector(o, true);
            return Coordinate3dHelper.ToPosition(vec);
        }

        /// <summary>
        /// 工具空间转世界空间
        /// </summary>
        /// <param name="rx"></param>
        /// <param name="rz"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public PositionXYZ PointD2O(double rx, double rz, PositionXYZ d)
        {
            var vec = GetDest2OrgMatrix(rx, rz) * Coordinate3dHelper.ToVector(d, true);
            return Coordinate3dHelper.ToPosition(vec);
        }
        /// <summary>
        /// 世界空间转工具空间
        /// </summary>
        /// <param name="rx"></param>
        /// <param name="rz"></param>
        /// <param name="oVec">世界空间（轴）</param>
        /// <returns></returns>
        public PositionXYZ RotateO2D(double rx, double rz, PositionXYZ oVec)
        {
            return PointO2D(rx, rz, oVec) - PointO2D(rx, rz, new PositionXYZ()) as PositionXYZ;
        }
        /// <summary>
        /// 工具空间转世界空间
        /// </summary>
        /// <param name="rx"></param>
        /// <param name="rz"></param>
        /// <param name="dVec"></param>
        /// <returns></returns>
        public PositionXYZ RotateD2O(double rx, double rz, PositionXYZ dVec)
        {
            return PointD2O(rx, rz, dVec) - PointD2O(rx, rz, new PositionXYZ()) as PositionXYZ;
        }

        /// <summary>
        /// 工具姿态转工件姿态
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public PositionXYZRxRyRz PoseTool2Work(PositionXYZRxRyRz t)
        {
            return new PositionXYZRxRyRz(t)
            {
                RX = -t.RX,
                RY = -t.RY,
                RZ = -t.RZ,
            };
        }
        /// <summary>
        /// 工件姿态转工具姿态
        /// </summary>
        /// <param name="w"></param>
        /// <returns></returns>
        public PositionXYZRxRyRz PoseWork2Tool(PositionXYZRxRyRz w)
        {
            return PoseTool2Work(w);
        }
        /// <summary>
        /// 世界空间转工具空间
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public PositionXYZRxRyRz PoseO2D(PositionXYZRxRyRz o)
        {
            var dp = PointO2D(o.RX, o.RZ, o);
            return new PositionXYZRxRyRz()
            {
                X = dp.X,
                Y = dp.Y,
                Z = dp.Z,
                RX = o.RX,
                RY = o.RY,
                RZ = o.RZ,
            };
        }
        /// <summary>
        /// 工具空间转世界空间
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public PositionXYZRxRyRz PoseD2O(PositionXYZRxRyRz d)
        {
            var op = PointD2O(d.RX, d.RZ, d);
            return new PositionXYZRxRyRz()
            {
                X = op.X,
                Y = op.Y,
                Z = op.Z,
                RX = d.RX,
                RY = d.RY,
                RZ = d.RZ,
            };
        }
        /// <summary>
        /// 工具偏移
        /// </summary>
        /// <param name="offset">偏移量</param>
        /// <returns></returns>
        public Coord5Axis TransForm(PositionXYZ offset)
        {
            Coord5Axis crd = new Coord5Axis(this);
            crd.ACenter = crd.ACenter + offset as PositionXYZ;
            crd.CCenter = crd.CCenter + offset as PositionXYZ;
            return crd;
        }
    }
}
