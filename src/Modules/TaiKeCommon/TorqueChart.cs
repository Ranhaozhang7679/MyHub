using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using SkiaSharp.Views.WPF;
using LiveChartsCore.SkiaSharpView.WPF;
using LiveChartsCore;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Xml.Linq;
using System.Drawing.Imaging;
using System.Drawing;
using System.Runtime.InteropServices.ComTypes;
using System.Diagnostics;
using LiveChartsCore.Measure;


namespace TaiKeCommon
{
    public class TorqueChart
    {

        LineSeries<ObservablePoint> torq_line = new LineSeries<ObservablePoint>();    //扭力线
        LineSeries<ObservablePoint> torq_lineF = new LineSeries<ObservablePoint>();       //上限线
        LineSeries<ObservablePoint> torq_lineSSL = new LineSeries<ObservablePoint>();     //SSL着座点
        LineSeries<ObservablePoint> torq_pointMinSeat = new LineSeries<ObservablePoint>();     //最小着座扭矩
        LineSeries<ObservablePoint> torqTimePress_line = new LineSeries<ObservablePoint>();    //时间压力线
        LineSeries<ObservablePoint> torqTimeTorque_line = new LineSeries<ObservablePoint>();    //时间扭矩线 

        LineSeries<ObservablePoint> xforce_line = new LineSeries<ObservablePoint>();    //Toe in X方向力
        LineSeries<ObservablePoint> yforce_line = new LineSeries<ObservablePoint>();    //Toe in Y方向力
        LineSeries<ObservablePoint> zforce_line = new LineSeries<ObservablePoint>();    //Toe in Z方向力







        Axis axis_x = new Axis(); // x轴
        Axis axis_y = new Axis(); // y轴
        Axis axis_xEnd = new Axis(); // x轴
        Axis axis_yEnd = new Axis(); // y轴


        Axis axis_toein_x = new Axis(); // Toe in x轴
        Axis axis_toein_y = new Axis(); // Toe in y轴




        public TorqueChart()
        {
            torq_line.Stroke = new SolidColorPaint(Colors.Blue.ToSKColor(), 1);  //定义颜色
            torq_line.LineSmoothness = 0;
            torq_line.Fill = new SolidColorPaint(Colors.Transparent.ToSKColor(), 1);
            torq_line.GeometrySize = 1;
            torq_line.GeometryStroke = new SolidColorPaint(Colors.Black.ToSKColor(), 1);
            torq_line.Name = "TestLine";
            torq_line.MiniatureShapeSize = 5;

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


            torqTimePress_line.Stroke = new SolidColorPaint(Colors.Blue.ToSKColor(), 1);  //定义颜色
            torqTimePress_line.LineSmoothness = 0;
            torqTimePress_line.Fill = new SolidColorPaint(Colors.Transparent.ToSKColor(), 1);
            torqTimePress_line.GeometrySize = 1;
            torqTimePress_line.GeometryStroke = new SolidColorPaint(Colors.Blue.ToSKColor(), 1);
            torqTimePress_line.Name = "TimePress";
            torqTimePress_line.MiniatureShapeSize = 5;


            torqTimeTorque_line.Stroke = new SolidColorPaint(Colors.Green.ToSKColor(), 1);  //定义颜色
            torqTimeTorque_line.LineSmoothness = 0;
            torqTimeTorque_line.Fill = new SolidColorPaint(Colors.Transparent.ToSKColor(), 1);
            torqTimeTorque_line.GeometrySize = 1;
            torqTimeTorque_line.GeometryStroke = new SolidColorPaint(Colors.Green.ToSKColor(), 1);
            torqTimeTorque_line.Name = "TimeTorque";
            torqTimeTorque_line.MiniatureShapeSize = 5;

            axis_x.MinLimit = 0;
            axis_x.ShowSeparatorLines = true;
            axis_x.SeparatorsPaint = new SolidColorPaint(Colors.Black.ToSKColor(), 1);

            axis_y.MinLimit = 0;
            axis_y.ShowSeparatorLines = true;
            axis_y.SeparatorsPaint = new SolidColorPaint(Colors.Black.ToSKColor(), 1);


            xforce_line.Stroke = new SolidColorPaint(Colors.Blue.ToSKColor(), 1);
            xforce_line.LineSmoothness = 0;
            xforce_line.Fill = new SolidColorPaint(Colors.Transparent.ToSKColor(), 1);
            xforce_line.GeometrySize = 1;
            xforce_line.GeometryStroke = new SolidColorPaint(Colors.Blue.ToSKColor(), 1);
            xforce_line.Name = "ToeIn-X-Force";
            xforce_line.MiniatureShapeSize = 5;


            yforce_line.Stroke = new SolidColorPaint(Colors.Green.ToSKColor(), 1);
            yforce_line.LineSmoothness = 0;
            yforce_line.Fill = new SolidColorPaint(Colors.Transparent.ToSKColor(), 1);
            yforce_line.GeometrySize = 1;
            yforce_line.GeometryStroke = new SolidColorPaint(Colors.Green.ToSKColor(), 1);
            yforce_line.Name = "ToeIn-Y-Force";
            yforce_line.MiniatureShapeSize = 5;


            zforce_line.Stroke = new SolidColorPaint(Colors.Black.ToSKColor(), 1);
            zforce_line.LineSmoothness = 0;
            zforce_line.Fill = new SolidColorPaint(Colors.Transparent.ToSKColor(), 1);
            zforce_line.GeometrySize = 1;
            zforce_line.GeometryStroke = new SolidColorPaint(Colors.Black.ToSKColor(), 1);
            zforce_line.Name = "ToeIn-Z-Force";
            zforce_line.MiniatureShapeSize = 5;
        }

        /// <summary>
        /// 角度扭矩图-第二张图
        /// </summary>
        /// <param name="chart"></param>          //图形控件
        /// <param name="textBlocks"></param>         //标签控件数组
        /// <param name="interval"></param>       //采样时间间隔
        /// <param name="type"></param>           //0:扭力vs时间，1:扭力vs角度
        /// <param name="savePath"></param>       //保存路径
        /// <param name="picName"></param>        //图片名称
        /// <param name="arrResultTK"></param>    //过程数据
        /// <param name="Max_Torque"></param>     //最大扭力
        /// <param name="Max_Angle"></param>      //最大角度
        /// <param name="torq_double"></param>    //扭力数据
        /// <param name="Angle_double"></param>   //角度数据
        /// <param name="titleAxisY"></param>     //Y轴标题
        public void Draw(CartesianChart chart, TextBlock[] textBlocks, int interval, int type, string[] arrResultTK, double Max_Torque, double Max_Angle, double[] torq_double, double[] Angle_double, string titleAxisY, int indexSSL)
        {
            string EC = arrResultTK[3];
            double torqDrawF = Convert.ToDouble(arrResultTK[20]);      //Drop_torque上限值
            double torqDrawSSL = torq_double[indexSSL];   //SSL着座值

            string[] items = new string[15];
            items[0] = "EC=" + arrResultTK[3];   //EC
            items[1] = "T1=" + arrResultTK[7];   //T1
            items[2] = "T2=" + arrResultTK[8];   //T2
            items[3] = "T3=" + arrResultTK[9];   //T3
            items[4] = "Tt=" + arrResultTK[11];  //Tt
            items[5] = "A1=" + arrResultTK[12];  //A1
            items[6] = "A2=" + arrResultTK[13];  //A2
            items[7] = "A3=" + arrResultTK[14];  //A3
            items[8] = "SSL=" + torqDrawSSL;    //SSL
            items[9] = "Max.Angle=" + Max_Angle;
            items[10] = "Max.Torque=" + Max_Torque;
            items[11] = "Seating torque=" + arrResultTK[32];    //
            items[12] = "Clamp torque=" + arrResultTK[34];
            items[13] = "Prevailing torque=" + arrResultTK[41];
            items[14] = "MC=" + arrResultTK[2];  //MC
            for (int i = 0; i < items.Length; i++)
            {
                textBlocks[i].Text = items[i];
            }

            chart.Series = null;
            var chart_series = new List<ISeries>();
            var torq_line_values = new List<ObservablePoint>();
            var torq_linef_values = new List<ObservablePoint>();
            var torq_linessl_values = new List<ObservablePoint>();
            var torq_pointMinSeat_values = new List<ObservablePoint>();

            int length = torq_double.Length;
            double avgAngle = 0;    //用多個值累加避免只用最後一個值正負都有可能
            for (int i = 0; i < length; i++)
            {
                avgAngle += Angle_double[i];
            }

            if (EC == "OK")
            {
                for (int i = 0; i < length; i++)   //对着上限值和扭力线进行描点
                {
                    if (type == 0)  //0:压力vs时间
                    {
                        torq_line_values.Add(new ObservablePoint(Angle_double[i], torq_double[i]));    //对线进行描点
                    }
                    else      //否则1:扭力vs角度
                    {
                        torq_line_values.Add(new ObservablePoint(Math.Abs(Angle_double[i]), torq_double[i]));    //反转时角度在图上显示正值

                        if (avgAngle > 0)  //正转时
                        {
                            torq_linef_values.Add(new ObservablePoint(Math.Abs(Angle_double[i]), torqDrawF));
                        }
                        else
                        {
                            torq_linef_values.Add(new ObservablePoint(Math.Abs(Angle_double[i]), -torqDrawF));
                        }
                    }
                }

                for (int i = 0; i < indexSSL + 1; i++)   //对着座值SSL进行描点
                {
                    if (type == 0)
                    {
                        torq_linessl_values.Add(new ObservablePoint(interval * i, torq_double[indexSSL]));
                    }
                    else
                    {
                        torq_linessl_values.Add(new ObservablePoint(Math.Abs(Angle_double[i]), torq_double[indexSSL]));
                    }
                }

                if (avgAngle > 0)  //正转时
                {
                    double torqDrawMinSeat = 0;   //最小着座值
                    double angleDrawMinSeat = 0;   //最小着座值
                    try
                    {
                        torqDrawMinSeat = Convert.ToDouble(arrResultTK[32]);   //最小着座值
                        angleDrawMinSeat = Convert.ToDouble(arrResultTK[36]);   //最小着座值
                    }
                    catch { }
                    if (torqDrawMinSeat != 0)
                    {
                        if (type == 0)  //0:扭力vs时间
                        {
                            //for (int i = length - 1; i >= 0; i--)   //画最小着座点
                            //{
                            //    //if (torqDrawMinSeat >= torq_double[i])
                            //    if (angleDrawMinSeat >= Angle_double[i])
                            //    {
                            //        torq_linessl_values.Add(new ObservablePoint(interval * i, torq_double[i]));
                            //        break;
                            //    }
                            //}
                        }
                        else      //否则1:扭力vs角度
                        {
                            torq_linessl_values.Add(new ObservablePoint(Math.Abs(angleDrawMinSeat), torqDrawMinSeat));
                        }
                    }
                }
            }
            else   //否则锁NG
            {
                for (int i = 0; i < length; i++)
                {
                    if (type == 0)
                    {
                        torq_line_values.Add(new ObservablePoint(Angle_double[i], torq_double[i]));
                    }
                    else
                    {
                        torq_line_values.Add(new ObservablePoint(Math.Abs(Angle_double[i]), torq_double[i]));
                    }
                }
            }


            if (type == 0)
            {
                torq_line.Values = torq_line_values;
                chart_series.Add(torq_line);
                chart.Series = chart_series;
            }
            else
            {
                torq_line.Values = torq_line_values;
                torq_lineF.Values = torq_linef_values;
                torq_lineSSL.Values = torq_linessl_values;
                //客户要求取消-20250921
                //chart_series.Add(torq_lineF);
                //客户要求取消-20250921
                //chart_series.Add(torq_lineSSL);
                chart_series.Add(torq_line);
                //客户要求取消-20250921
                //chart_series.Add(torq_pointMinSeat);
                chart.Series = chart_series;
            }

            if (type == 0) //0:扭力vs时间，1:扭力vs角度
            {
                torq_line.Name = "Press VS Time";
                axis_x.Name = "Time/ms";
                axis_y.Name = "Press/kg";
            }
            else
            {
                torq_line.Name = "Torque VS Angle";
                axis_x.Name = "Angle/deg";
                axis_y.Name = "Torque/kgf.cm";
            }
            axis_y.Name = titleAxisY;

            // X/Y 轴上限根据实际数据自适应：避免硬编码上限导致数据压顶或分辨率浪费
            // type==0: X=时间(interval*i), Y=扭矩 ; type==1: X=角度, Y=扭矩
            double[] xAxisData = (type == 0)
                ? Enumerable.Range(0, length).Select(i => (double)(interval * i)).ToArray()
                : Angle_double;
            double axis_x_max = ComputeAxisMaxFromZero(xAxisData, fallbackMax: 2500, minStep: 200);
            double axis_y_max = ComputeAxisMaxFromZero(torq_double, fallbackMax: 1.0, minStep: 0.1);

            axis_x.NameTextSize = 16;
            axis_x.TextSize = 16;
            axis_x.MinLimit = 0;
            axis_x.MaxLimit = axis_x_max;
            axis_x.MinStep = 200;



            axis_y.NameTextSize = 16;
            axis_x.TextSize = 16;
            axis_y.MinLimit = 0;
            axis_y.MaxLimit = axis_y_max;
            axis_y.MinStep = 0.1;

            chart.XAxes = new List<Axis>() { axis_x };
            chart.YAxes = new List<Axis>() { axis_y };

            chart.LegendPosition = LiveChartsCore.Measure.LegendPosition.Top;
            chart.LegendTextSize = 18;
        }


