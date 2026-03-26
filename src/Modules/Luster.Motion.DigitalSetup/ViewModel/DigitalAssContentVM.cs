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
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.DigitalSetup.Datas;
using Luster.Motion.DigitalSetup.Services;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;
using System.Collections.ObjectModel;
using System.IO;
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
                                   IFloatingInfoConfigService floatingInfoConfigService) : base(cBus)
        {
            _regionManager = regionManager;
            _dialogService = dialogService;

            // 通过依赖注入获取设置服务
            _settingsService = settingsService;

            // 通过依赖注入获取浮动信息服务
            _floatingInfoService = floatingInfoService;
            _floatingInfoConfigService = floatingInfoConfigService;

            var recipeDir = cBus.CurrentRecipe?.GetRecipePath();
            var digitalDir = Path.Combine(recipeDir, "DigitalSetUpDataValidation");
            _floatingInfoConfigService.SetConfigPath(recipeDir);
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


    }
}