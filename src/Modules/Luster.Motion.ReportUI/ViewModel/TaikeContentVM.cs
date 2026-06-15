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
using TaiKeCommon;
using System.Windows.Controls;
using LiveChartsCore.SkiaSharpView.WPF;
using LiveChartsCore.Defaults;

using SkiaSharp.Views.WPF;
//using LiveCharts.Wpf;
using LiveCharts;
using Luster.Common.Tools;
using Luster.Motion.ReportUI.Model;
using System.Windows.Forms;
using Luster.Common.DataAccess.Repositories;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.Drawing;
using AxisPosition = LiveChartsCore.Measure.AxisPosition;
using LiveCharts.Wpf.Charts.Base;
using LiveChartsCore.SkiaSharpView.VisualElements;
//using Luster.SimDevice.SubSystem.Events;

namespace Luster.Motion.ReportUI.ViewModel
{
    public class TaikeContentVM : ReportBaseVM
    {

        public override string ReportName => "TorqueChart";

        private ICommonBus _commonBus;

        private IDialogService _dialogService;

        private IMotionController _motionController;

        private IMotionEngine _motionEngine;

        // UI线程
        private Dispatcher _dispatcher;

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

        // 界面的控件集合，需要传递给扭力类使用
        object[] screwControls = null;
        object[] pressControls = null;
        object[] torqueControls = null;

        object[] xForceControls = null;
        object[] yForceControls = null;
        object[] zForceControls = null;



        private double _torqueUp = 0.4;

        public double TorqueUp
        {
            get => _torqueUp;
            set => SetProperty(ref _torqueUp, value);
        }


        private double _torqueLow = 0.35;

        public double TorqueLow
        {
            get => _torqueLow;
            set => SetProperty(ref _torqueLow, value);
        }

        private double _angleUp = 2000;

        public double AngleUp
        {
            get => _angleUp;
            set => SetProperty(ref _angleUp, value);
        }


        private double _angleLow = 700;

        public double AngleLow
        {
            get => _angleLow;
            set => SetProperty(ref _angleLow, value);
        }





        LineSeries<ObservablePoint> torq_lineF = new LineSeries<ObservablePoint>();       //上限线
        LineSeries<ObservablePoint> torq_lineSSL = new LineSeries<ObservablePoint>();     //SSL着座点
        LineSeries<ObservablePoint> torq_pointMinSeat = new LineSeries<ObservablePoint>();     //最小着座扭矩

        Axis axis_x = new Axis(); // x轴
        Axis axis_y = new Axis(); // y轴


        Axis axis_x2 = new Axis(); // x轴
        Axis axis_y2 = new Axis(); // y轴
        Axis axis_y2y = new Axis(); // y轴


        Axis axis_xx = new Axis(); // x轴
        Axis axis_yy = new Axis(); // y轴

        Axis axis_xp = new Axis(); // x轴
        Axis axis_yp = new Axis(); // y轴


        Axis axis_XForce_X = new Axis();//X方向力_X轴
        Axis axis_XForce_Y = new Axis();//X方向力_Y轴
        Axis axis_XForce_YY = new Axis();//X方向力_Y轴


        Axis axis_YForce_X = new Axis();//Y方向力_X轴
        Axis axis_YForce_Y = new Axis();//Y方向力_Y轴

        Axis axis_YForce_YY = new Axis();//Y方向力_Y轴

        Axis axis_ZForce_X = new Axis();//Z方向力_X轴
        Axis axis_ZForce_Y = new Axis();//Z方向力_Y轴
        Axis axis_ZForce_YY = new Axis();//Z方向力_Y轴



        private IMotionController _mController;


        public TaikeContentVM()
        {
            //((CartesianChart)screwControls[0]).Series = null;
            //((CartesianChart)pressControls[0]).Series = null;
            //((CartesianChart)torqueControls[0]).Series = null;
            //((CartesianChart)xForceControls[0]).Series = null;
            //((CartesianChart)yForceControls[0]).Series = null;
            //((CartesianChart)zForceControls[0]).Series = null;
        }

        public TaikeContentVM(IRepository reporitory, IMotionController motionController, Dispatcher dispatcher) : base(reporitory, motionController)
        {
            _mController = motionController;
            _dispatcher = dispatcher;
        }

        ElectricScrewDrivers driver;
        string name = "";

        protected override void RegisterEvent(IEventAggregator bus)
        {
            bus.GetEvent<TorqueRegisterEvent>().Subscribe(screwDriver =>
            {
                CartesianChart AngleTorqueChart = (CartesianChart)screwControls[0];
                CartesianChart TimePressChart = (CartesianChart)pressControls[0];
                CartesianChart TimeAngleChart = (CartesianChart)torqueControls[0];
                CartesianChart XForceChart = (CartesianChart)xForceControls[0];
                CartesianChart YForceChart = (CartesianChart)yForceControls[0];
                CartesianChart ZForceChart = (CartesianChart)zForceControls[0];
            });
        }




        /// <summary>
        /// 设置Chart
        /// </summary>
        private DelegateCommand _setCommand;
        public DelegateCommand SetCommand => _setCommand ?? (_setCommand = new DelegateCommand(() =>
        {

        }));

        private DelegateCommand<object> _getChartControls;
        public DelegateCommand<object> GetChartControls => _getChartControls ?? (_getChartControls = new DelegateCommand<object>((items) =>
        {
            AssignControls(ref screwControls, items, nameof(GetChartControls));
        }));

        private DelegateCommand<object> _getPressChartControls;
        public DelegateCommand<object> GetPressChartControls => _getPressChartControls ?? (_getPressChartControls = new DelegateCommand<object>((items) =>
        {
            AssignControls(ref pressControls, items, nameof(GetPressChartControls));
        }));

        private DelegateCommand<object> _getTorqueChartControls;
        public DelegateCommand<object> GetTorqueChartControls => _getTorqueChartControls ?? (_getTorqueChartControls = new DelegateCommand<object>((items) =>
        {
            AssignControls(ref torqueControls, items, nameof(GetTorqueChartControls));
        }));

        private DelegateCommand<object> _getToeinXForceChartControls;
        public DelegateCommand<object> GetToeinXForceChartControls => _getToeinXForceChartControls ?? (_getToeinXForceChartControls = new DelegateCommand<object>((items) =>
        {
            AssignControls(ref xForceControls, items, nameof(GetToeinXForceChartControls));
        }));

        private DelegateCommand<object> _getToeinYForceChartControls;
        public DelegateCommand<object> GetToeinYForceChartControls => _getToeinYForceChartControls ?? (_getToeinYForceChartControls = new DelegateCommand<object>((items) =>
        {
            AssignControls(ref yForceControls, items, nameof(GetToeinYForceChartControls));
        }));

        private DelegateCommand<object> _getToeinZForceChartControls;
        public DelegateCommand<object> GetToeinZForceChartControls => _getToeinZForceChartControls ?? (_getToeinZForceChartControls = new DelegateCommand<object>((items) =>
        {
            AssignControls(ref zForceControls, items, nameof(GetToeinZForceChartControls));
        }));

        // 通用赋值与初始化
        private void AssignControls(ref object[] target, object items, string caller)
        {
            try
            {
                // 调试日志：记录 items 类型（运行时可改为 proper logging）
                System.Diagnostics.Trace.WriteLine($"AssignControls called by {caller}, items type: {(items == null ? "null" : items.GetType().FullName)}");

                if (items == null)
                {
                    target = null;
                }
                else if (items is object[] arr)
                {
                    target = arr;
                }
                else
                {
                    // 如果传来单个控件，封装成数组
                    target = new object[] { items };
                }

                InitializeChartsIfReady();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"AssignControls error: {ex}");
            }
        }

        // 更完整的初始化，处理所有 chart controls
        private void InitializeChartsIfReady()
        {
            if (_dispatcher == null) _dispatcher = System.Windows.Application.Current?.Dispatcher;

            _dispatcher?.Invoke(() =>
            {
                Action<object[]> init = arr =>
                {
                    if (arr == null || arr.Length == 0) return;
                    if (arr[0] == null) return;
                    if (arr[0] is CartesianChart chart)
                    {
                        if (chart.Series == null) chart.Series = new List<ISeries>();
                    }
                };

                init(screwControls);
                init(pressControls);
                init(torqueControls);
                init(xForceControls);
                init(yForceControls);
                init(zForceControls);
            });
        }






        #region DockContent
        /// <summary>
        /// 对应的Key
        /// </summary>
        public string Name => "TorqueForm";

        /// <summary>
        /// 对应的区域
        /// </summary>
        public string RegionName { get; set; } = "ChartTorqueContent";
        #endregion



