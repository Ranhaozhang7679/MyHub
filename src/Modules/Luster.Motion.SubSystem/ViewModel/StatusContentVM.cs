#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       StatusContentVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.SubSystem.ViewModel
* 文 件 名:       StatusContentVM.cs
* 创建时间:       2022/7/2 22:27:11
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      6bf721ec-935c-4d39-a43e-4c43424304c5
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/2 22:27:11
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.DataModels;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.Enums;
using Prism.Commands;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Enums;
using System.Windows.Controls;
using Luster.SimDevice.EngineUI.Events;
using System.Windows.Threading;
using Luster.Motion.TaskFlow.Engine;
using Luster.Motion.TaskFlow.Engine.Models;
using Luster.Control.Wpf.Motion.Flow;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Luster.Motion.CommonUI.Converter;
using System.Collections.ObjectModel;
using Luster.Motion.SubSystem.Models;
using System.Reflection;
using System.IO;
using Prism.Services.Dialogs;
using Luster.Motion.EditorUI.Extensions;
using Luster.Motion.Integration.Web;
using Luster.TaskFlow.Motion.Logic;
using Luster.TaskFlow.Common.Attributes;
using static System.Windows.Forms.AxHost;

namespace Luster.Motion.SubSystem.ViewModel
{
    public class StatusContentVM : MotionVM
    {
        private IDeviceEngine _deviceEngine;

        private Dispatcher dispatcher;

        private IDialogService _dialogService;

        private IMotionController _mController;

        private readonly string PDCANAME = "Extend_PDCA启用";
        private readonly string SFCNAME = "Extend_SFC启用";
        private ParameterAttribute _pdcaAttr = null;
        private ParameterAttribute _sfcAttr = null;

        private bool PDCAEnabled;
        private bool SFCEnabled;


        public StatusContentVM(ICommonBus _commonBus,
            IDeviceEngine deviceEngine,
            Dispatcher dispatcher,
            IMotionEngine mEngine,
            IMotionController mController,
            IDialogService dialogService) : base(_commonBus)
        {
            _deviceEngine = deviceEngine;
            _mController = mController;
            _dialogService = dialogService;
            _deviceEngine.LogEvent -= DeviceOnLog;
            _deviceEngine.LogEvent += DeviceOnLog;

            _mController.NetStatusChangedEvent -= mController_NetStatusChangedEvent;
            _mController.NetStatusChangedEvent += mController_NetStatusChangedEvent;



            ModeList = typeof(DeviceMode).EnumToDataSource();
            SelectMode = _deviceEngine.DeviceMode;
            this.dispatcher = dispatcher;

            Version = Luster.Common.Tools.SoftVersionTool.GetVersion("LusterMotion.exe");
            // 构建资源对象
            Languages = new List<Lang> {
                new Lang() {
                    Text = "zh-cn",
                    Image = new BitmapImage(new Uri("pack://application:,,,/Luster.Common.Assets;component/image/flag/cn.png"))
                },
                new Lang() {
                    Text = "en",
                    Image = new BitmapImage(new Uri("pack://application:,,,/Luster.Common.Assets;component/image/flag/en.png"))
                }
            };

            CurrentLang = Languages.FirstOrDefault(u => u.Text == AppConfig.Lang);
            RegisterMuliLang(nameof(ModeList));

            InitSysItems();
            SfcBackBrush = "Gray";
            PdcaBackBrush = "Gray";
            ForeTextBrush = "Black";
            FFUBackBrush = "Gray";
        }



        private void mController_NetStatusChangedEvent(string netName, bool status, string state = "")
        {

            // 获取SFC和PDCA是否启用
            var globalModule = _mController.MotionEngine.Get(GlobalModule.GlobalID);
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

            if (netName == "SFC")
            {
                SFCEnabled = true;
                SfcBackBrush = !SFCEnabled ? "Gray" : status ? "Green" : "Red";
            }

            if (netName == "PDCA")
            {
                PDCAEnabled = true;
                PdcaBackBrush = !PDCAEnabled ? "Gray" : status ? "Green" : "Red";
            }
            //新增触发
            if (netName == "FFUCOM")
            {
                FFUBackBrush = status ? "Green" : "Red";
                FFUState = state;
                return;//不在进行下面的判断
            }
            if (netName == "FFU")
            {
                FFUBackBrush = status ? "Green" : "Red";
                FFUState = state;
            }

        }




