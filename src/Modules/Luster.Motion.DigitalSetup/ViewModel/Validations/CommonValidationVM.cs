using Luster.Common.Assets.FloatingInfo.Models;
using Luster.Motion.DigitalSetup.Datas;
using Luster.Motion.DigitalSetup.Views;
using Luster.Motion.EditorUI.Views;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Input;

namespace Luster.Motion.DigitalSetup.ViewModel.Validations
{
    /// <summary>
    /// 配置项模型
    /// </summary>
    public class ConfigItemModel : BindableBase
    {
        private string _key;
        /// <summary>
        /// 配置键
        /// </summary>
        public string Key
        {
            get => _key;
            set => SetProperty(ref _key, value);
        }

        private string _value;
        /// <summary>
        /// 配置值
        /// </summary>
        public string Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        private string _description;
        /// <summary>
        /// 说明
        /// </summary>
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public ConfigItemModel()
        {
            Key = "";
            Value = "";
            Description = "";
        }

        public ConfigItemModel(string key, string value, string description = "")
        {
            Key = key;
            Value = value;
            Description = description;
        }
    }

    /// <summary>
    /// 通用验证 ViewModel
    /// </summary>
    public class CommonValidationVM : BindableBase, INavigationAware
    {
        private readonly IRegionManager _regionManager;

        private const string PageName = "DataValidation";

        private bool _suppressConfigChanged;

        /// <summary>
        /// 是否抑制配置变化事件（初始化期间使用，防止触发不必要的自动保存）
        /// </summary>
        public bool SuppressConfigChanged
        {
            get => _suppressConfigChanged;
            set => _suppressConfigChanged = value;
        }

        /// <summary>
        /// 已知的路径类型配置键（这些键对应的值是文件/目录路径，需要做相对路径转换）
        /// </summary>
        private static readonly HashSet<string> PathConfigKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "template", "data_file", "data_dir", "mapping_file"
        };

        /// <summary>
        /// 基准路径（配方目录），用于相对路径与绝对路径之间的转换
        /// </summary>
        private string _basePath;
        public string BasePath
        {
            get => _basePath;
            set => SetProperty(ref _basePath, value);
        }
        #region 属性

        private string _validationName;
        /// <summary>
        /// 验证项名称
        /// </summary>
        public string ValidationName
        {
            get => _validationName;
            set => SetProperty(ref _validationName, value);
        }

        private string _description;
        /// <summary>
        /// 验证描述
        /// </summary>
        public string Description
        {
            get => _description;
            set
            {
                SetProperty(ref _description, value);
                OnConfigChanged();
            }
        }

        private DateTime _lastRunTime;
        /// <summary>
        /// 最后运行时间
        /// </summary>
        public DateTime LastRunTime
        {
            get => _lastRunTime;
            set => SetProperty(ref _lastRunTime, value);
        }

        private bool _isRunning;
        /// <summary>
        /// 是否正在运行
        /// </summary>
        public bool IsRunning
        {
            get => _isRunning;
            set => SetProperty(ref _isRunning, value);
        }

        private string _validationResult;
        /// <summary>
        /// 验证结果
        /// </summary>
        public string ValidationResult
        {
            get => _validationResult;
            set
            {
                SetProperty(ref _validationResult, value);
                OnConfigChanged();
            }
        }

        private string _scriptPath;
        /// <summary>
        /// Python脚本路径
        /// </summary>
        public string ScriptPath
        {
            get => _scriptPath;
            set
            {
                SetProperty(ref _scriptPath, value);
                OnConfigChanged();
            }
        }

        private string _pyexePath;
        /// <summary>
        /// Python解释器路径
        /// </summary>
        public string PyexePath
        {
            get => _pyexePath;
            set
            {
                SetProperty(ref _pyexePath, value);
                OnConfigChanged();
            }
        }

        private string _scriptOutput;
        /// <summary>
        /// 脚本执行输出
        /// </summary>
        public string ScriptOutput
        {
            get => _scriptOutput;
            set => SetProperty(ref _scriptOutput, value);
        }

        /// <summary>
        /// 配置项集合
        /// </summary>
        public ObservableCollection<ConfigItemModel> ConfigItems { get; set; }

