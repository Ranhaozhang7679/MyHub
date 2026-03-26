#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       UserDefineMainContentVM
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.SubSystem.ViewModel
* 文 件 名:       UserDefineMainContentVM.cs
* 创建时间:       2022/9/6 15:46:33
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      d8a7bc81-f833-423e-8960-603956805ecb
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/6 15:46:33
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using Luster.Common.DataAccess.Tables;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.Tools;
using Luster.Control.Wpf.Motion.Controls;
using Luster.Controls.Wpf.Motion.Commands;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Dock;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.CommonUI.Models;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.EditorUI;
using Luster.Motion.EditorUI.Events;
using Luster.Motion.EditorUI.Extensions;
using Luster.Motion.Integration.Web;
using Luster.Motion.SubSystem.Models;
using Luster.Motion.TaskFlow.Engine;
using Luster.Motion.TaskFlow.Engine.Engine;
using Luster.Motion.TaskFlow.Engine.Models;
using Luster.SimDevice.EngineUI.Models;
using Luster.SimDevice.SubSystem.Events;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Logic;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Configuration;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Luster.Motion.SubSystem.ViewModel
{
    /// <summary>
    /// 首页缓存报表
    /// </summary>
    public class ProFXContentVM : MotionVM, IDockContent
    {
        private readonly string PDCANAME = "Extend_PDCA启用";
        private readonly string SFCNAME = "Extend_SFC启用";
        private readonly string CPKNAME = "Extend_CPK启用";


        // 带Tray盘空跑
        private readonly string TRAYNAME = "Extend_TrayEmpty";

        /// <summary>
        /// 数据库访问
        /// </summary>
        private IDbManager _dbManager;

        /// <summary>
        /// 运控控制
        /// </summary>
        private IMotionController _mController;

        /// <summary>
        /// UI线程
        /// </summary>
        private Dispatcher _dispatcher;

        /// <summary>
        /// 循环
        /// </summary>
        private WhileTool? whileTool;

        private List<IMotionModule> _stations = new List<IMotionModule>();

        /// <summary>
        /// 流程Bus
        /// </summary>
        private FlowBus flowBus;

        public class ButtonData
        {
            public string Content { get; set; }
        }

        private ObservableCollection<ButtonData> _buttons;

        public ObservableCollection<ButtonData> Buttons
        {
            get => _buttons;
            set
            {
                if (_buttons != value)
                {
                    _buttons = value;
                    OnPropertyChanged(nameof(Buttons));
                }
            }
        }
        private ObservableCollection<RunningModuleItemModel> _runningModuleList;
        public ObservableCollection<RunningModuleItemModel> RunningModuleList
        {
            get { return _runningModuleList; }
            set { SetProperty(ref _runningModuleList, value); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public IDeviceEngine Engine { get; set; }
        /// <summary>
        /// PDCA属性
        /// </summary>
        private ParameterAttribute _pdcaAttr = null;
        private ParameterAttribute _sfcAttr = null;
        private ParameterAttribute _cpkAttr = null;
        private ParameterAttribute _trayAttr = null;
        private IErrorManager _errorManager = null;
        private IDialogService _dialogService;
        private IDeviceEngine _deviceEngine = null;
        private HiveAPI hiveAPI;


        public event Action<Brush> HiveRepairStartEvent;

        public ProFXContentVM() 
        { 
            _buttons = new ObservableCollection<ButtonData>();
        }


        public ProFXContentVM(ICommonBus _commonBus, IDbManager dbManager,
            IMotionController motionController, IErrorManager errorManager, IDeviceEngine deviceEngine,
            Dispatcher dispatcher, IDialogService dialogService, HiveAPI _hiveApi, FlowBus _flowBus) : base(_commonBus)
        {
            flowBus = _flowBus;
            _dbManager = dbManager;
            _mController = motionController;
            _dispatcher = dispatcher;
            _errorManager = errorManager;
            _deviceEngine = deviceEngine;
            hiveAPI = _hiveApi;


            InitGlobal();
            // 手动模式或自动模式
            _mController.CanAutoRunEvent -= MController_CanAutoRunEvent;
            _mController.CanAutoRunEvent += MController_CanAutoRunEvent;
            _dialogService = dialogService;
        }

        /// <summary>
        /// 显示当前机台是手动模式，还是自动模式
        /// </summary>
        /// <param name="obj"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void MController_CanAutoRunEvent(bool obj)
        {
            IsAutoMode = obj;
        }

        private List<ButtonData> data = new List<ButtonData>();
        /// <summary>
        /// 初始化全局变量
        /// </summary>
        private void InitGlobal()
        {
            TrayVisible = false;
            // 对SFC和PDCA进行监控
            var globalModule = _mController.MotionEngine.Get(GlobalModule.GlobalID);
            foreach (var item in globalModule.Parameters)
            {
                if (item.Key == PDCANAME || item.Key == SFCNAME || item.Key == CPKNAME || item.Key == TRAYNAME)
                {
                    if (item.Value != null)
                    {
                        item.Value.IsBindUI = true;
                        item.Value.IsSensitive = true;
                    }
                }
            }

            globalModule.PropertyChangedEvent -= GlobalModule_PropertyChangedEvent;
            globalModule.PropertyChangedEvent += GlobalModule_PropertyChangedEvent;

            if (globalModule.Parameters.ContainsKey(PDCANAME))
            {
                _pdcaAttr = globalModule.Parameters[PDCANAME];
                if (_pdcaAttr != null && bool.TryParse(_pdcaAttr.Value?.ToString(), out var isEnabled))
                {
                    PDCAEnabled = isEnabled;
                }
            }

            if (globalModule.Parameters.ContainsKey(SFCNAME))
            {
                _sfcAttr = globalModule.Parameters[SFCNAME];
                if (_sfcAttr != null && bool.TryParse(_sfcAttr.Value?.ToString(), out var isEnabled))
                {
                    SFCEnabled = isEnabled;
                }
            }

            if (globalModule.Parameters.ContainsKey(CPKNAME))
            {
                _cpkAttr = globalModule.Parameters[CPKNAME];
                if (_cpkAttr != null && bool.TryParse(_cpkAttr.Value?.ToString(), out var isEnabled))
                {
                    CPKEnabled = isEnabled;
                }
            }

            if (globalModule.Parameters.ContainsKey(TRAYNAME))
            {
                _trayAttr = globalModule.Parameters[TRAYNAME];
                if (_trayAttr != null && bool.TryParse(_trayAttr.Value?.ToString(), out var isEnabled))
                {
                    TrayEnabled = isEnabled;
                }

                // 显示空盘功能
                TrayVisible = true;
            }

            InitMotionModules();
        }

        private void InitMotionModules()
        {
            // 确保在UI线程更新绑定集合，避免 CollectionView/ObservableCollection 跨线程修改
            if (_dispatcher != null && !_dispatcher.CheckAccess())
            {
                _dispatcher.Invoke(InitMotionModules);
                return;
            }

            if (whileTool != null)
            {
                whileTool.Stop();
                whileTool = null;
            }

            _stations.Clear();
            _stations = _mController.MotionEngine.GetStations();
            Buttons?.Clear();
            Buttons = new ObservableCollection<ButtonData>();
            RunningModuleList?.Clear();
            RunningModuleList = new ObservableCollection<RunningModuleItemModel>();
            for (int i = 0; i < _stations.Count; i++)
            {
                //if (stations[i].TaskFunction.Alias == "TestStation")
                //{
                //    AddNewButton(stations[i].Alias);
                //}

                switch (_stations[i].TaskFunction.Alias)
                {
                    case "TestStation":
                        //AddNewButton(_stations[i].Alias);
                        break;
                    case "FreeStation":
                        RunningModuleList.Add(new RunningModuleItemModel(_stations[i].Alias, _stations[i].ID.ToString()));
                        break;
                }
            }

            whileTool = new WhileTool(true);
            // 开启一个循环读取
            Task.Run(() =>
            {
                whileTool.While(Refresh, 5000);
            });
        }

        public void Refresh()
        {
            _dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                if (_mController.MachineStatus == EngineStatus.Running)
                {
                    try
                    {
                        foreach (var item in RunningModuleList)
                        {
                            var module = _stations.Where(s => s.ID.ToString() == item.StationID).FirstOrDefault()?.TaskFunction as IFreeStation;
                            var runModule = module?.GetRunningModule();
                            item.ModuleName = runModule?.Alias ?? string.Empty;
                            item.ModuleID = runModule?.ID.ToString() ?? string.Empty;
                            string path = string.Empty;
                            runModule?.GetModuleAlias(runModule, ref path);
                            item.ModuleAlias = path;
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }));
        }

        public void AddNewButton(string data)
        {
            Buttons.Add(new ButtonData
            {
                Content = data
            });
        }
        public void OnButtonClicked(string buttonName)
        {
            if (_deviceEngine.GetMachineStatus() == EngineStatus.Ready)
            {
                var stations = _mController.MotionEngine.GetStations();
                foreach (var stat in stations)
                {
                    if (stat.Alias == buttonName)
                    {
                        flowBus.OnRunOne(stat.ID);
                    }
                }
            }
            else
            {
                System.Windows.MessageBox.Show(
                    "回零完成后方可运行测试流程",
                    "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error
                );
            }
        }
        private void OnButtonClick(object parameter)
        {
            // 处理按钮点击逻辑
            MessageBox.Show("Button clicked: " + (parameter ?? ""));
        }

        private void GlobalModule_PropertyChangedEvent(Luster.TaskFlow.Common.Module.IModule module, string propName, object src, object newV)
        {
            if (propName == PDCANAME)
            {
                if (_pdcaAttr == null)
                {
                    _pdcaAttr = module.Parameters[propName];
                }

                if (bool.TryParse(newV?.ToString(), out var isEnabled))
                {
                    PDCAEnabled = isEnabled;
                }
            }

            if (propName == SFCNAME)
            {
                if (_sfcAttr == null)
                {
                    _sfcAttr = module.Parameters[propName];
                }

                if (bool.TryParse(newV?.ToString(), out var isEnabled))
                {
                    SFCEnabled = isEnabled;
                }
            }

            if (propName == CPKNAME)
            {
                if (_cpkAttr == null)
                {
                    _cpkAttr = module.Parameters[propName];
                }

                if (bool.TryParse(newV?.ToString(), out var isEnabled))
                {
                    CPKEnabled = isEnabled;
                }
            }

            if (propName == TRAYNAME)
            {
                if (_trayAttr == null)
                {
                    _trayAttr = module.Parameters[propName];
                    TrayVisible = true;
                }
                else
                {
                    bool isOpen = false;
                    if (bool.TryParse(newV?.ToString(), out isOpen))
                    {

                    }

                    TrayEnabled = isOpen;

                    TrayText = isOpen ? "带Tray跑料" : "不带Tray跑料";
                }
            }

           
        }

        /// <summary>
        /// 是否启用
        /// </summary>
        private string _trayText = "带Tray跑料";
        public string TrayText
        {
            get { return _trayText; }
            set
            {
                SetProperty(ref _trayText, value);
            }
        }

        /// <summary>
        /// PDCA是否启用
        /// </summary>
        private bool _pdcaEnabled;
        public bool PDCAEnabled
        {
            get { return _pdcaEnabled; }
            set
            {
                SetProperty(ref _pdcaEnabled, value);
            }
        }

        /// <summary>
        /// PDCA是否启用
        /// </summary>
        private bool _sfcEnabled;
        public bool SFCEnabled
        {
            get { return _sfcEnabled; }
            set
            {
                SetProperty(ref _sfcEnabled, value);
            }
        }

        /// <summary>
        /// PDCA是否启用
        /// </summary>
        private bool _cpkEnabled;
        public bool CPKEnabled
        {
            get { return _cpkEnabled; }
            set
            {
                SetProperty(ref _cpkEnabled, value);
            }
        }

        /// <summary>
        /// 错误详情
        /// </summary>
        private ErrorDetail _errorDetail;
        public ErrorDetail ErrorDetail
        {
            get { return _errorDetail; }
            set
            {
                SetProperty(ref _errorDetail, value);
            }
        }

        /// <summary>
        /// 是否手动模式
        /// </summary>
        private bool _isAutoMode = false;
        public bool IsAutoMode
        {
            get { return _isAutoMode; }
            set
            {
                SetProperty(ref _isAutoMode, value);
            }
        }


        private static SubscriptionToken _recipeAddedToken = null;
        private static SubscriptionToken _recipeRemovedToken = null;
        private static SubscriptionToken _recipeSkipedToken = null;
        private static SubscriptionToken _recipePastedToken = null;
        private static SubscriptionToken _recipeMovedToken = null;

        private static SubscriptionToken _recipeOpenToken = null;
        private static SubscriptionToken _alarmToken = null;
        private static SubscriptionToken _operationToken = null;

        protected override void RegisterEvent(IEventAggregator bus)
        {
            base.RegisterEvent(bus);

            Unsubscribe<RecipeOpenEvent>(_recipeOpenToken);
            _recipeOpenToken = bus.GetEvent<RecipeOpenEvent>().Subscribe(r =>
            {
                InitGlobal();
            });

            // 报警
            Unsubscribe<AlarmEvent>(_alarmToken);
            _alarmToken = bus.GetEvent<AlarmEvent>().Subscribe(a =>
            {
                if (a == null)
                {
                    ErrorDetail = null;
                }
                else
                {
                    ErrorDetail = _errorManager.GetErrorDetail(a, "Hive");
                }
            });

            // 机台空闲和停止状态，才能开门
            Unsubscribe<OperationEvent>(_operationToken);
            _operationToken = bus.GetEvent<OperationEvent>().Subscribe(s =>
            {
                CanOpenDoor = s.Dst == EngineStatus.Stop || s.Dst == EngineStatus.Idle || s.Dst == EngineStatus.Ready || s.Dst == EngineStatus.Pause;
            });

            // 异步更新当前工站
            Unsubscribe<ModuleAddEvent>(_recipeAddedToken);
            _recipeAddedToken = bus.GetEvent<ModuleAddEvent>().Subscribe(r =>
            {
                InitMotionModules();
            });

            Unsubscribe<ModuleMovedEvent>(_recipeMovedToken);
            _recipeMovedToken = bus.GetEvent<ModuleMovedEvent>().Subscribe(r =>
            {
                InitMotionModules();
            });

            Unsubscribe<ModuleRemoveEvent>(_recipeRemovedToken);
            _recipeRemovedToken = bus.GetEvent<ModuleRemoveEvent>().Subscribe(r =>
            {
                InitMotionModules();
            });

            Unsubscribe<ModuleSkipEvent>(_recipeSkipedToken);
            _recipeSkipedToken = bus.GetEvent<ModuleSkipEvent>().Subscribe(r =>
            {
                InitMotionModules();
            });

            Unsubscribe<ModulePastedEvent>(_recipePastedToken);
            _recipePastedToken = bus.GetEvent<ModulePastedEvent>().Subscribe(r =>
            {
                InitMotionModules();
            });
        }

        /// <summary>
        /// HIVE 保修命令
        /// </summary>
        private DelegateCommand _hiveRepairCommand;
        public DelegateCommand HiveRepairCommand => _hiveRepairCommand ?? (_hiveRepairCommand = new DelegateCommand(() =>
        {

            //叫修
            commonBus.EventBus.GetEvent<HiveRepairEvent>().Publish(true);
            //蜂鸣1S/10S
            LogTool.Warn("蜂鸣器开始，每10s持续1s");
            _mController.SetBuzzer(true, 1000, 10000);

            _dialogService.ShowHive1Dialog("设备保养", r =>
            {

            });

        }));


        public string Name => "FXContent";

        public string RegionName { get; set; } = "ProFXContent";


        private Brush hiveTestColor;
        public Brush HiveTestColor
        {
            get { return hiveTestColor; }
            set
            {
                SetProperty(ref hiveTestColor, value);
            }
        }


        /// <summary>
        /// HIVE 认证
        /// </summary>
        private DelegateCommand _hiveSetUpCommand;
        public DelegateCommand HiveSetUpCommand => _hiveSetUpCommand ?? (_hiveSetUpCommand = new DelegateCommand(() =>
        {
            Task.Run(() =>
            {
                string errmsg;
                HiveTestColor = Brushes.LightBlue;
                errmsg = ""; // hiveAPI.SetupQualification();
                if (errmsg == "error")
                {
                    HiveTestColor = Brushes.Red;
                }
                else
                {
                    HiveTestColor = Brushes.Green;
                }
            });
        }));

        #region 开门命令
        /// <summary>
        /// PDCA是否启用
        /// </summary>
        private bool _canOpenDoor = true;
        public bool CanOpenDoor
        {
            get { return _canOpenDoor; }
            set
            {
                SetProperty(ref _canOpenDoor, value);
            }
        }

        /// <summary>
        /// 开门命令
        /// </summary>
        private DelegateCommand<object> _openDoorCommand;
        public DelegateCommand<object> OpenDoorCommand => _openDoorCommand ?? (_openDoorCommand = new DelegateCommand<object>((opened) =>
        {
            if (bool.TryParse(opened?.ToString(), out var isOpen))
            {
                // 一键开启安全门
                var doorList = _deviceEngine.GetVDevices<VIO>().Where(u => u.Behavior == IOBehavior.Output && u.Name.Contains("锁")).ToList();
                foreach (var item in doorList)
                {
                    item.SetDigital(!isOpen);
                }
            }

        }, (isOpened) => CanOpenDoor).ObservesProperty(() => CanOpenDoor));
        #endregion

        #region 带空TrayEmpty命令
        /// <summary>
        /// 带空Tray盘运控
        /// </summary>
        private bool _trayVisible = false;
        public bool TrayVisible
        {
            get { return _trayVisible; }
            set
            {
                SetProperty(ref _trayVisible, value);
            }
        }

        /// <summary>
        /// 带空Tray盘运控
        /// </summary>
        private bool _TrayEnabled = true;
        public bool TrayEnabled
        {
            get { return _TrayEnabled; }
            set
            {
                SetProperty(ref _TrayEnabled, value);
            }
        }

        /// <summary>
        /// 空盘是否启用
        /// </summary>
        private bool _canTrayEmpty = true;
        public bool CanTrayEmpty
        {
            get { return _canTrayEmpty; }
            set
            {
                SetProperty(ref _canTrayEmpty, value);
            }
        }

        /// <summary>
        /// 开门命令
        /// </summary>
        private DelegateCommand<object> _emptyTrayCommand;
        public DelegateCommand<object> EmptyTrayCommand => _emptyTrayCommand ?? (_emptyTrayCommand = new DelegateCommand<object>((opened) =>
        {
            if (bool.TryParse(opened?.ToString(), out var isOpen))
            {
                _trayAttr.Value = isOpen;
            }

        }, (opened) => CanTrayEmpty).ObservesProperty(() => CanTrayEmpty));
        #endregion

    }
}
