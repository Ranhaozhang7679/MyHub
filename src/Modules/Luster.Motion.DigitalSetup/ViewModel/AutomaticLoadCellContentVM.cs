using LiveCharts;
using Luster.Common.DataAccess.Repositories;
using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.CommonUI;
using Luster.Motion.DataStruct;
using Luster.Motion.DigitalSetup.AssTables;
using Luster.Motion.DigitalSetup.Datas;
using Luster.Motion.DigitalSetup.Helpers;
using Luster.Motion.EditorUI;
using Luster.Motion.EditorUI.Models;
using Luster.Motion.TaskFlow.Engine;
using Luster.Motion.TaskFlow.Engine.HyperTrain;
using Luster.SimDevice.SubSystem.Events;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Logic;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static System.Windows.Forms.AxHost;
using Luster.Motion.DigitalSetup.Services;

namespace Luster.Motion.DigitalSetup.ViewModel
{
    /// <summary>
    /// loadcell
    /// </summary>
    public class AutomaticLoadCellContentVM : BaseAss
    {
        // 配方执行保存数据CSV文件路径
        //string csvPath = @"D:\Motion\AssData.csv";
        // 新增3个按钮和1个进度条的定义
        private double _progressValue;
        private bool _isChartVisible;
        private const string PageName = "LoadCell";

        public ICommand EndCommand { get; private set; }
        public ICommand OneKeyCheckCommand { get; private set; }
        public ICommand UpdateItemsCommand { get; private set; }

        /// <summary>
        /// 流程Bus
        /// </summary>
        private FlowBus flowBus;

        private IDeviceEngine _deviceEngine = null;
        /// <summary>
        /// 运控控制
        /// </summary>
        private IMotionController _mController;

        public double ProgressValue
        {
            get { return _progressValue; }
            set { SetProperty(ref _progressValue, value); }
        }

        public bool IsChartVisible
        {
            get { return _isChartVisible; }
            set { SetProperty(ref _isChartVisible, value); }
        }

        /// <summary>
        /// 曲线数据
        /// </summary>
        private SeriesCollection _loadCellSeriesCollection;
        public SeriesCollection LoadCellSeriesCollection
        {
            get { return _loadCellSeriesCollection; }
            set { SetProperty(ref _loadCellSeriesCollection, value); }
        }


        private CommonPageModel _seletedReportPage;
        public new CommonPageModel SelectedReportPage
        {
            get => _seletedReportPage;
            set
            {
                if (_seletedReportPage != null)
                {
                    if (value != null && _seletedReportPage?.Name != value.Name)
                    {
                        SaveGridItems(ItemModels);
                    }

                }
                SetProperty(ref _seletedReportPage, value);

                //同步赋值给基类属性
                base.SelectedReportPage = value;
                if (_seletedReportPage == null) return;
                if (_seletedReportPage.ViewType == typeof(AssTbCalibrationTable) 
                    || _seletedReportPage.ViewType == typeof(AssTbPressureRepetition)
                    || _seletedReportPage.ViewType == typeof(AssTbSuctionNozzle))
                {
                    IsChartVisible = true;
                }
                else
                {
                    IsChartVisible = false;
                }

                if (_seletedReportPage.ViewType == typeof(AssTbCalibrationTable))
                {
                    ConfigKey = "CalibrationTableConfig";
                }
                if (_seletedReportPage.ViewType == typeof(AssTbPressureRepetition))
                {
                    ConfigKey = "PressureRepetitionConfig";
                }
                if (_seletedReportPage.ViewType == typeof(AssTbSuctionNozzle))
                {
                    ConfigKey = "SuctionNozzle";
                }
                // 加载界面属性
                LoadStationConfigFromJson();
                //更新界面属性
                UpdateStationConfigs();

            }
        }

