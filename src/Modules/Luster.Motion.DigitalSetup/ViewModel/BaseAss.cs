using HandyControl.Controls;
using HandyControl.Data;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using Luster.Common.Assets;
using Luster.Common.Assets.FloatingInfo.Services;
using Luster.Common.DataAccess.Repositories;
using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.CommonUI;
using Luster.Motion.DigitalSetup.AssTables;
using Luster.Motion.DigitalSetup.Datas;
using Luster.Motion.DigitalSetup.Helpers;
using Luster.Motion.DigitalSetup.Services;
using Luster.Motion.EditorUI;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Motion.Logic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Linq;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using MessageBox = System.Windows.MessageBox;


namespace Luster.Motion.DigitalSetup.ViewModel
{
    public class BaseAss : BindableBase, IRegionMemberLifetime
    {
        public string csvPath = @"D:\Motion\AssData.csv";
        public CancellationTokenSource _cts = new CancellationTokenSource();

        /// <summary>
        /// 保持视图实例不被销毁，切换页面时保留状态
        /// </summary>
        public bool KeepAlive => true;

        /// <summary>
        /// 标记是否已初始化，防止LoadedCommand重复触发
        /// </summary>
        protected bool _isInitialized = false;

        /// <summary>
        /// 全局保存命令，用于注册到GlobalCommands.SaveCommand
        /// </summary>
        private DelegateCommand _globalSaveCommand;
        private Task _task;
        private readonly SynchronizationContext _syncContext;
        // 定义任务状态事件
        public event EventHandler<string> TaskStarted;
        public event EventHandler<string> TaskCompleted;
        public event EventHandler<string> TaskCanceled;
        public event EventHandler<string> TaskFailed;

        /// <summary>
        /// 数据库访问
        /// </summary>
        protected IRepository _reporitory;

        protected IRegionManager _regionManager;

        protected ICommonBus _commonbus;
        protected FlowBus _flowBus;

        public readonly CSVHelper _csvHelper;
        public string ConfigKey { get; set; } // 每个界面ViewModel设置自己的字段名

        /// <summary>
        /// 父页面Region名称，用于构建子页面的浮动信息窗口PageId
        /// 子类应在构造函数中设置此属性
        /// </summary>
        protected string _parentRegionName = "DigitalInfoDialog";

        /// <summary>
        /// 持久化服务，用于保存和加载页面配置
        /// </summary>
        protected DataValidationPersistenceService _persistenceService;

        /// <summary>
        /// 页面启用设置服务，用于加载和应用页面启用状态
        /// </summary>
        protected PageEnableSettingsService _pageEnableSettingsService;

        /// <summary>
        /// 浮动信息服务，用于显示浮动信息窗口
        /// </summary>
        protected IFloatingInfoService _floatingInfoService;

        /// <summary>
        /// 对话框服务，用于显示对话框（子类可设置）
        /// </summary>
        protected IDialogService _dialogService;

        /// <summary>
        /// 设置点检确认消息命令
        /// </summary>
        public ICommand SetCheckMessageCommand { get; protected set; }


        #region  属性
        /// <summary>
        /// 跳转页码
        /// </summary>
        private int _pageIndex;
        public int PageIndex
        {
            get { return _pageIndex; }
            set { SetProperty(ref _pageIndex, value); }
        }

        private string _logMsg;
        public string LogMsg
        {
            get { return _logMsg; }
            set { SetProperty(ref _logMsg, value); }
        }

        /// <summary>
        /// 每页数量
        /// </summary>
        private int _perPageCount = 20;
        public int PerPageCount
        {
            get { return _perPageCount; }
            set { SetProperty(ref _perPageCount, value); }
        }

        /// <summary>
        /// 页数
        /// </summary>
        private int _pageCount;
        public int PageCount
        {
            get { return _pageCount; }
            set { SetProperty(ref _pageCount, value); }
        }

        /// <summary>
        /// 子界面表格信息
        /// </summary>
        private ObservableCollection<object> _itemModels;
        public ObservableCollection<object> ItemModels
        {
            get { return _itemModels; }
            set { SetProperty(ref _itemModels, value); }
        }

        /// <summary>
        /// 当前选中的页
        /// </summary>
        private CommonPageModel _seletedReportPage;
        public CommonPageModel SelectedReportPage
        {
            get => _seletedReportPage;
            set
            {
                if (_seletedReportPage != null)
                {
                    if (value != null && _seletedReportPage?.Name != value.Name)
                    {
                        SaveGridItems(ItemModels);
                    }

                }
                SetProperty(ref _seletedReportPage, value);
            }
        }

        /// <summary>
        /// 当前选中的行
        /// </summary>
        private object _selectedLine;
        public object SelectedLine
        {
            get { return _selectedLine; }
            set { SetProperty(ref _selectedLine, value); }
        }

        #endregion

        #region  Command
        public ObservableCollection<CommonPageModel> Pages { get; set; }

        public DelegateCommand<FunctionEventArgs<int>> PageUpdatedCommand { get; set; }

        public DelegateCommand<CommonPageModel> SelectedCommand { get; set; }

        /// <summary>
        /// 查询
        /// </summary>
        private DelegateCommand _queryCommand;
        public DelegateCommand QueryCommand => _queryCommand ?? (_queryCommand = new DelegateCommand(() =>
        {
            InitModels();
        }));

        /// <summary>
        /// UnLoaded
        /// </summary>
        private DelegateCommand<ObservableCollection<object>> _unLoadedCommand;
        public DelegateCommand<ObservableCollection<object>> UnLoadedCommand => _unLoadedCommand ?? (_unLoadedCommand = new DelegateCommand<ObservableCollection<object>>((items) =>
        {
            SaveGridItems(items);
        }));

        /// <summary>
        /// 模块加载 - 只在第一次加载时初始化，避免重复触发
        /// </summary>
        private DelegateCommand<object> _loadedCommand;
        public DelegateCommand<object> LoadedCommand => _loadedCommand ?? (_loadedCommand = new DelegateCommand<object>((obj) =>
        {
            if (!_isInitialized)
            {
                _isInitialized = true;
                InitModels();
            }
        }));

        #endregion


        #region 工站选择

        public ICommand SaveStationConfigCommand { get; set; }

        public ObservableCollection<string> stationNames { get; set; } = new ObservableCollection<string>();

        protected string _selectedStationName;
        public virtual string SelectedStationName
        {
            get => _selectedStationName;
            set { SetProperty(ref _selectedStationName, value); }
        }

        private int _stationCount = 4;
        public int StationCount
        {
            get => _stationCount;
            set
            {
                if (SetProperty(ref _stationCount, value))
                {
                    UpdateStationConfigs();
                    UpdateStationNames();
                }
            }
        }

        private string _selectedGlobal;

