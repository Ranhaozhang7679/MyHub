#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       FloatingInfoWindowVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.Assets.FloatingInfo.ViewModel
* 文 件 名:       FloatingInfoWindowVM.cs
* 创建时间:       2026/03/24
* 作    者:       Luster
* 版    权:       <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 唯一标识：      a1b2c3d4-e5f6-7890-abcd-ef1234567900
* 创建年份:       2026
************************************************************************************/

#endregion

using Luster.Common.Assets.FloatingInfo.Models;
using Luster.Common.Assets.FloatingInfo.Services;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace Luster.Common.Assets.FloatingInfo.ViewModel
{
    /// <summary>
    /// 浮动信息窗口ViewModel
    /// </summary>
    public class FloatingInfoWindowVM : BindableBase
    {
        #region 私有字段

        private readonly IFloatingInfoConfigService _configService;
        private readonly IDialogService _dialogService;
        private FloatingInfoConfig _config;
        private bool _isMinimized;
        private double _windowWidth = 400;
        private double _windowHeight = 300;
        private double _windowLeft = double.NaN;
        private double _windowTop = double.NaN;
        private string _title;
        private bool _showSettingsButton = true;
        private System.Windows.Window _window;

        #endregion

        #region 属性

        /// <summary>
        /// 窗口标题
        /// </summary>
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        /// <summary>
        /// 页面ID
        /// </summary>
        public string PageId => _config?.PageId;

        /// <summary>
        /// 内容项集合
        /// </summary>
        public ObservableCollection<ContentItem> ContentItems { get; set; }

        /// <summary>
        /// 是否最小化
        /// </summary>
        public bool IsMinimized
        {
            get => _isMinimized;
            set
            {
                if (SetProperty(ref _isMinimized, value))
                {
                    RaisePropertyChanged(nameof(IsNormal));
                }
            }
        }

        /// <summary>
        /// 是否正常状态（非最小化)
        /// </summary>
        public bool IsNormal => !_isMinimized;

        /// <summary>
        /// 窗口宽度
        /// </summary>
        public double WindowWidth
        {
            get => _windowWidth;
            set => SetProperty(ref _windowWidth, value);
        }

        /// <summary>
        /// 窗口高度
        /// </summary>
        public double WindowHeight
        {
            get => _windowHeight;
            set => SetProperty(ref _windowHeight, value);
        }

        /// <summary>
        /// 窗口左边位置
        /// </summary>
        public double WindowLeft
        {
            get => _windowLeft;
            set => SetProperty(ref _windowLeft, value);
        }

        /// <summary>
        /// 窗口顶部位置
        /// </summary>
        public double WindowTop
        {
            get => _windowTop;
            set => SetProperty(ref _windowTop, value);
        }

        /// <summary>
        /// 是否显示设置按钮
        /// </summary>
        public bool ShowSettingsButton
        {
            get => _showSettingsButton;
            set => SetProperty(ref _showSettingsButton, value);
        }

        #endregion

        #region 命令

        private DelegateCommand _minimizeCommand;
        /// <summary>
        /// 最小化命令
        /// </summary>
        public DelegateCommand MinimizeCommand =>
            _minimizeCommand ?? (_minimizeCommand = new DelegateCommand(ExecuteMinimize));

        private DelegateCommand _closeCommand;
        /// <summary>
        /// 关闭命令
        /// </summary>
        public DelegateCommand CloseCommand =>
            _closeCommand ?? (_closeCommand = new DelegateCommand(ExecuteClose));

        private DelegateCommand _openSettingsCommand;
        /// <summary>
        /// 打开设置命令
        /// </summary>
        public DelegateCommand OpenSettingsCommand =>
            _openSettingsCommand ?? (_openSettingsCommand = new DelegateCommand(ExecuteOpenSettings));

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="configService">配置服务</param>
        /// <param name="dialogService">对话框服务</param>
        public FloatingInfoWindowVM(IFloatingInfoConfigService configService, IDialogService dialogService)
        {
            _configService = configService;
            _dialogService = dialogService;
            ContentItems = new ObservableCollection<ContentItem>();
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 初始化配置
        /// </summary>
        /// <param name="config">配置对象</param>
        public void Initialize(FloatingInfoConfig config)
        {
            _config = config;
            Title = config.PageName;
            _windowWidth = config.WindowWidth;
            _windowHeight = config.WindowHeight;
            _windowLeft = config.WindowLeft;
            _windowTop = config.WindowTop;
            _showSettingsButton = config.ShowSettingsButton;

            // 加载内容项
            ContentItems.Clear();
            if (config.ContentItems != null)
            {
                foreach (var item in config.ContentItems)
                {
                    ContentItems.Add(item);
                }
            }

            RaisePropertyChanged(nameof(PageId));
        }

        /// <summary>
        /// 设置窗口引用
        /// </summary>
        /// <param name="window">窗口实例</param>
        public void SetWindow(System.Windows.Window window)
        {
            _window = window;
        }

        /// <summary>
        /// 保存窗口位置
        /// </summary>
        public void SaveWindowPosition()
        {
            if (_config == null || _window == null)
                return;

            _config.WindowLeft = _window.Left;
            _config.WindowTop = _window.Top;
            _config.WindowWidth = _window.Width;
            _config.WindowHeight = _window.Height;
            _configService.SaveConfig(_config);
        }

        #endregion

        #region 命令实现

        private void ExecuteMinimize()
        {
            IsMinimized = true;
            if (_window != null)
            {
                _window.WindowState = WindowState.Minimized;
            }
        }

        private void ExecuteClose()
        {
            SaveWindowPosition();
            _window?.Close();
        }

        private void ExecuteOpenSettings()
        {
            if (_config == null)
                return;

            var parameters = new DialogParameters
            {
                { "PageId", _config.PageId },
                { "PageName", _config.PageName }
            };

            _dialogService.Show("FloatingInfoSettingsDialog", parameters, result =>
            {
                if (result.Result == ButtonResult.OK)
                {
                    // 设置已更新，重新加载配置
                    var updatedConfig = _configService.GetConfig(PageId);
                    if (updatedConfig != null)
                    {
                        Initialize(updatedConfig);
                    }
                }
            });
        }

        #endregion
    }
}
