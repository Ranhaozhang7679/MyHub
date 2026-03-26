#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ChartConfigureContentVM
* 机器名称:       L05590
* 命名空间:       Luster.Motion.SubSystem.ViewModel
* 文 件 名:       ChartConfigureContentVM.cs
* 创建时间:       2022/10/24 18:21:29
* 作    者:       L05590
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       fanlu5590@lusterinc.com 
* 唯一标识：      d13d682c-7958-4ef5-ba2a-202242cd8422
* 登录用户:       fanlu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/10/24 18:21:29
* 修 改 人:		  L05590
************************************************************************************/
#endregion

using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.CommonUI.Models;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.EditorUI.Extensions;
using Luster.Motion.CommonUI.Dock;
using Luster.Motion.TaskFlow.Engine;
using Luster.Motion.TaskFlow.Engine.Models;
using Prism.Commands;
using Prism.Events;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Windows.Threading;

using SkiaSharp;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Luster.TaskFlow.Motion.Logic;
using System.Timers;
using System.Windows;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Enums;
using Luster.Motion.DataStruct;

namespace Luster.Motion.SubSystem.ViewModel
{
    public class ChartCopyContentVM : MotionVM, IDockContent
    {
        private ICommonBus _commonBus;

        private IDialogService _dialogService;

        private IMotionController _motionController;

        private IMotionEngine _motionEngine;



        private bool _isOutPutChange = false;

        // UI线程
        private Dispatcher _dispatcher;

        Dictionary<string, ObservableCollection<double>> dicChart = new Dictionary<string, ObservableCollection<double>>();

        private List<SKColor> listColor;//报表曲线颜色

        private List<MapData> _MapDatas;


        /// <summary>
        /// 数据源
        /// </summary>
        private ObservableCollection<ChartModel> _chartList;
        public ObservableCollection<ChartModel> ChartList
        {
            get { return _chartList; }
            set => SetProperty(ref _chartList, value);
        }

        List<ChartDataModel> chartdataList;

        /// <summary>
        /// Chart个数
        /// </summary>
        private int _chartCount;
        public int ChartCount
        {
            get { return _chartCount; }
            set => SetProperty(ref _chartCount, value);
        }

        /// <summary>
        /// 是否显示
        /// </summary>
        private bool _isVisible = false;
        public bool IsVisible
        {
            get { return _isVisible; }
            set => SetProperty(ref _isVisible, value);
        }

        /// <summary>
        /// 启动状态按钮不可点击
        /// </summary>
        private bool _btnEnable = true;
        public bool BtnEnable
        {
            get { return _btnEnable; }
            set { SetProperty(ref _btnEnable, value); }
        }

        /// <summary>
        /// x轴标签样式
        /// </summary>
        private Axis[] _xAxes;
        public Axis[] XAxes
        {
            get { return _xAxes; }
            set { SetProperty(ref _xAxes, value); }
        }

        /// <summary>
        /// y轴标签样式
        /// </summary>
        private Axis[] _yAxes;
        public Axis[] YAxes
        {
            get { return _yAxes; }
            set { SetProperty(ref _yAxes, value); }
        }


        System.Timers.Timer aTimer ;






        public ChartCopyContentVM()
        {
        }

        public ChartCopyContentVM(ICommonBus commonBus, IDialogService dialogService, IMotionController motionController, Dispatcher dispatcher, IMotionController mController, IMotionEngine motionEngine) : base(commonBus)
        {
            _dialogService = dialogService;
            _commonBus = commonBus;
            _motionController = motionController;
            _motionEngine = motionEngine;
            _dispatcher = dispatcher;
            ChartList = new ObservableCollection<ChartModel>();
            InitListColor();
            InitData();
            _motionController.ProductEvent -= _motionController_ProductEvent;
            _motionController.ProductEvent += _motionController_ProductEvent;

            aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new ElapsedEventHandler(timer_Elapsed); ;
            aTimer.Interval = 10;  //设置时间间隔
            aTimer.Enabled = true;


        }


