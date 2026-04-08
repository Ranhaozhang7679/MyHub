using Luster.Common.Assets.FloatingInfo.Services;
using Prism.Ioc;

namespace Luster.Motion.DigitalSetup.Services
{
    /// <summary>
    /// 数字配置模块的服务定位器
    /// 用于在无法通过构造函数注入的场景下获取服务实例
    /// </summary>
    public static class DigitalSetupServiceLocator
    {
        private static IContainerProvider _container;

        /// <summary>
        /// 初始化服务定位器
        /// </summary>
        /// <param name="container">容器提供者</param>
        public static void Initialize(IContainerProvider container)
        {
            _container = container;
        }

        /// <summary>
        /// 获取页面启用设置服务
        /// </summary>
        public static PageEnableSettingsService PageEnableSettingsService => _container?.Resolve<PageEnableSettingsService>();

        /// <summary>
        /// 获取浮动信息服务
        /// </summary>
        public static IFloatingInfoService FloatingInfoService => _container?.Resolve<IFloatingInfoService>();

        /// <summary>
        /// IoC 容器，供需要手动解析服务的场景使用
        /// </summary>
        public static IContainerProvider Container => _container;

        /// <summary>
        /// 获取 BUSOP 配置服务
        /// </summary>
        public static BusopConfigService BusopConfigService => _container?.Resolve<BusopConfigService>();
    }
}
