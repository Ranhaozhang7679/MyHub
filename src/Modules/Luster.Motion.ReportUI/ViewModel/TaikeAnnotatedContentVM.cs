using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.Measure;
using SkiaSharp;
using SkiaSharp.Views.WPF;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Luster.Common.DataAccess.Repositories;
using Luster.Common.Tools;
using Luster.Motion.CommonUI;
using Luster.Motion.ReportUI.Model;
using Luster.Motion.TaskFlow.Engine;

namespace Luster.Motion.ReportUI.ViewModel
{
    /// <summary>
    /// 泰克标注曲线 ViewModel，单个图表叠加多条 CSV 曲线 + 步骤标注
    /// </summary>
    public class TaikeAnnotatedContentVM : ReportBaseVM
    {
        public override string ReportName => "TaikeAnnotatedCurve";

        private Dispatcher _dispatcher;
        private IMotionController _motionController;
        private ICommonBus _commonBus;
        private IDialogService _dialogService;

        // 图表数据
        private List<ISeries> _series = new List<ISeries>();
        public List<ISeries> Series
        {
            get => _series;
            set => SetProperty(ref _series, value);
        }

        private List<Axis> _xAxes = new List<Axis> { new Axis() };
        public List<Axis> XAxes
        {
            get => _xAxes;
            set => SetProperty(ref _xAxes, value);
        }

        private List<Axis> _yAxes = new List<Axis> { new Axis() };
        public List<Axis> YAxes
        {
            get => _yAxes;
            set => SetProperty(ref _yAxes, value);
        }

        // 步骤配置
        private List<StepAnnotationConfigModel> _steps = new List<StepAnnotationConfigModel>();

        // X 轴刻度间隔
        private double _xAxisStep = 50;
        /// <summary>
        /// X 轴刻度间隔（ms）
        /// </summary>
        public double XAxisStep
        {
            get => _xAxisStep;
            set => SetProperty(ref _xAxisStep, value);
        }

        private double _yAxisStep = 0.05;
        /// <summary>
        /// Y 轴刻度间隔（kgf）
        /// </summary>
        public double YAxisStep
        {
            get => _yAxisStep;
            set => SetProperty(ref _yAxisStep, value);
        }

        private double _yAxisMax = 0.12;
        /// <summary>
        /// Y 轴上限（kgf）
        /// </summary>
        public double YAxisMax
        {
            get => _yAxisMax;
            set => SetProperty(ref _yAxisMax, value);
        }

        private double _yAxisMin = 0;
        /// <summary>
        /// Y 轴下限（kgf），设为负值可查看负数压力
        /// </summary>
        public double YAxisMin
        {
            get => _yAxisMin;
            set => SetProperty(ref _yAxisMin, value);
        }

        // 原始数据缓存
        private List<List<ObservablePoint>> _rawDataCache = new List<List<ObservablePoint>>();
        private double _cachedTimeMax = 1000;
        private double _cachedPressMax = 1.0;

        public TaikeAnnotatedContentVM() { }

        public TaikeAnnotatedContentVM(
            IRepository reporitory,
            IMotionController motionController,
            Dispatcher dispatcher,
            ICommonBus commonBus,
            IDialogService dialogService)
            : base(reporitory, motionController)
        {
            _motionController = motionController;
            _dispatcher = dispatcher;
            _commonBus = commonBus;
            _dialogService = dialogService;
        }

        #region 命令

        private DelegateCommand _importCommand;
        public DelegateCommand ImportCommand =>
            _importCommand ?? (_importCommand = new DelegateCommand(ImportCsvFiles));

        private DelegateCommand _openStepConfigCommand;
        public DelegateCommand OpenStepConfigCommand =>
            _openStepConfigCommand ?? (_openStepConfigCommand = new DelegateCommand(OpenStepConfigDialog));

        private DelegateCommand _refreshChartCommand;
        /// <summary>
        /// 刷新图表（应用新的刻度间隔）
        /// </summary>
        public DelegateCommand RefreshChartCommand =>
            _refreshChartCommand ?? (_refreshChartCommand = new DelegateCommand(() =>
            {
                if (_xAxisStep <= 0) XAxisStep = 50;
                RedrawChart();
            }));

        private DelegateCommand<FrameworkElement> _saveImageCommand;
        /// <summary>
        /// 保存图表为 PNG 图片
        /// </summary>
        public DelegateCommand<FrameworkElement> SaveImageCommand =>
            _saveImageCommand ?? (_saveImageCommand = new DelegateCommand<FrameworkElement>(SaveChartImage));

        #endregion

        #region CSV 导入

