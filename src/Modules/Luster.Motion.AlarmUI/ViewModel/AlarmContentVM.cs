#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       AlarmContentVM
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.AlarmUI.ViewModel
* 文 件 名:       AlarmContentVM.cs
* 创建时间:       2022/7/12 9:17:35
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:       <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com
* 唯一标识：      270ca33e-beb3-4134-8af3-461d9560a5a2
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:               2022/7/12 9:17:35
* 修 改 人:               Z05592
************************************************************************************/
#endregion
using HandyControl.Data;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using LiveCharts.Configurations;
using Luster.Common.DataAccess.Repositories;
using Luster.Common.DataAccess.Tables;
using Luster.Common.Tools;
using Luster.Motion.AlarmUI.Model;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Models;
using Luster.Motion.CommonUI.ViewModel;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Luster.Motion.DataStruct.Enums;
using Luster.Common.DataStruct.Extensions;

namespace Luster.Motion.AlarmUI.ViewModel
{
    public class AlarmContentVM : MotionVM, INavigationAware
    {
        //本地画笔
        public List<SolidColorBrush> _localBrushes;

        /// <summary>
        /// 报警页面标识
        /// </summary>
        private bool _isAlarmView;
        public bool IsAlarmView
        {
            get { return _isAlarmView; }
            set { SetProperty(ref _isAlarmView, value); }
        }

        public DelegateCommand<string> ChangeSerieCommand { get; set; }

        public DelegateCommand ChangeViewCommand { get; set; }

        public DelegateCommand QueryCommand { get; set; }

        public DelegateCommand<FunctionEventArgs<int>> PageUpdatedCommand { get; set; }

        public DelegateCommand ShowAlarmViewCommand { get; private set; }
        public DelegateCommand ShowLogViewCommand { get; private set; }

        #region 报警分页参数
        /// <summary>
        /// 报警条目列表
        /// </summary>
        private ObservableCollection<AlarmItemModel> _alarItemVMs;
        public ObservableCollection<AlarmItemModel> AlarmItemVMs
        {
            get { return _alarItemVMs; }
            set { SetProperty(ref _alarItemVMs, value); }
        }
        /// <summary>
        /// 最大页数
        /// </summary>
        private int _alarmPageCount;
        public int AlarmPageCount
        {
            get { return _alarmPageCount; }
            set { SetProperty(ref _alarmPageCount, value); }
        }

        /// <summary>
        /// 每页数量
        /// </summary>
        private int _alarmPerPageCount;
        public int AlarmPerPageCount
        {
            get { return _alarmPerPageCount; }
            set { SetProperty(ref _alarmPerPageCount, value); }
        }

        /// <summary>
        /// 当日全部报警
        /// </summary>
        private ObservableCollection<AlarmItemModel> _currentDayAlarmVMs;
        public ObservableCollection<AlarmItemModel> CurrentDayAlarmVMs
        {
            get { return _currentDayAlarmVMs; }
            set { SetProperty(ref _currentDayAlarmVMs, value); }
        }

        /// <summary>
        /// 页码
        /// </summary>
        private int _alarmPageIndex;
        public int AlarmPageIndex
        {
            get { return _alarmPageIndex; }
            set { SetProperty(ref _alarmPageIndex, value); }
        }
        #endregion


        #region 日志分页参数
        /// <summary>
        /// 报警Items
        /// </summary>
        private ObservableCollection<LogItemModel> _logItemVMs;
        public ObservableCollection<LogItemModel> LogItemVMs
        {
            get { return _logItemVMs; }
            set { SetProperty(ref _logItemVMs, value); }
        }

        /// <summary>
        /// 最大页数
        /// </summary>
        private int _logPageCount;
        public int LogPageCount
        {
            get { return _logPageCount; }
            set { SetProperty(ref _logPageCount, value); }
        }

        /// <summary>
        /// 每页数量
        /// </summary>
        private int _logPerPageCount;
        public int LogPerPageCount
        {
            get { return _logPerPageCount; }
            set { SetProperty(ref _logPerPageCount, value); }
        }

        /// <summary>
        /// 页码
        /// </summary>
        private int _logPageIndex;
        public int LogPageIndex
        {
            get { return _logPageIndex; }
            set { SetProperty(ref _logPageIndex, value); }
        }
        #endregion

        #region 图表相关设置

        /// <summary>
        /// 图标系列
        /// </summary>
        private SeriesCollection _sCollections;
        public SeriesCollection SCollections
        {
            get { return _sCollections; }
            set { SetProperty(ref _sCollections, value); }
        }

        /// <summary>
        /// X轴标签
        /// </summary>
        private string[] _alarmLables;
        public string[] AlarmLables
        {
            get { return _alarmLables; }
            set { SetProperty(ref _alarmLables, value); }
        }

        /// <summary>
        /// Y轴标签格式器
        /// </summary>
        private Func<double, string> _alarmFormatter;
        public Func<double, string> AlarmFormatter
        {
            get { return _alarmFormatter; }
            set { SetProperty(ref _alarmFormatter, value); }
        }

        #endregion

        #region IPhone图表相关设置
        /// <summary>
        /// 报警时长标签统计
        /// </summary>
        private List<string> _durations;
        public List<string> Durations
        {
            get { return _durations; }
            set { SetProperty(ref _durations, value); }
        }

        /// <summary>
        /// 报警时长统计
        /// </summary>
        private SeriesCollection _alarmDurations;
        public SeriesCollection AlarmDurations
        {
            get { return _alarmDurations; }
            set { SetProperty(ref _alarmDurations, value); }
        }
        /// <summary>
        /// 报警类型饼状图
        /// </summary>
        private SeriesCollection _pieSeriesCollection;
        public SeriesCollection PieSeriesCollection
        {
            get { return _pieSeriesCollection; }
            set { SetProperty(ref _pieSeriesCollection, value); }
        }

        /// <summary>
        /// 报警按组统计
        /// </summary>
        private ObservableCollection<AlarmGroupModel> _alarmGroupList;
        public ObservableCollection<AlarmGroupModel> AlarmGroupList
        {
            get { return _alarmGroupList; }
            set { SetProperty(ref _alarmGroupList, value); }
        }
        #endregion

        #region 数据选择
        /// <summary>
        /// 开始时间
        /// </summary>
        private DateTime _startTime;
        public DateTime StartTime
        {
            get { return _startTime; }
            set { SetProperty(ref _startTime, value); }
        }

        /// <summary>
        /// 结束时间
        /// </summary>
        private DateTime _endTime;
        public DateTime EndTime
        {
            get { return _endTime; }
            set { SetProperty(ref _endTime, value); }
        }

        /// <summary>
        /// 筛选条件
        /// </summary>
        private string _searchParas;
        public string SearchParas
        {
            get { return _searchParas; }
            set { SetProperty(ref _searchParas, value); }
        }

        #endregion

        #region DownTime(宕机时间段图)

        public class DownTimeSegment
        {
            public DateTime Day { get; set; }
            public DateTime Start { get; set; }
            public DateTime End { get; set; }
            public double Minutes { get; set; }
            public string AlarmType { get; set; }
            public string Module { get; set; }
        }

        private SeriesCollection _downTimeSeries;
        public SeriesCollection DownTimeSeries
        {
            get { return _downTimeSeries; }
            set { SetProperty(ref _downTimeSeries, value); }
        }

        private string[] _downTimeLabels;
        public string[] DownTimeLabels
        {
            get { return _downTimeLabels; }
            set { SetProperty(ref _downTimeLabels, value); }
        }

        private Func<double, string> _downTimeFormatter;
        public Func<double, string> DownTimeFormatter
        {
            get { return _downTimeFormatter; }
            set { SetProperty(ref _downTimeFormatter, value); }
        }

        private ObservableCollection<DownTimeSegment> _downTimeSegments;
        public ObservableCollection<DownTimeSegment> DownTimeSegments
        {
            get { return _downTimeSegments; }
            set { SetProperty(ref _downTimeSegments, value); }
        }

        // 强制24小时横轴范围(单位：分钟)
        private double _downTimeMinValue = 0;

        public double DownTimeMinValue
        {
            get { return _downTimeMinValue; }
            set 
            { 
                if (value < 0) value = 0;
                SetProperty(ref _downTimeMinValue, value); 
            }
        }

        private double _downTimeMaxValue = 1440;
                public double DownTimeMaxValue
        {
            get { return _downTimeMaxValue; }
            set 
            { 
                if (value > 1440) value = 1440;
                SetProperty(ref _downTimeMaxValue, value); 
            }
        }

        private Func<double, string> _downTimeYLabelFormatter;
        public Func<double, string> DownTimeYLabelFormatter
        {
            get { return _downTimeYLabelFormatter; }
            set { SetProperty(ref _downTimeYLabelFormatter, value); }
        }

        private double _downTimeYMinValue;
        public double DownTimeYMinValue
        {
            get { return _downTimeYMinValue; }
            set { SetProperty(ref _downTimeYMinValue, value); }
        }

        private double _downTimeYMaxValue;
        public double DownTimeYMaxValue
        {
            get { return _downTimeYMaxValue; }
            set { SetProperty(ref _downTimeYMaxValue, value); }
        }


