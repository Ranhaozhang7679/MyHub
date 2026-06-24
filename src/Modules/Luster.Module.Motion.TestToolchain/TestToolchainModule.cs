using DC.Authorization;
using Luster.Module.Motion.TestToolchain.Manual;
using Luster.TaskFlow.Motion;

namespace Luster.Module.Motion.TestToolchain
{
    /// <summary>
    /// 测试工具链模块（TES-34 P9-A/B/C）。
    /// 承载调试模式参数表(DebugProfile)、手动操作回退栈(ManualStack)、
    /// 仿真(SimulationProfile/OpticalSimulator/VirtualModeMapping)。
    /// 零侵入 Shell：模块经 <c>IModuleFactory.LoadModules</c> 反射发现（CopyToMotionsFolder），
    /// 卸载 DLL 后平台标准运控不受影响（可还原）。
    /// </summary>
    /// <remarks>
    /// <b>容器注册</b>：<see cref="ManualStack"/> 需 <c>IAuthorizationFacade</c> 注入，
    /// 由 App 启动时调用 <see cref="Configure(object, IAuthorizationFacade)"/> 注册一行
    /// （类比 <c>ProductionModule.Configure</c>，属常规扩展，非 Shell 主干改动）。
    /// </remarks>
    public class TestToolchainModule : MotionModule
    {
        public override void InitFunctions()
        {
            // P9 当前为服务门面（非 MotionFunction 节点），无节点注册。
            // 后续手动操作 UI 节点 / 虚拟相机光学预测节点可在此 AddFunction<T>()。
        }

        /// <summary>
        /// 注册 ManualStack 服务 + 测试工具链权限项（供 App.RegisterTypes 调用一行）。
        /// </summary>
        /// <param name="container">Prism 容器（IContainerRegistry）</param>
        /// <param name="auth">权限门面（已由 WpfAuthorizationModule 注册为单例）</param>
        public static void Configure(object container, IAuthorizationFacade auth)
        {
            if (auth == null) return;

            // 注册测试工具链权限项（非侵入，不改 AuthKeys.cs）
            try { auth.RegisterRights(TestAuthItems.ToRights()); } catch { /* 重复注册忽略 */ }

            // 注册 ManualStack 单例
            var stack = new ManualStack(auth);
            RegisterService(container, stack);
        }

        /// <summary>容器注册（反射兼容 Prism IContainerRegistry，避免编译期耦合 Shell）</summary>
        private static void RegisterService(object container, IManualStack service)
        {
            // Prism IContainerRegistry.RegisterSingleton<T>() 经反射调用
            var method = container?.GetType().GetMethod("RegisterSingleton", new[] { typeof(IManualStack) });
            method?.Invoke(container, new object[] { service });
        }
    }

    /// <summary>模块创建器（被 IModuleFactory.LoadModules 反射发现）</summary>
    public class TestToolchainModuleCreator : MotionModuleCreator<TestToolchainModule>
    {
        public override int Sort => 8;
        public override string Icon => "\xe6b2";
    }
}
