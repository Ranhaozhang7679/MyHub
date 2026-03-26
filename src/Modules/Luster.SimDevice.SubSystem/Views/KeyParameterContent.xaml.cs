using Luster.SimDevice.SubSystem.ViewModel;
using Microsoft.Win32;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Luster.SimDevice.SubSystem.Views
{
    /// <summary>
    /// KeyParameterContent.xaml 的交互逻辑
    /// </summary>
    public partial class KeyParameterContent : UserControl
    {
        private KeyParameterContentVM _viewModel;

        public KeyParameterContent()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // 调用 ViewModel 的加载方法
                _viewModel = DataContext as KeyParameterContentVM;
                _viewModel?.OnViewLoaded();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            // 清理资源
        }

        private void ExpandProdDataButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is PDCAKeyParameterRow row)
            {
                row.IsProdDataExpanded = !row.IsProdDataExpanded;

                // 更新箭头图标方向
                if (button.Template.FindName("ExpandIcon", button) is Path path)
                {
                    path.Data = row.IsProdDataExpanded ?
                        Geometry.Parse("M0,6 L6,0 12,6") : // 向上箭头
                        Geometry.Parse("M0,0 L6,6 12,0");  // 向下箭头
                }

                // 更新文本显示
                if (VisualTreeHelper.GetParent(button) is Grid grid)
                {
                    var textBlock = grid.Children.OfType<TextBlock>().FirstOrDefault();
                    if (textBlock != null)
                    {
                        if (row.IsProdDataExpanded)
                        {
                            textBlock.MaxHeight = 1000;
                            textBlock.TextTrimming = TextTrimming.None;
                        }
                        else
                        {
                            textBlock.MaxHeight = 60;
                            textBlock.TextTrimming = TextTrimming.CharacterEllipsis;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 检查是否有数据
        /// </summary>
        public bool HasData()
        {
            return _viewModel?.ParameterRows?.Count > 0;
        }

        /// <summary>
        /// 获取模块总数
        /// </summary>
        public int GetModuleCount()
        {
            return _viewModel?.TotalModules ?? 0;
        }

        /// <summary>
        /// 获取加载状态
        /// </summary>
        public bool IsLoading()
        {
            return _viewModel?.IsLoading ?? false;
        }
    }

    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}