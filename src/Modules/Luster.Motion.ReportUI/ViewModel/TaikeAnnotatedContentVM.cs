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

        // 第二个图表
        private List<ISeries> _series2 = new List<ISeries>();
        public List<ISeries> Series2
        {
            get => _series2;
            set => SetProperty(ref _series2, value);
        }

        private List<Axis> _xAxes2 = new List<Axis> { new Axis() };
        public List<Axis> XAxes2
        {
            get => _xAxes2;
            set => SetProperty(ref _xAxes2, value);
        }

        private List<Axis> _yAxes2 = new List<Axis> { new Axis() };
        public List<Axis> YAxes2
        {
            get => _yAxes2;
            set => SetProperty(ref _yAxes2, value);
        }

        // 图表2 模式切换：false=Position-Press, true=Time-Position
        private bool _isChart2TimePosition;
        /// <summary>
        /// 图表2是否为Time-Position模式
        /// </summary>
        public bool IsChart2TimePosition
        {
            get => _isChart2TimePosition;
            set => SetProperty(ref _isChart2TimePosition, value);
        }

        private string _chart2Title = "Position-Press";
        /// <summary>
        /// 图表2标题
        /// </summary>
        public string Chart2Title
        {
            get => _chart2Title;
            set => SetProperty(ref _chart2Title, value);
        }

        private string _chart2XLabel = "X间隔(mm):";
        public string Chart2XLabel
        {
            get => _chart2XLabel;
            set => SetProperty(ref _chart2XLabel, value);
        }

        private string _chart2YLabel = "Y间隔(kgf):";
        public string Chart2YLabel
        {
            get => _chart2YLabel;
            set => SetProperty(ref _chart2YLabel, value);
        }

        private string _chart2YMaxLabel = "Y上限(kgf):";
        public string Chart2YMaxLabel
        {
            get => _chart2YMaxLabel;
            set => SetProperty(ref _chart2YMaxLabel, value);
        }

        // 步骤配置
        private List<StepAnnotationConfigModel> _steps = new List<StepAnnotationConfigModel>();

        // === 图表1 参数（Time-Press）===
        private double _xAxisStep = 50;
        /// <summary>
        /// 图表1 X轴刻度间隔（ms）
        /// </summary>
        public double XAxisStep
        {
            get => _xAxisStep;
            set => SetProperty(ref _xAxisStep, value);
        }

        private double _yAxisStep = 0.05;
        /// <summary>
        /// 图表1 Y轴刻度间隔（kgf）
        /// </summary>
        public double YAxisStep
        {
            get => _yAxisStep;
            set => SetProperty(ref _yAxisStep, value);
        }

        private double _yAxisMax = 0.3;
        /// <summary>
        /// 图表1 Y轴上限（kgf），设置为0时自动调整
        /// </summary>
        public double YAxisMax
        {
            get => _yAxisMax;
            set => SetProperty(ref _yAxisMax, value);
        }

        // === 图表2 参数 ===
        private double _xAxisStep2 = 50;
        /// <summary>
        /// 图表2 X轴刻度间隔
        /// </summary>
        public double XAxisStep2
        {
            get => _xAxisStep2;
            set => SetProperty(ref _xAxisStep2, value);
        }

        private double _yAxisStep2 = 0.5;
        /// <summary>
        /// 图表2 Y轴刻度间隔
        /// </summary>
        public double YAxisStep2
        {
            get => _yAxisStep2;
            set => SetProperty(ref _yAxisStep2, value);
        }

        private double _yAxisMax2 = 5.0;
        /// <summary>
        /// 图表2 Y轴上限，设置为0时自动调整
        /// </summary>
        public double YAxisMax2
        {
            get => _yAxisMax2;
            set => SetProperty(ref _yAxisMax2, value);
        }
        //只显示Y+
        private bool _onlyShowPositiveIsEnabled = true;
        public bool OnlyShowPositiveIsEnabled
        {
            get => _onlyShowPositiveIsEnabled;
            set
            {
                if (SetProperty(ref _onlyShowPositiveIsEnabled, value))
                    RedrawChart();
            }
        }
        //是否启用平滑曲线
        private bool _smoothCurveProcessingsEnabled = true;
        public bool SmoothCurveProcessingsEnabled
        {
            get => _smoothCurveProcessingsEnabled;
            set => SetProperty(ref _smoothCurveProcessingsEnabled, value);
        }

        private int _sliderValue = 11;
        public int SliderValue
        {
            get => _sliderValue;
            set => SetProperty(ref _sliderValue, value);
        }
        // 原始数据缓存
        private List<List<ObservablePoint>> _rawDataCache = new List<List<ObservablePoint>>();
        private List<List<ObservablePoint>> _rawPositionPressCache = new List<List<ObservablePoint>>();
        private List<List<ObservablePoint>> _rawTimePositionCache = new List<List<ObservablePoint>>();
        //平滑处理后的数据
        private List<List<ObservablePoint>> _rawDataCache1 = new List<List<ObservablePoint>>();
        private double _cachedTimeMax = 1000;
        private double _cachedPressMin = 0;
        private double _cachedPressMax = 1.0;
        private double _cachedPositionMin = 0;
        private double _cachedPositionMax = 1.0;

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

        private DelegateCommand _toggleChart2ModeCommand;
        /// <summary>
        /// 切换图表2模式：Position-Press / Time-Position
        /// </summary>
        public DelegateCommand ToggleChart2ModeCommand =>
            _toggleChart2ModeCommand ?? (_toggleChart2ModeCommand = new DelegateCommand(() =>
            {
                IsChart2TimePosition = !IsChart2TimePosition;
                if (IsChart2TimePosition)
                {
                    Chart2Title = "Time-Position";
                    Chart2XLabel = "X间隔(ms):";
                    Chart2YLabel = "Y间隔(mm):";
                    Chart2YMaxLabel = "Y上限(mm,0自动):";
                }
                else
                {
                    Chart2Title = "Position-Press";
                    Chart2XLabel = "X间隔(mm):";
                    Chart2YLabel = "Y间隔(kgf):";
                    Chart2YMaxLabel = "Y上限(kgf,0自动):";
                }
                RedrawChart();
            }));

        #endregion

        #region CSV 导入

        private void ImportCsvFiles()
        {
            if (_dispatcher == null) _dispatcher = Application.Current?.Dispatcher;

            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "CSV 文件 (*.csv)|*.csv|所有文件 (*.*)|*.*";
            openFileDialog.Title = "请选择压力曲线数据文件";
            openFileDialog.InitialDirectory = @"D:\lmv-2026-043011\0430FCLP2\CC上传\NUM1";

            if (openFileDialog.ShowDialog() != true) return;

            string[] files = openFileDialog.FileNames;
            if (files.Length == 0) return;

            _rawDataCache.Clear();
            _rawPositionPressCache.Clear();
            _rawTimePositionCache.Clear();
            double timeMax = double.MinValue;
            double pressMin = double.MaxValue;
            double pressMax = double.MinValue;
            double positionMin = double.MaxValue;
            double positionMax = double.MinValue;

            foreach (var file in files)
            {
                List<TotalPressModel> pressModels = CSVTool.OpenCSV<TotalPressModel>(file);
                var values = new List<ObservablePoint>();
                var posPressValues = new List<ObservablePoint>();
                var timePosValues = new List<ObservablePoint>();

                foreach (var item in pressModels)
                {
                    var t = Math.Abs(item.Time);
                    values.Add(new ObservablePoint(t, item.Press));
                    posPressValues.Add(new ObservablePoint(item.Position, item.Press));
                    timePosValues.Add(new ObservablePoint(t, item.Position));
                    if (t > timeMax) timeMax = t;
                    if (item.Press < pressMin) pressMin = item.Press;
                    if (item.Press > pressMax) pressMax = item.Press;
                    if (item.Position < positionMin) positionMin = item.Position;
                    if (item.Position > positionMax) positionMax = item.Position;
                }

                _rawDataCache.Add(values);
                _rawPositionPressCache.Add(posPressValues);
                _rawTimePositionCache.Add(timePosValues);
            }
            if (timeMax <= 0) timeMax = 1000;
            if (pressMin == double.MaxValue) pressMin = 0;
            if (pressMax <= pressMin) pressMax = pressMin + 1.0;
            if (positionMin == double.MaxValue) positionMin = 0;
            if (positionMax <= positionMin) positionMax = positionMin + 1.0;

            _cachedTimeMax = timeMax;
            _cachedPressMin = pressMin;
            _cachedPressMax = pressMax;
            _cachedPositionMin = positionMin;
            _cachedPositionMax = positionMax;

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
            if (_smoothCurveProcessingsEnabled)
            {
                _rawDataCache1 = Smoothing.Pxcl(_rawDataCache, _sliderValue);
            }         
            if (_dispatcher == null) _dispatcher = Application.Current?.Dispatcher;

            double timeMax = _cachedTimeMax;
            double pressMin = _cachedPressMin;
            double pressMax = _cachedPressMax;
            double positionMin = _cachedPositionMin;
            double positionMax = _cachedPositionMax;

            // 计算对称的 Y 轴范围（支持负数压力）
            double pressAbsMax = Math.Max(Math.Abs(pressMin), Math.Abs(pressMax));

            // === 图表1：Time-Press ===
            var chart1Series = new List<ISeries>();
            if (_smoothCurveProcessingsEnabled)
            {
                foreach (var cachedValues in _rawDataCache1)
                {
                    var line = new LineSeries<ObservablePoint>();
                    line.Stroke = new SolidColorPaint(Colors.Black.ToSKColor(), 1);
                    line.LineSmoothness = 0;
                    line.Fill = new SolidColorPaint(Colors.Transparent.ToSKColor(), 1);
                    line.GeometrySize = 0;
                    line.MiniatureShapeSize = 0;
                    line.Name = null;
                    line.Values = new List<ObservablePoint>(cachedValues);
                    chart1Series.Add(line);
                }
            }
            else
            {
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
                    chart1Series.Add(line);
                }
            }
            double xStep1 = XAxisStep > 0 ? XAxisStep : 50;
            double yStep1 = YAxisStep > 0 ? YAxisStep : 0.05;
            double yMax1 = YAxisMax > 0 ? YAxisMax : pressAbsMax * 1.05;
            double xMax = Math.Ceiling(timeMax / xStep1) * xStep1 + xStep1;

            var axis_x_time = new Axis();
            axis_x_time.Name = "Time/ms";
            axis_x_time.NameTextSize = 12;
            axis_x_time.TextSize = 10;
            axis_x_time.MinLimit = 0;
            axis_x_time.MaxLimit = xMax;
            axis_x_time.MinStep = xStep1;
            axis_x_time.ForceStepToMin = true;
            axis_x_time.ShowSeparatorLines = true;
            axis_x_time.SeparatorsPaint = new SolidColorPaint(Colors.LightGray.ToSKColor(), 1);
            axis_x_time.Padding = new Padding(4, 0, 4, 0);
            axis_x_time.NamePadding = new Padding(4, 0, 4, 0);

            var axis_y_press1 = new Axis();
            axis_y_press1.Name = "Press/kgf";
            axis_y_press1.NameTextSize = 12;
            axis_y_press1.TextSize = 10;
            axis_y_press1.MinLimit = _onlyShowPositiveIsEnabled ? 0:- yMax1;
            axis_y_press1.MaxLimit = yMax1;
            axis_y_press1.MinStep = yStep1;
            axis_y_press1.ForceStepToMin = true;
            axis_y_press1.ShowSeparatorLines = true;
            axis_y_press1.SeparatorsPaint = new SolidColorPaint(Colors.LightGray.ToSKColor(), 1);
            axis_y_press1.Padding = new Padding(4, 0, 4, 0);
            axis_y_press1.NamePadding = new Padding(4, 0, 4, 0);

            // 步骤标注（仅 Time-Press 图表）
            DrawStepAnnotations(chart1Series, _steps, pressMax, xMax);

            // === 图表2：Position-Press 或 Time-Position ===
            var chart2Series = new List<ISeries>();
            Axis axis_x2, axis_y2;

            if (_isChart2TimePosition)
            {
                // Time-Position 模式：X=Time, Y=Position
                if (_rawTimePositionCache.Count > 0)
                {
                    foreach (var cachedValues in _rawTimePositionCache)
                    {
                        var line = new LineSeries<ObservablePoint>();
                        line.Stroke = new SolidColorPaint(Colors.Black.ToSKColor(), 1);
                        line.LineSmoothness = 0;
                        line.Fill = new SolidColorPaint(Colors.Transparent.ToSKColor(), 1);
                        line.GeometrySize = 0;
                        line.MiniatureShapeSize = 0;
                        line.Name = null;
                        line.Values = new List<ObservablePoint>(cachedValues);
                        chart2Series.Add(line);
                    }
                }

                double xStep2tp = XAxisStep2 > 0 ? XAxisStep2 : 50;
                double yStep2tp = YAxisStep2 > 0 ? YAxisStep2 : 0.5;
                double yMax2tp = YAxisMax2 > 0 ? YAxisMax2 : positionMax * 1.1;

                double xMax2tp = Math.Ceiling(timeMax / xStep2tp) * xStep2tp + xStep2tp;

                axis_x2 = new Axis();
                axis_x2.Name = "Time/ms";
                axis_x2.NameTextSize = 12;
                axis_x2.TextSize = 10;
                axis_x2.MinLimit = 0;
                axis_x2.MaxLimit = xMax2tp;
                axis_x2.MinStep = xStep2tp;
                axis_x2.ForceStepToMin = true;
                axis_x2.ShowSeparatorLines = true;
                axis_x2.SeparatorsPaint = new SolidColorPaint(Colors.LightGray.ToSKColor(), 1);
                axis_x2.Padding = new Padding(4, 0, 4, 0);
                axis_x2.NamePadding = new Padding(4, 0, 4, 0);

                axis_y2 = new Axis();
                axis_y2.Name = "Position/mm";
                axis_y2.NameTextSize = 12;
                axis_y2.TextSize = 10;
                axis_y2.MinLimit = _onlyShowPositiveIsEnabled ? 0 : -yMax2tp;
                axis_y2.MaxLimit = yMax2tp;
                axis_y2.MinStep = yStep2tp;
                axis_y2.ForceStepToMin = true;
                axis_y2.ShowSeparatorLines = true;
                axis_y2.SeparatorsPaint = new SolidColorPaint(Colors.LightGray.ToSKColor(), 1);
                axis_y2.Padding = new Padding(4, 0, 4, 0);
                axis_y2.NamePadding = new Padding(4, 0, 4, 0);
            }
            else
            {
                // Position-Press 模式：X=Position, Y=Press
                if (_rawPositionPressCache.Count > 0)
                {
                    foreach (var cachedValues in _rawPositionPressCache)
                    {
                        var line = new LineSeries<ObservablePoint>();
                        line.Stroke = new SolidColorPaint(Colors.Black.ToSKColor(), 1);
                        line.LineSmoothness = 0;
                        line.Fill = new SolidColorPaint(Colors.Transparent.ToSKColor(), 1);
                        line.GeometrySize = 0;
                        line.MiniatureShapeSize = 0;
                        line.Name = null;
                        line.Values = new List<ObservablePoint>(cachedValues);
                        chart2Series.Add(line);
                    }
                }

                double xStep2pp = XAxisStep2 > 0 ? XAxisStep2 : 0.5;
                double yStep2pp = YAxisStep2 > 0 ? YAxisStep2 : 0.05;
                double yMax2pp = YAxisMax2 > 0 ? YAxisMax2 : pressAbsMax * 1.05;

                double posRange = positionMax - positionMin;
                double posPadding = posRange * 0.05;
                double posMinLimit = positionMin - posPadding;
                double posMaxLimit = positionMax + posPadding;

                axis_x2 = new Axis();
                axis_x2.Name = "Position/mm";
                axis_x2.NameTextSize = 12;
                axis_x2.TextSize = 10;
                axis_x2.MinLimit = posMinLimit;
                axis_x2.MaxLimit = posMaxLimit;
                axis_x2.MinStep = xStep2pp;
                axis_x2.ForceStepToMin = true;
                axis_x2.ShowSeparatorLines = true;
                axis_x2.SeparatorsPaint = new SolidColorPaint(Colors.LightGray.ToSKColor(), 1);
                axis_x2.Padding = new Padding(4, 0, 4, 0);
                axis_x2.NamePadding = new Padding(4, 0, 4, 0);

                axis_y2 = new Axis();
                axis_y2.Name = "Press/kgf";
                axis_y2.NameTextSize = 12;
                axis_y2.TextSize = 10;
                axis_y2.MinLimit = _onlyShowPositiveIsEnabled ? 0 : -yMax2pp;
                axis_y2.MaxLimit = yMax2pp;
                axis_y2.MinStep = yStep2pp;
                axis_y2.ForceStepToMin = true;
                axis_y2.ShowSeparatorLines = true;
                axis_y2.SeparatorsPaint = new SolidColorPaint(Colors.LightGray.ToSKColor(), 1);
                axis_y2.Padding = new Padding(4, 0, 4, 0);
                axis_y2.NamePadding = new Padding(4, 0, 4, 0);
            }

            _dispatcher.Invoke(new Action(() =>
            {
                // 图表1：Time-Press
                Series = chart1Series;
                XAxes = new List<Axis> { axis_x_time };
                YAxes = new List<Axis> { axis_y_press1 };

                // 图表2
                Series2 = chart2Series;
                XAxes2 = new List<Axis> { axis_x2 };
                YAxes2 = new List<Axis> { axis_y2 };
            }));
        }

        #endregion

        #region 步骤标注

        private void DrawStepAnnotations(List<ISeries> chartSeries, List<StepAnnotationConfigModel> steps, double pressMax, double xMax)
        {
            if (steps == null || steps.Count == 0) return;

            double annotationY = pressMax * 1.08;

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

                // 垂直分隔线
                var sep = new LineSeries<ObservablePoint>();
                sep.Stroke = new SolidColorPaint(new SKColor(stepColor.Red, stepColor.Green, stepColor.Blue, 120), 1);
                sep.LineSmoothness = 0;
                sep.Fill = new SolidColorPaint(SKColors.Transparent);
                sep.GeometrySize = 0;
                sep.MiniatureShapeSize = 0;
                sep.Name = null;
                sep.Values = new List<ObservablePoint>
                {
                    new ObservablePoint(start, 0),
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
                    new ObservablePoint(lastStep.EndTimeMs, 0),
                    new ObservablePoint(lastStep.EndTimeMs, annotationY),
                };
                chartSeries.Add(endLine);
            }
        }

        #endregion

        #region 辅助方法

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
    //平滑曲线的算法
    public static class Smoothing
    {
        // 移动平均平滑
        public static double[] MovingAverage(double[] data, int windowSize)
        {
            int n = data.Length;
            double[] result = new double[n];
            int half = windowSize / 2;

            for (int i = 0; i < n; i++)
            {
                double sum = 0;
                int count = 0;
                for (int j = -half; j <= half; j++)
                {
                    int idx = i + j;
                    if (idx >= 0 && idx < n)
                    {
                        sum += data[idx];
                        count++;
                    }
                }
                result[i] = sum / count;
            }

            return result;
        }

        // 高斯平滑
        public static double[] GaussianSmooth(double[] data, int windowSize)
        {
            int n = data.Length;
            double[] result = new double[n];
            int half = windowSize / 2;
            double sigma = windowSize / 6.0;

            // 预计算高斯权重
            double[] weights = new double[windowSize];
            double weightSum = 0;
            for (int i = 0; i < windowSize; i++)
            {
                double x = i - half;
                weights[i] = Math.Exp(-(x * x) / (2 * sigma * sigma));
                weightSum += weights[i];
            }

            // 归一化权重
            for (int i = 0; i < windowSize; i++)
            {
                weights[i] /= weightSum;
            }

            for (int i = 0; i < n; i++)
            {
                double sum = 0;
                for (int j = -half; j <= half; j++)
                {
                    int idx = i + j;
                    if (idx >= 0 && idx < n)
                    {
                        sum += data[idx] * weights[j + half];
                    }
                }
                result[i] = sum;
            }

            return result;
        }

        // Savitzky-Golay 平滑（使用二次多项式拟合）
        public static double[] SavitzkyGolay(double[] data, int windowSize)
        {
            int n = data.Length;
            double[] result = new double[n];
            int half = windowSize / 2;

            // 计算Savitzky-Golay卷积系数（二次多项式）
            // 使用最小二乘法求解
            int m = half;
            double[,] A = new double[5, 5];
            double[] b = new double[5];

            // 构建正规方程
            for (int k = -m; k <= m; k++)
            {
                A[0, 0] += 1;
                A[0, 1] += k;
                A[0, 2] += k * k;
                A[1, 0] += k;
                A[1, 1] += k * k;
                A[1, 2] += k * k * k;
                A[2, 0] += k * k;
                A[2, 1] += k * k * k;
                A[2, 2] += k * k * k * k;
            }

            // 对于平滑（求第0阶导数），b向量
            b[0] = 1;
            b[1] = 0;
            b[2] = 0;

            // 使用3x3子矩阵求解（简化）
            double[,] mat = new double[3, 4];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                    mat[i, j] = A[i, j];
                mat[i, 3] = b[i];
            }

            // 高斯消元法求解系数
            SolveLinearSystem3x3(mat);
            double c0 = mat[0, 3];
            // double c1 = mat[1, 3]; // 不需要
            // double c2 = mat[2, 3];

            // 计算卷积核: c(j) = c0 * 1 (对于SG平滑，中心点的权重)
            // 实际上SG平滑的卷积系数
            double[] coeffs = new double[windowSize];
            for (int j = -half; j <= half; j++)
            {
                coeffs[j + half] = c0; // 简化处理
            }

            // 更精确的做法：直接用最小二乘拟合
            for (int i = 0; i < n; i++)
            {
                // 对每个点用二次多项式拟合窗口内数据
                double sumY = 0, sumX = 0, sumX2 = 0, sumXY = 0, sumX2Y = 0;
                int count = 0;

                for (int j = -half; j <= half; j++)
                {
                    int idx = i + j;
                    if (idx >= 0 && idx < n)
                    {
                        double x = j;
                        double y = data[idx];
                        sumY += y;
                        sumX += x;
                        sumX2 += x * x;
                        sumXY += x * y;
                        sumX2Y += x * x * y;
                        count++;
                    }
                }

                // 最小二乘拟合 y = a + b*x + c*x^2 在x=0处的值就是a
                // 使用正规方程求解a
                double det = count * sumX2 * sumX2 * sumX2
                    - count * sumX2 * (count > 0 ? 0 : 1); // 简化

                // 更简单的做法：直接使用移动平均的加权版本
                // 实际SG平滑：中心点值为a
                // 正规方程: [n, Σx, Σx²] [a]   [Σy]
                //           [Σx, Σx², Σx³] [b] = [Σxy]
                //           [Σx², Σx³, Σx⁴] [c]   [Σx²y]
                // 由于对称窗口，Σx = 0, Σx³ = 0
                // 简化为: [n, Σx²] [a]   [Σy]
                //         [Σx², Σx⁴] [c] = [Σx²y]
                // b = Σxy / Σx²

                double S0 = count;
                double S2 = sumX2;
                double S4 = 0;
                for (int j = -half; j <= half; j++)
                {
                    int idx = i + j;
                    if (idx >= 0 && idx < n)
                    {
                        S4 += j * j * j * j;
                    }
                }

                double detA = S0 * S4 - S2 * S2;
                if (Math.Abs(detA) > 1e-12)
                {
                    result[i] = (S4 * sumY - S2 * sumX2Y) / detA;
                }
                else
                {
                    result[i] = sumY / count;
                }
            }

            return result;
        }

        // 3x3线性方程组求解（高斯消元法）
        private static void SolveLinearSystem3x3(double[,] mat)
        {
            int n = 3;
            for (int col = 0; col < n; col++)
            {
                // 选主元
                int maxRow = col;
                for (int row = col + 1; row < n; row++)
                {
                    if (Math.Abs(mat[row, col]) > Math.Abs(mat[maxRow, col]))
                        maxRow = row;
                }

                // 交换行
                for (int j = 0; j <= n; j++)
                {
                    double tmp = mat[col, j];
                    mat[col, j] = mat[maxRow, j];
                    mat[maxRow, j] = tmp;
                }

                // 消元
                for (int row = col + 1; row < n; row++)
                {
                    double factor = mat[row, col] / mat[col, col];
                    for (int j = col; j <= n; j++)
                    {
                        mat[row, j] -= factor * mat[col, j];
                    }
                }
            }

            // 回代
            for (int i = n - 1; i >= 0; i--)
            {
                mat[i, n] /= mat[i, i];
                for (int k = 0; k < i; k++)
                {
                    mat[k, n] -= mat[k, i] * mat[i, n];
                }
            }
        }
        /// <summary>
        /// 波形数据平滑处理
        /// </summary>
        /// <param name="observablePoint"></param>
        /// <returns></returns>
        public static List<List<ObservablePoint>> Pxcl(List<List<ObservablePoint>> observablePoint,int Windowsize)
        {
            List<List<ObservablePoint>> date = new List<List<ObservablePoint>>();
            foreach (List<ObservablePoint> sj in observablePoint)
            {
                List<double> ytemp = new List<double>();
                List<ObservablePoint> hc = new List<ObservablePoint>();
                foreach (ObservablePoint tt in sj)
                {
                    ytemp.Add((double)tt.Y);
                }
                double[] doubles = MovingAverage(ytemp.ToArray(), Windowsize);
                for (int i = 0; i < doubles.Length; i++)
                {
                    ObservablePoint tempdata = sj[i];
                    tempdata.Y= doubles[i];
                    hc.Add(tempdata);
                }
                date.Add(hc);
            }
            return date;
        }
    }
}
