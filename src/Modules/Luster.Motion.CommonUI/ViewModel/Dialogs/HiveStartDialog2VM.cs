using HandyControl.Controls;
using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Extensions;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.EditorUI.Extensions;
using Luster.Motion.Integration.SFC;
using Luster.Motion.Integration.Web;
using Luster.Motion.TaskFlow.Engine;
using Luster.Motion.TaskFlow.Engine.Models;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Luster.Motion.CommonUI.ViewModel.Dialogs
{
    public class HiveStartDialog2VM : MotionDialogVM
    {
        private CancellationTokenSource _cts;

        /// <summary>
        /// 报警开始时间/// 
        /// </summary>
        private DateTime _errorStartTime;
        public DateTime ErrorStartTime
        {
            get { return _errorStartTime; }
            set { SetProperty(ref _errorStartTime, value); }
        }

        /// <summary>
        /// 维修开始时间/// 
        /// </summary>
        private DateTime _repairStartTime;
        public DateTime RepairStartTime
        {
            get { return _repairStartTime; }
            set { SetProperty(ref _repairStartTime, value); }
        }


        /// <summary>
        /// 维修结束时间/// 
        /// </summary>
        private DateTime _repairEndTime;
        public DateTime RepairEndTime
        {
            get { return _repairEndTime; }
            set { SetProperty(ref _repairEndTime, value); }
        }


        /// <summary>
        /// 宕机时间///
        /// 单位分钟///
        /// </summary>
        private int _downTime;
        public int DownTime
        {
            get { return _downTime; }
            set { SetProperty(ref _downTime, value); }
        }


        /// <summary>
        /// 等级list///
        /// </summary>
        private List<KeyValue> _rateLists;
        public List<KeyValue> RateLists
        {
            get { return _rateLists; }
            set { SetProperty(ref _rateLists, value); }
        }

        /// <summary>
        /// 等级///
        /// </summary>
        private string _rate;
        public string Rate
        {
            get { return _rate; }
            set { SetProperty(ref _rate, value); }
        }

        /// <summary>
        /// 宕机原因list///
        /// </summary>
        private List<KeyValue> _downTimeLists;
        public List<KeyValue> DownTimeLists
        {
            get { return _downTimeLists; }
            set { SetProperty(ref _downTimeLists, value); }
        }

        /// <summary>
        /// 宕机原因
        /// </summary>
        private string _downTimeCause;
        public string DownTimeCause
        {
            get { return _downTimeCause; }
            set { SetProperty(ref _downTimeCause, value); }
        }


        /// <summary>
        /// 消耗物品list///
        /// </summary>
        private List<KeyValue> _sparePartLists;
        public List<KeyValue> SparePartLists
        {
            get { return _sparePartLists; }
            set { SetProperty(ref _sparePartLists, value); }
        }

        /// <summary>
        /// 消耗物品
        /// </summary>
        private string _sparePart;
        public string SparePart
        {
            get { return _sparePart; }
            set { SetProperty(ref _sparePart, value); }
        }

        /// <summary>
        /// 维修动作
        /// </summary>
        private string _actionTaken;
        public string ActionTaken
        {
            get { return _actionTaken; }
            set { SetProperty(ref _actionTaken, value); }
        }


        private string cardID = "";

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
        private IMotionController mController;
        private IDialogService _dialogService;
        private SFCHelper sfcHelper;
        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        HiveAPI _hiveAPI;
        private ProductStat _productStat;

        public Visibility MaintenanceVisibility { get; set; } = Visibility.Visible;

        private bool hasCardAccept = false;

        private void _mController_ProStatEvent(ProductStat pStatus, ImpactData arg2)
        {
            _productStat = pStatus;
        }

        public HiveStartDialog2VM(IMotionController _motionController, IDialogService dialogService, SFCHelper sFCHelper, HiveAPI hiveAPI, ProductStat productStat)
        {
            _cts = new CancellationTokenSource();
            mController = _motionController;
            mController.ProStatEvent += _mController_ProStatEvent;
            _dialogService = dialogService;
            _hiveAPI = hiveAPI;
            _productStat = productStat;
            DownTimeLists = new List<KeyValue>();
            SparePartLists = new List<KeyValue>();
            RateLists = new List<KeyValue>();
            RateLists.Add(new KeyValue() { Desc = "1" });
            RateLists.Add(new KeyValue() { Desc = "2" });
            RateLists.Add(new KeyValue() { Desc = "3" });
            RateLists.Add(new KeyValue() { Desc = "4" });
            RateLists.Add(new KeyValue() { Desc = "5" });

            dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
            dispatcherTimer.Tick += Timer_Tick;
            _motionController.FileConfig.HiveHelpRequestTime = DateTime.Now;
            dispatcherTimer.Start();
            List<string> dwTimes = mController.FileConfig.DownTimeCauses.Split('@').ToList();
            foreach (var rate in dwTimes)
            {
                if (rate != null) DownTimeLists.Add(new KeyValue() { Desc = rate });
            }

            List<string> spParts = mController.FileConfig.SparePartsUsed.Split('@').ToList();
            foreach (var part in spParts)
            {
                if (part != null) SparePartLists.Add(new KeyValue() { Desc = part });
            }

            sfcHelper = sFCHelper;
            ErrorStartTime = mController.FileConfig.HiveHelpRequestTime;
            RepairStartTime = DateTime.Now;
            MaintenanceVisibility = mController.FileConfig.IsHiveContinueMaintenanceVisible ? Visibility.Visible : Visibility.Hidden;
            //_hiveAPI.MachinetStart();

        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        private DelegateCommand<object> _click1Command;
        public DelegateCommand<object> Click1Command => _click1Command ?? (_click1Command = new DelegateCommand<object>((o) =>
        {
            _dialogService.ShowHiveStopDialog2("停机", r =>
            {
            });
            mController.Stop();
        }));

        /// <summary>
        /// 关闭窗口
        /// </summary>
        private DelegateCommand<object> _click2Command;
        public DelegateCommand<object> Click2Command => _click2Command ?? (_click2Command = new DelegateCommand<object>((o) =>
        {
            _dialogService.ShowHivePauseDialog("暂停", r =>
            {
            });
            mController.Pause();
        }));


        /// <summary>
        /// 观察5个窗口
        /// </summary>
        private DelegateCommand<object> _observeCommand;
        public DelegateCommand<object> ObserveCommand => _observeCommand ?? (_observeCommand = new DelegateCommand<object>((o) =>
        {
            try
            {
                if (!mController.CanAutoRun(out var errMsg))
                {
                    throw new FriendlyException(errMsg);
                }
                mController.Start();
                int startCount = _productStat.AllCount;
                Task.Run(() =>
                {
                    while (!_cts.IsCancellationRequested && (_productStat.AllCount - startCount) < 5 && mController.MotionEngine.EngineStatus != EngineStatus.Stop)
                    {
                        Thread.Sleep(1000);
                    }
                    //富士康客户要求的效果是点击一下生产5个后自动暂停，不切换到Alarm状态，再点击后可以再次运行
                    //如果先点击观察5个机台，再刷卡取消窗口，就取消这个task
                    if (_cts != null && _cts.IsCancellationRequested)
                    {
                        return;
                    }
                    mController.Pause(false);
                }, _cts.Token);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }));


        /// <summary>
        /// 刷卡命令
        /// </summary>
        private DelegateCommand<object> _scanCommand;

        public DelegateCommand<object> ScanCommand => _scanCommand ?? (new DelegateCommand<object>((obj) =>
        {
            //接收刷卡响应消息
            var e = obj as KeyEventArgs;
            if (e != null)
            {
                if (e.Key == Key.Enter)
                {
                    {
                        if (cardID.Length > 10 || cardID.Length < 6)
                        {
                            cardID = "";
                            return;
                        }
                        if (mController.FileConfig.IsHiveContinueMaintenanceVisible)
                        {
                            //如果必填项目为空，则不进行后续流程
                            if (string.IsNullOrEmpty(DownTimeCause) || string.IsNullOrEmpty(SparePart))
                            {
                                CardLog = $"请填写宕机原因和消耗物品";
                                CardBkColor = new SolidColorBrush(Color.FromRgb(255, 83, 88));
                                return;
                            }

                        }
                        else
                        {
                            //如果必填项目为空，则不进行后续流程
                            if (string.IsNullOrEmpty(DownTimeCause))
                            {
                                CardLog = $"请填写宕机原因";
                                CardBkColor = new SolidColorBrush(Color.FromRgb(255, 83, 88));
                                return;
                            }
                            mController.FileConfig.IsHiveContinueMaintenanceVisible = true;
                        }
                        //进行报文上报
                        var currentID = cardID.Trim();
                        if (cardID.Substring(0, 1) == "0")
                            cardID = cardID.Substring(1, cardID.Length - 1);
                        var ret = sfcHelper.CheckCard(cardID, _hiveAPI.machineSN, out string auth);
                        //var ret = true;
                        cardID = "";

                        //刷卡后，需要判断权限是否满足
                        //如果满足，需要弹窗2
                        if (ret && (auth == "L1" || auth == "L2" || auth == "L3" || auth == "L6" || auth == "L7" || auth == "L8" || auth == "L9"))
                        {
                            if (hasCardAccept)
                            {
                                return;
                            }
                            hasCardAccept = true;
                            if (cardID.Substring(0) == "0")
                                cardID = cardID.Substring(1, cardID.Length - 1);
                            CardLog = $"ID:{currentID}" + Environment.NewLine +
                                      $"Badge info sent" + Environment.NewLine +
                                      $"MES info received" + Environment.NewLine +
                                      $"You may start repair";
                            CardBkColor = new SolidColorBrush(Color.FromRgb(141, 212, 113));
                            base.CloseDialog("ok");
                            _hiveAPI.EndAlarm(DownTimeCause, ActionTaken, SparePart, auth, Rate);

                            mController.Start();
                        }
                        else
                        {
                            CardLog = $"ID:{currentID}" + Environment.NewLine +
                                      $"Badge info sent" + Environment.NewLine +
                                      $"MES info received" + Environment.NewLine +
                                      $"Access denied";
                            CardBkColor = new SolidColorBrush(Color.FromRgb(255, 83, 88));
                        }
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
        /// 1S刷新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object sender, EventArgs e)
        {
            RepairEndTime = DateTime.Now;
            DownTime = (int)Math.Ceiling((_repairEndTime - ErrorStartTime).TotalMinutes);
        }

        /// <summary>
        /// 关闭弹窗
        /// </summary>
        /// <param name="parameter"></param>
        protected override void CloseDialog(string parameter)
        {
            base.CloseDialog(parameter);
            _cts?.Cancel();
            dispatcherTimer.Stop();
        }
    }
}
