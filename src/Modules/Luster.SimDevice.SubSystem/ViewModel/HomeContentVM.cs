#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       HomeContentVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.SubSystem.ViewModel
* 文 件 名:       HomeContentVM.cs
* 创建时间:       2022/6/21 17:27:29
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      ee2be2cc-6026-42bc-b3cf-1465ba83fc9d
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/21 17:27:29
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using HandyControl.Expression.Shapes;
using Luster.Common.Assets;
using Luster.Common.Assets.Global;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.DataStruct.Real;
using Luster.SimDevice.EngineUI;
using Luster.SimDevice.EngineUI.Events;
using Luster.SimDevice.EngineUI.Models;
using Luster.SimDevice.SubSystem.Config;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Luster.SimDevice.SubSystem.ViewModel
{
    public class HomeContentVM : PageVM
    {
        private SolidColorBrush CheckDeviceSuccessBrush = new SolidColorBrush(Color.FromRgb(0x56, 0xA9, 0xFD));
        private SolidColorBrush CheckDeviceFailBrush = new SolidColorBrush(Color.FromRgb(248, 73, 30));

        private List<AutoCheckDeviceModel> checkDeviceModels=new List<AutoCheckDeviceModel>();

        private ISimDeviceEngineUI _engine;

        private string _checkErrorMessage = string.Empty;
        private bool _checkResult = false;
        private Dispatcher _dispatcher;
        /// <summary>
        /// IO数量
        /// </summary>
        private int _ioCount;
        public int IoCount
        {
            get => _ioCount;
            set { SetProperty(ref _ioCount, value); }
        }

        /// <summary>
        /// 轴卡数量
        /// </summary>
        private int _cardCount;
        public int CardCount
        {
            get { return _cardCount; }
            set { SetProperty(ref _cardCount, value); }
        }

        /// <summary>
        /// 真空数量
        /// </summary>
        private int _vacuumCount;
        public int VacuumCount
        {
            get => _vacuumCount;
            set => SetProperty(ref _vacuumCount, value);
        }

        /// <summary>
        /// 气缸数量
        /// </summary>
        private int _cylinderCount;
        public int CylinderCount
        {
            get => _cylinderCount;
            set { SetProperty(ref _cylinderCount, value); }
        }

        /// <summary>
        /// 回零管理
        /// </summary>
        private bool _isHome;
        public bool IsHome
        {
            get { return _isHome; }
            set { SetProperty(ref _isHome, value); }
        }


        /// <summary>
        /// 自动点检
        /// </summary>
        private bool _isAutoCheck;
        public bool IsAutoCheck
        {
            get { return _isAutoCheck; }
            set { SetProperty(ref _isAutoCheck, value); }
        }


        /// <summary>
        /// 监控标识
        /// </summary>
        private bool _isMoniter;
        public bool IsMoniter
        {
            get { return _isMoniter; }
            set { SetProperty(ref _isMoniter, value); }
        }


        private bool _monitorEnabled;
        public bool MonitorEnabled
        {
            get => _monitorEnabled;
            set { SetProperty(ref _monitorEnabled, value); }
        }

        /// <summary>
        /// 功能切换
        /// </summary>
        public DelegateCommand<string> ChangeParamCommand { get; private set; }

        /// <summary>
        /// 点检命令
        /// </summary>
        public DelegateCommand CheckDeviceCommand { get; private set; }

        /// <summary>
        /// 功能切换
        /// </summary>
        public DelegateCommand EmergencyStopDeviceCommand { get; private set; }

        /// <summary>
        /// 上移命令
        /// </summary>
        public DelegateCommand<HomeItemModel> MoveUpCommand { get; private set; }

        /// <summary>
        /// 下移命令
        /// </summary>
        public DelegateCommand<HomeItemModel> MoveDownCommand { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public DelegateCommand<AutoCheckDeviceModel> GoUpCommand { get; private set; }

        public DelegateCommand<AutoCheckDeviceModel> GoDownCommand { get; private set; }



        /// <summary>
        /// 归零命令
        /// </summary>
        public DelegateCommand HomeCommand { get; private set; }

        /// <summary>
        /// 归零命令
        /// </summary>
        public DelegateCommand StopHomeCommand { get; private set; }

        /// <summary>
        /// 移除监控IO
        /// </summary>
        public DelegateCommand<MonitorIOModel> RemoveMonitorIOCommand { get; private set; }

        public DelegateCommand<VIO> AddMonitorIOCommand { get; private set; }


        public DelegateCommand StartMonitorCommand { get; private set; }

        public DelegateCommand StopMonitorCommand { get; private set; }

        // 定义点检Model集合
        private ObservableCollection<AutoCheckDeviceModel> _autoCheckDevices;
        public ObservableCollection<AutoCheckDeviceModel> AutoCheckModels
        {
            get => _autoCheckDevices;
            set => SetProperty(ref _autoCheckDevices, value);
        }

        /// <summary>
        /// 回零设备集合
        /// </summary>
        private ObservableCollection<HomeItemModel> _homeModels;
        public ObservableCollection<HomeItemModel> HomeModels
        {
            get => _homeModels;
            set => SetProperty(ref _homeModels, value);
        }

        /// <summary>
        /// 屏蔽设备集合
        /// </summary>
        private ObservableCollection<DeviceShieldModel> _deviceShieldModels;
        public ObservableCollection<DeviceShieldModel> DeviceShieldModels
        {
            get => _deviceShieldModels;
            set => SetProperty(ref _deviceShieldModels, value);
        }

        /// <summary>
        /// 所有监控设备
        /// </summary>
        private ObservableCollection<VIO> _vIOs;
        public ObservableCollection<VIO> VIOs
        {
            get => _vIOs;
            set => SetProperty(ref _vIOs, value);
        }

        /// <summary>
        /// 监控设备列表
        /// </summary>
        private ObservableCollection<MonitorIOModel> _monitorIOModels;
        public ObservableCollection<MonitorIOModel> MonitorIOModels
        {
            get => _monitorIOModels;
            set => SetProperty(ref _monitorIOModels, value);
        }


        protected HomeContentVM(ISimDeviceEngineUI engine, Dispatcher dispatcher) : base(engine)
        {
            _engine = engine;
            _dispatcher = dispatcher;
            InitModel(_engine);
            ChangeParamCommand = new DelegateCommand<string>(ChangeParam);
            CheckDeviceCommand = new DelegateCommand(CheckDevice);
            EmergencyStopDeviceCommand = new DelegateCommand(EmergencyStopDevice);
            MoveUpCommand = new DelegateCommand<HomeItemModel>(MoveUp);
            MoveDownCommand = new DelegateCommand<HomeItemModel>(MoveDown);
            GoDownCommand = new DelegateCommand<AutoCheckDeviceModel>(GoDown);
            GoUpCommand = new DelegateCommand<AutoCheckDeviceModel>(GoUp);

            HomeCommand = new DelegateCommand(Home);
            AddMonitorIOCommand = new DelegateCommand<VIO>(AddMonitorIO);
            RemoveMonitorIOCommand = new DelegateCommand<MonitorIOModel>(RemoveMonitorIO);
            StartMonitorCommand = new DelegateCommand(StartMonitor);
            StopMonitorCommand = new DelegateCommand(StopMonitor);
            IsHome = true;
            IsAutoCheck = false;
            IsMoniter = false;
            MonitorEnabled = false;
        }

        protected override void Subscribe(ISimDeviceEngineUI engineUI)
        {
            base.Subscribe(engineUI);

            engineUI.Subscribe<ProjOpenEvent, ProjectInfo>(obj =>
            {
                InitModel(_engine);
                GlobalProperty.IsEnabeld = true;
            });

            // 设备发生变更
            engineUI.Subscribe<DeviceChangedEvent>(() =>
            {
                InitModel(_engine);
            });
        }
        private void StopMonitor()
        {
            MonitorEnabled = true;
            _engine.Engine.StopDeviceMonitor();
        }

        private void StartMonitor()
        {
            MonitorEnabled = false;
            _engine.Engine.StartDeviceMonitor();
        }

        /// <summary>
        /// 移除监控设备
        /// </summary>
        /// <param name="monitor"></param>
        private void RemoveMonitorIO(MonitorIOModel monitor)
        {
            var io = monitor.Tag;
            monitor.Tag.Montor = Motion.DataStruct.Enums.IOMonitor.None;
            var nextIo = VIOs.Where(x => x.Index > io.Index && x.Behavior == io.Behavior).FirstOrDefault();
            if (nextIo == null)
            {
                if (io.Behavior == Motion.DataStruct.Enums.IOBehavior.Input)
                {
                    var outIo = VIOs.Where(x => x.Behavior == Motion.DataStruct.Enums.IOBehavior.Output).FirstOrDefault();
                    if (outIo == null)
                    {
                        VIOs.Add(io);
                    }
                    else
                    {
                        var index = VIOs.IndexOf(outIo);
                        VIOs.Insert(index, io);
                    }
                }
            }
            else
            {
                var index = VIOs.IndexOf(nextIo);
                VIOs.Insert(index, io);
            }
            MonitorIOModels.Remove(monitor);
            UpdateMonitorIo();
        }

        /// <summary>
        /// 添加监控设备
        /// </summary>
        /// <param name="io"></param>
        private void AddMonitorIO(VIO io)
        {
            var model = new MonitorIOModel(io);
            VIOs.Remove(io);
            MonitorIOModels.Add(model);
            UpdateMonitorIo();
        }

        /// <summary>
        /// 跟新添加监控设备序号
        /// </summary>
        private void UpdateMonitorIo()
        {
            var index = 1;
            foreach (var model in MonitorIOModels)
            {
                model.Index = index;
                index++;
            }
        }

        /// <summary>
        /// 回零
        /// </summary>
        private void Home()
        {
            if (HomeModels.Count > 0)
            {
                foreach (var home in HomeModels)
                {
                    if (home.NeedHome)
                    {
                        home.Tag.Home();
                    }
                }
            }
        }

        /// <summary>
        /// 下移
        /// </summary>
        /// <param name="model"></param>
        private void MoveDown(HomeItemModel model)
        {
            var index = HomeModels.IndexOf(model);
            if (index == HomeModels.Count - 1)
            {
                return;
            }
            var tempModel = HomeModels[index + 1];
            HomeModels.Remove(tempModel);
            HomeModels.Insert(index, tempModel);
            UpdateHomeSort();
        }

        /// <summary>
        /// 上移
        /// </summary>
        /// <param name="model"></param>
        private void MoveUp(HomeItemModel model)
        {
            var index = HomeModels.IndexOf(model);
            if (index == 0)
            {
                return;
            }
            var tempModel = HomeModels[index - 1];
            HomeModels.Remove(tempModel);
            HomeModels.Insert(index, tempModel);
            UpdateHomeSort();
        }

        /// <summary>
        /// 更新回零排序
        /// </summary>
        public void UpdateHomeSort()
        {
            int sort = 0;
            foreach (var item in HomeModels)
            {
                item.Index = sort + 1;
                item.Tag.HomeSort = sort;
                sort++;
            }
        }


        private void GoDown(AutoCheckDeviceModel deviceModel)
        {
            var index = AutoCheckModels.IndexOf(deviceModel);
            if (index == AutoCheckModels.Count - 1)
            {
                return;
            }
            var tempModel = AutoCheckModels[index + 1];
            AutoCheckModels.Remove(tempModel);
            AutoCheckModels.Insert(index, tempModel);
            UpdateCheckSort();
        }
        private void GoUp(AutoCheckDeviceModel deviceModel)
        {
            var index = AutoCheckModels.IndexOf(deviceModel);
            if (index ==0)
            {
                return;
            }
            var tempModel = AutoCheckModels[index -1];
            AutoCheckModels.Remove(tempModel);
            AutoCheckModels.Insert(index, tempModel);
            UpdateCheckSort();
        }

        public void UpdateCheckSort()
        {
            int Index = 0;
            foreach (var item in AutoCheckModels)
            {
                item.Index = Index + 1;
                item.DeviceTag.CheckSort=Index;
                Index++;
            }
        }

        #region 初始化数据
        private void InitModel(ISimDeviceEngineUI engine)
        {
            _dispatcher.Invoke(() =>
            {
                // 获取IO数量
                IoCount = engine.Engine.GetDevices(typeof(VIO)).Count();

                // 获取轴数量
                CardCount = engine.Engine.GetDevices(typeof(VAxis)).Count();

                // 获取真空数量
                VacuumCount = engine.Engine.GetDevices(typeof(VVacuum)).Count();

                // 获取气缸数量
                CylinderCount = engine.Engine.GetDevices(typeof(VCylinder)).Count();

                // 获取点检设备列表
                SetAutoCheckDevice(engine);

                // 获取需要归零的设备
                SetHomeDevice(engine);

                // 获取屏蔽设备
                SetShieldDevice(engine);

                //获取IO设备
                SetIoDevice(engine);
            });
        }
                

        /// <summary>
        /// 设置点检设备列表
        /// </summary>
        /// <param name="engine"></param>
        private void SetAutoCheckDevice(ISimDeviceEngineUI engine)
        {
            //var devices = engine.Engine.GetDevices(typeof(IAutoCheck)).OrderBy(x => x.Sort).ToList();
            var devices = engine.Engine.GetDevices()
            .Where(u=>u is IAutoCheck check)
            .Select(u => u as IAutoCheck)
            .OrderBy(u => u.CheckSort)
            .ToList();
            AutoCheckModels = new ObservableCollection<AutoCheckDeviceModel>();

            var index = 1;
            foreach (var device in devices)
            {
                var checkModel = new AutoCheckDeviceModel(device);
                checkModel.Index = index;
                AutoCheckModels.Add(checkModel);
                index++;
            }
        }

        /// <summary>
        /// 设置回零设备列表
        /// </summary>
        /// <param name="engine"></param>
        private void SetHomeDevice(ISimDeviceEngineUI engine)
        {
            HomeModels = new ObservableCollection<HomeItemModel>();
            // 获取所有回零对象
            var homeDevices = engine.Engine.GetDevices()
                        .Where(u => u is not VAxisM && u is IHome home)
                        .Select(u => u as IHome)
                        .OrderBy(u => u.HomeSort)
                        .ToList();

            var index = 1;
            foreach (var device in homeDevices)
            {
                var homeModel = new HomeItemModel(device);
                homeModel.Index = index;
                HomeModels.Add(homeModel);
                index++;
            }
        }

        /// <summary>
        /// 设置监控设备列表
        /// </summary>
        /// <param name="engine"></param>
        private void SetIoDevice(ISimDeviceEngineUI engine)
        {
            VIOs = new ObservableCollection<VIO>();
            MonitorIOModels = new ObservableCollection<MonitorIOModel>();
            var ios = engine.Engine.GetDevices(typeof(VIO))
                .Select(u => u as VIO)
                .Where(x => x.IOType == Motion.DataStruct.Enums.IOType.Digital).ToList();

            var monitorIos = ios.Where(x => x.Montor != Motion.DataStruct.Enums.IOMonitor.None);

            foreach (var device in monitorIos)
            {
                MonitorIOModels.Add(new MonitorIOModel(device));
            }
            UpdateMonitorIo();

            var otherIOs = ios.Where(x => x.Montor == Motion.DataStruct.Enums.IOMonitor.None);

            VIOs.AddRange(otherIOs);

        }

        /// <summary>
        /// 设置屏蔽设备列表
        /// </summary>
        /// <param name="engine"></param>
        private void SetShieldDevice(ISimDeviceEngineUI engine)
        {
            DeviceShieldModels = new ObservableCollection<DeviceShieldModel>();
            // 获取屏蔽管理对象
            var shieldDevices = engine.Engine.GetDevices()
                        .Where(u => u is IDeviceShield shield)
                        .Select(u => u as IDeviceShield)
                        .ToList();

            var index = 1;
            foreach (var device in shieldDevices)
            {
                var model = new DeviceShieldModel(device);
                model.Index = index;
                DeviceShieldModels.Add(model);
                index++;
            }
        }

        #endregion


        /// <summary>
        /// 开始点检
        /// </summary>
        private void CheckDevice()
        {
            checkDeviceModels.Clear();
            if (AutoCheckModels != null && AutoCheckModels.Count > 0)
            {
                ///开始点检时清空界面
                foreach(var _model in AutoCheckModels)
                {
                    _model.Result = "";
                    _model.ErrorMessage = "";
                    _model.CheckTime = "";
                }

                foreach (var model in AutoCheckModels)
                {
                    model.DeviceTag.OnCheckEvent += DeviceTag_OnCheckEvent;
                    model.CheckTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    if (model.DeviceTag.Check())
                    {
                        model.Result = "OK";
                        model.ErrorMessage = "";
                        model.ForeColor = CheckDeviceSuccessBrush;
                        checkDeviceModels.Add(model);
                    }
                    else
                    {
                        model.Result = "Failed";
                        model.ErrorMessage = _checkErrorMessage;
                        model.ForeColor = CheckDeviceFailBrush;
                        ///点检失败后确认是否继续,不继续就退出
                        if (!DeviceCheckContinue())
                        {
                            checkDeviceModels.Add(model);
                            model.DeviceTag.OnCheckEvent -= DeviceTag_OnCheckEvent;
                            break;
                        }
                        checkDeviceModels.Add(model);
                    }
                    model.DeviceTag.OnCheckEvent -= DeviceTag_OnCheckEvent;
                }
               var saveFileDialog = new Microsoft.Win32.SaveFileDialog();
                saveFileDialog.Filter = "CSV|*.csv";
                if (saveFileDialog.ShowDialog()==true)
                {
                    Luster.Common.Tools.CSVTool.SaveCSV(checkDeviceModels,saveFileDialog.FileName, Encoding.UTF8);
                }
                Luster.Common.Tools.CSVTool.SaveCSV(checkDeviceModels, $"D:\\Luster\\Check\\AutoCheckDevice{DateTime.Now.ToString("yyyy-MM-dd HH")}.csv", Encoding.UTF8);
            }
        }

        private bool DeviceCheckContinue() 
        {
            bool isContinue=false;
            dialogService.ShowConfirm("点检异常是否继续", r =>
            {

                if (r.Result == ButtonResult.OK)
                {
                    isContinue= true;
                }
                else
                {
                    isContinue= false;
                }
            });
            return isContinue;
        }


        private bool DeviceTag_OnCheckEvent(string arg,bool result)
        {
            
            dialogService.ShowConfirm(arg, r =>
            {
                ///默认人工来判定点检结果
                if (result)
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        _checkResult = true;
                    }
                    else
                    {
                        _checkResult = false;
                        _checkErrorMessage = $"{arg} :否";
                    }
                }
                ///通讯等需要自动判定点检结果
                else
                {
                    _checkResult = false;
                    _checkErrorMessage = $"{arg} :否";
                }
            });
            return _checkResult;
        }

        /// <summary>
        /// 紧急停止
        /// </summary>
        private void EmergencyStopDevice()
        {

        }

        /// <summary>
        /// 需要对Home页面做缓存，否则安全校验区域会有异常
        /// </summary>
        /// <param name="navigationContext"></param>
        /// <returns></returns>
        public override bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        /// <summary>
        /// 切换视图
        /// </summary>
        /// <param name="paras"></param>
        private void ChangeParam(string paras)
        {
            IsHome = paras == "Home";
            IsAutoCheck = paras == "Check";
            IsMoniter = paras == "Monitor";
        }
    }
}