        private void ImportCsvFiles()
        {
            if (_dispatcher == null) _dispatcher = Application.Current?.Dispatcher;

            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "CSV 文件 (*.csv)|*.csv|所有文件 (*.*)|*.*";
            openFileDialog.Title = "请选择压力曲线数据文件";
            openFileDialog.InitialDirectory = @"D:\TaiKeScrewDatas";

            if (openFileDialog.ShowDialog() != true) return;

            string[] files = openFileDialog.FileNames;
            if (files.Length == 0) return;

            _rawDataCache.Clear();
            double timeMax = double.MinValue;
            double pressMax = double.MinValue;

            foreach (var file in files)
            {
                List<TotalPressModel> pressModels = CSVTool.OpenCSV<TotalPressModel>(file);
                var values = new List<ObservablePoint>();

                foreach (var item in pressModels)
                {
                    var t = Math.Abs(item.Time);
                    values.Add(new ObservablePoint(t, item.Press));
                    if (t > timeMax) timeMax = t;
                    if (item.Press > pressMax) pressMax = item.Press;
                }

                _rawDataCache.Add(values);
            }

            if (timeMax <= 0) timeMax = 1000;
            if (pressMax <= 0) pressMax = 1.0;

            _cachedTimeMax = timeMax;
            _cachedPressMax = pressMax;

            // 自动加载步骤配置
            LoadSteps();

            RedrawChart();
        }

        #endregion

        #region 步骤配置

        private void LoadSteps()
        {
            _steps.Clear();
            var configPath = GetRecipePath();
            var config = StepAnnotationConfig.LoadByCsvName(null, configPath);
            if (config.Steps.Count > 0)
            {
                foreach (var step in config.Steps)
                {
                    _steps.Add(new StepAnnotationConfigModel
                    {
                        Name = step.Name,
                        StartTimeMs = step.StartTimeMs,
                        EndTimeMs = step.EndTimeMs,
                        Color = step.Color
                    });
                }
            }
        }

        private void OpenStepConfigDialog()
        {
            if (_dialogService == null) return;

            var parameters = new DialogParameters();
            parameters.Add("Steps", _steps.Select(s => new StepAnnotationConfigModel
            {
                Name = s.Name,
                StartTimeMs = s.StartTimeMs,
                EndTimeMs = s.EndTimeMs,
                Color = s.Color
            }).ToList());
            parameters.Add("RecipePath", GetRecipePath());
            parameters.Add("CsvFileName", "_default");

            _dialogService.ShowDialog("StepConfigDialog", parameters, (result) =>
            {
                if (result.Result == ButtonResult.OK)
                {
                    var updatedSteps = result.Parameters.GetValue<List<StepAnnotationConfigModel>>("Steps");
                    if (updatedSteps != null)
                    {
                        _steps.Clear();
                        foreach (var step in updatedSteps)
                        {
                            _steps.Add(new StepAnnotationConfigModel
                            {
                                Name = step.Name,
                                StartTimeMs = step.StartTimeMs,
                                EndTimeMs = step.EndTimeMs,
                                Color = step.Color
                            });
                        }
                        RedrawChart();
                    }
                }
            });
        }

        #endregion

        #region 配方路径

