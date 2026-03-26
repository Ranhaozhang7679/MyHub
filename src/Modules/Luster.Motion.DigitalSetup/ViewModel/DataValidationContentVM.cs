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

        #endregion

        public DataValidationContentVM(IRepository repository, IRegionManager regionManager, ICommonBus commonBus, FlowBus flowBus, CSVHelper csvHelper, IDialogService dialogService)
            : base(repository, regionManager, commonBus, csvHelper, flowBus, dialogService)
        {
            Pages = new ObservableCollection<CommonPageModel>();
            Pages.Add(new CommonPageModel() { Name = "DataValidation", IsSelected = false, Region = "", ViewType = typeof(AssTbAutomaticPosAndLeveling) });
            _dialogService = dialogService;
            // 注册子页面到DigitalAssPageModel
            DigitalAssPageModel.RegisterSubPages("DataValidationContent", Pages);

            SelectedReportPage = Pages.Where(x => x.IsSelected).FirstOrDefault();
            InitModels();
            ConfigKey = "DataValidation";

            // 初始化集合
            ValidationItems = new ObservableCollection<ValidationItemModel>();

            // 初始化命令
            AddItemCommand = new DelegateCommand(OnAddItem);
            RemoveItemCommand = new DelegateCommand(OnRemoveItem, CanRemoveItem).ObservesProperty(() => SelectedItem);
            SelectItemCommand = new DelegateCommand<ValidationItemModel>(OnSelectItem);
            SaveConfigCommand = new DelegateCommand(OnSaveConfig);

            // 监听集合变化，自动保存
            ValidationItems.CollectionChanged += OnValidationItemsCollectionChanged;

            // 从本地持久化加载数据
            LoadFromPersistence();
            LoadCheckConfirmMessages();
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

            // 清空现有数据
            ValidationItems.Clear();
            _validationConfigCache.Clear();

            // 加载持久化的验证项
            foreach (var itemData in data.ValidationItems)
            {
                var item = new ValidationItemModel
                {
                    Name = itemData.Name,
                    ValidationType = (ValidationType)itemData.ValidationType,
                    Status = (ValidationStatus)itemData.Status
                };

                ValidationItems.Add(item);

                // 缓存配置数据
                _validationConfigCache[itemData.Name] = itemData;
            }
        }

        /// <summary>
        /// 加载示例数据
        /// </summary>
        private void LoadSampleData()
        {
            ValidationItems.Add(new ValidationItemModel { Name = "视觉标定验证", Status = Validations.ValidationStatus.Pass });
            ValidationItems.Add(new ValidationItemModel { Name = "IO检测验证", Status = Validations.ValidationStatus.Fail });
            ValidationItems.Add(new ValidationItemModel { Name = "运动精度验证", Status = Validations.ValidationStatus.Pending });
        }

        /// <summary>
        /// 保存配置到本地
        /// </summary>
        private void OnSaveConfig()
        {
            SaveToPersistence();
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
            commonVM.Initialize(item.Name, item.ValidationType);

            // 监听配置变化（必须在LoadFromConfigData之前订阅，因为LoadFromConfigData内部会调用UpdateConfigItemsByValidationType触发ConfigChanged）
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
        PressPaperResults,
        VisionFlowImages,
        FoolProofingImages,
        KeyParameters,
        CPK,
        ScannerCheck,
        VacuumCalibration,
    }
}
