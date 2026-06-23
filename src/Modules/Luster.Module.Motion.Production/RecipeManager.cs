using DC.Authorization;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Models;
using System.Collections.Generic;
using System.IO;

namespace Luster.Module.Motion.Production
{
    /// <summary>
    /// <see cref="IRecipeManager"/> 默认实现（TES-33 P8-A）。
    /// 委托 lmv 既有 <see cref="ICommonBus"/> + <see cref="ProjectInfo"/>，补齐 import/export/copy/delete，
    /// 关键操作（保存/切换/复制/删除/导入导出）经 <see cref="IAuthorizationFacade"/> 权限校验 + 审计。
    /// </summary>
    public class RecipeManager : IRecipeManager
    {
        private readonly ICommonBus _commonBus;
        private readonly IAuthorizationFacade _auth;

        public RecipeManager(ICommonBus commonBus, IAuthorizationFacade auth)
        {
            _commonBus = commonBus;
            _auth = auth;
        }

        /// <inheritdoc/>
        public Recipe CurrentRecipe => _commonBus?.CurrentRecipe;

        /// <inheritdoc/>
        public ProjectInfo CurrentProject => _commonBus?.ProjInfo;

        /// <inheritdoc/>
        public List<Recipe> LoadRecipes()
        {
            var proj = CurrentProject;
            if (proj == null) return new List<Recipe>();
            return proj.RecipeList ?? new List<Recipe>();
        }

        /// <inheritdoc/>
        public void Save(string recipeName = "")
        {
            if (!CheckAndAudit(RecipeAuthItems.SaveRecipe, "保存配方", CurrentRecipe?.Name, recipeName)) return;
            _commonBus.OnSaveRecipe(recipeName);
        }

        /// <inheritdoc/>
        public bool Switch(string recipeName)
        {
            var proj = CurrentProject;
            var target = proj?.RecipeList?.Find(r => r.Name == recipeName);
            if (target == null) return false;

            string before = CurrentRecipe?.Name;
            if (!CheckAndAudit(RecipeAuthItems.SwitchRecipe, "切换配方", before, recipeName)) return false;

            // 委托既有切换路径（发布 RecipeOpenEvent，无改代码，≤30s 硬验收靠既有机制）
            _commonBus.OnActiveRecipe(target);
            return true;
        }

        /// <inheritdoc/>
        public bool Copy(string sourceRecipeName, string newRecipeName)
        {
            var proj = CurrentProject;
            if (proj == null) return false;
            var source = proj.RecipeList?.Find(r => r.Name == sourceRecipeName);
            if (source == null) return false;
            if (proj.RecipeList != null && proj.RecipeList.Exists(r => r.Name == newRecipeName)) return false;

            if (!CheckAndAudit(RecipeAuthItems.CopyRecipe, "复制配方", sourceRecipeName, newRecipeName)) return false;

            // 新建空配方 + 复制源配方文件内容
            Recipe newRecipe = Recipe.NewRecipe(newRecipeName, proj);
            try
            {
                CopyDirectory(source.GetRecipePath(), newRecipe.GetRecipePath());
            }
            catch
            {
                // 复制失败不阻断新建（NewRecipe 已建空配方）
            }
            proj.AddNewRecipe(newRecipeName);
            return true;
        }

        /// <inheritdoc/>
        public bool Delete(string recipeName)
        {
            var proj = CurrentProject;
            if (proj == null) return false;
            var target = proj.RecipeList?.Find(r => r.Name == recipeName);
            if (target == null) return false;
            if (target.IsActive) return false; // 激活配方不可删

            if (!CheckAndAudit(RecipeAuthItems.DeleteRecipe, "删除配方", recipeName, null)) return false;

            proj.RemoveRecipe(recipeName);
            try
            {
                if (Directory.Exists(target.GetRecipePath()))
                {
                    Directory.Delete(target.GetRecipePath(), true);
                }
            }
            catch
            {
                // 磁盘删除失败不阻断列表移除
            }
            return true;
        }

        /// <inheritdoc/>
        public bool Export(string recipeName, string targetDir)
        {
            var proj = CurrentProject;
            var target = proj?.RecipeList?.Find(r => r.Name == recipeName);
            if (target == null || string.IsNullOrEmpty(targetDir)) return false;

            if (!CheckAndAudit(RecipeAuthItems.ImportExportRecipe, "导出配方", recipeName, targetDir)) return false;

            try
            {
                Directory.CreateDirectory(targetDir);
                CopyDirectory(target.GetRecipePath(), Path.Combine(targetDir, recipeName));
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public bool Import(string sourceRecipeDir, string newRecipeName)
        {
            var proj = CurrentProject;
            if (proj == null || !Directory.Exists(sourceRecipeDir)) return false;
            if (proj.RecipeList != null && proj.RecipeList.Exists(r => r.Name == newRecipeName)) return false;

            if (!CheckAndAudit(RecipeAuthItems.ImportExportRecipe, "导入配方", sourceRecipeDir, newRecipeName)) return false;

            Recipe newRecipe = Recipe.NewRecipe(newRecipeName, proj);
            try
            {
                CopyDirectory(sourceRecipeDir, newRecipe.GetRecipePath());
            }
            catch
            {
                // 导入失败不阻断空配方
            }
            proj.AddNewRecipe(newRecipeName);
            return true;
        }

        /// <summary>权限校验 + 审计（关键 Profile 变更触发 Audit）</summary>
        /// <returns>true=有权限可继续；false=无权限已拦截</returns>
        private bool CheckAndAudit(AuthItem right, string operation, string before, string after)
        {
            if (_auth == null) return true; // 未注入权限（如纯逻辑测试）放行
            if (!_auth.HasAuth(right))
            {
                _auth.PopNoAuthNotification(right);
                return false;
            }
            // 审计：before/after 用字符串快照（Recipe 完整对象序列化较重，配方名+路径足以追溯）
            _auth.Audit(operation, $"{right.Operation}: {before ?? "(无)"} → {after ?? "(无)"}", before, after);
            return true;
        }

        /// <summary>
        /// 权限判定（纯逻辑，便于单测）。
        /// 对齐 <c>AuthorizationFacade.HasAuth</c>：未注入权限门面时放行（纯逻辑测试/未启用权限），
        /// 注入后按 <c>HasAuth</c> 判定，无权限时记录到 <paramref name="deniedReason"/>。
        /// </summary>
        /// <param name="auth">权限门面（可空）</param>
        /// <param name="right">待校验权限项</param>
        /// <param name="deniedReason">输出：无权限原因（有权限时为 null）</param>
        /// <returns>true=有权限</returns>
        public static bool EvaluatePermission(IAuthorizationFacade auth, AuthItem right, out string deniedReason)
        {
            deniedReason = null;
            if (auth == null) return true;
            if (!auth.HasAuth(right))
            {
                deniedReason = $"无[{right.Operation}]权限";
                return false;
            }
            return true;
        }

        /// <summary>递归复制目录（对齐源端 CopyFolder 语义）</summary>
        public static void CopyDirectory(string sourceDir, string targetDir)
        {
            if (!Directory.Exists(sourceDir)) return;
            Directory.CreateDirectory(targetDir);
            foreach (var file in Directory.GetFiles(sourceDir))
            {
                File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file)), true);
            }
            foreach (var sub in Directory.GetDirectories(sourceDir))
            {
                CopyDirectory(sub, Path.Combine(targetDir, Path.GetFileName(sub)));
            }
        }
    }
}