        private string GetOverallStatus()
        {
            if (ItemModels == null || ItemModels.Count == 0)
                return "未点检";

            foreach (var item in ItemModels)
            {
                string status = "";
                if (item is AssTbCalibrationTable calibration)
                    status = calibration.状态;
                else if (item is AssTbPressureRepetition pressure)
                    status = pressure.状态;
                else if (item is AssTbSuctionNozzle suction)
                    status = suction.状态;

                if (status == "NG")
                    return "NG";
            }
            return "OK";
        }


        //stationName
        //private string _stationname;
        //public string stationName
        //{
        //    get => _stationname;
        //    set
        //    {
        //        if (SetProperty(ref _stationname, value))
        //        {
        //            UpdateStationNames();
        //        }
        //    }
        //}

        private void UpdateStationNames()
        {
            stationNames.Clear();
            for (int i = 0; i <= StationConfigs.Count; i++)
            {
                stationNames.Add($"{StationConfigs[i].Name}");
            }
        }

        /// <summary>
        /// 加载全局变量
        /// </summary>
        private void LoadGlobalKeys()
        {
            GlobalKeys.Clear();
            var gID = GlobalModule.GlobalID;
            var gModule = flowBus.GetModule(gID);
            foreach (var item in gModule.Parameters)
            {
                if (item.Value.Type == typeof(LStatus)) continue;
                GlobalKeys.Add(new KeyValue() { Value = item.Key, Desc = $"Global.{item.Value.Alias}" });
            }

        }

        public ObservableCollection<KeyValue> GlobalKeys { get; } = new ObservableCollection<KeyValue>();