        #endregion

        private IDbManager _dbManager;
        public DateTime _firstClassTime = DateTime.Now;
        private IRepository _repository;
        public AlarmContentVM(ICommonBus _commonBus, IDbManager dbManager, IRepository repository) : base(_commonBus)
        {
            InitModel();
            InitAlarmModels(dbManager);
            _repository = repository;
        }
        private void InitModel()
        {
            IsAlarmView = true;
            ChangeSerieCommand = new DelegateCommand<string>(ChangeSerie);

            // 兼容旧XAML：仍保留切换命令（如需要），但新的XAML使用明确的 ShowAlarmViewCommand/ShowLogViewCommand
            ChangeViewCommand = new DelegateCommand(ChangeView);
            ShowAlarmViewCommand = new DelegateCommand(() => SwitchMainView(true));
            ShowLogViewCommand = new DelegateCommand(() => SwitchMainView(false));

            PageUpdatedCommand = new DelegateCommand<FunctionEventArgs<int>>(PageUpdated);
            QueryCommand = new DelegateCommand(Query);
            AlarmPerPageCount = 20;
            LogPerPageCount = 20;
        }

        private void InitAlarmModels(IDbManager dbManager)
        {
            _dbManager = dbManager;
            _firstClassTime = _dbManager.GetStartTime();
            if (DateTime.Now < _firstClassTime)
            {
                StartTime = _firstClassTime.AddDays(-1).Date;
                EndTime = _firstClassTime.Date;
            }
            else
            {
                StartTime = _firstClassTime.Date;
                EndTime = _firstClassTime.AddDays(1).Date;
            }
            _localBrushes = new List<SolidColorBrush>()
            {
                 // 红 / 橙 / 黄 / 灰：更符合“报警”语义
                 new SolidColorBrush(Color.FromRgb(255, 193, 7)),   // 黄(警告)  #FFC107
                 new SolidColorBrush(Color.FromRgb(108, 117, 125)), // 灰        #6C757D
                 new SolidColorBrush(Color.FromRgb(73, 80, 87)),    // 深灰      #495057
                 new SolidColorBrush(Color.FromRgb(255, 234, 0)),   // 亮黄      #FFEA00
                 new SolidColorBrush(Color.FromRgb(255, 140, 0)),   // 深橙      #FF8C00
                 new SolidColorBrush(Color.FromRgb(255, 152, 0)),   // 橙        #FF9800
                 new SolidColorBrush(Color.FromRgb(244, 67, 54)),   // 红        #F44336
                 new SolidColorBrush(Color.FromRgb(156, 39, 6)),    // 深红棕    #9C2706
                 new SolidColorBrush(Color.FromRgb(220, 53, 69)),   // 红(危险)  #DC3545
                 new SolidColorBrush(Color.FromRgb(255, 69, 0)),    // 橙红      #FF4500                
            };

            LoadAlarm(StartTime, EndTime, string.Empty);
            LoadDayData();
            LoadCurrentDayAlarm(StartTime, EndTime);
        }

        /// <summary>
        /// 获取当天全部报警
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="empty"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void LoadCurrentDayAlarm(DateTime startTime, DateTime endTime)
        {
            var alarms = _dbManager.GetAnalyzeModels(startTime, endTime);
            CurrentDayAlarmVMs = new ObservableCollection<AlarmItemModel>();
            foreach (var alarm in alarms)
            {
                CurrentDayAlarmVMs.Add(new AlarmItemModel()
                {
                    Id = alarm.ID,
                    Module = alarm.Module,
                    AlarmType = alarm.AlarmType,
                    AlarmLongTime = alarm.AlarmLongTime,
                    EndTime = alarm.EndTime == DateTime.Parse("0001/01/01 00:00:00") ? String.Empty : alarm.EndTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    StartTime = alarm.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    ProcMethod = alarm.ProcMethod,
                    Reason = alarm.Reason,
                    ProcUser = alarm.ProcUser,
                    AlarmCode = alarm.AlarmCode,
                });
            }
        }

        /// <summary>
        ///  查询数据
        /// </summary>
        protected virtual void Query()
        {
            var startTime = StartTime.AddHours(_firstClassTime.Hour).AddMinutes(_firstClassTime.Minute);
            var endTime = EndTime.AddHours(_firstClassTime.Hour).AddMinutes(_firstClassTime.Minute);

            if (IsAlarmView)
            {
                // 统计Tab：刷新统计表
                if (SelectedAlarmTab == AlarmSubTab.Statistics)
                {
                    BuildStatistics(startTime, endTime);
                }
                else if (SelectedAlarmTab == AlarmSubTab.DownTime)
                {
                    BuildDownTimeChart(startTime, endTime);
                }
                else
                {
                    LoadAlarm(startTime, endTime, SearchParas);
                }
            }
            else
            {
                LoadLog(startTime, endTime, SearchParas);
            }

            LoadCurrentDayAlarm(startTime, endTime);
            InitIphoneChart();
            InitPieSeriesData();

        }

