#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       MotionEngineControl
* 机器名称:       L05123-02
* 命名空间:       Luster.Motion.TaskFlow.Engine.Engine
* 文 件 名:       MotionEngineControl.cs
* 创建时间:       2022/12/20 8:58:47
* 作    者:       刘克志
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      30c73e56-256a-4b08-9320-b0474df5cd4c
* 登录用户:       刘克志
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/12/20 8:58:47
* 修 改 人:		  刘克志
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.TaskFlow.Engine.Models;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Motion.interfaces;
using Luster.TaskFlow.Motion.Interfaces;
using Luster.TaskFlow.Motion.Logic;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Luster.TaskFlow.Common.Module;
using Luster.TaskFlow.Common;
using Luster.TaskFlow.Common.Interfaces;
using System.Collections.Concurrent;
using Luster.Motion.DataStruct;
using Luster.Common.DataStruct.Extensions;
using static System.Collections.Specialized.BitVector32;
using Luster.TaskFlow.Motion.Functions;
using Luster.Common.DataAccess.Tables;
using Luster.TaskFlow.Motion.Enums;
using System.IO;
using System.Diagnostics;
using Prism.Regions;

namespace Luster.Motion.TaskFlow.Engine
{
    public partial class MotionEngine
    {

        /// <summary>
        /// 工作流
        /// </summary>
        public WorkFlow WorkFlow { get; set; }

        /// <summary>
        /// 运行中的模块
        /// </summary>
        private ConcurrentDictionary<Guid, IPauseFunction> PausedModules { get; set; }

        /// <summary>
        /// 设备引擎
        /// </summary>
        public IDeviceEngine DeviceEngine { get; set; }

        /// <summary>
        /// NG 工站
        /// </summary>
        private IMotionModule ngModule = null;

        #region 事件
        /// <summary>
        /// 流程暂停
        /// <para>参数是 模块名称</para>
        /// </summary>
        public event Action<IMotionModule, StationResult> ProLoadedEvent;

        /// <summary>
        /// 产品出料事件
        /// </summary>
        public event Action<IMotionModule, StationResult, Dictionary<string, object>, Action<IMotionModule, StationResult, Dictionary<string, object>, double>> ProUnloadedEvent;

        /// <summary>
        /// 产品出料事件DB
        /// </summary>
        public event Action<IMotionModule, StationResult, Dictionary<string, object>, double> ProUnloadedDBEvent;

        /// <summary>
        /// 流程暂停
        /// <para>参数是 模块名称</para>
        /// <para>参数是 物料名称</para>
        /// </summary>
        public event Action<StationResult, string, string> ProThrowEvent;

        /// <summary>
        /// 回零失败
        /// </summary>
        public event Action<string> HomeFailEvent;

        /// <summary>
        /// 运行加载
        /// </summary>
        public event Action<int, int, string> LoadingEvent;

        /// <summary>
        /// 时间统计事件
        /// </summary>
        public event Action<List<StationTime>, bool> TimeStatisEvent;

        public event Action<List<StationTime>, bool> VisionEvent;

        /// <summary>
        /// 引擎状态变化 用于定制首页显示运行状态
        /// 引擎状态变化 用于定制首页显示运行状态
        /// </summary>
        public event Func<EngineStatus, EngineStatus, bool> EngineStatusEvent;


        //如果复位失败,需要整机停机
        public event Action ResetFailEvent;

        #endregion

        #region 控制
        /// <summary>
        /// 原始状态
        /// </summary>
        private EngineStatus _eStatus = EngineStatus.Idle;

        /// <summary>
        /// 引擎状态信息
        /// </summary>
        public EngineStatus EngineStatus
        {
            get => _eStatus;
            set
            {
                var status = _eStatus;
                OnLog(LogType.Info, $"设备当前状态{value}");
                if (status != value)
                {
                    bool? canChange = EngineStatusEvent?.Invoke(status, value);
                    if (canChange.Value)
                    {
                        _eStatus = value;
                    }

                    if (DeviceEngine != null)
                    {
                        DeviceEngine.OnStatusChanged(status, value);
                    }

                    OnLog(LogType.Info, $"引擎:{ExcutorNo} {value.GetDescription()},状态:{status}->{value}");

                    // 软件状态变更,添加处理
                    RunNGStation();
                    _prvStatus = _eStatus;

                }
            }
        }