        /// <summary>
        /// 时间压力图-第一张图
        /// 原来的三合一曲线，现在只保留压力时间曲线-20250922
        /// </summary>
        /// <param name="chart"></param>
        /// <param name="textBlocks"></param>
        /// <param name="interval"></param>
        /// <param name="type"></param>
        /// <param name="arrResultTK"></param>
        /// <param name="Max_Torque"></param>
        /// <param name="Max_Angle"></param>
        /// <param name="torq_double"></param>
        /// <param name="Angle_double"></param>
        /// <param name="titleAxisY"></param>
        /// <param name="indexSSL"></param>
        /// <param name="Time"></param>
        /// <param name="Press"></param>
        public void DrawTotalCurve(CartesianChart chart, TextBlock[] textBlocks, int interval, int type, string[] arrResultTK, double Max_Torque, double Max_Angle, double[] torq_double, double[] Angle_double, string titleAxisY, int indexSSL, double[] Time, double[] Press)
        {
            string EC = arrResultTK[3];
            double torqDrawF = Convert.ToDouble(arrResultTK[20]);      //Drop_torque上限值
            double torqDrawSSL = torq_double[indexSSL];   //SSL着座值
            var max = Press.Max();
            string[] items = new string[16];
            items[0] = "EC=" + arrResultTK[3];   //EC
            items[1] = "T1=" + arrResultTK[7];   //T1
            items[2] = "T2=" + arrResultTK[8];   //T2
            items[3] = "T3=" + arrResultTK[9];   //T3
            items[4] = "Tt=" + arrResultTK[11];  //Tt
            items[5] = "A1=" + arrResultTK[12];  //A1
            items[6] = "A2=" + arrResultTK[13];  //A2
            items[7] = "A3=" + arrResultTK[14];  //A3
            items[8] = "SSL=" + torqDrawSSL;    //SSL
            items[9] = "Max.Angle=" + Max_Angle;
            items[10] = "Max.Torque=" + Max_Torque;
            items[11] = "Seating torque=" + arrResultTK[32];    //
            items[12] = "Clamp torque=" + arrResultTK[34];
            items[13] = "Prevailing torque=" + arrResultTK[41];
            items[14] = "MC=" + arrResultTK[2];  //MC
            items[15] = "MAX Press=" + max.ToString();  //Max Press
            for (int i = 0; i < items.Length; i++)
            {
                textBlocks[i].Text = items[i];
            }

            chart.Series = null;
            var chart_series = new List<ISeries>();
            var torq_line_values = new List<ObservablePoint>();
            var torq_lineTimePress_values = new List<ObservablePoint>();
            var torq_lineTimeTorque_values = new List<ObservablePoint>();
            var torq_linef_values = new List<ObservablePoint>();
            var torq_linessl_values = new List<ObservablePoint>();
            var torq_pointMinSeat_values = new List<ObservablePoint>();

            int length = torq_double.Length;
            int lengthPress = Press.Length;
            double avgAngle = 0;    //用多個值累加避免只用最後一個值正負都有可能
            for (int i = 0; i < length; i++)
            {
                avgAngle += Angle_double[i];
            }

            if (EC == "OK")
            {
                for (int i = 0; i < length; i++)   //对着上限值和扭力线进行描点
                {
                    torq_line_values.Add(new ObservablePoint(Angle_double[i], torq_double[i]));    //角度/扭矩
                    torq_lineTimeTorque_values.Add(new ObservablePoint(interval * i, torq_double[i]));    //对线进行描点   
                                                                                                          // torq_linef_values.Add(new ObservablePoint(interval * i, torqDrawF));        //对上限线进行描点  
                }
                for (int i = 0; i < lengthPress; i++)   //对着上限值和扭力线进行描点
                {
                    torq_lineTimePress_values.Add(new ObservablePoint(Time[i], Press[i]));    //时间/压力
                }
            }
            else   //否则锁NG
            {
                for (int i = 0; i < length; i++)   //对着上限值和扭力线进行描点
                {
                    torq_line_values.Add(new ObservablePoint(Angle_double[i], torq_double[i]));    //角度/扭矩
                    torq_lineTimeTorque_values.Add(new ObservablePoint(interval * i, torq_double[i]));    //对线进行描点    //时间/压力 
                    // torq_linef_values.Add(new ObservablePoint(interval * i, torqDrawF));        //对上限线进行描点  
                }
                for (int i = 0; i < lengthPress; i++)   //对着上限值和扭力线进行描点
                {
                    torq_lineTimePress_values.Add(new ObservablePoint(Time[i], Press[i]));    //时间/压力
                }
            }

            torq_line.Values = torq_line_values;
            torq_line.ScalesXAt = 1;
            torq_line.ScalesYAt = 0;
            torqTimePress_line.Values = torq_lineTimePress_values;
            torqTimePress_line.ScalesXAt = 0;
            torqTimePress_line.ScalesYAt = 1;
            torqTimeTorque_line.Values = torq_lineTimeTorque_values;
            torqTimeTorque_line.ScalesXAt = 0;
            torqTimeTorque_line.ScalesYAt = 0;

            //客户要求取消，时间扭矩图-20250921

            //chart_series.Add(torq_line);
            chart_series.Add(torqTimePress_line);

            //客户要求取消，时间扭矩图-20250921
            //chart_series.Add(torqTimeTorque_line);
            chart.Series = chart_series;

            //torq_line.Name = "Torque/Angle";
            torqTimePress_line.Name = "Time/Press";
            //torqTimeTorque_line.Name = "Time/Torque";


            //axis_x.Name = "time/ms     ";
            //axis_y.Name = "Press/kgf  torque/kgf.cm";

            axis_x.Name = "time/ms";
            //axis_y.Name = "torque/kgf.cm";
            //axis_xEnd.Name = "angle/deg";
            axis_yEnd.Name = "press/kgf";

            // X轴最大值根据实际时间数据动态计算:向上取整到 500ms 倍数并预留 1 个刻度间隔的余量
            const int timeAxisStep = 500;
            double timeMaxValue = Time != null && Time.Length > 0 ? Time.Max() : 0;
            double timeMaxLimit = Math.Ceiling((timeMaxValue + timeAxisStep) / (double)timeAxisStep) * timeAxisStep;
            if (timeMaxLimit < timeAxisStep * 2) timeMaxLimit = timeAxisStep * 2;

            axis_x.NameTextSize = 16;
            axis_x.TextSize = 16;
            axis_x.MinLimit = 0;
            axis_x.MaxLimit = timeMaxLimit;
            axis_x.ShowSeparatorLines = false;  // true
            axis_x.Padding = new LiveChartsCore.Drawing.Padding(0, 0, 0, 0);
            axis_x.SeparatorsPaint = new SolidColorPaint(Colors.Black.ToSKColor(), 1);
            axis_x.MinStep = timeAxisStep;
            axis_x.Position = AxisPosition.Start;




            //axis_y.NameTextSize = 16;
            //axis_y.TextSize = 16;
            //axis_y.MinLimit = 0;
            //axis_y.MaxLimit = 1.0;
            //axis_y.ShowSeparatorLines = true;
            //axis_y.SeparatorsPaint = new SolidColorPaint(Colors.Black.ToSKColor(), 1);

            //axis_xEnd.NameTextSize = 16;
            //axis_xEnd.TextSize = 16;
            //axis_xEnd.MinLimit = 0;
            //axis_xEnd.MaxLimit = 3500; // 2025-4-25 时间X轴不变为1500，角度X轴最大值修改为3500
            //axis_xEnd.ShowSeparatorLines = false; // true
            //axis_xEnd.Padding = new LiveChartsCore.Drawing.Padding(0, 0, 0, 0);
            //axis_xEnd.SeparatorsPaint = new SolidColorPaint(Colors.Black.ToSKColor(), 1);
            //axis_xEnd.MinStep = 200;

            axis_yEnd.NameTextSize = 16;
            axis_yEnd.TextSize = 16;
            axis_yEnd.MinLimit = 0;
            // Y 轴(Press)上限自适应：基于实际压力数据 + 余量，避免硬编码 2.5 导致曲线只占一半高度
            axis_yEnd.MaxLimit = ComputeAxisMaxFromZero(Press, fallbackMax: 2.5, minStep: 0.2);
            axis_yEnd.ShowSeparatorLines = true;
            axis_yEnd.SeparatorsPaint = new SolidColorPaint(Colors.Black.ToSKColor(), 1);
            axis_yEnd.MinStep = 0.2;
            axis_yEnd.Position = AxisPosition.Start;


            chart.XAxes = new List<Axis>() { axis_x, axis_x };
            chart.YAxes = new List<Axis>() { axis_yEnd, axis_yEnd };

            chart.LegendPosition = LiveChartsCore.Measure.LegendPosition.Top;
            chart.LegendTextSize = 12;
        }