        #region DownTime图表构建
        private void BuildDownTimeChart(DateTime startTime, DateTime endTime)
        {
            try
            {
                var list = _dbManager.GetAnalyzeModels(startTime, endTime)?.ToList() ?? new List<TbAlarm>();

                bool IsDownTimeType(TbAlarm a)
                {
                    var t = (a?.AlarmType ?? string.Empty).Trim();
                    if (string.IsNullOrWhiteSpace(t)) return false;
                    if (string.Equals(t, AlarmType.WarningTip.GetDescription(), StringComparison.OrdinalIgnoreCase)) return false;
                    if (string.Equals(t, "弹窗提示", StringComparison.OrdinalIgnoreCase)) return false;
                    return true;
                }

                bool HasValidEndTime(DateTime start, DateTime end)
                {
                    if (end == DateTime.MinValue) return false;
                    if (end == DateTime.Parse("0001-01-01 00:00:00")) return false;
                    return end > start;
                }

                var down = list
                    .Where(IsDownTimeType)
                    .OrderBy(a => a.StartTime)
                    .ToList();

                var segments = new List<DownTimeSegment>();

                foreach (var a in down)
                {
                    if (a == null) continue;

                    var s = a.StartTime;
                    var e = a.EndTime;

                    // 先裁剪到查询范围
                    if (s < startTime) s = startTime;

                    // EndTime 无效：只统计到当前时刻/查询结束，但禁止跨天（避免产生额外日期）
                    var now = DateTime.Now;
                    var clampEnd = now < endTime ? now : endTime;

                    bool validEnd = HasValidEndTime(s, e);
                    if (!validEnd)
                    {
                        e = s.AddMinutes(3);

                        // 禁止跨天：只保留起始当天的段（最多到 24:00 或 e）
                        var dayEnd = s.Date.AddDays(1);
                        if (e > dayEnd) e = dayEnd;
                    }
                    else
                    {
                        // EndTime 有效，则正常裁剪到查询结束
                        if (e > endTime) e = endTime;
                    }

                    if (e <= s) continue;

                    // 只有 EndTime 有效且真实跨天，才允许拆分到多天
                    if (validEnd && s.Date < e.Date)
                    {
                        var cur = s;
                        while (cur.Date < e.Date)
                        {
                            var dayEnd = cur.Date.AddDays(1);
                            var segEnd = dayEnd < e ? dayEnd : e;

                            segments.Add(new DownTimeSegment
                            {
                                Day = cur.Date,
                                Start = cur,
                                End = segEnd,
                                Minutes = (segEnd - cur).TotalMinutes,
                                AlarmType = a.AlarmType,
                                Module = a.Module,
                            });

                            cur = dayEnd;
                        }

                        if (cur < e)
                        {
                            segments.Add(new DownTimeSegment
                            {
                                Day = cur.Date,
                                Start = cur,
                                End = e,
                                Minutes = (e - cur).TotalMinutes,
                                AlarmType = a.AlarmType,
                                Module = a.Module,
                            });
                        }
                    }
                    else
                    {
                        // 不跨天（或 EndTime 无效被强制限制为不跨天）
                        segments.Add(new DownTimeSegment
                        {
                            Day = s.Date,
                            Start = s,
                            End = e,
                            Minutes = (e - s).TotalMinutes,
                            AlarmType = a.AlarmType,
                            Module = a.Module,
                        });
                    }
                }

                DownTimeSegments = new ObservableCollection<DownTimeSegment>(segments);

                // activeDays 只来自 segments -> 没有报警的日期不会出现
                var activeDays = segments.Select(s => s.Day.Date).Distinct().OrderBy(d => d).ToList();
                if (activeDays.Count == 0) activeDays.Add(DateTime.Now.Date);

                DownTimeLabels = activeDays.Select(d => d.ToString("yyyy-MM-dd")).ToArray();

                const double yPadding = 0.5;
                DownTimeYMinValue = 0 - yPadding;
                DownTimeYMaxValue = (activeDays.Count > 1 ? activeDays.Count - 1 : 1) + yPadding;

                DownTimeYLabelFormatter = v =>
                {
                    var v1 = v - 0.1;
                    var nearest = Math.Round(v1);
                    //if (Math.Abs(v - nearest) > 0.45) return string.Empty;

                    var idx = (int)nearest;
                    if (DownTimeLabels == null || idx < 0 || idx >= DownTimeLabels.Length) return string.Empty;
                    return DownTimeLabels[idx];
                };

                DownTimeMinValue = 0;
                DownTimeMaxValue = 1440;
                DownTimeFormatter = v => TimeSpan.FromMinutes(v).ToString(@"hh\:mm");

                // 后续 seriesCollection 构建逻辑保持不变（使用 activeDays.IndexOf(seg.Day)）
                var seriesCollection = new SeriesCollection();
                var groupedSegments = segments.GroupBy(s => s.AlarmType).ToList();

                int colorIndex = 0;
                foreach (var group in groupedSegments)
                {
                    var alarmType = group.Key;
                    var chartValues = new ChartValues<ObservablePoint>();
                    var sortedSegments = group.OrderBy(s => s.Day).ThenBy(s => s.Start).ToList();

                    foreach (var seg in sortedSegments)
                    {
                        var dayIndex = activeDays.IndexOf(seg.Day);
                        if (dayIndex == -1) continue;

                        var startMin = (seg.Start - seg.Day).TotalMinutes;
                        var endMin = (seg.End - seg.Day).TotalMinutes;

                        if (startMin < 0) startMin = 0;
                        if (endMin > 1440) endMin = 1440;
                        if (endMin <= startMin) continue;

                        const double minVisibleMinutes = 2.0;
                        if (endMin - startMin < minVisibleMinutes)
                            endMin = Math.Min(1440, startMin + minVisibleMinutes);

                        chartValues.Add(new ObservablePoint(startMin, dayIndex));
                        chartValues.Add(new ObservablePoint(endMin, dayIndex));
                        chartValues.Add(new ObservablePoint(double.NaN, double.NaN));
                    }

                    if (chartValues.Count > 0)
                    {
                        var brush = (_localBrushes != null && _localBrushes.Count > 0)
                            ? _localBrushes[colorIndex % _localBrushes.Count]
                            : new SolidColorBrush(Color.FromRgb(255, 140, 0));

                        seriesCollection.Add(new LineSeries
                        {
                            Title = alarmType,
                            Values = chartValues,
                            Stroke = brush,
                            Fill = Brushes.Transparent,
                            StrokeThickness = 20,
                            PointGeometry = null,
                            LineSmoothness = 0,
                        });
                        colorIndex++;
                    }
                }

                DownTimeSeries = seriesCollection;

                // legend 同原逻辑（略）
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BuildDownTimeChart error: {ex}");
                DownTimeSegments = new ObservableCollection<DownTimeSegment>();
                DownTimeSeries = new SeriesCollection();
                DownTimeLabels = Array.Empty<string>();
                DownTimeYMinValue = 0;
                DownTimeYMaxValue = 0;
                DownTimeYLabelFormatter = v => string.Empty;
                DownTimeMinValue = 0;
                DownTimeMaxValue = 1440;
                DownTimeFormatter = v => TimeSpan.FromMinutes(v).ToString(@"hh\:mm");
            }
        }
        private void BuildDownTimeChart1(DateTime startTime, DateTime endTime)
        {
            try
            {
                var list = _dbManager.GetAnalyzeModels(startTime, endTime)?.ToList() ?? new List<TbAlarm>();

                bool IsDownTimeType(TbAlarm a)
                {
                    var t = (a?.AlarmType ?? string.Empty).Trim();
                    if (string.IsNullOrWhiteSpace(t)) return false;
                    if (string.Equals(t, AlarmType.WarningTip.GetDescription(), StringComparison.OrdinalIgnoreCase)) return false;
                    if (string.Equals(t, "弹窗提示", StringComparison.OrdinalIgnoreCase)) return false;
                    return true;
                }

                var down = list
                    .Where(IsDownTimeType)
                    .OrderBy(a => a.StartTime)
                    .ToList();

                var segments = new List<DownTimeSegment>();
                foreach (var alarm in down)
                {
                    if (alarm == null) continue;

                    var start = alarm.StartTime;
                    var end = alarm.EndTime;
                    if (end == DateTime.MinValue || end == DateTime.Parse("0001-01-01 00:00:00") || end <= start)
                    {
                        end = start.AddMinutes(3);
                    }
                    if (start < startTime) start = startTime;
                    if (end > endTime) end = endTime;
                    if (end <= start) continue;

                    var cur = start;
                    while (cur.Date < end.Date)
                    {
                        var dayEnd = cur.Date.AddDays(1);
                        var segEnd = dayEnd < end ? dayEnd : end;

                        segments.Add(new DownTimeSegment
                        {
                            Day = cur.Date,
                            Start = cur,
                            End = segEnd,
                            Minutes = (segEnd - cur).TotalMinutes,
                            AlarmType = alarm.AlarmType,
                            Module = alarm.Module,
                        });

                        cur = dayEnd;
                    }

                    if (cur < end)
                    {
                        segments.Add(new DownTimeSegment
                        {
                            Day = cur.Date,
                            Start = cur,
                            End = end,
                            Minutes = (end - cur).TotalMinutes,
                            AlarmType = alarm.AlarmType,
                            Module = alarm.Module,
                        });
                    }
                }

                DownTimeSegments = new ObservableCollection<DownTimeSegment>(segments);
                var activeDays = segments.Select(s => s.Day.Date).Distinct().OrderBy(d => d).ToList();

                if (activeDays.Count == 0)
                {
                    activeDays.Add(DateTime.Now.Date);
                }

                DownTimeLabels = activeDays.Select(d => d.ToString("yyyy-MM-dd")).ToArray();

                // 原来：
                // DownTimeYMinValue = 0;
                // DownTimeYMaxValue = activeDays.Count > 1 ? activeDays.Count - 1 : 1;

                // 修复顶/底行被裁剪导致看起来更窄的问题：为粗线预留上下缓冲
                // 这里使用 0.5 的视觉缓冲即可，不依赖实际 StrokeThickness，简单稳定。

                // … BuildDownTimeChart 里设置 Y 轴范围后 …
                const double yPadding = 0.5;
                DownTimeYMinValue = 0 - yPadding;
                DownTimeYMaxValue = (activeDays.Count > 1 ? activeDays.Count - 1 : 1) + yPadding;

                // 允许 0.25 的容差窗口，避免浮点误差导致整数刻度标签被过滤
                DownTimeYLabelFormatter = v =>
                {
                    var v1 = v - 0.1;
                    var nearest = Math.Round(v1);
                    //if (Math.Abs(v - nearest) > 0.45) return string.Empty;

                    var idx = (int)nearest;
                    if (DownTimeLabels == null || idx < 0 || idx >= DownTimeLabels.Length) return string.Empty;
                    return DownTimeLabels[idx];
                };

                DownTimeMinValue = 0;
                DownTimeMaxValue = 1440;
                DownTimeFormatter = v => TimeSpan.FromMinutes(v).ToString(@"hh\:mm");

                var seriesCollection = new SeriesCollection();
                var groupedSegments = segments.GroupBy(s => s.AlarmType).ToList();

                int colorIndex = 0;

                foreach (var group in groupedSegments)
                {
                    var alarmType = group.Key;
                    var chartValues = new ChartValues<ObservablePoint>();

                    var sortedSegments = group.OrderBy(s => s.Day).ThenBy(s => s.Start).ToList();

                    foreach (var seg in sortedSegments)
                    {
                        var dayIndex = activeDays.IndexOf(seg.Day);
                        if (dayIndex == -1) continue; 

                        var startMin = (seg.Start - seg.Day).TotalMinutes;
                        var endMin = (seg.End - seg.Day).TotalMinutes;

                        if (startMin < 0) startMin = 0;
                        if (endMin > 1440) endMin = 1440;
                        if (endMin <= startMin) continue;

                        const double minVisibleMinutes = 2.0; 
                        if (endMin - startMin < minVisibleMinutes)
                        {
                            endMin = Math.Min(1440, startMin + minVisibleMinutes);
                        }
                        chartValues.Add(new ObservablePoint(startMin, dayIndex));
                        chartValues.Add(new ObservablePoint(endMin, dayIndex));
                        chartValues.Add(new ObservablePoint(double.NaN, double.NaN));
                    }

                    if (chartValues.Count > 0)
                    {
                        var brush = (_localBrushes != null && _localBrushes.Count > 0)
                            ? _localBrushes[colorIndex % _localBrushes.Count]
                            : new SolidColorBrush(Color.FromRgb(40, 150, 221));

                        var lineSeries = new LineSeries
                        {
                            Title = alarmType,
                            Values = chartValues,
                            Stroke = brush,
                            Fill = Brushes.Transparent,
                            StrokeThickness = 20, 
                            PointGeometry = null, 
                            LineSmoothness = 0,   
                            LabelPoint = point =>
                            {
                              
                                var timeTime = TimeSpan.FromMinutes(point.X).ToString(@"hh\:mm");
                                var dateStr = "";
                                int yIdx = (int)Math.Round(point.Y);
                                if (DownTimeLabels != null && yIdx >= 0 && yIdx < DownTimeLabels.Length)
                                {
                                    dateStr = DownTimeLabels[yIdx];
                                }
                                return $"{alarmType} ({dateStr} {timeTime})";
                            }
                        };
                        seriesCollection.Add(lineSeries);
                        colorIndex++;
                    }
                }

                DownTimeSeries = seriesCollection;

                // 在生成 seriesCollection 循环后
                var legend = new List<AlarmGroupModel>();
                int colorIndexLegend = 0;
                foreach (var group in groupedSegments)
                {
                    var brush = (_localBrushes != null && _localBrushes.Count > 0)
                        ? _localBrushes[colorIndexLegend % _localBrushes.Count]
                        : new SolidColorBrush(Color.FromRgb(40, 150, 221));

                    legend.Add(new AlarmGroupModel
                    {
                        AlarmType = group.Key,
                        Color = brush
                    });
                    colorIndexLegend++;
                }
                AlarmGroupList = new ObservableCollection<AlarmGroupModel>(legend);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BuildDownTimeChart error: {ex}");
                DownTimeSegments = new ObservableCollection<DownTimeSegment>();
                DownTimeSeries = new SeriesCollection();
                DownTimeLabels = Array.Empty<string>();
                DownTimeYMinValue = 0;
                DownTimeYMaxValue = 0;
                DownTimeYLabelFormatter = v => string.Empty;
                DownTimeMinValue = 0;
                DownTimeMaxValue = 1440;
                DownTimeFormatter = v => TimeSpan.FromMinutes(v).ToString(@"hh\:mm");
            }
        }
        private void BuildDownTimeChart2(DateTime startTime, DateTime endTime)
        {
            try
            {
                var list = _dbManager.GetAnalyzeModels(startTime, endTime)?.ToList() ?? new List<TbAlarm>();

                bool IsDownTimeType(TbAlarm a)
                {
                    var t = (a?.AlarmType ?? string.Empty).Trim();
                    if (string.IsNullOrWhiteSpace(t)) return false;
                    if (string.Equals(t, AlarmType.WarningTip.GetDescription(), StringComparison.OrdinalIgnoreCase)) return false;
                    if (string.Equals(t, "弹窗提示", StringComparison.OrdinalIgnoreCase)) return false;
                    return true;
                }

                var down = list
                    .Where(IsDownTimeType)
                    .OrderBy(a => a.StartTime)
                    .ToList();

                var segments = new List<DownTimeSegment>();
                foreach (var a in down)
                {
                    if (a == null) continue;

                    var s = a.StartTime;
                    var e = a.EndTime;

                    if (e == DateTime.MinValue || e == DateTime.Parse("0001-01-01 00:00:00") || e <= s)
                    {
                        var now = DateTime.Now;
                        e = now < endTime ? now : endTime;
                    }

                    if (s < startTime) s = startTime;
                    if (e > endTime) e = endTime;
                    if (e <= s) continue;

                    var cur = s;
                    while (cur.Date < e.Date)
                    {
                        var dayEnd = cur.Date.AddDays(1);
                        var segEnd = dayEnd < e ? dayEnd : e;

                        segments.Add(new DownTimeSegment
                        {
                            Day = cur.Date,
                            Start = cur,
                            End = segEnd,
                            Minutes = (segEnd - cur).TotalMinutes,
                            AlarmType = a.AlarmType,
                            Module = a.Module,
                        });

                        cur = dayEnd;
                    }

                    if (cur < e)
                    {
                        segments.Add(new DownTimeSegment
                        {
                            Day = cur.Date,
                            Start = cur,
                            End = e,
                            Minutes = (e - cur).TotalMinutes,
                            AlarmType = a.AlarmType,
                            Module = a.Module,
                        });
                    }
                }

                DownTimeSegments = new ObservableCollection<DownTimeSegment>(segments);
                var activeDays = segments.Select(s => s.Day.Date).Distinct().OrderBy(d => d).ToList();
                
                if (activeDays.Count == 0)
                {
                   activeDays.Add(DateTime.Now.Date);
                }

                DownTimeLabels = activeDays.Select(d => d.ToString("yyyy-MM-dd")).ToArray();DownTimeYMinValue = 0;
                DownTimeYMaxValue = activeDays.Count > 1 ? activeDays.Count - 1 : 1; // 鍗充娇鍙湁涓�琛岋紝Y杞存渶澶у�间篃搴斿尮閰嶇储寮?

                DownTimeYLabelFormatter = v =>
                {
                    var idx = (int)Math.Round(v);
                    if (DownTimeLabels == null || idx < 0 || idx >= DownTimeLabels.Length) return string.Empty;
                    return DownTimeLabels[idx];
                };

                DownTimeMinValue = 0;
                DownTimeMaxValue = 1440;
                DownTimeFormatter = v => TimeSpan.FromMinutes(v).ToString(@"hh\:mm");

                var seriesCollection = new SeriesCollection();
                var groupedSegments = segments.GroupBy(s => s.AlarmType).ToList();
                
                int colorIndex = 0;

                foreach (var group in groupedSegments)
                {
                    var alarmType = group.Key;
                    var chartValues = new ChartValues<ObservablePoint>();

                    // 鎺掑簭淇濊瘉鐢荤嚎椤哄簭
                    var sortedSegments = group.OrderBy(s => s.Day).ThenBy(s => s.Start).ToList();

                    foreach (var seg in sortedSegments)
                    {
                        // 鎵惧埌璇ユ鏃ユ湡鍦╕杞存爣绛句腑鐨勭储寮?
                        var dayIndex = activeDays.IndexOf(seg.Day);
                        if (dayIndex == -1) continue; // 鐞嗚涓婁笉搴斿彂鐢?

                        var startMin = (seg.Start - seg.Day).TotalMinutes;
                        var endMin = (seg.End - seg.Day).TotalMinutes;

                        // 杈圭晫淇濇姢
                        if (startMin < 0) startMin = 0;
                        if (endMin > 1440) endMin = 1440;
                        if (endMin <= startMin) continue;

                        const double minVisibleMinutes = 2.0; // 绋嶅井澧炲姞鏈�灏忓彲瑙佸搴?
                        if (endMin - startMin < minVisibleMinutes)
                        {
                            endMin = Math.Min(1440, startMin + minVisibleMinutes);
                        }

                        // 鏂偣澶勭悊锛氬鏋淰alues閲屽凡缁忔湁鐐癸紝涓斾笂涓�涓偣鐨刌涓嶇瓑浜庡綋鍓峐锛屾垨鑰匵涓嶈繛缁紝鎻掑叆NaN鏂紑
                        // LineSeries 榛樿杩炴帴鎵�鏈夌偣銆備负浜嗙敾鍑虹嫭绔嬬殑鈥滄潯鈥濓紝姣忓姞涓�娈靛墠鍚庨兘闇�瑕佹柇寮�閫昏緫锛?
                        // 浣嗘渶楂樻晥鐨勬槸锛氭瘡娈典綔涓轰竴涓嫭绔嬬殑灏忔姌绾?(Start, Y) -> (End, Y) -> (NaN, NaN)
                        
                        chartValues.Add(new ObservablePoint(startMin, dayIndex));
                        chartValues.Add(new ObservablePoint(endMin, dayIndex));
                        chartValues.Add(new ObservablePoint(double.NaN, double.NaN));
                    }

                    if (chartValues.Count > 0)
                    {
                        // 閫夊彇棰滆壊
                        var brush = (_localBrushes != null && _localBrushes.Count > 0)
                            ? _localBrushes[colorIndex % _localBrushes.Count]
                            : new SolidColorBrush(Color.FromRgb(40, 150, 221));

                        var lineSeries = new LineSeries
                        {
                            Title = alarmType,
                            Values = chartValues,
                            Stroke = brush,
                            Fill = Brushes.Transparent,
                            StrokeThickness = 20, // 妯℃嫙鏉″舰楂樺害
                            PointGeometry = null, // 闅愯棌绔偣鍦嗗湀
                            LineSmoothness = 0,   // 鐩寸嚎
                            // 鑷畾涔?Tooltip 鏄剧ず鍐呭
                            LabelPoint = point => 
                            {
                                // 鐢变簬point鍙寘鍚玐/Y锛屽緢闅惧弽鏌ュ師濮嬩俊鎭�?
                                // 浣嗘牴鎹甔鍊?鍒嗛挓)杞崲鍥炴椂闂存樉绀烘槸鍙鐨勩�?
                                var timeTime = TimeSpan.FromMinutes(point.X).ToString(@"hh\:mm");
                                // Y杞存槸鏃ユ湡绱㈠紩锛屾嬁鍒版棩鏈熷瓧绗︿覆
                                var dateStr = "";
                                int yIdx = (int)Math.Round(point.Y);
                                if (DownTimeLabels != null && yIdx >= 0 && yIdx < DownTimeLabels.Length)
                                {
                                     dateStr = DownTimeLabels[yIdx];
                                }
                                return $"{alarmType} ({dateStr} {timeTime})";
                            }
                        };
                        seriesCollection.Add(lineSeries);
                        colorIndex++;
                    }
                }

                DownTimeSeries = seriesCollection;

                // 在生成 seriesCollection 循环后
                var legend = new List<AlarmGroupModel>();
                int colorIndexLegend = 0;
                foreach (var group in groupedSegments)
                {
                    var brush = (_localBrushes != null && _localBrushes.Count > 0)
                        ? _localBrushes[colorIndexLegend % _localBrushes.Count]
                        : new SolidColorBrush(Color.FromRgb(40, 150, 221));

                    legend.Add(new AlarmGroupModel
                    {
                        AlarmType = group.Key,
                        Color = brush
                    });
                    colorIndexLegend++;
                }
                AlarmGroupList = new ObservableCollection<AlarmGroupModel>(legend);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BuildDownTimeChart error: {ex}");
                DownTimeSegments = new ObservableCollection<DownTimeSegment>();
                DownTimeSeries = new SeriesCollection();
                DownTimeLabels = Array.Empty<string>();
                DownTimeYMinValue = 0;
                DownTimeYMaxValue = 0;
                DownTimeYLabelFormatter = v => string.Empty;
                DownTimeMinValue = 0;
                DownTimeMaxValue = 1440;
                DownTimeFormatter = v => TimeSpan.FromMinutes(v).ToString(@"hh\:mm");
            }
        }
        #endregion

