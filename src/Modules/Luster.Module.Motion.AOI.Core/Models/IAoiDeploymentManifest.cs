using System.Collections.Generic;

namespace Luster.Module.Motion.AOI.Core.Models
{
    /// <summary>
    /// 部署清单：声明站点对应的 DLL 包版本、模块列表、设备列表。
    /// 启动时与 site-profile 做版本一致性校验，不匹配则拦截。
    /// </summary>
    public interface IAoiDeploymentManifest
    {
        /// <summary>部署包版本，须与 profile.Version 一致。</summary>
        string PackageVersion { get; }

        /// <summary>站点类型，须与 profile.SiteType 一致。</summary>
        AoiSiteType SiteType { get; }

        /// <summary>实际部署的运控模块（DLL 文件名，无扩展名）。</summary>
        IReadOnlyList<string> Modules { get; }

        /// <summary>实际部署的设备模块。</summary>
        IReadOnlyList<string> Devices { get; }
    }
}
