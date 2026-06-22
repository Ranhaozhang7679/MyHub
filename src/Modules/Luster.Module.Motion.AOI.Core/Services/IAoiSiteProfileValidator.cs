using Luster.Module.Motion.AOI.Core.Models;

namespace Luster.Module.Motion.AOI.Core.Services
{
    /// <summary>
    /// 站点 profile 校验器。
    /// 必须在自动流程启动前执行，校验失败应触发 <see cref="AoiSiteProfileException"/> 阻止自动流程。
    /// </summary>
    public interface IAoiSiteProfileValidator
    {
        /// <summary>
        /// 校验 profile 完整性。
        /// </summary>
        AoiProfileValidationResult Validate(IAoiSiteProfile profile, IAoiDeploymentManifest? manifest);
    }
}