using HandyControl.Data.Enum;
using Luster.Common.Assets;
using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Extensions;
using Luster.Control.Wpf.Motion;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.CommonUI.Models;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Real;
using Luster.Motion.DataStruct.VDevice;
using Luster.Motion.DataStruct.Virtual;
using Luster.Motion.EditorUI;
using Luster.Motion.EditorUI.Extensions;
using Luster.Motion.EditorUI.Models;
using Luster.Motion.SubSystem.Models;
using Luster.Motion.TaskFlow.Engine;
using Luster.Motion.TaskFlow.Engine.Models;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using Luster.WindowsAPICodePack.Dialogs;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace Luster.Motion.SubSystem.ViewModel
{
    public class SoftConfigureContentVM : MotionPageVM
    {

        /// <summary>
        /// 对话框服务
        /// </summary>
        private IDialogService _dialogService;

        private IMotionController _mController;

        // 设备引擎
        private IDeviceEngine _deviceEngine;

        private Dispatcher _dispatcher;

        private IMotionEngine _engine;



        public SoftConfigureContentVM(ICommonBus commonBus, IDialogService dialogService, IMotionController mController, IDeviceEngine deviceEngine,
            Dispatcher dispatcher, IMotionEngine engine) : base(commonBus)
        {
            _dialogService = dialogService;
            _mController = mController;
            _deviceEngine = deviceEngine;
            _dispatcher = dispatcher;
            _engine = engine;

            deviceEngine.ModeChangedEvent -= DeviceEngine_ModeChangedEvent;
            deviceEngine.ModeChangedEvent += DeviceEngine_ModeChangedEvent;
            RegisterMuliLang(nameof(FileTypeList));
            RegisterMuliLang(nameof(DoorTypeList));
            InitData(commonBus.CurrentRecipe);
            BuildBtns(commonBus.CurrentRecipe);
            BuildClassList();
            BuildLightCurtainList();
            BuildRunMode(_deviceEngine.DeviceMode);
        }

        /// <summary>
        /// 配方切换刷新配置显示
        /// </summary>
        /// <param name="bus"></param>
        protected override void RegisterEvent(IEventAggregator bus)
        {
            base.RegisterEvent(bus);

            bus.GetEvent<RecipeOpenEvent>().Subscribe(r =>
            {
                InitData(r);
                BuildClassList();
                BuildLightCurtainList();
                BuildBtns(r);
                BuildRunMode(_deviceEngine.DeviceMode);
            });
        }


        #region  属性
        /// <summary>
        /// 三色灯
        /// </summary>
        private ObservableCollection<DeviceConfigModel> _tricolorLamp;
        public ObservableCollection<DeviceConfigModel> TricolorLamp
        {
            get { return _tricolorLamp; }
            set { SetProperty(ref _tricolorLamp, value); }
        }

        /// <summary>
        /// 按钮集合
        /// </summary>
        private ObservableCollection<DeviceConfigModel> _btnCollections;
        public ObservableCollection<DeviceConfigModel> BtnCollections
        {
            get { return _btnCollections; }
            set { SetProperty(ref _btnCollections, value); }
        }

        /// <summary>
        /// 光幕集合
        /// </summary>
        private ObservableCollection<LightCurtainModel> _listLightCurtains;
        public ObservableCollection<LightCurtainModel> ListLightCurtains
        {
            get { return _listLightCurtains; }
            set { SetProperty(ref _listLightCurtains, value); }
        }

        /// <summary>
        /// 用户列表
        /// </summary>
        private ObservableCollection<ClassModel> _classList;
        public ObservableCollection<ClassModel> ClassList
        {
            get { return _classList; }
            set { SetProperty(ref _classList, value); }
        }

        /// <summary>
        /// 运动模式列表
        /// </summary>
        private ObservableCollection<RunModeModel> _listMode;
        public ObservableCollection<RunModeModel> ListMode
        {
            get { return _listMode; }
            set
            {
                SetProperty(ref _listMode, value);
            }
        }

        /// <summary>
        /// 开关门配置列表
        /// </summary>
        private ObservableCollection<DoorLockModel> _listOpenCloseDoor;
        public ObservableCollection<DoorLockModel> ListOpenCloseDoor
        {
            get { return _listOpenCloseDoor; }
            set
            {
                SetProperty(ref _listOpenCloseDoor, value);
                IsSave = true;
            }
        }

        /// <summary>
        /// 启动，暂停，停止状态IO配置
        /// </summary>
        private ObservableCollection<SysOperateIOModel> _listOperateIOSet;
        public ObservableCollection<SysOperateIOModel> ListOperateIOSet
        {
            get { return _listOperateIOSet; }
            set
            {
                SetProperty(ref _listOperateIOSet, value);
                IsSave = true;
            }
        }
        /// <summary>
        /// 门锁类型
        /// </summary>
        private ObservableCollection<KeyValue> doortypelist;
        public ObservableCollection<KeyValue> DoorTypeList
        {
            get { return doortypelist; }
            set { SetProperty(ref doortypelist, value); }
        }

        /// <summary>
        /// 稼动状态
        /// </summary>
        private ObservableCollection<KeyValue> _sysOperateList;
        public ObservableCollection<KeyValue> SysOperateList
        {
            get { return _sysOperateList; }
            set { SetProperty(ref _sysOperateList, value); }
        }

        /// <summary>
        /// 运动模式列表
        /// </summary>
        private string _selectIO;
        public string SelectIO
        {
            get { return _selectIO; }
            set { SetProperty(ref _selectIO, value); }
        }

        /// <summary>
        /// 开关门颜色
        /// </summary>
        private SolidColorBrush _doorColor;
        public SolidColorBrush DoorColor
        {
            get { return _doorColor; }
            set { SetProperty(ref _doorColor, value); }
        }

        /// <summary>
        /// CT
        /// </summary>
        private int _ct;
        public int CT
        {
            get { return _ct; }
            set
            {
                SetProperty(ref _ct, value);
                _mController.SysConfig.CT = value;
            }
        }

        /// <summary>
        /// 是否开启CT统计
        /// </summary>
        private bool _issEnableCTStatistics;
        public bool IsEnableCTStatistics
        {
            get { return _issEnableCTStatistics; }
            set
            {
                SetProperty(ref _issEnableCTStatistics, value);
                _mController.SysConfig.IsEnableCTStatistics = value;
                IsSave = true;
            }
        }

        /// <summary>
        /// 是否开启CT统计
        /// </summary>
        private bool _isEnableAutoSetCT;
        public bool IsEnableAutoSetCT
        {
            get { return _isEnableAutoSetCT; }
            set
            {
                SetProperty(ref _isEnableAutoSetCT, value);
                _mController.MotionEngine.DeviceEngine.AutoSetMotionModuleCT = value;//不保存到本地，临时状态
            }
        }

        /// <summary>
        /// 是否开启CT统计
        /// </summary>
        private bool _isDoorLock;
        public bool IsDoorLock
        {
            get { return _isDoorLock; }
            set
            {
                SetProperty(ref _isDoorLock, value);
                _mController.SysConfig.IsDoorLock = value;
                IsSave = true;
            }
        }

        /// <summary>
        /// 用户操作是否弹窗对话提示框
        /// </summary>
        private bool _isShowOperationTip;
        public bool IsShowOperationTip
        {
            get { return _isShowOperationTip; }
            set
            {
                SetProperty(ref _isShowOperationTip, value);
                _mController.SysConfig.IsShowOperationTip = value;
                IsSave = true;
            }
        }

        /// <summary>
        /// 稼动率刷新频率，单位s
        /// </summary>
        private double _pvRefreshInterval;
        public double PvRefreshInterval
        {
            get { return _pvRefreshInterval; }
            set
            {
                SetProperty(ref _pvRefreshInterval, value);
                _mController.SysConfig.PvRefreshInterval = value;
            }
        }

        private double _highLevelTime;
        public double HighLevelTime
        {
            get { return _highLevelTime; }
            set
            {
                SetProperty(ref _highLevelTime, value);
                _mController.SysConfig.HighLevelTime = value;
            }
        }
        /// <summary>
        /// 抛料固定耗时，单位：s
        /// </summary>
        private double _tossingTime;
        public double TossingTime
        {
            get { return _tossingTime; }
            set
            {
                SetProperty(ref _tossingTime, value);
                _mController.SysConfig.TossingTime = value;
            }
        }

        /// <summary>
        /// CT 文件存储类型
        /// </summary>
        private CtFileType _fileType;

        public CtFileType FileType
        {
            get => _fileType;
            set
            {
                SetProperty(ref _fileType, value);
                _mController.SysConfig.CtFileType = FileType;
                IsSave = true;
            }
        }

        /// <summary>
        /// 模式行数
        /// </summary>
        private int _rowCount = 1;
        public int RowCount
        {
            get { return _rowCount; }
            set
            {
                SetProperty(ref _rowCount, value);
            }
        }

        /// <summary>
        /// 模式列表
        /// </summary>
        public List<KeyValue> FileTypeList
        {
            get; set;
        }

        /// <summary>
        /// 是否启用蜂鸣器
        /// </summary>
        private bool _issEnableBuzzer;
        public bool IsEnableBuzzer
        {
            get { return _issEnableBuzzer; }
            set
            {
                SetProperty(ref _issEnableBuzzer, value);
                _mController.SysConfig.IsEnableBuzzer = value;
                IsSave = true;
            }
        }

        /// <summary>
        /// 是否启用光栅
        /// </summary>
        private bool _issEnableLightCurtain;
        public bool IsEnableLightCurtain
        {
            get { return _issEnableLightCurtain; }
            set
            {
                SetProperty(ref _issEnableLightCurtain, value);
                _mController.SysConfig.EnableLightCurtain = value;
                IsSave = true;
            }
        }

        /// <summary>
        /// 是否启用安全门
        /// </summary>
        private bool _issEnableSafetyDoor;
        public bool IsEnableSaftyDoor
        {
            get { return _issEnableSafetyDoor; }
            set
            {
                SetProperty(ref _issEnableSafetyDoor, value);
                _mController.SysConfig.EnableSaftyDoor = value;
                IsSave = true;
            }
        }

        /// <summary>
        /// 班别
        /// </summary>
        private bool _firstClass;
        public bool FirstClass
        {
            get { return _firstClass; }
            set { SetProperty(ref _firstClass, value); }
        }


        /// <summary>
        /// 是否可编辑
        /// </summary>
        private bool _isEdit;
        public bool IsEdit
        {
            get { return _isEdit; }
            set { SetProperty(ref _isEdit, value); }
        }

        /// <summary>
        /// 是否保存
        /// </summary>
        private bool _isSave = false;
        public bool IsSave
        {
            get { return _isSave; }
            set
            {
                SetProperty(ref _isSave, value);
                commonBus.IsNeedSave = value;
            }
        }

        /// <summary>
        /// EditEnable
        /// </summary>
        private bool _editEnable = true;
        public bool EditEnable
        {
            get { return _editEnable; }
            set
            {
                SetProperty(ref _editEnable, value);
            }
        }

        /// <summary>
        /// ListenEnable
        /// </summary>
        private bool _listenEnable = false;
        public bool ListenEnable
        {
            get { return _listenEnable; }
            set
            {
                SetProperty(ref _listenEnable, value);
            }
        }

        /// <summary>
        /// 离线颜色
        /// </summary>
        private SolidColorBrush _offLineColor;
        public SolidColorBrush OffLineColor
        {
            get { return _offLineColor; }
            set
            {
                SetProperty(ref _offLineColor, value);
            }
        }

        /// <summary>
        /// 在线颜色
        /// </summary>
        private SolidColorBrush _onLineColor;
        public SolidColorBrush OnLineColor
        {
            get { return _onLineColor; }
            set
            {
                SetProperty(ref _onLineColor, value);
            }
        }

        /// <summary>
        /// 空跑颜色
        /// </summary>
        private SolidColorBrush _emptyRunColor;
        public SolidColorBrush EmptyRunColor
        {
            get { return _emptyRunColor; }
            set
            {
                SetProperty(ref _emptyRunColor, value);
            }
        }

        /// <summary>
        /// 变量列表
        /// </summary>
        private List<string> _varDatas;
        public List<string> VarDatas
        {
            get { return _varDatas; }
            set { SetProperty(ref _varDatas, value); }
        }

        #endregion

        /// <summary>
        /// 获取用户列表
        /// </summary>
        private void BuildClassList()
        {
            var classes = _mController.SysConfig.ClassModels;
            if (classes != null && classes.Count > 0)
            {
                var classModels = new List<ClassModel>();
                foreach (var model in classes)
                {
                    classModels.Add(new ClassModel()
                    {
                        ClassType = model.ClassName,
                        StartTime = model.StartTime,
                        EndTime = model.EndTime,
                        FirstClass = model.FirstClass,
                    });
                }
                ClassList = new ObservableCollection<ClassModel>();
                ClassList.AddRange(classModels);
            }
        }

        /// <summary>
        /// 获取光幕列表
        /// </summary>
        private void BuildLightCurtainList()
        {
            var lightcurtains = _mController.SysConfig.LightCurtainList;
            if (lightcurtains != null && lightcurtains.Count > 0)
            {
                ListLightCurtains = new ObservableCollection<LightCurtainModel>();
                foreach (var model in lightcurtains)
                {
                    ListLightCurtains.Add(new LightCurtainModel()
                    {
                        Device = model.Device,
                        DeviceName = model.DeviceName,
                        Globalvar = model.Globalvar
                    });
                }
            }
        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        private void InitData(Recipe r)
        {
            IsEdit = false;
            if (_mController.SysConfig == null && commonBus.ProjInfo != null && !string.IsNullOrEmpty(commonBus.ProjInfo.ProjPath))
            {
                string sysConfig = Path.Combine(r.ProjInfo.ProjPath, "Config", "SystemConfig.xml");
                _mController.SysConfig = new TaskFlow.Engine.Models.SystemConfig();
                _mController.SysConfig.LoadSysConfig(sysConfig);//加载配置
            }

            CT = _mController.SysConfig.CT == 0 ? 0 : _mController.SysConfig.CT;
            FileType = _mController.SysConfig.CtFileType;
            IsEnableCTStatistics = _mController.SysConfig.IsEnableCTStatistics;
            IsDoorLock = _mController.SysConfig.IsDoorLock;
            IsEnableBuzzer = _mController.SysConfig.IsEnableBuzzer;
            IsEnableLightCurtain = _mController.SysConfig.EnableLightCurtain;
            IsEnableSaftyDoor = _mController.SysConfig.EnableSaftyDoor;
            IsShowOperationTip = _mController.SysConfig.IsShowOperationTip;
            PvRefreshInterval = _mController.SysConfig.PvRefreshInterval == 0 ? 1 : _mController.SysConfig.PvRefreshInterval;
            TossingTime = _mController.SysConfig.TossingTime == 0 ? 1 : _mController.SysConfig.TossingTime;
            HighLevelTime = _mController.SysConfig.HighLevelTime;

            ListMode = new ObservableCollection<RunModeModel>();
            if (_mController.SysConfig.ListRunMode != null)
            {
                _mController.SysConfig.ListRunMode.ForEach(m => ListMode.Add(m));
                //ListMode = _mController.SysConfig.ListRunMode;
                RowCount = ListMode.Count / 4 + (ListMode.Count % 4 == 0 ? 0 : 1);
            }
            commonBus.IsNeedSave = false;

            FileTypeList = typeof(CtFileType).EnumToDataSource();
            var doorList = typeof(DoorType).EnumToDataSource();
            DoorTypeList = new ObservableCollection<KeyValue>();
            if (doorList != null)
            {
                doorList.ForEach(u => DoorTypeList.Add(u));
            }

            var operateList = typeof(SystemOperation).EnumToDataSource();
            SysOperateList = new ObservableCollection<KeyValue>();
            if (operateList != null)
            {
                foreach (var item in operateList)
                {
                    if (item.Desc == SystemOperation.Start.GetDescription() || item.Desc == SystemOperation.Pause.GetDescription() || item.Desc == SystemOperation.Stop.GetDescription())
                    {
                        SysOperateList.Add(item);
                    }
                }
            }

            ListOpenCloseDoor = new ObservableCollection<DoorLockModel>();
            if (_mController.SysConfig.ListOpenCloseDoor != null)
            {
                _mController.SysConfig.ListOpenCloseDoor.ForEach(u => ListOpenCloseDoor.Add(u));
                foreach (var item in ListOpenCloseDoor)
                {
                    item.GetDescName();
                }
            }

            ListOperateIOSet = new ObservableCollection<SysOperateIOModel>();
            if (_mController.SysConfig.ListSysOperateIO != null)
            {
                _mController.SysConfig.ListSysOperateIO.ForEach(u => ListOperateIOSet.Add(u));
                foreach (var item in ListOperateIOSet)
                {
                    item.GetDescName();
                }
            }


        }

        private void BuildModes()
        {

        }


        /// <summary>
        /// 获取三色灯，按钮列表
        /// </summary>
        private void BuildBtns(Recipe r)
        {
            if (r != null && !File.Exists(r.ProjInfo.ProjPath))
            {
                string sysConfig = Path.Combine(r.ProjInfo.ProjPath, "Config", "SystemConfig.xml");
                DeviceConfigModel.SysConfingPath = sysConfig;

                _dispatcher.Invoke(() =>
                {
                    BtnCollections = new ObservableCollection<DeviceConfigModel>();
                    BtnCollections.AddRange(DeviceConfigModel.BtnCollections);

                    TricolorLamp = new ObservableCollection<DeviceConfigModel>();
                    TricolorLamp.AddRange(DeviceConfigModel.TricolorLamp);
                });

            }
        }


        /// <summary>
        /// 设置运行模式
        /// </summary>
        private void BuildRunMode(DeviceMode obj)
        {
            _dispatcher.Invoke(() =>
            {
                if (obj == DeviceMode.Real)
                {
                    OnLineColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4FCB03"));
                    OffLineColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d7d8d9"));
                    EmptyRunColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d7d8d9"));
                }
                else if (obj == DeviceMode.Empty)
                {
                    OnLineColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d7d8d9"));
                    OffLineColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d7d8d9"));
                    EmptyRunColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4FCB03"));
                }
                else
                {
                    OnLineColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d7d8d9"));
                    OffLineColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4FCB03"));
                    EmptyRunColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d7d8d9"));
                }
            });

        }

        /// <summary>
        /// 运行模式切换事件
        /// </summary>
        /// <param name="obj"></param>
        private void DeviceEngine_ModeChangedEvent(DeviceMode obj)
        {
            BuildRunMode(obj);
        }

        /// <summary>
        /// 添加班别
        /// </summary>
        private DelegateCommand _addClassCommand;
        public DelegateCommand AddClassCommand => _addClassCommand ?? (_addClassCommand = new DelegateCommand(() =>
        {
            ClassModel model = null;
            _dialogService.ShowAddClass(model, (r) =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    ClassModel classModel = new ClassModel();
                    classModel.FirstClass = false;
                    if (r.Parameters.TryGetValue("ClassName", out string className))
                    {
                        classModel.ClassType = className;
                    }
                    if (r.Parameters.TryGetValue("StartTime", out DateTime startTime))
                    {
                        classModel.StartTime = startTime;
                    }
                    if (r.Parameters.TryGetValue("EndTime", out DateTime endTime))
                    {
                        classModel.EndTime = endTime;
                    }
                    if (ClassList == null)
                    {
                        ClassList = new ObservableCollection<ClassModel>();
                    }
                    ClassList.Add(classModel);
                    ProcessClass();
                }
            });
        }));

        /// <summary>
        /// 添加光幕
        /// </summary>
        private DelegateCommand _addLightCurtainCommand;
        public DelegateCommand AddLightCurtainCommand => _addLightCurtainCommand ?? (_addLightCurtainCommand = new DelegateCommand(() =>
        {
            _dialogService.ShowLightCurtain((r) =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    LightCurtainModel lightModel = new LightCurtainModel();
                    if (r.Parameters.TryGetValue("VDevice", out VDevice device))
                    {
                        lightModel.Device = device;
                        lightModel.DeviceName = device.Name;
                    }
                    if (r.Parameters.TryGetValue("VarGlobal", out string global))
                    {
                        lightModel.Globalvar = global;
                    }
                    if (r.Parameters.TryGetValue("GlobalKey", out string globalkey))
                    {
                        lightModel.GlobalKey = globalkey;
                    }
                    if (ListLightCurtains == null)
                    {
                        ListLightCurtains = new ObservableCollection<LightCurtainModel>();
                    }
                    ListLightCurtains.Add(lightModel);
                    ProcessLightCurtain();
                }
            });
        }));

        /// <summary>
        /// 编辑操作按钮
        /// </summary>
        private DelegateCommand<object> _editOperateCommand;
        public DelegateCommand<object> EditOPerateCommand => _editOperateCommand ?? (_editOperateCommand = new DelegateCommand<object>((o) =>
        {
            if (IsEdit)
            {
                DeviceConfigModel model = (DeviceConfigModel)o;
                ParameterAttribute args = new ParameterAttribute(Luster.Motion.Assests.Langs.LangProvider.GetLang("Device"), 0);
                args.EditorType = typeof(VIO);
                _dialogService.ShowDeviceConfig(args, "", r =>
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        if (r.Parameters.TryGetValue("Device", out VIO vDevice))
                        {
                            model.SelectDevice = vDevice.Name;
                            SetSelectDevice(model, vDevice);
                        }
                    }
                    if (r.Result == ButtonResult.Cancel)
                    {
                        CancelSelectDevice(model);
                    }
                    commonBus.IsNeedSave = true;
                });
            }
        }));



        /// <summary>
        /// 选中设备
        /// </summary>
        /// <param name="model"></param>
        /// <param name="vDevice"></param>
        private void SetSelectDevice(DeviceConfigModel model, VIO vDevice)
        {
            model.BtnColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4FCB03"));
            model.IsNameVisible = Visibility.Collapsed;
            model.IsDeviceVisible = Visibility.Visible;

            switch (model.Name)
            {
                case "Start":
                    _mController.SysConfig.StartButton = new VDevice() { DeviceID = vDevice.ID, Name = vDevice.Name };
                    break;
                case "Pause":
                    _mController.SysConfig.PauseButton = new VDevice() { DeviceID = vDevice.ID, Name = vDevice.Name };
                    break;
                case "Recoverey":
                    _mController.SysConfig.RecoveryButton = new VDevice() { DeviceID = vDevice.ID, Name = vDevice.Name };
                    break;
                case "Stop":
                    _mController.SysConfig.StopButton = new VDevice() { DeviceID = vDevice.ID, Name = vDevice.Name };
                    break;
                case "EmergencyStop":
                    _mController.SysConfig.EmergencyButton = new VDevice() { DeviceID = vDevice.ID, Name = vDevice.Name };
                    break;
                case "Ignore":
                    _mController.SysConfig.IgnoreButton = new VDevice() { DeviceID = vDevice.ID, Name = vDevice.Name };
                    break;
                case "Retry":
                    _mController.SysConfig.RetryButton = new VDevice() { DeviceID = vDevice.ID, Name = vDevice.Name };
                    break;
                case "ManualSwitch":
                    _mController.SysConfig.ManualSwitchButton = new VDevice() { DeviceID = vDevice.ID, Name = vDevice.Name };
                    break;
                case "RedLamp":
                    _mController.SysConfig.LightRed = new VDevice() { DeviceID = vDevice.ID, Name = vDevice.Name };
                    break;
                case "GreenLamp":
                    _mController.SysConfig.LightGreen = new VDevice() { DeviceID = vDevice.ID, Name = vDevice.Name };
                    break;
                case "YellowLamp":
                    _mController.SysConfig.LightYellow = new VDevice() { DeviceID = vDevice.ID, Name = vDevice.Name };
                    break;
                case "Buzzer":
                    _mController.SysConfig.Buzzer = new VDevice() { DeviceID = vDevice.ID, Name = vDevice.Name };
                    break;
                case "StartLamp":
                    _mController.SysConfig.LightStart = new VDevice() { DeviceID = vDevice.ID, Name = vDevice.Name };
                    break;
                case "PauseLamp":
                    _mController.SysConfig.LightPause = new VDevice() { DeviceID = vDevice.ID, Name = vDevice.Name };
                    break;
                case "RecoveryLamp":
                    _mController.SysConfig.LightRecovery = new VDevice() { DeviceID = vDevice.ID, Name = vDevice.Name };
                    break;
                case "StopLamp":
                    _mController.SysConfig.LightStop = new VDevice() { DeviceID = vDevice.ID, Name = vDevice.Name };
                    break;

                case "SmokeAlarmDevice":
                    _mController.SysConfig.SmokeAlarmDevice = new VDevice() { DeviceID = vDevice.ID, Name = vDevice.Name };
                    break;

            }
        }

        /// <summary>
        /// 取消选中设备
        /// </summary>
        /// <param name="model"></param>
        private void CancelSelectDevice(DeviceConfigModel model)
        {
            //model.CancelSelectBtnDevice(model);
            model.SelectDevice = "";
            model.BtnColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d7d8d9"));
            model.IsNameVisible = Visibility.Visible;
            model.IsDeviceVisible = Visibility.Collapsed;
            switch (model.Name)
            {
                case "Start":
                    _mController.SysConfig.StartButton = null;
                    break;
                case "Pause":
                    _mController.SysConfig.PauseButton = null;
                    break;
                case "Recoverey":
                    _mController.SysConfig.RecoveryButton = null;
                    break;
                case "Stop":
                    _mController.SysConfig.StopButton = null;
                    break;
                case "EmergencyStop":
                    _mController.SysConfig.EmergencyButton = null;
                    break;
                case "Ingore":
                    _mController.SysConfig.IgnoreButton = null;
                    break;
                case "Retry":
                    _mController.SysConfig.RetryButton = null;
                    break;
                case "ManualSwitch":
                    _mController.SysConfig.ManualSwitchButton = null;
                    break;
                case "RedLamp":
                    _mController.SysConfig.LightRed = null;
                    break;
                case "GreenLamp":
                    _mController.SysConfig.LightGreen = null;
                    break;
                case "YellowLamp":
                    _mController.SysConfig.LightYellow = null;
                    break;
                case "Buzzer":
                    _mController.SysConfig.Buzzer = null;
                    break;
                case "StartLamp":
                    _mController.SysConfig.LightStart = null;
                    break;
                case "PauseLamp":
                    _mController.SysConfig.LightPause = null;
                    break;
                case "RecoveryLamp":
                    _mController.SysConfig.LightRecovery = null;
                    break;
                case "StopLamp":
                    _mController.SysConfig.LightStop = null;
                    break;
                case "SmokeAlarmDevice":
                    _mController.SysConfig.SmokeAlarmDevice = null;
                    break;
            }
        }


        #region 运行模式
        /// <summary>
        /// 创建模式
        /// </summary>
        private DelegateCommand _runModeCommand;
        public DelegateCommand AddRunModeCommand => _runModeCommand ?? (_runModeCommand = new DelegateCommand(() =>
        {
            _dialogService.ShowSetRunMode(null, r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    if (r.Parameters.TryGetValue("RunModeModel", out RunModeModel model))
                    {
                        if (model == null) return;

                        if (string.IsNullOrEmpty(model.Mode))
                        {
                            throw new FriendlyException($"模式摩擦不能为空!");
                        }

                        // 判断模式是否存在!
                        if (ListMode.Any(u => u.Mode == model.Mode))
                        {
                            throw new FriendlyException($"模式:{model.Mode}已存在!");
                        }

                        List<RunModeModel> modeList = new List<RunModeModel>();
                        foreach (var item in ListMode)
                        {
                            if (model.IsRunMode)
                            {
                                item.IsRunMode = false;
                            }

                            modeList.Add(item);
                        }

                        modeList.Add(model);
                        ListMode.Clear();
                        modeList.ForEach(m => ListMode.Add(m));
                        RowCount = ListMode.Count / 4 + (ListMode.Count % 4 == 0 ? 0 : 1);
                        //ListMode = modeList;
                        _mController.SysConfig.ListRunMode = modeList;
                    }

                    commonBus.IsNeedSave = true;
                }

            });
        }));

        /// <summary>
        /// 运动模式变量设置
        /// </summary>
        private DelegateCommand<object> _setGolbalVarCommand;
        public DelegateCommand<object> SetGlobalVarCommand => _setGolbalVarCommand ?? (_setGolbalVarCommand = new DelegateCommand<object>((o) =>
        {
            if (!IsEdit) return;

            if (o == null) return;

            var model = o as RunModeModel;
            _dialogService.ShowSetRunMode(model, r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    if (r.Parameters.TryGetValue("RunModeModel", out RunModeModel models))
                    {
                        if (model == null) return;

                        // 判断模式是否存在!
                        if (ListMode.Count(u => u.Mode == model.Mode) > 1)
                        {
                            throw new FriendlyException($"模式:{model.Mode}已存在!");
                        }

                        List<RunModeModel> modeList = new List<RunModeModel>();
                        foreach (var item in ListMode)
                        {
                            if (models.IsRunMode)
                            {
                                item.IsRunMode = item.Mode == model.Mode;
                            }

                            modeList.Add(item);
                        }
                        ListMode.Clear();
                        modeList.ForEach(m => ListMode.Add(m));
                        RowCount = ListMode.Count / 4 + (ListMode.Count % 4 == 0 ? 0 : 1);
                        //ListMode = modeList;
                        _mController.SysConfig.ListRunMode = modeList;
                    }

                    commonBus.IsNeedSave = true;
                }

            });
        }));


        /// <summary>
        /// 新建门锁
        /// </summary>
        private DelegateCommand _doorOnOffCommand;
        public DelegateCommand DoorOnOffCommand => _doorOnOffCommand ?? (_doorOnOffCommand = new DelegateCommand(() =>
        {
            var doorLockmodel = new DoorLockModel();
            _dialogService.ShowSetOpenCloseDoor(r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    if (r.Parameters.TryGetValue("SelectIOList", out List<IVirtualDevice> models))
                    {
                        if (models == null) return;
                        List<VDevice> selectList = new List<VDevice>();
                        foreach (var item in models)
                        {
                            var device = new VDevice()
                            {
                                DeviceID = item.ID,
                                Name = item.Name
                            };
                            selectList.Add(device);
                        }

                        string SelectIOName = "";
                        foreach (var item in selectList)
                        {
                            if (SelectIOName == "")
                            {
                                SelectIOName = item.Name;
                            }
                            else
                            {
                                SelectIOName = $"{SelectIOName},{item.Name}";
                            }

                        }
                        doorLockmodel.ListLockIO = selectList;

                    }
                    if (r.Parameters.TryGetValue("DoorType", out DoorType doortype))
                    {
                        doorLockmodel.DoorType = doortype;
                        doorLockmodel.GetDescName();
                    }
                    var existmodel = ListOpenCloseDoor.FirstOrDefault(u => u.DoorType == doorLockmodel.DoorType);
                    if (existmodel != null)
                    {
                        throw new FriendlyException("该类型已存在！");
                    }
                    ListOpenCloseDoor.Add(doorLockmodel);
                    _mController.SysConfig.ListOpenCloseDoor = ListOpenCloseDoor.ToList();
                    commonBus.IsNeedSave = true;
                }

            });
        }));


        /// <summary>
        /// 新建稼动状态IO设置
        /// </summary>
        private DelegateCommand _sysOperateIOCommand;
        public DelegateCommand SysOperateIOCommand => _sysOperateIOCommand ?? (_sysOperateIOCommand = new DelegateCommand(() =>
        {
            var sysOperateIOmodel = new SysOperateIOModel();
            _dialogService.ShowSysOperateIOSet(r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    if (r.Parameters.TryGetValue("SelectIOList", out List<IVirtualDevice> models))
                    {
                        if (models == null) return;
                        List<VDevice> selectList = new List<VDevice>();
                        foreach (var item in models)
                        {
                            var device = new VDevice()
                            {
                                DeviceID = item.ID,
                                Name = item.Name
                            };
                            selectList.Add(device);
                        }
                        var listIO = new List<SysSetIOModel>();
                        foreach (var item in selectList)
                        {
                            var setiomodel = new SysSetIOModel();
                            setiomodel.SetIO = item;
                            setiomodel.Status = false;
                            listIO.Add(setiomodel);
                        }

                        sysOperateIOmodel.ListIO = listIO;

                    }
                    if (r.Parameters.TryGetValue("SysOperateype", out SystemOperation sysOperateype))
                    {
                        sysOperateIOmodel.SysOperateType = sysOperateype;
                        sysOperateIOmodel.GetDescName();
                    }
                    var existmodel = ListOperateIOSet.FirstOrDefault(u => u.SysOperateType == sysOperateIOmodel.SysOperateType);
                    if (existmodel != null)
                    {
                        throw new FriendlyException("该类型已存在！");
                    }
                    ListOperateIOSet.Add(sysOperateIOmodel);
                    _mController.SysConfig.ListSysOperateIO = ListOperateIOSet.ToList();
                    commonBus.IsNeedSave = true;
                }

            });
        }));

        /// <summary>
        /// 删除门锁
        /// </summary>
        private DelegateCommand<DoorLockModel> _doordeleteCommand;
        public DelegateCommand<DoorLockModel> DoorDeleteCommand => _doordeleteCommand ?? (_doordeleteCommand = new DelegateCommand<DoorLockModel>((doorlock) =>
        {
            var deletemodel = ListOpenCloseDoor.FirstOrDefault(u => u.DoorType == doorlock.DoorType);
            if (deletemodel == null) return;
            ListOpenCloseDoor.Remove(deletemodel);
            _mController.SysConfig.ListOpenCloseDoor = ListOpenCloseDoor.ToList();
        }));

        /// <summary>
        /// 删除启动，暂停，停止配置
        /// </summary>
        private DelegateCommand<SysOperateIOModel> _sysOperateIOdeleteCommand;
        public DelegateCommand<SysOperateIOModel> SysOperateIODeleteCommand => _sysOperateIOdeleteCommand ?? (_sysOperateIOdeleteCommand = new DelegateCommand<SysOperateIOModel>((sysoperateIO) =>
        {
            var deletemodel = ListOperateIOSet.FirstOrDefault(u => u.SysOperateType == sysoperateIO.SysOperateType);
            if (deletemodel == null) return;
            ListOperateIOSet.Remove(deletemodel);
            _mController.SysConfig.ListSysOperateIO = ListOperateIOSet.ToList();
        }));


        private void DoorLockSet(bool IsAdd, DoorLockModel doorLock)
        {

        }
        /// <summary>
        /// 运行模式更改后改变系统全局变量参数
        /// </summary>
        private void ChangeGlobalVarStatus(RunModeModel model)
        {
            var gModule = GetGlobal();
            foreach (var item in gModule.Parameters)
            {
                if (item.Value.Type.Name == "Boolean")
                {
                    if (!item.Value.Visible) continue;
                    var globalmodel = new GlobalVar(item.Value);
                    foreach (var itemvar in model.SelectGlobalVar)
                    {
                        if (globalmodel.Key == itemvar.GlobalKey)
                        {
                            globalmodel.Visible = true;
                            globalmodel.IsChecked = itemvar.IsSelected;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IMotionModule GetGlobal()
        {
            var id = Luster.TaskFlow.Motion.Logic.GlobalModule.GlobalID;
            return _engine.Get(id);
        }

        /// <summary>
        /// 删除模式
        /// </summary>
        private DelegateCommand<RunModeModel> _deleteModeCommand;
        public DelegateCommand<RunModeModel> DeleteModeCommand => _deleteModeCommand ?? (_deleteModeCommand = new DelegateCommand<RunModeModel>((s) =>
        {
            var selectmode = ListMode.FirstOrDefault(m => m.Mode == s.Mode);
            if (selectmode != null)
            {
                ListMode.Remove(selectmode);
                RowCount = ListMode.Count / 4 + (ListMode.Count % 4 == 0 ? 0 : 1);
            }
            _mController.SysConfig.ListRunMode.Remove(selectmode);
            commonBus.IsNeedSave = true;
        }));


        #endregion

        private void ProcessClass()
        {
            if (ClassList != null)
            {
                _mController.SysConfig.ClassModels.Clear();
                foreach (var item in ClassList)
                {
                    var model = new ClassInfo()
                    {
                        ClassName = item.ClassType,
                        StartTime = item.StartTime,
                        EndTime = item.EndTime,
                        FirstClass = item.FirstClass
                    };
                    _mController.SysConfig.ClassModels.Add(model);
                }
                commonBus.IsNeedSave = true;
            }
        }

        private void ProcessLightCurtain()
        {
            if (ListLightCurtains != null)
            {
                if (_mController.SysConfig.LightCurtainList != null)
                {
                    _mController.SysConfig.LightCurtainList.Clear();
                }
                else
                {
                    _mController.SysConfig.LightCurtainList = new List<LightCurtainModel>();
                }
                foreach (var item in ListLightCurtains)
                {
                    var model = new LightCurtainModel()
                    {
                        DeviceName = item.DeviceName,
                        Device = item.Device,
                        Globalvar = item.Globalvar,
                        GlobalKey = item.GlobalKey
                    };
                    _mController.SysConfig.LightCurtainList.Add(model);
                }
                commonBus.IsNeedSave = true;
            }
        }

        /// <summary>
        /// 编辑班别
        /// </summary>
        private DelegateCommand<ClassModel> _editClassCommand;
        public DelegateCommand<ClassModel> EditClassCommand => _editClassCommand ?? (_editClassCommand = new DelegateCommand<ClassModel>((model) =>
        {
            _dialogService.ShowAddClass(model, (r) =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    var obj = ClassList.FirstOrDefault(x => x.ClassType == model.ClassType);
                    if (obj != null)
                    {
                        if (r.Parameters.TryGetValue("ClassName", out string className))
                        {
                            obj.ClassType = className;
                        }
                        if (r.Parameters.TryGetValue("StartTime", out DateTime startTime))
                        {
                            obj.StartTime = startTime;
                        }
                        if (r.Parameters.TryGetValue("EndTime", out DateTime endTime))
                        {
                            obj.EndTime = endTime;
                        }
                        ProcessClass();
                    }
                }
            });
        }));

        /// <summary>
        /// 删除班别
        /// </summary>
        private DelegateCommand<ClassModel> _deleteClassCommand;
        public DelegateCommand<ClassModel> DeleteClassCommand => _deleteClassCommand ?? (_deleteClassCommand = new DelegateCommand<ClassModel>((classmodel) =>
        {
            if (classmodel != null)
            {
                if (ClassList != null)
                {
                    var model = _mController.SysConfig.ClassModels.Where(x => x.ClassName == classmodel.ClassType).FirstOrDefault();
                    if (model != null)
                    {
                        _mController.SysConfig.ClassModels.Remove(model);
                        commonBus.IsNeedSave = true;
                    }
                    ClassList.Remove(classmodel);
                    ProcessClass();
                }
            }
        }));

        /// <summary>
        /// 删除光幕
        /// </summary>
        private DelegateCommand<LightCurtainModel> _deleteLightCurtainCommand;
        public DelegateCommand<LightCurtainModel> DeleteLightCurtainCommand => _deleteLightCurtainCommand ?? (_deleteLightCurtainCommand = new DelegateCommand<LightCurtainModel>((lightcurtainmodel) =>
        {
            if (lightcurtainmodel != null)
            {
                if (ListLightCurtains != null)
                {
                    var model = _mController.SysConfig.LightCurtainList.Where(x => x.DeviceName == lightcurtainmodel.DeviceName).FirstOrDefault();
                    if (model != null)
                    {
                        _mController.SysConfig.LightCurtainList.Remove(model);
                        commonBus.IsNeedSave = true;
                    }
                    ListLightCurtains.Remove(lightcurtainmodel);
                    ProcessLightCurtain();
                }
            }
        }));

        /// <summary>
        /// 首班Check
        /// </summary>
        private DelegateCommand<ClassModel> _checkCommand;
        public DelegateCommand<ClassModel> CheckCommand => _checkCommand ?? (_checkCommand = new DelegateCommand<ClassModel>((classmodel) =>
        {
            if (classmodel != null)
            {
                if (ClassList != null)
                {
                    commonBus.IsNeedSave = true;
                    foreach (var item in ClassList)
                    {
                        if (item.ClassType != classmodel.ClassType)
                        {
                            if (classmodel.FirstClass)
                            {
                                item.FirstClass = false;
                            }
                        }
                        else
                        {
                            if (classmodel.FirstClass)
                            {
                                item.FirstClass = false;
                            }
                            else
                            {
                                item.FirstClass = true;
                            }
                        }
                    }
                    ProcessClass();
                }
            }
        }));

        /// <summary>
        /// 监听
        /// </summary>
        private DelegateCommand _listenCommand;
        public DelegateCommand ListenCommand => _listenCommand ?? (_listenCommand = new DelegateCommand(() =>
        {
            IsEdit = false;
            _mController.StartButtonMontor();
            EditEnable = true;
            ListenEnable = false;

        }));

        /// <summary>
        /// 按钮是否可编辑
        /// </summary>
        private DelegateCommand _editEnableCommand;
        public DelegateCommand EditEnableCommand => _editEnableCommand ?? (_editEnableCommand = new DelegateCommand(() =>
        {
            _mController.StopButtonMontor();
            IsEdit = true;
            EditEnable = false;
            ListenEnable = true;
        }));

        /// <summary>
        /// 启用蜂鸣器
        /// </summary>
        private DelegateCommand _enableBuzzerCommand;
        public DelegateCommand EnableBuzzerCommand => _enableBuzzerCommand ?? (_enableBuzzerCommand = new DelegateCommand(() =>
        {
            _mController.SetBuzzer(true);
        }));

        /// <summary>
        /// 禁用蜂鸣器
        /// </summary>
        private DelegateCommand _disableBuzzerCommand;
        public DelegateCommand DisableBuzzerCommand => _disableBuzzerCommand ?? (_disableBuzzerCommand = new DelegateCommand(() =>
        {
            _mController.SetBuzzer(false);
        }));

        /// <summary>
        /// 启用安全门
        /// </summary>
        private DelegateCommand _enableSafetyDoorCommand;

        public DelegateCommand EnableSafetyDoorCommand => _enableSafetyDoorCommand ?? (_enableSafetyDoorCommand = new DelegateCommand(() =>
        {
            _mController.SysConfig.EnableSaftyDoor = true;
        }));

        /// <summary>
        /// 屏蔽安全门
        /// </summary>
        private DelegateCommand _disableSafetyDoorCommand;

        public DelegateCommand DisableSafetyDoorCommand => _disableSafetyDoorCommand ?? (_disableSafetyDoorCommand = new DelegateCommand(() =>
        {
            _mController.SysConfig.EnableSaftyDoor = false;
        }));

        /// <summary>
        /// 启用安全门
        /// </summary>
        private DelegateCommand _enableLightCurtainCommand;

        public DelegateCommand EnableLightCurtainCommand => _enableLightCurtainCommand ?? (_enableLightCurtainCommand = new DelegateCommand(() =>
        {
            _mController.SysConfig.EnableLightCurtain = true;
        }));

        /// <summary>
        /// 屏蔽安全门
        /// </summary>
        private DelegateCommand _disableLightCurtainCommand;

        public DelegateCommand DisableLightCurtainCommand => _disableLightCurtainCommand ?? (_disableLightCurtainCommand = new DelegateCommand(() =>
        {
            _mController.SysConfig.EnableLightCurtain = false;
        }));
    }
}
