using DC.Authorization;
using Luster.Motion.CommonUI;
using Luster.TaskFlow.Motion;
using System;

namespace Luster.Module.Motion.Production
{
    /// <summary>
    /// 生产交付模块（TES-33 P8）。
    /// 当前实现 P8-A 配方管理门面 + P8-C 配方权限项；P8-B/D/E 后续轮次扩展。
    /// 零侵入 Shell：模块经 <c>IModuleFactory.LoadModules</c> 反射发现（CopyToMotionsFolder），
    /// 卸载 DLL 后平台标准运控不受影响（可还原）。
    /// </summary>
    /// <remarks>
    /// <b>容器注册</b>：<see cref="RecipeManager"/> 需 <c>ICommonBus</c> + <c>IAuthorizationFacade</c> 注入，
    /// 由 App 启动时调用 <see cref="Configure(IContainerRegistry, ICommonBus, IAuthorizationFacade)"/> 注册一行
    /// （类比 App.RegisterTypes 既有 20+ 注册行，属常规扩展，非 Shell 主干改动）。
    /// </remarks>
    public class ProductionModule : MotionModule
    {
        public override void InitFunctions()
        {
            // P8-A/C 当前为服务门面（非 MotionFunction 节点），无节点注册。
            // P8-D 拍照握手状态机、P8-E OEE 节点后续轮次在此 AddFunction<T>()。
        }

        /// <summary>
        /// 注册 RecipeManager 服务 + 配方权限项（供 App.RegisterTypes 调用一行）。
        /// </summary>
        /// <param name="container">Prism 容器（IContainerRegistry）</param>
        /// <param name="commonBus">公共总线（已由 App 注册为单例）</param>
        /// <param name="auth">权限门面（已由 WpfAuthorizationModule 注册为单例）</param>
        public static void Configure(object container, ICommonBus commonBus, IAuthorizationFacade auth)
        {
            if (commonBus == null || auth == null) return;

            // 注册配方权限项（非侵入，不改 AuthKeys.cs）
            try { auth.RegisterRights(RecipeAuthItems.ToRights()); } catch { /* 重复注册忽略 */ }

            // 注册 RecipeManager 单例
            var recipeManager = new RecipeManager(commonBus, auth);
            RegisterService(container, recipeManager);
        }

        /// <summary>容器注册（反射兼容 Prism IContainerRegistry，避免编译期耦合 Shell）</summary>
        private static void RegisterService(object container, IRecipeManager service)
        {
            // Prism IContainerRegistry.RegisterSingleton<T>() 经反射调用
            var method = container?.GetType().GetMethod("RegisterSingleton", new[] { typeof(IRecipeManager) });
            method?.Invoke(container, new object[] { service });
        }
    }

    /// <summary>模块创建器（被 IModuleFactory.LoadModules 反射发现）</summary>
    public class ProductionModuleCreator : MotionModuleCreator<ProductionModule>
    {
        public override int Sort => 7;

        public override string Icon => "\xe6b1";
    }
}