        /// <summary>
        /// 角度-扭矩图-第二张图
        /// </summary>
        /// <param name="chart"></param>          //图形控件
        /// <param name="textBlocks"></param>         //标签控件数组
        /// <param name="interval"></param>       //采样时间间隔
        /// <param name="type"></param>           //0:扭力vs时间，1:扭力vs角度
        /// <param name="savePath"></param>       //保存路径
        /// <param name="picName"></param>        //图片名称
        /// <param name="arrResultTK"></param>    //过程数据
        /// <param name="Max_Torque"></param>     //最大扭力
        /// <param name="Max_Angle"></param>      //最大角度
        /// <param name="torq_double"></param>    //扭力数据
        /// <param name="Angle_double"></param>   //角度数据
        /// <param name="titleAxisY"></param>     //Y轴标题
        public void DrawPress(CartesianChart chart, TextBlock[] textBlocks, int interval, int type, string[] arrResultTK, double Max_Torque, double Max_Angle, double[] torq_double, double[] Angle_double, string titleAxisY, int indexSSL, double[] Time, double[] Press)
        {
            string EC = arrResultTK[3];
            double torqDrawF = Convert.ToDouble(arrResultTK[20]);      //Drop_torque上限值
            double torqDrawSSL = torq_double[indexSSL];   //SSL着座值
            double max = Press.Max();
            string[] items = new string[16];
            items[0] = "EC=" + arrResultTK[3];   //EC
            items[1] = "T1=" + arrResultTK[7];   //T1
            items[2] = "T2=" + arrResultTK[8];   //T2
            items[3] = "T3=" + arrResultTK[9];   //T3
            items[4] = "Tt=" + arrResultTK[11];  //Tt
            items[5] = "A1=" + arrResultTK[12];  //A1
            items[6] = "A2=" + arrResultTK[13];  //A2
            items[7] = "A3=" + arrResultTK[14];  //A3
            items[8] = "SSL=" + torqDrawSSL;     //SSL
            items[9] = "Max.Angle=" + Max_Angle;
            items[10] = "Max.Torque=" + Max_Torque;
            items[11] = "Seating torque=" + arrResultTK[32];  //
            items[12] = "Clamp torque=" + arrResultTK[34];
            items[13] = "Prevailing torque=" + arrResultTK[41];
            items[14] = "MC=" + arrResultTK[2];  //MC
            items[15] = "MAX Press=" + max.ToString();  //Max Press
            for (int i = 0; i < items.Length; i++)
            {
                textBlocks[i].Text = items[i];
            }

            chart.Series = null;
            var chart_series = new List<ISeries>();
            var torq_line_values = new List<ObservablePoint>();
            var torq_linef_values = new List<ObservablePoint>();
            var torq_linessl_values = new List<ObservablePoint>();
            var torq_pointMinSeat_values = new List<ObservablePoint>();

            int press = Press.Length;

            if (EC == "OK")
            {
                for (int i = 0; i < press; i++)   //对着上限值和扭力线进行描点
                {
                    if (type == 0)  //0:压力vs时间
                    {
                        torq_line_values.Add(new ObservablePoint(Time[i], Press[i]));    //对线进行描点
                    }
                }
            }
            else   //否则锁NG
            {
                for (int i = 0; i < press; i++)
                {
                    if (type == 0)
                    {
                        torq_line_values.Add(new ObservablePoint(Time[i], Press[i]));
                    }
                }
            }


            if (type == 0)
            {
                torq_line.Values = torq_line_values;
                chart_series.Add(torq_line);
                chart.Series = chart_series;
            }
            else
            {
                torq_line.Values = torq_line_values;
                torq_lineF.Values = torq_linef_values;
                torq_lineSSL.Values = torq_linessl_values;

                //chart_series.Add(torq_lineF); 
                //chart_series.Add(torq_lineSSL);          
                chart_series.Add(torq_line);
                //chart_series.Add(torq_pointMinSeat);
                chart.Series = chart_series;
            }

            if (type == 0) //0:扭力vs时间，1:扭力vs角度
            {
                torq_line.Name = "Press VS Time";
                axis_x.Name = "Time/ms";
                axis_y.Name = "Press/kg";
            }
            else
            {
                torq_line.Name = "Torque VS Angle";
                axis_x.Name = "Angle/deg";
                axis_y.Name = "Torque/kgf.cm";
                axis_y.Name = titleAxisY;
            }


            axis_x.NameTextSize = 16;
            axis_x.TextSize = 16;
            axis_x.MinLimit = 0;
            // X 轴(Time)上限自适应：基于实际时间数据 + 余量，避免硬编码 5000 浪费分辨率
            axis_x.MaxLimit = ComputeAxisMaxFromZero(Time, fallbackMax: 5000, minStep: 500);
            axis_x.MinStep = 500;


            axis_y.NameTextSize = 16;
            axis_y.TextSize = 16;
            // Y 轴(Press)上限自适应：基于实际压力数据 + 余量，避免硬编码 2.0 导致压顶
            axis_y.MaxLimit = ComputeAxisMaxFromZero(Press, fallbackMax: 2.0, minStep: 0.2);
            axis_y.MinStep = 0.2;
            axis_y.MinLimit = 0;

            chart.XAxes = new List<Axis>() { axis_x };
            chart.YAxes = new List<Axis>() { axis_y };

            chart.LegendPosition = LiveChartsCore.Measure.LegendPosition.Top;
            chart.LegendTextSize = 18;
        }


        /// <summary>
        /// Cowling扣合压力图-第三张图
        /// </summary>
        /// <param name="chart"></param>
        /// <param name="ToeinXForce"></param>
        /// <param name="ToeinYForce"></param>
        /// <param name="ToeinZForce"></param>
        /// <param name="Time"></param>
        public void DrawToeinForce(CartesianChart chart, double[] ToeinXForce, double[] ToeinYForce, double[] ToeinZForce, double[] Time)
        {
            chart.Series = null;
            var chart_series = new List<ISeries>();
            var xforce_line_values = new List<ObservablePoint>();
            var yforce_line_values = new List<ObservablePoint>();
            var zforce_line_values = new List<ObservablePoint>();
            int count = Time.Length;
            for (int i = 0; i < count; i++)
            {
                xforce_line_values.Add(new ObservablePoint(Time[i], ToeinXForce[i]));
                yforce_line_values.Add(new ObservablePoint(Time[i], ToeinYForce[i]));
                zforce_line_values.Add(new ObservablePoint(Time[i], ToeinZForce[i]));
            }

            xforce_line.Values = xforce_line_values;
            yforce_line.Values = yforce_line_values;
            zforce_line.Values = zforce_line_values;

            xforce_line.Name = "Toein X Force1";
            yforce_line.Name = "Toein Y Force1";
            zforce_line.Name = "Toein Z Force1";
            //xforce_line.Stroke = new SolidColorPaint(Colors.Red.ToSKColor(), 1); // 红色线条
            //yforce_line.Stroke = new SolidColorPaint(Colors.Green.ToSKColor(), 1);
            //zforce_line.Stroke = new SolidColorPaint(Colors.Blue.ToSKColor(), 1);

            chart_series.Add(xforce_line);
            chart_series.Add(yforce_line);
            chart_series.Add(zforce_line);
            chart.Series = chart_series;


            axis_toein_x.Name = "Time/ms";
            axis_toein_x.NameTextSize = 16;
            axis_toein_x.TextSize = 16;
            //axis_toein_x.MinLimit = 0;
            //axis_toein_x.MaxLimit = 5000;

            axis_toein_y.Name = "Force/N";
            axis_toein_y.NameTextSize = 16;
            axis_toein_y.TextSize = 16;
            //axis_toein_y.MaxLimit = 100;
            //axis_toein_y.MinLimit = 0;

            chart.XAxes = new List<Axis>() { axis_toein_x };
            chart.YAxes = new List<Axis>() { axis_toein_y };

            chart.LegendPosition = LiveChartsCore.Measure.LegendPosition.Top;
            chart.LegendTextSize = 18;
        }







        public void ShotPicture(FrameworkElement target, string savePath, string picName)   //截图
        {

            // 等待控件布局完成
            if (target.ActualWidth <= 0 || target.ActualHeight <= 0)
            {
                return;
                target.UpdateLayout(); // 尝试强制布局
            }
            if (target.ActualWidth <= 0 || target.ActualHeight <= 0)
            {
                throw new InvalidOperationException("控件未布局完成或无有效尺寸，无法截图。");
            }

            if (!Directory.Exists(savePath))        //创建保存路径
                Directory.CreateDirectory(savePath);

            string path = savePath + "\\" + picName + ".png";

            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext context = drawingVisual.RenderOpen())
            {
                VisualBrush brush = new VisualBrush(target) { Stretch = Stretch.None };
                context.DrawRectangle(brush, null, new Rect(0, 0, target.ActualWidth, target.ActualHeight));
                context.Close();
            }
            RenderTargetBitmap targetBitmap = new RenderTargetBitmap((int)target.ActualWidth, (int)target.ActualHeight, 96d, 96d, PixelFormats.Default);
            targetBitmap.Render(drawingVisual);
            PngBitmapEncoder saveEncoder = new PngBitmapEncoder();
            saveEncoder.Frames.Add(BitmapFrame.Create(targetBitmap));
            string tempFile = path;
            System.IO.FileStream fs = System.IO.File.Open(tempFile, System.IO.FileMode.OpenOrCreate);
            saveEncoder.Save(fs);
            fs.Close();

        }


