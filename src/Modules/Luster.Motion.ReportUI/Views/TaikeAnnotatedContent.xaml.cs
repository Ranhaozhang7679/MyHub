using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.WPF;
using Luster.Motion.ReportUI.ViewModel;

namespace Luster.Motion.ReportUI.Views
{
    /// <summary>
    /// TaikeAnnotatedContent.xaml 的交互逻辑。
    /// 负责合并视图（MergedChart）与分开视图（SplitChartLeft / SplitChartRight）的左键命中检测转发到 VM；业务逻辑在 VM。
    /// </summary>
    public partial class TaikeAnnotatedContent : UserControl
    {
        // 与 VM.CurveNamePrefix 保持一致：仅命中此命名模式的 Series 才视为曲线
        private const string CurveNamePrefix = "tk-curve-";
        // 命中后再用屏幕像素距离阈值过滤，超过则视为"未命中曲线"
        private const double HoverPixelThreshold = 18.0;

        public TaikeAnnotatedContent()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Loaded 可能多次触发（容器切换），先 -= 再 += 避免重复订阅
            // 只订阅左键事件：hover 高亮需要重建 Series 才能刷新（LiveCharts2 修改 Stroke 不重绘），
            // 鼠标移动时频繁重建会卡顿，因此改为仅在左键时确定选中曲线
            if (MergedChart != null)
            {
                MergedChart.PreviewMouseLeftButtonDown -= HandlePreviewMouseLeftButtonDown;
                MergedChart.PreviewMouseLeftButtonDown += HandlePreviewMouseLeftButtonDown;
            }
            if (SplitChartLeft != null)
            {
                SplitChartLeft.PreviewMouseLeftButtonDown -= HandlePreviewMouseLeftButtonDown;
                SplitChartLeft.PreviewMouseLeftButtonDown += HandlePreviewMouseLeftButtonDown;
            }
            if (SplitChartRight != null)
            {
                SplitChartRight.PreviewMouseLeftButtonDown -= HandlePreviewMouseLeftButtonDown;
                SplitChartRight.PreviewMouseLeftButtonDown += HandlePreviewMouseLeftButtonDown;
            }
        }

        private void HandlePreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // sender 是触发事件的 CartesianChart（合并 / 左 / 右）；同一份命中算法 + 同一份 VM 入口
            if (!(DataContext is TaikeAnnotatedContentVM vm)) return;
            if (!(sender is CartesianChart chart)) return;
            vm.SetHoveredCurveByIndex(HitTestCurve(chart, e.GetPosition(chart)));
        }

        // 遍历 chart.CoreChart.Series 中名为 tk-curve-{i} 的 LineSeries，用 Scaler 把每个数据点
        // 转为屏幕像素，取距离鼠标最近且 < 阈值者。比 GetPointsAt(CompareAllTakeClosest) 更可靠，
        // 后者在 LiveCharts2 rc1.2 实测命中不稳定（p.Context.Series.Name 经常取不到，导致整段过滤失败）。
        // 合并视图：双 Y 轴（yAxes.Length >= 2），按 ScalesYAt 选 Scaler；
        // 分开视图：单 Y 轴（yAxes.Length == 1），全部用 yAxes[0]。
        private int HitTestCurve(CartesianChart chart, Point screenPos)
        {
            try
            {
                if (chart == null || chart.CoreChart == null) return -1;

                // chart.CoreChart 静态类型是 IChart（缺 Series / DrawMarginLocation），
                // 实际对象是 CartesianChart<SkiaSharpDrawingContext>，cast 后才能访问
                if (!(chart.CoreChart is CartesianChart<SkiaSharpDrawingContext> core)) return -1;
                if (core.Series == null) return -1;

                var xAxes = core.XAxes?.ToArray();
                var yAxes = core.YAxes?.ToArray();
                if (xAxes == null || xAxes.Length == 0 || yAxes == null || yAxes.Length == 0) return -1;

                var drawLoc = core.DrawMarginLocation;
                var drawSize = core.DrawMarginSize;
                var xScaler = new Scaler(drawLoc, drawSize, xAxes[0]);
                bool isDualY = yAxes.Length >= 2;
                var yScaler0 = new Scaler(drawLoc, drawSize, yAxes[0]);
                // 单 Y 轴时复用 yScaler0，下面分支逻辑无需为分开视图特判
                var yScaler1 = isDualY ? new Scaler(drawLoc, drawSize, yAxes[1]) : yScaler0;

                double mx = screenPos.X;
                double my = screenPos.Y;

                int bestIdx = -1;
                double bestDist = HoverPixelThreshold;

                foreach (var series in core.Series)
                {
                    if (!(series is LineSeries<ObservablePoint> line)) continue;
                    var name = line.Name;
                    if (string.IsNullOrEmpty(name) || !name.StartsWith(CurveNamePrefix, StringComparison.Ordinal)) continue;
                    if (!int.TryParse(name.Substring(CurveNamePrefix.Length), out int idx)) continue;

                    var yScaler = (!isDualY || line.ScalesYAt == 0) ? yScaler0 : yScaler1;

                    foreach (var pt in line.Values)
                    {
                        if (!(pt is ObservablePoint op)) continue;
                        // ObservablePoint.X/Y 在 LiveCharts2 中是 double?，空值跳过
                        if (!op.X.HasValue || !op.Y.HasValue) continue;
                        double px = xScaler.ToPixels(op.X.Value);
                        double py = yScaler.ToPixels(op.Y.Value);
                        double dx = px - mx;
                        double dy = py - my;
                        double d = Math.Sqrt(dx * dx + dy * dy);
                        if (d < bestDist) { bestDist = d; bestIdx = idx; }
                    }
                }

                return bestIdx;
            }
            catch
            {
                // CoreChart 未就绪/异常时按"未命中"处理，不阻塞左键交互
                return -1;
            }
        }
    }
}
