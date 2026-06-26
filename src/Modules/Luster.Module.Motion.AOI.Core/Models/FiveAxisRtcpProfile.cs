using System.Collections.Generic;

namespace Luster.Module.Motion.AOI.Core.Models
{
    /// <summary>
    /// <see cref="IFiveAxisRtcpProfile"/> 的 POCO 实现。
    /// </summary>
    public sealed class FiveAxisRtcpProfile : IFiveAxisRtcpProfile
    {
        public int CoordinateSystem { get; set; }
        public List<string> Virtual { get; } = new List<string>();
        public List<string> Real { get; } = new List<string>();
        public (double X, double Y, double Z) RotationCenter { get; set; }
        public (double X, double Y, double Z) ToolCenterPoint { get; set; }

        IReadOnlyList<string> IFiveAxisRtcpProfile.VirtualAxes => Virtual;
        IReadOnlyList<string> IFiveAxisRtcpProfile.RealAxes => Real;
    }
}