        public void SaveTorqueCurveImageWithInfo(
            int interval, int indexSSL, double Max_Torque,
            double Max_Angle, int type,
           double[] torque, double[] angle,
           string[] arrResultTK, string savePath, string fileName,
           string title = "TorqueVSAngle")
        {
            string EC = arrResultTK[3];
            double torqDrawF = Convert.ToDouble(arrResultTK[20]);      //Drop_torque上限值
            double torqDrawSSL = torque[indexSSL];   //SSL着座值
            double max = torque.Max();
            string[] items = new string[15];
            items[0] = "EC=" + arrResultTK[3];   //EC
            items[1] = "T1=" + arrResultTK[7];   //T1
            items[2] = "T2=" + arrResultTK[8];   //T2
            items[3] = "T3=" + arrResultTK[9];   //T3
            items[4] = "Tt=" + arrResultTK[11];  //Tt
            items[5] = "A1=" + arrResultTK[12];  //A1
            items[6] = "A2=" + arrResultTK[13];  //A2
            items[7] = "A3=" + arrResultTK[14];  //A3
            items[8] = "SSL=" + torqDrawSSL;    //SSL
            items[9] = "Max.Angle=" + Max_Angle;
            items[10] = "Max.Torque=" + Max_Torque;
            items[11] = "Seating torque=" + arrResultTK[32];  //
            items[12] = "Clamp torque=" + arrResultTK[34];
            items[13] = "Prevailing torque=" + arrResultTK[41];
            items[14] = "MC=" + arrResultTK[2];  //MC


            int length = torque.Length;
            double avgAngle = 0;    //用多個值累加避免只用最後一個值正負都有可能
            for (int i = 0; i < length; i++)
            {
                avgAngle += angle[i];
            }


            // 右侧数据区宽度
            int infoWidth = 220;
            int width = 900 + infoWidth, height = 600;
            int marginLeft = 60, marginRight = 30, marginTop = 50, marginBottom = 60;
            int plotWidth = 900 - marginLeft - marginRight;
            int plotHeight = height - marginTop - marginBottom;

            // 选择横轴和纵轴数据
            double[] xData = angle;
            double[] yData = torque;

            // 计算坐标轴范围
            //double xMin = xData.Min(), xMax = xData.Max();
            //double yMin = yData.Min(), yMax = yData.Max();
            double xMin = 0;
            double yMin = 0;
            // 角度 X 轴 / 扭矩 Y 轴上限自适应，避免硬编码导致数据压顶或分辨率浪费
            double xMax = ComputeAxisMaxFromZero(xData, fallbackMax: 2500, minStep: 200);
            double yMax = ComputeAxisMaxFromZero(yData, fallbackMax: 0.6, minStep: 0.05);

            // 防止极端数据导致坐标轴重叠
            if (xMax - xMin < 1e-6) { xMax = xMin + 1; }
            if (yMax - yMin < 1e-6) { yMax = yMin + 1; }

            using (var bitmap = new SKBitmap(width, height))
            using (var canvas = new SKCanvas(bitmap))
            {
                canvas.Clear(SKColors.White);

                // 坐标轴
                var axisPaint = new SKPaint { Color = SKColors.Black, StrokeWidth = 2, IsAntialias = true };
                // X轴
                //canvas.DrawLine(marginLeft, height - marginBottom, marginLeft + plotWidth, height - marginBottom, axisPaint);

                int angleStep = (int)Math.Max(100, NiceNumber(xMax / 6.0)); // 角度刻度自适应，约 6 个主刻度
                for (double x = Math.Ceiling(xMin / angleStep) * angleStep; x <= xMax; x += angleStep)
                {
                    float px = marginLeft + (float)((x - xMin) / (xMax - xMin) * plotWidth);
                    canvas.DrawLine(px, marginTop, px, height - marginBottom, new SKPaint { Color = SKColors.LightGray, StrokeWidth = 1 });
                    var tickPaint = new SKPaint { Color = SKColors.Black, TextSize = 16, IsAntialias = true };
                    canvas.DrawText(x.ToString("0"), px, height - marginBottom + 20, tickPaint);
                }
                // Y轴
                //canvas.DrawLine(marginLeft, height - marginBottom, marginLeft, marginTop, axisPaint);

                double yStep = Math.Max(0.05, NiceNumber(yMax / 5.0)); // 扭矩刻度自适应，约 5 个主刻度
                for (double y = Math.Ceiling(yMin / yStep) * yStep; y <= yMax; y += yStep)
                {
                    float py = height - marginBottom - (float)((y - yMin) / (yMax - yMin) * plotHeight);
                    // 分隔线
                    canvas.DrawLine(marginLeft, py, marginLeft + plotWidth, py, new SKPaint { Color = SKColors.LightGray, StrokeWidth = 1 });
                    // 刻度
                    var tickPaint = new SKPaint { Color = SKColors.Black, TextSize = 16, IsAntialias = true };
                    canvas.DrawText(y.ToString("0.0"), marginLeft - 45, py + 5, tickPaint);
                }

                // 标题
                var titlePaint = new SKPaint { Color = SKColors.Black, TextSize = 28, IsAntialias = true, TextAlign = SKTextAlign.Center };
                canvas.DrawText(title, (marginLeft + plotWidth) / 2, marginTop - 15, titlePaint);

                // 画单条曲线（蓝色）
                if (xData.Length > 1 && yData.Length == xData.Length)
                {
                    var paint = new SKPaint { Color = SKColors.Blue, StrokeWidth = 2, IsAntialias = true };
                    for (int i = 1; i < xData.Length; i++)
                    {
                        double xValue1, xValue2;
                        if (type == 0)
                        {
                            xValue1 = xData[i - 1];
                            xValue2 = xData[i];

                        }
                        else
                        {
                            xValue1 = Math.Abs(xData[i - 1]);
                            xValue2 = Math.Abs(xData[i]);
                        }
                        float x0 = marginLeft + (float)((xValue1 - xMin) / (xMax - xMin) * plotWidth);
                        float y0 = height - marginBottom - (float)((yData[i - 1] - yMin) / (yMax - yMin) * plotHeight);
                        float x1 = marginLeft + (float)((xValue2 - xMin) / (xMax - xMin) * plotWidth);
                        float y1 = height - marginBottom - (float)((yData[i] - yMin) / (yMax - yMin) * plotHeight);
                        canvas.DrawLine(x0, y0, x1, y1, paint);
                    }
                }

                //if (type != 0 && xData.Length > 1)
                //{
                //    var paint = new SKPaint { Color = SKColors.Black, StrokeWidth = 2, IsAntialias = true };

                //    // 根据 avgAngle 的值确定 Y 坐标
                //    double yValue = avgAngle > 0 ? torqDrawF : -torqDrawF;

                //    for (int i = 1; i < xData.Length; i++)
                //    {
                //        float x0 = marginLeft + (float)((xData[i - 1] - xMin) / (xMax - xMin) * plotWidth);
                //        float y0 = height - marginBottom - (float)((yValue - yMin) / (yMax - yMin) * plotHeight);
                //        float x1 = marginLeft + (float)((xData[i] - xMin) / (xMax - xMin) * plotWidth);
                //        float y1 = y0; // 横线的 Y 值保持不变

                //        canvas.DrawLine(x0, y0, x1, y1, paint);
                //    }
                //}

                //if (EC == "OK" && indexSSL < yData.Length)
                //{
                //    double xValue1 = 0, xValue2 = 0;
                //    var paint = new SKPaint { Color = SKColors.Black, StrokeWidth = 2, IsAntialias = true };
                //    for (int i = 1; i < indexSSL; i++)
                //    {

                //        if (type == 0)
                //        {
                //            xValue1 = interval * (i - 1);
                //            xValue2 = interval * i;
                //        }
                //        else
                //        {
                //            xValue1 = Math.Abs(xData[i - 1]);
                //            xValue2 = Math.Abs(xData[i]);
                //        }
                //        double yValue = yData[indexSSL];

                //        float x0 = marginLeft + (float)((xValue1 - xMin) / (xMax - xMin) * plotWidth);
                //        float y0 = height - marginBottom - (float)((yValue - yMin) / (yMax - yMin) * plotHeight);
                //        float x1 = marginLeft + (float)((xValue2 - xMin) / (xMax - xMin) * plotWidth);
                //        float y1 = y0;
                //        canvas.DrawLine(x0, y0, x1, y1, paint);
                //    }

                //    if (type != 0 && avgAngle > 0)
                //    {
                //        double torqDrawMinSeat = 0;   //最小着座值
                //        double angleDrawMinSeat = 0;   //最小着座值
                //        try
                //        {
                //            torqDrawMinSeat = Convert.ToDouble(arrResultTK[32]);   //最小着座值
                //            angleDrawMinSeat = Convert.ToDouble(arrResultTK[36]);   //最小着座值
                //        }
                //        catch { }
                //        if (torqDrawMinSeat != 0)
                //        {
                //            float x0 = marginLeft + (float)((xValue2 - xMin) / (xMax - xMin) * plotWidth);
                //            float y0 = height - marginBottom - (float)((yData[indexSSL] - yMin) / (yMax - yMin) * plotHeight);
                //            float x1 = marginLeft + (float)((angleDrawMinSeat - xMin) / (xMax - xMin) * plotWidth);
                //            float y1 = height - marginBottom - (float)((torqDrawMinSeat - yMin) / (yMax - yMin) * plotHeight);
                //            canvas.DrawLine(x0, y0, x1, y1, paint);
                //        }
                //    }

                //}

                // 图例
                var legendPaint = new SKPaint { Color = SKColors.Black, TextSize = 18, IsAntialias = true };
                int legendX = marginLeft + plotWidth - 160, legendY = marginTop + 10;
                canvas.DrawText("torque-angle", legendX, legendY, legendPaint);
                canvas.DrawLine(legendX + 100, legendY - 5, legendX + 130, legendY - 5, new SKPaint { Color = SKColors.Blue, StrokeWidth = 3 });

                // 坐标轴标签
                var labelPaint = new SKPaint { Color = SKColors.Black, TextSize = 18, IsAntialias = true };
                string xLabel = "angle/deg";
                canvas.DrawText(xLabel, marginLeft + plotWidth / 2, height - 20, labelPaint);

                // 主Y轴（torque）
                canvas.Save();
                canvas.RotateDegrees(-90, marginLeft - 40, height / 2);
                canvas.DrawText("torque/kgf.cm", marginLeft - 40, height / 2, labelPaint);
                canvas.Restore();


                // 右侧数据区
                var infoPaint = new SKPaint { Color = SKColors.Black, TextSize = 18, IsAntialias = true };
                int infoX = marginLeft + plotWidth + 30;
                int infoY = marginTop + 10;
                int lineHeight = 30;
                for (int i = 0; i < items.Length; i++)
                {
                    canvas.DrawText(items[i], infoX, infoY + i * lineHeight, infoPaint);
                }

                // 保存
                if (!Directory.Exists(savePath))
                    Directory.CreateDirectory(savePath);
                using (var image = SKImage.FromBitmap(bitmap))
                using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                using (var stream = File.OpenWrite(System.IO.Path.Combine(savePath, fileName + ".png")))
                {
                    data.SaveTo(stream);
                }
            }
        }

