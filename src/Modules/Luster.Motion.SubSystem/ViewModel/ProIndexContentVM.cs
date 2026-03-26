#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       UserDefineMainContentVM
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.SubSystem.ViewModel
* 文 件 名:       UserDefineMainContentVM.cs
* 创建时间:       2022/9/6 15:46:33
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      d8a7bc81-f833-423e-8960-603956805ecb
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/6 15:46:33
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using Luster.Common.DataAccess.Tables;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Extensions;
using Luster.Control.Wpf.Motion.Controls;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.CommonUI.Models;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.EditorUI.Events;
using Luster.Motion.EditorUI.Extensions;
using Luster.Motion.SubSystem.Models;
using Luster.Motion.TaskFlow.Engine;
using Luster.Motion.TaskFlow.Engine.Models;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Luster.Motion.DataStruct;
using HandyControl.Controls;
using LiveChartsCore.Defaults;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView.Painting;
using System.Windows.Media;
using SkiaSharp;
using Luster.Common.Assets;
using Luster.Common.Assets.Extension;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using LiveChartsCore.SkiaSharpView;

namespace Luster.Motion.SubSystem.ViewModel
{
    public class ProIndexContentVM : MotionPageVM
    {
        /// <summary>
        /// 自动换班间隔检查
        /// </summary>
        private int ClassInterval = 0;

        private IRegionManager _regionManager;

        public IDialogService _dialogService;

        /// <summary>
        /// 物料占位名称
        /// </summary>
        public string MaterialName = Luster.Motion.Assests.Langs.LangProvider.GetLang("Material");


        public IDbManager _dbManager;

        /// <summary>
        /// 控制器
        /// </summary>
        public IMotionController _mController;

        /// <summary>
        /// UI线程
        /// </summary>
        private Dispatcher _dispatcher;

        /// <summary>
        /// 时间
        /// </summary>
        private DispatcherTimer _dispatherTimer;

        public DelegateCommand ResetProductCommand { get; private set; }
        public DelegateCommand<string> NavigateCommand { get; set; }

        public DelegateCommand<string> ChangeAccessoryCommand { get; set; }

        /// <summary>
        /// 抛料对象
        /// </summary>
        private ObservableCollection<ThrowMaterialModel> _materials;
        public ObservableCollection<ThrowMaterialModel> Materials
        {
            get => _materials;
            set => SetProperty(ref _materials, value);
        }

        public DelegateCommand HomeChangeCommand { get; private set; }

        public ProIndexContentVM(ICommonBus _commonBus, IDbManager dbManager, IRegionManager regionManager,
            IMotionController motionController, Dispatcher dispatcher, IDialogService dialogService) : base(_commonBus)
        {
            _dbManager = dbManager;
            _mController = motionController;
            _dispatcher = dispatcher;
            _regionManager = regionManager;
            _dialogService = dialogService;
            _mController.ProStatEvent -= _motionController_ProStatEvent;
            _mController.ProStatEvent += _motionController_ProStatEvent;
            commonBus.EventBus.GetEvent<AlarmEvent>().Subscribe(LogAlarm);

            ResetProductCommand = new DelegateCommand(ResetProduct);
            HomeChangeCommand = new DelegateCommand(HomeChange);
            NavigateCommand = new DelegateCommand<string>(Navigate);
            ChangeAccessoryCommand = new DelegateCommand<string>(ChangeAccessory);

            // 计时器
            _dispatherTimer = new DispatcherTimer();
            _dispatherTimer.Tick += _dispatherTimer_Tick;
            _dispatherTimer.Interval = new TimeSpan(0, 0, 0, 1);
            _dispatherTimer.Start();
            Init();
        }

        /// <summary>
        /// 计时器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _dispatherTimer_Tick(object sender, EventArgs e)
        {
            RealTime = DateTime.Now.ToString("yyyy/MM/dd|HH:mm:ss");

            // 计算运行时间
            if (_impackData != null)
            {
                RunTime = Math.Round(_impackData.GetRunTime() / 60, 2);

                if (_impackData != null)
                {
                    WorkRate = _impackData.GetImpactPer();
                }
            }

            // 每 60s 300s 做一下换班处理、做下数据删除处理
            ClassInterval++;
            if (ClassInterval > 300)
            {
                ClassInterval = 0;

                // 不进行自动切班
                //_mController.ChangeClass(DateTime.Now);
                commonBus.StartHistoryFileDelete();
            }
        }