        private ConfigItemModel _selectedConfigItem;
        /// <summary>
        /// 选中的配置项
        /// </summary>
        public ConfigItemModel SelectedConfigItem
        {
            get => _selectedConfigItem;
            set => SetProperty(ref _selectedConfigItem, value);
        }

        #endregion

        #region 命令

        /// <summary>
        /// 添加配置项命令
        /// </summary>
        public ICommand AddConfigCommand { get; private set; }

        /// <summary>
        /// 删除配置项命令
        /// </summary>
        public ICommand RemoveConfigCommand { get; private set; }

        /// <summary>
        /// 删除指定配置项命令（带参数）
        /// </summary>
        public ICommand RemoveConfigItemCommand { get; private set; }

        /// <summary>
        /// 开始验证命令
        /// </summary>
        public ICommand StartValidationCommand { get; private set; }

        /// <summary>
        /// 停止验证命令
        /// </summary>
        public ICommand StopValidationCommand { get; private set; }

        /// <summary>
        /// 重置命令
        /// </summary>
        public ICommand ResetCommand { get; private set; }

        /// <summary>
        /// 浏览脚本路径命令
        /// </summary>
        public ICommand BrowseScriptCommand { get; private set; }

        /// <summary>
        /// 浏览Python解释器路径命令
        /// </summary>
        public ICommand BrowsePyexeCommand { get; private set; }

        #endregion

        #region 事件

        /// <summary>
        /// 配置变化事件
        /// </summary>
        public event EventHandler ConfigChanged;

        /// <summary>
        /// 验证状态变化事件
        /// </summary>
        public event EventHandler<ValidationStatusChangedEventArgs> ValidationStatusChanged;

        #endregion

        public CommonValidationVM(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            
            ValidationName = "通用验证";
            Description = "这是一个通用验证项，可以用于基本的验证操作。";
            LastRunTime = DateTime.Now;
            IsRunning = false;
            ValidationResult = "等待验证...";

            // 初始化配置项集合
            ConfigItems = new ObservableCollection<ConfigItemModel>();

            // 监听配置项集合变化
            ConfigItems.CollectionChanged += OnConfigItemsCollectionChanged;

            // 初始化命令
            AddConfigCommand = new DelegateCommand(OnAddConfig);
            RemoveConfigCommand = new DelegateCommand(OnRemoveConfig, CanRemoveConfig).ObservesProperty(() => SelectedConfigItem);
            RemoveConfigItemCommand = new DelegateCommand<ConfigItemModel>(OnRemoveConfigItem);
            StartValidationCommand = new DelegateCommand(OnStartValidation);
            StopValidationCommand = new DelegateCommand(OnStopValidation);
            ResetCommand = new DelegateCommand(OnReset);
            BrowseScriptCommand = new DelegateCommand(OnBrowseScript);
            BrowsePyexeCommand = new DelegateCommand(OnBrowsePyexe);
        }

        /// <summary>
        /// 初始化验证数据
        /// </summary>
        /// <param name="itemName">验证项名称</param>
        /// <param name="validationType">验证类型</param>
        public void Initialize(string itemName, ValidationType validationType = ValidationType.Common)
        {
            ValidationName = itemName;
            _currentValidationType = validationType;
            HasValidationTypeConfig = validationType != ValidationType.Common;
            UpdateConfigContent(); // 更新ConfigContent属性用于ContentControl绑定
        }

        private ValidationType _currentValidationType = ValidationType.Common;

        private object _configContent;
        /// <summary>
        /// 特定验证类型的配置内容（ViewModel）
        /// </summary>
        public object ConfigContent
        {
            get => _configContent;
            set => SetProperty(ref _configContent, value);
        }

        /// <summary>
        /// View加载完成后调用，更新配置内容
        /// </summary>
        public void OnViewLoaded()
        {
            UpdateConfigContent();
        }

        /// <summary>
        /// 根据验证类型更新配置内容
        /// </summary>
        private void UpdateConfigContent()
        {
            ConfigContent = _currentValidationType switch
            {
                ValidationType.LoadCellCalibration => new Configs.LoadCellCalibrationConfigVM(),
                ValidationType.CCDCalibration => new Configs.CCDCalibrationConfigVM(),
                _ => null
            };
            //ConfigContent = null;

            // 根据验证类型更新/合并 ConfigItems
            //UpdateConfigItemsByValidationType(_currentValidationType);
        }

