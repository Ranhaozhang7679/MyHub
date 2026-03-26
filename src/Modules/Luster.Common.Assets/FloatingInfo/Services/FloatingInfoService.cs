#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       FloatingInfoService
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.Assets.FloatingInfo.Services
* 文 件 名:       FloatingInfoService.cs
* 创建时间:       2026/03/24
* 作    者:       Luster
* 版    权:       <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 唯一标识：      a1b2c3d4-e5f6-7890-abcd-ef1234567901
* 创建年份:       2026
************************************************************************************/

#endregion

using Luster.Common.Assets.FloatingInfo.Models;
using Luster.Common.Assets.FloatingInfo.Views;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Luster.Common.Assets.FloatingInfo.Services
{
    /// <summary>
    /// 浮动信息窗口服务实现
    /// </summary>
    public class FloatingInfoService : IFloatingInfoService
    {
        #region 私有字段

        private readonly IFloatingInfoConfigService _configService;
        private readonly IDialogService _dialogService;
        private readonly Dictionary<string, FloatingInfoWindow> _activeWindows = new Dictionary<string, FloatingInfoWindow>();
        private readonly Dictionary<string, FloatingInfoConfig> _windowConfigs = new Dictionary<string, FloatingInfoConfig>();

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="configService">配置服务</param>
        /// <param name="dialogService">对话框服务</param>
        public FloatingInfoService(IFloatingInfoConfigService configService, IDialogService dialogService)
        {
            _configService = configService;
            _dialogService = dialogService;
        }

        #endregion

        #region IFloatingInfoService 实现

        /// <summary>
        /// 显示指定页面的浮动信息窗口
        /// </summary>
        /// <param name="pageId">页面唯一标识</param>
        public void ShowFloatingInfo(string pageId)
        {
            if (string.IsNullOrEmpty(pageId))
                return;

            // 如果窗口已存在，激活它
            if (_activeWindows.TryGetValue(pageId, out var existingWindow))
            {
                existingWindow.Activate();
                existingWindow.Show();
                existingWindow.Focus();
                return;
            }

            // 获取配置
            var config = _configService.GetConfig(pageId);
            if (config == null || !config.IsEnabled)
                return;

            // 创建新窗口
            var window = new FloatingInfoWindow
            {
                Width = config.WindowWidth,
                Height = config.WindowHeight,
                Left = double.IsNaN(config.WindowLeft) ? SystemParameters.PrimaryScreenWidth / 2 - config.WindowWidth / 2 : config.WindowLeft,
                Top = double.IsNaN(config.WindowTop) ? SystemParameters.PrimaryScreenHeight / 10 : config.WindowTop
            };

            // 设置DataContext
            var viewModel = new ViewModel.FloatingInfoWindowVM(_configService, _dialogService);
            viewModel.Initialize(config);
            window.DataContext = viewModel;

            // 窗口关闭时清理
            window.Closed += (s, e) =>
            {
                _activeWindows.Remove(pageId);
            };

            _activeWindows[pageId] = window;
            window.Show();
        }

        /// <summary>
        /// 隐藏指定页面的浮动信息窗口
        /// </summary>
        /// <param name="pageId">页面唯一标识</param>
        public void HideFloatingInfo(string pageId)
        {
            if (string.IsNullOrEmpty(pageId))
                return;

            if (_activeWindows.TryGetValue(pageId, out var window))
            {
                window.Close();
                _activeWindows.Remove(pageId);
            }
        }

        /// <summary>
        /// 隐藏所有浮动信息窗口
        /// </summary>
        public void HideAllFloatingInfo()
        {
            var pageIds = _activeWindows.Keys.ToList();
            foreach (var pageId in pageIds)
            {
                HideFloatingInfo(pageId);
            }
        }

        /// <summary>
        /// 检查指定页面的浮动信息窗口是否可见
        /// </summary>
        /// <param name="pageId">页面唯一标识</param>
        /// <returns>是否可见</returns>
        public bool IsVisible(string pageId)
        {
            if (string.IsNullOrEmpty(pageId))
                return false;

            return _activeWindows.TryGetValue(pageId, out var window) && window.IsVisible;
        }

        /// <summary>
        /// 最小化指定页面的浮动信息窗口
        /// </summary>
        /// <param name="pageId">页面唯一标识</param>
        public void MinimizeFloatingInfo(string pageId)
        {
            if (string.IsNullOrEmpty(pageId))
                return;

            if (_activeWindows.TryGetValue(pageId, out var window))
            {
                window.WindowState = WindowState.Minimized;
            }
        }

        /// <summary>
        /// 恢复最小化的浮动信息窗口
        /// </summary>
        /// <param name="pageId">页面唯一标识</param>
        public void RestoreFloatingInfo(string pageId)
        {
            if (string.IsNullOrEmpty(pageId))
                return;

            if (_activeWindows.TryGetValue(pageId, out var window))
            {
                window.WindowState = WindowState.Normal;
                window.Activate();
            }
        }

        /// <summary>
        /// 注册页面配置
        /// </summary>
        /// <param name="config">配置对象</param>
        public void RegisterConfig(FloatingInfoConfig config)
        {
            if (config == null || string.IsNullOrEmpty(config.PageId))
                return;

            _windowConfigs[config.PageId] = config;
            _configService.SaveConfig(config);
        }

        /// <summary>
        /// 打开设置对话框
        /// </summary>
        /// <param name="pageId">页面唯一标识</param>
        public void OpenSettings(string pageId)
        {
            if (string.IsNullOrEmpty(pageId))
                return;

            var parameters = new DialogParameters
            {
                { "PageId", pageId }
            };

            _dialogService.ShowDialog("FloatingInfoSettingsDialog", parameters, result =>
            {
                if (result.Result == ButtonResult.OK)
                {
                    // 设置已更新，如果窗口正在显示则刷新
                    if (_activeWindows.TryGetValue(pageId, out var window))
                    {
                        var config = _configService.GetConfig(pageId);
                        if (config != null && window.DataContext is ViewModel.FloatingInfoWindowVM vm)
                        {
                            vm.Initialize(config);
                        }
                    }
                }
            });
        }

        /// <summary>
        /// 获取所有活动窗口的页面ID
        /// </summary>
        /// <returns>页面ID列表</returns>
        public IReadOnlyCollection<string> GetActiveWindowPageIds()
        {
            return _activeWindows.Keys.ToList().AsReadOnly();
        }

        #endregion
    }
}
