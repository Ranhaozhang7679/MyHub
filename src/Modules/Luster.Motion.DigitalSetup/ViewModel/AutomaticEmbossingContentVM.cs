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
using Luster.Motion.TaskFlow.Engine;
using Luster.Motion.TaskFlow.Engine.HyperTrain;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Motion.Logic;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Luster.Motion.DigitalSetup.ViewModel
{
    /// <summary>
    /// 自动压印
    /// </summary>
    public class AutomaticEmbossingContentVM : BaseAss
    {
        // 新增3个按钮和1个进度条的定义
        private double _progressValue;

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
        public AutomaticEmbossingContentVM(IRepository repository,
                                           IRegionManager regionManager, ICommonBus commonBus, IMotionController motionController,
                                           IDeviceEngine deviceEngine, CSVHelper cSVHelper, FlowBus _flowBus)
                                            : base(repository, regionManager, commonBus, cSVHelper, _flowBus)
        {
            flowBus = _flowBus;
            _deviceEngine = deviceEngine;
            _mController = motionController;
            Pages = new ObservableCollection<CommonPageModel>();
            Pages.Add(new CommonPageModel() { Name = "AutomaticEmbossing", IsSelected = true, Region = "", ViewType = typeof(AssTbAutomaticEmbossing) });
            SelectedReportPage = Pages.Where(x => x.IsSelected).FirstOrDefault();
            InitModels();

            EndCommand = new DelegateCommand(OnEnd);
            OneKeyCheckCommand = new DelegateCommand<object>(OnOneKeyCheck);
            UpdateItemsCommand = new DelegateCommand(OnUpdateItems);

            ConfigKey = "AutomaticEmbossingConfig";
            // 加载界面属性
            LoadStationConfigFromJson();
            //更新界面属性
            UpdateStationConfigs();

        }

        public override void OnEnd()
        {
            // 子界面的结束逻辑
            ProgressValue = 0; // 清空进度条
            base.OnEnd();
        }

        public override async void OnOneKeyCheck(object obj)
        {
            base.OnOneKeyCheck( obj);
            // 子界面的一键点检逻辑
            try
            {
                ProgressValue = 0; // 进度

                if (SelectedReportPage.ViewType == typeof(AssTbAutomaticEmbossing))
                {
                    if (_deviceEngine.GetMachineStatus() == EngineStatus.Ready)
                    {
                        var stations = _mController.MotionEngine.GetStations();
                        var stat = stations.FirstOrDefault(s => s.Alias == "自动压印");
                        if (stat != null)
                        {
                            flowBus.OnRunOne(stat.ID);
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
                                if (ItemModels[i] is AssTbAutomaticEmbossing PressRepe)
                                {
                                    // 如果AssData.csv表格内，状态一列为空，再根据标准值和实测值进行判断，填写状态
                                    // 如果AssData.csv表格内，状态一列不为空，使用表格的数据
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

                            // 绘制曲线
                            DrawPressureRepetitionChart();
                        }
                        else
                        {
                            throw new Exception("未找到自动压印站点");
                        }

                        //foreach (var stat in stations)
                        //{
                        //    if (stat.Alias == "自动压印")
                        //    {                                
                        //    }                           
                        //}
                    }
                    else
                    {
                        throw new FriendlyException("回零完成后方可运行测试流程");
                    }
                    //// 检查“标准值”是否为空
                    //if (string.IsNullOrWhiteSpace(autoEmboss.标准))
                    //{
                    //    autoEmboss.状态 = "格式错误";
                    //    continue;
                    //}
                    //FillTableContent(autoEmboss);
                    //ProgressValue = (i + 1) * 100 / ItemModels.Count; // 进度
                }

            }
            catch (Exception ex)
            {
                _commonbus.OnLog(new LogInfo() { LogType = LogType.Info, LogMessage = $"获取自动压印相关数据失败" });
                throw;
            }
            finally
            {

            }
        }

        public void FillTableContent(AssTbAutomaticEmbossing autoEmboss)
        {
            if (string.IsNullOrEmpty(autoEmboss.实测))
            {
                autoEmboss.状态 = "未完成";
            }
            else if (autoEmboss.标准 == autoEmboss.实测)
            {
                autoEmboss.状态 = "OK";
            }
            else if (autoEmboss.标准.Contains('~'))
            {
                var range = ParseColumnRange(autoEmboss.标准);
                double.TryParse(autoEmboss.实测, out double 实测浮点值);
                if (实测浮点值 >= range.lower && 实测浮点值 <= range.upper)
                {
                    autoEmboss.状态 = "OK";
                }
                else
                {
                    autoEmboss.状态 = "NG";
                }
            }
            else
            {
                autoEmboss.状态 = "NG";
            }
        }

        private void OnUpdateItems()
        {
            if (ItemModels.Any(x => ((AssTbAutomaticEmbossing)x).项次 == null))
            {
                ItemModels.Clear();
            }
            try
            {
                switch (SelectedReportPage.Name)
                {
                    case "AutomaticEmbossing":
                        var existingItem = ItemModels.OfType<AssTbAutomaticEmbossing>()
                                                               .FirstOrDefault(item => item.项次 == "吸嘴自动压印");
                        if (existingItem == null)
                        {
                            //// 填入平台默认的项次信息和对应的标准
                            //AssTbAutomaticEmbossing item0 = new AssTbAutomaticEmbossing()
                            //{
                            //    项序 = 0,
                            //    项次 = "吸嘴自动压印",
                            //    标准 = "999",
                            //    实测 = "",
                            //    完成时间 = DateTime.Now
                            //};
                            //AssTbAutomaticEmbossing item1 = new AssTbAutomaticEmbossing()
                            //{
                            //    项序 = 1,
                            //    项次 = "吸嘴1：灰度",
                            //    标准 = "999",
                            //    实测 = "",
                            //    完成时间 = DateTime.Now
                            //};
                            //AssTbAutomaticEmbossing item2 = new AssTbAutomaticEmbossing()
                            //{
                            //    项序 = 2,
                            //    项次 = "吸嘴1：面积",
                            //    标准 = "999",
                            //    实测 = "",
                            //    完成时间 = DateTime.Now
                            //};
                            //AssTbAutomaticEmbossing item3 = new AssTbAutomaticEmbossing()
                            //{
                            //    项序 = 3,
                            //    项次 = "吸嘴2：灰度",
                            //    标准 = "999",
                            //    实测 = "",
                            //    完成时间 = DateTime.Now
                            //};
                            //AssTbAutomaticEmbossing item4 = new AssTbAutomaticEmbossing()
                            //{
                            //    项序 = 4,
                            //    项次 = "吸嘴2：面积",
                            //    标准 = "999",
                            //    实测 = "",
                            //    完成时间 = DateTime.Now
                            //};

                            //ItemModels.Add(item0);
                            //ItemModels.Add(item1);
                            //ItemModels.Add(item2);
                            //ItemModels.Add(item3);
                            //ItemModels.Add(item4);
                        }
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
}
