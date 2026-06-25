using Luster.Motion.DataStruct.Enums;
using Luster.Motion.TaskFlow.Engine.Models;

namespace Luster.Module.Motion.TestToolchain.Simulation
{
    /// <summary>
    /// 虚拟模式映射（TES-34 P9-C）。
    /// 源端 SP-2025140 用 <c>ProfilePluginManager.VIRTUAL_MODE</c>（static bool）作为全局短路 flag，
    /// 散布于 ~150 处组件 return true。lmv 已有 <see cref="DeviceMode"/> 枚举 +
    /// <c>Luster.SimDevice.SubSystem</c>（VCamera/VLineLaser/IOSim）真正的虚拟设备层，
    /// 无需搬移源端散点短路。本类提供源端开关 → lmv RunMode 的映射说明与辅助判断。
    /// </summary>
    /// <remarks>
    /// <b>源端 IO 语义警告</b>：源端 InputComponent 在 VIRTUAL_MODE 下 return false（与“成功”语义相反，
    /// 可能有意模拟“无物料”）。lmv 侧 IOSim 不照搬此语义，读 IO 真实反映虚拟 IO 状态。
    /// </remarks>
    public static class VirtualModeMapping
    {
        /// <summary>
        /// 判断当前是否处于虚拟（离线）模式。
        /// 对应源端 <c>ProfilePluginManager.VIRTUAL_MODE == true</c>。
        /// </summary>
        public static bool IsVirtualMode(SystemConfig config)
        {
            return config != null && config.RunMode == DeviceMode.Virtual;
        }

        /// <summary>
        /// 切换虚拟/实机模式（TES-68 P9-C 验收点：VIRTUAL_MODE 全局开关可正常切换）。
        /// 源端 <c>ProfilePluginManager.VIRTUAL_MODE</c>（static bool）在 lmv 对应
        /// <c>SystemConfig.RunMode == DeviceMode.Virtual</c>，本方法提供与源端等价的切换接口。
        /// </summary>
        /// <param name="config">系统配置（null 时安全跳过，对齐 IsVirtualMode 的 null 容错语义）。</param>
        /// <param name="enabled">true→Virtual（离线仿真）；false→Real（实机）。</param>
        public static void SetVirtualMode(SystemConfig config, bool enabled)
        {
            if (config != null)
            {
                config.RunMode = enabled ? DeviceMode.Virtual : DeviceMode.Real;
            }
        }

        /// <summary>源端开关：ProfilePluginManager.VIRTUAL_MODE（static bool，PluginComponent.Start() 置 true）</summary>
        public const string SourceVirtualModeSwitch = "ProfilePluginManager.VIRTUAL_MODE";

        /// <summary>lmv 对应开关：SystemConfig.RunMode == DeviceMode.Virtual（SystemConfig 默认即 Virtual）</summary>
        public const string TargetVirtualModeSwitch = "SystemConfig.RunMode == DeviceMode.Virtual";
    }
}
