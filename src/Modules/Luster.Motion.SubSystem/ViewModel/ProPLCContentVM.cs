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
using Luster.Control.Wpf.Motion.Controls;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.CommonUI.Models;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.EditorUI.Events;
using Luster.Motion.EditorUI.Extensions;
using Luster.Motion.SubSystem.Models;
using Luster.Motion.TaskFlow.Engine;
using Luster.Motion.TaskFlow.Engine.Models;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Luster.Motion.DataStruct;
using Luster.SimDevice.SubSystem.Events;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using Luster.Motion.CommonUI.Dock;
using Luster.Motion.DataStruct.VDevice;
using Luster.Common.Tools;
using Luster.Control.Wpf.Motion.Flow;
using System.Windows.Media;
using Luster.Motion.Integration.Web;
using Luster.Motion.TaskFlow.Engine.HyperTrain;
using System.Xaml.Schema;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Interfaces;
using Luster.Motion.TaskFlow.Engine.Engine;
using Luster.TaskFlow.Common.Enums;
using System.Diagnostics;
using Luster.Common.DataStruct;

namespace Luster.Motion.SubSystem.ViewModel
{
    /// <summary>
    /// 首页缓存报表
    /// </summary>
    public class ProPLCContentVM : MotionVM, IDockContent
    {
        /// <summary>
        /// 数据库访问
        /// </summary>
        private IDbManager _dbManager;

        /// <summary>
        /// 运控控制
        /// </summary>
        private IMotionController _mController;

        /// <summary>
        /// 三色灯管理
        /// </summary>
        private ILightManager _lightManager;
        private readonly IMotionEngine _motionEngine;

        /// <summary>
        /// Web服务器
        /// </summary>
        private IWebService _webService;

        /// <summary>
        /// UI线程
        /// </summary>
        private Dispatcher _dispatcher;

        /// <summary>
        /// 当前状态
        /// 1.AutoRun
        /// 2.Idle
        /// 3.Block
        /// </summary>
        private static int currentStatus;

        /// <summary>
        /// 进入Idle状态计时
        /// </summary>
        private static int idleCount;

        /// <summary>
        /// 进入Block状态计时
        /// </summary>
        private static int blockCount;


        /// <summary>
        /// 设备
        /// </summary>
        private IDeviceEngine _deviceEngine;

        #region 信号状态
        /// <summary>
        /// 主流线上游有料
        /// </summary>
        private bool _mainPrevHave;
        public bool MainPrevHave
        {
            get => _mainPrevHave;
            set => SetProperty(ref _mainPrevHave, value);
        }

        /// <summary>
        /// 主流线本站要料
        /// </summary>
        private bool _MainThisGet;
        public bool MainThisGet
        {
            get => _MainThisGet;
            set => SetProperty(ref _MainThisGet, value);
        }

        /// <summary>
        /// 主流线本站有料
        /// </summary>
        private bool _MainThisHave;
        public bool MainThisHave
        {
            get => _MainThisHave;
            set => SetProperty(ref _MainThisHave, value);
        }

        /// <summary>
        /// 主流线下游要料
        /// </summary>
        private bool _MainNextGet;
        public bool MainNextGet
        {
            get => _MainNextGet;
            set => SetProperty(ref _MainNextGet, value);
        }

        /// <summary>
        /// 回流线上游有料
        /// </summary>
        private bool _SubPrevHave;
        public bool SubPrevHave
        {
            get => _SubPrevHave;
            set => SetProperty(ref _SubPrevHave, value);
        }

        /// <summary>
        /// 回流线本站要料
        /// </summary>
        private bool _SubThisGet;
        public bool SubThisGet
        {
            get => _SubThisGet;
            set => SetProperty(ref _SubThisGet, value);
        }

        /// <summary>
        /// 回流线本站有料
        /// </summary>
        private bool _SubThisHave;
        public bool SubThisHave
        {
            get => _SubThisHave;
            set => SetProperty(ref _SubThisHave, value);
        }

        /// <summary>
        /// 回流线下游要料
        /// </summary>
        private bool _SubNextGet;
        public bool SubNextGet
        {
            get => _SubNextGet;
            set => SetProperty(ref _SubNextGet, value);
        }