        /// <summary>
        /// 根据验证类型获取默认配置项
        /// </summary>
        /// <param name="validationType">验证类型</param>
        /// <returns>默认配置项列表</returns>
        private List<ConfigItemModel> GetDefaultConfigItems(ValidationType validationType)
        {
            return validationType switch
            {
                ValidationType.LoadCellCalibration => new List<ConfigItemModel>
                {
                    new ConfigItemModel("template", null, "绝对文件路径 | 需要被回填数据的【Excel 模板文件】完整路径。"),
                    new ConfigItemModel("data_file", null, "绝对文件路径 | loadcell 测试输出的具体【数据 Excel 文件】路径（通常为一个如 `loadcell输出.xlsx` 或类似的文件实体，非目录"),
                },
                ValidationType.CCDCalibration => new List<ConfigItemModel>
                {
                    new ConfigItemModel("template", null, "绝对文件路径 | 需要被回填数据的【Excel 模板文件】完整路径。"),
                    new ConfigItemModel("data_dir", null, "绝对目录路径 | 存放视觉测试原数据的【目录位置】。该目录下需包含 `Data.xlsx` 及配套 `.png` 截图。"),
                },
                ValidationType.VisionStaticData => new List<ConfigItemModel>
                {
                    new ConfigItemModel("template", null, "绝对文件路径 | 需要被回填数据的【Excel 模板文件】完整路径。"),
                    new ConfigItemModel("data_dir", null, "绝对目录路径 | 存放视觉数据的【根目录位置】。**且该目录下必须具备“静态”和“动态”两个子文件夹**。"),
                    new ConfigItemModel("ccd_target", null, "要回填的【目标 CCD 名称】，如 `CCD1` 或 `CCD3` 等"),
                },
                ValidationType.GantryDynamicRepeatibilityData => new List<ConfigItemModel>
                {
                    new ConfigItemModel("template", null, "绝对文件路径 | 需要被回填数据的【Excel 模板文件】完整路径。"),
                    new ConfigItemModel("data_dir", null, "绝对目录路径 | 存放视觉数据的【根目录位置】。**且该目录下必须具备“静态”和“动态”两个子文件夹**。"),
                    new ConfigItemModel("ccd_target", null, "要回填的【目标 CCD 名称】，如 `CCD1` 或 `CCD3` 等"),
                },
                ValidationType.PressPaperResults => new List<ConfigItemModel>
                {

                },
                ValidationType.VisionFlowImages => new List<ConfigItemModel>
                {

                },
                ValidationType.FoolProofingImages => new List<ConfigItemModel>
                {

                },
                ValidationType.KeyParameters => new List<ConfigItemModel>
                {

                },
                ValidationType.CPK => new List<ConfigItemModel>
                {
                    new ConfigItemModel("template", null, "绝对文件路径 | 需要被回填数据的【Excel 模板文件】完整路径"),
                    new ConfigItemModel("data_file", null, "绝对文件路径 | 包含 `pdata@` 参数片段的【日志 TXT 文件】路径（无论加密与否均能自动透传识别）"),
                    new ConfigItemModel("mapping_file", null, "绝对文件路径 | 上位机临时打包序列化的【JSON 映射配置】路径，需包含 TXT 提取词与模板内列名的坐标化映射配置（附带写入行起止极限定位约束）"),
                    new ConfigItemModel("lsl", "", "规格下限"),
                },
                ValidationType.ScannerCheck => new List<ConfigItemModel>
                {

                },
                ValidationType.VacuumCalibration => new List<ConfigItemModel>
                {

                },
                _ => new List<ConfigItemModel>() // Common 类型返回空列表
            };
        }

