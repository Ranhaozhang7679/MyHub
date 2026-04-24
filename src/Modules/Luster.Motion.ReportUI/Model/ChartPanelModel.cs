using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.Defaults;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Luster.Motion.ReportUI.Model
{
    /// <summary>
    /// 图表面板模型，表示单个图表面板的完整状态
    /// 包含：图表系列、坐标轴、步骤配置、数据缓存
    /// </summary>
    public class ChartPanelModel : BindableBase
    {
        private string _title = "图表";
        /// <summary>
        /// 面板标题
        /// </summary>
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private string _csvFileName;
        /// <summary>
        /// 对应的 CSV 文件名（不含扩展名），用于步骤配置映射
        /// </summary>
        public string CsvFileName
        {
            get => _csvFileName;
            set => SetProperty(ref _csvFileName, value);
        }

        private List<ISeries> _series = new List<ISeries>();
        /// <summary>
        /// 图表系列数据
        /// </summary>
        public List<ISeries> Series
        {
            get => _series;
            set => SetProperty(ref _series, value);
        }

        private List<Axis> _xAxes = new List<Axis> { new Axis() };
        /// <summary>
        /// X 轴配置
        /// </summary>
        public List<Axis> XAxes
        {
            get => _xAxes;
            set => SetProperty(ref _xAxes, value);
        }

        private List<Axis> _yAxes = new List<Axis> { new Axis() };
        /// <summary>
        /// Y 轴配置
        /// </summary>
        public List<Axis> YAxes
        {
            get => _yAxes;
            set => SetProperty(ref _yAxes, value);
        }

        /// <summary>
        /// 步骤标注配置列表（可内联编辑）
        /// </summary>
        public ObservableCollection<StepAnnotationConfigModel> Steps { get; }
            = new ObservableCollection<StepAnnotationConfigModel>();

        /// <summary>
        /// 原始曲线数据缓存（每条曲线的点集合），步骤变更时用于重绘
        /// </summary>
        internal List<List<ObservablePoint>> RawDataCache { get; } = new List<List<ObservablePoint>>();

        /// <summary>
        /// 缓存的时间最大值
        /// </summary>
        internal double CachedTimeMax { get; set; } = 1000;

        /// <summary>
        /// 缓存的压力最大值
        /// </summary>
        internal double CachedPressMax { get; set; } = 1.0;

        /// <summary>
        /// 步骤属性变更回调（由 ViewModel 设置）
        /// </summary>
        internal Action<ChartPanelModel> OnStepChanged { get; set; }

        /// <summary>
        /// 订阅步骤属性变更事件
        /// </summary>
        internal void SubscribeStepEvents()
        {
            Steps.CollectionChanged -= Steps_CollectionChanged;
            Steps.CollectionChanged += Steps_CollectionChanged;
            foreach (var step in Steps)
            {
                step.PropertyChanged -= Step_PropertyChanged;
                step.PropertyChanged += Step_PropertyChanged;
            }
        }

        /// <summary>
        /// 取消订阅步骤属性变更事件
        /// </summary>
        internal void UnsubscribeStepEvents()
        {
            Steps.CollectionChanged -= Steps_CollectionChanged;
            foreach (var step in Steps)
            {
                step.PropertyChanged -= Step_PropertyChanged;
            }
        }

        private void Steps_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (StepAnnotationConfigModel step in e.NewItems)
                {
                    step.PropertyChanged -= Step_PropertyChanged;
                    step.PropertyChanged += Step_PropertyChanged;
                }
            }
            if (e.OldItems != null)
            {
                foreach (StepAnnotationConfigModel step in e.OldItems)
                {
                    step.PropertyChanged -= Step_PropertyChanged;
                }
            }
            OnStepChanged?.Invoke(this);
        }

        private void Step_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnStepChanged?.Invoke(this);
        }
    }
}
