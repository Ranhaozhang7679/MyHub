using Luster.Common.Assets;
using Luster.Common.DataAccess.Factory;
using Luster.Common.DataAccess.Repositories;
using Luster.Common.DataAccess.Tables;
using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.Tools;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.EditorUI.Extensions;
using Luster.Motion.CommonUI.Interfaces;
using Luster.Motion.CommonUI.Models;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.Integration.AOI;
using Luster.Motion.Integration.SFC;
using Luster.Motion.SubSystem.Models;
using Luster.Motion.TaskFlow.Engine;
using Luster.Motion.TaskFlow.Engine.Engine;
using Luster.Motion.TaskFlow.Engine.HyperTrain;
using Luster.Motion.TaskFlow.Engine.Models;
using Luster.TaskFlow.Common;
using Luster.TaskFlow.Common.Logics;
//using Luster.TaskFlow.Engine;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Enums;
using Luster.TaskFlow.Motion.Interfaces;
using Luster.TaskFlow.Motion.Logic;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.FileIO;
using Microsoft.VisualBasic.Logging;
using Prism.Events;
using Prism.Modularity;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using Luster.Motion.Integration.Web;
using Luster.Motion.Integration.WorkCardVerify;
using Luster.Motion.DataStruct.Real;

namespace Luster.Motion.CommonUI
{
    /// <summary>
    /// 
    /// </summary>
    public class CommonBus : ICommonBus
    {
        private VisionAPI _visionAPI;
        /// <summary>
        /// 运控引擎
        /// </summary>
        private IMotionEngine _motionEngine;

        /// <summary>
        /// 控制器
        /// </summary>
        private IMotionController _mController;

        /// <summary>
        /// 错误管理器
        /// </summary>
        private IErrorManager _errorManager;

        /// <summary>
        /// 配方
        /// </summary>
        public event Action<XElement> LoadRecipeEvent;

        /// <summary>
        /// 是否保存
        /// </summary>
        private bool isNeedSave = false;
        public bool IsNeedSave
        {
            get { return isNeedSave; }
            set
            {
                isNeedSave = value;
                if (value && !_isLoading)
                {
                    _editCount++;
                    CheckAutoVersionReminder();
                }
            }
        }

        /// <summary>
        /// 编辑计数器（用于自动弹出版本管理对话框）
        /// </summary>
        private int _editCount = 0;
        public int EditCount => _editCount;
        private bool _isVersionReminderShowing = false;
        private bool _isLoading = false;

        /// <summary>
        /// 当前的配方
        /// </summary>
        public Recipe CurrentRecipe { get; set; }

        /// <summary>
        /// 当前用户
        /// </summary>
        public UserModel CurrentUser { get; set; }

        /// <summary>
        /// 事件总线
        /// </summary>
        public IEventAggregator EventBus { get; set; }

        /// <summary>
        /// 用户配置
        /// </summary>
        public UserConfig UserConfig { get; set; }

        /// <summary>
        /// 条码配置
        /// </summary>
        public BarcodeConfig BarConfig { get; set; }

        /// <summary>
        /// 工程信息
        /// </summary>
        public ProjectInfo ProjInfo { get; set; }

        /// <summary>
        /// 设备硬件
        /// </summary>
        private IDeviceEngine _deviceEngine;

        /// <summary>
        /// 弹窗
        /// </summary>
        private IDialogService _dialogService;

        /// <summary>
        /// DB工程
        /// </summary>
        private IDBFactory _dbFactory;

        /// <summary>
        /// 3D的UI引擎
        /// </summary>
        //private ITaskEngineUI threeEngineUI;

        /// <summary>
        /// 数据仓储
        /// </summary>
        private IRepository _repository;

        /// <summary>
        /// Web服务
        /// </summary>
        private readonly IWebService _webService;

        /// <summary>
        /// 配置
        /// </summary>
        private readonly IConfigManager _configManager;

        /// <summary>
        /// 数据库管理
        /// </summary>
        private readonly IDbManager _dbManager;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="bus"></param>
        public CommonBus(IEventAggregator bus,
            IMotionEngine mEngine,
            IErrorManager errorManager,
            IDeviceEngine deviceEngine,
            IMotionController mController,
            IDialogService dialogService,
            IDBFactory dbFactory,
            IWebService webService,
            IConfigManager configManager,
            IDbManager dbManager,
            //ITaskEngineUI taskEngineUI,
            VisionAPI visionAPI,
            IRepository repository)
        {
            EventBus = bus;
            _motionEngine = mEngine;
            _deviceEngine = deviceEngine;
            _dialogService = dialogService;
            _dbFactory = dbFactory;
            _repository = repository;
            _errorManager = errorManager;
            _webService = webService;
            _configManager = configManager;
            _dbManager = dbManager;
            _visionAPI = visionAPI;

            _motionEngine.PropertyChanged -= MotionEngine_PropertyChanged;
            _motionEngine.PropertyChanged += MotionEngine_PropertyChanged;

            // 报警注册
            mEngine.AlarmEvent_MEngine -= MEngine_AlarmEvent;
            mEngine.AlarmEvent_MEngine += MEngine_AlarmEvent;
            _deviceEngine.AlarmEvent_device -= MEngine_AlarmEvent;
            _deviceEngine.AlarmEvent_device += MEngine_AlarmEvent;

            _deviceEngine.LoadingEvent -= LoadingEvent;
            _deviceEngine.LoadingEvent += LoadingEvent;

            // 属性发送变更
            _deviceEngine.PropertyChangedEvent -= DeviceEngine_PropertyChangedEvent;
            _deviceEngine.PropertyChangedEvent += DeviceEngine_PropertyChangedEvent;

            mEngine.EngineStatusEvent -= MEngine_EngineStatusEvent;
            mEngine.EngineStatusEvent += MEngine_EngineStatusEvent;

            mEngine.LoadingEvent -= LoadingEvent;
            mEngine.LoadingEvent += LoadingEvent;
            mEngine.MapDataChangeEvent -= MEngine_MapDataChangeEvent;
            mEngine.MapDataChangeEvent += MEngine_MapDataChangeEvent;

            mController.ChangeRecipeEvent -= MController_ChangeRecipeEvent;
            mController.ChangeRecipeEvent += MController_ChangeRecipeEvent;

            mController.RoleChangedEvent -= MController_RoleChangedEvent;
            mController.RoleChangedEvent += MController_RoleChangedEvent;

            // 报警处理
            mController.AlarmProcEvent -= MController_AlarmProcEvent;
            mController.AlarmProcEvent += MController_AlarmProcEvent;

            mController.AlarmInfosUpEvent -= MController_AlarmInfoUpEvent;
            mController.AlarmInfosUpEvent += MController_AlarmInfoUpEvent;

            mController.AlarmInfosGetEvent -= MController_AlarmInfoGetEvent;
            mController.AlarmInfosGetEvent += MController_AlarmInfoGetEvent;

            mController.ModeChangedEvent -= MController_ModeChangedEvent;
            mController.ModeChangedEvent += MController_ModeChangedEvent;

            // Log事件
            mEngine.LogEvent -= OnLog;
            mEngine.LogEvent += OnLog;

            // aoi 日志事件
            AOIHelper.Instance.LogEvent -= Instance_LogEvent;
            AOIHelper.Instance.LogEvent += Instance_LogEvent;
            AOIHelper.Instance.AlarmEvent -= Instance_AlarmEvent;
            AOIHelper.Instance.AlarmEvent += Instance_AlarmEvent;

            UserConfig = new UserConfig();
            ProjInfo = new ProjectInfo();
            BarConfig = new BarcodeConfig();
            _mController = mController;

            // 3D 平台的日志
            //threeEngineUI = taskEngineUI;
            //threeEngineUI.GetTaskEngine().LogEvent -= OnLog;
            //threeEngineUI.GetTaskEngine().LogEvent += OnLog;


            //太科电批事件
            _motionEngine.TaiKeScrewRegisterEvent -= _motionEngine_TaiKeScrewRegisterEvent;
            _motionEngine.TaiKeScrewRegisterEvent += _motionEngine_TaiKeScrewRegisterEvent;


            //压力事件
            _motionEngine.PressRegisterEvent -= _motionEngine_PressRegisterEvent;
            _motionEngine.PressRegisterEvent += _motionEngine_PressRegisterEvent;


            //Toein事件
            _motionEngine.ToeinForceRegisterEvent -= _motionEngine_ToeinForceRegisterEvent;
            _motionEngine.ToeinForceRegisterEvent += _motionEngine_ToeinForceRegisterEvent;

        }

