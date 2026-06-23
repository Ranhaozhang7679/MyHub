using DC.Authorization;
using System.Collections.Generic;

namespace Luster.Module.Motion.Production.Trace
{
    /// <summary>
    /// 参数变更审计 helper（TES-33 P8-B）。
    /// 关键 Profile（AutoCaliProfile/Check5AxisBaseProfile 等）保存时触发
    /// <see cref="IAuthorizationFacade.Audit{T}"/> 统一审计（before/after diff）。
    /// </summary>
    /// <remarks>
    /// batch1 RecipeManager 已接入 Audit，本轮扩展到标定/五轴 Profile。
    /// <see cref="IAuthorizationFacade.Audit{T}"/> 内部用 <c>Utility.CompareProperties</c> 做 diff，
    /// 生成"将X由A改成B"字符串写 <c>audit_log</c>。本 helper 统一入口 + diff 预检（无变更不审计）。
    /// </remarks>
    public class ProfileAuditor
    {
        private readonly IAuthorizationFacade _auth;

        public ProfileAuditor(IAuthorizationFacade auth)
        {
            _auth = auth;
        }

        /// <summary>
        /// 审计 Profile 变更（before → after）。
        /// 自动 diff，有变更才审计；无变更或未登录跳过。
        /// </summary>
        /// <typeparam name="T">Profile 类型</typeparam>
        /// <param name="operation">操作名（如"保存标定参数"/"保存五轴Profile"）</param>
        /// <param name="profileName">Profile 名称（如"AutoCaliProfile"）</param>
        /// <param name="before">变更前快照</param>
        /// <param name="after">变更后快照</param>
        /// <returns>true=已审计；false=无变更/未审计</returns>
        public bool AuditProfileChange<T>(string operation, string profileName, T before, T after)
        {
            if (_auth == null) return false;
            if (!HasDifference(before, after)) return false;

            string detail = string.IsNullOrEmpty(profileName) ? operation : $"{operation}: {profileName}";
            _auth.Audit(operation, detail, before, after);
            return true;
        }

        /// <summary>
        /// 差异判定（纯逻辑，便于单测）。
        /// 简化策略：引用相等或均为 null 视为无差异；否则视为有差异
        /// （精确 diff 由 <c>IAuthorizationFacade.Audit</c> 内部 <c>Utility.CompareProperties</c> 完成）。
        /// </summary>
        public static bool HasDifference<T>(T before, T after)
        {
            if (ReferenceEquals(before, after)) return false;
            if (before == null && after == null) return false;
            // 一方为 null 视为有差异
            if (before == null || after == null) return true;
            // 值类型/字符串直接比较
            if (before is System.IEquatable<T> eq)
            {
                return !eq.Equals(after);
            }
            // 引用类型默认视为有差异（精确 diff 由 Audit 内部完成）
            return true;
        }

        /// <summary>关键 Profile 名称常量（对齐源端 AutoCaliProfile/Check5AxisBaseProfile）</summary>
        public static class ProfileNames
        {
            public const string AutoCaliProfile = "AutoCaliProfile";
            public const string Check5AxisBaseProfile = "Check5AxisBaseProfile";
            public const string SysProfile = "SysProfile";
            public const string Recipe = "Recipe";
        }
    }
}