        /// <summary>
        /// 治具数量
        /// </summary>
        private int _carrierCount;
        public int CarrierCount
        {
            get => _carrierCount;
            set => SetProperty(ref _carrierCount, value);
        }

        /// <summary>
        /// 总治具数量
        /// </summary>
        private int _totalCarrierCount;
        public int TotalCarrierCount
        {
            get => _totalCarrierCount;
            set => SetProperty(ref _totalCarrierCount, value);
        }

        /// <summary>
        /// 主流线治具数量
        /// </summary>
        private int _mainCarrierCount;
        public int MainCarrierCount
        {
            get => _mainCarrierCount;
            set => SetProperty(ref _mainCarrierCount, value);
        }

        /// <summary>
        /// 物流线治具数量
        /// </summary>
        private int _subCarrierCount;
        public int SubCarrierCount
        {
            get => _subCarrierCount;
            set => SetProperty(ref _subCarrierCount, value);
        }

        /// <summary>
        /// 回流线治具数量
        /// </summary>
        private int _backCarrierCount;
        public int BackCarrierCount
        {
            get => _backCarrierCount;
            set => SetProperty(ref _backCarrierCount, value);
        }


        #endregion


        #region 临时变量
        private int MainIdleTimeMemory = 0;
        private int MainBlockTimeMemory = 0;
        private int SubIdleTimeMemory = 0;
        private int SubBlockTimeMemory = 0;
        /// <summary>
        /// Idle状态延时10s响应
        /// </summary>
        private const int IdleDelayTime = 10000;
        #endregion

        //入料时间记忆
        public DateTime ProductLoadTimeMemory;

        /// <summary>
        /// 是否启用编辑
        /// </summary>
        private List<string> _plcAddress;
        public List<string> PlcAddress
        {
            get => _plcAddress;
            set => SetProperty(ref _plcAddress, value);
        }

        /// <summary>
        /// 是否启用编辑
        /// </summary>
        private bool _isEdit = false;
        public bool IsEdit
        {
            get => _isEdit;
            set => SetProperty(ref _isEdit, value);
        }

        /// <summary>
        /// PLC启动
        /// </summary>
        private bool _canStart;
        public bool CanStart
        {
            get => _canStart;
            set => SetProperty(ref _canStart, value);
        }




        /// <summary>
        /// 主流线idle时间
        /// </summary>
        private int _mainIdleTime;
        public int MainIdleTime
        {
            get => _mainIdleTime;
            set => SetProperty(ref _mainIdleTime, value);
        }

        /// <summary>
        /// 主流线block时间
        /// </summary>
        private int _mainBlockTime;
        public int MainBlockTime
        {
            get => _mainBlockTime;
            set => SetProperty(ref _mainBlockTime, value);
        }

        /// <summary>
        /// 回流线idle时间
        /// </summary>
        private int _subIdleTime;
        public int SubIdleTime
        {
            get => _subIdleTime;
            set => SetProperty(ref _subIdleTime, value);
        }

        /// <summary>
        /// 回流线block时间
        /// </summary>
        private int _subBlockTime;
        public int SubBlockTime
        {
            get => _subBlockTime;
            set => SetProperty(ref _subBlockTime, value);
        }

        #region 机台状态
        /// <summary>
        /// 机台状态
        /// </summary>
        private Brush _MStatusBrush = FlowItem.RunningBrush;
        public Brush MStatusBrush
        {
            get { return _MStatusBrush; }
            set
            {
                SetProperty(ref _MStatusBrush, value);
            }
        }

        /// <summary>
        /// 机台状态
        /// </summary>
        private string _MStatus = "Idle";
        public string MStatus
        {
            get { return _MStatus; }
            set
            {
                SetProperty(ref _MStatus, value);
                switch (value)
                {
                    case "Idle":
                        MStatusBrush = FlowItem.RunningBrush;
                        break;
                    case "Pause":
                        MStatusBrush = FlowItem.WarningBrush;
                        break;
                    case "Block":
                        MStatusBrush = FlowItem.SkipBrush;
                        break;
                    case "Running":
                        MStatusBrush = FlowItem.SuccessBrush;
                        break;
                    case "Down":
                        MStatusBrush = FlowItem.FailBrush;
                        break;
                }
            }
        }
        #endregion
        /// <summary>
        /// PLC对象
        /// </summary>
        private VPlc vPlc = null;

