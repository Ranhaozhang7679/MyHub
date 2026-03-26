using Luster.Common.DataStruct;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.CommonUI.Models;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.VDevice;
using Luster.Motion.EditorUI.Extensions;
using Luster.Motion.Integration.SFC;
using Luster.Motion.Integration.Web;
using Luster.Motion.TaskFlow.Engine;
using Luster.TaskFlow.Motion.Interfaces;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Luster.Motion.CommonUI.ViewModel.Dialogs
{
    public class HiveOperationDialogVM : MotionDialogVM
    {

        private HiveAPI hiveAPI;
        private VisionAPI visionAPI;
        private SFCHelper sfcHelper;

        private IMotionController motionController;
        private Dispatcher dispatcher;
        private EStopReason stopReason;
        private string cmdStr = string.Empty;
        DateTime beginTime;
        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        private bool hasCardAccept = false;

        public SystemOperation Operation { get; private set; }


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
        /// 停机原因
        /// </summary>
        private string _stopReason;
        public string StopReason
        {
            get { return _stopReason; }
            set { SetProperty(ref _stopReason, value); }
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
                SetProperty
                (
                   ref _remainWeekMaintenanceDays, value
                );
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
        private Brush _weekMaintenanceColor;

        public Brush WeekMaintenanceColor
        {
            get => _weekMaintenanceColor;
            set => SetProperty(ref _weekMaintenanceColor, value);
        }

        /// <summary>
        /// 月维护颜色
        /// </summary>
        private Brush _monthMaintenanceColor;
        public Brush MonthMaintenanceColor
        {
            get => _monthMaintenanceColor;
            set => SetProperty(ref _monthMaintenanceColor, value);
        }

        // 本地画笔      
        private SolidColorBrush RedBrush = new SolidColorBrush(Color.FromRgb(225, 83, 88));
        private SolidColorBrush GreenBrush = new SolidColorBrush(Color.FromRgb(141, 212, 133));


        private IDialogService _dialogService;
        private ICommonBus _commonBus;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="motionController"></param>
        /// <param name="_hiveApi"></param>
        public HiveOperationDialogVM(IMotionController _motionController, HiveAPI _hiveApi, VisionAPI _visionAPI, SFCHelper sFCHelper, Dispatcher dispatcher, IDialogService dialogService, ICommonBus commonBus)
        {
            hiveAPI = _hiveApi;
            visionAPI = _visionAPI;
            motionController = _motionController;
            this.dispatcher = dispatcher;
            _dialogService = dialogService;
            _commonBus = commonBus;
            CardBkColor = new SolidColorBrush(Color.FromRgb(180, 180, 180));
            sfcHelper = sFCHelper;
            UpdateRemainMaintenanceDays();
        }


        /// <summary>
        /// 扫码
        /// </summary>
        private DelegateCommand<object> _loadCommand;
        public DelegateCommand<object> LoadCommand => _loadCommand ?? (new DelegateCommand<object>((obj) =>
        {
            SendHiveMsg("");
            // var ret = sfcHelper.CheckCard("1111111111"); 
        }));

        /// <summary>
        /// 扫码
        /// </summary>
        private DelegateCommand<object> _scanCommand;


        private string cardID = "";
        public DelegateCommand<object> ScanCommand => _scanCommand ?? (new DelegateCommand<object>((obj) =>
        {
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

                    if (string.IsNullOrEmpty(cmdStr))
                    {
                        CardLog = $"请先选择停机原因";
                    }
                    else
                    {
                        motionController.FileConfig.IsCardEnd = true;
                        //进行报文上报
                        var currentID = cardID.Trim();
                        if (cardID.Substring(0, 1) == "0")
                            cardID = cardID.Substring(1, cardID.Length - 1);

                        var ret = sfcHelper.CheckCard(cardID, hiveAPI.machineSN, out string auth);

                        SendHiveMsg(cmdStr);
                        cardID = "";

                        if (ret && (auth == "L1" || auth == "L2" || auth == "L3" || auth == "L6" /*|| auth == "L7" || auth == "L8"*/ || auth == "L9"))
                        {
                            if (hasCardAccept)
                            {
                                return;
                            }
                            //motionController.Stop();
                            hasCardAccept = true;
                            IsButtonEnable = false;
                            //if (cardID.Substring(0) == "0")
                            //    cardID = cardID.Substring(1, cardID.Length - 1);
                            CardLog = $"ID:{currentID}" + Environment.NewLine +
                            $"Badge info sent:" + Environment.NewLine +
                            $"MES info received:" + Environment.NewLine +
                            $"Machine stopped. You may now start repair";
                            CardBkColor = new SolidColorBrush(Color.FromRgb(141, 212, 113));
                            Task.Delay(0).ContinueWith(task =>
                            {
                                this.dispatcher.Invoke(() =>
                                {
                                    _commonBus.OnLog(Common.DataStruct.Enums.LogType.Info, $"刷卡信息响应成功，[Operation]窗口即将关闭");
                                    //判断是否选择的是记录非停机异常
                                    if (cmdStr != "记录非停机异常")
                                    {
                                        DoCheck(cmdStr);
                                        //IDialogResult dialogRes = new DialogResult();
                                        //Ok(dialogRes);
                                        closeWay = "Card";
                                        CloseCommand.Execute("ok"); // qrErrCode
                                        hiveAPI.HiveManualStart(cmdStr, auth);
                                        if (cmdStr == "周保养")
                                        {
                                            _dialogService.ShowHiveStopDialog3("停机", r =>
                                            {
                                            });
                                        }
                                        else if (cmdStr == "月保养")
                                        {
                                            _dialogService.ShowHiveStopDialog4("停机", r =>
                                            {
                                            });
                                        }
                                        else
                                        {
                                            _dialogService.ShowHiveStopDialog2("停机", r =>
                                            {
                                            });

                                        }
                                    }
                                    else
                                    {
                                        closeWay = "Card";
                                        CloseCommand.Execute("ok");
                                        hiveAPI.HiveManualStart(cmdStr, auth);

                                        if (Operation == SystemOperation.Pause)
                                        {
                                            //    motionController.FileConfig.IsHiveContinueMaintenanceVisible = false;
                                            //    _dialogService.HiveRunPopUp(DataStruct.Enums.SystemOperation.Start, r =>
                                            //    {
                                            //        motionController.Start();
                                            //    });
                                        }
                                    }
                                });
                            });
                        }
                        else if (ret && (auth == "L7" || auth == "L8"))
                        {
                            //hive 2.7要求L7/L8不对停机做出反应
                        }
                        else
                        {
                            CardLog = $"ID:{currentID}. Badge info sent,\r\n MES info received. Access denied. ";
                            CardBkColor = new SolidColorBrush(Color.FromRgb(225, 83, 88));
                        }
                        //记录停止时间

                        motionController.FileConfig.HiveHelpRequestTime = DateTime.Now;

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
        /// 按钮选中,停机原因
        /// </summary>
        private DelegateCommand<object> _checkCommand;
        public DelegateCommand<object> CheckCommand => _checkCommand ?? (_checkCommand = new DelegateCommand<object>((o) =>
        {
            DoCheck((string)o);
        }));
        private string qrErrCode = "";
        private void DoCheck(string cmdStr)
        {
            string alarmCode = "";
            string alarmMsg = "";
            switch (cmdStr)
            {
                case "一级供应商":
                    stopReason = EStopReason.Onest_Tier_Vendor;
                    alarmCode = "F99OOOO-01";
                    alarmMsg = "Stopped for repair by 1st tier vendor";
                    break;

                case "一+二级供应商":
                    stopReason = EStopReason.OnestATwond_Tier_Vendor;
                    alarmCode = "F99OOOO-02";
                    alarmMsg = "Stopped for repair by 2 vendors";
                    break;

                case "凌云光":
                    stopReason = EStopReason.Luster_Vision;
                    alarmCode = "F99OOOO-03";
                    alarmMsg = "Stopped for repair by Cognex";
                    break;

                case "汇川":
                    stopReason = EStopReason.Inovance_PLC;
                    alarmCode = "F99OOOO-04";
                    alarmMsg = "Stopped for repair by Keyence";
                    break;

                case "工厂人员":
                    stopReason = EStopReason.Inovance_Manufacturer;
                    alarmCode = "F99OOOO-11";
                    alarmMsg = "Stopped for repair by contract manufacturer";
                    break;

                case "耗材/物料补充":
                    stopReason = EStopReason.Consumable_material_replenishment;
                    alarmCode = "F99OOOO-08";
                    alarmMsg = "Stopped for Consumable/material replenishment";
                    break;

                case "非生产时间调试":
                    stopReason = EStopReason.Finetuning_during_nonproduction_time;
                    alarmCode = "F99OOOO-09";
                    //alarmMsg = "Stopped for Consumable/material replenishment";
                    alarmMsg = "Fine-tuning during non-production time";
                    break;

                case "周保养":
                    stopReason = EStopReason.WeeklyMaintenance;
                    alarmCode = "F99OOOO-05";
                    alarmMsg = "Stopped for Weekly Maintenance";
                    break;

                case "月保养":
                    stopReason = EStopReason.MonthlyMaintenance;
                    alarmCode = "F99OOOO-06";
                    alarmMsg = "Stopped for Monthly Maintenance";
                    break;
                case "计划关机/待机":
                    stopReason = EStopReason.Planned_Shutdown;
                    alarmCode = "F99OOOO-07";
                    alarmMsg = "Planned shutdown/idle";
                    break;

                case "记录非停机异常":
                    stopReason = EStopReason.Idle;
                    alarmCode = "F99OOOO-10";
                    alarmMsg = "Degraded mode";
                    break;

                default:
                    stopReason = EStopReason.UI;
                    alarmCode = "F99OOOO-20";
                    alarmMsg = "Machine stopped through UI";
                    break;
            }
            qrErrCode = alarmCode;
            if (stopReason != EStopReason.Idle)
            {
                Task.Run(() =>
                {
                    hiveAPI.MachineStatusChange(alarmCode, alarmMsg, DataStruct.EngineStatus.Running, DataStruct.EngineStatus.Alarm);

                    //开启蜂鸣0.5/10S,蜂鸣一次
                    motionController.SetBuzzer(true, 1000, 10000);
                });

                //发送视觉指令，开启视觉编辑功能
                //Need To Do
                if (stopReason == EStopReason.Luster_Vision)
                {
                }

            }

            // 由于维护过卡，所以用异步
            Task.Run(() =>
            {
                if (stopReason < EStopReason.Idle)
                {
                    visionAPI.MachineMaintain(alarmMsg);
                }
                if (stopReason == EStopReason.WeeklyMaintenance)
                {
                    visionAPI.MachineInspect();
                    motionController.FileConfig.LastWeekMaintenanceDate = DateTime.Now;

                }
                else if (stopReason == EStopReason.MonthlyMaintenance)
                {
                    visionAPI.MachineInspect();
                    motionController.FileConfig.LastMonthMaintenanceDate = DateTime.Now;
                }
                UpdateRemainMaintenanceDays();

            });

            //base.CloseDialog("ok");
        }
        private DelegateCommand<object> _setCommand;
        public DelegateCommand<object> SetCommand => _setCommand ?? (_setCommand = new DelegateCommand<object>((o) =>
        {
            cmdStr = o.ToString();
            CardLog = $"已选择{cmdStr}，请刷卡确认";
        }));


        private DelegateCommand<object> _resumeCommand;
        public DelegateCommand<object> ResumeCommand => _resumeCommand ?? (_resumeCommand = new DelegateCommand<object>((o) =>
        {
            if (!motionController.CanAutoRun(out var msg)) throw new FriendlyException(msg);
            _commonBus.OnLog(LogType.Info, "点击 恢复生产 按钮，设备即将启动");
            if (motionController.Start())
            {
                _commonBus.OnLog(Common.DataStruct.Enums.LogType.Info, $"设备从暂停状态中成功启动，[Operation]窗口即将关闭");
                // 记录按钮关闭方式
                closeWay = "Button";
                base.CloseDialog("ok");
                hiveAPI.MachineStatusChange("", "", DataStruct.EngineStatus.Pause, DataStruct.EngineStatus.Running, false, "");
            }
            else
                throw new FriendlyException($"设备启动失败,请考虑刷卡！");
        }));


        private void SendHiveMsg(string stopString)
        {
            string alarmCode = "";
            string alarmMsg = "";
            var currentCardID = cardID;
            switch (cmdStr)
            {
                case "一级供应商":
                    stopReason = EStopReason.Onest_Tier_Vendor;
                    alarmCode = "F99OOOO-01";
                    alarmMsg = "Stopped for repair by 1st tier vendor";
                    break;

                case "一+二级供应商":
                    stopReason = EStopReason.OnestATwond_Tier_Vendor;
                    alarmCode = "F99OOOO-02";
                    alarmMsg = "Stopped for repair by 2 vendors";
                    break;

                case "凌云光":
                    stopReason = EStopReason.Luster_Vision;
                    alarmCode = "F99OOOO-03";
                    alarmMsg = "Stopped for repair by Cognex";
                    break;

                case "汇川":
                    stopReason = EStopReason.Inovance_PLC;
                    alarmCode = "F99OOOO-04";
                    alarmMsg = "Stopped for repair by Keyence";
                    break;

                case "耗材/物料补充":
                    stopReason = EStopReason.Consumable_material_replenishment;
                    alarmCode = "F99OOOO-08";
                    alarmMsg = "Stopped for Consumable/material replenishment";
                    break;

                case "工厂人员":
                    stopReason = EStopReason.Inovance_Manufacturer;
                    alarmCode = "F99OOOO-11";
                    alarmMsg = "Stopped for repair by contract manufacturer";
                    break;

                case "非生产时间调试":
                    stopReason = EStopReason.Finetuning_during_nonproduction_time;
                    alarmCode = "F99OOOO-09";
                    //alarmMsg = "Stopped for Consumable/material replenishment";
                    alarmMsg = "Fine-tuning during non-production time";
                    break;

                case "周保养":
                    stopReason = EStopReason.WeeklyMaintenance;
                    alarmCode = "F99OOOO-05";
                    alarmMsg = "Stopped for Weekly Maintenance";
                    break;

                case "月保养":
                    stopReason = EStopReason.MonthlyMaintenance;
                    alarmCode = "F99OOOO-06";
                    alarmMsg = "Stopped for Monthly Maintenance";
                    break;
                case "计划关机/待机":
                    stopReason = EStopReason.Planned_Shutdown;
                    alarmCode = "F99OOOO-07";
                    alarmMsg = "Planned shutdown/idle";
                    break;

                case "记录非停机异常":
                    stopReason = EStopReason.Idle;
                    alarmCode = "F99OOOO-10";
                    alarmMsg = "Degraded mode";
                    motionController.FileConfig.IsHiveContinueMaintenanceVisible = true;
                    break;

                default:
                    stopReason = EStopReason.UI;
                    alarmCode = "F99OOOO-20";
                    alarmMsg = "Machine stopped through UI";
                    currentCardID = "";
                    break;
            }

            hiveAPI.MachineStatusChange(alarmCode, alarmMsg, DataStruct.EngineStatus.Running, DataStruct.EngineStatus.Alarm, false, currentCardID);
            if (stopReason != EStopReason.Idle)
            {
                Task.Run(() =>
                {
                    //开启蜂鸣0.5/10S,蜂鸣一次
                    motionController.SetBuzzer(true, 1000, 10000);
                });
            }

            // 由于维护过卡，所以用异步
            Task.Run(() =>
            {
                if (stopReason < EStopReason.Idle)
                {
                    visionAPI.MachineMaintain(alarmMsg);
                }
                else if (stopReason == EStopReason.WeeklyMaintenance || (stopReason == EStopReason.MonthlyMaintenance))
                {
                    visionAPI.MachineInspect();
                }
            });
            //return result;
        }
        private string closeWay = "";
        /// <summary>
        /// 重载OK逻辑，添加回调参数
        /// </summary>
        /// <param name="result"></param>
        protected override void Ok(IDialogResult result)
        {
            result.Parameters.Add("QrErrorCode", qrErrCode);

            if (cmdStr == "记录非停机异常" && Operation == SystemOperation.Pause)
            {
                result.Parameters.Add("command", "pause");
            }
            result.Parameters.Add("closeWay", closeWay);
            //string alarmCode = "";
            //string alarmMsg = "";

            //switch (stopReason)
            //{
            //    case EStopReason.Onest_Tier_Vendor:
            //        alarmCode = "F99OOOO-01";
            //        alarmMsg = "Stopped for repair";
            //        break;

            //    case EStopReason.OnestATwond_Tier_Vendor:
            //        alarmCode = "F99OOOO-02";
            //        alarmMsg = "Stopped for repair by 2 vendors";
            //        break;

            //    case EStopReason.Luster_Vision:
            //        alarmCode = "F99OOOO-03";
            //        alarmMsg = "Stopped for repair by Luster VA";
            //        break;

            //    case EStopReason.Idle:
            //        alarmCode = "";
            //        alarmMsg = "";
            //        break;

            //    case EStopReason.Inovance_PLC:
            //        alarmCode = "F99OOOO-04";
            //        alarmMsg = "Stopped for repair by Inovance";
            //        break;
            //    case EStopReason.Planed_Maintenance:
            //        alarmCode = "Stopped for planned maintenance ";
            //        alarmMsg = "F99OOOO-05";
            //        break;
            //}

            //if (stopReason != EStopReason.Idle)
            //{
            //    Task.Run(() =>
            //    {
            //        hiveAPI.MachineStatusChange(alarmCode, alarmMsg, DataStruct.EngineStatus.Running, DataStruct.EngineStatus.Alarm);
            //        //开启蜂鸣0.5/10S,蜂鸣一次
            //        motionController.SetBuzzer(true, 500, 10000);
            //    });

            //    //发送视觉指令，开启视觉编辑功能
            //    //Need To Do
            //    if (stopReason == EStopReason.Luster_Vision)
            //    {
            //    }

            //}

            //// 由于维护过卡，所以用异步
            //Task.Run(() =>
            //{
            //    if (stopReason < EStopReason.Idle)
            //    {
            //        visionAPI.MachineMaintain(alarmMsg);
            //    }
            //    else if (stopReason == EStopReason.Planed_Maintenance)
            //    {
            //        visionAPI.MachineInspect();
            //    }
            //});
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            beginTime = DateTime.Now.AddMinutes(5);
            dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
            dispatcherTimer.Tick += Timer_Tick;
            dispatcherTimer.Start();
            IsButtonEnable = true;

            // 获取 Operation 参数
            var operation = parameters.GetValue<SystemOperation>("Operation");

            this.Operation = operation;
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

        /// <summary>
        /// 停机原因
        /// </summary>
        enum EStopReason
        {
            Onest_Tier_Vendor,
            OnestATwond_Tier_Vendor,
            Luster_Vision,
            Inovance_PLC,
            Consumable_material_replenishment,
            Finetuning_during_nonproduction_time,
            WeeklyMaintenance,
            MonthlyMaintenance,
            Planned_Shutdown,
            Idle,
            UI,
            Inovance_Manufacturer
        }


        private void UpdateRemainMaintenanceDays()
        {
            RemainWeekMaintenanceDays = (int)(motionController.FileConfig.LastWeekMaintenanceDate.AddDays(7) - DateTime.Now).TotalDays;
            RemainMonthMaintenanceDays = (int)(motionController.FileConfig.LastMonthMaintenanceDate.AddDays(30) - DateTime.Now).TotalDays;
            WeekMaintenanceColor = RemainWeekMaintenanceDays >= 0 ? GreenBrush : RedBrush;
            MonthMaintenanceColor = RemainMonthMaintenanceDays >= 0 ? GreenBrush : RedBrush;
        }
    }
}