        /// <summary>
        /// 线程中断
        /// </summary>
        private static CancellationTokenSource tokenSource;

        /// <summary>
        /// 线程工厂
        /// </summary>
        private TaskFactory taskFactory;

        /// <summary>
        /// 只要是停止了，就可以回零操作
        /// 设备回零,回零是一个耗时操作，外部UI调用需要用异步调用,防止卡顿
        /// </summary>
        public void Home(Action successFunc = null, HomeType homeType = HomeType.Home)
        {
            // 终止所有线程
            tokenSource?.Cancel();

            // 清除所有缓存对象
            CacheManager.RemoveAll();

            // 状态更新
            EngineStatus = EngineStatus.Homing;

            // 清理待复位的模块，防止停止后，没有复位的模块继续复位
            PausedModules?.Clear();

            Task.Run(() =>
            {
                // 1.流程终止,中断循环,清理缓存数据
                this.ForEach(u =>
                {
                    u.DataID = string.Empty;
                    u.IsBreak = true;
                    u.TaskFunction.Status.SetDefault();
                    u.RunNum = 0;

                    if (u.TaskFunction is IStation s)
                    {
                        s.Datas.Clear();
                        s.ClearDatas();
                    }

                    // GoTo对象需要初始化状态
                    if (u.TaskFunction is IGoTo goTo)
                    {
                        goTo.InitStatus();
                    }

                    if (u.TaskFunction is INGStation ngStation)
                    {
                        ngModule = u;
                    }

                });

                // 2.执行回零任务
                if (!HomeTask(homeType, out var errMsg))
                {
                    // 状态还原
                    OnLog(LogType.Error, $"回零失败:{errMsg}");
                    HomeFailEvent?.Invoke(errMsg);
                    return;
                }

                // 3.实现IHomeFunction模块进行回零
                ModuleHome();

                // 4.终止模块所有流程
                SetDefaultStatus(RunStatus.Default);

                // 5.复位全局变量
                InitGlobals();

                // 6.如果此时还在回零过程中，那么就将将状态改变为机台就绪状态
                if (EngineStatus == EngineStatus.Homing)
                {
                    EngineStatus = EngineStatus.Ready;
                    OnLog(LogType.Info, "引擎回零成功");

                    // 6.工艺流也回零
                    WorkFlow.Home();

                    // 7.变为无信号状态
                    pauseResetEvent.Set();

                    // 8.防止内部循环还没有结束中断条件又恢复了
                    Thread.Sleep(100);
                    this.ForEach(u => u.IsBreak = false);

                    // 9.成功回调
                    successFunc?.Invoke();
                }
            });
        }


        /// <summary>
        /// 复位，不对轴回零，只是单纯的将设备调整的安全位置
        /// </summary>
        public void InitGlobals()
        {
            // 6.回零是对全局变量值进行还原
            var gModule = Get(GlobalModule.GlobalID);
            foreach (var item in gModule.Parameters)
            {
                //var p = item.Value;
                //10-17 回零回复默认值
                if (item.Value != null && item.Value.IsHomeDefault)
                {
                    gModule.Parameters[item.Key].Value = item.Value.DefaultV;
                }
            }
        }

        /// <summary>
        /// 进度条更新
        /// </summary>
        /// <param name="total"></param>
        /// <param name="curVal"></param>
        /// <param name="message"></param>
        private void Progress(int total, ref int curVal, string message)
        {
            curVal++;
            LoadingEvent?.Invoke(total, curVal, message);
        }