        /// <summary>
        /// 根据验证类型更新/合并 ConfigItems
        /// </summary>
        /// <param name="validationType">验证类型</param>
        private void UpdateConfigItemsByValidationType(ValidationType validationType)
        {
            var defaultItems = GetDefaultConfigItems(validationType);
            if (defaultItems == null || defaultItems.Count == 0)
            {
                return; // 没有默认配置项，不做任何修改
            }

            // 获取现有配置项的Key集合
            var existingKeys = ConfigItems.Select(c => c.Key).ToHashSet();

            // 检查是否有需要添加的新项
            bool hasNewItems = defaultItems.Any(d => !existingKeys.Contains(d.Key));
            if (!hasNewItems)
            {
                return; // 所有默认项都已存在，无需更新
            }

            // 暂时取消集合变化监听（避免重复触发事件）
            ConfigItems.CollectionChanged -= OnConfigItemsCollectionChanged;

            try
            {
                // 添加缺失的默认配置项（不覆盖已存在的）
                foreach (var defaultItem in defaultItems)
                {
                    if (!existingKeys.Contains(defaultItem.Key))
                    {
                        var newItem = new ConfigItemModel(defaultItem.Key, defaultItem.Value, defaultItem.Description);
                        newItem.PropertyChanged += OnConfigItemPropertyChanged;
                        ConfigItems.Add(newItem);
                    }
                }
            }
            finally
            {
                // 恢复集合变化监听
                ConfigItems.CollectionChanged += OnConfigItemsCollectionChanged;
            }

            // 触发配置变化事件
            OnConfigChanged();
        }

        private bool _hasValidationTypeConfig;
        /// <summary>
        /// 是否有特定验证类型的配置
        /// </summary>
        public bool HasValidationTypeConfig
        {
            get => _hasValidationTypeConfig;
            set => SetProperty(ref _hasValidationTypeConfig, value);
        }

        /// <summary>
        /// 获取当前验证类型
        /// </summary>
        public ValidationType CurrentValidationType => _currentValidationType;

        /// <summary>
        /// 从配置数据加载
        /// </summary>
        /// <param name="configData">配置数据</param>
        public void LoadFromConfigData(ValidationItemData configData)
        {
            if (configData == null) return;

            string basePath = BasePath;

            // 加载期间抑制配置变化事件：标量属性（Description/ScriptPath/PyexePath/ValidationResult）
            // 的 setter 会调用 OnConfigChanged()，而此时 ConfigItems 尚未重新填充，会生成空快照，
            // 进而被订阅者写回缓存/磁盘，导致"刚选中的验证项配置被清空"。
            bool previousSuppress = _suppressConfigChanged;
            _suppressConfigChanged = true;

            // 暂时取消集合变化监听
            ConfigItems.CollectionChanged -= OnConfigItemsCollectionChanged;

            try
            {
                // 加载描述
                if (!string.IsNullOrEmpty(configData.Description))
                {
                    Description = configData.Description;
                }

                // 加载最后运行时间
                if (configData.LastRunTime.HasValue)
                {
                    LastRunTime = configData.LastRunTime.Value;
                }

                // 加载验证结果
                if (!string.IsNullOrEmpty(configData.ValidationResult))
                {
                    ValidationResult = configData.ValidationResult;
                }

                // 加载脚本路径（相对路径转绝对路径）
                if (!string.IsNullOrEmpty(configData.ScriptPath))
                {
                    ScriptPath = PathConverter.ToAbsolutePath(configData.ScriptPath, basePath);
                }

                // 加载Python解释器路径（相对路径转绝对路径）
                if (!string.IsNullOrEmpty(configData.PyexePath))
                {
                    PyexePath = PathConverter.ToAbsolutePath(configData.PyexePath, basePath);
                }

                // 加载配置项 - 先取消所有现有项的订阅
                foreach (var item in ConfigItems)
                {
                    item.PropertyChanged -= OnConfigItemPropertyChanged;
                }
                ConfigItems.Clear();

                // 添加新配置项并订阅属性变化（路径配置项转为绝对路径）
                foreach (var configItem in configData.ConfigItems)
                {
                    var newItem = new ConfigItemModel(
                        configItem.Key,
                        ConvertConfigValueToAbsolute(configItem.Key, configItem.Value, basePath),
                        configItem.Description);
                    newItem.PropertyChanged += OnConfigItemPropertyChanged;
                    ConfigItems.Add(newItem);
                }
            }
            finally
            {
                // 恢复集合变化监听
                ConfigItems.CollectionChanged += OnConfigItemsCollectionChanged;
                // 恢复抑制标志（保留调用者原本的设置）
                _suppressConfigChanged = previousSuppress;
            }

            // 已有本地配置时不补回默认参数，保留用户的手动修改（如删除参数）
            // 仅在首次使用（无本地配置）时由 SwitchValidationView 调用初始化
        }