        /// <summary>
        /// 构建集成系统
        /// </summary>
        private void InitSysItems()
        {
            SysItems = new ObservableCollection<SysStatusModel>();
            _mController.WebService.GetMESAPI().ForEach(s =>
            {
                SysItems.Add(new SysStatusModel(s as BaseMESAPI));
            });
        }


        /// <summary>
        /// 注册设备log
        /// </summary>
        /// <param name="logType"></param>
        /// <param name="logMsg"></param>
        private void DeviceOnLog(LogType logType, string logMsg)
        {
            commonBus.OnLog(new LogInfo() { LogType = logType, LogMessage = logMsg });
        }

        /// <summary>
        /// 事件注册
        /// </summary>
        /// <param name="bus"></param>
        protected override void RegisterEvent(IEventAggregator bus)
        {
            // log日志
            bus.GetEvent<LogInfoEvent>().Subscribe(logInfo =>
            {
                dispatcher.Invoke(() =>
                {
                    if (logInfo.LogType == LogType.Error)
                    {
                        Message = logInfo.LogMessage;
                        ForeBrush = "Red";
                    }
                    else if (logInfo.LogType == LogType.Info)
                    {
                        if (logInfo.LogMessage == SystemConsts.ProjectSaved)
                        {
                            Message = logInfo.LogMessage;
                            ForeBrush = "Blue";
                        }
                    }
                    else
                    {
                        Message = String.Empty;
                    }
                });
            });

            // 工程打开才能进行模式切换
            bus.GetEvent<RecipeOpenEvent>().Subscribe(r => ModeEnabled = true);

            // 模式切换注册
            bus.GetEvent<RunModeEvent>().Subscribe(mode =>
            {
                // 会额外触发该命令
                SelectMode = mode;

                // 通知
                commonBus.EventBus.GetEvent<DeviceModeEvent>().Publish(SelectMode);
            });

            // 设备状态
            bus.GetEvent<OperationEvent>().Subscribe(sys =>
            {
                dispatcher.Invoke(() =>
                {
                    ModeEnabled = sys.Dst == EngineStatus.Stop;
                });
            });
            bus.GetEvent<DeviceModeChangeEvent>().Subscribe(mode =>
            {
                // 会额外触发该命令
                SelectMode = mode;
            });



        }

        /// <summary>
        /// 当前模式
        /// </summary>
        private DeviceMode _selectMode;
        public DeviceMode SelectMode
        {
            get { return _selectMode; }
            set
            {
                SetProperty(ref _selectMode, value);
            }
        }

        /// <summary>
        /// 当前模式
        /// </summary>
        private bool _modeEnabled;
        public bool ModeEnabled
        {
            get { return _modeEnabled; }
            set
            {
                SetProperty(ref _modeEnabled, value);
            }
        }

        /// <summary>
        /// 模式列表
        /// </summary>
        private List<KeyValue> _modeList;
        public List<KeyValue> ModeList
        {
            get => _modeList; set
            {
                SetProperty(ref _modeList, value);
            }
        }

        /// <summary>
        /// 切换模式
        /// </summary>
        private DelegateCommand<object> _changeModeCommand;
        public DelegateCommand<object> ChangeModeCommand => _changeModeCommand ?? (_changeModeCommand = new DelegateCommand<object>((arg) =>
        {
            var cArgs = arg as SelectionChangedEventArgs;

            if (cArgs.AddedItems.Count > 0)
            {
                var keyVal = cArgs.AddedItems[0] as KeyValue;
                DeviceMode mode = (DeviceMode)keyVal.Value;
                if (mode == _deviceEngine.DeviceMode) return;

                commonBus.ChangeDeviceMode(mode);
                _mController.SysConfig.RunMode = mode;
            }
        }));

        private bool _isProgress;
        public bool IsProgress
        {
            get { return _isProgress; }
            set { _isProgress = value; }
        }

