using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.FXVirtual;
using Luster.SimDevice.SubSystem.Views;
using Prism.Commands;
using Prism.Mvvm;

namespace Luster.SimDevice.SubSystem.ViewModel
{
    public class FXTCPContentVM : BindableBase, IDisposable
    {
        private readonly FXTCP _fxtcp;
        private readonly IDeviceEngine _deviceEngine;
        private bool _isServiceRunning;
        private string _connectionStatus = "未连接";
        private bool _isBusy;
        private string _logText = string.Empty;
        private string _ip = "127.0.0.1";
        private int _port = 502;
        private string _folderPath = "E:\\";

        private int _receiveTimeSpan = 1000; // 接收数据间隔时间，单位毫秒
        private int _receiveLen = 2048;//单次读取数据的Buffer长度

        private readonly LogEventService _logService;


        private readonly object _logFileLock = new object();
        private string _currentLogFilePath;
        private const int MAX_LOG_FILES = 30; // 最多保留30个日志文件
        private const int MAX_FILE_SIZE_MB = 10; // 单个文件最大10MB

        private readonly string ConfigPath = string.Empty;


        public FXTCPContentVM(IDeviceEngine deviceEngine)
        {
            // 获取日志服务实例
            _logService = LogEventService.Instance;
            // 订阅日志事件
            _logService.LogEvent += HandleLogEvent;
            // 创建 FXTCP 实例并注入依赖
            _fxtcp = FXTCP.Insatnce;
            // 初始化命令
            StartServiceCommand = new DelegateCommand(StartService, CanStartService)
                .ObservesProperty(() => IsServiceRunning)
                .ObservesProperty(() => IsBusy);

            StopServiceCommand = new DelegateCommand(StopService, CanStopService)
                .ObservesProperty(() => IsServiceRunning)
                .ObservesProperty(() => IsBusy);

            ClearLogsCommand = new DelegateCommand(ClearLogs);

            ExportVIOVAxisCommand = new DelegateCommand(ExportVIOVAxis);

            // 初始化日志集合
            LogEntries = new ObservableCollection<LogEntry>();

            // 添加示例日志
            AddLog("FXTCP 服务已初始化", LogLevel.Info);
            this._deviceEngine = deviceEngine;
            // 设置初始选择项
            SelectedAspectModeItem = AspectModeEnumValues
                .FirstOrDefault(item => item.Key == AspectMode);
            ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "fxtcp.txt");
            LoadConfig();
        }

        private void ExportVIOVAxis()
        {
            _fxtcp?.InjectDeviceEngine(_deviceEngine);
            _fxtcp?.ExportXml(FolderPath);
            SaveConfig();
        }

