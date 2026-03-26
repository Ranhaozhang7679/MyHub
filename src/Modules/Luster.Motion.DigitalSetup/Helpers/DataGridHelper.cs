using System;
using System.Collections.Generic;
//using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Security.Policy;
using System.Windows;
using System.Windows.Media;

namespace Luster.Motion.DigitalSetup.Helpers
{
    public class DataGridHelper
    {

        public static DataGridCell GetCell(DataGrid dataGrid, DataGridRow row, int columnIndex)
        {
            if (row != null)
            {
                var presenter = dataGrid.ItemContainerGenerator.ContainerFromItem(row.Item) as DataGridRow;
                if (presenter != null)
                {
                    var cell = presenter.FindVisualChild<DataGridCell>(columnIndex);
                    return cell;
                }
            }
            return null;
        }
        public static void SetCellBackground(DataGridCell cell, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                cell.Background = Brushes.LightGray;
            }
            else if (text.Contains("OK"))
            {
                cell.Background = Brushes.LightGreen;
            }
            else if (text.Contains("NG"))
            {
                cell.Background = Brushes.IndianRed;
            }
        }
        public static void HandleCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            // 获取当前编辑的单元格
            var dataGrid = sender as DataGrid;
            var row = e.Row;
            var columnIndex = dataGrid.Columns.IndexOf(e.Column);

            // 获取 DataGridCell
            var cell = DataGridHelper.GetCell(dataGrid, row, columnIndex);

            if (cell != null)
            {
                // 获取当前编辑的控件
                var editingElement = e.EditingElement as TextBox;
                if (editingElement != null)
                {
                    var text = editingElement.Text;

                    // 根据文本内容设置背景颜色
                    DataGridHelper.SetCellBackground(cell, text);
                }
            }
        }
    }

    public static class VisualTreeHelperExtensions
    {
        public static T FindVisualChild<T>(this DependencyObject dependencyObject, int index) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
            {
                var child = VisualTreeHelper.GetChild(dependencyObject, i);
                if (child is T)
                {
                    if (index == 0)
                        return (T)child;
                    else
                        index--;
                }
                else
                {
                    var result = FindVisualChild<T>(child, index);
                    if (result != null)
                        return result;
                }
            }
            return null;
        }
    }
}
