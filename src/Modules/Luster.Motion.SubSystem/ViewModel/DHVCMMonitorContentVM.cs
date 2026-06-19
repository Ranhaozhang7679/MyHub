using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Dock;
using Luster.Motion.ReportUI.Model;
using Luster.Motion.ReportUI.ViewModel;
using Prism.Commands;
using Prism.Mvvm;
using SkiaSharp;
using SkiaSharp.Views.WPF;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace Luster.Motion.SubSystem.ViewModel
{
    /// <summary>
    /// 大寰音圈电机（DHRoboticsVCM）曲线监控 ViewModel。
    /// 监听 D:\力控数据存储\{yyyyMMdd}\NUM1\ 与 NUM2\ 下的 CSV 落盘事件，
    /// 文件到达后解析并刷新左右两个工位的「时间-压力 + 时间-位置」二合一双 Y 轴曲线。
    /// 完全不依赖 DHRoboticsVCM 的内部状态，仅靠 SaveFile() 落盘的 CSV 契约工作。
    /// 实现 IDockContent 接口，作为 AvalonDock 模块挂载到主页 Home 的 ModuleDisplayContent 中，
    /// 通过「双击配置 → 是否显示」勾选启用。
    /// </summary>
    public class DHVCMMonitorContentVM : BindableBase, IDisposable, IDockContent
    {
        private const string RootDir = @"D:\力控数据存储";
        private const string Station1Segment = @"\NUM1\";
        private const string Station2Segment = @"\NUM2\";
        private const int DebounceMs = 600;

        // IDockContent：模块名（与 Lang 资源键一致）+ RegionName（与 ControlName 一致）
        string IDockContent.Name => "DHVCMMonitor";
        string IDockContent.RegionName { get; set; } = "DHVCMMonitorContent";

        private readonly Dispatcher _dispatcher;
        private readonly ICommonBus _commonBus;
        private FileSystemWatcher _watcher1;
        private FileSystemWatcher _watcher2;
        private string _currentDayDir;

        // 每个文件路径对应一个 debounce token；新事件覆盖旧 token，保证只刷新最终状态
        private readonly ConcurrentDictionary<string, CancellationTokenSource> _pendingRefreshes
            = new ConcurrentDictionary<string, CancellationTokenSource>();

        // 自动保存：加载中标志 + 节流计时器（与 TaikeAnnotatedContentVM 同实现）
        private bool _isLoadingChartSettings;
        private DispatcherTimer _saveChartSettingsTimer;

        // 缓存每个工位最近一次 CSV 路径，供"刷新"按钮重绘时使用
        private string _lastCsvPath1;
        private string _lastCsvPath2;

        /// <summary>
        /// 无参构造：仅供 ModuleConfigureDialog 反射读取 IDockContent.Name / RegionName。
        /// 不要在此初始化 watcher —— 没有线程 Dispatcher，且 dialog 每次开关都会创建一次实例。
        /// 实际使用的实例由 Prism 容器通过下方带 Dispatcher 的构造函数解析。
        /// </summary>
        public DHVCMMonitorContentVM()
        {
        }

        public DHVCMMonitorContentVM(Dispatcher dispatcher, ICommonBus commonBus)
        {
            _dispatcher = dispatcher;
            _commonBus = commonBus;
            ResetWatchersCommand = new DelegateCommand(ResetWatchers);
            RefreshChartParamsCommand = new DelegateCommand(ReapplyParamsToCurrent);

            UpdateTodayLabel();
            InitWatchers();
            LoadChartSettings();
            _ = Task.Run(() => ReloadLatest());
        }

        // ===== 工位 1（NUM1，左图） =====
        private List<ISeries> _seriesMerge1 = new List<ISeries>();
        public List<ISeries> SeriesMerge1
        {
            get => _seriesMerge1;
            set => SetProperty(ref _seriesMerge1, value);
        }

        private List<Axis> _xAxesMerge1 = new List<Axis> { new Axis { Name = "Time/ms" } };
        public List<Axis> XAxesMerge1
        {
            get => _xAxesMerge1;
            set => SetProperty(ref _xAxesMerge1, value);
        }

        private List<Axis> _yAxesMerge1 = new List<Axis>
        {
            new Axis
            {
                Name = "Press/kgf",
                Position = AxisPosition.Start,
                NamePaint = new SolidColorPaint(Colors.Red.ToSKColor()),
                LabelsPaint = new SolidColorPaint(Colors.Red.ToSKColor()),
            },
            new Axis
            {
                Name = "Position/mm",
                Position = AxisPosition.End,
                NamePaint = new SolidColorPaint(Colors.Blue.ToSKColor()),
                LabelsPaint = new SolidColorPaint(Colors.Blue.ToSKColor()),
            },
        };
        public List<Axis> YAxesMerge1
        {
            get => _yAxesMerge1;
            set => SetProperty(ref _yAxesMerge1, value);
        }

        // ===== 工位 2（NUM2，右图） =====
        private List<ISeries> _seriesMerge2 = new List<ISeries>();
        public List<ISeries> SeriesMerge2
        {
            get => _seriesMerge2;
            set => SetProperty(ref _seriesMerge2, value);
        }

        private List<Axis> _xAxesMerge2 = new List<Axis> { new Axis { Name = "Time/ms" } };
        public List<Axis> XAxesMerge2
        {
            get => _xAxesMerge2;
            set => SetProperty(ref _xAxesMerge2, value);
        }

        private List<Axis> _yAxesMerge2 = new List<Axis>
        {
            new Axis
            {
                Name = "Press/kgf",
                Position = AxisPosition.Start,
                NamePaint = new SolidColorPaint(Colors.Red.ToSKColor()),
                LabelsPaint = new SolidColorPaint(Colors.Red.ToSKColor()),
            },
            new Axis
            {
                Name = "Position/mm",
                Position = AxisPosition.End,
                NamePaint = new SolidColorPaint(Colors.Blue.ToSKColor()),
                LabelsPaint = new SolidColorPaint(Colors.Blue.ToSKColor()),
            },
        };
        public List<Axis> YAxesMerge2
        {
            get => _yAxesMerge2;
            set => SetProperty(ref _yAxesMerge2, value);
        }

        // ===== UI 状态 =====
        private string _todayLabel;
        public string TodayLabel
        {
            get => _todayLabel;
            set => SetProperty(ref _todayLabel, value);
        }

        private string _stationStatus1 = "等待数据";
        public string StationStatus1
        {
            get => _stationStatus1;
            set => SetProperty(ref _stationStatus1, value);
        }

        private string _stationStatus2 = "等待数据";
        public string StationStatus2
        {
            get => _stationStatus2;
            set => SetProperty(ref _stationStatus2, value);
        }

        public DelegateCommand ResetWatchersCommand { get; }

        public DelegateCommand RefreshChartParamsCommand { get; }

        // ===== 图表参数（与 TaikeAnnotatedContentVM 共享同一份 StepAnnotationConfig.json 的 _default.ChartSettings）=====

        private double _xAxisStep = 50;
        public double XAxisStep
        {
            get => _xAxisStep;
            set { if (SetProperty(ref _xAxisStep, value)) ScheduleSaveChartSettings(); }
        }

        private double _yAxisStep = 0.05;
        public double YAxisStep
        {
            get => _yAxisStep;
            set { if (SetProperty(ref _yAxisStep, value)) ScheduleSaveChartSettings(); }
        }

        private double _yAxisMax = 0.3;
        public double YAxisMax
        {
            get => _yAxisMax;
            set { if (SetProperty(ref _yAxisMax, value)) ScheduleSaveChartSettings(); }
        }

        private double _xAxisStep2 = 50;
        public double XAxisStep2
        {
            get => _xAxisStep2;
            set { if (SetProperty(ref _xAxisStep2, value)) ScheduleSaveChartSettings(); }
        }

        private double _yAxisStep2 = 0.5;
        public double YAxisStep2
        {
            get => _yAxisStep2;
            set { if (SetProperty(ref _yAxisStep2, value)) ScheduleSaveChartSettings(); }
        }

        private double _yAxisMax2 = 5.0;
        public double YAxisMax2
        {
            get => _yAxisMax2;
            set { if (SetProperty(ref _yAxisMax2, value)) ScheduleSaveChartSettings(); }
        }

        private bool _onlyShowPositiveIsEnabled = false;
        public bool OnlyShowPositiveIsEnabled
        {
            get => _onlyShowPositiveIsEnabled;
            set { if (SetProperty(ref _onlyShowPositiveIsEnabled, value)) ScheduleSaveChartSettings(); }
        }

        private bool _smoothCurveProcessingsEnabled = false;
        public bool SmoothCurveProcessingsEnabled
        {
            get => _smoothCurveProcessingsEnabled;
            set { if (SetProperty(ref _smoothCurveProcessingsEnabled, value)) ScheduleSaveChartSettings(); }
        }

        private int _sliderValue = 11;
        public int SliderValue
        {
            get => _sliderValue;
            set { if (SetProperty(ref _sliderValue, value)) ScheduleSaveChartSettings(); }
        }

        // ============================================================

        private void UpdateTodayLabel()
        {
            _currentDayDir = Path.Combine(RootDir, DateTime.Now.ToString("yyyyMMdd"));
            TodayLabel = $"今天 {DateTime.Now:yyyyMMdd}  根目录: {RootDir}";
        }

        /// <summary>
        /// 初始化两个 FileSystemWatcher，分别监听当天 NUM1 与 NUM2 子目录。
        /// 若当天根目录不存在则创建（与 DHRoboticsVCM.SaveFile 行为一致）。
        /// </summary>
        private void InitWatchers()
        {
            DisposeWatchers();

            try
            {
                if (!Directory.Exists(_currentDayDir))
                {
                    Directory.CreateDirectory(_currentDayDir);
                }
                string dir1 = Path.Combine(_currentDayDir, "NUM1");
                string dir2 = Path.Combine(_currentDayDir, "NUM2");
                if (!Directory.Exists(dir1)) Directory.CreateDirectory(dir1);
                if (!Directory.Exists(dir2)) Directory.CreateDirectory(dir2);

                _watcher1 = CreateWatcher(dir1);
                _watcher2 = CreateWatcher(dir2);
            }
            catch (Exception ex)
            {
                StationStatus1 = $"监听初始化失败: {ex.Message}";
                StationStatus2 = StationStatus1;
            }
        }

        private FileSystemWatcher CreateWatcher(string path)
        {
            var w = new FileSystemWatcher(path)
            {
                IncludeSubdirectories = true,
                Filter = "*.csv",
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime,
                EnableRaisingEvents = true,
            };
            w.Created += OnCsvChanged;
            w.Changed += OnCsvChanged;
            w.Renamed += OnCsvChanged;
            w.Error += OnWatcherError;
            return w;
        }

        private void OnWatcherError(object sender, ErrorEventArgs e)
        {
            _dispatcher.BeginInvoke(new Action(() =>
            {
                StationStatus1 = $"监听异常: {e.GetException().Message}";
                StationStatus2 = StationStatus1;
            }));
        }

        private void OnCsvChanged(object sender, FileSystemEventArgs e)
        {
            string path = e.FullPath;
            int station = ClassifyStation(path);
            if (station == 0) return;
            if (!IsUnderCurrentDay(path)) return;

            // Debounce：同一路径 600ms 内多次事件只触发一次刷新
            if (_pendingRefreshes.TryGetValue(path, out var existing))
            {
                existing.Cancel();
                _pendingRefreshes.TryRemove(path, out _);
            }

            var cts = new CancellationTokenSource();
            if (!_pendingRefreshes.TryAdd(path, cts)) return;

            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(DebounceMs, cts.Token);
                    _pendingRefreshes.TryRemove(path, out _);
                    _dispatcher.BeginInvoke(new Action(() => RefreshChart(station, path)));
                }
                catch (TaskCanceledException)
                {
                    // 被后续事件覆盖，正常退出
                }
            }, cts.Token);
        }

        private int ClassifyStation(string path)
        {
            if (string.IsNullOrEmpty(path)) return 0;
            // 用目录分隔符包裹避免误匹配 NUM11 / XNUM1
            string normalized = path.Replace("/", "\\");
            if (normalized.Contains(Station1Segment)) return 1;
            if (normalized.Contains(Station2Segment)) return 2;
            return 0;
        }

        private bool IsUnderCurrentDay(string path)
        {
            if (string.IsNullOrEmpty(_currentDayDir)) return true;
            string normalized = path.Replace("/", "\\");
            return normalized.StartsWith(_currentDayDir + "\\", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 解析 CSV 并刷新对应工位的 Series 与坐标轴。
        /// 参数（X/Y 间隔、Y 上限、只显示正数、平滑曲线、平滑窗口）来自共享配置 StepAnnotationConfig.json _default.ChartSettings。
        /// 左右两个 Y 轴默认对称（MinLimit=-yMax, MaxLimit=+yMax）让 Y=0 对齐；勾选只显示正数时改为 0 ~ +yMax。
        /// </summary>
        private void RefreshChart(int station, string csvPath)
        {
            SampleBatch batch = DHVCMCsvReader.Read(csvPath);
            if (batch.TimeMs.Length == 0)
            {
                SetStationStatus(station, $"解析失败: {Path.GetFileName(csvPath)}");
                return;
            }

            var pressPoints = new List<ObservablePoint>(batch.TimeMs.Length);
            var posPoints = new List<ObservablePoint>(batch.TimeMs.Length);
            double pressMin = double.MaxValue, pressMax = double.MinValue;
            double posMin = double.MaxValue, posMax = double.MinValue;
            for (int i = 0; i < batch.TimeMs.Length; i++)
            {
                double t = batch.TimeMs[i];
                double p = batch.PressKgf[i];
                double pos = batch.PositionMm[i];
                pressPoints.Add(new ObservablePoint(t, p));
                posPoints.Add(new ObservablePoint(t, pos));
                if (p < pressMin) pressMin = p;
                if (p > pressMax) pressMax = p;
                if (pos < posMin) posMin = pos;
                if (pos > posMax) posMax = pos;
            }

            // 平滑曲线（与 TaikeAnnotatedContentVM.Smoothing.Pxcl 同实现）
            List<ObservablePoint> pressPointsToDraw = pressPoints;
            List<ObservablePoint> posPointsToDraw = posPoints;
            if (_smoothCurveProcessingsEnabled)
            {
                pressPointsToDraw = Smoothing.Pxcl(
                    new List<List<ObservablePoint>> { pressPoints }, _sliderValue)[0];
                posPointsToDraw = Smoothing.Pxcl(
                    new List<List<ObservablePoint>> { posPoints }, _sliderValue)[0];
            }

            var series = new List<ISeries>
            {
                BuildSeries(pressPointsToDraw, Colors.Red, scalesYAt: 0),
                BuildSeries(posPointsToDraw, Colors.Blue, scalesYAt: 1),
            };

            // X 轴：步长优先用户设置，否则 CalculateNiceStep
            double timeMax = batch.TimeMs.Max();
            double xStep = _xAxisStep > 0 ? _xAxisStep : CalculateNiceStep(timeMax, 10);
            double xMax = timeMax <= 0 ? xStep : Math.Ceiling(timeMax / xStep) * xStep + xStep;
            var xAxis = new Axis
            {
                Name = "Time/ms",
                NameTextSize = 10,
                TextSize = 9,
                MinLimit = 0,
                MaxLimit = xMax,
                MinStep = xStep,
                ForceStepToMin = true,
                ShowSeparatorLines = true,
                SeparatorsPaint = new SolidColorPaint(Colors.LightGray.ToSKColor(), 1),
                Padding = new Padding(2, 0, 2, 0),
                NamePadding = new Padding(2, 0, 2, 0),
            };

            // Y 轴：用户 Y 上限优先，否则按数据绝对值对称
            double pressAbsMax = Math.Max(Math.Abs(pressMin), Math.Abs(pressMax)) * 1.05;
            if (pressAbsMax < 0.1) pressAbsMax = 0.1;
            double posAbsMax = Math.Max(Math.Abs(posMin), Math.Abs(posMax)) * 1.05;
            if (posAbsMax < 1.0) posAbsMax = 1.0;

            double yMax1 = _yAxisMax > 0 ? _yAxisMax : pressAbsMax;
            double yMax2 = _yAxisMax2 > 0 ? _yAxisMax2 : posAbsMax;

            double yStep1 = Math.Max(_yAxisStep, CalculateNiceStep(yMax1, 8));
            double yStep2 = Math.Max(_yAxisStep2, CalculateNiceStep(yMax2, 8));

            var yAxes = BuildSymmetricYAxes(yMax1, yMax2, yStep1, yStep2, _onlyShowPositiveIsEnabled);

            if (station == 1)
            {
                _lastCsvPath1 = csvPath;
                SeriesMerge1 = series;
                XAxesMerge1 = new List<Axis> { xAxis };
                YAxesMerge1 = yAxes;
                StationStatus1 = $"已加载 {batch.TimeMs.Length} 点 @ {DateTime.Now:HH:mm:ss}";
            }
            else
            {
                _lastCsvPath2 = csvPath;
                SeriesMerge2 = series;
                XAxesMerge2 = new List<Axis> { xAxis };
                YAxesMerge2 = yAxes;
                StationStatus2 = $"已加载 {batch.TimeMs.Length} 点 @ {DateTime.Now:HH:mm:ss}";
            }
        }

        /// <summary>
        /// 构建 Press/Position 双 Y 轴。默认对称（-yMax ~ +yMax）让两侧 Y=0 在画布上对齐；
        /// onlyShowPositive=true 时改为 0 ~ +yMax（与 TaikeAnnotatedContent 的"只显示正数"语义一致）。
        /// </summary>
        private static List<Axis> BuildSymmetricYAxes(double yMax1, double yMax2, double yStep1, double yStep2, bool onlyShowPositive)
        {
            var axisPress = new Axis
            {
                Name = "Press/kgf",
                NameTextSize = 10,
                TextSize = 9,
                MinLimit = onlyShowPositive ? 0 : -yMax1,
                MaxLimit = yMax1,
                MinStep = yStep1,
                ForceStepToMin = true,
                ShowSeparatorLines = true,
                SeparatorsPaint = new SolidColorPaint(Colors.LightGray.ToSKColor(), 1),
                Padding = new Padding(2, 0, 2, 0),
                NamePadding = new Padding(2, 0, 2, 0),
                Position = AxisPosition.Start,
                NamePaint = new SolidColorPaint(Colors.Red.ToSKColor()),
                LabelsPaint = new SolidColorPaint(Colors.Red.ToSKColor()),
            };

            var axisPos = new Axis
            {
                Name = "Position/mm",
                NameTextSize = 10,
                TextSize = 9,
                MinLimit = onlyShowPositive ? 0 : -yMax2,
                MaxLimit = yMax2,
                MinStep = yStep2,
                ForceStepToMin = true,
                ShowSeparatorLines = false, // 只画左轴分隔线，避免双轴网格重叠
                SeparatorsPaint = new SolidColorPaint(Colors.Transparent.ToSKColor(), 1),
                Padding = new Padding(2, 0, 2, 0),
                NamePadding = new Padding(2, 0, 2, 0),
                Position = AxisPosition.End,
                NamePaint = new SolidColorPaint(Colors.Blue.ToSKColor()),
                LabelsPaint = new SolidColorPaint(Colors.Blue.ToSKColor()),
            };

            return new List<Axis> { axisPress, axisPos };
        }

        /// <summary>
        /// 计算"好看的"刻度步长（1/2/5 × 10^n）。targetTicks 是期望的刻度数。
        /// 与 TaikeAnnotatedContentVM.CalculateNiceStep 同实现。
        /// </summary>
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

        private static LineSeries<ObservablePoint> BuildSeries(List<ObservablePoint> points, Color color, int scalesYAt)
        {
            return new LineSeries<ObservablePoint>
            {
                Values = points,
                Stroke = new SolidColorPaint(color.ToSKColor(), 1),
                Fill = new SolidColorPaint(Colors.Transparent.ToSKColor(), 1),
                LineSmoothness = 0,
                GeometrySize = 0,
                MiniatureShapeSize = 0,
                ScalesYAt = scalesYAt,
            };
        }

        private void SetStationStatus(int station, string text)
        {
            if (station == 1) StationStatus1 = text;
            else StationStatus2 = text;
        }

        /// <summary>
        /// 启动 / 重置时扫一遍 NUM1 / NUM2 子目录，取 LastWriteTime 最新的 CSV 预加载。
        /// </summary>
        private void ReloadLatest()
        {
            ReloadStation(1, Path.Combine(_currentDayDir, "NUM1"));
            ReloadStation(2, Path.Combine(_currentDayDir, "NUM2"));
        }

        private void ReloadStation(int station, string dir)
        {
            try
            {
                if (!Directory.Exists(dir))
                {
                    _dispatcher.BeginInvoke(new Action(() => SetStationStatus(station, "等待数据")));
                    return;
                }

                var latest = Directory.EnumerateFiles(dir, "*.csv", SearchOption.AllDirectories)
                    .OrderByDescending(File.GetLastWriteTime)
                    .FirstOrDefault();

                if (latest == null)
                {
                    _dispatcher.BeginInvoke(new Action(() => SetStationStatus(station, "等待数据")));
                    return;
                }

                _dispatcher.BeginInvoke(new Action(() => RefreshChart(station, latest)));
            }
            catch (Exception ex)
            {
                _dispatcher.BeginInvoke(new Action(() => SetStationStatus(station, $"扫描失败: {ex.Message}")));
            }
        }

        private void ResetWatchers()
        {
            UpdateTodayLabel();
            InitWatchers();
            _ = Task.Run(() => ReloadLatest());
        }

        private void DisposeWatchers()
        {
            if (_watcher1 != null)
            {
                _watcher1.Created -= OnCsvChanged;
                _watcher1.Changed -= OnCsvChanged;
                _watcher1.Renamed -= OnCsvChanged;
                _watcher1.Error -= OnWatcherError;
                _watcher1.Dispose();
                _watcher1 = null;
            }
            if (_watcher2 != null)
            {
                _watcher2.Created -= OnCsvChanged;
                _watcher2.Changed -= OnCsvChanged;
                _watcher2.Renamed -= OnCsvChanged;
                _watcher2.Error -= OnWatcherError;
                _watcher2.Dispose();
                _watcher2 = null;
            }
        }

        /// <summary>
        /// "刷新"按钮：用当前参数重新绘制两个工位最近一次的 CSV。
        /// </summary>
        private void ReapplyParamsToCurrent()
        {
            if (!string.IsNullOrEmpty(_lastCsvPath1) && File.Exists(_lastCsvPath1))
                RefreshChart(1, _lastCsvPath1);
            if (!string.IsNullOrEmpty(_lastCsvPath2) && File.Exists(_lastCsvPath2))
                RefreshChart(2, _lastCsvPath2);
        }

        // ===== 共享图表参数持久化（与 TaikeAnnotatedContentVM 共用 StepAnnotationConfig.json _default.ChartSettings）=====

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

        /// <summary>
        /// 加载共享配置。加载期间关闭自动保存，避免回写。
        /// </summary>
        private void LoadChartSettings()
        {
            try
            {
                _isLoadingChartSettings = true;
                var config = StepAnnotationConfig.LoadByCsvName(null, GetRecipePath());
                var s = config.ChartSettings ?? new ChartSettings();
                XAxisStep = s.XAxisStep;
                YAxisStep = s.YAxisStep;
                YAxisMax = s.YAxisMax;
                XAxisStep2 = s.XAxisStep2;
                YAxisStep2 = s.YAxisStep2;
                YAxisMax2 = s.YAxisMax2;
                OnlyShowPositiveIsEnabled = s.OnlyShowPositiveIsEnabled;
                SmoothCurveProcessingsEnabled = s.SmoothCurveProcessingsEnabled;
                SliderValue = s.SliderValue;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"加载图表参数失败: {ex.Message}");
            }
            finally
            {
                _isLoadingChartSettings = false;
            }
        }

        /// <summary>500ms 节流调度保存，避免连续输入频繁 IO。</summary>
        private void ScheduleSaveChartSettings()
        {
            if (_isLoadingChartSettings) return;
            if (_saveChartSettingsTimer == null)
            {
                _saveChartSettingsTimer = new DispatcherTimer(DispatcherPriority.Background)
                {
                    Interval = TimeSpan.FromMilliseconds(500)
                };
                _saveChartSettingsTimer.Tick += (s, e) =>
                {
                    _saveChartSettingsTimer.Stop();
                    SaveChartSettings();
                };
            }
            _saveChartSettingsTimer.Stop();
            _saveChartSettingsTimer.Start();
        }

        /// <summary>
        /// 读全量 → 改 _default.ChartSettings → 写全量，保留其它 SN 的步骤配置不被覆盖。
        /// </summary>
        private void SaveChartSettings()
        {
            try
            {
                var recipePath = GetRecipePath();
                var allConfigs = StepAnnotationConfig.LoadAll(recipePath);
                StepAnnotationConfig current;
                if (allConfigs.ContainsKey(StepAnnotationConfig.DefaultKey))
                    current = allConfigs[StepAnnotationConfig.DefaultKey];
                else
                    current = new StepAnnotationConfig();

                current.ChartSettings = new ChartSettings
                {
                    XAxisStep = XAxisStep,
                    YAxisStep = YAxisStep,
                    YAxisMax = YAxisMax,
                    XAxisStep2 = XAxisStep2,
                    YAxisStep2 = YAxisStep2,
                    YAxisMax2 = YAxisMax2,
                    OnlyShowPositiveIsEnabled = OnlyShowPositiveIsEnabled,
                    SmoothCurveProcessingsEnabled = SmoothCurveProcessingsEnabled,
                    SliderValue = SliderValue,
                };
                allConfigs[StepAnnotationConfig.DefaultKey] = current;
                StepAnnotationConfig.SaveAll(recipePath, allConfigs);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"保存图表参数失败: {ex.Message}");
            }
        }

        public void Dispose()
        {
            DisposeWatchers();
            foreach (var kvp in _pendingRefreshes)
            {
                kvp.Value.Cancel();
            }
            _pendingRefreshes.Clear();
        }
    }
}
