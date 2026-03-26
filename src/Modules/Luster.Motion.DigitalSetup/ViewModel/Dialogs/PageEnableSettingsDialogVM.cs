using Luster.Motion.CommonUI;
using Luster.Motion.DigitalSetup.Datas;
using Luster.Motion.DigitalSetup.Services;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Luster.Motion.DigitalSetup.ViewModel.Dialogs
{
    /// <summary>
    /// 页面启用设置对话框 ViewModel
    /// </summary>
    public class PageEnableSettingsDialogVM : BindableBase, IDialogAware
    {
        #region 属性

        private readonly PageEnableSettingsService _settingsService;

        private ObservableCollection<PageEnableItemVM> _pageSettings;
        /// <summary>
        /// 页面设置列表
        /// </summary>
        public ObservableCollection<PageEnableItemVM> PageSettings
        {
            get => _pageSettings;
            set => SetProperty(ref _pageSettings, value);
        }

        public string Title => "页面启用设置";

        public event Action<IDialogResult> RequestClose;

        #endregion

        #region 命令

        public ICommand SaveCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        #endregion

        public PageEnableSettingsDialogVM(ICommonBus commonBus, PageEnableSettingsService settingsService)
        {
            _settingsService = settingsService;

            SaveCommand = new DelegateCommand(OnSave);
            CancelCommand = new DelegateCommand(OnCancel);

            LoadSettings();
        }

        /// <summary>
        /// 加载设置
        /// </summary>
        private void LoadSettings()
        {
            var settings = _settingsService.LoadOrMergeWithDefaults();
            PageSettings = new ObservableCollection<PageEnableItemVM>();

            foreach (var pageSetting in settings.PageSettings)
            {
                var pageItemVM = new PageEnableItemVM
                {
                    Name = pageSetting.Name,
                    Region = pageSetting.Region,
                    IsEnabled = pageSetting.IsEnabled
                };

                // 加载子页面设置
                foreach (var subPage in pageSetting.SubPages)
                {
                    pageItemVM.SubPages.Add(new SubPageEnableItemVM
                    {
                        Name = subPage.Name,
                        Region = subPage.Region,
                        IsEnabled = subPage.IsEnabled
                    });
                }

                PageSettings.Add(pageItemVM);
            }
        }

        /// <summary>
        /// 保存设置
        /// </summary>
        private void OnSave()
        {
            var settings = new PageEnableSettings();

            foreach (var pageVM in PageSettings)
            {
                var pageItem = new PageEnableItem
                {
                    Name = pageVM.Name,
                    Region = pageVM.Region,
                    IsEnabled = pageVM.IsEnabled
                };

                foreach (var subVM in pageVM.SubPages)
                {
                    pageItem.SubPages.Add(new SubPageEnableItem
                    {
                        Name = subVM.Name,
                        Region = subVM.Region,
                        IsEnabled = subVM.IsEnabled
                    });
                }

                settings.PageSettings.Add(pageItem);
            }

            _settingsService.Save(settings);
            _settingsService.ApplySettings(settings);

            RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
        }

        /// <summary>
        /// 取消
        /// </summary>
        private void OnCancel()
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
        }

        #region IDialogAware

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
        }

        #endregion
    }

    /// <summary>
    /// 页面启用项 ViewModel（一级按钮）
    /// </summary>
    public class PageEnableItemVM : BindableBase
    {
        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string _region;
        public string Region
        {
            get => _region;
            set => SetProperty(ref _region, value);
        }

        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        private bool _isExpanded = true;
        /// <summary>
        /// 是否展开二级菜单
        /// </summary>
        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }

        private ObservableCollection<SubPageEnableItemVM> _subPages;
        public ObservableCollection<SubPageEnableItemVM> SubPages
        {
            get => _subPages ?? (_subPages = new ObservableCollection<SubPageEnableItemVM>());
            set => SetProperty(ref _subPages, value);
        }

        /// <summary>
        /// 是否有子页面
        /// </summary>
        public bool HasSubPages => SubPages != null && SubPages.Count > 0;

        /// <summary>
        /// 切换展开/折叠状态
        /// </summary>
        public void ToggleExpanded()
        {
            IsExpanded = !IsExpanded;
        }
    }

    /// <summary>
    /// 子页面启用项 ViewModel（二级按钮）
    /// </summary>
    public class SubPageEnableItemVM : BindableBase
    {
        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string _region;
        public string Region
        {
            get => _region;
            set => SetProperty(ref _region, value);
        }

        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }
    }
}
