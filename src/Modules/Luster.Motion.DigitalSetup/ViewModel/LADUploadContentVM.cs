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
using Luster.Motion.DigitalSetup.Services;
using Luster.Motion.DigitalSetup.ViewModel;
using Luster.Motion.EditorUI;
using Luster.Motion.TaskFlow.Engine;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Motion;
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
using System.Windows.Controls;
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
        private string _pythonExePath;

        // 所有CPK数据项集合
        private ObservableCollection<CPKDataModel> _allCPKData;

        // 当前显示的CPK数据
        private ObservableCollection<CPKDataModel> _currentDisplayData;

        // 所有图表数据项集合
        private ObservableCollection<ChartItemModel> _allChartItems = new ObservableCollection<ChartItemModel>();

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
        private string _tempPythonExePath;
        private ObservableCollection<string> _tempParameterList;
        private ObservableCollection<string> _tempSelectedParameters;

        // 映射配置相关
        private ObservableCollection<MappingItem> _mappingItems;
        private MappingItem _selectedMappingItem;
        private string _logText;

        // 配置保存相关
        private const string CONFIG_DIR_NAME = "DigitalSetUpDataValidation";
        private const string CONFIG_FILE_NAME = "LADUpdateConfig.json";
        private const string STATIONS_LIST_FILE_NAME = "LADStations.json"; // 工站列表配置文件
        private string _configDirectory; // 配置目录（不包含文件名）
        public ICommand ClearDataCommand { get; private set; }
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
        public ICommand BrowseTempPythonExeCommand { get; private set; }
        public ICommand ConfirmCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        // 映射配置命令
        public ICommand AddMappingItemCommand { get; private set; }
        public ICommand DeleteMappingItemCommand { get; private set; }

        // 工站管理命令
        public ICommand AddStationCommand { get; private set; }
        public ICommand DeleteStationCommand { get; private set; }
        public ICommand EditStationCommand { get; private set; }

        // 多工站配置相关
        private ObservableCollection<LADStationConfig> _ladStations;
        private LADStationConfig _selectedLADStation;

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

        public string PythonExePath
        {
            get => _pythonExePath;
            set => SetProperty(ref _pythonExePath, value);
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

        public string TempPythonExePath
        {
            get => _tempPythonExePath;
            set => SetProperty(ref _tempPythonExePath, value);
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

        public MappingItem SelectedMappingItem
        {
            get => _selectedMappingItem;
            set => SetProperty(ref _selectedMappingItem, value);
        }

        public string LogText
        {
            get => _logText;
            set => SetProperty(ref _logText, value);
        }

        /// <summary>
        /// LAD 工站配置集合
        /// </summary>
        public ObservableCollection<LADStationConfig> LADStations
        {
            get => _ladStations;
            set => SetProperty(ref _ladStations, value);
        }

        /// <summary>
        /// 当前选中的 LAD 工站
        /// </summary>
        public LADStationConfig SelectedLADStation
        {
            get => _selectedLADStation;
            set
            {
                if (SetProperty(ref _selectedLADStation, value))
                {
                    // 工站切换时保存当前配置并加载新工站配置
                    OnStationChanged(value);

                    // 更新删除命令的可用状态
                    ((DelegateCommand)DeleteStationCommand)?.RaiseCanExecuteChanged();
                }
            }
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
                                  CSVHelper cSVHelper,IDialogService dialogService, CheckStatusService checkStatusService)
                                  : base(repository, regionManager, commonBus, cSVHelper, _flowBus, dialogService, checkStatusService)
        {
            _parentRegionName = "LADUploadContent";

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

            // 注册子页面到DigitalAssPageModel，用于状态聚合
            DigitalAssPageModel.RegisterSubPages("LADUploadContent", Pages);

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
            AddMappingItemCommand = new DelegateCommand(OnAddMappingItem);
            DeleteMappingItemCommand = new DelegateCommand(OnDeleteMappingItem);

            // 工站管理命令
            AddStationCommand = new DelegateCommand(OnAddStation);
            DeleteStationCommand = new DelegateCommand(OnDeleteStation, CanDeleteStation);
            EditStationCommand = new DelegateCommand(OnEditStation);

            BrowseTempFile1Command = new DelegateCommand(OnBrowseTempFile1);
            BrowseTempFile2Command = new DelegateCommand(OnBrowseTempFile2);
            BrowseTempPythonScriptCommand = new DelegateCommand(OnBrowseTempPythonScript);
            BrowseTempPythonExeCommand = new DelegateCommand(OnBrowseTempPythonExe);
            ConfirmCommand = new DelegateCommand(OnConfirm);
            CancelCommand = new DelegateCommand(OnCancel);

            ConfigKey = "CPKTestConfig";

            // 初始化多工站配置
            LADStations = new ObservableCollection<LADStationConfig>();
            InitializeLADStations();

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
            ClearDataCommand = new DelegateCommand(OnClearData);
            LoadConfig();

            // 延迟加载点检状态，确保 UI 绑定已建立
            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                LoadCheckStatusForAllPages();
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        /// <summary>
        /// 加载所有子页面的历史点检状态
        /// </summary>
        private void LoadCheckStatusForAllPages()
        {
            if (_checkStatusService == null || Pages == null)
                return;

            try
            {
                foreach (var page in Pages)
                {
                    if (page != null)
                    {
                        page.ParentRegion = "LADUploadContent";
                        var record = _checkStatusService.GetRecord(page.PageKey);
                        if (record != null)
                        {
                            page.CheckStatus = record.Status;
                        }
                        else
                        {
                            page.CheckStatus = CheckStatus.NotChecked;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载点检状态失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 初始化 LAD 工站列表，优先从保存的配置加载，否则从 StationConfigs 同步
        /// </summary>
        private void InitializeLADStations()
        {
            try
            {
                LADStations.Clear();

                // 检查是否有旧版配置需要迁移
                MigrateOldConfig();

                // 尝试从配置文件加载保存的工站列表
                string stationsListPath = Path.Combine(_configDirectory, STATIONS_LIST_FILE_NAME);
                if (File.Exists(stationsListPath))
                {
                    try
                    {
                        string json = File.ReadAllText(stationsListPath);
                        var savedStations = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(json);
                        if (savedStations != null && savedStations.Count > 0)
                        {
                            foreach (var stationName in savedStations)
                            {
                                var existingStation = LADStations.FirstOrDefault(s => s.StationName == stationName);
                                if (existingStation == null)
                                {
                                    var newStation = new LADStationConfig
                                    {
                                        StationName = stationName,
                                        CheckStatus = CheckStatus.NotChecked
                                    };
                                    LoadStationConfig(newStation);
                                    LADStations.Add(newStation);
                                }
                            }
                            AddLog($"[调试] 从配置文件加载了 {LADStations.Count} 个工站");
                        }
                    }
                    catch (Exception ex)
                    {
                        AddLog($"加载工站列表配置失败: {ex.Message}");
                    }
                }

                // 如果配置文件不存在或加载失败，从 StationConfigs 同步
                if (LADStations.Count == 0 && StationConfigs != null && StationConfigs.Count > 0)
                {
                    foreach (var stationConfig in StationConfigs)
                    {
                        if (stationConfig != null && !string.IsNullOrEmpty(stationConfig.Name))
                        {
                            var existingStation = LADStations.FirstOrDefault(s => s.StationName == stationConfig.Name);
                            if (existingStation == null)
                            {
                                var newStation = new LADStationConfig
                                {
                                    StationName = stationConfig.Name,
                                    CheckStatus = CheckStatus.NotChecked
                                };
                                LoadStationConfig(newStation);
                                LADStations.Add(newStation);
                            }
                        }
                    }
                }

                // 如果没有工站，创建默认工站
                if (LADStations.Count == 0)
                {
                    var defaultStation = LADStationConfig.CreateDefault("工站1");
                    LoadStationConfig(defaultStation);
                    LADStations.Add(defaultStation);
                }

                // 默认选中第一个工站
                if (LADStations.Count > 0)
                {
                    SelectedLADStation = LADStations[0];
                    LoadConfigFromObject(SelectedLADStation.LadConfig);
                }
            }
            catch (Exception ex)
            {
                _commonbus?.OnLog(new LogInfo()
                {
                    LogType = LogType.Error,
                    LogMessage = $"初始化LAD工站列表失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 工站切换处理
        /// </summary>
        private void OnStationChanged(LADStationConfig newStation)
        {
            if (newStation == null) return;

            try
            {
                // 保存当前工站配置（如果之前有选中的工站）
                if (SelectedLADStation != null && SelectedLADStation != newStation)
                {
                    SaveStationConfig(SelectedLADStation);
                }

                // 加载新工站配置到界面
                if (newStation.LadConfig != null)
                {
                    LoadConfigFromObject(newStation.LadConfig);
                }
                SetStationGlobalVariable(newStation.StationName);
            }
            catch (Exception ex)
            {
                _commonbus?.OnLog(new LogInfo()
                {
                    LogType = LogType.Error,
                    LogMessage = $"切换工站失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 添加新工站
        /// </summary>
        private void OnAddStation()
        {
            try
            {
                string newStationName = $"工站{LADStations.Count + 1}";

                // 检查名称是否重复
                while (LADStations.Any(s => s.StationName == newStationName))
                {
                    int num = 2;
                    newStationName = $"工站{LADStations.Count + num}";
                    num++;
                }

                var newStation = LADStationConfig.CreateDefault(newStationName);
                LADStations.Add(newStation);
                SelectedLADStation = newStation;

                AddLog($"已添加新工站: {newStationName}");

                // 保存当前工站列表到配置文件
                SaveStationsList();
            }
            catch (Exception ex)
            {
                _commonbus?.OnLog(new LogInfo()
                {
                    LogType = LogType.Error,
                    LogMessage = $"添加工站失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 删除工站
        /// </summary>
        private void OnDeleteStation()
        {
            try
            {
                if (SelectedLADStation == null)
                {
                    MessageBox.Show("请先选择要删除的工站！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (LADStations.Count <= 1)
                {
                    MessageBox.Show("至少需要保留一个工站！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show(
                    $"确定要删除工站 \"{SelectedLADStation.StationName}\" 吗？\n\n该工站的配置文件和点检状态也将被删除。",
                    "确认删除",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                {
                    return;
                }

                string stationName = SelectedLADStation.StationName;

                // 删除配置文件
                try
                {
                    if (File.Exists(SelectedLADStation.ConfigFilePath))
                    {
                        File.Delete(SelectedLADStation.ConfigFilePath);
                    }
                }
                catch (Exception ex)
                {
                    AddLog($"删除配置文件失败: {ex.Message}");
                }

                // 删除工站
                int currentIndex = LADStations.IndexOf(SelectedLADStation);
                LADStations.Remove(SelectedLADStation);

                // 选中下一个工站
                if (currentIndex >= LADStations.Count)
                {
                    currentIndex = LADStations.Count - 1;
                }
                SelectedLADStation = LADStations[currentIndex];

                AddLog($"已删除工站: {stationName}");

                // 保存当前工站列表到配置文件
                SaveStationsList();
            }
            catch (Exception ex)
            {
                _commonbus?.OnLog(new LogInfo()
                {
                    LogType = LogType.Error,
                    LogMessage = $"删除工站失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 是否可以删除工站
        /// </summary>
        private bool CanDeleteStation()
        {
            return LADStations != null && LADStations.Count > 1 && SelectedLADStation != null;
        }

        /// <summary>
        /// 编辑工站名称
        /// </summary>
        private void OnEditStation()
        {
            try
            {
                if (SelectedLADStation == null)
                {
                    MessageBox.Show("请先选择要编辑的工站！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 使用 InputDialog 显示输入对话框
                string newName = Views.Dialogs.InputDialog.ShowDialog(
                    "编辑工站名称",
                    "请输入新的工站名称：",
                    SelectedLADStation.StationName,
                    Application.Current.MainWindow);

                if (!string.IsNullOrWhiteSpace(newName))
                {
                    newName = newName.Trim();

                    if (LADStations.Any(s => s.StationName == newName && s != SelectedLADStation))
                    {
                        MessageBox.Show($"工站名称 '{newName}' 已存在，请使用其他名称！",
                            "名称重复", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    string oldName = SelectedLADStation.StationName;
                    SelectedLADStation.StationName = newName;
                    RenameStationConfigFile(oldName, newName);
                    UpdateStationStatusKey(oldName, newName);
                    SaveStationsList();

                    AddLog($"工站名称已从 '{oldName}' 更改为 '{newName}'");
                    RaisePropertyChanged(nameof(LADStations));
                }
            }
            catch (Exception ex)
            {
                _commonbus?.OnLog(new LogInfo()
                {
                    LogType = LogType.Error,
                    LogMessage = $"编辑工站失败: {ex.Message}"
                });
                MessageBox.Show($"编辑工站失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 重命名工站配置文件
        /// </summary>
        private void RenameStationConfigFile(string oldStationName, string newStationName)
        {
            try
            {
                if (string.IsNullOrEmpty(_configDirectory))
                {
                    InitializeConfigPath();
                }

                string oldConfigPath = GetStationConfigPath(oldStationName);
                string newConfigPath = GetStationConfigPath(newStationName);

                if (File.Exists(oldConfigPath) && oldConfigPath != newConfigPath)
                {
                    // 如果新路径的文件已存在，先备份
                    if (File.Exists(newConfigPath))
                    {
                        string backupPath = newConfigPath + ".bak";
                        File.Copy(newConfigPath, backupPath, true);
                        File.Delete(newConfigPath);
                    }

                    File.Move(oldConfigPath, newConfigPath);
                    SelectedLADStation.ConfigFilePath = newConfigPath;
                    AddLog($"配置文件已重命名: {oldConfigPath} -> {newConfigPath}");
                }
            }
            catch (Exception ex)
            {
                AddLog($"重命名配置文件失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新工站状态键
        /// </summary>
        private void UpdateStationStatusKey(string oldStationName, string newStationName)
        {
            try
            {
                string oldKey = $"LADUpload_{oldStationName}";
                string newKey = $"LADUpload_{newStationName}";

                // 获取旧状态值
                var oldStatus = PageStatusService.Instance.GetStatus(oldKey);

                if (!string.IsNullOrEmpty(oldStatus))
                {
                    // 复制状态到新键
                    PageStatusService.Instance.UpdateStatus(newKey, oldStatus);
                    // 清除旧键
                    PageStatusService.Instance.UpdateStatus(oldKey, null);
                }
            }
            catch (Exception ex)
            {
                AddLog($"更新状态键失败: {ex.Message}");
            }
        }


        /// <summary>
        /// 获取全局模块
        /// </summary>
        private IMotionModule GetGlobal()
        {
            try
            {
                var globalId = Luster.TaskFlow.Motion.Logic.GlobalModule.GlobalID;
                return _mController?.MotionEngine?.Get(globalId);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 根据工站名称设置对应的全局变量为 true
        /// </summary>
        private void SetStationGlobalVariable(string stationName)
        {
            try
            {
                var globalModule = GetGlobal();
                if (globalModule?.Parameters == null) return;

                foreach (var item in globalModule.Parameters)
                {
                    if (item.Value == null || !item.Value.Visible) continue;

                    // 匹配全局变量名称
                    if (item.Value.Name == stationName || item.Value.CN == stationName)
                    {
                        if (item.Value.Value is bool)
                        {
                            item.Value.Value = true;
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _commonbus?.OnLog(new LogInfo()
                {
                    LogType = LogType.Error,
                    LogMessage = $"设置工站全局变量失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 迁移旧版配置到默认工站
        /// </summary>
        private void MigrateOldConfig()
        {
            try
            {
                if (string.IsNullOrEmpty(_configDirectory))
                {
                    InitializeConfigPath();
                }

                // 检查是否存在旧版配置文件（在根目录下，没有工站前缀）
                string oldConfigPath = Path.Combine(_configDirectory, CONFIG_FILE_NAME);

                if (!File.Exists(oldConfigPath))
                {
                    return;
                }

                // 读取旧配置
                string json = File.ReadAllText(oldConfigPath);
                var oldConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<LADUpdateConfig>(json);

                if (oldConfig != null)
                {
                    // 创建默认工站并导入配置
                    var defaultStation = LADStationConfig.CreateDefault("工站1");
                    defaultStation.LadConfig = oldConfig;
                    defaultStation.ConfigFilePath = GetStationConfigPath("工站1");

                    // 保存到新位置
                    SaveStationConfig(defaultStation);

                    // 备份旧配置文件
                    string backupPath = oldConfigPath + ".bak";
                    File.Move(oldConfigPath, backupPath);

                    AddLog("已迁移旧版配置到工站1");
                }
            }
            catch (Exception ex)
            {
                _commonbus?.OnLog(new LogInfo()
                {
                    LogType = LogType.Warning,
                    LogMessage = $"迁移旧版配置失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 刷新点检状态 - 每次页面激活时调用
        /// </summary>
        protected override void RefreshCheckStatus()
        {
            LoadCheckStatusForAllPages();
            LoadStationCheckStatus();
        }

        /// <summary>
        /// 加载所有工站的点检状态
        /// </summary>
        private void LoadStationCheckStatus()
        {
            if (LADStations == null) return;

            try
            {
                foreach (var station in LADStations)
                {
                    if (station != null)
                    {
                        // 从 PageStatusService 加载该工站的状态
                        var statusKey = $"LADUpload_{station.StationName}";
                        var statusString = PageStatusService.Instance.GetStatus(statusKey);

                        // 解析状态字符串
                        if (statusString == "OK")
                        {
                            station.CheckStatus = CheckStatus.CheckedOK;
                        }
                        else if (statusString == "NG")
                        {
                            station.CheckStatus = CheckStatus.CheckedFail;
                        }
                        else
                        {
                            station.CheckStatus = CheckStatus.NotChecked;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载工站点检状态失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 保存当前工站的点检状态
        /// </summary>
        private void SaveCurrentStationCheckStatus(CheckStatus status, string remark)
        {
            if (SelectedLADStation == null)
            {
                AddLog($"[调试] SaveCurrentStationCheckStatus: SelectedLADStation 为 null，无法保存状态");
                return;
            }

            try
            {
                // 更新当前工站的状态
                SelectedLADStation.CheckStatus = status;
                AddLog($"[调试] 已更新 SelectedLADStation.CheckStatus = {status}");

                // 保存到 PageStatusService
                string statusKey = $"LADUpload_{SelectedLADStation.StationName}";
                string statusValue = status == CheckStatus.CheckedOK ? "OK" :
                                    status == CheckStatus.CheckedFail ? "NG" : "NotChecked";
                PageStatusService.Instance.UpdateStatus(statusKey, statusValue);

                AddLog($"工站 {SelectedLADStation.StationName} 点检状态: {statusValue} - {remark}");
            }
            catch (Exception ex)
            {
                _commonbus?.OnLog(new LogInfo()
                {
                    LogType = LogType.Error,
                    LogMessage = $"保存工站点检状态失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 获取所有工站的聚合状态
        /// </summary>
        private CheckStatus GetAggregatedStationStatus()
        {
            if (LADStations == null || LADStations.Count == 0)
            {
                AddLog($"[调试] GetAggregatedStationStatus: LADStations 为空或数量为0");
                return CheckStatus.NotChecked;
            }

            // 调试日志：输出每个工站的状态
            foreach (var station in LADStations)
            {
                AddLog($"[调试] 工站 {station.StationName} 状态: {station.CheckStatus}, 是否为SelectedLADStation: {station == SelectedLADStation}");
            }

            // 聚合规则：任一 NG → NG，全部 OK → OK，有未检 → NotChecked
            bool hasNG = LADStations.Any(s => s.CheckStatus == CheckStatus.CheckedFail);
            if (hasNG) return CheckStatus.CheckedFail;

            bool allOK = LADStations.All(s => s.CheckStatus == CheckStatus.CheckedOK);
            if (allOK) return CheckStatus.CheckedOK;

            AddLog($"[调试] GetAggregatedStationStatus: 返回 NotChecked (hasNG={hasNG}, allOK={allOK})");
            return CheckStatus.NotChecked;
        }

        /// <summary>
        /// 重写基类方法，直接返回当前选中页面的点检状态
        /// 因为 LAD 点检不依赖 ItemModels，而是使用工站状态聚合
        /// </summary>
        protected override CheckStatus GetCurrentPageCheckStatus()
        {
            // 优先使用当前选中页面的状态
            if (SelectedReportPage != null)
            {
                AddLog($"[调试] GetCurrentPageCheckStatus: 返回 SelectedReportPage.CheckStatus = {SelectedReportPage.CheckStatus}");
                return SelectedReportPage.CheckStatus;
            }

            // 如果没有选中页面，返回工站聚合状态
            var stationStatus = GetAggregatedStationStatus();
            AddLog($"[调试] GetCurrentPageCheckStatus: 返回工站聚合状态 = {stationStatus}");
            return stationStatus;
        }

        /// <summary>
        /// 刷新 LAD 页面的状态（包括 CommonPageModel 和 DigitalAssPageModel）
        /// </summary>
        private void RefreshLADPageStatus()
        {
            try
            {
                // 1. 更新 CommonPageModel（二级页面）状态
                if (SelectedReportPage != null)
                {
                    // 使用工站聚合状态更新 CommonPageModel
                    var aggregatedStatus = GetAggregatedStationStatus();
                    SelectedReportPage.CheckStatus = aggregatedStatus;
                    AddLog($"[调试] RefreshLADPageStatus: 更新 CommonPageModel.CheckStatus = {aggregatedStatus}");
                }

                // 2. 更新 DigitalAssPageModel（一级页面）状态
                string pageName = DigitalAssPageModel.GetNameByRegion(_parentRegionName);
                if (!string.IsNullOrEmpty(pageName))
                {
                    // 从静态集合中查找对应的 DigitalAssageModel
                    var parentPage = DigitalAssPageModel.Pages?.FirstOrDefault(p => p.Name == pageName);
                    if (parentPage != null)
                    {
                        // 直接使用当前的 Pages 集合作为 SubPages
                        var subPages = Pages?.ToList();
                        if (subPages != null && subPages.Count > 0)
                        {
                            parentPage.SubPages = subPages;
                        }

                        // 调用 RefreshCheckStatus 触发聚合计算
                        parentPage.RefreshCheckStatus();
                        AddLog($"[调试] RefreshLADPageStatus: 更新 DigitalAssageModel [{pageName}].CheckStatus = {parentPage.CheckStatus}");
                    }
                }
            }
            catch (Exception ex)
            {
                AddLog($"刷新 LAD 页面状态失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 清除CPK文件数据
        /// </summary>
        private void OnClearData()
        {
            try
            {
                // 检查是否有选中的文件
                if (string.IsNullOrEmpty(TempConfigFile1))
                {
                    MessageBox.Show("请先选择要清除数据的CPK文件！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 检查文件是否存在
                if (!File.Exists(TempConfigFile1))
                {
                    MessageBox.Show($"文件不存在：\n{TempConfigFile1}", "文件不存在", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // 确认对话框
                var result = MessageBox.Show(
                    $"确定要清除文件内容吗？\n\n文件路径：{TempConfigFile1}\n\n注意：此操作不可恢复！",
                    "确认清除",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                {
                    return;
                }

                // 清空文件内容
                File.WriteAllText(TempConfigFile1, string.Empty);

                AddLog($"已清除CPK文件内容：{TempConfigFile1}");

                // 提示成功
                MessageBox.Show("数据清除成功！", "操作完成", MessageBoxButton.OK, MessageBoxImage.Information);

                // 触发属性变化通知
                RaisePropertyChanged(nameof(TempParameterList));
                RaisePropertyChanged(nameof(TempSelectedParameters));
            }
            catch (Exception ex)
            {
                AddLog($"清除数据失败：{ex.Message}");
                _commonbus?.OnLog(new LogInfo()
                {
                    LogType = LogType.Error,
                    LogMessage = $"清除CPK文件数据失败：{ex.Message}"
                });

                MessageBox.Show($"清除数据失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                _configDirectory = Path.Combine(recipeDir, CONFIG_DIR_NAME);

                // 确保目录存在
                if (!Directory.Exists(_configDirectory))
                {
                    Directory.CreateDirectory(_configDirectory);
                }

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
        /// 获取指定工站的配置文件路径
        /// </summary>
        private string GetStationConfigPath(string stationName)
        {
            if (string.IsNullOrEmpty(_configDirectory))
            {
                InitializeConfigPath();
            }
            return Path.Combine(_configDirectory, $"{stationName}_{CONFIG_FILE_NAME}");
        }

        /// <summary>
        /// 获取当前选中工站的配置文件路径
        /// </summary>
        private string GetCurrentStationConfigPath()
        {
            if (SelectedLADStation == null)
            {
                return Path.Combine(_configDirectory, $"Default_{CONFIG_FILE_NAME}");
            }
            return GetStationConfigPath(SelectedLADStation.StationName);
        }

        /// <summary>
        /// 保存配置到文件
        /// </summary>
        private void SaveConfig()
        {
            try
            {
                if (string.IsNullOrEmpty(_configDirectory))
                {
                    InitializeConfigPath();
                    if (string.IsNullOrEmpty(_configDirectory)) return;
                }

                string configPath = GetCurrentStationConfigPath();

                // 保存到当前选中工站的配置
                var config = new LADUpdateConfig
                {
                    ConfigFile1 = this.ConfigFile1,
                    ConfigFile2 = this.ConfigFile2,
                    PythonScriptPath = this.PythonScriptPath,
                    PythonExePath = this.PythonExePath,
                    SelectedParameters = this.SelectedParameters?.ToList() ?? new List<string>(),
                    MappingItems = this.MappingItems?.ToList() ?? new List<MappingItem>()
                };

                string json = Newtonsoft.Json.JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(configPath, json);

                // 同时更新 SelectedLADStation 的配置
                if (SelectedLADStation != null)
                {
                    SelectedLADStation.LadConfig = config;
                    SelectedLADStation.ConfigFilePath = configPath;
                }
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
        /// 保存指定工站的配置
        /// </summary>
        private void SaveStationConfig(LADStationConfig station)
        {
            if (station == null || station.LadConfig == null) return;

            try
            {
                string configPath = GetStationConfigPath(station.StationName);
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(station.LadConfig, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(configPath, json);
                station.ConfigFilePath = configPath;
            }
            catch (Exception ex)
            {
                _commonbus?.OnLog(new LogInfo()
                {
                    LogType = LogType.Error,
                    LogMessage = $"保存工站 {station.StationName} 配置失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 保存当前工站列表到配置文件
        /// </summary>
        private void SaveStationsList()
        {
            try
            {
                if (string.IsNullOrEmpty(_configDirectory))
                {
                    InitializeConfigPath();
                    if (string.IsNullOrEmpty(_configDirectory)) return;
                }

                // 提取所有工站名称
                var stationNames = LADStations.Select(s => s.StationName).ToList();

                // 保存到配置文件
                string stationsListPath = Path.Combine(_configDirectory, STATIONS_LIST_FILE_NAME);
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(stationNames, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(stationsListPath, json);

                AddLog($"[调试] 已保存工站列表: {string.Join(", ", stationNames)}");
            }
            catch (Exception ex)
            {
                _commonbus?.OnLog(new LogInfo()
                {
                    LogType = LogType.Error,
                    LogMessage = $"保存工站列表失败: {ex.Message}"
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
                if (string.IsNullOrEmpty(_configDirectory))
                {
                    InitializeConfigPath();
                    if (string.IsNullOrEmpty(_configDirectory)) return;
                }

                string configPath = GetCurrentStationConfigPath();

                if (!File.Exists(configPath))
                {
                    return;
                }

                string json = File.ReadAllText(configPath);
                var config = Newtonsoft.Json.JsonConvert.DeserializeObject<LADUpdateConfig>(json);

                if (config != null)
                {
                    LoadConfigFromObject(config);

                    // 更新 SelectedLADStation 的配置
                    if (SelectedLADStation != null)
                    {
                        SelectedLADStation.LadConfig = config;
                        SelectedLADStation.ConfigFilePath = configPath;
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
        /// 从配置对象加载配置到界面
        /// </summary>
        private void LoadConfigFromObject(LADUpdateConfig config)
        {
            if (config == null) return;

            ConfigFile1 = config.ConfigFile1;
            ConfigFile2 = config.ConfigFile2;

            // 加载Python脚本路径
            if (!string.IsNullOrEmpty(config.PythonScriptPath))
            {
                PythonScriptPath = config.PythonScriptPath;
            }

            // 加载Python.exe路径
            if (!string.IsNullOrEmpty(config.PythonExePath))
            {
                PythonExePath = config.PythonExePath;
            }

            // 加载映射配置
            if (config.MappingItems != null && config.MappingItems.Count > 0)
            {
                MappingItems?.Clear();
                MappingItems = new ObservableCollection<MappingItem>();
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

        /// <summary>
        /// 加载指定工站的配置
        /// </summary>
        private void LoadStationConfig(LADStationConfig station)
        {
            if (station == null) return;

            try
            {
                string configPath = GetStationConfigPath(station.StationName);

                if (!File.Exists(configPath))
                {
                    // 文件不存在，使用默认配置
                    station.LadConfig = new LADUpdateConfig
                    {
                        ConfigFile1 = "",
                        ConfigFile2 = "",
                        PythonScriptPath = PythonScriptPath,
                        PythonExePath = PythonExePath,
                        SelectedParameters = new List<string>(),
                        MappingItems = new List<MappingItem>
                        {
                            new MappingItem { TxtKey = "Install_Force", ExcelKey = "1# Paste Force", StartRow = "23", MaxRow = "18", MinRow = "20" },
                            new MappingItem { TxtKey = "Install_Gap_X", ExcelKey = "X1", StartRow = "23", MaxRow = "18", MinRow = "20" },
                            new MappingItem { TxtKey = "Install_Gap_Y", ExcelKey = "Y1", StartRow = "23", MaxRow = "18", MinRow = "20" },
                            new MappingItem { TxtKey = "Install_CC", ExcelKey = "1# CC ", StartRow = "23", MaxRow = "18", MinRow = "20" }
                        }
                    };
                    station.ConfigFilePath = configPath;
                    return;
                }

                string json = File.ReadAllText(configPath);
                var config = Newtonsoft.Json.JsonConvert.DeserializeObject<LADUpdateConfig>(json);

                if (config != null)
                {
                    station.LadConfig = config;
                    station.ConfigFilePath = configPath;
                }
            }
            catch (Exception ex)
            {
                _commonbus?.OnLog(new LogInfo()
                {
                    LogType = LogType.Error,
                    LogMessage = $"加载工站 {station.StationName} 配置失败: {ex.Message}"
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

                // 检查 Python.exe 路径是否配置
                if (string.IsNullOrEmpty(PythonExePath) || !File.Exists(PythonExePath))
                {
                    AddLog($"Python.exe 路径未配置或不存在: {PythonExePath ?? "未设置"}");
                    MessageBox.Show($"请先在配置中设置正确的 Python.exe 路径！\n\n当前路径: {PythonExePath ?? "未设置"}",
                        "Python 未配置", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

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
                AddLog($"> \"{PythonExePath}\" " + args);

                // 执行Python脚本 - 使用配置的 Python.exe 路径
                var processInfo = new ProcessStartInfo
                {
                    FileName = PythonExePath,
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

                                    // 保存当前工站的点检状态
                                    SaveCurrentStationCheckStatus(CheckStatus.CheckedOK, "LAD上传完成");

                                    // 保存页面级别点检状态（使用聚合状态）
                                    var aggregatedStatus = GetAggregatedStationStatus();
                                    SaveCheckStatus(aggregatedStatus, "LAD上传完成");

                                    // 刷新 LAD 页面的状态（CommonPageModel 和 DigitalAssPageModel）
                                    RefreshLADPageStatus();

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
            var series = new SeriesCollection();

            if (values == null || values.Count < 1)
                return series;

            // 真实范围
            double min = specMin;
            double max = specMax;
            double range = max - min;
            if (range < 0.001) range = 0.001;

            // 固定 15 组
            int binCount = 15;
            double binWidth = range / binCount;

            // 统计每个区间数量
            int[] counts = new int[binCount];
            for (int i = 0; i < binCount; i++)
            {
                double start = min + i * binWidth;
                double end = start + binWidth;
                counts[i] = values.Count(v => v >= start && v < end);
            }


            // 归一化到 0.0~1.0
            int maxCount = counts.Max();
            var freq = new ChartValues<double>();
            foreach (int c in counts)
            {
                freq.Add(maxCount == 0 ? 0 : (double)c / maxCount);
            }

            // 绿色柱子
            series.Add(new ColumnSeries
            {
                Title = "Frequency",
                Values = freq,
                Fill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(76, 175, 80)),
                Stroke = System.Windows.Media.Brushes.Black,
                StrokeThickness = 1,
                MaxColumnWidth = 15,
                ColumnPadding = 0.1
            });

            // 正态曲线（同样归一化到 0~1）
            double mean = values.Average();
            double sigma = CalculateStdDev(values);
            if (sigma < 0.0001) sigma = 0.0001;

            var normal = new ChartValues<double>();
            double maxDensity = 0;
            for (int i = 0; i < binCount; i++)
            {
                double x = min + (i + 0.5) * binWidth;
                double y = Math.Exp(-0.5 * Math.Pow((x - mean) / sigma, 2)) / (sigma * Math.Sqrt(2 * Math.PI));
                normal.Add(y);
                if (y > maxDensity) maxDensity = y;
            }

            // 归一化
            for (int i = 0; i < normal.Count; i++)
            {
                normal[i] /= maxDensity;
            }

            series.Add(new LineSeries
            {
                Title = "Normal",
                Values = normal,
                PointGeometry = null,
                StrokeThickness = 2,
                Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0)),
                Fill = System.Windows.Media.Brushes.Transparent
            });

            return series;
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

        /// <summary>
        /// 添加映射项
        /// </summary>
        private void OnAddMappingItem()
        {
            var newItem = new MappingItem
            {
                TxtKey = $"NewKey_{MappingItems.Count + 1}",
                ExcelKey = "",
                StartRow = "1",
                MaxRow = "1",
                MinRow = "1"
            };
            MappingItems.Add(newItem);
            AddLog($"已添加新的映射项: {newItem.TxtKey}");
        }

        /// <summary>
        /// 删除映射项
        /// </summary>
        private void OnDeleteMappingItem()
        {
            try
            {
                if (SelectedMappingItem != null)
                {
                    string keyName = SelectedMappingItem.TxtKey;
                    MappingItems.Remove(SelectedMappingItem);
                    AddLog($"已删除映射项: {keyName}");
                }
                else
                {
                    MessageBox.Show("请先选择要删除的行！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                AddLog($"删除映射项失败: {ex.Message}");
                _commonbus?.OnLog(new LogInfo()
                {
                    LogType = LogType.Error,
                    LogMessage = $"删除映射项失败: {ex.Message}"
                });
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

        private void OnBrowseTempPythonExe()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "可执行文件|*.exe|所有文件|*.*",
                Title = "选择Python可执行文件"
            };

            if (dialog.ShowDialog() == true)
            {
                TempPythonExePath = dialog.FileName;
            }
        }

        private void OnConfig()
        {
            TempConfigFile1 = ConfigFile1;
            TempConfigFile2 = ConfigFile2;
            TempPythonScriptPath = PythonScriptPath;
            TempPythonExePath = PythonExePath;
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
            PythonExePath = TempPythonExePath;

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
            catch (OperationCanceledException)
            {
                _commonbus.OnLog(new LogInfo() { LogType = LogType.Info, LogMessage = "CPK测试被用户中止" });
                throw;
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
            finally
            {
                ProgressValue = 100;

                // 保存当前子页面的点检状态
                var checkStatus = CheckStatus.NotChecked;
                string remark = "";

                // 检查是否被中止
                bool wasCancelled = _cts.IsCancellationRequested;

                if (wasCancelled)
                {
                    // 用户中止 - CPK测试不支持继续，需从头开始
                    checkStatus = CheckStatus.CheckedFail;
                    remark = "执行中止，需从头开始";
                }
                else
                {
                    checkStatus = CheckStatus.CheckedOK;
                    remark = "CPK测试完成";
                }

                // 保存当前工站的点检状态
                SaveCurrentStationCheckStatus(checkStatus, remark);

                // 保存页面级别点检状态（使用聚合状态）
                var aggregatedStatus = GetAggregatedStationStatus();
                SaveCheckStatus(aggregatedStatus, remark);

                // 刷新 LAD 页面的状态（CommonPageModel 和 DigitalAssPageModel）
                RefreshLADPageStatus();
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
        /// Python.exe 路径
        /// </summary>
        public string PythonExePath { get; set; }

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