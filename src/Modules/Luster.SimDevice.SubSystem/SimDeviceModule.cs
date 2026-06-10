#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       SimDeviceModule
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.SubSystem
* 文 件 名:       SimDeviceModule.cs
* 创建时间:       2022/4/13 14:16:51
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      3446f075-9dd2-4be5-b727-6dcf03f7b7ed
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/13 14:16:51
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.SimDevice.Engine;
using Luster.SimDevice.SubSystem.ViewModel;
using Luster.SimDevice.SubSystem.Views;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Modularity;
using Luster.SimDevice.SubSystem.Views.Device;
using Luster.SimDevice.SubSystem.ViewModel.Device;
using Luster.SimDevice.SubSystem.ViewModel.Dialog;
using Luster.SimDevice.SubSystem.Views.Dialog;
using Luster.SimDevice.EngineUI;
using Luster.SimDevice.SubSystem.Views.Virtual;
using Luster.SimDevice.SubSystem.ViewModel.Virtual;
using System.Windows.Threading;
using System.Windows;
using Luster.Motion.DataStruct;
using Luster.Common.Assets.Views;
using Luster.Common.Assets.ViewModel;

namespace Luster.SimDevice.SubSystem
{
    public class SimDeviceModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            // 视图注册
            var regionManager = containerProvider.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion("DeviceMenuRegion", typeof(LeftMenuContent));
            regionManager.RegisterViewWithRegion("DeviceMainRegion", typeof(MainContent));
            regionManager.RegisterViewWithRegion("DeviceLogRegion", typeof(LogContent));
            regionManager.RegisterViewWithRegion("DeviceContent", typeof(SimDeviceControl));

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<Dispatcher>(() => Application.Current.Dispatcher);

            // 注册视图和模型示例
            ViewModelLocationProvider.Register<SimDeviceControl, DeviceContentVM>();
            ViewModelLocationProvider.Register<MainContent, MainContentVM>();
            ViewModelLocationProvider.Register<LeftMenuContent, LeftMenuVM>();
            ViewModelLocationProvider.Register<LogContent, LogContentVM>();

            //
            containerRegistry.RegisterForNavigation<HomeContent, HomeContentVM>();
            containerRegistry.RegisterForNavigation<GroupDebugContent, GroupDebugContentVM>();
            containerRegistry.RegisterForNavigation<ControlParaContent, ControlParaVM>();


            // 注册Navigation视图（硬件）
            containerRegistry.RegisterForNavigation<CameraContent, CameraContentVM>();
            containerRegistry.RegisterForNavigation<LineLaserContent, LineLaserContentVM>();
            containerRegistry.RegisterForNavigation<MotionCardContent, MotionCardContentVM>();
            containerRegistry.RegisterForNavigation<LightControllerContent, LightControllerContentVM>();
            containerRegistry.RegisterForNavigation<RobotContent, RobotContentVM>();

            // 注册Navigation视图（虚拟设备）
            containerRegistry.RegisterForNavigation<VAxisContent, VAxisContentVM>();            // 轴卡
            containerRegistry.RegisterForNavigation<VIOContent, VIOContentVM>();                // IO信号
            containerRegistry.RegisterForNavigation<VLineLaserContent, VLineLaserContentVM>();  // 3D激光
            containerRegistry.RegisterForNavigation<VCameraContent, VCameraContentVM>();        // 2D相机
            containerRegistry.RegisterForNavigation<VVacuumContent, VVacuumContentVM>();        // 真空
            containerRegistry.RegisterForNavigation<VCylinderContent, VCylinderContentVM>();    // 气缸
            containerRegistry.RegisterForNavigation<VAxisMContent, VAxisMContentVM>();          // 多轴
            containerRegistry.RegisterForNavigation<VPrinterContent, VPrinterContentVM>();      // 仿真打印机
            containerRegistry.RegisterForNavigation<PrinterContent, PrinterContentVM>();      // 打印机
            containerRegistry.RegisterForNavigation<VPlcContent, VPlcContentVM>();
            containerRegistry.RegisterForNavigation<VIOSimulationContent, VIOSimulationContentVM>();      //串口
            containerRegistry.RegisterForNavigation<VCommuncationContent, VCommuncationContentVM>();      //通用通信
            containerRegistry.RegisterForNavigation<VPCylinderContent, VPCylinderContentVM>();
            containerRegistry.RegisterForNavigation<AxisPosContent, AxisPosContentVM>();        // 轴点位内容配置
            containerRegistry.RegisterForNavigation<VRobotContent, VRobotContentVM>();   //仿真机器人


            containerRegistry.RegisterForNavigation<VFlyingPhotoContent, VFlyingPhotoContentVM>();   //仿真飞怕模块
            containerRegistry.RegisterForNavigation<AxisIODebugContent, AxisIODebugContentVM>();     // 轴点位内容配置
            containerRegistry.RegisterForNavigation<VPlcIOContent, VPlcIOContentVM>();               // PLC IO 监控

            // 注册保养页面  
            containerRegistry.RegisterForNavigation<MaintainContent, MaintainContentVM>();

            // 注册错误页面
            containerRegistry.RegisterForNavigation<ErrorContent, ErrorContentVM>();
            containerRegistry.RegisterForNavigation<ErrorContentCustom, ErrorContentCustomVM>();