        private void _motionEngine_TaiKeScrewRegisterEvent(object obj, string name)
        {
            EventBus.GetEvent<TorqueRegisterEvent>().Publish(new TurqueModel() { Driver = obj, Name = name });
        }

        //六轴XYZ压力
        private void _motionEngine_PressRegisterEvent(object obj, string name)
        {
            EventBus.GetEvent<PressRegisterEvent>().Publish(new PressModel() { Driver = obj, Name = name });
        }

        private void _motionEngine_ToeinForceRegisterEvent(object obj, string name)
        {
            EventBus.GetEvent<ToeinRegisterEvent>().Publish(new ToeinModel() { Driver = obj, Name = name });
        }



        #region 文件自动删除
        /// <summary>
        /// 启动历史文件自动删除功能
        /// </summary>
        public void StartHistoryFileDelete()
        {
            if (CurrentRecipe == null)
            {
                return;
            }

            // 异步清理数据
            Task.Run(() =>
            {
                try
                {
                    // 1.数据库文件
                    // 2.配方文件
                    // 3.Hardware 文件
                    // 4.日志文件
                    var fileConfig = _mController.FileConfig;
                    var day = fileConfig.LogBackUpDays == 0 ? 7 : fileConfig.LogBackUpDays;

                    // 配方文件、hardware 文件
                    var backDir = Path.Combine(CurrentRecipe.ProjInfo.ProjPath, ".luster");
                    DeleteHisFiles(backDir, fileConfig.RecipeBackUpDays);

                    // log 文件
                    string logDir = Path.Combine(CurrentRecipe.GetRecipePath(), "logs");
                    DeleteHisFiles(logDir, day);

                    // db 文件
                    string dbDir = fileConfig.DumpDBPath;
                    DeleteHisFiles(dbDir, fileConfig.DBBackUpDays);
                }
                catch (Exception ex)
                {
                    OnLog(LogType.Error, $"备份数据清除异常:{ex.Message}");
                }
            });
        }

        /// <summary>
        /// 通过时间自动删除文件
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="days"></param>
        private void DeleteHisFiles(string dir, int days)
        {
            if (Directory.Exists(dir) && days > 0)
            {
                foreach (var item in Directory.GetDirectories(dir))
                {
                    DeleteHisFiles(item, days);
                }

                foreach (var item in Directory.GetFiles(dir))
                {
                    var fileInfo = new FileInfo(item);
                    if ((DateTime.Now - fileInfo.CreationTime).TotalDays > days)
                    {
                        fileInfo.Delete();
                    }
                }
            }

        }

        #endregion
        /// <summary>
        /// 机台模式发生变更
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private void MController_ModeChangedEvent(string arg1, string arg2)
        {
            EventBus.GetEvent<MachineModeEvent>().Publish(new MachineMode()
            {
                OldMode = arg1,
                NewMode = arg2
            });
        }

        /// <summary>
        /// 实例报警
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_AlarmEvent(string obj)
        {
            MEngine_AlarmEvent(new AlarmInfo(AOIHelper.Instance, AlarmType.InfoTip, obj, "100"));
        }

        /// <summary>
        /// log 
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private void Instance_LogEvent(LogType logType, string message, Exception ex)
        {
            switch (logType)
            {
                case LogType.Debug:
                    LogTool.Debug(message, "AOI");
                    break;
                case LogType.Info:
                    LogTool.Info(message, "AOI");
                    break;
                case LogType.Warning:
                    LogTool.Warn(message, "AOI");
                    break;
                case LogType.Error:
                    LogTool.Error(message, ex, "AOI");
                    break;
            }
        }

        /// <summary>
        /// 模块属性变更
        /// </summary>
        /// <param name="module"></param>
        /// <param name="propName"></param>
        /// <param name="srcV"></param>
        /// <param name="newV"></param>
        private void MotionEngine_PropertyChanged(IMotionModule module, string propName, object srcV, object newV)
        {
            if (srcV != newV)
            {
                OnChangeRecord(OperationType.Update, module.Alias, propName, $"oldValue:{srcV}->newValue:{newV}");
                IsNeedSave = true;
            }
        }

        /// <summary>
        /// 硬件参数属性变更
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        private void DeviceEngine_PropertyChangedEvent(string propName, object srcV, object newV)
        {
            if (srcV != newV)
            {
                OnChangeRecord(OperationType.Update, $"Hardware", propName, $"oldValue:{srcV}->newValue:{newV}");
                IsNeedSave = true;
            }
        }

        private void MController_RoleChangedEvent(SystemRole oldRole, SystemRole newRole)
        {
            //if (oldRole == newRole) return;
            EventBus.GetEvent<RoleChangeEvent>().Publish(newRole);
        }

        /// <summary>
        /// 状态发生变化
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private bool MEngine_EngineStatusEvent(EngineStatus src, EngineStatus dst)
        {
            EventBus.GetEvent<OperationEvent>().Publish(new StatusChanged(src, dst));


            return true;
        }

        private void LoadingEvent(int total, int curVal, string message)
        {
            EventBus.GetEvent<LoadingEvent>().Publish(new LoadingModel(total, curVal, message));
        }

        /// <summary>
        /// 删除流程，基于该流程的输出项删除
        /// </summary>
        private void MEngine_MapDataChangeEvent()
        {
            EventBus.GetEvent<MapDataEvent>().Publish(GetMapDatas().FirstOrDefault());
            IsNeedSave = true;
        }

        /// <summary>
        /// 检查编辑次数是否达到阈值，自动弹出版本管理对话框
        /// </summary>
        private void CheckAutoVersionReminder()
        {
            if (_isVersionReminderShowing) return;
            if (string.IsNullOrEmpty(ProjInfo?.FullName)) return;

            // 设备正在运行时不弹出自动提醒，避免阻塞操作
            var status = GetStatus();
            if (status == EngineStatus.Running || status == EngineStatus.Pause) return;

            string enabled = ConfigHelper.GetAppKey("AutoRecipeVersionReminder");
            if (string.IsNullOrEmpty(enabled) || enabled != "True") return;

            string thresholdStr = ConfigHelper.GetAppKey("RecipeVersionReminderThreshold");
            int threshold = 100;
            if (!string.IsNullOrEmpty(thresholdStr))
            {
                int.TryParse(thresholdStr, out threshold);
                if (threshold <= 0) threshold = 100;
            }

            if (_editCount < threshold) return;
            _isVersionReminderShowing = true;
            Application.Current.Dispatcher.Invoke(() =>
            {
                _dialogService.ShowRecipeVersionDialog((result) =>
                {
                    if (result.Result == ButtonResult.OK)
                    {
                        string versionNo = result.Parameters.GetValue<string>("VersionNo");
                        string versionType = result.Parameters.GetValue<string>("VersionType");
                        var changeContents = result.Parameters.GetValue<List<string>>("ChangeContents");
                        string modifiedBy = result.Parameters.GetValue<string>("ModifiedBy");
                        string modifiedTime = result.Parameters.GetValue<string>("ModifiedTime");

                        if (changeContents != null && changeContents.Count > 0)
                        {
                            string fileName = versionType == "PLC" ? "PLCVersion.json" : "Version.json";
                            RecipeVersionHelper.UpdateRecipeVersion(versionNo, changeContents, fileName, ProjInfo, modifiedBy, modifiedTime);
                        }
                    }
                    _editCount = 0;
                    _isVersionReminderShowing = false;
                });
            });
        }