        public void SaveTimeCurveImageWithInfo(int interval,
          int indexSSL, double Max_Torque, double Max_Angle,
          double[] time, double[] torque, double[] angle, double[] press,
          string[] arrResultTK, string savePath, string fileName,
          string title = "Time-Press Curve")
        {
            double torqDrawSSL = torque[indexSSL];   //SSL着座值
            double maxPress = press.Max();
            string[] items = new string[16];
            items[0] = "EC=" + arrResultTK[3];   //EC
            items[1] = "T1=" + arrResultTK[7];   //T1
            items[2] = "T2=" + arrResultTK[8];   //T2
            items[3] = "T3=" + arrResultTK[9];   //T3
            items[4] = "Tt=" + arrResultTK[11];  //Tt
            items[5] = "A1=" + arrResultTK[12];  //A1
            items[6] = "A2=" + arrResultTK[13];  //A2
            items[7] = "A3=" + arrResultTK[14];  //A3
            items[8] = "SSL=" + torqDrawSSL;    //SSL
            items[9] = "Max.Angle=" + Max_Angle;
            items[10] = "Max.Torque=" + Max_Torque;
            items[11] = "Seating torque=" + arrResultTK[32];  //
            items[12] = "Clamp torque=" + arrResultTK[34];
            items[13] = "Prevailing torque=" + arrResultTK[41];
            items[14] = "MC=" + arrResultTK[2];  //MC
            items[15] = "MAX Press=" + maxPress.ToString();  //Max Press

            // 右侧数据区宽度
            int infoWidth = 220;
            int width = 900 + infoWidth, height = 600;
            int marginLeft = 60, marginRight = 30, marginTop = 50, marginBottom = 60;
            int plotWidth = 900 - marginLeft - marginRight;
            int plotHeight = height - marginTop - marginBottom;

            // 坐标轴范围
            //double timeMin = time.Min(), timeMax = time.Max();
            //double angleMin = angle.Min(), angleMax = angle.Max();
            //double torqueMin = torque.Min(), torqueMax = torque.Max();
            //double pressMin = press.Min(), pressMax = press.Max();

            // X轴时间最大值根据实际数据动态计算,与界面显示保持一致(向上取整到 500ms 倍数并预留 1 个刻度间隔的余量)
            const int timeAxisStep = 500;
            double timeMaxRaw = time != null && time.Length > 0 ? time.Max() : 0;
            double timeMin = 0, timeMax = Math.Ceiling((timeMaxRaw + timeAxisStep) / (double)timeAxisStep) * timeAxisStep;
            if (timeMax < timeAxisStep * 2) timeMax = timeAxisStep * 2;
            // 角度/扭矩/压力轴上限自适应：避免硬编码导致数据压顶或分辨率浪费
            double angleMin = 0, angleMax = ComputeAxisMaxFromZero(angle, fallbackMax: 3500, minStep: 200);
            double torqueMin = 0, torqueMax = ComputeAxisMaxFromZero(torque, fallbackMax: 0.5, minStep: 0.05);
            double pressMin = 0, pressMax = ComputeAxisMaxFromZero(press, fallbackMax: 2.5, minStep: 0.2);

            // 防止极端数据导致坐标轴重叠
            if (timeMax - timeMin < 1e-6) { timeMax = timeMin + 1; }
            if (angleMax - angleMin < 1e-6) { angleMax = angleMin + 1; }
            if (torqueMax - torqueMin < 1e-6) { torqueMax = torqueMin + 1; }
            if (pressMax - pressMin < 1e-6) { pressMax = pressMin + 1; }

            using (var bitmap = new SKBitmap(width, height))
            using (var canvas = new SKCanvas(bitmap))
            {
                canvas.Clear(SKColors.White);

                // 坐标轴画笔
                var axisPaint = new SKPaint { Color = SKColors.Black, StrokeWidth = 2, IsAntialias = true };

                // 画主X轴（time）分隔线和刻度
                int xStep = timeAxisStep; // 与界面 axis_x.MinStep 保持一致(500ms)
                for (double x = Math.Ceiling(timeMin / xStep) * xStep; x <= timeMax; x += xStep)
                {
                    float px = marginLeft + (float)((x - timeMin) / (timeMax - timeMin) * plotWidth);
                    // 分隔线
                    canvas.DrawLine(px, height - marginBottom, px, marginTop, new SKPaint { Color = SKColors.LightGray, StrokeWidth = 1 });
                    // 刻度
                    var tickPaint = new SKPaint { Color = SKColors.Black, TextSize = 16, IsAntialias = true };
                    canvas.DrawText(x.ToString("0"), px, height - marginBottom + 20, tickPaint);
                }

                //// 画主Y轴（torque）分隔线和刻度
                //double yStep = 0.2; // 可根据axis_y.MinStep调整
                //for (double y = Math.Ceiling(torqueMin / yStep) * yStep; y <= torqueMax; y += yStep)
                //{
                //    float py = height - marginBottom - (float)((y - torqueMin) / (torqueMax - torqueMin) * plotHeight);
                //    // 分隔线
                //    canvas.DrawLine(marginLeft, py, marginLeft + plotWidth, py, new SKPaint { Color = SKColors.LightGray, StrokeWidth = 1 });
                //    // 刻度
                //    var tickPaint = new SKPaint { Color = SKColors.Black, TextSize = 16, IsAntialias = true };
                //    canvas.DrawText(y.ToString("0.0"), marginLeft - 45, py + 5, tickPaint);
                //}

                //// 画次X轴（angle）分隔线和刻度
                //int angleStep = 200; // 可根据axis_xEnd.MinStep调整
                //for (double x = Math.Ceiling(angleMin / angleStep) * angleStep; x <= angleMax; x += angleStep)
                //{
                //    float px = marginLeft + (float)((x - angleMin) / (angleMax - angleMin) * plotWidth);
                //    canvas.DrawLine(px, marginTop, px, height - marginBottom, new SKPaint { Color = SKColors.LightGray, StrokeWidth = 1 });
                //    var tickPaint = new SKPaint { Color = SKColors.Black, TextSize = 16, IsAntialias = true };
                //    canvas.DrawText(x.ToString("0"), px, marginTop - 10, tickPaint);
                //}

                // 画次Y轴（press）分隔线和刻度
                double pressStep = 0.2; // 可根据axis_yEnd.MinStep调整
                for (double y = Math.Ceiling(pressMin / pressStep) * pressStep; y <= pressMax; y += pressStep)
                {
                    float py = height - marginBottom - (float)((y - pressMin) / (pressMax - pressMin) * plotHeight);
                    canvas.DrawLine(marginLeft, py, marginLeft + plotWidth, py, new SKPaint { Color = SKColors.LightGray, StrokeWidth = 1 });
                    var tickPaint = new SKPaint { Color = SKColors.Black, TextSize = 16, IsAntialias = true };
                    canvas.DrawText(y.ToString("0.0"), marginLeft + plotWidth + 10, py + 5, tickPaint);
                }

                // 标题（上移，避免与次X轴重叠）
                var titlePaint = new SKPaint { Color = SKColors.Black, TextSize = 28, IsAntialias = true, TextAlign = SKTextAlign.Center };
                canvas.DrawText(title, marginLeft + plotWidth / 2, marginTop - 30, titlePaint);

                // 画三条曲线
                //// 1. torque-angle（黑色，X:angle，Y:torque，映射到主Y轴和次X轴）
                //if (angle.Length > 1 && torque.Length == angle.Length)
                //{
                //    var paint = new SKPaint { Color = SKColors.Black, StrokeWidth = 2, IsAntialias = true };
                //    for (int i = 1; i < angle.Length; i++)
                //    {
                //        float x0 = marginLeft + (float)((angle[i - 1] - angleMin) / (angleMax - angleMin) * plotWidth);
                //        float y0 = height - marginBottom - (float)((torque[i - 1] - torqueMin) / (torqueMax - torqueMin) * plotHeight);
                //        float x1 = marginLeft + (float)((angle[i] - angleMin) / (angleMax - angleMin) * plotWidth);
                //        float y1 = height - marginBottom - (float)((torque[i] - torqueMin) / (torqueMax - torqueMin) * plotHeight);
                //        canvas.DrawLine(x0, y0, x1, y1, paint);
                //    }
                //}

                // 2. press-time（蓝色，X:time，Y:press，映射到主X轴和次Y轴）
                if (time.Length > 1 && press.Length == time.Length)
                {
                    var paint = new SKPaint { Color = SKColors.Blue, StrokeWidth = 2, IsAntialias = true };
                    for (int i = 1; i < time.Length; i++)
                    {
                        float x0 = marginLeft + (float)((time[i - 1] - timeMin) / (timeMax - timeMin) * plotWidth);
                        float y0 = height - marginBottom - (float)((press[i - 1] - pressMin) / (pressMax - pressMin) * plotHeight);
                        float x1 = marginLeft + (float)((time[i] - timeMin) / (timeMax - timeMin) * plotWidth);
                        float y1 = height - marginBottom - (float)((press[i] - pressMin) / (pressMax - pressMin) * plotHeight);
                        canvas.DrawLine(x0, y0, x1, y1, paint);
                    }
                }

                // 3. torque-time（绿色，X:time，Y:torque，映射到主X轴和主Y轴）
                // interval为采样周期，X轴为 interval*i
                //int minLen = Math.Min(time.Length, torque.Length);
                //if (minLen > 1)
                //{
                //    var paint = new SKPaint { Color = SKColors.Green, StrokeWidth = 2, IsAntialias = true };
                //    for (int i = 1; i < torque.Length; i++)
                //    {
                //        float x0 = marginLeft + (float)(((interval * (i - 1)) - timeMin) / (timeMax - timeMin) * plotWidth);
                //        float y0 = height - marginBottom - (float)((torque[i - 1] - torqueMin) / (torqueMax - torqueMin) * plotHeight);
                //        float x1 = marginLeft + (float)(((interval * i) - timeMin) / (timeMax - timeMin) * plotWidth);
                //        float y1 = height - marginBottom - (float)((torque[i] - torqueMin) / (torqueMax - torqueMin) * plotHeight);
                //        canvas.DrawLine(x0, y0, x1, y1, paint);
                //    }
                //}

                // 图例
                //var legendPaint = new SKPaint { Color = SKColors.Black, TextSize = 18, IsAntialias = true };
                //int legendX = marginLeft + plotWidth - 120, legendY = marginTop + 10;
                //canvas.DrawText("torque-angle", legendX, legendY, legendPaint);
                //canvas.DrawLine(legendX + 70, legendY - 5, legendX + 110, legendY - 5, new SKPaint { Color = SKColors.Black, StrokeWidth = 3 });
                //canvas.DrawText("press-time", legendX, legendY + 25, legendPaint);
                //canvas.DrawLine(legendX + 70, legendY + 20, legendX + 110, legendY + 20, new SKPaint { Color = SKColors.Blue, StrokeWidth = 3 });
                //canvas.DrawText("torque-time", legendX, legendY + 50, legendPaint);
                //canvas.DrawLine(legendX + 70, legendY + 45, legendX + 110, legendY + 45, new SKPaint { Color = SKColors.Green, StrokeWidth = 3 });

                // 坐标轴标签
                var labelPaint = new SKPaint { Color = SKColors.Black, TextSize = 18, IsAntialias = true };
                // 主X轴（time）
                canvas.DrawText("time/ms", marginLeft + plotWidth / 2, height - 20, labelPaint);
                // 主Y轴（torque）
                canvas.Save();
                //canvas.RotateDegrees(-90, marginLeft - 40, height / 2);
                //canvas.DrawText("torque/kgf.cm", marginLeft - 40, height / 2, labelPaint);
                //canvas.Restore();
                //// 次X轴（angle）标签（下移，避免与标题重叠）
                //canvas.DrawText("angle/deg", marginLeft + 30, marginTop - 25, labelPaint);
                //// 次Y轴（press）名称（左移，避免与右侧数据区重叠）
                //canvas.Save();
                canvas.RotateDegrees(-90, marginLeft + plotWidth + 50, height / 2);
                canvas.DrawText("press/kgf", marginLeft + plotWidth + 40, height / 2, labelPaint);
                canvas.Restore();

                // 右侧数据区
                var infoPaint = new SKPaint { Color = SKColors.Black, TextSize = 18, IsAntialias = true };
                int infoX = marginLeft + plotWidth + 70;
                int infoY = marginTop + 10;
                int lineHeight = 30;
                for (int i = 0; i < items.Length; i++)
                {
                    canvas.DrawText(items[i], infoX, infoY + i * lineHeight, infoPaint);
                }

                // 保存
                if (!Directory.Exists(savePath))
                    Directory.CreateDirectory(savePath);
                using (var image = SKImage.FromBitmap(bitmap))
                using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                using (var stream = File.OpenWrite(System.IO.Path.Combine(savePath, fileName + ".png")))
                {
                    data.SaveTo(stream);
                }
            }
        }

