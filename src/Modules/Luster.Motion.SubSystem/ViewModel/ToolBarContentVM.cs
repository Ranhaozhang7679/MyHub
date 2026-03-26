#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ToolBarContentVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.SubSystem.ViewModel
* 文 件 名:       ToolBarContentVM.cs
* 创建时间:       2022/5/18 9:37:15
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      81bc30d2-effd-46a1-b9f7-8d09065ba8f8
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/18 9:37:15
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using HandyControl.Expression.Shapes;
using LiveCharts.Dtos;
using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Extensions;
using Luster.Control.Wpf.Motion.Flow;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.CommonUI.Models;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.EditorUI.Events;
using Luster.Motion.EditorUI.Extensions;
using Luster.Motion.EditorUI.Models;
using Luster.Motion.Integration.Web;
using Luster.Motion.Integration.WorkCardVerify;
using Luster.Motion.SubSystem.Models;
using Luster.Motion.SubSystem.ViewModel.Base;
using Luster.Motion.TaskFlow.Engine;
using Luster.Motion.TaskFlow.Engine.Engine;
using Luster.Motion.TaskFlow.Engine.HyperTrain;
using Luster.Motion.TaskFlow.Engine.Models;
using Luster.SimDevice.SubSystem.Events;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Enums;
using Luster.TaskFlow.Motion.Logic;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;
// 20251028
//using QRCoder;
// 二维码库替换为ZXing
using ZXing;
using ZXing.Common;
using ZXing.Rendering;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static Luster.Common.Tools.SharedMemory.CircularBuffer;
using ZXing.QrCode.Internal;
using ZXing.QrCode;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Color = System.Windows.Media.Color;

namespace Luster.Motion.SubSystem.ViewModel
{
    public class ToolBarContentVM : MotionVM
    {
        /// <summary>
        /// 导航事件
        /// </summary>
        public DelegateCommand<PageModel> NavigateCommand { get; private set; }
        private readonly IAuthService _authService;
        /// <summary>
        /// 运动控制器
        /// </summary>
        public IMotionController mController;

        /// <summary>
        /// 流程控制引擎
        /// </summary>
        private IMotionEngine mEngine;

        /// <summary>
        /// 流程控制引擎
        /// </summary>
        private IDbManager _dbManager;

        /// <summary>
        /// 流程控制引擎
        /// </summary>
        public IDialogService _dialogService;

        /// <summary>
        /// 
        /// </summary>
        private Dispatcher _dispatcher;

        /// <summary>
        /// 默认软件标题，之前是：SKP产品线
        /// </summary>
        private string _defaultTitle = "Motion";

        /// <summary>
        /// 错误管理器
        /// </summary>
        private readonly IErrorManager _errorManager;

        /// <summary>
        /// Web配置
        /// </summary>
        private WebConfig webConfig = null;

        /// <summary>
        /// Web配置
        /// </summary>
        private bool IsStopOrPauseState = false;


