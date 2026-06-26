using Luster.Motion.FiveAxis.Kinematics;
using Luster.Motion.FiveAxis.Position;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Motion;
using System;
using System.ComponentModel;

namespace Luster.Module.Motion.FiveAxis.Functions
{
    /// <summary>
    /// 五轴运动学正逆解算子节点(P5-2)。
    /// 包装 <see cref="Coord5Axis"/>:IN 工件/目标坐标 + 旋转角 rx/rz + Coord5Axis 参数(ACenter/ADir/CCenter/CDir/CirPulses),
    /// 调 Coord5Axis.PointO2D(正解)/PointD2O(逆解),OUT 5 轴目标 X/Y/Z/U/V(喂 MultiAxis/AxisContinuousMove)。
    /// 参数契约与源端 FiveAxisPara(即 Coord5Axis,P2-C)对齐。
    /// 经 FiveAxisModule.AddFunction 注册,可由 XML recipe 编排(TES-29 MotionFunction 节点范式)。
    /// </summary>
    /// <remarks>
    /// Coord5Axis 配置字段(ACenter/ADir/CCenter/CDir)源端为 PositionXYZ 复合类型,此处按 P6-A ParamGrid 范式
    /// 展平为标量 [Parameter](X/Y/Z),ParamGrid 直接可编辑、recipe 可序列化;P6-A 可再切换为复合展开型。
    /// 算法本体零改动(路 X 原样迁 MathNet),可还原性 diff &lt;=1e-6 不受影响。
    /// </remarks>
    public class FiveAxisKinematicsNode : MotionFunction
    {
        /// <summary>正逆解模式</summary>
        public enum KinematicsMode
        {
            [Description("正解(工件→目标)")]
            Org2Dest,

            [Description("逆解(目标→工件)")]
            Dest2Org,
        }

        public FiveAxisKinematicsNode()
        {
            this.Tips = "五轴正逆解";
        }

        #region 输入:旋转角 + 工件/目标坐标

        /// <summary>A 轴旋转角 rx(度)</summary>
        [Parameter("A轴旋转角rx(度)", 0, Group = "输入", CN = "RX", DefaultV = 0.0)]
        public virtual double RX { get; set; }

        /// <summary>C 轴旋转角 rz(度)</summary>
        [Parameter("C轴旋转角rz(度)", 1, Group = "输入", CN = "RZ", DefaultV = 0.0)]
        public virtual double RZ { get; set; }

        /// <summary>输入坐标 X(正解=工件坐标,逆解=目标坐标)</summary>
        [Parameter("输入坐标X", 2, Group = "输入", CN = "输入X", DefaultV = 0.0)]
        public virtual double InputX { get; set; }

        /// <summary>输入坐标 Y</summary>
        [Parameter("输入坐标Y", 3, Group = "输入", CN = "输入Y", DefaultV = 0.0)]
        public virtual double InputY { get; set; }

        /// <summary>输入坐标 Z</summary>
        [Parameter("输入坐标Z", 4, Group = "输入", CN = "输入Z", DefaultV = 0.0)]
        public virtual double InputZ { get; set; }

        /// <summary>正逆解模式</summary>
        [Parameter("正逆解模式", 5, Group = "输入", CN = "模式", DefaultV = KinematicsMode.Org2Dest)]
        public virtual KinematicsMode Mode { get; set; }

        #endregion

        #region 输入:Coord5Axis 参数(A 轴)

        [Parameter("A旋转中心X", 10, Group = "A轴", CN = "ACenterX", DefaultV = 1.0)]
        public virtual double ACenterX { get; set; }

        [Parameter("A旋转中心Y", 11, Group = "A轴", CN = "ACenterY", DefaultV = 0.0)]
        public virtual double ACenterY { get; set; }

        [Parameter("A旋转中心Z", 12, Group = "A轴", CN = "ACenterZ", DefaultV = 0.0)]
        public virtual double ACenterZ { get; set; }

        [Parameter("A轴方向矢量X", 13, Group = "A轴", CN = "ADirX", DefaultV = 0.0)]
        public virtual double ADirX { get; set; }

        [Parameter("A轴方向矢量Y", 14, Group = "A轴", CN = "ADirY", DefaultV = 0.0)]
        public virtual double ADirY { get; set; }

        [Parameter("A轴方向矢量Z", 15, Group = "A轴", CN = "ADirZ", DefaultV = 0.0)]
        public virtual double ADirZ { get; set; }