        private void HandleLogEvent(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                AddLog(message, LogLevel.Warning);
            });
        }

        #region 属性

        public string Name => "FXTCP";
        public string RegionName { get; set; } = "FXTCPContent";

        public string IP
        {
            get => _ip;
            set => SetProperty(ref _ip, value);
        }

        public int ReceiveTimeSpan
        {
            get => _receiveTimeSpan;
            set => SetProperty(ref _receiveTimeSpan, value);
        }

        public int ReceiveLen
        {
            get => _receiveLen;
            set => SetProperty(ref _receiveLen, value);
        }

        public int Port
        {
            get => _port;
            set => SetProperty(ref _port, value);
        }
        public string FolderPath
        {
            get => _folderPath;
            set => SetProperty(ref _folderPath, value);
        }

        public bool IsServiceRunning
        {
            get => _isServiceRunning;
            set => SetProperty(ref _isServiceRunning, value);
        }

        public string ConnectionStatus
        {
            get => _connectionStatus;
            set => SetProperty(ref _connectionStatus, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private AspectModeEnum _aspectMode = AspectModeEnum.NoTcp;
        public AspectModeEnum AspectMode
        {
            get => _aspectMode;
            set
            {
                if (SetProperty(ref _aspectMode, value))
                {
                    // 更新选择项
                    SelectedAspectModeItem = AspectModeEnumValues
                        .FirstOrDefault(item => item.Key == value);
                }
            }
        }
        // 用于绑定的选择项属性
        private KeyValuePair<AspectModeEnum, string>? _selectedAspectModeItem;
        public KeyValuePair<AspectModeEnum, string>? SelectedAspectModeItem
        {
            get => _selectedAspectModeItem;
            set
            {
                if (SetProperty(ref _selectedAspectModeItem, value) && value.HasValue)
                {
                    // 更新枚举值
                    AspectMode = value.Value.Key;
                }
            }
        }
        public static IEnumerable<KeyValuePair<AspectModeEnum, string>> AspectModeEnumValues
       => EnumBindingSourceExtension.AspectModeEnumValues;

        public ObservableCollection<LogEntry> LogEntries { get; }

        public string LogText
        {
            get => _logText;
            set => SetProperty(ref _logText, value);
        }

        #endregion

        #region 命令

        public DelegateCommand StartServiceCommand { get; }
        public DelegateCommand StopServiceCommand { get; }
        public DelegateCommand ClearLogsCommand { get; }

        private bool CanStartService() => !IsServiceRunning && !IsBusy;
        private bool CanStopService() => IsServiceRunning && !IsBusy;

        public DelegateCommand ExportVIOVAxisCommand { get; }

        #endregion

        #region 服务控制方法

        private async void StartService()
        {
            try
            {
                IsBusy = true;
                AddLog("正在启动 FXTCP 服务...", LogLevel.Info);

                // 在后台线程执行连接操作
                await Task.Run(() =>
                {
                    bool success = _fxtcp.ReConnect(
                        times: 3,
                        retryMs: 500,
                        ip: IP,
                        port: Port,
                        _deviceEngine
                    );

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (success)
                        {
                            IsServiceRunning = true;
                            ConnectionStatus = $"已连接 {IP}--{Port}";
                            AddLog("FXTCP 服务启动成功", LogLevel.Success);

                            // 开始接收数据循环
                            StartDataReceiving();
                        }
                        else
                        {
                            ConnectionStatus = "连接失败";
                            AddLog("无法连接到 TCP 服务器", LogLevel.Error);
                        }
                    });
                });
            }
            catch (Exception ex)
            {
                AddLog($"启动服务时出错: {ex.Message}", LogLevel.Error);
                ConnectionStatus = "连接错误";
            }
            finally
            {
                IsBusy = false;
                FXVirtualMotionCardAspect.SetAspectMode((int)AspectMode);
            }
        }

        private async void StopService()
        {
            try
            {
                IsBusy = true;
                AddLog("正在停止 FXTCP 服务...", LogLevel.Info);

                await Task.Run(() =>
                {
                    // 关闭 TCP 连接
                    _fxtcp.Dispose();

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        IsServiceRunning = false;
                        ConnectionStatus = "已断开";
                        AddLog("FXTCP 服务已停止", LogLevel.Success);
                    });
                });
            }
            catch (Exception ex)
            {
                AddLog($"停止服务时出错: {ex.Message}", LogLevel.Error);
            }
            finally
            {
                IsBusy = false;
                FXVirtualMotionCardAspect.SetAspectMode((int)AspectModeEnum.NoTcp);
            }
        }

        #endregion

        #region 日志管理

        private async void AddLog(string message, LogLevel level = LogLevel.Info)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var logEntry = new LogEntry
                {
                    Timestamp = DateTime.Now,
                    Message = message,
                    Level = level
                };

                LogEntries.Add(logEntry);

                // 限制日志数量（最多保留1000条）
                if (LogEntries.Count > 1000)
                {
                    LogEntries.RemoveAt(0);
                }

                // 更新纯文本日志（可选）
                LogText += $"[{logEntry.Timestamp:HH:mm:ss}] {message}\n";
            });

            Task.Run(() => SaveLogToFile(message, level));
        }

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

            // 3. 兜底：exe程序所在的盘
            var fallback = Path.GetPathRoot(AppDomain.CurrentDomain.BaseDirectory);
            if (fallback != null) return fallback;

            return "";
        }
        private void SaveLogToFile(string message, LogLevel level)
        {
            try
            {
                lock (_logFileLock)
                {
                    // 1. 确保日志目录存在
                    var root = PickAvailableDrive();
                    string logDir = Path.Combine(root, "DefaultStation", "LUSTER", "FXTCP Log");
                    //string logDir = Path.Combine($"E:\\DefaultStation", "LUSTER", "FXTCP Log");
                    Directory.CreateDirectory(logDir);

                    // 2. 检查是否需要创建新文件
                    if (string.IsNullOrEmpty(_currentLogFilePath))
                        _currentLogFilePath = GetNewLogFilePath(logDir);

                    // 3. 检查文件大小
                    FileInfo fileInfo = new FileInfo(_currentLogFilePath);
                    if (fileInfo.Exists && fileInfo.Length > MAX_FILE_SIZE_MB * 1024 * 1024)
                    {
                        _currentLogFilePath = GetNewLogFilePath(logDir);
                    }

                    // 4. 写入日志
                    string logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
                    File.AppendAllText(_currentLogFilePath, logLine + Environment.NewLine);

                    // 5. 清理旧日志文件
                    CleanupOldLogFiles(logDir);
                }
            }
            catch (Exception ex)
            {
                // 在UI线程显示错误
                Application.Current.Dispatcher.Invoke(() =>
                {
                    AddLog($"保存日志失败: {ex.Message}", LogLevel.Error);
                });
            }
        }
        private string GetNewLogFilePath(string logDir)
        {
            return Path.Combine(logDir, $"FXLog_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
        }

        private void CleanupOldLogFiles(string logDir)
        {
            try
            {
                var logFiles = Directory.GetFiles(logDir, "FXLog_*.txt")
                    .OrderByDescending(f => new FileInfo(f).CreationTime)
                    .ToList();

                if (logFiles.Count > MAX_LOG_FILES)
                {
                    foreach (var file in logFiles.Skip(MAX_LOG_FILES))
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch { /* 忽略删除错误 */ }
                    }
                }
            }
            catch { /* 忽略清理错误 */ }
        }

        private void ClearLogs()
        {
            LogEntries.Clear();
            LogText = string.Empty;
            AddLog("日志已清空", LogLevel.Info);
        }

        #endregion

        #region 数据接收

        private bool _isReceiving;

        private void StartDataReceiving()
        {
            if (_isReceiving) return;

            _isReceiving = true;

            Task.Run(async () =>
            {
                while (IsServiceRunning && _isReceiving)
                {
                    try
                    {
                        if (AspectMode == AspectModeEnum.AllTcp)
                        {
                            // 接收数据,一直读取会导致发送的数据缓存对于的中转被覆盖掉
                            _fxtcp.ReceiveData(ReceiveLen);
                            // 记录接收状态
                            AddLog($"已监听一次数据: {BitConverter.ToString(_fxtcp.RecvDataBuff, 0, Math.Min(16, _fxtcp.RecvDataBuff.Length))}...", LogLevel.Debug);
                        }
                        else
                        {
                            AddLog($"拒绝监听数据:AspectMode不等于【AllTcp】; ", LogLevel.Debug);
                        }

                        if (!_fxtcp.IsConnect)
                        {
                            AddLog($"通讯断联", LogLevel.Error);
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        AddLog($"数据接收错误: {ex.Message}", LogLevel.Error);
                    }
                    finally
                    {
                        if (ReceiveTimeSpan < 50)
                        {
                            ReceiveTimeSpan = 50; // 确保最小间隔为50毫秒
                        }
                        await Task.Delay(ReceiveTimeSpan);
                    }
                }
                //任务结束后，重新赋值变量
                _isReceiving = false;
            });
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (IsServiceRunning)
            {
                StopService();
            }
        }

        #endregion

        #region 配置读取保存
        public void SaveConfig()
        {
            try
            {
                // 确保配置目录存在
                var configDir = Path.GetDirectoryName(ConfigPath);
                if (!Directory.Exists(configDir))
                {
                    Directory.CreateDirectory(configDir);
                }

                // 创建配置行
                var configLines = new List<string>
                {
                    $"IP={IP}",
                    $"Port={Port}",
                    $"FolderPath={FolderPath}",
                    $"ReceiveTimeSpan={ReceiveTimeSpan}",
                    $"ReceiveLen={ReceiveLen}"
                };

                // 写入文件
                File.WriteAllLines(ConfigPath, configLines);
                AddLog("配置保存成功", LogLevel.Warning);
            }
            catch (Exception ex)
            {
                AddLog($"保存配置失败{ex.Message}", LogLevel.Error);
            }
        }

        // 从文件加载配置
        public void LoadConfig()
        {
            if (!File.Exists(ConfigPath))
            {
                AddLog("配置文件不存在，使用默认配置", LogLevel.Warning);
                return;
            }

            try
            {
                var configLines = File.ReadAllLines(ConfigPath);
                var configDict = new Dictionary<string, string>();

                // 解析每行配置
                foreach (var line in configLines)
                {
                    var parts = line.Split('=');
                    if (parts.Length == 2)
                    {
                        configDict[parts[0]] = parts[1];
                    }
                }

                // 应用配置
                if (configDict.TryGetValue("IP", out var ip)) IP = ip;
                if (configDict.TryGetValue("Port", out var portStr) && int.TryParse(portStr, out var port)) Port = port;
                if (configDict.TryGetValue("FolderPath", out var path)) FolderPath = path;
                if (configDict.TryGetValue("ReceiveTimeSpan", out var timeSpanStr) && int.TryParse(timeSpanStr, out var timeSpan))
                    ReceiveTimeSpan = timeSpan;
                if (configDict.TryGetValue("ReceiveLen", out var recLenStr) && int.TryParse(recLenStr, out var recLen))
                    ReceiveLen = recLen;
                AddLog("配置加载成功", LogLevel.Warning);
            }
            catch (Exception ex)
            {
                AddLog($"加载配置失败: {ex.Message}", LogLevel.Error);
            }
        }
        #endregion

    }
    public class LogEntry : BindableBase
    {
        private DateTime _timestamp;
        private string _message;
        private LogLevel _level;

        public DateTime Timestamp
        {
            get => _timestamp;
            set => SetProperty(ref _timestamp, value);
        }

        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        public LogLevel Level
        {
            get => _level;
            set => SetProperty(ref _level, value);
        }
    }

    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Success
    }
    public enum AspectModeEnum
    {
        [Description("不通过TCP通讯")]
        NoTcp = 0,

        [Description("所有API通过TCP")]
        AllTcp = 1,

        [Description("仅发送指令通过TCP")]
        OnlyTcpSend = 2,
    }
}