        /// <summary>
        /// 保存力控压力-时间 + 位置-时间曲线图（上下两个独立子图，使用SkiaSharp直接渲染）
        /// Y轴根据数据动态调节最大值和最小值（支持正负值）
        /// </summary>
        /// <param name="timeData">时间数据(ms)</param>
        /// <param name="pressData">压力数据(N)</param>
        /// <param name="posData">位置数据(mm)</param>
        /// <param name="savePath">保存路径</param>
        /// <param name="fileName">文件名(不含扩展名)</param>
        public void SavePressureCurveImageNew(
            double[] timeData, double[] pressData, double[] posData,
            string savePath, string fileName)
        {
            if (timeData == null || pressData == null || timeData.Length == 0 || pressData.Length == 0)
                return;

            bool hasPosData = posData != null && posData.Length > 0;

            // 布局：上下两个独立子图共享 X 轴（参考现场参考图样式）
            int width = 900;
            int marginLeft = 80, marginRight = 40, marginTop = 60, marginBottom = 60;
            int plotWidth = width - marginLeft - marginRight;
            int subPlotHeight = 220;             // 每个子图绘图区高度
            int subGap = 30;                     // 子图间距
            int totalPlotHeight = hasPosData ? (subPlotHeight * 2 + subGap) : subPlotHeight;
            int height = marginTop + totalPlotHeight + marginBottom;

            // X 轴范围（两个子图共享）
            double timeMax = timeData.Max();
            if (timeMax < 1e-6) timeMax = 1;
            double timeStep = NiceNumber(timeMax / 6.0);
            if (timeStep < 1) timeStep = 1;
            timeMax = Math.Ceiling(timeMax / timeStep) * timeStep;

            // Y 轴自适应
            ComputeNiceAxis(pressData, out double pressMin, out double pressMax, out double pressStep);
            double posMin = -1, posMax = 1, posStep = 0.5;
            if (hasPosData)
                ComputeNiceAxis(posData, out posMin, out posMax, out posStep);

            // 子图 Y 坐标
            int topY1 = marginTop;
            int botY1 = marginTop + subPlotHeight;
            int topY2 = botY1 + subGap;
            int botY2 = topY2 + subPlotHeight;

            using (var bitmap = new SKBitmap(width, height))
            using (var canvas = new SKCanvas(bitmap))
            {
                canvas.Clear(SKColors.White);

                var gridPaint = new SKPaint { Color = SKColors.Black, StrokeWidth = 1 };
                var tickPaint = new SKPaint { Color = SKColors.Black, TextSize = 14, IsAntialias = true };
                var labelPaint = new SKPaint { Color = SKColors.Black, TextSize = 16, IsAntialias = true };
                var titlePaint = new SKPaint { Color = SKColors.Black, TextSize = 20, IsAntialias = true };
                var curvePaintRed = new SKPaint { Color = SKColors.Red, StrokeWidth = 2, IsAntialias = true };
                var curvePaintBlue = new SKPaint { Color = SKColors.Blue, StrokeWidth = 2, IsAntialias = true };
                var axisPaint = new SKPaint { Color = SKColors.Black, StrokeWidth = 2, IsAntialias = true };

                // 总标题
                canvas.DrawText("Force_Arc&&Move_Arc", marginLeft + plotWidth / 2 - 120, marginTop - 25, titlePaint);

                // 上方子图：Force（不画 X 轴刻度数字）
                DrawPressureSubPlot(canvas, timeData, pressData,
                    marginLeft, plotWidth, topY1, botY1,
                    timeMax, timeStep,
                    pressMin, pressMax, pressStep,
                    "Force(Kgf)", "Force_Arc",
                    curvePaintRed, gridPaint, tickPaint, labelPaint, axisPaint,
                    drawXLabels: false);

                // 下方子图：Move（带 X 轴刻度数字和标签）
                if (hasPosData)
                {
                    DrawPressureSubPlot(canvas, timeData, posData,
                        marginLeft, plotWidth, topY2, botY2,
                        timeMax, timeStep,
                        posMin, posMax, posStep,
                        "Move(mm)", "Move_Arc",
                        curvePaintBlue, gridPaint, tickPaint, labelPaint, axisPaint,
                        drawXLabels: true);
                }
                else
                {
                    // 没有 pos 数据：直接给上方子图标 X 轴
                    canvas.DrawText("Time(ms)", marginLeft + plotWidth / 2 - 30, botY1 + 42, labelPaint);
                }

                // 保存JPEG
                if (!Directory.Exists(savePath))
                    Directory.CreateDirectory(savePath);
                using (var image = SKImage.FromBitmap(bitmap))
                using (var data = image.Encode(SKEncodedImageFormat.Jpeg, 90))
                using (var stream = File.OpenWrite(System.IO.Path.Combine(savePath, fileName + ".jpeg")))
                {
                    data.SaveTo(stream);
                }
            }
        }

