using Luster.Common.Assets.ViewModel;
using Luster.Common.Assets.Views;
using Luster.Motion.EditorUI;
using Luster.Motion.EditorUI.ViewModel;
using Luster.Motion.EditorUI.ViewModel.Dialogs;
using Luster.Motion.EditorUI.Views;
using Luster.Motion.EditorUI.Views.Dialogs;
using Luster.Motion.FiveAxis.Service;
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
            containerRegistry.RegisterDialog<InfoInputDialog, InfoInputDialogVM>();
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
            containerRegistry.RegisterDialog<EditorWindowDialog, EditorWindowDialogVM>();// 新窗口打开模块编辑

            // 注册配置导页面
            containerRegistry.RegisterForNavigation<FlowContent, FlowContentVM>();
            // FiveAxis 标定服务(激光/粗标/精标/原点),LaserCaliTabViewModel 依赖
            // IFiveAxisFrame 不入容器(运行时反射设备实例,同 IFiveAxisRTCP 范式);frame=null 仅阻塞精标 AccurateCalibrate,激光/粗标/原点三阶段可用;精标 frame 由精标执行 issue 运行时从 IDeviceEngine 取卡注入。
            containerRegistry.RegisterSingleton<IFiveAxisCalibrationService, FiveAxisCalibrationService>();
            // 激光标定 Tab 导航宿主(AutoWireViewModel 自动接 LaserCaliTabViewModel)
            containerRegistry.RegisterForNavigation<LaserCaliTabView, LaserCaliTabViewModel>();
        }
    }
}