        /// <summary>
        /// 首次使用时初始化默认配置项
        /// </summary>
        public void InitializeDefaultConfigItems()
        {
            UpdateConfigItemsByValidationType(_currentValidationType);
        }

        /// <summary>
        /// 转换为配置数据
        /// </summary>
        /// <returns>配置数据</returns>
        public ValidationItemData ToConfigData()
        {
            string basePath = BasePath;

            var data = new ValidationItemData
            {
                Name = ValidationName,
                Description = Description,
                LastRunTime = LastRunTime,
                ValidationResult = ValidationResult,
                ScriptPath = PathConverter.ToRelativePathIfUnderBase(ScriptPath, basePath),
                PyexePath = PathConverter.ToRelativePathIfUnderBase(PyexePath, basePath),
                ConfigItems = ConfigItems
                    .Where(c => !string.IsNullOrWhiteSpace(c.Key))
                    .Select(c => new ConfigItemData
                    {
                        Key = c.Key,
                        Value = ConvertConfigValueToRelative(c.Key, c.Value, basePath),
                        Description = c.Description
                    })
                    .ToList()
            };

            return data;
        }

        /// <summary>
        /// 将配置项的值转为相对路径（仅对已知的路径类型配置键进行转换）
        /// </summary>
        private string ConvertConfigValueToRelative(string key, string value, string basePath)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(basePath))
                return value;

            if (!PathConfigKeys.Contains(key))
                return value;

            return PathConverter.ToRelativePathIfUnderBase(value, basePath);
        }

        /// <summary>
        /// 将配置项的值从相对路径解析为绝对路径（仅对已知的路径类型配置键进行转换）
        /// </summary>
        private string ConvertConfigValueToAbsolute(string key, string value, string basePath)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(basePath))
                return value;

            if (!PathConfigKeys.Contains(key))
                return value;

            return PathConverter.ToAbsolutePath(value, basePath);
        }

        /// <summary>
        /// 配置项集合变化时触发配置变化事件
        /// </summary>
        private void OnConfigItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // 处理新增的项 - 订阅属性变化
            if (e.NewItems != null)
            {
                foreach (ConfigItemModel newItem in e.NewItems)
                {
                    newItem.PropertyChanged += OnConfigItemPropertyChanged;
                }
            }

            // 处理移除的项 - 取消订阅
            if (e.OldItems != null)
            {
                foreach (ConfigItemModel oldItem in e.OldItems)
                {
                    oldItem.PropertyChanged -= OnConfigItemPropertyChanged;
                }
            }

            // 触发配置变化事件
            OnConfigChanged();
        }

        /// <summary>
        /// 配置项属性变化时触发配置变化事件
        /// </summary>
        private void OnConfigItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // 当配置项的 Key、Value 或 Description 变化时触发保存
            if (e.PropertyName == nameof(ConfigItemModel.Key) ||
                e.PropertyName == nameof(ConfigItemModel.Value) ||
                e.PropertyName == nameof(ConfigItemModel.Description))
            {
                OnConfigChanged();
            }
        }

        /// <summary>
        /// 触发配置变化事件
        /// </summary>
        protected virtual void OnConfigChanged()
        {
            if (_suppressConfigChanged) return;
            ConfigChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 触发验证状态变化事件
        /// </summary>
        protected virtual void OnValidationStatusChanged(ValidationStatus status, string message)
        {
            string statusText = status switch
            {
                ValidationStatus.Pending => "Pending",
                ValidationStatus.Pass => "OK",
                ValidationStatus.Fail => "Fail",
                _ => "未知状态"
            };
            PageStatusService.Instance.UpdateStatus(PageName, statusText);
            ValidationStatusChanged?.Invoke(this, new ValidationStatusChangedEventArgs(status, message));
        }

        #region 配置项操作

        /// <summary>
        /// 添加配置项
        /// </summary>
        private void OnAddConfig()
        {
            var newItem = new ConfigItemModel();
            // 订阅属性变化事件会在 CollectionChanged 中自动处理
            ConfigItems.Add(newItem);
        }

        /// <summary>
        /// 是否可以删除配置项
        /// </summary>
        private bool CanRemoveConfig()
        {
            return SelectedConfigItem != null;
        }

        /// <summary>
        /// 删除配置项
        /// </summary>
        private void OnRemoveConfig()
        {
            if (SelectedConfigItem != null)
            {
                // 取消订阅会在 CollectionChanged 中自动处理
                ConfigItems.Remove(SelectedConfigItem);
                SelectedConfigItem = null;
            }
        }

        /// <summary>
        /// 删除指定的配置项（从DataGrid行中调用）
        /// </summary>
        /// <param name="item">要删除的配置项</param>
        private void OnRemoveConfigItem(ConfigItemModel item)
        {
            if (item != null && ConfigItems.Contains(item))
            {
                ConfigItems.Remove(item);
                // 如果删除的是选中项，清除选中状态
                if (SelectedConfigItem == item)
                {
                    SelectedConfigItem = null;
                }
            }
        }

        /// <summary>
        /// 获取配置的JSON字符串
        /// </summary>
        public string GetConfigJson()
        {
            var configDict = ConfigItems
                .Where(c => !string.IsNullOrWhiteSpace(c.Key))
                .ToDictionary(c => c.Key, c => c.Value);

            return JsonSerializer.Serialize(configDict, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }

        /// <summary>
        /// 浏览脚本路径
        /// </summary>
        private void OnBrowseScript()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Python脚本 (*.py)|*.py|所有文件 (*.*)|*.*",
                Title = "选择验证脚本",
                CheckFileExists = true,
                CheckPathExists = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                ScriptPath = openFileDialog.FileName;
            }
        }

        /// <summary>
        /// 浏览Python解释器路径
        /// </summary>
        private void OnBrowsePyexe()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "可执行文件 (*.exe)|*.exe|所有文件 (*.*)|*.*",
                Title = "选择Python解释器",
                CheckFileExists = true,
                CheckPathExists = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                PyexePath = openFileDialog.FileName;
            }
        }

        #endregion

        #region 验证操作

        /// <summary>
        /// 开始验证
        /// </summary>
        private void OnStartValidation()
        {
            IsRunning = true;
            LastRunTime = DateTime.Now;
            OnValidationStatusChanged(ValidationStatus.Pending, string.Empty);
            // 获取配置JSON
            string configJson = GetConfigJson();

            // 如果配置了脚本路径，执行Python脚本
            if (!string.IsNullOrWhiteSpace(ScriptPath) && File.Exists(ScriptPath))
            {
                // 异步执行Python脚本，避免阻塞UI
                _ = ExecutePythonScriptAsync(ScriptPath, configJson);
            }
            else
            {
                string message = $"配置参数:\n{configJson}\n\n提示: 未配置有效的脚本路径";
                ValidationResult = $"验证出错\n\n{message}";
                IsRunning = false;
                // 触发验证失败状态
                OnValidationStatusChanged(ValidationStatus.Fail, message);
            }            
        }

        /// <summary>
        /// 异步执行Python脚本
        /// </summary>
        /// <param name="scriptPath">脚本路径</param>
        /// <param name="configJson">配置JSON字符串</param>
        private async System.Threading.Tasks.Task ExecutePythonScriptAsync(string scriptPath, string configJson)
        {
            try
            {
                // 尝试找到Python可执行文件
                string pythonExe = FindPythonExecutable();
                if (string.IsNullOrEmpty(pythonExe))
                {
                    // 需要在UI线程上显示消息框
                    await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        // 提示用户安装Python
                        var result = System.Windows.MessageBox.Show(
                            "未检测到Python环境，是否前往Python官网下载安装？\n\n安装完成后请重启应用程序。",
                            "Python环境未安装",
                            System.Windows.MessageBoxButton.YesNo,
                            System.Windows.MessageBoxImage.Question);

                        if (result == System.Windows.MessageBoxResult.Yes)
                        {
                            // 打开Python下载页面
                            try
                            {
                                Process.Start(new ProcessStartInfo
                                {
                                    FileName = "https://www.python.org/downloads/",
                                    UseShellExecute = true
                                });
                            }
                            catch { }
                        }
                    });

                    ValidationResult = "错误: 未找到Python环境。\n\n请安装Python并确保勾选 'Add Python to PATH' 选项。";
                    IsRunning = false;
                    return;
                }

                // 解析JSON配置并转换为命令行参数
                string cmdArgs = BuildCommandLineArgs(configJson);

                var startInfo = new ProcessStartInfo
                {
                    FileName = pythonExe,
                    // -u 参数强制Python使用无缓冲的stdout/stderr，配置通过命令行参数传递
                    Arguments = $"-u \"{scriptPath}\" {cmdArgs}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    // 使用系统默认编码（中文Windows为GBK），避免乱码
                    StandardOutputEncoding = System.Text.Encoding.GetEncoding("GBK"),
                    StandardErrorEncoding = System.Text.Encoding.GetEncoding("GBK")
                };

                using (var process = new Process { StartInfo = startInfo })
                {
                    process.Start();

                    // 异步读取输出
                    var outputTask = process.StandardOutput.ReadToEndAsync();
                    var errorTask = process.StandardError.ReadToEndAsync();

                    // 异步等待进程退出
                    await System.Threading.Tasks.Task.Run(() => process.WaitForExit());

                    string output = await outputTask;
                    string error = await errorTask;

                    var exitCode = process.ExitCode;

                    // 调试信息
                    System.Diagnostics.Debug.WriteLine($"Python Exit Code: {exitCode}");
                    System.Diagnostics.Debug.WriteLine($"Python Output: {output}");
                    System.Diagnostics.Debug.WriteLine($"Python Error: {error}");

                    if (exitCode != 0 || !string.IsNullOrEmpty(error))
                    {
                        ScriptOutput = $"Exit Code: {exitCode}\nError: {error}";
                        ValidationResult = $"验证出错 (Exit Code: {exitCode})\n\n错误信息:\n{error}\n\n输出:\n{output}";
                        // 触发验证失败状态
                        OnValidationStatusChanged(ValidationStatus.Fail, ValidationResult);
                    }
                    else
                    {
                        // 尝试格式化JSON输出
                        string formattedOutput = FormatJsonOutput(output);
                        ScriptOutput = string.IsNullOrEmpty(output) ? "(无输出)" : output;
                        if (string.IsNullOrEmpty(output))
                        {
                            ValidationResult = $"验证完成 (Exit Code: {exitCode})\n\n脚本执行成功，但无输出内容。\n\n配置参数:\n{configJson}";
                            // 无输出内容，判定为失败
                            OnValidationStatusChanged(ValidationStatus.Fail, ValidationResult);
                        }
                        else
                        {
                            ValidationResult = $"验证完成 (Exit Code: {exitCode})\n\n脚本输出:\n{formattedOutput}";
                            
                            // 检查overall_judgement字段是否为PASS
                            bool isPass = CheckOverallJudgement(output);
                            if (isPass)
                            {
                                OnValidationStatusChanged(ValidationStatus.Pass, ValidationResult);
                            }
                            else
                            {
                                OnValidationStatusChanged(ValidationStatus.Fail, ValidationResult);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string message = $"执行脚本异常:\n{ex.Message}";
                ScriptOutput = $"Exception: {ex.Message}";
                ValidationResult = message;
                // 触发验证失败状态
                OnValidationStatusChanged(ValidationStatus.Fail, message);
            }
            finally
            {
                IsRunning = false;
            }
        }

        /// <summary>
        /// 将JSON配置转换为命令行参数格式
        /// </summary>
        /// <param name="configJson">JSON配置字符串</param>
        /// <returns>命令行参数字符串</returns>
        private string BuildCommandLineArgs(string configJson)
        {
            try
            {
                var configDict = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, string>>(configJson);
                if (configDict == null || configDict.Count == 0)
                {
                    return string.Empty;
                }

                var args = new System.Text.StringBuilder();
                foreach (var kvp in configDict)
                {
                    // 如果key已经以--开头，直接使用；否则添加--前缀
                    string key = kvp.Key.StartsWith("--") ? kvp.Key : $"--{kvp.Key}";
                    // 清理值中的首尾引号（用户可能误输入），转义内部引号
                    string value = kvp.Value?.Trim('"') ?? "";
                    // 将内部双引号转义为 \"
                    value = value.Replace("\"", "\\\"");
                    args.Append($"{key} \"{value}\" ");
                }

                return args.ToString().TrimEnd();
            }
            catch (System.Text.Json.JsonException)
            {
                // 如果JSON解析失败，返回空字符串
                return string.Empty;
            }
        }

        /// <summary>
        /// 格式化JSON输出，使其更易读
        /// </summary>
        /// <param name="output">原始输出字符串</param>
        /// <returns>格式化后的字符串</returns>
        private string FormatJsonOutput(string output)
        {
            if (string.IsNullOrWhiteSpace(output))
            {
                return output;
            }

            try
            {
                // 尝试解析为JSON并格式化
                var jsonOptions = new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                // 尝试解析为JsonElement
                var jsonElement = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(output);
                return System.Text.Json.JsonSerializer.Serialize(jsonElement, jsonOptions);
            }
            catch (System.Text.Json.JsonException)
            {
                // 如果不是有效的JSON，返回原始输出
                return output;
            }
        }

        /// <summary>
        /// 检查JSON输出中的overall_judgement字段是否为PASS
        /// </summary>
        /// <param name="output">原始输出字符串</param>
        /// <returns>如果overall_judgement为PASS返回true，否则返回false</returns>
        private bool CheckOverallJudgement(string output)
        {
            if (string.IsNullOrWhiteSpace(output))
            {
                return false;
            }

            try
            {
                var jsonElement = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(output);
                
                // 检查是否包含overall_judgement字段
                if (jsonElement.TryGetProperty("overall_judgement", out var judgementProperty))
                {
                    string judgementValue = judgementProperty.GetString();
                    return string.Equals(judgementValue, "PASS", System.StringComparison.OrdinalIgnoreCase);
                }
                
                return false;
            }
            catch (System.Text.Json.JsonException)
            {
                // JSON解析失败，返回false
                return false;
            }
        }

        /// <summary>
        /// 停止验证
        /// </summary>
        private void OnStopValidation()
        {
            IsRunning = false;
            ValidationResult = "验证已停止。";
        }

        /// <summary>
        /// 重置
        /// </summary>
        private void OnReset()
        {
            IsRunning = false;
            ValidationResult = "等待验证...";
        }

        /// <summary>
        /// 查找Python可执行文件
        /// </summary>
        /// <returns>Python可执行文件路径，如果未找到返回null</returns>
        private string FindPythonExecutable()
        {
            // 如果用户配置了Python解释器路径，优先使用
            if (!string.IsNullOrEmpty(PyexePath) && File.Exists(PyexePath))
            {
                return PyexePath;
            }

            // 尝试的Python可执行文件列表
            string[] pythonCommands = new string[]
            {
                "python",
                "python3",
                "py",
                "py3"
            };

            foreach (var cmd in pythonCommands)
            {
                try
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = cmd,
                        Arguments = "--version",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };

                    using (var process = Process.Start(startInfo))
                    {
                        if (process != null)
                        {
                            process.WaitForExit(5000);
                            if (process.ExitCode == 0)
                            {
                                return cmd;
                            }
                        }
                    }
                }
                catch
                {
                    // 继续尝试下一个
                }
            }

            // 尝试常见安装路径
            string[] commonPaths = new string[]
            {
                @"C:\Python312\python.exe",
                @"C:\Python311\python.exe",
                @"C:\Python310\python.exe",
                @"C:\Python39\python.exe",
                @"C:\Python38\python.exe",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Programs\Python\Python312\python.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Programs\Python\Python311\python.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Programs\Python\Python310\python.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Programs\Python\Python39\python.exe"),
            };

            foreach (var path in commonPaths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }

            return null;
        }

        #endregion
        
        #region INavigationAware 实现
        
        /// <summary>
        /// 是否导航目标
        /// </summary>
        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }
        
        /// <summary>
        /// 导航进入
        /// </summary>
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            // 可以从导航参数中获取验证类型
            if (navigationContext.Parameters.TryGetValue("ValidationType", out ValidationType validationType))
            {
                _currentValidationType = validationType;
                HasValidationTypeConfig = validationType != ValidationType.Common;
                UpdateConfigContent(); // 更新ConfigContent属性用于ContentControl绑定
            }
        }
        
        /// <summary>
        /// 导航离开
        /// </summary>
        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            // 清理资源
        }
        
        #endregion
    }

}