        /// <summary>
        /// 实时时间
        /// </summary>
        private string _realTime;
        public string RealTime
        {
            get { return _realTime; }
            set { SetProperty(ref _realTime, value); }
        }

        #region 稼动相关 暂未用

        /// <summary>
        /// 稼动率
        /// </summary>
        private double _workRate = 100;
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
        #endregion

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
        private int _uph;
        public int UPH
        {
            get { return _uph; }
            set { SetProperty(ref _uph, value); }
        }

        /// <summary>
        /// 抛料
        /// </summary>
        private int _toss;
        public int Toss
        {
            get { return _toss; }
            set { SetProperty(ref _toss, value); }
        }

        /// <summary>
        /// 报警时间
        /// </summary>
        private DateTime _alarmTime;
        public DateTime AlarmTime
        {
            get { return _alarmTime; }
            set { SetProperty(ref _alarmTime, value); }
        }

        private string _alarmMsg;
        public string AlarmMsg
        {
            get => _alarmMsg;
            set { SetProperty(ref _alarmMsg, value); }
        }


        /// <summary>
        /// 当前产品结果
        /// </summary>
        private string _productResult;
        public string ProductResult
        {
            get => _productResult;
            set { SetProperty(ref _productResult, value); }
        }

        /// <summary>
        /// 中文显示
        /// </summary>
        private bool _chVisible = true;
        public bool ChVisible
        {
            get { return _chVisible; }
            set { SetProperty(ref _chVisible, value); }
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
        /// 英文显示
        /// </summary>
        private bool _enVisible;
        public bool EnVisible
        {
            get { return _enVisible; }
            set { SetProperty(ref _enVisible, value); }
        }

        /// <summary>
        /// 当前产品结果
        /// </summary>
        private double _ngRate;
        public double NGRate
        {
            get => _ngRate;
            set { SetProperty(ref _ngRate, value); }
        }

        /// <summary>
        /// Input/OutPut
        /// </summary>
        private string _inOutCount;
        public string InOutCount
        {
            get => _inOutCount;
            set { SetProperty(ref _inOutCount, value); }
        }


        /// <summary>
        /// pass/fail
        /// </summary>
        private string _passfailcount;
        public string PassFailCount
        {
            get => _passfailcount;
            set { SetProperty(ref _passfailcount, value); }
        }

        /// <summary>
        /// 是否查询
        /// </summary>
        private bool _isQuery = false;
        public bool IsQuery
        {
            get { return _isQuery; }
            set { SetProperty(ref _isQuery, value); }
        }


        /// <summary>
        /// 配方名称
        /// </summary>
        private string _recipeName;
        public string RecipeName
        {
            get { return _recipeName; }
            set { SetProperty(ref _recipeName, value); }
        }

        /// <summary>
        /// 图表数据列
        /// </summary>
        private List<PieSeries<ObservableValue>> _series;
        public List<PieSeries<ObservableValue>> Series
        {
            get => _series;
            set
            {
                SetProperty(ref _series, value);
            }
        }

        #endregion

        protected override void RegisterEvent(IEventAggregator bus)
        {
            bus.GetEvent<LangChangedDoneEvent>().Subscribe(() =>
            {
                if (Materials != null)
                {
                    foreach (var material in Materials)
                    {
                        if (material.MaterialName == "物料" || material.MaterialName == "Material")
                        {
                            material.MaterialName = Luster.Motion.Assests.Langs.LangProvider.GetLang("Material");
                        }
                    }
                }
                if (AppConfig.Lang == "zh-cn")
                {
                    ChVisible = true;
                    EnVisible = false;
                }
                else
                {
                    ChVisible = false;
                    EnVisible = true;
                }

            });

            bus.GetEvent<OperationEvent>().Subscribe((op) =>
            {
                BtnEnable = op.Dst != EngineStatus.Running;
            });

            bus.GetEvent<RecipeOpenEvent>().Subscribe(r =>
            {
                Init();
                RecipeName = r.Name;
                InitGlobal();
            });
        }

        /// <summary>
        /// 初始化产品数据
        /// </summary>
        private void Init()
        {
            Yield = 0;
            NYield = 0;
            RealCT = _mController.SysConfig.CT;
            ProductResult = "OK";
            InOutCount = "0/0";
            PassFailCount = "0/0";
            Toss = 0;

            // 初始化物料信息
            Materials = new ObservableCollection<ThrowMaterialModel>
            {
                new ThrowMaterialModel()
                {
                    MaterialName = MaterialName,
                    Count = 0,
                    Rate = 0,
                },

                new ThrowMaterialModel()
                {
                    MaterialName = MaterialName,
                    Count = 0,
                    Rate = 0,
                },
            };

            chartBuilder = null;
            Series = new List<PieSeries<ObservableValue>>();

            // 默认良率100%
            BuildSeries(0, 100, 0);

            // 获取产品良率信息
            TbProductYeild proYield = _dbManager.GetProYield();

            if (proYield != null)
            {
                UpdateMaterial(proYield);

                var pState = new ProductStat()
                {
                    AllCount = proYield.AllCount,
                    OKCount = proYield.OKCount,
                    NGCount = proYield.NGCount,
                    Toss = proYield.Toss,
                    ThrowMaterialDic = proYield.GetThrowMaterial()
                };

                //_dbManager.getu
                int uph = (int)_dbManager.GetCurrentUPH();
                pState.SetUPH(uph);
                UPH = uph;

                _mController.SetProductInfo(pState);
            }

        }

        public GaugeBuilder chartBuilder = null;
        protected virtual void BuildSeries(double ngYeild, double okYeild, double toss)
        {
            if (chartBuilder == null)
            {
                chartBuilder = new GaugeBuilder().WithLabelsSize(18)
                 .WithBackground(new SolidColorPaint(new SKColor(100, 181, 246, 128)))
                 .WithLabelsPosition(PolarLabelsPosition.Start)
                 .WithLabelFormatter(point => $"{point.Context.Series.Name} {point.PrimaryValue}%")
                 .WithInnerRadius(30)
                 .WithOffsetRadius(8)
                 .WithBackgroundInnerRadius(30)
                 .WithBackgroundOffsetRadius(8)
                 .AddValue(Math.Round(toss, 2), "Toss", SKColors.Orange)
                 .AddValue(Math.Round(ngYeild, 2), "NG", SKColors.Red)
                 .AddValue(Math.Round(okYeild, 2), "OK", SKColors.Green);

                Series = chartBuilder.BuildSeries();
            }
            else
            {
                int i = 0;
                foreach (var item in Series)
                {
                    if (i == 0)
                    {
                        item.Values.ElementAt(0).Value = toss;
                    }

                    if (i == 1)
                    {
                        item.Values.ElementAt(1).Value = ngYeild;
                    }

                    if (i == 2)
                    {
                        item.Values.ElementAt(2).Value = okYeild;
                    }

                    if (i == 2)
                    {
                        break;
                    }

                    i++;
                }
            }
        }

        protected virtual void InitGlobal()
        { }

        /// <summary>
        /// 更新统计
        /// </summary>
        /// <param name="tbYield"></param>
        private void UpdateMaterial(TbProductYeild tbYield)
        {
            ProductCount = tbYield.AllCount;
            OkCount = tbYield.OKCount;
            NgCount = tbYield.NGCount;
            Yield = tbYield.GetYield();
            NYield = Math.Round((100 - Yield-tbYield.GetThrowRate(tbYield.Toss)), 2);
            InOutCount = $"{ProductCount}/{ProductCount}";
            PassFailCount = $"{OkCount}/{NgCount}";
            Toss = tbYield.Toss;

            Dictionary<string, int> mat = tbYield.ThrowMaterial;
            if (mat.Count == 0 && !string.IsNullOrEmpty(tbYield.Data))
            {
                mat = tbYield.GetThrowMaterial();
            }

            foreach (var material in mat)
            {
                var model = Materials.FirstOrDefault(x => x.MaterialName == material.Key);
                if (model != null)
                {
                    model.Count = material.Value;
                    model.Rate = tbYield.GetThrowRate(material.Value);
                }
                else
                {
                    // 1.选择物料名称
                    var exist = Materials.FirstOrDefault(x => x.MaterialName == "物料");
                    if (exist != null)
                    {
                        exist.MaterialName = material.Key;
                        exist.Count = material.Value;
                        exist.Rate = tbYield.GetThrowRate(material.Value);
                    }
                    else
                    {
                        Materials.Add(new ThrowMaterialModel()
                        {
                            MaterialName = material.Key,
                            Count = material.Value,
                            Rate = tbYield.GetThrowRate(material.Value)
                        });
                    }
                }
            }

            BuildSeries(NYield, Yield, tbYield.GetThrowRate(Toss));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pStat"></param>
        /// <returns></returns>
        private TbProductYeild ProStatToYeild(ProductStat pStat)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in pStat.ThrowMaterialDic)
            {
                sb.Append($"{item.Key}_{item.Value}|");
            }

            if (sb.Length > 0)
            {
                sb.Remove(sb.Length - 1, 1);
            }

            return new TbProductYeild()
            {
                ID = 1,
                OKCount = pStat.OKCount,
                AllCount = pStat.AllCount,
                NGCount = pStat.NGCount,
                ThrowMaterial = pStat.ThrowMaterialDic,
                Toss = pStat.Toss,
                Data = sb.ToString()
            };
        }