        /// <summary>
        /// 循环
        /// </summary>
        private WhileTool whileTool = null;

        /// <summary>
        /// 弹窗
        /// </summary>
        private IDialogService _dialogService;

        private bool IdleToRun = false;

        /// <summary>
        /// 空构造函数
        /// </summary>
        public ProPLCContentVM() { }
        private object objLock = new object();
        private int InIdleTimeS = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_commonBus"></param>
        /// <param name="dbManager"></param>
        /// <param name="motionController"></param>
        /// <param name="dispatcher"></param>
        public ProPLCContentVM(ICommonBus _commonBus,
            IDialogService dialogService,
            IDbManager dbManager,
            IMotionController motionController,
            IWebService webService,
            Dispatcher dispatcher,
            IDeviceEngine deviceEngine,
            ILightManager lightManager,
            IMotionEngine motionEngine) : base(_commonBus)
        {
            _dbManager = dbManager;
            _mController = motionController;
            _lightManager = lightManager;
            this._motionEngine = motionEngine;
            _dispatcher = dispatcher;
            PlcAddress = new List<string>();
            _dialogService = dialogService;
            _deviceEngine = deviceEngine;
            _webService = webService;

            // PLC对象
            vPlc = _deviceEngine.GetVDevices<VPlc>().FirstOrDefault();


            // PLC状态监控
            _mController.PlcStatusEvent -= MController_PlcStatusEvent;
            _mController.PlcStatusEvent += MController_PlcStatusEvent;


            _mController.MotionEngine.ProLoadedEvent -= MotionEngineProLoadedEvent;
            _mController.MotionEngine.ProLoadedEvent += MotionEngineProLoadedEvent;


            whileTool = new WhileTool(true);

            // 开启一个现场循环检查PLC地址信号
            Task.Run(() =>
            {
                whileTool.While(Refresh, 1000);
            });

            ShowPlcStatus();
        }