        private string GetRecipePath()
        {
            try
            {
                if (_commonBus?.CurrentRecipe != null)
                    return _commonBus.CurrentRecipe.GetRecipePath();
                if (_commonBus?.ProjInfo != null && !string.IsNullOrEmpty(_commonBus.ProjInfo.ProjPath))
                    return _commonBus.ProjInfo.ProjPath;
            }
            catch { }
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        #endregion

        #region 图表重绘

        private void RedrawChart()
        {
            if (_rawDataCache.Count == 0) return;
            if (_dispatcher == null) _dispatcher = Application.Current?.Dispatcher;

            double timeMax = _cachedTimeMax;
            double pressMax = _cachedPressMax;

            var chartSeries = new List<ISeries>();

            // 每条 CSV 一条黑色曲线
            foreach (var cachedValues in _rawDataCache)
            {
                var line = new LineSeries<ObservablePoint>();
                line.Stroke = new SolidColorPaint(Colors.Black.ToSKColor(), 1);
                line.LineSmoothness = 0;
                line.Fill = new SolidColorPaint(Colors.Transparent.ToSKColor(), 1);
                line.GeometrySize = 0;
                line.MiniatureShapeSize = 0;
                line.Name = null;
                line.Values = new List<ObservablePoint>(cachedValues);
                chartSeries.Add(line);
            }

            // 轴范围
            double xStep = XAxisStep > 0 ? XAxisStep : 50;
            double yStep = YAxisStep > 0 ? YAxisStep : 0.05;
            double yMax = YAxisMax > 0 ? YAxisMax : pressMax * 1.05;
            double yMin = YAxisMin;
            double xMax = Math.Ceiling(timeMax / xStep) * xStep + xStep;

            // 计算标注 Y 位置：用户设定上限时标注放在范围内，自动模式扩展上限
            double annotationY;
            if (_steps.Count > 0)
            {
                if (YAxisMax > 0)
                {
                    // 用户设置了上限，标注放在可见范围内
                    annotationY = Math.Min(pressMax * 1.08, yMax * 0.92);
                    annotationY = Math.Max(annotationY, pressMax * 1.02);
                }
                else
                {
                    annotationY = pressMax * 1.08;
                    yMax = Math.Max(yMax, annotationY * 1.05);
                }
            }
            else
            {
                annotationY = pressMax * 1.08;
            }

            var axis_x = new Axis();
            axis_x.Name = "Time/ms";
            axis_x.NameTextSize = 12;
            axis_x.TextSize = 10;
            axis_x.MinLimit = 0;
            axis_x.MaxLimit = xMax;
            axis_x.MinStep = xStep;
            axis_x.ForceStepToMin = true;
            axis_x.ShowSeparatorLines = true;
            axis_x.SeparatorsPaint = new SolidColorPaint(Colors.LightGray.ToSKColor(), 1);
            axis_x.Padding = new Padding(4, 0, 4, 0);
            axis_x.NamePadding = new Padding(4, 0, 4, 0);

            var axis_y = new Axis();
            axis_y.Name = "Press/kgf";
            axis_y.NameTextSize = 12;
            axis_y.TextSize = 10;
            axis_y.MinLimit = yMin;
            axis_y.MaxLimit = yMax;
            axis_y.MinStep = yStep;
            axis_y.ForceStepToMin = true;
            axis_y.ShowSeparatorLines = true;
            axis_y.SeparatorsPaint = new SolidColorPaint(Colors.LightGray.ToSKColor(), 1);
            axis_y.Padding = new Padding(4, 0, 4, 0);
            axis_y.NamePadding = new Padding(4, 0, 4, 0);

            // 步骤标注
            DrawStepAnnotations(chartSeries, _steps, pressMax, xMax, yMin, annotationY);

            // 顶部辅助 X 轴：显示步骤名称
            Axis axis_x_labels = null;
            if (_steps.Count > 0)
            {
                axis_x_labels = new Axis();
                axis_x_labels.Position = AxisPosition.End;
                axis_x_labels.MinLimit = 0;
                axis_x_labels.MaxLimit = xMax;
                axis_x_labels.ShowSeparatorLines = false;
                axis_x_labels.SeparatorsPaint = new SolidColorPaint(SKColors.Transparent);
                axis_x_labels.TextSize = 11;
                axis_x_labels.Padding = new Padding(0, 8, 0, 0);
                axis_x_labels.NamePadding = new Padding(0, 0, 0, 0);
                axis_x_labels.LabelsPaint = new SolidColorPaint(SKColors.Black)
                {
                    FontFamily = "Microsoft YaHei"
                };

                // 在每个步骤中点放置刻度
                var separators = new List<double>();
                var nameMap = new Dictionary<double, string>();
                foreach (var step in _steps)
                {
                    double start = Math.Max(0, step.StartTimeMs);
                    double end = Math.Min(step.EndTimeMs, xMax);
                    if (end <= start) continue;
                    double mid = (start + end) / 2;
                    separators.Add(mid);
                    nameMap[mid] = step.Name;
                }
                axis_x_labels.CustomSeparators = separators.ToArray();
                axis_x_labels.Labeler = (value) =>
                {
                    foreach (var kvp in nameMap)
                    {
                        if (Math.Abs(value - kvp.Key) < 1)
                            return kvp.Value;
                    }
                    return "";
                };
            }

            _dispatcher.Invoke(new Action(() =>
            {
                Series = new List<ISeries>();
                XAxes = new List<Axis>();
                YAxes = new List<Axis>();

                Series = chartSeries;
                XAxes = axis_x_labels != null
                    ? new List<Axis> { axis_x, axis_x_labels }
                    : new List<Axis> { axis_x };
                YAxes = new List<Axis> { axis_y };
            }));
        }

        #endregion

        #region 步骤标注

        private void DrawStepAnnotations(List<ISeries> chartSeries, List<StepAnnotationConfigModel> steps, double pressMax, double xMax, double yMin, double annotationY)
        {
            if (steps == null || steps.Count == 0) return;

            foreach (var step in steps)
            {
                double start = Math.Max(0, step.StartTimeMs);
                double end = Math.Min(step.EndTimeMs, xMax);
                if (end <= start) continue;

                SKColor stepColor;
                try
                {
                    var c = (System.Windows.Media.Color)ColorConverter.ConvertFromString(step.Color);
                    stepColor = new SKColor(c.R, c.G, c.B, 180);
                }
                catch { stepColor = new SKColor(76, 175, 80, 180); }

                // 标注条
                var bar = new LineSeries<ObservablePoint>();
                bar.Stroke = new SolidColorPaint(stepColor, 10);
                bar.LineSmoothness = 0;
                bar.Fill = new SolidColorPaint(Colors.Transparent.ToSKColor(), 1);
                bar.GeometrySize = 0;
                bar.MiniatureShapeSize = 0;
                bar.Name = step.Name;
                bar.Values = new List<ObservablePoint>
                {
                    new ObservablePoint(start, annotationY),
                    new ObservablePoint(end, annotationY),
                };
                chartSeries.Add(bar);

                // 垂直分隔线（从 Y 轴下限到标注位置）
                var sep = new LineSeries<ObservablePoint>();
                sep.Stroke = new SolidColorPaint(new SKColor(stepColor.Red, stepColor.Green, stepColor.Blue, 120), 1);
                sep.LineSmoothness = 0;
                sep.Fill = new SolidColorPaint(SKColors.Transparent);
                sep.GeometrySize = 0;
                sep.MiniatureShapeSize = 0;
                sep.Name = null;
                sep.Values = new List<ObservablePoint>
                {
                    new ObservablePoint(start, yMin),
                    new ObservablePoint(start, annotationY),
                };
                chartSeries.Add(sep);
            }

            // 最后步骤结束线
            var lastStep = steps.Last();
            if (lastStep.EndTimeMs <= xMax)
            {
                SKColor lastColor;
                try
                {
                    var c = (System.Windows.Media.Color)ColorConverter.ConvertFromString(lastStep.Color);
                    lastColor = new SKColor(c.R, c.G, c.B, 120);
                }
                catch { lastColor = new SKColor(76, 175, 80, 120); }

                var endLine = new LineSeries<ObservablePoint>();
                endLine.Stroke = new SolidColorPaint(lastColor, 1);
                endLine.LineSmoothness = 0;
                endLine.Fill = new SolidColorPaint(SKColors.Transparent);
                endLine.GeometrySize = 0;
                endLine.MiniatureShapeSize = 0;
                endLine.Name = null;
                endLine.Values = new List<ObservablePoint>
                {
                    new ObservablePoint(lastStep.EndTimeMs, yMin),
                    new ObservablePoint(lastStep.EndTimeMs, annotationY),
                };
                chartSeries.Add(endLine);
            }
        }

        #endregion

        #region 辅助方法

        private void SaveChartImage(FrameworkElement element)
        {
            if (element == null || element.ActualWidth <= 0 || element.ActualHeight <= 0) return;

            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.Filter = "PNG 图片|*.png";
            dialog.DefaultExt = ".png";
            dialog.FileName = $"JDPressure_{DateTime.Now:yyyyMMdd_HHmmss}";
            if (dialog.ShowDialog() != true) return;

            double dpi = 2.0;
            int w = (int)element.ActualWidth;
            int h = (int)element.ActualHeight;

            var renderBitmap = new RenderTargetBitmap(
                (int)(w * dpi), (int)(h * dpi),
                96 * dpi, 96 * dpi,
                System.Windows.Media.PixelFormats.Pbgra32);

            // 白底
            var bgVisual = new System.Windows.Media.DrawingVisual();
            using (var dc = bgVisual.RenderOpen())
            {
                dc.DrawRectangle(System.Windows.Media.Brushes.White, null,
                    new Rect(0, 0, w, h));
            }
            renderBitmap.Render(bgVisual);

            // 图表内容
            renderBitmap.Render(element);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
            using (var stream = File.Create(dialog.FileName))
            {
                encoder.Save(stream);
            }

            MessageBox.Show($"图片已保存至：{dialog.FileName}", "保存成功",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private static double CalculateNiceStep(double range, int targetTicks)
        {
            if (range <= 0) return 0.1;
            double rawStep = range / targetTicks;
            double magnitude = Math.Pow(10, Math.Floor(Math.Log10(rawStep)));
            double residual = rawStep / magnitude;
            double niceStep;
            if (residual <= 1.5) niceStep = magnitude;
            else if (residual <= 3.5) niceStep = 2 * magnitude;
            else if (residual <= 7.5) niceStep = 5 * magnitude;
            else niceStep = 10 * magnitude;
            return niceStep;
        }

        #endregion
    }
}