        private int _homeIndex = 0;
        /// <summary>
        /// 复位
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        private bool HomeTask(HomeType homeType, out string errMsg)
        {
            // 提示信息
            string tip = homeType.GetDescription();
            errMsg = string.Empty;
            bool isHomeSuccess = false;

            // 1.获取回零站
            var homeStation = GetStations().FirstOrDefault(u =>
                            u.Status != RunStatus.Skip
                            && typeof(IHomeStation).IsAssignableFrom(u.TaskFunction.GetType())
                            && (u.TaskFunction as IHomeStation).HomeType == homeType);

            _homeIndex = 0;
            int total = 1;
            if (homeStation != null)
            {
                total = homeStation.Children.Count() + 2;
            }

            Progress(total, ref _homeIndex, $"设备开始{tip}..");

            // 2.设备回零,仅仅是对设备进行初始化（比如Jog要调用停止、通信要进行重新连接）
            isHomeSuccess = DeviceEngine.Home(out errMsg);
            if (!isHomeSuccess)
            {
                // 修复回零失败后
                this.ForEach(u => u.IsBreak = false);
                LoadingEvent?.Invoke(total, total, $"{tip}失败");
                return false;
            }

            // 3.回零工站开始回零
            Progress(total, ref _homeIndex, $"{tip}工站开始进行..");

            // 4.回零工站执行
            if (homeStation != null)
            {
                SetModuleBreak(homeStation, false);

                // 回零事件注册
                if (homeStation.TaskFunction is IHomeStation hStation)
                {
                    hStation.HomeProgressEvent -= HomeStation_HomeProgressEvent;
                    hStation.HomeProgressEvent += HomeStation_HomeProgressEvent;
                }

                isHomeSuccess = homeStation.DoFunction();

                if (!isHomeSuccess)
                {
                    errMsg = homeStation.StatusMsg;
                }
                else if (homeStation.AlarmInfo != null)
                {
                    // 回零模块报警
                    errMsg = homeStation.AlarmInfo.Message;
                    isHomeSuccess = false;
                }

                // 如果被终止代表回零失败
                if (homeStation.IsBreak)
                {
                    isHomeSuccess = false;
                }

                // 状态变为true，默认执行完成
                SetModuleBreak(homeStation, true);
            }

            // 5.回零成功
            if (isHomeSuccess)
            {
                this.ForEach(u => u.IsBreak = false);
            }

            LoadingEvent?.Invoke(total, total, isHomeSuccess ? $"{tip}成功" : $"{tip}失败:{errMsg}");

            //回零时，ct log缓存清零
            cacheTimes.Clear();
            return isHomeSuccess;
        }

        /// <summary>
        /// 回零进度条
        /// </summary>
        /// <param name="total">总结</param>
        /// <param name="curVal">当前值</param>
        /// <param name="message">消息</param>
        private void HomeStation_HomeProgressEvent(int total, ref int refV, string message)
        {
            LoadingEvent?.Invoke(total + 2, _homeIndex + refV, message);
        }

        /// <summary>
        /// 对模块进行回零
        /// </summary>
        private void ModuleHome()
        {
            // 将所有模块状态初始化
            foreach (var item in this)
            {
                if (item.Status == RunStatus.Skip) continue;

                if (item.TaskFunction is IHomeFunction homeFunc)
                {
                    homeFunc.Home();
                }
            }
        }

        public void RunStartStation()
        {

            // 1.获取开始工站
            var startStation = GetStations().FirstOrDefault(u =>
                            u.Status != RunStatus.Skip
                            && typeof(IStartStation).IsAssignableFrom(u.TaskFunction.GetType()));
            if (startStation != null)
            {
                motionRunEngine.SetSkipWait(true);
                startStation.DoFunction();
            }
        }


        public void RunBackGroundStation()
        {
            // 1.获取后台工站
            var backgroundStation = GetStations().FirstOrDefault(u =>
                            u.Status != RunStatus.Skip
                            && typeof(IBackGroundStation).IsAssignableFrom(u.TaskFunction.GetType()));
            if (backgroundStation != null)
            {
                motionRunEngine.SetSkipWait(true);
                backgroundStation.DoFunction();
                RunBackGroundStation();
            }
        }



