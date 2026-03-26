#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       MainWindowVM
* 机器名称:       L05123-NB
* 命名空间:       LusterMotion.ViewModel
* 文 件 名:       MainWindowVM.cs
* 创建时间:       2022/6/28 8:48:50
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      d20203d5-b7e3-46c2-86f4-e182d645e932
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/28 8:48:50
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using DC.Authorization;
using DC.Authorization.WPF;
using LiveCharts.Dtos;
using Luster.Common.Assets;
using Luster.Common.DataAccess.Factory;
using Luster.Common.DataStruct;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.Tools;
using Luster.Control.Wpf.Motion.Flow;
using Luster.Motion.Assests.Langs;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.CommonUI.Models;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.CommonUI.ViewModel.Dialogs;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.EditorUI.Events;
using Luster.Motion.EditorUI.Extensions;
using Luster.Motion.Integration.SFC;
using Luster.Motion.Integration.Web;
using Luster.Motion.SubSystem.ViewModel;
using Luster.Motion.SubSystem.Views;
using Luster.Motion.SubSystem.Views.Login;
using Luster.Motion.TaskFlow.Engine;
using Luster.Motion.TaskFlow.Engine.HyperTrain;
using Luster.Motion.TaskFlow.Engine.Models;
using Luster.SimDevice.EngineUI.Models;
using Luster.TaskFlow.Common;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Enums;
using Luster.TaskFlow.Motion.Logic;
using Newtonsoft.Json.Linq;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using static FreeSql.Internal.GlobalFilter;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using PageModel = Luster.Motion.CommonUI.Models.PageModel;

namespace LusterMotion.ViewModel
{
    public class MainWindowVM : MotionVM
    {
        #region 字段
        // 页面区域切换
        private IRegionManager _regionManager;

        // 多线程
        private Dispatcher _dispatcher;

        // 数据库工厂
        private IDBFactory _dbFactory;

        // 流程引擎
        private IMotionEngine _motionEngine;

        // 对话框
        private IDialogService _dialogService;

        // 运控控制器
        private IMotionController _mController;

        // 数据访问工具
        private IDbManager _dbManager;

        // 设备引擎
        private IDeviceEngine _deviceEngine;

        /// <summary>
        /// 线程中断信号量
        /// </summary>
        private static CancellationTokenSource _cts = new CancellationTokenSource();

        private Window _mainwindow;


        private ProFXContentVM proFXContentVM;

        private HiveDialogVM hiveDialogVM;

        /// <summary>
        /// Web服务
        /// </summary>
        private readonly IWebService _webService;

        /// <summary>
        /// 会话超时管理器（来自 Luster.Authorization.Client）
        /// </summary>
        private readonly ISessionManager _sessionManager;
        private readonly ILoginService _loginService;
        private string CpkFilePath = "Config\\CPK_WIP.txt";


        #endregion

