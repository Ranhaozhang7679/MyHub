using Luster.Motion.CommonUI.ViewModel.Dialogs;
using Luster.Motion.CommonUI.Views.Dialogs;
using Luster.Motion.SubSystem.Dialog;
using Luster.Motion.SubSystem.Dialogs;
using Luster.Motion.SubSystem.ViewModel;
using Luster.Motion.SubSystem.ViewModel.Dialogs;
using Luster.Motion.SubSystem.Views;
using Luster.Motion.SubSystem.Views.Dialogs;
using Luster.Motion.SubSystem.Views.Login;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Configuration;
using System.Linq;

namespace Luster.Motion.SubSystem
{
    public class MotionModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            string keyvalue = "LoginContent";
            if (ConfigurationManager.AppSettings.AllKeys.Contains("StartContent"))
            {
                var configValue = ConfigurationManager.AppSettings["StartContent"];
                if (!string.IsNullOrEmpty(configValue))
                {
                    keyvalue = configValue;
                }
            }
            // 视图注册
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion("ToolBarRegion", typeof(ToolBarContent));
            if (keyvalue == "LoginContentFX") regionManager.RegisterViewWithRegion("MainRegion", typeof(LoginContentFX));
            else regionManager.RegisterViewWithRegion("MainRegion", typeof(LoginContent));
            regionManager.RegisterViewWithRegion("StatusRegion", typeof(StatusContent));
            regionManager.RegisterViewWithRegion("ConfigurationRegion", typeof(SoftConfigureContent));
            //regionManager.RegisterViewWithRegion("StationRegion", typeof(StationDisplayContent));
            //regionManager.RegisterViewWithRegion("MainRightContent", typeof(MainRightContent));
            regionManager.RegisterViewWithRegion("ModuleDisplayRegion", typeof(ModuleDisplayContent));
            //regionManager.RegisterViewWithRegion("ChartReportDisplayRegion", typeof(ChartConfigureContent));
            regionManager.RegisterViewWithRegion("GlobalVarDisplayRegion", typeof(GlobalVarContent));
            regionManager.RegisterViewWithRegion("ProReportRegion", typeof(ProReportContent));
            regionManager.RegisterViewWithRegion("ProIndexRegion", typeof(ProIndexContent));
            regionManager.RegisterViewWithRegion("WorkFlowRegion", typeof(WorkFlowContent));
            //FFU VVM
            regionManager.RegisterViewWithRegion("FFURegion", typeof(FFUContent));
            regionManager.RegisterViewWithRegion("DustRegion", typeof(DustContent));
            

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            //containerRegistry.Register<Dispatcher>(() => Application.Current.Dispatcher);

            // 注册视图和模型示例
            ViewModelLocationProvider.Register<ToolBarContent, ToolBarContentVM>(); //手动注册特定视图的ViewModel
            //ViewModelLocationProvider.Register<MainContent, MainContentVM>();
            containerRegistry.RegisterForNavigation<MainContent, MainContentVM>();
            //ViewModelLocationProvider.Register<MainRightContent, MainRightContentVM>();
            //containerRegistry.RegisterForNavigation<StationDisplayContent, StationDisplayContentVM>();

            //注册ToolBar导航
            //containerRegistry.RegisterForNavigation<AlarmContent, AlarmContentVM>();
            containerRegistry.RegisterForNavigation<ProjectContent, ProjectContentVM>();
            containerRegistry.RegisterForNavigation<StatusContent, StatusContentVM>();
            containerRegistry.RegisterForNavigation<ConfigureContent, ConfigureContentVM>();

            //登录
            containerRegistry.RegisterForNavigation<LoginContent, LoginContentVM>();
            containerRegistry.RegisterForNavigation<LoginContentFX, LoginContentFXVM>();
            

            //注册配置导航
            containerRegistry.RegisterForNavigation<SoftConfigureContent, SoftConfigureContentVM>();
            containerRegistry.RegisterForNavigation<ProductInfoContent, ProductInfoContentVM>();
            containerRegistry.RegisterForNavigation<VisionConfig, VisionConfigVM>();
            containerRegistry.RegisterForNavigation<UserContent, UserContentVM>();
            containerRegistry.RegisterForNavigation<PlcAlarmContent, PlcAlarmContentVM>();
            containerRegistry.RegisterForNavigation<RobotConfigureContent, RobotConfigureContentVM>();
            containerRegistry.RegisterForNavigation<FileConfigContent, FileConfigContentVM>();
            containerRegistry.RegisterForNavigation<FunctionEnableContent, FunctionEnableContentVM>();
            //注册品质管理导航
            containerRegistry.RegisterForNavigation<QualitySetContent, QualitySetContentVM>();
            //containerRegistry.RegisterForNavigation<UserDefineMainContent, UserDefineMainContentVM>();

