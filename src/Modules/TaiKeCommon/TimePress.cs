using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.IO;
using System.IO.Ports;
using LiveChartsCore.SkiaSharpView.WPF;
using System.Diagnostics;
using System.Windows.Threading;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore;
using SkiaSharp.Views.WPF;

namespace TaiKeCommon  
{
    public class TimePress : IDisposable
    {
        public TimePress()
        {

        }

        public void SetChartControl(
            Dispatcher dispatcher,
            CartesianChart chartTimePress,
            FrameworkElement chartTimePressParent)  
        {
            this.dispatcher = dispatcher;
            this.chartTimePress = chartTimePress;
            this.chartTimePressParent = chartTimePressParent; 

        }

        static string rootSaveDir = $"D:\\PressData\\{DateTime.Now.ToString("yyyyMMdd")}\\";  //数据保存路径  
        string dirPressData = $"{rootSaveDir}Data\\";
        string dirPressCurve = $"{rootSaveDir}Curve\\";  

        // 绘制图表的控件
        public CartesianChart chartTimePress; 
        public FrameworkElement chartTimePressParent;

        Axis axis_x = new Axis(); // x轴
        Axis axis_y = new Axis(); // y轴
        LineSeries<ObservablePoint> press_line = new LineSeries<ObservablePoint>();    //压力线 
        LineSeries<ObservablePoint> press1_line = new LineSeries<ObservablePoint>();    //0.1压力线 
        LineSeries<ObservablePoint> press2_line = new LineSeries<ObservablePoint>();    //0.2压力线 
        LineSeries<ObservablePoint> press3_line = new LineSeries<ObservablePoint>();    //0.3压力线 
        LineSeries<ObservablePoint> press4_line = new LineSeries<ObservablePoint>();    //0.4压力线 

        Dispatcher dispatcher = null;

        private void ShotPressPicture(string root,string SN)
        { 
            Thread.Sleep(1500);
            {
                string fileName = SN;
                string logPath = $"{root}" + fileName + ".png";
                FileInfo fi = new FileInfo(logPath);
                if (!fi.Directory.Exists)
                {
                    fi.Directory.Create();
                }
                if (dispatcher != null)
                {
                    dispatcher.Invoke(new Action(() =>
                    {
                       ShotPicture(chartTimePressParent, root, fileName);   //截图
                    }));
                }
            }
        }


        public void ShotPicture(FrameworkElement target, string savePath, string picName)   //截图
        {
            if (!Directory.Exists(savePath))        //创建保存路径
                Directory.CreateDirectory(savePath);

            string path = savePath + picName + ".png";

            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext context = drawingVisual.RenderOpen())
            {
                VisualBrush brush = new VisualBrush(target) { Stretch = Stretch.None };
                context.DrawRectangle(brush, null, new Rect(0, 0, target.ActualWidth, target.ActualHeight));
                context.Close();
            }
          //  RenderTargetBitmap targetBitmap = new RenderTargetBitmap((int)target.ActualWidth, (int)target.ActualHeight, 96d, 96d, PixelFormats.Default);
            RenderTargetBitmap targetBitmap = new RenderTargetBitmap(580, 294, 96d, 96d, PixelFormats.Default);
            targetBitmap.Render(drawingVisual);
            PngBitmapEncoder saveEncoder = new PngBitmapEncoder();
            saveEncoder.Frames.Add(BitmapFrame.Create(targetBitmap));
            string tempFile = path;
            System.IO.FileStream fs = System.IO.File.Open(tempFile, System.IO.FileMode.OpenOrCreate);
            saveEncoder.Save(fs);
            fs.Close();

        }



        public void UpdatePress(string root,string sn,double[] time, double[] press)
        {
            dispatcher.Invoke(new Action(() =>
            {
                DrawPress(chartTimePress, time, press);   //绘图扭力对时间
                chartTimePressParent.InvalidateVisual();
            }));


            if (dispatcher != null)
            {
                Task.Run(() =>
                {
                    ShotPressPicture(root + "Curve\\",sn);
                });
            }
        }


        public void DrawPress(CartesianChart chart, double[] Time, double[] Press)
        {
            axis_x.MinLimit = 0;
            axis_x.MaxLimit = 4000;
            axis_x.ShowSeparatorLines = true;
            axis_x.SeparatorsPaint = new SolidColorPaint(Colors.Black.ToSKColor(), 1);
            axis_x.MinStep = 1000;
            axis_x.ForceStepToMin = true;


            axis_y.MinLimit = 0;
            axis_y.MaxLimit = 1.5;
            axis_y.ShowSeparatorLines = true;
            axis_y.SeparatorsPaint = new SolidColorPaint(Colors.Black.ToSKColor(), 1);
            axis_y.MinStep = 0.2;
            axis_y.ForceStepToMin = true;


            axis_x.NameTextSize = 16;
            axis_x.TextSize = 8;


            axis_y.NameTextSize = 16;
            axis_y.TextSize = 8;


            press_line.Stroke = new SolidColorPaint(Colors.Blue.ToSKColor(), 1);  //定义颜色
            press_line.LineSmoothness = 0;
            press_line.Fill = new SolidColorPaint(Colors.Transparent.ToSKColor(), 1);
            press_line.GeometrySize = 1;
            press_line.GeometryStroke = new SolidColorPaint(Colors.Black.ToSKColor(), 1);
            press_line.MiniatureShapeSize = 0;



            chart.Series = null;
            var chart_series = new List<ISeries>();
            var press_line_values = new List<ObservablePoint>();
            int press = Press.Length;


            for (int i = 0; i < press; i++)   //对着上限值和扭力线进行描点
            {
                press_line_values.Add(new ObservablePoint(Time[i], Press[i]));    //对线进行描点
            }


            press_line.Values = press_line_values;
            chart_series.Add(press_line);
            chart.Series = chart_series;



            press_line.Name = "Force";
            axis_x.Name = "Time/ms";
            axis_y.Name = "Press/kg";

            chart.XAxes = new List<Axis>() { axis_x };
            chart.YAxes = new List<Axis>() { axis_y };

            chart.LegendPosition = LiveChartsCore.Measure.LegendPosition.Top;
            chart.LegendTextSize = 18;
        }








        //#region 保存数据
        //CRecordValue recordValueTorque1 = new CRecordValue();    //实例化对象1
        //public void Save_TimePress()   //
        //{
        //    string fileName = ProductSn + "_" + DateTime.Now.ToString("yyyy-M-dd_HH-mm-ss-fff");
        //    string logPath = dirTCRData + fileName + ".csv";

        //    FileInfo fi = new FileInfo(logPath);
        //    if (!fi.Directory.Exists)
        //    {
        //        fi.Directory.Create();
        //    }

        //    string title = "No" + "," + "Torque1" + "," + "Angle1";
        //    string value = "";
        //    int len = torq_double_1.Length;

        //    for (int i = 0; i < len; i++)
        //    {
        //        value = i + 1 + "," + torq_double_1[i] + "," + Angle_double_1[i];
        //        recordValueTorque1.RecordValue(dirTCRData, fileName, title, value);
        //    }
        //}
        //#endregion



        // 其它代码
        public void Dispose()
        {
            //disposing=true;//，手动释放托管资源
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~TimePress() 
        {
            // disposing=false，不释放托管资源，交给终结器释放
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // 释放托管资源

            }

            // 释放非托管资源

            _disposed = true;
        }
        private bool _disposed;
    }
}
