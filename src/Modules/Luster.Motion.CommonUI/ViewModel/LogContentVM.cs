#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LogContentVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.EditorUI.ViewModel
* 文 件 名:       LogContentVM.cs
* 创建时间:       2022/8/7 15:33:38
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      9b5d0f31-a0d4-4ea9-bbfd-4cf5313146b0
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/8/7 15:33:38
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.Tools;
using Luster.Control.Wpf.Motion.Controls;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.Integration.Web;
using Luster.Motion.TaskFlow.Engine;
using Luster.TaskFlow.Motion;
using Microsoft.VisualBasic.Logging;
using Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Luster.Motion.CommonUI.ViewModel
{
    [SyncActiveStateAttribute]
    public class LogContentVM : MotionPageVM, IActiveAware
    {
        private Dictionary<int, List<LogInfo>> logCache = new Dictionary<int, List<LogInfo>>();

        private ConcurrentQueue<LogInfo> logQueue;

        /// <summary>
        /// 日志控件
        /// </summary>
        private LogCtrl logCtrl;

        /// <summary>
        /// 线程
        /// </summary>
        private Dispatcher _dispatcher;

        private WhileTool whileTool = new WhileTool(true);

        private IMotionEngine _mEngine = null;

        private IMotionController _motionController;

        private ICommonBus _bus;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="commonBus"></param>
        /// <param name="dispatcher"></param>
        /// <param name="engine"></param>
        public LogContentVM(ICommonBus commonBus, Dispatcher dispatcher, IMotionEngine engine, IMotionController motionController) : base(commonBus)
        {
            _bus = commonBus;
            _motionController = motionController;
            _mEngine = engine;
            _dispatcher = dispatcher;
            logQueue = new ConcurrentQueue<LogInfo>();
            Task.Run(() =>
            {
                whileTool.While(() =>
                {
                    logQueue.TryDequeue(out var log);
                    if (logCtrl != null)
                    {

                        logCtrl.AddLog(log);
                    }
                }, 10);
            });

            DebugChecked = true;
            WarningChecked = true;
            InfoChecked = true;
            ErrorChecked = true;
            MonitorText = string.Empty;
        }


        /// <summary>
        /// 监控模块
        /// </summary>
        private IMotionModule _monitorModule = null;

        private static object lockLog = new object();

        /// <summary>
        /// 是否运行中
        /// </summary>
        /// <returns></returns>
        private bool IsRunning()
        {
            return _mEngine.EngineStatus == EngineStatus.Running;
        }

        private DeviceMode GetDeviceMode()
        {
            return _mEngine.DeviceEngine.DeviceMode;
        }

        protected override void RegisterEvent(IEventAggregator bus)
        {
            bus.GetEvent<LogInfoEvent>().Subscribe(log =>
            {
                if (IsRunning())
                {
                    // 运行中只记录错误日志
                    if (_monitorModule == null)
                    {
                        RecordLog(log);
                    }
                    else
                    {
                        int threadID = Thread.CurrentThread.ManagedThreadId;
                        if (_monitorModule.ThreadID == threadID)
                        {
                            if (DebugChecked && log.LogType == LogType.Debug)
                            {
                                // 运行过程中，不在记录日志
                                logQueue.Enqueue(log);
                            }

                            if (InfoChecked && log.LogType == LogType.Info)
                            {

                                logQueue.Enqueue(log);
                            }

                            RecordLog(log);
                        }
                    }
                }
                else
                {
                    // 调试阶段
                    if (GetDeviceMode() == DeviceMode.Virtual || GetDeviceMode() == DeviceMode.Empty)
                    {
                        log.LogThreadID = $"[{GetDeviceMode().GetDescription()}] [{Thread.CurrentThread.ManagedThreadId}]";
                    }
                    else
                    {
                        // 在线模式不显示当前的状态
                        log.LogThreadID = $"[{Thread.CurrentThread.ManagedThreadId}]";
                    }

                    if (DebugChecked && log.LogType == LogType.Debug)
                    {
                        // 运行过程中，不在记录日志
                        logQueue.Enqueue(log);
                    }

                    if (InfoChecked && log.LogType == LogType.Info)
                    {

                        logQueue.Enqueue(log);
                    }

                    RecordLog(log);
                }
            });

            // LOG 监控
            bus.GetEvent<LogMonitorEvent>().Subscribe(m =>
            {
                _monitorModule = m;

                // 监控工站信息
                MonitorText = m.Station.Alias;

                ShowMonitor = true;

                // 加载隶属当前线程的log
                //if (logCache.ContainsKey(threadID))
                //{
                //    var logList = logCache[threadID];
                //    var tmpList = new List<LogInfo>(logList);
                //    foreach (var item in tmpList)
                //    {
                //        if (logCtrl != null)
                //        {
                //            logCtrl.AddLog(item);
                //        }
                //    }

                //    tmpList.Clear();
                //}
            });

            // 操作事件
            bus.GetEvent<OperationEvent>().Subscribe(u =>
            {
                if (u.Dst == EngineStatus.Ready)
                {
                    logCache.Clear();
                }
            });
        }

        /// <summary>
        /// 记录报警日志
        /// </summary>
        /// <param name="log"></param>
        private void RecordLog(LogInfo log)
        {
            // 警告类日志必须保存
            if (log.LogType == LogType.Warning)
            {
                logQueue.Enqueue(log);
            }

            // 错误类日志必须保存
            if (log.LogType == LogType.Error)
            {
                logQueue.Enqueue(log);
            }
        }


        public DelegateCommand<object> _loadedCommand;
        public DelegateCommand<object> LoadedCommand => _loadedCommand ?? (_loadedCommand = new DelegateCommand<object>((obj) =>
        {
            RoutedEventArgs args = obj as RoutedEventArgs;
            if (args != null)
            {
                logCtrl = args.Source as LogCtrl;
            }
        }));

        /// <summary>
        /// 日志清除
        /// </summary>
        public DelegateCommand _clearLogCommand;
        public DelegateCommand ClearLogCommand => _clearLogCommand ?? (_clearLogCommand = new DelegateCommand(() =>
        {
            logCtrl.ClearLog();
        }));

        /// <summary>
        /// 日志清除
        /// </summary>
        public DelegateCommand _clearMonitorCommand;
        public DelegateCommand ClearMonitorCommand => _clearMonitorCommand ?? (_clearMonitorCommand = new DelegateCommand(() =>
        {
            _monitorModule = null;
            ShowMonitor = false;
        }));

        /// <summary>
        /// 调试日志
        /// </summary>
        private bool _showMonitor;
        public bool ShowMonitor
        {
            get { return _showMonitor; }
            set { SetProperty(ref _showMonitor, value); }
        }

        /// <summary>
        /// 调试日志
        /// </summary>
        private bool _debugChecked;
        public bool DebugChecked
        {
            get { return _debugChecked; }
            set { SetProperty(ref _debugChecked, value); }
        }

        /// <summary>
        /// 调试日志
        /// </summary>
        private bool _infoChecked;
        public bool InfoChecked
        {
            get { return _infoChecked; }
            set { SetProperty(ref _infoChecked, value); }
        }

        /// <summary>
        /// 调试日志
        /// </summary>
        private bool _warningChecked;
        public bool WarningChecked
        {
            get { return _warningChecked; }
            set { SetProperty(ref _warningChecked, value); }
        }

        /// <summary>
        /// 调试日志
        /// </summary>
        private bool _errorChecked;
        public bool ErrorChecked
        {
            get { return _errorChecked; }
            set { SetProperty(ref _errorChecked, value); }
        }

        /// <summary>
        /// 监控日志
        /// </summary>
        private string _monitorText;

        public event EventHandler IsActiveChanged;

        public string MonitorText
        {
            get { return _monitorText; }
            set { SetProperty(ref _monitorText, value); }
        }

        private bool _isActive;
        public bool IsActive
        {
            get => _isActive; set
            {
                _isActive = value;
            }
        }
        ///// <summary>
        ///// 本地日志路径
        ///// </summary>
        //private string _logFolderPath;
        //public string LogFolderPath
        //{
        //    get => _logFolderPath;
        //    set => SetProperty(ref _logFolderPath, value);
        //}

        public WebConfig sysConfig = null;
        private DelegateCommand _openLogFolderCommand;
        public DelegateCommand OpenLogFolderCommand =>
            _openLogFolderCommand ?? (_openLogFolderCommand = new DelegateCommand(() =>
            {
                sysConfig = _motionController.WebService.GetConfig() as WebConfig;
                string root = _bus.PickAvailableDrive();
                string path = Path.Combine(root, sysConfig?.StationName_Vision ?? "DefaultStation", "LUSTER");
                if (string.IsNullOrWhiteSpace(path))
                {
                    throw new FriendlyException($"日志路径为空，无法打开!");
                    //System.Windows.MessageBox.Show("路径为空，无法打开。");
                }

                // 取盘符【暂不需要】
                //var root = System.IO.Path.GetPathRoot(path);
                //if (!string.IsNullOrEmpty(root) &&
                //    !System.IO.DriveInfo.GetDrives()
                //                        .Any(d => d.Name.Equals(root, System.StringComparison.OrdinalIgnoreCase)))
                //{
                //    System.Windows.MessageBox.Show($"本地不存在 {root} 盘，无法打开目录。");
                //    return;
                //}

                if (!System.IO.Directory.Exists(path))
                {
                    throw new FriendlyException($"日志路径所在的目录不存在：{path}");
                }

                System.Diagnostics.Process.Start("explorer.exe", path);
            }));

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            base.OnNavigatedFrom(navigationContext);
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
        }
    }
}