using System;
using System.Collections.Generic;

namespace Luster.Module.Motion.AOI.Core.Models
{
    /// <summary>
    /// 站点 profile 校验结果。
    /// </summary>
    public sealed class AoiProfileValidationResult
    {
        private readonly List<string> _errors = new List<string>();

        /// <summary>校验是否通过。</summary>
        public bool IsValid => _errors.Count == 0;

        /// <summary>所有错误，已按发现顺序记录。</summary>
        public IReadOnlyList<string> Errors => _errors;

        /// <summary>站点类型。</summary>
        public AoiSiteType SiteType { get; set; } = AoiSiteType.Unspecified;

        /// <summary>profile id（用于报错文本拼装）。</summary>
        public string ProfileId { get; set; } = string.Empty;

        public void AddError(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            _errors.Add(message);
        }

        /// <summary>聚合报警文本，按行换行。供启动拦截弹窗/日志使用。</summary>
        public string FormatAlarmText()
        {
            if (IsValid)
            {
                return $"站点 [{ProfileId}/{SiteType}] 校验通过。";
            }

            return string.Concat(
                $"站点 [{ProfileId}/{SiteType}] 校验失败，共 {_errors.Count} 项：",
                Environment.NewLine,
                string.Join(Environment.NewLine, _errors));
        }
    }
}
