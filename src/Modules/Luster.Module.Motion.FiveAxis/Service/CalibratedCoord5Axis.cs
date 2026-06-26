using Luster.Motion.FiveAxis.Kinematics;
using Luster.Motion.FiveAxis.Position;
using MathNet.Numerics.LinearAlgebra;
using System;

namespace Luster.Motion.FiveAxis.Service
{
    /// <summary>
    /// 标定后的五轴运动学包装(P2-B「CalibratedCoord5Axis 包装 Coord5Axis」)。
    /// 源端语义(见 Form5Cali.laserCaliApply):运行时结构参数 FiveAxisPara = Accurate5Para.TransForm(CameraOffset),
    /// 即把精确标定结果按相机-激光偏移平移后作为运行时 Coord5Axis。本类封装该语义:持有精确标定参数 + 偏移,
    /// 暴露有效 Coord5Axis(<see cref="EffectivePara"/>)并委托正逆解,供检测站/算子节点调用,不修改 Coord5Axis 本体。
    /// </summary>
    public class CalibratedCoord5Axis
    {
        /// <summary>精确标定得到的五轴参数</summary>
        public Coord5Axis Accurate5Para { get; }

        /// <summary>相机-激光偏移(激光标定产生,作用于精确参数得到运行时参数)</summary>
        public PositionXYZ CameraOffset { get; }

        /// <summary>运行时有效五轴参数(Accurate5Para.TransForm(CameraOffset))</summary>
        public Coord5Axis EffectivePara { get; }

        public CalibratedCoord5Axis(Coord5Axis accurate5Para, PositionXYZ cameraOffset)
        {
            this.Accurate5Para = accurate5Para ?? throw new ArgumentNullException(nameof(accurate5Para));
            this.CameraOffset = cameraOffset ?? new PositionXYZ();
            this.EffectivePara = this.Accurate5Para.TransForm(this.CameraOffset);
        }

        public Matrix<double> GetOrg2DestMatrix(double rx, double rz) => EffectivePara.GetOrg2DestMatrix(rx, rz);
        public Matrix<double> GetDest2OrgMatrix(double rx, double rz) => EffectivePara.GetDest2OrgMatrix(rx, rz);
        public PositionXYZ PointO2D(double rx, double rz, PositionXYZ o) => EffectivePara.PointO2D(rx, rz, o);
        public PositionXYZ PointD2O(double rx, double rz, PositionXYZ d) => EffectivePara.PointD2O(rx, rz, d);
        public PositionXYZ RotateO2D(double rx, double rz, PositionXYZ oVec) => EffectivePara.RotateO2D(rx, rz, oVec);
        public PositionXYZ RotateD2O(double rx, double rz, PositionXYZ dVec) => EffectivePara.RotateD2O(rx, rz, dVec);
        public PositionXYZRxRyRz PoseO2D(PositionXYZRxRyRz o) => EffectivePara.PoseO2D(o);
        public PositionXYZRxRyRz PoseD2O(PositionXYZRxRyRz d) => EffectivePara.PoseD2O(d);
    }
}
