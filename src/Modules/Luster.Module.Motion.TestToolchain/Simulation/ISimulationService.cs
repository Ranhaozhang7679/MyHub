using System;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.TaskFlow.Engine.Models;

namespace Luster.Module.Motion.TestToolchain.Simulation
{
    /// <summary>
    /// 仿真服务接口（TES-68 P9-C 接口预留）。
    /// 为未来仿真前端（P6-E OCCT 3D 可视化就位后）提供统一的最小仿真能力入口，
    /// 复用已入库的 <see cref="OpticalSimulator"/>（脉宽→灰度）与 <see cref="SimulationProfile"/>（仿真几何参数）。
    /// </summary>
    /// <remarks>
    /// <b>范围冻结（2026-06-26 人类裁决）</b>：lmv 已有 <c>OfflineMode</c>（DeviceMode.Virtual）覆盖"虚拟模式"需求，
    /// 本接口不重建虚拟模式框架、不做仿真 UI、不碰 AOI#1 全模拟流程。当前仅预留接口 + 桩实现，
    /// 方法体抛 <see cref="NotImplementedException"/>，待 P6-E 仿真前端就位后接入真实实现。
    /// 已入库的 <see cref="VirtualModeMapping"/> 保留不动，不在本接口扩展。
    /// </remarks>
    public interface ISimulationService
    {
        /// <summary>
        /// 加载仿真几何参数（底板/Y轴/A轴形状+偏移+A轴旋转中心）。
        /// 对应源端 <c>SimulationProfile</c> 的几何配置载体。
        /// </summary>
        /// <param name="profile">仿真几何参数（null 抛 ArgumentNullException）。</param>
        void LoadGeometry(SimulationProfile profile);

        /// <summary>
        /// 获取当前已加载的仿真几何参数（未加载时返回默认实例）。
        /// </summary>
        SimulationProfile GetGeometry();

        /// <summary>
        /// 光学模拟器脉宽→灰度调用入口（包装 <see cref="OpticalSimulator.SimulateCamera"/>）。
        /// 用于无硬件环境下离线光调参验证，不依赖真实相机。
        /// </summary>
        /// <param name="rPulse">R 通道脉宽</param>
        /// <param name="gPulse">G 通道脉宽</param>
        /// <param name="bPulse">B 通道脉宽</param>
        /// <param name="lightWeightR">R 光源亮度权重（0-1）</param>
        /// <param name="lightWeightG">G 光源亮度权重</param>
        /// <param name="lightWeightB">B 光源亮度权重</param>
        /// <returns>RGB 灰度值（0-255，钳位取整）。</returns>
        OpticalSimulator.RGBValues SimulatePulseToGray(int rPulse, int gPulse, int bPulse,
                                                       float lightWeightR, float lightWeightG, float lightWeightB);

        /// <summary>
        /// 当前是否处于虚拟（离线）模式。委托 <see cref="VirtualModeMapping.IsVirtualMode"/>。
        /// </summary>
        bool IsVirtualMode { get; }
    }

    /// <summary>
    /// <see cref="ISimulationService"/> 桩实现（TES-68 P9-C 接口预留）。
    /// 方法体抛 <see cref="NotImplementedException"/>，仅满足接口契约与编译，待 P6-E 仿真前端就位后替换为真实实现。
    /// </summary>
    public sealed class SimulationServiceStub : ISimulationService
    {
        /// <summary>已入库的光学模拟器实例（脉宽→灰度算法已对齐源端，可直接复用）</summary>
        private readonly OpticalSimulator _opticalSimulator = new OpticalSimulator();

        /// <summary>当前已加载的仿真几何参数（桩实现下保持默认实例）</summary>
        private SimulationProfile _geometry = new SimulationProfile();

        /// <inheritdoc />
        public void LoadGeometry(SimulationProfile profile)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            // 桩实现：暂不持久化到配置体系，仅内存持有。P6-E 接入后落 SystemConfig/Recipe。
            _geometry = profile;
            throw new NotImplementedException("TES-68 P9-C: 仿真几何参数加载待 P6-E 仿真前端就位后实现。");
        }

        /// <inheritdoc />
        public SimulationProfile GetGeometry()
        {
            // 桩实现：返回内存持有的几何参数（未 LoadGeometry 时为默认实例）。
            return _geometry;
        }

        /// <inheritdoc />
        public OpticalSimulator.RGBValues SimulatePulseToGray(int rPulse, int gPulse, int bPulse,
                                                              float lightWeightR, float lightWeightG, float lightWeightB)
        {
            // 已入库算法可直接复用（确定性、已对齐源端），不抛 NotImplementedException，
            // 便于 P6-E 前端就位前即可离线验证光调参链路。
            return _opticalSimulator.SimulateCamera(rPulse, gPulse, bPulse,
                                                    lightWeightR, lightWeightG, lightWeightB);
        }

        /// <inheritdoc />
        public bool IsVirtualMode
        {
            get
            {
                // 桩实现：未持 SystemConfig 实例，暂返回 false。P6-E 接入后委托
                // VirtualModeMapping.IsVirtualMode(SystemConfig)。
                throw new NotImplementedException("TES-68 P9-C: 虚拟模式判定待 P6-E 接入 SystemConfig 后实现。");
            }
        }
    }
}