        /// <summary>
        /// 消息
        /// </summary>
        private string _message;
        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }


        /// <summary>
        /// 前景色
        /// </summary>
        private string _foreBrush;

        public string ForeBrush
        {
            get { return _foreBrush; }
            set { SetProperty(ref _foreBrush, value); }
        }

        /// <summary>
        /// 前景色
        /// </summary>
        private string _foreTextBrush;

        public string ForeTextBrush
        {
            get { return _foreTextBrush; }
            set { SetProperty(ref _foreTextBrush, value); }
        }


        /// <summary>
        /// sfc背景色
        /// </summary>
        private string _sfcBackBrush;

        public string SfcBackBrush
        {
            get { return _sfcBackBrush; }
            set { SetProperty(ref _sfcBackBrush, value); }
        }


        /// <summary>
        /// pdca背景色
        /// </summary>
        private string _pdcaBackBrush;

        public string PdcaBackBrush
        {
            get { return _pdcaBackBrush; }
            set { SetProperty(ref _pdcaBackBrush, value); }
        }

        /// <summary>
        /// FFU背景色
        /// </summary>
        private string _ffuBackBrush;

        public string FFUBackBrush
        {
            get { return _ffuBackBrush; }
            set { SetProperty(ref _ffuBackBrush, value); }
        }

        private string _ffuState = "FFU";

        public string FFUState
        {
            get { return _ffuState; }
            set { SetProperty(ref _ffuState, value); }
        }



        /// <summary>
        /// 软件版本信息
        /// </summary>
        private string _version;
        public string Version
        {
            get { return _version; }
            set { SetProperty(ref _version, value); }
        }

        /// <summary>
        /// 多语言
        /// </summary>
        private List<Lang> _languages;
        public List<Lang> Languages
        {
            get { return _languages; }
            set { SetProperty(ref _languages, value); }
        }

        /// <summary>
        /// 多语言
        /// </summary>
        private Lang _currentLang;
        public Lang CurrentLang
        {
            get { return _currentLang; }
            set { SetProperty(ref _currentLang, value); }
        }

        /// <summary>
        /// 切换模式
        /// </summary>
        private DelegateCommand<object> _changeLangCommand;
        public DelegateCommand<object> ChangeLangCommand => _changeLangCommand ?? (_changeLangCommand = new DelegateCommand<object>((obj) =>
        {
            var args = obj as SelectionChangedEventArgs;
            if (args != null && args.AddedItems.Count > 0)
            {
                var lang = args.AddedItems[0] as Lang;
                commonBus.EventBus.GetEvent<LangChangedEvent>().Publish(lang.Text);
                _mController.SysConfig.Language = lang.Text;
            }
        }));

        /// <summary>
        /// 集成系统集合
        /// </summary>
        private ObservableCollection<SysStatusModel> _sysItems;
        public ObservableCollection<SysStatusModel> SysItems
        {
            get { return _sysItems; }
            set
            {
                SetProperty(ref _sysItems, value);
            }
        }

        #region Hive
        /// <summary>
        /// 更改模式命令
        /// </summary>
        private DelegateCommand<object> _sysCommand;
        public DelegateCommand<object> SysCommand => _sysCommand ?? (_sysCommand = new DelegateCommand<object>((obj) =>
        {
            var sysModel = obj as SysStatusModel;
            if (sysModel != null)
            {
                if (sysModel.Name == "Hive")
                {
                    //hive.ManualRepairCall();
                    //_dialogService.ShowHive1Dialog("", r =>
                    //{
                    //    //_mController.OnManualStop()
                    //});
                }
            }
        }));
        #endregion

        /// <summary>
        /// 显示版本命令
        /// </summary>
        private DelegateCommand<object> _showVersionCommand;
        public DelegateCommand<object> ShowVersionCommand => _showVersionCommand ?? (_showVersionCommand = new DelegateCommand<object>((obj) =>
        {
            _dialogService.ShowVersionDialog(r =>
            {

            });
        }));




    }

    public class Lang
    {
        /// <summary>
        /// 图片
        /// </summary>
        public ImageSource Image { get; set; }

        public string Text { get; set; }
    }
}