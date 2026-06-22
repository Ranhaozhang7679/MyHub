using System.Collections.Generic;

namespace Luster.Module.Motion.AOI.Core.Models
{
    /// <summary>
    /// <see cref="IAoiSiteProfile"/> 的 POCO 实现。
    /// 仅承载数据，校验逻辑统一放在 <c>AoiSiteProfileValidator</c>，避免装载时半校验。
    /// </summary>
    public sealed class AoiSiteProfile : IAoiSiteProfile
    {
        public string ProfileId { get; set; } = string.Empty;
        public AoiSiteType SiteType { get; set; } = AoiSiteType.Unspecified;
        public string Version { get; set; } = string.Empty;

        public List<string> MotionModules { get; } = new List<string>();
        public List<string> DeviceModules { get; } = new List<string>();
        public Dictionary<string, string> Devices { get; } = new Dictionary<string, string>();
        public AoiAxisMap Axes { get; } = new AoiAxisMap();
        public FiveAxisRtcpProfile? Rtcp { get; set; }
        public Dictionary<string, string> Handshakes { get; } = new Dictionary<string, string>();

        public string RecipeRoot { get; set; } = string.Empty;
        public string TraceRoot { get; set; } = string.Empty;
        public string LogRoot { get; set; } = string.Empty;
        public string CardConfigPath { get; set; } = string.Empty;
        public string EntryStation { get; set; } = string.Empty;

        IReadOnlyList<string> IAoiSiteProfile.RequiredMotionModules => MotionModules;
        IReadOnlyList<string> IAoiSiteProfile.RequiredDeviceModules => DeviceModules;
        IReadOnlyDictionary<string, string> IAoiSiteProfile.DeviceNames => Devices;
        IAoiAxisMap IAoiSiteProfile.AxisMap => Axes;
        IFiveAxisRtcpProfile? IAoiSiteProfile.RtcpProfile => Rtcp;
        IReadOnlyDictionary<string, string> IAoiSiteProfile.HandshakeChannels => Handshakes;
    }
}
