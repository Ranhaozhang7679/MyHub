#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       MotionController
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.TaskFlow.Engine
* 文 件 名:       MotionController.cs
* 创建时间:       2022/5/19 20:37:20
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      b0ad9363-d3bc-4c81-9390-e89499645dfb
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/19 20:37:20
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataAccess.Repositories;
using Luster.Common.DataAccess.Tables;
using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.Dongle;
using Luster.Common.Tools;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.DataStruct.Network;
using Luster.Motion.DataStruct.Real;
using Luster.Motion.DataStruct.VDevice;
using Luster.Motion.TaskFlow.Engine.Engine;
using Luster.Motion.TaskFlow.Engine.HyperTrain;
using Luster.Motion.TaskFlow.Engine.Models;
using Luster.TaskFlow.Common;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Enums;
using Luster.TaskFlow.Motion.Interfaces;
using Luster.TaskFlow.Motion.Logic;
using Prism.Services.Dialogs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Media.Media3D;
using System.Xml.Linq;
using static FreeSql.Internal.GlobalFilter;

namespace Luster.Motion.TaskFlow.Engine
{
    public class MotionController : IMotionController
    {
        /// <summary>
        /// 软件第一次运行流程
        /// </summary>
        //private bool _firstStartFlag = true;

        public bool _firstStartFlag { get; set; }

        /// <summary>
        /// pv锁
        /// </summary>
        private object _pvLock = new object();

        /// <summary>
        /// tossig data锁
        /// </summary>
        private static readonly object _tossingDataLock = new object();


        /// <summary>
        /// 用户主动暂停流程标识
        /// </summary>
        private bool _activePause = false;

        /// <summary>
        ///稼动IO报警只设置一次
        /// </summary>
        private bool _AlemIOSet = false;

        private UploadProductionQuota _uploadProductionQuota;

        /// <summary>
        /// 
        /// </summary>
        public ManualResetEventSlim pauseReset1 = new ManualResetEventSlim(true);

        /// <summary>
        /// 产品信息统计
        /// </summary>
        private ProductStat _productStat = new ProductStat();

        /// <summary>
        /// 稼动率统计
        /// </summary>
        private ImpactData _impactData = new ImpactData();

        /// <summary>
        /// 流程引擎
        /// </summary>
        public IMotionEngine MotionEngine { get; set; }

