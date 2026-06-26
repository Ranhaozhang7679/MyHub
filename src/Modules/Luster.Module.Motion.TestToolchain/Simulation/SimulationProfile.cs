namespace Luster.Module.Motion.TestToolchain.Simulation
{
    /// <summary>
    /// 仿真几何参数结构（TES-34 P9-C，迁移自源端 SimulationProfile.cs）。
    /// 底板/Y 轴/A 轴的形状与偏移 + A 轴旋转中心，用于 5 轴仿真几何标定。
    /// </summary>
    /// <remarks>
    /// <b>源端状态</b>：源端 SimulationProfile 为孤儿类（全仓无消费方、无序列化点）。
    /// 本类等价定义几何结构预留，供后续虚拟模式几何仿真使用。点类型用 <see cref="SimPoint3"/>（自包含，零依赖）。
    /// </remarks>
    public class SimulationProfile
    {
        /// <summary>底板形状</summary>
        public SimPoint3 FloorShape { get; set; } = new SimPoint3();

        /// <summary>底板偏移</summary>
        public SimPoint3 FloorOffsetPosi { get; set; } = new SimPoint3();

        /// <summary>Y 轴形状</summary>
        public SimPoint3 YShape { get; set; } = new SimPoint3();

        /// <summary>Y 轴偏移</summary>
        public SimPoint3 YOffsetPosi { get; set; } = new SimPoint3();

        /// <summary>A 轴形状</summary>
        public SimPoint3 AShape { get; set; } = new SimPoint3();

        /// <summary>A 轴偏移</summary>
        public SimPoint3 AOffsetPosi { get; set; } = new SimPoint3();

        /// <summary>A 轴旋转中心</summary>
        public SimPoint3 ARotateCenter { get; set; } = new SimPoint3();

        public SimulationProfile() { }

        public SimulationProfile(SimulationProfile other)
        {
            if (other != null) CopyFrom(other);
        }

        /// <summary>逐字段复制（对齐源端 CopyFrom）</summary>
        public void CopyFrom(SimulationProfile other)
        {
            if (other == null) return;
            FloorShape = new SimPoint3(other.FloorShape);
            FloorOffsetPosi = new SimPoint3(other.FloorOffsetPosi);
            YShape = new SimPoint3(other.YShape);
            YOffsetPosi = new SimPoint3(other.YOffsetPosi);
            AShape = new SimPoint3(other.AShape);
            AOffsetPosi = new SimPoint3(other.AOffsetPosi);
            ARotateCenter = new SimPoint3(other.ARotateCenter);
        }
    }

    /// <summary>
    /// 仿真几何点（X/Y/Z），对应源端 PositionXYZ。
    /// 自包含结构，避免引入 ThreeD.Algorithm 或 MathNet 依赖。
    /// </summary>
    public struct SimPoint3
    {
        public double X;
        public double Y;
        public double Z;

        public SimPoint3(double x = 0, double y = 0, double z = 0)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public SimPoint3(SimPoint3 other)
        {
            X = other.X;
            Y = other.Y;
            Z = other.Z;
        }
    }
}
