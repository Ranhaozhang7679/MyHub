#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ProductReportContentVM
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.ReportUI.ViewModel
* 文 件 名:       ProductReportContentVM.cs
* 创建时间:       2022/10/17 16:20:32
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      815f351e-4518-4455-ab21-4d99e2356251
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/10/17 16:20:32
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using HandyControl.Data;
using LiveCharts;
using LiveCharts.Wpf;
using Luster.Common.DataAccess.Repositories;
using Luster.Common.DataAccess.Tables;
using Luster.Common.DataStruct;
using Luster.Common.Tools;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.CommonUI.Models;
using Luster.Motion.ReportUI.Model;
using Luster.Motion.TaskFlow.Engine;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using static Luster.Common.Tools.SharedMemory.CircularBuffer;

namespace Luster.Motion.ReportUI.ViewModel
{
    public class ProductReportContentVM : ReportBaseVM
    {
        //本地画笔
        private SolidColorBrush GreenBrush = new SolidColorBrush(Color.FromRgb(47, 194, 90));
        private SolidColorBrush OrangeBrush = new SolidColorBrush(Color.FromRgb(245, 127, 101));

        // Cancel previous query to avoid piling up background work
        private CancellationTokenSource _queryCts;

        private List<AnalyzeModel> _analyzeModels = new List<AnalyzeModel>();

        private List<AnalyzeModel> _analyzeWeekModels = new List<AnalyzeModel>();


        /// <summary>
        /// 良率标识
        /// </summary>
        private bool _isPassedRate;
        public bool IsPassedRate
        {
            get { return _isPassedRate; }
            set
            {
                SetProperty(ref _isPassedRate, value);
            }
        }

        #region livechart相关
        /// <summary>
        /// 良率表系列
        /// </summary>
        private SeriesCollection _rateSeriesCollection;
        public SeriesCollection RateSeriesCollection
        {
            get => _rateSeriesCollection;
            set { SetProperty(ref _rateSeriesCollection, value); }
        }

        /// <summary>
        /// 良率表X轴标签
        /// </summary>
        private string[] _rateLables;
        public string[] RateLabels
        {
            get { return _rateLables; }
            set { SetProperty(ref _rateLables, value); }
        }

        /// <summary>
        /// IPhone良率标签
        /// </summary>
        private string[] _iphonerateLables;
        public string[] IPhoneRateLabels
        {
            get { return _iphonerateLables; }
            set { SetProperty(ref _iphonerateLables, value); }
        }

        /// <summary>
        /// Y轴标签格式器
        /// </summary>
        private Func<double, string> _rateFormatter;
        public Func<double, string> RateFormatter
        {
            get { return _rateFormatter; }
            set { SetProperty(ref _rateFormatter, value); }
        }

        /// <summary>
        /// 产能表系列
        /// </summary>
        private SeriesCollection _capacitySeriesCollection;
        public SeriesCollection CapacitySeriesCollection
        {
            get { return _capacitySeriesCollection; }
            set { SetProperty(ref _capacitySeriesCollection, value); }
        }

        /// <summary>
        /// iphone产能表系列
        /// </summary>
        private SeriesCollection _iphoneCapacitySeries;
        public SeriesCollection IphoneCapacitySeries
        {
            get { return _iphoneCapacitySeries; }
            set { SetProperty(ref _iphoneCapacitySeries, value); }
        }

        /// <summary>
        /// 班别产能表系列
        /// </summary>
        private SeriesCollection _classCapacitySeries;
        public SeriesCollection ClassCapacitySeries
        {
            get { return _classCapacitySeries; }
            set { SetProperty(ref _classCapacitySeries, value); }
        }

        /// <summary>
        /// 产能表x轴标签
        /// </summary>
        private string[] _capacityLabels;
        public string[] CapacityLabels
        {
            get { return _capacityLabels; }
            set { SetProperty(ref _capacityLabels, value); }
        }

        /// <summary>
        /// chart宽度
        /// </summary>
        private double _chartWidth=440;
        public double ChartWidth
        {
            get { return _chartWidth; }
            set { SetProperty(ref _chartWidth, value); }
        }