        // 本地画笔      
        private SolidColorBrush RedBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(225, 83, 88));
        private SolidColorBrush GreenBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(141, 212, 133));

        private static readonly BitmapImage? _transparentQr = new BitmapImage();

        private bool _HiveStartDialog2Part2_Opend = false;

        HiveAPI _hiveAPI;

        // 90s切idle
        private readonly DispatcherTimer _timer;
        private readonly VisionAPI _visionAPI;
        private int _timeoutSeconds = 90;

        /// <summary>
        /// 区域视图管理
        /// </summary>
        protected ToolBarContentVM(ICommonBus commonBus, IMotionController mController, IMotionEngine motionEngine,
            IDbManager dbManager, Dispatcher dispatcher, IDialogService dialogService,
            IErrorManager errorManager, IAuthService authService, HiveAPI hiveAPI, VisionAPI visionAPI) : base(commonBus)
        {
            _dispatcher = dispatcher;
            _dbManager = dbManager;
            _dialogService = dialogService;
            NavigateCommand = new DelegateCommand<PageModel>(Navigate);
            UserName = Luster.Motion.Assests.Langs.LangProvider.GetLang("Login");
            this.mController = mController;
            mEngine = motionEngine;
            BuildCommands();
            BuildPages();
            Title = _defaultTitle;
            _errorManager = errorManager;
            _authService = authService;
            RunModes = new ObservableCollection<FeatureModel>();
            webConfig = mController.WebService.GetConfig() as WebConfig;
            _hiveAPI = hiveAPI;

            _visionAPI = visionAPI;
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(_timeoutSeconds)
            };
            _timer.Tick += OnTimerTimeout;      // 超时触发
            // 入料事件触发计时器重置
            motionEngine.ProLoadedEvent -= ResetTimer;
            motionEngine.ProLoadedEvent += ResetTimer;  // 监听
            _timer.Start();                     // 第一次启动

            // 报警集合
            AlarmInfos = new ObservableCollection<ErrorDetail>();

            SetMachineStatus(EngineStatus.Idle);

            mController.HomeEndEvent -= MController_HomeEndEvent;
            mController.HomeEndEvent += MController_HomeEndEvent;

            mController.MachineStartEvent -= MController_MachineStartEvent;
            mController.MachineStartEvent += MController_MachineStartEvent;

            mController.MachineStopEvent -= MController_MachineStopEvent;
            mController.MachineStopEvent += MController_MachineStopEvent;

            mController.MachinePauseEvent -= MController_MachinePauseEvent;
            mController.MachinePauseEvent += MController_MachinePauseEvent;

            mController.MachineManualEvent -= MController_MachineManualEvent;
            mController.MachineManualEvent += MController_MachineManualEvent;

            mEngine.DeviceEngine.PrevModeChangeEvent -= DeviceEngine_ModeChangeEvent;
            mEngine.DeviceEngine.PrevModeChangeEvent += DeviceEngine_ModeChangeEvent;

            var converter = new System.Windows.Media.BrushConverter();
            PageBtnBrush = (System.Windows.Media.Brush)converter.ConvertFromString("#3A4249");
            string keyvalue = "LoginContent";
            if (ConfigurationManager.AppSettings.AllKeys.Contains("StartContent"))
            {
                var configValue = ConfigurationManager.AppSettings["StartContent"];
                if (!string.IsNullOrEmpty(configValue))
                {
                    keyvalue = configValue;
                }
            }
            StartContent = keyvalue;

            // 构造 1×1 透明 PNG
            using var bmp = new Bitmap(1, 1);
            bmp.SetPixel(0, 0, System.Drawing.Color.Transparent);
            using var ms = new MemoryStream();
            bmp.Save(ms, ImageFormat.Png);
            ms.Position = 0;

            _transparentQr.BeginInit();
            _transparentQr.StreamSource = ms;
            _transparentQr.CacheOption = BitmapCacheOption.OnLoad;
            _transparentQr.EndInit();
            _transparentQr.Freeze();   // 必须 Freeze，才能跨线程

        }

        // 90s切idle：计时到90s，进行超时触发
        private void OnTimerTimeout(object sender, EventArgs e)
        {
            _timer.Stop();              // 超时后先停掉，避免重复触发
            // 超时通知，判断是否需要发送 running -> idle
            if (MStatus == EngineStatus.Running.GetDescription() && (mController.GetCurrentMode().Contains("生产") || mController.GetCurrentMode().Contains("空跑")))
            {
                if (webConfig.HiveEnabled && webConfig.VisionEnabled)
                {
                    // 将耗时的HTTP发送放到线程池，避免阻塞UI线程
                    _ = Task.Run(() => _hiveAPI.SendStausChange(2, "", "", RepairState.Normal));

                    // 界面右上角显示：待料中，与机台原本的 空闲中 进行区分
                    SetMachineStatus(EngineStatus.MaterialPending);
                    _hiveAPI.Status = TrainRunMode.Idle;
                    _visionAPI.Status = TrainRunMode.Idle;
                }
                else if (!webConfig.HiveEnabled && webConfig.VisionEnabled)
                {
                    SetMachineStatus(EngineStatus.MaterialPending);
                    //_visionAPI.MachineStatusUpload(TrainRunMode.Running, TrainRunMode.Idle, "", "");
                    _visionAPI.CheckUpStatus(TrainRunMode.Running, TrainRunMode.Idle, "", "");
                    _visionAPI.Status = TrainRunMode.Idle;
                }
            }
            else
            {
                _timer.Stop();              // 机台未运行，也需要重新计时
            }
            _timer.Start();             // 重新开始下一轮 90 s 监控
        }
        // 未到90s，持续在入料
        public void ResetTimer(IMotionModule module, StationResult result)
        {
            // DispatcherTimer 没有 Reset，先停再开
            _timer.Stop();
            _timer.Start();
            // 判断是否需要发送 idle -> running，先复用机台原有的“空闲中”
            if (MStatus == EngineStatus.MaterialPending.GetDescription() && (mController.GetCurrentMode().Contains("生产") || mController.GetCurrentMode().Contains("空跑")))
            {
                if (webConfig.HiveEnabled && webConfig.VisionEnabled)
                {
                    _ = Task.Run(() => _hiveAPI.SendStausChange(1, "", "", RepairState.Normal));
                    // 界面右上角显示：运行中
                    SetMachineStatus(EngineStatus.Running);
                    _hiveAPI.Status = TrainRunMode.Running;
                    _visionAPI.Status = TrainRunMode.Running;
                }
                else if (!webConfig.HiveEnabled && webConfig.VisionEnabled)
                {
                    SetMachineStatus(EngineStatus.Running);
                    _visionAPI.CheckUpStatus(TrainRunMode.Idle, TrainRunMode.Running, "", "");
                    _visionAPI.Status = TrainRunMode.Running;
                }
            }
        }

        // 基于ZXing，把字符串生成二维码 BitmapImage（WPF 可直接绑定到 Image 控件）
        public void GenerateQr(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            // 1. 编码
            var writer = new BarcodeWriter<Bitmap>
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    Height = 700,          // 0 = 让 ZXing 根据内容自动选最小尺寸
                    Width = 700,
                    Margin = 2,         // 四周白边（单位：像素） whiteBorder
                    ErrorCorrection = ErrorCorrectionLevel.L
                },
                Renderer = new ZXing.Rendering.BitmapRenderer
                {
                    Foreground = System.Drawing.Color.Black,
                    Background = System.Drawing.Color.White
                }
            };

            using (var bmp = writer.Write(text))
            {
                QrSource = BmpToImageSource(bmp);   // 你原来的转换方法
            }
        }

        //public void GenerateQr(string text)
        //{
        //    if (string.IsNullOrWhiteSpace(text)) return;

        //    using (var qrGen = new QRCodeGenerator())
        //    using (var qrData = qrGen.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q))
        //    using (var qrCode = new QRCode(qrData))
        //    using (var bmp = qrCode.GetGraphic(20))   // 20 像素/模块
        //    {
        //        QrSource = BmpToImageSource(bmp);
        //    }
        //}
        private static BitmapImage BmpToImageSource(Bitmap bmp)
        {
            var ms = new MemoryStream();
            bmp.Save(ms, ImageFormat.Png);
            ms.Position = 0;

            var img = new BitmapImage();
            img.BeginInit();
            img.StreamSource = ms;
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.EndInit();
            img.Freeze();          // 跨线程访问必须 Freeze
            return img;
        }

        private void DeviceEngine_ModeChangeEvent(bool bIsEnable)
        {
            Commands[0].ChangeBtnForMotionCardInit(bIsEnable);
        }

        /// <summary>
        /// 硬件按钮操作start
        /// </summary>
        /// <param name="obj"></param>
        private void MController_MachineStartEvent(string obj)
        {
            //20250329
            //如果有Hive弹窗，不再响应设备按键
            if (useHiveDialog && webConfig.HiveEnabled)
            {
                return;
            }
            //如果之前状态是暂停的话，则直接启动
            //否则需要弹窗
            if (IsStopOrPauseState)
            {
                return;
            }
            tryStart();

        }

        /// <summary>
        /// 硬件按钮操作stop
        /// </summary>
        /// <param name="obj"></param>
        private void MController_MachineStopEvent(string obj)
        {
            //20250329
            //如果有Hive弹窗，不再响应设备按键
            if (useHiveDialog && webConfig.HiveEnabled)
            {
                return;
            }
            if (webConfig.HiveEnabled && (mController.GetCurrentMode().Contains("生产") || mController.GetCurrentMode().Contains("空跑")))
            {
                _dialogService.ShowSelectTip(webConfig.HiveEnabled, SystemOperation.Stop, (r) =>
                {
                    mController.Stop();
                    mController.CloseOperateIO(SystemOperation.Stop);

                    // todo 获取暂停原因上传驾驶舱
                    /*if (r.Parameters.TryGetValue<string>("Memo", out memo))
                    {
                        mController.OnManualStop(command.Key, memo);
                    }*/

                });
            }
            else
            {
                //非生产模式
                mController.Stop();
            }
        }

        /// <summary>
        /// 硬件按钮操作pause
        /// </summary>
        /// <param name="obj"></param>
        private void MController_MachinePauseEvent(string obj)
        {
            //20250329
            //如果有Hive弹窗，不再响应设备按键
            if (useHiveDialog && webConfig.HiveEnabled)
            {
                return;
            }
            if (webConfig.HiveEnabled)
            {
                mController.Pause(false);
                Commands[0].SetEnabled(false);
                useHiveDialog = true;
                IsStopOrPauseState = true;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _dialogService.ShowSelectTip((webConfig.HiveEnabled && (mController.GetCurrentMode().Contains("生产") || mController.GetCurrentMode().Contains("空跑"))), SystemOperation.Pause, (r) =>
                    {
                        if (!mController.FileConfig.IsHiveContinueMaintenanceVisible && webConfig.HiveEnabled)
                        {
                            mController.Start();
                        }
                        useHiveDialog = false;
                        Commands[0].ChangeButton(new StatusChanged(mController.MachineStatus, mController.MachineStatus));
                        //mController.Stop();
                        //mController.CloseOperateIO(SystemOperation.Stop);

                        // todo 获取暂停原因上传驾驶舱
                        /*if (r.Parameters.TryGetValue<string>("Memo", out memo))
                        {
                            mController.OnManualStop(command.Key, memo);
                        }*/
                        IsStopOrPauseState = false;
                    });
                });
            }
            else
            {
                //非生产模式
                mController.Pause(false);
                Commands[0].ChangeButton(new StatusChanged(mController.MachineStatus, mController.MachineStatus));
            }
        }

        private void MController_MachineManualEvent(string obj)
        {
            //如果有Hive弹窗，不再响应设备按键
            if (useHiveDialog && webConfig.HiveEnabled)
            {
                return;
            }
            if (webConfig.HiveEnabled && (mController.GetCurrentMode().Contains("生产") || mController.GetCurrentMode().Contains("空跑"))) //且生产模式
            {
                var curStatus = mController.MachineStatus;
                mController.Pause(false);
                Commands[0].SetEnabled(false);
                useHiveDialog = true;
                IsStopOrPauseState = true;
                //if (curStatus == EngineStatus.Ready)
                //{
                //    Commands[0].ChangeButton(new StatusChanged(mController.MachineStatus, mController.MachineStatus));
                //    useHiveDialog = false;
                //    IsStopOrPauseState = false;
                //    return;
                //}
                if (curStatus == EngineStatus.Running || curStatus == EngineStatus.MaterialPending)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _dialogService.ShowSelectTip(webConfig.HiveEnabled, SystemOperation.Pause, (r) =>
                        {
                            useHiveDialog = false;
                            Commands[0].ChangeButton(new StatusChanged(mController.MachineStatus, mController.MachineStatus));
                            IsStopOrPauseState = false;
                        });
                    });
                }
                else
                {
                    Commands[0].ChangeButton(new StatusChanged(mController.MachineStatus, mController.MachineStatus));
                    useHiveDialog = false;
                    IsStopOrPauseState = false;
                }
            }
            else
            {
                //非生产模式
                mController.Pause(false);
                Commands[0].ChangeButton(new StatusChanged(mController.MachineStatus, mController.MachineStatus));
            }
        }

        private void MController_HomeEndEvent(string obj)
        {
            if (CurrentMode != obj)
            {
                throw new FriendlyException($"机台此时模式:{CurrentMode},程序内部模式:{obj},请重新切换并回零");
            }
            UpdateRemainMaintenanceDays();
        }


        /// <summary>
        /// 配置机台状态
        /// </summary>
        /// <param name="status"></param>
        protected virtual void SetMachineStatus(EngineStatus status)
        {
            MStatus = status.GetDescription();
        }

        protected override void RegisterEvent(IEventAggregator bus)
        {
            base.RegisterEvent(bus);

            //用户登录成功
            bus.GetEvent<UserLoginEvent>().Subscribe(usermodel =>
            {
                LoginCheck = false;
                BuildCommands();
                BuildPages();
                UserName = usermodel.UserName;
            });
            string keyvalue = "LoginContent";
            if (ConfigurationManager.AppSettings.AllKeys.Contains("StartContent"))
            {
                var configValue = ConfigurationManager.AppSettings["StartContent"];
                if (!string.IsNullOrEmpty(configValue))
                {
                    keyvalue = configValue;
                }
            }
            if (keyvalue == "LoginContentFX")
            {
                //用户登录成功
                bus.GetEvent<UserInfoEvent>().Subscribe(userInfo =>
                {
                    ForeColor = System.Windows.Media.Brushes.White;
                    //if (userInfo.Role == SystemRole.Admin || userInfo.Role == SystemRole.Integrator)
                    //{
                    //    ForeColor = System.Windows.Media.Brushes.White;
                    //}
                    BackColor = userInfo.Role switch
                    {
                        //SystemRole.Operator => new SolidColorBrush(Color.FromArgb(128, 128, 128, 128)),
                        //SystemRole.Maintenance => new SolidColorBrush(Color.FromArgb(240, 238, 235, 88)),
                        //SystemRole.Integrator => new SolidColorBrush(Color.FromArgb(64, 0, 0, 255)),
                        //SystemRole.Admin => new SolidColorBrush(Color.FromArgb(64, 128, 0, 128)),
                        SystemRole.Operator => new SolidColorBrush(Color.FromArgb(128, 128, 128, 128)), // (128, 214, 214, 214)看不见启动按钮三个图标
                        SystemRole.Maintenance => new SolidColorBrush(Color.FromArgb(240, 255, 218, 90)),
                        SystemRole.Integrator => new SolidColorBrush(Color.FromArgb(64, 81, 143, 255)),
                        SystemRole.Admin => new SolidColorBrush(Color.FromArgb(64, 66, 51, 166)),
                        _ => System.Windows.Media.Brushes.ForestGreen
                    };
                    UserMsg = string.Concat(userInfo.Name, "-", userInfo.Company, "-", userInfo.Level);
                    _authService?.OnRoleChanged(userInfo);

                    foreach (var item in Pages)
                    {
                        if (userInfo.Role == SystemRole.Operator)
                        {
                            Pages.FirstOrDefault(x => x.Name == "Flow").page_IsEnabled = false;
                            Pages.FirstOrDefault(x => x.Name == "Configure").page_IsEnabled = false;
                            Pages.FirstOrDefault(x => x.Name == "Project").page_IsEnabled = false;

                        }
                        else
                        {
                            Pages.FirstOrDefault(x => x.Name == "Flow").page_IsEnabled = true;
                            Pages.FirstOrDefault(x => x.Name == "Configure").page_IsEnabled = true;
                            Pages.FirstOrDefault(x => x.Name == "Project").page_IsEnabled = true;

                        }

                    }

                });
                bus.GetEvent<UserLogoutEvent>().Subscribe((Timeout) =>
                {
                    RemainT = $"ReaminSeconds-{Timeout}";
                });
            }
            bus.GetEvent<OperationEvent>().Subscribe(sChanged =>
            {
                SetMachineStatus(sChanged.Dst);
                switch (sChanged.Dst)
                {
                    case EngineStatus.Idle:
                        StatusBrush = FlowItem.NonAreaBrush;
                        LXStatus = "Idle";
                        break;
                    case EngineStatus.Homing:
                    case EngineStatus.Running:
                    case EngineStatus.Resetting:
                        StatusBrush = FlowItem.SuccessBrush;
                        LXStatus = "Running";
                        break;
                    case EngineStatus.Ready:
                        StatusBrush = FlowItem.RunningBrush;
                        LXStatus = "Idle";
                        break;
                    case EngineStatus.Alarm:
                        StatusBrush = FlowItem.FailBrush;
                        LXStatus = "DownTime";
                        break;
                    case EngineStatus.Pause:
                    case EngineStatus.Stop:
                        StatusBrush = FlowItem.TimeoutBrush;
                        LXStatus = "DownTime";
                        break;
                    default:
                        break;
                }

                // 更新按钮状态
                _dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    if (useHiveDialog == false)
                    {
                        Commands[0].ChangeButton(sChanged);
                        SetBtnStatus(sChanged);
                    }

                    // 软件处于启动、回零完成、暂停和停止状态下才能进行切换
                    BtnEnable = sChanged.Dst == EngineStatus.Idle || sChanged.Dst == EngineStatus.Stop || sChanged.Dst == EngineStatus.Pause || sChanged.Dst == EngineStatus.Ready;

                    if (sChanged.Dst != EngineStatus.Alarm && sChanged.Dst != EngineStatus.Stop)
                    {
                        SetAlarm();
                    }
                }));

                // 回零完成自动切换到主页面
                if (sChanged.Dst == EngineStatus.Ready)
                {
                    commonBus.OnNavigate(Pages.FirstOrDefault(x => x.Name == "Home"));
                }

                ModeEnabled = sChanged.Dst == EngineStatus.Idle || sChanged.Dst == EngineStatus.Ready || sChanged.Dst == EngineStatus.Stop;
            });

            bus.GetEvent<RecipeOpenEvent>().Subscribe(recipe =>
            {
                webConfig = mController.WebService.GetConfig() as WebConfig;
                if (string.IsNullOrEmpty(webConfig.MachineSn))
                {
                    AlarmInfo alarmInfo = new AlarmInfo("", AlarmType.InfoTip, $"驾驶舱参数未加载成功，请检查webConfig文件！！！", "");
                    mController.MotionEngine.OnAlarm(alarmInfo);
                }
                RecipeActive(recipe);
                ModeEnabled = true;
            });

            //
            bus.GetEvent<SystemConfigChangeEvent>().Subscribe(() =>
            {
                Title = webConfig.MachineName ?? "CGLink";
                StationId = webConfig.StationId;
            });

            bus.GetEvent<AlarmEvent>().Subscribe(a =>
            {
                _dispatcher.Invoke(() =>
                {
                    SetAlarm(a);

                    //20250329 alarm出现后，禁止界面按钮和设备按键
                    _dispatcher.Invoke(() =>
                    {
                        var module = a.Sender as IMotionModule;
                        string message = module == null ? a.Message : $"模块:{module.Alias} {a.Message}";
                        // 新增“警告提示”类型，弹窗功能：Hive报警窗
                        if (a.AlarmType == AlarmType.PopInfoTip || a.AlarmType == AlarmType.WarningTip ||
                        a.AlarmType == AlarmType.DeviceError || a.AlarmType == AlarmType.Timeout || a.AlarmType == AlarmType.FailError ||
                        a.AlarmType == AlarmType.PlcAlarm)
                        {
                            //禁止按钮
                            useHiveDialog = true;
                            Commands[0].SetEnabled(false);
                            Commands[0].EnableRecoveryButton();
                            Commands[0].ClearCmdButton();

                            // 报警弹窗
                            _dialogService.ShowAlarmNew(a, _HiveStartDialog2Part2_Opend, res =>
                            {
                                //恢复按钮
                                useHiveDialog = false;
                                Commands[0].ChangeButton(new StatusChanged(mController.MachineStatus, mController.MachineStatus));

                                // 1.记录开始时间
                                res.Parameters.TryGetValue<DateTime>("StartTime", out var startTime);

                                // 2.处理方式
                                if (res.Parameters.TryGetValue<Luster.Motion.DataStruct.Enums.AlarmProc>("AlarmProc", out var proc))
                                {
                                    var curModule = a.Sender as IMotionModule;
                                    switch (proc)
                                    {
                                        //case Luster.Motion.DataStruct.Enums.AlarmProc.PlcClearError:
                                        case Luster.Motion.DataStruct.Enums.AlarmProc.Contine:

                                            // 忽略
                                            if (a.OnAlarmProc(proc))
                                            {
                                                if (a.AlarmType == AlarmType.InfoTip)
                                                {
                                                    commonBus.OnLog(LogType.Info, $"信息提示:{a.Message}被忽略");
                                                }
                                                else
                                                {
                                                    res.Parameters.TryGetValue<int>("Curve", out var Curve);

                                                    var gModule = mEngine.Get(GlobalModule.GlobalID);
                                                    if (gModule.Parameters.ContainsKey("Extend_料盘穴位索引"))
                                                    {
                                                        var gItem = gModule.Parameters["Extend_料盘穴位索引"];
                                                        if (gItem.Type == typeof(int))
                                                        {
                                                            gItem.Value = Curve;
                                                        }
                                                    }
                                                    // 警告类的因为将流程暂停了，所以需要恢复流程
                                                    mController.Recovery();
                                                }
                                            }

                                            break;
                                        case Luster.Motion.DataStruct.Enums.AlarmProc.Repair:

                                            mController.SetBuzzer(false);

                                            break;
                                        case Luster.Motion.DataStruct.Enums.AlarmProc.Stop:
                                        case Luster.Motion.DataStruct.Enums.AlarmProc.Check:

                                            // 设备检修,关掉当前的窗口，对设备进行调试，此时流程暂停
                                            if (a.OnAlarmProc(proc))
                                            {
                                                commonBus.OnLog(LogType.Info, $"弹窗报警人员点击软件停止");
                                                mController.Stop();
                                            }

                                            break;

                                        case Luster.Motion.DataStruct.Enums.AlarmProc.Retry:
                                            {
                                                /*LoadingMsg = $"{r.Message} 尝试重新运行...";
                                                IsLoading = true;*/

                                                if (a.OnAlarmProc(proc))
                                                {
                                                    // 重新运行当前模块
                                                    mController.Retry(curModule);
                                                }

                                                /*IsLoading = false;
                                                LoadingMsg = "";*/

                                                break;
                                            }
                                        case Luster.Motion.DataStruct.Enums.AlarmProc.Ng:
                                            // 产品NG处理
                                            Task.Run(() =>
                                            {
                                                /*LoadingMsg = $"{r.Message} 尝试运行NG处理模块...";
                                                IsLoading = true;*/
                                                a.OnAlarmProc(proc);

                                                // 回复
                                                mController.Recovery();

                                                /*IsLoading = false;
                                                LoadingMsg = "";*/

                                            });
                                            break;
                                    }
                                }
                                // 点击 确认-清掉故障 按钮，需要清掉界面二维码
                                if (res.Parameters.TryGetValue<string>("closeWay", out var closeway) && closeway == "Button")
                                {
                                    QrSource = _transparentQr;
                                }
                                //_dbManager.AddAlarm(module?.Name, r.AlarmType, r.Message, startTime, proc, r.AlarmCode);
                            });
                            // 报警时生成QR码，信息提示不生成
                            //GenerateQr("https://github.com/codebude/QRCoder");
                            string errCode = "";
                            if (a.AlarmCode.Contains('@')) errCode = a.AlarmCode.Split('@')[0];
                            else errCode = a.AlarmCode;
                            GenerateQr(webConfig.StationId + "+" + webConfig.Product + "+" + webConfig.VendorName?.ToUpper() + "+" + errCode);
                        }
                        else if (a.AlarmType == AlarmType.RetryAlarm || a.AlarmType == AlarmType.ManuOperationAlarm)
                        {
                            //禁止按钮
                            //useHiveDialog = true;
                            Commands[0].SetEnabled(false);
                            Commands[0].EnableRecoveryButton();
                            Commands[0].ClearCmdButton();

                            // 报警弹窗
                            _dialogService.ShowAlarmNew(a, _HiveStartDialog2Part2_Opend, res =>
                            {
                                //恢复按钮
                                //useHiveDialog = false;
                                Commands[0].ChangeButton(new StatusChanged(mController.MachineStatus, mController.MachineStatus));

                                // 1.记录开始时间
                                res.Parameters.TryGetValue<DateTime>("StartTime", out var startTime);

                                // 2.处理方式
                                if (res.Parameters.TryGetValue<Luster.Motion.DataStruct.Enums.AlarmProc>("AlarmProc", out var proc))
                                {
                                    var curModule = a.Sender as IMotionModule;
                                    switch (proc)
                                    {
                                        //case Luster.Motion.DataStruct.Enums.AlarmProc.PlcClearError:
                                        case Luster.Motion.DataStruct.Enums.AlarmProc.Contine:

                                            // 忽略
                                            if (a.OnAlarmProc(proc))
                                            {
                                                if (a.AlarmType == AlarmType.InfoTip)
                                                {
                                                    commonBus.OnLog(LogType.Info, $"信息提示:{a.Message}被忽略");
                                                }
                                                else if (a.AlarmType == AlarmType.ManuOperationAlarm)
                                                {
                                                    commonBus.OnLog(LogType.Info, $"信息提示:{a.Message}操作完成");
                                                }
                                                else
                                                {
                                                    res.Parameters.TryGetValue<int>("Curve", out var Curve);

                                                    var gModule = mEngine.Get(GlobalModule.GlobalID);
                                                    if (gModule.Parameters.ContainsKey("Extend_料盘穴位索引"))
                                                    {
                                                        var gItem = gModule.Parameters["Extend_料盘穴位索引"];
                                                        if (gItem.Type == typeof(int))
                                                        {
                                                            gItem.Value = Curve;
                                                        }
                                                    }
                                                    // 警告类的因为将流程暂停了，所以需要恢复流程
                                                    mController.Recovery();
                                                }
                                            }

                                            break;
                                        case Luster.Motion.DataStruct.Enums.AlarmProc.Repair:

                                            mController.SetBuzzer(false);

                                            break;
                                        case Luster.Motion.DataStruct.Enums.AlarmProc.Stop:
                                        case Luster.Motion.DataStruct.Enums.AlarmProc.Check:

                                            // 设备检修,关掉当前的窗口，对设备进行调试，此时流程暂停
                                            if (a.OnAlarmProc(proc))
                                            {
                                                commonBus.OnLog(LogType.Info, $"弹窗报警人员点击软件停止");
                                                mController.Stop();
                                            }

                                            break;

                                        case Luster.Motion.DataStruct.Enums.AlarmProc.Retry:
                                            {
                                                //LoadingMsg = $"{r.Message} 尝试重新运行...";
                                                //IsLoading = true;

                                                if (a.OnAlarmProc(proc))
                                                {
                                                    // 重新运行当前模块
                                                    mController.Retry(curModule);
                                                }

                                                //IsLoading = false;
                                                //LoadingMsg = "";

                                                break;
                                            }
                                        case Luster.Motion.DataStruct.Enums.AlarmProc.Ng:
                                            // 产品NG处理
                                            Task.Run(() =>
                                            {
                                                //LoadingMsg = $"{a.Message} 尝试运行NG处理模块...";
                                                //IsLoading = true;
                                                a.OnAlarmProc(proc);

                                                // 回复
                                                mController.Recovery();

                                                //IsLoading = false;
                                                //LoadingMsg = "";

                                            });
                                            break;
                                    }
                                }

                                //_dbManager.AddAlarm(module?.Name, r.AlarmType, r.Message, startTime, proc, r.AlarmCode);
                            });
                        }
                        else
                        {

                        }

                        return;
                    });
                });
            });

            // Hive Comm Error/OffBoard状态下，弹出Onboard报警弹窗
            bus.GetEvent<AlarmEventOnboard>().Subscribe(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _dialogService.HiveOnboardAlarmPopUp((r) =>
                    {
                        //if (r.Result == ButtonResult.OK)
                        //{
                        //    // 用户点了按钮，给Hive发送消息

                        //}
                    });
                });
            });
            bus.GetEvent<AlarmEventOnboard2>().Subscribe(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _dialogService.HiveOnboardAlarmPopUp((r) =>
                    {
                    });
                });
            });
            // Hive Idle/Running/Down状态下，关闭Onboard报警弹窗
            bus.GetEvent<AlarmEventOnboardClosed>().Subscribe(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _dialogService.HiveOnboardAlarmClosed((r) =>
                    {
                    });
                });
            });

            bus.GetEvent<ShowHolo3D>().Subscribe(() =>
            {
                AddHolo3D();
            });

            bus.GetEvent<PageNavEvent>().Subscribe(pageModel =>
            {
                foreach (var item in Pages)
                {
                    item.page_IsSelected = false;
                }
                if (pageModel != null)
                {
                    pageModel.page_IsSelected = true;
                }

            });

            // 单步调试未运行完成，不允许页面切换
            bus.GetEvent<ModulePrevRunEvent>().Subscribe(() =>
            {
                // 运行过程中，不允许切换界面
                foreach (var page in Pages)
                {
                    // 运行过程中禁止页面切换
                    if (page.Name == "Project")
                    {
                        page.page_IsEnabled = false;
                    }
                }
            });

            // 单步调试运行完成，允许页面切换
            bus.GetEvent<ModulePostRunEvent>().Subscribe((ms) =>
            {
                // 运行过程中，不允许切换界面
                foreach (var page in Pages)
                {
                    // 运行过程中禁止页面切换
                    if (page.Name == "Project")
                    {
                        page.page_IsEnabled = true;
                    }
                }
            });

            bus.GetEvent<SensorEvent>().Subscribe((ms) =>
            {
                SensorIsOK = ms;
                token?.Cancel();
            });

            // 在线离线切换时候
            bus.GetEvent<RunModeEvent>().Subscribe((mode) =>
            {
                if (mode == DeviceMode.Real)
                {
                    mController.SetCurrentMode(SystemConsts.ProductMode);
                }
            });

            bus.GetEvent<MachineModeEvent>().Subscribe((mode) =>
            {
                CurrentMode = mode.NewMode;
                foreach (var item in RunModes)
                {
                    item.IsSelected = item.Name == CurrentMode;
                }
            });

            bus.GetEvent<RoleChangeEvent>().Subscribe((newRole) =>
            {
                RoleChange(newRole);
            });
            bus.GetEvent<LoginEvent>().Subscribe(() =>
            {
                Login(null);
            });

            bus.GetEvent<HiveReportStateChangedEvent>().Subscribe(isOpen =>
            {
                _HiveStartDialog2Part2_Opend = isOpen;
                _hiveAPI.Dialog2Part2_Opend = isOpen;
            });
        }



        protected virtual void RecipeActive(Recipe recipe)
        {
            ProjName = $"{recipe.ProjInfo.ProjName}-{recipe.Name}  {webConfig.SoftVersion}";
            TitleVisible = true;
            Title = webConfig.MachineName ?? "CGLink";
            StationId = webConfig.StationId;
            InitModes();
        }


        /// <summary>
        /// 初始化运行模式
        /// </summary>
        public void InitModes(bool isInit = true)
        {
            _dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                RunModes = new ObservableCollection<FeatureModel>();
                var modes = mController.GetModeList();
                if (modes != null && modes.Count > 0)
                {
                    foreach (var mode in modes)
                    {
                        RunModes.Add(new FeatureModel()
                        {
                            IsSelected = mode.IsRunMode,
                            Name = mode.Mode
                        });

                        // 第一次初始化的时候需要设置该模式状态
                        if (isInit)
                        {
                            // 操作员默认使用生产模式
                            if (commonBus.CurrentUser != null && commonBus.CurrentUser.UserRole == SystemRole.Operator)
                            {
                                mController.SetCurrentMode(SystemConsts.ProductMode);
                                CurrentMode = SystemConsts.ProductMode;
                            }
                            else
                            {
                                // 初始化当前的模式
                                if (mode.IsRunMode && CurrentMode != mode.Mode)
                                {
                                    CurrentMode = mode.Mode;
                                    mController.SetCurrentMode(CurrentMode);
                                }
                            }
                        }
                    }
                }
            }));
        }

        /// <summary>
        /// 报警集合
        /// </summary>
        private ObservableCollection<ErrorDetail> _alarmInfos = null;
        public ObservableCollection<ErrorDetail> AlarmInfos { get => _alarmInfos; set => SetProperty(ref _alarmInfos, value); }

        /// <summary>
        /// Alarm报警
        /// </summary>
        /// <param name="alarmInfo"></param>
        private void SetAlarm(AlarmInfo alarmInfo = null)
        {
            if (alarmInfo == null)
            {
                IsAlarm = false;
                AlarmInfos?.Clear();
            }
            else
            {
                IsAlarm = true;

                var errDetail = _errorManager.GetErrorDetail(alarmInfo);
                if (!AlarmInfos.Any(u => u.Code == errDetail.Code && u.Message == errDetail.Message && u.Addition == errDetail.Addition))
                {
                    AlarmInfos.Add(errDetail);

                    // 如果报警超过20次，就移除第一个
                    if (AlarmInfos.Count > 10)
                    {
                        AlarmInfos.RemoveAt(0);
                    }
                }
            }
        }
        /// <summary>
        /// 角色切换
        /// </summary>
        /// <param name="role"></param>
        protected virtual void RoleChange(SystemRole role)
        {

            UpdateRemainMaintenanceDays();
        }

        protected virtual void BuildCommands()
        {
            Commands = new ObservableCollection<CommandModel>();
            Commands.AddRange(CommandModel.Commands);
            Commands[0].SetUserPermission(commonBus.CurrentUser);
        }

        protected virtual void BuildPages()
        {
            Pages = new ObservableCollection<PageModel>();
            Pages.AddRange(PageModel.Pages);
        }

        /// <summary>
        /// Click 页面导航
        /// </summary>
        /// <param name="pagemodel"></param>
        protected virtual void Navigate(PageModel pagemodel)
        {
            if (pagemodel.Region != null)
            {
                commonBus.OnNavigate(pagemodel);
                LoginCheck = false;
                //FingerTest(pagemodel.Name, (isOK) =>
                //{
                //    if (isOK)
                //    {
                //        commonBus.OnNavigate(pagemodel);
                //        LoginCheck = false;
                //    }
                //});
            }
        }

        /// <summary>
        /// Click 启停操作
        /// </summary>
        private DelegateCommand<CommandModel> _operateCommand;
        public DelegateCommand<CommandModel> OperateCommand => _operateCommand ?? (_operateCommand = new DelegateCommand<CommandModel>((command) =>
        {
            string memo = string.Empty;

            commonBus.OnLog(Common.DataStruct.Enums.LogType.Info, $"软件点击动作:{command.Key}");
            try
            {
                BtnClick = false;
                switch (command.Key)
                {
                    case DataStruct.Enums.SystemOperation.Start:
                        if (!mController.CanAutoRun(out var errMsg))
                        {
                            commonBus.OnLog(Common.DataStruct.Enums.LogType.Info, $"当前未切换至自动按钮下,不满足启动条件");
                            throw new FriendlyException(errMsg);
                        }

                        if (!mController.DoorIsClosed(out errMsg))
                        {
                            commonBus.OnLog(Common.DataStruct.Enums.LogType.Info, $"安全门未关闭,不满足启动条件");
                            throw new FriendlyException(errMsg);
                        }

                        tryStart();

                        break;
                    case DataStruct.Enums.SystemOperation.Recovery:

                        mController.Recovery();

                        break;

                    case DataStruct.Enums.SystemOperation.Home:

                        if (!mController.DoorIsClosed(out errMsg))
                        {
                            throw new FriendlyException(errMsg);
                        }
                        Commands[0].SetEnabled(false);
                        mController.Home();
                        mController.SetCurrentMode(SystemConsts.ProductMode);
                        CurrentMode = SystemConsts.ProductMode;
                        var converter = new System.Windows.Media.BrushConverter();
                        PageBtnBrush = (System.Windows.Media.Brush)converter.ConvertFromString("#3A4249");
                        commonBus.EventBus.GetEvent<ProductModeEvent>().Publish(CurrentMode);
                        break;

                    case DataStruct.Enums.SystemOperation.Pause:
                        mController.Pause(false);
                        mController.CloseOperateIO(SystemOperation.Pause);
                        break;
                    case DataStruct.Enums.SystemOperation.Stop:
                        mController.Pause(false);
                        mController.CloseOperateIO(SystemOperation.Pause);
                        break;
                }



                // 根据配置的提示来进行弹窗
                if (command.Key == SystemOperation.Stop || command.Key == SystemOperation.Pause)
                {
                    mController.FileConfig.IsCardEnd = false;
                    useHiveDialog = true;
                    Commands[0].SetEnabled(false);
                    GenerateQr(webConfig.StationId + "+" + webConfig.Product + "+" + webConfig.VendorName?.ToUpper() + "+" + "F99OOOO-20");
                    _dialogService.ShowSelectTip((webConfig.HiveEnabled && (mController.GetCurrentMode().Contains("生产") || mController.GetCurrentMode().Contains("空跑"))), command.Key, (r) =>
                    {
                        //string ss = r.Result.ToString(); // ss = "OK"
                        useHiveDialog = false;
                        Commands[0].ChangeButton(new StatusChanged(mController.MachineStatus, mController.MachineStatus));
                        _dbManager.AddSysOperation(command.Key, memo, commonBus.CurrentUser?.UserName);

                        //if (!mController.FileConfig.IsHiveContinueMaintenanceVisible && webConfig.HiveEnabled)
                        //{
                        //    mController.Start();
                        //}
                        //else 
                        if (command.Key == SystemOperation.Stop)
                        {
                            //if (mController.FileConfig.IsCardEnd || !webConfig.HiveEnabled)
                            //{
                            mController.Stop();
                            mController.CloseOperateIO(SystemOperation.Stop);

                            // todo 获取暂停原因上传驾驶舱
                            if (r.Parameters.TryGetValue<string>("Memo", out memo))
                            {
                                mController.OnManualStop(command.Key, memo);
                            }
                            //}
                        }
                        else if (command.Key == SystemOperation.Pause)
                        {
                            if (r.Parameters.TryGetValue<string>("closeWay", out var closeway2) && closeway2 == "Card")
                            {
                                if (r.Parameters.TryGetValue<string>("command", out var cmd) && cmd == "pause")
                                {
                                    Commands[0].SetEnabled(false);
                                    mController.FileConfig.IsHiveContinueMaintenanceVisible = false;
                                    useHiveDialog = true;
                                    _dialogService.HiveRunPopUp(SystemOperation.Start, (r) =>
                                    {
                                        useHiveDialog = false;
                                        Commands[0].ChangeButton(new StatusChanged(mController.MachineStatus, mController.MachineStatus));
                                        _dbManager.AddSysOperation(SystemOperation.Start, "", commonBus.CurrentUser?.UserName);
                                        // 切回 UI 线程再置空
                                        QrSource = _transparentQr;          // 清掉二维码 或者 new BitmapImage() 放一张透明图
                                    });
                                }
                            }


                        }
                        // 此时界面还没切换状态，取不到运行中
                        //if (MStatus == "运行中")
                        if (r.Parameters.TryGetValue<string>("closeWay", out var closeway) && closeway == "Button")
                        {
                            QrSource = _transparentQr;
                        }
                        else
                        {
                            if (r.Parameters.TryGetValue<string>("QrErrorCode", out string qrErrCode))
                            {
                                GenerateQr(webConfig.StationId + "+" + webConfig.Product + "+" + webConfig.VendorName?.ToUpper() + "+" + qrErrCode);

                            }
                        }
                    });
                }

            }
            catch (Exception)
            {
                // 切换失败后，按钮复位状态
                Commands[0].ChangeButton(new StatusChanged(mController.MachineStatus, mController.MachineStatus));
                throw;
            }
            finally
            {
                BtnClick = true;

                // 更新下状态
                if (useHiveDialog == false)
                {
                    Commands[0].ChangeButton(new StatusChanged(mController.MachineStatus, mController.MachineStatus));

                    _dbManager.AddSysOperation(command.Key, memo, commonBus.CurrentUser?.UserName);
                }
                else
                {
                    //Commands[0].SetEnabled(false);
                }
            }

        }, (c) => BtnClick).ObservesCanExecute(() => BtnClick));


        private void tryStart()
        {
            if (webConfig.HiveEnabled && (mController.GetCurrentMode().Contains("生产") || mController.GetCurrentMode().Contains("空跑"))) //启用hive，且生产模式
            {
                Commands[0].SetEnabled(false);
                mController.FileConfig.IsHiveContinueMaintenanceVisible = true;
                useHiveDialog = true;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _dialogService.HiveRunPopUp(SystemOperation.Start, (r) =>
                    {
                        useHiveDialog = false;
                        Commands[0].ChangeButton(new StatusChanged(mController.MachineStatus, mController.MachineStatus));
                        _dbManager.AddSysOperation(SystemOperation.Start, "", commonBus.CurrentUser?.UserName);
                        // 切回 UI 线程再置空
                        QrSource = _transparentQr;          // 清掉二维码 或者 new BitmapImage() 放一张透明图
                    });
                });

            }
            else
            {
                //不启用hive
                if (mController.Start())
                {
                    Commands[0].ChangeButton(new StatusChanged(mController.MachineStatus, mController.MachineStatus));
                    _dbManager.AddSysOperation(SystemOperation.Start, "", commonBus.CurrentUser?.UserName);
                }
            }
        }

        /// <summary>
        /// 启动认证程序
        /// </summary>
        private void StartFinExe()
        {
            string exe = "FinSensor.exe";
            string exePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools", "Finger", exe);
            if (!File.Exists(exePath))
            {
                return;
            }

            var process = System.Diagnostics.Process.GetProcessesByName("FinSensor");
            if (process.Count() == 0)
            {
                var startInfo = new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = exePath
                };

                var p = System.Diagnostics.Process.Start(startInfo);
            }
            else
            {
                ShowFinSensor(true);
            }
        }

        private static CancellationTokenSource token = null;
        private static bool SensorIsOK = false;
        private static int enterNum = 0;

        private void FingerTest(string region, Action<bool> navagateTo)
        {

            if (region != "Flow" && region != "Configure")
            {
                navagateTo(true);
                return;
            }

            if (GetByConfiguration<bool>("FPSensor", out var auth) && auth)
            {
                try
                {
                    if (enterNum > 0) return;
                    Interlocked.Increment(ref enterNum);

                    StartFinExe();

                    token?.Cancel();
                    Thread.Sleep(100);
                    token = new CancellationTokenSource();

                    // 等待成功的通知
                    Task.Run(() =>
                    {
                        while (true)
                        {
                            if (token != null && token.IsCancellationRequested)
                            {
                                Interlocked.Decrement(ref enterNum);
                                ShowFinSensor(false);
                                navagateTo(SensorIsOK);
                                break;
                            }

                            // 如果10s超时，那么直接退出
                            //if (time > 10000)
                            //{
                            //    ShowFinSensor(false);
                            //    navagateTo(false);
                            //    break;
                            //}
                            for (int i = 0; i < 500; i += 50)
                            {
                                if (token != null && token.IsCancellationRequested)
                                {
                                    break;
                                }

                                Thread.Sleep(50);
                            }
                        }
                    }, token.Token);
                }
                catch (Exception ex)
                {
                    commonBus.OnLog(Common.DataStruct.Enums.LogType.Error, $"指纹报错:{ex.Message}");
                }
            }
            else
            {
                navagateTo(true);
            }
        }

        private bool GetByConfiguration<T>(string name, out T value)
        {
            value = default(T);
            bool isContain = ConfigurationManager.AppSettings.AllKeys.Contains(name);

            if (!isContain)
            {
                return false;
            }

            bool isResult = false;
            string strValue = ConfigurationManager.AppSettings[name];
            try
            {
                value = (T)strValue.ConvertToType(typeof(T));
                isResult = true;
            }
            catch (Exception e)
            {
                commonBus.OnLog(LogType.Error, $"字段:{name} 转换类型:{typeof(T).Name} 失败!");
            }

            return isResult;
        }


        private string _startContent;
        public string StartContent
        {
            get { return _startContent; }
            set { SetProperty(ref _startContent, value); }
        }

        /// <summary>
        /// 登录ISCheck
        /// </summary>
        private bool _isAlarm;
        public bool IsAlarm
        {
            get { return _isAlarm; }
            set { SetProperty(ref _isAlarm, value); }
        }

        /// <summary>
        /// 启动状态按钮不可点击
        /// </summary>
        private bool _btnEnable = true;
        public bool BtnEnable
        {
            get { return _btnEnable; }
            set { SetProperty(ref _btnEnable, value); }
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
        /// 按钮是否可以点击
        /// </summary>
        private bool _btnClick = true;
        public bool BtnClick
        {
            get { return _btnClick; }
            set { SetProperty(ref _btnClick, value); }
        }

        private System.Windows.Media.Brush _backColor;
        public System.Windows.Media.Brush BackColor
        {
            get { return _backColor; }
            set { SetProperty(ref _backColor, value); }
        }

        private System.Windows.Media.Brush _foreColor = System.Windows.Media.Brushes.White;
        public System.Windows.Media.Brush ForeColor
        {
            get { return _foreColor; }
            set { SetProperty(ref _foreColor, value); }
        }

        /// <summary>
        /// 工具栏背景色
        /// </summary>
        private System.Windows.Media.Brush _statusBrush = FlowItem.NonAreaBrush;
        public System.Windows.Media.Brush StatusBrush
        {
            get { return _statusBrush; }
            set { SetProperty(ref _statusBrush, value); }
        }

        /// <summary>
        /// 工具栏按钮页背景色
        /// </summary>
        private System.Windows.Media.Brush _pageBtnBrush;
        public System.Windows.Media.Brush PageBtnBrush
        {
            get { return _pageBtnBrush; }
            set { SetProperty(ref _pageBtnBrush, value); }
        }

        /// <summary>
        /// 机台信息
        /// </summary>
        private string _title;
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        /// <summary>
        /// StationId
        /// </summary>
        private string _stationId;
        public string StationId
        {
            get { return _stationId; }
            set { SetProperty(ref _stationId, value); }
        }

        /// <summary>
        /// 机台状态
        /// </summary>
        protected string _mStatus = EngineStatus.Idle.GetDescription();
        public string MStatus
        {
            get { return _mStatus; }
            set
            {
                SetProperty(ref _mStatus, value);
                // 状态切为运行中，需重置定时器
                if (MStatus == EngineStatus.Running.GetDescription())
                {
                    _timer?.Stop();
                    _timer?.Start();
                }
            }
        }

        /// <summary>
        /// 机台状态
        /// </summary>
        private string _lXStatus = "Idle";
        public string LXStatus
        {
            get { return _lXStatus; }
            set { SetProperty(ref _lXStatus, value); }
        }

        /// <summary>
        /// 登录ISCheck
        /// </summary>
        private bool _loginCheck = true;
        public bool LoginCheck
        {
            get { return _loginCheck; }
            set { SetProperty(ref _loginCheck, value); }
        }

        /// <summary>
        /// 登录ISEnable
        /// </summary>
        private bool _loginEnable = true;
        public bool LoginEnable
        {
            get { return _loginEnable; }
            set { SetProperty(ref _loginEnable, value); }
        }

        /// <summary>
        /// 标题是否显示
        /// </summary>
        private bool _titleVisible = false;
        public bool TitleVisible
        {
            get { return _titleVisible; }
            set { SetProperty(ref _titleVisible, value); }
        }


        /// <summary>
        /// 用户名称
        /// </summary>
        private string _userName;
        public string UserName
        {
            get => _userName;
            set { SetProperty(ref _userName, value); }
        }

        private string _userMsg;
        public string UserMsg
        {
            get => _userMsg;
            set { SetProperty(ref _userMsg, value); }
        }

        private string _remainT;
        public string RemainT
        {
            get => _remainT;
            set { SetProperty(ref _remainT, value); }
        }

        /// <summary>
        /// 当前的运行模式
        /// </summary>
        private string _currentMode = "None";
        public string CurrentMode
        {
            get => _currentMode;
            set { SetProperty(ref _currentMode, value); }
        }

        /// <summary>
        /// 工程名称
        /// </summary>
        private string _projName;
        public string ProjName
        {
            get => _projName;
            set { SetProperty(ref _projName, value); }
        }


        private int _remainWeekMaintenanceDays;
        /// <summary>
        /// 剩余周维护天数
        /// </summary>
        public int RemainWeekMaintenanceDays
        {
            get => _remainWeekMaintenanceDays;
            set
            {
                if (value < -360) value = -360;
                SetProperty(ref _remainWeekMaintenanceDays, value);
            }
        }

        private int _remainMonthMaintenanceDays;
        /// <summary>
        /// 剩余月维护天数
        /// </summary>
        public int RemainMonthMaintenanceDays
        {
            get => _remainMonthMaintenanceDays;
            set
            {
                if (value < -360) value = -360;
                SetProperty(ref _remainMonthMaintenanceDays, value);
            }
        }


        /// <summary>
        /// 周维护颜色
        /// </summary>
        private System.Windows.Media.Brush _weekMaintenanceColor;

        public System.Windows.Media.Brush WeekMaintenanceColor
        {
            get => _weekMaintenanceColor;
            set => SetProperty(ref _weekMaintenanceColor, value);
        }

        /// <summary>
        /// 月维护颜色
        /// </summary>
        private System.Windows.Media.Brush _monthMaintenanceColor;
        public System.Windows.Media.Brush MonthMaintenanceColor
        {
            get => _monthMaintenanceColor;
            set => SetProperty(ref _monthMaintenanceColor, value);
        }


        /// <summary>
        /// 命令按钮
        /// </summary>
        private ObservableCollection<CommandModel> _commands;
        public ObservableCollection<CommandModel> Commands
        {
            get { return _commands; }
            set { SetProperty(ref _commands, value); }
        }

        /// <summary>
        /// 菜单信息
        /// </summary>
        private ObservableCollection<PageModel> _pages;
        public ObservableCollection<PageModel> Pages
        {
            get { return _pages; }
            set { SetProperty(ref _pages, value); }
        }

        /// <summary>
        /// 运行模式集合
        /// </summary>
        private ObservableCollection<FeatureModel> _runModes;
        public ObservableCollection<FeatureModel> RunModes
        {
            get { return _runModes; }
            set { SetProperty(ref _runModes, value); }
        }
        private string _role = "NoLogin";
        public string Role
        {

            get { return _role; }
            set { SetProperty(ref _role, value); }
        }

        private System.Windows.Media.Brush _roleColor = new SolidColorBrush(System.Windows.Media.Color.FromRgb(204, 204, 204));
        public System.Windows.Media.Brush RoleColor
        {
            get { return _roleColor; }
            set { SetProperty(ref _roleColor, value); }
        }

        private bool useHiveDialog = false;
        // 界面QR码
        private ImageSource _qrSource;
        public ImageSource QrSource
        {
            get { return _qrSource; }
            set { SetProperty(ref _qrSource, value); }
        }


        /// <summary>
        /// 登录
        /// </summary>
        private DelegateCommand<string> _loginCommand;
        public DelegateCommand<string> LoginCommand => _loginCommand ?? (_loginCommand = new DelegateCommand<string>((strRegion) =>
        {
            Login(strRegion);

            //DialogParameters param = new DialogParameters();
            //param.Add("Title", "Login");
            //_dialogService.Show("ChangeUserDialog", param, null);

        }));

        protected virtual void Login(string strRegion)
        {
            LoginCheck = true;
            foreach (var pageItem in Pages)
            {
                pageItem.page_IsSelected = false;
            }

            commonBus.OnNavigate(new PageModel() { Region = strRegion });

        }

        /// <summary>
        /// 登录
        /// </summary>
        private DelegateCommand _clearAlarmCommand;
        public DelegateCommand ClearAlarmCommand => _clearAlarmCommand ?? (_clearAlarmCommand = new DelegateCommand(() =>
        {
            AlarmInfos?.Clear();
        }));

        /// <summary>
        /// 更改模式命令
        /// </summary>
        private DelegateCommand<object> _changeModeCommand;
        public DelegateCommand<object> ChangeModeCommand => _changeModeCommand ?? (_changeModeCommand = new DelegateCommand<object>((obj) =>
        {
            var rButton = obj as RadioButton;
            if (rButton != null)
            {
                // 关闭popup
                var grid = CommonHelper.VisualUpwardSearch<Grid>(rButton) as Grid;
                if (grid != null && grid.Parent is Popup popup)
                {
                    popup.IsOpen = false;
                }


                // 设置模式 
                string txtMode = rButton.Content.ToString();
                mController.SetCurrentMode(txtMode);
                CurrentMode = txtMode;

                // 刷新
                InitModes(false);
                commonBus.EventBus.GetEvent<ProductModeEvent>().Publish(CurrentMode);

                var converter = new System.Windows.Media.BrushConverter();
                if (CurrentMode.Contains("生产"))
                //if (CurrentMode == "生产模式")
                {
                    PageBtnBrush = (System.Windows.Media.Brush)converter.ConvertFromString("#3A4249");
                    switch (mEngine.EngineStatus)
                    {
                        case EngineStatus.Idle:
                            StatusBrush = FlowItem.NonAreaBrush;
                            LXStatus = "Idle";
                            break;
                        case EngineStatus.Homing:
                        case EngineStatus.Running:
                        case EngineStatus.Resetting:
                            StatusBrush = FlowItem.SuccessBrush;
                            LXStatus = "Running";
                            break;
                        case EngineStatus.Ready:
                            StatusBrush = FlowItem.RunningBrush;
                            LXStatus = "Idle";
                            break;
                        case EngineStatus.Alarm:
                            StatusBrush = FlowItem.FailBrush;
                            LXStatus = "DownTime";
                            break;
                        case EngineStatus.Pause:
                        case EngineStatus.Stop:
                            StatusBrush = FlowItem.TimeoutBrush;
                            LXStatus = "DownTime";
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    PageBtnBrush = (System.Windows.Media.Brush)converter.ConvertFromString("#e84b4b");
                    StatusBrush = (System.Windows.Media.Brush)converter.ConvertFromString("#d12323");
                }
            }
        }));

        /// <summary>
        /// 设置按钮状态
        /// </summary>
        /// <param name="isenable"></param>
        private void SetBtnStatus(StatusChanged obj)
        {
            //if (obj.Dst == EngineStatus.Running)
            //{
            //    Pages[0].SetEnable("Project", false);
            //    LoginEnable = false;
            //}
            if (obj.Dst == EngineStatus.Stop || obj.Dst == EngineStatus.Idle)
            {
                Pages[0].SetEnable("Project", true);
                LoginEnable = true;
            }
        }

        /// <summary>
        /// 关闭
        /// </summary>
        private DelegateCommand _closeCommand;
        public DelegateCommand CloseCommand => _closeCommand ?? (_closeCommand = new DelegateCommand(() =>
        {
            commonBus.EventBus.GetEvent<CloseEvent>().Publish();
        }));

        /// <summary>
        /// 最小化
        /// </summary>
        private DelegateCommand _minCommand;
        public DelegateCommand MinCommand => _minCommand ?? (_minCommand = new DelegateCommand(() =>
        {
            commonBus.EventBus.GetEvent<WinMinEvent>().Publish();
        }));

        /// <summary>
        /// 最大化
        /// </summary>
        private DelegateCommand _maxCommand;
        public DelegateCommand MaxCommand => _maxCommand ?? (_maxCommand = new DelegateCommand(() =>
        {
            commonBus.EventBus.GetEvent<WinMaxEvent>().Publish();
        }));


        /// <summary>
        /// 打开配方
        /// </summary>
        private DelegateCommand _openRecipeFolderCommand;
        public DelegateCommand OpenRecipeFolderCommand => _openRecipeFolderCommand ?? (_openRecipeFolderCommand = new DelegateCommand(() =>
        {
            if (commonBus.CurrentRecipe != null)
            {
                string recipepath = string.IsNullOrEmpty(mController.FileConfig.LogsSavePath) ?
                    commonBus.CurrentRecipe.GetRecipePath() : mController.FileConfig.LogsSavePath;
                if (recipepath != null && recipepath != "")
                {
                    System.Diagnostics.Process.Start("explorer.exe", recipepath);
                }
                else
                {
                    throw new FriendlyException($"{Luster.Motion.Assests.Langs.LangProvider.GetLang("NotFoundActiveRecipePath")}！");
                }
            }
            else
            {
                throw new FriendlyException($"{Luster.Motion.Assests.Langs.LangProvider.GetLang("ActiveRecipeNotFound")}！");
            }
        }));

        private DelegateCommand _openLogFolderCommand;
        public DelegateCommand OpenLogFolderCommand => _openLogFolderCommand ?? (_openLogFolderCommand = new DelegateCommand(() =>
        {
            if (commonBus.CurrentRecipe != null)
            {
                string logPath;
                if (string.IsNullOrEmpty(mController.FileConfig.LogsSavePath))
                {
                    logPath = commonBus.CurrentRecipe.GetRecipePath();
                    FileInfo fileInfo = new FileInfo(logPath);
                    logPath = fileInfo.DirectoryName + "_logs";
                }
                else
                {
                    logPath = mController.FileConfig.LogsSavePath;
                }

                if (logPath != null && logPath != "")
                {
                    System.Diagnostics.Process.Start("explorer.exe", logPath);
                }
                else
                {
                    throw new FriendlyException($"{Luster.Motion.Assests.Langs.LangProvider.GetLang("NotFoundActiveRecipePath")}！");
                }
            }
            else
            {
                throw new FriendlyException($"{Luster.Motion.Assests.Langs.LangProvider.GetLang("ActiveRecipeNotFound")}！");
            }
        }));

        #region 3D 页面
        private static readonly string Holo3D = "Holo3D";

        /// <summary>
        /// 添加3D
        /// </summary>
        public void AddHolo3D()
        {
            var holoPage = Pages.FirstOrDefault(u => u.Name == Holo3D);
            if (holoPage != null)
            {
                holoPage.page_IsVisible = true;
            }
            else
            {
                var holo3D = new PageModel()
                {
                    Name = Holo3D,
                    page_IsSelected = false,
                    Region = "Holo3DContent",
                    Iconfont = "\xe65a",
                    page_IsVisible = true,
                    page_IsEnabled = true
                };
                PageModel.Pages.Add(holo3D);

                Pages.Add(holo3D);
            }
        }
        #endregion

        #region 指纹认证通信相关
        [DllImport("User32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, IntPtr lParam);

        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        private const int OKMsg = 0x100A;
        private const int NGMsg = 0x200A;
        private const int HideMsg = 0x300A;
        private const int ShowMsg = 0x400A;

        private void ShowFinSensor(bool isShow = false)
        {
            IntPtr windowHandle = FindWindow(null, "指纹认证");
            if (windowHandle == IntPtr.Zero)
            {
                return;
            }

            SendMessage(windowHandle, isShow ? ShowMsg : HideMsg, 0, IntPtr.Zero);
        }
        #endregion

        private void UpdateRemainMaintenanceDays()
        {
            RemainWeekMaintenanceDays = (int)(mController.FileConfig.LastWeekMaintenanceDate.AddDays(7) - DateTime.Now).TotalDays;
            RemainMonthMaintenanceDays = (int)(mController.FileConfig.LastMonthMaintenanceDate.AddDays(30) - DateTime.Now).TotalDays;
            WeekMaintenanceColor = RemainWeekMaintenanceDays >= 0 ? GreenBrush : RedBrush;
            MonthMaintenanceColor = RemainMonthMaintenanceDays >= 0 ? GreenBrush : RedBrush;
        }
    }
}