using Luster.SimDevice.SubSystem.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Luster.SimDevice.SubSystem.Views
{
    public partial class PositionParameterContent : UserControl
    {
        // 存储当前选中的行
        private Border _selectedRow = null;
        private Brush _originalBackground = null;

        public PositionParameterContent()
        {
            InitializeComponent();
        }

        private void GridSplitter_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (sender is GridSplitter splitter && splitter.Tag is string columnIndexStr)
            {
                if (int.TryParse(columnIndexStr, out int columnIndex))
                {
                    var headerGrid = splitter.Parent as Grid;
                    if (headerGrid != null && columnIndex >= 0 && columnIndex < headerGrid.ColumnDefinitions.Count)
                    {
                        var column = headerGrid.ColumnDefinitions[columnIndex];
                        var newWidth = column.ActualWidth + e.HorizontalChange;

                        if (newWidth < column.MinWidth)
                        {
                            newWidth = column.MinWidth;
                        }
                        column.Width = new GridLength(newWidth);
                        headerGrid.UpdateLayout();
                        HeaderScrollViewer?.UpdateLayout();
                    }
                }
            }
        }

        private void Row_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border border && border != _selectedRow)
            {
                _originalBackground = border.Background;

                // 设置悬停背景色（浅灰色）
                border.Background = new SolidColorBrush(Color.FromArgb(255, 245, 245, 245));
            }
        }

        private void Row_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Border border && border != _selectedRow)
            {
                // 恢复原始背景色
                border.Background = Brushes.White;
            }
        }

        // 行鼠标释放事件
        private void Row_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void ComboBox_DropDownOpened(object sender, EventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.DataContext is PositionParameterContentVM viewModel)
            {
                // 调用ViewModel中的刷新方法
                viewModel.RefreshStationList();
            }
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is PositionParameterContentVM viewModel)
            {
                viewModel.OnViewActivated();
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is PositionParameterContentVM viewModel)
            {
                viewModel.OnViewDeactivated();
            }
        }
        private void ContentScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // 同步列头的横向滚动位置
            if (e.HorizontalChange != 0)
            {
                HeaderScrollViewer.ScrollToHorizontalOffset(e.HorizontalOffset);
            }
        }

    }
}