        /// <summary>
        /// 产能Y轴标签格式器
        /// </summary>
        private Func<double, string> _capacityFormatter;
        public Func<double, string> CapacityFormatter
        {
            get { return _capacityFormatter; }
            set { SetProperty(ref _capacityFormatter, value); }
        }
        #endregion

        /// <summary>
        /// 报表数据
        /// </summary>
        private ObservableCollection<ReportItem> _reportItemVMs;
        public ObservableCollection<ReportItem> ReportItemVMs
        {
            get { return _reportItemVMs; }
            set { SetProperty(ref _reportItemVMs, value); }
        }

        public DelegateCommand ChangeSeriesCommand { get; set; }
        public DelegateCommand<string> DateChangeCommand { get; set; }

        /// <summary>
        /// 输出数据名称
        /// </summary>
        public List<string> DataHeaders { get; set; }

        // UI线程
        private Dispatcher _dispatcher;

        public override string ReportName => "ProductStatistics";

        private IMotionController _mController;
        public ProductReportContentVM() : base()
        {

        }

        public ProductReportContentVM(IRepository reporitory, IMotionController motionController, Dispatcher dispatcher) : base(reporitory, motionController)
        {
            _mController = motionController;
            _dispatcher = dispatcher;

            // Keep initialization lightweight (no Dispatcher.Invoke heavy work)
            Init();

            // Initial load async
            Query();
        }

        protected override void Init()
        {
            base.Init();
            IsPassedRate = true;
            //ChangeSeriesCommand = new DelegateCommand(ChangeSeries);
            DataHeaders = motionController.MotionEngine.MapDatas.GroupBy(x => x.Alias).Select(x => x.Key).ToList();
            IPhoneEndTime = DateTime.Now;
            IPhoneStartTime = DateTime.Now.AddDays(-1);
        }

        private void InitIphoneData()
        {
            DateChangeCommand = new DelegateCommand<string>(DataChange);
            GetWeekCapacity(IPhoneStartTime, IPhoneEndTime);
            InitIphoneChart();
            InitIOData();
        }

        /// <summary>
        /// 初始化图表信息
        /// </summary>
        private void InitChart()
        {
            // 更新轴标签
            UpdateAxis();

            // 默认更新良率表
            GetOKNGSeries();

            // 更新产能图表
            UpdateCapacityChart();
        }


        /// <summary>
        /// 初始化Iphone图表信息
        /// </summary>
        private void InitIphoneChart()
        {
         
            var okRate = new ChartValues<double>();
            var ngRate = new ChartValues<double>();

            var lables = new List<string>();
            int i = 1;
            foreach (var item in _analyzeWeekModels)
            {
                okRate.Add(item.OkRate);
                ngRate.Add(item.NgRate);
                lables.Add($"Day{i}");
                i++;
            }

            IphoneCapacitySeries = new SeriesCollection
            {
                new ColumnSeries
                {
                    Values = okRate,
                    Title="OK",
                    DataLabels=true,
                    Fill = GreenBrush,
                },
                new ColumnSeries
                {
                    Values = ngRate,
                    Title ="NG",
                    DataLabels=true,
                    Fill = OrangeBrush,
                }
            };

            IPhoneRateLabels = lables.ToArray();
        }

