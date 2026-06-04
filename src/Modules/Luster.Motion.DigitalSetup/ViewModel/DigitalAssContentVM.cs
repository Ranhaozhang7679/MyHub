#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       MainContentVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.EditorUI.ViewModel
* 文 件 名:       MainContentVM.cs
* 创建时间:       2022/5/24 10:54:07
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      4fd534e1-34fc-4474-bae1-fa2f2c671817
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/24 10:54:07
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.Assets.FloatingInfo.Services;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.DataStruct;
using Luster.Motion.DigitalSetup.Datas;
using Luster.Motion.DigitalSetup.Services;
using Luster.Motion.TaskFlow.Engine;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace Luster.Motion.DigitalSetup.ViewModel
{
    /// <summary>
    /// 流程首页
    /// </summary>
    public class DigitalAssContentVM : MotionPageVM
    {
        private IRegionManager _regionManager;

        /// <summary>
        /// 弹窗
        /// </summary>
        private IDialogService _dialogService;

        /// <summary>
        /// 页面启用设置服务
        /// </summary>
        private PageEnableSettingsService _settingsService;

        /// <summary>
        /// 浮动信息服务
        /// </summary>
        private IFloatingInfoService _floatingInfoService;

        private IFloatingInfoConfigService _floatingInfoConfigService;

        /// <summary>
        /// 点检状态服务
        /// </summary>
        private CheckStatusService _checkStatusService;

        /// <summary>
        /// 菜单信息
        /// </summary>
        private ObservableCollection<DigitalAssPageModel> _pages;
        public ObservableCollection<DigitalAssPageModel> Pages
        {
            get { return _pages; }
            set { SetProperty(ref _pages, value); }
        }

        /// <summary>
        /// 控制左侧菜单区域的显示
        /// </summary>
        private bool _reportSelectVisible = true;
        public bool ReportSelectVisible
        {
            get { return _reportSelectVisible; }
            set { SetProperty(ref _reportSelectVisible, value); }
        }

        /// <summary>
        /// 设备是否正在运行（用于界面禁用和提示显示）
        /// </summary>
        private bool _isMachineRunning = false;
        public bool IsMachineRunning
        {
            get { return _isMachineRunning; }
            set { SetProperty(ref _isMachineRunning, value); }
        }


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="cBus"></param>
        /// <param name="bus"></param>
        public DigitalAssContentVM(ICommonBus cBus,
                                   IRegionManager regionManager,
                                   IDialogService dialogService,
                                   Dispatcher dispatcher,
                                   PageEnableSettingsService settingsService,
                                   IFloatingInfoService floatingInfoService,
                                   IFloatingInfoConfigService floatingInfoConfigService,
                                   CheckStatusService checkStatusService) : base(cBus)
        {
            _regionManager = regionManager;
            _dialogService = dialogService;

            // 通过依赖注入获取设置服务
            _settingsService = settingsService;

            // 通过依赖注入获取浮动信息服务
            _floatingInfoService = floatingInfoService;
            _floatingInfoConfigService = floatingInfoConfigService;

            // 通过依赖注入获取点检状态服务
            _checkStatusService = checkStatusService;

            var recipeDir = cBus.CurrentRecipe?.GetRecipePath();
            var digitalDir = Path.Combine(recipeDir, "DigitalSetUpDataValidation");

            // 设置配置路径
            _floatingInfoConfigService.SetConfigPath(recipeDir);
            _checkStatusService.SetConfigPath(recipeDir);
            PageStatusService.Instance.SetConfigPath(recipeDir);

            // 订阅状态变更事件
            _checkStatusService.StatusChanged += OnCheckStatusChanged;

            // 订阅设备运行状态事件，用于禁用界面
            cBus.EventBus.GetEvent<OperationEvent>().Subscribe(OnMachineStatusChanged);

            BuildPages();
        }


        protected override void RegisterEvent(IEventAggregator bus)
        {
            base.RegisterEvent(bus);

        }

        /// <summary>
        /// 获取菜单信息，并应用本地保存的启用设置
        /// </summary>
        private void BuildPages()
        {
            // 首先加载并应用本地配置
            var settings = _settingsService.LoadOrMergeWithDefaults();
            _settingsService.ApplySettings(settings);

            Pages = new ObservableCollection<DigitalAssPageModel>();
            Pages.AddRange(DigitalAssPageModel.Pages);

            // 初始化每个一级页面的 SubPages 属性，用于状态聚合
            foreach (var page in Pages)
            {
                var subPages = DigitalAssPageModel.GetSubPages(page.Region);
                if (subPages != null && subPages.Count > 0)
                {
                    page.SubPages = subPages;
                }
            }

            // 启动时延迟加载所有一级页面的点检状态
            LoadAllCheckStatusOnStartup();
        }

        /// <summary>
        /// 启动时加载所有一级页面的点检状态
        /// 直接从 CheckStatusService 读取记录并计算聚合状态，同时更新子页面状态
        /// </summary>
        private void LoadAllCheckStatusOnStartup()
        {
            // 使用延迟加载，等待 CheckStatusService 完成初始化
            System.Threading.Timer timer = null;
            int retryCount = 0;
            const int maxRetries = 10;

            timer = new System.Threading.Timer((state) =>
            {
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        // 直接从 CheckStatusService 获取所有记录，按父页面分组计算聚合状态
                        int loadedCount = 0;

                        foreach (var page in Pages)
                        {
                            // 获取该父页面下的所有子页面记录
                            var records = _checkStatusService.GetRecordsByParentRegion(page.Region);

                            if (records != null && records.Count > 0)
                            {
                                // 获取子页面列表
                                var subPages = DigitalAssPageModel.GetSubPages(page.Region);

                                // 先更新每个子页面的状态
                                foreach (var record in records)
                                {
                                    if (subPages != null)
                                    {
                                        var subPage = subPages.FirstOrDefault(sp => sp.PageKey == record.PageKey);
                                        if (subPage != null)
                                        {
                                            subPage.CheckStatus = record.Status;
                                            subPage.LastCheckTime = record.CheckTime;
                                            subPage.LastCheckOperator = record.Operator;
                                            subPage.CheckRemark = record.Remark;
                                        }
                                    }
                                }

                                // 优先使用持久化的总状态（如AutoVerification在子界面中已聚合并保存的状态）
                                var overallRecord = records.FirstOrDefault(r => r.SubPageName == "Overall");
                                if (overallRecord != null)
                                {
                                    page.CheckStatus = overallRecord.Status;
                                }
                                else
                                {
                                    CheckStatus aggregatedStatus = AggregateStatusFromRecords(records);
                                    page.CheckStatus = aggregatedStatus;
                                }
                                loadedCount++;
                            }
                            else
                            {
                                // 没有记录，设置为未点检
                                page.CheckStatus = CheckStatus.NotChecked;
                            }
                        }

                        timer?.Dispose();
                        System.Diagnostics.Debug.WriteLine($"启动时加载点检状态完成，加载了 {loadedCount}/{Pages.Count} 个页面");
                    }
                    catch (Exception ex)
                    {
                        retryCount++;
                        System.Diagnostics.Debug.WriteLine($"启动时加载点检状态失败: {ex.Message}，第 {retryCount} 次重试");

                        if (retryCount >= maxRetries)
                        {
                            timer?.Dispose();
                        }
                    }
                });
            }, null, 1000, 500); // 1秒后开始，每500ms检查一次
        }

        /// <summary>
        /// 从点检记录中聚合计算状态
        /// 规则: 任一 NG → 整体 NG；全部 OK → OK；否则 NotChecked
        /// </summary>
        private CheckStatus AggregateStatusFromRecords(System.Collections.Generic.List<PageCheckRecord> records)
        {
            if (records == null || records.Count == 0)
                return CheckStatus.NotChecked;

            bool hasNG = false;
            bool hasOK = false;
            bool hasNotChecked = false;

            foreach (var record in records)
            {
                switch (record.Status)
                {
                    case CheckStatus.CheckedFail:
                        hasNG = true;
                        break; // 任一 NG，整体就是 NG
                    case CheckStatus.CheckedOK:
                        hasOK = true;
                        break;
                    case CheckStatus.NotChecked:
                        hasNotChecked = true;
                        break;
                }
            }

            // 有 NG 直接返回 NG
            if (hasNG)
                return CheckStatus.CheckedFail;

            // 有未点检的，返回 NotChecked
            if (hasNotChecked)
                return CheckStatus.NotChecked;

            // 全部 OK
            if (hasOK)
                return CheckStatus.CheckedOK;

            return CheckStatus.NotChecked;
        }

        /// <summary>
        /// 状态变更事件处理 - 通知所有已注册的子页面更新状态，并刷新父级页面的聚合状态
        /// </summary>
        private void OnCheckStatusChanged(string pageKey, CheckStatus status)
        {
            // 在UI线程上更新界面
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher != null)
            {
                dispatcher.Invoke(() =>
                {
                    // 遍历所有一级页面，查找并更新对应的子页面
                    foreach (var page in Pages)
                    {
                        var subPages = DigitalAssPageModel.GetSubPages(page.Region);
                        if (subPages != null)
                        {
                            foreach (var subPage in subPages)
                            {
                                // 确保子页面有 ParentRegion
                                if (string.IsNullOrEmpty(subPage.ParentRegion))
                                {
                                    subPage.ParentRegion = page.Region;
                                }

                                // 检查是否是当前要更新的页面
                                if (subPage.PageKey == pageKey)
                                {
                                    subPage.CheckStatus = status;

                                    // 同时更新点检记录的详细信息
                                    var record = _checkStatusService.GetRecord(pageKey);
                                    if (record != null)
                                    {
                                        subPage.LastCheckTime = record.CheckTime;
                                        subPage.LastCheckOperator = record.Operator;
                                        subPage.CheckRemark = record.Remark;
                                    }

                                    // 刷新父级页面的聚合状态（用于 lstDevice 中的状态圆圈显示）
                                    page.RefreshCheckStatus();

                                    return;
                                }
                            }
                        }
                    }
                });
            }
        }

        /// <summary>
        /// 设备运行状态变更处理 - 运行时禁用界面，停止时恢复
        /// </summary>
        private void OnMachineStatusChanged(StatusChanged statusChanged)
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                if (statusChanged.Dst == EngineStatus.Running || statusChanged.Dst == EngineStatus.MaterialPending)
                {
                    IsMachineRunning = true;
                }
                else if (statusChanged.Dst == EngineStatus.Stop || statusChanged.Dst == EngineStatus.Idle)
                {
                    IsMachineRunning = false;
                }
            });
        }

        /// <summary>
        /// 菜单切换功能
        /// </summary>
        private DelegateCommand<DigitalAssPageModel> _selectedCommand;
        public DelegateCommand<DigitalAssPageModel> SelectedCommand => _selectedCommand ?? (_selectedCommand = new DelegateCommand<DigitalAssPageModel>((item) =>
        {
            if (item == null) return;
            SetSelected(item.Name);
            _regionManager.RequestNavigate("DigitalAssEditorRegion", item.Region);

            // 显示浮动信息窗口
            ShowFloatingInfoForPage(item.Region);

            // 导航完成后，自动加载该页面的所有子页面点检状态
            LoadCheckStatusDelayed(item.Region);
        }));

        /// <summary>
        /// 显示页面对应的浮动信息窗口
        /// </summary>
        /// <param name="pageRegion">页面Region名称</param>
        private void ShowFloatingInfoForPage(string pageRegion)
        {
            try
            {
                // 先隐藏所有浮动窗口
                _floatingInfoService?.HideAllFloatingInfo();

                // 显示当前页面的浮动窗口
                _floatingInfoService?.ShowFloatingInfo(pageRegion);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"显示浮动信息窗口失败: {ex.Message}");
            }
        }

        private void SetSelected(string name)
        {
            foreach (var item in Pages)
            {
                if (item.Name != name)
                {
                    item.IsSelected = false;
                }
                else
                {
                    item.IsSelected = true;
                }
            }
        }


        /// <summary>
        /// 模块加载
        /// </summary>
        private DelegateCommand<object> _loadedCommand;
        public DelegateCommand<object> LoadedCommand => _loadedCommand ?? (_loadedCommand = new DelegateCommand<object>((obj) =>
        {
            // 默认日志页面
            _regionManager.RequestNavigate("LogContentRegon_Ass", "LogContent");
        }));

        /// <summary>
        /// 打开设置对话框（仅管理员可用）
        /// </summary>
        private DelegateCommand _openSettingsCommand;
        public DelegateCommand OpenSettingsCommand => _openSettingsCommand ?? (_openSettingsCommand = new DelegateCommand(() =>
        {
            if (!IsAdmin)
            {
                System.Windows.MessageBox.Show("当前用户权限不足，无法打开设置！", "提示", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            _dialogService.Show("PageEnableSettingsDialog", new DialogParameters(), (result) =>
            {
                if (result.Result == ButtonResult.OK)
                {
                    // 设置已保存并应用，刷新页面列表
                    BuildPages();
                }
            });
        }));

        /// <summary>
        /// 延迟加载指定页面的点检状态（等待 ViewModel 创建完成）
        /// </summary>
        private void LoadCheckStatusDelayed(string parentRegion)
        {
            int retryCount = 0;
            const int maxRetries = 10;

            // 使用延迟加载，等待子页面 ViewModel 创建完成
            System.Threading.Timer timer = null;
            timer = new System.Threading.Timer((state) =>
            {
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        var subPages = DigitalAssPageModel.GetSubPages(parentRegion);
                        if (subPages != null && subPages.Count > 0)
                        {
                            // 子页面已注册，加载状态
                            _checkStatusService.ApplyStatusToPages(subPages, parentRegion);

                            // 直接从 CheckStatusService 获取聚合状态并设置父级页面状态
                            var records = _checkStatusService.GetRecordsByParentRegion(parentRegion);
                            var parentPage = Pages.FirstOrDefault(p => p.Region == parentRegion);
                            if (parentPage != null)
                            {
                                if (records != null && records.Count > 0)
                                {
                                    // 有记录，优先使用持久化的总状态
                                    var overallRecord = records.FirstOrDefault(r => r.SubPageName == "Overall");
                                    parentPage.CheckStatus = overallRecord != null
                                        ? overallRecord.Status
                                        : AggregateStatusFromRecords(records);
                                }
                                else
                                {
                                    // 没有记录，基于子页面聚合
                                    parentPage.RefreshCheckStatus();
                                }
                            }

                            timer?.Dispose();
                            System.Diagnostics.Debug.WriteLine($"成功加载页面 {parentRegion} 的点检状态");
                        }
                        else
                        {
                            retryCount++;
                            if (retryCount >= maxRetries)
                            {
                                // 超过最大重试次数，直接从 CheckStatusService 获取状态
                                var records = _checkStatusService.GetRecordsByParentRegion(parentRegion);
                                var parentPage = Pages.FirstOrDefault(p => p.Region == parentRegion);
                                if (parentPage != null)
                                {
                                    if (records != null && records.Count > 0)
                                    {
                                        var overallRec = records.FirstOrDefault(r => r.SubPageName == "Overall");
                                        parentPage.CheckStatus = overallRec != null
                                            ? overallRec.Status
                                            : AggregateStatusFromRecords(records);
                                    }
                                    else
                                    {
                                        parentPage.CheckStatus = CheckStatus.NotChecked;
                                    }
                                }

                                timer?.Dispose();
                                System.Diagnostics.Debug.WriteLine($"加载页面 {parentRegion} 的点检状态超时，使用持久化数据");
                            }
                            // 如果子页面还未注册，等待下次重试
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"延迟加载点检状态失败: {ex.Message}");
                        timer?.Dispose();
                    }
                });
            }, null, 500, 500); // 500ms 后开始，每 500ms 检查一次
        }



    }
}