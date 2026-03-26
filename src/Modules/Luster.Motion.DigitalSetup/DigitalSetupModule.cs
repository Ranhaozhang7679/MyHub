using Luster.Motion.DigitalSetup.Helpers;
using Luster.Motion.DigitalSetup.ViewModel;
using Luster.Motion.DigitalSetup.Views;
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
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion("DigitalAssEditorRegion", typeof(ParamConfirmContent)); //初始加载视图
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<CSVHelper>();

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


        }
    }
}
