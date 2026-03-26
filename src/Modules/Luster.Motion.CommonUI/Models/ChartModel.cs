#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ChartModel
* 机器名称:       L05590
* 命名空间:       Luster.Motion.CommonUI.Models
* 文 件 名:       ChartModel.cs
* 创建时间:       2022/10/25 13:54:17
* 作    者:       L05590
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       fanlu5590@lusterinc.com 
* 唯一标识：      a8d09b63-13ce-4e08-ba60-2865c6d03de8
* 登录用户:       fanlu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/10/25 13:54:17
* 修 改 人:		  L05590
************************************************************************************/
#endregion

using LiveCharts;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.CommonUI.Models
{
    public class ChartModel : BindableBase
    {
        /// <summary>
        /// 曲线集合
        /// </summary>
        private SeriesCollection _srCollection;
        public SeriesCollection SrCollection
        {
            get { return _srCollection; }
            set { SetProperty(ref _srCollection, value); }
        }

        private ISeries[] _srSeries;
        public ISeries[] SrSeries
        {
            get { return _srSeries; }
            set { SetProperty(ref _srSeries, value); }
        }
        /// <summary>
        /// X轴标签样式
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


        /// <summary>
        /// 名称
        /// </summary>
        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        private Func<double, string> _yFormatter;
        public Func<double, string> YFormatter
        {
            get { return _yFormatter; }
            set { SetProperty(ref _yFormatter, value); }
        }

        /// <summary>
        /// 宽度
        /// </summary>
        private double _width;
        public double Width
        {
            get { return _width; }
            set { SetProperty(ref _width, value); }
        }

        /// <summary>
        /// 高度
        /// </summary>
        private double _height;
        public double Height
        {
            get { return _height; }
            set { SetProperty(ref _height, value); }
        }

        /// <summary>
        /// 上限
        /// </summary>
        private double _upperLimit;
        public double UpperLimit
        {
            get { return _upperLimit; }
            set { SetProperty(ref _upperLimit, value); }
        }

        /// <summary>
        /// 下限
        /// </summary>
        private double _lowerLimit;
        public double LowerLimit
        {
            get { return _lowerLimit; }
            set { SetProperty(ref _lowerLimit, value); }
        }

        /// <summary>
        /// 标准值
        /// </summary>
        private double _standardValue;
        public double StandardValue
        {
            get { return _standardValue; }
            set { SetProperty(ref _standardValue, value); }
        }


        /// <summary>
        /// 间距
        /// </summary>
        private double _spaceFactor;
        public double SpaceFactor
        {
            get { return _spaceFactor; }
            set { SetProperty(ref _spaceFactor, value); }
        }




        /// <summary>
        /// x轴数据
        /// </summary>
        private string[] _xLabels;
        public string[] XLabels
        {
            get { return _xLabels; }
            set { SetProperty(ref _xLabels, value); }
        }


        /// <summary>
        /// 图例样式
        /// </summary>
        private SolidColorPaint _legendTextPaint;
        public SolidColorPaint LegendTextPaint
        {
            get { return _legendTextPaint; }
            set { SetProperty(ref _legendTextPaint, value); }
        }

        /// <summary>
        /// tooltip
        /// </summary>
        private SolidColorPaint _tooltipTextPaint;
        public SolidColorPaint TooltipTextPaint
        {
            get { return _tooltipTextPaint; }
            set { SetProperty(ref _tooltipTextPaint, value); }
        }

    }
}