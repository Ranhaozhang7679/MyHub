#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       MotionEngine
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.TaskFlow.Engine.Manager
* 文 件 名:       MotionEngine.cs
* 创建时间:       2022/5/18 11:38:40
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      b6e286af-6c7b-4f6f-ad43-efd94a5a124f
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/18 11:38:40
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Extensions;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.TaskFlow.Engine.Events;
using Luster.Motion.TaskFlow.Engine.Models;
using Luster.TaskFlow.Common;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Common.Interfaces;
using Luster.TaskFlow.Common.Models;
using Luster.TaskFlow.Common.Module;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Enums;
using Luster.TaskFlow.Motion.Interfaces;
using Luster.TaskFlow.Motion.Logic;
using Luster.TaskFlow.Motion.Modules;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using static FreeSql.Internal.GlobalFilter;

namespace Luster.Motion.TaskFlow.Engine
{
    public partial class MotionEngine : List<IMotionModule>, IMotionEngine, IModuleCollection
    {
        #region 字段

        /// <summary>
        /// 起始模块
        /// </summary>
        private Luster.TaskFlow.Motion.Logic.Root _rootModule;

        /// <summary>
        /// 全局模块
        /// </summary>
        private Luster.TaskFlow.Motion.Logic.GlobalModule _globalModule;

        /// <summary>
        /// 暂停中断线程
        /// </summary>
        private ManualResetEventSlim pauseResetEvent;

        /// <summary>
        /// 控制模块运行
        /// </summary>
        private MotionRunEngine motionRunEngine;

        /// <summary>
        /// 控制NG工站
        /// </summary>
        private ManualResetEventSlim ngBreakoff;

        /// <summary>
        /// 组合模块
        /// </summary>
        private Composition _composition;

        private EngineStatus _prvStatus = EngineStatus.Idle;

        /// <summary>
        /// 版本信息
        /// </summary>
        private string _version;
        #endregion

        #region 属性
        /// <summary>
        /// 容器对象
        /// </summary>
        public IIocManager IocManager { get; set; }

        /// <summary>
        /// 订单
        /// </summary>
        public IOrderManager OrderManager { get; set; }

        /// <summary>
        /// 卷料
        /// </summary>
        public IRollManager RollManager { get; set; }

        /// <summary>
        /// 系统配置
        /// </summary>
        public IConfigManager ConfigManager { get; set; }

        /// <summary>
        /// 灯管理器
        /// </summary>
        public ILightManager LightManager { get; set; }

        /// <summary>
        /// 错误管理器
        /// </summary>
        public IErrorManager ErrorManager { get; set; }

        /// <summary>
        /// 解析算子的工厂
        /// </summary>
        public IModuleFactory Factory { get; set; }

        /// <summary>
        /// 运行的编号
        /// </summary>
        public string ExcutorNo { get; set; }

        /// <summary>
        /// 数据关联对象
        /// </summary>
        public List<MapData> MapDatas { get; set; }

        /// <summary>
        /// 缓存管理器
        /// </summary>
        public ICacheManager CacheManager { get; set; }

        /// <summary>
        /// 支持暂停模块
        /// </summary>
        private List<IMotionModule> pauseModules;

        /// <summary>
        /// 数据库访问
        /// </summary>
        private IDbHelper DbHelper { get; set; }
        #endregion

        private Dictionary<Guid, string> dicGuidToModule = new Dictionary<Guid, string>();

        /// <summary>
        /// 模块ID到模块的字典缓存，用于O(1)快速查找
        /// </summary>
        private readonly Dictionary<Guid, IMotionModule> _moduleIdCache = new Dictionary<Guid, IMotionModule>();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="factory"></param>
        public MotionEngine(IModuleFactory factory, IDeviceEngine deviceEngine, ICacheManager cacheManager, IOrderManager orderManager, IConfigManager configManager,
            ILightManager lightManager, IDbHelper dbHelper, IIocManager iocManager, IRollManager rollManager, IErrorManager errorManager)
        {
            Factory = factory;
            Initialize();
            RegisterTypes();
            EngineStatus = EngineStatus.Idle;
            CacheManager = cacheManager;
            OrderManager = orderManager;
            ConfigManager = configManager;
            LightManager = lightManager;
            RollManager = rollManager;
            DbHelper = dbHelper;
            ErrorManager = errorManager;

            // 用于控制线程暂停
            pauseResetEvent = new ManualResetEventSlim(true);

            //控制ng工站
            ngBreakoff = new ManualResetEventSlim(true);


            // 设备引擎
            DeviceEngine = deviceEngine;

            // 运行引擎
            motionRunEngine = new MotionRunEngine();

            // 数据对象
            MapDatas = new List<MapData>();

            // 组合模块
            _composition = new Composition(this);

            // 工作流
            WorkFlow = new WorkFlow(this);

            // 初始化
            PausedModules = new ConcurrentDictionary<Guid, IPauseFunction>();

            // 支持暂停的模块
            pauseModules = new List<IMotionModule>();

            // 版本信息
            _version = Luster.Common.Tools.SoftVersionTool.GetVersion("LusterMotion.exe");
            IocManager = iocManager;

            //控制参数发生变化
            deviceEngine.ControlValueChangedEvent -= DeviceEngine_ControlValueChangedEvent;
            deviceEngine.ControlValueChangedEvent += DeviceEngine_ControlValueChangedEvent;

            //设备名称发生变化
            deviceEngine.DeviceNameChangedEvent -= DeviceEngine_DeviceNameChangedEvent;
            deviceEngine.DeviceNameChangedEvent += DeviceEngine_DeviceNameChangedEvent;

        }