        /// <summary>
        /// 更新产能图表
        /// </summary>
        private void UpdateCapacityChart()
        {
            var okCount = new ChartValues<double>();
            var ngCount = new ChartValues<double>();
            foreach (var item in _analyzeModels)
            {
                okCount.Add(item.OkCount);
                ngCount.Add(item.NgCount);
            }

            CapacitySeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Values = okCount,
                    LineSmoothness = 0,
                    Title=Luster.Motion.Assests.Langs.LangProvider.GetLang("EndProduct"),
                    PointGeometry=DefaultGeometries.Circle,
                    PointGeometrySize=5,
                    Stroke = GreenBrush,
                },
                new LineSeries
                {
                    Values = ngCount,
                    LineSmoothness = 0,
                    Title ="NG",
                    PointGeometry=DefaultGeometries.Circle,
                    PointGeometrySize=5,
                    Stroke = OrangeBrush,
                }
            };
        }

        /// <summary>
        /// 更新图表标签
        /// </summary>
        private void UpdateAxis()
        {
            var lables = new List<string>();
            foreach (var item in _analyzeModels)
            {
                lables.Add(item.Time.ToString("t"));
            }

            if (_analyzeModels.Count > 11)
            {
                ChartWidth = 40 * _analyzeModels.Count;//默认最小宽度440，柱状图11列，平均到每列40宽度
            }

            RateLabels = lables.ToArray();
            RateFormatter = value => value.ToString("G");

            CapacityLabels = lables.ToArray();
            CapacityFormatter = value => value.ToString("G");
        }

        /// <summary>
        /// 获取图表分析数据
        /// </summary>
        private void GetAnalyzeModel()
        {
            var startTime = StartTime;
            var endTime = EndTime;
            var products = reporitory.GetList<TbProductInfo>(x => x.CreateTime > startTime && x.CreateTime < endTime, x => x.ID).ToList();
            // 2根据小时分组
            var dic = new Dictionary<DateTime, List<TbProductInfo>>();
            startTime = DateTime.Parse($"{startTime.ToString("yyyy-MM-dd")} {startTime.Hour}:00:00");
            while (startTime < endTime)
            {
                var pInfos = products.Where(x => x.EnterTime > startTime && x.EnterTime < startTime.AddHours(1)).ToList();
                if (pInfos.Count > 0)
                {
                    dic.Add(startTime, pInfos);
                }
                else
                {
                    dic.Add(startTime, pInfos);
                }
                startTime = startTime.AddHours(1);
            }

            // 3统计各分组数量
            var analyzeModels = new List<AnalyzeModel>();
            foreach (var item in dic)
            {
                var model = new AnalyzeModel() { Time = item.Key };
                if (model != null)
                {
                    if (item.Value.Count > 0)
                    {
                        model.OkCount = item.Value.Count(x => x.Result == "OK");
                        model.NgCount = item.Value.Count(x => x.Result != "OK");
                    }
                    else
                    {
                        model.OkCount = 0;
                        model.NgCount = 0;
                    }

                }
                analyzeModels.Add(model);
            }

            // 4计算个分组良率/NG率
            for (int i = 0; i < analyzeModels.Count; i++)
            {
                if (analyzeModels[i].OkCount > 0 || analyzeModels[i].NgCount > 0)
                {
                    var rate = Math.Round(((double)analyzeModels[i].OkCount) / (analyzeModels[i].OkCount + analyzeModels[i].NgCount) * 100, 2);
                    analyzeModels[i].OkRate = rate;
                    analyzeModels[i].NgRate = 100 - rate;
                }
            }
            _analyzeModels = analyzeModels.OrderBy(x => x.Time).ToList();
        }


        /// <summary>
        /// 获取图表分析数据
        /// </summary>
        private void GetWeekCapacity(DateTime startTime,DateTime endTime)
        {
            var products = reporitory.GetList<TbProductInfo>(x => x.CreateTime > startTime && x.CreateTime < endTime, x => x.ID).ToList();
            // 2根据小时分组
            var dic = new Dictionary<DateTime, List<TbProductInfo>>();
            startTime = DateTime.Parse($"{startTime.ToString("yyyy-MM-dd")} 00:00:00");
            while (startTime < endTime)
            {
                var pInfos = products.Where(x => x.EnterTime > startTime && x.EnterTime < startTime.AddDays(1)).ToList();
                if (pInfos.Count > 0)
                {
                    dic.Add(startTime, pInfos);
                }
                else
                {
                    dic.Add(startTime, pInfos);
                }
                startTime = startTime.AddDays(1);
            }

            // 3统计各分组数量
            var analyzeModels = new List<AnalyzeModel>();
            foreach (var item in dic)
            {
                var model = new AnalyzeModel() { Time = item.Key };
                if (model != null)
                {
                    if (item.Value.Count > 0)
                    {
                        model.OkCount = item.Value.Count(x => x.Result == "OK");
                        model.NgCount = item.Value.Count(x => x.Result != "OK");
                        var sumCT=item.Value.Sum(x=>x.CT);
                        model.SumCT = sumCT;
                    }
                    else
                    {
                        model.OkCount = 0;
                        model.NgCount = 0;
                        model.SumCT = 0;
                    }

                }
                analyzeModels.Add(model);
            }

            // 4计算个分组良率/NG率
            for (int i = 0; i < analyzeModels.Count; i++)
            {
                if (analyzeModels[i].OkCount > 0 || analyzeModels[i].NgCount > 0)
                {
                    var rate = Math.Round(((double)analyzeModels[i].OkCount) / (analyzeModels[i].OkCount + analyzeModels[i].NgCount) * 100, 2);
                    analyzeModels[i].OkRate = rate;
                    analyzeModels[i].NgRate = Math.Round(100 - rate,2);
                }
            }
            _analyzeWeekModels = analyzeModels.OrderBy(x => x.Time).ToList();
        }

        private void InitIOData()
        {
            if (_analyzeWeekModels == null) return;
            double totalOkCount = 0;
            double totalNgCount = 0;
            double sumCT = 0;
            foreach (var item in _analyzeWeekModels)
            {
                totalOkCount+=item.OkCount;
                totalNgCount+=item.NgCount;
                sumCT += item.SumCT;
            }
            InOutCount = $"{totalNgCount+totalOkCount}/{totalNgCount + totalOkCount}";
            PassFailCount = $"{totalOkCount}/{totalNgCount}";
            Yield = Math.Round(totalOkCount / (totalOkCount + totalNgCount), 2).ToString();
            RealCT = Math.Round(sumCT / (totalOkCount + totalNgCount), 2).ToString();
            double TargetUph = _mController.SysConfig.CT == 0 ? 0 : (int)(3600 / _mController.SysConfig.CT);
            UPH = RealCT=="0" ? TargetUph.ToString() : Math.Round((3600 / Convert.ToDouble(RealCT)),0).ToString();
            if (_mController.SysConfig.ListRecipe != null)
            {
               var currentRecipe= _mController.SysConfig.ListRecipe.FirstOrDefault(x=>x.IsActive=true);
               RecipeName= currentRecipe.RecipeName;
            }
           
        }

        /// <summary>
        /// OK/NG柱状图
        /// </summary>
        private void GetOKNGSeries()
        {

            UpdateAxis();
            var ngValue = new ChartValues<int>();
            var okValue = new ChartValues<int>();
            foreach (var item in _analyzeModels)
            {
                ngValue.Add(item.NgCount);
                okValue.Add(item.OkCount);
            }
            RateSeriesCollection = new SeriesCollection
            {
                new StackedColumnSeries
                {
                    StackMode = StackMode.Values,
                    DataLabels = true,
                    LabelsPosition=BarLabelPosition.Perpendicular,
                    Fill= new SolidColorBrush(Color.FromRgb(0, 255, 0)),
                    Title ="OK",
                    Values=okValue

                },
            };

            RateSeriesCollection.Add(new StackedColumnSeries
            {
                StackMode = StackMode.Values,
                DataLabels= true,
                LabelsPosition=BarLabelPosition.Perpendicular,
                Fill = new SolidColorBrush(Color.FromRgb(255, 0, 0)),
                Title = "NG",
                Values =ngValue
            });

        }

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <summary>
        /// 查询数据 - 优化版：只查一次DB，同时产出趋势和分析模型
        /// </summary>
        protected override void Query()
        {
            if (EndTime < StartTime)
                throw new FriendlyException($"结束时间{EndTime}请早于开始时间{StartTime}!");

            var old = Interlocked.Exchange(ref _queryCts, new CancellationTokenSource());
            if (old != null)
            {
                try { old.Cancel(); } catch { }
                old.Dispose();
            }
            var token = _queryCts.Token;

            var queryStart = StartTime;
            var queryEnd = EndTime;

            Task.Run(() =>
            {
                token.ThrowIfCancellationRequested();

                // 只查一次DB，同时产出两份聚合结果
                var (trendModels, analyzeModels, isByDay) = BuildAllModelsFast(queryStart, queryEnd, token);

                // build chart values in background
                var rateOk = new ChartValues<int>();
                var rateNg = new ChartValues<int>();
                var capOk = new ChartValues<double>();
                var capNg = new ChartValues<double>();
                var labels = new List<string>(analyzeModels.Count);

                // 根据粒度选择格式：按天显示日期，按小时显示时间
                var labelFormat = isByDay ? "MM/dd" : "HH:mm";

                foreach (var m in analyzeModels)
                {
                    token.ThrowIfCancellationRequested();
                    labels.Add(m.Time.ToString(labelFormat));
                    rateOk.Add(m.OkCount);
                    rateNg.Add(m.NgCount);
                    capOk.Add(m.OkCount);
                    capNg.Add(m.NgCount);
                }

                _dispatcher?.BeginInvoke(new Action(() =>
                {
                    if (token.IsCancellationRequested) return;

                    _analyzeModels = analyzeModels;
                    ChartWidth = _analyzeModels.Count > 11 ? 40 * _analyzeModels.Count : 440;

                    RateLabels = labels.ToArray();
                    CapacityLabels = labels.ToArray();
                    RateFormatter = v => v.ToString("G");
                    CapacityFormatter = v => v.ToString("G");

                    RateSeriesCollection = new SeriesCollection
                    {
                        new StackedColumnSeries
                        {
                            StackMode = StackMode.Values,
                            DataLabels = true,
                            LabelsPosition = BarLabelPosition.Perpendicular,
                            Fill = new SolidColorBrush(Color.FromRgb(0, 255, 0)),
                            Title = "OK",
                            Values = rateOk
                        },
                        new StackedColumnSeries
                        {
                            StackMode = StackMode.Values,
                            DataLabels = true,
                            LabelsPosition = BarLabelPosition.Perpendicular,
                            Fill = new SolidColorBrush(Color.FromRgb(255, 0, 0)),
                            Title = "NG",
                            Values = rateNg
                        }
                    };

                    CapacitySeriesCollection = new SeriesCollection
                    {
                        new LineSeries
                        {
                            Values = capOk,
                            LineSmoothness = 0,
                            Title = Luster.Motion.Assests.Langs.LangProvider.GetLang("EndProduct"),
                            PointGeometry = DefaultGeometries.Circle,
                            PointGeometrySize = 5,
                            Stroke = GreenBrush,
                        },
                        new LineSeries
                        {
                            Values = capNg,
                            LineSmoothness = 0,
                            Title = "NG",
                            PointGeometry = DefaultGeometries.Circle,
                            PointGeometrySize = 5,
                            Stroke = OrangeBrush,
                        }
                    };

                    // load first page
                    PageUpdated(new FunctionEventArgs<int>(1));
                    PageIndex = 1;
                }));
            }, token);
        }

        /// <summary>
        /// 只查一次DB，同时产出趋势模型和分析模型，并返回是否按天聚合
        /// </summary>
        private (List<HistoryTrendModel> trend, List<AnalyzeModel> analyze, bool isByDay) BuildAllModelsFast(
            DateTime startTime, DateTime endTime, CancellationToken token)
        {
            // 只查一次
            var products = reporitory.GetList<TbProductInfo>(
                x => x.CreateTime > startTime && x.CreateTime < endTime,
                x => x.ID, false).ToList();

            token.ThrowIfCancellationRequested();

            // 根据时间跨度决定粒度
            var byDay = (endTime - startTime).TotalDays > 1;

            // Trend buckets (按 CreateTime)
            var trendBuckets = new SortedDictionary<DateTime, HistoryTrendModel>();

            // Analyze buckets (按 EnterTime 的小时)
            var alignedStart = new DateTime(startTime.Year, startTime.Month, startTime.Day, startTime.Hour, 0, 0);
            var analyzeBuckets = new Dictionary<DateTime, AnalyzeModel>();

            // 预建小时桶（如果点数太多则改为按天）
            var analyzeByDay = (endTime - startTime).TotalHours > 200; // 超过200小时改按天
            var analyzeStep = analyzeByDay ? TimeSpan.FromDays(1) : TimeSpan.FromHours(1);
            var analyzeAlignedStart = analyzeByDay ? startTime.Date : alignedStart;

            for (var t = analyzeAlignedStart; t < endTime; t = t.Add(analyzeStep))
            {
                analyzeBuckets[t] = new AnalyzeModel { Time = t };
            }

            // 单次遍历，同时填充两个聚合
            for (int i = 0; i < products.Count; i++)
            {
                if ((i & 4095) == 0) token.ThrowIfCancellationRequested();

                var p = products[i];
                var isOk = string.Equals(p.Result, "OK", StringComparison.OrdinalIgnoreCase);

                // Trend: 按 CreateTime
                var trendKey = byDay ? p.CreateTime.Date : new DateTime(p.CreateTime.Year, p.CreateTime.Month, p.CreateTime.Day, p.CreateTime.Hour, 0, 0);
                if (!trendBuckets.TryGetValue(trendKey, out var trendModel))
                {
                    trendModel = new HistoryTrendModel { Date = trendKey };
                    trendBuckets[trendKey] = trendModel;
                }
                if (isOk) trendModel.OkCount++; else trendModel.NgCount++;

                // Analyze: 按 EnterTime
                var analyzeKey = analyzeByDay
                    ? p.EnterTime.Date
                    : new DateTime(p.EnterTime.Year, p.EnterTime.Month, p.EnterTime.Day, p.EnterTime.Hour, 0, 0);
                if (analyzeBuckets.TryGetValue(analyzeKey, out var analyzeModel))
                {
                    if (isOk) analyzeModel.OkCount++; else analyzeModel.NgCount++;
                }
            }

            // 计算良率
            foreach (var m in analyzeBuckets.Values)
            {
                var total = m.OkCount + m.NgCount;
                if (total > 0)
                {
                    m.OkRate = Math.Round((double)m.OkCount / total * 100.0, 2);
                    m.NgRate = 100 - m.OkRate;
                }
            }

            // 补齐 Trend 空桶
            var trendStep = byDay ? TimeSpan.FromDays(1) : TimeSpan.FromHours(1);
            var trendAlignedStart = byDay ? startTime.Date : alignedStart;
            for (var t = trendAlignedStart; t < endTime; t = t.Add(trendStep))
            {
                if (!trendBuckets.ContainsKey(t))
                    trendBuckets[t] = new HistoryTrendModel { Date = t };
            }

            return (
                trendBuckets.Values.OrderBy(x => x.Date).ToList(),
                analyzeBuckets.Values.OrderBy(x => x.Time).ToList(),
                analyzeByDay
            );
        }

        /// <summary>
        /// Faster analyze model: single-pass aggregation rather than per-hour LINQ scans.
        /// </summary>
        private List<AnalyzeModel> BuildAnalyzeModelsFast(DateTime startTime, DateTime endTime, CancellationToken token)
        {
            // Pull minimal rows (still full-range; but remove O(hours*N) scans)
            var products = reporitory.GetList<TbProductInfo>(x => x.CreateTime > startTime && x.CreateTime < endTime, x => x.ID, false).ToList();
            token.ThrowIfCancellationRequested();

            // Align to hour
            var alignedStart = DateTime.Parse($"{startTime:yyyy-MM-dd} {startTime.Hour}:00:00");

            // Prepare buckets
            var buckets = new List<AnalyzeModel>();
            var index = new Dictionary<DateTime, AnalyzeModel>();
            for (var t = alignedStart; t < endTime; t = t.AddHours(1))
            {
                var m = new AnalyzeModel { Time = t };
                buckets.Add(m);
                index[t] = m;
            }

            // Aggregate by hour of EnterTime (keep same business rule as old code)
            for (int i = 0; i < products.Count; i++)
            {
                if ((i & 4095) == 0) token.ThrowIfCancellationRequested();

                var p = products[i];
                var key = DateTime.Parse($"{p.EnterTime:yyyy-MM-dd} {p.EnterTime.Hour}:00:00");
                if (!index.TryGetValue(key, out var bucket))
                    continue;

                if (string.Equals(p.Result, "OK", StringComparison.OrdinalIgnoreCase))
                    bucket.OkCount++;
                else
                    bucket.NgCount++;
            }

            // Rates
            foreach (var m in buckets)
            {
                token.ThrowIfCancellationRequested();
                var total = m.OkCount + m.NgCount;
                if (total > 0)
                {
                    var rate = Math.Round((double)m.OkCount / total * 100.0, 2);
                    m.OkRate = rate;
                    m.NgRate = 100 - rate;
                }
            }

            return buckets.OrderBy(x => x.Time).ToList();
        }

        /// <summary>
        /// Faster history trend: single-pass bucket aggregation.
        /// </summary>
        private List<HistoryTrendModel> GetHistiryTrendModelFast(DateTime startTime, DateTime endTime, CancellationToken token)
        {
            var list = reporitory.GetList<TbProductInfo>(x => x.CreateTime > startTime && x.CreateTime < endTime, x => x.ID, false).ToList();
            token.ThrowIfCancellationRequested();

            var byDay = startTime.AddDays(1) < endTime;
            var buckets = new SortedDictionary<DateTime, HistoryTrendModel>();

            DateTime BucketKey(DateTime dt)
            {
                if (byDay) return dt.Date;
                return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0);
            }

            for (int i = 0; i < list.Count; i++)
            {
                if ((i & 4095) == 0) token.ThrowIfCancellationRequested();

                var p = list[i];
                var key = BucketKey(p.CreateTime);
                if (!buckets.TryGetValue(key, out var model))
                {
                    model = new HistoryTrendModel { Date = key, OkCount = 0, NgCount = 0 };
                    buckets.Add(key, model);
                }

                if (string.Equals(p.Result, "OK", StringComparison.OrdinalIgnoreCase))
                    model.OkCount++;
                else
                    model.NgCount++;
            }

            // Ensure continuous x-axis buckets between start/end (optional)
            var alignedStart = byDay ? startTime.Date : new DateTime(startTime.Year, startTime.Month, startTime.Day, startTime.Hour, 0, 0);
            var step = byDay ? TimeSpan.FromDays(1) : TimeSpan.FromHours(1);
            for (var t = alignedStart; t < endTime; t = t.Add(step))
            {
                token.ThrowIfCancellationRequested();
                if (!buckets.ContainsKey(t))
                    buckets.Add(t, new HistoryTrendModel { Date = t, OkCount = 0, NgCount = 0 });
            }

            return buckets.Values.OrderBy(x => x.Date).ToList();
        }

        protected override void PageUpdated(FunctionEventArgs<int> obj)
        {
            // Load page data off UI thread, then marshal to UI for binding.
            var startTime = StartTime;
            var endTime = EndTime;

            if (ConfigurationManager.AppSettings.AllKeys.Contains("UITemplate"))
            {
                var keyvalue = ConfigurationManager.AppSettings["UITemplate"];
                if (keyvalue != "")
                {
                    startTime = IPhoneStartTime;
                    endTime = IPhoneEndTime;
                }
            }

            var index = obj.Info;
            var search = SearchParas;
            var perPage = PerPageCount;

            // Pre-build empty columns template once per call (small)
            var emptyData = new Dictionary<string, object>();
            if (DataHeaders != null && DataHeaders.Count > 0)
            {
                foreach (var header in DataHeaders)
                    emptyData[header] = "";
            }

            Task.Run(() =>
            {
                long count;
                List<TbProductInfo> productInfos;

                if (string.IsNullOrEmpty(search))
                {
                    productInfos = reporitory.GetPage<TbProductInfo>(x => x.CreateTime > startTime && x.CreateTime < endTime,
                        x => x.ID, index, perPage, out count).ToList();
                }
                else
                {
                    productInfos = reporitory.GetPage<TbProductInfo>(x => x.CreateTime > startTime && x.CreateTime < endTime && x.SNCode.Contains(search),
                        x => x.ID, index, perPage, out count).ToList();
                }

                var items = new ObservableCollection<ReportItem>();
                if (productInfos != null && productInfos.Count > 0)
                {
                    foreach (var product in productInfos)
                    {
                        var model = new ReportItem
                        {
                            Id = product.ID,
                            BarCode = product.SNCode,
                            EnterTime = product.EnterTime,
                            OutTime = product.OutTime,
                            CT = product.CT,
                            Jig = product.Jig,
                            Result = product.IsToss ? "Toss" : product.Result,
                            NgReason = product.NgReason,
                            ImagePath = product.ImagePath,
                        };

                        // Copy template dictionary (one per row). If headers are large, consider lazy loading of Data.
                        var rowDict = new Dictionary<string, object>(emptyData);
                        if (!string.IsNullOrEmpty(product.Data))
                        {
                            var splitGroup = product.Data.Split('|');
                            foreach (var item in splitGroup)
                            {
                                var tempValue = item.Split(':');
                                if (tempValue.Length == 2)
                                {
                                    if (rowDict.ContainsKey(tempValue[0]))
                                        rowDict[tempValue[0]] = tempValue[1];
                                }
                            }
                        }
                        model.Data = rowDict;
                        items.Add(model);
                    }
                }

                _dispatcher?.BeginInvoke(new Action(() =>
                {
                    ReportItemVMs = items;
                    PageCount = count > 0 ? (int)(count % perPage == 0 ? count / perPage : count / perPage + 1) : 1;
                }));
            });
        }

        private void UpdateChart(DateTime startTime, DateTime endTime, List<HistoryTrendModel> trendModels)
        {
            // ...existing code...
        }

        /// <summary>
        /// 初始化Model
        /// </summary>
        private void InitItemsModel()
        {
            PageUpdated(new FunctionEventArgs<int>(1));
            PageIndex = 1;
        }


        #region I/O Data
        private string _recipeName;
        public string RecipeName
        {
            get { return _recipeName; }
            set { SetProperty(ref _recipeName, value); }
        }
        /// <summary>
        /// input/output
        /// </summary>
        private string _inOutCount;
        public string InOutCount
        {
            get { return _inOutCount; }
            set { SetProperty(ref _inOutCount, value); }
        }

        /// <summary>
        /// 良率
        /// </summary>
        private string _yield;
        public string Yield
        {
            get { return _yield; }
            set { SetProperty(ref _yield, value); }
        }

        /// <summary>
        /// pass/fail
        /// </summary>
        private string _passFailCount;
        public string PassFailCount
        {
            get { return _passFailCount; }
            set { SetProperty(ref _passFailCount, value); }
        }

        /// <summary>
        /// UPH
        /// </summary>
        private string _uph;
        public string UPH
        {
            get { return _uph; }
            set { SetProperty(ref _uph, value); }
        }

        /// <summary>
        /// realct
        /// </summary>
        private string _realCT;
        public string RealCT
        {
            get { return _realCT; }
            set { SetProperty(ref _realCT, value); }
        }

        private void DataChange(string commandparam)
        {
            if (string.IsNullOrEmpty(commandparam)) return;
            switch (commandparam)
            {
                case "IPhoneStartDate":
                    QueryIphoneData();
                    break;
                case "IPhoneEndDate":
                    QueryIphoneData();
                    break;
                case "ClassStartDate":
                    break;
                case "ClassEndDate":
                    break;
            }
        }

        private void QueryIphoneData()
        {
            if (IPhoneStartTime < IPhoneEndTime)
            {
                InitItemsModel();
                InitIphoneData();
            }
        }
        #endregion

        #region 导航接口
        public override bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return false;
        }

        /// <summary>
        /// 离开当前页面
        /// </summary>
        /// <param name="navigationContext"></param>
        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            var cts = Interlocked.Exchange(ref _queryCts, null);
            if (cts != null)
            {
                try { cts.Cancel(); } catch { }
                cts.Dispose();
            }
        }

        /// <summary>
        /// 进入当前页面
        /// </summary>
        /// <param name="navigationContext"></param>
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
            Query();
        }

        #endregion
    }
}
