#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       MainRightContentVM
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.SubSystem.ViewModel
* 文 件 名:       MainRightContentVM.cs
* 创建时间:       2022/7/26 10:12:32
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      c3358a00-8295-4c8b-a2d9-b51b39739fd1
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/26 10:12:32
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using LiveCharts;
using LiveCharts.Wpf;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Extensions;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.CommonUI.Models;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.SubSystem.Models;
using Luster.Motion.TaskFlow.Engine;
using Luster.Motion.TaskFlow.Engine.Models;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Luster.Motion.DataStruct;
using System.Windows.Threading;

namespace Luster.Motion.SubSystem.ViewModel
{
    public class MainRightContentVM : MotionVM
    {
        /// <summary>
        /// 物料占位名称
        /// </summary>
        private const string MaterialName = "物料";

        #region 数据展示
        /// <summary>
        /// 产品数量
        /// </summary>
        private int _productCount;

        public int ProductCount
        {
            get { return _productCount; }
            set { SetProperty(ref _productCount, value); }
        }

        /// <summary>
        /// OK产品数量
        /// 
        /// </summary>
        private int _okCount;

        public int OkCount
        {
            get { return _okCount; }
            set { SetProperty(ref _okCount, value); }
        }

        /// <summary>
        /// NG 产品数量
        /// </summary>
        private int _ngCount;
        public int NgCount
        {
            get { return _ngCount; }
            set { SetProperty(ref _ngCount, value); }
        }

        /// <summary>
        /// 良率
        /// </summary>
        private double _yield;
        public double Yield
        {
            get { return _yield; }
            set { SetProperty(ref _yield, value); }
        }


        /// <summary>
        /// 良率
        /// </summary>
        private double _nYield;
        public double NYield
        {
            get { return _nYield; }
            set { SetProperty(ref _nYield, value); }
        }

        /// <summary>
        /// 抛料数
        /// </summary>
        private int _throwCount;
        public int ThrowCount
        {
            get { return _throwCount; }
            set { SetProperty(ref _throwCount, value); }
        }

        /// <summary>
        /// 袍料率
        /// </summary>
        private double _throwRate;
        public double ThrowRate
        {
            get { return _throwRate; }
            set { SetProperty(ref _throwRate, value); }
        }

        /// <summary>
        /// 稼动率
        /// </summary>
        private double _workRate;
        public double WorkRate
        {
            get { return _workRate; }
            set { SetProperty(ref _workRate, value); }
        }

        /// <summary>
        /// 运行时间
        /// </summary>
        private double _runTime;
        public double RunTime
        {
            get { return _runTime; }
            set { SetProperty(ref _runTime, value); }
        }

        /// <summary>
        /// 故障时间
        /// </summary>
        private double _downTime;
        public double DownTime
        {
            get { return _downTime; }
            set { SetProperty(ref _downTime, value); }
        }

        /// <summary>
        /// 待机时间
        /// </summary>
        private double _idelTime;
        public double IdelTime
        {
            get { return _idelTime; }
            set { SetProperty(ref _idelTime, value); }
        }

        /// <summary>
        /// 维护时间
        /// </summary>
        private double _pmTime;
        public double PmTime
        {
            get { return _pmTime; }
            set { SetProperty(ref _pmTime, value); }
        }

        /// <summary>
        /// 真实
        /// </summary>
        private double _realCT;
        public double RealCT
        {
            get { return _realCT; }
            set { SetProperty(ref _realCT, value); }
        }

        /// <summary>
        /// 理论
        /// </summary>
        private double _theoreticalCT;
        public double TheoreticalCT
        {
            get { return _theoreticalCT; }
            set { SetProperty(ref _theoreticalCT, value); }
        }

        #endregion

        /// <summary>
        /// 运行信息
        /// </summary>
        private string _runInfo;
        public string RunInfo
        {
            get { return _runInfo; }
            set { SetProperty(ref _runInfo, value); }
        }

        #region 趋势图相关

        /// <summary>
        /// 是否显示测量系列
        /// </summary>
        private bool _onlyShowMeasure;
        public bool OnlyShowMeasure
        {
            get { return _onlyShowMeasure; }
            set { SetProperty(ref _onlyShowMeasure, value); }
        }

