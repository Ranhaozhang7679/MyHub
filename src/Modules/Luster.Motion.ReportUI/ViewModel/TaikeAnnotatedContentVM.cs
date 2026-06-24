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

        // 自动保存：加载中标志 + 节流计时器
        private bool _isLoadingChartSettings;
        private DispatcherTimer _saveChartSettingsTimer;

        // === 二合一曲线选中/删除交互（仅 IsMergedView 合并视图生效）===
        // 当前悬停的配对索引；-1 = 无高亮。由 View 命中检测后调 SetHoveredCurveByIndex 设置
        private int _hoveredCurveIndex = -1;
        // 已被右键删除的配对索引集合（仅删当前显示，不动 _groupedCache / _raw*Cache / CSV）
        private readonly HashSet<int> _hiddenCurveIndices = new HashSet<int>();
        // 高亮色（橙）：红/蓝原色都能清晰区分；加粗到 3px 让选中曲线在多条叠加中可辨
        private static readonly SKColor HighlightColor = Colors.Orange.ToSKColor();
        private const int HighlightStrokeWidth = 3;
        private const int NormalStrokeWidth = 1;
        // Series.Name 前缀：红/蓝同索引共用 "tk-curve-{i}"，使命中后能从 Series 反查配对索引
        private const string CurveNamePrefix = "tk-curve-";

        // 图表数据
        // 合并模式专用 Series（独立实例，避免与分开左图共享 Series 导致 LiveCharts 内部状态混乱）
        private List<ISeries> _seriesMerge = new List<ISeries>();
        public List<ISeries> SeriesMerge
        {
            get => _seriesMerge;
            set => SetProperty(ref _seriesMerge, value);
        }

        // 分开模式左图专用 Series（Time-Press）
        private List<ISeries> _series = new List<ISeries>();
        public List<ISeries> Series
        {
            get => _series;
            set => SetProperty(ref _series, value);
        }

        // 时间筛选：默认覆盖昨天到明天（整点 00:00:00），由顶部 DateTimePicker 调整
        private DateTime _filterStartTime = DateTime.Today.AddDays(-1);
        public DateTime FilterStartTime
        {
            get => _filterStartTime;
            set => SetProperty(ref _filterStartTime, value);
        }

        private DateTime _filterEndTime = DateTime.Today.AddDays(1);
        public DateTime FilterEndTime
        {
            get => _filterEndTime;
            set => SetProperty(ref _filterEndTime, value);
        }

        private bool _enableTimeFilter = true;
        public bool EnableTimeFilter
        {
            get => _enableTimeFilter;
            set => SetProperty(ref _enableTimeFilter, value);
        }

        // 合并模式专用轴（独立实例，避免与分开左图共享 Axis 导致标签重复渲染）
        // 未导入数据时不显示占位标签，由 RedrawMerged 重新填充完整配置
        private List<Axis> _xAxesMerge = new List<Axis> { new Axis() };
        public List<Axis> XAxesMerge
        {
            get => _xAxesMerge;
            set => SetProperty(ref _xAxesMerge, value);
        }

        private List<Axis> _yAxesMerge = new List<Axis>
        {
            new Axis { Position = AxisPosition.Start },
            new Axis { Position = AxisPosition.End },
        };
        public List<Axis> YAxesMerge
        {
            get => _yAxesMerge;
            set => SetProperty(ref _yAxesMerge, value);
        }

        // 分开模式左图专用轴（Time-Press）
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
        private bool _isChart2TimePosition = true;
        /// <summary>
        /// 图表2是否为Time-Position模式
        /// </summary>
        public bool IsChart2TimePosition
        {
            get => _isChart2TimePosition;
            set => SetProperty(ref _isChart2TimePosition, value);
        }

        // 合并/分开显示：true=单图双 Y 轴叠加, false=左右双子图
        private bool _isMergedView = true;
        /// <summary>
        /// 是否合并显示（单图双 Y 轴）。false 时为左右双子图
        /// </summary>
        public bool IsMergedView
        {
            get => _isMergedView;
            set
            {
                if (_isMergedView == value) return;
                _isMergedView = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsSplitView));
                RaisePropertyChanged(nameof(MergeViewButtonText));
                RaisePropertyChanged(nameof(MergedHeight));
                RaisePropertyChanged(nameof(SplitHeight));
                RedrawChart();
                ScheduleSaveChartSettings();
            }
        }

        /// <summary>
        /// 合并区域高度：合并模式下占满，分开模式下为 0
        /// </summary>
        public GridLength MergedHeight => IsMergedView ? new GridLength(1, GridUnitType.Star) : new GridLength(0);

        /// <summary>
        /// 分开区域高度：分开模式下占满，合并模式下为 0
        /// </summary>
        public GridLength SplitHeight => IsMergedView ? new GridLength(0) : new GridLength(1, GridUnitType.Star);

        /// <summary>
        /// IsMergedView 反向，供 XAML 反向 Visibility 绑定
        /// </summary>
        public bool IsSplitView => !_isMergedView;

        /// <summary>
        /// 切换按钮文字：合并模式下显示"切换分开"，分开模式下显示"切换合并"
        /// </summary>
        public string MergeViewButtonText => IsMergedView ? "切换分开" : "切换合并";

        private string _chart2Title = "Time-Position";
        /// <summary>
        /// 图表2标题
        /// </summary>
        public string Chart2Title
        {
            get => _chart2Title;
            set => SetProperty(ref _chart2Title, value);
        }

        private string _chart2XLabel = "X间隔(ms):";
        public string Chart2XLabel
        {
            get => _chart2XLabel;
            set => SetProperty(ref _chart2XLabel, value);
        }

        private string _chart2YLabel = "Y间隔(mm):";
        public string Chart2YLabel
        {
            get => _chart2YLabel;
            set => SetProperty(ref _chart2YLabel, value);
        }

        private string _chart2YMaxLabel = "Y上限(mm,0自动):";
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
            set
            {
                if (SetProperty(ref _xAxisStep, value))
                    ScheduleSaveChartSettings();
            }
        }

        private double _yAxisStep = 0.05;
        /// <summary>
        /// 图表1 Y轴刻度间隔（kgf）
        /// </summary>
        public double YAxisStep
        {
            get => _yAxisStep;
            set
            {
                if (SetProperty(ref _yAxisStep, value))
                    ScheduleSaveChartSettings();
            }
        }

        private double _xAxisMax = 0;
        /// <summary>
        /// 图表1 Y轴上限（kgf），设置为0时自动调整
        /// </summary>
        public double XAxisMax
        {
            get => _xAxisMax;
            set
            {
                if (SetProperty(ref _xAxisMax, value))
                    ScheduleSaveChartSettings();
            }
        }

        private double _yAxisMax = 0.3;
        /// <summary>
        /// 图表1 Y轴上限（kgf），设置为0时自动调整
        /// </summary>
        public double YAxisMax
        {
            get => _yAxisMax;
            set
            {
                if (SetProperty(ref _yAxisMax, value))
                    ScheduleSaveChartSettings();
            }
        }

        // === 图表2 参数 ===
        private double _xAxisStep2 = 50;
        /// <summary>
        /// 图表2 X轴刻度间隔
        /// </summary>
        public double XAxisStep2
        {
            get => _xAxisStep2;
            set
            {
                if (SetProperty(ref _xAxisStep2, value))
                    ScheduleSaveChartSettings();
            }
        }

        private double _yAxisStep2 = 0.5;
        /// <summary>
        /// 图表2 Y轴刻度间隔
        /// </summary>
        public double YAxisStep2
        {
            get => _yAxisStep2;
            set
            {
                if (SetProperty(ref _yAxisStep2, value))
                    ScheduleSaveChartSettings();
            }
        }

        private double _yAxisMax2 = 5.0;
        /// <summary>
        /// 图表2 Y轴上限，设置为0时自动调整
        /// </summary>
        public double YAxisMax2
        {
            get => _yAxisMax2;
            set
            {
                if (SetProperty(ref _yAxisMax2, value))
                    ScheduleSaveChartSettings();
            }
        }
        //只显示Y+
        private bool _onlyShowPositiveIsEnabled = false;
        public bool OnlyShowPositiveIsEnabled
        {
            get => _onlyShowPositiveIsEnabled;
            set
            {
                if (SetProperty(ref _onlyShowPositiveIsEnabled, value))
                {
                    RedrawChart();
                    ScheduleSaveChartSettings();
                }
            }
        }
        //是否启用平滑曲线
        private bool _smoothCurveProcessingsEnabled = false;
        public bool SmoothCurveProcessingsEnabled
        {
            get => _smoothCurveProcessingsEnabled;
            set
            {
                if (SetProperty(ref _smoothCurveProcessingsEnabled, value))
                    ScheduleSaveChartSettings();
            }
        }

        private int _sliderValue = 11;
        public int SliderValue
        {
            get => _sliderValue;
            set
            {
                if (SetProperty(ref _sliderValue, value))
                    ScheduleSaveChartSettings();
            }
        }

        // ===== 异常数据过滤 =====
        // 现场偶发采集异常：Time 飙到几十秒甚至几十分钟（采集卡死或传感器异常）。
        // 正常 CSV 采集时间 < 5s（不同工位采样频率不同，点数差异大：CGHP1 ~50 点、FIFO ~2500 点都正常）。
        // 所以用 Time 作为主判据；点数阈值仅作兜底防范极端情况。
        // 阈值 10s 远高于正常（5s 内），能精准命中 24s+ 的异常数据。
        private const int AnomalyMaxPoints = 5000;
        private const double AnomalyMaxTimeMs = 10_000;

        private bool _removeAnomalyEnabled = false;
        /// <summary>
        /// 是否去除异常数据。默认 false——保留全部数据。
        /// 开启后按 AnomalyMaxPoints/AnomalyMaxTimeMs 双判据跳过异常 CSV。
        /// 切换后立即重绘当前分组（数据已缓存到 _groupedCache，无需重新导入）。
        /// </summary>
        public bool RemoveAnomalyEnabled
        {
            get => _removeAnomalyEnabled;
            set
            {
                if (SetProperty(ref _removeAnomalyEnabled, value))
                {
                    // 切异常过滤会重建 _rawDataCache，索引语义漂移；清空删除状态保持一致
                    _hiddenCurveIndices.Clear();
                    _hoveredCurveIndex = -1;
                    IsCurveHovered = false;
                    ScheduleSaveChartSettings();
                    ApplyGroupSelection();
                }
            }
        }

        // 上次导入 CSV 的文件夹路径：下次打开对话框时自动定位到此处
        // 非绑定属性，仅持久化用；空串表示从未导入过
        private string _lastImportFolder = string.Empty;
        public string LastImportFolder
        {
            get => _lastImportFolder;
            set => SetProperty(ref _lastImportFolder, value ?? string.Empty);
        }

        // 硬编码兜底路径：当从未导入过（LastImportFolder 为空）时作为首次对话框默认路径
        private const string DefaultImportFolder = @"D:\lmv-2026-043011\0430FCLP2\CC上传\NUM1";
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

        // ===== 分组筛选：导入后按 (工位, 日期, 工序) 三元组分组，下拉框切换 =====
        // 路径模式: {root}\{工位}\{日期yyyyMMdd}\{时间戳}\{SN} {工序}.csv.csv
        // 分组键 = "工位 / 日期 / 工序"; 显示键 = 分组键 + "  (N 条)"
        private readonly Dictionary<string, GroupCurves> _groupedCache = new Dictionary<string, GroupCurves>();
        private readonly Dictionary<string, string> _displayToRawKey = new Dictionary<string, string>();

        private List<string> _groupKeys = new List<string>();
        /// <summary>
        /// 分组下拉框的选项列表（显示用，含条数）
        /// </summary>
        public List<string> GroupKeys
        {
            get => _groupKeys;
            set => SetProperty(ref _groupKeys, value);
        }

        private string _selectedGroupKey;
        /// <summary>
        /// 当前选中的分组（显示键）。切换时触发 ApplyGroupSelection 重新装载该分组数据并重绘。
        /// </summary>
        public string SelectedGroupKey
        {
            get => _selectedGroupKey;
            set
            {
                if (SetProperty(ref _selectedGroupKey, value))
                {
                    // 切换分组：清空删除/悬停状态（索引在新分组里指向不同曲线）
                    _hiddenCurveIndices.Clear();
                    _hoveredCurveIndex = -1;
                    IsCurveHovered = false;
                    ApplyGroupSelection();
                }
            }
        }

        /// <summary>
        /// 一条 CSV 曲线的解析结果（含异常标记）。导入时全部保留，绘制时按 RemoveAnomalyEnabled 过滤。
        /// </summary>
        private class CurveEntry
        {
            public List<ObservablePoint> TimePress;
            public List<ObservablePoint> PosPress;
            public List<ObservablePoint> TimePos;
            public double TimeMax;
            public double PressMin;
            public double PressMax;
            public double PositionMin;
            public double PositionMax;
            public bool IsAnomaly;
        }

        /// <summary>
        /// 一个分组下的所有曲线 Entry
        /// </summary>
        private class GroupCurves
        {
            public List<CurveEntry> Entries = new List<CurveEntry>();
        }

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

            LoadChartSettings();
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
        /// 切换图表2模式：Position-Press / Time-Position（仅分开模式有效）
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

        private DelegateCommand _toggleMergeViewCommand;
        /// <summary>
        /// 切换显示模式：合并单图（双 Y 轴）/ 分开双子图（左右 Time-Press + Time-Position）
        /// </summary>
        public DelegateCommand ToggleMergeViewCommand =>
            _toggleMergeViewCommand ?? (_toggleMergeViewCommand = new DelegateCommand(() =>
            {
                IsMergedView = !IsMergedView;
            }));

        // === 二合一曲线选中/删除交互（仅合并视图）===
        // 是否有曲线被悬停。供 ContextMenu 的"删除该曲线"MenuItem IsEnabled 绑定。
        private bool _isCurveHovered;
        public bool IsCurveHovered
        {
            get => _isCurveHovered;
            private set
            {
                if (SetProperty(ref _isCurveHovered, value))
                    _deleteHoveredCurveCommand?.RaiseCanExecuteChanged();
            }
        }

        private DelegateCommand _deleteHoveredCurveCommand;
        /// <summary>
        /// 右键删除当前悬停的配对曲线：从 SeriesMerge 临时移除（仅删当前显示，不动源数据）。
        /// 仅在 _hoveredCurveIndex ≥ 0 时可执行。
        /// </summary>
        public DelegateCommand DeleteHoveredCurveCommand =>
            _deleteHoveredCurveCommand ?? (_deleteHoveredCurveCommand = new DelegateCommand(
                ExecuteDeleteHoveredCurve,
                () => _hoveredCurveIndex >= 0));

        private void ExecuteDeleteHoveredCurve()
        {
            if (_hoveredCurveIndex < 0) return;
            _hiddenCurveIndices.Add(_hoveredCurveIndex);
            int removed = _hoveredCurveIndex;
            _hoveredCurveIndex = -1;
            IsCurveHovered = false;
            RedrawChart();
        }

        /// <summary>
        /// View 层命中检测后回调：更新当前选中的配对索引。
        /// 传入 -1 表示未命中任何曲线。
        /// 注意：LiveCharts2 只监听数据变更，修改 LineSeries.Stroke 不会触发重绘，
        /// 所以必须 RedrawChart 重建 SeriesMerge，BuildMergedLineSeries 中才会按新 _hoveredCurveIndex 用橙色加粗。
        /// </summary>
        public void SetHoveredCurveByIndex(int curveIndex)
        {
            if (curveIndex == _hoveredCurveIndex) return;
            _hoveredCurveIndex = curveIndex;
            IsCurveHovered = curveIndex >= 0;
            if (IsMergedView && _rawDataCache.Count > 0)
                RedrawChart();
        }

        #endregion

        #region CSV 导入

        private void ImportCsvFiles()
        {
            if (_dispatcher == null) _dispatcher = Application.Current?.Dispatcher;

            // 重新导入新数据：之前的"已删除"状态语义失效（索引对应的是旧分组），清空
            _hiddenCurveIndices.Clear();
            _hoveredCurveIndex = -1;
            IsCurveHovered = false;

            // 选择文件夹：递归扫描该文件夹（含子文件夹）下所有 CSV
            var folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderDialog.Description = "请选择压力曲线数据所在文件夹（含子文件夹，将自动扫描所有 CSV）";
            folderDialog.ShowNewFolderButton = false;
            // 不设置 RootFolder=Desktop：该限制会让 SelectedPath 难以定位到 D 盘下路径，
            // 参考 TaikeContentVM.ImportTotalAuto 的写法（同样未限制 RootFolder）。
            // 优先用上次导入的文件夹，否则用硬编码兜底；都不存在时 SelectedPath 留空，
            // FolderBrowserDialog 会自动回退到系统默认位置。
            string initialFolder = !string.IsNullOrEmpty(_lastImportFolder) && System.IO.Directory.Exists(_lastImportFolder)
                ? _lastImportFolder
                : DefaultImportFolder;
            if (System.IO.Directory.Exists(initialFolder))
                folderDialog.SelectedPath = initialFolder;

            if (folderDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            string selectedFolder = folderDialog.SelectedPath;
            if (string.IsNullOrEmpty(selectedFolder) || !System.IO.Directory.Exists(selectedFolder)) return;

            string[] files = System.IO.Directory.GetFiles(selectedFolder, "*.csv", System.IO.SearchOption.AllDirectories);
            if (files.Length == 0)
            {
                MessageBox.Show("所选文件夹中没有找到任何 CSV 文件。", "提示");
                return;
            }

            // 文件名时间戳过滤：保留 startTime <= fileTimestamp <= endTime 的文件
            if (_enableTimeFilter)
            {
                files = files.Where(f =>
                    FileNameTimestampParser.IsInTimeRange(f, _filterStartTime, _filterEndTime)).ToArray();
                if (files.Length == 0)
                {
                    MessageBox.Show(
                        $"在所选时间范围内没有找到 CSV 文件。\n开始: {_filterStartTime:yyyy-MM-dd HH:mm:ss}\n结束: {_filterEndTime:yyyy-MM-dd HH:mm:ss}",
                        "提示");
                    return;
                }
            }

            _groupedCache.Clear();
            _displayToRawKey.Clear();

            foreach (var file in files)
            {
                List<TotalPressModel> pressModels = CSVTool.OpenCSV<TotalPressModel>(file);
                var values = new List<ObservablePoint>();
                var posPressValues = new List<ObservablePoint>();
                var timePosValues = new List<ObservablePoint>();
                double timeMax = double.MinValue;
                double pressMin = double.MaxValue;
                double pressMax = double.MinValue;
                double positionMin = double.MaxValue;
                double positionMax = double.MinValue;

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

                // 异常判据：点数过多 OR Time 范围过大（典型异常是 1.9 万点、Time 飙到百万 ms）
                bool isAnomaly = pressModels.Count > AnomalyMaxPoints || timeMax > AnomalyMaxTimeMs;

                var entry = new CurveEntry
                {
                    TimePress = values,
                    PosPress = posPressValues,
                    TimePos = timePosValues,
                    TimeMax = timeMax,
                    PressMin = pressMin,
                    PressMax = pressMax,
                    PositionMin = positionMin,
                    PositionMax = positionMax,
                    IsAnomaly = isAnomaly,
                };

                string rawKey = ParseGroupKey(file);
                if (!_groupedCache.TryGetValue(rawKey, out var bucket))
                {
                    bucket = new GroupCurves();
                    _groupedCache[rawKey] = bucket;
                }
                bucket.Entries.Add(entry);
            }

            // 生成显示键列表（含 正常/总数 条数），并建立 显示键 → 原始键 反向映射
            var displayKeys = new List<string>();
            foreach (var rawKey in _groupedCache.Keys.OrderBy(k => k))
            {
                var bucket = _groupedCache[rawKey];
                int total = bucket.Entries.Count;
                int normal = bucket.Entries.Count(e => !e.IsAnomaly);
                // 没有异常时只显示总数；有异常时显示 "正常/总数"
                string countPart = (normal == total) ? $"{total} 条" : $"{normal}/{total} 条";
                string display = $"{rawKey}  ({countPart})";
                _displayToRawKey[display] = rawKey;
                displayKeys.Add(display);
            }
            GroupKeys = displayKeys;

            // 自动加载步骤配置
            LoadSteps();

            // 默认选第一项；为空则清空当前显示
            // 显式 ApplyGroupSelection 兜底：若 SelectedGroupKey 与上次相同（如重复导入同文件夹），
            // setter 的 SetProperty 返回 false 不会触发重绘
            SelectedGroupKey = displayKeys.Count > 0 ? displayKeys[0] : null;
            ApplyGroupSelection();

            // 记忆本次选择的文件夹，供下次打开对话框时自动定位
            if (!string.Equals(_lastImportFolder, selectedFolder, StringComparison.OrdinalIgnoreCase))
            {
                LastImportFolder = selectedFolder;
                SaveChartSettings();
            }
        }

        /// <summary>
        /// 从 CSV 路径解析分组键。
        /// - 第一个 8 位纯数字目录作为日期，其父目录作为工位
        /// - 若日期目录下紧跟 NUM1/NUM2 段（FIFO/FCLP 等工位有此结构），把工位号纳入键：工位 / 日期 / 工位号 / 工序
        /// - 否则键为：工位 / 日期 / 工序
        /// 文件名空格后的部分作为工序；后缀支持 .csv 和 .csv.csv；解析失败的字段回退为 "_unknown"。
        /// </summary>
        private static string ParseGroupKey(string csvPath)
        {
            if (string.IsNullOrEmpty(csvPath)) return "_unknown / _unknown / _unknown";

            string normalized = csvPath.Replace('/', '\\');
            var parts = normalized.Split('\\');
            string date = null;
            int dateIdx = -1;
            for (int i = 0; i < parts.Length; i++)
            {
                string seg = parts[i];
                if (seg.Length == 8 && long.TryParse(seg, out _))
                {
                    date = seg;
                    dateIdx = i;
                    break;
                }
            }
            string station = (dateIdx >= 1) ? parts[dateIdx - 1] : null;

            // 日期目录后紧跟的 NUM1/NUM2 段（FIFO 等：FIFO/{date}/{NUM1|NUM2}/{SN}/file）
            string stationNum = null;
            if (dateIdx >= 0 && dateIdx + 1 < parts.Length)
            {
                string nextSeg = parts[dateIdx + 1];
                if (nextSeg.Equals("NUM1", StringComparison.OrdinalIgnoreCase) ||
                    nextSeg.Equals("NUM2", StringComparison.OrdinalIgnoreCase))
                {
                    stationNum = nextSeg.ToUpperInvariant();
                }
            }

            string fname = Path.GetFileName(csvPath);
            string lower = fname.ToLowerInvariant();
            if (lower.EndsWith(".csv.csv")) fname = fname.Substring(0, fname.Length - 8);
            else if (lower.EndsWith(".csv")) fname = fname.Substring(0, fname.Length - 4);

            string step = null;
            int spaceIdx = fname.IndexOf(' ');
            if (spaceIdx >= 0) step = fname.Substring(spaceIdx + 1).Trim();

            string stationPart = station ?? "_unknown";
            string datePart = date ?? "_unknown";
            string stepPart = step ?? "_unknown";

            return stationNum != null
                ? $"{stationPart} / {datePart} / {stationNum} / {stepPart}"
                : $"{stationPart} / {datePart} / {stepPart}";
        }

        /// <summary>
        /// 响应分组下拉切换 / 异常过滤开关切换：按当前 RemoveAnomalyEnabled 决定是否包含异常 Entry，
        /// 装入 _rawDataCache 等，重算轴范围并重绘。
        /// </summary>
        private void ApplyGroupSelection()
        {
            if (string.IsNullOrEmpty(SelectedGroupKey) ||
                !_displayToRawKey.TryGetValue(SelectedGroupKey, out var rawKey) ||
                !_groupedCache.TryGetValue(rawKey, out var bucket))
            {
                _rawDataCache = new List<List<ObservablePoint>>();
                _rawPositionPressCache = new List<List<ObservablePoint>>();
                _rawTimePositionCache = new List<List<ObservablePoint>>();
                _rawDataCache1 = new List<List<ObservablePoint>>();
                return;
            }

            IEnumerable<CurveEntry> entries = bucket.Entries;
            if (_removeAnomalyEnabled)
                entries = entries.Where(e => !e.IsAnomaly);

            var entryList = entries.ToList();
            _rawDataCache = entryList.Select(e => e.TimePress).ToList();
            _rawPositionPressCache = entryList.Select(e => e.PosPress).ToList();
            _rawTimePositionCache = entryList.Select(e => e.TimePos).ToList();
            _rawDataCache1 = new List<List<ObservablePoint>>();

            double timeMax = entryList.Count > 0 ? entryList.Max(e => e.TimeMax) : 0;
            double pressMin = entryList.Count > 0 ? entryList.Min(e => e.PressMin) : double.MaxValue;
            double pressMax = entryList.Count > 0 ? entryList.Max(e => e.PressMax) : double.MinValue;
            double positionMin = entryList.Count > 0 ? entryList.Min(e => e.PositionMin) : double.MaxValue;
            double positionMax = entryList.Count > 0 ? entryList.Max(e => e.PositionMax) : double.MinValue;

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

        #region 图表参数自动保存

        /// <summary>
        /// 从 StepAnnotationConfig.json 的 _default 条目加载图表参数（与步骤配置共用同一文件）。
        /// 在带参构造函数末尾调用，加载期间关闭自动保存避免回写。
        /// </summary>
        private void LoadChartSettings()
        {
            try
            {
                _isLoadingChartSettings = true;
                var config = StepAnnotationConfig.LoadByCsvName(null, GetRecipePath());
                var s = config.ChartSettings ?? new ChartSettings();
                // 读取上次导入文件夹，仅作字段缓存；真实 SelectedPath 在 ImportCsvFiles 中按存在性校验
                LastImportFolder = s.LastImportFolder ?? string.Empty;
                XAxisStep = s.XAxisStep;
                YAxisStep = s.YAxisStep;
                YAxisMax = s.YAxisMax;
                XAxisStep2 = s.XAxisStep2;
                YAxisStep2 = s.YAxisStep2;
                YAxisMax2 = s.YAxisMax2;
                OnlyShowPositiveIsEnabled = s.OnlyShowPositiveIsEnabled;
                SmoothCurveProcessingsEnabled = s.SmoothCurveProcessingsEnabled;
                SliderValue = s.SliderValue;
                IsMergedView = s.IsMergedView;
                RemoveAnomalyEnabled = s.RemoveAnomalyEnabled;
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

        /// <summary>
        /// 节流调度保存：500ms 内多次属性变更只写盘一次，避免连续输入时频繁 IO。
        /// </summary>
        private void ScheduleSaveChartSettings()
        {
            if (_isLoadingChartSettings) return;
            if (_dispatcher == null) _dispatcher = Application.Current?.Dispatcher;
            if (_dispatcher == null) { SaveChartSettings(); return; }

            _dispatcher.Invoke(new Action(() =>
            {
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
            }));
        }

        /// <summary>
        /// 将当前图表参数写入 StepAnnotationConfig.json 的 _default 条目，保留其它 CSV 文件名下的步骤配置。
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
                    IsMergedView = IsMergedView,
                    RemoveAnomalyEnabled = RemoveAnomalyEnabled,
                    LastImportFolder = LastImportFolder ?? string.Empty,
                };
                allConfigs[StepAnnotationConfig.DefaultKey] = current;
                StepAnnotationConfig.SaveAll(recipePath, allConfigs);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"保存图表参数失败: {ex.Message}");
            }
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
            double pressAbsMax = Math.Max(Math.Abs(pressMin), Math.Abs(pressMax));

            if (IsMergedView)
                RedrawMerged(timeMax, pressMin, pressMax, positionMin, positionMax, pressAbsMax);
            else
                RedrawSplit(timeMax, pressMin, pressMax, positionMin, positionMax, pressAbsMax);
        }

        // === 合并模式：单图双 Y 轴（Time-Press 红 左 Y + Time-Position 蓝 右 Y） ===
        private void RedrawMerged(double timeMax, double pressMin, double pressMax, double positionMin, double positionMax, double pressAbsMax)
        {
            var chartSeries = new List<ISeries>();
            if (_smoothCurveProcessingsEnabled)
            {
                for (int i = 0; i < _rawDataCache1.Count; i++)
                {
                    if (_hiddenCurveIndices.Contains(i)) continue;
                    chartSeries.Add(BuildMergedLineSeries(_rawDataCache1[i], i, isPress: true));
                }
            }
            else
            {
                for (int i = 0; i < _rawDataCache.Count; i++)
                {
                    if (_hiddenCurveIndices.Contains(i)) continue;
                    chartSeries.Add(BuildMergedLineSeries(_rawDataCache[i], i, isPress: true));
                }
            }

            for (int i = 0; i < _rawTimePositionCache.Count; i++)
            {
                if (_hiddenCurveIndices.Contains(i)) continue;
                chartSeries.Add(BuildMergedLineSeries(_rawTimePositionCache[i], i, isPress: false));
            }

            double xStep1 = XAxisStep > 0 ? XAxisStep : 50;
            double yStep1 = YAxisStep > 0 ? YAxisStep : 0.05;
            double yMax1 = YAxisMax > 0 ? YAxisMax : pressAbsMax * 1.05;
            // 自适应：timeMax 异常大时（如异常数据 190 万 ms）用户设的小步长会产生过多刻度,
            // 用 CalculateNiceStep 估算 ~12 个刻度的步长作为下限,避免标签挤成一团
            double xNiceStep = CalculateNiceStep(timeMax, 12);
            if (xStep1 < xNiceStep) xStep1 = xNiceStep;
            double xMax = XAxisMax > 0 ? XAxisMax : Math.Ceiling(timeMax / xStep1) * xStep1 + xStep1;

            double niceStep1 = CalculateNiceStep(yMax1, 8);
            if (yStep1 < niceStep1) yStep1 = niceStep1;

            double posAbsMax = Math.Max(Math.Abs(positionMin), Math.Abs(positionMax));
            double yStep2 = YAxisStep2 > 0 ? YAxisStep2 : 0.5;
            double yMax2 = YAxisMax2 > 0 ? YAxisMax2 : (posAbsMax * 1.1);
            double niceStep2 = CalculateNiceStep(yMax2, 8);
            if (yStep2 < niceStep2) yStep2 = niceStep2;

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

            var axis_y_press = new Axis();
            axis_y_press.Name = "Press/kgf";
            axis_y_press.NameTextSize = 12;
            axis_y_press.TextSize = 10;
            axis_y_press.MinLimit = _onlyShowPositiveIsEnabled ? 0 : -yMax1;
            axis_y_press.MaxLimit = yMax1;
            axis_y_press.MinStep = yStep1;
            axis_y_press.ForceStepToMin = true;
            axis_y_press.ShowSeparatorLines = true;
            axis_y_press.SeparatorsPaint = new SolidColorPaint(Colors.LightGray.ToSKColor(), 1);
            axis_y_press.Padding = new Padding(4, 0, 4, 0);
            axis_y_press.NamePadding = new Padding(4, 0, 4, 0);
            axis_y_press.Position = AxisPosition.Start;
            axis_y_press.NamePaint = new SolidColorPaint(Colors.Red.ToSKColor());
            axis_y_press.LabelsPaint = new SolidColorPaint(Colors.Red.ToSKColor());

            var axis_y_position = new Axis();
            axis_y_position.Name = "Position/mm";
            axis_y_position.NameTextSize = 12;
            axis_y_position.TextSize = 10;
            axis_y_position.MinLimit = _onlyShowPositiveIsEnabled ? 0 : -yMax2;
            axis_y_position.MaxLimit = yMax2;
            axis_y_position.MinStep = yStep2;
            axis_y_position.ForceStepToMin = true;
            axis_y_position.ShowSeparatorLines = false;
            axis_y_position.SeparatorsPaint = new SolidColorPaint(Colors.Transparent.ToSKColor(), 1);
            axis_y_position.Padding = new Padding(4, 0, 4, 0);
            axis_y_position.NamePadding = new Padding(4, 0, 4, 0);
            axis_y_position.Position = AxisPosition.End;
            axis_y_position.NamePaint = new SolidColorPaint(Colors.Blue.ToSKColor());
            axis_y_position.LabelsPaint = new SolidColorPaint(Colors.Blue.ToSKColor());

            DrawStepAnnotations(chartSeries, _steps, pressMax, xMax);

            _dispatcher.Invoke(new Action(() =>
            {
                XAxesMerge = new List<Axis> { axis_x_time };
                YAxesMerge = new List<Axis> { axis_y_press, axis_y_position };
                SeriesMerge = chartSeries;
            }));
        }

        // 构造合并视图的一条曲线 Series：Name 用 "tk-curve-{pairIndex}" 让红/蓝同索引配对；
        // _hoveredCurveIndex 命中时改用橙色高亮 + 加粗，便于在密集叠加中辨识选中曲线
        private LineSeries<ObservablePoint> BuildMergedLineSeries(List<ObservablePoint> values, int pairIndex, bool isPress)
        {
            bool highlight = _hoveredCurveIndex == pairIndex;
            var baseColor = isPress ? Colors.Red.ToSKColor() : Colors.Blue.ToSKColor();
            var line = new LineSeries<ObservablePoint>();
            line.Stroke = new SolidColorPaint(
                highlight ? HighlightColor : baseColor,
                highlight ? HighlightStrokeWidth : NormalStrokeWidth);
            line.LineSmoothness = 0;
            line.Fill = new SolidColorPaint(Colors.Transparent.ToSKColor(), 1);
            line.GeometrySize = 0;
            line.MiniatureShapeSize = 0;
            line.Name = $"{CurveNamePrefix}{pairIndex}";
            line.Values = new List<ObservablePoint>(values);
            line.ScalesYAt = isPress ? 0 : 1;
            return line;
        }

        // === 分开模式：左右双子图，共享 Time X 轴 ===
        private void RedrawSplit(double timeMax, double pressMin, double pressMax, double positionMin, double positionMax, double pressAbsMax)
        {
            // 左侧子图：Time-Press（红）
            var chart1Series = new List<ISeries>();
            if (_smoothCurveProcessingsEnabled)
            {
                foreach (var cachedValues in _rawDataCache1)
                {
                    var line = new LineSeries<ObservablePoint>();
                    line.Stroke = new SolidColorPaint(Colors.Red.ToSKColor(), 1);
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
                    line.Stroke = new SolidColorPaint(Colors.Red.ToSKColor(), 1);
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
            // 自适应：timeMax 异常大时（如异常数据 190 万 ms）用户设的小步长会产生过多刻度,
            // 用 CalculateNiceStep 估算 ~12 个刻度的步长作为下限,避免标签挤成一团
            double xNiceStep = CalculateNiceStep(timeMax, 12);
            if (xStep1 < xNiceStep) xStep1 = xNiceStep;
            double xMax = XAxisMax > 0 ? XAxisMax : Math.Ceiling(timeMax / xStep1) * xStep1 + xStep1;

            double niceStep1 = CalculateNiceStep(yMax1, 8);
            if (yStep1 < niceStep1) yStep1 = niceStep1;

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
            axis_y_press1.MinLimit = _onlyShowPositiveIsEnabled ? 0 : -yMax1;
            axis_y_press1.MaxLimit = yMax1;
            axis_y_press1.MinStep = yStep1;
            axis_y_press1.ForceStepToMin = true;
            axis_y_press1.ShowSeparatorLines = true;
            axis_y_press1.SeparatorsPaint = new SolidColorPaint(Colors.LightGray.ToSKColor(), 1);
            axis_y_press1.Padding = new Padding(4, 0, 4, 0);
            axis_y_press1.NamePadding = new Padding(4, 0, 4, 0);

            DrawStepAnnotations(chart1Series, _steps, pressMax, xMax);

            // 右侧子图：Time-Position（蓝，共享 Time X 轴）
            var chart2Series = new List<ISeries>();
            foreach (var cachedValues in _rawTimePositionCache)
            {
                var line = new LineSeries<ObservablePoint>();
                line.Stroke = new SolidColorPaint(Colors.Blue.ToSKColor(), 1);
                line.LineSmoothness = 0;
                line.Fill = new SolidColorPaint(Colors.Transparent.ToSKColor(), 1);
                line.GeometrySize = 0;
                line.MiniatureShapeSize = 0;
                line.Name = null;
                line.Values = new List<ObservablePoint>(cachedValues);
                chart2Series.Add(line);
            }

            double posAbsMax = Math.Max(Math.Abs(positionMin), Math.Abs(positionMax));
            double yStep2 = YAxisStep2 > 0 ? YAxisStep2 : 0.5;
            double yMax2 = YAxisMax2 > 0 ? YAxisMax2 : (posAbsMax * 1.1);
            double niceStep2 = CalculateNiceStep(yMax2, 8);
            if (yStep2 < niceStep2) yStep2 = niceStep2;

            var axis_x2 = new Axis();
            axis_x2.Name = "Time/ms";
            axis_x2.NameTextSize = 12;
            axis_x2.TextSize = 10;
            axis_x2.MinLimit = 0;
            axis_x2.MaxLimit = xMax;
            axis_x2.MinStep = xStep1;
            axis_x2.ForceStepToMin = true;
            axis_x2.ShowSeparatorLines = true;
            axis_x2.SeparatorsPaint = new SolidColorPaint(Colors.LightGray.ToSKColor(), 1);
            axis_x2.Padding = new Padding(4, 0, 4, 0);
            axis_x2.NamePadding = new Padding(4, 0, 4, 0);

            var axis_y2 = new Axis();
            axis_y2.Name = "Position/mm";
            axis_y2.NameTextSize = 12;
            axis_y2.TextSize = 10;
            axis_y2.MinLimit = _onlyShowPositiveIsEnabled ? 0 : -yMax2;
            axis_y2.MaxLimit = yMax2;
            axis_y2.MinStep = yStep2;
            axis_y2.ForceStepToMin = true;
            axis_y2.ShowSeparatorLines = true;
            axis_y2.SeparatorsPaint = new SolidColorPaint(Colors.LightGray.ToSKColor(), 1);
            axis_y2.Padding = new Padding(4, 0, 4, 0);
            axis_y2.NamePadding = new Padding(4, 0, 4, 0);

            _dispatcher.Invoke(new Action(() =>
            {
                XAxes = new List<Axis> { axis_x_time };
                YAxes = new List<Axis> { axis_y_press1 };
                Series = chart1Series;

                XAxes2 = new List<Axis> { axis_x2 };
                YAxes2 = new List<Axis> { axis_y2 };
                Series2 = chart2Series;
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
        public static List<List<ObservablePoint>> Pxcl(List<List<ObservablePoint>> observablePoint, int Windowsize)
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
                    tempdata.Y = doubles[i];
                    hc.Add(tempdata);
                }
                date.Add(hc);
            }
            return date;
        }
    }
}