        /// <summary>
        /// 引擎状态
        /// </summary>
        public EngineStatus MachineStatus
        {
            get
            {
                if (MotionEngine == null)
                {
                    return EngineStatus.Idle;
                }
                else
                {
                    return MotionEngine.EngineStatus;
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        public SystemOperation systemOperation;

        /// <summary>
        /// 入料缓存，用于出料时匹配
        /// </summary>
        private ConcurrentDictionary<string, ProductInfo> _productDic = new ConcurrentDictionary<string, ProductInfo>();

        /// <summary>
        /// 设备引擎
        /// </summary>
        private IDeviceEngine _deviceEngine;

        /// <summary>
        /// 系统配置
        /// </summary>
        private SystemConfig _systemConfig = null;
        public SystemConfig SysConfig
        {
            get => _systemConfig;
            set
            {
                _systemConfig = value;
                _lightManager?.SetSysConfig(value);
            }
        }

        private FileConfig _fileConfig;
        public FileConfig FileConfig
        {
            get => _fileConfig;
            set
            {
                _fileConfig = value;
            }
        }

        /// <summary>
        /// 弹窗提示
        /// </summary>
        public event Action<bool> CanAutoRunEvent;

        /// <summary>
        /// 回零事件
        /// </summary>
        public event Func<HomeType> StartHomeEvent;

        /// <summary>
        /// 回零事件
        /// </summary>
        public event Action<string> HomeEndEvent;

        /// <summary>
        /// 设备启动事件
        /// </summary>
        public event Action<string> MachineStartEvent;

        /// <summary>
        /// 设备停止事件
        /// </summary>
        public event Action<string> MachineStopEvent;

        /// <summary>
        /// 设备暂停事件
        /// </summary>
        public event Action<string> MachinePauseEvent;

        /// <summary>
        /// 设备手自动切换事件
        /// </summary>
        public event Action<string> MachineManualEvent;

        /// <summary>
        /// 设备保养事件
        /// </summary>
        public event Action MachinenMaintenanceEvent;


        /// <summary>
        /// 报警结束
        /// </summary>
        public event Action<AlarmInfo, bool> AlarmProcEvent;

        /// <summary>
        /// 报警信息清除/记录
        /// true:清除 false:记录
        /// </summary>
        public event Action<List<AlarmInfo>, bool> AlarmInfosUpEvent;

        /// <summary>
        /// 从DB中获取报警信息
        /// </summary>
        public event Action<List<AlarmInfo>> AlarmInfosGetEvent;

        /// <summary>
        /// Plc 报警
        /// </summary>
        public event Action<PlcStatus> PlcStatusEvent;

        /// <summary>
        /// 报警对象
        /// </summary>
        public AlarmInfo AlarmInfo { get; set; }

        /// <summary>
        /// 报警集合
        /// </summary>
        public List<AlarmInfo> AlarmInfos { get; set; } = new List<AlarmInfo>();

        /// <summary>
        /// 传递引擎状态变化 用于新首页状态更新
        /// </summary>
        public event Action<object, EngineStatus, EngineStatus> EngineStatusEvent;

        /// <summary>
        /// 首先产品信息更新事件
        /// </summary>
        public event Action<ProductStat, ImpactData> ProStatEvent;

        /// <summary>
        /// 首页趋势图更新事件
        /// </summary>
        public event Action<ProductInfo, bool, double> ProductEvent;


        /// <summary>
        /// 小料触发事件
        /// </summary>
        public event Action<StationResult> ProductTrowEvent;

        /// <summary>
        /// 机台模式切换
        /// </summary>
        public event Action<string, string> ModeChangedEvent;


        /// <summary>
        /// 网络状态切换
        /// </summary>
        public event Action<string, bool, string> NetStatusChangedEvent;



        /// <summary>
        /// 产品出料时间表
        /// </summary>
        private List<DateTime> _productUnloadedTimeList = new List<DateTime>();

        /// <summary>
        /// 数据
        /// </summary>
        private const int dataLength = 21;

        /// <summary>
        /// 配置管理器
        /// </summary>
        private readonly IConfigManager _configManager;

        /// <summary>
        /// 灯管理
        /// </summary>
        private readonly ILightManager _lightManager;
        private readonly IDialogService service;

        /// <summary>
        /// Web服务
        /// </summary>
        public IWebService WebService { get; set; }

        /// <summary>
        /// PLC对象
        /// </summary>
        private VPlc vPlc = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="_facotry">模块工程</param>
        public MotionController(IMotionEngine motionEngine, IDeviceEngine deviceEngine,
            IConfigManager cManger, ILightManager lightManager,
            IWebService webService,
            IDialogService service)
        {
            MotionEngine = motionEngine;
            WebService = webService;
            this.service = service;
            _uploadProductionQuota = new UploadProductionQuota();
            motionEngine.ProLoadedEvent -= MotionEngine_ProLoadedEvent;
            motionEngine.ProLoadedEvent += MotionEngine_ProLoadedEvent;

            motionEngine.ProUnloadedEvent -= MotionEngine_ProUnloadedEvent;
            motionEngine.ProUnloadedEvent += MotionEngine_ProUnloadedEvent;

            motionEngine.ProThrowEvent -= MotionEngine_ProThrowEvent;
            motionEngine.ProThrowEvent += MotionEngine_ProThrowEvent;

            motionEngine.AlarmEvent_MEngine -= OnAlarm;
            motionEngine.AlarmEvent_MEngine += OnAlarm;

            motionEngine.EngineStatusEvent -= MotionEngine_EngineStatusEvent;
            motionEngine.EngineStatusEvent += MotionEngine_EngineStatusEvent;


            motionEngine.StationTimeEvent -= MotionEngine_StationTimeEvent;
            motionEngine.StationTimeEvent += MotionEngine_StationTimeEvent;

            _lightManager = lightManager;
            _deviceEngine = deviceEngine;

            // 安全位报警，需要暂停程序
            _deviceEngine.AlarmEvent_device -= OnAlarm;
            _deviceEngine.AlarmEvent_device += OnAlarm;

            _deviceEngine.PrevDeleteEvent -= DeviceEngine_PrevDeleteEvent;
            _deviceEngine.PrevDeleteEvent += DeviceEngine_PrevDeleteEvent;

            _deviceEngine.AxisPosDeleteEvent -= DeviceEngine_AxisPosDeleteEvent;
            _deviceEngine.AxisPosDeleteEvent += DeviceEngine_AxisPosDeleteEvent;

            // 属性变更事件
            _deviceEngine.PropertyChangedEvent -= OnPropertyChanged;
            _deviceEngine.PropertyChangedEvent += OnPropertyChanged;
            _deviceEngine.GetMachineStatusEvent -= DeviceEngine_GetMachineStatusEvent;
            _deviceEngine.GetMachineStatusEvent += DeviceEngine_GetMachineStatusEvent;

            _deviceEngine.GetModuleListEvent -= DeviceEngine_GetModuleListEvent;
            _deviceEngine.GetModuleListEvent += DeviceEngine_GetModuleListEvent;
            _deviceEngine.UpdateAlarmModuleParamsEvent -= DeviceEngine_UpdateAlarmModuleParamsEvent;
            _deviceEngine.UpdateAlarmModuleParamsEvent += DeviceEngine_UpdateAlarmModuleParamsEvent;
            _deviceEngine.GetPDCAModulesEvent -= DeviceEngine_GetPDCAModulesEvent;
            _deviceEngine.GetPDCAModulesEvent += DeviceEngine_GetPDCAModulesEvent;
            _deviceEngine.GetSFCModulesEvent -= DeviceEngine_GetSFCModulesEvent;
            _deviceEngine.GetSFCModulesEvent += DeviceEngine_GetSFCModulesEvent;

            SysConfig = new SystemConfig();
            FileConfig = new FileConfig();
            _configManager = cManger;
            _configManager.SetWebConfig(WebService.GetConfig());
            //获取PLC对象
            vPlc = _deviceEngine.GetVDevices<VPlc>().FirstOrDefault();
            _firstStartFlag = true;

            // 从数据库加载报警信息，初始化报警列表
            AlarmInfosGetEvent?.Invoke(AlarmInfos);
        }

        /// <summary>
        /// 获取机台状态
        /// </summary>
        /// <returns></returns>
        private EngineStatus DeviceEngine_GetMachineStatusEvent()
        {
            return MachineStatus;
        }


        private void MotionEngine_StationTimeEvent(string WIP, string InputTime, string OutputTime, bool Result)
        {
            StationTimeEvent?.Invoke(WIP, InputTime, OutputTime, Result);
        }
        /// <summary>
        /// 点位删除，进行引用判断
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private string DeviceEngine_AxisPosDeleteEvent(AxisPosition axisPos)
        {
            string errMsg = string.Empty;

            foreach (var item in MotionEngine)
            {
                foreach (var kVal in item.Parameters)
                {
                    var pAttr = kVal.Value;
                    if (pAttr.Value is VAxisPos vPos && vPos.Any(u => u.PosKey == axisPos.Key))
                    {
                        errMsg = $"点位:{axisPos.Name} 被模块{item.Alias}引用,不能删除!";
                        break;
                    }
                }
            }

            return errMsg;
        }

        /// <summary>
        /// 设备是否被模块引用
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private string DeviceEngine_PrevDeleteEvent(DataStruct.Virtual.IVirtualDevice arg)
        {
            string errMsg = string.Empty;

            foreach (var item in MotionEngine)
            {
                foreach (var kVal in item.Parameters)
                {
                    var pAttr = kVal.Value;
                    if (pAttr.Value is VDevice vDevice && vDevice.DeviceID == arg.ID)
                    {
                        errMsg = item.Alias;
                        break;
                    }
                }
            }

            return errMsg;
        }

        /// <summary>
        /// 传递引擎状态，用于新首页状态显示
        /// </summary>
        /// <param name="obj"></param>
        private bool MotionEngine_EngineStatusEvent(EngineStatus src, EngineStatus obj)
        {
            EngineStatusEvent?.Invoke(this, src, obj);
            return true;
        }



        /// <summary>
        /// 报警信息
        /// </summary>
        /// <param name="alarmInfo"></param>
        private void OnAlarm(AlarmInfo alarmInfo)
        {
            // 如果机台已经处于报警状态，需要对当前报警进行清除
            if (alarmInfo == null) return;  //Hive2.4合入变更

            #region 报警信息处理
            //防止多线程同时执行导致数据重复赋值出错
            lock (this)
            {
                if (alarmInfo.AlarmType != AlarmType.InfoTip) // && alarmInfo.AlarmType != AlarmType.PlcAlarm)
                {
                    if (!_AlemIOSet)
                    {
                        MotionEngine.OnLog(LogType.Error, $"错误类型：{alarmInfo.AlarmType} 错误信息:{alarmInfo.Message}触发停止稼动IO");
                        CloseOperateIO(SystemOperation.Alarm);
                    }
                }
            }

            if (AlarmInfo == null)
            {
                // 记录机台的状态
                alarmInfo.EStatus = MachineStatus;
            }
            else
            {
                if (alarmInfo.AlarmCode == AlarmInfo.AlarmCode && alarmInfo.AlarmType == AlarmInfo.AlarmType)
                {
                    alarmInfo.AlarmID = AlarmInfo.AlarmID;
                }

                alarmInfo.EStatus = AlarmInfo.EStatus;
            }

            // 回零报警，则直接停止流程
            /*if (MachineStatus == EngineStatus.Homing)
            {
                MotionEngine.OnLog(LogType.Error, $"回零异常:{alarmInfo.Message}触发软件停止");

                // 流程停止
                Stop();

                return;
            }

            // 如果回零过程中报警，则停止流程
            if (alarmInfo.AlarmType == AlarmType.DeviceError)
            {
                MotionEngine.OnLog(LogType.Error, $"设备异常:{alarmInfo.Message}触发软件停止");

                // 流程停止
                Stop(() =>
                {
                    // 蜂鸣器响
                    _lightManager.SetBuzzer(true);
                });
            }*/

            // 其他功能
            if (alarmInfo.AlarmType == AlarmType.WarningTip ||
                alarmInfo.AlarmType == AlarmType.FailError ||
                alarmInfo.AlarmType == AlarmType.Timeout ||
                alarmInfo.AlarmType == AlarmType.PopInfoTip||
                alarmInfo.AlarmType == AlarmType.DeviceError)

            {
                // 流程暂停
                Pause(true, () =>
                {
                    // 蜂鸣器响
                    _lightManager.SetBuzzer(true);
                });
            }

            // 记录最后一笔报警信息
            if (alarmInfo.AlarmType != AlarmType.InfoTip) //&& alarmInfo.AlarmType != AlarmType.PlcAlarm)
            {
                AlarmInfo = alarmInfo;

                // 将报警添加到集合中
                AlarmInfos.Add(alarmInfo);
            }

            // PLC 报警，需要红灯亮，蜂鸣器响
            if (alarmInfo.AlarmType == AlarmType.PlcAlarm)
            {
                _lightManager.StopLight(true);
                //_lightManager.SetBuzzer(true, 400, 400);
            }

            if (AlarmInfos.Count > 0)
            {
                //保存数据库
                AlarmInfosUpEvent?.Invoke(AlarmInfos, false);
            }
            #endregion
        }

        /// <summary>
        /// 重新计数
        /// </summary>
        public void ClearProductInfo()
        {
            // 稼动率重新计算
            _impactData = new ImpactData(DateTime.Now);

            // 产能清零
            if (_productStat != null)
            {
                _productStat.AllCount = 0;
                _productStat.ThrowMaterialDic = new Dictionary<string, int>();
                #region add_tossing_data
                _productStat.ThrowMaterialDicDetail?.Clear();
                _productStat.ThrowMaterialDicDetail = new Dictionary<string, List<(string, string)>>();
                #endregion
                _productStat.OKCount = 0;
                _productStat.NGCount = 0;
                _productStat.RealCT = SysConfig.CT;
                //_productStat.UPH = 0;
                _productStat.Toss = 0;
            }

            // CT重新计算
            if (_productUnloadedTimeList.Count > 0)
            {
                _productUnloadedTimeList.Clear();
            }

            // 产品数据刷新
            ProStatEvent?.Invoke(_productStat, _impactData);
        }

        /// <summary>
        /// 更新产能
        /// </summary>
        /// <param name="productStat"></param>
        public void SetProductInfo(ProductStat productStat)
        {
            _productStat = productStat;
        }

        /// <summary>
        /// 获取产能上传数据
        /// </summary>
        /// <returns></returns>
        public UploadProductionQuota GetUploadProductionQuota(DateTime startTime, DateTime endTime)
        {
            var model = _uploadProductionQuota.GetUploadProductionQuotaByTime(startTime, endTime);
            var timeSpan = endTime.Subtract(startTime).TotalSeconds;
            model.TargetUph = SysConfig.CT == 0 ? 0 : (int)(timeSpan / SysConfig.CT);
            model.RealUph = _productStat.RealCT == 0 ? model.TargetUph : (int)(timeSpan / _productStat.RealCT);
            return model;
        }

        private void MotionEngine_ProThrowEvent(StationResult stationResult, string module, string material)
        {
            if (_productStat != null)
            {
                var throwDic = _productStat.ThrowMaterialDic;
                if (throwDic.Keys.Contains(material))
                {
                    throwDic[material] = throwDic[material] + 1;
                }
                else
                {
                    throwDic.Add(material, 1);
                }
                _uploadProductionQuota.AddTossingProductTime(DateTime.Now, material);

                #region add_tossing_data
                var throwDicDetail = _productStat.ThrowMaterialDicDetail;
                if (throwDicDetail.Keys.Contains(material))
                {
                    lock (_tossingDataLock)
                    {
                        throwDicDetail[material].Add((stationResult.ErrMsg, stationResult.ProCode));
                    }
                }
                else
                {
                    lock (_tossingDataLock)
                    {
                        throwDicDetail.Add(material, new List<(string, string)>() { (stationResult.ErrMsg, stationResult.ProCode) });
                    }
                }
                #endregion

                lock (_pvLock)
                {
                    var tossing = SysConfig.TossingTime > 0 ? SysConfig.TossingTime : 1;
                    _impactData.TossingTime += tossing;
                }

                Task.Run(() =>
                {
                    // 触发抛料产品信息变更
                    ProStatEvent?.Invoke(_productStat, _impactData);

                    // 触发抛料时间
                    ProductTrowEvent?.Invoke(stationResult);
                });
            }
        }

        /// <summary>
        /// 出料事件处理
        /// </summary>
        /// <param name="station"></param>
        /// <param name="data"></param>
        private void MotionEngine_ProUnloadedEvent(IMotionModule module, StationResult station, Dictionary<string, object> data, Action<IMotionModule, StationResult, Dictionary<string, object>, double> callback)
        {
            if (station == null || _productStat == null) return;

            // 当前产品出料时间
            var proUnloadTime = DateTime.Now;

            if (_productUnloadedTimeList.Count > dataLength)
            {
                _productUnloadedTimeList.RemoveAt(0);
            }

            _productUnloadedTimeList.Add(proUnloadTime);

            // 计数
            _productStat.AllCount++;

            // UHP++
            _productStat.SetUPH();

            // 产品是否发生
            if (station.IsToss)
            {
                _productStat.Toss++;
            }
            else
            {
                if (station.Result)
                {
                    _productStat.OKCount++;
                    _uploadProductionQuota.AddOKProductTime(proUnloadTime);
                }
                else
                {
                    _productStat.NGCount++;
                    _uploadProductionQuota.AddNGProductTime(proUnloadTime);
                }
            }
            double CT = 0.0;
            if (_productUnloadedTimeList.Count > 1)
            {
                // 计算时间
                int count = _productUnloadedTimeList.Count;
                var allCT = (_productUnloadedTimeList[count - 1] - _productUnloadedTimeList[0]).TotalSeconds;
                var realCT = Math.Round(allCT / (count - 1), 3);
                CT = Math.Round((_productUnloadedTimeList[count - 1] - _productUnloadedTimeList[count - 2]).TotalSeconds, 2);
                // 计算每小时的产能数据
                _productStat.SetRealCT(realCT);
            }
            else
            {
                // 标准CT
                _productStat.SetRealCT(SysConfig.CT);
            }

            // 更新出料结果信息
            _productStat.Result = station.Result;


            // 出料事件放到异步方法中，防止后处理太耗时
            Task.Run(() =>
            {
                #region add_tossing_data
                var throwDicDetail = _productStat.ThrowMaterialDicDetail;
                lock (_tossingDataLock)
                {
                    if (throwDicDetail.Count() > 0)
                    {
                        station.TossDatas = new List<(string, string)>();
                        foreach (var item in throwDicDetail)
                        {
                            station.TossDatas.AddRange(item.Value);
                        }
                        throwDicDetail.Clear();
                    }
                }
                #endregion

                //上传DB
                callback?.Invoke(module, station, data, CT);

                ProStatEvent?.Invoke(_productStat, _impactData);

                // 构建产品对象
                OnProductInfoUnload(proUnloadTime, station, data, CT);
            });
        }

        /// <summary>
        /// 构建产品信息出料信息
        /// </summary>
        /// <param name="unloadTime">产品出料信息</param>
        /// <param name="station"></param>
        /// <param name="data"></param>
        private void OnProductInfoUnload(DateTime unloadTime, StationResult station, Dictionary<string, object> data, double CT)
        {
            // 构建出料产品对象
            ProductInfo pInfo = null;
            if (!string.IsNullOrEmpty(station.ProCode))
            {
                _productDic.TryRemove(station.ProCode, out pInfo);
            }

            if (pInfo != null)
            {
                pInfo.Data = data;
                pInfo.Result = station;
                pInfo.OutTime = unloadTime;
                pInfo.ImagePath = station.ImagePath;
            }
            else
            {
                pInfo = new ProductInfo()
                {
                    Data = data,
                    Result = station,
                    Barcode = station.ProCode,
                    EnterTime = station.EnterTime,
                    OutTime = unloadTime,
                    ImagePath = station.ImagePath
                };
            }

            ProductEvent?.Invoke(pInfo, true, CT);
        }

        /// <summary>
        /// 入料事件处理
        /// </summary>
        /// <param name="station"></param>
        private void MotionEngine_ProLoadedEvent(IMotionModule module, StationResult station)
        {
            var pInfo = new ProductInfo()
            {
                Result = station,
                Barcode = station.ProCode,
                EnterTime = station.EnterTime,
            };

            _productDic.TryAdd(station.ProCode, pInfo);

            _uploadProductionQuota.AddProductInTime(station.EnterTime);
        }

        /// <summary>
        /// 启动
        /// </summary>
        public bool Start()
        {
            if (!CanAutoRun(out var msg)) throw new FriendlyException(msg);

            //本地测试观察五个机台Hive弹窗应用
            //DialogParameters param = new DialogParameters();
            //param.Add("Title", "Hive");
            //param.Add("Operation", SystemOperation.Home);
            //service.Show("HiveStartDialog2", param, null);

            // 0 首先运行开始工站
            //_deviceEngine.OnLog(LogType.Info, "首先执行开始工站");
            //MotionEngine.RunStartStation();

            AlarmInfo = null;
            RecoveryIOSetInit();
            if (MachineStatus == EngineStatus.Pause)
            {
                _deviceEngine.OnLog(LogType.Info, $"当前lockcommand值1：{lockCommand}!");
                _deviceEngine.OnLog(LogType.Info, "当前暂停状态,即将启动");

                // 防止连续点击 Hive 2.4合入
                if (lockCommand > 0) { _deviceEngine.OnLog(LogType.Info, $"暂停lockcommand次数>0,:{lockCommand}，直接退出!"); return false; }
                Interlocked.Increment(ref lockCommand);
                _deviceEngine.OnLog(LogType.Info, $"lockCommand为{lockCommand}");
                //_deviceEngine.OnLog(LogType.Info, "首先执行开始工站");
                //MotionEngine.RunStartStation();

                // PLC 和监控要进行复位检查
                if (_deviceEngine.Recovery())
                {
                    _deviceEngine.OnLog(LogType.Info, "MotionEngine.Recovery成功");
                    // 防止连续点击
                    /*if (lockCommand > 0) { _deviceEngine.OnLog(LogType.Info, $"暂停lockcommand次数>0,:{lockCommand}，直接退出!"); return; }
                    Interlocked.Increment(ref lockCommand);
                    _deviceEngine.OnLog(LogType.Info, $"lockCommand为{lockCommand}");*/
                    MotionEngine.Reset(AlarmInfo, (isOK) =>
                    {
                        _deviceEngine.OnLog(LogType.Info, "Reset成功");
                        MotionEngine.Recovery((isOK) =>
                        {
                            _deviceEngine.OnLog(LogType.Info, "MotionEngine.Recovery成功");
                            // 1.Plc恢复
                            //SysConfig?.PlcStart(_deviceEngine);

                            // 4.防止连续点击
                            Interlocked.Decrement(ref lockCommand);
                            _deviceEngine.OnLog(LogType.Info, $"暂停lockcommand复位：{lockCommand}!");

                            // 2.启动灯
                            _lightManager.RunningLight();
                        });

                        // 程序经过暂停->恢复，需要刷新稼动率
                        _impactData.UpdateRecoveryTime();
                    });
                    _deviceEngine.OnLog(LogType.Info, "即将关闭机器人所有信号");
                    //机器人所有信号关闭
                    SetRobotDO(false);

                    //机器人清错
                    ProcessIO(SysConfig.RobotClearFault, io => io.SetDigital(true));

                    //机器人复位
                    ProcessIO(SysConfig.RobotReset, io => io.SetDigital(true));

                    //机器人开始
                    ProcessIO(SysConfig.RobotStart, io => io.SetDigital(true));

                    //机器人2清错
                    ProcessIO(SysConfig.RobotClearFault2, io => io.SetDigital(true));

                    //机器人2复位
                    ProcessIO(SysConfig.RobotReset2, io => io.SetDigital(true));

                    //机器人2开始
                    ProcessIO(SysConfig.RobotStart2, io => io.SetDigital(true));

                    LockDoor(true);

                    //恢复暂停被关掉的ON的DIO
                    RecoveryOperateIO();
                    _deviceEngine.OnLog(LogType.Info, "复位所有完成");
                    return true;
                }
                else
                {
                    _deviceEngine.OnLog(LogType.Info, "MotionEngine.Recovery失败");
                    // 防止连续点击
                    Interlocked.Decrement(ref lockCommand);
                    _deviceEngine.OnLog(LogType.Info, $"暂停lockcommand复位：{lockCommand}!");
                    return false;
                }
            }
            else if (MachineStatus == EngineStatus.Ready)
            {
                _deviceEngine.OnLog(LogType.Info, $"当前lockcommand值2：{lockCommand}!");
                _deviceEngine.OnLog(LogType.Info, "当前就绪状态,即将启动");
                // 如果程序已经在运行了，不允许在点击开始按钮
                if (MachineStatus == EngineStatus.Running)
                {
                    _deviceEngine.OnLog(LogType.Info, "已经是启动状态，直接退出!");
                    return false;
                }
                // 防止连续点击
                if (lockCommand > 0) { _deviceEngine.OnLog(LogType.Info, $"lockcommand次数>0,:{lockCommand}，直接退出!"); return false; }
                Interlocked.Increment(ref lockCommand);
                _deviceEngine.OnLog(LogType.Info, $"监控lockcommand次数，当前为:{lockCommand}");
                _deviceEngine.OnLog(LogType.Info, "首先执行开始工站");
                // 记录执行前的报警数量
                int alarmCountBefore = AlarmInfos.Count;
                // 开始工站可能涉及通信等待，放到后台线程避免阻塞UI
                bool startResult = false;
                var frame = new DispatcherFrame();
                Task.Run(() =>
                {
                    try
                    {
                        MotionEngine.RunStartStation();
                        // 开始工站执行后检查是否产生了新的报警（OnAlarm 中 AlarmInfo 赋值是同步的，不受 Ready 状态限制）
                        if (AlarmInfos.Count > alarmCountBefore)
                        {
                            _deviceEngine.OnLog(LogType.Info, "开始工站执行后产生报警，不允许启动!");
                            Interlocked.Decrement(ref lockCommand);
                            _deviceEngine.OnLog(LogType.Info, $"readylockcommand复位报警：{lockCommand}!");
                            return;
                        }
                        _deviceEngine.OnLog(LogType.Info, $"开始检查轴状态!");
                        // 1.Plc启动
                        // 设备回零检查
                        if (!_deviceEngine.IsHome(out var errMsg))
                        {
                            _deviceEngine.OnLog(LogType.Info, "存在轴未回零完成，不允许启动!");
                            Interlocked.Decrement(ref lockCommand);
                            _deviceEngine.OnLog(LogType.Info, $"readylockcommand复位1：{lockCommand}!");
                            return;
                        }

                        _deviceEngine.OnLog(LogType.Info, $"记录班别!");
                        // 记录当前班别信息
                        currentClass = SysConfig.GetCurrentClass(DateTime.Now);

                        _deviceEngine.OnLog(LogType.Info, $"稼动率刷新!");

                        // 2.稼动率刷新
                        if (_firstStartFlag)
                        {
                            _firstStartFlag = false;

                            //ClearProductInfo();//53AE要求只能手动清零
                            // 稼动率重新计算
                            _impactData = new ImpactData(DateTime.Now);

                        }
                        else
                        {
                            // 程序经过停止->回零->启动，需要刷新稼动率
                            _impactData.UpdateRecoveryTime();
                        }
                        // 3.流程启动
                        _deviceEngine.OnLog(LogType.Info, $"运行工站!");
                        MotionEngine.RunStations();

                        _deviceEngine.OnLog(LogType.Info, $"运行三色灯!");
                        // 4.启动灯
                        _lightManager.RunningLight();

                        _deviceEngine.OnLog(LogType.Info, $"关闭机器人信号!");
                        // 5.机器人所有信号关闭
                        SetRobotDO(false);

                        _deviceEngine.OnLog(LogType.Info, $"启动机器人!");
                        // 6.机器人启动
                        ProcessIO(SysConfig.RobotStart, io => io.SetDigital(true));
                        ProcessIO(SysConfig.RobotStart2, io => io.SetDigital(true));

                        // 7.防止连续点击
                        Interlocked.Decrement(ref lockCommand);
                        _deviceEngine.OnLog(LogType.Info, $"readylockcommand复位2：{lockCommand}!");
                        _deviceEngine.OnLog(LogType.Info, $"关闭门锁!");
                        LockDoor(true);
                        startResult = true;
                    }
                    finally
                    {
                        frame.Continue = false;
                    }
                });
                Dispatcher.PushFrame(frame);
                return startResult;
            }
            else
            {
                _deviceEngine.OnLog(LogType.Info, "当前状态不允许启动!");
                return false;
            }

        }

        /// <summary>
        /// 开锁
        /// </summary>
        /// <param name="isLock"></param>
        private void LockDoor(bool isLock)
        {
            // 默认使用配置的门锁
            if (SysConfig.ListOpenCloseDoor != null && SysConfig.ListOpenCloseDoor.Count > 0)
            {
                var door = SysConfig.ListOpenCloseDoor.FirstOrDefault(u => u.DoorType == DoorType.DoorLock);
                if (door != null)
                {
                    foreach (var item in door.ListLockIO)
                    {
                        ProcessIO(item, io => io.SetDigital(isLock));
                    }
                }
            }
            else
            {
                var doorList = _deviceEngine.GetVDevices<VIO>().Where(u => u.Behavior == IOBehavior.Output && u.Name.Contains("锁")).ToList();
                foreach (var item in doorList)
                {
                    item.SetDigital(isLock);
                }
            }
        }

        /// <summary>
        /// 是否处于自动运行
        /// </summary>
        /// <returns></returns>
        public bool CanAutoRun(out string mesage)
        {
            bool isAuto = false;
            bool ioChecked = false;
            string error = "";
            mesage = "";
            // 如果有配置自动/手动,检查按钮
            ProcessIO(SysConfig.ManualSwitchButton, (r) =>
            {
                ioChecked = true;
                isAuto = r.GetDigitalIn();

                if (!isAuto)
                {
                    error = "机台处于手动状态,不允许自动运行!";
                }
            });

            // 未配置手动/自动切换按钮时，默认允许自动运行
            if (!ioChecked)
            {
                isAuto = true;
            }

            mesage = error;
            return isAuto;
        }

        /// <summary>
        /// 安全门是否关闭
        /// </summary>
        /// <returns></returns>
        public bool DoorIsClosed(out string message)
        {
            bool isClosed = true;
            message = "";
            VIO alarmIO = null;
            if (SysConfig.EnableSaftyDoor)
            {
                if (SysConfig.ListOpenCloseDoor != null && SysConfig.ListOpenCloseDoor.Count > 0)
                {
                    var door = SysConfig.ListOpenCloseDoor.FirstOrDefault(u => u.DoorType == DoorType.DoorContact);
                    if (door != null)
                    {
                        foreach (var item in door.ListLockIO)
                        {
                            ProcessIO(item, io =>
                            {
                                isClosed = isClosed && io.GetDigitalIn();
                                if (isClosed)
                                {
                                    alarmIO = io;
                                }
                            });
                        }
                    }
                }

                if (!isClosed)
                {
                    //OnAlarm(new AlarmInfo(this, AlarmType.InfoTip, alarmIO.Name + "被打开", alarmIO.Errors[DeviceError.SensorFail]));

                    // 修改建议：alarmIO 可能为 null，alarmIO.Errors 也可能为 null 或未包含指定键，需做空值判断
                    string alarmName = alarmIO != null ? alarmIO.Name : "未知IO";
                    string alarmError = (alarmIO != null && alarmIO.Errors != null && alarmIO.Errors.ContainsKey(DeviceError.SensorFail))
                        ? alarmIO.Errors[DeviceError.SensorFail]
                        : "未知错误";
                    OnAlarm(new AlarmInfo(this, AlarmType.InfoTip, alarmName + "被打开", alarmError));
                    message = "安全门未关闭!";
                }
            }
            return isClosed;
        }

        private void Lock(bool isLock = true)
        {
            _deviceEngine.OnLog(LogType.Info, $"当前lockcommand值3：{lockCommand}!");
            if (isLock)
            {
                Interlocked.Increment(ref lockCommand);
                _deviceEngine.OnLog(LogType.Info, $"Locklockcommand置位：{lockCommand}!");
            }
            else
            {
                Interlocked.Decrement(ref lockCommand);
                _deviceEngine.OnLog(LogType.Info, $"Locklockcommand复位：{lockCommand}!");
            }
        }

        /// <summary>
        /// 停止\做完最后一片产品
        /// </summary>
        public void Stop(Action action = null)
        {
            IsHoming = false;

            pauseReset1?.Set();
            if (MachineStatus == EngineStatus.Stop) return;

            // 防止连续点击
            if (lockCommand > 0) return;
            Lock();

            // 记录停止时间
            _impactData.UpdatePauseTime();

            // 先停止流程，防止多走
            MotionEngine.Stop(() =>
            {
                // 流程调试，再停止硬件
                _deviceEngine.Stop();

                //_lightManager.StopLight(false);
                _lightManager.UnHome();

                //机器人所有信号关闭
                SetRobotDO(false);

                //机器人停止
                ProcessIO(SysConfig.RobotStop, io => io.SetDigital(true));

                // 停止流程时主动设置光栅检测全局变量为有效
                LightCurtainProcess(item =>
                {
                    if (item.GlobalParameter != null)
                    {
                        item.GlobalParameter.Value = true;
                    }
                });

                action?.Invoke();

                // 停止时清理活跃报警，写入EndTime
                clearAlarm();

                // 防止连续点击
                Lock(false);

                //停止时，关闭需要关闭的IO
                //CloseOperateIO(SystemOperation.Pause);

            });
        }

        /// <summary>
        /// 设备复位
        /// </summary>
        public void Recovery()
        {
            _deviceEngine.OnLog(LogType.Info, $"当前lockcommand值4：{lockCommand}!");
            //Hive 2.4合入
            if (lockCommand > 0)
            {
                _deviceEngine.OnLog(LogType.Info, $"由于当前正在进行其他流程，不允许执行复位流程!");
                return;
            }
            if (!DoorIsClosed(out var errMsg))
            {
                throw new FriendlyException(errMsg);
            }
            // 关闭报警
            _lightManager.SetBuzzer(false);

            // 由于PLC报警，此时机台运行中，但是报警灯处于红色，所以需要关闭红灯，开绿灯
            if (MachineStatus == EngineStatus.Running)
            {
                _lightManager.RunningLight();
            }

            // 只有报警状态才支持复位
            if (MachineStatus != EngineStatus.Alarm) return;

            // 先对硬件安全门进行检查
            if (_deviceEngine.Recovery())
            {
                // 急停状态检测
                if (IsEmg)
                {
                    AlarmProcEvent?.Invoke(AlarmInfo, false);
                    throw new FriendlyException("设备急停,请先停止软件,进行回零!");
                }

                // 安全光栅检测
                if (!CheckLightCurtain())
                {
                    AlarmProcEvent?.Invoke(AlarmInfo, false);
                    return;
                }

                // PLC状态检测
                if (SysConfig.PlcIsAlarm(_deviceEngine, out var plcErr))
                {
                    AlarmProcEvent?.Invoke(AlarmInfo, false);
                    throw new FriendlyException($"请先清除PLC报警:{plcErr}");
                }

                // 防止连续点击复位
                if (lockCommand > 0) return;
                Interlocked.Increment(ref lockCommand);
                _deviceEngine.OnLog(LogType.Info, $"Recoverylockcommand置位：{lockCommand}!");

                pauseReset1?.Reset();
                MotionEngine.Reset(AlarmInfo, (isResult) =>
                {

                    if (isResult)
                    {
                        AlarmProcEvent?.Invoke(AlarmInfo, true);

                        clearAlarm();

                        // 复位成功，要启动灯闪烁
                        _lightManager.ReadyLight();
                    }
                    else
                    {

                    }

                    // 防止连续点击复位
                    Interlocked.Decrement(ref lockCommand);

                    _deviceEngine.OnLog(LogType.Info, $"Recoverylockcommand复位：{lockCommand}!");
                    pauseReset1?.Set();
                    if (!isResult)
                    {
                        closeOtherAlarms();
                        AlarmProcEvent?.Invoke(AlarmInfo, false);
                        OnAlarm(AlarmInfo);
                    }


                });


                //机器人所有信号关闭
                SetRobotDO(false);

                //机器人清错
                ProcessIO(SysConfig.RobotClearFault, io => io.SetDigital(true));
                ProcessIO(SysConfig.RobotClearFault2, io => io.SetDigital(true));
                //机器人复位
                ProcessIO(SysConfig.RobotReset, io => io.SetDigital(true));
                ProcessIO(SysConfig.RobotReset2, io => io.SetDigital(true));

                // 锁门
                LockDoor(true);
            }
        }

        private void clearAlarm()
        {
            // 记录报警时间
            AlarmInfo = null;
            // 如果 alarmInfos 存在元素，则对每个元素检查是否有 EndTime，若 EndTime 为 null 或默认值则赋值为当前时间
            if (AlarmInfos != null && AlarmInfos.Count > 0)
            {
                foreach (var item in AlarmInfos)
                {
                    if (item.EndTime == null || item.EndTime == default(DateTime))
                    {
                        AlarmProcEvent?.Invoke(item, true);
                    }
                }
            }
            AlarmInfos?.Clear();
            AlarmInfosUpEvent?.Invoke(AlarmInfos, true);
        }

        /// <summary>
        /// 关闭除当前报警外的其他历史报警（用于复位失败场景）
        /// </summary>
        private void closeOtherAlarms()
        {
            if (AlarmInfos != null && AlarmInfos.Count > 0)
            {
                foreach (var item in AlarmInfos)
                {
                    if (item != AlarmInfo
                        && (item.EndTime == null || item.EndTime == default(DateTime)))
                    {
                        AlarmProcEvent?.Invoke(item, false);
                    }
                }
            }
        }
        /// <summary>
        /// 用于防止检查IO时同时触发多次命令 
        /// </summary>
        private int lockCommand = 0;

        /// <summary>
        /// 暂停
        /// </summary>
        /// <param name="isAlarm">是否切换到报警停机状态</param>
        /// <param name="action"></param>
        public void Pause(bool isAlarm = true, Action action = null)
        {
            // 暂停设备监控,放到此处是防止安全门
            _deviceEngine.Pause();
            _deviceEngine.OnLog(LogType.Info, $"当前lockcommand值5：{lockCommand}!");
            //if (MachineStatus == EngineStatus.Pause
            //    || MachineStatus == EngineStatus.Ready
            //    || MachineStatus == EngineStatus.Idle
            //    || MachineStatus == EngineStatus.Stop) return;
            pauseReset1?.Wait();
            if ((!isAlarm && MachineStatus == EngineStatus.Pause)
                || MachineStatus == EngineStatus.Ready
                || MachineStatus == EngineStatus.Idle
                || MachineStatus == EngineStatus.Stop) return;

            // 防止连续点击
            if (lockCommand > 0)
                return;
            Interlocked.Increment(ref lockCommand);
            _deviceEngine.OnLog(LogType.Info, $"Pauselockcommand置位：{lockCommand}!");
            // 记录停止时间
            _impactData.UpdatePauseTime();

            //if (isAlarm && (MachineStatus == EngineStatus.Alarm))
            //{
            //    _activePause = false;

            //    // 第二次报警进来的场景，直接返回
            //    if (AlarmInfo != null)
            //    {
            //        Interlocked.Decrement(ref lockCommand);
            //        _deviceEngine.OnLog(LogType.Info, $"Pauselockcommand复位：{lockCommand}!");
            //        return;
            //    }
            //}
            //else
            //{
            //    _activePause = true;
            //}
            _activePause = true;
            // 流程暂停
            MotionEngine.Pause(isAlarm, (isOk, ex) =>
            {
                if (!isOk)
                {
                    MotionEngine.OnAlarm(new AlarmInfo(this, AlarmType.DeviceError, $"暂停失败：{ex?.Message ?? "暂停流程失败，请检查设备！"}", "F01ESOO-01@Emergency stop error"));
                }
                _lightManager.PauseLight(isAlarm);

                action?.Invoke();

                Interlocked.Decrement(ref lockCommand);
                _deviceEngine.OnLog(LogType.Info, $"流程暂停lockcommand复位：{lockCommand}!");
            });

            //机器人所有信号关闭
            SetRobotDO(false);
            //机器人暂停
            ProcessIO(SysConfig.RobotPause, io => io.SetDigital(true));
            ProcessIO(SysConfig.RobotPause2, io => io.SetDigital(true));

            //ProcessIO(SysConfig.ListSysOperateIO, io => io.SetDigital(false));

            // 开门锁
            LockDoor(false);

            //暂停时，关闭数字量IO
            //CloseOperateIO(SystemOperation.Pause);
        }

        /// <summary>
        /// 重试
        /// </summary>
        /// <param name="module"></param>
        public void Retry(IMotionModule module)
        {
            // 1.Plc复位
            SysConfig?.PlcRecovery(_deviceEngine);

            // 2.Plc启动
            SysConfig?.PlcStart(_deviceEngine);

            // 3.重试要清理报警状态
            AlarmInfo = null;

            // 4.重试
            MotionEngine.Reset(AlarmInfo, (isOK) =>
            {
                _lightManager.PauseLight(false);
            });
        }

        /// <summary>
        /// 空跑模式
        /// </summary>
        public void EmptyRun(bool isRun = true)
        {
            //SysConfig?.PlcEmpty(_deviceEngine, isRun);
        }

        /// <summary>
        /// 只要是停止了，就可以回零操作
        /// 设备回零,回零是一个耗时操作，外部UI调用需要用异步调用,防止卡顿
        /// </summary>
        public void Home()
        {
            // 1.流程是否暂停
            if (MachineStatus == EngineStatus.Running)
            {
                throw new FriendlyException("流程正在运行,不能进行回零!");
            }
            if (_deviceEngine.DeviceMode == DeviceMode.Virtual)
            {
                throw new FriendlyException("离线模式无法回零！请切为在线模式");
            }
            //_firstStartFlag = true;
            // 2.回零弹窗提示
            HomeType homeType = HomeType.Cancel;
            if (StartHomeEvent != null)
            {
                homeTime = 0;

                homeType = StartHomeEvent.Invoke();
            }

            // 取消回零
            if (homeType == HomeType.Cancel)
            {
                return;
            }

            _lightManager.SetBuzzer(false);

            IsHoming = true;
            // 3.清理报警
            clearAlarm();

            // 4.在开始监控
            StartButtonMontor();

            // 5.防止PLC
            Thread.Sleep(100);

            // 6.当软件是处于急停状态时，需要确认下急停按钮是否
            if (IsEmg)
            {
                // 急停状态检测
                ProcessIO(SysConfig.EmergencyButton, io =>
                {
                    // 急停常闭
                    if (io.GetDigitalIn() == false)
                    {
                        AlarmProcEvent?.Invoke(AlarmInfo, false);
                        throw new FriendlyException("设备急停中,请先松开急停按钮!");
                    }
                    else
                    {
                        IsEmg = false;
                    }
                });
                // 重新连接板卡
                if (!_deviceEngine.SetEngineMode(_deviceEngine.DeviceMode, out var err))
                {
                    throw new FriendlyException($"软件状态->{_deviceEngine.DeviceMode.GetDescription()} 失败,{err}!");
                }
            }

            if (IsSmokeAlarm)
            {
                ProcessIO(SysConfig.SmokeAlarmDevice, io =>
                {
                    if (io.GetDigitalIn())
                    {
                        AlarmProcEvent?.Invoke(AlarmInfo, false);
                        throw new FriendlyException("设备急停中,请先检查烟雾报警器!");
                    }
                    else
                    {
                        IsSmokeAlarm = false;
                    }
                });
            }

            // 模式切换到当前模式

            // 6.设备回零
            MotionEngine?.Home(() =>
            {
                IsEmg = false;
                IsSmokeAlarm = false;

                // 机器人所有信号关闭
                SetRobotDO(false);

                // 机器人清错
                ProcessIO(SysConfig.RobotClearFault, io => io.SetDigital(true));
                ProcessIO(SysConfig.RobotReset, io => io.SetDigital(true));
                ProcessIO(SysConfig.RobotStart, io => io.SetDigital(true));
                ProcessIO(SysConfig.RobotClearFault2, io => io.SetDigital(true));
                ProcessIO(SysConfig.RobotReset2, io => io.SetDigital(true));
                ProcessIO(SysConfig.RobotStart2, io => io.SetDigital(true));

                _lightManager.ReadyLight();
                var curMode = SystemConsts.ProductMode;
                // 切换机台模式
                SetCurrentMode(curMode);
                HomeEndEvent?.Invoke(curMode);
                IsHoming = false;
            }, homeType);

        }



        /// <summary>
        /// 保存系统配置
        /// </summary>
        /// <returns></returns>
        public XElement SaveSysConfig()
        {
            if (SysConfig == null) return new XElement("SysConfig");

            return SysConfig.ExportXml();
        }

        /// <summary>
        /// 加载系统配置
        /// </summary>
        public void LoadSysConfig(XElement xSys)
        {
            SysConfig.ParserXml(xSys);

            // 设置按钮灯和三色灯
            SysConfig.SetDeviceEngine(_deviceEngine);
        }

        /// <summary>
        /// 按下回零的时间
        /// </summary>
        private int homeTime = 0;

        /// <summary>
        /// 按下忽略时间
        /// </summary>
        private int ignoreTime = 0;

        /// <summary>
        /// IO 通用处理
        /// </summary>
        /// <param name="io"></param>
        private void ProcessIO(VDevice vDevice, Action<VIO> ioAction)
        {
            // 离线仿真，不进行监控IO
            //if (_deviceEngine.DeviceMode == DeviceMode.Virtual)
            //{
            //    return;
            //}

            // 启动按钮
            if (vDevice != null && !vDevice.IsEmpty())
            {
                var io = vDevice.GetVDevice<VIO>(_deviceEngine);
                ioAction(io);
            }
        }

        /// <summary>
        /// 是否处于急停状态
        /// </summary>
        private int EmergencyNum = 0;

        /// <summary>
        /// 机台是否处于急停状态
        /// </summary>
        private bool IsEmg = false;

        /// <summary>
        /// 烟雾报警次数
        /// </summary>
        private int SmokeAlarmNum = 0;

        /// <summary>
        /// 机台是否处于烟雾报警状态
        /// </summary>
        private bool IsSmokeAlarm = false;

        /// <summary>
        /// 定时器，监控IO、按钮、开关门、急停报警等
        /// </summary>
        //private Timer timer = null;
        private System.Timers.Timer timer = null;

        /// <summary>
        /// 启动定时器
        /// </summary>
        private void StartTimer()
        {
            if (timer != null)
            {
                timer.Stop();
            }

            timer = new System.Timers.Timer();
            timer.Interval = 100;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        /// <summary>
        /// 定时刷新函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            timer.Stop();
            try
            {
                // 机台状态监控
                MachineMonitor();

                // Plc 状态监控
                PlcMonitor();
            }
            catch (Exception ex)
            {
                MotionEngine.OnLog(LogType.Error, $"监控出错:{ex.Message},{ex.StackTrace}");
                Pause();
            }
            timer?.Start();//修复切换状态会抛异常的问题，阻碍调试
        }

        /// <summary>
        /// 是否处于手动模式
        /// </summary>
        private bool? isAutoMode = null;

        /// <summary>
        /// 手动/自动切换按钮消抖计数器
        /// </summary>
        private int manualSwitchDebounceCount = 0;

        /// <summary>
        /// 机台状态监控
        /// </summary>
        private void MachineMonitor()
        {
            // 机台处于暂停和准备就绪才能进行按钮操作
            if (MachineStatus == EngineStatus.Ready || MachineStatus == EngineStatus.Pause)
            {
                ProcessIO(SysConfig.StartButton, io =>
                {
                    if (io.GetDigitalIn())
                    {
                        // 安全门检查
                        if (!DoorIsClosed(out var errMsg))
                        {
                            MotionEngine.OnAlarm(new AlarmInfo(this, AlarmType.WarningTip, "安全门被打开", "F02SCOO-01@The security door was opened"));
                            return;
                        }

                        // 自动打开
                        if (!CanAutoRun(out errMsg))
                        {
                            //MotionEngine.OnAlarm(new AlarmInfo(this, AlarmType.InfoTip, "安全门被打开", "N03OOOO-01"));
                            return;
                        }

                        //触发启动事件，用于Hive界面显示
                        MachineStartEvent?.Invoke("Start");
                        //Start();
                        MotionEngine.OnLog(LogType.Info, $"IO信号:{io.Name}触发{(MachineStatus == EngineStatus.Ready ? "启动" : "恢复")}");

                    }
                });
            }

            // 机台处于运行中，才能进行暂停
            if (MachineStatus == EngineStatus.Running || MachineStatus == EngineStatus.Homing)
            {
                //运行中门锁报警
                if (!DoorIsClosed(out var errMsg))
                {
                    MotionEngine.OnAlarm(new AlarmInfo(this, AlarmType.WarningTip, "安全门被打开", "F02SCOO-01@The security door was opened"));
                    return;
                }

                ProcessIO(SysConfig.PauseButton, io =>
                {
                    if (io.GetDigitalIn())
                    {
                        //Pause(false);
                        MachinePauseEvent?.Invoke("Pause");
                        MotionEngine.OnLog(LogType.Warning, $"IO信号:{io.Name} 触发暂停");
                    }
                });

            }

            // 机台监控手自动切换按钮
            // 自动/手动切换
            ProcessIO(SysConfig.ManualSwitchButton, io =>
            {
                var canRun = io.GetDigitalIn();

                // 仅在IO值发生变化时记录，避免每100ms重复打印
                if (isAutoMode != canRun)
                {
                    MotionEngine.OnLog(LogType.Debug, $"ManualSwitch IO变化: {isAutoMode}->{canRun}, 消抖计数: {manualSwitchDebounceCount}");
                }

                // 开机启动时，触发一次
                if (isAutoMode == null)
                {
                    MotionEngine.OnLog(LogType.Info, $"ManualSwitch 首次初始化: IO值={canRun}");
                    CanAutoRunEvent?.Invoke(canRun);
                    isAutoMode = canRun;
                }

                // 触发上升沿或下降延，才进行开门或则锁门动作
                if (isAutoMode != canRun)
                {
                    manualSwitchDebounceCount++;
                    // 连续3次（300ms）读取到相同变化才确认有效，防止IO信号抖动
                    if (manualSwitchDebounceCount < 3)
                    {
                        return;
                    }

                    CanAutoRunEvent?.Invoke(canRun);
                    //自动打到手动后，先把三色灯颜色改变
                    if (MachineStatus == EngineStatus.Running)
                    {
                        _lightManager.PauseLight();
                    }
                    // 如果设备处于报警状态，不让切换为pause
                    if (canRun == false && MachineStatus != EngineStatus.Alarm)
                    {
                        MotionEngine.OnLog(LogType.Info, $"IO信号:{io.Name} 触发暂停");
                        MachineManualEvent?.Invoke("Manual");
                        //Pause(false);
                    }

                    if (SysConfig.IsDoorLock)
                    {
                        LockDoor(!canRun);
                    }
                    else
                    {
                        LockDoor(canRun);
                    }
                    // 进行门锁功能切换

                }
                else
                {
                    manualSwitchDebounceCount = 0;
                }

                isAutoMode = canRun;
            });

            ///烟雾报警
            if (!IsSmokeAlarm)
            {
                //烟雾报警器报警监控
                ProcessIO(SysConfig.SmokeAlarmDevice, io =>
                {
                    if (io.GetDigitalIn())
                    {
                        SmokeAlarmNum++;
                        if (SmokeAlarmNum == 3)
                        {
                            IsSmokeAlarm = true;
                            MotionEngine.OnAlarm(new AlarmInfo(this, AlarmType.DeviceError, "烟雾报警器触发停止&Smoke alarm triggers stop", "N03OOOO-01"));
                        }
                    }
                    else
                    {
                        SmokeAlarmNum = 0;
                    }
                    Thread.Sleep(10);
                });
            }

            // 机台处于运行中、报警，暂停，才能进行停止
            if (MachineStatus == EngineStatus.Running
                || MachineStatus == EngineStatus.Alarm
                || MachineStatus == EngineStatus.Pause
                || MachineStatus == EngineStatus.Homing)
            {
                ProcessIO(SysConfig.StopButton, io =>
                {
                    if (io.GetDigitalIn() == false)
                    {
                        Pause(false);
                        //触发启动事件，用于Hive界面显示
                        //MachineStopEvent?.Invoke("Stop");
                        //Stop();
                        MotionEngine.OnLog(LogType.Warning, $"IO信号:{io.Name}触发停止");
                    }
                });
            }

            // 急停
            if (!IsEmg)
            {
                ProcessIO(SysConfig.EmergencyButton, io =>
                {
                    // 急停常闭
                    if (io.GetDigitalIn() == false)
                    {
                        EmergencyNum++;

                        // 只有三次急停才触发
                        if (EmergencyNum > 3)
                        {
                            IsEmg = true;
                            MotionEngine.OnAlarm(new AlarmInfo(this, AlarmType.DeviceError, "Emergency stop error", "F01ESOO-01@Emergency stop error"));
                        }

                        Thread.Sleep(10);
                    }
                });
            }

            // 回零
            if (MachineStatus == EngineStatus.Idle || MachineStatus == EngineStatus.Stop)
            {
                ProcessIO(SysConfig.RecoveryButton, io =>
                {
                    // 只有初始状态或者停止状态才能够进行回零操作
                    if (io.GetDigitalIn())
                    {
                        homeTime += 100;
                        if (homeTime > 3000)
                        {
                            Home();
                        }

                        homeTime += 200; // 因为蜂鸣器会有耗时操作
                    }
                    else
                    {
                        // 如果松开，CT更新为0ms
                        homeTime = 0;
                    }
                });
            }

            // 通过复位信号，可以清除报警
            ProcessIO(SysConfig.RecoveryButton, io =>
            {
                // 只有初始状态或者停止状态才能够进行回零操作
                if (io.GetDigitalIn())
                {
                    Recovery();
                    MotionEngine.OnLog(LogType.Warning, $"IO信号:{io.Name}触发清除报警");
                }
            });

            if (MachineStatus == EngineStatus.Alarm || MachineStatus == EngineStatus.Idle)
            {
                // 如果流程被暂停了，通过忽略按钮支持继续
                ProcessIO(SysConfig.StartButton, io =>
                {
                    if (io.GetDigitalIn() && AlarmInfo != null &&
                        (AlarmInfo.AlarmType == AlarmType.InfoTip
                        || AlarmInfo.AlarmType == AlarmType.WarningTip
                        || AlarmInfo.AlarmType == AlarmType.PlcAlarm))
                    {
                        // 只有初始状态或者停止状态才能够进行回零操作
                        if (io.GetDigitalIn())
                        {
                            ignoreTime += 100;
                            if (ignoreTime > 3000)
                            {
                                Recovery();
                            }

                            // 恢复会将蜂鸣器关闭
                            _lightManager.SetBuzzer(false);
                            ignoreTime += 200; // 因为蜂鸣器会有耗时操作
                        }
                        else
                        {
                            // 如果松开，CT更新为0ms
                            ignoreTime = 0;
                        }
                    }
                });
            }

            // 光幕检查
            if (!CheckLightCurtain())
            {
                var lightAlarm = new AlarmInfo(this, AlarmType.WarningTip, "光幕被遮挡&The light curtain is blocked", "N03OOOO-01");
                MotionEngine.OnAlarm(lightAlarm);
            }
        }

        /// <summary>
        /// 状态监控
        /// </summary>
        private PlcStatus plcStatus = PlcStatus.None;

        /// <summary>
        /// PLC 的状态监控
        /// </summary>
        private void PlcMonitor()
        {
            SysConfig.PlcStatus(_deviceEngine, (status) =>
            {
                // 触发PLC状态
                PlcStatusEvent?.Invoke(status);

                // 因为和其他报警是并行报警，所以需要判断如果程序停止了，需要持续监控，但不需要在变更PLC状态
                if (MachineStatus == EngineStatus.Stop
                    || MachineStatus == EngineStatus.Alarm
                    || MachineStatus == EngineStatus.Pause
                    || MachineStatus == EngineStatus.Ready
                    || MachineStatus == EngineStatus.Idle
                    || MachineStatus == EngineStatus.Homing
                    )
                {
                    return;
                }
                // 记录PLC状态是否发生变更
                bool isPlcChanged = plcStatus != status;
                plcStatus = status;

                // 如果从普通状态切换到报警状态
                if (status == PlcStatus.Alarm && isPlcChanged)
                {
                    // 先设置报警状态，防止定时器重入导致重复报警
                    //MotionEngine.EngineStatus = EngineStatus.Alarm;

                    //如果当前在报警中，则退出
                    //Thread.Sleep(100);
                    if (vPlc == null)
                    {
                        vPlc = _deviceEngine.GetVDevices<VPlc>().FirstOrDefault();
                    }

                    if (vPlc == null || string.IsNullOrEmpty(SysConfig.TricolorStatusAddr)) return;
                    if (vPlc.ReadNumber(SysConfig.TricolorStatusAddr) == 5) return;

                    SysConfig?.PlcAlarm(_deviceEngine, (alarms, alarmType) =>
                    {
                        string alarmMsg = $"PLC:{string.Join(",", alarms.Select(u => u.Desc))}";

                        var plcAlarm = new AlarmInfo(this, AlarmType.PlcAlarm, "PLC报警&PLC Alarm", "N03PLOO-01@PLC Alarm", "System", "PLC");

                        //WritePlcValueInt(SysConfig.TricolorStatusAddr, 5);
                        MotionEngine.OnAlarm(plcAlarm);
                        MotionEngine.EngineStatus = EngineStatus.Alarm;
                    });
                }
            });
        }

        /// <summary>
        /// 启动Button监听
        /// </summary>
        public void StartButtonMontor()
        {
            // 关闭监控
            StopButtonMontor();

            if (_deviceEngine.DeviceMode == DeviceMode.Virtual) return;

            // 开启线程重新启动服务监控
            Task.Run(() =>
            {
                WebService?.StartService(this);
                _configManager.SetWebConfig(WebService.GetConfig());
            });



            EmergencyNum = 0;
            homeTime = 0;

            // 通过定时器来
            StartTimer();
        }

        /// <summary>
        /// 检查光栅
        /// </summary>
        /// <returns></returns>
        private bool CheckLightCurtain()
        {
            bool isRet = true;
            if (SysConfig.EnableLightCurtain)
            {
                LightCurtainProcess((item) =>
                {
                    // 换成对象
                    if (item.GlobalParameter == null)
                    {
                        var parameters = MotionEngine.Get(GlobalModule.GlobalID).Parameters;
                        if (parameters.ContainsKey(item.GlobalKey))
                        {
                            item.GlobalParameter = parameters[item.GlobalKey];
                        }
                    }

                    // 全部变量如果开启，代表进行光幕检查
                    if (item.GlobalParameter.Type == typeof(bool)
                    && bool.TryParse(item.GlobalParameter.Value?.ToString(), out var isOn) && isOn)
                    {
                        var vIO = item.Device.GetVDevice<VIO>(_deviceEngine);
                        if (!vIO.GetDigitalIn())
                        {
                            isRet = false;
                        }
                    }
                });
            }
            return isRet;
        }

        /// <summary>
        /// 光栅检查
        /// </summary>
        /// <param name="aciton"></param>
        private void LightCurtainProcess(Action<LightCurtainModel> aciton)
        {
            if (SysConfig.LightCurtainList != null && SysConfig.LightCurtainList.Count > 0)
            {
                foreach (var item in SysConfig.LightCurtainList)
                {
                    aciton(item);
                }
            }
        }

        /// <summary>
        /// 是否处于急停状态
        /// </summary>
        /// <returns></returns>
        public bool IsEmergency()
        {
            bool isEme = false;
            ProcessIO(SysConfig.EmergencyButton, io =>
            {
                isEme = (io.GetDigitalIn() == false);
            });

            return isEme;
        }

        /// <summary>
        /// 引导式
        /// </summary>
        public void OnNavigation()
        {
            // 如果机台空闲中，才进行灯切换
            if (MachineStatus == EngineStatus.Idle)
            {
                // 1.软件开启亮黄灯
                _lightManager.StartLight();
            }
        }

        /// <summary>
        /// 设置机器人输出
        /// </summary>
        /// <param name="isOn"></param>
        private void SetRobotDO(bool isOn)
        {
            ProcessIO(SysConfig.RobotStart, io => io.SetDigital(isOn));
            ProcessIO(SysConfig.RobotPause, io => io.SetDigital(isOn));
            ProcessIO(SysConfig.RobotStop, io => io.SetDigital(isOn));
            ProcessIO(SysConfig.RobotReset, io => io.SetDigital(isOn));
            ProcessIO(SysConfig.RobotClearFault, io => io.SetDigital(isOn));

            ProcessIO(SysConfig.RobotStart2, io => io.SetDigital(isOn));
            ProcessIO(SysConfig.RobotPause2, io => io.SetDigital(isOn));
            ProcessIO(SysConfig.RobotStop2, io => io.SetDigital(isOn));
            ProcessIO(SysConfig.RobotReset2, io => io.SetDigital(isOn));
            ProcessIO(SysConfig.RobotClearFault2, io => io.SetDigital(isOn));
        }
        /// <summary>
        /// 关闭稼动IO
        /// </summary>
        public void CloseOperateIO(SystemOperation sysOperateIOModel)
        {
            if (sysOperateIOModel == SystemOperation.Alarm)
            {
                _AlemIOSet = true;
            }

            if (SysConfig.ListSysOperateIO != null)
            {
                foreach (var item in SysConfig.ListSysOperateIO)
                {
                    if (item != null && item.SysOperateType == sysOperateIOModel)
                    {
                        MotionEngine.OnLog(LogType.Info, $"类型：{sysOperateIOModel.ToString()}  记住当前稼动IO，并把已设置变量置为True");
                        foreach (var io in item.ListIO)
                        {
                            if (!io.IsSet)//防止重复设置
                            {
                                io.Status = io.SetIO.GetVDevice<VIO>(_deviceEngine).GetDigitalOut();
                                io.IsSet = true;
                                ProcessIO(io.SetIO, io => io.SetDigital(false));
                            }
                        }
                    }
                }

                systemOperation = sysOperateIOModel;
            }
        }

        /// <summary>
        /// 启动稼动IO
        /// </summary>
        private void RecoveryOperateIO()
        {
            if (systemOperation == SystemOperation.Pause || systemOperation == SystemOperation.Stop || systemOperation == SystemOperation.Alarm)
            {
                if (SysConfig.ListSysOperateIO != null)
                {
                    var item = SysConfig.ListSysOperateIO.FirstOrDefault(i => i.SysOperateType == systemOperation);
                    if (item != null)
                    {
                        MotionEngine.OnLog(LogType.Info, $"恢复稼动IO");
                        var IOList = GetIOList();
                        foreach (var dio in item.ListIO)
                        {
                            var selectIO = IOList.FirstOrDefault(m => m.Name == dio.SetIO.Name);
                            if (selectIO == null) return;
                            selectIO.SetDigital(dio.Status);
                            //ProcessIO(dio.SetIO, io => io.SetDigital((dio.Status)));
                        }
                    }
                }
            }
        }


        private void RecoveryIOSetInit()
        {
            //所有状态全部设置为初始状态
            _AlemIOSet = false;
            if (SysConfig.ListSysOperateIO != null)
            {
                MotionEngine.OnLog(LogType.Info, $"恢复稼动IO为未设置状态");
                foreach (var select in SysConfig.ListSysOperateIO)
                {
                    if (select != null)
                    {
                        foreach (var io in select.ListIO)
                        {
                            io.IsSet = false;
                        }
                    }
                }
            }
        }


        private List<VIO> GetIOList()
        {
            var list = _deviceEngine.GetDevices(typeof(VIO))
                         .Select(u => u as VIO)
                         .Where(u => u.IOType == IOType.Digital)
                         .Where(u => u.Behavior == IOBehavior.Output);
            var devices = _deviceEngine.GetDevices(typeof(VIO)).Select(u => u as VIO).Where(u => u.Behavior == IOBehavior.Output);
            var IOlist = new List<VIO>();
            foreach (var item in devices)
            {
                IOlist.Add(item);
                bool status = item.GetDigitalOut();
            }
            return IOlist;
        }

        /// <summary>
        /// 终止监听
        /// </summary>
        public void StopButtonMontor()
        {
            if (timer != null)
            {
                timer.Stop();
                timer = null;
            }

            //_lightManager.SetBuzzer(false);
        }

        /// <summary>
        /// 启用蜂鸣器
        /// </summary>
        public void SetBuzzer(bool isOn, int OnTime = 200, int OffTime = 200)
        {
            _lightManager.SetBuzzer(isOn, OnTime, OffTime);
        }

        #region 属性变更事件
        /// <summary>
        /// 属性发现变更
        /// </summary>
        /// <param name="propertyName">属性名称，可能也包含模块名称</param>
        /// <param name="src">原始值</param>
        /// <param name="val">当前值</param>
        public void OnPropertyChanged(string propertyName, object src, object val)
        {
            PropertyChanged?.Invoke(propertyName, src, val);
        }

        /// <summary>
        /// 属性发生变更
        /// </summary>
        public event Action<string, object, object> PropertyChanged;
        #endregion

        #region 主动停机
        /// <summary>
        /// 主动停机事件
        /// <para>MotionController</para>
        /// <para>SystemOperation</para>
        /// <para>停机原因</para>
        /// </summary>
        public event Action<object, SystemOperation, string> ManualDownEvent;

        /// <summary>
        /// 主动停机事件
        /// </summary>
        /// <param name="msg"></param>
        public void OnManualStop(SystemOperation op, string msg)
        {
            ManualDownEvent?.Invoke(this, op, msg);
        }
        #endregion

        #region 模式切换
        /// <summary>
        /// 获取模式
        /// </summary>
        /// <returns></returns>
        public List<RunModeModel> GetModeList()
        {
            return SysConfig.ListRunMode;
        }

        public string GetCurrentMode()
        {
            var data = SysConfig.ListRunMode.FirstOrDefault(u => u.IsRunMode);
            if (data != null)
            {
                return data.Mode;
            }
            else
            {
                return SystemConsts.ProductMode;
            }
        }

        /// <summary>
        /// 切换模式
        /// </summary>
        /// <param name="mode"></param>
        public bool SetCurrentMode(string mode)
        {
            bool isChanged = false;
            var gModule = MotionEngine.Get(GlobalModule.GlobalID);
            StringBuilder sb = new StringBuilder();
            sb.Append($"模式切换 ( ");
            foreach (var item in SysConfig.ListRunMode)
            {
                sb.Append($"{item.Mode}={item.IsRunMode} ");
            }

            sb.Append(") ");

            // 模式切换
            string currentMode = GetCurrentMode();
            sb.Append($"{currentMode}->{mode} ");

            // 2.更新激活模块
            List<GlobalModel> gModels = new List<GlobalModel>();
            foreach (var item in SysConfig.ListRunMode)
            {
                item.IsRunMode = false;
                var isSelect = item.Mode == mode;
                if (isSelect)
                {
                    gModels = item.SelectGlobalVar;
                    item.IsRunMode = true;
                }
            }

            // 3.更新全局变量值
            foreach (var item in gModels)
            {
                if (gModule.Parameters.ContainsKey(item.GlobalKey))
                {
                    var gItem = gModule.Parameters[item.GlobalKey];
                    if (gItem.Type == typeof(bool))
                    {
                        gItem.Value = item.IsSelected;
                        sb.Append($"变量:{gItem.Alias} = {item.IsSelected} ");
                    }
                }
            }

            // 4.判断当前设备模式是否是离线
            bool isVirtual = _deviceEngine.DeviceMode == DeviceMode.Virtual;

            if (mode.IndexOf(SystemConsts.EmptyMode) == 0)
            {
                // 如果存在空跑模式
                EmptyRun(true);
                if (!isVirtual && !_deviceEngine.SetEngineMode(DeviceMode.Empty, out var errMsg))
                {
                    throw new FriendlyException(errMsg);
                }
            }
            else
            {
                // 如果存在空跑模式
                EmptyRun(false);
                if (!isVirtual && !_deviceEngine.SetEngineMode(DeviceMode.Real, out var errMsg))
                {
                    throw new FriendlyException(errMsg);
                }
            }

            MotionEngine.OnLog(LogType.Info, sb.ToString());

            // 模式切换
            if (currentMode != mode)
            {

                ModeChangedEvent?.Invoke(currentMode, mode);
                if (mode != "生产模式")
                {
                    var modeAlarm = new AlarmInfo(this, AlarmType.InfoTip, "模式被切换", $"模式由:{currentMode}切换为模式:{mode}");

                    MotionEngine.OnAlarm(modeAlarm);
                }

            }

            // 非离线模式切换成功
            if (!isVirtual)
            {
                isChanged = true;
            }

            return isChanged;
        }
        #endregion

        #region 切换配方

        /// <summary>
        /// 切换配方
        /// </summary>
        public event Action<string> ChangeRecipeEvent;

        public void ChangeRecipe(string Recipe)
        {
            ChangeRecipeEvent?.Invoke(Recipe);
        }
        #endregion

        #region 换料事件
        /// <summary>
        /// 切换配方
        /// </summary>
        public event Action<string> MaterialChangeEvent;

        public void OnMaterialChange(string reason)
        {
            MaterialChangeEvent?.Invoke(reason);
        }
        #endregion

        #region 工程备份
        /// <summary>
        /// 切换配方
        /// </summary>
        public event Action<string> ProjBackUpEvent;

        public void ProjBackUp(string projPath)
        {
            ProjBackUpEvent?.Invoke(projPath);
        }
        #endregion

        #region 角色变更
        /// <summary>
        /// 系统角色
        /// </summary>
        public SystemRole SysRole { get; private set; } = SystemRole.Operator;
        public bool IsHoming { get; set; } = false;


        /// <summary>
        /// 切换角色
        /// </summary>
        /// <param name="role"></param>
        public void OnUserLogin(SystemRole role)
        {
            // 用户登录
            RoleChangedEvent(SysRole, role);
            MotionEngine.DeviceEngine.OnUserLogin(role);
            SysRole = role;
        }

        /// <summary>
        /// 角色变更事件
        /// </summary>
        public event Action<SystemRole, SystemRole> RoleChangedEvent;

        /// <summary>
        /// 工站结束事件
        /// </summary>
        public event Action<string, string, string, bool> StationTimeEvent;
        #endregion

        /// <summary>
        /// 当前班别
        /// </summary>
        private ClassInfo currentClass = null;

        /// <summary>
        /// 自动换班
        /// </summary>
        public void ChangeClass(DateTime dateTime)
        {
            if (currentClass == null)
            {
                currentClass = SysConfig.GetCurrentClass(dateTime);
            }
            else
            {
                var temp = SysConfig.GetCurrentClass(dateTime);

                var dayOff = (temp.StartTime - currentClass.StartTime).TotalDays;
                // 和当前班别进行比较，如果不匹配就需要进行换班处理
                if (temp.ClassName != currentClass.ClassName || dayOff > 0)
                {
                    currentClass = temp;

                    // 即使换班也不清除数据
                    // ClearProductInfo();
                }
            }
        }





        bool sfcConnectStatus = false;
        public async Task<bool> Update_SFC_ConnectStatus()
        {
            string sfcurl = _configManager.GetWebConfig("SFCUrl");
            string ip;
            if (!string.IsNullOrEmpty(sfcurl))
            {

                Uri uri;
                Uri.TryCreate(sfcurl, UriKind.Absolute, out uri);
                if (uri != null)
                {
                    if (uri.Host != null)
                    {
                        ip = uri.Host;

                        sfcConnectStatus = await CheckComm(ip);
                        NetStatusChangedEvent?.Invoke("SFC", sfcConnectStatus, "");
                        sfcConnectStatus = !sfcConnectStatus;
                    }
                }
            }
            return sfcConnectStatus;
        }

        bool pdcaConnectStatus = false;

        public async Task<bool> Update_PDCA_ConnectStatus()
        {
            var pdca = _deviceEngine.GetVDevices<VCommuncation>().FirstOrDefault(u => u.Name.ToUpper().Contains("PDCA"));

            if (pdca != null)
            {
                string ip = ((CommTCP)(pdca.Communication)).Network.Ip;

                pdcaConnectStatus = await CheckComm(ip);
                NetStatusChangedEvent?.Invoke("PDCA", pdcaConnectStatus, "");
            }

            return pdcaConnectStatus;
        }
        private bool CheckSerialPortIsExist(string cmd)
        {
            return SerialPort.GetPortNames().Any(p =>
            {
                int idx = cmd.IndexOf(p, StringComparison.OrdinalIgnoreCase);
                if (idx < 0) return false;

                int end = idx + p.Length;
                return end < cmd.Length && cmd[end] == '/';
            });
        }

        bool ffuConnectStatus = false;
        string ffuState = "FFU";

        public async Task<bool> Update_FFU_ConnectStatus()
        {
            if (_deviceEngine.DeviceMode == DeviceMode.Virtual)
                return false;

            // FFUCOM存在时直接返回当前状态
            if (_deviceEngine.GetVDevices<VCommuncation>().FirstOrDefault(u => u.Name.ToUpper() == "FFUCOM") != null)
            {
                return ffuConnectStatus;
            }

            var ffu = _deviceEngine.GetVDevices<VCommuncation>().FirstOrDefault(u => u.Name.ToUpper() == "FFU");
            var ffu2 = _deviceEngine.GetVDevices<VCommuncation>().FirstOrDefault(u => u.Name.ToUpper() == "FFU2");

            async Task CheckFFUAsync(VCommuncation ffuDevice)
            {
                try
                {
                    await Task.Run(() => ffuDevice.Open());
                    var coilVs = await Task.Run(() => ffuDevice.Read<short>("211 03 256 9"));
                    await Task.Delay(10);
                    if (coilVs == null || coilVs.Count == 0)
                    {
                        ffuState = "FFU连接异常";
                        ffuConnectStatus = false;
                    }
                    else
                    {
                        if (coilVs.Count > 6 && int.Parse(coilVs[4].ToString()) == 0)
                        {
                            ffuState = "FFU关机";
                            ffuConnectStatus = false;
                        }
                        else
                        {
                            var level = coilVs[5];
                            ffuState = $"FFU{level}档";
                            ffuConnectStatus = true;
                        }
                    }
                }
                catch
                {
                    ffuState = "FFU连接异常";
                    ffuConnectStatus = false;
                }
            }

            if (ffu != null && CheckSerialPortIsExist(ffu.ConnectStr))
            {
                await CheckFFUAsync(ffu);
            }

            if (ffu2 != null && CheckSerialPortIsExist(ffu2.ConnectStr))
            {
                await CheckFFUAsync(ffu2);
            }

            NetStatusChangedEvent?.Invoke("FFU", ffuConnectStatus, ffuState);
            return ffuConnectStatus;
        }


        private System.Net.NetworkInformation.Ping ping = new Ping();


        /// <summary>
        /// 通信连接
        /// </summary>
        /// <returns></returns>
        public async Task<bool> CheckComm(string serverIp)
        {
#if !DEBUG
            try
            {
                // 2s ping
                var pingResult = await Task.Run(() => ping.Send(serverIp, 2000));
                return pingResult.Status == IPStatus.Success;
            }
            catch
            {
                return false;
            }
#else
            {
                Thread.Sleep(2000);
                return true;

            }
#endif

        }

        public void LightInit()
        {
            _lightManager.SetDeviceEngine(_deviceEngine);
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

        public void Update_FFUCOM_ConnectStatus(string netName, bool isOpen, string state)
        {
            NetStatusChangedEvent?.Invoke(netName, isOpen, state);
        }


        /// <summary>
        /// 返回自由工站或自由工站中的点位运动和IO信息
        /// </summary>
        private List<object> DeviceEngine_GetModuleListEvent(string guid, string type)
        {
            var stations = new List<object>();

            if (guid is null)
            {
                stations.AddRange(MotionEngine.Where(s => s.TaskFunction is IStation && s.Status != RunStatus.Skip)
                                              .Select(s => new { ID = s.ID, Alias = s.Alias })
                                              .ToList());
            }
            else
            {
                string key = string.Empty;
                switch (type)
                {
                    case "AxisPosMove":
                        key = "AxisPos";
                        break;
                    case "GetIO":
                        key = "Device";
                        break;
                    case "SetIO":
                        key = "IO";
                        break;
                }

                if (!string.IsNullOrEmpty(key))
                {
                    var sTypes = MotionEngine.Where(s => s.TaskFunction.ToString().Contains(type) && s.Status != RunStatus.Skip).ToList();

                    if (sTypes.Count() > 0)
                    {
                        var sModules = new List<IMotionModule>();
                        foreach (IMotionModule item in sTypes)
                        {
                            try
                            {
                                if (item.Station == null)
                                {
                                    item.UpdateStation();
                                }

                                var station = item.Station as IMotionModule;
                                if (station != null && station.ID.ToString() == guid)
                                {
                                    var directOuterModule = FindDirectOuterModule(item, station);
                                    if (directOuterModule != null && directOuterModule.Status == RunStatus.Skip) continue;
                                    sModules.Add(item);
                                }
                            }
                            catch (Exception ex)
                            {

                            }
                        }

                        if (sModules.Count() > 0)
                        {
                            var lists = sModules.Select(s => new { ID = s.ID.ToString(), Name = s.Alias, Values = s.Parameters[key].Value }).ToList();

                            stations.AddRange(lists);
                        }
                    }
                }
            }

            return stations;
        }

        /// <summary>
        /// 更新运行时报警模块参数：通过 MotionEngine.Get(id) 直接定位模块并更新 AlarmCode/Message/Detail
        /// </summary>
        private bool DeviceEngine_UpdateAlarmModuleParamsEvent(string moduleId, string code, string message, string detail)
        {
            try
            {
                if (!Guid.TryParse(moduleId, out var guid) || guid == Guid.Empty)
                    return false;

                var module = MotionEngine.Get(guid);
                if (module == null) return false;

                // 更新 Parameters 字典
                if (module.Parameters.ContainsKey("AlarmCode"))
                {
                    var p = module.Parameters["AlarmCode"];
                    if (p.Value?.ToString() != code) p.Value = code;
                }
                if (module.Parameters.ContainsKey("Message"))
                {
                    var p = module.Parameters["Message"];
                    if (p.Value?.ToString() != message) p.Value = message;
                }
                if (module.Parameters.ContainsKey("Detail"))
                {
                    var p = module.Parameters["Detail"];
                    if (p.Value?.ToString() != detail) p.Value = detail;
                }

                // 更新 TaskFunction 属性（通过反射，避免直接引用 Luster.Module.Motion.Logic）
                var func = module.TaskFunction;
                if (func != null)
                {
                    var funcType = func.GetType();
                    if (funcType.Name == "Alarm")
                    {
                        var pAlarmCode = funcType.GetProperty("AlarmCode");
                        var pMessage = funcType.GetProperty("Message");
                        var pDetail = funcType.GetProperty("Detail");
                        if (pAlarmCode != null && pAlarmCode.GetValue(func)?.ToString() != code)
                            pAlarmCode.SetValue(func, code);
                        if (pMessage != null && pMessage.GetValue(func)?.ToString() != message)
                            pMessage.SetValue(func, message);
                        if (pDetail != null && pDetail.GetValue(func)?.ToString() != detail)
                            pDetail.SetValue(func, detail);
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 查找点位模块的外层模块
        /// </summary>
        private IMotionModule FindDirectOuterModule(IMotionModule module, IMotionModule station)
        {
            if (module == null || station == null)
                return null;

            //从模块开始向上查找，直到找到工站
            var current = module;

            while (current != null)
            {
                if (current.Parent == station)
                {
                    return current;
                }
                if (current.Parent != null && current.Parent != station)
                {
                    if (current.Parent.Parent == station)
                    {
                        return current.Parent;
                    }
                }
                current = current.Parent as IMotionModule;
            }

            //如果模块的父级不是工站，返回父级
            if (module.Parent != null && module.Parent != station)
            {
                return module.Parent as IMotionModule;
            }
            return null;
        }

        /// <summary>
        /// 获取PDCA模块信息
        /// </summary>
        private IEnumerable<object> DeviceEngine_GetPDCAModulesEvent()
        {

            // 获取所有PDCA模块
            var allPDCAModules = MotionEngine
                .Where(m => m.Status != RunStatus.Skip && IsPDCAModule(m))
                .Select(x => x as object);

            return allPDCAModules;


            /// <summary>
            /// 判断是否为PDCA模块
            /// </summary>
            bool IsPDCAModule(IMotionModule module)
            {
                if (module?.TaskFunction == null) return false;

                var functionName = module.TaskFunction.GetType().Name;
                var alias = module.Alias ?? "";

                return functionName.Contains("PDCA") ||
                       alias.Contains("PDCA") ||
                       functionName.Contains("PDCAELimit");
            }
        }
        /// <summary>
        /// 获取SFC模块信息
        /// </summary>
        private IEnumerable<object> DeviceEngine_GetSFCModulesEvent()
        {

            // 获取所有SFC模块
            var allSFCModules = MotionEngine
                .Where(m => m.Status != RunStatus.Skip && IsSFCModule(m))
                .Select(x => x as object);

            return allSFCModules;


            /// <summary>
            /// 判断是否为SFC模块
            /// </summary>
            bool IsSFCModule(IMotionModule module)
            {
                if (module?.TaskFunction == null) return false;

                var functionName = module.TaskFunction.GetType().Name;
                var alias = module.Alias ?? "";

                return functionName.Contains("SFC") ||
                       alias.Contains("SFC");
            }
        }
    }
}