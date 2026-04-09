using Luster.Motion.DigitalSetup.Helpers;
using Luster.Motion.DigitalSetup.Services;
using Luster.Motion.DigitalSetup.ViewModel;
using Luster.Motion.DigitalSetup.ViewModel.Dialogs;
using Luster.Motion.DigitalSetup.ViewModel.Validations.Configs;
using Luster.Motion.DigitalSetup.Views;
using Luster.Motion.DigitalSetup.Views.Dialogs;
using Luster.Motion.DigitalSetup.Views.Validations.Configs;
using Luster.Motion.Integration.Web;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.DigitalSetup
{
    public class DigitalSetupModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            // 初始化服务定位器
            DigitalSetupServiceLocator.Initialize(containerProvider);
            
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion("DigitalAssEditorRegion", typeof(ParamConfirmContent)); //初始加载视图
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<CSVHelper>();
            containerRegistry.RegisterSingleton<PageEnableSettingsService>();

            containerRegistry.RegisterForNavigation<AutoCommunicationConfigContent, AutoCommunicationConfigContentVM>();
            containerRegistry.RegisterForNavigation<AutomaticEmbossingContent, AutomaticEmbossingContentVM>();
            containerRegistry.RegisterForNavigation<AutomaticLoadCellContent, AutomaticLoadCellContentVM>();
            containerRegistry.RegisterForNavigation<DigitalAssContent, DigitalAssContentVM>();
            containerRegistry.RegisterForNavigation<DigitalVisionContent, DigitalVisionContentVM>();
            containerRegistry.RegisterForNavigation<IOinspectionContent, IOinspectionContentVM>();
            containerRegistry.RegisterForNavigation<ParamConfirmContent, ParamConfirmContentVM>();
            containerRegistry.RegisterForNavigation<PlatformLevelAutoConfirmContent, PlatformLevelAutoConfirmContentVM>();
            containerRegistry.RegisterForNavigation<PointTeachingContent, PointTeachingContentVM>();
            containerRegistry.RegisterForNavigation<AutoVisualCalibrationContent, AutoVisualCalibrationContentVM>();
            containerRegistry.RegisterForNavigation<DataValidationContent, DataValidationContentVM>();

            // 注册对话框
            containerRegistry.RegisterDialog<AddValidationItemDialog, AddValidationItemDialogVM>("AddValidationItemDialog");
            containerRegistry.RegisterDialog<PageEnableSettingsDialog, PageEnableSettingsDialogVM>("PageEnableSettingsDialog");
            containerRegistry.RegisterForNavigation<LADUploadContent, LADUploadContentVM>();
            containerRegistry.RegisterForNavigation<AutoVericationContent, AutoVericationContentVM>();

            // 注册验证类型配置控件及其ViewModel
            containerRegistry.RegisterForNavigation<LoadCellCalibrationConfigControl, LoadCellCalibrationConfigVM>("LoadCellCalibrationConfig");
            containerRegistry.RegisterForNavigation<CCDCalibrationConfigControl, CCDCalibrationConfigVM>("CCDCalibrationConfig");

            // BUSOP 页面注册
            containerRegistry.RegisterForNavigation<BusopContent, BusopContentVM>();
            containerRegistry.RegisterDialog<BusopSettingsDialog, BusopSettingsDialogVM>("BusopSettingsDialog");
            containerRegistry.RegisterDialog<TextInputDialog, TextInputDialogVM>("TextInputDialog");
            containerRegistry.RegisterSingleton<BusopConfigService>();

        }
    }
}