        /// <summary>
        /// 报警时间分布柱状图
        /// </summary>
        protected virtual void InitIphoneChart()
        {
        }

        /// <summary>
        /// 报警时长分类饼状图
        /// </summary>
        protected virtual void InitPieSeriesData()
        {
        }

        /// <summary>
        /// 更新报警图表
        /// </summary>
        /// <param name="type"></param>
        private void ChangeSerie(object type)
        {
            var str = type.ToString();
            switch (str)
            {
                case "天":
                    LoadDayData();
                    break;
                case "周":
                    LoadWeekData();
                    break;
                case "月":
                    LoadMonthData();
                    break;
                default:
                    LoadDayData();
                    break;
            }
        }

        /// <summary>
        /// 重置图表数据
        /// </summary>
        private void ResetChart()
        {
            SCollections = new SeriesCollection();
            AlarmLables = new string[24];
        }

        /// <summary>
        /// 加载一天报警数据
        /// </summary>
        private void LoadDayData()
        {
            var startTime = DateTime.Now;
            var endTime = DateTime.Now;
            if (DateTime.Now < _firstClassTime)
            {
                startTime = _firstClassTime.AddDays(-1);
                endTime = _firstClassTime;
            }
            else
            {
                startTime = _firstClassTime;
                endTime = _firstClassTime.AddDays(1);
            }
            LoadCurrentDayAlarm(startTime, endTime);
            var analyzeData = _dbManager.GetAnalyzeModels(startTime, endTime);

            var dicModel = new Dictionary<string, List<AlarmAnalyzeModel>>();
            //2根据类型分组
            var dic = analyzeData.GroupBy(x => x.AlarmType).ToDictionary(x => x.Key);

            // 3统计各类型分组中时间
            foreach (var item in dic)
            {
                var modelList = new List<AlarmAnalyzeModel>();
                var groups = new Dictionary<int, List<TbAlarm>>();

                int i = 0;
                // 根据时间定位
                while (startTime.AddHours(i) < endTime)
                {
                    var pInfos = item.Value.Where(x => x.CreateTime > startTime.AddHours(i) && x.CreateTime < startTime.AddHours(i + 1)).ToList();
                    if (pInfos.Count > 0)
                    {
                        groups.Add(i, pInfos);
                    }
                    i++;
                }

                foreach (var group in groups)
                {
                    modelList.Add(new AlarmAnalyzeModel() { Index = group.Key, Count = group.Value.Count(), Weight = group.Value.Sum(x => x.AlarmLongTime) });
                }

                dicModel.Add(item.Key, modelList);
            }
            if (dicModel.Count > 0)
            {
                AlarmLables = new string[24];
                for (int i = 0; i < 24; i++)
                {
                    AlarmLables[i] = startTime.AddHours(i).ToString("HH:mm");
                }
                UpdateChart(dicModel);
            }
            else
            {
                ResetChart();
            }

            _chartStart = startTime;
            _chartMode = ChartMode.Day;
        }