        /// <summary>
        /// 注册事件
        /// </summary>
        /// <param name="bus"></param>
        protected override void RegisterEvent(IEventAggregator bus)
        {
            base.RegisterEvent(bus);

            // 机台停止，状态切换为Down装填
            bus.GetEvent<OperationEvent>().Subscribe(r =>
            {
                if (r.Dst == EngineStatus.Stop || r.Dst == EngineStatus.Alarm)
                {
                    MStatus = "Down";
                    currentStatus = 3;
                }
                else if (r.Dst == EngineStatus.Running)
                {
                    MStatus = "Running";
                    currentStatus = 1;
                    idleCount = 0;
                }
                else if (r.Dst == EngineStatus.Homing || r.Dst == EngineStatus.Ready)
                {
                    MStatus = "Homing";
                    currentStatus = 2;
                }
                else if (r.Dst == EngineStatus.Idle || r.Dst == EngineStatus.Pause)
                {
                    MStatus = r.Dst == EngineStatus.Pause ? "Pause" : "Idle";
                    currentStatus = 4;
                }
            });
        }
        /// <summary>
        /// 刷新UI
        /// </summary>
        private void Refresh()
        {
            //下游要料 本站有料  本站要料  上游有料   序号   状态   
            //   0        0          0        0        0     Idle
            //   0        0          0        1        1     Idle
            //   0        0          1        0        2     Idle
            //   0        0          1        1        3     Run
            //   0        1          0        0        4     Block
            //   0        1          0        1        5     Block
            //   0        1          1        0        6     Block
            //   0        1          1        1        7     Run
            //   1        0          0        0        8     Idle
            //   1        0          0        1        9     Idle
            //   1        0          1        0        10    Idle
            //   1        0          1        1        11    Run
            //   1        1          0        0        12    Run
            //   1        1          0        1        13    Run
            //   1        1          1        0        14    Run
            //   1        1          1        1        15    Run

            _dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {

                //需求描述（20250506，可以先实现 1、2、5）
                //1、設備在正常生產中亮綠燈；
                //2、設備報警中亮紅燈且蜂鳴器閃鳴；
                //3、設備停止且未回原閃紅燈；
                //4、設備回原完成但未啟動閃黃燈；
                //5、設備已經啟動且無異常，上游無來料或下游堵料10秒後閃黃燈；
                //6、設備缺小料，滿料，拋料時閃黃燈 + 蜂鳴器閃鳴。

                #region 不用
                MainPrevHave = GetPlcValue(_mController.SysConfig.MainPrevHaveAddr);
                MainThisHave = GetPlcValue(_mController.SysConfig.MainThisHaveAddr);
                MainThisGet = GetPlcValue(_mController.SysConfig.MainThisGetAddr);
                MainNextGet = GetPlcValue(_mController.SysConfig.MainNextGetAddr);

                SubPrevHave = GetPlcValue(_mController.SysConfig.SubPrevHaveAddr);
                SubThisHave = GetPlcValue(_mController.SysConfig.SubThisHaveAddr);
                SubThisGet = GetPlcValue(_mController.SysConfig.SubThisGetAddr);
                SubNextGet = GetPlcValue(_mController.SysConfig.SubNextGetAddr);

                //TotalCarrierCount = GetPlcIntValue(_mController.SysConfig.TotalCarrierCountAddr);
                MainCarrierCount = GetPlcIntValue(_mController.SysConfig.MainCarrierCountAddr);
                SubCarrierCount = GetPlcIntValue(_mController.SysConfig.SubCarrierCountAddr);
                BackCarrierCount = GetPlcIntValue(_mController.SysConfig.BackCarrierCountAddr);
                TotalCarrierCount = 0;
                if (MainCarrierCount >= 0)
                {
                    TotalCarrierCount += MainCarrierCount;
                }
                if (SubCarrierCount >= 0)
                {
                    TotalCarrierCount += SubCarrierCount;
                }
                if (BackCarrierCount >= 0)
                {
                    TotalCarrierCount += BackCarrierCount;
                }
                int smemaStatus = SmemaToInt(MainNextGet, MainThisHave, MainThisGet, MainPrevHave);

                #endregion

                var idleStatus = GetPlcIntValue(_mController.SysConfig.IdleStatusAddr);
                bool IsIdle = idleStatus == 2 || idleStatus == 3;//Idle状态
                bool IsRunning = idleStatus == 7;//流线动作
                bool IsToss = idleStatus == 8;//堵料、抛料
                var runStatus = GetPlcIntValue(_mController.SysConfig.StatusAddr);
                bool IsAlarm = runStatus == 6;

                if (currentStatus == 1)
                {
                    currentStatus = 0;
                    _lightManager.RunningLight();
                    Debug.WriteLine("Running");
                }//点击启动按钮触发一次Running
                if (IsAlarm || currentStatus == 3)
                {
                    //currentStatus = 0;
                    _lightManager.StopLight(true);
                    Debug.WriteLine("IsAlarm");
                }
                else if (currentStatus == 2)
                {
                    _lightManager.ReadyLight();
                    Debug.WriteLine("Homing");
                }
                else if (IsIdle) //  || currentStatus == 4
                {
                    if (_mController.MotionEngine.EngineStatus == EngineStatus.Running)
                    {
                        InIdleTimeS++;
                        if (InIdleTimeS <= 10) return;
                        InIdleTimeS = 0;
                        //_lightManager.IdleLight(IdleDelayTime, () => { _webService.OnMachineStatus("待料", ""); });
                        _lightManager.IdleLight(IdleDelayTime, () =>
                        {
                            if (_motionEngine is MotionEngine engine)
                            {
                                //engine.EngineStatus = EngineStatus.Idle;
                                MStatus = "PLC-Idle";
                            }
                        });
                        Debug.WriteLine("IsIdle");
                    }
                }
                else if (IsRunning)
                {
                    if (_mController.MotionEngine.EngineStatus == EngineStatus.Running)
                    {
                        _lightManager.RunningLight(() =>
                        {
                            if (_motionEngine is MotionEngine engine)
                            {
                                //engine.EngineStatus = EngineStatus.Running;
                            }
                        });
                        Debug.WriteLine("IsRunning");
                    }
                    //if (_mController is MotionController motionController)
                    //{
                    //    if (motionController._firstStartFlag == false && _mController.MotionEngine.EngineStatus == EngineStatus.Idle)
                    //    {
                    //        if (!_mController.CanAutoRun(out var errMsg))
                    //        {
                    //            if (!_isExceptionDialogOpen)
                    //            {
                    //                commonBus.OnLog(Common.DataStruct.Enums.LogType.Info, $"当前未切换至自动按钮下,不满足启动条件");
                    //                _isExceptionDialogOpen = true;
                    //                throw new FriendlyException(errMsg);
                    //            }
                    //            return;
                    //        }
                    //        _isExceptionDialogOpen = false;
                    //        //_lightManager.RunningLight(() => { _webService.OnMachineStatus("运行中", ""); });
                    //    }
                    //}
                }
                else if (IsToss)
                {
                    _lightManager.TossLight();
                    Debug.WriteLine("IsToss");
                }
                //停止时，不通过入料事件切换状态
                if (_mController.MotionEngine.EngineStatus == EngineStatus.Stop) return;
                //报警时，不通过入料事件切换状态
                if (_mController.MotionEngine.EngineStatus == EngineStatus.Alarm) return;
                //暂停时，不通过入料事件切换状态
                if (_mController.MotionEngine.EngineStatus == EngineStatus.Pause) return;


                //写入PC状态到PLC地址，通知PLC，目前PC状态
                if (_mController.MotionEngine.EngineStatus == EngineStatus.Running)
                {
                    WritePlcValueInt(_mController.SysConfig.PCStatusAddr, 1);
                }
                else if (_mController.MotionEngine.EngineStatus == EngineStatus.Idle)
                {
                    WritePlcValueInt(_mController.SysConfig.PCStatusAddr, 2);
                }
                else
                {
                    WritePlcValueInt(_mController.SysConfig.PCStatusAddr, 0);
                }

            }));
        }


        /// <summary>
        /// 当前模式
        /// </summary>
        private Brush _plcStatusBrush = FlowItem.DefaultBrush;
        public Brush PLCStatusBrush
        {
            get { return _plcStatusBrush; }
            set
            {
                SetProperty(ref _plcStatusBrush, value);
            }
        }


        private PlcStatus curStatus = PlcStatus.None;

        /// <summary>
        /// 控制
        /// </summary>
        /// <param name="obj"></param>
        private void MController_PlcStatusEvent(TaskFlow.Engine.Models.PlcStatus pStatus)
        {
            _dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                if (curStatus == pStatus)
                {
                    return;
                }

                CanStart = false;

                curStatus = pStatus;
                switch (pStatus)
                {
                    case PlcStatus.None:
                    case PlcStatus.Manual:
                        PLCStatusBrush = FlowItem.DefaultBrush;
                        break;
                    case PlcStatus.Init:
                        PLCStatusBrush = FlowItem.DefaultBrush;
                        break;
                    case PlcStatus.Inited:
                        PLCStatusBrush = FlowItem.SuccessBrush;
                        CanStart = true;
                        break;
                    case PlcStatus.AutoRun:
                        PLCStatusBrush = FlowItem.RunningBrush;
                        break;
                    case PlcStatus.Pause:
                        PLCStatusBrush = FlowItem.FailBrush;
                        break;
                    case PlcStatus.Alarm:
                        PLCStatusBrush = FlowItem.TimeoutBrush;
                        break;
                    default:
                        PLCStatusBrush = FlowItem.DefaultBrush;
                        commonBus.OnLog(LogType.Error, $"{Luster.Motion.Assests.Langs.LangProvider.GetLang("UnknownPLCStatus")}:{pStatus}");
                        break;
                }

                ShowPlcStatus();
            }));
        }

        /// <summary>
        /// 入料时间记录
        /// </summary>
        private void MotionEngineProLoadedEvent(IMotionModule module, StationResult station)
        {
            ProductLoadTimeMemory = DateTime.Now;
        }
        /// <summary>
        /// 抛料触发黄灯闪烁
        /// </summary>
        /// <param name="module"></param>
        /// <param name="station"></param>
        private void MotionEngineProThrowEventEvent(IMotionModule module, StationResult station)
        {
            ProductLoadTimeMemory = DateTime.Now;
        }

        private void ShowPlcStatus()
        {
            try
            {
                if (AppConfig.Lang == "zh-cn")
                {
                    PLCStatusText = $"PLC{curStatus.GetDescription()}";
                }
                else
                {
                    PLCStatusText = $"PLC{curStatus}";
                }
            }
            catch (Exception ex)
            {
                commonBus.OnLog(LogType.Error, $"{Luster.Motion.Assests.Langs.LangProvider.GetLang("UnknownPLCStatus")}:{curStatus},{ex.Message}");
            }
        }

        /// <summary>
        /// 状态文本
        /// </summary>
        private string _plcStatusText;
        public string PLCStatusText
        {
            get { return _plcStatusText; }
            set
            {
                SetProperty(ref _plcStatusText, value);
            }
        }

        /// <summary>
        /// 组合Smema 4种状态为十进制
        /// </summary>
        /// <param name="NextReq">下站要料</param>
        /// <param name="ThisHave">本站有料</param>
        /// <param name="ThisReq">本站要料</param>
        /// <param name="PreHave">上站有料</param>
        /// <returns></returns>
        private int SmemaToInt(bool NextReq, bool ThisHave, bool ThisReq, bool PreHave)
        {
            return Convert.ToInt32(NextReq) * 8 + Convert.ToInt32(ThisHave) * 4 + Convert.ToInt32(ThisReq) * 2 + Convert.ToInt32(PreHave) * 1;
        }


        /// <summary>
        /// 获取Plc地址
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        private bool GetPlcValue(string addr)
        {
            if (vPlc == null || string.IsNullOrEmpty(addr)) { return false; }

            try
            {
                return vPlc.ReadBool(addr);
            }
            catch (Exception)
            {
            }

            return false;
        }

        /// <summary>
        /// 写入PLC值，bool
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        private bool WritePlcValueBool(string addr, bool val)
        {
            if (vPlc == null || string.IsNullOrEmpty(addr)) { return false; }

            try
            {
                vPlc.Write(addr, val, $"01 05 {addr} 1");
            }
            catch (Exception)
            {
            }
            return true;
        }


        /// <summary>
        /// 写入PLC值，int
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        private bool WritePlcValueInt(string addr, short val)
        {
            if (vPlc == null || string.IsNullOrEmpty(addr)) { return false; }

            try
            {
                vPlc.Write(addr, val, $"01 06 {addr} 1");
            }
            catch (Exception)
            {
            }
            return true;
        }





        /// <summary>
        /// 获取Plc地址
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        private int GetPlcIntValue(string addr)
        {
            if (vPlc == null || string.IsNullOrEmpty(addr)) { return -1; }

            try
            {
                return (int)vPlc.ReadNumber(addr);
            }
            catch (Exception)
            {
            }

            return -1;
        }

        /// <summary>
        /// PLC初始化
        /// </summary>
        private DelegateCommand _initCommand;
        public DelegateCommand InitCommand => _initCommand ?? (new DelegateCommand(() =>
        {
        }));

        /// <summary>
        /// 启动PLC
        /// </summary>
        private DelegateCommand _startCommand;
        // 在ProPLCContentVM类中添加一个静态字段用于标记窗口是否已弹出
        private static bool _isExceptionDialogOpen = false;

        public DelegateCommand StartCommand => _startCommand ?? (new DelegateCommand(() =>
        {
            _mController.SysConfig.PlcStart(_deviceEngine);

        }, () => CanStart).ObservesCanExecute(() => CanStart));

        #region PLC 状态显示
        /// <summary>
        /// PLC 内容
        /// </summary>
        public string Name => "PLC";

        /// <summary>
        /// PLC 区域
        /// </summary>
        public string RegionName { get; set; } = "ProPLCContent";
        #endregion
    }
}
