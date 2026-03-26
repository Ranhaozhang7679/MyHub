using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Luster.SimDevice.SubSystem.Views
{
    /// <summary>
    /// KeyParameterGlobal.xaml 的交互逻辑
    /// </summary>
    public partial class KeyParameterGlobal : UserControl
    {
        public KeyParameterGlobal()
        {
            InitializeComponent();
        }

        private void GridSplitter_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            var splitter = sender as GridSplitter;
            if (splitter == null) return;

            var tag = splitter.Tag.ToString();
            int index = int.Parse(tag);

            // 获取当前列
            var column = HeaderGrid.ColumnDefinitions[index];

            // 调整列宽
            double newWidth = column.Width.Value + e.HorizontalChange;
            if (newWidth >= column.MinWidth)
            {
                column.Width = new GridLength(newWidth);

                // 强制更新布局
                HeaderGrid.UpdateLayout();
                HeaderScrollViewer?.UpdateLayout();
            }
        }

        private void ContentScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // 同步列头的横向滚动位置
            if (e.HorizontalChange != 0 && HeaderScrollViewer != null)
            {
                HeaderScrollViewer.ScrollToHorizontalOffset(e.HorizontalOffset);
            }
        }
    }
}