        /// <summary>
        /// 加载一周报警数据
        /// </summary>
        private void LoadWeekData()
        {
            var startTime = DateTime.Now;
            var endTime = DateTime.Now;
            if (DateTime.Now < _firstClassTime)
            {
                startTime = _firstClassTime.AddDays(-7);
                endTime = _firstClassTime;
            }
            else
            {
                startTime = _firstClassTime.AddDays(-6);
                endTime = _firstClassTime.AddDays(1);
            }
            LoadCurrentDayAlarm(startTime, endTime);
            var analyzeData = _dbManager.GetAnalyzeModels(startTime, endTime);

            UpdateAlarmChart(analyzeData, startTime, endTime);

            _chartStart = startTime;
            _chartMode = ChartMode.Week;
        }

        /// <summary>
        /// 加载一月报警数据
        /// </summary>
        private void LoadMonthData()
        {
            var startTime = DateTime.Now;
            var endTime = DateTime.Now;
            if (DateTime.Now < _firstClassTime)
            {
                startTime = _firstClassTime.AddMonths(-1).AddDays(-1);
                endTime = _firstClassTime;
            }
            else
            {
                startTime = _firstClassTime.AddMonths(-1);
                endTime = _firstClassTime.AddDays(1);
            }

            LoadCurrentDayAlarm(startTime, endTime);
            var analyzeData = _dbManager.GetAnalyzeModels(startTime, endTime);

            UpdateAlarmChart(analyzeData, startTime, endTime, false);

            _chartStart = startTime;
            _chartMode = ChartMode.Month;
        }

        private void UpdateAlarmChart(IEnumerable<TbAlarm> alarms, DateTime startTime, DateTime endTime, bool isWeek = true)
        {
            var dicModel = new Dictionary<string, List<AlarmAnalyzeModel>>();
            // 根据类型分组
            var dic = alarms.GroupBy(x => x.AlarmType).ToDictionary(x => x.Key);

            // 统计各类型分组中时间
            foreach (var item in dic)
            {
                var modelList = new List<AlarmAnalyzeModel>();
                var groups = new Dictionary<int, List<TbAlarm>>();

                int i = 0;
                // 根据日期确定点位
                while (startTime.AddDays(i) < endTime)
                {
                    var pInfos = item.Value.Where(x => x.CreateTime > startTime.AddDays(i) && x.CreateTime < startTime.AddDays(i + 1)).ToList();
                    if (pInfos.Count > 0)
                    {
                        groups.Add(i, pInfos);
                    }
                    i++;
                }

                foreach (var group in groups)
                {
                    modelList.Add(new AlarmAnalyzeModel() { Index = group.Key, Count = group.Value.Count(), Weight = group.Value.Sum(x => x.AlarmLongTime) });
                }
                dicModel.Add(item.Key, modelList);
            }
            if (dicModel.Count > 0)
            {
                if (isWeek)
                {
                    AlarmLables = new string[7];
                    for (int i = 0; i < 7; i++)
                    {
                        AlarmLables[i] = startTime.AddDays(i).ToString("dddd");
                    }
                }
                else
                {
                    var timeSpan = endTime.Subtract(startTime);
                    AlarmLables = new string[timeSpan.Days + 1];
                    for (int i = 0; i <= timeSpan.Days; i++)
                    {
                        AlarmLables[i] = startTime.AddDays(i).ToString("M");
                    }
                }

                UpdateChart(dicModel);
            }
            else
            {
                ResetChart();
            }
        }

        private void UpdateChart(Dictionary<string, List<AlarmAnalyzeModel>> analyzeData)
        {
            SCollections = new SeriesCollection();
            for (int i = 0; i < analyzeData.Count; i++)
            {
                // 本地10种画笔根据 取余确定画笔
                var y = i % 10;
                var element = analyzeData.ElementAt(i);

                // 把 element 和 brushes 拷贝到局部变量，避免闭包问题
                var elementKey = element.Key;
                var brush = _localBrushes != null && _localBrushes.Count > 0 ? _localBrushes[y] : new SolidColorBrush(Color.FromRgb(111, 111, 111));

                var series = new ScatterSeries()
                {
                    MinPointShapeDiameter = 15,
                    MaxPointShapeDiameter = 45,
                    Fill = brush,
                    Stroke = brush,
                    // 修改点提示：增加时间信息并给时长加单位（秒）
                    LabelPoint = chartPoint =>
                    {
                        // 尝试从底层 ScatterPoint 获取索引与权重
                        var sp = chartPoint.Instance as ScatterPoint;
                        int idx = sp != null ? (int)Math.Round(sp.X) : (int)Math.Round(chartPoint.X);
                        int qty = (int)Math.Round(chartPoint.Y);
                        double dur = sp != null ? sp.Weight : chartPoint.Weight; // 假定为秒
                                                                                 // 构造时间字符串：优先使用 AlarmLables，再回退到根据 StartTime 计算的时间
                        string timeText;
                        if (AlarmLables != null && idx >= 0 && idx < AlarmLables.Length)
                        {
                            var label = AlarmLables[idx];
                            if (label.Contains(":")) // 小时刻度
                                timeText = StartTime.AddHours(idx).ToString("yyyy-MM-dd ") + label;
                            else
                                timeText = StartTime.AddDays(idx).ToString("yyyy-MM-dd ") + label;
                        }
                        else
                        {
                            timeText = StartTime.AddHours(idx).ToString("yyyy-MM-dd HH:mm");
                        }

                        return $"时间: {timeText}\n数量: {qty}\n时长: {Math.Round(dur, 2)} s";
                    },
                };

                series.Title = $"{element.Key}";
                var points = new ChartValues<ScatterPoint>();
                foreach (var point in element.Value)
                {
                    // 使用 Weight 保存时长（秒），X 保存索引，Y 保存数量
                    points.Add(new ScatterPoint(point.Index, point.Count, point.Weight));
                }
                series.Values = points;
                SCollections.Add(series);
            }
        }


