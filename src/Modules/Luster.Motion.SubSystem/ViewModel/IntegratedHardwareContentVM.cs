using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.SubSystem.Models;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Luster.Motion.SubSystem.ViewModel
{
    /// <summary>
    /// 硬件调试配置集成内容视图的 ViewModel
    /// </summary>
    public class IntegratedHardwareContentVM : MotionPageVM
    {
        private readonly IRegionManager _regionManager;

        // 缓存选中的菜单项，避免 LINQ 查询
        private NavigationItemModel _selectedConfigItem;
        private NavigationItemModel _selectedDebugItem;

        // 跟踪当前导航到的视图，避免重复导航
        private string _currentConfigRegion = "";
        private string _currentDebugRegion = "";

        // 跨实例持久化标签页状态
        private static int _lastSelectedTabIndex = 0;

        /// <summary>
        /// 配置导航命令
        /// </summary>
        public DelegateCommand<NavigationItemModel> NavigateConfigCommand { get; private set; }

        /// <summary>
        /// 调试导航命令
        /// </summary>
        public DelegateCommand<NavigationItemModel> NavigateDebugCommand { get; private set; }

        /// <summary>
        /// 切换标签页命令
        /// </summary>
        public ICommand SelectTabCommand { get; private set; }

        /// <summary>
        /// 是否显示配置占位符
        /// </summary>
        private bool _showConfigPlaceholder = false;
        public bool ShowConfigPlaceholder
        {
            get { return _showConfigPlaceholder; }
            set { SetProperty(ref _showConfigPlaceholder, value); }
        }

        /// <summary>
        /// 是否显示调试占位符
        /// </summary>
        private bool _showDebugPlaceholder = false;
        public bool ShowDebugPlaceholder
        {
            get { return _showDebugPlaceholder; }
            set { SetProperty(ref _showDebugPlaceholder, value); }
        }

        /// <summary>
        /// 当前选中的标签页索引
        /// </summary>
        private int _selectedIndex = 0;
        public int SelectedTabIndex
        {
            get { return _selectedIndex; }
            set { SetProperty(ref _selectedIndex, value); }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public IntegratedHardwareContentVM(ICommonBus commonBus, IRegionManager regionManager) : base(commonBus)
        {
            _regionManager = regionManager;
            NavigateConfigCommand = new DelegateCommand<NavigationItemModel>(NavigateConfig);
            NavigateDebugCommand = new DelegateCommand<NavigationItemModel>(NavigateDebug);
            SelectTabCommand = new DelegateCommand<object>(SelectTab);

            BuildConfigPages();
            BuildDebugPages();
        }

        /// <summary>
        /// 注册事件
        /// </summary>
        protected override void RegisterEvent(IEventAggregator bus)
        {
            bus.GetEvent<LangChangedDoneEvent>().Subscribe(() =>
            {
                // 刷新所有菜单项的显示名称
                if (ConfigPages != null)
                {
                    foreach (var item in ConfigPages)
                    {
                        item.RefreshDisplayName();
                    }
                }

                if (DebugPages != null)
                {
                    foreach (var item in DebugPages)
                    {
                        item.RefreshDisplayName();
                    }
                }

                // 触发选中项显示名称的属性更改通知
                RaisePropertyChanged(nameof(SelectedConfigDisplayName));
                RaisePropertyChanged(nameof(SelectedDebugDisplayName));
            });
        }

        /// <summary>
        /// 导航到默认页面（视图加载后调用）
        /// </summary>
        public void NavigateToDefault()
        {
            // 切换到默认标签页（配置），会自动导航到第一个菜单项
            SelectTab(0);
        }

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            base.OnNavigatedFrom(navigationContext);
            _lastSelectedTabIndex = SelectedTabIndex;
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
            _currentConfigRegion = "";
            _currentDebugRegion = "";

            // 恢复上次离开时的标签页
            SelectTab(_lastSelectedTabIndex);
        }

        /// <summary>
        /// 构建配置导航页面列表
        /// </summary>
        private void BuildConfigPages()
        {
            ConfigPages = new ObservableCollection<NavigationItemModel>
            {
                new NavigationItemModel { Index = 1, Name = "Cockpit", Region = "ProductInfoContent", IsSelected = true },
                new NavigationItemModel { Index = 2, Name = "Maintenance", Region = "MaintainContent", IsSelected = false },
                new NavigationItemModel { Index = 3, Name = "KeyParameters", Region = "KeyParameterContent", IsSelected = false },
                new NavigationItemModel { Index = 4, Name = "FunctionEnable", Region = "FunctionEnableContent", IsSelected = false }
            };
        }

        /// <summary>
        /// 构建调试导航页面列表
        /// </summary>
        private void BuildDebugPages()
        {
            DebugPages = new ObservableCollection<NavigationItemModel>
            {
                new NavigationItemModel { Index = 1, Name = "VIOSimulation", Region = "VIOSimulationContent", IsSelected = true },
                new NavigationItemModel { Index = 2, Name = "VCylinder", Region = "VCylinderContent", IsSelected = false },
                new NavigationItemModel { Index = 3, Name = "VVacuum", Region = "VVacuumContent", IsSelected = false },
                new NavigationItemModel { Index = 4, Name = "VIO", Region = "VIOContent", IsSelected = false },
                new NavigationItemModel { Index = 5, Name = "VAxisM", Region = "AxisIODebugContent", IsSelected = false },
                new NavigationItemModel { Index = 6, Name = "VCommuncation", Region = "VCommuncationContent", IsSelected = false },
                new NavigationItemModel { Index = 7, Name = "VAxis", Region = "VAxisContent", IsSelected = false }
            };
        }

        /// <summary>
        /// 配置页面列表
        /// </summary>
        private ObservableCollection<NavigationItemModel> _configPages;
        public ObservableCollection<NavigationItemModel> ConfigPages
        {
            get { return _configPages; }
            set { SetProperty(ref _configPages, value); }
        }

        /// <summary>
        /// 调试页面列表
        /// </summary>
        private ObservableCollection<NavigationItemModel> _debugPages;
        public ObservableCollection<NavigationItemModel> DebugPages
        {
            get { return _debugPages; }
            set { SetProperty(ref _debugPages, value); }
        }

        /// <summary>
        /// 切换标签页
        /// </summary>
        private void SelectTab(object index)
        {
            int i = 0;
            if (index is int intVal)
            {
                i = intVal;
            }
            else if (index is string strVal && int.TryParse(strVal, out int parsed))
            {
                i = parsed;
            }
            SelectedTabIndex = i;

            // 切换标签页后，导航到该标签页当前选中的菜单项（保持之前的状态）
            switch (i)
            {
                case 0: // 配置标签页
                    if (ConfigPages != null && ConfigPages.Count > 0)
                    {
                        // 如果没有选中项（首次），导航到第一个；否则导航到当前选中项
                        var target = ConfigPages.FirstOrDefault(p => p.IsSelected) ?? ConfigPages[0];
                        NavigateConfig(target);
                    }
                    break;
                case 1: // 调试标签页
                    if (DebugPages != null && DebugPages.Count > 0)
                    {
                        // 如果没有选中项（首次），导航到第一个；否则导航到当前选中项
                        var target = DebugPages.FirstOrDefault(p => p.IsSelected) ?? DebugPages[0];
                        NavigateDebug(target);
                    }
                    break;
                case 2: // 数字架线标签页 - 导航到 DigitalAssContent
                    _regionManager.RequestNavigate("DigitalAssContentRegion", "DigitalAssContent");
                    break;
            }
        }

        /// <summary>
        /// 配置导航
        /// </summary>
        private void NavigateConfig(NavigationItemModel item)
        {
            if (item == null)
                return;

            // 设置选中状态
            foreach (var page in ConfigPages)
            {
                page.IsSelected = (page == item);
            }

            // 更新缓存的选中项
            _selectedConfigItem = item;

            // 触发属性更新以刷新页面标题
            RaisePropertyChanged(nameof(SelectedConfigIndex));
            RaisePropertyChanged(nameof(SelectedConfigName));
            RaisePropertyChanged(nameof(SelectedConfigDisplayName));

            // 如果有目标视图，导航过去（跳过重复导航）
            if (!string.IsNullOrEmpty(item.Region))
            {
                if (item.Region != _currentConfigRegion)
                {
                    _currentConfigRegion = item.Region;
                    ShowConfigPlaceholder = false;
                    _regionManager.RequestNavigate("ConfigContentRegion", item.Region);
                }
            }
            else
            {
                // 显示占位符
                _currentConfigRegion = "";
                ShowConfigPlaceholder = true;
            }
        }

        /// <summary>
        /// 调试导航
        /// </summary>
        private void NavigateDebug(NavigationItemModel item)
        {
            if (item == null)
                return;

            // 设置选中状态
            foreach (var page in DebugPages)
            {
                page.IsSelected = (page == item);
            }

            // 更新缓存的选中项
            _selectedDebugItem = item;

            // 触发属性更新以刷新页面标题
            RaisePropertyChanged(nameof(SelectedDebugIndex));
            RaisePropertyChanged(nameof(SelectedDebugName));
            RaisePropertyChanged(nameof(SelectedDebugDisplayName));

            // 如果有目标视图，导航过去（跳过重复导航）
            if (!string.IsNullOrEmpty(item.Region))
            {
                if (item.Region != _currentDebugRegion)
                {
                    _currentDebugRegion = item.Region;
                    ShowDebugPlaceholder = false;
                    _regionManager.RequestNavigate("DebugContentRegion", item.Region);
                }
            }
            else
            {
                // 显示占位符
                _currentDebugRegion = "";
                ShowDebugPlaceholder = true;
            }
        }

        /// <summary>
        /// 当前选中的配置项索引
        /// </summary>
        public int SelectedConfigIndex => _selectedConfigItem?.Index ?? 1;

        /// <summary>
        /// 当前选中的配置项名称（资源键）
        /// </summary>
        public string SelectedConfigName => _selectedConfigItem?.Name ?? "";

        /// <summary>
        /// 当前选中的配置项显示名称（本地化）
        /// </summary>
        public string SelectedConfigDisplayName => _selectedConfigItem?.DisplayName ?? "";

        /// <summary>
        /// 当前选中的调试项索引
        /// </summary>
        public int SelectedDebugIndex => _selectedDebugItem?.Index ?? 1;

        /// <summary>
        /// 当前选中的调试项名称（资源键）
        /// </summary>
        public string SelectedDebugName => _selectedDebugItem?.Name ?? "";

        /// <summary>
        /// 当前选中的调试项显示名称（本地化）
        /// </summary>
        public string SelectedDebugDisplayName => _selectedDebugItem?.DisplayName ?? "";
    }
}