        // 保存时赋值
        [Obsolete]
        private void SaveStationConfig1()
        {
            try
            {
                // 构建工站与全局变量的映射字典
                var stationGlobalMap = new Dictionary<string, string>();
                foreach (var station in StationConfigs)
                {
                    // station.Name: 工站名称
                    // station.SelectedGlobalKey: 选中的全局变量Key
                    if (!string.IsNullOrEmpty(station.Name) && !string.IsNullOrEmpty(station.SelectedGlobalKey))
                    {
                        stationGlobalMap[station.Name] = station.SelectedGlobalKey;
                    }
                }

                // 保存到全局参数（如GlobalModule或系统配置）
                var gID = GlobalModule.GlobalID;

                var gModule = flowBus.GetModule(gID);

                // 直接将stationGlobalMap的key值保存至gModule.Parameters["stationGlobalMap[key]"]中
                foreach (var kvp in stationGlobalMap)
                {
                    string paramKey = kvp.Value;
                    if (gModule.Parameters.ContainsKey(paramKey))
                    {
                        var param = gModule.Parameters[paramKey];
                        param.Value = kvp.Key;
                    }
                    else
                    {
                        throw new FriendlyException($"未找到全局变量{kvp.Value}");
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }


            // 通知保存系统配置
            //_commonbus?.OnSaveSystem();
        }


        public AutomaticLoadCellContentVM(IRepository repository,
                                          IRegionManager regionManager, ICommonBus commonBus, IMotionController motionController, IDeviceEngine deviceEngine, FlowBus _flowBus, CSVHelper cSVHelper, IDialogService dialogService,
                                          CheckStatusService checkStatusService)
                                          : base(repository, regionManager, commonBus, cSVHelper, _flowBus, dialogService, checkStatusService)
        {
            flowBus = _flowBus;
            _deviceEngine = deviceEngine;
            _mController = motionController;
            _parentRegionName = "AutomaticLoadCellContent";
            Pages = new ObservableCollection<CommonPageModel>();
            Pages.Add(new CommonPageModel() { Name = "SuctionNozzle", IsSelected = false, Region = "", ViewType = typeof(AssTbSuctionNozzle) });
            Pages.Add(new CommonPageModel() { Name = "CalibrationTable", IsSelected = false, Region = "", ViewType = typeof(AssTbCalibrationTable) });
            Pages.Add(new CommonPageModel() { Name = "PressureRepetition", IsSelected = false, Region = "", ViewType = typeof(AssTbPressureRepetition) });

            // 注册子页面到DigitalAssPageModel
            DigitalAssPageModel.RegisterSubPages("AutomaticLoadCellContent", Pages);

            SelectedReportPage = Pages.Where(x => x.IsSelected).FirstOrDefault();
            InitModels();

            EndCommand = new DelegateCommand(OnEnd);
            OneKeyCheckCommand = new DelegateCommand<object>(OnOneKeyCheck);
            UpdateItemsCommand = new DelegateCommand(OnUpdateItems);

            ConfigKey = "CalibrationTableConfig";
            // 加载界面属性
            LoadStationConfigFromJson();
            //更新界面属性
            UpdateStationConfigs();
            // 加载工站点检状态
            LoadStationCheckStatus();
            //DrawPressureRepetitionChartOpt();
            DrawPressureLinearChartOpt();
            LoadCheckConfirmMessages();

            // 延迟加载点检状态，确保 UI 绑定已建立
            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                LoadCheckStatusForAllPages();
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        /// <summary>
        /// 加载所有子页面的历史点检状态
        /// </summary>
        private void LoadCheckStatusForAllPages()
        {
            if (_checkStatusService == null || Pages == null)
                return;

            try
            {
                foreach (var page in Pages)
                {
                    if (page != null)
                    {
                        page.ParentRegion = "AutomaticLoadCellContent";
                        var record = _checkStatusService.GetRecord(page.PageKey);
                        if (record != null)
                        {
                            page.CheckStatus = record.Status;
                        }
                        else
                        {
                            page.CheckStatus = CheckStatus.NotChecked;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载点检状态失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 刷新点检状态 - 每次页面激活时调用
        /// </summary>
        protected override void RefreshCheckStatus()
        {
            LoadCheckStatusForAllPages();
        }


        public override void OnEnd()
        {
            // 子界面的结束逻辑
            ProgressValue = 0; // 清空进度条
            base.OnEnd();
        }

        public override async void OnOneKeyCheck(object obj)
        {
            if (IsChecking) return;
            IsChecking = true;

            await base.OnOneKeyCheckAsync(obj);

            // 子界面的一键点检逻辑
            try
            {
                ProgressValue = 0; // 进度
                var stations = _mController.MotionEngine.GetStations();
                if (SelectedReportPage.ViewType == typeof(AssTbCalibrationTable))
                {
                    if (_deviceEngine.GetMachineStatus() == EngineStatus.Ready)
                    {
                        
                        var stat = stations.FirstOrDefault(s => s.Alias == "压力线性");
                        if (stat != null)
                        {
                            flowBus.OnRunOne(stat.ID);
                            // 异步等待stat.Status == RunStatus.Success
                            await Task.Run(async () =>
                            {
                                // 阶段1：等待流程启动（离开Default）
                                while (stat.Status == RunStatus.Default)
                                {
                                    _cts.Token.ThrowIfCancellationRequested();
                                    await Task.Delay(100);
                                }
                                // 阶段2：等待流程完成（Running/Pause继续等待，其他状态视为结束）
                                while (stat.Status == RunStatus.Running || stat.Status == RunStatus.Pause)
                                {
                                    _cts.Token.ThrowIfCancellationRequested();
                                    await Task.Delay(200);
                                }
                            }, _cts.Token);
                            //更新表格
                            UpdateItemsFromCsv();
                            for (int i = 0; i < ItemModels.Count; i++)
                            {
                                if (ItemModels[i] is AssTbCalibrationTable PressRepe)
                                {
                                    if (string.IsNullOrWhiteSpace(PressRepe.标准))
                                    {
                                        PressRepe.状态 = "格式错误";
                                        continue;
                                    }
                                    if (string.IsNullOrEmpty(PressRepe.实测))
                                    {
                                        PressRepe.状态 = "未完成";
                                    }
                                    //FillTableContent(PressRepe);
                                }
                                ProgressValue = (i + 1) * 100 / ItemModels.Count; // 进度
                            }

                            // 绘制曲线
                            //DrawPressureRepetitionChart();
                            DrawPressureLinearChartOpt();
                        }
                        else
                        {
                            throw new FriendlyException("未找到Alias为'压力线性'的工站");
                        }
                    }
                    else
                    {
                        throw new FriendlyException("回零完成后方可运行测试流程");
                    }
                }
                else if (SelectedReportPage.ViewType == typeof(AssTbSuctionNozzle))
                    {
                    if (_deviceEngine.GetMachineStatus() == EngineStatus.Ready)
                    {
                        var stat = stations.FirstOrDefault(s => s.Alias == "吸嘴压力标定");
                        if (stat != null)
                        {
                            flowBus.OnRunOne(stat.ID);
                            // 异步等待stat.Status == RunStatus.Success
                            await Task.Run(async () =>
                            {
                                // 阶段1：等待流程启动（离开Default）
                                while (stat.Status == RunStatus.Default)
                                {
                                    _cts.Token.ThrowIfCancellationRequested();
                                    await Task.Delay(100);
                                }
                                // 阶段2：等待流程完成（Running/Pause继续等待，其他状态视为结束）
                                while (stat.Status == RunStatus.Running || stat.Status == RunStatus.Pause)
                                {
                                    _cts.Token.ThrowIfCancellationRequested();
                                    await Task.Delay(200);
                                }
                            }, _cts.Token);
                            //更新表格
                            UpdateItemsFromCsv();
                            for (int i = 0; i < ItemModels.Count; i++)
                            {
                                if (ItemModels[i] is AssTbSuctionNozzle PressRepe)
                                {
                                    if (string.IsNullOrEmpty(PressRepe.状态))
                                    {
                                        if (string.IsNullOrWhiteSpace(PressRepe.标准))
                                        {
                                            PressRepe.状态 = "格式错误";
                                            continue;
                                        }
                                        FillTableContent(PressRepe);
                                    }     
                                }
                                ProgressValue = (i + 1) * 100 / ItemModels.Count; // 进度
                            }
                        }
                        else
                        {
                            throw new FriendlyException("未找到Alias为'吸嘴压力标定'的工站");
                        }
                    }
                    else
                    {
                        throw new FriendlyException("回零完成后方可运行测试流程");
                    }

                }
                else if (SelectedReportPage.ViewType == typeof(AssTbPressureRepetition))
                {
                    if (_deviceEngine.GetMachineStatus() == EngineStatus.Ready)
                    {
                        var stat = stations.FirstOrDefault(s => s.Alias == "压力重复性");
                        if (stat != null)
                        {
                            //模块运行
                            flowBus.OnRunOne(stat.ID);

                            // 异步等待stat.Status == RunStatus.Success
                            await Task.Run(async () =>
                            {
                                // 阶段1：等待流程启动（离开Default）
                                while (stat.Status == RunStatus.Default)
                                {
                                    _cts.Token.ThrowIfCancellationRequested();
                                    await Task.Delay(100);
                                }
                                // 阶段2：等待流程完成（Running/Pause继续等待，其他状态视为结束）
                                while (stat.Status == RunStatus.Running || stat.Status == RunStatus.Pause)
                                {
                                    _cts.Token.ThrowIfCancellationRequested();
                                    await Task.Delay(200);
                                }
                            }, _cts.Token);

                            //更新表格
                            UpdateItemsFromCsv();

                            for (int i = 0; i < ItemModels.Count; i++)
                            {
                                if (ItemModels[i] is AssTbPressureRepetition PressRepe)
                                {
                                    if (string.IsNullOrWhiteSpace(PressRepe.标准))
                                    {
                                        PressRepe.状态 = "格式错误";
                                        continue;
                                    }
                                    FillTableContent(PressRepe);                                  
                                }
                                ProgressValue = (i + 1) * 100 / ItemModels.Count; // 进度
                            }

                            // 绘制曲线
                            DrawPressureRepetitionChart();
                        }
                        else
                        {
                            throw new FriendlyException("未找到Alias为'压力重复性'的工站");
                        }
                    }
                    else
                    {
                        throw new FriendlyException("回零完成后方可运行测试流程");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _commonbus.OnLog(new LogInfo() { LogType = LogType.Info, LogMessage = "LoadCell点检被用户中止" });
                throw;
            }
            catch (Exception ex)
            {
                _commonbus.OnLog(new LogInfo() { LogType = LogType.Info, LogMessage = $"获取LoadCell相关数据失败: {ex.Message}" });
                throw;
            }
            finally
            {
                IsChecking = false;
                ProgressValue = 100;

                // 保存当前子页面的点检状态
                var currentOverallStatus = GetOverallStatus();
                var checkStatus = CheckStatus.NotChecked;
                string remark = "";

                // 检查是否被中止
                bool wasCancelled = _cts.IsCancellationRequested;

                if (wasCancelled)
                {
                    // 用户中止
                    bool canContinue = CanContinueFromLastCheck();

                    if (canContinue)
                    {
                        checkStatus = CheckStatus.NotChecked;
                        remark = "执行中止，可从上次继续";
                    }
                    else
                    {
                        checkStatus = CheckStatus.CheckedFail;
                        remark = "执行中止，需从头开始";
                    }
                }
                else if (currentOverallStatus == "OK")
                {
                    checkStatus = CheckStatus.CheckedOK;
                    remark = "全部点检项合格";
                }
                else if (currentOverallStatus == "NG")
                {
                    checkStatus = CheckStatus.CheckedFail;
                    remark = "发现点检不合格项";
                }
                else
                {
                    checkStatus = CheckStatus.NotChecked;
                    remark = "未完成点检";
                }

                SaveCheckStatus(checkStatus, remark);

                // 同步一级界面整体状态到 PageStatusService
                SyncOverallStatusToPageStatusService();
            }
        }

        //public void FillTableContent(AssTbCalibrationTable caliTable)
        public void FillTableContent(AssTb caliTable)
        {
            if (string.IsNullOrEmpty(caliTable.实测))
            {
                caliTable.状态 = "未完成";
            }
            else if (caliTable.标准 == caliTable.实测)
            {
                caliTable.状态 = "OK";
            }
            else if (caliTable.标准.Contains('~'))
            {
                var range = ParseColumnRange(caliTable.标准);
                double.TryParse(caliTable.实测, out double 实测浮点值);
                if (实测浮点值 >= range.lower && 实测浮点值 <= range.upper)
                {
                    caliTable.状态 = "OK";
                }
                else
                {
                    caliTable.状态 = "NG";
                }
            }
            else
            {
                caliTable.状态 = "NG";
            }
        }

        private void OnUpdateItems()
        {
            try
            {
                switch (SelectedReportPage.Name)
                {
                    case "CalibrationTable":

                        ItemModels.Clear();
                        //ItemModels.Add(new AssTbCalibrationTable());

                        #region
                        //var existingItem = ItemModels.OfType<AssTbCalibrationTable>()
                        //                                       .FirstOrDefault(item => item.项次 == "设置压力表模式");
                        //if (existingItem == null)
                        //{
                        //    // 填入平台默认的项次信息和对应的标准
                        //    AssTbCalibrationTable item0 = new AssTbCalibrationTable()
                        //    {
                        //        项序 = 0,
                        //        项次 = "设置压力表模式",
                        //        标准 = "/", // 默认标准值为空，无需填写
                        //        实测 = "/", // 默认实测值为空，无需填写
                        //        完成时间 = DateTime.Now
                        //    };
                        //    AssTbCalibrationTable item1 = new AssTbCalibrationTable()
                        //    {
                        //        项序 = 1,
                        //        项次 = "将0.25kg的砝码放置在指定位置",
                        //        标准 = "/",
                        //        实测 = "/",
                        //        完成时间 = DateTime.Now
                        //    };
                        //    AssTbCalibrationTable item2 = new AssTbCalibrationTable()
                        //    {
                        //        项序 = 2,
                        //        项次 = "读取0.25kg标定值",
                        //        标准 = "0.248~0.252", // 默认标准值为0.25+-0.002kg
                        //        实测 = "",            // 默认实测值为空
                        //        完成时间 = DateTime.Now
                        //    };
                        //    AssTbCalibrationTable item3 = new AssTbCalibrationTable()
                        //    {
                        //        项序 = 3,
                        //        项次 = "将0.35kg的砝码放置在指定位置",
                        //        标准 = "/",
                        //        实测 = "/",
                        //        完成时间 = DateTime.Now
                        //    };
                        //    AssTbCalibrationTable item4 = new AssTbCalibrationTable()
                        //    {
                        //        项序 = 4,
                        //        项次 = "读取0.35kg标定值",
                        //        标准 = "0.348~0.352",
                        //        实测 = "",
                        //        完成时间 = DateTime.Now
                        //    };
                        //    AssTbCalibrationTable item5 = new AssTbCalibrationTable()
                        //    {
                        //        项序 = 5,
                        //        项次 = "判断标定结果（平均K值）",
                        //        标准 = "0.95~1.05",
                        //        实测 = "",
                        //        完成时间 = DateTime.Now
                        //    };
                        //    ItemModels.Add(item0);
                        //    ItemModels.Add(item1);
                        //    ItemModels.Add(item2);
                        //    ItemModels.Add(item3);
                        //    ItemModels.Add(item4);
                        //    ItemModels.Add(item5);
                        //}
                        #endregion

                        break;

                    case "SuctionNozzle":
                        ItemModels.Clear();
                        //ItemModels.Add(new AssTbSuctionNozzle());
                        break;

                    case "PressureRepetition":

                        ItemModels.Clear();
                        //ItemModels.Add(new AssTbPressureRepetition());

                        #region                        
                        //var existingItem3 = ItemModels.OfType<AssTbPressureRepetition>()
                        //                                      .FirstOrDefault(item => item.项次 == "1");
                        //if (existingItem3 == null)
                        //{
                        //    for (int i = 0; i < 4; i++)
                        //    {
                        //        for (int j = 0; j < 32; j++)
                        //        {
                        //            ItemModels.Add(new AssTbPressureRepetition()
                        //            {
                        //                项序 = i * 32 + j,
                        //                项次 = $"工位{i + 1}测试压力{j + 1}",
                        //                标准 = "1", // 默认标准值为1
                        //                实测 = "/", // 默认实测值为空，无需填写
                        //                完成时间 = DateTime.Now
                        //            });
                        //        }
                        //    }
                        //}
                        
                        #endregion


                        break;
                }
            }
            catch (Exception)
            {
                // 异常处理逻辑
            }
            PageStatusService.Instance.UpdateStatus(PageName, "未点检");
        }

        private static (double lower, double upper) ParseColumnRange(string standardValue)
        {
            // 1) 不含 ~ ：按单个数字处理
            if (!standardValue.Contains('~'))
            {
                if (!double.TryParse(standardValue, NumberStyles.Float, CultureInfo.InvariantCulture, out double v))
                    throw new FormatException("第一列单值格式非法");
                return (v, v);
            }

            // 2) 含 ~ ：必须是“下限~上限”且仅出现一次 ~
            string[] tokens = standardValue.Split('~');
            if (tokens.Length != 2)
                throw new FormatException("第一列区间只能包含一个 '~'");

            string lowerStr = tokens[0].Trim();
            string upperStr = tokens[1].Trim();

            if (!double.TryParse(lowerStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double lower) ||
                !double.TryParse(upperStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double upper))
                throw new FormatException("第一列区间上下限格式非法");

            if (lower > upper)
                throw new FormatException("第一列区间下限不能大于上限");

            return (lower, upper);
        }
    }

}