        [Obsolete]
        private void UpdateChart1(Dictionary<string, List<AlarmAnalyzeModel>> analyzeData)
        {
            SCollections = new SeriesCollection();
            for (int i = 0; i < analyzeData.Count; i++)
            {
                // 本地10种画笔根据 取余确定画笔
                var y = i % 10;
                var element = analyzeData.ElementAt(i);
                var series = new ScatterSeries()
                {
                    MinPointShapeDiameter = 15,
                    MaxPointShapeDiameter = 45,
                    Fill = _localBrushes[y],
                    Stroke = _localBrushes[y],
                    LabelPoint = point => $"{element.Key} \n\u6570\u91cf: {point.Y} \n\u65f6\u957f: {point.Weight}",
                };

                series.Title = $"{element.Key}";
                var points = new ChartValues<ScatterPoint>();
                foreach (var point in element.Value)
                {
                    points.Add(new ScatterPoint(point.Index, point.Count, point.Weight));
                }
                series.Values = points;
                SCollections.Add(series);
            }
        }

        /// <summary>
        /// 报警/日志页面切换
        /// </summary>
        private void SwitchMainView(bool isAlarmView)
        {
            if (IsAlarmView == isAlarmView)
            {
                // 已经是目标页面，仍强制刷新一次，避免UI数据不刷新
                if (isAlarmView)
                {
                    AlarmPageIndex = 1;
                    PageUpdated(new FunctionEventArgs<int>(1));
                }
                else
                {
                    LogPageIndex = 1;
                    PageUpdated(new FunctionEventArgs<int>(1));
                }
                return;
            }

            IsAlarmView = isAlarmView;
            SearchParas = string.Empty;

            var startTime = StartTime.AddHours(_firstClassTime.Hour).AddMinutes(_firstClassTime.Minute);
            var endTime = EndTime.AddHours(_firstClassTime.Hour).AddMinutes(_firstClassTime.Minute);

            if (IsAlarmView)
            {
                SelectedAlarmTab = AlarmSubTab.AlarmList;
                LoadAlarm(startTime, endTime, SearchParas);
            }
            else
            {
                LoadLog(startTime, endTime, SearchParas);
            }

            // 让一些基于 IsAlarmView 的可见性属性立即刷新
            RaisePropertyChanged(nameof(IsAlarmListVisible));
        }

        // 旧逻辑：保留为“切换”，但不再推荐用于双RadioButton
        private void ChangeView()
        {
            SwitchMainView(!IsAlarmView);
        }

        /// <summary>
        /// 加载报警
        /// </summary>
        public void LoadAlarm(DateTime startTime, DateTime endTime, string searchParas)
        {
            PageUpdated(new FunctionEventArgs<int>(1));
            AlarmPageIndex = 1;
        }

        /// <summary>
        /// 加载日志
        /// </summary>
        public void LoadLog(DateTime startTime, DateTime endTime, string searchParas)
        {
            PageUpdated(new FunctionEventArgs<int>(1));
            LogPageIndex = 1;
        }

        /// <summary>
        /// 获取报警单页数据
        /// </summary>
        /// <param name="info"></param>
        private void PageUpdated(FunctionEventArgs<int> info)
        {
            if (IsAlarmView)
            {
                UpdataAlarmPage(info.Info);
            }
            else
            {
                UpdataLogPage(info.Info);
            }
        }

        /// <summary>
        /// 更新报警
        /// </summary>
        /// <param name="pageIndex"></param>
        private void UpdataAlarmPage(int pageIndex)
        {
            long count = 0;
            var startTime = StartTime.AddHours(_firstClassTime.Hour).AddMinutes(_firstClassTime.Minute);
            var endTime = EndTime.AddHours(_firstClassTime.Hour).AddMinutes(_firstClassTime.Minute);

            // 统计Tab：不走分页列表（避免“切换没反应”）
            if (SelectedAlarmTab == AlarmSubTab.Statistics)
            {
                AlarmItemVMs = new ObservableCollection<AlarmItemModel>();
                AlarmPageCount = 1;
                return;
            }

            // 根据当前子Tab 决定过滤逻辑：
            // AlarmList(报警) = 只包含 alarmTypes；WarningList(警告) = 排除 alarmTypes
            var filterTypes = _alarmTypes.ToList();
            bool excludeTypes = SelectedAlarmTab == AlarmSubTab.WarningList;

            var listALarms = _dbManager.GetAlarmPageData(
                startTime,
                endTime,
                SearchParas,
                nameof(AlarmItemModel.Id),
                pageIndex,
                AlarmPerPageCount,
                out count,
                filterTypes,
                excludeTypes).ToList();

            var pageModels = new List<AlarmItemModel>();
            if (listALarms != null && listALarms.Count > 0)
            {
                foreach (var alarm in listALarms)
                {
                    pageModels.Add(new AlarmItemModel()
                    {
                        Id = alarm.ID,
                        Module = alarm.Module,
                        AlarmType = alarm.AlarmType,
                        AlarmLongTime = alarm.AlarmLongTime,
                        EndTime = alarm.EndTime == DateTime.Parse("0001/01/01 00:00:00") ? String.Empty : alarm.EndTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        StartTime = alarm.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        ProcMethod = alarm.ProcMethod,
                        Reason = alarm.Reason,
                        ProcUser = alarm.ProcUser,
                        AlarmCode = alarm.AlarmCode,
                    });
                }
            }

            AlarmItemVMs = new ObservableCollection<AlarmItemModel>(pageModels);

            if (count > 0)
            {
                AlarmPageCount = (int)(count % AlarmPerPageCount == 0 ? count / AlarmPerPageCount : count / AlarmPerPageCount + 1);
            }
            else
            {
                AlarmPageCount = 1;
            }
        }

        /// <summary>
        /// 更新日志
        /// </summary>
        /// <param name="pageIndex"></param>
        private void UpdataLogPage(int pageIndex)
        {
            long count = 0;
            var startTime = StartTime.AddHours(_firstClassTime.Hour).AddMinutes(_firstClassTime.Minute);
            var endTime = EndTime.AddHours(_firstClassTime.Hour).AddMinutes(_firstClassTime.Minute);
            var listLogs = _dbManager.GetOperationPageData(startTime, endTime, SearchParas, nameof(LogItemModel.Id), pageIndex, LogPerPageCount, out count).ToList();
            if (listLogs != null && listLogs.Count > 0)
            {
                LogItemVMs = new ObservableCollection<LogItemModel>();
                foreach (var log in listLogs)
                {
                    LogItemVMs.Add(new LogItemModel()
                    {
                        Id = log.ID,
                        Operation = log.Operation,
                        UserName = log.User,
                        CreateTime = log.CreateTime,
                    }); ;
                }
                if (count > 0)
                {
                    LogPageCount = (int)(count % LogPerPageCount == 0 ? count / LogPerPageCount : count / LogPerPageCount + 1);
                }
                else
                {
                    LogPageCount = 1;
                }
            }
            else
            {
                LogItemVMs = new ObservableCollection<LogItemModel>();
            }
        }

        /// <summary>
        /// Excel 导出
        /// </summary>
        private DelegateCommand _exportCommand;
        public DelegateCommand ExportCommand => _exportCommand ?? (_exportCommand = new DelegateCommand(() =>
        {
            var saveFile = new SaveFileDialog();
            saveFile.Filter = "CSV|*.csv";
            saveFile.FileName = "Alarm.csv";
            if (saveFile.ShowDialog() == true)
            {
                var baseFile = saveFile.FileName;
                SaveAlarmToCSVFile(StartTime, EndTime, SearchParas, baseFile);
            }
        }));

        private void SaveAlarmToCSVFile(DateTime startTime, DateTime endTime, string paras, string fileName)
        {
            // 统一使用“班别修正后”的时间范围
            var qStart = startTime.AddHours(_firstClassTime.Hour).AddMinutes(_firstClassTime.Minute);
            var qEnd = endTime.AddHours(_firstClassTime.Hour).AddMinutes(_firstClassTime.Minute);

            // 先在UI线程生成/快照 DownTimeSegments，避免后台线程改VM属性导致UI被清空
            List<DownTimeSegment> downTimeSnapshot = null;
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (DownTimeSegments == null || DownTimeSegments.Count == 0)
                {
                    try { BuildDownTimeChart(qStart, qEnd); } catch { }
                }

                downTimeSnapshot = (DownTimeSegments ?? new ObservableCollection<DownTimeSegment>()).ToList();
            });

