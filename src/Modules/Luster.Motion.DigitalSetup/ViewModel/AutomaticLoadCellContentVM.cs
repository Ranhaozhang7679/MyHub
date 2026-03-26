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
                                          IRegionManager regionManager, ICommonBus commonBus, IMotionController motionController, IDeviceEngine deviceEngine, FlowBus _flowBus, CSVHelper cSVHelper) 
                                          : base(repository, regionManager, commonBus, cSVHelper, _flowBus)
        {
            flowBus = _flowBus;
            _deviceEngine = deviceEngine;
            _mController = motionController;
            Pages = new ObservableCollection<CommonPageModel>();
            Pages.Add(new CommonPageModel() { Name = "CalibrationTable", IsSelected = true, Region = "", ViewType = typeof(AssTbCalibrationTable) });
            Pages.Add(new CommonPageModel() { Name = "PressureRepetition", IsSelected = true, Region = "", ViewType = typeof(AssTbPressureRepetition) });
            Pages.Add(new CommonPageModel() { Name = "SuctionNozzle", IsSelected = true, Region = "", ViewType = typeof(AssTbSuctionNozzle) });
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
            //DrawPressureRepetitionChartOpt();
            DrawPressureLinearChartOpt();
        }


        public override void OnEnd()
        {
            // 子界面的结束逻辑
            ProgressValue = 0; // 清空进度条
            base.OnEnd();
        }

        public override async void OnOneKeyCheck(object obj)
        {
            base.OnOneKeyCheck(obj);

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
                                while (stat.Status != RunStatus.Success)
                                {
                                    await Task.Delay(200); // 200ms轮询
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
                                    FillTableContent(PressRepe);
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
                                while (stat.Status != RunStatus.Success)
                                {
                                    await Task.Delay(200); // 200ms轮询
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
                                while (stat.Status != RunStatus.Success)
                                {
                                    await Task.Delay(200); // 200ms轮询
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
            catch (Exception ex)
            {
                _commonbus.OnLog(new LogInfo() { LogType = LogType.Info, LogMessage = $"获取LoadCell相关数据失败" });
                throw;
            }
            finally
            {

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

    public class StationConfig : BindableBase
    {
        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string _selectedGlobalKey;

        public string SelectedGlobalKey
        {
            get => _selectedGlobalKey;
            set => SetProperty(ref _selectedGlobalKey, value);
        }


    }

}