        /// <summary>
        /// 获取状态
        /// </summary>
        /// <returns></returns>
        public EngineStatus GetStatus()
        {
            return _motionEngine.EngineStatus;
        }

        #region 报警处理
        /// <summary>
        /// 警告弹窗，添加报警记录
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <param name="arg4"></param>
        private void MEngine_AlarmEvent(AlarmInfo alarmInfo)
        {
            EventBus.GetEvent<AlarmEvent>().Publish(alarmInfo);

            // 报警记录消息
            OnLog(LogType.Warning, $"AlarmType:{alarmInfo.AlarmType},Message:{alarmInfo.Message}");

            if (alarmInfo.AlarmType == AlarmType.InfoTip || alarmInfo.AlarmID > 0) return;

            // 设备报警要添加报警信息到数据库中
            var tbAlarm = new TbAlarm()
            {
                AlarmType = alarmInfo.AlarmType.GetDescription(),
                AlarmCode = alarmInfo.AlarmCode,
                Module = "",
                ProcUser = CurrentUser?.UserName,
                StartTime = alarmInfo.StartTime,
                Reason = alarmInfo.Message,
                CreateTime = DateTime.Now,
            };

            // 配置模块
            if (alarmInfo.Sender is IMotionModule m)
            {
                tbAlarm.Module = m.Alias;
            }
            else if (alarmInfo.Sender is IDevice device)
            {
                tbAlarm.Module = device.Name;
            }
            else
            {
                tbAlarm.Module = "system";
            }

                // 更新报警的ID
                alarmInfo.AlarmID = _repository.Insert(tbAlarm);
        }

        /// <summary>
        /// 报警处理:结束时间
        /// </summary>
        /// <param name="alarmInfo"></param>
        private void MController_AlarmProcEvent(AlarmInfo alarmInfo, bool isOk = true)
        {
            if (alarmInfo != null && alarmInfo.AlarmID > 0)
            {
                alarmInfo.ProcMethod = isOk ? "重试成功" : "重试失败";
                alarmInfo.EndTime = DateTime.Now;
                alarmInfo.Duration = (alarmInfo.EndTime - alarmInfo.StartTime).Seconds;
                var item = new TbAlarm
                {
                    ID = alarmInfo.AlarmID,
                    ProcMethod = alarmInfo.ProcMethod,
                    EndTime = alarmInfo.EndTime,
                    AlarmLongTime = (int)alarmInfo.Duration
                };
                int count = _repository.Update<TbAlarm>(item, new string[] { "ProcMethod", "EndTime", "AlarmLongTime" });
            }
        }

        private void MController_AlarmInfoUpEvent(List<AlarmInfo> alarmInfos, bool up_dele)
        {
            try
            {
                // up_dele=true: 清空 TbAlarminfosTable
                if (up_dele)
                {
                    // 直接清空表（FreeSql delete all）
                    _dbFactory.GetDbConnection()
                        .Delete<TbAlarminfos>()
                        .Where("1=1")
                        .ExecuteAffrows();

                    return;
                }

                // up_dele=false: 覆盖更新 TbAlarminfosTable
                if (alarmInfos == null || alarmInfos.Count == 0)
                    return;

                var tbList = new List<TbAlarminfos>(alarmInfos.Count);
                foreach (var a in alarmInfos)
                {
                    string moduleName = "system";
                    if (a.Sender is IMotionModule mm) moduleName = mm.Alias;
                    else if (a.Sender is IDevice dev) moduleName = dev.Name;

                    tbList.Add(new TbAlarminfos
                    {
                        // BaseTable
                        ID = a.AlarmID,
                        CreateTime = DateTime.Now,

                        Module = moduleName,
                        AlarmCode = a.AlarmCode,
                        AlarmType = a.AlarmType.ToString(),
                        Reason = a.Message,
                        ProcMethod = a.ProcMethod,
                        Duration = a.Duration,
                        ProcUser = CurrentUser?.UserName,
                        StartTime = a.StartTime,
                        EndTime = a.EndTime
                    });
                }

                // 全量覆盖：先清空再批量插入
                _dbFactory.GetDbConnection()
                    .Delete<TbAlarminfos>()
                    .Where("1=1")
                    .ExecuteAffrows();

                _repository.BatchInsert(tbList);
            }
            catch (Exception ex)
            {
                OnLog(LogType.Error, $"更新 TbAlarminfosTable 异常:{ex.Message}");
            }
        }