        /// <summary>
        /// X轴标签集合
        /// </summary>
        private string[] _xLables;
        public string[] XLables
        {
            get { return _xLables; }
            set { SetProperty(ref _xLables, value); }
        }

        /// <summary>
        /// Y轴标签格式器
        /// </summary>
        private Func<double, string> _yFormatter;
        public Func<double, string> YFormatter
        {
            get { return _yFormatter; }
            set { SetProperty(ref _yFormatter, value); }
        }

        /// <summary>
        /// 标准值曲线
        /// </summary>
        private ChartValues<double> _standardValues;
        public ChartValues<double> StandardValues
        {
            get { return _standardValues; }
            set { SetProperty(ref _standardValues, value); }
        }

        /// <summary>
        /// 上限曲线
        /// </summary>
        private ChartValues<double> _upLimitValues;
        public ChartValues<double> UplimtValues
        {
            get { return _upLimitValues; }
            set { SetProperty(ref _upLimitValues, value); }
        }

        /// <summary>
        /// 下限曲线
        /// </summary>
        private ChartValues<double> _lowLimitValues;
        public ChartValues<double> LowlimtValues
        {
            get { return _lowLimitValues; }
            set { SetProperty(ref _lowLimitValues, value); }
        }

        /// <summary>
        /// 测量曲线
        /// </summary>
        private ChartValues<double> _measureValues;
        public ChartValues<double> MeasureValues
        {
            get { return _measureValues; }
            set { SetProperty(ref _measureValues, value); }
        }

        /// <summary>
        /// 数据源
        /// </summary>
        private string _qualitySourceAlias;
        public string QualitySourceAlia
        {
            get { return _qualitySourceAlias; }
            set { SetProperty(ref _qualitySourceAlias, value); }
        }

        #endregion

        public ObservableCollection<ThrowMaterialModel> ThrowMaterialModels { get; set; }


        public DelegateCommand<string> NavigateCommand { get; set; }

        private List<ProductInfo> _productInfos = new List<ProductInfo>();
        private const int DataLength = 20;
        private ICommonBus _commonBus;
        private IDbManager _dbManager;
        private IMotionController _motionController;
        // UI线程
        private Dispatcher _dispatcher;
        /// <summary>
        /// 当前产品结果
        /// </summary>
        private string _productResult;
        public string ProductResult
        {
            get => _productResult;
            set { SetProperty(ref _productResult, value); }
        }


        public DelegateCommand ResetProductCommand { get; private set; }
        public DelegateCommand HomeChangeCommand { get; private set; }

        private MapData _selectedMap;

        public MainRightContentVM(ICommonBus commonBus, IDbManager dbManager,
            IMotionController motionController, Dispatcher dispatcher) : base(commonBus)
        {
            _commonBus = commonBus;
            _dispatcher = dispatcher;
            _dbManager = dbManager;
            OnlyShowMeasure = false;
            NavigateCommand = new DelegateCommand<string>(Navigate);
            YFormatter = value => value.ToString("G");
            XLables = new string[DataLength];
            for (int i = 0; i < DataLength; i++)
            {
                XLables[i] = (i + 1).ToString();
            }
            _motionController = motionController;

            //_motionController.ProStatEvent -= _motionController_ProStatEvent;
            //_motionController.ProductEvent -= _motionController_ProductEvent;
            //_motionController.ProStatEvent += _motionController_ProStatEvent;
            //_motionController.ProductEvent += _motionController_ProductEvent;
            //_motionController.EngineStatusEvent -= _motionController_EngineStatusEvent;
            //_motionController.EngineStatusEvent += _motionController_EngineStatusEvent;
            //_commonBus.EventBus.GetEvent<RecipeOpenEvent>().Subscribe(RecipeOpen);
            //_commonBus.EventBus.GetEvent<MapDataEvent>().Subscribe(MapDataChanged);
            ResetProductCommand = new DelegateCommand(ResetProduct);
            HomeChangeCommand = new DelegateCommand(HomeChange);
            Yield = 100;
            NYield = 0;
            QualitySourceAlia = string.Empty;
            InitThrowModels();
        }

