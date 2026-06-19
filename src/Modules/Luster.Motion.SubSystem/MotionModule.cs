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
            // ��ͼע��
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

            // ע����ͼ��ģ��ʾ��
            ViewModelLocationProvider.Register<ToolBarContent, ToolBarContentVM>(); //�ֶ�ע���ض���ͼ��ViewModel
            //ViewModelLocationProvider.Register<MainContent, MainContentVM>();
            containerRegistry.RegisterForNavigation<MainContent, MainContentVM>();
            //ViewModelLocationProvider.Register<MainRightContent, MainRightContentVM>();
            //containerRegistry.RegisterForNavigation<StationDisplayContent, StationDisplayContentVM>();

            //ע��ToolBar����
            //containerRegistry.RegisterForNavigation<AlarmContent, AlarmContentVM>();
            containerRegistry.RegisterForNavigation<ProjectContent, ProjectContentVM>();
            containerRegistry.RegisterForNavigation<StatusContent, StatusContentVM>();
            containerRegistry.RegisterForNavigation<ConfigureContent, ConfigureContentVM>();

            //��¼
            containerRegistry.RegisterForNavigation<LoginContent, LoginContentVM>();
            containerRegistry.RegisterForNavigation<LoginContentFX, LoginContentFXVM>();
            

            //ע�����õ���
            containerRegistry.RegisterForNavigation<SoftConfigureContent, SoftConfigureContentVM>();
            containerRegistry.RegisterForNavigation<ProductInfoContent, ProductInfoContentVM>();
            containerRegistry.RegisterForNavigation<VisionConfig, VisionConfigVM>();
            containerRegistry.RegisterForNavigation<UserContent, UserContentVM>();
            containerRegistry.RegisterForNavigation<PlcAlarmContent, PlcAlarmContentVM>();
            containerRegistry.RegisterForNavigation<RobotConfigureContent, RobotConfigureContentVM>();
            containerRegistry.RegisterForNavigation<FileConfigContent, FileConfigContentVM>();
            containerRegistry.RegisterForNavigation<FunctionEnableContent, FunctionEnableContentVM>();
            //ע��Ʒ�ʹ�������
            containerRegistry.RegisterForNavigation<QualitySetContent, QualitySetContentVM>();
            //containerRegistry.RegisterForNavigation<UserDefineMainContent, UserDefineMainContentVM>();

            //ģ����ӻ�����
            //containerRegistry.RegisterForNavigation<ChartConfigureContent, ChartConfigureContentVM>();//Chart������
            containerRegistry.RegisterForNavigation<ModuleDisplayContent, ModuleDisplayContentVM>();//ģ����ӻ�����
            containerRegistry.RegisterForNavigation<GlobalVarContent, GlobalVarContentVM>();//ȫ�ֱ���

            //Ť������
            containerRegistry.RegisterForNavigation<ChartTorqueContent, ChartTorqueContentVM>();//ģ����ӻ�����
            containerRegistry.RegisterForNavigation<ChartTorque2Content, ChartTorque2ContentVM>();//ģ����ӻ�����

            //ѹ������
            containerRegistry.RegisterForNavigation<Press1Content, Press1ContentVM>();//ģ����ӻ�����
            containerRegistry.RegisterForNavigation<Press2Content, Press2ContentVM>();//ģ����ӻ�����
            containerRegistry.RegisterForNavigation<Press3Content, Press3ContentVM>();//ģ����ӻ�����
            containerRegistry.RegisterForNavigation<Press4Content, Press4ContentVM>();//ģ����ӻ�����

            containerRegistry.RegisterDialog<WinFormDialog, WinFormDialogVM>();       // ����Windows Form
            containerRegistry.RegisterDialog<HiveDialog, HiveDialogVM>();       // ����Windows Form
            containerRegistry.RegisterDialog<HiveDialog1, HiveDialogVM>();       // ����Windows Form


            // ��ҳ��̬����
            containerRegistry.RegisterForNavigation<ProReportContent, ProReportContentVM>();
            containerRegistry.RegisterForNavigation<ProIndexContent, ProIndexContentVM>();

            containerRegistry.RegisterForNavigation<ProOrderContent, ProOrderContentVM>();


            containerRegistry.RegisterDialog<ModuleConfigureDialog, ModuleConfigureDialogVM>();   //ģ������
            containerRegistry.RegisterForNavigation<HeartMonitorContent, HeartMonitorContentVM>();
            containerRegistry.RegisterForNavigation<WorkFlowContent, WorkFlowContentVM>();

            containerRegistry.RegisterForNavigation<ChartCopyContent, ChartCopyContentVM>();
            containerRegistry.RegisterForNavigation<ProFXContent, ProFXContentVM>();
            containerRegistry.RegisterForNavigation<ProPLCContent, ProPLCContentVM>();
            containerRegistry.RegisterForNavigation<ConfirmBtnContent, ConfirmBtnContentVM>();
            containerRegistry.RegisterForNavigation<FlowWaitContent, FlowWaitContentVM>();
            containerRegistry.RegisterForNavigation<RollSetContent, RollSetContentVM>();
            containerRegistry.RegisterForNavigation<ProTestBottonContent, ProTestBottonContentVM>();

            // �豸����
            containerRegistry.RegisterForNavigation<DeviceDebugContent, DeviceDebugContentVM>();
            //FFU
            containerRegistry.RegisterForNavigation<FFUContent, FFUContentVM>();
            //FFUCom
            containerRegistry.RegisterForNavigation<FFUComContent, FFUComContentVM>();

            containerRegistry.RegisterForNavigation<DustContent, DustContentVM>();

            // 大寰音圈电机曲线监控（作为 AvalonDock 模块挂载到主页）
            containerRegistry.RegisterForNavigation<DHVCMMonitorContent, DHVCMMonitorContentVM>();

            // Ӳ���������ü�������
            containerRegistry.RegisterForNavigation<IntegratedHardwareContent, IntegratedHardwareContentVM>();

            ////  ���ּ���
            //containerRegistry.RegisterForNavigation<DigitalAssContent, DigitalAssContentVM>();      //  ���ּ���һ���˵�����
            //containerRegistry.RegisterForNavigation<IOinspectionContent, IOinspectionContentVM>();  // IO���
            //containerRegistry.RegisterForNavigation<AutoCommunicationConfigContent, AutoCommunicationConfigContentVM>();// ͨѶ�˿��Զ�����            
            //containerRegistry.RegisterForNavigation<ParamConfirmContent, ParamConfirmContentVM>();  // ��������ȷ��
            //containerRegistry.RegisterForNavigation<PlatformLevelAutoConfirmContent, PlatformLevelAutoConfirmContentVM>();// ƽ̨ˮƽ�Զ�ȷ��
            //containerRegistry.RegisterForNavigation<AutomaticLoadCellContent, AutomaticLoadCellContentVM>();// �Զ�LoadCell
            //containerRegistry.RegisterForNavigation<AutomaticEmbossingContent, AutomaticEmbossingContentVM>();// �Զ�ѹӡ
            //containerRegistry.RegisterForNavigation<DigitalVisionContent, DigitalVisionContentVM>();// �Ӿ��궨            
        }
    }
}
