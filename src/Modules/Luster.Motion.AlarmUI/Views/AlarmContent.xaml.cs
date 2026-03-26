using LiveCharts.Wpf;
using Luster.Motion.AlarmUI.ViewModel;
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

namespace Luster.Motion.AlarmUI.Views
{
    /// <summary>
    /// AlarmContent.xaml 的交互逻辑
    /// </summary>
    public partial class AlarmContent : UserControl
    {
        public AlarmContent()
        {
            InitializeComponent();
        }


        // 新增：DataClick 事件处理器，转发到 ViewModel 的筛选方法
        private void CartesianChart_DataClick(object sender, LiveCharts.ChartPoint chartPoint)
        {
            if (DataContext is AlarmContentVM vm && chartPoint != null)
            {
                // series title 保存了报警类型
                var alarmType = chartPoint.SeriesView?.Title ?? string.Empty;

                // 获取索引（X 值），ScatterPoint 存在时使用其 X，否则用 chartPoint.X
                int index = 0;
                if (chartPoint.Instance is LiveCharts.Defaults.ScatterPoint sp)
                {
                    index = (int)Math.Round(sp.X);
                }
                else
                {
                    index = (int)Math.Round(chartPoint.X);
                }

                vm.FilterAlarmsByTypeAndIndex(alarmType, index);
            }
        }


        // DownTimeChart：鼠标跟随 Tooltip
        private void DownTimeChart_MouseMove(object sender, MouseEventArgs e)
        {
            var chart = sender as CartesianChart;
            if (chart == null) return;

            var vm = DataContext as AlarmContentVM;
            if (vm == null) return;

            var mousePos = e.GetPosition(chart);
            var values = chart.ConvertToChartValues(mousePos);

            var minutes = values.X;
            var dayIndex = (int)Math.Round(values.Y);

            var info = vm.GetAlarmInfo(minutes, dayIndex);
            if (string.IsNullOrWhiteSpace(info))
            {
                CustomTooltip_DownTime.Visibility = Visibility.Collapsed;
                return;
            }

            TooltipText_DownTime.Text = info;
            CustomTooltip_DownTime.Visibility = Visibility.Visible;

            const double offset = 16;

            CustomTooltip_DownTime.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var tipSize = CustomTooltip_DownTime.DesiredSize;

            var x = mousePos.X + offset;
            var y = mousePos.Y + offset;

            if (x + tipSize.Width > chart.ActualWidth) x = mousePos.X - tipSize.Width - offset;
            if (y + tipSize.Height > chart.ActualHeight) y = mousePos.Y - tipSize.Height - offset;

            if (x < 0) x = 0;
            if (y < 0) y = 0;

            Canvas.SetLeft(CustomTooltip_DownTime, x);
            Canvas.SetTop(CustomTooltip_DownTime, y);
        }

        private void DownTimeChart_MouseLeave(object sender, MouseEventArgs e)
        {
            CustomTooltip_DownTime.Visibility = Visibility.Collapsed;
        }
    }
}