        /// <summary>
        /// 运行全部工站，循环运行
        /// </summary>
        public void RunStations()
        {
            if (EngineStatus == EngineStatus.Running) return;

            // 1.首先要判断设备是否都有回零 
            if (EngineStatus != EngineStatus.Ready)
            {
                throw new FriendlyException("设备状态未回零成功!");
            }

            // 2.状态更新
            EngineStatus = EngineStatus.Running;

            // 3.启动线程
            tokenSource = new CancellationTokenSource();
            taskFactory = new TaskFactory();

            var stations = GetStations();

            // 4.每个工站开启一个独立的线程
            foreach (var item in stations)
            {
                // 回零站忽略不参与运行
                if (item.TaskFunction is IHomeStation) continue;

                // 测试站忽略不参与运行
                if (item.TaskFunction is ITestStation) continue;

                // 忽略的工站不运行
                if (item.Status == RunStatus.Skip) continue;

                // NG处理工站忽略掉
                if (item.TaskFunction is INGStation) continue;

                // 开始工站忽略掉

                if (item.TaskFunction is IStartStation) continue;

                // 空白工站不运行
                if (item.Children == null || item.Children.Count == 0) continue;

                taskFactory.StartNew((obj) =>
                {
                    var stationModule = obj as IMotionModule;
                    var station = stationModule.TaskFunction as IFreeStation;

                    while (true)
                    {
                        // 中断线程
                        if (tokenSource.IsCancellationRequested)
                        {
                            break;
                        }

                        // 暂停
                        if (pauseResetEvent != null && !pauseResetEvent.IsSet)
                        {
                            pauseResetEvent.Wait();
                        }

                        // 判断模块是否启用
                        if (!station.GetIsEnabled())
                        {
                            // 支持启用按钮
                            Thread.Sleep(200);
                            continue;
                        }

                        // 更新模块状态
                        SetModuleStatus(stationModule, RunStatus.Default);

                        // 程序运行
                        bool isSuccess = stationModule.DoFunction();
                        if (!isSuccess)
                        {
                            OnLog(LogType.Error, $"{stationModule.Alias}工站失败->停止:{stationModule.StatusMsg}");
                            OnLog(LogType.Error, stationModule.AlarmInfo?.ToString() ?? "无报警信息");
                            Stop();
                            //此时非正常停止，需要报警红灯蜂鸣
                            LightManager.StopLight(true);
                        }

                        // 休眠1s
                        Thread.Sleep(5);
                    }

                }, item, tokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }
        }




        /// <summary>
        /// 暂停
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Pause(bool isAlarm = false, Action<bool, Exception> action = null)
        {
            // 直接将流程终止掉
            pauseResetEvent.Reset();

            // 防止程序暂停等待还没有触发，就结束流程导出程序停止
            Thread.Sleep(100);

            // 开启异步线程
            Task.Run(() =>
            {
                // 清理历史运行的模块
                //RunningModules.Clear();

                // 更新模块状态 一定要按照开始顺序来暂停和恢复 
                foreach (var item in pauseModules)
                {
                    if (item.TaskFunction is IPauseFunction p)
                    {
                        // 保证轴模块和报警模块才会触发暂停
                        if ((p.IsNeedPause && item.Status == RunStatus.Running) || item.Status == RunStatus.Alarmed)
                        {
                            item.Status = RunStatus.Pause;
                        }
                    }
                }

                int curVal = 0;
                int total = PausedModules.Count;
                LoadingEvent?.Invoke(total, curVal, "模块开始暂停..");
                Exception caughtEx = null;
                bool isSuccess = true;

                try
                {
                    // 单独执行暂停功能，因为轴相关模块会暂停
                    foreach (var item in PausedModules)
                    {
                        var func = item.Value as IMotionFunction;

                        item.Value.Pause();
                        Progress(total, ref curVal, $"模块:{func.Owner.Alias} 暂停");
                    }
                }
                catch (Exception ex)
                {
                    isSuccess = false;
                    caughtEx = ex;
                    OnLog(LogType.Error, $"暂停失败:{ex.Message}");
                }
                finally
                {
                    LoadingEvent(total, total, "暂停完成");

                    action?.Invoke(isSuccess, caughtEx);
                }

                // 更新模块状态
                if (isSuccess)
                {
                    EngineStatus = isAlarm ? EngineStatus.Alarm : EngineStatus.Pause;
                }
            });
        }

        /// <summary>
        /// 恢复运行
        /// </summary>
        public void Recovery(Action<bool> action = null)
        {
            if (EngineStatus == EngineStatus.Pause)
            {
                pauseResetEvent?.Set();

                // 更新机台状态
                EngineStatus = EngineStatus.Running;
            }

            action?.Invoke(true);
        }

        /// <summary>
        /// 复位
        /// </summary>
        /// <param name="action"></param>
        public void Reset(AlarmInfo alarmInfo, Action<bool> callback = null)
        {
            Task.Run(() =>
            {
                bool isSuccess = true;
                string errMsg = string.Empty;
                int curVal = 0;
                int total = PausedModules.Count;
                List<Guid> resetIDs = new List<Guid>();
                var srcS = EngineStatus;
                OnLog(LogType.Debug, $"total值为:{total} ");
                // 总数量贴膜复检 开始复位.. 
                if (total > 0)
                {
                    // 切换状态到复位中
                    EngineStatus = EngineStatus.Resetting;
                    LoadingEvent?.Invoke(total, curVal, "模块开始复位..");

                    // 循环还原对象,此时优先还原暂停的模块
                    foreach (var item in PausedModules.OrderBy(u => u.Value.StartTime))
                    {
                        var module = Get(item.Key);

                        OnLog(LogType.Debug, $"模块:{module.Alias} 开始复位..");
                        DateTime rStartTime = module.StartTime;
                        isSuccess = module.DoFunction();

                        // 判断当前机台是否点击了停止
                        bool isStop = EngineStatus == EngineStatus.Stop;
                        if (isStop)
                        {
                            // 如果还原失败，同步也要还原开始时间
                            module.StartTime = rStartTime;
                            srcS = EngineStatus.Stop;
                            OnLog(LogType.Debug, $"程序停止，复位未成功!");
                            break;
                        }

                        errMsg = module.StatusMsg;
                        if (module.AlarmInfo != null && (module.AlarmInfo.AlarmType != AlarmType.InfoTip && module.AlarmInfo.AlarmType != AlarmType.PlcAlarm && module.AlarmInfo.AlarmType != AlarmType.RetryAlarm))
                        {
                            // 如果还原失败，同步也要还原开始时间
                            module.StartTime = rStartTime;

                            // 针对Alarm报警模块
                            module.Status = RunStatus.Alarmed;
                            Progress(total, ref curVal, $"模块:{module.Alias} 复位失败!");
                            //复位失败后直接停机
                            //ResetFailEvent?.Invoke();

                            break;
                        }
                        else
                        {
                            resetIDs.Add(item.Key);
                            Progress(total, ref curVal, $"模块:{module.Alias} 复位完成");
                        }
                    }
                }


                // 如果所有都复位成功，更新流程状态
                if (resetIDs.Count == PausedModules.Count)
                {
                    // 此时程序没有运行，但是触发报警，那么状态要切换到Ready状态
                    if (alarmInfo != null)
                    {
                        if (alarmInfo.EStatus == EngineStatus.Ready)
                        {
                            EngineStatus = EngineStatus.Ready;
                            pauseResetEvent?.Set();
                        }
                        else if (alarmInfo.EStatus == EngineStatus.Idle)
                        {
                            EngineStatus = EngineStatus.Idle;
                            pauseResetEvent?.Set();
                        }
                        else
                        {
                            EngineStatus = EngineStatus.Pause;
                        }
                    }
                    else
                    {
                        EngineStatus = EngineStatus.Pause;
                    }

                    callback?.Invoke(true);
                }
                else
                {
                    // 切换状态到复位中
                    EngineStatus = srcS;
                    LoadingEvent(total, total, "复位失败!");
                    OnLog(LogType.Error, $"暂停失败:{errMsg}");

                    callback?.Invoke(false);
                }

                // 移除掉复位成功的模块
                if (resetIDs.Count > 0)
                {
                    foreach (var item in resetIDs)
                    {
                        PausedModules.TryRemove(item, out var v);
                    }
                }

                resetIDs.Clear();

            });
        }

        /// <summary>
        /// 停止,当前已有料运行完成
        /// </summary>
        public void Stop(Action callback = null)
        {
            if (EngineStatus == EngineStatus.Stop) return;

            // 防止回零异常时，异步线程中的任务没有停止导致程序撞机
            if (pauseResetEvent != null && pauseResetEvent.IsSet)
            {
                pauseResetEvent.Reset();
            }

            // 开启异步线程
            Task.Run(() =>
            {
                // 转换为空闲状态
                tokenSource?.Cancel();

                try
                {
                    // 清理待复位的模块，防止停止后，没有复位的模块继续复位
                    PausedModules?.Clear();

                    // 模块进行停止处理
                    foreach (var item in this)
                    {
                        // NG 工站不中断Break
                        if (item.Station == null)
                        {
                            item.UpdateStation();

                        }

                        if (item.Station != null && item.Station.TaskFunction is INGStation)
                        {
                            continue;
                        }

                        item.IsBreak = true;

                        // 执行停止模块,模块必须是运行还实现Stop才进行停止动作
                        if (item.Status == RunStatus.Running && item.TaskFunction is IMotionFunction stop)
                        {
                            stop.Stop();
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    // 直接将状态更新为停止状态，防止Stop方法报警，状态变为报警状态
                    EngineStatus = EngineStatus.Stop;
                    callback?.Invoke();
                    // 必须要放到最后，防止点击暂停后，再点击停止导致程序还会运行下一步
                    pauseResetEvent?.Set();
                }
            });
        }

        /// <summary>
        /// 设置默认状态
        /// </summary>
        /// <param name="motion"></param>
        public void SetDefaultStatus(RunStatus status)
        {
            // 将所有模块状态初始化
            foreach (var item in this)
            {
                if (item.Status == RunStatus.Skip) continue;
                item.Status = status;
                if (status == RunStatus.Default)
                {
                    item.StatusMsg = "";

                    // 复位CT
                    item.StartTimer();
                }
            }
        }

        /// <summary>
        /// 设置模块状态
        /// </summary>
        /// <param name="motionModule"></param>
        /// <param name="status"></param>
        private void SetModuleStatus(IMotionModule motionModule, RunStatus status)
        {
            // 忽略状态的模块不重置状态，避免忽略被意外取消
            if (motionModule.Status == RunStatus.Skip) return;

            motionModule.Status = status;
            if (motionModule.Children.Count > 0)
            {
                foreach (var item in motionModule.Children)
                {
                    if (item.Status == RunStatus.Skip) continue;

                    item.Status = status;
                    item.StatusMsg = string.Empty;
                    item.StartTimer();
                    SetModuleStatus(item, status);
                }
            }
        }

        /// <summary>
        /// 模块中断
        /// </summary>
        /// <param name="motionModule"></param>
        /// <param name="isBreak"></param>
        private void SetModuleBreak(IMotionModule motionModule, bool isBreak)
        {
            motionModule.IsBreak = isBreak;

            if (motionModule.Children.Count > 0)
            {
                foreach (var item in motionModule.Children)
                {
                    item.IsBreak = isBreak;
                    SetModuleBreak(item, isBreak);
                }
            }
        }

        /// <summary>
        /// 查找隶属工站
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        public void GetOwnerStation(IMotionModule module, ref IProStation station)
        {
            var parent = module.Parent;
            if (parent == null) return;

            if (parent.TaskFunction is IProStation)
            {
                station = parent.TaskFunction as IProStation;
            }
            else
            {
                GetOwnerStation(parent, ref station);
            }
        }

        /// <summary>
        /// 报警
        /// </summary>
        /// <param name="type"></param>
        /// <param name="msg"></param>
        public void OnAlarm(AlarmInfo alarmInfo)
        {
            //AlarmEvent_MEngine?.Invoke(alarmInfo);           
            //Thread.Sleep(500);
            M_AlarmEvent(alarmInfo);
        }

        #endregion

        #region 数据事件

        private ConcurrentDictionary<string, List<StationTime>> cacheTimes = new ConcurrentDictionary<string, List<StationTime>>();

        /// <summary>
        /// 工站时间
        /// </summary>
        /// <param name="obj">集合对下</param>
        private void Station_StationTimeEvent(List<StationTime> cts, bool IsUseLog)
        {
            List<StationTime> ctInfos = new List<StationTime>();
            // 获取当前班次信息
            if (cts.Count == 0)
            {
                return;
            }
            //将CT统计和CTlog标准化的事件提前，不进行判断SN
            TimeStatisEvent?.Invoke(cts, IsUseLog);
            //由于一个自由工站里面可能存在两个SN，一个是上个遗留的，一个是过程中赋值的
            //所以从最后开始往前找第一个有效SN；
            var lastValidSn = "None";//给初始值，
            lastValidSn = cts.Where(x => !string.IsNullOrEmpty(x.SN) && !string.Equals(x.SN, "None")).LastOrDefault().SN;
            if (string.IsNullOrEmpty(lastValidSn) || string.Equals(lastValidSn, "None"))
            {
                Debug.WriteLine($"当前工站{cts[0].Station}未找到SN");
                return;
            }
            Debug.WriteLine($"当前工站{cts[0].Station}找到SN{lastValidSn}");
            //给单个自由工站里面的所有模块赋值SN
            for (int i = 0; i < cts.Count; i++)
            {
                var stationTime = cts[i];
                stationTime.SN = lastValidSn;
                cts[i] = stationTime;
            }
            ctInfos = new List<StationTime>(cts);
            VisionEvent?.Invoke(ctInfos, IsUseLog);
            OnLog(LogType.Info, $"工站名称{ctInfos[0].Station}");
        }

        /// <summary>
        /// 模块更新
        /// </summary>
        /// <param name="arg1">模块</param>
        /// <param name="arg2">模块更新类别</param>
        private void Module_UpdateEvent(IModule arg1, ModuleUpdate arg2)
        {
            ModuleUpdateEvent?.Invoke(arg1 as IMotionModule, arg2);
        }

        // 声明一千个字符
        StringBuilder _loadSB = new StringBuilder(2000);

        /// <summary>
        /// 产品入料事件
        /// </summary>
        /// <param name="module">模块</param>
        /// <param name="s">入料结果信息</param>
        private void Module_ProLoadedEvent(IMotionModule module, StationResult station, List<LColumn> data)
        {
            // 入料事件一旦触发,就创建一条缓存记录
            string dataID = station.ProCode;
            CacheManager.AddItem(dataID);

            // 字符串缓存
            _loadSB.Clear();
            _loadSB.Append($"入料事件: SN:{dataID} ");
            //File.WriteAllText("D://SN.txt", dataID);
            var cacheItem = CacheManager.GetItem(dataID) as CacheItem;

            if (cacheItem != null)
            {
                cacheItem.EnterTime = station.EnterTime;
            }

            // 工站上如果存在数据的情况下
            if (cacheItem != null && data != null && data.Count() > 0)
            {
                cacheItem.EnterTime = station.EnterTime;

                foreach (var item in data)
                {
                    cacheItem.Add(item);
                    _loadSB.Append($"{item.Alias}:{item.Value} ");
                }

                // 添加全局变量
                if (_globalModule.IsOutData)
                {
                    foreach (var item in _globalModule.Parameters)
                    {
                        if (!item.Value.IsOutData) continue;

                        string alias = item.Value.Alias;

                        if (!cacheItem.Any(u => u.Alias == alias))
                        {
                            var lCol = new LColumn()
                            {
                                ParamKey = alias,
                                ID = _globalModule.ID,
                                Alias = alias,
                                Value = item.Value.Value
                            };

                            cacheItem.Add(lCol);

                            _loadSB.Append($"{lCol.Alias}:{lCol.Value} ");
                        }
                    }
                }

                // 清理数据
                if (module.Station.TaskFunction is IStation s && s.Datas.Count > 0)
                {
                    s.Datas.Clear();
                }
            }

            // 触发入料事件
            OnLog(LogType.Info, $"入料事件:SN:{station.ProCode} {_loadSB}");

            ProLoadedEvent?.Invoke(module, station);
        }

        /// <summary>
        /// 数据上传
        /// </summary>
        /// <param name="moduleID">模块ID</param>
        /// <param name="code">产品编码</param>
        /// <param name="uploadDatas">缓存的数据</param>
        private void Module_DataUploadEvent(Guid moduleID, string code, List<LColumn> keyDatas)
        {
            CacheManager.AddItemData(code, moduleID, keyDatas);
        }

        // 声明一千个字符
        StringBuilder _unloadSB = new StringBuilder(2000);

        /// <summary>
        /// 产品出料
        /// </summary>
        /// <param name="module">模块</param>
        /// <param name="barCode">二维码</param>
        /// <param name="sResult">最终结果</param>
        /// <param name="data">最终数据</param>
        /// <exception cref="NotImplementedException"></exception>
        private void Module_ProUnloadedEvent(IMotionModule module, StationResult s)
        {
            // 字符串缓存
            _unloadSB.Clear();
            _unloadSB.Append($"出料事件: SN:{s.ProCode},JIG:{s.JigCode},Result:{s.Result} ");
            //File.WriteAllText("D://SN.txt",s.ProCode);

            try
            {
                Dictionary<string, object> outData = new Dictionary<string, object>();
                var cacheItem = CacheManager.GetItem(s.ProCode) as CacheItem;
                if (cacheItem != null && cacheItem.IsDeleted == false)
                {
                    s.EnterTime = cacheItem.EnterTime;
                    // 解析要进行上传的数据
                    foreach (var item in cacheItem)
                    {
                        if (!outData.ContainsKey(item.Alias))
                        {
                            if (item.Value.ToString() == "True")
                                item.Value = "OK";
                            else if (item.Value.ToString() == "False")
                                item.Value = "NG";
                            outData.Add(item.Alias, item.Value);
                            _unloadSB.Append($"{item.Alias}:{item.Value} ");
                        }
                        else
                        {
                            //OnLog(LogType.Warning, $"数据:{s.ProCode} 缓存字段:{item.Alias} 重复!");
                            string bString = outData[item.Alias].ToString();
                            string aString = item.Value.ToString();
                            if (string.IsNullOrEmpty(bString) && !string.IsNullOrEmpty(aString))
                            {
                                if (item.Value.ToString() == "True")
                                    item.Value = "OK";
                                else if (item.Value.ToString() == "False")
                                    item.Value = "NG";

                                outData[item.Alias] = item.Value;
                                _unloadSB.Append($"{item.Alias}:{item.Value} ");
                                OnLog(LogType.Warning, $"数据:{s.ProCode} 缓存字段:{item.Alias}为空, update to [{item.Value}]!");
                            }
                        }
                    }
                }

                // 检测全局变量是否有作为输出参数
                if (_globalModule.IsOutData)
                {
                    foreach (var item in _globalModule.Parameters)
                    {
                        if (!item.Value.IsOutData) continue;

                        //string alias = item.Value.Alias;
                        //if (!outData.ContainsKey(alias))
                        //{
                        //    if (item.Value.Value.ToString() == "True")
                        //        item.Value.Value = "OK";
                        //    else if (item.Value.Value.ToString() == "False")
                        //        item.Value.Value = "NG";

                        //    outData.Add(alias, item.Value.Value);
                        //    _unloadSB.Append($"{alias}:{item.Value.Value} ");
                        //}

                        if (MapDatas.Find(X => X.Key.Equals(item.Key)) is var vau)
                        {
                            if (vau != null)
                            {
                                if (!outData.ContainsKey(vau.Alias))
                                {
                                    if (item.Value.Value.ToString() == "True")
                                        item.Value.Value = "OK";
                                    else if (item.Value.Value.ToString() == "False")
                                        item.Value.Value = "NG";

                                    outData.Add(vau.Alias, item.Value.Value);
                                    _unloadSB.Append($"{vau.Alias}:{item.Value.Value} ");
                                }
#if True
                                else
                                {
                                    string bString = outData[vau.Alias].ToString();
                                    string aString = item.Value.Value.ToString();
                                    if (item.Value.Type.Name.Contains("Int") || item.Value.Type.Name.Contains("Double"))
                                    {
                                        if ((bString == "0") && (aString != "0"))
                                        {
                                            outData[vau.Alias] = aString;
                                            _unloadSB.Append($"{vau.Alias}:{item.Value} ");
                                            OnLog(LogType.Warning, $"数据:{s.ProCode} 缓存字段:{vau.Alias}为空, update to [{aString}]!");
                                        }
                                    }
                                    else
                                    { 
                                        if (string.IsNullOrEmpty(bString) && !string.IsNullOrEmpty(aString))
                                        {
                                            if (item.Value.ToString() == "True")
                                                outData[vau.Alias] = "OK";
                                            else if (item.Value.ToString() == "False")
                                                outData[vau.Alias] = "NG";
                                            else
                                                outData[vau.Alias] = aString;

                                                _unloadSB.Append($"{vau.Alias}:{item.Value} ");
                                            OnLog(LogType.Warning, $"数据:{s.ProCode} 缓存字段:{vau.Alias}为空, update to [{aString}]!");
                                        }
                                    }
                                }
#endif
                            }
                        }

                    }
                }
                ProUnloadedEvent?.Invoke(module, s, outData, ProUnloadedDBEvent);
            }
            catch (Exception ex)
            {
                OnLog(LogType.Error, $"出料事件异常:{ex.Message}");
            }
            finally
            {
                OnLog(LogType.Info, _unloadSB.ToString());

                // 处理完成后,要将记录都清理掉
                CacheManager.RemoveItem(s.ProCode);
            }

            //if (s.IsToss)
            //{
            //    OnAlarm(AlarmType.InfoTip, $"产品:{s.ProCode} Toss,{s.ErrMsg}", "Product Tossing");
            //}
            //else if (!s.Result)
            //{
            //    OnAlarm(AlarmType.InfoTip, $"产品:{s.ProCode} NG,{s.ErrMsg}", "Product NG");
            //}
        }

        /// <summary> 
        /// 产品抛料
        /// </summary>
        /// <param name="obj"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void Module_ProThrowEvent(StationResult stationResult, string moduleName, string material)
        {
            ProThrowEvent?.Invoke(stationResult, moduleName, material);
            OnLog(LogType.Info, $"抛料事件: 抛料模块：{moduleName} 物料名：{material}");
        }
#endregion
    }
}