        /// <summary>
        /// 导入数据
        /// </summary>
        [Obsolete]
        protected override void Import()
        {
            torq_lineF.Stroke = new SolidColorPaint(Colors.Yellow.ToSKColor(), 1);  //定义颜色
            torq_lineF.LineSmoothness = 0;
            torq_lineF.Fill = new SolidColorPaint(Colors.Transparent.ToSKColor(), 1);
            torq_lineF.GeometrySize = 1;
            torq_lineF.GeometryStroke = new SolidColorPaint(Colors.Black.ToSKColor(), 1);
            torq_lineF.Name = "";
            torq_lineF.MiniatureShapeSize = 0;


            torq_lineSSL.Stroke = new SolidColorPaint(Colors.LightGreen.ToSKColor(), 1);  //定义颜色
            torq_lineSSL.LineSmoothness = 0;
            torq_lineSSL.Fill = new SolidColorPaint(Colors.Transparent.ToSKColor(), 1);
            torq_lineSSL.GeometrySize = 1;
            torq_lineSSL.GeometryStroke = new SolidColorPaint(Colors.Black.ToSKColor(), 1);
            torq_lineSSL.Name = "";
            torq_lineSSL.MiniatureShapeSize = 0;

            torq_pointMinSeat.Stroke = new SolidColorPaint(Colors.Orange.ToSKColor(), 1);  //定义颜色
            torq_pointMinSeat.LineSmoothness = 0;
            torq_pointMinSeat.Fill = new SolidColorPaint(Colors.Transparent.ToSKColor(), 1);
            torq_pointMinSeat.GeometrySize = 1;
            torq_pointMinSeat.GeometryStroke = new SolidColorPaint(Colors.Black.ToSKColor(), 1);
            torq_pointMinSeat.Name = "";
            torq_pointMinSeat.MiniatureShapeSize = 0;

            axis_x.MinLimit = 0;
            axis_x.MaxLimit = 2500;
            axis_x.ShowSeparatorLines = true;
            axis_x.SeparatorsPaint = new SolidColorPaint(Colors.Black.ToSKColor(), 1);

            axis_y.MinLimit = 0;
            axis_y.MaxLimit = 1.0;
            axis_y.ShowSeparatorLines = true;
            axis_y.SeparatorsPaint = new SolidColorPaint(Colors.Black.ToSKColor(), 1);

            axis_xx.MinLimit = 0;
            axis_x.MaxLimit = 3000;
            axis_xx.ShowSeparatorLines = true;
            axis_xx.SeparatorsPaint = new SolidColorPaint(Colors.Black.ToSKColor(), 1);

            axis_yy.MinLimit = 0;
            axis_yy.MaxLimit = 2.0;
            axis_yy.ShowSeparatorLines = true;
            axis_yy.SeparatorsPaint = new SolidColorPaint(Colors.Black.ToSKColor(), 1);


            ((CartesianChart)screwControls[0]).Series = null;
            ((CartesianChart)pressControls[0]).Series = null;
            var folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "请选择文件夹";

            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string[] files = Directory.GetFiles(folderBrowserDialog.SelectedPath);

                double torqDrawF = 0.6;      //Drop_torque上限值
                var torq_linef_values = new List<ObservablePoint>();

                for (double i = 0; i < 1400; i += 100)
                {
                    torq_linef_values.Add(new ObservablePoint(i, torqDrawF));
                }

                var chart_series = new List<ISeries>();
                chart_series.Clear();
                foreach (string file in files)
                {
                    LineSeries<ObservablePoint> torq_line = new LineSeries<ObservablePoint>();    //扭力线
                    torq_line.Stroke = new SolidColorPaint(Colors.Blue.ToSKColor(), 1);  //定义颜色
                    torq_line.LineSmoothness = 0;
                    torq_line.Fill = new SolidColorPaint(Colors.Transparent.ToSKColor(), 1);
                    torq_line.GeometrySize = 0.5;
                    torq_line.GeometryStroke = new SolidColorPaint(Colors.Black.ToSKColor(), 1);
                    //torq_line.Name = "TestLine";
                    torq_line.MiniatureShapeSize = 0;
                    torq_line.Name = null;




                    List<double> torq_double = new List<double>();
                    List<double> Angle_double = new List<double>();
                    List<TaikeModel> _taikeModel = CSVTool.OpenCSV<TaikeModel>(file);
                    foreach (var item in _taikeModel)
                    {
                        torq_double.Add(item.Torque1);
                        Angle_double.Add(item.Angle1);
                    }


                    double Max_Torque = Math.Round(torq_double.ToArray().Max(), 1);  //求最大扭力
                    double Max_Angle = Math.Round(Angle_double.ToArray().Max(), 1);    //求最大角度


                    var torq_line_values = new List<ObservablePoint>();


                    int length = torq_double.Count;

                    for (int i = 0; i < length; i += 10)
                    {
                        torq_line_values.Add(new ObservablePoint(torq_double[i], Math.Abs(Angle_double[i])));
                    }

                    torq_line.Values = torq_line_values;
                    torq_line.ScalesYAt = 0;
                    torq_line.ScalesXAt = 0;
                    chart_series.Add(torq_line);
                }
                torq_lineF.Values = torq_linef_values;

                chart_series.Add(torq_lineF);    //把线添加到图纸
                _dispatcher.Invoke(new Action(() =>
                {
                    ((CartesianChart)screwControls[0]).Series = chart_series;
                }));
                torq_lineF.Name = null;


                axis_x.Name = "Time/deg";
                axis_y.Name = "Torque/kgf.cm";

                axis_x.NameTextSize = 16;
                axis_x.TextSize = 16;

                axis_y.NameTextSize = 16;
                axis_y.TextSize = 16;
                axis_xx.Name = "Angle/deg";
                axis_yy.Name = "Press/kgf";

                axis_xx.NameTextSize = 16;
                axis_xx.TextSize = 16;
                axis_xx.Position = LiveChartsCore.Measure.AxisPosition.End;
                axis_yy.NameTextSize = 16;
                axis_yy.TextSize = 16;
                axis_yy.Position = LiveChartsCore.Measure.AxisPosition.End;
                ((CartesianChart)screwControls[0]).XAxes = new List<Axis>() { axis_x, axis_xx };
                ((CartesianChart)screwControls[0]).YAxes = new List<Axis>() { axis_y, axis_yy };

                ((CartesianChart)screwControls[0]).LegendPosition = LiveChartsCore.Measure.LegendPosition.Top;
                ((CartesianChart)screwControls[0]).LegendTextSize = 18;

            }
        }

        /// <summary>
        /// CG7 曲线汇总
        /// 导入数据
        /// </summary>
        protected override void ImportTotal()
        {

            if (screwControls == null || screwControls.Length == 0 || screwControls[0] == null)
            {
                System.Diagnostics.Trace.WriteLine("ImportTotal: screwControls not ready - aborting.");
                System.Windows.MessageBox.Show("图表尚未初始化，请稍候再试。");
                return;
            }
            if (_dispatcher == null) _dispatcher = System.Windows.Application.Current?.Dispatcher;

            #region 坐标轴设置

            //1
            axis_x.MinLimit = 0;
            axis_x.MaxLimit = 2500;
            axis_x.ShowSeparatorLines = true;
            // 增大左右内边距，给轴名称和刻度留出空间
            axis_x.Padding = new LiveChartsCore.Drawing.Padding(4, 0, 4, 0);
            axis_x.NamePadding = new LiveChartsCore.Drawing.Padding(4, 0, 4, 0);
            axis_x.SeparatorsPaint = new SolidColorPaint(Colors.Black.ToSKColor(), 1);
            axis_x.MinStep = 500;
            axis_x.ForceStepToMin = true;

            axis_y.MinLimit = 0;
            axis_y.MaxLimit = 0.6;
            axis_y.ShowSeparatorLines = true;
            axis_y.Padding = new LiveChartsCore.Drawing.Padding(4, 0, 4, 0);
            axis_y.NamePadding = new LiveChartsCore.Drawing.Padding(4, 0, 4, 0);
            axis_y.SeparatorsPaint = new SolidColorPaint(Colors.Black.ToSKColor(), 1);
            axis_y.MinStep = 0.1;

            //2
            axis_x2.MinLimit = 0;
            axis_x2.MaxLimit = 1000;
            axis_x2.ShowSeparatorLines = true;
            axis_x2.Padding = new LiveChartsCore.Drawing.Padding(0, 0, 0, 0);
            axis_x2.NamePadding = new LiveChartsCore.Drawing.Padding(0, 0, 0, 0);
            axis_x2.SeparatorsPaint = new SolidColorPaint(Colors.Black.ToSKColor(), 1);

            axis_y2.MinLimit = 0;
            axis_y2.MaxLimit = 0.6;
            axis_y2.ShowSeparatorLines = true;
            axis_y2.Padding = new LiveChartsCore.Drawing.Padding(4, 0, 4, 0);
            axis_y2.NamePadding = new LiveChartsCore.Drawing.Padding(4, 0, 4, 0);
            axis_y2.SeparatorsPaint = new SolidColorPaint(Colors.Black.ToSKColor(), 1);
            axis_y2.MinStep = 0.1;

            axis_y2y.MinLimit = 0;
            axis_y2y.MaxLimit = 0.6;
            axis_y2y.ShowSeparatorLines = true;
            // 右侧作为视觉辅助，隐藏名称与文字时不需要额外间距
            axis_y2y.Padding = new LiveChartsCore.Drawing.Padding(0, 0, 0, 0);
            axis_y2y.NamePadding = new LiveChartsCore.Drawing.Padding(0, 0, 0, 0);
            axis_y2y.SeparatorsPaint = new SolidColorPaint(Colors.Black.ToSKColor(), 1);
            axis_y2y.MinStep = 0.1;
            axis_y2y.TextSize = 0;
            axis_y2y.Position = LiveChartsCore.Measure.AxisPosition.End;

            //3
            axis_xp.MinLimit = 0;
            axis_xp.MaxLimit = 1500;
            axis_xp.ShowSeparatorLines = true;
            axis_xp.Padding = new LiveChartsCore.Drawing.Padding(4, 0, 4, 0);
            axis_xp.NamePadding = new LiveChartsCore.Drawing.Padding(4, 0, 4, 0);
            axis_xp.SeparatorsPaint = new SolidColorPaint(Colors.Black.ToSKColor(), 1);

            axis_yp.MinLimit = 0;
            axis_yp.MaxLimit = 2.5;
            axis_yp.ShowSeparatorLines = true;
            axis_yp.Padding = new LiveChartsCore.Drawing.Padding(4, 0, 4, 0);
            axis_yp.NamePadding = new LiveChartsCore.Drawing.Padding(4, 0, 4, 0);
            axis_yp.SeparatorsPaint = new SolidColorPaint(Colors.Black.ToSKColor(), 1);
            axis_yp.MinStep = 0.5;


            //X方向力
            axis_XForce_X.MinLimit = 0;
            axis_XForce_X.MaxLimit = 8;
            axis_XForce_X.ShowSeparatorLines = true;
            axis_XForce_X.Padding = new LiveChartsCore.Drawing.Padding(4, 0, 4, 0);
            axis_XForce_X.NamePadding = new LiveChartsCore.Drawing.Padding(4, 0, 4, 0);
            axis_XForce_X.SeparatorsPaint = new SolidColorPaint(Colors.Black.ToSKColor(), 1);
            axis_XForce_X.Position = LiveChartsCore.Measure.AxisPosition.Start;

            axis_XForce_Y.MinLimit = 0;
            axis_XForce_Y.MaxLimit = 10;
            axis_XForce_Y.ShowSeparatorLines = true;
            axis_XForce_Y.Padding = new LiveChartsCore.Drawing.Padding(4, 0, 4, 0);
            axis_XForce_Y.NamePadding = new LiveChartsCore.Drawing.Padding(4, 0, 4, 0);
            axis_XForce_Y.SeparatorsPaint = new SolidColorPaint(Colors.Black.ToSKColor(), 1);
            axis_XForce_Y.Position = LiveChartsCore.Measure.AxisPosition.Start;

            axis_XForce_YY.MinLimit = 0;
            axis_XForce_YY.MaxLimit = 1;
            axis_XForce_YY.ShowSeparatorLines = true;
            // 右侧辅助轴不显示文字，去掉名称间距
            axis_XForce_YY.Padding = new LiveChartsCore.Drawing.Padding(0, 0, 0, 0);
            axis_XForce_YY.NamePadding = new LiveChartsCore.Drawing.Padding(0, 0, 0, 0);
            axis_XForce_YY.SeparatorsPaint = new SolidColorPaint(Colors.Black.ToSKColor(), 1);
            axis_XForce_YY.Position = LiveChartsCore.Measure.AxisPosition.End;
            axis_XForce_YY.TextSize = 0;

            //Y方向力
            axis_YForce_X.MinLimit = 0;
            axis_YForce_X.MaxLimit = 8;
            axis_YForce_X.ShowSeparatorLines = true;
            axis_YForce_X.Padding = new LiveChartsCore.Drawing.Padding(4, 0, 4, 0);
            axis_YForce_X.NamePadding = new LiveChartsCore.Drawing.Padding(4, 0, 4, 0);
            axis_YForce_X.SeparatorsPaint = new SolidColorPaint(Colors.Black.ToSKColor(), 1);
            axis_YForce_X.Position = LiveChartsCore.Measure.AxisPosition.Start;

            axis_YForce_Y.MinLimit = 0;
            axis_YForce_Y.MaxLimit = 10;
            axis_YForce_Y.ShowSeparatorLines = true;
            axis_YForce_Y.Padding = new LiveChartsCore.Drawing.Padding(4, 0, 4, 0);
            axis_YForce_Y.NamePadding = new LiveChartsCore.Drawing.Padding(4, 0, 4, 0);
            axis_YForce_Y.SeparatorsPaint = new SolidColorPaint(Colors.Black.ToSKColor(), 1);
            axis_YForce_Y.Position = LiveChartsCore.Measure.AxisPosition.Start;

            axis_YForce_YY.MinLimit = 0;
            axis_YForce_YY.MaxLimit = 1;
            axis_YForce_YY.ShowSeparatorLines = true;
            axis_YForce_YY.Padding = new LiveChartsCore.Drawing.Padding(0, 0, 0, 0);
            axis_YForce_YY.NamePadding = new LiveChartsCore.Drawing.Padding(0, 0, 0, 0);
            axis_YForce_YY.SeparatorsPaint = new SolidColorPaint(Colors.Black.ToSKColor(), 1);
            axis_YForce_YY.Position = LiveChartsCore.Measure.AxisPosition.End;
            axis_YForce_YY.TextSize = 0;

            //Z方向力
            axis_ZForce_X.MinLimit = 0;
            axis_ZForce_X.MaxLimit = 8;
            axis_ZForce_X.ShowSeparatorLines = true;
            axis_ZForce_X.Padding = new LiveChartsCore.Drawing.Padding(4, 0, 4, 0);
            axis_ZForce_X.NamePadding = new LiveChartsCore.Drawing.Padding(4, 0, 4, 0);
            axis_ZForce_X.SeparatorsPaint = new SolidColorPaint(Colors.Black.ToSKColor(), 1);
            axis_ZForce_X.Position = LiveChartsCore.Measure.AxisPosition.Start;

            axis_ZForce_Y.MinLimit = 0;
            axis_ZForce_Y.MaxLimit = 10;
            axis_ZForce_Y.ShowSeparatorLines = true;
            axis_ZForce_Y.Padding = new LiveChartsCore.Drawing.Padding(4, 0, 4, 0);
            axis_ZForce_Y.NamePadding = new LiveChartsCore.Drawing.Padding(4, 0, 4, 0);
            axis_ZForce_Y.SeparatorsPaint = new SolidColorPaint(Colors.Black.ToSKColor(), 1);
            axis_ZForce_Y.Position = LiveChartsCore.Measure.AxisPosition.Start;

            axis_ZForce_YY.MinLimit = 0;
            axis_ZForce_YY.MaxLimit = 1;
            axis_ZForce_YY.ShowSeparatorLines = true;
            axis_ZForce_YY.Padding = new LiveChartsCore.Drawing.Padding(0, 0, 0, 0);
            axis_ZForce_YY.NamePadding = new LiveChartsCore.Drawing.Padding(0, 0, 0, 0);
            axis_ZForce_YY.SeparatorsPaint = new SolidColorPaint(Colors.Black.ToSKColor(), 1);
            axis_ZForce_YY.Position = LiveChartsCore.Measure.AxisPosition.End;
            axis_ZForce_YY.TextSize = 0;


            ((CartesianChart)screwControls[0]).Series = null;
            ((CartesianChart)pressControls[0]).Series = null;
            ((CartesianChart)torqueControls[0]).Series = null;
            ((CartesianChart)xForceControls[0]).Series = null;
            ((CartesianChart)yForceControls[0]).Series = null;
            ((CartesianChart)zForceControls[0]).Series = null;

            var chart_series = new List<ISeries>();
            var chart_series_press = new List<ISeries>();
            var chart_series_torque = new List<ISeries>();
            var chart_series_xforce = new List<ISeries>();
            var chart_series_yforce = new List<ISeries>();
            var chart_series_zforce = new List<ISeries>();

            chart_series.Clear();
            chart_series_press.Clear();
            chart_series_torque.Clear();
            chart_series_xforce.Clear();
            chart_series_yforce.Clear();
            chart_series_zforce.Clear();
            #endregion

            #region 角度扭矩

            var folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = @"D:\TaiKeScrewDatas";
            folderBrowserDialog.Description = "请选择角度扭矩数据文件夹";

            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string[] files = Directory.GetFiles(folderBrowserDialog.SelectedPath);
                foreach (string file in files)
                {
                    LineSeries<ObservablePoint> torq_line = new LineSeries<ObservablePoint>();    //扭力线

                    List<double> torq_double = new List<double>();
                    List<double> Angle_double = new List<double>();
                    List<TaikeModel> _taikeModel = CSVTool.OpenCSV<TaikeModel>(file);
                    foreach (var item in _taikeModel)
                    {
                        torq_double.Add(item.Angle1);
                        Angle_double.Add(item.Torque1);
                    }


                    double Max_Torque = Math.Round(torq_double.ToArray().Max(), 3);  //求最大扭力
                    double Max_Angle = Math.Round(Angle_double.ToArray().Max(), 3);    //求最大角度

                    if (Max_Torque > TorqueLow * 5 && Max_Torque < TorqueUp * 5 && Max_Angle * 5 > AngleLow * 5 && Max_Angle < AngleUp * 5 || true)
                    {
                        torq_line.Stroke = new SolidColorPaint(Colors.Blue.ToSKColor(), 1);  //定义颜色
                        torq_line.LineSmoothness = 0;
                        torq_line.Fill = new SolidColorPaint(Colors.Transparent.ToSKColor(), 1);
                        torq_line.GeometrySize = 0.3;
                        torq_line.GeometryStroke = new SolidColorPaint(Colors.Blue.ToSKColor(), 1);
                        torq_line.MiniatureShapeSize = 0;
                        torq_line.Name = null;
                    }
                    else
                    {
                        torq_line.Stroke = new SolidColorPaint(Colors.Red.ToSKColor(), 1);  //定义颜色
                        torq_line.LineSmoothness = 0;
                        torq_line.Fill = new SolidColorPaint(Colors.Transparent.ToSKColor(), 1);
                        torq_line.GeometrySize = 0.3;
                        torq_line.GeometryStroke = new SolidColorPaint(Colors.Red.ToSKColor(), 1);
                        torq_line.MiniatureShapeSize = 0;
                        torq_line.Name = null;
                    }

                    var torq_line_values = new List<ObservablePoint>();
                    int length = torq_double.Count;
                    for (int i = 0; i < length; i += 10)
                    {
                        torq_line_values.Add(new ObservablePoint(Math.Abs(Angle_double[i]), torq_double[i]));
                    }

                    torq_line.Values = torq_line_values;
                    torq_line.ScalesXAt = 0;
                    torq_line.ScalesYAt = 0;
                    chart_series.Add(torq_line);
                }
            }

            _dispatcher.Invoke(new Action(() =>
            {
                ((CartesianChart)screwControls[0]).Series = chart_series;
            }));

            axis_x.Name = "angle/deg";
            axis_y.Name = "torque/kgf.cm";

            axis_x.NameTextSize = 12;
            axis_x.TextSize = 12;

            axis_y.NameTextSize = 12;
            axis_y.TextSize = 12;

            ((CartesianChart)screwControls[0]).XAxes = new List<Axis>() { axis_x}; // , axis_x 
            ((CartesianChart)screwControls[0]).YAxes = new List<Axis>() { axis_y}; // , axis_y 
            ((CartesianChart)screwControls[0]).LegendPosition = LiveChartsCore.Measure.LegendPosition.Hidden;
            ((CartesianChart)screwControls[0]).LegendTextSize = 12;
            #endregion

            #region  扭矩-角度/压力-时间
            // 动态范围
            double timeMin = 0;
            double timeMax = double.MinValue;
            double angleMin = 0;
            double angleMax = double.MinValue;
            double torqueMax = double.MinValue;
            double pressMax = double.MinValue;

            // 新增：记录两条曲线末点值（用于Y轴对齐）
            double torqueEnd = 0;
            double pressEnd = 0;
            // 新增：获取最后一个有效Y值
            double GetLastPositiveY(List<ObservablePoint> points)
            {
                for (int i = points.Count - 1; i >= 0; i--)
                {
                    var y = points[i].Y ?? 0;
                    if (y > 0) return y;
                }
                return 0;
            }
            folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = @"D:\TaiKeScrewDatas";
            folderBrowserDialog.Description = "请选择时间-扭矩/角度/压力数据文件夹";
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string[] files = Directory.GetFiles(folderBrowserDialog.SelectedPath);
                foreach (string file in files)
                {
                    // 扭矩-角度曲线（蓝色，对应底部X轴-角度，左侧Y轴-扭矩）
                    var torqueSeries = new LineSeries<ObservablePoint>();
                    torqueSeries.Stroke = new SolidColorPaint(Colors.Blue.ToSKColor(), 1);
                    torqueSeries.LineSmoothness = 0;
                    torqueSeries.Fill = new SolidColorPaint(Colors.Transparent.ToSKColor(), 1);
                    torqueSeries.GeometrySize = 0.1;
                    torqueSeries.GeometryStroke = new SolidColorPaint(Colors.Blue.ToSKColor(), 1);
                    torqueSeries.MiniatureShapeSize = 0;
                    torqueSeries.Name = null;

                    // 压力-时间曲线（红色，对应顶部X轴-时间，右侧Y轴-压力）
                    var pressSeries = new LineSeries<ObservablePoint>();
                    pressSeries.Stroke = new SolidColorPaint(Colors.Red.ToSKColor(), 1);
                    pressSeries.LineSmoothness = 0;
                    pressSeries.Fill = new SolidColorPaint(Colors.Transparent.ToSKColor(), 1);
                    pressSeries.GeometrySize = 0.1;
                    pressSeries.GeometryStroke = new SolidColorPaint(Colors.Red.ToSKColor(), 1);
                    pressSeries.MiniatureShapeSize = 0;
                    pressSeries.Name = null;

                    // 原来（已替换）的声明位置：
                    List<double> noList = new List<double>();
                    List<double> torqueList = new List<double>();
                    List<double> angleList = new List<double>();
                    List<double> timeList = new List<double>();
                    List<double> pressList = new List<double>();

                    // 读取CSV文件，格式：No,Torque1,Angle1,Time,Press
                    List<TimeTorqueAnglePressModel> dataModels = CSVTool.OpenCSV<TimeTorqueAnglePressModel>(file);

                    foreach (var item in dataModels)
                    {
                        noList.Add(item.No);
                        torqueList.Add(item.Torque1);
                        angleList.Add(item.Angle1);
                        // Time 和 Press 不再使用可空类型，直接添加
                        timeList.Add(item.Time);
                        pressList.Add(item.Press);
                    }

                    var torqueValues = new List<ObservablePoint>();
                    for (int i = 0; i < torqueList.Count; i++)
                    {
                        double angleVal = Math.Abs(angleList[i]);
                        torqueValues.Add(new ObservablePoint(angleVal, torqueList[i]));
                        if (angleVal > angleMax) angleMax = angleVal;
                        if (torqueList[i] > torqueMax) torqueMax = torqueList[i];
                    }
                    if (torqueValues.Count > 0)
                    {
                        torqueSeries.Values = torqueValues;
                        torqueSeries.ScalesXAt = 0; // 底部 X 轴 = Angle
                        torqueSeries.ScalesYAt = 0; // 左侧 Y 轴 = Torque
                        chart_series_torque.Add(torqueSeries);

                        // 记录末点
                        // 记录末点（取最后一个有效值）
                        var lastTorque = GetLastPositiveY(torqueValues);
                        if (lastTorque > torqueEnd) torqueEnd = lastTorque;
                    }

                    // 压力-时间曲线数据（X轴为Time，Y轴为Press，过滤无效点）
                    var pressValues = new List<ObservablePoint>();
                    for (int i = 0; i < pressList.Count; i++)
                    {
                        // 使用非空的 IsValidPressPoint(double time, double press)
                        if (IsValidPressPoint(timeList[i], pressList[i]))
                        {
                            var tx = Math.Abs(timeList[i]);
                            pressValues.Add(new ObservablePoint(tx, pressList[i]));
                            if (tx > timeMax) timeMax = tx;
                            if (pressList[i] > pressMax) pressMax = pressList[i];
                        }
                    }
                    if (pressValues.Count > 0)
                    {
                        pressSeries.Values = pressValues;
                        pressSeries.ScalesXAt = 1; // 顶部X轴-时间
                        pressSeries.ScalesYAt = 1; // 右侧Y轴-压力
                        chart_series_torque.Add(pressSeries);

                        // 记录末点
                        var lastPress = pressValues[pressValues.Count - 1].Y ?? 0;
                        if (lastPress > pressEnd) pressEnd = lastPress;
                    }

                }
            }

            _dispatcher.Invoke(new Action(() =>
            {
                ((CartesianChart)torqueControls[0]).Series = chart_series_torque;
            }));

            // 确保最大值有合理的默认值
            if (torqueMax <= 0) torqueMax = 0.5;
            if (angleMax <= 0) angleMax = 1500;
            if (pressMax <= 0) pressMax = 1.5;
            if (timeMax <= 0) timeMax = 1500;

            // 新增：保护末点默认值
            if (torqueEnd <= 0) torqueEnd = torqueMax;
            if (pressEnd <= 0) pressEnd = pressMax;

            // 底部X轴配置（角度 - 蓝色）
            var axis_x_angle = new Axis();
            axis_x_angle.Name = "Angle/deg";
            axis_x_angle.NameTextSize = 12;
            axis_x_angle.TextSize = 12;
            axis_x_angle.MinLimit = 0;
            axis_x_angle.MaxLimit = Math.Ceiling(angleMax / 500) * 500 + 500;
            axis_x_angle.MinStep = 500;
            axis_x_angle.ShowSeparatorLines = true;
            axis_x_angle.SeparatorsPaint = new SolidColorPaint(Colors.LightGray.ToSKColor(), 1);
            axis_x_angle.Padding = new LiveChartsCore.Drawing.Padding(0, 0, 0, 0);
            axis_x_angle.NamePadding = new LiveChartsCore.Drawing.Padding(0, 0, 0, 0);
            axis_x_angle.Position = AxisPosition.Start; // 底部
            axis_x_angle.NamePaint = new SolidColorPaint(Colors.Blue.ToSKColor());
            axis_x_angle.LabelsPaint = new SolidColorPaint(Colors.Blue.ToSKColor());

            // 顶部X轴配置（时间 - 红色）
            var axis_x_time = new Axis();
            axis_x_time.Name = "Time/ms";
            axis_x_time.NameTextSize = 12;
            axis_x_time.TextSize = 12;
            axis_x_time.MinLimit = 0;
            axis_x_time.MaxLimit = Math.Ceiling(timeMax / 500) * 500 + 500;
            axis_x_time.MinStep = 500;
            axis_x_time.ShowSeparatorLines = false; // 避免与底部X轴分隔线重叠
            axis_x_time.Padding = new LiveChartsCore.Drawing.Padding(0, 0, 0, 0);
            axis_x_time.NamePadding = new LiveChartsCore.Drawing.Padding(0, 0, 0, 0);
            axis_x_time.Position = AxisPosition.End; // 顶部
            axis_x_time.NamePaint = new SolidColorPaint(Colors.Red.ToSKColor());
            axis_x_time.LabelsPaint = new SolidColorPaint(Colors.Red.ToSKColor());

            // 左侧Y轴配置（扭矩 - 蓝色）
            var axis_y_torque = new Axis();
            axis_y_torque.Name = "Torque/kgf.cm";
            axis_y_torque.NameTextSize = 12;
            axis_y_torque.TextSize = 12;
            axis_y_torque.MinLimit = 0;
            axis_y_torque.MaxLimit = Math.Ceiling(torqueMax * 10) / 10 + 0.1;
            axis_y_torque.MinStep = 0.1;
            axis_y_torque.ShowSeparatorLines = true;
            axis_y_torque.SeparatorsPaint = new SolidColorPaint(Colors.LightGray.ToSKColor(), 1);
            axis_y_torque.Position = AxisPosition.Start; // 左侧
            axis_y_torque.Padding = new LiveChartsCore.Drawing.Padding(4, 0, 4, 0);
            axis_y_torque.NamePadding = new LiveChartsCore.Drawing.Padding(4, 0, 4, 0);
            axis_y_torque.NamePaint = new SolidColorPaint(Colors.Blue.ToSKColor());
            axis_y_torque.LabelsPaint = new SolidColorPaint(Colors.Blue.ToSKColor());

            // 右侧Y轴配置（压力 - 红色）
            var axis_y_press = new Axis();
            axis_y_press.Name = "Press/kgf";
            axis_y_press.NameTextSize = 12;
            axis_y_press.TextSize = 12;
            axis_y_press.MinLimit = 0;

            // 关键调整：按末点比例换算，使两条曲线终点视觉对齐
            var ratio = torqueEnd > 0 ? pressEnd / torqueEnd : 1;
            var pressAlignedMax = axis_y_torque.MaxLimit * ratio;
            axis_y_press.MaxLimit = Math.Max(pressAlignedMax ?? 0 * 1.02, pressEnd);
            axis_y_press.MinStep = axis_y_press.MaxLimit / 5 ?? 0;

            axis_y_press.MinStep = 0.2;
            axis_y_press.ShowSeparatorLines = false; // 避免分隔线重叠
            axis_y_press.Position = AxisPosition.End; // 右侧
            axis_y_press.Padding = new LiveChartsCore.Drawing.Padding(4, 0, 4, 0);
            axis_y_press.NamePadding = new LiveChartsCore.Drawing.Padding(4, 0, 4, 0);
            axis_y_press.NamePaint = new SolidColorPaint(Colors.Red.ToSKColor());
            axis_y_press.LabelsPaint = new SolidColorPaint(Colors.Red.ToSKColor());

            ((CartesianChart)torqueControls[0]).XAxes = new List<Axis>() { axis_x_angle, axis_x_time };
            ((CartesianChart)torqueControls[0]).YAxes = new List<Axis>() { axis_y_torque, axis_y_press };

            ((CartesianChart)torqueControls[0]).LegendPosition = LiveChartsCore.Measure.LegendPosition.Hidden;
            ((CartesianChart)torqueControls[0]).LegendTextSize = 12;
            #endregion

            #region  扭矩-角度/压力-时间
            //// 动态范围
            //double timeMin = 0;
            //double timeMax = double.MinValue;
            //double angleMin = 0;
            //double angleMax = double.MinValue;
            //double torqueMax = double.MinValue;
            //double pressMax = double.MinValue;

            //folderBrowserDialog = new FolderBrowserDialog();
            //folderBrowserDialog.SelectedPath = @"D:\TaiKeScrewDatas";
            //folderBrowserDialog.Description = "请选择时间-扭矩/角度/压力数据文件夹";
            //if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //{
            //    string[] files = Directory.GetFiles(folderBrowserDialog.SelectedPath);
            //    foreach (string file in files)
            //    {
            //        // 扭矩-角度曲线（蓝色，对应底部X轴-角度，左侧Y轴-扭矩）
            //        var torqueSeries = new LineSeries<ObservablePoint>();
            //        torqueSeries.Stroke = new SolidColorPaint(Colors.Blue.ToSKColor(), 1);
            //        torqueSeries.LineSmoothness = 0;
            //        torqueSeries.Fill = new SolidColorPaint(Colors.Transparent.ToSKColor(), 1);
            //        torqueSeries.GeometrySize = 0.1;
            //        torqueSeries.GeometryStroke = new SolidColorPaint(Colors.Blue.ToSKColor(), 1);
            //        torqueSeries.MiniatureShapeSize = 0;
            //        torqueSeries.Name = null;

            //        // 压力-时间曲线（红色，对应顶部X轴-时间，右侧Y轴-压力）
            //        var pressSeries = new LineSeries<ObservablePoint>();
            //        pressSeries.Stroke = new SolidColorPaint(Colors.Red.ToSKColor(), 1);
            //        pressSeries.LineSmoothness = 0;
            //        pressSeries.Fill = new SolidColorPaint(Colors.Transparent.ToSKColor(), 1);
            //        pressSeries.GeometrySize = 0.1;
            //        pressSeries.GeometryStroke = new SolidColorPaint(Colors.Red.ToSKColor(), 1);
            //        pressSeries.MiniatureShapeSize = 0;
            //        pressSeries.Name = null;

            //        // 原来（已替换）的声明位置：
            //        List<double> noList = new List<double>();
            //        List<double> torqueList = new List<double>();
            //        List<double> angleList = new List<double>();
            //        List<double> timeList = new List<double>();
            //        List<double> pressList = new List<double>();

            //        // 读取CSV文件，格式：No,Torque1,Angle1,Time,Press
            //        List<TimeTorqueAnglePressModel> dataModels = CSVTool.OpenCSV<TimeTorqueAnglePressModel>(file);

            //        foreach (var item in dataModels)
            //        {
            //            noList.Add(item.No);
            //            torqueList.Add(item.Torque1);
            //            angleList.Add(item.Angle1);
            //            // Time 和 Press 不再使用可空类型，直接添加
            //            timeList.Add(item.Time);
            //            pressList.Add(item.Press);
            //        }

            //        var torqueValues = new List<ObservablePoint>();
            //        for (int i = 0; i < torqueList.Count; i++)
            //        {
            //            double angleVal = Math.Abs(angleList[i]);
            //            torqueValues.Add(new ObservablePoint(angleVal, torqueList[i]));
            //            if (angleVal > angleMax) angleMax = angleVal;
            //            if (torqueList[i] > torqueMax) torqueMax = torqueList[i];
            //        }
            //        if (torqueValues.Count > 0)
            //        {
            //            torqueSeries.Values = torqueValues;
            //            torqueSeries.ScalesXAt = 0; // 底部 X 轴 = Angle
            //            torqueSeries.ScalesYAt = 0; // 左侧 Y 轴 = Torque
            //            chart_series_torque.Add(torqueSeries);
            //        }

            //        // 压力-时间曲线数据（X轴为Time，Y轴为Press，过滤无效点）
            //        var pressValues = new List<ObservablePoint>();
            //        for (int i = 0; i < pressList.Count; i++)
            //        {
            //            // 使用非空的 IsValidPressPoint(double time, double press)
            //            if (IsValidPressPoint(timeList[i], pressList[i]))
            //            {
            //                var tx = Math.Abs(timeList[i]);
            //                pressValues.Add(new ObservablePoint(tx, pressList[i]));
            //                if (tx > timeMax) timeMax = tx;
            //                if (pressList[i] > pressMax) pressMax = pressList[i];
            //            }
            //        }
            //        if (pressValues.Count > 0)
            //        {
            //            pressSeries.Values = pressValues;
            //            pressSeries.ScalesXAt = 1; // 顶部X轴-时间
            //            pressSeries.ScalesYAt = 1; // 右侧Y轴-压力
            //            chart_series_torque.Add(pressSeries);
            //        }

            //    }
            //}

            //_dispatcher.Invoke(new Action(() =>
            //{
            //    ((CartesianChart)torqueControls[0]).Series = chart_series_torque;
            //}));

            //// 确保最大值有合理的默认值
            //if (torqueMax <= 0) torqueMax = 0.5;
            //if (angleMax <= 0) angleMax = 1500;
            //if (pressMax <= 0) pressMax = 1.5;
            //if (timeMax <= 0) timeMax = 1500;

            //// 底部X轴配置（角度 - 蓝色）
            //var axis_x_angle = new Axis();
            //axis_x_angle.Name = "Angle/deg";
            //axis_x_angle.NameTextSize = 12;
            //axis_x_angle.TextSize = 12;
            //axis_x_angle.MinLimit = 0;
            //axis_x_angle.MaxLimit = Math.Ceiling(angleMax / 500) * 500 + 500;
            //axis_x_angle.MinStep = 500;
            //axis_x_angle.ShowSeparatorLines = true;
            //axis_x_angle.SeparatorsPaint = new SolidColorPaint(Colors.LightGray.ToSKColor(), 1);
            //axis_x_angle.Padding = new LiveChartsCore.Drawing.Padding(0, 0, 0, 0);
            //axis_x_angle.NamePadding = new LiveChartsCore.Drawing.Padding(0, 0, 0, 0);
            //axis_x_angle.Position = AxisPosition.Start; // 底部
            //axis_x_angle.NamePaint = new SolidColorPaint(Colors.Blue.ToSKColor());
            //axis_x_angle.LabelsPaint = new SolidColorPaint(Colors.Blue.ToSKColor());

            //// 顶部X轴配置（时间 - 红色）
            //var axis_x_time = new Axis();
            //axis_x_time.Name = "Time/ms";
            //axis_x_time.NameTextSize = 12;
            //axis_x_time.TextSize = 12;
            //axis_x_time.MinLimit = 0;
            //axis_x_time.MaxLimit = Math.Ceiling(timeMax / 500) * 500 + 500;
            //axis_x_time.MinStep = 500;
            //axis_x_time.ShowSeparatorLines = false; // 避免与底部X轴分隔线重叠
            //axis_x_time.Padding = new LiveChartsCore.Drawing.Padding(0, 0, 0, 0);
            //axis_x_time.NamePadding = new LiveChartsCore.Drawing.Padding(0, 0, 0, 0);
            //axis_x_time.Position = AxisPosition.End; // 顶部
            //axis_x_time.NamePaint = new SolidColorPaint(Colors.Red.ToSKColor());
            //axis_x_time.LabelsPaint = new SolidColorPaint(Colors.Red.ToSKColor());

            //// 左侧Y轴配置（扭矩 - 蓝色）
            //var axis_y_torque = new Axis();
            //axis_y_torque.Name = "Torque/kgf.cm";
            //axis_y_torque.NameTextSize = 12;
            //axis_y_torque.TextSize = 12;
            //axis_y_torque.MinLimit = 0;
            //axis_y_torque.MaxLimit = Math.Ceiling(torqueMax * 10) / 10 + 0.1;
            //axis_y_torque.MinStep = 0.1;
            //axis_y_torque.ShowSeparatorLines = true;
            //axis_y_torque.SeparatorsPaint = new SolidColorPaint(Colors.LightGray.ToSKColor(), 1);
            //axis_y_torque.Position = AxisPosition.Start; // 左侧
            //axis_y_torque.Padding = new LiveChartsCore.Drawing.Padding(4, 0, 4, 0);
            //axis_y_torque.NamePadding = new LiveChartsCore.Drawing.Padding(4, 0, 4, 0);
            //axis_y_torque.NamePaint = new SolidColorPaint(Colors.Blue.ToSKColor());
            //axis_y_torque.LabelsPaint = new SolidColorPaint(Colors.Blue.ToSKColor());

            //// 右侧Y轴配置（压力 - 红色）
            //var axis_y_press = new Axis();
            //axis_y_press.Name = "Press/kgf";
            //axis_y_press.NameTextSize = 12;
            //axis_y_press.TextSize = 12;
            //axis_y_press.MinLimit = 0;
            //axis_y_press.MaxLimit = Math.Ceiling(pressMax * 2) / 2 + 0.5;
            //axis_y_press.MinStep = 0.2;
            //axis_y_press.ShowSeparatorLines = false; // 避免分隔线重叠
            //axis_y_press.Position = AxisPosition.End; // 右侧
            //axis_y_press.Padding = new LiveChartsCore.Drawing.Padding(4, 0, 4, 0);
            //axis_y_press.NamePadding = new LiveChartsCore.Drawing.Padding(4, 0, 4, 0);
            //axis_y_press.NamePaint = new SolidColorPaint(Colors.Red.ToSKColor());
            //axis_y_press.LabelsPaint = new SolidColorPaint(Colors.Red.ToSKColor());

            //((CartesianChart)torqueControls[0]).XAxes = new List<Axis>() { axis_x_angle, axis_x_time };
            //((CartesianChart)torqueControls[0]).YAxes = new List<Axis>() { axis_y_torque, axis_y_press };

            //((CartesianChart)torqueControls[0]).LegendPosition = LiveChartsCore.Measure.LegendPosition.Hidden;
            //((CartesianChart)torqueControls[0]).LegendTextSize = 12;
            #endregion

            #region  时间-扭矩/角度/压力  3个Y轴
            //// 动态范围
            //double timeMin = 0;
            //double timeMax = double.MinValue;
            //double torqueMax = double.MinValue;
            //double angleMax = double.MinValue;
            //double pressMax = double.MinValue;

            //folderBrowserDialog = new FolderBrowserDialog();
            //folderBrowserDialog.SelectedPath = @"D:\TaiKeScrewDatas";
            //folderBrowserDialog.Description = "请选择时间-扭矩/角度/压力数据文件夹";
            //if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //{
            //    string[] files = Directory.GetFiles(folderBrowserDialog.SelectedPath);
            //    foreach (string file in files)
            //    {
            //        // 扭矩-时间曲线（蓝色）
            //        var torqueSeries = new LineSeries<ObservablePoint>();
            //        torqueSeries.Stroke = new SolidColorPaint(Colors.Blue.ToSKColor(), 1);
            //        torqueSeries.LineSmoothness = 0;
            //        torqueSeries.Fill = new SolidColorPaint(Colors.Transparent.ToSKColor(), 1);
            //        torqueSeries.GeometrySize = 0.1;
            //        torqueSeries.GeometryStroke = new SolidColorPaint(Colors.Blue.ToSKColor(), 1);
            //        torqueSeries.MiniatureShapeSize = 0;
            //        torqueSeries.Name = null;

            //        // 角度-时间曲线（红色）
            //        var angleSeries = new LineSeries<ObservablePoint>();
            //        angleSeries.Stroke = new SolidColorPaint(Colors.Red.ToSKColor(), 1);
            //        angleSeries.LineSmoothness = 0;
            //        angleSeries.Fill = new SolidColorPaint(Colors.Transparent.ToSKColor(), 1);
            //        angleSeries.GeometrySize = 0.1;
            //        angleSeries.GeometryStroke = new SolidColorPaint(Colors.Red.ToSKColor(), 1);
            //        angleSeries.MiniatureShapeSize = 0;
            //        angleSeries.Name = null;

            //        // 压力-时间曲线（绿色）
            //        var pressSeries = new LineSeries<ObservablePoint>();
            //        pressSeries.Stroke = new SolidColorPaint(Colors.Green.ToSKColor(), 1);
            //        pressSeries.LineSmoothness = 0;
            //        pressSeries.Fill = new SolidColorPaint(Colors.Transparent.ToSKColor(), 1);
            //        pressSeries.GeometrySize = 0.1;
            //        pressSeries.GeometryStroke = new SolidColorPaint(Colors.Green.ToSKColor(), 1);
            //        pressSeries.MiniatureShapeSize = 0;
            //        pressSeries.Name = null;

            //        List<double> noList = new List<double>();
            //        List<double> torqueList = new List<double>();
            //        List<double> angleList = new List<double>();
            //        List<double?> timeList = new List<double?>();
            //        List<double?> pressList = new List<double?>();

            //        // 读取CSV文件，格式：No,Torque1,Angle1,Time,Press
            //        List<TimeTorqueAnglePressModel> dataModels = CSVTool.OpenCSV<TimeTorqueAnglePressModel>(file);

            //        foreach (var item in dataModels)
            //        {
            //            noList.Add(item.No);
            //            torqueList.Add(item.Torque1);
            //            angleList.Add(item.Angle1);
            //            timeList.Add(item.Time);
            //            pressList.Add(item.Press);
            //        }

            //        // 扭矩曲线数据（X轴为No，Y轴为Torque1）
            //        var torqueValues = new List<ObservablePoint>();
            //        for (int i = 0; i < torqueList.Count; i++)
            //        {
            //            var tx = Math.Abs(noList[i]);
            //            torqueValues.Add(new ObservablePoint(tx, torqueList[i]));
            //            if (tx > timeMax) timeMax = tx;
            //            if (torqueList[i] > torqueMax) torqueMax = torqueList[i];
            //        }
            //        torqueSeries.Values = torqueValues;
            //        torqueSeries.ScalesXAt = 0; // X轴-时间
            //        torqueSeries.ScalesYAt = 0; // Y轴0-扭矩（左侧）
            //        chart_series_torque.Add(torqueSeries);

            //        // 角度曲线数据（X轴为No，Y轴为Angle1）
            //        var angleValues = new List<ObservablePoint>();
            //        for (int i = 0; i < angleList.Count; i++)
            //        {
            //            var tx = Math.Abs(noList[i]);
            //            angleValues.Add(new ObservablePoint(tx, angleList[i]));
            //            if (angleList[i] > angleMax) angleMax = angleList[i];
            //        }
            //        angleSeries.Values = angleValues;
            //        angleSeries.ScalesXAt = 0; // X轴-时间
            //        angleSeries.ScalesYAt = 1; // Y轴1-角度（右侧第一个）
            //        chart_series_torque.Add(angleSeries);

            //        // 压力曲线数据（X轴为Time，Y轴为Press）
            //        //var pressValues = new List<ObservablePoint>();
            //        //for (int i = 0; i < pressList.Count; i++)
            //        //{
            //        //    if (timeList[i].HasValue && pressList[i].HasValue)
            //        //    {
            //        //        var tx = Math.Abs(timeList[i].Value);
            //        //        pressValues.Add(new ObservablePoint(tx, pressList[i].Value));
            //        //        if (tx > timeMax) timeMax = tx;
            //        //        if (pressList[i].Value > pressMax) pressMax = pressList[i].Value;
            //        //    }
            //        //}
            //        //if (pressValues.Count > 0)
            //        //{
            //        //    pressSeries.Values = pressValues;
            //        //    pressSeries.ScalesXAt = 0; // X轴-时间
            //        //    pressSeries.ScalesYAt = 2; // Y轴2-压力（右侧第二个）
            //        //    chart_series_torque.Add(pressSeries);
            //        //}
            //        // 在 ImportTotal() 方法中，替换原始构建 pressure 曲线的循环为下面代码段：
            //        /* 替换处示例（在处理 pressList/timeList 的位置） */
            //        var pressValues = new List<ObservablePoint>();
            //        for (int i = 0; i < pressList.Count; i++)
            //        {
            //            if (IsValidPressPoint(timeList[i], pressList[i]))
            //            {
            //                var tx = Math.Abs(timeList[i].Value);
            //                pressValues.Add(new ObservablePoint(tx, pressList[i].Value));
            //                if (tx > timeMax) timeMax = tx;
            //                if (pressList[i].Value > pressMax) pressMax = pressList[i].Value;
            //            }
            //        }
            //        if (pressValues.Count > 0)
            //        {
            //            pressSeries.Values = pressValues;
            //            pressSeries.ScalesXAt = 0; // X轴-时间
            //            pressSeries.ScalesYAt = 2; // Y轴2-压力（右侧第二个）
            //            chart_series_torque.Add(pressSeries);
            //        }
            //    }
            //}

            //_dispatcher.Invoke(new Action(() =>
            //{
            //    ((CartesianChart)torqueControls[0]).Series = chart_series_torque;
            //}));

            //// 确保最大值有合理的默认值
            //if (torqueMax <= 0) torqueMax = 0.6;
            //if (angleMax <= 0) angleMax = 2500;
            //if (pressMax <= 0) pressMax = 2.5;
            //if (timeMax <= 0) timeMax = 1000;

            //// 为了让三个Y轴0点重合，所有Y轴的MinLimit都设为0
            //// X轴配置（时间）
            //axis_x2.Name = "time/ms";
            //axis_x2.NameTextSize = 12;
            //axis_x2.TextSize = 12;
            //axis_x2.MinLimit = 0;
            //axis_x2.MaxLimit = Math.Ceiling(timeMax / 100) * 100; // 向上取整到100的倍数
            //axis_x2.MinStep = Math.Max(1, Math.Ceiling(timeMax / 10));
            //axis_x2.ShowSeparatorLines = true;
            //axis_x2.SeparatorsPaint = new SolidColorPaint(Colors.LightGray.ToSKColor(), 1);
            //axis_x2.Padding = new LiveChartsCore.Drawing.Padding(0, 0, 0, 0);
            //axis_x2.NamePadding = new LiveChartsCore.Drawing.Padding(0, 0, 0, 0);

            //// Y轴0-扭矩（蓝色，左侧）
            //axis_y2.Name = "torque/kgf.cm";
            //axis_y2.NameTextSize = 12;
            //axis_y2.TextSize = 12;
            //axis_y2.MinLimit = 0;
            //axis_y2.MaxLimit = Math.Ceiling(torqueMax * 10) / 10 + 0.1; // 稍微留出余量
            //axis_y2.MinStep = 0.1;
            //axis_y2.ShowSeparatorLines = true;
            //axis_y2.SeparatorsPaint = new SolidColorPaint(Colors.Blue.ToSKColor(), 0.5f);
            //axis_y2.Position = AxisPosition.Start;
            //axis_y2.Padding = new LiveChartsCore.Drawing.Padding(4, 0, 4, 0);
            //axis_y2.NamePadding = new LiveChartsCore.Drawing.Padding(4, 0, 4, 0);
            //// 轴文字和名称颜色与曲线一致
            //axis_y2.NamePaint = new SolidColorPaint(Colors.Blue.ToSKColor());
            //axis_y2.LabelsPaint = new SolidColorPaint(Colors.Blue.ToSKColor());

            //// Y轴1-角度（红色，右侧第一个）
            //var axis_y_angle = new Axis();
            //axis_y_angle.Name = "angle/deg";
            //axis_y_angle.NameTextSize = 12;
            //axis_y_angle.TextSize = 12;
            //axis_y_angle.MinLimit = 0;
            //axis_y_angle.MaxLimit = Math.Ceiling(angleMax / 500) * 500 + 500; // 向上取整到500的倍数
            //axis_y_angle.MinStep = 500;
            //axis_y_angle.ShowSeparatorLines = false; // 避免分隔线重叠
            //axis_y_angle.Position = AxisPosition.End;
            //axis_y_angle.Padding = new LiveChartsCore.Drawing.Padding(4, 0, 4, 0);
            //axis_y_angle.NamePadding = new LiveChartsCore.Drawing.Padding(4, 0, 4, 0);
            //// 轴文字和名称颜色与曲线一致
            //axis_y_angle.NamePaint = new SolidColorPaint(Colors.Red.ToSKColor());
            //axis_y_angle.LabelsPaint = new SolidColorPaint(Colors.Red.ToSKColor());

            //// Y轴2-压力（绿色，右侧第二个）
            //var axis_y_press = new Axis();
            //axis_y_press.Name = "press/kgf";
            //axis_y_press.NameTextSize = 12;
            //axis_y_press.TextSize = 12;
            //axis_y_press.MinLimit = 0;
            //axis_y_press.MaxLimit = Math.Ceiling(pressMax * 2) / 2 + 0.5; // 向上取整到0.5的倍数
            //axis_y_press.MinStep = 0.5;
            //axis_y_press.ShowSeparatorLines = false; // 避免分隔线重叠
            //axis_y_press.Position = AxisPosition.End;
            //axis_y_press.Padding = new LiveChartsCore.Drawing.Padding(4, 0, 60, 0); // 右侧增加间距，避免与角度轴重叠
            //axis_y_press.NamePadding = new LiveChartsCore.Drawing.Padding(4, 0, 4, 0);
            //// 轴文字和名称颜色与曲线一致
            //axis_y_press.NamePaint = new SolidColorPaint(Colors.Green.ToSKColor());
            //axis_y_press.LabelsPaint = new SolidColorPaint(Colors.Green.ToSKColor());

            //((CartesianChart)torqueControls[0]).XAxes = new List<Axis>() { axis_x2 };
            //((CartesianChart)torqueControls[0]).YAxes = new List<Axis>() { axis_y2, axis_y_angle, axis_y_press };

            //((CartesianChart)torqueControls[0]).LegendPosition = LiveChartsCore.Measure.LegendPosition.Hidden;
            //((CartesianChart)torqueControls[0]).LegendTextSize = 12;
            #endregion

            #region  时间-扭矩/角度 2个Y轴
            //// 动态时间范围
            //double timeMin = double.MaxValue;
            //double timeMax = double.MinValue;
            //folderBrowserDialog = new FolderBrowserDialog();
            //folderBrowserDialog.SelectedPath = @"D:\TaiKeScrewDatas";
            //folderBrowserDialog.Description = "请选择时间-扭矩/角度/压力数据文件夹";
            //if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //{
            //    string[] files = Directory.GetFiles(folderBrowserDialog.SelectedPath);
            //    foreach (string file in files)
            //    {
            //        // 扭矩-时间曲线
            //        var torqueSeries = new LineSeries<ObservablePoint>();
            //        torqueSeries.Stroke = new SolidColorPaint(Colors.Blue.ToSKColor(), 1);
            //        torqueSeries.LineSmoothness = 0;
            //        torqueSeries.Fill = new SolidColorPaint(Colors.Transparent.ToSKColor(), 1);
            //        torqueSeries.GeometrySize = 0.1;
            //        torqueSeries.GeometryStroke = new SolidColorPaint(Colors.Blue.ToSKColor(), 1);
            //        torqueSeries.MiniatureShapeSize = 0;
            //        torqueSeries.Name = null;

            //        // 角度-时间曲线
            //        var angleSeries = new LineSeries<ObservablePoint>();
            //        angleSeries.Stroke = new SolidColorPaint(Colors.Red.ToSKColor(), 1);
            //        angleSeries.LineSmoothness = 0;
            //        angleSeries.Fill = new SolidColorPaint(Colors.Transparent.ToSKColor(), 1);
            //        angleSeries.GeometrySize = 0.1;
            //        angleSeries.GeometryStroke = new SolidColorPaint(Colors.Red.ToSKColor(), 1);
            //        angleSeries.MiniatureShapeSize = 0;
            //        angleSeries.Name = null;

            //        List<double> time = new List<double>();
            //        List<double> torque = new List<double>();
            //        List<double> angle = new List<double>();
            //        List<TimeTorqueAngleModel> _taikeModel = CSVTool.OpenCSV<TimeTorqueAngleModel>(file);

            //        foreach (var item in _taikeModel)
            //        {
            //            // CSV格式：No,Torque1,Angle1
            //            time.Add(item.No);
            //            torque.Add(item.Torque1);
            //            angle.Add(item.Angle1);
            //        }

            //        var torqueValues = new List<ObservablePoint>();
            //        for (int i = 0; i < torque.Count; i++)
            //        {
            //            var tx = Math.Abs(time[i]);
            //            torqueValues.Add(new ObservablePoint(tx, torque[i]));
            //            if (tx < timeMin) timeMin = tx;
            //            if (tx > timeMax) timeMax = tx;
            //        }
            //        torqueSeries.Values = torqueValues;
            //        torqueSeries.ScalesXAt = 0; // 使用左侧X轴
            //        torqueSeries.ScalesYAt = 0; // 左侧Y轴-扭矩
            //        chart_series_torque.Add(torqueSeries);

            //        if (angle.Count == time.Count && angle.Count > 0)
            //        {
            //            var angleValues = new List<ObservablePoint>();
            //            for (int i = 0; i < angle.Count; i++)
            //            {
            //                var tx = Math.Abs(time[i]);
            //                angleValues.Add(new ObservablePoint(tx, angle[i]));
            //            }
            //            angleSeries.Values = angleValues;
            //            angleSeries.ScalesXAt = 0; // 同一X轴-时间
            //            angleSeries.ScalesYAt = 1; // 右侧Y轴-角度
            //            chart_series_torque.Add(angleSeries);
            //        }
            //    }
            //}
            //_dispatcher.Invoke(new Action(() =>
            //{
            //    ((CartesianChart)torqueControls[0]).Series = chart_series_torque;
            //}));

            //axis_x2.Name = "time/ms";
            //axis_y2.Name = "torque/kgf.cm";
            //// 右侧角度轴
            //var axis_y_angle = new Axis();
            //axis_y_angle.Name = "angle/deg";
            //axis_y_angle.Padding = new LiveChartsCore.Drawing.Padding(0, 0, 0, 0);
            //axis_y_angle.NamePadding = new LiveChartsCore.Drawing.Padding(0, 0, 0, 0);
            //axis_y_angle.Position = AxisPosition.End;
            //axis_y_angle.ShowSeparatorLines = true;
            //axis_y_angle.SeparatorsPaint = new SolidColorPaint(Colors.Black.ToSKColor(), 1);
            //// 轴文字颜色与曲线颜色一致，便于辨识
            //axis_y2.TextBrush = new SolidColorPaint(Colors.Blue.ToSKColor());           
            //axis_y2.NamePaint = new SolidColorPaint(Colors.Blue.ToSKColor());
            //axis_y_angle.TextBrush = new SolidColorPaint(Colors.Red.ToSKColor());
            //axis_y_angle.NamePaint = new SolidColorPaint(Colors.Red.ToSKColor());

            //axis_x2.NameTextSize = 12;
            //axis_x2.TextSize = 12;
            //// 根据数据范围优化X轴显示
            //if (timeMin != double.MaxValue && timeMax != double.MinValue && timeMax > timeMin)
            //{
            //    axis_x2.MinLimit = timeMin;
            //    axis_x2.MaxLimit = timeMax;
            //    axis_x2.MinStep = Math.Max(1, (timeMax - timeMin) / 10);
            //}
            //else
            //{
            //    // 无法计算时，提供合理默认范围
            //    axis_x2.MinLimit = 0;
            //    axis_x2.MaxLimit = 1000;
            //    axis_x2.MinStep = 100;
            //}

            //axis_y.NameTextSize = 12;
            //axis_y.TextSize = 12;

            //axis_y_angle.NameTextSize = 12;
            //axis_y_angle.TextSize = 12;

            //((CartesianChart)torqueControls[0]).XAxes = new List<Axis>() { axis_x2 };
            //((CartesianChart)torqueControls[0]).YAxes = new List<Axis>() { axis_y2, axis_y_angle };

            //((CartesianChart)torqueControls[0]).LegendPosition = LiveChartsCore.Measure.LegendPosition.Hidden;
            //((CartesianChart)torqueControls[0]).LegendTextSize = 12;
            #endregion

            #region  时间压力
            folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = @"D:\TaiKeScrewDatas";

            folderBrowserDialog.Description = "请选择时间压力数据文件夹";

            // 记录所有曲线的最大值，用于动态调整坐标轴范围，确保所有曲线都能显示
            double pressTimeMax = 0;
            double pressValueMax = 0;

            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string[] files = Directory.GetFiles(folderBrowserDialog.SelectedPath);

                foreach (string file in files)
                {
                    LineSeries<ObservablePoint> torq_line = new LineSeries<ObservablePoint>();
                    torq_line.Stroke = new SolidColorPaint(Colors.Blue.ToSKColor(), 1);
                    torq_line.LineSmoothness = 0;
                    torq_line.Fill = new SolidColorPaint(Colors.Transparent.ToSKColor(), 1);
                    torq_line.GeometrySize = 0.5;
                    torq_line.GeometryStroke = new SolidColorPaint(Colors.Blue.ToSKColor(), 1);
                    torq_line.MiniatureShapeSize = 0;
                    torq_line.Name = null;

                    List<double> time = new List<double>();
                    List<double> press = new List<double>();
                    try
                    {
                        // 直接按列读取 CSV，避免 CSVTool.OpenCSV 因列数不匹配（缺 Position 列）抛 IndexOutOfRangeException
                        // 兼容两种格式：No,Time,Press 或 No,Time,Press,Position
                        var lines = File.ReadAllLines(file);
                        for (int li = 1; li < lines.Length; li++)  // 跳过标题行
                        {
                            var row = lines[li];
                            if (string.IsNullOrWhiteSpace(row)) continue;
                            var cells = row.Split(',');
                            if (cells.Length < 3) continue;
                            // 第2列 Time，第3列 Press
                            if (double.TryParse(cells[1], out double t) &&
                                double.TryParse(cells[2], out double p))
                            {
                                time.Add(t);
                                press.Add(p);
                                // 更新最大值（用于动态坐标轴）
                                if (t > pressTimeMax) pressTimeMax = t;
                                if (p > pressValueMax) pressValueMax = p;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine($"读取文件失败 {file}: {ex.Message}");
                        continue;  // 当前文件失败时跳过，不影响其他曲线
                    }

                    var torq_line_values = new List<ObservablePoint>();
                    int length = press.Count;
                    for (int i = 0; i < length; i++)
                    {
                        torq_line_values.Add(new ObservablePoint(Math.Abs(time[i]), press[i]));
                    }

                    torq_line.Values = torq_line_values;
                    torq_line.ScalesXAt = 0;
                    torq_line.ScalesYAt = 0;  // 仅有1个Y轴(axis_yp)，索引必须为0；之前为1会引用不存在的Y轴导致曲线无法显示
                    chart_series_press.Add(torq_line);
                }
            }

            _dispatcher.Invoke(new Action(() =>
            {
                ((CartesianChart)pressControls[0]).Series = chart_series_press;
            }));

            // 根据实际数据动态调整坐标轴范围，避免固定 MaxLimit 导致超出范围的曲线被截断
            // X 轴(time)：向上取整到 100 的倍数，并留 100 余量
            axis_xp.MinLimit = 0;
            axis_xp.MaxLimit = Math.Ceiling((pressTimeMax + 100) / 100.0) * 100;
            // Y 轴(press)：向上取整到 0.2 的倍数，并留 0.1 余量
            axis_yp.MinLimit = 0;
            axis_yp.MaxLimit = Math.Ceiling((pressValueMax + 0.1) / 0.2) * 0.2;

            axis_xp.Name = "time/ms";
            axis_yp.Name = "Press/kgf";
            axis_xp.NameTextSize = 12;
            axis_xp.TextSize = 12;
            axis_yp.NameTextSize = 12;
            axis_yp.TextSize = 12;

            ((CartesianChart)pressControls[0]).XAxes = new List<Axis>() { axis_xp}; // , axis_xp
            ((CartesianChart)pressControls[0]).YAxes = new List<Axis>() { axis_yp}; // , axis_yp

            ((CartesianChart)pressControls[0]).LegendPosition = LiveChartsCore.Measure.LegendPosition.Hidden;
            ((CartesianChart)pressControls[0]).LegendTextSize = 12;
            #endregion

            #region Cowling Toe in X
            folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = @"D:\TaiKeScrewDatas";
            folderBrowserDialog.SelectedPath = @"D:\TaiKeScrewDatas";
            folderBrowserDialog.Description = "请选择Cowling Toe In X压力数据文件夹";
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string[] files = Directory.GetFiles(folderBrowserDialog.SelectedPath);

                foreach (string file in files)
                {
                    LineSeries<ObservablePoint> xforce_line = new LineSeries<ObservablePoint>();    

                    List<double> time_double = new List<double>();
                    List<double> force_double = new List<double>();
                    List<CowlingForceModel> _cowlingForceModel = CSVTool.OpenCSV<CowlingForceModel>(file);
                    foreach (var item in _cowlingForceModel)
                    {
                        time_double.Add(item.Time);
                        force_double.Add(item.Force);
                    }

                    xforce_line.Stroke = new SolidColorPaint(Colors.Blue.ToSKColor(), 1);  
                    xforce_line.LineSmoothness = 0;
                    xforce_line.Fill = new SolidColorPaint(Colors.Transparent.ToSKColor(), 1);
                    xforce_line.GeometrySize = 0.5;
                    xforce_line.GeometryStroke = new SolidColorPaint(Colors.Blue.ToSKColor(), 1);
                    xforce_line.MiniatureShapeSize = 0;
                    xforce_line.Name = null;


                    var xforce_line_values = new List<ObservablePoint>();
                    int length = force_double.Count;
                    for (int i = 0; i < length; i++)
                    {
                        xforce_line_values.Add(new ObservablePoint(Math.Abs(time_double[i])/1000, force_double[i]));
                    }

                    xforce_line.Values = xforce_line_values;
                    xforce_line.ScalesXAt = 1;
                    xforce_line.ScalesYAt = 0;
                    chart_series_xforce.Add(xforce_line);
                }
            }

            _dispatcher.Invoke(new Action(() =>
            {
                ((CartesianChart)xForceControls[0]).Series = chart_series_xforce;
            }));

            axis_XForce_X.Name = "time/S";
            axis_XForce_Y.Name = "force/N";

            axis_XForce_X.NameTextSize = 12;
            axis_XForce_X.TextSize = 12;
            axis_XForce_X.ShowSeparatorLines = true;

            axis_XForce_Y.NameTextSize = 12;
            axis_XForce_Y.TextSize = 12;
            axis_XForce_Y.ShowSeparatorLines = true;

            ((CartesianChart)xForceControls[0]).XAxes = new List<Axis>() { axis_XForce_X, axis_XForce_X };
            ((CartesianChart)xForceControls[0]).YAxes = new List<Axis>() { axis_XForce_Y, axis_XForce_YY };

            ((CartesianChart)xForceControls[0]).LegendPosition = LiveChartsCore.Measure.LegendPosition.Hidden;
            ((CartesianChart)xForceControls[0]).LegendTextSize = 12;
            #endregion


            #region Cowling Toe in Y
            folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = @"D:\TaiKeScrewDatas";

            folderBrowserDialog.Description = "请选择Cowling Toe In Y压力数据文件夹";
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string[] files = Directory.GetFiles(folderBrowserDialog.SelectedPath);

                foreach (string file in files)
                {
                    LineSeries<ObservablePoint> yforce_line = new LineSeries<ObservablePoint>();

                    List<double> time_double = new List<double>();
                    List<double> force_double = new List<double>();
                    List<CowlingForceModel> _cowlingForceModel = CSVTool.OpenCSV<CowlingForceModel>(file);
                    foreach (var item in _cowlingForceModel)
                    {
                        time_double.Add(item.Time);
                        force_double.Add(item.Force);
                    }

                    yforce_line.Stroke = new SolidColorPaint(Colors.Blue.ToSKColor(), 1);
                    yforce_line.LineSmoothness = 0;
                    yforce_line.Fill = new SolidColorPaint(Colors.Transparent.ToSKColor(), 1);
                    yforce_line.GeometrySize = 0.3;
                    yforce_line.GeometryStroke = new SolidColorPaint(Colors.Blue.ToSKColor(), 1);
                    yforce_line.MiniatureShapeSize = 0;
                    yforce_line.Name = null;


                    var yforce_line_values = new List<ObservablePoint>();
                    int length = force_double.Count;
                    for (int i = 0; i < length; i++)
                    {
                        yforce_line_values.Add(new ObservablePoint(Math.Abs(time_double[i]) / 1000, force_double[i]));
                    }

                    yforce_line.Values = yforce_line_values;
                    yforce_line.ScalesXAt = 1;
                    yforce_line.ScalesYAt = 0;
                    chart_series_yforce.Add(yforce_line);
                }
            }

            _dispatcher.Invoke(new Action(() =>
            {
                ((CartesianChart)yForceControls[0]).Series = chart_series_yforce;
            }));

            axis_YForce_X.Name = "time/S";
            axis_YForce_Y.Name = "force/N";

            axis_YForce_X.NameTextSize = 12;
            axis_YForce_X.TextSize = 12;

            axis_YForce_Y.NameTextSize = 12;
            axis_YForce_Y.TextSize = 12;

            ((CartesianChart)yForceControls[0]).XAxes = new List<Axis>() { axis_YForce_X, axis_YForce_X };
            ((CartesianChart)yForceControls[0]).YAxes = new List<Axis>() { axis_YForce_Y, axis_YForce_YY };
            ((CartesianChart)yForceControls[0]).LegendPosition = LiveChartsCore.Measure.LegendPosition.Hidden;
            ((CartesianChart)yForceControls[0]).LegendTextSize = 12;
            #endregion


            #region Cowling Toe in Z
            folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = @"D:\TaiKeScrewDatas";
            folderBrowserDialog.Description = "请选择Cowling Toe In Z压力数据文件夹";
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string[] files = Directory.GetFiles(folderBrowserDialog.SelectedPath);

                foreach (string file in files)
                {
                    LineSeries<ObservablePoint> zforce_line = new LineSeries<ObservablePoint>();

                    List<double> time_double = new List<double>();
                    List<double> force_double = new List<double>();
                    List<CowlingForceModel> _cowlingForceModel = CSVTool.OpenCSV<CowlingForceModel>(file);
                    foreach (var item in _cowlingForceModel)
                    {
                        time_double.Add(item.Time);
                        force_double.Add(item.Force);
                    }

                    zforce_line.Stroke = new SolidColorPaint(Colors.Blue.ToSKColor(), 1);
                    zforce_line.LineSmoothness = 0;
                    zforce_line.Fill = new SolidColorPaint(Colors.Transparent.ToSKColor(), 1);
                    zforce_line.GeometrySize = 0.3;
                    zforce_line.GeometryStroke = new SolidColorPaint(Colors.Blue.ToSKColor(), 1);
                    zforce_line.MiniatureShapeSize = 0;
                    zforce_line.Name = null;


                    var zforce_line_values = new List<ObservablePoint>();
                    int length = force_double.Count;
                    for (int i = 0; i < length; i++)
                    {
                        zforce_line_values.Add(new ObservablePoint(Math.Abs(time_double[i]) / 1000, force_double[i]));
                    }

                    zforce_line.Values = zforce_line_values;
                    zforce_line.ScalesXAt = 1;
                    zforce_line.ScalesYAt = 0;
                    chart_series_zforce.Add(zforce_line);
                }
            }

            _dispatcher.Invoke(new Action(() =>
            {
                ((CartesianChart)zForceControls[0]).Series = chart_series_zforce;
            }));

            axis_ZForce_X.Name = "time/S";
            axis_ZForce_Y.Name = "force/N";

            axis_ZForce_X.NameTextSize = 12;
            axis_ZForce_X.TextSize = 12;

            axis_ZForce_Y.NameTextSize = 12;
            axis_ZForce_Y.TextSize = 12;

          

            ((CartesianChart)zForceControls[0]).XAxes = new List<Axis>() { axis_ZForce_X, axis_ZForce_X };
            ((CartesianChart)zForceControls[0]).YAxes = new List<Axis>() { axis_ZForce_Y, axis_ZForce_YY };

            ((CartesianChart)zForceControls[0]).LegendPosition = LiveChartsCore.Measure.LegendPosition.Hidden;
            ((CartesianChart)zForceControls[0]).LegendTextSize = 12;
            #endregion


        }

        // 新增：过滤压力点的判断方法（放入 TaikeContentVM 类中）
        private bool IsValidPressPoint(double time, double press)
        {
            // 跳过无效或占位点（0 视为无效），也可按需设置更严格阈值
            if (double.IsNaN(time) || double.IsNaN(press)) return false;
            if (time == 0 || press == 0) return false;
            return true;
        }
    }

    public class TimeTorqueAnglePressModel
    {
        public double No { get; set; }
        public double Torque1 { get; set; }
        public double Angle1 { get; set; }
        public double Time { get; set; }
        public double Press { get; set; }
    }
}
