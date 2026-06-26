using System.Collections.Generic;

namespace Luster.Module.Motion.AOI.Core.Models
{
    /// <summary>
    /// 五轴 RTCP（刀尖跟随）配置。
    /// 仅 AOI 站启用，Wipe 站可为 null。详见 ADR 中 IFiveAxisRTCP 旁路接口约束。
    /// </summary>
    public interface IFiveAxisRtcpProfile
    {
        /// <summary>正运动卡上的坐标系号。</summary>
        int CoordinateSystem { get; }

        /// <summary>虚拟轴名称列表（通常 X/Y/Z/U/V）。</summary>
        IReadOnlyList<string> VirtualAxes { get; }

        /// <summary>实际伺服轴名称列表。</summary>
        IReadOnlyList<string> RealAxes { get; }

        /// <summary>旋转中心 (X, Y, Z)。</summary>
        (double X, double Y, double Z) RotationCenter { get; }

        /// <summary>刀尖参考点 (X, Y, Z)。</summary>
        (double X, double Y, double Z) ToolCenterPoint { get; }
    }
}
