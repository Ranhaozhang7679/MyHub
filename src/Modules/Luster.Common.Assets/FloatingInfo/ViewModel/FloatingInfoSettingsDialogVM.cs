#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       FloatingInfoSettingsDialogVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.Assets.FloatingInfo.ViewModel
* 文 件 名:       FloatingInfoSettingsDialogVM.cs
* 创建时间:       2026/03/24
* 作    者:       Luster
* 版    权:       <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 唯一标识：      a1b2c3d4-e5f6-7890-abcd-ef1234567899
* 创建年份:       2026
************************************************************************************/

#endregion

using Luster.Common.Assets.FloatingInfo.Models;
using Luster.Common.Assets.FloatingInfo.Services;
using Luster.Common.Assets.ViewModel;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace Luster.Common.Assets.FloatingInfo.ViewModel
{
    /// <summary>
    /// 浮动信息设置对话框ViewModel
    /// </summary>
    public class FloatingInfoSettingsDialogVM : BaseDialogVM
    {
        private readonly IFloatingInfoConfigService _configService;

        private string _pageId;
        private string _pageName;

        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        private double _windowWidth = 400;
        public double WindowWidth
        {
            get => _windowWidth;
            set => SetProperty(ref _windowWidth, value);
        }

        private double _windowHeight = 300;
        public double WindowHeight
        {
            get => _windowHeight;
            set => SetProperty(ref _windowHeight, value);
        }

        private double _windowLeft = double.NaN;
        public double WindowLeft
        {
            get => _windowLeft;
            set => SetProperty(ref _windowLeft, value);
        }

        private double _windowTop = double.NaN;
        public double WindowTop
        {
            get => _windowTop;
            set => SetProperty(ref _windowTop, value);
        }

        /// <summary>
        /// 内容项集合
        /// </summary>
        public ObservableCollection<ContentItem> ContentItems { get; set; }

        /// <summary>
        /// 选中的内容项
        /// </summary>
        private ContentItem _selectedContentItem;
        public ContentItem SelectedContentItem
        {
            get => _selectedContentItem;
            set
            {
                SetProperty(ref _selectedContentItem, value);
                UpdateEditCommands();
            }
        }

        /// <summary>
        /// 可用的内容类型列表
        /// </summary>
        public Array ContentTypeValues => Enum.GetValues(typeof(ContentType));

        /// <summary>
        /// 当前选择的新增内容类型
        /// </summary>
        private ContentType _selectedNewContentType = ContentType.Text;
        public ContentType SelectedNewContentType
        {
            get => _selectedNewContentType;
            set => SetProperty(ref _selectedNewContentType, value);
        }

        /// <summary>
        /// 当前选中的内容项类型（用于编辑）
        /// </summary>
        private ContentType _selectedContentType;
        public ContentType SelectedContentType
        {
            get => _selectedContentType;
            set
            {
                if (_selectedContentType != value && SelectedContentItem != null)
                {
                    ChangeContentItemType(value);
                }
                SetProperty(ref _selectedContentType, value);
            }
        }

        public DelegateCommand SaveCommand { get; }
        public DelegateCommand CancelCommand { get; }
        public DelegateCommand AddContentItemCommand { get; }
        public DelegateCommand RemoveContentItemCommand { get; }
        public DelegateCommand MoveUpCommand { get; }
        public DelegateCommand MoveDownCommand { get; }
        public DelegateCommand<ImageContentItem> SelectImageCommand { get; }

        public FloatingInfoSettingsDialogVM(IFloatingInfoConfigService configService)
        {
            _configService = configService;
            Title = "浮动信息设置";

            ContentItems = new ObservableCollection<ContentItem>();

            SaveCommand = new DelegateCommand(ExecuteSave);
            CancelCommand = new DelegateCommand(ExecuteCancel);
            AddContentItemCommand = new DelegateCommand(ExecuteAddContentItem);
            RemoveContentItemCommand = new DelegateCommand(ExecuteRemoveContentItem, CanExecuteRemoveContentItem);
            MoveUpCommand = new DelegateCommand(ExecuteMoveUp, CanExecuteMoveUp);
            MoveDownCommand = new DelegateCommand(ExecuteMoveDown, CanExecuteMoveDown);
            SelectImageCommand = new DelegateCommand<ImageContentItem>(ExecuteSelectImage);
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);

            if (parameters.TryGetValue("PageId", out string pageId))
            {
                _pageId = pageId;
            }

            if (parameters.TryGetValue("PageName", out string pageName))
            {
                _pageName = pageName;
                Title = $"浮动信息设置 - {_pageName}";
            }

            // 加载现有配置
            var config = _configService.GetConfig(_pageId);
            if (config != null)
            {
                IsEnabled = config.IsEnabled;
                WindowWidth = config.WindowWidth;
                WindowHeight = config.WindowHeight;
                WindowLeft = config.WindowLeft;
                WindowTop = config.WindowTop;

                // 加载内容项
                ContentItems.Clear();
                if (config.ContentItems != null)
                {
                    foreach (var item in config.ContentItems)
                    {
                        ContentItems.Add(item);
                    }
                }
            }
        }

        private void UpdateEditCommands()
        {
            RemoveContentItemCommand?.RaiseCanExecuteChanged();
            MoveUpCommand?.RaiseCanExecuteChanged();
            MoveDownCommand?.RaiseCanExecuteChanged();

            if (SelectedContentItem != null)
            {
                _selectedContentType = SelectedContentItem.ContentType;
                RaisePropertyChanged(nameof(SelectedContentType));
            }
        }

        private void ExecuteAddContentItem()
        {
            ContentItem newItem;

            if (SelectedNewContentType == ContentType.Image)
            {
                newItem = new ImageContentItem
                {
                    Order = ContentItems.Count + 1,
                    MaxWidth = WindowWidth - 20,
                    MaxHeight = WindowHeight / 2
                };
            }
            else
            {
                newItem = new TextContentItem
                {
                    Order = ContentItems.Count + 1,
                    Text = "新文本内容",
                    FontSize = 14
                };
            }

            ContentItems.Add(newItem);
            SelectedContentItem = newItem;
        }

        private bool CanExecuteRemoveContentItem()
        {
            return SelectedContentItem != null;
        }

        private void ExecuteRemoveContentItem()
        {
            if (SelectedContentItem != null)
            {
                var index = ContentItems.IndexOf(SelectedContentItem);
                ContentItems.Remove(SelectedContentItem);

                // 重新排序
                ReorderItems();

                // 选择下一个项
                if (ContentItems.Count > 0)
                {
                    SelectedContentItem = ContentItems[Math.Min(index, ContentItems.Count - 1)];
                }
                else
                {
                    SelectedContentItem = null;
                }
            }
        }

        private bool CanExecuteMoveUp()
        {
            if (SelectedContentItem == null) return false;
            return ContentItems.IndexOf(SelectedContentItem) > 0;
        }

        private void ExecuteMoveUp()
        {
            if (SelectedContentItem == null) return;

            var index = ContentItems.IndexOf(SelectedContentItem);
            if (index > 0)
            {
                ContentItems.Move(index, index - 1);
                ReorderItems();
            }
        }

        private bool CanExecuteMoveDown()
        {
            if (SelectedContentItem == null) return false;
            return ContentItems.IndexOf(SelectedContentItem) < ContentItems.Count - 1;
        }

        private void ExecuteMoveDown()
        {
            if (SelectedContentItem == null) return;

            var index = ContentItems.IndexOf(SelectedContentItem);
            if (index < ContentItems.Count - 1)
            {
                ContentItems.Move(index, index + 1);
                ReorderItems();
            }
        }

        private void ReorderItems()
        {
            for (int i = 0; i < ContentItems.Count; i++)
            {
                ContentItems[i].Order = i + 1;
            }
        }

        private void ExecuteSelectImage(ImageContentItem item)
        {
            if (item == null) return;

            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "选择图片",
                Filter = "图片文件|*.jpg;*.jpeg;*.png;*.bmp;*.gif|所有文件|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                item.ImagePath = dialog.FileName;
            }
        }

        private void ChangeContentItemType(ContentType newType)
        {
            if (SelectedContentItem == null) return;
            if (SelectedContentItem.ContentType == newType) return;

            var index = ContentItems.IndexOf(SelectedContentItem);
            ContentItem newItem;

            if (newType == ContentType.Image)
            {
                newItem = new ImageContentItem
                {
                    Order = SelectedContentItem.Order,
                    MaxWidth = WindowWidth - 20,
                    MaxHeight = WindowHeight / 2
                };
            }
            else
            {
                newItem = new TextContentItem
                {
                    Order = SelectedContentItem.Order,
                    Text = "新文本内容",
                    FontSize = 14
                };
            }

            ContentItems[index] = newItem;
            SelectedContentItem = newItem;
        }

        private void ExecuteSave()
        {
            // 保存配置
            var config = _configService.GetConfig(_pageId);
            if (config != null)
            {
                config.IsEnabled = IsEnabled;
                config.WindowWidth = WindowWidth;
                config.WindowHeight = WindowHeight;
                config.WindowLeft = WindowLeft;
                config.WindowTop = WindowTop;

                // 保存内容项
                config.ContentItems.Clear();
                foreach (var item in ContentItems)
                {
                    config.ContentItems.Add(item);
                }

                _configService.SaveConfig(config);
            }

            RaiseRequestClose(new DialogResult(ButtonResult.OK));
        }

        private void ExecuteCancel()
        {
            RaiseRequestClose(new DialogResult(ButtonResult.Cancel));
        }
    }
}