        /// <summary>
        /// 从数据库读取保存的 TbAlarminfos 并赋值到 alarmInfos 列表
        /// </summary>
        /// <param name="alarmInfos">调用方传入的列表，方法会清空并填充最新数据</param>
        private void MController_AlarmInfoGetEvent(List<AlarmInfo> alarmInfos)
        {
            try
            {
                if (alarmInfos == null) return;

                var db = _dbFactory.GetDbConnection();
                if (db == null)
                {
                    OnLog(LogType.Warning, "数据库连接为空，无法读取报警信息");
                    return;
                }

                List<TbAlarminfos> tbList = null;
                try
                {
                    tbList = db.Select<TbAlarminfos>()?.ToList();
                }
                catch (Exception ex)
                {
                    OnLog(LogType.Error, $"读取 TbAlarminfos 异常(Select)：{ex.Message}");
                    return;
                }

                alarmInfos.Clear();
                if (tbList == null || tbList.Count == 0) return;

                foreach (var tb in tbList)
                {
                    try
                    {
                        // 解析 AlarmType，使用泛型 Enum.TryParse 避免重载不匹配
                        AlarmType atype = AlarmType.InfoTip;
                        if (!string.IsNullOrEmpty(tb.AlarmType))
                        {
                            Enum.TryParse<AlarmType>(tb.AlarmType, true, out atype);
                        }

                        // AlarmInfo 构造函数需要参数：sender, AlarmType, message, alarmCode, module, name
                        var senderObj = (object)tb.Module;
                        var alarm = new AlarmInfo(senderObj, atype, tb.Reason ?? "", tb.AlarmCode ?? "", tb.Module ?? "", tb.Module ?? "");

                        // 尝试赋值可用的字段
                        try { alarm.AlarmID = tb.ID; } catch { }
                        try { alarm.StartTime = tb.StartTime; } catch { }
                        try { alarm.Module = tb.Module; } catch { }
                        // 不再访问不存在的 ProcUser 字段

                        alarmInfos.Add(alarm);
                    }
                    catch (Exception exInner)
                    {
                        OnLog(LogType.Warning, $"映射 TbAlarminfos(ID={tb?.ID}) 到 AlarmInfo 时发生异常：{exInner.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                OnLog(LogType.Error, $"获取报警信息异常:{ex.Message}");
            }
        }

        #endregion

        /// <summary>
        /// 当前页面
        /// </summary>
        public PageModel CurrentPage { get; set; }

        /// <summary>
        /// 导航
        /// </summary>
        /// <param name="regionName"></param>
        public void OnNavigate(PageModel pageModel)
        {
            if (CurrentPage != null && CurrentPage.Region == pageModel.Region) return;

            EventBus.GetEvent<PageNavEvent>().Publish(pageModel);
            CurrentPage = pageModel;
        }

        public void OnSaveSystem(string sysConfig = "")
        {
            if (ProjInfo != null & ProjInfo.FullName != null)
            {
                string projDri = Path.Combine(Path.GetDirectoryName(ProjInfo.FullName), "Config");
                sysConfig = Path.Combine(projDri, "SystemConfig.xml");
                string fileconfig = Path.Combine(projDri, "FileConfig.xml");

                // 保存配置信息
                if (_deviceEngine.DeviceMode == DeviceMode.Empty)
                {
                    _mController.SysConfig.RunMode = DeviceMode.Real;
                }
                else
                {
                    _mController.SysConfig.RunMode = _deviceEngine.DeviceMode;
                }
                _mController.SysConfig.SaveSysConfig(sysConfig);
                _mController.FileConfig.SaveFileConfig(fileconfig);

                if (_webService != null)
                {
                    string webConfig = Path.Combine(projDri, "WebConfig.xml");
                    _webService.SaveConfig(webConfig);
                }

                EventBus.GetEvent<SystemConfigChangeEvent>().Publish();
            }
        }

        /// <summary>
        /// 保存错误
        /// </summary>
        public void OnSaveError(string sysConfig = "")
        {
            if (ProjInfo != null & ProjInfo.FullName != null)
            {
                // 错误导出
                string projDri = Path.Combine(Path.GetDirectoryName(ProjInfo.FullName), "Config");
                string errPath = Path.Combine(projDri, "Errors.xml");

                // 发起保存前，强制重新从设备引擎读取当前最新的设备名，并同步覆盖掉内存旧数据
                _errorManager.LoadErrorConfig(errPath, _deviceEngine);

                var xErr = _errorManager.ExportXml();
                xErr.Save(errPath);
            }
        }


        public void OnLoadSystem(string sysConfig)
        {
            _mController.SysConfig = new TaskFlow.Engine.Models.SystemConfig();
            _mController.SysConfig.LoadSysConfig(sysConfig);
        }

        public void OnLoadFileConfig(string fileConfig)
        {
            if (!File.Exists(fileConfig))
            {
                //FileConfig不存在，实例化防止把其它配置带入新的配方
                _mController.FileConfig = new FileConfig();
            }
            _mController.FileConfig.LoadFileConfig(fileConfig);
        }

        #region Chart报表配置保存加载
        public void SaveChartConfig(string chartconfig, List<ChartDataModel> chartList)
        {
            if (!Directory.Exists(Path.GetDirectoryName(chartconfig)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(chartconfig));
            }
            if (chartList != null)
            {
                XElement xRoot = new XElement("ChartConfig");
                foreach (var item in chartList)
                {
                    xRoot.Add(item.ExportXml());
                }
                xRoot.Save(chartconfig);
            }
        }

        public List<ChartDataModel> LoadChartList()
        {
            List<ChartDataModel> chartDataModels = new List<ChartDataModel>();
            if (ProjInfo != null && ProjInfo.ProjPath != "")
            {
                string chartConfig = Path.Combine(Path.GetDirectoryName(ProjInfo.FullName), "Config", "ChartConfig.xml");
                if (File.Exists(chartConfig))
                {
                    XElement xRoots = XElement.Load(chartConfig);
                    foreach (var xItems in xRoots.Elements("ChartConfig"))
                    {
                        ChartDataModel data = new ChartDataModel();
                        data.ParserXml(xItems);
                        chartDataModels.Add(data);
                    }
                }
            }
            return chartDataModels;
        }
        #endregion

        #region 用户管理
        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="model"></param>
        public void OnUserLogin(UserModel model)
        {
            EventBus.GetEvent<UserLoginEvent>().Publish(model);
            _mController.OnUserLogin(model.UserRole);
        }

        public void OnUserRoleChange(UserInfo userInfo)
        {
            EventBus.GetEvent<UserInfoEvent>().Publish(userInfo);
        }
        public void OnRemainTimeChange(int remainTime)
        {
            EventBus.GetEvent<UserLogoutEvent>().Publish(remainTime);
        }

        #endregion

        #region 模块布局保存
        public void OnAvalonLayoutSave()
        {
            EventBus.GetEvent<LayOutSaveEvent>().Publish();
        }
        #endregion

        // 选择一个可用的磁盘作为日志存储路径的根目录
        public string PickAvailableDrive()
        {
            var allFixed = DriveInfo.GetDrives()
                                    .Where(d => d.DriveType == DriveType.Fixed && d.IsReady)
                                    .ToList();

            // 1. 必须用 E:
            var eDrive = allFixed.FirstOrDefault(d => d.Name.StartsWith(@"E:\", StringComparison.OrdinalIgnoreCase));
            if (eDrive != null) return eDrive.Name;

            // 2. 其次用 D:
            var dDrive = allFixed.FirstOrDefault(d => d.Name.StartsWith(@"D:\", StringComparison.OrdinalIgnoreCase));
            if (dDrive != null) return dDrive.Name;

            // 3. exe程序所在的盘
            var fallback = Path.GetPathRoot(AppDomain.CurrentDomain.BaseDirectory);
            if (fallback != null) return fallback;

            return "";
        }
        #region 配方管理
        /// <summary>
        /// 配方激活
        /// </summary>
        /// <param name="recipe"></param>
        public void OnActiveRecipe(Recipe recipe)
        {
            if (_deviceEngine.GetMachineStatus() == EngineStatus.Running)
            {
                return;//防止运行过程中切换配方！！！
            }

            string hardConfigFile = "Hardware.dproj";
            string slnDir = Path.GetDirectoryName(recipe.ProjInfo.FullName);
            // 配置路径
            _deviceEngine.RecipeSlnPath = slnDir;
            string configDir = Path.Combine(slnDir, "Config");
            _deviceEngine.RecipeConfigPath = configDir;
            if (!Directory.Exists(configDir))
            {
                Directory.CreateDirectory(configDir);
            }

            //配置log目录
            //如果路径为空
            if (string.IsNullOrEmpty(_mController.FileConfig.LogsSavePath))
            {
                string webFile = "WebConfig.xml";
                string webConfig = Path.Combine(configDir, webFile);
                _webService.LoadConfig(webConfig);

                var sysConfig = _webService.GetConfig() as WebConfig;
                // 不再硬编码 E:
                var root = PickAvailableDrive();
                string slogPath = Path.Combine(root, sysConfig?.StationName_Vision ?? "DefaultStation", "LUSTER", "STEP");
                //string slogPath=Path.Combine($"E:\\{sysConfig?.StationName_Vision}", "LUSTER","STEP"); // StationName
                LogTool.SetLogPath(slogPath);
            }
            else
            {
                string slogPath = Path.Combine($"{_mController.FileConfig.LogsSavePath}", "STEP");
                LogTool.SetLogPath(slogPath);
            }

            if (recipe == CurrentRecipe)
            {
                var homeContent = PageModel.Pages.FirstOrDefault(u => u.Name == "Home");
                OnNavigate(homeContent);
                OnLog(LogType.Debug, "要激活配方和当前配方相同");
                return;
            }

            string hardwareConfig = Path.Combine(configDir, hardConfigFile);
            if (!File.Exists(hardwareConfig))
            {
                // 尝试从本地拷贝过来
                string existFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", hardConfigFile);
                if (File.Exists(existFile))
                {
                    File.Copy(existFile, hardwareConfig);
                }
                else
                {
                    XElement xHard = new XElement("Devices");
                    xHard.Save(hardwareConfig);
                }
            }

            // 后台线程执行耗时加载；任何会触发UI绑定集合变更的事件/导航必须切回UI线程
            Task.Run(() =>
            {
                _isLoading = true;
                try
                {
                    _deviceEngine.OnLoading(10, 1, $"配方：{recipe.Name}加载中...");

                    // 1.打开硬件配置
                    _deviceEngine.OnLoading(10, 2, "加载硬件配置..");
                    if (CurrentRecipe != recipe)
                    {
                        // 1.1 如果配方不同，则重新初始化
                        _deviceEngine.Initialize(hardwareConfig);

                    }

                    // 2.加载系统配置
                    _deviceEngine.OnLoading(10, 3, "加载系统配置..");
                    string sysFile = "SystemConfig.xml";
                    string sysConfig = Path.Combine(configDir, sysFile);
                    string filename = "FileConfig.xml";
                    string fileConfig = Path.Combine(configDir, filename);
                    if (!File.Exists(sysConfig))
                    {
                        //SysConfig不存在，实例化防止把其它配置带入新的配方
                        _mController.SysConfig = new TaskFlow.Engine.Models.SystemConfig();
                    }

                    OnLoadSystem(sysConfig);

                    OnLoadFileConfig(fileConfig);

                    _deviceEngine.OnLoading(10, 4, "加载Web系统配置..");
                    string webFile = "WebConfig.xml";
                    string webConfig = Path.Combine(configDir, webFile);
                    _webService.LoadConfig(webConfig);

                    _configManager.SetWebConfig(_webService.GetConfig());

                    // 加载配置错误
                    _deviceEngine.OnLoading(10, 5, "加载错误类型配置..");
                    if (_errorManager != null)
                    {
                        string errConfig = Path.Combine(configDir, "Errors.xml");
                        _errorManager.LoadErrorConfig(errConfig, _deviceEngine);
                    }
                    _mController.LightInit();

                    // 加载csv配置文件
                    _visionAPI.LoadCtConfig(configDir);
                    _dbManager.LoadCtConfig(configDir);

                    // 3.激活配方
                    _deviceEngine.OnLoading(10, 7, "加载任务配置..");
                    CurrentRecipe = recipe;
                    var recipePath = recipe.GetFullname();
                    if (File.Exists(recipePath))
                    {
                        try
                        {
                            // 加载工艺配置
                            _motionEngine.LoadProjfile(recipePath);

                            // 加载配置文件
                            string iniConfig = Path.Combine(recipe.GetRecipePath(), $"{Path.GetFileNameWithoutExtension(recipe.Name)}.data");
                            _deviceEngine.RecipeDataPath = iniConfig;

                            _deviceEngine.ModuleConfigPath = Path.Combine(recipe.ProjInfo.ProjPath, "ModuleConfig");

                            if (File.Exists(iniConfig))
                            {
                                var xData = XElement.Load(iniConfig);
                                _motionEngine.ParseConfig(xData);
                                var xPos = xData.Element("PosGroup");
                                if (xPos != null)
                                {
                                    _deviceEngine.LoadPosGroup(xPos);
                                }

                                var xModule = xData.Element("ModuleNameGroup");
                                if (xModule != null)
                                {
                                    _deviceEngine.LoadModuleNameGroup(xModule);
                                }
                            }

                            LoadRecipeEvent?.Invoke(XElement.Load(recipePath));
                        }
                        catch (Exception ex)
                        {
                            _deviceEngine.OnLoading(10, 10, "加载任务配置..");
                            throw new FriendlyException(ex.Message);
                        }
                    }

                    _deviceEngine.OnLoading(10, 9, "加载数据库..");

                    // 数据库加载
                    InitDb(recipe.GetRecipePath());

                    _deviceEngine.OnLoading(10, 10, "加载工程配置..");

                    // 如果打开非激活配方
                    if (!recipe.IsActive)
                    {
                        recipe.ProjInfo.RecipeList.ForEach(u => u.IsActive = false);
                        recipe.IsActive = true;
                    }

                    // 同时激活工程
                    ProjectList.ForEach(u => u.IsActive = false);
                    recipe.ProjInfo.IsActive = true;

                    // 这些操作会触发订阅方更新ObservableCollection，必须在UI线程执行
                    var dispatcher = Application.Current?.Dispatcher;
                    if (dispatcher != null)
                    {
                        dispatcher.Invoke(() =>
                        {
                            EventBus.GetEvent<RecipeOpenEvent>().Publish(CurrentRecipe);

                            GlobalProperty.IsEnabeld = true;
                            if (CurrentUser != null)
                            {
                                var homeContent = PageModel.Pages.FirstOrDefault(u => u.Name == "Home");
                                OnNavigate(homeContent);
                            }

                            string oldProjName = ProjInfo.ProjName;
                            ProjInfo = recipe.ProjInfo;
                            if (recipe.ProjInfo.ProjName != oldProjName)
                            {
                                EventBus.GetEvent<ProjectChangeEvent>().Publish(recipe.ProjInfo);
                            }
                        });
                    }
                    else
                    {
                        // 兜底：无Dispatcher场景直接执行
                        EventBus.GetEvent<RecipeOpenEvent>().Publish(CurrentRecipe);
                    }

                    _dbManager.ProjectPath = recipe.ProjInfo.ProjPath;

                    GetCurrentRecipes();//创建当前项目配方信息供接口调用

                    // 关键参数保存（轴运动相关），每次配方激活保存一次
                    System.Threading.Tasks.Task.Run(() =>
                    {
                        _dbManager.WriteParameterToCSV();
                        // CGP工站SFCINFO保存
                        var sysConfig = _webService.GetConfig() as WebConfig;
                        if (sysConfig != null && sysConfig.StationId.Contains("CGP"))
                        {
                            // webConfig.xml拷贝到Vision系统指定目录，并更名为SFCINFO.txt
                            string targetDir = @"E:\CGP\LUSTER";
                            if (Directory.Exists(targetDir))
                            {
                                try
                                {
                                    // 目标文件路径（修改文件名和后缀）
                                    string targetFile = Path.Combine(targetDir, "SFCINFO.txt");
                                    // 拷贝文件（覆盖已存在的）
                                    File.Copy(webConfig, targetFile, overwrite: true);
                                }
                                catch (Exception ex)
                                {
                                    OnLog(LogType.Error, $"拷贝SFCINFO.txt失败:{ex.Message}");
                                }
                            }
                            
                        }
                    });
                }
                catch (Exception ex)
                {
                    // 避免后台线程异常被吞掉导致无提示
                    OnLog(LogType.Error, $"激活配方异常:{ex.Message}");
                }
                finally
                {
                    _isLoading = false;
                    isNeedSave = false;
                    _editCount = 0;
                }
            });
        }

        /// <summary>
        /// 已经打开的数据库文件
        /// </summary>
        private string _dbfile;

        private void InitDb(string recipeDir)
        {
            if (!string.IsNullOrEmpty(_dbfile))
            {
                // 切换配方后，在原配方数据库插入一条关闭记录
                _dbManager.AddSysOperation(SystemOperation.SoftClose, CurrentUser?.UserName);
                //关闭之前配方数据库
                _dbFactory.DbClose();
            }

            // 1.配置数据库
            var dbDir = Path.Combine(recipeDir, "db");
            if (!Directory.Exists(dbDir))
            {
                Directory.CreateDirectory(dbDir);
            }

            _dbfile = Path.Combine(dbDir, "recipe.db");
            var _dbfile_Ass = Path.Combine(dbDir, "recipe_Ass.db");
            string connectionString = $"Data Source={_dbfile};Version=3;";
            string connectionString_Ass = $"Data Source={_dbfile_Ass};Version=3;";
            _dbFactory.SetConnectionStr(connectionString, connectionString_Ass, dbDir);
        }

        /// <summary>
        /// 工程保存
        /// </summary>
        public void OnSaveRecipe(string saveRecipe = "")
        {
            if (CurrentRecipe == null) return;

            string savePath = saveRecipe;
            if (string.IsNullOrEmpty(savePath))
            {
                savePath = CurrentRecipe.GetFullname();
            }

            XElement xRecipe = _motionEngine.ExportXml();
            xRecipe.SetAttributeValue("Name", CurrentRecipe.Name);
            xRecipe.Save(savePath);

            // 导出参数配置文件
            var iniDataFile = Path.Combine(Path.GetDirectoryName(savePath), $"{Path.GetFileNameWithoutExtension(savePath)}.data");
            _motionEngine.BuildPrimModule();
            XElement xIni = _motionEngine.ExportConfig();

            // 将点位信息保持到组中
            _deviceEngine.SavePosGroup(xIni);

            // 将模组信息保持到组中
            _deviceEngine.SaveModuleNameGroup(xIni);
            xIni.Save(iniDataFile);

            // 此时是主动保存，需要将IsNeedSave置为false
            if (string.IsNullOrEmpty(saveRecipe))
            {
                IsNeedSave = false;
                _editCount = 0;
            }
            if (saveRecipe == "")
            {
                OnLog(LogType.Info, SystemConsts.ProjectSaved);
            }

            // 3D任务保存
            //if (threeEngineUI != null && threeEngineUI.IsNeedSave)
            //{
            //    threeEngineUI.OnSaveProj();
            //}
        }

        /// <summary>
        /// 配方备份
        /// </summary>
        /// <param name="saveRecipe"></param>
        public void OnBackUpRecipe(bool isManual = false)
        {
            if (CurrentRecipe == null) return;
            string saveType = isManual ? "Manual" : "Auto";

            string backDir = Path.Combine(CurrentRecipe.ProjInfo.ProjPath, ".luster");
            if (!Directory.Exists(backDir))
            {
                FolderTool.CreateDir(backDir, true);
            }

            // 将配方存放到工程目录下
            var dstDir = CurrentRecipe.GetSavePath();

            string filename = $"{saveType}_{DateTime.Now.ToString("yy_MM_dd_HH_mm_ss")}";
            string dstFile = Path.Combine(dstDir, $"{filename}.recipe");

            // 文件Copy
            File.Copy(CurrentRecipe.GetFullname(), dstFile, true);

            string dstDataFile = Path.Combine(dstDir, $"{filename}.data");
            File.Copy(CurrentRecipe.GetFullData(), dstDataFile, true);

            // 硬件配置
            OnBackHardware(isManual);
        }

        /// <summary>
        /// 备份硬件
        /// </summary>
        /// <param name="isManual">是否手动</param>
        private void OnBackHardware(bool isManual = false)
        {
            if (CurrentRecipe == null) return;
            string SaveType = isManual ? "Manual" : "Auto";

            // 将配方存放到工程目录下
            string dstDir = Path.Combine(CurrentRecipe.ProjInfo.ProjPath, ".luster", "Config");
            if (!Directory.Exists(dstDir))
            {
                FolderTool.CreateDir(dstDir, true);
            }
            string dstFile = Path.Combine(dstDir, $"{SaveType}_{DateTime.Now.ToString("yy_MM_dd_HH_mm_ss")}.dproj");

            string src = Path.Combine(CurrentRecipe.ProjInfo.ProjPath, "Config", "Hardware.dproj");
            File.Copy(src, dstFile, true);
        }

        /// <summary>
        /// 事件触发
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="K"></typeparam>
        /// <param name="eventData"></param>
        public void PublishEvent<T, K>(K eventData) where T : PubSubEvent<K>, new()
        {
            EventBus.GetEvent<T>().Publish(eventData);
        }

        /// <summary>
        /// 保存
        /// </summary>
        public void OnSaveDevice()
        {
            _motionEngine.DeviceEngine.Save("");
        }
        #endregion 工程管理

        /// <summary>
        /// 多语言
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string L(string key)
        {
            return Luster.Motion.Assests.Langs.LangProvider.GetLang(key);
        }

        /// <summary>
        /// 模式切换
        /// </summary>
        /// <param name="mode"></param>
        public void ChangeDeviceMode(DeviceMode mode)
        {
            Task.Run(() =>
            {
                if (mode == _deviceEngine.DeviceMode) return;

                // 需要先关闭监控，否则会触发急停监控
                if (_deviceEngine.DeviceMode == DeviceMode.Real)
                {
                    _mController.StopButtonMontor();
                }

                if (!_deviceEngine.SetEngineMode(mode, out var msg))
                {
                    CurrentUser.CurrentMode = _deviceEngine.DeviceMode;

                    OnLog(new LogInfo() { LogType = LogType.Error, LogMessage = msg });
                }

                EventBus.GetEvent<RunModeEvent>().Publish(_deviceEngine.DeviceMode);
            });
        }

        #region Log事件
        /// <summary>
        /// 触发Log
        /// </summary>
        /// <param name="logInfo"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void OnLog(LogInfo logInfo)
        {
            EventBus.GetEvent<LogInfoEvent>().Publish(logInfo);
            switch (logInfo.LogType)
            {
                case LogType.Debug:
                    LogTool.Debug(logInfo.LogMessage);
                    break;

                case LogType.Info:
                    LogTool.Info(logInfo.LogMessage);
                    break;

                case LogType.Warning:
                    LogTool.Warn(logInfo.LogMessage);
                    break;

                case LogType.Error:
                    LogTool.Error(logInfo.LogMessage);

                    // 如果有错误，那么就弹出提示创建
                    //HandyControl.Controls.Growl.Error(logInfo.LogMessage);
                    break;

                default:
                    LogTool.Error(logInfo.LogMessage);
                    break;
            }
        }

        /// <summary>
        /// Log
        /// </summary>
        /// <param name="logType"></param>
        /// <param name="message"></param>
        public void OnLog(LogType logType, string message, string logThreadNo = "")
        {
            OnLog(new LogInfo() { LogType = logType, LogMessage = message, LogThreadID = logThreadNo });
            //EventBus.GetEvent<LogInfoEvent>().Publish();
        }
        #endregion


        #region 数据映射
        /// <summary>
        /// 获取任务对应的数据
        /// </summary>
        /// <returns></returns>
        public List<LNode> GetOutDataTree()
        {
            List<LNode> outDataTree = new List<LNode>();

            var global = _motionEngine.Get(GlobalModule.GlobalID);
            outDataTree.Add(CreateNode(global));

            // 构建数据树
            var stations = _motionEngine.GetStations();
            foreach (var item in stations)
            {
                var stationNode = CreateNode(item);
                outDataTree.Add(stationNode);
            }

            return outDataTree;
        }

        /// <summary>
        /// 创建子节点
        /// </summary>
        /// <param name="motionModule"></param>
        /// <returns></returns>
        private LNode CreateNode(IMotionModule motionModule)
        {
            LNode mNode = new LNode() { Key = motionModule.ID.ToString(), Text = motionModule.Alias, Tag = "Module" };
            foreach (var item in motionModule.Parameters)
            {
                if (motionModule is not IGlobal && item.Value.ParamType == Luster.TaskFlow.Common.Enums.ParamType.IN) continue;

                if (item.Key == "Status") continue;
                LNode paramNode = new LNode()
                {
                    Text = item.Value.CN,
                    Tag = new MapData() { ID = motionModule.ID, DataSource = $"{motionModule.Alias}.{item.Value.CN}", Alias = $"{item.Value.CN}", Key = item.Value.Name, DataType = item.Value.Type },
                };

                mNode.Children.Add(paramNode);
            }

            // 子节点遍历
            foreach (var item in motionModule.Children)
            {
                var subNode = CreateNode(item);
                mNode.Children.Add(subNode);
            }

            return mNode;
        }

        /// <summary>
        /// 获取数据映射
        /// </summary>
        /// <returns></returns>
        public List<MapData> GetMapDatas()
        {
            return _motionEngine.MapDatas;
        }

        /// <summary>
        /// 添加映射对象
        /// </summary>
        /// <param name="id"></param>
        /// <param name="key"></param>
        /// <param name="alias"></param>
        public void AddMapData(MapData mapData)
        {
            if (_motionEngine.MapDatas.Any(u => u.ID == mapData.ID && u.Key == mapData.Key))
            {
                throw new FriendlyException("对象已存在!");
            }

            // 更新输出模块
            var outModule = _motionEngine.Get(mapData.ID);
            mapData.Parameter = outModule.Parameters[mapData.Key];
            mapData.Parameter.IsOutData = true;
            mapData.Parameter.Alias = mapData.Alias;
            outModule.IsOutData = outModule.Parameters.Any(u => u.Value.IsOutData);
            _motionEngine.MapDatas.Add(mapData);
            IsNeedSave = true;
            EventBus.GetEvent<MapDataEvent>().Publish(mapData);
        }

        /// <summary>
        /// 移除映射对象
        /// </summary>
        /// <param name="mapData">映射对象</param>
        public void RemoveMapData(MapData mapData)
        {
            _motionEngine.MapDatas.Remove(mapData);
            IsNeedSave = true;
            EventBus.GetEvent<MapDataEvent>().Publish(mapData);
            // 更新输出模块
            var outModule = _motionEngine.Get(mapData.ID);

            if (outModule != null)
            {
                outModule.Parameters[mapData.Key].IsOutData = false;
                outModule.IsOutData = outModule.Parameters.Any(u => u.Value.IsOutData);
            }
        }

        /// <summary>
        /// 对象更新
        /// </summary>
        /// <param name="data"></param>
        public void UpdateMapData(MapData mapData)
        {
            var map = _motionEngine.MapDatas.FirstOrDefault(u => u.ID == mapData.ID && u.Key == mapData.Key && u.Alias == mapData.Alias);
            if (map == null)
            {
                throw new FriendlyException("对象不已存在!");
            }
            IsNeedSave = true;

            EventBus.GetEvent<MapDataEvent>().Publish(map);
        }


        /// <summary>
        /// 跟新数据源
        /// </summary>
        /// <param name="mapData"></param>
        public void UpdayeMapDataSource(List<MapData> mapData, List<MapData> newMapDatas, List<MapData> removeMapData)
        {
            _motionEngine.MapDatas = mapData;

            if (newMapDatas.Count > 0)
            {
                foreach (var map in newMapDatas)
                {
                    // 更新输出模块
                    var outModule = _motionEngine.Get(map.ID);
                    map.Parameter = outModule.Parameters[map.Key];
                    map.Parameter.IsOutData = true;
                    map.Parameter.Alias = map.Alias;
                    outModule.IsOutData = outModule.Parameters.Any(u => u.Value.IsOutData);
                }
            }

            if (removeMapData.Count > 0)
            {
                foreach (var map in removeMapData)
                {
                    // 更新输出模块
                    var outModule = _motionEngine.Get(map.ID);
                    if (outModule != null)
                    {
                        outModule.Parameters[map.Key].IsOutData = false;
                        outModule.IsOutData = outModule.Parameters.Any(u => u.Value.IsOutData);
                    }
                }
            }
            EventBus.GetEvent<MapDataEvent>().Publish(mapData.FirstOrDefault());
            IsNeedSave = true;
        }
        #endregion

        #region 工程
        /// <summary>
        /// 工程列表
        /// </summary>
        public List<ProjectInfo> ProjectList { get; set; }

        /// <summary>
        /// 加载工程
        /// </summary>
        /// <param name="proj"></param>
        public void InitSolution(string solution = "")
        {
            ProjectList = new List<ProjectInfo>();
            string projConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "SolutionConfig.xml");
            if (File.Exists(projConfigPath))
            {
                try
                {
                    XElement xRoots = XElement.Load(projConfigPath);
                    foreach (var xItems in xRoots.Elements("Solution"))
                    {
                        ProjectInfo data = new ProjectInfo();
                        data.ParserXml(xItems);
                        if (Directory.Exists(data.ProjPath))
                        {
                            data.RecipeList = data.BuildRecipeList(data.FullName, data);
                            ProjectList.Add(data);
                        }

                    }
                }
                catch (Exception e)
                {
                    OnLog(LogType.Error, $"解析SolutionConfig.xml 异常:{e.Message}");
                }
            }
        }

        /// <summary>
        /// 添加工程
        /// </summary>
        /// <param name="projName"></param>
        /// <param name="rootPath"></param>
        /// <param name="recipe"></param>
        public void AddProject(string projName, string slnPath, string recipe)
        {
            ProjectInfo proj = ProjectInfo.NewProject(slnPath, projName, recipe);
            ProjInfo = proj;
            ProjectList.Add(proj);
            IsNeedSave = true;
            SaveSolution();
        }

        /// <summary>
        /// 删除工程
        /// </summary>
        /// <param name="slnpath"></param>
        [Obsolete]
        public void RemoveProject1(string slnpath)
        {
            var proj = ProjectList.FirstOrDefault(u => u.FullName == slnpath);
            if (proj != null)
            {
                //ProjectInfo existProj = ProjectList.FirstOrDefault(t => t.FullName == slnpath);
                ProjectList.Remove(proj);

                if (proj.RecipeList != null)
                {
                    //删除配方中的日志
                    foreach (var recipeItem in proj.RecipeList)
                    {
                        string logpath = Path.Combine(recipeItem.GetRecipePath(), "logs");
                        if (Directory.Exists(logpath))
                        {
                            Directory.Delete(logpath, true);
                        }
                    }
                }

                if (Directory.Exists(proj.ProjPath))
                {
                    Microsoft.VisualBasic.FileIO.FileSystem.DeleteDirectory(proj.ProjPath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                }
                IsNeedSave = true;
            }
        }
        public void RemoveProject(string slnpath)
        {
            var proj = ProjectList.FirstOrDefault(u => u.FullName == slnpath);
            if (proj != null)
            {
                //ProjectInfo existProj = ProjectList.FirstOrDefault(t => t.FullName == slnpath);
                ProjectList.Remove(proj);

                //if (proj.RecipeList != null)
                //{
                //    //删除配方中的日志
                //    foreach (var recipeItem in proj.RecipeList)
                //    {
                //        string logpath = Path.Combine(recipeItem.GetRecipePath(), "logs");
                //        if (Directory.Exists(logpath))
                //        {
                //            Directory.Delete(logpath, true);
                //        }
                //    }
                //}

                //if (Directory.Exists(proj.ProjPath))
                //{
                //    Microsoft.VisualBasic.FileIO.FileSystem.DeleteDirectory(proj.ProjPath, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                //}
                IsNeedSave = true;
            }
        }

        /// <summary>
        /// 打开已有工程
        /// </summary>
        /// <param name="projname"></param>
        public void OpenExistProj(string projName, string slnPath)
        {
            ProjectInfo proj = ProjectInfo.OpenExistProj(projName, slnPath);
            ProjectList.Add(proj);
            IsNeedSave = true;
        }

        /// <summary>
        /// 保存Solution
        /// </summary>
        public void SaveSolution(string solution = "")
        {
            if (string.IsNullOrEmpty(solution))
            {
                solution = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "SolutionConfig.xml");
            }

            if (!Directory.Exists(Path.GetDirectoryName(solution)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(solution));
            }

            if (ProjectList != null)
            {
                XElement xRoot = new XElement("SolutionConfig");
                foreach (var item in ProjectList)
                {
                    xRoot.Add(item.ExportXml());
                    var receiptlist = item.RecipeList;
                    if (receiptlist != null)
                    {
                        XElement xrecipe = new XElement("Solution");
                        foreach (var recipe in receiptlist)
                        {
                            xrecipe.Add(recipe.ExportXml());
                        }
                        if (!Directory.Exists(Path.GetDirectoryName(item.FullName)))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(item.FullName));
                        }
                        xrecipe.Save(item.FullName);
                    }
                }
                xRoot.Save(solution);
            }

        }

        private void GetCurrentRecipes()
        {
            if (ProjInfo == null) return;
            if (ProjInfo.RecipeList == null) return;
            List<RecipeModel> listrecipe = new List<RecipeModel>();
            foreach (var proj in ProjInfo.RecipeList)
            {
                var model = new RecipeModel()
                {
                    ProjectName = proj.ProjInfo.ProjName,
                    RecipeName = proj.Name,
                    IsActive = proj.IsActive,
                    RecipePath = proj.GetRecipePath(),
                };
                listrecipe.Add(model);
            }
            _mController.SysConfig.ListRecipe = listrecipe;
            _mController.SaveSysConfig();


        }
        #endregion

        #region 数据库/日志/配方备份管理
        public void CheckBackUpFile()
        {
            if (ProjectList != null)
            {
                foreach (var proj in ProjectList)
                {
                    string slnDir = proj.ProjPath;
                    string configDir = Path.Combine(slnDir, "Config");
                    string fileConfig = Path.Combine(configDir, "FileConfig.xml");
                    foreach (var recipe in proj.RecipeList)
                    {
                        string dbPath = Path.Combine(slnDir, $"{recipe.Name}\\db\\recipe.db");
                        OnLoadFileConfig(fileConfig);
                        CheckBackupDB(dbPath, recipe.Name);
                    }
                }
            }
        }

        /// <summary>
        /// 数据库检查备份
        /// </summary>
        /// <param name="dbpath"></param>
        /// <param name="projname"></param>
        public void CheckBackupDB(string dbpath, string projname)
        {
            string DumpDBPath = _mController.FileConfig.DumpDBPath ?? "";
            if (DumpDBPath == "") return;
            if (File.Exists(dbpath))
            {
                FileInfo filedb = new FileInfo(dbpath);
                DateTime dbCreateTime = filedb.CreationTime;
                int DbSaveDays = _mController.FileConfig.DBSaveDays;
                if (DbSaveDays > 0)
                {
                    double days = (DateTime.Now - dbCreateTime).TotalDays;
                    if (days > DbSaveDays)
                    {
                        string RenameDB = dbCreateTime.ToString("yyMMdd") + "-" + DateTime.Now.ToString("yyMMdd");
                        string destionpath = Path.Combine(DumpDBPath, projname, $"{RenameDB}.db");
                        if (!Directory.Exists(Path.GetDirectoryName(destionpath)))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(destionpath));
                        }
                        filedb.CopyTo(destionpath, true);
                        string connectionString = $"Data Source={dbpath}";
                        _dbFactory.SetConnectionStr(connectionString);

                        // 获取所有数据实体
                        Assembly assembly = Assembly.LoadFrom("Luster.Common.DataAccess.dll");// 加载数据文件所在的程序集
                        var allTypes = assembly.GetTypes().Where(x => !x.IsAbstract && !x.IsInterface && x.Name.StartsWith("Tb")).ToArray();
                        var removes = allTypes.Where(x => x != typeof(TbLTolerance) && x != typeof(TbVersion));
                        if (removes.Count() > 0)
                        {
                            _dbFactory.DbDrop(removes.ToArray());
                        }

                        File.SetCreationTime(dbpath, DateTime.Now);//修改数据库创建时间
                    }
                }
            }


        }
        #endregion

        public void ChangeLanguage()
        {
            if (ProjectList != null && ProjectList.Count > 0)
            {
                var projectInfo = ProjectList.FirstOrDefault(u => u.IsActive == true);
                ProjectInfo SelectProject = null;
                if (projectInfo != null)
                {
                    SelectProject = projectInfo;
                }
                else
                {
                    SelectProject = ProjectList[0];
                }
                if (SelectProject != null)
                {
                    string slnDir = SelectProject.ProjPath;
                    string configDir = Path.Combine(slnDir, "Config");
                    string sysConfig = Path.Combine(configDir, "SystemConfig.xml");
                    OnLoadSystem(sysConfig);
                    if (_mController.SysConfig.Language != null)
                    {
                        EventBus.GetEvent<LangChangedEvent>().Publish(_mController.SysConfig.Language);
                    }
                }
            }
        }

        /// <summary>
        /// 工程名称
        /// </summary>
        /// <param name="taskName"></param>
        public void NewOrOpenHolo3D(IMotionModule holoModule, string taskName, bool isNew = true)
        {
            // 通知添加Holo3D页面
            EventBus.GetEvent<ShowHolo3D>().Publish();

            // 切换到3D页面
            var holoContent = PageModel.Pages.FirstOrDefault(u => u.Name == "Holo3D");
            OnNavigate(holoContent);

            var holo3D = holoModule.TaskFunction as IHolo3D;
            if (isNew)
            {
                taskName = Path.Combine(CurrentRecipe.GetRecipePath(), "Holo3D", $"{taskName}.3dProj");

                // 构建路径
                FolderTool.CreateDir(Path.GetDirectoryName(taskName));

                if (File.Exists(taskName))
                {
                    throw new FriendlyException($"文件:{taskName}已经存在!");
                }

                // 配置3D参数
                var newPath = new LPath(taskName);
                holoModule.SetInParameter(nameof(holo3D.TaskPath), newPath);
            }

            //var curFlowEngine = holo3D.Holo3DEngine as ITaskFlowEngine;

            // 配置参数
            //threeEngineUI?.SetFlowEngine(curFlowEngine, taskName);
        }

        /// <summary>
        /// 切换配方
        /// </summary>
        /// <param name="obj"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void MController_ChangeRecipeEvent(string recipe)
        {
            if (recipe == "" || recipe == null) return;
            if (ProjInfo == null) return;
            foreach (var item in ProjInfo.RecipeList)
            {
                if (item.Name == recipe)
                {
                    OnActiveRecipe(item);
                }
            }
        }


        #region 模块变更
        /// <summary>
        /// 触发变更记录
        /// </summary>
        /// <param name="changeType">新增/修改/删除</param>
        /// <param name="module">模块</param>
        /// <param name="prop">属性</param>
        /// <param name="content">变更内容</param>
        public void OnChangeRecord(OperationType changeType, string module, string prop, string content)
        {
            var newRecord = new TbChangeRecord()
            {
                ChangeType = changeType.ToString(),
                Content = content,
                CreateTime = DateTime.Now,
                Module = module,
                Property = prop,
                Role = CurrentUser.UserName,
            };

            _dbManager.AddChangeRecord(newRecord);

        }
        #endregion

        #region SystemUI dll
        private Type uiModuleType = null;
        private Type toolBarContentType = null;
        private Type mainContentType = null;

        /// <summary>//
        /// 加载SystemUI
        /// </summary>
        public void RegisterSystemDll()
        {
            string uiDll = $"Luster.Motion.SubSystem";
            string extension = ConfigHelper.GetAppKey("UITemplate");
            if (!string.IsNullOrEmpty(extension))
            {
                uiDll = $"{uiDll}.{extension}";
            }

            string assemblyFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{uiDll}.dll");
            if (!File.Exists(assemblyFile))
            {
                throw new FriendlyException($"文件:{assemblyFile}不存在");
            }

            // 动态加载程序集
            Assembly assembly = Assembly.LoadFile(assemblyFile);

            foreach (var item in assembly.GetTypes())
            {
                if (item.IsInterface || item.IsAbstract) continue;

                if (typeof(IModule).IsAssignableFrom(item))
                {
                    uiModuleType = item;
                }

                // 对
                if (typeof(IToolbarContent).IsAssignableFrom(item))
                {
                    toolBarContentType = (Type)item;
                }

                if (typeof(IMainContent).IsAssignableFrom(item))
                {
                    mainContentType = (Type)item;
                }
            }
        }

        /// <summary>
        /// 获取主页类型
        /// </summary>
        /// <returns></returns>
        public Type GetUiModuleType()
        {
            return uiModuleType;
        }

        /// <summary>
        /// 获取主页类型
        /// </summary>
        /// <returns></returns>
        public Type GetMainContentType()
        {
            return mainContentType;
        }

        /// <summary>
        /// 获取工具栏类型
        /// </summary>
        /// <returns></returns>
        public Type GetToolbarContentType()
        {
            return toolBarContentType;
        }
        #endregion
    }
}