        public string SelectedGlobal
        {
            get => _selectedGlobal;
            set => SetProperty(ref _selectedGlobal, value);
        }

        /// <summary>
        /// 工站集合
        /// </summary>
        private ObservableCollection<StationConfig> _stationConfigs = new ObservableCollection<StationConfig>();

        public ObservableCollection<StationConfig> StationConfigs
        {
            get => _stationConfigs;
            set
            {
                if (SetProperty(ref _stationConfigs, value))
                {
                    UpdateStationNames();
                }
            }
        }

        private void UpdateStationNames()
        {
            stationNames.Clear();
            for (int i = 0; i < StationConfigs.Count; i++)
            {
                stationNames.Add($"{StationConfigs[i].Name}");
            }
        }

        /// <summary>
        /// 刷新工站配置列表
        /// </summary>
        public void UpdateStationConfigs()
        {
            // 保持StationConfigs数量与StationCount一致
            while (StationConfigs.Count < StationCount)
            {
                StationConfigs.Add(new StationConfig { Name = $"工站{StationConfigs.Count + 1}" });
            }
            while (StationConfigs.Count > StationCount)
            {
                StationConfigs.RemoveAt(StationConfigs.Count - 1);
            }
        }

        /// <summary>
        /// 保存工站配置处理方法
        /// </summary>
        public void SaveStationConfigHandler()
        {
            SaveStationConfig(); // 原有全局变量逻辑
            SaveStationConfigToJson(); // 新增保存到JSON
        }

        /// <summary>
        /// 保存工站配置到全局变量
        /// </summary>
        public void SaveStationConfig()
        {
            try
            {
                // 获取全局模块ID
                var gID = GlobalModule.GlobalID;
                var gModule = _flowBus.GetModule(gID);

                // 参数名为 "Ass_SelecStation"，可根据实际参数名调整
                string paramKey = SelectedGlobal = "Extend_Ass_SelectedStation";
                if (gModule.Parameters.ContainsKey(paramKey))
                {
                    var param = gModule.Parameters[paramKey];
                    param.Value = SelectedStationName;
                }
                else
                {
                    // 自动新建全局变量
                    var paramAttr = ParameterAttribute.CreateByType(
                        paramKey,
                        typeof(string),
                        gModule,
                        "Ass_SelectedStation",
                        5,
                        ParamType.IN
                    );
                    paramAttr.Value = SelectedStationName;
                    gModule.Parameters.Add(paramKey, paramAttr);
                    //throw new FriendlyException($"未找到全局变量{paramKey}");
                }
            }
            catch (Exception ex)
            {
                // 可根据实际需求记录日志或提示
                throw;
            }
        }

        /// <summary>
        /// 保存工站配置到Json
        /// </summary>
        public void SaveStationConfigToJson()
        {
            try
            {
                var recipeDir = _commonbus.CurrentRecipe.GetRecipePath();
                var configFile = Path.Combine(recipeDir, "db", "Ass_Data", "StationConfig.json");
                JObject allConfig;
                if (File.Exists(configFile))
                {
                    allConfig = JObject.Parse(File.ReadAllText(configFile));
                }
                else
                {
                    allConfig = new JObject();
                }

                // 构造当前界面配置对象
                var configObj = new JObject
                {
                    ["StationCount"] = StationCount,
                    ["SelectedStationName"] = SelectedStationName,
                    ["StationConfigs"] = JArray.FromObject(StationConfigs)
                };

                // 保存到对应字段
                if (!string.IsNullOrEmpty(ConfigKey))
                {
                    allConfig[ConfigKey] = configObj;
                    File.WriteAllText(configFile, allConfig.ToString(Formatting.Indented));
                }
            }
            catch (Exception ex)
            {
                // 可根据实际需求记录日志或提示
                throw;
            }
        }

        public void LoadStationConfigFromJson()
        {
            try
            {
                var recipeDir = _commonbus.CurrentRecipe.GetRecipePath();
                var configFile = Path.Combine(recipeDir, "db", "Ass_Data", "StationConfig.json");
                if (!File.Exists(configFile) || string.IsNullOrEmpty(ConfigKey))
                    return;

                var allConfig = JObject.Parse(File.ReadAllText(configFile));
                var configObj = allConfig[ConfigKey] as JObject;
                if (configObj == null)
                    return;

                StationCount = configObj["StationCount"]?.Value<int>() ?? StationCount;
                var configs = configObj["StationConfigs"] as JArray;
                if (configs != null)
                {
                    var stationConfigs = configs.ToObject<ObservableCollection<StationConfig>>();
                    StationConfigs = stationConfigs ?? new ObservableCollection<StationConfig>();
                }
                SelectedStationName = configObj["SelectedStationName"]?.Value<string>() ?? SelectedStationName;


            }
            catch (Exception ex)
            {
                // 可根据实际需求记录日志或提示
                throw;
            }
        }