        /// 保存力控压力-时间 + 位置-时间曲线图（总图）
        /// Y轴根据数据动态调节最大值和最小值（支持正负值）
        /// </summary>
        /// <param name="timeData">时间数据(ms)</param>
        /// <param name="pressData">压力数据(N)</param>
        /// <param name="posData">位置数据(mm)</param>
        /// <param name="savePath">保存路径</param>
        /// <param name="fileName">文件名(不含扩展名)</param>
        public void SavePressureCurveImage(
            double[] timeData, double[] pressData, double[] posData,
            string savePath, string fileName)
        {
            if (timeData == null || pressData == null || timeData.Length == 0 || pressData.Length == 0)
                return;

            bool hasPosData = posData != null && posData.Length > 0;

            int width = 900;
            int marginLeft = 70, marginRight = hasPosData ? 70 : 30, marginTop = 40, marginBottom = 50;
            int plotWidth = width - marginLeft - marginRight;
            int plotHeight = 450;
            int height = marginTop + plotHeight + marginBottom;

            double timeMin = 0;
            double timeMax = timeData.Max();
            if (timeMax - timeMin < 1e-6) timeMax = timeMin + 1;

            // 固定静态Y轴范围：左右轴均关于0对称，保证原点(0)落在同一水平线上
            // 左轴(Force)
            double pressMax = 0.5;
            double pressMin = -pressMax;
            double pressStep = 0.1;         // 左轴固定刻度
            // 右轴(Move)
            double posMax = 8.0;
            double posMin = -posMax;
            double posStep = 2.0;           // 右轴固定刻度

            // X轴(时间)固定刻度（timeMax仍取自数据以容纳全部曲线，避免截断）
            double timeStep = 500;
            timeMax = Math.Ceiling(timeMax / timeStep) * timeStep;

            using (var bitmap = new SKBitmap(width, height))
            using (var canvas = new SKCanvas(bitmap))
            {
                canvas.Clear(SKColors.White);

                var gridPaint = new SKPaint { Color = SKColors.Black, StrokeWidth = 1 };
                var tickPaint = new SKPaint { Color = SKColors.Black, TextSize = 14, IsAntialias = true };
                var tickPaintBlue = new SKPaint { Color = SKColors.Black, TextSize = 14, IsAntialias = true };
                var tickPaintRed = new SKPaint { Color = SKColors.Black, TextSize = 14, IsAntialias = true };
                var labelPaint = new SKPaint { Color = SKColors.Black, TextSize = 16, IsAntialias = true };
                var labelPaintRed = new SKPaint { Color = SKColors.Black, TextSize = 16, IsAntialias = true };
                var curvePaint = new SKPaint { Color = SKColors.Blue, StrokeWidth = 2, IsAntialias = true };
                var curvePaintRed = new SKPaint { Color = SKColors.Red, StrokeWidth = 2, IsAntialias = true };
                var axisPaint = new SKPaint { Color = SKColors.Black, StrokeWidth = 2, IsAntialias = true };
                var axisPaintBlue = new SKPaint { Color = SKColors.Black, StrokeWidth = 2, IsAntialias = true };
                var axisPaintRed = new SKPaint { Color = SKColors.Black, StrokeWidth = 2, IsAntialias = true };

                int topY = marginTop;
                int botY = marginTop + plotHeight;

                // ===== 标题 =====
                var titlePaint = new SKPaint { Color = SKColors.Black, TextSize = 20, IsAntialias = true };
                canvas.DrawText("Force_Arc&&Move_Arc", marginLeft + plotWidth / 2 - 120, topY - 10, titlePaint);

                // ===== X轴网格和刻度 =====
                for (double x = 0; x <= timeMax; x += timeStep)
                {
                    float px = marginLeft + (float)(x / timeMax * plotWidth);
                    canvas.DrawLine(px, topY, px, botY, gridPaint);
                    canvas.DrawLine(px, botY, px, botY + 5, axisPaint);   // X轴主刻度线
                    canvas.DrawText(x.ToString("0"), px - 10, botY + 18, tickPaint);
                }

                // X轴次刻度线（细分）
                double timeMinorStep = 100;
                for (double x = 0; x <= timeMax + 1e-9; x += timeMinorStep)
                {
                    if (Math.Abs(x - Math.Round(x / timeStep) * timeStep) < 1e-6) continue; // 跳过主刻度
                    float px = marginLeft + (float)(x / timeMax * plotWidth);
                    canvas.DrawLine(px, botY, px, botY + 3, axisPaint);
                }

                // ===== 左Y轴（压力）网格和刻度 =====
                for (double y = Math.Ceiling(pressMin / pressStep) * pressStep; y <= pressMax + 1e-9; y += pressStep)
                {
                    float py = botY - (float)((y - pressMin) / (pressMax - pressMin) * plotHeight);
                    canvas.DrawLine(marginLeft, py, marginLeft + plotWidth, py, gridPaint);
                    canvas.DrawLine(marginLeft - 5, py, marginLeft, py, axisPaint);   // 左Y轴主刻度线
                    canvas.DrawText(y.ToString("0.##"), marginLeft - 50, py + 5, tickPaint);
                }

                // 左Y轴次刻度线（细分）
                double pressMinorStep = 0.02;
                for (double y = pressMin; y <= pressMax + 1e-9; y += pressMinorStep)
                {
                    if (Math.Abs(y - Math.Round(y / pressStep) * pressStep) < 1e-6) continue;
                    float py = botY - (float)((y - pressMin) / (pressMax - pressMin) * plotHeight);
                    canvas.DrawLine(marginLeft - 3, py, marginLeft, py, axisPaint);
                }

                var yLabelPaint = new SKPaint { Color = SKColors.Black, TextSize = 16, IsAntialias = true };

                // 左Y轴标签
                canvas.Save();
                canvas.RotateDegrees(-90, marginLeft - 55, topY + plotHeight / 2);
                canvas.DrawText("Force(Kgf)", marginLeft - 55, topY + plotHeight / 2, yLabelPaint);
                canvas.Restore();

                // ===== 右Y轴（位置）刻度 =====
                if (hasPosData)
                {
                    for (double y = Math.Ceiling(posMin / posStep) * posStep; y <= posMax + 1e-9; y += posStep)
                    {
                        float py = botY - (float)((y - posMin) / (posMax - posMin) * plotHeight);
                        canvas.DrawLine(marginLeft + plotWidth, py, marginLeft + plotWidth + 5, py, axisPaint);   // 右Y轴主刻度线
                        canvas.DrawText(y.ToString("0.##"), marginLeft + plotWidth + 5, py + 5, tickPaint);
                    }

                    // 右Y轴次刻度线（细分）
                    double posMinorStep = 0.5;
                    for (double y = posMin; y <= posMax + 1e-9; y += posMinorStep)
                    {
                        if (Math.Abs(y - Math.Round(y / posStep) * posStep) < 1e-6) continue;
                        float py = botY - (float)((y - posMin) / (posMax - posMin) * plotHeight);
                        canvas.DrawLine(marginLeft + plotWidth, py, marginLeft + plotWidth + 3, py, axisPaint);
                    }

                    // 右Y轴标签
                    canvas.Save();
                    canvas.RotateDegrees(90, marginLeft + plotWidth + 55, topY + plotHeight / 2);
                    canvas.DrawText("Move(mm)", marginLeft + plotWidth + 55, topY + plotHeight / 2, yLabelPaint);
                    canvas.Restore();
                }

                // ===== 坐标轴线 =====
                // 左Y轴（蓝色）
                canvas.DrawLine(marginLeft, topY, marginLeft, botY, axisPaint);
                // X轴（黑色）
                canvas.DrawLine(marginLeft, botY, marginLeft + plotWidth, botY, axisPaint);
                // 右Y轴（红色）
                if (hasPosData)
                    canvas.DrawLine(marginLeft + plotWidth, topY, marginLeft + plotWidth, botY, axisPaint);

                // X轴标签
                var xLabelPaint = new SKPaint { Color = SKColors.Black, TextSize = 16, IsAntialias = true };
                canvas.DrawText("Time(ms)", marginLeft + plotWidth / 2 - 30, botY + 40, xLabelPaint);

                // ===== 画压力曲线（红色） =====
                if (timeData.Length > 1 && pressData.Length == timeData.Length)
                {
                    for (int i = 1; i < timeData.Length; i++)
                    {
                        float x0 = marginLeft + (float)((timeData[i - 1]) / timeMax * plotWidth);
                        float y0 = botY - (float)((pressData[i - 1] - pressMin) / (pressMax - pressMin) * plotHeight);
                        float x1 = marginLeft + (float)((timeData[i]) / timeMax * plotWidth);
                        float y1 = botY - (float)((pressData[i] - pressMin) / (pressMax - pressMin) * plotHeight);
                        canvas.DrawLine(x0, y0, x1, y1, curvePaintRed);
                    }
                }

                // ===== 画位置曲线（蓝色） =====
                if (hasPosData)
                {
                    int posLen = Math.Min(timeData.Length, posData.Length);
                    if (posLen > 1)
                    {
                        for (int i = 1; i < posLen; i++)
                        {
                            float x0 = marginLeft + (float)((timeData[i - 1]) / timeMax * plotWidth);
                            float y0 = botY - (float)((posData[i - 1] - posMin) / (posMax - posMin) * plotHeight);
                            float x1 = marginLeft + (float)((timeData[i]) / timeMax * plotWidth);
                            float y1 = botY - (float)((posData[i] - posMin) / (posMax - posMin) * plotHeight);
                            canvas.DrawLine(x0, y0, x1, y1, curvePaint);
                        }
                    }
                }

                // ===== 图例 =====
                if (hasPosData)
                {
                    float legendX = marginLeft + plotWidth - 180;
                    float legendY = topY + 20;
                    var legendBgPaint = new SKPaint { Color = new SKColor(255, 255, 255, 220), IsAntialias = true };
                    var legendBorderPaint = new SKPaint { Color = SKColors.Gray, StrokeWidth = 1, IsAntialias = true, Style = SKPaintStyle.Stroke };
                    canvas.DrawRect(legendX - 10, legendY - 15, 180, 45, legendBgPaint);
                    canvas.DrawRect(legendX - 10, legendY - 15, 180, 45, legendBorderPaint);
                    // 压力图例
                    canvas.DrawLine(legendX, legendY, legendX + 25, legendY, curvePaintRed);
                    canvas.DrawText("Force_Arc", legendX + 30, legendY + 5, tickPaintRed);
                    // 位置图例
                    canvas.DrawLine(legendX, legendY + 20, legendX + 25, legendY + 20, curvePaint);
                    canvas.DrawText("Move_Arc", legendX + 30, legendY + 25, tickPaintBlue);
                }

                // 保存JPEG
                if (!Directory.Exists(savePath))
                    Directory.CreateDirectory(savePath);
                using (var image = SKImage.FromBitmap(bitmap))
                using (var data = image.Encode(SKEncodedImageFormat.Jpeg, 90))
                using (var stream = File.OpenWrite(System.IO.Path.Combine(savePath, fileName + ".jpeg")))
                {
                    data.SaveTo(stream);
                }
            }
        }

        /// <summary>
        /// 绘制单个子图（Y轴+X轴网格+曲线）。drawXLabels 控制是否绘制X轴刻度数字（仅最下层子图绘制）。
        /// </summary>
        private static void DrawPressureSubPlot(
            SKCanvas canvas, double[] timeData, double[] valueData,
            int marginLeft, int plotWidth, int topY, int botY,
            double timeMax, double timeStep,
            double vMin, double vMax, double vStep,
            string yLabel, string curveName,
            SKPaint curvePaint, SKPaint gridPaint, SKPaint tickPaint, SKPaint labelPaint, SKPaint axisPaint,
            bool drawXLabels)
        {
            int plotHeight = botY - topY;
            double vRange = vMax - vMin;
            if (vRange < 1e-9) vRange = 1;

            // ===== Y 轴主刻度 + 网格 =====
            for (double y = Math.Ceiling(vMin / vStep) * vStep; y <= vMax + 1e-9; y += vStep)
            {
                float py = botY - (float)((y - vMin) / vRange * plotHeight);
                canvas.DrawLine(marginLeft, py, marginLeft + plotWidth, py, gridPaint);
                canvas.DrawLine(marginLeft - 5, py, marginLeft, py, axisPaint);
                canvas.DrawText(y.ToString("0.###"), marginLeft - 55, py + 5, tickPaint);
            }

            // Y 轴次刻度
            double vMinorStep = vStep / 5.0;
            for (double y = vMin; y <= vMax + 1e-9; y += vMinorStep)
            {
                if (Math.Abs(y - Math.Round(y / vStep) * vStep) < 1e-6) continue;
                float py = botY - (float)((y - vMin) / vRange * plotHeight);
                canvas.DrawLine(marginLeft - 3, py, marginLeft, py, axisPaint);
            }

            // Y 轴标签（旋转 -90）
            canvas.Save();
            canvas.RotateDegrees(-90, marginLeft - 60, topY + plotHeight / 2);
            canvas.DrawText(yLabel, marginLeft - 60, topY + plotHeight / 2, labelPaint);
            canvas.Restore();

            // 子图标题（左上角）
            canvas.DrawText(curveName, marginLeft + 8, topY + 18, tickPaint);

            // ===== X 轴主刻度 + 网格（贯穿子图） =====
            for (double x = 0; x <= timeMax + 1e-9; x += timeStep)
            {
                float px = marginLeft + (float)(x / timeMax * plotWidth);
                canvas.DrawLine(px, topY, px, botY, gridPaint);
                canvas.DrawLine(px, botY, px, botY + 5, axisPaint);
                if (drawXLabels)
                    canvas.DrawText(x.ToString("0"), px - 10, botY + 20, tickPaint);
            }

            // X 轴次刻度
            double timeMinorStep = timeStep / 5.0;
            for (double x = 0; x <= timeMax + 1e-9; x += timeMinorStep)
            {
                if (Math.Abs(x - Math.Round(x / timeStep) * timeStep) < 1e-6) continue;
                float px = marginLeft + (float)(x / timeMax * plotWidth);
                canvas.DrawLine(px, botY, px, botY + 3, axisPaint);
            }

            // 坐标轴边框（左 Y 轴 + X 轴 + 顶边框 + 右边框）
            canvas.DrawLine(marginLeft, topY, marginLeft, botY, axisPaint);
            canvas.DrawLine(marginLeft, botY, marginLeft + plotWidth, botY, axisPaint);
            canvas.DrawLine(marginLeft, topY, marginLeft + plotWidth, topY, axisPaint);
            canvas.DrawLine(marginLeft + plotWidth, topY, marginLeft + plotWidth, botY, axisPaint);

            // X 轴标签（仅最下层）
            if (drawXLabels)
                canvas.DrawText("Time(ms)", marginLeft + plotWidth / 2 - 30, botY + 42, labelPaint);

            // ===== 曲线 =====
            int len = Math.Min(timeData.Length, valueData.Length);
            if (len > 1)
            {
                for (int i = 1; i < len; i++)
                {
                    float x0 = marginLeft + (float)(timeData[i - 1] / timeMax * plotWidth);
                    float y0 = botY - (float)((valueData[i - 1] - vMin) / vRange * plotHeight);
                    float x1 = marginLeft + (float)(timeData[i] / timeMax * plotWidth);
                    float y1 = botY - (float)((valueData[i] - vMin) / vRange * plotHeight);
                    canvas.DrawLine(x0, y0, x1, y1, curvePaint);
                }
            }
        }