        /// <summary>
        /// 注册事件
        /// </summary>
        /// <param name="bus"></param>
        protected override void RegisterEvent(IEventAggregator bus)
        {
            bus.GetEvent<MapUpdateEvent>().Subscribe((mapdatas) =>
            {
                _isOutPutChange = true;
                _MapDatas = mapdatas;
            });
            bus.GetEvent<SysOperationBtnEvent>().Subscribe((btnobj) =>
            {
                if (btnobj == DataStruct.Enums.SystemOperation.Start)
                {
                    BtnEnable = false;
                }
                else
                {
                    BtnEnable = true;
                }
            });

            bus.GetEvent<ProjectChangeEvent>().Subscribe((mapdatas) =>
            {
                _dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    InitData();
                }));
            });
        }

        private void InitData()
        {
            _MapDatas = _motionEngine.MapDatas;
            chartdataList = _commonBus.LoadChartList();
            XAxes = new Axis[1];
            YAxes = new Axis[1];
            InitChart(chartdataList);
        }

        /// <summary>
        /// 设置Chart
        /// </summary>
        private DelegateCommand _setCommand;
        public DelegateCommand SetCommand => _setCommand ?? (_setCommand = new DelegateCommand(() =>
        {

            _dialogService.ShowChartVASet(chartdataList, (r) =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    chartdataList = r.Parameters.GetValue<List<ChartDataModel>>("ChartDataList");
                    if (chartdataList != null)
                    {
                        SaveConfig(chartdataList);
                        InitChart(chartdataList);
                    }
                }
            });
        }));

        /// <summary>
        /// 初始化sr颜色
        /// </summary>
        private void InitListColor()
        {
            listColor = new List<SKColor>();
            listColor.Add(new(199, 21, 133));
            listColor.Add(new(0, 255, 0));
            listColor.Add(new(255, 255, 0));
            listColor.Add(new(147, 112, 219));
            listColor.Add(new(0, 255, 255));
        }

        /// <summary>
        /// 输出项改变更新输出参数
        /// </summary>
        private void UpdateOutPut(List<MapData> mapdatas)
        {
            _MapDatas = mapdatas;
            chartdataList = _commonBus.LoadChartList();
            InitChart(chartdataList);
        }

        /// <summary>
        /// 初始化Chart
        /// </summary>
        /// <param name="chartDataList"></param>
        private void InitChart(List<ChartDataModel> chartDataList)
        {
            ChartList.Clear();
            if (chartDataList.Count > 0)
            {
                IsVisible = false;
                dicChart.Clear();
                ChartCount = chartDataList.Count;

                foreach (var item in chartDataList)
                {
                    //创建series
                    Axis axis = new Axis()
                    {
                        TextSize = 12,
                        MinStep = 1,
                        ForceStepToMin = true,
                        ShowSeparatorLines = true,
                    };
                    XAxes[0] = axis;
                    Axis yaxis = new Axis()
                    {
                        MinStep = item.SpaceFactor,
                        ForceStepToMin = false,
                        TextSize = 12,
                        ShowSeparatorLines = true,
                    };
                    YAxes[0] = yaxis;

                    var solidColorPaint = new SolidColorPaint()
                    {
                        SKTypeface = SKTypeface.FromFamilyName("微软雅黑"),
                        SKFontStyle = SKFontStyle.Normal,
                        Color = new SKColor(50, 50, 50),
                    };
                    var tooltipTextPaint = new SolidColorPaint()
                    {
                        SKTypeface = SKTypeface.FromFamilyName("微软雅黑"),
                        SKFontStyle = SKFontStyle.Normal,
                        Color = new SKColor(50, 50, 50),
                    };

                    var model = new ChartModel();
                    model.Name = item.Name;
                    model.Width = item.Width;
                    model.Height = item.Height;
                    model.SpaceFactor = item.SpaceFactor;
                    model.XAxes = XAxes;
                    model.YAxes = YAxes;
                    model.TooltipTextPaint = tooltipTextPaint;
                    model.LegendTextPaint = solidColorPaint;
                    if (item.SelectList != null && item.SelectList.Count > 0)
                    {
                        ISeries[] srs = new ISeries[item.SelectList.Count + 3];
                        for (int i = 0; i < item.SelectList.Count; i++)
                        {
                            if (item.SelectList != null)
                            {
                                ObservableCollection<double> value = new ObservableCollection<double>();
                                LineSeries<double> sr = new LineSeries<double>
                                {
                                    Name = item.SelectList[i].AliasName,
                                    Values = value,
                                    LineSmoothness = 0,
                                    Fill = null,
                                    GeometrySize = 5,
                                    Stroke = new SolidColorPaint(listColor[i % 5], 1),

                                };
                                if (!dicChart.ContainsKey(item.Name + "-" + item.SelectList[i].Source))
                                {
                                    dicChart.Add(item.Name + "-" + item.SelectList[i].Source, value);
                                    srs[i] = sr;
                                }
                            }
                        }
                        CreateChartSeries("标准值", srs);
                        CreateChartSeries("上限", srs);
                        CreateChartSeries("下限", srs);
                        var output = _MapDatas.FirstOrDefault(x => x.Alias == item.SelectList[0].AliasName);
                        if (output != null)
                        {
                            model.UpperLimit = Math.Round(output.StandardValue + output.PositivestandardDeviation, 4);
                            model.LowerLimit = Math.Round(output.StandardValue - output.NegativestandardDeviation, 4);
                            model.StandardValue = Math.Round(output.StandardValue, 4);
                            model.SrSeries = srs;
                            model.YFormatter = value => value.ToString("G");
                        }
                    }
                    ChartList.Add(model);
                }
            }
            else
            {
                IsVisible = true;
            }
        }

        private void CreateChartSeries(string seriesName, ISeries[] srs)
        {
            ObservableCollection<double> value = new ObservableCollection<double>();
            SKColor srColor = new SKColor();
            int index = 0;
            switch (seriesName)
            {
                case "标准值":
                    srColor = new(34, 139, 34);
                    index = srs.Length - 3;
                    break;
                case "上限":
                    srColor = new(255, 0, 0);
                    index = srs.Length - 2;
                    break;
                case "下限":
                    srColor = new(255, 165, 0);
                    index = srs.Length - 1;
                    break;
            }
            LineSeries<double> sr = new LineSeries<double>
            {
                Name = seriesName,
                Values = value,
                LineSmoothness = 0,
                GeometrySize = 5,
                Fill = null,
                Stroke = new SolidColorPaint(srColor, 1),
            };
            srs[index] = sr;
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        /// <param name="chartDataList"></param>
        private void SaveConfig(List<ChartDataModel> chartDataList)
        {
            if (commonBus.ProjInfo != null & commonBus.ProjInfo.ProjPath != "")
            {
                string chartConfig = Path.Combine(Path.GetDirectoryName(commonBus.ProjInfo.FullName), "Config", "ChartConfig.xml");
                _commonBus.SaveChartConfig(chartConfig, chartDataList);
                commonBus.IsNeedSave = false;
            }
        }

       
        /// <summary>
        /// 接收数据,根据出料事件
        /// </summary>
        /// <param name="pInfo"></param>
        /// <param name="enter"></param>
        private void _motionController_ProductEvent(ProductInfo pInfo, bool enter, double ct)
        {

            RefreshChart( pInfo,  enter);
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ProductInfo product=new ProductInfo();
            RefreshChart(product, true);
        }




        private void RefreshChart(ProductInfo pInfo, bool enter)
        {
            if (_motionEngine.EngineStatus != EngineStatus.Running)
                return;
            _dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                if (_isOutPutChange)
                {
                    InitData();
                    _isOutPutChange = false;
                }
                foreach (var item in ChartList)
                {
                    if (item.SrSeries == null) continue;
                    foreach (var sritem in item.SrSeries)
                    {
                        var srmodel = sritem as LineSeries<double>;
                        if (srmodel != null)
                        {
                            var listValue = srmodel.Values.ToList();
                            if (listValue.Count >= 20)
                            {
                                listValue.RemoveAt(0);
                            }
                            if (srmodel.Name == "标准值")
                            {
                                listValue.Add(Math.Round(item.StandardValue, 4));
                            }
                            else if (srmodel.Name == "上限")
                            {
                                listValue.Add(Math.Round(item.UpperLimit, 4));
                            }
                            else if (srmodel.Name == "下限")
                            {
                                listValue.Add(Math.Round(item.LowerLimit, 4));
                            }
                            else if(pInfo.Data!=null)
                            {
                                 if (pInfo.Data.ContainsKey(srmodel.Name))
                                {
                                    item.YAxes[0].ForceStepToMin = true;
                                    listValue.Add(Math.Round(Convert.ToDouble(pInfo.Data[srmodel.Name]), 4));
                                }
                                ObservableCollection<double> observalue = new ObservableCollection<double>();
                                listValue.ForEach(m => observalue.Add(m));
                                srmodel.Values = observalue;
                            }
                            else if(pInfo.Data == null && chartdataList[ChartList.IndexOf(item)].IsRealTime)
                            {
                                var globalModule = _motionEngine.Get(GlobalModule.GlobalID);
                                foreach (var item2 in globalModule.Parameters)
                                {
                                    if (item2.Key ==$"Extend_{srmodel.Name}" )
                                    {
                                        if (item2.Value != null)
                                        {
                                            //item.YAxes[0].ForceStepToMin = true;
                                            listValue.Add(Math.Round(Convert.ToDouble(item2.Value.Value), 4));

                                        }
                                    }
                                }

                                ObservableCollection<double> observalue = new ObservableCollection<double>();
                                listValue.ForEach(m => observalue.Add(m));
                                srmodel.Values = observalue;
                            }

                            //else if (pInfo.Data.ContainsKey(srmodel.Name))
                            //{
                            //    item.YAxes[0].ForceStepToMin = true;
                            //    listValue.Add(Math.Round(Convert.ToDouble(pInfo.Data[srmodel.Name]), 4));
                            //}

                        }
                    }
                }
            }));
        }

        //定时刷新


        #region DockContent
        /// <summary>
        /// 对应的Key
        /// </summary>
        public string Name => "ReportForm";

        /// <summary>
        /// 对应的区域
        /// </summary>
        public string RegionName { get; set; } = "ChartCopyContent";
        #endregion
    }
}