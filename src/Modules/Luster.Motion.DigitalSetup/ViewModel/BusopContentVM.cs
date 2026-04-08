using Luster.Common.DataAccess.Repositories;
using Luster.Motion.CommonUI;
using Luster.Motion.DigitalSetup.Datas;
using Luster.Motion.DigitalSetup.Helpers;
using Luster.Motion.DigitalSetup.Services;
using Luster.Motion.DigitalSetup.Views;
using Luster.Motion.EditorUI;
using Luster.Motion.Integration.Web;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace Luster.Motion.DigitalSetup.ViewModel
{
    /// <summary>
    /// BUSOP 一级页面 ViewModel
    /// 管理18个子界面
    /// </summary>
    public class BusopContentVM : BaseAss
    {
        private readonly BusopConfigService _busopConfigService;
        private BusopConfig _busopConfig;

        /// <summary>
        /// 当前选中的子界面实例
        /// </summary>
        private BusopSubItemVM _currentSubItemVM;
        public BusopSubItemVM CurrentSubItemVM
        {
            get => _currentSubItemVM;
            set => SetProperty(ref _currentSubItemVM, value);
        }

        public BusopContentVM(
            IRepository repository,
            IRegionManager regionManager,
            ICommonBus commonBus,
            CSVHelper csvHelper,
            FlowBus flowBus,
            IDialogService dialogService,
            CheckStatusService checkStatusService,
            BusopConfigService busopConfigService)
            : base(repository, regionManager, commonBus, csvHelper, flowBus, dialogService, checkStatusService)
        {
            _busopConfigService = busopConfigService;
            _parentRegionName = "BusopContent";

            // 加载配置
            _busopConfig = _busopConfigService.LoadConfig();

            // 初始化18个子页面
            Pages = new ObservableCollection<CommonPageModel>();
            for (int i = 0; i < _busopConfig.SubItems.Count; i++)
            {
                Pages.Add(new CommonPageModel
                {
                    Name = _busopConfig.SubItems[i].Name,
                    IsSelected = i == 0,
                    Region = "",
                    ViewType = typeof(BusopSubItem)
                });
            }

            // 注册子页面到 DigitalAssPageModel
            DigitalAssPageModel.RegisterSubPages("BusopContent", Pages);

            // 默认选中第一个
            SelectedReportPage = Pages.FirstOrDefault(p => p.IsSelected) ?? Pages.FirstOrDefault();

            // 初始化第一个子界面
            InitializeSubItem(SelectedReportPage);

            // 延迟加载点检状态
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                LoadCheckStatusForAllPages();
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        /// <summary>
        /// 选中子页面时的回调（覆盖 BaseAss 的 SelectedCommand）
        /// </summary>
        protected override void Selected(CommonPageModel page)
        {
            base.Selected(page);
            if (page != null)
            {
                InitializeSubItem(page);
            }
        }

        /// <summary>
        /// 初始化子界面的 ViewModel
        /// </summary>
        private void InitializeSubItem(CommonPageModel page)
        {
            if (page == null || _busopConfig == null)
                return;

            var subItemConfig = _busopConfig.SubItems.FirstOrDefault(s => s.Name == page.Name);
            if (subItemConfig == null)
                return;

            // 通过服务定位器创建子界面 VM 实例
            var subItemVM = (BusopSubItemVM)DigitalSetupServiceLocator.Container.Resolve(typeof(BusopSubItemVM));
            var fullPath = _busopConfigService.GetExcelFullPath(_busopConfig.ExcelFilePath);
            subItemVM.Initialize(subItemConfig, fullPath);
            CurrentSubItemVM = subItemVM;
        }

        /// <summary>
        /// 加载所有子页面的点检状态
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
                        page.ParentRegion = "BusopContent";
                        var record = _checkStatusService.GetRecord(page.PageKey);
                        if (record != null)
                        {
                            page.CheckStatus = record.Status;
                            page.LastCheckTime = record.CheckTime;
                            page.LastCheckOperator = record.Operator;
                            page.CheckRemark = record.Remark;
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
                System.Diagnostics.Debug.WriteLine($"加载 BUSOP 点检状态失败: {ex.Message}");
            }
        }

        protected override void RefreshCheckStatus()
        {
            LoadCheckStatusForAllPages();
        }
    }
}