            // 异步导出文件，不卡顿UI线程
            Task.Run(() =>
            {
                // 1) 取原始报警数据
                var all = _repository
                    .GetList<TbAlarm>(u => u.CreateTime >= qStart && u.CreateTime <= qEnd, r => r.ID, false)
                    .ToList();

                bool IsAlarmType(string t)
                {
                    if (string.IsNullOrWhiteSpace(t)) return false;
                    return _alarmTypes.Contains(t.Trim());
                }

                var alarmRows = all.Where(a => IsAlarmType(a.AlarmType)).ToList();
                var warningRows = all.Where(a => !IsAlarmType(a.AlarmType)).ToList();

                // 2) DownTime: 使用快照数据，不再触碰 VM 属性
                var downTimeRows = (downTimeSnapshot ?? new List<DownTimeSegment>())
                    .Select(s => new DownTimeBomExportRow
                    {
                        Day = s.Day,
                        Start = s.Start,
                        End = s.End,
                        Minutes = s.Minutes,
                        AlarmType = s.AlarmType,
                        Module = s.Module
                    })
                    .ToList();

                // 3) 输出三个文件：同目录、同文件名基底
                var dir = System.IO.Path.GetDirectoryName(fileName);
                var baseName = System.IO.Path.GetFileNameWithoutExtension(fileName);
                if (string.IsNullOrWhiteSpace(baseName)) baseName = "Export";

                var alarmFile = System.IO.Path.Combine(dir, baseName + "-Alarm.csv");
                var warningFile = System.IO.Path.Combine(dir, baseName + "-Warning.csv");
                var downTimeFile = System.IO.Path.Combine(dir, baseName + "-DownTime.csv");

                void WriteAlarmTable(string outputFile, IEnumerable<TbAlarm> src)
                {
                    var lines = new List<string>();
                    lines.Add("ID,CreateTime,Line,Machine,Module,AlarmCode,AlarmType,Reason,ProcMethod,AlarmLongTime,ProcUser,StartTime,EndTime");
                    foreach (var a in src)
                    {
                        lines.Add(string.Join(",", new[]
                        {
                            a.ID.ToString(),
                            a.CreateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            CsvEscape(a.Line),
                            CsvEscape(a.Machine),
                            CsvEscape(a.Module),
                            CsvEscape(a.AlarmCode),
                            CsvEscape(a.AlarmType),
                            CsvEscape(a.Reason),
                            CsvEscape(a.ProcMethod),
                            a.AlarmLongTime.ToString(),
                            CsvEscape(a.ProcUser),
                            a.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                            (a.EndTime == DateTime.Parse("0001/01/01 00:00:00") ? string.Empty : a.EndTime.ToString("yyyy-MM-dd HH:mm:ss"))
                        }));
                    }
                    System.IO.File.WriteAllLines(outputFile, lines, Encoding.UTF8);
                }

                // Alarm.csv
                WriteAlarmTable(alarmFile, alarmRows);

                // Warning.csv
                WriteAlarmTable(warningFile, warningRows);

                // DownTime.csv
                {
                    var lines = new List<string>();
                    lines.Add("Day,Start,End,Minutes,AlarmType,Module");
                    foreach (var r in downTimeRows)
                    {
                        lines.Add(string.Join(",", new[]
                        {
                            r.Day.ToString("yyyy-MM-dd"),
                            r.Start.ToString("yyyy-MM-dd HH:mm:ss"),
                            r.End.ToString("yyyy-MM-dd HH:mm:ss"),
                            r.Minutes.ToString("F2"),
                            CsvEscape(r.AlarmType),
                            CsvEscape(r.Module)
                        }));
                    }
                    System.IO.File.WriteAllLines(downTimeFile, lines, Encoding.UTF8);
                }
            });
        }

        private static string CsvEscape(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            // 需要转义逗号/引号/换行
            if (s.Contains(",") || s.Contains("\"") || s.Contains("\r") || s.Contains("\n"))
            {
                return "\"" + s.Replace("\"", "\"\"") + "\"";
            }
            return s;
        }

        private class DownTimeBomExportRow
        {
            public DateTime Day { get; set; }
            public DateTime Start { get; set; }
            public DateTime End { get; set; }
            public double Minutes { get; set; }
            public string AlarmType { get; set; }
            public string Module { get; set; }
        }

        #region 导航接口实现
        public void OnNavigatedTo(NavigationContext navigationContext)
        {

        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return false;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }
        #endregion

        #region 统计信息(报警/警告汇总)

        public class AlarmStatisticsRowModel
        {
            public int Index { get; set; }

            /// <summary>
            /// 报警/警告
            /// </summary>
            public string Category { get; set; }

            public string AlarmType { get; set; }
            public string Module { get; set; }

            public int Count { get; set; }

            /// <summary>
            /// 总时长(min)。注意：宕机时长只统计“报警类型”，警告不计入。
            /// </summary>
            public double TotalMinutes { get; set; }

            public double AvgMinutes { get; set; }

            public double MaxMinutes { get; set; }

            /// <summary>
            /// 按“报警类型”总宕机时长计算占比(0-100)。警告行占比固定为0。
            /// </summary>
            public double PercentByAlarmDownTime { get; set; }
        }

        private ObservableCollection<AlarmStatisticsRowModel> _alarmStatisticsRows;
        public ObservableCollection<AlarmStatisticsRowModel> AlarmStatisticsRows
        {
            get { return _alarmStatisticsRows; }
            set { SetProperty(ref _alarmStatisticsRows, value); }
        }

        #endregion

        #region 报警Tab(报警/警告/统计/DownTime/DownTimeBom)

        private enum AlarmSubTab
        {
            AlarmList,
            WarningList,
            Statistics,
            DownTime,
            DownTimeBom
        }

        private AlarmSubTab _selectedAlarmTab = AlarmSubTab.AlarmList;

        /// <summary>
        /// 当前选中的子Tab
        /// </summary>
        private AlarmSubTab SelectedAlarmTab
        {
            get { return _selectedAlarmTab; }
            set
            {
                if (SetProperty(ref _selectedAlarmTab, value))
                {
                    RaisePropertyChanged(nameof(IsAlarmTab));
                    RaisePropertyChanged(nameof(IsWarningTab));
                    RaisePropertyChanged(nameof(IsStatisticsTab));
                    RaisePropertyChanged(nameof(IsDownTimeTab));
                    RaisePropertyChanged(nameof(IsDownTimeBomTab));
                    RaisePropertyChanged(nameof(IsAlarmListVisible));
                    RaisePropertyChanged(nameof(IsDownTimeVisible));
                    RaisePropertyChanged(nameof(IsDownTimeBomVisible));

                    if (IsAlarmView)
                    {
                        var startTime = StartTime.AddHours(_firstClassTime.Hour).AddMinutes(_firstClassTime.Minute);
                        var endTime = EndTime.AddHours(_firstClassTime.Hour).AddMinutes(_firstClassTime.Minute);

                        if (SelectedAlarmTab == AlarmSubTab.Statistics)
                        {
                            BuildStatistics(startTime, endTime);
                            AlarmItemVMs = new ObservableCollection<AlarmItemModel>();
                            AlarmPageCount = 1;
                            AlarmPageIndex = 1;
                        }
                        else if (SelectedAlarmTab == AlarmSubTab.DownTime)
                        {
                            BuildDownTimeChart(startTime, endTime);
                            AlarmItemVMs = new ObservableCollection<AlarmItemModel>();
                            AlarmPageCount = 1;
                            AlarmPageIndex = 1;
                        }
                        else if (SelectedAlarmTab == AlarmSubTab.DownTimeBom)
                        {
                            // TowntimeBom 依赖 DownTimeSegments
                            BuildDownTimeChart(startTime, endTime);
                            AlarmItemVMs = new ObservableCollection<AlarmItemModel>();
                            AlarmPageCount = 1;
                            AlarmPageIndex = 1;
                        }
                        else
                        {
                            AlarmPageIndex = 1;
                            PageUpdated(new FunctionEventArgs<int>(1));
                        }
                    }
                }
            }
        }

        public bool IsAlarmTab
        {
            get { return SelectedAlarmTab == AlarmSubTab.AlarmList; }
            set
            {
                if (value) SelectedAlarmTab = AlarmSubTab.AlarmList;
            }
        }

        public bool IsWarningTab
        {
            get { return SelectedAlarmTab == AlarmSubTab.WarningList; }
            set
            {
                if (value) SelectedAlarmTab = AlarmSubTab.WarningList;
            }
        }

        public bool IsStatisticsTab
        {
            get { return SelectedAlarmTab == AlarmSubTab.Statistics; }
            set
            {
                if (value) SelectedAlarmTab = AlarmSubTab.Statistics;
            }
        }

        public bool IsDownTimeTab
        {
            get { return SelectedAlarmTab == AlarmSubTab.DownTime; }
            set
            {
                if (value) SelectedAlarmTab = AlarmSubTab.DownTime;
            }
        }

        public bool IsDownTimeBomTab
        {
            get { return SelectedAlarmTab == AlarmSubTab.DownTimeBom; }
            set
            {
                if (value) SelectedAlarmTab = AlarmSubTab.DownTimeBom;
            }
        }

        public bool IsAlarmListVisible
        {
            get { return IsAlarmView && (SelectedAlarmTab == AlarmSubTab.AlarmList || SelectedAlarmTab == AlarmSubTab.WarningList); }
        }

        public bool IsDownTimeVisible
        {
            get { return IsAlarmView && SelectedAlarmTab == AlarmSubTab.DownTime; }
        }

        public bool IsDownTimeBomVisible
        {
            get { return IsAlarmView && SelectedAlarmTab == AlarmSubTab.DownTimeBom; }
        }

        /*
         * NOTE:
         * 该文件历史上曾存在多段重复的“报警Tab(报警/警告/统计/DownTime...)”实现。
         * 为避免重复定义(enum/字段/属性)导致编译错误与行为不一致，
         * 统一保留文件后部的完整实现（含 DownTimeBom），此处不再保留旧实现。
         */

        #endregion

        // 报警分类定义：警告提示、设备异常、回零异常、运行超时、PLC报警 归类为“报警”；其它归类为“警告”
        private static readonly HashSet<string> _alarmTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            AlarmType.WarningTip.GetDescription(),
            AlarmType.DeviceError.GetDescription(),
            AlarmType.HomeError.GetDescription(),
            AlarmType.Timeout.GetDescription(),
            AlarmType.FailError.GetDescription(),
            AlarmType.PlcAlarm.GetDescription(),
        };

        [Obsolete("Use _alarmTypes instead.")]
        private static readonly HashSet<string> _stopAlarmTypes = _alarmTypes;

        private static bool IsStopAlarm(AlarmItemModel alarm)
        {
            if (alarm == null) return false;
            if (string.IsNullOrWhiteSpace(alarm.AlarmType)) return false;
            return _alarmTypes.Contains(alarm.AlarmType.Trim());
        }

        private DateTime _chartStart;
        private enum ChartMode { Day, Week, Month }
        private ChartMode _chartMode = ChartMode.Day;

        // 新增公有方法：根据点击的类型和索引筛选并更新右侧表格（AlarmItemVMs）
        public void FilterAlarmsByTypeAndIndex(string alarmType, int index)
        {
            try
            {
                DateTime begin;
                DateTime end;
                switch (_chartMode)
                {
                    case ChartMode.Day:
                        begin = _chartStart.AddHours(index);
                        end = begin.AddHours(1);
                        break;
                    case ChartMode.Week:
                        begin = _chartStart.AddDays(index);
                        end = begin.AddDays(1);
                        break;
                    case ChartMode.Month:
                    default:
                        begin = _chartStart.AddDays(index);
                        end = begin.AddDays(1);
                        break;
                }

                var query = _dbManager.GetAnalyzeModels(begin, end);
                var alarms = query
                    .Where(a => string.Equals((a.AlarmType ?? string.Empty), (alarmType ?? string.Empty), StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(a => a.CreateTime)
                    .ToList();

                var pageModels = new List<AlarmItemModel>();
                foreach (var alarm in alarms)
                {
                    pageModels.Add(new AlarmItemModel()
                    {
                        Id = alarm.ID,
                        Module = alarm.Module,
                        AlarmType = alarm.AlarmType,
                        AlarmLongTime = alarm.AlarmLongTime,
                        EndTime = alarm.EndTime == DateTime.Parse("0001/01/01 00:00:00") ? String.Empty : alarm.EndTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        StartTime = alarm.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        ProcMethod = alarm.ProcMethod,
                        Reason = alarm.Reason,
                        ProcUser = alarm.ProcUser,
                        AlarmCode = alarm.AlarmCode,
                    });
                }

                CurrentDayAlarmVMs = new ObservableCollection<AlarmItemModel>(pageModels);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"FilterAlarmsByTypeAndIndex error: {ex}");
            }
        }

        private void BuildStatistics(DateTime startTime, DateTime endTime)
        {
            try
            {
                var alarms = _dbManager.GetAnalyzeModels(startTime, endTime)?.ToList() ?? new List<TbAlarm>();

                var valid = alarms
                    .Where(a => !string.IsNullOrWhiteSpace(a.AlarmType))
                    .ToList();

                double GetDownMinutes(TbAlarm a)
                {
                    if (a == null) return 0;

                    try
                    {
                        if (a.EndTime > a.StartTime)
                        {
                            var m = (a.EndTime - a.StartTime).TotalMinutes;
                            if (m > 0) return m;
                        }
                    }
                    catch
                    {
                        // ignore
                    }

                    var v = a.AlarmLongTime;
                    if (v <= 0) return 0;

                    if (v >= 60 * 1000)
                    {
                        return v / 60000.0;
                    }
                    if (v >= 60)
                    {
                        return v / 60.0;
                    }
                    return v / 60000.0;
                }

                double totalAlarmDownMinutes = valid
                    .Where(a => _alarmTypes.Contains((a.AlarmType ?? string.Empty).Trim()))
                    .Sum(GetDownMinutes);

                var rows = valid
                    .GroupBy(a => new
                    {
                        Type = (a.AlarmType ?? string.Empty).Trim(),
                        Module = (a.Module ?? string.Empty).Trim()
                    })
                    .Select(g =>
                    {
                        var isAlarm = _alarmTypes.Contains(g.Key.Type);
                        var category = isAlarm ? "报警" : "警告";

                        var minuteList = isAlarm
                            ? g.Select(GetDownMinutes).Where(m => m > 0).ToList()
                            : new List<double>();

                        var totalMin = minuteList.Sum();
                        var avgMin = minuteList.Count > 0 ? minuteList.Average() : 0;
                        var maxMin = minuteList.Count > 0 ? minuteList.Max() : 0;

                        var percent = (isAlarm && totalAlarmDownMinutes > 0)
                            ? (totalMin / totalAlarmDownMinutes * 100.0)
                            : 0;

                        return new AlarmStatisticsRowModel
                        {
                            Category = category,
                            AlarmType = g.Key.Type,
                            Module = g.Key.Module,
                            Count = g.Count(),
                            TotalMinutes = Math.Round(totalMin, 2),
                            AvgMinutes = Math.Round(avgMin, 2),
                            MaxMinutes = Math.Round(maxMin, 2),
                            PercentByAlarmDownTime = Math.Round(percent, 2),
                        };
                    })
                    .OrderByDescending(r => r.TotalMinutes)
                    .ThenByDescending(r => r.Count)
                    .ThenBy(r => r.Category)
                    .ToList();

                for (int i = 0; i < rows.Count; i++)
                {
                    rows[i].Index = i + 1;
                }

                AlarmStatisticsRows = new ObservableCollection<AlarmStatisticsRowModel>(rows);

                // 统计页不应主动重建 DownTime 图（避免覆盖 DownTimeTab 的 RowSeries）
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"BuildStatistics error: {ex}");
                AlarmStatisticsRows = new ObservableCollection<AlarmStatisticsRowModel>();
            }
        }


        public string GetAlarmInfo(double minutes, int dayIndex)
        {
            try
            {
                if (double.IsNaN(minutes)) return null;

                // 构建日期列表：优先使用 DownTimeLabels（若存在），否则从 DownTimeSegments 派生
                List<DateTime> days = null;

                if (DownTimeLabels != null && DownTimeLabels.Length > 0)
                {
                    days = new List<DateTime>(DownTimeLabels.Length);
                    foreach (var s in DownTimeLabels)
                    {
                        if (DateTime.TryParse(s, out var d))
                            days.Add(d.Date);
                        else
                            days.Add(DateTime.MinValue); // 占位，后续会检测索引范围
                    }
                }
                else if (DownTimeSegments != null && DownTimeSegments.Count > 0)
                {
                    days = DownTimeSegments
                        .Select(s => s.Day.Date)
                        .Distinct()
                        .OrderBy(d => d)
                        .ToList();
                }
                else
                {
                    return null;
                }

                if (dayIndex < 0 || dayIndex >= days.Count) return null;
                var day = days[dayIndex];
                if (day == DateTime.MinValue) return null;

                // Clamp minutes 到合理范围（0..1440）
                var mins = Math.Max(0, Math.Min(1440, minutes));
                var time = TimeSpan.FromMinutes(mins);

                if (DownTimeSegments == null || DownTimeSegments.Count == 0) return null;

                var segment = DownTimeSegments.FirstOrDefault(s =>
                    s.Day.Date == day.Date &&
                    time >= s.Start.TimeOfDay &&
                    time <= s.End.TimeOfDay);

                if (segment != null)
                {
                    return $"{segment.AlarmType}\n{segment.Start:dd-HH:mm} - {segment.End:dd-HH:mm}\n{segment.Module}";
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
