using DocumentFormat.OpenXml.Office.Word;
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
using Luster.Motion.DigitalSetup.ViewModel.Validations;
using Luster.Motion.DigitalSetup.Views;
using Luster.Motion.EditorUI;
using Luster.Motion.TaskFlow.Engine;
using Luster.Motion.TaskFlow.Engine.HyperTrain;
using Luster.TaskFlow.Common.Enums;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Luster.Motion.DigitalSetup.ViewModel
{
    /// <summary>
    /// 数据验证界面 ViewModel
    /// </summary>
    public class DataValidationContentVM : BaseAss
    {
        private readonly IDialogService _dialogService;

        // 取消令牌源，用于中止点检
        private System.Threading.CancellationTokenSource _cancellationTokenSource;

        // 进度条
        private double _progressValue;

        #region 属性

        /// <summary>
        /// 验证项集合
        /// </summary>
        public ObservableCollection<ValidationItemModel> ValidationItems { get; set; }

        private ValidationItemModel _selectedItem;
        /// <summary>
        /// 当前选中的验证项
        /// </summary>
        public ValidationItemModel SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        private object _currentValidationVM;
        /// <summary>
        /// 当前显示的验证ViewModel
        /// </summary>
        public object CurrentValidationVM
        {
            get => _currentValidationVM;
            set => SetProperty(ref _currentValidationVM, value);
        }

        /// <summary>
        /// 存储每个验证项对应的配置数据
        /// </summary>
        private Dictionary<string, ValidationItemData> _validationConfigCache = new Dictionary<string, ValidationItemData>();

        /// <summary>
        /// 进度条
        /// </summary>
        public double ProgressValue
        {
            get { return _progressValue; }
            set { SetProperty(ref _progressValue, value); }
        }

        #endregion

        #region 命令

        /// <summary>
        /// 添加验证项命令
        /// </summary>
        public ICommand AddItemCommand { get; private set; }

        /// <summary>
        /// 删除验证项命令
        /// </summary>
        public ICommand RemoveItemCommand { get; private set; }

        /// <summary>
        /// 选择验证项命令
        /// </summary>
        public ICommand SelectItemCommand { get; private set; }

        /// <summary>
        /// 保存配置命令
        /// </summary>
        public ICommand SaveConfigCommand { get; private set; }

        /// <summary>
        /// 一键点检命令
        /// </summary>
        public ICommand OneKeyCheckCommand { get; private set; }

        /// <summary>
        /// 中止点检命令
        /// </summary>
        public ICommand EndCommand { get; private set; }

        #endregion

        public DataValidationContentVM(IRepository repository, IRegionManager regionManager, ICommonBus commonBus, FlowBus flowBus, CSVHelper csvHelper, IDialogService dialogService, CheckStatusService checkStatusService)
            : base(repository, regionManager, commonBus, csvHelper, flowBus, dialogService, checkStatusService)
        {
            _parentRegionName = "DataValidationContent";

            Pages = new ObservableCollection<CommonPageModel>();
            Pages.Add(new CommonPageModel() { Name = "DataValidation", IsSelected = false, Region = "DataValidationContent", ViewType = typeof(AssTbAutomaticPosAndLeveling) });
            _dialogService = dialogService;
            // 注册子页面到DigitalAssPageModel
            DigitalAssPageModel.RegisterSubPages("DataValidationContent", Pages);

            SelectedReportPage = Pages.Where(x => x.IsSelected).FirstOrDefault();
            InitModels();
            ConfigKey = "DataValidation";

            // 初始化集合
            ValidationItems = new ObservableCollection<ValidationItemModel>();

            // 初始化进度条
            _progressValue = 0;

            // 初始化命令
            AddItemCommand = new DelegateCommand(OnAddItem);
            RemoveItemCommand = new DelegateCommand(OnRemoveItem, CanRemoveItem).ObservesProperty(() => SelectedItem);
            SelectItemCommand = new DelegateCommand<ValidationItemModel>(OnSelectItem);
            SaveConfigCommand = new DelegateCommand(OnSaveConfig);
            OneKeyCheckCommand = new DelegateCommand(() => OnOneKeyCheck(null));
            EndCommand = new DelegateCommand(OnEnd);

            // 监听集合变化，自动保存
            ValidationItems.CollectionChanged += OnValidationItemsCollectionChanged;

            // 从本地持久化加载数据
            LoadFromPersistence();
            LoadCheckConfirmMessages();

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
                        page.ParentRegion = "DataValidationContent";
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
        /// 刷新点检状态 - 每次页面激活时调用
        /// </summary>
        protected override void RefreshCheckStatus()
        {
            LoadCheckStatusForAllPages();
        }

        /// <summary>
        /// 一键点检 - 顺序执行所有验证项的验证
        /// </summary>
        public override async void OnOneKeyCheck(object obj)
        {
            try
            {
                // 如果正在运行，先中止
                if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
                {
                    return;
                }

                // 初始化取消令牌
                _cancellationTokenSource = new System.Threading.CancellationTokenSource();
                var token = _cancellationTokenSource.Token;

                // 重置进度
                ProgressValue = 0;

                // 检查是否有验证项
                if (ValidationItems == null || ValidationItems.Count == 0)
                {
                    _commonbus.OnLog(new LogInfo() { LogType = LogType.Info, LogMessage = "没有可执行的验证项" });
                    SaveCheckStatus(CheckStatus.CheckedFail, "没有可执行的验证项");
                    SyncOverallStatusToPageStatusService();
                    return;
                }

                int totalItems = ValidationItems.Count;
                int completedItems = 0;
                int passCount = 0;
                int failCount = 0;

                _commonbus.OnLog(new LogInfo() { LogType = LogType.Info, LogMessage = $"开始一键点检，共 {totalItems} 个验证项" });

                // 顺序执行每个验证项
                foreach (var item in ValidationItems)
                {
                    // 检查是否已取消
                    if (token.IsCancellationRequested)
                    {
                        _commonbus.OnLog(new LogInfo() { LogType = LogType.Info, LogMessage = "点检已中止" });
                        break;
                    }

                    // 更新当前选中的项，以便触发验证
                    SelectedItem = item;

                    // 等待一小段时间确保 UI 更新
                    await System.Threading.Tasks.Task.Delay(100, token);

                    // 获取当前验证项的配置数据
                    var configData = GetValidationConfig(item.Name);
                    if (configData == null)
                    {
                        _commonbus.OnLog(new LogInfo() { LogType = LogType.Warning, LogMessage = $"验证项 {item.Name} 没有配置数据，跳过" });
                        item.Status = ValidationStatus.Fail;
                        failCount++;
                        completedItems++;
                        ProgressValue = (completedItems * 100.0 / totalItems);
                        // 同步状态到 PageStatusService
                        PageStatusService.Instance.UpdateStatus(item.Name, "NG");
                        continue;
                    }

                    // 检查是否配置了脚本路径
                    if (string.IsNullOrEmpty(configData.ScriptPath) || !System.IO.File.Exists(configData.ScriptPath))
                    {
                        _commonbus.OnLog(new LogInfo() { LogType = LogType.Warning, LogMessage = $"验证项 {item.Name} 脚本路径无效，跳过" });
                        item.Status = ValidationStatus.Fail;
                        failCount++;
                        // 同步状态到 PageStatusService
                        PageStatusService.Instance.UpdateStatus(item.Name, "NG");
                    }
                    else
                    {
                        // 执行验证
                        _commonbus.OnLog(new LogInfo() { LogType = LogType.Info, LogMessage = $"正在执行: {item.Name}" });

                        // 创建验证任务完成信号
                        var validationTcs = new System.Threading.Tasks.TaskCompletionSource<bool>();

                        // 订阅验证状态变更事件
                        EventHandler<ValidationStatusChangedEventArgs> statusHandler = null;
                        statusHandler = (s, e) =>
                        {
                            if (e.Status == ValidationStatus.Pass || e.Status == ValidationStatus.Fail)
                            {
                                if (e.Status == ValidationStatus.Pass)
                                    passCount++;
                                else
                                    failCount++;

                                validationTcs.TrySetResult(e.Status == ValidationStatus.Pass);
                            }
                        };

                        // 获取当前的 CommonValidationVM
                        if (CurrentValidationVM is CommonValidationVM validationVM)
                        {
                            validationVM.ValidationStatusChanged += statusHandler;

                            // 触发验证
                            validationVM.StartValidationCommand.Execute(null);

                            // 等待验证完成或超时（5分钟超时）
                            var timeoutTask = System.Threading.Tasks.Task.Delay(TimeSpan.FromMinutes(5), token);
                            var completedTask = await System.Threading.Tasks.Task.WhenAny(validationTcs.Task, timeoutTask);

                            validationVM.ValidationStatusChanged -= statusHandler;

                            if (completedTask == timeoutTask)
                            {
                                _commonbus.OnLog(new LogInfo() { LogType = LogType.Warning, LogMessage = $"验证项 {item.Name} 超时" });
                                item.Status = ValidationStatus.Fail;
                                failCount++;
                            }
                        }
                        else
                        {
                            // 没有激活的验证VM，跳过
                            _commonbus.OnLog(new LogInfo() { LogType = LogType.Warning, LogMessage = $"验证项 {item.Name} 未激活，跳过" });
                            item.Status = ValidationStatus.Fail;
                            failCount++;
                        }

                        // 同步该验证项的状态到 PageStatusService
                        string itemStatusText = item.Status switch
                        {
                            ValidationStatus.Pass => "OK",
                            ValidationStatus.Fail => "NG",
                            _ => "未点检"
                        };
                        PageStatusService.Instance.UpdateStatus(item.Name, itemStatusText);
                    }

                    completedItems++;
                    ProgressValue = (completedItems * 100.0 / totalItems);

                    // 保存当前验证状态到持久化
                    SaveToPersistence();
                }

                // 判定最终状态
                ProgressValue = 100;
                CheckStatus finalStatus;
                string remark;

                if (token.IsCancellationRequested)
                {
                    finalStatus = CheckStatus.NotChecked;
                    remark = $"点检已中止，已完成 {completedItems}/{totalItems} 项";
                }
                else if (failCount == 0 && passCount == totalItems)
                {
                    finalStatus = CheckStatus.CheckedOK;
                    remark = $"所有验证项通过 (Pass: {passCount}, Fail: {failCount})";
                }
                else
                {
                    finalStatus = CheckStatus.CheckedFail;
                    remark = $"部分验证项失败 (Pass: {passCount}, Fail: {failCount})";
                }

                SaveCheckStatus(finalStatus, remark);

                // 同步一级界面整体状态到 PageStatusService
                SyncOverallStatusToPageStatusService();

                _commonbus.OnLog(new LogInfo() { LogType = LogType.Info, LogMessage = $"点检完成: {remark}" });
            }
            catch (Exception ex)
            {
                _commonbus.OnLog(new LogInfo() { LogType = LogType.Error, LogMessage = $"点检异常: {ex.Message}" });
                SaveCheckStatus(CheckStatus.CheckedFail, $"点检异常: {ex.Message}");
                SyncOverallStatusToPageStatusService();
            }
            finally
            {
                // 清理取消令牌
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        /// <summary>
        /// 中止点检
        /// </summary>
        public override void OnEnd()
        {
            base.OnEnd();

            // 请求取消
            _cancellationTokenSource?.Cancel();

            _commonbus.OnLog(new LogInfo() { LogType = LogType.Info, LogMessage = "正在中止点检..." });
        }

        /// <summary>
        /// 获取整体点检状态 - 基于所有 ValidationItems 的状态
        /// </summary>
        protected override CheckStatus GetOverallCheckStatus()
        {
            // 如果没有验证项，返回未点检
            if (ValidationItems == null || ValidationItems.Count == 0)
                return CheckStatus.NotChecked;

            bool hasNG = false;
            bool hasOK = false;
            bool hasNotChecked = false;

            foreach (var item in ValidationItems)
            {
                switch (item.Status)
                {
                    case ValidationStatus.Fail:
                        hasNG = true;
                        break; // 任一 NG，整体就是 NG
                    case ValidationStatus.Pass:
                        hasOK = true;
                        break;
                    default:
                        hasNotChecked = true;
                        break;
                }
            }

            // 判定逻辑
            if (hasNG)
                return CheckStatus.CheckedFail;
            else if (hasNotChecked)
                return CheckStatus.NotChecked;
            else if (hasOK)
                return CheckStatus.CheckedOK;
            else
                return CheckStatus.NotChecked;
        }

        /// <summary>
        /// 从本地持久化加载数据
        /// </summary>
        private void LoadFromPersistence()
        {
            var data = _persistenceService.Load();

            if (data.ValidationItems.Count == 0)
            {
                // 如果没有持久化数据，加载示例数据
                LoadSampleData();
                return;
            }

            // 加载期间暂停自动保存，避免 Add 触发保存时 cache 还未更新导致数据丢失
            ValidationItems.CollectionChanged -= OnValidationItemsCollectionChanged;

            try
            {
                // 清空现有数据
                ValidationItems.Clear();
                _validationConfigCache.Clear();

                // 加载持久化的验证项
                foreach (var itemData in data.ValidationItems)
                {
                    // 先更新缓存，再添加到集合
                    _validationConfigCache[itemData.Name] = itemData;

                    var item = new ValidationItemModel
                    {
                        Name = itemData.Name,
                        ValidationType = (ValidationType)itemData.ValidationType,
                        Status = (ValidationStatus)itemData.Status
                    };

                    ValidationItems.Add(item);
                }
            }
            finally
            {
                // 恢复自动保存监听
                ValidationItems.CollectionChanged += OnValidationItemsCollectionChanged;
            }
        }

        /// <summary>
        /// 加载示例数据
        /// </summary>
        private void LoadSampleData()
        {
            ValidationItems.Add(new ValidationItemModel { Name = "Step1 Load Cell Calibration", ValidationType = ValidationType.LoadCellCalibration});
            ValidationItems.Add(new ValidationItemModel { Name = "Step2 CCD Calibration", ValidationType = ValidationType.CCDCalibration });
            ValidationItems.Add(new ValidationItemModel { Name = "Step3 Vision Static Data", ValidationType = ValidationType.VisionStaticData });
            ValidationItems.Add(new ValidationItemModel { Name = "Step4 Gantry Dynamic Data", ValidationType = ValidationType.GantryDynamicRepeatibilityData });
            ValidationItems.Add(new ValidationItemModel { Name = "Step5 Vision Flow Images", ValidationType = ValidationType.VisionFlowImages });
            ValidationItems.Add(new ValidationItemModel { Name = "Step6 Scanner Check", ValidationType = ValidationType.ScannerCheck });
            ValidationItems.Add(new ValidationItemModel { Name = "Step7 Cosmetic Check", ValidationType = ValidationType.CosmeticCheck });
            ValidationItems.Add(new ValidationItemModel { Name = "Step8 Press Paper Test", ValidationType = ValidationType.PressPaperResults });
            ValidationItems.Add(new ValidationItemModel { Name = "Step9 Fool Proofing Images", ValidationType = ValidationType.FoolProofingImages });
            ValidationItems.Add(new ValidationItemModel { Name = "Step10 Screwdrive Calibration", ValidationType = ValidationType.ScrewdriveCalibration });
            ValidationItems.Add(new ValidationItemModel { Name = "Step11 Recheck Image", ValidationType = ValidationType.RecheckImage });
            ValidationItems.Add(new ValidationItemModel { Name = "Step12 PeelOff PF Validation", ValidationType = ValidationType.PeelOffPFValidation });
            ValidationItems.Add(new ValidationItemModel { Name = "Step13 Paste PF Validation", ValidationType = ValidationType.PastePFValidation });
            ValidationItems.Add(new ValidationItemModel { Name = "Step14 AOI Capability Check", ValidationType = ValidationType.AOICapabilityCheck });
            ValidationItems.Add(new ValidationItemModel { Name = "Step15 GRR", ValidationType = ValidationType.GRR });
            ValidationItems.Add(new ValidationItemModel { Name = "Step16 Key Parameters", ValidationType = ValidationType.KeyParameters });
            ValidationItems.Add(new ValidationItemModel { Name = "Step17 Vacuum Calibration", ValidationType = ValidationType.VacuumCalibration });
            ValidationItems.Add(new ValidationItemModel { Name = "Step18 CPK", ValidationType = ValidationType.CPK });
        }

        /// <summary>
        /// 保存配置到本地
        /// </summary>
        private void OnSaveConfig()
        {
            // 保存前先同步当前激活的 CommonValidationVM 的最新数据到缓存
            SyncCurrentValidationVMToCache();
            SaveToPersistence();
        }

        /// <summary>
        /// 将当前激活的 CommonValidationVM 的最新配置同步到缓存
        /// </summary>
        private void SyncCurrentValidationVMToCache()
        {
            if (CurrentValidationVM is CommonValidationVM commonVM && SelectedItem != null)
            {
                var updatedData = commonVM.ToConfigData();
                _validationConfigCache[SelectedItem.Name] = updatedData;
            }
        }

        /// <summary>
        /// 保存数据到本地持久化
        /// </summary>
        private void SaveToPersistence()
        {
            var data = new DataValidationPersistenceData();

            foreach (var item in ValidationItems)
            {
                var itemData = new ValidationItemData
                {
                    Name = item.Name,
                    ValidationType = (int)item.ValidationType,
                    Status = (int)item.Status
                };

                // 如果有缓存的配置数据，使用缓存
                if (_validationConfigCache.TryGetValue(item.Name, out var cachedData))
                {
                    itemData.ConfigItems = cachedData.ConfigItems;
                    itemData.Description = cachedData.Description;
                    itemData.LastRunTime = cachedData.LastRunTime;
                    itemData.ValidationResult = cachedData.ValidationResult;
                    itemData.ScriptPath = cachedData.ScriptPath;
                    itemData.PyexePath = cachedData.PyexePath;
                }

                data.ValidationItems.Add(itemData);
            }

            bool res = _persistenceService.Save(data);
            //string messgae = res ? "保持成功" : "保持失败";
            //MessageBox.Show(messgae);
        }

        /// <summary>
        /// 验证项集合变化时自动保存
        /// </summary>
        private void OnValidationItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add ||
                e.Action == NotifyCollectionChangedAction.Remove ||
                e.Action == NotifyCollectionChangedAction.Replace ||
                e.Action == NotifyCollectionChangedAction.Reset)
            {
                SaveToPersistence();
            }
        }

        /// <summary>
        /// 更新验证项的配置数据
        /// </summary>
        /// <param name="itemName">验证项名称</param>
        /// <param name="configData">配置数据</param>
        public void UpdateValidationConfig(string itemName, ValidationItemData configData)
        {
            if (!string.IsNullOrEmpty(itemName))
            {
                _validationConfigCache[itemName] = configData;
                SaveToPersistence();
            }
        }

        /// <summary>
        /// 获取验证项的配置数据
        /// </summary>
        /// <param name="itemName">验证项名称</param>
        /// <returns>配置数据</returns>
        public ValidationItemData GetValidationConfig(string itemName)
        {
            if (_validationConfigCache.TryGetValue(itemName, out var data))
            {
                return data;
            }
            return null;
        }

        /// <summary>
        /// 添加验证项
        /// </summary>
        private void OnAddItem()
        {
            // 弹出对话框让用户输入验证项名称
            _dialogService.ShowDialog("AddValidationItemDialog", null, result =>
            {
                if (result.Result == ButtonResult.OK)
                {
                    var name = result.Parameters.GetValue<string>("ItemName");
                    var validationType = result.Parameters.GetValue<ValidationType>("ValidationType");
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        var newItem = new ValidationItemModel
                        {
                            Name = name,
                            Status = Validations.ValidationStatus.Pending,
                            ValidationType = validationType
                        };

                        ValidationItems.Add(newItem);

                        // 初始化配置缓存
                        _validationConfigCache[name] = new ValidationItemData
                        {
                            Name = name,
                            ValidationType = (int)validationType,
                            Status = (int)Validations.ValidationStatus.Pending,
                            ConfigItems = new System.Collections.Generic.List<ConfigItemData>()
                        };
                    }
                }
            });
        }

        /// <summary>
        /// 是否可以删除验证项
        /// </summary>
        private bool CanRemoveItem()
        {
            return SelectedItem != null;
        }

        /// <summary>
        /// 删除验证项
        /// </summary>
        private void OnRemoveItem()
        {
            if (SelectedItem != null)
            {
                if (MessageBox.Show($"确认删除{SelectedItem.Name}?", "InfoTip", button: MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                {
                    return;
                }

                // 删除配置缓存
                _validationConfigCache.Remove(SelectedItem.Name);

                ValidationItems.Remove(SelectedItem);
                SelectedItem = null;
            }
        }

        /// <summary>
        /// 选择验证项
        /// </summary>
        private void OnSelectItem(ValidationItemModel item)
        {
            if (item != null)
            {
                // 切换前先同步当前 VM 的最新配置到缓存
                SyncCurrentValidationVMToCache();

                SelectedItem = item;
                // 根据验证类型切换右侧显示的ViewModel
                SwitchValidationView(item);
            }
        }

        /// <summary>
        /// 根据验证类型切换视图
        /// </summary>
        private void SwitchValidationView(ValidationItemModel item)
        {
            // 获取缓存的配置数据
            var configData = GetValidationConfig(item.Name);

            // 所有验证类型都使用CommonValidationVM，通过Region加载特定配置控件
            // 传入RegionManager以支持Region导航
            var commonVM = new CommonValidationVM(_regionManager);
            commonVM.BasePath = _commonbus?.CurrentRecipe?.GetRecipePath() ?? "";
            commonVM.Initialize(item.Name, item.ValidationType);

            // 监听配置变化（必须在配置加载之前订阅）
            commonVM.ConfigChanged += (s, e) =>
            {
                var updatedData = commonVM.ToConfigData();
                UpdateValidationConfig(item.Name, updatedData);
            };

            // 监听验证状态变化，更新左侧列表状态
            commonVM.ValidationStatusChanged += (s, e) =>
            {
                item.Status = e.Status;
            };

            // 加载保存的配置数据
            if (configData != null)
            {
                commonVM.LoadFromConfigData(configData);
            }
            else
            {
                // 首次使用，初始化默认参数
                commonVM.InitializeDefaultConfigItems();
            }

            CurrentValidationVM = commonVM;
            //switch (item.ValidationType)
            //{
            //    case ValidationType.Common:
            //    case ValidationType.LoadCellCalibration:
            //    case ValidationType.CCDCalibration:
            //        // 这些类型都使用CommonValidationVM + Region配置控件
            //        break;
            //    case ValidationType.VisionStaticData:
            //        // TODO: 创建 VisionStaticDataValidationVM
            //        CurrentValidationVM = null;
            //        break;
            //    case ValidationType.GantryDynamicRepeatibilityData:
            //        // TODO: 创建 GantryDynamicRepeatibilityDataValidationVM
            //        CurrentValidationVM = null;
            //        break;
            //    case ValidationType.PressPaperResults:
            //        // TODO: 创建 PressPaperResultsValidationVM
            //        CurrentValidationVM = null;
            //        break;
            //    case ValidationType.VisionFlowImages:
            //        // TODO: 创建 VisionFlowImagesValidationVM
            //        CurrentValidationVM = null;
            //        break;
            //    default:
            //        CurrentValidationVM = null;
            //        break;
            //}
        }
    }

    /// <summary>
    /// 验证项模型
    /// </summary>
    public class ValidationItemModel : BindableBase
    {
        private string _name;
        /// <summary>
        /// 验证项名称
        /// </summary>
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private Validations.ValidationStatus _status;
        /// <summary>
        /// 验证状态
        /// </summary>
        public Validations.ValidationStatus Status
        {
            get => _status;
            set
            {
                SetProperty(ref _status, value);
                RaisePropertyChanged(nameof(StatusText));
            }
        }

        /// <summary>
        /// 状态显示文本
        /// </summary>
        public string StatusText => Status switch
        {
            Validations.ValidationStatus.Pass => "Pass",
            Validations.ValidationStatus.Fail => "Fail",
            Validations.ValidationStatus.Pending => "Pending",
            _ => "Pending"
        };

        private ValidationType _validationType;
        /// <summary>
        /// 验证类型
        /// </summary>
        public ValidationType ValidationType
        {
            get => _validationType;
            set => SetProperty(ref _validationType, value);
        }
    }

    public enum ValidationType
    {
        Common = 0,
        LoadCellCalibration = 1,
        CCDCalibration,
        VisionStaticData,
        GantryDynamicRepeatibilityData,
        VisionFlowImages,
        ScannerCheck,
        CosmeticCheck,
        PressPaperResults,
        FoolProofingImages,
        ScrewdriveCalibration,
        RecheckImage,
        PeelOffPFValidation,
        PastePFValidation,
        AOICapabilityCheck,
        GRR,
        KeyParameters,
        VacuumCalibration,
        CPK,
    }
}
