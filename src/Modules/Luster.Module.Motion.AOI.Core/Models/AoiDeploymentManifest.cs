using System.Collections.Generic;

namespace Luster.Module.Motion.AOI.Core.Models
{
    /// <summary>
    /// <see cref="IAoiDeploymentManifest"/> 的 POCO 实现。
    /// </summary>
    public sealed class AoiDeploymentManifest : IAoiDeploymentManifest
    {
        public string PackageVersion { get; set; } = string.Empty;
        public AoiSiteType SiteType { get; set; } = AoiSiteType.Unspecified;
        public List<string> Modules { get; } = new List<string>();
        public List<string> Devices { get; } = new List<string>();

        IReadOnlyList<string> IAoiDeploymentManifest.Modules => Modules;
        IReadOnlyList<string> IAoiDeploymentManifest.Devices => Devices;
    }
}
