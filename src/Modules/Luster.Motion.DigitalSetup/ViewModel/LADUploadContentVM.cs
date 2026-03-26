using LiveCharts;
using LiveCharts.Wpf;
using Luster.Common.DataAccess.Repositories;
using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.CommonUI;
using Luster.Motion.DataStruct;
using Luster.Motion.DigitalSetup.AssTables;
using Luster.Motion.DigitalSetup.Datas;
using Luster.Motion.DigitalSetup.Helpers;
using Luster.Motion.EditorUI;
using Luster.Motion.TaskFlow.Engine;
using Luster.TaskFlow.Common.Enums;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Luster.Motion.DigitalSetup.ViewModel
{
    /// <summary>
    /// CPK数据模型
    /// </summary>
    public class CPKDataModel : BindableBase
    {
        private string _parameterName;
        public string ParameterName
        {
            get => _parameterName;
            set => SetProperty(ref _parameterName, value);
        }

        private double _mean;
        public double Mean
        {
            get => _mean;
            set => SetProperty(ref _mean, value);
        }

        private double _sigma;
        public double Sigma
        {
            get => _sigma;
            set => SetProperty(ref _sigma, value);
        }

        private double _minValue;
        public double MinValue
        {
            get => _minValue;
            set => SetProperty(ref _minValue, value);
        }

        private double _maxValue;
        public double MaxValue
        {
            get => _maxValue;
            set => SetProperty(ref _maxValue, value);
        }

        private double _targetValue;
        public double TargetValue
        {
            get => _targetValue;
            set => SetProperty(ref _targetValue, value);
        }

        private double _ca;
        public double Ca
        {
            get => _ca;
            set => SetProperty(ref _ca, value);
        }

        private double _cp;
        public double Cp
        {
            get => _cp;
            set => SetProperty(ref _cp, value);
        }

        private double _cpk;
        public double Cpk
        {
            get => _cpk;
            set => SetProperty(ref _cpk, value);
        }

        private List<double> _dataValues;
        public List<double> DataValues
        {
            get => _dataValues;
            set => SetProperty(ref _dataValues, value);
        }
    }

    /// <summary>
    /// 映射项数据模型
    /// </summary>
    public class MappingItem : BindableBase
    {
        private string _txtKey;
        public string TxtKey
        {
            get => _txtKey;
            set => SetProperty(ref _txtKey, value);
        }

        private string _excelKey;
        public string ExcelKey
        {
            get => _excelKey;
            set => SetProperty(ref _excelKey, value);
        }

        private string _startRow;
        public string StartRow
        {
            get => _startRow;
            set => SetProperty(ref _startRow, value);
        }

        private string _maxRow;
        public string MaxRow
        {
            get => _maxRow;
            set => SetProperty(ref _maxRow, value);
        }

        private string _minRow;
        public string MinRow
        {
            get => _minRow;
            set => SetProperty(ref _minRow, value);
        }
    }

    public class LADUploadContentVM : BaseAss
    {
        private double _progressValue;
        private bool _isChartVisible;

        // 文件路径属性
        private string _configFile1;
        private string _configFile2;
        private string _pythonScriptPath;

        // 所有CPK数据项集合
        private ObservableCollection<CPKDataModel> _allCPKData;

        // 当前显示的CPK数据
        private ObservableCollection<CPKDataModel> _currentDisplayData;

        // 所有图表数据项集合
        private ObservableCollection<ChartItemModel> _allChartItems;

        // 全局图表集合
        private SeriesCollection _seriesCollection;

        // 参数列表（从CPK文件中解析出来的检测位置名称）
        private ObservableCollection<string> _parameterList;

        // 当前选中的参数列表（多选）
        private ObservableCollection<string> _selectedParameters;

        // 弹窗临时属性
        private string _tempConfigFile1;
        private string _tempConfigFile2;
        private string _tempPythonScriptPath;
        private ObservableCollection<string> _tempParameterList;
        private ObservableCollection<string> _tempSelectedParameters;

        // 映射配置相关
        private ObservableCollection<MappingItem> _mappingItems;
        private string _logText;

        // 配置保存相关
        private const string CONFIG_DIR_NAME = "DigitalSetUpLADUpdate";
        private const string CONFIG_FILE_NAME = "LADUpdateConfig.json";
        private string _configSavePath;

        public ICommand EndCommand { get; private set; }
        public ICommand OneKeyCheckCommand { get; private set; }
        public ICommand UpdateItemsCommand { get; private set; }
        public ICommand SaveParametersCommand { get; private set; }

        // 新增命令
        public ICommand BrowseFile1Command { get; private set; }
        public ICommand BrowseFile2Command { get; private set; }
        public ICommand RunCommand { get; private set; }
        public ICommand StopCommand { get; private set; }
        public ICommand ConfigCommand { get; private set; }

        // 弹窗命令
        public ICommand BrowseTempFile1Command { get; private set; }
        public ICommand BrowseTempFile2Command { get; private set; }
        public ICommand BrowseTempPythonScriptCommand { get; private set; }
        public ICommand ConfirmCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        /// <summary>
        /// 流程Bus
        /// </summary>
        private FlowBus flowBus;

        private IDeviceEngine _deviceEngine = null;
        /// <summary>
        /// 运控控制
        /// </summary>
        private IMotionController _mController;

        public double ProgressValue
        {
            get { return _progressValue; }
            set { SetProperty(ref _progressValue, value); }
        }

        public bool IsChartVisible
        {
            get { return _isChartVisible; }
            set { SetProperty(ref _isChartVisible, value); }
        }

        /// <summary>
        /// 曲线数据
        /// </summary>
        public SeriesCollection SeriesCollection
        {
            get { return _seriesCollection; }
            set { SetProperty(ref _seriesCollection, value); }
        }

        public string ConfigFile1
        {
            get => _configFile1;
            set => SetProperty(ref _configFile1, value);
        }

        public string ConfigFile2
        {
            get => _configFile2;
            set => SetProperty(ref _configFile2, value);
        }

        public string PythonScriptPath
        {
            get => _pythonScriptPath;
            set => SetProperty(ref _pythonScriptPath, value);
        }

        public ObservableCollection<CPKDataModel> AllCPKData
        {
            get => _allCPKData;
            set => SetProperty(ref _allCPKData, value);
        }

        public ObservableCollection<CPKDataModel> CurrentDisplayData
        {
            get => _currentDisplayData;
            set => SetProperty(ref _currentDisplayData, value);
        }

        public ObservableCollection<ChartItemModel> AllChartItems
        {
            get => _allChartItems;
            set => SetProperty(ref _allChartItems, value);
        }

        public ObservableCollection<string> ParameterList
        {
            get => _parameterList;
            set => SetProperty(ref _parameterList, value);
        }

        public ObservableCollection<string> SelectedParameters
        {
            get => _selectedParameters;
            set => SetProperty(ref _selectedParameters, value);
        }

        public string TempConfigFile1
        {
            get => _tempConfigFile1;
            set => SetProperty(ref _tempConfigFile1, value);
        }

        public string TempConfigFile2
        {
            get => _tempConfigFile2;
            set => SetProperty(ref _tempConfigFile2, value);
        }

        public string TempPythonScriptPath
        {
            get => _tempPythonScriptPath;
            set => SetProperty(ref _tempPythonScriptPath, value);
        }

        public ObservableCollection<string> TempParameterList
        {
            get => _tempParameterList;
            set => SetProperty(ref _tempParameterList, value);
        }

        public ObservableCollection<string> TempSelectedParameters
        {
            get => _tempSelectedParameters;
            set => SetProperty(ref _tempSelectedParameters, value);
        }

        public ObservableCollection<MappingItem> MappingItems
        {
            get => _mappingItems;
            set => SetProperty(ref _mappingItems, value);
        }

        public string LogText
        {
            get => _logText;
            set => SetProperty(ref _logText, value);
        }

        private CommonPageModel _seletedReportPage;
        public new CommonPageModel SelectedReportPage
        {
            get => _seletedReportPage;
            set
            {
                if (_seletedReportPage != null)
                {
                    if (value != null && _seletedReportPage?.Name != value.Name)
                    {
                        SaveGridItems(ItemModels);
                    }
                }
                SetProperty(ref _seletedReportPage, value);
                base.SelectedReportPage = value;

                if (_seletedReportPage.ViewType == typeof(AssTbCPKTest))
                {
                    IsChartVisible = true;
                    ConfigKey = "CPKTestConfig";
                }
                else
                {
                    IsChartVisible = false;
                }

                LoadStationConfigFromJson();
                UpdateStationConfigs();
            }
        }

        public LADUploadContentVM(IRepository repository,
                                  IRegionManager regionManager,
                                  ICommonBus commonBus,
                                  IMotionController motionController,
                                  IDeviceEngine deviceEngine,
                                  FlowBus _flowBus,
                                  CSVHelper cSVHelper,IDialogService dialogService)
                                  : base(repository, regionManager, commonBus, cSVHelper, _flowBus, dialogService)
        {
            flowBus = _flowBus;
            _deviceEngine = deviceEngine;
            _mController = motionController;

            Pages = new ObservableCollection<CommonPageModel>();
            Pages.Add(new CommonPageModel()
            {
                Name = "CPKTest",
                IsSelected = true,
                Region = "",
                ViewType = typeof(AssTbCPKTest)
            });

            SelectedReportPage = Pages.FirstOrDefault(x => x.IsSelected);
            InitModels();

            // 初始化命令
            EndCommand = new DelegateCommand(OnEnd);
            OneKeyCheckCommand = new DelegateCommand<object>(OnOneKeyCheck);
            UpdateItemsCommand = new DelegateCommand(OnUpdateItems);
            SaveParametersCommand = new DelegateCommand(OnSaveParameters);
            BrowseFile1Command = new DelegateCommand(OnBrowseFile1);
            BrowseFile2Command = new DelegateCommand(OnBrowseFile2);
            RunCommand = new DelegateCommand(OnRun, CanRun);
            StopCommand = new DelegateCommand(OnStop, CanStop);
            ConfigCommand = new DelegateCommand(OnConfig);

            BrowseTempFile1Command = new DelegateCommand(OnBrowseTempFile1);
            BrowseTempFile2Command = new DelegateCommand(OnBrowseTempFile2);
            BrowseTempPythonScriptCommand = new DelegateCommand(OnBrowseTempPythonScript);
            ConfirmCommand = new DelegateCommand(OnConfirm);
            CancelCommand = new DelegateCommand(OnCancel);

            ConfigKey = "CPKTestConfig";

            LoadStationConfigFromJson();
            UpdateStationConfigs();

            SeriesCollection = new SeriesCollection();
            ParameterList = new ObservableCollection<string>();
            SelectedParameters = new ObservableCollection<string>();
            TempParameterList = new ObservableCollection<string>();
            TempSelectedParameters = new ObservableCollection<string>();
            AllCPKData = new ObservableCollection<CPKDataModel>();
            AllChartItems = new ObservableCollection<ChartItemModel>();
            CurrentDisplayData = new ObservableCollection<CPKDataModel>();

            // 初始化映射配置
            MappingItems = new ObservableCollection<MappingItem>
            {
                new MappingItem { TxtKey = "Install_Force", ExcelKey = "1# Paste Force", StartRow = "23", MaxRow = "18", MinRow = "20" },
                new MappingItem { TxtKey = "Install_Gap_X", ExcelKey = "X1", StartRow = "23", MaxRow = "18", MinRow = "20" },
                new MappingItem { TxtKey = "Install_Gap_Y", ExcelKey = "Y1", StartRow = "23", MaxRow = "18", MinRow = "20" },
                new MappingItem { TxtKey = "Install_CC", ExcelKey = "1# CC ", StartRow = "23", MaxRow = "18", MinRow = "20" }
            };

            LogText = "";

            // 初始化配置保存路径并加载配置
            InitializeConfigPath();
            LoadConfig();
        }

        /// <summary>
        /// 初始化配置文件保存路径
        /// </summary>
        private void InitializeConfigPath()
        {
            try
            {
                // 获取配方路径
                string recipeDir = _commonbus?.CurrentRecipe?.GetRecipePath() ?? "D:\\Luster\\DigitalSetUp\\";

                // 构建配置目录路径
                string configDirectory = Path.Combine(recipeDir, CONFIG_DIR_NAME);

                // 确保目录存在
                if (!Directory.Exists(configDirectory))
                {
                    Directory.CreateDirectory(configDirectory);
                }

                // 保存完整配置文件路径
                _configSavePath = Path.Combine(configDirectory, CONFIG_FILE_NAME);

                // 设置默认Python脚本路径
                string defaultPythonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CPK.py");
                
                PythonScriptPath = defaultPythonPath;
            }
            catch (Exception ex)
            {
                _commonbus?.OnLog(new LogInfo()
                {
                    LogType = LogType.Error,
                    LogMessage = $"初始化配置目录失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 保存配置到文件
        /// </summary>
        private void SaveConfig()
        {
            try
            {
                if (string.IsNullOrEmpty(_configSavePath))
                {
                    InitializeConfigPath();
                    if (string.IsNullOrEmpty(_configSavePath)) return;
                }

                var config = new LADUpdateConfig
                {
                    ConfigFile1 = this.ConfigFile1,
                    ConfigFile2 = this.ConfigFile2,
                    PythonScriptPath = this.PythonScriptPath,
                    SelectedParameters = this.SelectedParameters?.ToList() ?? new List<string>(),
                    MappingItems = this.MappingItems?.ToList() ?? new List<MappingItem>()
                };

                string json = Newtonsoft.Json.JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(_configSavePath, json);
            }
            catch (Exception ex)
            {
                _commonbus?.OnLog(new LogInfo()
                {
                    LogType = LogType.Error,
                    LogMessage = $"保存LAD更新配置失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 从文件加载配置
        /// </summary>
        private void LoadConfig()
        {
            try
            {
                if (string.IsNullOrEmpty(_configSavePath))
                {
                    InitializeConfigPath();
                    if (string.IsNullOrEmpty(_configSavePath)) return;
                }

                if (!File.Exists(_configSavePath))
                {
                    return;
                }

                string json = File.ReadAllText(_configSavePath);
                var config = Newtonsoft.Json.JsonConvert.DeserializeObject<LADUpdateConfig>(json);

                if (config != null)
                {
                    ConfigFile1 = config.ConfigFile1;
                    ConfigFile2 = config.ConfigFile2;

                    // 加载Python脚本路径
                    if (!string.IsNullOrEmpty(config.PythonScriptPath))
                    {
                        PythonScriptPath = config.PythonScriptPath;
                    }

                    // 加载映射配置
                    if (config.MappingItems != null && config.MappingItems.Count > 0)
                    {
                        MappingItems.Clear();
                        foreach (var item in config.MappingItems)
                        {
                            MappingItems.Add(item);
                        }
                    }

                    // 如果有选中的参数，解析文件后应用
                    if (!string.IsNullOrEmpty(ConfigFile1))
                    {
                        ParseCPKFile(ConfigFile1);

                        // 应用选中的参数
                        if (config.SelectedParameters != null && config.SelectedParameters.Count > 0)
                        {
                            SelectedParameters = new ObservableCollection<string>(config.SelectedParameters);

                            // 根据选中的参数过滤显示的图表
                            if (SelectedParameters.Count > 0 && AllChartItems != null)
                            {
                                var filteredItems = AllChartItems.Where(x => SelectedParameters.Contains(x.PositionName)).ToList();
                                AllChartItems = new ObservableCollection<ChartItemModel>(filteredItems);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _commonbus?.OnLog(new LogInfo()
                {
                    LogType = LogType.Error,
                    LogMessage = $"加载LAD更新配置失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 添加日志
        /// </summary>
        private void AddLog(string message)
        {
            LogText += $"[{DateTime.Now:HH:mm:ss}] {message}\n";
            _commonbus?.OnLog(new LogInfo()
            {
                LogType = LogType.Info,
                LogMessage = message
            });
        }

        /// <summary>
        /// 执行Python脚本
        /// </summary>
        private async Task ExecutePythonScriptAsync()
        {
            try
            {
                AddLog("==== 开始构建测试流程 ====");

                // 构建映射字典
                var dict = new Dictionary<string, object>();
                foreach (var item in MappingItems)
                {
                    if (!string.IsNullOrWhiteSpace(item.TxtKey) && !string.IsNullOrWhiteSpace(item.ExcelKey))
                    {
                        dict[item.TxtKey] = new
                        {
                            col_name = item.ExcelKey,
                            start_row = item.StartRow,
                            max_row = item.MaxRow,
                            min_row = item.MinRow
                        };
                    }
                }

                if (dict.Count == 0)
                {
                    AddLog("未配置至少一项有效的映射字典！");
                    return;
                }

                // 获取执行目录
                string exeDir = AppDomain.CurrentDomain.BaseDirectory;
                string pyScript = PythonScriptPath;
                string mapJson = Path.Combine(exeDir, "temp_mapping.json");

                // 检查Python脚本是否存在
                if (!File.Exists(pyScript))
                {
                    AddLog($"找不到Python脚本: {pyScript}");
                    MessageBox.Show($"找不到Python脚本文件:\n{pyScript}\n\n请检查配置！", "脚本不存在", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // 序列化映射配置
                string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(dict, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(mapJson, jsonString, new System.Text.UTF8Encoding(false));
                AddLog($"已序列化 {dict.Count} 项映射至 {mapJson}");

                // 构建命令行参数
                string args = $"\"{pyScript}\" --template \"{ConfigFile2}\" --data_file \"{ConfigFile1}\" --mapping_file \"{mapJson}\"";
                AddLog("> python " + args);

                // 执行Python脚本
                var processInfo = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = System.Text.Encoding.UTF8,
                    StandardErrorEncoding = System.Text.Encoding.UTF8
                };
                processInfo.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";

                using (var process = new Process())
                {
                    process.StartInfo = processInfo;
                    process.Start();

                    string output = await process.StandardOutput.ReadToEndAsync();
                    string error = await process.StandardError.ReadToEndAsync();
                    process.WaitForExit();

                    if (!string.IsNullOrWhiteSpace(error))
                    {
                        AddLog($"[底层异常(stderr)] {error.Trim()}");
                    }

                    // 处理输出
                    string[] lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    string lastJsonLine = null;

                    foreach (string line in lines)
                    {
                        if (line.Trim().StartsWith("{") && line.Trim().EndsWith("}"))
                        {
                            lastJsonLine = line.Trim();
                        }
                        else
                        {
                            AddLog($"[脚本打印] {line}");
                        }
                    }

                    // 处理JSON返回结果
                    if (lastJsonLine != null)
                    {
                        AddLog($"\n[截获反馈] {lastJsonLine}");
                        try
                        {
                            using (JsonDocument doc = JsonDocument.Parse(lastJsonLine))
                            {
                                int code = doc.RootElement.GetProperty("code").GetInt32();
                                string msg = doc.RootElement.GetProperty("message").GetString();

                                if (code == 0)
                                {
                                    PageStatusService.Instance.UpdateStatus("LADUpload", "OK");
                                    AddLog($">>> {msg} (SUCCESS)");
                                    MessageBox.Show(msg, "SUCCESS!", MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                                else
                                {
                                    AddLog($">>> 打断 [{code}] - {msg}");
                                    MessageBox.Show($"服务报错 [{code}]\n" + msg, "FAILED!", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            AddLog($"解析 JSON 包失败: {ex.Message}");
                        }
                    }
                    else
                    {
                        AddLog($"× 脚本执行完毕，未捕获 JSON 返回。请检查 Python 脚本输出。");
                    }
                }
            }
            catch (Exception ex)
            {
                AddLog($"[严重故障] 执行时发生未知错误：{ex.Message}");
            }
        }

        /// <summary>
        /// 解析CPK文件，获取所有@pdata@格式的参数数据
        /// </summary>
        private void ParseCPKFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    _commonbus.OnLog(new LogInfo()
                    {
                        LogType = LogType.Warning,
                        LogMessage = $"CPK文件不存在：{filePath}"
                    });
                    return;
                }

                string content = File.ReadAllText(filePath);

                // 使用正则表达式匹配所有 @pdata@ 格式的数据
                // 格式: @pdata@参数名@值@最小值@最大值@单位
                string pattern = @"@pdata@([^@]+)@([^@]+)@([^@]*)@([^@]*)@([^@]*)";
                var matches = Regex.Matches(content, pattern);

                var foundParameters = new ObservableCollection<string>();
                var cpkDataList = new List<CPKDataModel>();

                // 按参数名分组收集数据
                var parameterDataDict = new Dictionary<string, List<double>>();
                var parameterMinDict = new Dictionary<string, double>();
                var parameterMaxDict = new Dictionary<string, double>();

                foreach (Match match in matches)
                {
                    if (match.Success)
                    {
                        string paramName = match.Groups[1].Value;
                        string valueStr = match.Groups[2].Value;
                        string minStr = match.Groups[3].Value;
                        string maxStr = match.Groups[4].Value;

                        double value;
                        double minValue = 0;
                        double maxValue = 0;

                        if (double.TryParse(valueStr, out value))
                        {
                            if (!parameterDataDict.ContainsKey(paramName))
                            {
                                parameterDataDict[paramName] = new List<double>();
                            }
                            parameterDataDict[paramName].Add(value);

                            // 记录Min和Max（取最后一次出现的值）
                            if (!string.IsNullOrEmpty(minStr) && double.TryParse(minStr, out minValue))
                            {
                                parameterMinDict[paramName] = minValue;
                            }
                            if (!string.IsNullOrEmpty(maxStr) && double.TryParse(maxStr, out maxValue))
                            {
                                parameterMaxDict[paramName] = maxValue;
                            }
                        }
                    }
                }

                // 为每个参数计算CPK值
                string[] colors = { "#2196F3", "#4CAF50", "#FF9800", "#9C27B0", "#F44336", "#00BCD4", "#FF5722", "#8BC34A", "#E91E63", "#3F51B5", "#009688", "#FFC107" };
                int colorIndex = 0;

                foreach (var kvp in parameterDataDict)
                {
                    string paramName = kvp.Key;
                    List<double> values = kvp.Value;

                    // 跳过非数值参数（如Operator_ID, Mode等）
                    if (paramName.Contains("ID") || paramName.Contains("Mode") || paramName.Contains("Priority"))
                        continue;

                    // 计算均值和标准差
                    double mean = values.Average();
                    double sigma = CalculateStdDev(values);

                    // 获取Min和Max
                    double min = parameterMinDict.ContainsKey(paramName) ? parameterMinDict[paramName] : 0;
                    double max = parameterMaxDict.ContainsKey(paramName) ? parameterMaxDict[paramName] : 100;

                    // 计算Target（Min和Max的中点）
                    double target = (min + max) / 2;

                    // 计算Ca
                    double halfRange = (max - min) / 2;
                    double ca = halfRange > 0 ? Math.Abs((mean - target) / halfRange) : 0;

                    // 计算Cp
                    double cp = sigma > 0 ? (max - min) / (6 * sigma) : 0;

                    // 计算Cpk
                    double cpk = (1 - ca) * cp;

                    // 创建CPK数据模型
                    var cpkData = new CPKDataModel
                    {
                        ParameterName = paramName,
                        Mean = mean,
                        Sigma = sigma,
                        MinValue = min,
                        MaxValue = max,
                        TargetValue = target,
                        Ca = ca,
                        Cp = cp,
                        Cpk = cpk,
                        DataValues = values
                    };

                    cpkDataList.Add(cpkData);

                    // 添加到参数列表
                    if (!foundParameters.Contains(paramName))
                    {
                        foundParameters.Add(paramName);
                    }

                    // 创建图表项
                    var chartItem = new ChartItemModel
                    {
                        PositionName = paramName,
                        PositionColor = colors[colorIndex % colors.Length],
                        CPKValue = cpk,
                        MaxValue = max,
                        TargetValue = target,
                        MinValue = min,
                        ChartSeriesCollection = GenerateCPKChartData(values, min, max, target)
                    };
                    AllChartItems.Add(chartItem);

                    colorIndex++;
                }

                // 按CPK值排序
                cpkDataList = cpkDataList.OrderBy(x => x.Cpk).ToList();

                AllCPKData = new ObservableCollection<CPKDataModel>(cpkDataList);
                CurrentDisplayData = new ObservableCollection<CPKDataModel>(cpkDataList);
                ParameterList = foundParameters;
            }
            catch (Exception ex)
            {
                _commonbus.OnLog(new LogInfo()
                {
                    LogType = LogType.Error,
                    LogMessage = $"解析CPK文件失败：{ex.Message}"
                });
                ParameterList = new ObservableCollection<string>();
                AllCPKData = new ObservableCollection<CPKDataModel>();
                AllChartItems = new ObservableCollection<ChartItemModel>();
            }
        }

        /// <summary>
        /// 计算标准差
        /// </summary>
        private double CalculateStdDev(List<double> values)
        {
            if (values == null || values.Count < 2)
                return 0;

            double mean = values.Average();
            double sumOfSquares = values.Sum(v => Math.Pow(v - mean, 2));
            return Math.Sqrt(sumOfSquares / (values.Count - 1));
        }

        /// <summary>
        /// 生成CPK测试图表数据（包含正态分布曲线）
        /// </summary>
        private SeriesCollection GenerateCPKChartData(List<double> values, double specMin, double specMax, double target)
        {
            var seriesCollection = new SeriesCollection();

            // 数据有效性校验
            if (values == null || values.Count == 0)
            {
                _commonbus.OnLog(new LogInfo()
                {
                    LogType = LogType.Warning,
                    LogMessage = "数据为空，无法生成图表"
                });
                return seriesCollection;
            }

            int totalSamples = values.Count;
            double dataMean = values.Average();
            double dataSigma = CalculateStdDev(values);
            double lsl = specMin;
            double usl = specMax;
            double specRange = usl - lsl;

            double chartMin = lsl;
            double chartMax = usl;
            double chartRange = chartMax - chartMin;

            // 异常处理：规格范围为0时的兜底
            if (Math.Abs(chartRange) < 0.001)
            {
                chartMin = dataMean - 0.1;
                chartMax = dataMean + 0.1;
                chartRange = 0.2;
            }

            int binCount = 15;
            double binWidth = chartRange / binCount;
            var frequencyValues = new ChartValues<double>();
            var binCenters = new List<double>();

            for (int i = 0; i < binCount; i++)
            {
                double binStart = chartMin + i * binWidth;
                double binEnd = binStart + binWidth;
                binCenters.Add(binStart + binWidth / 2);

                int sampleCountInBin = values.Count(v => v >= binStart && v < binEnd);
                frequencyValues.Add(sampleCountInBin);
            }

            seriesCollection.Add(new ColumnSeries
            {
                Title = "Frequency",
                Values = frequencyValues,
                Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(76, 175, 80)),
                Stroke = System.Windows.Media.Brushes.Black,
                StrokeThickness = 1,
                MaxColumnWidth = 15,
                DataLabels = false,
                ColumnPadding = 0.1
            });

            var normalCurveValues = new ChartValues<double>();
            foreach (double center in binCenters)
            {
                double probabilityDensity = (1 / (dataSigma * Math.Sqrt(2 * Math.PI)))
                                          * Math.Exp(-Math.Pow(center - dataMean, 2) / (2 * dataSigma * dataSigma));

                double curveValue = probabilityDensity * totalSamples * binWidth;
                normalCurveValues.Add(curveValue);
            }

            seriesCollection.Add(new LineSeries
            {
                Title = "Normal Distribution",
                Values = normalCurveValues,
                PointGeometry = null,
                LineSmoothness = 0.8,
                StrokeThickness = 2,
                Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(76, 175, 80)),
                Fill = System.Windows.Media.Brushes.Transparent
            });

            return seriesCollection;
        }

        /// <summary>
        /// 解析临时CPK文件
        /// </summary>
        private void ParseTempCPKFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    TempParameterList = new ObservableCollection<string>();
                    return;
                }

                string content = File.ReadAllText(filePath);

                // 使用正则表达式匹配所有 @pdata@ 格式的参数名
                string pattern = @"@pdata@([^@]+)@";
                var matches = Regex.Matches(content, pattern);

                var foundParameters = new ObservableCollection<string>();

                foreach (Match match in matches)
                {
                    if (match.Success)
                    {
                        string paramName = match.Groups[1].Value;
                        if (!foundParameters.Contains(paramName))
                        {
                            foundParameters.Add(paramName);
                        }
                    }
                }

                TempParameterList = foundParameters;
                TempSelectedParameters.Clear();
            }
            catch (Exception ex)
            {
                _commonbus.OnLog(new LogInfo()
                {
                    LogType = LogType.Error,
                    LogMessage = $"解析CPK文件失败：{ex.Message}"
                });
                TempParameterList = new ObservableCollection<string>();
            }
        }

        private void OnBrowseFile1()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "配置文件|*.txt;*.json|所有文件|*.*",
                Title = "选择CPK文件"
            };

            if (dialog.ShowDialog() == true)
            {
                ConfigFile1 = dialog.FileName;
                ParseCPKFile(ConfigFile1);
            }
        }

        private void OnBrowseFile2()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Excel文件|*.xlsx;*.xls|所有文件|*.*",
                Title = "选择Busop模板文件"
            };

            if (dialog.ShowDialog() == true)
            {
                ConfigFile2 = dialog.FileName;
            }
        }

        private void OnBrowseTempPythonScript()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Python脚本|*.py|所有文件|*.*",
                Title = "选择Python脚本文件"
            };

            if (dialog.ShowDialog() == true)
            {
                TempPythonScriptPath = dialog.FileName;
            }
        }

        private void OnConfig()
        {
            TempConfigFile1 = ConfigFile1;
            TempConfigFile2 = ConfigFile2;
            TempPythonScriptPath = PythonScriptPath;
            TempParameterList = new ObservableCollection<string>(ParameterList);
            TempSelectedParameters = new ObservableCollection<string>(SelectedParameters);

            var dialog = new Luster.Motion.DigitalSetup.Views.ConfigDialog();
            dialog.DataContext = this;
            dialog.Owner = Application.Current.MainWindow;
            dialog.ShowDialog();
        }

        private void OnBrowseTempFile1()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "配置文件|*.txt;*.json|所有文件|*.*",
                Title = "选择CPK文件"
            };

            if (dialog.ShowDialog() == true)
            {
                TempConfigFile1 = dialog.FileName;
                ParseTempCPKFile(TempConfigFile1);
            }
        }

        private void OnBrowseTempFile2()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Excel文件|*.xlsx;*.xls|所有文件|*.*",
                Title = "选择Busop模板文件"
            };

            if (dialog.ShowDialog() == true)
            {
                TempConfigFile2 = dialog.FileName;
            }
        }

        private void OnConfirm()
        {
            AllChartItems.Clear();
            AllCPKData.Clear();
            SeriesCollection?.Clear();
            ConfigFile1 = TempConfigFile1;
            ConfigFile2 = TempConfigFile2;
            PythonScriptPath = TempPythonScriptPath;

            // 重新解析CPK文件
            if (!string.IsNullOrEmpty(ConfigFile1))
            {
                ParseCPKFile(ConfigFile1);
            }

            if (TempSelectedParameters != null)
            {
                SelectedParameters = new ObservableCollection<string>(TempSelectedParameters);

                // 根据选中的参数过滤显示的图表
                if (SelectedParameters.Count > 0)
                {
                    var filteredItems = AllChartItems.Where(x => SelectedParameters.Contains(x.PositionName)).ToList();
                    AllChartItems = new ObservableCollection<ChartItemModel>(filteredItems);
                }
            }

            // 保存配置到文件
            SaveConfig();

            var dialog = Application.Current.Windows.OfType<Luster.Motion.DigitalSetup.Views.ConfigDialog>().FirstOrDefault();
            dialog?.Close();
        }

        private void OnCancel()
        {
            var dialog = Application.Current.Windows.OfType<Luster.Motion.DigitalSetup.Views.ConfigDialog>().FirstOrDefault();
            dialog?.Close();
        }

        private async void OnRun()
        {
            try
            {
                if (string.IsNullOrEmpty(ConfigFile1) || string.IsNullOrEmpty(ConfigFile2))
                {
                    AddLog("请先选择CPK文件和Busop模板文件！");
                    MessageBox.Show("请先选择CPK文件和Busop模板文件！", "配置缺失", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(PythonScriptPath))
                {
                    AddLog("请先配置Python脚本路径！");
                    MessageBox.Show("请先配置Python脚本路径！", "配置缺失", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SelectedParameters == null || SelectedParameters.Count == 0)
                {
                    AddLog("请至少选择一个检测位置！");
                    MessageBox.Show("请至少选择一个检测位置！", "配置缺失", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 运行前保存配置
                SaveConfig();

                // 清空日志
                LogText = "";
                AddLog($"开始运行CPK测试，已选择 {SelectedParameters.Count} 个检测位置");
                AddLog($"使用Python脚本: {PythonScriptPath}");

                ProgressValue = 0;

                // 执行Python脚本
                await ExecutePythonScriptAsync();

                ProgressValue = 100;
                AddLog("CPK测试运行完成");

                ((DelegateCommand)StopCommand).RaiseCanExecuteChanged();
                ((DelegateCommand)RunCommand).RaiseCanExecuteChanged();
            }
            catch (Exception ex)
            {
                AddLog($"运行失败：{ex.Message}");
                _commonbus.OnLog(new LogInfo()
                {
                    LogType = LogType.Error,
                    LogMessage = $"运行失败：{ex.Message}"
                });
            }
        }

        private bool CanRun() => true;

        private void OnStop()
        {
            try
            {
                ProgressValue = 0;

                ((DelegateCommand)StopCommand).RaiseCanExecuteChanged();
                ((DelegateCommand)RunCommand).RaiseCanExecuteChanged();
            }
            catch (Exception ex)
            {
                _commonbus.OnLog(new LogInfo()
                {
                    LogType = LogType.Error,
                    LogMessage = $"停止失败：{ex.Message}"
                });
            }
        }

        private bool CanStop() => ProgressValue > 0 && ProgressValue < 100;

        public override void OnEnd()
        {
            ProgressValue = 0;
            base.OnEnd();
        }

        public override async void OnOneKeyCheck(object obj)
        {
            base.OnOneKeyCheck(obj);

            try
            {
                ProgressValue = 0;

                if (_deviceEngine.GetMachineStatus() == EngineStatus.Ready)
                {
                    var stations = _mController.MotionEngine.GetStations();
                    var stat = stations.FirstOrDefault(s => s.Alias == "CPK测试");

                    if (stat != null)
                    {
                        flowBus.OnRunOne(stat.ID);

                        await Task.Run(async () =>
                        {
                            while (stat.Status != RunStatus.Success)
                            {
                                await Task.Delay(200);
                            }
                        }, _cts.Token);

                        ProgressValue = 100;
                    }
                    else
                    {
                        throw new FriendlyException("未找到Alias为'CPK测试'的工站");
                    }
                }
                else
                {
                    throw new FriendlyException("回零完成后方可运行测试流程");
                }
            }
            catch (Exception ex)
            {
                _commonbus.OnLog(new LogInfo()
                {
                    LogType = LogType.Info,
                    LogMessage = $"获取CPK测试数据失败：{ex.Message}"
                });
                throw;
            }
        }

        private void OnUpdateItems()
        {
            try
            {

            }
            catch (Exception ex)
            {
                _commonbus.OnLog(new LogInfo()
                {
                    LogType = LogType.Info,
                    LogMessage = $"更新列表失败：{ex.Message}"
                });
            }
        }

        private void OnSaveParameters()
        {
            try
            {

            }
            catch (Exception ex)
            {
                _commonbus.OnLog(new LogInfo()
                {
                    LogType = LogType.Info,
                    LogMessage = $"保存参数失败：{ex.Message}"
                });
            }
        }
    }

    /// <summary>
    /// 图表项数据模型
    /// </summary>
    public class ChartItemModel : BindableBase
    {
        private string _positionName;
        public string PositionName
        {
            get => _positionName;
            set => SetProperty(ref _positionName, value);
        }

        private double _cpkValue;
        public double CPKValue
        {
            get => _cpkValue;
            set => SetProperty(ref _cpkValue, value);
        }

        private double _maxValue;
        public double MaxValue
        {
            get => _maxValue;
            set => SetProperty(ref _maxValue, value);
        }

        private double _targetValue;
        public double TargetValue
        {
            get => _targetValue;
            set => SetProperty(ref _targetValue, value);
        }

        private double _minValue;
        public double MinValue
        {
            get => _minValue;
            set => SetProperty(ref _minValue, value);
        }

        private string _positionColor;
        public string PositionColor
        {
            get => _positionColor;
            set => SetProperty(ref _positionColor, value);
        }

        private SeriesCollection _chartSeriesCollection;
        public SeriesCollection ChartSeriesCollection
        {
            get => _chartSeriesCollection;
            set => SetProperty(ref _chartSeriesCollection, value);
        }
    }

    /// <summary>
    /// CPK测试表格数据模型
    /// </summary>
    public class AssTbCPKTest : AssTb
    {
    }

    /// <summary>
    /// LAD更新配置类
    /// </summary>
    public class LADUpdateConfig
    {
        /// <summary>
        /// CPK文件路径
        /// </summary>
        public string ConfigFile1 { get; set; }

        /// <summary>
        /// BuSop位置文件路径
        /// </summary>
        public string ConfigFile2 { get; set; }

        /// <summary>
        /// Python脚本路径
        /// </summary>
        public string PythonScriptPath { get; set; }

        /// <summary>
        /// 选中的参数列表
        /// </summary>
        public List<string> SelectedParameters { get; set; }

        /// <summary>
        /// 映射配置列表
        /// </summary>
        public List<MappingItem> MappingItems { get; set; }
    }
}