        public MainWindowVM(
            IAuthorizationFacade auth,
            ISessionManager sessionManager,
            ILoginService loginService,
            IModuleFactory factory,
            ICommonBus _commonBus,
            IRegionManager regionManager,
            IMotionEngine mEngine,
            IDBFactory dbFactory,
            Dispatcher Dispatcher,
            IDeviceEngine deviceEngine,
            IDialogService dialogService,
            IMotionController motionController,
            IDbManager dbManager,
            IWebService webService,
            ProFXContentVM _proFXContentVM,
            HiveDialogVM _hiveDialogVM) : base(_commonBus, auth)
        {
            _regionManager = regionManager;
            _dispatcher = Dispatcher;
            _motionEngine = mEngine;
            _mController = motionController;
            _webService = webService;
            mEngine.LanguageEvent += (key) => _commonBus.L(key);
            _dbFactory = dbFactory;
            _dialogService = dialogService;
            _dbManager = dbManager;
            _deviceEngine = deviceEngine;

            motionController.StartHomeEvent -= MotionController_StartHomeEvent;
            motionController.StartHomeEvent += MotionController_StartHomeEvent;

            mEngine.DialogBoxEvent -= mEngine_DialogBoxEvent;

            mEngine.DialogBoxEvent += mEngine_DialogBoxEvent;

            proFXContentVM = _proFXContentVM;
            hiveDialogVM = _hiveDialogVM;
            // 设备运行状态
            //deviceEngine.LoadingEvent -= DeviceEngine_LoadingEvent;
            //deviceEngine.LoadingEvent += DeviceEngine_LoadingEvent;

            ChangeToolBarBackColor();
            deviceEngine.ModeChangedEvent += (mode) =>
            {
                _mController.SysConfig.RunMode = mode;
                if (mode == DeviceMode.Real || mode == DeviceMode.Empty)
                {
                    // 检测急停信号有无启用
                    if (_mController.IsEmergency())
                    {
                        throw new FriendlyException("急停被按下!");
                    }

                    // 需要监控第一次回零按钮
                    _mController.StartButtonMontor();
                    _mController.OnNavigation();
                }
                else
                {
                    _mController.StopButtonMontor();
                }
            };

            _motionEngine.HomeFailEvent -= MotionEngine_HomeFailEvent;
            _motionEngine.HomeFailEvent += MotionEngine_HomeFailEvent;

            proFXContentVM.HiveRepairStartEvent -= ProFXContentVM_HiveRepairStartEvent;
            proFXContentVM.HiveRepairStartEvent += ProFXContentVM_HiveRepairStartEvent;
            hiveDialogVM.HiveRepairEndEvent -= HiveDialogVM_HiveRepairEndEvent;
            hiveDialogVM.HiveRepairEndEvent += HiveDialogVM_HiveRepairEndEvent;




            MontorInteraction();
            var converter = new System.Windows.Media.BrushConverter();
            MainBackColor = (Brush)converter.ConvertFromString("#f5f5f5");
            //开启定时器，1min/次,清理缓存
            SetTimer();
            auth.PopLoginWindowAction = () =>
            {
                _dispatcher.Invoke(() =>
                {
                    _dialogService.ShowDialog(nameof(DC.Authorization.WPF.Views.LoginView));
                });
            };
            _loginService = loginService;
            // 启动会话超时管理器，并订阅超时事件
            _sessionManager = sessionManager;
            //_sessionManager.Start();
            _sessionManager.SessionExpired += OnSessionExpired;

        }

        private InteractiveType mEngine_DialogBoxEvent(string content)
        {
            InteractiveType interactiveType = InteractiveType.Continue;
            _dispatcher.Invoke(() =>
            {
                _dialogService.ShowInteractiveDialog(content, r =>
                {
                    r.Parameters.TryGetValue<InteractiveType>("InteractiveType", out interactiveType);
                });
            });
            return interactiveType;
        }

        bool isFirstRun;
        // 变量定义，用于存放用户名
        private string userNameCur = "None";

        /// <summary>
        /// 会话超时回调：降级为 Operator、广播状态、安全居启用、跳回主页
        /// </summary>
        private void OnSessionExpired(object sender, EventArgs e)
        {
            var curUserModel = new UserModel()
            {
                Id = 3,
                UserName = SystemRole.Operator.ToString(),
                UserRole = SystemRole.Operator,
            };
            commonBus.CurrentUser = curUserModel;
            _dispatcher.Invoke(() =>
            {
                _loginService.Login("114516", "123456");//切换到op
                commonBus.OnUserLogin(curUserModel);
                commonBus.OnUserRoleChange(new Luster.Motion.Integration.WorkCardVerify.UserInfo());
                _mController.SysConfig.EnableSaftyDoor = true;
                _mController.SysConfig.EnableLightCurtain = true;
                _mController.StartButtonMontor();
                var homePage = PageModel.Pages.FirstOrDefault(u => u.Name == "Home");
                commonBus.OnNavigate(homePage);
            });
        }

        /// <summary>
        /// 界面监控
        /// </summary>
        private void MontorInteraction()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    if (_cts.IsCancellationRequested) break;

                    // 机台运行中，60s 无交互则跳回主页
                    if (_motionEngine.EngineStatus == EngineStatus.Running)
                    {
                        if ((DateTime.Now - mouseMoveTime).TotalSeconds > 60)
                        {
                            var homePage = PageModel.Pages.FirstOrDefault(u => u.Name == "Home");
                            commonBus.OnNavigate(homePage);
                        }
                    }

                    // 将 SessionManager 的倒计时广播到 commonBus
                    if (commonBus.CurrentUser != null)
                    {
                        commonBus.OnRemainTimeChange(_sessionManager.CountDown);
                    }

