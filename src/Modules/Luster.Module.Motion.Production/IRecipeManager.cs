using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Models;

namespace Luster.Module.Motion.Production
{
    /// <summary>
    /// 配方管理门面（TES-33 P8-A，非侵入新增）。
    /// 包装 lmv 既有 <see cref="ICommonBus"/> + <see cref="ProjectInfo"/>/`Recipe`，
    /// 补齐源端要求的"加载/保存/导入/导出/切换/复制/删除"统一门面。
    /// </summary>
    /// <remarks>
    /// lmv 既有配方能力散在 <c>ICommonBus.OnActiveRecipe/OnSaveRecipe/OnBackUpRecipe</c> +
    /// <c>ProjectInfo.AddNewRecipe/RemoveRecipe</c>，无统一门面，import/export/copy/delete 散落
    /// （<c>ProjectContentVM.cs:513</c> ad-hoc CopyFolder）。本门面聚合为单一入口，不改动
    /// <c>ICommonBus</c>/<c>CommonBus.cs</c>（R1 非侵入），仅委托调用。
    /// 配方切换 ≤30 秒（用户硬验收）：切换走 <see cref="ICommonBus.OnActiveRecipe"/> 既有路径，
    /// 该路径已发布 <c>RecipeOpenEvent</c> 供 ~25 VM 重载，无改代码需求。
    /// </remarks>
    public interface IRecipeManager
    {
        /// <summary>当前激活配方（委托 ICommonBus.CurrentRecipe）</summary>
        Recipe CurrentRecipe { get; }

        /// <summary>当前工程（委托 ICommonBus.ProjInfo）</summary>
        ProjectInfo CurrentProject { get; }

        /// <summary>加载工程下全部配方（委托 ProjectInfo.BuildRecipeList）</summary>
        System.Collections.Generic.List<Recipe> LoadRecipes();

        /// <summary>保存当前配方（委托 ICommonBus.OnSaveRecipe）</summary>
        void Save(string recipeName = "");

        /// <summary>切换激活配方（多品种切换，委托 ICommonBus.OnActiveRecipe）</summary>
        bool Switch(string recipeName);

        /// <summary>复制配方（基于源配方创建新配方）</summary>
        /// <param name="sourceRecipeName">源配方名</param>
        /// <param name="newRecipeName">新配方名</param>
        bool Copy(string sourceRecipeName, string newRecipeName);

        /// <summary>删除配方</summary>
        bool Delete(string recipeName);

        /// <summary>导出配方到指定目录</summary>
        /// <param name="recipeName">配方名</param>
        /// <param name="targetDir">目标目录</param>
        bool Export(string recipeName, string targetDir);

        /// <summary>从源目录导入配方</summary>
        /// <param name="sourceRecipeDir">源配方目录（含 .recipe 文件）</param>
        /// <param name="newRecipeName">新配方名</param>
        bool Import(string sourceRecipeDir, string newRecipeName);
    }
}