            //注册关键参数配置页面
            containerRegistry.RegisterForNavigation<PositionParameterContent, PositionParameterContentVM>();
            containerRegistry.RegisterForNavigation<KeyParameterContent, KeyParameterContentVM>();

            containerRegistry.RegisterForNavigation<FXTCPContent, FXTCPContentVM>();
            
            ///注册模组
            containerRegistry.RegisterForNavigation<VModuleContent, VModuleContentVM>();

            containerRegistry.RegisterForNavigation<ModuleNameContent, ModuleNameContentVM>();

            // 注册Dialog
            containerRegistry.RegisterDialog<Luster.Common.Assets.Views.MessageDialog, Luster.Common.Assets.ViewModel.MessageDialogVM>();
            containerRegistry.RegisterDialog<LineLaserDialog, LineLaserDialogVM>();
            containerRegistry.RegisterDialog<CameraDialog, CameraDialogVM>();
            containerRegistry.RegisterDialog<MotionCardDialog, MotionCardDialogVM>();
            containerRegistry.RegisterDialog<LightDialog, LightDialogVM>();
            containerRegistry.RegisterDialog<RobotDialog, RobotDialogVM>();

            containerRegistry.RegisterDialog<AxisDialog, AxisDialogVM>();
            containerRegistry.RegisterDialog<IODialog, IODialogVM>();

            containerRegistry.RegisterDialog<IOUpdateDialog, IOUpdateDialogVM>();

            containerRegistry.RegisterDialog<VideoDialog, VideoDialogVM>();
            containerRegistry.RegisterDialog<VacuumDialog, VacuumDialogVM>(); // 真空Dialog
            containerRegistry.RegisterDialog<CylinderDialog, CylinderDialogVM>(); // 气缸Dialog
            containerRegistry.RegisterDialog<SearchCameraDialog, SearchCameraDialogVM>();
            containerRegistry.RegisterDialog<SearchLineLaserDialog, SearchLineLaserDialogVM>();//线扫相机
            containerRegistry.RegisterDialog<VCameraDialog, VCameraDialogVM>();
            containerRegistry.RegisterDialog<VLineLaserDialog, VLineLaserDialogVM>();//线扫参数设置
            containerRegistry.RegisterDialog<VAxisMDialog, VAxisMDialogVM>();
            containerRegistry.RegisterDialog<VPrinterDialog, VPrinterDialogVM>(); //仿真打印机

            containerRegistry.RegisterDialog<VRobotDialog, VRobotDialogVM>(); //仿真机器人

            containerRegistry.RegisterDialog<VFlyingPhotoDialog, VFlyingPhotoDialogVM>(); //仿真飞拍

            containerRegistry.RegisterDialog<PrinterDialog, PrinterDialogVM>();             //注册打印机Dialog
            containerRegistry.RegisterDialog<ImportExDialog, ImportExDialogVM>();           //按钮Dialog
            containerRegistry.RegisterDialog<IOSimDialog, IOSimDialogVM>();                 //仿真IO设备
            containerRegistry.RegisterDialog<IOActionDialog, IOActionDialogVM>();           //仿真IO动作
            containerRegistry.RegisterDialog<SocketDialog, SocketDialogVM>();           //仿真IO动作
            containerRegistry.RegisterDialog<SlaveActionDialog, SlaveActionDialogVM>();
            containerRegistry.RegisterDialog<SocketActionDialog, SocketActionDialogVM>();           //仿真IO动作
            containerRegistry.RegisterDialog<PCylinderDialog, PCylinderDialogVM>();
            containerRegistry.RegisterDialog<SafeRegionDialog, SafeRegionDialogVM>();

            containerRegistry.RegisterDialog<PosionSafeRegionDialog, PosionSafeRegionDialogVM>();

            containerRegistry.RegisterDialog<VPlcDialog, VPlcDialogVM>();
            containerRegistry.RegisterDialog<PlcAddressDialog, PlcAddressDialogVM>();
            containerRegistry.RegisterDialog<ImportAddressDialog, ImportAddressDialogVM>();
            containerRegistry.RegisterDialog<CommuncationAddressDialog, CommuncationAddressDialogVM>();
            containerRegistry.RegisterDialog<TeachPositionDialog, TeachPositionDialogVM>();    //注册示教添加窗口
            containerRegistry.RegisterDialog<TeachRobotPositionDialog, TeachRobotPositionDialogVM>();    //注册机器人示教窗口
            containerRegistry.RegisterDialog<TeachFlyingPhotoPointDialog, TeachFlyingPhotoPointDialogVM>();    //注册飞拍模块示教窗口

            containerRegistry.RegisterDialog<CalcDialog, CalcDialogVM>();
            containerRegistry.RegisterDialog<MotionConditionsDialog, MotionConditionsDialogVM>();

            containerRegistry.RegisterDialog<ErrorCustomDialog, ErrorCustomDialogVM>();

            containerRegistry.RegisterDialog<ModuleNameDialog, ModuleNameDialogVM>();

            // 注册引擎
            if (!containerRegistry.IsRegistered<IDeviceEngine>())
            {
                containerRegistry.RegisterSingleton<IDeviceEngine, DeviceEngine>();
            }

            // 注册引擎
            containerRegistry.RegisterSingleton<ISimDeviceEngineUI, SimDeviceEngineUI>();

            // 设备
            containerRegistry.RegisterForNavigation<SimDeviceControl>();

            containerRegistry.RegisterDialogWindow<DialogWindow>();
        }
    }
}