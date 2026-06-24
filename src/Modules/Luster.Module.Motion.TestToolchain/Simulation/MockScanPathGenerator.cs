using System.Collections.Generic;

namespace Luster.Module.Motion.TestToolchain.Simulation
{
    /// <summary>
    /// Mock 5 轴扫描路径生成器（TES-68 P9-C wiring 项 3）。
    /// 消费 <see cref="SimulationProfile"/> 7 几何字段生成 mock 扫描点位,
    /// 喂给残留 <c>MotorPosiHelper</c> 软件运动学(替代被 VIRTUAL_MODE 旁路的 RTCP 正逆解),
    /// 使 AOI#1 全模拟流程在无 P5 五轴链下可跑通。
    /// </summary>
    /// <remarks>
    /// 源端 <c>SimulationProfile</c> 是孤儿类(无消费方),本生成器是其在虚拟模式下的首个消费方。
    /// 生成的点位是 mock 几何(底板/Y/A 轴形状 + 偏移 + 旋转中心),不依赖真实 Coord5Axis 标定参数。
    /// 真实五轴运动学属 P5 链(TES-97),虚拟模式旁路硬件正逆解,只需 mock 几何跑流程。
    /// </remarks>
    public class MockScanPathGenerator
    {
        /// <summary>
        /// 按 SimulationProfile 几何生成扫描点位序列。
        /// </summary>
        /// <param name="profile">仿真几何参数</param>
        /// <param name="steps">每轴采样步数(默认 10)</param>
        public IReadOnlyList<SimPoint3> GenerateScanPath(SimulationProfile profile, int steps = 10)
        {
            var path = new List<SimPoint3>();
            if (profile == null || steps <= 0) return path;

            // 基于底板形状 + 偏移生成底板扫描线(模拟 AOI 检测扫描轨迹)
            var floorStart = profile.FloorOffsetPosi;
            var floorShape = profile.FloorShape;
            for (int i = 0; i < steps; i++)
            {
                double t = (double)i / (steps - 1);
                path.Add(new SimPoint3
                {
                    X = floorStart.X + floorShape.X * t,
                    Y = floorStart.Y + floorShape.Y * t,
                    Z = floorStart.Z + floorShape.Z * t
                });
            }

            // Y 轴形状 + 偏移:Y 方向扫描
            var yStart = profile.YOffsetPosi;
            var yShape = profile.YShape;
            for (int i = 0; i < steps; i++)
            {
                double t = (double)i / (steps - 1);
                path.Add(new SimPoint3
                {
                    X = yStart.X,
                    Y = yStart.Y + yShape.Y * t,
                    Z = yStart.Z
                });
            }

            // A 轴形状 + 偏移 + 旋转中心:A 轴旋转扫描(模拟五轴 A 轴旋转检测)
            var aStart = profile.AOffsetPosi;
            var aShape = profile.AShape;
            var aCenter = profile.ARotateCenter;
            for (int i = 0; i < steps; i++)
            {
                double t = (double)i / (steps - 1);
                // 围绕旋转中心生成 A 轴旋转点位(mock,非真实运动学)
                path.Add(new SimPoint3
                {
                    X = aCenter.X + (aStart.X - aCenter.X) + aShape.X * t,
                    Y = aCenter.Y + (aStart.Y - aCenter.Y),
                    Z = aCenter.Z + (aStart.Z - aCenter.Z) + aShape.Z * t
                });
            }

            return path;
        }

        /// <summary>
        /// 路径点数判定(纯逻辑,便于单测)。
        /// </summary>
        public static int ExpectedPointCount(int steps)
        {
            // 底板 + Y 轴 + A 轴 各 steps 点
            return steps <= 0 ? 0 : steps * 3;
        }
    }
}
