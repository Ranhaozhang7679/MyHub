using Luster.Common.DataStruct;
using Luster.Common.Dongle;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.EditorUI.Extensions;
using Luster.Motion.Integration.SFC;
using Luster.Motion.Integration.Web;
using Luster.Motion.TaskFlow.Engine;
using Luster.TaskFlow.Motion.Logic;
using Prism.Commands;
using Prism.Events;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Luster.Motion.CommonUI.ViewModel.Dialogs
{
    public class HiveAlarmDialogVM : MotionDialogVM
    {
        private bool CardEnd = false;
        private Dispatcher dispatcher;
        /// <summary>
        /// 倒计时
        /// </summary>
        private string _countDown;
        public string CountDown
        {
            get => _countDown;
            set => SetProperty(ref _countDown, value);
        }

        /// <summary>
        /// 确认按钮是否可点击
        /// </summary>
        private bool _isButtonEnable;
        public bool IsButtonEnable
        {
            get => _isButtonEnable;
            set => SetProperty(ref _isButtonEnable, value);
        }

        /// <summary>
        /// 故障开始时间
        /// </summary>
        private string _errorStartTime;
        public string ErrorStartTime
        {
            get => _errorStartTime;
            set => SetProperty(ref _errorStartTime, value);
        }


        /// <summary>
        /// 故障代码
        /// </summary>
        private string _errorCode;
        public string ErrorCode
        {
            get => _errorCode;
            set => SetProperty(ref _errorCode, value);
        }


        /// <summary>
        /// 故障信息-英文
        /// </summary>
        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        /// <summary>
        /// 故障描述-中文
        /// </summary>
        private string _errorDetail;
        public string ErrorDetail
        {
            get => _errorDetail;
            set => SetProperty(ref _errorDetail, value);
        }


        /// <summary>
        /// 刷卡背景色        /// </summary>
        private Brush _cardBkColor;
        public Brush CardBkColor
        {
            get { return _cardBkColor; }
            set { SetProperty(ref _cardBkColor, value); }
        }
        /// <summary>
        /// 刷卡提示
        /// </summary>
        private string _cardLog;
        public string CardLog
        {
            get { return _cardLog; }
            set { SetProperty(ref _cardLog, value); }
        }

        /// <summary>
        /// 刷卡
        /// </summary>
        private DelegateCommand<object> _scanCommand;

        private string cardID = "";


        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        DateTime beginTime;
        private AlarmInfo alarmInfo;
        private IDialogService _dialogService;
        private IMotionController _motionController;
        private SFCHelper sfcHelper;
        private HiveAPI hiveAPI;
        private readonly ICommonBus _bus;

        /// <summary>
        /// 设备引擎
        /// </summary>
        private IDeviceEngine _deviceEngine;

        bool isHome = false;
        // 监控HiveReport窗口是否已打开
        private bool _greenIsOpen = false;
        // Help Request是否已发送
        private bool _helpRequested = false;

        private SubscriptionToken _alarmClosedToken;

        public HiveAlarmDialogVM(IMotionController motionController, IDeviceEngine deviceEngine, HiveAPI _hiveApi, IDialogService dialogService, VisionAPI _visionAPI, ICommonBus commonBus, SFCHelper sFCHelper)
        {
            _dialogService = dialogService;
            _motionController = motionController;
            sfcHelper = sFCHelper;
            hiveAPI = _hiveApi;
            this.IsButtonEnable = true;
            _bus = commonBus;
            _deviceEngine = deviceEngine;

            _alarmClosedToken = _bus.EventBus.GetEvent<HiveReportStateChangedEvent>().Subscribe(isOpen =>
            {
                _greenIsOpen = isOpen;
                if (!isOpen && _helpRequested)   // ② 绿窗刚关，且之前延迟过
                {
                    IsButtonEnable = !_greenIsOpen && (beginTime - DateTime.Now).TotalSeconds > 0;// 现在补执行
                    _helpRequested = false;
                    hiveAPI.HelpRequest(ErrorMessage, ErrorCode);

                }
            });

        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            stopwatch.Start();
            //判断设备是否回零,如果回零才能执行暂停
            try
            {
                isHome = _deviceEngine.IsHome(out var errmsg);
            }
            catch
            (Exception ex)
            {

            }

            // 1.报警消息
            if (parameters.TryGetValue<AlarmInfo>("AlarmInfo", out alarmInfo) && alarmInfo != null)
            {
                ErrorStartTime = alarmInfo.StartTime.ToString();
                if (string.IsNullOrEmpty(alarmInfo.AlarmCode))
                    alarmInfo.AlarmCode = "S12ABC99-01@alarm";
                string[] arrAlarm = alarmInfo.AlarmCode.Split('@');
                ErrorCode = arrAlarm[0];
                ErrorMessage = arrAlarm.Length > 1 ? arrAlarm[1] : "Recipe Config Error,please recheck";
                ErrorDetail = alarmInfo.Message;
            }
            // 警告提示也需要转入Hive报警，因为会造成机台切为“报警中”，进而导致停机
            if (alarmInfo != null && alarmInfo.AlarmType == DataStruct.Enums.AlarmType.InfoTip) // WarningTip
            {
                //如果是信息提示（InfoTip），则不需要转入Hive报警。
            }
            else
            {
                //beginTime = DateTime.Now.AddMinutes(5);
                //dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
                //dispatcherTimer.Tick += Timer_Tick;
                //_motionController.FileConfig.HiveHelpRequestTime = DateTime.Now;

                //dispatcherTimer.Start();
                //IsButtonEnable = true;

                // 1.报警消息
                /*if (parameters.TryGetValue<AlarmInfo>("AlarmInfo", out alarmInfo) && alarmInfo != null)
                {
                    ErrorStartTime = alarmInfo.StartTime.ToString();
                    if (string.IsNullOrEmpty(alarmInfo.AlarmCode))
                        alarmInfo.AlarmCode = "S12ABC99-01@Track temperature alarm";
                    string[] arrAlarm = alarmInfo.AlarmCode.Split('@');
                    ErrorCode = arrAlarm[0];
                    ErrorMessage = arrAlarm.Length > 1 ? arrAlarm[1] : "Axis Error";
                    ErrorDetail = alarmInfo.Message;
                }*/
                /*if (_motionController.MotionEngine.EngineStatus == DataStruct.EngineStatus.Homing ||
                    _motionController.MotionEngine.EngineStatus == DataStruct.EngineStatus.Ready)
                {

                }
                else*/
                {
                    if (_motionController.WebService.GetConfig() is WebConfig webConfig)
                    {
                        if (webConfig != null && webConfig.HiveEnabled && (_motionController.GetCurrentMode().Contains("生产") || _motionController.GetCurrentMode().Contains("空跑")))
                        {
                            //目前先改成15min
                            //倒计时15min,未来要改成5min
                            beginTime = DateTime.Now.AddMinutes(5);
                            dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
                            dispatcherTimer.Tick += Timer_Tick;
                            _motionController.FileConfig.HiveHelpRequestTime = DateTime.Now;

                            dispatcherTimer.Start();

                            // 如果HiveReport窗口存在，暂不发送 Help Request，绿色窗口关闭后再发送
                            if (parameters.TryGetValue<bool>("HiveStart2_Opend", out _greenIsOpen) && _greenIsOpen)
                                _helpRequested = true; // 绿窗开着 → 先标记，不执行
                            else
                                hiveAPI.HelpRequest(ErrorMessage, ErrorCode);  // 绿窗没开 → 立即执行

                            IsButtonEnable = !_greenIsOpen;
                        }
                    }

                }
                //回零完成，才允许停止
                if (isHome)
                {
                    _motionController.Pause();
                }
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            CountDown = (beginTime - DateTime.Now).Minutes.ToString() + ":" + (beginTime - DateTime.Now).Seconds.ToString();
            if ((beginTime - DateTime.Now).TotalSeconds <= 0)
            {
                dispatcherTimer.Stop();
                IsButtonEnable = false;
            }
        }

        private string currentAuth = "";
        private string OneNum = "";
        private Stopwatch stopwatch = new Stopwatch();

        public DelegateCommand<object> ScanCommand => _scanCommand ?? (new DelegateCommand<object>((obj) =>
        {
            // 若绿色信息提交窗口已打开，则弹出提示并返回
            if (_greenIsOpen)
            {
                if (obj is KeyEventArgs ke) ke.Handled = true; // 吞掉 Enter，避免传给新弹窗

                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    _dialogService.ShowDialog(
                        "MessageDialog",
                        new DialogParameters
                        {
                            { "title", "提示" },
                            { "Message", "请先关闭其他Hive窗口" }
                        },
                        null
                    );
                }), DispatcherPriority.Background);

                return;
            }

            //接收刷卡响应消息
            var e = obj as KeyEventArgs;
            if (e != null)
            {
                if (e.Key == Key.Enter)
                {
                    //cardID = "1234567";
                    //var ret = true;
                    //var auth = "L3";

                    if (cardID.Length > 10 || cardID.Length < 6)
                    {
                        cardID = "";
                        return;
                    }
                    //进行报文上报
                    var currentID = cardID.Trim();
                    if (cardID.Substring(0, 1) == "0")
                        cardID = cardID.Substring(1, cardID.Length - 1);
                    var ret = sfcHelper.CheckCard(cardID, out string auth);
                    currentAuth = auth;
                    cardID = "";
                    //刷卡后，需要判断权限是否满足
                    //如果满足，需要弹窗2
                    //if (ret && (auth == "L1" || auth == "L2" || auth == "L3" || auth == "L6" || auth == "L7" || auth == "L8" || auth == "L9"))
                    // Hive 2.7要求，L7和L8权限不允许停机
                    if (ret && (auth == "L1" || auth == "L2" || auth == "L3" || auth == "L6" || auth == "L9"))
                    {

                        CardLog = $"ID:{currentID}" + Environment.NewLine +
                                  $"Badge info sent" + Environment.NewLine +
                                  $"MES info received" + Environment.NewLine +
                                  $"You may start repair";

                        this.IsButtonEnable = false;
                        CardBkColor = new SolidColorBrush(Color.FromRgb(141, 212, 113));
                        // Delay 3000
                        Task.Delay(0).ContinueWith(task =>
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                _bus.OnLog(Common.DataStruct.Enums.LogType.Info, $"刷卡信息响应成功，[Alarm]窗口即将关闭");
                                base.CloseDialog("ok");
                                _dialogService.ShowHiveStopDialog("停机", r =>
                                {
                                    //0720，取消每次刷卡后都停止，减少回零次数
                                    //_motionController.Stop();
                                });

                                hiveAPI.RepairStart(ErrorMessage, ErrorCode, auth);
                            });
                        });
                    }
                    else if (ret && auth == "L7")
                    {
                        //不做动作，等待按钮
                    }
                    else
                    {
                        CardLog = $"ID:{currentID}" + Environment.NewLine +
                                  $"Badge info sent" + Environment.NewLine +
                                  $"MES info received" + Environment.NewLine +
                                  $"Access denied";
                        CardBkColor = new SolidColorBrush(Color.FromRgb(225, 83, 88));
                    }

                }
                else
                {
                    //记录数据
                    if (e.Key.ToString().Contains('D'))
                    {
                        cardID += e.Key.ToString()[1];
                    }
                }
            }
        }));


        /// <summary>
        /// 确认按钮
        /// </summary>
        private DelegateCommand<object> _clickCommand;
        public DelegateCommand<object> ClickCommand => _clickCommand ?? (_clickCommand = new DelegateCommand<object>((o) =>
        {
            if (_greenIsOpen)
            {
                _dialogService.ShowDialog(
                    "MessageDialog",
                    new DialogParameters
                    {
                        { "title", "提示" },
                        { "message", "请先关闭其他Hive窗口" }
                    },
                    null
                );
                return;
            }
            //_motionController.Recovery();
            //Thread.Sleep(500);


            //if(_motionController.WebService.GetConfig() is WebConfig webConfig)
            //{
            //    if (webConfig != null && !webConfig.HiveEnabled)
            //    {
            //        _motionController.Stop();
            //        base.CloseDialog("ok");
            //        return;

            //    }
            //}

            //未回零完成
            if (!isHome || _motionController.IsHoming)
            {
                _bus.OnLog(Common.DataStruct.Enums.LogType.Info, $"设备未回零完成!!!");
                base.CloseDialog("ok");
                _motionController.Stop();
                return;
                throw new FriendlyException($"设备未回零完成！！！");
            }


            //未切到自动按钮
            if (!_motionController.CanAutoRun(out string errMsg))
            {
                _bus.OnLog(Common.DataStruct.Enums.LogType.Info, $"当前未切换至自动按钮下,不满足启动条件");
                return;
                throw new FriendlyException(errMsg);

            }

            HashSet<EngineStatus> hsa = new HashSet<EngineStatus>() { EngineStatus.Alarm, EngineStatus.Resetting };

            if (hsa.Contains(_motionController.MachineStatus))
            {
                //base.CloseDialog("ok");
                _bus.OnLog(Common.DataStruct.Enums.LogType.Info, $"设备未复位,请执行复位操作！");
                throw new FriendlyException($"设备未复位,请执行复位操作！");
                //hiveAPI.ClearAlarm();
                //return;
            }

            HashSet<EngineStatus> hsa1 = new HashSet<EngineStatus>() { EngineStatus.Stop };

            if (hsa1.Contains(_motionController.MachineStatus))
            {
                base.CloseDialog("ok");
                _bus.OnLog(Common.DataStruct.Enums.LogType.Info, $"当前设备状态Stop,不满足启动条件");
                //hiveAPI.ClearAlarm();
                return;
            }

            HashSet<EngineStatus> hsb = new HashSet<EngineStatus>() { EngineStatus.Ready, EngineStatus.Running };
            if (hsb.Contains(_motionController.MachineStatus))
            {
                base.CloseDialog("ok");
                _bus.OnLog(Common.DataStruct.Enums.LogType.Info, $"当前设备状态可保持原状,无需启动");
                return;
            }
            // 记录按钮关闭方式
            closeWay = "Button";
            _bus.OnLog(Common.DataStruct.Enums.LogType.Info, $"点击 确认清除故障 按钮，设备即将启动");
            if (_motionController.Start())
            {
                _bus.OnLog(Common.DataStruct.Enums.LogType.Info, $"设备从报警状态中成功启动，[Alarm]窗口即将关闭");
                base.CloseDialog("ok");
                hiveAPI.ClearAlarm();
            }
            else
                throw new FriendlyException($"设备启动失败,请再次执行复位操作！");
        }));

        protected override void CloseDialog(string parameter)
        {
            base.CloseDialog(parameter);
            dispatcherTimer.Stop();
            // 取消事件订阅
            if (_alarmClosedToken != null)
                _bus.EventBus.GetEvent<AlarmEventOnboardClosed>().Unsubscribe(_alarmClosedToken);
        }
        private string closeWay = "";
        protected override void Ok(IDialogResult result)
        {
            result.Parameters.Add("closeWay", closeWay);
        }
        public void Dispose()
        {
            dispatcherTimer.Stop();
            // 取消事件订阅
            if (_alarmClosedToken != null)
                _bus.EventBus.GetEvent<AlarmEventOnboardClosed>().Unsubscribe(_alarmClosedToken);
        }
    }
}