        // 新增监听方法
        public void StationConfig_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(StationConfig.Name))
            {
                UpdateStationNames();
            }
        }

        #endregion

        #region chart 曲线

        /// <summary>
        /// 曲线数据
        /// </summary>
        private SeriesCollection _loadCellSeriesCollection;
        public SeriesCollection SeriesCollection
        {
            get { return _loadCellSeriesCollection; }
            set { SetProperty(ref _loadCellSeriesCollection, value); }
        }

        /// <summary>
        /// x轴标签
        /// </summary>
        private string[] _LoadCellLabels;
        public string[] LoadCellLabels
        {
            get { return _LoadCellLabels; }
            set { SetProperty(ref _LoadCellLabels, value); }
        }

        /// <summary>
        /// Y轴标签格式器
        /// </summary>
        private Func<double, string> _LoadCellFormatter;
        public Func<double, string> YFormatter
        {
            get { return _LoadCellFormatter; }
            set { SetProperty(ref _LoadCellFormatter, value); }
        }


        /// <summary>
        /// 绘制压力重复性曲线
        /// </summary>
        public void DrawPressureRepetitionChart()
        {
            // 1. 按"项次"分组
            var groups = ItemModels.OfType<AssTb>()
                .GroupBy(x => x.项次)
                .ToList();

            var seriesCollection = new LiveCharts.SeriesCollection();
            var colorList = new[]
            {
                System.Windows.Media.Color.FromRgb(33, 150, 243),
                System.Windows.Media.Color.FromRgb(76, 175, 80),
                System.Windows.Media.Color.FromRgb(255, 193, 7),
                System.Windows.Media.Color.FromRgb(244, 67, 54),
                System.Windows.Media.Color.FromRgb(156, 39, 176),
                System.Windows.Media.Color.FromRgb(0, 188, 212),
                System.Windows.Media.Color.FromRgb(205, 220, 57),
                System.Windows.Media.Color.FromRgb(121, 85, 72)
            };

            int colorIndex = 0;
            foreach (var group in groups)
            {
                // 2. 组内数据按"项序"排序
                var ordered = group.OrderBy(x => x.项序).ToList();
                var values = new LiveCharts.ChartValues<double>();
                foreach (var item in ordered)
                {
                    double v;
                    if (!double.TryParse(item.实测, out v))
                        v = 0;
                    values.Add(v);
                }

                // 3. 创建曲线
                var lineSeries = new LiveCharts.Wpf.LineSeries
                {
                    Title = group.Key,
                    Values = values,
                    Stroke = new System.Windows.Media.SolidColorBrush(colorList[colorIndex % colorList.Length]),
                    Fill = System.Windows.Media.Brushes.Transparent,
                    PointGeometry = LiveCharts.Wpf.DefaultGeometries.Circle,
                    PointGeometrySize = 1
                };
                seriesCollection.Add(lineSeries);
                colorIndex++;
            }

            // 4. 设置X轴标签
            //var firstGroup = groups.FirstOrDefault();
            //if (firstGroup != null)
            //{
            //    LoadCellLabels = firstGroup.OrderBy(x => x.项序).Select(x => x.项序.ToString()).ToArray();
            //}
            //else
            //{
            //    LoadCellLabels = Array.Empty<string>();
            //}

            // 5. 设置Y轴格式器
            YFormatter = value => value.ToString("F2");

            // 6. 绑定到视图
            SeriesCollection = seriesCollection;
        }

        // 新增：所有项次列表（下拉框数据源）
        private ObservableCollection<string> _availableItems;
        public ObservableCollection<string> AvailableItems
        {
            get { return _availableItems; }
            set { SetProperty(ref _availableItems, value); }
        }
        // 新增：选中的项次
        private string _selectedItem;
        public string SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                SetProperty(ref _selectedItem, value);
                // 选择改变时，刷新图表
                if (_currentPage == "PressureRepetition")
                {
                    IsLinear = "Collapsed";
                    UpdateChart();
                }
                else if (_currentPage == "CalibrationTable")
                {
                    IsLinear = "Visible";
                    UpdateChart2();
                }
                // “自动LoadCell”页面初始加载
                else if (_currentPage == null)
                {
                    IsLinear = "Visible";
                    UpdateChart2();
                }
            }
        }
        // 缓存所有分组数据，方便筛选
        private List<IGrouping<string, AssTb>> _cachedGroups;
        // 绘制全部曲线，或根据下拉菜单选择的项次绘制对应的单条曲线
        public void DrawPressureRepetitionChartOpt()
        {
            // 1. 按"项次"分组并缓存
            _cachedGroups = ItemModels.OfType<AssTb>()
                .GroupBy(x => x.项次)
                .ToList();

            // 2. 填充下拉框选项（"全部" + 所有项次）
            AvailableItems.Clear();
            AvailableItems.Add("全部");  // 默认显示所有曲线
            foreach (var group in _cachedGroups)
            {
                AvailableItems.Add(group.Key);
            }

            // 3. 默认选中"全部"
            SelectedItem = "全部";
        }

        // 根据选择更新图表
        private void UpdateChart()
        {
            if (_cachedGroups == null) return;

            var seriesCollection = new SeriesCollection();
            var colorList = new[]
            {
            System.Windows.Media.Color.FromRgb(33, 150, 243),
            System.Windows.Media.Color.FromRgb(76, 175, 80),
            System.Windows.Media.Color.FromRgb(255, 193, 7),
            System.Windows.Media.Color.FromRgb(244, 67, 54),
            System.Windows.Media.Color.FromRgb(156, 39, 176),
            System.Windows.Media.Color.FromRgb(0, 188, 212),
            System.Windows.Media.Color.FromRgb(205, 220, 57),
            System.Windows.Media.Color.FromRgb(121, 85, 72)
        };

            int colorIndex = 0;

            // 筛选要显示的分组
            var groupsToShow = SelectedItem == "全部"
                ? _cachedGroups
                : _cachedGroups.Where(g => g.Key == SelectedItem);

            foreach (var group in groupsToShow)
            {
                var ordered = group.OrderBy(x => x.项序).ToList();
                var values = new ChartValues<double>();

                foreach (var item in ordered)
                {
                    double v = double.TryParse(item.实测, out v) ? v : 0;
                    values.Add(v);
                }

                var lineSeries = new LineSeries
                {
                    Title = group.Key,
                    Values = values,
                    Stroke = new SolidColorBrush(colorList[colorIndex % colorList.Length]),
                    Fill = Brushes.Transparent,
                    PointGeometry = DefaultGeometries.Circle,
                    PointGeometrySize = 5  // 建议改大一点，单条曲线时更好看
                };

                seriesCollection.Add(lineSeries);
                colorIndex++;
            }

            SeriesCollection = seriesCollection;
        }
        // 新增压力拟合曲线的k和b约束值
        private string _isLinear;
        public string IsLinear
        {
            get { return _isLinear; }
            set { SetProperty(ref _isLinear, value); }
        }
        private string _kValue = "0.05";
        public string KValue
        {
            get { return _kValue; }
            set { SetProperty(ref _kValue, value); }
        }
        private string _bValue = "0.05";
        public string BValue
        {
            get { return _bValue; }
            set { SetProperty(ref _bValue, value); }
        }
        private string _resValue;
        public string ResValue
        {
            get { return _resValue; }
            set { SetProperty(ref _resValue, value); }
        }
        private string _resColor;
        public string ResColor
        {
            get { return _resColor; }
            set { SetProperty(ref _resColor, value); }
        }
        // 压力线性曲线
        public void DrawPressureLinearChartOpt()
        {
            _cachedGroups = ItemModels.OfType<AssTb>()
                .GroupBy(x => x.项次)
                .ToList();
            AvailableItems.Clear();
            AvailableItems.Add("全部");  // 默认显示所有曲线
            foreach (var group in _cachedGroups)
            {
                AvailableItems.Add(group.Key);
            }

            // 3. 默认选中"全部"
            SelectedItem = "全部";
        }
        private string _fitEquation;
        public string FitEquation
        {
            get => _fitEquation;
            set => SetProperty(ref _fitEquation, value);
        }
        /// <summary>
        /// 最小二乘法线性拟合 y = kx + b
        /// </summary>
        private (double k, double b) LinearFit(double[] x, double[] y)
        {
            int n = x.Length;
            double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;

            for (int i = 0; i < n; i++)
            {
                sumX += x[i];
                sumY += y[i];
                sumXY += x[i] * y[i];
                sumX2 += x[i] * x[i];
            }

            double denominator = n * sumX2 - sumX * sumX;
            if (Math.Abs(denominator) < 1e-10) return (0, 0);  // 防止除零

            double k = (n * sumXY - sumX * sumY) / denominator;
            double b = (sumY - k * sumX) / n;

            return (k, b);
        }
        private void UpdateChart2()
        {
            if (_cachedGroups == null) return;

            var seriesCollection = new SeriesCollection();
            var colorList = new[]
            {
            System.Windows.Media.Color.FromRgb(33, 150, 243),
            System.Windows.Media.Color.FromRgb(76, 175, 80),
            System.Windows.Media.Color.FromRgb(255, 193, 7),
            System.Windows.Media.Color.FromRgb(244, 67, 54),
            System.Windows.Media.Color.FromRgb(156, 39, 176),
            System.Windows.Media.Color.FromRgb(0, 188, 212),
            System.Windows.Media.Color.FromRgb(205, 220, 57),
            System.Windows.Media.Color.FromRgb(121, 85, 72)
        };

            int colorIndex = 0;

            // 筛选要显示的分组
            var groupsToShow = SelectedItem == "全部"
                ? _cachedGroups
                : _cachedGroups.Where(g => g.Key == SelectedItem);
            foreach (var group in groupsToShow)
            {
                // 1. 提取数据点
                var points = group.OrderBy(x => x.项序)
                    .Select(x => new
                    {
                        X = double.TryParse(x.实测, out var s) ? s : 0,
                        Y = double.TryParse(x.标准, out var m) ? m : 0
                    })
                    .Where(p => p.X != 0 || p.Y != 0)  // 过滤无效数据
                    .ToList();

                if (points.Count < 2) continue;

                // 2. 最小二乘法拟合 y = kx + b
                var fitResult = LinearFit(points.Select(p => p.X).ToArray(),
                                          points.Select(p => p.Y).ToArray());
                double k = fitResult.k;
                double b = fitResult.b;

                // 3. 生成拟合曲线上的点（用很多点让曲线看起来平滑）
                var minX = points.Min(p => p.X);
                var maxX = points.Max(p => p.X);
                var fittedPoints = new ChartValues<ObservablePoint>();

                // 生成100个点让直线看起来是连续的
                for (int i = 0; i <= 100; i++)
                {
                    double x = minX + (maxX - minX) * i / 100.0;
                    double y = k * x + b;
                    fittedPoints.Add(new ObservablePoint(x, y));
                }

                // 4. 添加拟合曲线（直线）
                var fitSeries = new LineSeries
                {
                    Title = $"{group.Key} (拟合: y={k:F4}x+{b:F4})",  // 显示在图例中
                    Values = fittedPoints,
                    Stroke = new SolidColorBrush(colorList[colorIndex % colorList.Length]),
                    StrokeThickness = 2,
                    Fill = Brushes.Transparent,
                    PointGeometry = null,  // 不显示点，只显示线
                    LineSmoothness = 0     // 直线
                };
                seriesCollection.Add(fitSeries);

                // 5. 添加原始散点（显示实际测量点）
                var scatterValues = new ChartValues<ObservablePoint>();
                foreach (var p in points)
                {
                    scatterValues.Add(new ObservablePoint(p.X, p.Y));
                }

                var scatterSeries = new ScatterSeries
                {
                    Title = $"{group.Key} (实测点)",
                    Values = scatterValues,
                    Fill = new SolidColorBrush(colorList[colorIndex % colorList.Length]),
                    Stroke = new SolidColorBrush(colorList[colorIndex % colorList.Length]),
                    MinPointShapeDiameter = 8,
                    MaxPointShapeDiameter = 8
                };
                seriesCollection.Add(scatterSeries);
                colorIndex++;
                // 6. 更新 UI 显示 k 和 b（绑定到 TextBlock）
                FitEquation = $"y = {k:F4}x + {b:F4}";
                // 根据约束值，给出压力线性的测量结果
                if (double.TryParse(KValue, out double KValued) && double.TryParse(BValue, out double BValued))
                {
                    if (Math.Abs(k - KValued) <= 1 && Math.Abs(b) <= BValued)
                    {
                        ResColor = "LightGreen";
                        ResValue = "OK";
                    }
                    else
                    {
                        ResColor = "LightPink";
                        ResValue = "NG";
                    }
                }
                else
                {
                    ResColor = "LightPink";
                    ResValue = "K/B非法";
                }
            }
            SeriesCollection = seriesCollection;
        }

        #endregion

        public BaseAss(IRepository repository, IRegionManager regionManager, ICommonBus commonhus, CSVHelper csvHelper, FlowBus flowBus, IDialogService dialogService)
        {

            _syncContext = SynchronizationContext.Current ?? new SynchronizationContext();
            TaskStarted += (s, msg) => LogStatus(msg);
            TaskCompleted += (s, msg) => LogStatus(msg);
            TaskCanceled += (s, msg) => LogStatus(msg);
            TaskFailed += (s, msg) => LogStatus(msg);


            PageUpdatedCommand = new DelegateCommand<FunctionEventArgs<int>>(PageUpdated);
            SelectedCommand = new DelegateCommand<CommonPageModel>(Selected);
            _reporitory = repository;
            _regionManager = regionManager;
            _commonbus = commonhus;
            _flowBus = flowBus;
            _dialogService = dialogService;
            ItemModels = new ObservableCollection<object>();
            _csvHelper = csvHelper;

            var recipeDir = _commonbus.CurrentRecipe?.GetRecipePath() ?? "D:\\Luster\\DigitalSetUp\\";
            _persistenceService = new DataValidationPersistenceService();
            _persistenceService?.SetConfigFilePath(recipeDir);

            // 通过服务定位器获取 PageEnableSettingsService 单例
            _pageEnableSettingsService = DigitalSetupServiceLocator.PageEnableSettingsService;

            // 通过服务定位器获取 FloatingInfoService 单例
            _floatingInfoService = DigitalSetupServiceLocator.FloatingInfoService;

            // 将 SaveStationConfigCommand 的初始化方式改为使用委托调用基类的受保护方法
            SaveStationConfigCommand = new DelegateCommand(SaveStationConfigHandler);


            // 监听StationConfigs集合变化
            StationConfigs.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (StationConfig sc in e.NewItems)
                    {
                        sc.PropertyChanged += StationConfig_PropertyChanged;
                    }
                }
                if (e.OldItems != null)
                {
                    foreach (StationConfig sc in e.OldItems)
                    {
                        sc.PropertyChanged -= StationConfig_PropertyChanged;
                    }
                }
            };
            // 初始化时为已有元素添加监听
            foreach (var sc in StationConfigs)
            {
                sc.PropertyChanged += StationConfig_PropertyChanged;
            }

            AvailableItems = new ObservableCollection<string>();

            // 注册到全局保存命令，主窗口保存时会自动保存当前页面数据
            _globalSaveCommand = new DelegateCommand(() => SaveGridItems(ItemModels));
            GlobalCommands.SaveCommand.RegisterCommand(_globalSaveCommand);

            // 初始化设置点检确认消息命令
            SetCheckMessageCommand = new DelegateCommand<CommonPageModel>(OnSetCheckMessage);
        }

        protected void LogStatus(string msg)
        {
            LogMsg = $"[{DateTime.Now:T}] {msg}";
        }

        #region 页面配置持久化方法

        /// <summary>
        /// 加载页面点检确认消息配置
        /// </summary>
        protected void LoadCheckConfirmMessages()
        {
            if (_persistenceService == null || Pages == null) return;

            try
            {
                var data = _persistenceService.LoadMessageConfig();
                if (data?.PageConfigs != null && data.PageConfigs.Count > 0)
                {
                    foreach (var pageConfig in data.PageConfigs)
                    {
                        var page = Pages.FirstOrDefault(p => p.Name == pageConfig.PageName);
                        if (page != null)
                        {
                            page.CheckConfirmMessage = pageConfig.CheckConfirmMessage;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载页面配置失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 保存页面点检确认消息配置
        /// </summary>
        protected void SaveCheckConfirmMessages()
        {
            if (_persistenceService == null || Pages == null) return;

            try
            {
                // 加载现有配置，避免覆盖其他界面的配置
                var data = _persistenceService.LoadMessageConfig();

                foreach (var page in Pages)
                {
                    // 查找是否已存在该页面的配置
                    var existingConfig = data.PageConfigs.FirstOrDefault(c => c.PageName == page.Name);
                    if (existingConfig != null)
                    {
                        // 更新现有配置
                        existingConfig.CheckConfirmMessage = page.CheckConfirmMessage;
                    }
                    else
                    {
                        // 添加新配置
                        data.PageConfigs.Add(new PageConfig
                        {
                            PageName = page.Name,
                            CheckConfirmMessage = page.CheckConfirmMessage
                        });
                    }
                }
                _persistenceService.SaveMessageConfig(data);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存页面配置失败: {ex.Message}");
            }
        }

        #endregion


        /// <summary>
        /// 保存表格数据
        /// </summary>
        /// <param name="collection"></param>
        [Obsolete("_reporitory弃用")]
        protected void SaveGridItems1(ObservableCollection<object> collection)
        {
            if (collection == null || collection.Count == 0) return;

            var itemType = collection[0].GetType();

            // 获取 Cast<T> 方法
            MethodInfo castMethod = typeof(Enumerable).GetMethod(
                "Cast",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(IEnumerable) },
                null);

            if (castMethod == null)
            {
                throw new InvalidOperationException("找不到 Enumerable.Cast 方法");
            }

            MethodInfo genericCastMethod = castMethod.MakeGenericMethod(itemType);

            object convertedEnumerable = genericCastMethod.Invoke(null, new object[] { collection });

            // 获取 repository 类型并调用 OverwriteAll<T>
            var method = _reporitory.GetType().GetMethod("InsertOrUpdateNew")
               ?.MakeGenericMethod(itemType);

            if (method == null)
            {
                throw new InvalidOperationException("找不到 InsertOrUpdate 泛型方法");
            }

            method.Invoke(_reporitory, new object[] { convertedEnumerable });
        }

        protected void SaveGridItems(ObservableCollection<object> collection)
        {
            if (collection == null)// || collection.Count == 0) return;
                return;
            //var itemType = collection[0].GetType();
            if (SelectedReportPage == null)
            {
                return;
            }
            var itemType = SelectedReportPage.ViewType;
            if (itemType == null) return;
            // 获取 Cast<T> 方法
            MethodInfo castMethod = typeof(Enumerable).GetMethod(
                "Cast",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(IEnumerable) },
                null);

            if (castMethod == null)
            {
                throw new InvalidOperationException("找不到 Enumerable.Cast 方法");
            }

            MethodInfo genericCastMethod = castMethod.MakeGenericMethod(itemType);

            object convertedEnumerable = genericCastMethod.Invoke(null, new object[] { collection });

            // 直接调用CSVHelper实例方法
            var method = typeof(CSVHelper).GetMethod("InsertOrUpdateNew")?.MakeGenericMethod(itemType);

            if (method == null)
            {
                throw new InvalidOperationException("找不到 InsertOrUpdateNew 泛型方法");
            }

            method.Invoke(_csvHelper, new object[] { convertedEnumerable });
        }

        protected void InitModels()
        {
            OnEnd();
            // 应用子页面启用设置
            ApplySubPageSettings();
            // 初始化数据
            PageUpdated(new FunctionEventArgs<int>(1));
            PageIndex = 1;
        }

        /// <summary>
        /// 应用子页面的启用设置（加载全部配置并应用到所有子页面）
        /// </summary>
        protected void ApplySubPageSettings()
        {
            if (_pageEnableSettingsService != null)
            {
                _pageEnableSettingsService.ApplySubPageSettings();
            }
        }
        // 保存当前页面标识
        private string _currentPage;
        protected void Selected(CommonPageModel obj)
        {
            if (obj != null)
            {
                SetSelected(obj.Name);
                _currentPage = obj.Name;
                //查询数据库，更新表格
                InitModels();
                // 更新曲线
                if (obj.Name == "PressureRepetition")
                {
                    DrawPressureRepetitionChartOpt();
                }
                // 压力线性，横坐标为标准值，纵坐标为实测值
                else if (obj.Name == "CalibrationTable")
                {
                    DrawPressureLinearChartOpt();
                }

                //// 显示子页面对应的浮动信息窗口
                //ShowFloatingInfoForSubPage(obj.Name);
            }
        }

        /// <summary>
        /// 显示子页面对应的浮动信息窗口
        /// </summary>
        /// <param name="subPageName">子页面名称</param>
        protected void ShowFloatingInfoForSubPage(string subPageName)
        {
            try
            {
                // 构建子页面的PageId，格式为：父页面Region_子页面Name
                // 例如：IOinspectionContent_Digital_In_Single
                // 如果_parentRegionName为空，则使用_currentPage作为备用
                string parentRegion = !string.IsNullOrEmpty(_parentRegionName) ? _parentRegionName : ConfigKey;
                string pageId = $"{parentRegion}_{subPageName}";
                
                // 先隐藏所有浮动窗口
                _floatingInfoService?.HideAllFloatingInfo();
                
                // 显示当前子页面的浮动窗口
                _floatingInfoService?.ShowFloatingInfo(pageId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"显示子页面浮动信息窗口失败: {ex.Message}");
            }
        }


        /// <summary>
        /// 页面内容更新
        /// </summary>
        [Obsolete("_reporitory弃用")]
        protected void PageUpdated1(FunctionEventArgs<int> obj)
        {

            try
            {
                var index = obj.Info;
                long count = 0;
                IEnumerable<object> infos = Enumerable.Empty<object>(); ;


                // 获取当前选中的类型
                Type targetType = SelectedReportPage.ViewType;

                MethodInfo methodInfo = _reporitory.GetType().GetMethod("GetAllDataNew");

                // 获取方法：GetAllData()
                MethodInfo getAllDataMethod = methodInfo.MakeGenericMethod(targetType);

                //// 准备参数
                object[] parameters = new object[]
                {
                    index,                    // 页码
                    PerPageCount,             // 每页数量
                    default(long),            // 总数（out 参数需初始化）
                };

                // 调用方法获取数据
                object result = getAllDataMethod.Invoke(_reporitory, parameters);

                count = (long)parameters[2];
                // result 是 object 类型，可以转换为 IEnumerable 或进一步处理
                infos = ((IEnumerable)result).Cast<object>().ToList();

                if (infos != null && infos.Any())
                {
                    ItemModels = new ObservableCollection<object>(infos);
                    if (count > 0)
                    {
                        PageCount = (int)(count % PerPageCount == 0 ? count / PerPageCount : count / PerPageCount + 1);
                    }
                    else
                    {
                        PageCount = 1;
                    }
                }
                else
                {
                    ItemModels = new ObservableCollection<object>();
                }
            }
            catch (Exception)
            {
                ItemModels = new ObservableCollection<object>();
            }
        }

        /* ---------- 两个小工具 ---------- */
        static string GetStringProp(object obj, string propName)
        {
            return obj.GetType().GetProperty(propName)?.GetValue(obj)?.ToString() ?? "";
        }

        static DateTime? GetDateTimeProp(object obj, string propName)
        {
            var val = obj.GetType().GetProperty(propName)?.GetValue(obj);
            if (val is DateTime dt) return dt;
            if (DateTime.TryParse(val?.ToString(), out var tmp)) return tmp;
            return null;
        }
        public virtual void PageUpdated(FunctionEventArgs<int> obj)
        {
            try
            {
                var index = obj.Info;
                long count = 0;
                IEnumerable<object> infos = Enumerable.Empty<object>();
                if (SelectedReportPage == null) return;
                // 获取当前选中的类型
                Type targetType = SelectedReportPage.ViewType;

                // 反射调用CSVHelper实例方法
                MethodInfo methodInfo = typeof(CSVHelper).GetMethod("GetAllDataNew");
                MethodInfo getAllDataMethod = methodInfo.MakeGenericMethod(targetType);

                object[] parameters = new object[] { index, PerPageCount, null };
                object result = getAllDataMethod.Invoke(_csvHelper, parameters);

                count = (long)parameters[2];
                infos = ((IEnumerable)result).Cast<object>().ToList();

                /* 1. 显式指定 GroupBy 的泛型实参：<object, string> 
                      把“项序+项次”直接拼成 string 当 key，省去匿名对象。 */
                var filtered = infos
                    .GroupBy(
                        keySelector: (object row) => GetStringProp(row, "项序") + "|" + GetStringProp(row, "项次"),
                        comparer: StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.OrderByDescending(row =>
                    {
                        var dt = GetDateTimeProp(row, "完成时间");
                        return dt ?? DateTime.MinValue;
                    })
                              .First())
                    .ToList();

                if (filtered != null && filtered.Any())
                {
                    ItemModels = new ObservableCollection<object>(filtered);
                    //ItemModels = new ObservableCollection<object>(infos);
                    if (count > 0)
                    {
                        PageCount = (int)(count % PerPageCount == 0 ? count / PerPageCount : count / PerPageCount + 1);
                    }
                    else
                    {
                        PageCount = 1;
                    }
                }
                else
                {
                    ItemModels = new ObservableCollection<object>();
                }
            }
            catch (Exception)
            {
                ItemModels = new ObservableCollection<object>();
            }
        }

        protected void SetSelected(string name)
        {
            foreach (var item in Pages)
            {
                if (item.Name != name)
                {
                    item.IsSelected = false;
                }
                else
                {
                    item.IsSelected = true;
                }
            }
        }

        /// <summary>
        /// 一键点检
        /// </summary>
        public virtual async void OnOneKeyCheck(object obj)
        {
            //耗时的点检操作
            //throw new FriendlyException("未实现一键点检功能");

        }

        public virtual async Task OnOneKeyCheckAsync(object obj)
        {
            // 如果当前页面设置了确认消息，先显示确认弹窗
            if (!string.IsNullOrEmpty(SelectedReportPage?.CheckConfirmMessage))
            {
                var result = await ShowConfirmAsync(SelectedReportPage.CheckConfirmMessage);
                if (result != ButtonResult.OK)
                {
                    return;
                }
            }

            ExecuteOneKeyCheck(obj);
        }

        /// <summary>
        /// 执行一键点检的实际逻辑
        /// </summary>
        private void ExecuteOneKeyCheck(object obj)
        {
            var collection = obj as ObservableCollection<object>;
            if (collection == null)
                return;
            if (collection.Count > 0)
            {
                //根据collection类型备份配方现有点检数据

                //删除"D:\\Motion\\AssData.csv"
                if (System.IO.File.Exists(csvPath))
                {
                    try
                    {
                        System.IO.File.Delete(csvPath);
                    }
                    catch (Exception ex)
                    {
                        _commonbus.OnLog(new LogInfo() { LogType = LogType.Error, LogMessage = $"删除AssData.csv失败: {ex.Message}" });
                        return;
                    }
                }
                return;
            }
            else
            {
                return;
            }
            //耗时的点检操作

            throw new FriendlyException("未实现一键点检功能");

        }

        /// <summary>
        /// 设置点检确认消息
        /// </summary>
        protected virtual void OnSetCheckMessage(CommonPageModel page)
        {
            if (page == null) return;

            //if (_dialogService == null)
            //{
            //    ShowMessage("对话框服务未初始化");
            //    return;
            //}

            // 显示子页面对应的浮动信息窗口
            ShowFloatingInfoForSubPage(page.Name);

            //_dialogService.ShowInfoInput($"设置 [{page.Name}] 的点检确认消息:", page.CheckConfirmMessage ?? string.Empty, r =>
            //{
            //    if (r.Result == ButtonResult.OK)
            //    {
            //        var text = r.Parameters.GetValue<string>("Text");
            //        page.CheckConfirmMessage = text;
            //        // 保存配置到文件
            //        SaveCheckConfirmMessages();
            //    }
            //});
        }

        /// <summary>
        /// 异步显示确认对话框
        /// </summary>
        protected Task<ButtonResult> ShowConfirmAsync(string message)
        {
            if (_dialogService == null)
            {
                return Task.FromResult(ButtonResult.None);
            }

            var tcs = new TaskCompletionSource<ButtonResult>();
            _dialogService.ShowConfirm(message, r => tcs.SetResult(r.Result), false);
            return tcs.Task;
        }
        /// <summary>
        /// 终止点检
        /// </summary>

        public virtual async void OnEnd()
        {
            //终止耗时的点检操作
            Stop();
            await WaitForCompletionAsync();
        }

        public void StartAsync()
        {
            if (_task != null && !_task.IsCompleted)
            {
                ShowMessage("任务已在运行中");
                return;
            }

            _cts = new CancellationTokenSource();

            // 触发任务开始事件（显示弹窗）
            //OnTaskStarted("任务开始执行...");

            _task = Task.Run(() =>
            {
                try
                {
                    ExecuteAsync(_cts.Token).Wait(_cts.Token);
                    OnTaskCompleted("任务成功完成！");
                }
                catch (OperationCanceledException)
                {
                    OnTaskCanceled("任务已被取消");
                }
                catch (Exception ex)
                {
                    OnTaskFailed($"任务失败: {ex.Message}");
                }
            }, _cts.Token);
        }

        public void Stop()
        {
            if (_task == null || _task.IsCompleted) return;

            //OnTaskCanceled("正在取消任务...");
            _cts?.Cancel();
        }

        public async Task WaitForCompletionAsync()
        {
            if (_task == null) return;

            try
            {
                await _task;
            }
            catch (OperationCanceledException)
            {
                // 取消操作是正常行为
            }
        }

        // 子类实现的具体异步逻辑
        protected virtual Task ExecuteAsync(CancellationToken token)
        {
            throw new NotImplementedException();
        }

        // 提供弹窗显示方法
        protected void ShowMessage(string message)
        {
            //使用同步上下文确保在UI线程执行
            _syncContext.Post(_ =>
            {
                MessageBox.Show(message, "任务状态");
            }, null);
        }

        // 事件触发方法（带同步上下文处理）
        protected virtual void OnTaskStarted(string message)
        {
            _syncContext.Post(_ =>
            {
                TaskStarted?.Invoke(this, message);
                ShowMessage(message);
            }, null);
        }

        protected virtual void OnTaskCompleted(string message)
        {
            _syncContext.Post(_ =>
            {
                TaskCompleted?.Invoke(this, message);
                ShowMessage(message);
            }, null);
        }

        protected virtual void OnTaskCanceled(string message)
        {
            _syncContext.Post(_ =>
            {
                TaskCanceled?.Invoke(this, message);
                ShowMessage(message);
            }, null);
        }

        protected virtual void OnTaskFailed(string message)
        {
            _syncContext.Post(_ =>
            {
                TaskFailed?.Invoke(this, message);
                ShowMessage(message);
            }, null);
        }

        public void Dispose()
        {
            // 取消注册全局保存命令
            if (_globalSaveCommand != null)
            {
                GlobalCommands.SaveCommand.UnregisterCommand(_globalSaveCommand);
            }

            _cts?.Cancel();
            _cts?.Dispose();
            _task?.Dispose();
            GC.SuppressFinalize(this);
        }


        #region  处理表格数据
        /// <summary>
        /// 读取"D:\\Motion\\AssData.csv"中的数据
        /// </summary>
        /// <summary>
        /// 通用CSV读取方法，兼容AssTbSuctionNozzle、AssTbCalibrationTable、AssTbPressureRepetition等类型
        /// 自动根据SelectedReportPage.ViewType读取D盘文件并更新到UI
        /// </summary>
        public virtual void UpdateItemsFromCsv(out bool csvReadSuccess)
        {
            csvReadSuccess = false;
            try
            {
                if (!System.IO.File.Exists(csvPath))
                {
                    _commonbus.OnLog(new LogInfo() { LogType = LogType.Error, LogMessage = $"文件未找到: {csvPath}" });
                    //ItemModels?.Clear();
                    return;
                }

                var lines = System.IO.File.ReadAllLines(csvPath, Encoding.Default);
                if (lines.Length < 2)
                {
                    // CSV文件为空或只有标题行时，清空界面数据
                    ItemModels?.Clear();
                    csvReadSuccess = true; // CSV读取成功，只是没有数据
                    _commonbus.OnLog(new LogInfo() { LogType = LogType.Info, LogMessage = "CSV文件无数据，已清空列表" });
                    return;
                }

                // 1. 自动识别类型
                Type targetType = null;
                if (ItemModels != null && ItemModels.Count > 0)
                    targetType = ItemModels[0].GetType();
                else
                    targetType = SelectedReportPage?.ViewType;
                if (targetType == null) return;

                var headers = lines[0].Split(',');
                int xiangciIndex = Array.IndexOf(headers, "项次");
                if (xiangciIndex < 0)
                {
                    _commonbus.OnLog(new LogInfo() { LogType = LogType.Error, LogMessage = $"未找到“项次”字段" });
                    return;
                }

                // 2. 读取CSV为对象集合
                var csvXiangciSet = new HashSet<string>();
                var newObjects = new List<object>();
                for (int i = 1; i < lines.Length; i++)
                {
                    var cols = lines[i].Split(',');
                    if (cols.Length > 0)
                    {
                        cols[0] = i.ToString(); // 仅将第一列设为i.ToString()
                    }
                    if (cols.Length <= xiangciIndex) continue;

                    string xiangciValue = cols[xiangciIndex];
                    if (string.IsNullOrEmpty(xiangciValue))
                    {
                        continue;
                    }
                    csvXiangciSet.Add(xiangciValue);

                    var obj = Activator.CreateInstance(targetType);
                    for (int j = 0; j < Math.Min(headers.Length, cols.Length); j++)
                    {
                        var prop = targetType.GetProperty(headers[j]);
                        if (prop != null && prop.CanWrite)
                        {
                            try
                            {
                                object value = Convert.ChangeType(cols[j], prop.PropertyType);
                                prop.SetValue(obj, value);
                            }
                            catch
                            {
                                prop.SetValue(obj, cols[j]);
                            }
                        }
                    }
                    // 若有完成时间字段，自动赋值
                    var timeProp = targetType.GetProperty("完成时间");
                    if (timeProp != null && timeProp.CanWrite)
                    {
                        timeProp.SetValue(obj, DateTime.Now);
                    }
                    newObjects.Add(obj);
                }

                // 3. 更新或添加到ItemModels
                // 2. 移除ItemModels中项次与CSV重复的数据
                var toRemove = new List<object>();
                foreach (var item in ItemModels)
                {
                    var prop = item.GetType().GetProperty("项次");
                    if (prop != null)
                    {
                        var value = prop.GetValue(item)?.ToString();
                        if (value != null && csvXiangciSet.Contains(value))
                        {
                            toRemove.Add(item);
                        }
                    }
                }
                foreach (var item in toRemove)
                {
                    ItemModels.Remove(item);
                }

                // 3. 添加CSV新数据
                foreach (var obj in newObjects)
                {
                    ItemModels.Add(obj);
                }
                // 在需要判断并删除的地方插入如下代码：
                if (ItemModels != null && ItemModels.Count > 0)
                {
                    var firstItem = ItemModels[0];
                    var prop = firstItem.GetType().GetProperty("项次");
                    if (prop != null)
                    {
                        var value = prop.GetValue(firstItem)?.ToString();
                        if (string.IsNullOrEmpty(value))
                        {
                            ItemModels.RemoveAt(0);
                        }
                    }
                }
                //// 4. 按项次排序，并重新赋值项序
                //var sorted = ItemModels
                //    .OrderBy(item =>
                //    {
                //        var prop = item.GetType().GetProperty("项次");
                //        return prop != null ? prop.GetValue(item)?.ToString() : "";
                //    })
                //    .ToList();

                //for (int i = 0; i < sorted.Count; i++)
                //{
                //    var xiangxuProp = sorted[i].GetType().GetProperty("项序");
                //    if (xiangxuProp != null && xiangxuProp.CanWrite)
                //    {
                //        xiangxuProp.SetValue(sorted[i], i + 1);
                //    }
                //}

                //// 用排序后的集合替换原ItemModels
                //ItemModels.Clear();
                //foreach (var item in sorted)
                //{
                //    ItemModels.Add(item);
                //}
                csvReadSuccess = true; // CSV读取成功
            }
            catch (Exception ex)
            {
                _commonbus.OnLog(new LogInfo() { LogType = LogType.Error, LogMessage = $"读取AssData.csv失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 无参数版本，保持向后兼容
        /// </summary>
        public virtual void UpdateItemsFromCsv()
        {
            UpdateItemsFromCsv(out _);
        }


        [Obsolete]
        public virtual void UpdateItemsFromCsv2()
        {
            try
            {
                if (!System.IO.File.Exists(csvPath))
                {
                    _commonbus.OnLog(new LogInfo() { LogType = LogType.Error, LogMessage = $"文件未找到: {csvPath}" });
                    return;
                }

                var lines = System.IO.File.ReadAllLines(csvPath, Encoding.Default);
                if (lines.Length < 2) return; // 没有数据

                Type targetType = SelectedReportPage?.ViewType;
                if (targetType == null) return;

                var headers = lines[0].Split(',');
                int xiangciIndex = Array.IndexOf(headers, "项次");
                if (xiangciIndex < 0)
                {
                    _commonbus.OnLog(new LogInfo() { LogType = LogType.Error, LogMessage = $"未找到“项次”字段" });
                    return;
                }

                // 1. 收集CSV中的项次和新对象
                var csvXiangciSet = new HashSet<string>();
                var newObjects = new List<object>();
                for (int i = 1; i < lines.Length; i++)
                {
                    var cols = lines[i].Split(',');
                    if (cols.Length > 0)
                    {
                        cols[0] = i.ToString(); // 仅将第一列设为i.ToString()
                    }
                    if (cols.Length <= xiangciIndex) continue;
                    string xiangciValue = cols[xiangciIndex];
                    csvXiangciSet.Add(xiangciValue);

                    var obj = Activator.CreateInstance(targetType);
                    for (int j = 0; j < Math.Min(headers.Length, cols.Length); j++)
                    {
                        var prop = targetType.GetProperty(headers[j]);
                        if (prop != null && prop.CanWrite)
                        {
                            try
                            {
                                object value = Convert.ChangeType(cols[j], prop.PropertyType);
                                prop.SetValue(obj, value);
                            }
                            catch
                            {
                                prop.SetValue(obj, cols[j]);
                            }
                        }
                    }
                    // 若有完成时间字段，自动赋值
                    var timeProp = targetType.GetProperty("完成时间");
                    if (timeProp != null && timeProp.CanWrite)
                    {
                        timeProp.SetValue(obj, DateTime.Now);
                    }
                    newObjects.Add(obj);
                }

                // 2. 移除ItemModels中项次与CSV重复的数据
                var toRemove = new List<object>();
                foreach (var item in ItemModels)
                {
                    var prop = item.GetType().GetProperty("项次");
                    if (prop != null)
                    {
                        var value = prop.GetValue(item)?.ToString();
                        if (value != null && csvXiangciSet.Contains(value))
                        {
                            toRemove.Add(item);
                        }
                    }
                }
                foreach (var item in toRemove)
                {
                    ItemModels.Remove(item);
                }

                // 3. 添加CSV新数据
                foreach (var obj in newObjects)
                {
                    ItemModels.Add(obj);
                }
            }
            catch (Exception ex)
            {
                _commonbus.OnLog(new LogInfo() { LogType = LogType.Error, LogMessage = $"读取AssData.csv失败: {ex.Message}" });
            }
        }
        [Obsolete]
        public virtual void UpdateItemsFromCsv1()
        {
            try
            {
                if (!System.IO.File.Exists(csvPath))
                {
                    _commonbus.OnLog(new LogInfo() { LogType = LogType.Error, LogMessage = $"文件未找到: {csvPath}" });
                    return;
                }

                var lines = System.IO.File.ReadAllLines(csvPath, Encoding.Default);
                if (lines.Length < 2) return; // 没有数据

                Type targetType = SelectedReportPage?.ViewType;
                if (targetType == null) return;

                ItemModels.Clear();

                // 获取属性名列表（假设第一行为表头，属性名与类型属性一致）
                var headers = lines[0].Split(',');
                for (int i = 1; i < lines.Length; i++)
                {
                    var cols = lines[i].Split(',');
                    if (cols.Length > 0)
                    {
                        cols[0] = i.ToString(); // 仅将第一列设为i.ToString()
                    }
                    var obj = Activator.CreateInstance(targetType);
                    for (int j = 0; j < Math.Min(headers.Length, cols.Length); j++)
                    {
                        var prop = targetType.GetProperty(headers[j]);
                        if (prop != null && prop.CanWrite)
                        {
                            try
                            {
                                object value = Convert.ChangeType(cols[j], prop.PropertyType);
                                prop.SetValue(obj, value);
                            }
                            catch
                            {
                                prop.SetValue(obj, cols[j]);
                            }
                        }
                    }
                    // 若有完成时间字段，自动赋值
                    var timeProp = targetType.GetProperty("完成时间");
                    if (timeProp != null && timeProp.CanWrite)
                    {
                        timeProp.SetValue(obj, DateTime.Now);
                    }
                    ItemModels.Add(obj);
                }
            }
            catch (Exception ex)
            {
                _commonbus.OnLog(new LogInfo() { LogType = LogType.Error, LogMessage = $"读取AssData.csv失败: {ex.Message}" });
            }
        }
        #endregion

    }
}