                    // 后台差居工站
                    if (_motionEngine.GetStations().FirstOrDefault(u =>
                               u.Status != RunStatus.Skip
                               && typeof(IBackGroundStation).IsAssignableFrom(u.TaskFunction.GetType())) != null)
                    {
                        _motionEngine.RunBackGroundStation();
                        isFirstRun = true;
                    }

                    await _mController.Update_SFC_ConnectStatus();
                    await _mController.Update_PDCA_ConnectStatus();
                    await _mController.Update_FFU_ConnectStatus();

                    Thread.Sleep(800);
                }
            }, _cts.Token);
        }



        private void HiveDialogVM_HiveRepairEndEvent(Brush obj)
        {
            MainBackColor = Brushes.Blue;
        }

        private void ProFXContentVM_HiveRepairStartEvent(Brush obj)
        {
            MainViewBrush = Brushes.Blue;
        }


        private void MotionEngine_HomeFailEvent(string errMsg)
        {
            _dispatcher.Invoke(() =>
            {
                _dialogService.ShowErrorTip($"回零失败:{errMsg}");
            }, DispatcherPriority.Normal);
        }

        protected override void RegisterEvent(IEventAggregator bus)
        {
            // 订阅用户信息事件
            bus.GetEvent<UserInfoEvent>().Subscribe(userInfo =>
            {
                userNameCur = userInfo.Name;
            });
            // 1.主页切换
            bus.GetEvent<PageNavEvent>().Subscribe(pageModel =>
            {
                //Task.Run(() =>
                //{
                // 1.加载开始
                _dispatcher.Invoke(() =>
                {
                    IsLoading = true;


                    // 判断是否为LoginContentFX且引擎正在运行
                    if ((pageModel.Region == "LoginContentFX" || pageModel.Region == "LoginContent") && _motionEngine.EngineStatus == EngineStatus.Running)
                    {
                        // 构造参数，传递 ProjectInable = false
                        var parameters = new NavigationParameters
                                 {
                                     { "ProjectEnable", false }
                                 };

                        _regionManager.RequestNavigate(
                            "MainRegion",
                            new Uri(pageModel.Region, UriKind.Relative), // Convert string to Uri
                            navResult => // Ensure the lambda expression is used for the navigation callback
                            {
                                pageModel.page_IsSelected = true;
                            },
                            parameters // Pass NavigationParameters correctly
                        );
                    }
                    else
                    {
                        var parameters = new NavigationParameters
                                 {
                                     { "ProjectEnable", true }
                                 };

                        _regionManager.RequestNavigate(
                            "MainRegion",
                            new Uri(pageModel.Region, UriKind.Relative), // Convert string to Uri
                            navResult => // Ensure the lambda expression is used for the navigation callback
                            {
                                pageModel.page_IsSelected = true;
                            },
                            parameters // Pass NavigationParameters correctly
                        );
                    }
                    //_regionManager.RequestNavigate("MainRegion", pageModel.Region, navResult =>
                    //{
                    //    pageModel.IsSelected = true;
                    //});

                    // 2.加载结束
                    IsLoading = false;
                });
                //});

            });

            // 2.日志
            //bus.GetEvent<LogInfoEvent>().Subscribe(logInfo =>
            //{

            //});

            // 3.配方变更
            bus.GetEvent<RecipeChangedEvent>().Subscribe(r =>
            {
                // 异步保存配方
                string dstFile = Path.Combine(r.GetSavePath(), $"{DateTime.Now.ToString("yy_MM_dd_HH_mm_ss")}.recipe");
                File.Copy(r.GetFullname(), dstFile, true);
            });

            // 4.打开配方
            bus.GetEvent<RecipeOpenEvent>().Subscribe(r =>
            {
                if (r == null) return;
                var recipeDir = r.GetRecipePath();

                if (!Directory.Exists(recipeDir))
                {
                    Directory.CreateDirectory(recipeDir);
                }

                //Assembly dataAss = Assembly.LoadFrom("Luster.Motion.CommonUI.dll");// 加载数据文件所在的程序集

                // 获取所有数据实体（应从 DataAccess 程序集同步，而不是 UI/CommonUI 程序集）
                var dataAss = Assembly.LoadFrom("Luster.Common.DataAccess.dll");
                var allTypes = dataAss.GetTypes()
                    .Where(x => !x.IsAbstract && !x.IsInterface && x.Name.StartsWith("Tb"))
                    .ToArray();
                if (allTypes.Length > 0)
                {
                    _dbFactory.DbInit(allTypes);
                }

                // AssTb 属于报表/点检类数据（来自 DigitalSetup 程序集）
                try
                {
                    var digitalAss = Assembly.LoadFrom("Luster.Motion.DigitalSetup.dll");
                    var allTypes_Ass = digitalAss.GetTypes()
                        .Where(x => !x.IsAbstract && !x.IsInterface && x.Name.StartsWith("AssTb"))
                        .ToArray();
                    if (allTypes_Ass.Length > 0)
                    {
                        _dbFactory.DbInit_Ass(allTypes_Ass);
                    }
                }
                catch
                {
                    // DigitalSetup.dll 可能未部署/未加载：忽略 AssTb 同步，不影响主流程
                }

                // 打开新配方后插入一条软件打开记录
                _dbManager.AddSysOperation(SystemOperation.SoftStart, commonBus.CurrentUser?.UserName);

                // 数据库完成连接后，通知首页取质量信息
                commonBus.EventBus.GetEvent<MapDataEvent>().Publish(null);


                // 切换模式
                //if (commonBus.CurrentUser.CurrentMode == DeviceMode.Real)
                //{
                commonBus.ChangeDeviceMode(commonBus.CurrentUser.CurrentMode);
                //}
                string cpkWipFilePath = recipeDir.Substring(0, recipeDir.LastIndexOf("\\"));
                cpkWipFilePath = Path.Combine(cpkWipFilePath, CpkFilePath);

                SFCHelper.SetCPKWIPFilePath(cpkWipFilePath);
                SFCHelper.SetConfig(_webService.GetConfig() as WebConfig);

                // 软件加载配方后，默认页面是可用的
                Luster.Common.Assets.Global.GlobalProperty.IsEnabeld = true;
            });

            // 5.报警事件
            /*bus.GetEvent<AlarmEvent>().Subscribe(r =>
            {
                _dispatcher.Invoke(() =>
                {
                    var module = r.Sender as IMotionModule;
                    string message = module == null ? r.Message : $"模块:{module.Alias} {r.Message}";

                    if (r.AlarmType == AlarmType.PopInfoTip||
                    r.AlarmType==AlarmType.DeviceError||r.AlarmType==AlarmType.Timeout||r.AlarmType==AlarmType.FailError||
                    r.AlarmType==AlarmType.PlcAlarm)
                    {
                        // 报警弹窗
                        _dialogService.ShowAlarm(r, res =>
                        {
                            // 1.记录开始时间
                            res.Parameters.TryGetValue<DateTime>("StartTime", out var startTime);                 

                            // 2.处理方式
                            if (res.Parameters.TryGetValue<Luster.Motion.DataStruct.Enums.AlarmProc>("AlarmProc", out var proc))
                            {
                                var curModule = r.Sender as IMotionModule;
                                switch (proc)
                                {
                                    //case Luster.Motion.DataStruct.Enums.AlarmProc.PlcClearError:
                                    case Luster.Motion.DataStruct.Enums.AlarmProc.Contine:

                                        // 忽略
                                        if (r.OnAlarmProc(proc))
                                        {
                                            if (r.AlarmType == AlarmType.InfoTip)
                                            {
                                                commonBus.OnLog(LogType.Info, $"信息提示:{r.Message}被忽略");
                                            }
                                            else
                                            {
                                                res.Parameters.TryGetValue<int>("Curve", out var Curve);

                                                var gModule = _motionEngine.Get(GlobalModule.GlobalID);
                                                if (gModule.Parameters.ContainsKey("Extend_料盘穴位索引"))
                                                {
                                                    var gItem = gModule.Parameters["Extend_料盘穴位索引"];
                                                    if (gItem.Type == typeof(int))
                                                    {
                                                        gItem.Value = Curve;
                                                    }
                                                }
                                                // 警告类的因为将流程暂停了，所以需要恢复流程
                                                _mController.Recovery();
                                            }
                                        }

                                        break;
                                    case Luster.Motion.DataStruct.Enums.AlarmProc.Repair:

                                        _mController.SetBuzzer(false);

                                        break;
                                    case Luster.Motion.DataStruct.Enums.AlarmProc.Stop:
                                    case Luster.Motion.DataStruct.Enums.AlarmProc.Check:

                                        // 设备检修,关掉当前的窗口，对设备进行调试，此时流程暂停
                                        if (r.OnAlarmProc(proc))
                                        {
                                            commonBus.OnLog(LogType.Info, $"弹窗报警人员点击软件停止");
                                            _mController.Stop();
                                        }

                                        break;

                                    case Luster.Motion.DataStruct.Enums.AlarmProc.Retry:
                                        {
                                            LoadingMsg = $"{r.Message} 尝试重新运行...";
                                            IsLoading = true;

                                            if (r.OnAlarmProc(proc))
                                            {
                                                // 重新运行当前模块
                                                _mController.Retry(curModule);
                                            }

                                            IsLoading = false;
                                            LoadingMsg = "";

                                            break;
                                        }
                                    case Luster.Motion.DataStruct.Enums.AlarmProc.Ng:
                                        // 产品NG处理
                                        Task.Run(() =>
                                        {
                                            LoadingMsg = $"{r.Message} 尝试运行NG处理模块...";
                                            IsLoading = true;
                                            r.OnAlarmProc(proc);

                                            // 回复
                                            _mController.Recovery();

                                            IsLoading = false;
                                            LoadingMsg = "";

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
            });*/

            // 6.异步加载进度条
            bus.GetEvent<LoadingEvent>().Subscribe(r =>
            {
                _dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
                {
                    if (r.Total > 0)
                    {
                        ProgressVal = (int)(r.CurValue * 100.0 / r.Total);
                        LoadingMsg = $"[{r.CurValue} / {r.Total}] {r.Message}";
                        if (r.Total == r.CurValue)
                        {
                            // 添加一个延时，用于进度显示100%
                            Thread.Sleep(200);
                            IsLoading = false;
                        }
                        else
                        {
                            IsLoading = true;
                        }

                        // 记录加载log
                        commonBus.OnLog(LogType.Debug, LoadingMsg);
                    }
                    else
                    {
                        ProgressVal = 55;
                        LoadingMsg = "程序运行中..";
                        IsLoading = true;
                        if (r.Total == r.CurValue)
                        {
                            LoadingMsg = "";
                            Thread.Sleep(200);
                            IsLoading = false;
                        }
                    }
                }));
            });

            // 7.关闭
            bus.GetEvent<CloseEvent>().Subscribe(() =>
            {
                _dispatcher.Invoke(() =>
                {
                    _mainwindow.Close();
                });
            });

            // 8.最小化

            bus.GetEvent<WinMinEvent>().Subscribe(() =>
            {
                _dispatcher.Invoke(() =>
                {
                    _mainwindow.WindowState = WindowState.Minimized;
                });
            });

            // 9.最大化
            bus.GetEvent<WinMaxEvent>().Subscribe(() =>
            {
                _dispatcher.Invoke(() =>
                {
                    if (_mainwindow.WindowState == WindowState.Maximized)
                    {
                        _mainwindow.WindowState = WindowState.Normal;
                    }
                    else
                    {
                        _mainwindow.WindowState = WindowState.Maximized;
                    }

                });
            });

            // 10.多语言
            bus.GetEvent<LangChangedEvent>().Subscribe((langName) =>
            {
                var cultureInfo = new CultureInfo(langName);
                // 更新切换后的语言
                Luster.Common.Assets.Langs.LangProvider.Culture = cultureInfo;
                Luster.Motion.Assests.Langs.LangProvider.Culture = cultureInfo;
                Luster.SimDevice.SubSystem.Langs.LangProvider.Culture = cultureInfo;
                AppConfig.Lang = langName;
                System.Globalization.CultureInfo.CurrentCulture = cultureInfo;
                HandyControl.Tools.ConfigHelper.Instance.SetLang(langName);

                commonBus.EventBus.GetEvent<LangChangedDoneEvent>().Publish();
            });

            //生产模式切换
            bus.GetEvent<ProductModeEvent>().Subscribe(mode =>
            {
                if (mode == null) return;
                var converter = new System.Windows.Media.BrushConverter();
                if (mode.Contains("生产"))
                //if (mode == "生产模式")
                {
                    MainBackColor = (Brush)converter.ConvertFromString("#f5f5f5");
                    ToolBarBackColor = (Brush)converter.ConvertFromString("#17202D");
                }
                else
                {
                    MainBackColor = (Brush)converter.ConvertFromString("#ff0000");
                    ToolBarBackColor = (Brush)converter.ConvertFromString("#ff0000");
                }
            });

            //Hive 维修切换背景颜色
            bus.GetEvent<HiveRepairEvent>().Subscribe(isStart =>
            {
                if (isStart)
                {

                    MainBackColor = Brushes.Blue;
                    ToolBarBackColor = Brushes.Blue;

                }
                else
                {
                    var converter = new System.Windows.Media.BrushConverter();
                    MainBackColor = (Brush)converter.ConvertFromString("#f5f5f5");
                    ToolBarBackColor = (Brush)converter.ConvertFromString("#17202D"); ;

                }
            });

            bus.GetEvent<SensorEvent>().Subscribe((ms) =>
            {
                SensorIsOK = ms;
                token?.Cancel();
            });

        }

        /// <summary>
        /// 修改工具栏背景色
        /// </summary>
        //非生产模式下背景为红色
        private void ChangeToolBarBackColor()
        {
            var converter = new System.Windows.Media.BrushConverter();
            if (ConfigurationManager.AppSettings.AllKeys.Contains("UITemplate"))
            {
                var keyvalue = ConfigurationManager.AppSettings["UITemplate"];
                if (keyvalue == "LuXshare" || keyvalue == "FX")
                {
                    ToolBarBackColor = (Brush)converter.ConvertFromString("#ffffff");
                    BtnToolColor = (Brush)converter.ConvertFromString("#000000");
                }
                else
                {
                    ToolBarBackColor = (Brush)converter.ConvertFromString("#17202D");
                    BtnToolColor = (Brush)converter.ConvertFromString("#ffffff");
                }
            }
        }

        /// <summary>
        /// 回零交互确认
        /// </summary>
        /// <returns></returns>
        private HomeType MotionController_StartHomeEvent()
        {
            HomeType homeType = HomeType.Cancel;

            _dispatcher.Invoke(() =>
            {
                _dialogService.ShowHomeDialog(r =>
                {
                    r.Parameters.TryGetValue<HomeType>("HomeType", out homeType);
                });
            });

            return homeType;
        }

        /// <summary>
        /// 是否加载中
        /// </summary>
        private bool _isLoading = false;
        public bool IsLoading
        {
            get { return _isLoading; }
            set { SetProperty(ref _isLoading, value); }
        }

        /// <summary>
        /// 信息加载
        /// </summary>
        private string _loadingMsg;
        public string LoadingMsg
        {
            get { return _loadingMsg; }
            set { SetProperty(ref _loadingMsg, value); }
        }

        /// <summary>
        /// 当前进度
        /// </summary>
        private int _progressVal;
        public int ProgressVal
        {
            get { return _progressVal; }
            set { SetProperty(ref _progressVal, value); }
        }

        /// <summary>
        /// ToolBar背景色
        /// </summary>
        private Brush _toolBarBackColor;
        public Brush ToolBarBackColor
        {
            get { return _toolBarBackColor; }
            set { SetProperty(ref _toolBarBackColor, value); }
        }

        /// <summary>
        /// 背景色
        /// </summary>
        private Brush _mainBackColor;
        public Brush MainBackColor
        {
            get { return _mainBackColor; }
            set { SetProperty(ref _mainBackColor, value); }
        }

        /// <summary>
        /// ToolBar关闭等按钮颜色
        /// </summary>
        private Brush _btnToolColor;
        public Brush BtnToolColor
        {
            get { return _btnToolColor; }
            set { SetProperty(ref _btnToolColor, value); }
        }


        /// <summary>
        /// ToolBar关闭等按钮颜色
        /// </summary>
        private Brush _mainViewBrush = Brushes.WhiteSmoke;
        public Brush MainViewBrush
        {
            get { return _mainViewBrush; }
            set { SetProperty(ref _mainViewBrush, value); }
        }

        /// <summary>
        /// 程序默认加载
        /// </summary>
        private DelegateCommand<object> _loadedCommand;
        public DelegateCommand<object> LoadedCommand => _loadedCommand ?? (_loadedCommand = new DelegateCommand<object>((o) =>
        {
            MainWindow mainWin = o as MainWindow;
            if (mainWin != null && mainWin.Tag is string str && !string.IsNullOrEmpty(str))
            {
                // 加载配方
            }

            // 默认项目功能
            //_regionManager.RequestNavigate("MainRegion", "ProjectContent");
            if (ConfigurationManager.AppSettings.AllKeys.Contains("StartContent"))
            {
                var keyvalue = ConfigurationManager.AppSettings["StartContent"];
                if (keyvalue == "")
                {
                    keyvalue = "LoginContent";
                }
                _regionManager.RequestNavigate("MainRegion", keyvalue);
            }
            _mainwindow = o as Window;
        }));

        /// <summary>
        /// 程序默认卸载
        /// </summary>
        private DelegateCommand<object> _closingCommand;
        public DelegateCommand<object> ClosingCommand => _closingCommand ?? (_closingCommand = new DelegateCommand<object>((obj) =>
        {

            CancelEventArgs args = obj as CancelEventArgs;

            if (_mController.MachineStatus == EngineStatus.Running || _mController.MachineStatus == EngineStatus.Pause)
            {
                try
                {
                    _dialogService.ShowErrorTip($"{Luster.Motion.Assests.Langs.LangProvider.GetLang("ProgramMustStop")}!");
                }
                catch (Exception ex) { }
                args.Cancel = true;
                return;
            }

            if (_deviceEngine.IsNeedSave || commonBus.IsNeedSave)
            {
                _dialogService.ShowConfirmWithoutNo($"{Luster.Motion.Assests.Langs.LangProvider.GetLang("SaveTask")}？", (result) =>
                {
                    if (result.Result == ButtonResult.OK)
                    {
                        // 保存关闭监控程序
                        _cts?.Cancel();
                        ClosingSplash();
                    }
                    else if (result.Result == ButtonResult.Cancel || result.Result == ButtonResult.None)
                    {
                        args.Cancel = true;
                        return;
                    }
                    else
                    {
                        // 不保存关闭
                        _cts?.Cancel();
                    }

                    if (commonBus.CurrentRecipe != null)
                    {
                        // 插入一条软件关闭记录
                        _dbManager.AddSysOperation(Luster.Motion.DataStruct.Enums.SystemOperation.SoftClose, commonBus.CurrentUser?.UserName);
                    }

                    _dbFactory.DbClose();
                });
            }
        }));

        /// <summary>
        /// 关闭Splash
        /// </summary>
        private void ClosingSplash()
        {
            SplashWindow splashWindow = new SplashWindow();

            Thread t = new Thread(() =>
            {
                splashWindow.Unloading();

                // 等500 ms
                Thread.Sleep(500);

                // 程序运行完成动画窗口关闭
                splashWindow.Dispatcher.Invoke(() =>
                {
                    splashWindow.Close();
                });
            });

            t.IsBackground = true;
            t.SetApartmentState(ApartmentState.STA);//设置单线程，在该模式下才能显示出来
            t.Start();
            splashWindow.ShowDialog();
        }

        protected override void BindGlobalCommnads()
        {
            GlobalCommands.SaveCommand.RegisterCommand(SaveCommand);
        }

        /// <summary>
        /// 模块保存
        /// </summary>
        private DelegateCommand _saveCommand;
        public DelegateCommand SaveCommand => _saveCommand ?? (_saveCommand = new DelegateCommand(() =>
        {
            //异步执行，解决点击保存，设备会卡顿的现象
            FingerTest((isOK) =>
            {
                if (isOK)
                {
                    _dispatcher.Invoke(() =>
                    {
                        Save();
                    });
                }
            });
        }));

        /// <summary>
        /// 用于计时，超过一定时间，自动切换到主页
        /// </summary>
        private DateTime mouseMoveTime = DateTime.Now;

        /// <summary>
        /// 程序默认加载
        /// </summary>
        private DelegateCommand<object> _mouseMoveCommand;
        public DelegateCommand<object> MouseMoveCommand => _mouseMoveCommand ?? (_mouseMoveCommand = new DelegateCommand<object>((obj) =>
        {
            mouseMoveTime = DateTime.Now;
        }));

        /// <summary>
        /// 定时器，1min/次
        /// </summary>
        private static void SetTimer()
        {
            System.Timers.Timer aTimer = new System.Timers.Timer(); //初始化定时器
            aTimer.Interval = 60000;//配置时间1分钟
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.AutoReset = true;//每到指定时间Elapsed事件是到时间就触发
            aTimer.Enabled = true; //指示 Timer 是否应引发 Elapsed 事件。
        }

        [DllImport("psapi.dll")]
        static extern int EmptyWorkingSet(IntPtr hwProc); //清理内存相关

        private static readonly object _memoryTrimLock = new object();
        private static DateTime _lastTrimTime = DateTime.MinValue;

        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            // 之前逻辑：每分钟强制 GC + 遍历系统所有进程 EmptyWorkingSet。
            // 这会产生非常高的 CPU 和系统调用开销，并可能导致 UI 卡顿。
            // 这里改为：
            // 1) 默认不强制 GC（让 CLR 自己决定）;
            // 2) 默认只对当前进程做 EmptyWorkingSet;
            // 3) 增加节流（避免重入/并发）与可配置开关。

            lock (_memoryTrimLock)
            {
                if ((DateTime.Now - _lastTrimTime).TotalSeconds < 55)
                    return;
                _lastTrimTime = DateTime.Now;
            }

            bool enableTrim = false;
            bool forceGc = false;
            bool trimAllProcesses = false;

            try
            {
                // 通过 App.config 可控制：
                // <add key="EnableMemoryTrim" value="true" />
                // <add key="ForceGcOnTrim" value="true" />
                // <add key="TrimAllProcesses" value="true" />
                if (ConfigurationManager.AppSettings.AllKeys.Contains("EnableMemoryTrim"))
                    bool.TryParse(ConfigurationManager.AppSettings["EnableMemoryTrim"], out enableTrim);
                if (ConfigurationManager.AppSettings.AllKeys.Contains("ForceGcOnTrim"))
                    bool.TryParse(ConfigurationManager.AppSettings["ForceGcOnTrim"], out forceGc);
                if (ConfigurationManager.AppSettings.AllKeys.Contains("TrimAllProcesses"))
                    bool.TryParse(ConfigurationManager.AppSettings["TrimAllProcesses"], out trimAllProcesses);
            }
            catch
            {
                // ignore
            }

            if (!enableTrim)
                return;

            try
            {
                if (forceGc)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }

                if (trimAllProcesses)
                {
                    Process[] processes = Process.GetProcesses();
                    foreach (Process process in processes)
                    {
                        try
                        {
                            // 系统进程/无权限进程会抛异常，直接忽略
                            EmptyWorkingSet(process.Handle);
                        }
                        catch
                        {
                        }
                        finally
                        {
                            try { process.Dispose(); } catch { }
                        }
                    }
                }
                else
                {
                    using (var cur = Process.GetCurrentProcess())
                    {
                        try { EmptyWorkingSet(cur.Handle); } catch { }
                    }
                }
            }
            catch
            {
                // ignore
            }
        }


        private static CancellationTokenSource token = null;
        private static bool SensorIsOK = false;
        private static int enterNum = 0;


        private void FingerTest(Action<bool> saveAction)
        {


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
                                saveAction(SensorIsOK);
                                break;
                            }

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
                    commonBus.OnLog(LogType.Error, $"指纹报错:{ex.Message}");
                }
            }
            else
            {
                saveAction(true);
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


        /// <summary>
        /// 保存
        /// </summary>
        private void Save()
        {
            var taskWaitForm = new TaskWaitWindow();

            taskWaitForm.SetTask(new Task(() =>
            {
                commonBus.OnSaveRecipe();
                taskWaitForm.UpdateProgress(10, 100);
                commonBus.OnSaveDevice();
                taskWaitForm.UpdateProgress(30, 100);
                commonBus.OnSaveSystem();
                taskWaitForm.UpdateProgress(50, 100);
                commonBus.OnSaveError();
                taskWaitForm.UpdateProgress(70, 100);
                commonBus.OnBackUpRecipe(true);
                taskWaitForm.UpdateProgress(90, 100);
                commonBus.SaveSolution();
                taskWaitForm.UpdateProgress(100, 100);
            }));

            taskWaitForm.ShowDialog();
        }

    }
}