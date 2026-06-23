using Luster.Motion.LightTuning.ViewModel;
using Luster.Motion.LightTuning.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace Luster.Motion.LightTuning
{
    /// <summary>
    /// 光调 UI 模块（TES-64 P6-F）：Prism 装配。
    /// 可还原 + 不侵入：仅 RegisterForNavigation，移除本模块（AddModule + ProjectReference + sln 块 + 目录）后平台标准 UI 不受影响。
    /// </summary>
    public class LightTuningModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            // 预留：如需在主区域常驻光调入口，在此 RegisterViewWithRegion。
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // 光调面板导航注册（Shell 用 RequestNavigate("MainRegion","LightTuningContent") 进入）
            containerRegistry.RegisterForNavigation<LightTuningContent, LightTuningContentVM>();
        }
    }
}
