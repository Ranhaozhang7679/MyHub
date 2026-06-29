using Luster.Motion.FiveAxis.UI.ViewModel;
using Luster.Motion.FiveAxis.UI.Views;
using Prism.Ioc;
using Prism.Modularity;

namespace Luster.Motion.FiveAxis.UI
{
    /// <summary>
    /// 五轴 AOI UI 模块（P6-A 基建）。
    /// 职责：把五轴 UI 挂进平台 Prism Shell，提供 MainRegion 可导航的标定参数编辑面板。
    /// 边界：仅做 UI 基建 + ParamGrid 特性对齐，不实现标定/运动业务算法（全栈工程师负责）。
    /// 可还原：移除本 csproj + Shell 的 ProjectReference + App.xaml.cs 的 AddModule 行，
    ///         平台标准 UI 不受影响；不侵入 Luster.Prism / Luster.TaskFlow.Common 核心。
    /// </summary>
    public class FiveAxisUIModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // 五轴标定参数面板注册到 MainRegion，可通过 RequestNavigate("MainRegion","FiveAxisContent") 导航
            containerRegistry.RegisterForNavigation<FiveAxisContent, FiveAxisContentVM>();
        }
    }
}