        private void InitThrowModels()
        {
            ThrowMaterialModels = new ObservableCollection<ThrowMaterialModel>();
            ThrowMaterialModels.Add(new ThrowMaterialModel()
            {
                MaterialName = MaterialName,
                Count = 0,
                Rate = 0,
            });
            ThrowMaterialModels.Add(new ThrowMaterialModel()
            {
                MaterialName = MaterialName,
                Count = 0,
                Rate = 0,
            });
            ThrowMaterialModels.Add(new ThrowMaterialModel()
            {
                MaterialName = MaterialName,
                Count = 0,
                Rate = 0,
            });
        }

        private void HomeChange()
        {
            var pageModel = PageModel.Pages.FirstOrDefault(x => x.Name == "Home");
            if (pageModel != null)
            {
                pageModel.Region = "MainContent";
            }
            _commonBus.OnNavigate(pageModel);
        }
        private void ResetProduct()
        {
            ProductResult = "OK";
            RealCT = TheoreticalCT;
            foreach (var model in ThrowMaterialModels)
            {
                model.MaterialName = MaterialName;
                model.Count = 0;
                model.Rate = 0;
            }
            _productInfos.Clear();
            _motionController.ClearProductInfo();
        }

        private void _motionController_EngineStatusEvent(object sender, EngineStatus src, EngineStatus status)
        {
            _dispatcher.Invoke(() =>
            {
                RunInfo = status.GetDescription();
            });
        }

        private void MapDataChanged(MapData obj)
        {
            ClearCurrentData();
            SetDataSource();
        }

        private void SetDataSource()
        {
            var map = _commonBus.GetMapDatas().FirstOrDefault(x => x.IsMonitor);
            _selectedMap = map;
            if (map != null)
            {
                QualitySourceAlia = map.Alias;
                MonitorValueChanged(_selectedMap);
            }
            else
            {
                QualitySourceAlia = string.Empty;
            }
        }

        private void RecipeOpen(Recipe obj)
        {
            if (_commonBus.ProjInfo != null && _commonBus.ProjInfo.FullName != null)
            {
                string sysConfig = Path.Combine(Path.GetDirectoryName(commonBus.ProjInfo.FullName), "Config", "SystemConfig.xml");
                _motionController.SysConfig.LoadSysConfig(sysConfig);
                RealCT = TheoreticalCT = _motionController.SysConfig.CT;
            }
        }

        /// <summary>
        /// 产品监控信息
        /// </summary>
        /// <param name="pInfo">产品信息</param>
        /// <param name="enter">来料标识 true出料 false 入料</param>
        private void _motionController_ProductEvent(ProductInfo pInfo, bool enter)
        {
            if (!enter)
            {
                return;
            }
            if (_productInfos.Count >= DataLength)
            {
                _productInfos.RemoveAt(0);
            }
            if (pInfo.Result.Result)
            {
                ProductResult = "OK";
            }
            else
            {
                ProductResult = "NG";
            }
            _productInfos.Add(pInfo);
            UpdateMeasureValue(pInfo);
        }

        /// <summary>
        /// 更新测量数据
        /// </summary>
        /// <param name="pInfo"></param>
        private void UpdateMeasureValue(ProductInfo pInfo)
        {
            if (MeasureValues == null)
            {
                MeasureValues = new ChartValues<double>();
            }

            object measureValue = 0;
            if (string.IsNullOrEmpty(QualitySourceAlia))
            {
                return;
            }
            if (pInfo.Data.ContainsKey(QualitySourceAlia))
            {
                measureValue = pInfo.Data[QualitySourceAlia];
            }

            if (MeasureValues.Count > DataLength)
            {
                _dispatcher.Invoke(() => { MeasureValues.RemoveAt(0); });
            }
            _dispatcher.Invoke(() =>
            {
                if (_selectedMap == null || _selectedMap.DataType == typeof(string)) return;
                if (_selectedMap.DataType == typeof(bool))
                {
                    if (bool.TryParse(measureValue?.ToString(), out var bVal) && bVal)
                    {
                        MeasureValues.Add(1);
                    }
                    else
                    {
                        MeasureValues.Add(0);
                    }
                }
                else
                {
                    if (double.TryParse(measureValue?.ToString(), out var val))
                    {
                        MeasureValues.Add(val);
                    }
                }
            });
        }