        /// <summary>
        /// 根据数据实际范围计算"漂亮"的Y轴范围与刻度步长，使曲线占满绘图区。
        /// 算法：取数据[min,max] -> 加10%余量 -> 用nice-number计算step -> 对齐到step整数倍。
        /// </summary>
        private static void ComputeNiceAxis(double[] data, out double min, out double max, out double step)
        {
            if (data == null || data.Length == 0)
            {
                min = -1; max = 1; step = 0.5;
                return;
            }
            double dMin = data.Min();
            double dMax = data.Max();
            double range = dMax - dMin;
            if (range < 1e-9)
            {
                // 退化情况：所有值相同。用绝对值构造一个对称小范围
                double absV = Math.Max(Math.Abs(dMax), 1e-3);
                dMin = -absV;
                dMax = absV;
                range = dMax - dMin;
            }
            double pad = range * 0.1;
            double looseMin = dMin - pad;
            double looseMax = dMax + pad;

            // 取5个主刻度左右
            step = NiceNumber((looseMax - looseMin) / 5.0);
            if (step < 1e-9) step = 1;

            min = Math.Floor(looseMin / step) * step;
            max = Math.Ceiling(looseMax / step) * step;

            // 至少包含0线（避免数据全正或全负时0线被切掉）
            if (min > 0) min = 0;
            if (max < 0) max = 0;
        }

        /// <summary>
        /// 计算"漂亮的"刻度间隔：1, 2, 5 × 10^n 形式。
        /// </summary>
        private static double NiceNumber(double x)
        {
            if (x <= 0) return 1;
            double exp = Math.Floor(Math.Log10(x));
            double f = x / Math.Pow(10, exp);
            double nf;
            if (f < 1.5) nf = 1;
            else if (f < 3) nf = 2;
            else if (f < 7) nf = 5;
            else nf = 10;
            return nf * Math.Pow(10, exp);
        }

        /// <summary>
        /// 从 0 开始计算 Y/X 轴的上限：基于数据最大值 + 10% 余量，向上取整到 NiceNumber 步长的整数倍。
        /// 用于扭矩/压力/角度/时间等非负数据的坐标轴自适应，避免硬编码上限导致曲线压顶或分辨率浪费。
        /// </summary>
        /// <param name="data">实际数据数组（扭矩 / 压力 / 角度 / 时间等）</param>
        /// <param name="fallbackMax">数据为空或全 0 时使用的默认上限</param>
        /// <param name="minStep">最小刻度步长，避免刻度太密（如扭矩 0.1、压力 0.2、角度 200、时间 500）</param>
        /// <returns>轴上限（>= minStep * 2）</returns>
        private static double ComputeAxisMaxFromZero(double[] data, double fallbackMax, double minStep)
        {
            double dMax = (data != null && data.Length > 0) ? data.Max() : 0;
            if (dMax <= 0) dMax = fallbackMax;
            double pad = dMax * 0.1;                       // 10% 顶部余量
            double looseMax = dMax + pad;
            double step = NiceNumber(looseMax / 5.0);      // 约 5 个主刻度
            if (step < minStep) step = minStep;
            double max = Math.Ceiling(looseMax / step) * step;
            if (max < step * 2) max = step * 2;            // 至少留出 2 个刻度的可视空间
            return max;
        }


        public void SaveToeinForceCurveImage(
             double[] ToeinXForce, double[] ToeinYForce, double[] ToeinZForce, double[] Time,
             string savePath, string fileName, string title = "Toein Force Curve")
        {
            int width = 900, height = 600;
            int marginLeft = 60, marginRight = 30, marginTop = 50, marginBottom = 60;
            int plotWidth = width - marginLeft - marginRight;
            int plotHeight = height - marginTop - marginBottom;

            double timeMin = 0, timeMax = Time.Length > 0 ? Time.Max() : 1000;
            double forceMin = 0, forceMax = Math.Max(
                Math.Max(ToeinXForce.Max(), ToeinYForce.Max()), ToeinZForce.Max());
            forceMax = Math.Ceiling(forceMax * 1.1);

            if (timeMax - timeMin < 1e-6) { timeMax = timeMin + 1; }
            if (forceMax - forceMin < 1e-6) { forceMax = forceMin + 1; }

            using (var bitmap = new SKBitmap(width, height))
            using (var canvas = new SKCanvas(bitmap))
            {
                canvas.Clear(SKColors.White);

                // X轴分隔线和刻度
                int xStep = 1000;
                for (double x = Math.Ceiling((timeMin / xStep) * xStep); x <= timeMax; x += xStep)
                {
                    double b = (x - timeMin) / (timeMax - timeMin) * plotWidth;
                    float c = (float)((x - timeMin) / (timeMax - timeMin) * plotWidth);
                    float px = marginLeft + (float)((x - timeMin) / (timeMax - timeMin) * plotWidth);
                    canvas.DrawLine(px, marginTop, px, height - marginBottom, new SKPaint { Color = SKColors.LightGray, StrokeWidth = 1 });
                    var tickPaint = new SKPaint { Color = SKColors.Black, TextSize = 16, IsAntialias = true };
                    canvas.DrawText((x / 1000).ToString("0.0"), px, height - marginBottom + 20, tickPaint);
                }

                // Y轴分隔线和刻度
                double yStep = Math.Max(1, Math.Ceiling((forceMax - forceMin) / 10));
                for (double y = Math.Ceiling(forceMin / yStep) * yStep; y <= forceMax; y += yStep)
                {
                    float py = height - marginBottom - (float)((y - forceMin) / (forceMax - forceMin) * plotHeight);
                    canvas.DrawLine(marginLeft, py, marginLeft + plotWidth, py, new SKPaint { Color = SKColors.LightGray, StrokeWidth = 1 });
                    var tickPaint = new SKPaint { Color = SKColors.Black, TextSize = 16, IsAntialias = true };
                    canvas.DrawText(y.ToString("0.0"), marginLeft - 45, py + 5, tickPaint);
                }

                // 标题
                var titlePaint = new SKPaint { Color = SKColors.Black, TextSize = 28, IsAntialias = true, TextAlign = SKTextAlign.Center };
                canvas.DrawText(title, marginLeft + plotWidth / 2, marginTop - 15, titlePaint);

                // 三条曲线
                if (Time.Length > 1)
                {
                    // X Force - 蓝色
                    var paintX = new SKPaint { Color = SKColors.Blue, StrokeWidth = 2, IsAntialias = true };
                    for (int i = 1; i < Time.Length; i++)
                    {
                        float x0 = marginLeft + (float)((Time[i - 1] - timeMin) / (timeMax - timeMin) * plotWidth);
                        float y0 = height - marginBottom - (float)((ToeinXForce[i - 1] - forceMin) / (forceMax - forceMin) * plotHeight);
                        float x1 = marginLeft + (float)((Time[i] - timeMin) / (timeMax - timeMin) * plotWidth);
                        float y1 = height - marginBottom - (float)((ToeinXForce[i] - forceMin) / (forceMax - forceMin) * plotHeight);
                        canvas.DrawLine(x0, y0, x1, y1, paintX);
                    }
                    // Y Force - 绿色
                    var paintY = new SKPaint { Color = SKColors.Green, StrokeWidth = 2, IsAntialias = true };
                    for (int i = 1; i < Time.Length; i++)
                    {
                        float x0 = marginLeft + (float)((Time[i - 1] - timeMin) / (timeMax - timeMin) * plotWidth);
                        float y0 = height - marginBottom - (float)((ToeinYForce[i - 1] - forceMin) / (forceMax - forceMin) * plotHeight);
                        float x1 = marginLeft + (float)((Time[i] - timeMin) / (timeMax - timeMin) * plotWidth);
                        float y1 = height - marginBottom - (float)((ToeinYForce[i] - forceMin) / (forceMax - forceMin) * plotHeight);
                        canvas.DrawLine(x0, y0, x1, y1, paintY);
                    }
                    // Z Force - 黑色
                    var paintZ = new SKPaint { Color = SKColors.Black, StrokeWidth = 2, IsAntialias = true };
                    for (int i = 1; i < Time.Length; i++)
                    {
                        float x0 = marginLeft + (float)((Time[i - 1] - timeMin) / (timeMax - timeMin) * plotWidth);
                        float y0 = height - marginBottom - (float)((ToeinZForce[i - 1] - forceMin) / (forceMax - forceMin) * plotHeight);
                        float x1 = marginLeft + (float)((Time[i] - timeMin) / (timeMax - timeMin) * plotWidth);
                        float y1 = height - marginBottom - (float)((ToeinZForce[i] - forceMin) / (forceMax - forceMin) * plotHeight);
                        canvas.DrawLine(x0, y0, x1, y1, paintZ);
                    }
                }

                // 图例
                var legendPaint = new SKPaint { Color = SKColors.Black, TextSize = 18, IsAntialias = true };
                int legendX = marginLeft + plotWidth - 160, legendY = marginTop + 10;
                canvas.DrawText("X Force", legendX, legendY, legendPaint);
                canvas.DrawLine(legendX + 80, legendY - 5, legendX + 120, legendY - 5, new SKPaint { Color = SKColors.Blue, StrokeWidth = 3 });
                canvas.DrawText("Y Force", legendX, legendY + 25, legendPaint);
                canvas.DrawLine(legendX + 80, legendY + 20, legendX + 120, legendY + 20, new SKPaint { Color = SKColors.Green, StrokeWidth = 3 });
                canvas.DrawText("Z Force", legendX, legendY + 50, legendPaint);
                canvas.DrawLine(legendX + 80, legendY + 45, legendX + 120, legendY + 45, new SKPaint { Color = SKColors.Black, StrokeWidth = 3 });

                // 坐标轴标签
                var labelPaint = new SKPaint { Color = SKColors.Black, TextSize = 18, IsAntialias = true };
                canvas.DrawText("Time/s", marginLeft + plotWidth / 2, height - 20, labelPaint);
                canvas.Save();
                canvas.RotateDegrees(-90, marginLeft - 40, height / 2);
                canvas.DrawText("Force/N", marginLeft - 40, height / 2, labelPaint);
                canvas.Restore();

                // 保存
                if (!Directory.Exists(savePath))
                    Directory.CreateDirectory(savePath);
                using (var image = SKImage.FromBitmap(bitmap))
                using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                using (var stream = File.OpenWrite(System.IO.Path.Combine(savePath, fileName + ".png")))
                {
                    data.SaveTo(stream);
                }
            }
        }
    }
}