            //模块可视化配置
            //containerRegistry.RegisterForNavigation<ChartConfigureContent, ChartConfigureContentVM>();//Chart可配置
            containerRegistry.RegisterForNavigation<ModuleDisplayContent, ModuleDisplayContentVM>();//模块可视化配置
            containerRegistry.RegisterForNavigation<GlobalVarContent, GlobalVarContentVM>();//全局变量

            //扭力曲线
            containerRegistry.RegisterForNavigation<ChartTorqueContent, ChartTorqueContentVM>();//模块可视化配置
            containerRegistry.RegisterForNavigation<ChartTorque2Content, ChartTorque2ContentVM>();//模块可视化配置

            //压力曲线
            containerRegistry.RegisterForNavigation<Press1Content, Press1ContentVM>();//模块可视化配置
            containerRegistry.RegisterForNavigation<Press2Content, Press2ContentVM>();//模块可视化配置
            containerRegistry.RegisterForNavigation<Press3Content, Press3ContentVM>();//模块可视化配置
            containerRegistry.RegisterForNavigation<Press4Content, Press4ContentVM>();//模块可视化配置

            containerRegistry.RegisterDialog<WinFormDialog, WinFormDialogVM>();       // 集成Windows Form
            containerRegistry.RegisterDialog<HiveDialog, HiveDialogVM>();       // 集成Windows Form
            containerRegistry.RegisterDialog<HiveDialog1, HiveDialogVM>();       // 集成Windows Form


            // 首页动态报表
            containerRegistry.RegisterForNavigation<ProReportContent, ProReportContentVM>();
            containerRegistry.RegisterForNavigation<ProIndexContent, ProIndexContentVM>();

            containerRegistry.RegisterForNavigation<ProOrderContent, ProOrderContentVM>();


            containerRegistry.RegisterDialog<ModuleConfigureDialog, ModuleConfigureDialogVM>();   //模块配置
            containerRegistry.RegisterForNavigation<HeartMonitorContent, HeartMonitorContentVM>();
            containerRegistry.RegisterForNavigation<WorkFlowContent, WorkFlowContentVM>();

            containerRegistry.RegisterForNavigation<ChartCopyContent, ChartCopyContentVM>();
            containerRegistry.RegisterForNavigation<ProFXContent, ProFXContentVM>();
            containerRegistry.RegisterForNavigation<ProPLCContent, ProPLCContentVM>();
            containerRegistry.RegisterForNavigation<ConfirmBtnContent, ConfirmBtnContentVM>();
            containerRegistry.RegisterForNavigation<FlowWaitContent, FlowWaitContentVM>();
            containerRegistry.RegisterForNavigation<RollSetContent, RollSetContentVM>();
            containerRegistry.RegisterForNavigation<ProTestBottonContent, ProTestBottonContentVM>();

            // 设备调试
            containerRegistry.RegisterForNavigation<DeviceDebugContent, DeviceDebugContentVM>();
            //FFU
            containerRegistry.RegisterForNavigation<FFUContent, FFUContentVM>();
            //FFUCom
            containerRegistry.RegisterForNavigation<FFUComContent, FFUComContentVM>();

            containerRegistry.RegisterForNavigation<DustContent, DustContentVM>();

            // 硬件调试配置集成内容
            containerRegistry.RegisterForNavigation<IntegratedHardwareContent, IntegratedHardwareContentVM>();

            ////  数字架线
            //containerRegistry.RegisterForNavigation<DigitalAssContent, DigitalAssContentVM>();      //  数字架线一级菜单界面
            //containerRegistry.RegisterForNavigation<IOinspectionContent, IOinspectionContentVM>();  // IO点检
            //containerRegistry.RegisterForNavigation<AutoCommunicationConfigContent, AutoCommunicationConfigContentVM>();// 通讯端口自动配置            
            //containerRegistry.RegisterForNavigation<ParamConfirmContent, ParamConfirmContentVM>();  // 参数导入确认
            //containerRegistry.RegisterForNavigation<PlatformLevelAutoConfirmContent, PlatformLevelAutoConfirmContentVM>();// 平台水平自动确认
            //containerRegistry.RegisterForNavigation<AutomaticLoadCellContent, AutomaticLoadCellContentVM>();// 自动LoadCell
            //containerRegistry.RegisterForNavigation<AutomaticEmbossingContent, AutomaticEmbossingContentVM>();// 自动压印
            //containerRegistry.RegisterForNavigation<DigitalVisionContent, DigitalVisionContentVM>();// 视觉标定            
        }
    }
}
