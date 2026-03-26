using Luster.Common.Assets.ViewModel;
using Luster.Common.Assets.Views;
using Luster.Motion.EditorUI;
using Luster.Motion.EditorUI.ViewModel;
using Luster.Motion.EditorUI.ViewModel.Dialogs;
using Luster.Motion.EditorUI.Views;
using Luster.Motion.EditorUI.Views.Dialogs;
using Luster.Motion.TaskFlow.Engine;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;
using System;

namespace Luster.Motion.EditorUI
{
    public class EditorModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            // 视图注册
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion("EditorRegion", typeof(EditorContent));

            // 流程
            regionManager.RegisterViewWithRegion("TopRegion", typeof(ModuleContent));
            //regionManager.RegisterViewWithRegion("GlobalRegion", typeof(GlobalContent));

            regionManager.RegisterViewWithRegion("InParamRegion", typeof(InParamContent));
            regionManager.RegisterViewWithRegion("SearchRegion", typeof(SearchContent));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // 注册为单例模式
            containerRegistry.RegisterSingleton<FlowBus>();

            // 注册视图和模型示例
            containerRegistry.RegisterForNavigation<EditorContent, EditorContentVM>();
            containerRegistry.RegisterForNavigation<ModuleContent, ModuleContentVM>();
            containerRegistry.RegisterForNavigation<InParamContent, InParamContentVM>();
            containerRegistry.RegisterForNavigation<GlobalContent, GlobalContentVM>();
            containerRegistry.RegisterForNavigation<AxisSetContent, AxisSetContentVM>();
            containerRegistry.RegisterForNavigation<CTConfigContent, CTConfigContentVM>();
            containerRegistry.RegisterForNavigation<ErrorConfigContent, ErrorConfigContentVM>();
            containerRegistry.RegisterForNavigation<FlowViewContent, FlowViewContentVM>();
            containerRegistry.RegisterForNavigation<SearchContent, SearchContentVM>();

            // 设备对话框
            containerRegistry.RegisterDialog<TextDialog, TextDialogVM>();           // 文本提示
            containerRegistry.RegisterDialog<SwitchDialog, SwitchDialogVM>();       // 条件
            containerRegistry.RegisterDialog<StringExDialog, StringExDialogVM>();       // 条件
            containerRegistry.RegisterDialog<AxisMDialog, AxisMDialogVM>();         // 多轴调试
            containerRegistry.RegisterDialog<AxisConfigDialog, AxisConfigDialogVM>();         // 多轴调试
            containerRegistry.RegisterDialog<AddInParamDialog, AddInParamDialogVM>();
            containerRegistry.RegisterDialog<ArrayDialog, ArrayDialogVM>(); // 数组弹窗
            containerRegistry.RegisterDialog<ExpDialog, ExpDialogVM>();
            containerRegistry.RegisterDialog<StringMatchDialog, StringMatchDialogVM>();
            containerRegistry.RegisterDialog<StationDialog, StationDialogVM>();
            containerRegistry.RegisterDialog<SetGlobalVarDialog, SetGlobalVarDialogVM>();   //全局变量配置
            containerRegistry.RegisterDialog<SetWorkFlowDialog, SetWorkFlowDialogVM>(); //设置工作流
            containerRegistry.RegisterDialog<AxisPosDialog, AxisPosDialogVM>(); //轴点位对话框
            containerRegistry.RegisterDialog<SetModeGlobalVarDialog, SetModeGlobalVarDialogVM>();// 设置运动模式全局变量
            containerRegistry.RegisterDialog<SetRunModeDialog, SetRunModeDialogVM>();// 添加设置模式
            containerRegistry.RegisterDialog<ModuleDialog, ModuleDialogVM>();// 添加设置模式

            // 注册配置导页面
            containerRegistry.RegisterForNavigation<FlowContent, FlowContentVM>();
        }
    }
}
