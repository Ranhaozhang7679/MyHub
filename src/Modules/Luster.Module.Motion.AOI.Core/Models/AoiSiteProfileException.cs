using System;

namespace Luster.Module.Motion.AOI.Core.Models
{
    /// <summary>
    /// 站点 profile 校验失败异常。
    /// 启动链应捕获该异常并阻止自动流程进入，给出明确报警/提示。
    /// </summary>
    public sealed class AoiSiteProfileException : Exception
    {
        public AoiProfileValidationResult ValidationResult { get; }

        public AoiSiteProfileException(AoiProfileValidationResult result)
            : base(result?.FormatAlarmText() ?? "站点 profile 校验失败。")
        {
            ValidationResult = result ?? new AoiProfileValidationResult();
        }
    }
}