        private void DeviceEngine_DeviceNameChangedEvent(Guid guid, string oldName, string newName)
        {
            foreach (var item in this)
            {
                if (item.TaskFunction != null)
                {
                    bool needUiRefresh = false;
                    foreach (var para in item.Parameters)
                    {
                        if (para.Value.Value is VDevice vDevice && vDevice.DeviceID == guid)
                        {
                            object newDeviceObj = null;

                            // 必须创建一个独立于原引用地址的新实例。
                            // 这是因为WPF自定义编辑器（如VIO，底层可能是DependencyProperty）只在对象引用（地址）变化时才会触发完整的视觉刷新重绘。
                            // 单纯改变原对象的属性（即使加了INotifyPropertyChanged）往往不会被那些没有进行TwoWay内部属性绑定的自定义UI控件侦测到。
                            if (vDevice.GetType() == typeof(VDevice))
                            {
                                newDeviceObj = new VDevice { DeviceID = vDevice.DeviceID, Name = newName, Virtual = vDevice.Virtual };
                            }
                            else if (vDevice is VAxisDevice axisDevice)
                            {
                                axisDevice.Name = newName;
                                var clonedAxis = axisDevice.Clone() as VAxisDevice;
                                if (clonedAxis != null)
                                {
                                    clonedAxis.DeviceID = vDevice.DeviceID;
                                    clonedAxis.Name = newName;
                                    clonedAxis.Virtual = vDevice.Virtual;
                                }
                                newDeviceObj = clonedAxis;
                            }
                            else if (vDevice is VAxisMDevice mDevice)
                            {
                                mDevice.Name = newName;
                                newDeviceObj = new VAxisMDevice(mDevice) { Name = newName, DeviceID = guid, Virtual = mDevice.Virtual };
                            }

                            if (newDeviceObj != null)
                            {
                                para.Value.Property?.SetValue(item.TaskFunction, newDeviceObj);
                                para.Value.Value = newDeviceObj;
                                needUiRefresh = true;
                            }
                        }
                    }

                    if (needUiRefresh)
                    {
                        item.OnUpdate(Luster.TaskFlow.Common.Enums.ModuleUpdate.ParameterNum);
                    }
                }
            }
        }

