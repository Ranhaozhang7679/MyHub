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

        /// <summary>源端开关：ProfilePluginManager.VIRTUAL_MODE（static bool，PluginComponent.Start() 置 true）</summary>
        public const string SourceVirtualModeSwitch = "ProfilePluginManager.VIRTUAL_MODE";

        /// <summary>lmv 对应开关：SystemConfig.RunMode == DeviceMode.Virtual（SystemConfig 默认即 Virtual）</summary>
        public const string TargetVirtualModeSwitch = "SystemConfig.RunMode == DeviceMode.Virtual";
    }
}
