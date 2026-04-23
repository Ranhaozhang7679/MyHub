#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       FloatingInfoConfig
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.Assets.FloatingInfo.Models
* 文 件 名:       FloatingInfoConfig.cs
* 创建时间:       2026/03/24
* 作    者:       Luster
* 版    权:       <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 唯一标识：      a1b2c3d4-e5f6-7890-abcd-ef1234567894
* 创建年份:       2026
************************************************************************************/

#endregion

using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;

namespace Luster.Common.Assets.FloatingInfo.Models
{
    /// <summary>
    /// 浮动信息窗口配置
    /// </summary>
    public class FloatingInfoConfig : BindableBase
    {
        /// <summary>
        /// 页面唯一标识
        /// </summary>
        private string _pageId;
        public string PageId
        {
            get => _pageId;
            set => SetProperty(ref _pageId, value);
        }

        /// <summary>
        /// 页面名称（用于显示标题）
        /// </summary>
        private string _pageName;
        public string PageName
        {
            get => _pageName;
            set => SetProperty(ref _pageName, value);
        }

        /// <summary>
        /// 是否启用浮动信息显示
        /// </summary>
        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        /// <summary>
        /// 窗口宽度
        /// </summary>
        private double _windowWidth = 400;
        public double WindowWidth
        {
            get => _windowWidth;
            set => SetProperty(ref _windowWidth, value);
        }

        /// <summary>
        /// 窗口高度
        /// </summary>
        private double _windowHeight = 300;
        public double WindowHeight
        {
            get => _windowHeight;
            set => SetProperty(ref _windowHeight, value);
        }

        /// <summary>
        /// 窗口初始X位置（屏幕坐标）
        /// </summary>
        private double _windowLeft = double.NaN;
        public double WindowLeft
        {
            get => _windowLeft;
            set => SetProperty(ref _windowLeft, value);
        }

        /// <summary>
        /// 窗口初始Y位置（屏幕坐标）
        /// </summary>
        private double _windowTop = double.NaN;
        public double WindowTop
        {
            get => _windowTop;
            set => SetProperty(ref _windowTop, value);
        }

        /// <summary>
        /// 是否显示设置按钮
        /// </summary>
        private bool _showSettingsButton = true;
        public bool ShowSettingsButton
        {
            get => _showSettingsButton;
            set => SetProperty(ref _showSettingsButton, value);
        }

        /// <summary>
        /// 内容项集合
        /// </summary>
        private ObservableCollection<ContentItem> _contentItems;
        public ObservableCollection<ContentItem> ContentItems
        {
            get => _contentItems;
            set => SetProperty(ref _contentItems, value);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public FloatingInfoConfig()
        {
            ContentItems = new ObservableCollection<ContentItem>();
        }

        /// <summary>
        /// 创建默认配置
        /// </summary>
        /// <param name="pageId">页面ID</param>
        /// <param name="pageName">页面名称</param>
        /// <returns>默认配置</returns>
        public static FloatingInfoConfig CreateDefault(string pageId, string pageName)
        {
            return new FloatingInfoConfig
            {
                PageId = pageId,
                PageName = pageName,
                IsEnabled = true,
                WindowWidth = 400,
                WindowHeight = 300,
                ContentItems = new ObservableCollection<ContentItem>()
            };
        }

        /// <summary>
        /// 克隆配置
        /// </summary>
        /// <returns>克隆的配置</returns>
        public FloatingInfoConfig Clone()
        {
            var clone = new FloatingInfoConfig
            {
                PageId = this.PageId,
                PageName = this.PageName,
                IsEnabled = this.IsEnabled,
                WindowWidth = this.WindowWidth,
                WindowHeight = this.WindowHeight,
                WindowLeft = this.WindowLeft,
                WindowTop = this.WindowTop,
                ShowSettingsButton = this.ShowSettingsButton
            };

            foreach (var item in this.ContentItems)
            {
                if (item is TextContentItem textItem)
                {
                    clone.ContentItems.Add(new TextContentItem
                    {
                        Order = textItem.Order,
                        Text = textItem.Text,
                        FontSize = textItem.FontSize,
                        FontWeight = textItem.FontWeight,
                        TextAlignment = textItem.TextAlignment,
                        TextWrapping = textItem.TextWrapping,
                        Foreground = textItem.Foreground,
                        Margin = textItem.Margin
                    });
                }
                else if (item is ImageContentItem imageItem)
                {
                    clone.ContentItems.Add(new ImageContentItem
                    {
                        Order = imageItem.Order,
                        ImagePath = imageItem.ImagePath,
                        MaxWidth = imageItem.MaxWidth,
                        MaxHeight = imageItem.MaxHeight,
                        Stretch = imageItem.Stretch,
                        Margin = imageItem.Margin
                    });
                }
            }

            return clone;
        }
    }
}
