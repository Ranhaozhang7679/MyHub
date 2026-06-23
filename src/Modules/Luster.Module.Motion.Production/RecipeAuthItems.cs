using DC.Authorization;
using DC.Authorization.Models;
using System.Linq;

namespace Luster.Module.Motion.Production
{
    /// <summary>
    /// 配方相关权限项（TES-33 P8-C，非侵入新增）。
    /// lmv <c>AuthDictionary</c>（<c>AuthKeys.cs</c>）无配方权限项，本类定义配方权限 AuthItem，
    /// 由 <c>ProductionModule</c> 启动时经 <c>IAuthorizationFacade.RegisterRights</c> 注册，
    /// 不改既有 <c>AuthKeys.cs</c>（R1 非侵入）。
    /// </summary>
    /// <remarks>
    /// 配方权限对齐源端三角色（工程师/操作员/管理员）：
    /// - SaveRecipe/SwitchRecipe/DeleteRecipe/ImportExportRecipe：管理员+工程师可操作，操作员只读
    /// - VizRecipePanel：三角色均可见配方面板（操作员不可编辑）
    /// 角色判定由 lmv 既有动态 Role（DB role_info）+ <c>IAuthorizationFacade.CheckAuth</c> 完成。
    /// </remarks>
    public static class RecipeAuthItems
    {
        /// <summary>保存配方</summary>
        public static readonly AuthItem SaveRecipe = new AuthItem("配方管理", "配方操作", "保存配方", "保存当前配方到磁盘", 1);

        /// <summary>切换配方（多品种切换）</summary>
        public static readonly AuthItem SwitchRecipe = new AuthItem("配方管理", "配方操作", "切换配方", "切换激活配方（多品种切换）", 2);

        /// <summary>新增/复制配方</summary>
        public static readonly AuthItem CopyRecipe = new AuthItem("配方管理", "配方操作", "新增复制配方", "新增或复制配方", 3);

        /// <summary>删除配方</summary>
        public static readonly AuthItem DeleteRecipe = new AuthItem("配方管理", "配方操作", "删除配方", "删除配方", 4);

        /// <summary>导入/导出配方</summary>
        public static readonly AuthItem ImportExportRecipe = new AuthItem("配方管理", "配方操作", "导入导出配方", "导入或导出配方", 5);

        /// <summary>配方面板可见（显示权限）</summary>
        public static readonly AuthItem VizRecipePanel = new AuthItem("配方管理", "配方面板", "配方面板", "配方管理面板可见", 6);

        /// <summary>配方权限项全集（供 RegisterRights 注册）</summary>
        public static readonly AuthItem[] All =
        {
            SaveRecipe, SwitchRecipe, CopyRecipe, DeleteRecipe, ImportExportRecipe, VizRecipePanel
        };

        /// <summary>
        /// 转换为 <see cref="Right"/> 数组供 <c>IAuthorizationFacade.RegisterRights</c> 注册
        /// （对齐 <c>AuthViewModelBase.AutoRegisterRights</c> 的 AuthItem→Right 转换）。
        /// VizRecipePanel 为显示权限，其余为操作权限。
        /// </summary>
        public static Right[] ToRights()
        {
            return All.Select(a => new Right
            {
                Name = a.Operation,
                ModuleName = a.Module,
                ViewName = a.View,
                Description = a.Description,
                Type = a.Operation == VizRecipePanel.Operation ? RightType.Visibility : RightType.Operation,
                SortOrder = a.Order
            }).ToArray();
        }
    }
}