        private void ChangeAccessory(string name)
        {
            _dialogService.ShowChangeAccessory(name, (r) =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    r.Parameters.TryGetValue<string>("AccessoryName", out var name);
                    r.Parameters.TryGetValue<string>("BatchNo", out var batchNo);
                    r.Parameters.TryGetValue<int>("Count", out var count);
                    r.Parameters.TryGetValue<string>("StationName", out var stationName);
                    _dbManager.AddChangeAccessoryLog(name, batchNo, count, stationName);
                    // todo 后续需要上传驾驶舱辅料更换记录
                }
            });
        }

        private void Navigate(string regionName)
        {
            commonBus.OnNavigate(new PageModel() { Region = regionName });
        }

        private void HomeChange()
        {
            var pageModel = PageModel.Pages.FirstOrDefault(x => x.Name == "Home");
            if (pageModel != null)
            {
                pageModel.Region = "MainContent";
            }
            commonBus.OnNavigate(pageModel);
        }

        private void LogAlarm(AlarmInfo obj)
        {
            _dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                AlarmTime = DateTime.Now;
                AlarmMsg = obj.Message;
            }));
        }

        protected virtual void ResetProduct()
        {
            _dialogService.ShowConfirm("确认对数据清零?", r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    ProductResult = "OK";

                    foreach (var model in Materials)
                    {
                        model.MaterialName = MaterialName;
                        model.Count = 0;
                        model.Rate = 0;
                    }

                    _mController.ClearProductInfo();
                    _dbManager.ClearCurrentCapacityInfo();
                }
            });
        }

        private ImpactData _impackData = null;

        /// <summary>
        /// 跟新数据统计信息
        /// </summary>
        /// <param name="pStat">产品生产信息</param>
        /// <param name="iData">稼动率信息</param>
        /// <param name="ct">CT值</param>      
        private void _motionController_ProStatEvent(ProductStat pStat, ImpactData iData)
        {
            // 稼动影响的数据
            _impackData = iData;

            _dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                ProductResult = pStat.Result ? "OK" : "NG";
                if (pStat != null)
                {
                    var pYeild = ProStatToYeild(pStat);

                    UpdateMaterial(pYeild);

                    // 同步更新到数据库中
                    _dbManager.UpdateProYield(pYeild);
                }

                if (iData != null)
                {
                    RunTime = Math.Round(iData.RunTime / 60, 2);
                    DownTime = Math.Round(iData.StopTime / 60, 2);
                    IdelTime = Math.Round(iData.IdleTime / 60, 2);
                    PmTime = Math.Round(iData.PMTime / 60, 2);
                    WorkRate = iData.GetImpactPer();
                }

                // ct 小于1s认为不合理  CT过小，UPH可能溢出
                RealCT = pStat.RealCT;
                UPH = pStat.UPH;
            }));
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
        }
    }
}
