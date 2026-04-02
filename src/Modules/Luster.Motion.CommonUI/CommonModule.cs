#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       CommonModule
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.CommonUI
* 文 件 名:       CommonModule.cs
* 创建时间:       2022/6/16 17:15:14
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      2a3fa8c4-32d7-4d8e-a2be-27ff7a9a00cf
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/16 17:15:14
* 修 改 人:		  L05123
************************************************************************************/
#endregion
using Luster.Common.Assets.FloatingInfo.Services;
using Luster.Common.Assets.FloatingInfo.ViewModel;
using Luster.Common.Assets.FloatingInfo.Views;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.CommonUI.ViewModel.Dialogs;
using Luster.Motion.CommonUI.Views;
using Luster.Motion.CommonUI.Views.Dialogs;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Luster.Motion.CommonUI
{
    public class CommonModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            //containerRegistry.Register<Dispatcher>(() => Application.Current.Dispatcher);

            // 注册浮动信息服务
            containerRegistry.RegisterSingleton<IFloatingInfoConfigService, FloatingInfoConfigService>();
            containerRegistry.RegisterSingleton<IFloatingInfoService, FloatingInfoService>();
            containerRegistry.RegisterDialog<FloatingInfoSettingsDialog, FloatingInfoSettingsDialogVM>(); // 浮动信息设置对话框
            // 通用方法及事件总线
            if (!containerRegistry.IsRegistered<ICommonBus>())
            {
                containerRegistry.RegisterSingleton<ICommonBus, CommonBus>();
            }

            if (!containerRegistry.IsRegistered<IDbManager>())
            {
                containerRegistry.RegisterSingleton<IDbManager, DbManager>();
            }


            // 注册视图和模型示例
            ViewModelLocationProvider.Register<Luster.Common.Assets.Views.MessageDialog, Luster.Common.Assets.ViewModel.MessageDialogVM>();
            ViewModelLocationProvider.Register<Luster.Common.Assets.Views.TextDialog, Luster.Common.Assets.ViewModel.TextDialogVM>();
            ViewModelLocationProvider.Register<Luster.Common.Assets.Views.InfoInputDialog, Luster.Common.Assets.ViewModel.InfoInputDialogVM>();

            // log
            containerRegistry.RegisterForNavigation<LogContent, LogContentVM>();

            // 心跳监控

            containerRegistry.RegisterForNavigation<CacheDataContent, CacheDataContentVM>();
            containerRegistry.RegisterDialog<DeviceDialog, DeviceDialogVM>();       // 设备Dialog选择
            containerRegistry.RegisterDialog<AlarmCodeDialog, AlarmCodeDialogVM>();       // 设备Dialog选择

            containerRegistry.RegisterDialog<SlaveDialog, SlaveDialogVM>();         // 从站Dialog选择
            containerRegistry.RegisterDialog<AlarmDialog, AlarmDialogVM>();       // 报警Dialog选择
            containerRegistry.RegisterDialog<HiveAlarmDialog, HiveAlarmDialogVM>();//Hive Dialog
            containerRegistry.RegisterDialog<HomeConfirmDialog, HomeConfirmDialogVM>();       // 报警Dialog选择
            containerRegistry.RegisterDialog<ClassDialog, ClassDialogVM>();       // 班别Dialog选择
            containerRegistry.RegisterDialog<AddUserDialog, AddUserDialogVM>();       // 添加用户Dialog选择
            containerRegistry.RegisterDialog<SetStationDialog, SetStationDialogVM>();       // 工站设置Dialog选择
            containerRegistry.RegisterDialog<CreateProjectDialog, CreateProjectDialogVM>();  // 创建项目
            containerRegistry.RegisterDialog<PlcAlarmDialog, PlcAlarmDialogVM>();       // Plc报警Dialog选择
            containerRegistry.RegisterDialog<SetOutPutChartDialog, SetOutPutChartDialogVM>();       // Plc报警Dialog选择
            containerRegistry.RegisterDialog<LightCurtainDialog, LightCurtainDialogVM>();   //添加光幕 
            containerRegistry.RegisterDialog<AddOperationTipDialog, AddOperationTipDialogVM>();//  添加操作提示
            containerRegistry.RegisterDialog<ShowOperationTipDialog, ShowOperationTipDialogVM>();//  添加操作提示

            containerRegistry.RegisterDialog<ErrorPasswordDialog, ErrorPasswordDialogVM>();   //添加密码错误提示 

            containerRegistry.RegisterDialog<HiveOperationDialog, HiveOperationDialogVM>();//  添加Hive提示
            containerRegistry.RegisterDialog<HiveStartDialog, HiveStartDialogVM>();//  添加Hive 启动提示
            containerRegistry.RegisterDialog<HiveStartDialog2, HiveStartDialog2VM>();//  添加Hive 启动提示
            containerRegistry.RegisterDialog<HiveOnboardAlarmDialog, HiveOnboardAlarmDialogVM>();//  添加Hive 状态报警提示
            containerRegistry.RegisterDialog<HiveStartDialog2Part1, HiveStartDialog2Part1VM>();//  添加Hive 启动Part1提示
            containerRegistry.RegisterDialog<HiveStartDialog2Part2, HiveStartDialog2Part2VM>();//  添加Hive 启动Part2提示
            containerRegistry.RegisterDialog<HiveStopDialog, HiveStopDialogVM>();//  添加Hive 启动提示
            containerRegistry.RegisterDialog<HiveStopDialog2, HiveStopDialog2VM>();//  添加Hive 停止提示
            containerRegistry.RegisterDialog<HivePauseDialog, HivePauseDialogVM>();//  添加Hive 暂停提示
            containerRegistry.RegisterDialog<HiveStopDialog3, HiveStopDialog3VM>();//  添加Hive 停止提示
            containerRegistry.RegisterDialog<HiveStopDialog4, HiveStopDialog4VM>();//  添加Hive 停止提示

            containerRegistry.RegisterDialog<ChangeAccessoryDialog, ChangeAccessoryDialogVM>();// 辅料更换
            containerRegistry.RegisterDialog<EditBarcodeDialog, EditBarcodeVM>();
            containerRegistry.RegisterDialog<VersionDialog, VersionDialogVM>();
            containerRegistry.RegisterDialog<RecipeVersionDialog, RecipeVersionDialogVM>();
            containerRegistry.RegisterDialog<SetOpenCloseDoorDialog, SetOpenCloseDoorDialogVM>();
            containerRegistry.RegisterDialog<SetSysOperateIODialog, SetSysOperateIODialogVM>();
            containerRegistry.RegisterDialog<InteractiveDialog, InteractiveDialogVM>();

            containerRegistry.RegisterDialog<ChangeUserDialog, ChangeUserDialogVM>();
            containerRegistry.RegisterForNavigation<CSharpEditor, CSharpEditorVM>();
            containerRegistry.RegisterForNavigation<PythonEditor, PythonEditorVM>();


        }
    }
}