        /// <summary>
        /// 跟新数据统计信息
        /// </summary>
        /// <param name="pStat">产品生产信息</param>
        /// <param name="iData">稼动率信息</param>
        /// <param name="ct">CT值</param>      
        private void _motionController_ProStatEvent(ProductStat pStat, ImpactData iData)
        {
            if (pStat != null)
            {
                _dispatcher.Invoke(() =>
                {
                    ProductCount = pStat.AllCount;
                    OkCount = pStat.OKCount;
                    NgCount = pStat.NGCount;
                    Yield = pStat.GetYield();
                    NYield = 100 - Yield;
                    var materialDics = pStat.ThrowMaterialDic;
                    foreach (var material in materialDics)
                    {
                        var model = ThrowMaterialModels.FirstOrDefault(x => x.MaterialName == material.Key);
                        if (model == null)
                        {
                            model = ThrowMaterialModels.FirstOrDefault(x => x.MaterialName == MaterialName);
                            if (model == null)
                            {
                                return;
                            }
                            model.MaterialName = material.Key;
                        }
                        model.Count = material.Value;
                        model.Rate = pStat.GetThrowRate(material.Value);
                    }
                });
            }

            if (iData != null)
            {
                _dispatcher.Invoke(() =>
                {
                    RunTime = Math.Round(iData.RunTime / 60, 2);
                    DownTime = Math.Round(iData.StopTime / 60, 2);
                    IdelTime = Math.Round(iData.IdleTime / 60, 2);
                    PmTime = Math.Round(iData.PMTime / 60, 2);
                    WorkRate = iData.GetImpactPer();
                    RealCT = pStat.RealCT;
                });
            }
        }

        /// <summary>
        /// 导航
        /// </summary>
        /// <param name="regionName"></param>
        private void Navigate(string regionName)
        {
            commonBus.OnNavigate(new PageModel() { Region = regionName });
        }

        /// <summary>
        /// 监控项目变化
        /// </summary>
        private void MonitorValueChanged(MapData map)
        {
            var tolerance = _dbManager.GetLTolerance(map.ID, map.Alias);
            if (tolerance == null)
            {
                OnlyShowMeasure = true;
                return;
            }

            var rule = new LTolerance()
            {
                Standard = tolerance.Standard,
                ToleranceMax = tolerance.ToleranceMax,
                ToleranceMin = tolerance.ToleranceMin,
            };
            RefreshChart(rule);
        }

        /// <summary>
        /// 更新图表
        /// </summary>
        /// <param name="rule"></param>
        private void RefreshChart(LTolerance rule)
        {
            var standard = rule.Standard;
            var upLimit = rule.ToleranceMax;
            var lowLimit = rule.ToleranceMin;
            StandardValues = new ChartValues<double>();
            UplimtValues = new ChartValues<double>();
            LowlimtValues = new ChartValues<double>();
            for (int i = 0; i < DataLength; i++)
            {
                StandardValues.Add(standard);
            }

            if (!upLimit.Equals(0))
            {
                var upValue = standard + upLimit;
                for (int i = 0; i < DataLength; i++)
                {
                    UplimtValues.Add(upValue);
                }
            }

            if (!upLimit.Equals(0))
            {
                var lowValue = standard + lowLimit;
                for (int i = 0; i < DataLength; i++)
                {
                    LowlimtValues.Add(lowValue);
                }
            }

            MeasureValues = new ChartValues<double>();
        }

        private void ClearCurrentData()
        {
            if (_productInfos != null)
                _productInfos.Clear();

            if (MeasureValues != null)
                MeasureValues.Clear();

            if (UplimtValues != null)
                UplimtValues.Clear();

            if (LowlimtValues != null)
                LowlimtValues.Clear();

            if (StandardValues != null)
                StandardValues.Clear();
        }
    }
}
