using Luster.Motion.DataStruct.Enums;

namespace Luster.Motion.DataStruct.Interfaces
{
    /// <summary>
    /// 安全输入快照（ADR-C seam）。
    /// <see cref="InterlockMatrix"/> 的 <c>Evaluate</c> 唯一输入——TES-28 P0-G 恢复闭环状态机
    /// 不直接读 IO，只消费 <c>InterlockMatrix.Evaluate(IInputSnapshot)</c> 的判定结果。
    /// 实现方负责把 VIO/VAxis/HandoverNode 的实时状态投影成 <see cref="SafetyInputKind"/> 维度的布尔值。
    /// </summary>
    public interface IInputSnapshot
    {
        /// <summary>
        /// 查询指定安全输入维度当前是否触发。
        /// </summary>
        /// <param name="kind">输入维度</param>
        /// <param name="target">
        /// 目标对象名（可选）：轴名(AxisLimitPos/Neg/ServoAlarm)、IO 设备名(EStop/DoorSafety/DoorLock)、
        /// 上游/下游站点名(Upstream/DownstreamInterlock)。null 表示该维度的默认/聚合对象。
        /// </param>
        /// <returns>true=当前触发（不安全状态）</returns>
        bool IsTriggered(SafetyInputKind kind, string target = null);
    }
}