        //控制参数发生变化
        private void DeviceEngine_ControlValueChangedEvent(Guid guid, string name, string value)
        {
            foreach (var item in this)
            {
                if (item.TaskFunction is IMotionFunction m)
                {
                    if (item.ID == Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"))
                    {
                        foreach (var global in item.Parameters)
                        {
                            if (global.Key == $"Extend_{name}")
                            {
                                global.Value.Value = value;
                                break;
                            }
                        }

                    }


                    if (item.ID == guid)
                    {
                        foreach (var para in item.Parameters)
                        {
                            if (para.Value.CN == name)
                            {
                                para.Value.Value = value;
                            }
                        }
                    }

                }
            }
        }

        /// <summary>
        /// 注册系统用到数据类型
        /// </summary>
        public virtual void RegisterTypes()
        {
            ParameterAttribute.ResgisterTypeMaps("Int", typeof(int));
            ParameterAttribute.ResgisterTypeMaps("String", typeof(string));
            ParameterAttribute.ResgisterTypeMaps("Double", typeof(double));
            ParameterAttribute.ResgisterTypeMaps("Bool", typeof(bool));
            ParameterAttribute.ResgisterTypeMaps("DateTime", typeof(DateTime));
            ParameterAttribute.ResgisterTypeMaps("Status", typeof(LStatus));

            ParameterAttribute.ResgisterTypeMaps("StringEx", typeof(LStringEx));

            ParameterAttribute.ResgisterTypeMaps("BArray", typeof(LArray<bool>));
            ParameterAttribute.ResgisterTypeMaps("DArray", typeof(LArray<double>));
            ParameterAttribute.ResgisterTypeMaps("IArray", typeof(LArray<int>));
            ParameterAttribute.ResgisterTypeMaps("SArray", typeof(LArray<string>));

            //ParameterAttribute.ResgisterTypeMaps("Tolerance", typeof(LTolerance));
            //ParameterAttribute.ResgisterTypeMaps("VDevice", typeof(VDevice));
            //ParameterAttribute.ResgisterTypeMaps("VAxisMDevice", typeof(VAxisMDevice));
        }

        #region 事件
        /// <summary>
        /// 属性变更
        /// </summary>
        public event Action<IMotionModule, string, object, object> PropertyChanged;

        /// <summary>
        /// 模块更新
        /// </summary>
        public event Action<IMotionModule, ModuleUpdate> ModuleUpdateEvent;

        /// <summary>
        /// Log日志
        /// </summary>
        public event Action<LogType, string, string> LogEvent;

        /// <summary>
        /// 多语言
        /// </summary>
        public event Func<string, string> LanguageEvent;

        /// <summary>
        /// 模块报警
        /// </summary>
        public event Action<AlarmInfo> AlarmEvent_MEngine;

        /// <summary>
        /// 回调
        /// </summary>
        public event Action<PostRunEventArgs> PostRunEvent;

        /// <summary>
        /// 模块运行更新
        /// </summary>
        public event Action<IMotionModule, ModuleUpdate> MouleUpdateEvent;

        /// <summary>
        /// 模块变化引起输出项配置变化
        /// </summary>
        public event Action MapDataChangeEvent;


        public event Func<string, InteractiveType> DialogBoxEvent;

        #endregion

        // 太科电批注册事件
        public event Action<object, string> TaiKeScrewRegisterEvent;


        // 太科电批注册事件
        public event Action<object, string> PressRegisterEvent;

        //工站结束时间事件
        public event Action<string, string, string, bool> StationTimeEvent;
        public event Action<object, string> ToeinForceRegisterEvent;

        #region 任务运行
        /// <summary>
        /// 单步执行
        /// </summary>
        /// <param name="mId">运行模块ID</param>
        /// <returns>是否运行成功</returns>
        public bool Run(Guid mId)
        {
            // 1.初始化运行状态
            EngineStatus = EngineStatus.Running;

            // 2.定义成功与否标识
            bool isSuccess = true;

            // 3.执行异常记录错误提示
            string errMsg = string.Empty;
            // 4.获取要运行的模块
            IMotionModule runModule = Get(mId);

            // 5.模块运行
            motionRunEngine.Run(runModule, ref isSuccess);

            // 运行完成，状态变为空闲状态
            EngineStatus = EngineStatus.Idle;

            return isSuccess;
        }

        /// <summary>
        /// 执行流程
        /// </summary>
        /// <returns></returns>
        public void RunAll()
        {
            taskFactory = new TaskFactory();
            var Stations = GetStations();

            EngineStatus = EngineStatus.Running;
            this.ForEach(u => u.IsBreak = false);

            // 记录所有
            List<Task> tasks = new List<Task>();

            // 每个工站开启一个独立的线程
            foreach (var item in Stations)
            {
                tasks.Add(taskFactory.StartNew((obj) =>
                {
                    var stationModule = obj as IMotionModule;
                    var station = stationModule.TaskFunction as IStation;

                    stationModule.StatusMsg = "";

                    // 模块运行
                    bool isSuccess = stationModule.DoFunction();
                    if (!isSuccess)
                    {
                        if (EngineStatus != EngineStatus.Stop)
                        {
                            LogEvent.Invoke(LogType.Error, $"模块:{stationModule.Alias} 运行失败,程序暂停！", ExcutorNo);
                        }
                       
                        // 停止
                        Stop();

                        //此时非正常停止，需要报警红灯蜂鸣
                        LightManager.StopLight(true);


                    }

                }, item));
            }

            // 等待运行完成
            Task.WaitAll(tasks.ToArray());

            // 更新状态
            EngineStatus = EngineStatus.Idle;
        }

        /// <summary>
        /// 单独运行
        /// </summary>
        /// <param name="mId"></param>
        /// <returns></returns>
        public bool RunOne(Guid mId)
        {
            IMotionModule module = Get(mId);
            if (module != null)
            {
                module.IsBreak = false;

                module.DoFunction();
                if (module.AlarmInfo != null)
                {
                    // 统一通过运行器进行报警事件挂载与上抛
                    motionRunEngine.RegisterAndRaiseAlarm(module);
                }

                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// 运行所有NG工站
        /// </summary>
        /// <returns></returns>
        public void RunNGStation()
        {
            taskFactory = new TaskFactory();
            var Stations = GetStations();
            foreach (var item in this)
            {
                if (item.Station != null && item.Station.TaskFunction.Alias == "NGStation")
                {
                    item.IsBreak = false;
                    item.BrokenOff = ngBreakoff;
                }
            }
            // 记录所有
            List<Task> tasks = new List<Task>();


            // 每个工站开启一个独立的线程
            foreach (var item in Stations)
            {
                var ngmodule = item.TaskFunction as INGStation;
                if (ngModule != null)
                {
                    tasks.Add(taskFactory.StartNew((obj) =>
                    {

                        var stationModule = obj as IMotionModule;
                        var station = stationModule.TaskFunction as INGStation;

                        stationModule.StatusMsg = "";


                        // 后处理
                        if (stationModule != null && stationModule.TaskFunction is INGStation ngFunction)
                        {
                            if (stationModule.Parameters.ContainsKey(nameof(ngFunction.MachineStatus)))
                            {
                                var mStatus = stationModule.Parameters[nameof(ngFunction.MachineStatus)];
                                if (mStatus != null && ((EngineStatus)mStatus.Value & EngineStatus) == EngineStatus)
                                {
                                    if (!(EngineStatus == EngineStatus.Alarm && (_prvStatus == EngineStatus.Pause || _prvStatus == EngineStatus.Alarm || _prvStatus == EngineStatus.Stop || _prvStatus == EngineStatus.Resetting)))
                                    {
                                        // 执行NG模块
                                        bool isSuccess = stationModule.DoFunction();
                                        if (!isSuccess)
                                        {
                                            LogEvent.Invoke(LogType.Error, $"模块:{stationModule.Alias} 运行失败！", ExcutorNo);
                                        }
                                    }
                                    //_prvStatus = EngineStatus;
                                }
                            }
                        }

                    }, item));
                }
            }

            // 等待运行完成
            Task.WaitAll(tasks.ToArray());
        }

        // 移除重复的报警挂载逻辑，统一由 MotionRunEngine 处理

        /// <summary>
        /// 单次运行与停止
        /// </summary>
        public void RunStop()
        {
            EngineStatus = EngineStatus.Idle;

            // 引擎停止
            DeviceEngine.Stop();

            // 流程终止,中断循环
            this.ForEach(u =>
            {
                u.IsBreak = true;
                if (u.TaskFunction is IStopFunction stop)
                {
                    stop.Stop();
                }
            });
            Task.Run(() =>
            {
                Thread.Sleep(2000);

                // 流程终止,中断循环
                this.ForEach(u =>
                {
                    u.IsBreak = false;
                });

                // 在启动
                pauseResetEvent?.Set();
            });
        }
        #endregion

        #region 算子管理
        /// <summary>
        /// 添加新的算子
        /// </summary>
        /// <param name="module">模块</param>
        public void Insert(IMotionModule module, int index = -1, IMotionModule pModule = null)
        {
            // 1.设置父节点
            if (pModule == null)
            {
                pModule = GetParentModule();
            }

            module.Parent = pModule;
            module.Level = pModule.Level + 1;

            // 2.设置对应的集合
            module.TaskModules = this;
            module.DeviceEngine = DeviceEngine;
            module.BrokenOff = pauseResetEvent;
            module.CacheManager = CacheManager;
            module.Version = _version;
            module.OrderManager = OrderManager;
            module.DbHelper = DbHelper;
            module.LightManager = LightManager;
            module.ErrorManager = ErrorManager;
            module.ConfigManager = ConfigManager;
            module.Ioc = IocManager;
            module.RollManager = RollManager;

            // 添加到Squence列表中
            if (index == -1)
            {
                // 添加到序列中
                Add(module);
                pModule.Children.Add(module);
            }
            else
            {
                Insert(index, module);
                pModule.Children.Insert(index, module);
            }

            // 添加到模块ID缓存
            _moduleIdCache[module.ID] = module;

            // 记录暂停模块
            if (module.TaskFunction is IMotionFunction mFunc)
            {
                mFunc.Added();

                // 缓存暂停模块
                if (mFunc is IPauseFunction)
                {
                    pauseModules.Add(module);
                }
            }

            // 11.工站耗时统计事件
            if (module.TaskFunction is IStation station)
            {
                station.StationTimeEvent -= Station_StationTimeEvent;
                station.StationTimeEvent += Station_StationTimeEvent;
            }
        }

        /// <summary>
        /// 通过模块名称和对应的函数名称来创建
        /// </summary>
        /// <param name="moduleName">模块名称</param>
        /// <param name="funcName">函数名称</param>
        /// <returns>模块</returns>
        public IMotionModule CreateByName(string moduleName, string funcName)
        {
            // 1.获取对应的Creator
            var mCreator = Factory.GetCreator(moduleName, "HoloMotion");
            if (mCreator == null)
            {
                throw new FriendlyException($"模块:{moduleName} 不存在!");
            }

            IMotionModule newModule = mCreator.Create() as IMotionModule;

            RegisterModuleEvent(newModule);

            newModule.SetFunction(funcName);

            // 默认排除最后
            newModule.Sort = Count + 1;
            newModule.TaskModules = this;

            return newModule;
        }

        // <summary>
        /// 通过ID删除模块
        /// </summary>
        /// <param name="mID"></param>
        public void RemoveByID(Guid mID)
        {
            RemoveAll(u => u.ID == mID);

            // 从模块ID缓存中删除
            _moduleIdCache.Remove(mID);

            pauseModules.RemoveAll(u => u.ID == mID);

            // 删除映射对象
            if (MapDatas != null && MapDatas.Count > 0)
            {
                MapDatas.RemoveAll(x => x.ID == mID);
                MapDataChangeEvent?.Invoke();
            }
        }

        /// <summary>
        /// 注册事件
        /// </summary>
        /// <param name="module"></param>
        private void RegisterModuleEvent(IMotionModule module)
        {
            // 1.给模块注册事件
            module.LogEvent -= OnLog;
            module.LogEvent += OnLog;

            // 2.注册报警事件
            module.AlarmEvent_module -= M_AlarmEvent;
            module.AlarmEvent_module += M_AlarmEvent;

            // 3.多语言
            module.LanguageEvent -= Module_LanguageEvent;
            module.LanguageEvent += Module_LanguageEvent;

            // 4.产品入料
            module.ProLoadedEvent -= Module_ProLoadedEvent;
            module.ProLoadedEvent += Module_ProLoadedEvent;

            // 5.产品出料
            module.ProUnloadedEvent -= Module_ProUnloadedEvent;
            module.ProUnloadedEvent += Module_ProUnloadedEvent;

            // 6.产品抛料
            module.ProThrowEvent -= Module_ProThrowEvent;
            module.ProThrowEvent += Module_ProThrowEvent;

            // 7.数据上传
            module.DataUploadEvent -= Module_DataUploadEvent;
            module.DataUploadEvent += Module_DataUploadEvent;

            // 8.产品更新
            module.UpdateEvent -= Module_UpdateEvent;
            module.UpdateEvent += Module_UpdateEvent;

            // 9.模块状态变更
            module.PropertyChangedEvent -= Module_PropertyChangedEvent;
            module.PropertyChangedEvent += Module_PropertyChangedEvent;

            //10.弹窗交互
            module.DialogBoxEvent -= Module_DialogBoxEvent;
            module.DialogBoxEvent += Module_DialogBoxEvent;

            //11.太科电批事件 只有太科电批模块有这个功能
            module.TaiKeScrewRegisterEvent -= Module_TaiKeScrewRegisterEvent;
            module.TaiKeScrewRegisterEvent += Module_TaiKeScrewRegisterEvent;


            //压力
            module.PressRegisterEvent -= Module_PressRegisterEvent;
            module.PressRegisterEvent += Module_PressRegisterEvent;

            module.ToeinForceRegisterEvent -= Module_ToeinForceRegisterEvent;
            module.ToeinForceRegisterEvent += Module_ToeinForceRegisterEvent;


            //13.工站时间结束事件
            module.StationTimeEvent -= Module_StationTimeEvent;
            module.StationTimeEvent += Module_StationTimeEvent;


        }

        private void Module_StationTimeEvent(string WIP, string InputTime, string OutputTime, bool Result)
        {
            StationTimeEvent?.Invoke(WIP, InputTime, OutputTime, Result);
        }
        private void Module_TaiKeScrewRegisterEvent(object obj, string name)
        {
            TaiKeScrewRegisterEvent?.Invoke(obj, name);
        }


        private void Module_PressRegisterEvent(object obj, string name)
        {
            PressRegisterEvent?.Invoke(obj, name);
        }

        private void Module_ToeinForceRegisterEvent(object obj, string name)
        {
            ToeinForceRegisterEvent?.Invoke(obj, name);
        }


        private InteractiveType Module_DialogBoxEvent(string content)
        {
            return DialogBoxEvent.Invoke(content);
        }

        private void Module_PropertyChangedEvent(IModule module, string propName, object srcV, object newV)
        {
            if (propName == "Status" && module.TaskFunction is IPauseFunction opControl)
            {
                var curStatus = newV.To<RunStatus>();

                // 如果模块处于报警或者暂停
                if (curStatus == RunStatus.Pause || curStatus == RunStatus.Alarmed)
                {
                    // 如果是报警模块，并且是信息提示或弹窗提示，不添加到恢复模块
                    if (module.TaskFunction is IAlarm a && (a.AlarmType == AlarmType.InfoTip || a.AlarmType == AlarmType.PopInfoTip))
                    {
                        return;
                    }

                    if (!PausedModules.ContainsKey(module.ID))
                    {
                        PausedModules.TryAdd(module.ID, opControl);
                        OnLog(LogType.Debug, $"模块:{module.Alias} 进入待复位状态!");
                    }
                }
            }

            // 用于外部事件注册
            if (module.Parameters.ContainsKey(propName))
            {
                var pAttr = module.Parameters[propName];
                if (pAttr.ParamType == ParamType.IN && pAttr.IsBindUI)
                    PropertyChanged?.Invoke(module as IMotionModule, propName, srcV, newV);
            }
        }

        /// <summary>
        /// 模块报警
        /// </summary>
        /// <param name="module">模块</param>
        /// <param name="message">消息</param>
        private void M_AlarmEvent(AlarmInfo alarmInfo)
        {
            // 发出报警提示
            AlarmEvent_MEngine?.Invoke(alarmInfo);
        }

        /// <summary>
        /// 多语言查询
        /// </summary>
        /// <param name="key">多语言关键字</param>
        /// <returns>Key对应的多语言</returns>
        private string Module_LanguageEvent(string key)
        {
            return LanguageEvent?.Invoke(key);
        }

        /// <summary>
        /// 算子触发事件后，触发LogEvent方法
        /// </summary>
        /// <param name="type">日志类型</param>
        /// <param name="message">消息</param>
        public void OnLog(LogType type, string message, string logThreadID = "")
        {
            LogEvent?.Invoke(type, message, logThreadID);
        }

        #endregion

        #region 通用方法

        /// <summary>
        /// 获取节点
        /// </summary>
        /// <returns></returns>
        private IMotionModule GetParentModule()
        {
            var pModule = this.FirstOrDefault(u => u.IsCurrent);
            if (pModule == null)
            {
                // 设置为默认Current
                _rootModule.IsCurrent = true;
                return _rootModule;
            }
            else
            {
                return pModule;
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
            // 释放已有对象
            if (Count > 0)
            {
                foreach (var item in this)
                {
                    item.Dispose();
                }
            }

            // 清理所有对象
            Clear();

            // 清除模块ID缓存
            _moduleIdCache.Clear();

            // 创建根节点基元
            _rootModule = new Luster.TaskFlow.Motion.Logic.Root();
            _rootModule.SetFunction("Group");
            _rootModule.TaskModules = this;
            _rootModule.IsCurrent = true;

            _globalModule = new Luster.TaskFlow.Motion.Logic.GlobalModule();
            _globalModule.SetFunction("Global");
            _globalModule.TaskModules = this;


            // 添加到节点中
            Add(_rootModule);
            Add(_globalModule);

            // 添加到模块ID缓存
            _moduleIdCache[_rootModule.ID] = _rootModule;
            _moduleIdCache[_globalModule.ID] = _globalModule;

            // 1.给模块注册事件
            _globalModule.LogEvent -= OnLog;
            _globalModule.LogEvent += OnLog;

            // 设置当前执行器所在的线程ID
            ExcutorNo = $"{System.Threading.Thread.CurrentThread.ManagedThreadId}";
            LogEvent?.Invoke(LogType.Info, $"新实例:{ExcutorNo} 被创建!", ExcutorNo);
        }

        /// <summary>
        /// 获取状态在Running的模块
        /// </summary>
        /// <returns>当前运行的模块</returns>
        public IMotionModule GetBusyModule()
        {
            return motionRunEngine.GetRunning();
        }


        /// <summary>
        /// 导出
        /// </summary>
        /// <returns></returns>
        public XElement ExportXml()
        {
            // 1.创建根节点
            XElement xRoot = new XElement("Recipe");

            // 2.导出全局变量
            if (_globalModule != null)
            {
                xRoot.Add(_globalModule.ExportXml());
            }

            // 3.创建基元集合
            var xPrims = new XElement("Modules");
            _rootModule.ExportXml();
            foreach (IModule item in _rootModule.Children.OrderBy(u => u.Sort))
            {
                var xPrim = item.ExportXml();
                if (xPrim != null)
                {
                    xPrims.Add(xPrim);
                }
            }

            xRoot.Add(xPrims);

            // 4.导出数据映射对象
            if (MapDatas.Count > 0)
            {
                XElement xMaps = new XElement(nameof(MapDatas));
                foreach (var item in MapDatas)
                {
                    xMaps.Add(item.ExportXml());
                }

                xRoot.Add(xMaps);
            }

            // 5.导出组合模块
            if (_composition.Modules.Count > 0)
            {
                xRoot.Add(_composition.ExportXml());
            }

            // 6.工作流
            if (WorkFlow != null)
            {
                xRoot.Add(WorkFlow.ExportXml());
            }

            return xRoot;
        }

        /// <summary>
        /// 导出参数配置
        /// </summary>
        /// <returns></returns>
        public XElement ExportConfig()
        {
            XElement xPConfig = new XElement("Config");
            foreach (var item in this)
            {
                if (item.TaskFunction is IMotionFunction m)
                {
                    var funcXml = m.ExportXml();
                    funcXml.SetAttributeValue("ID", item.ID);
                    funcXml.SetAttributeValue("Alias", item.Alias);
                    //添加模组名
                    if (dicGuidToModule.ContainsKey(item.ID))
                    {
                        funcXml.SetAttributeValue("Module", dicGuidToModule[item.ID]);

                        //为子元素添加模块名，方便界面显示
                        foreach (var element in funcXml.Elements())
                        {
                            element.SetAttributeValue("Module", dicGuidToModule[item.ID]);
                        }
                    }
                    xPConfig.Add(funcXml);
                }
            }

            return xPConfig;
        }

        /// <summary>
        /// 解析配置文件
        /// </summary>
        /// <param name="xElement">元素配置</param>
        public void ParseConfig(XElement xElement)
        {
            foreach (var xFunc in xElement.Elements())
            {
                xFunc.GetAttribute("ID", (id) =>
                {
                    var mID = Guid.Parse(id);
                    var m = Get(mID);
                    if (m != null)
                    {
                        m.TaskFunction.ParserXml(xFunc);
                    }
                });
            }
        }

        /// <summary>
        /// 加载工程文件
        /// </summary>
        /// <param name="projfile">工程文件路径，后缀是*.3dproj</param>
        public void LoadProjfile(string projfile)
        {
            if (!File.Exists(projfile))
            {
                throw new FriendlyException($"文件:{projfile} 不存在!");
            }

            // 1.解析所有模块
            var xProj = XElement.Load(projfile);
            LoadProjfile(xProj);
        }

        /// <summary>
        /// 加载项目文件
        /// </summary>
        /// <param name="xProj"></param>
        public void LoadProjfile(XElement xProj)
        {
            try
            {
                // 1.解析所有模块
                ParserXml(xProj);//1300ms
                // 2.缓存支持暂停的模块
                Stopwatch sw = Stopwatch.StartNew();
                pauseModules.Clear();
                foreach (var item in this)
                {
                    // 1.暂停模块添加到集合
                    if (item.TaskFunction is IPauseFunction)
                    {
                        pauseModules.Add(item);
                    }
                    // 2.解析跨模块引用丢失的BUG
                    foreach (var pItem in item.Parameters)
                    {
                        var pAttr = pItem.Value;
                        if (pAttr.ParamType == ParamType.OUT || pAttr.CanRef != ParamRef.Ref) continue;

                        // 加载引用
                        pAttr.LoadFromRefXml();
                    }
                    // 更新引用关系
                    item.UpdateReferences();
                }
                sw.Stop();
                Debug.WriteLine($"SWWW{sw.ElapsedMilliseconds.ToString()}");
                // 任务加载后，状态未空闲状态
                EngineStatus = EngineStatus.Idle;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 导入
        /// </summary>
        /// <param name="xElement"></param>
        public void ParserXml(XElement xElement)
        {
            // 1.对象初始化
            Initialize();

            // 2.解析参数信息
            XElement xGlobal = xElement.Element("Global");
            if (xGlobal != null)
            {
                _globalModule.ParserXml(xGlobal);
            }

            // 3.组合模块，由于任务会有用到组合模块，所以需要放到前面
            var xComposition = xElement.Element(nameof(Composition));
            if (xComposition != null)
            {
                _composition.ParserXml(xComposition);
            }

            // 4.任务流程解析
            var xModules = xElement.Element("Modules");
            if (xModules != null)
            {
                foreach (XElement xModule in xModules.Elements("Module"))
                {
                    Parse(xModule, _rootModule);

                }

            }


            // 5.解析数据映射对象
            MapDatas.Clear();
            XElement xMaps = xElement.Element(nameof(MapDatas));
            if (xMaps != null)
            {
                foreach (var xMap in xMaps.Elements())
                {
                    MapData mData = new MapData();
                    mData.ParserXml(xMap);
                    var module = Get(mData.ID);
                    if (module != null)
                    {
                        module.IsOutData = true;
                        if (module.Parameters.ContainsKey(mData.Key))
                        {
                            mData.Parameter = module.Parameters[mData.Key];
                            mData.Parameter.IsOutData = true;
                            mData.Parameter.Alias = mData.Alias;
                        }
                    }

                    MapDatas.Add(mData);
                }
            }

            // 6.解析组合对象
            var xWorkFlow = xElement.Element(nameof(WorkFlow));
            if (xWorkFlow != null)
            {
                WorkFlow.ParserXml(xWorkFlow);
            }
        }

        /// <summary>
        /// 解析基元
        /// </summary>
        /// <param name="xModule">xModule</param>
        /// <param name="parent">parent</param>
        public IMotionModule Parse(XElement xModule, IMotionModule parent, int index = -1)
        {
            // 1.获取基元类别
            var mName = xModule.Attribute("Name").Value;

            // 2.获取基元创建器
            IModuleCreator taskCreator = Factory.GetCreator(mName, "HoloMotion");

            if (taskCreator == null)
            {
                throw new KeyNotFoundException($"Module:{mName} is not exist!");
            }

            // 3.创建基元
            var taskModule = taskCreator.Create() as IMotionModule;
            taskModule.TaskModules = this;
            taskModule.DeviceEngine = DeviceEngine;
            taskModule.BrokenOff = pauseResetEvent;
            taskModule.CacheManager = CacheManager;

            RegisterModuleEvent(taskModule);

            // 4.解析XML
            taskModule.ParserXml(xModule);

            var mID = taskModule.ID;
            var exist = Get(taskModule.ID);
            if (exist != null)
            {
                throw new FriendlyException($"task engine:{taskModule.Alias} is exist!");
            }

            Insert(taskModule, index, parent);

            var xChildren = xModule.Element("Children");
            if (xChildren != null)
            {
                IMotionModule prevModule = null;
                if (xChildren != null)
                {
                    var xChilds = xChildren.Elements("Module");
                    foreach (var xChild in xChilds)
                    {
                        var mItem = Parse(xChild, taskModule);

                        // Switch 内部因为包裹了一层Group，所以group之间不能有依赖关系
                        if (taskModule.TaskFunction is not ISwitch && taskModule.TaskFunction is not IBranch && taskModule.TaskFunction is not IParallel)
                        {
                            if (prevModule == null)
                            {
                                prevModule = mItem;
                            }
                            else
                            {
                                mItem.PrevModule = prevModule;
                                prevModule.NextModule = mItem;

                                // 更新上一步
                                prevModule = mItem;
                            }
                        }
                    }
                }
            }

            return taskModule;
        }


        /// <summary>
        /// 对象Clone
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            // 导出XML
            var exportXml = this.ExportXml();

            var newObj = new MotionEngine(Factory, DeviceEngine, CacheManager, OrderManager, ConfigManager, LightManager, DbHelper, IocManager, RollManager, ErrorManager);
            newObj.LoadProjfile(exportXml);

            return newObj;
        }

        /// <summary>
        /// 模块获取
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IMotionModule Get(Guid id)
        {
            // 使用缓存实现O(1)查找，回退到线性搜索以确保兼容性
            if (_moduleIdCache.TryGetValue(id, out var cachedModule))
            {
                return cachedModule;
            }
            return this.FirstOrDefault(u => u.ID == id);
        }

        /// <summary>
        /// 获取所有工站
        /// </summary>
        /// <returns></returns>
        public List<IMotionModule> GetStations()
        {
            var stations = new List<IMotionModule>();
            var snapshot = this.ToList(); // 快照
            foreach (var item in snapshot)
            {
                // 屏蔽状态不进行运行
                if (item.Status == RunStatus.Skip) continue;
                if (item.TaskFunction is IStation station)
                {
                    stations.Add(item);
                    // 更新模块的Icon图标
                    item.Icon = item.TaskFunction.Icon;
                }
            }
            return stations;
        }
        #endregion

        #region IModuleCollection
        /// <summary>
        /// 是否存在当前ID
        /// </summary>
        /// <param name="mId">模块ID</param>
        /// <returns>是否包含</returns>
        public bool Contains(Guid mId)
        {
            // 使用缓存实现O(1)查找，回退到线性搜索以确保兼容性
            if (_moduleIdCache.ContainsKey(mId))
            {
                return true;
            }
            return this.Any(u => u.ID == mId);
        }

        /// <summary>
        /// 索引器
        /// </summary>
        /// <param name="mId">模块ID</param>
        /// <returns>模块</returns>
        public IModule this[Guid mId] => Get(mId);

        /// <summary>
        /// 索引器
        /// </summary>
        /// <param name="mId">模块ID</param>
        /// <returns>模块</returns>
        public IModule this[string mId] => Get(Guid.Parse(mId));
        #endregion

        #region 模块提取到组合步骤中
        /// <summary>
        /// 添加或更新
        /// </summary>
        public void AddComposition(IMotionModule groupModule)
        {
            if (_composition.Modules.Any(u => u.Alias == groupModule.Alias))
            {
                throw new FriendlyException($"组合模组:{groupModule.Alias}已重复!");
            }

            _composition.Modules.Add(groupModule);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="guid"></param>
        public void RemoveComposition(Guid guid)
        {
            if (_composition.Modules.Any(u => u.ID == guid))
            {
                _composition.Modules.RemoveAll(u => u.ID == guid);
            }
        }

        public List<IMotionModule> GetCompositions()
        {
            return _composition.Modules;
        }
        #endregion


        /// <summary>
        /// 等待运行
        /// </summary>
        /// <param name="waitMessage">等待的消息</param>
        public void WaitRunning(string waitMessage)
        {
            if (pauseResetEvent != null && !pauseResetEvent.IsSet)
            {
                OnLog(LogType.Warning, $"等待恢复:{waitMessage}");
                pauseResetEvent.Wait();
                OnLog(LogType.Warning, $"完成恢复:{waitMessage}");
            }
        }


        /// <summary>
        /// 生成基元对应的模组名称
        /// </summary>
        public void BuildPrimModule()
        {
            dicGuidToModule.Clear();
            var Stations = GetStations();
            foreach (var xItem in Stations)
            {
                foreach (var xPrim in xItem.Children)
                {
                    if (xItem.Parameters.ContainsKey("Module"))
                        dicGuidToModule.Add(xPrim.ID, xItem.Parameters["Module"].Value.ToString());
                    //给工站生成一个对应的模块名
                    if (!dicGuidToModule.ContainsKey(xItem.ID))
                        if (xItem.Parameters.ContainsKey("Module"))
                            dicGuidToModule.Add(xItem.ID, xItem.Parameters["Module"].Value.ToString());

                }
            }
        }
    }
}