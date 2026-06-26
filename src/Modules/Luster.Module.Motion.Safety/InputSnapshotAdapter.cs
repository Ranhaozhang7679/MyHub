using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using Luster.TaskFlow.Motion;
using System;

namespace Luster.Module.Motion.Safety
{
    /// <summary>
    /// 默认 <see cref="IInputSnapshot"/> 实现（ADR-C seam 适配器）。
    /// 把 <see cref="IMotionModule"/>.DeviceEngine 中的 VIO/VAxis 实时状态
    /// 投影成 <see cref="SafetyInputKind"/> 维度的布尔值，供 <see cref="InterlockMatrix"/> 求值。
    /// </summary>
    /// <remarks>
    /// - EStop/DoorSafety/DoorLock：读 <see cref="VIO.GetDigitalIn()"/>，默认常闭（false=触发/不安全），对齐源端 NC 接线。
    /// - AxisLimitPos/Neg/ServoAlarm：读 <see cref="VAxis.GetAxisStatus(bool)"/> 的 Pel/Mel/Alarm 位。
    /// - Brake/UpstreamInterlock/DownstreamInterlock：待 TES-37 <c>HandoverNode</c> / 硬件接入，本轮返回 false。
    /// 站级可通过 <see cref="SafetyModule.RegisterSnapshotFactory"/> 注册自定义实现覆盖默认极性/映射。
    /// </remarks>
    public class InputSnapshotAdapter : IInputSnapshot
    {
        private readonly IMotionModule _module;

        public InputSnapshotAdapter(IMotionModule module)
        {
            _module = module;
        }

        /// <inheritdoc/>
        public bool IsTriggered(SafetyInputKind kind, string target)
        {
            if (_module?.DeviceEngine == null || string.IsNullOrEmpty(target)) return false;
            try
            {
                var vd = _module.DeviceEngine.GetVirtualByName(target);
                switch (kind)
                {
                    case SafetyInputKind.EStop:
                    case SafetyInputKind.DoorSafety:
                    case SafetyInputKind.DoorLock:
                        return vd is VIO vio && !vio.GetDigitalIn(); // 常闭：false=触发

                    case SafetyInputKind.AxisLimitPos:
                        return vd is VAxis axisPos && GetAxisBit(axisPos, AxisStatus.Pel);

                    case SafetyInputKind.AxisLimitNeg:
                        return vd is VAxis axisNeg && GetAxisBit(axisNeg, AxisStatus.Mel);

                    case SafetyInputKind.ServoAlarm:
                        return vd is VAxis axisAlm && GetAxisBit(axisAlm, AxisStatus.Alarm);

                    case SafetyInputKind.Brake:
                    case SafetyInputKind.UpstreamInterlock:
                    case SafetyInputKind.DownstreamInterlock:
                        // 待 TES-37 HandoverNode.GetSnapshot() / 硬件接入
                        return false;

                    default:
                        return false;
                }
            }
            catch
            {
                // 单点异常不致误触发，返回 false（条件不成立）
                return false;
            }
        }

        /// <summary>读 VAxis 指定状态位，失败/缺失返回 false</summary>
        private static bool GetAxisBit(VAxis axis, AxisStatus bit)
        {
            var status = axis?.GetAxisStatus(false);
            if (status == null) return false;
            return status.TryGetValue(bit, out var v) && v;
        }
    }
}