        [Parameter("A一圈脉冲数", 16, Group = "A轴", CN = "ACirPulses", DefaultV = 0.0)]
        public virtual double ACirPulses { get; set; }

        #endregion

        #region 输入:Coord5Axis 参数(C 轴)

        [Parameter("C旋转中心X", 20, Group = "C轴", CN = "CCenterX", DefaultV = 0.0)]
        public virtual double CCenterX { get; set; }

        [Parameter("C旋转中心Y", 21, Group = "C轴", CN = "CCenterY", DefaultV = 0.0)]
        public virtual double CCenterY { get; set; }

        [Parameter("C旋转中心Z", 22, Group = "C轴", CN = "CCenterZ", DefaultV = 0.0)]
        public virtual double CCenterZ { get; set; }

        [Parameter("C轴方向矢量X", 23, Group = "C轴", CN = "CDirX", DefaultV = 0.0)]
        public virtual double CDirX { get; set; }

        [Parameter("C轴方向矢量Y", 24, Group = "C轴", CN = "CDirY", DefaultV = 0.0)]
        public virtual double CDirY { get; set; }

        [Parameter("C轴方向矢量Z", 25, Group = "C轴", CN = "CDirZ", DefaultV = 1.0)]
        public virtual double CDirZ { get; set; }

        [Parameter("C一圈脉冲数", 26, Group = "C轴", CN = "CCirPulses", DefaultV = 0.0)]
        public virtual double CCirPulses { get; set; }

        #endregion

        #region 输出:5 轴目标(X/Y/Z/U/V)

        /// <summary>目标 X(变换后坐标)</summary>
        [Parameter("目标X", 50, Group = "输出", CN = "目标X", ParamType = ParamType.OUT)]
        public virtual double TargetX { get; set; }

        /// <summary>目标 Y</summary>
        [Parameter("目标Y", 51, Group = "输出", CN = "目标Y", ParamType = ParamType.OUT)]
        public virtual double TargetY { get; set; }

        /// <summary>目标 Z</summary>
        [Parameter("目标Z", 52, Group = "输出", CN = "目标Z", ParamType = ParamType.OUT)]
        public virtual double TargetZ { get; set; }

        /// <summary>目标 U(= rx,3+2 构型旋转轴)</summary>
        [Parameter("目标U(=RX)", 53, Group = "输出", CN = "目标U", ParamType = ParamType.OUT)]
        public virtual double TargetU { get; set; }

        /// <summary>目标 V(= rz,3+2 构型 V=旋转R)</summary>
        [Parameter("目标V(=RZ)", 54, Group = "输出", CN = "目标V", ParamType = ParamType.OUT)]
        public virtual double TargetV { get; set; }

        #endregion

        /// <summary>
        /// 由标量参数构造 Coord5Axis 运动学核心(算法本体原样迁,未改)。
        /// </summary>
        private Coord5Axis BuildCoord5Axis()
        {
            var coord = new Coord5Axis
            {
                ACenter = new PositionXYZ(ACenterX, ACenterY, ACenterZ),
                ADir = new PositionXYZ(ADirX, ADirY, ADirZ),
                ACirPulses = ACirPulses,
                CCenter = new PositionXYZ(CCenterX, CCenterY, CCenterZ),
                CDir = new PositionXYZ(CDirX, CDirY, CDirZ),
                CCirPulses = CCirPulses,
            };
            return coord;
        }

        public override bool DoExcute(out string errMsg)
        {
            errMsg = string.Empty;

            var coord = BuildCoord5Axis();
            var input = new PositionXYZ(InputX, InputY, InputZ);

            // 正解 PointO2D(工件→目标)/ 逆解 PointD2O(目标→工件),均原样调 Coord5Axis
            PositionXYZ result = Mode == KinematicsMode.Dest2Org
                ? coord.PointD2O(RX, RZ, input)
                : coord.PointO2D(RX, RZ, input);

            TargetX = result.X;
            TargetY = result.Y;
            TargetZ = result.Z;
            // 3+2 构型:旋转轴目标 = 旋转角命令(U=rx,V=rz),直线轴目标 = 变换后 XYZ
            TargetU = RX;
            TargetV = RZ;

            MyOwner?.OnLog(Luster.Common.DataStruct.Enums.LogType.Debug,
                $"FiveAxisKinematics({Mode}): in=({InputX},{InputY},{InputZ}) rx/rz=({RX},{RZ}) -> out=({TargetX},{TargetY},{TargetZ}) U/V=({TargetU},{TargetV})");

            return true;
        }
    }
}
