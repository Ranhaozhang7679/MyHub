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
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Luster.Motion.DigitalSetup.ViewModel
{
    /// <summary>
    /// 手眼标定
    /// </summary>
    public class AutoVisualCalibrationContentVM : BaseAss
    {
        // 新增3个按钮和1个进度条的定义
        private double _progressValue;

        public ICommand EndCommand { get; private set; }
        public ICommand OneKeyCheckCommand { get; private set; }
        public ICommand UpdateItemsCommand { get; private set; }

        private bool _isChartVisible = true;
        public bool IsChartVisible
        {
            get { return _isChartVisible; }
            set { SetProperty(ref _isChartVisible, value); }
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

                //if (_seletedReportPage.ViewType == typeof(AssTbAutoFocusing))
                //{
                //    IsChartVisible = true;
                //}
                //else
                //{
                //    IsChartVisible = false;
                //}

                ConfigKey = _seletedReportPage.ViewType switch
                {
                    Type type when type == typeof(AssTbAutoFocusing) => "AutoVisualCalibration",
                    _ => ConfigKey
                };

                // 加载界面属性
                LoadStationConfigFromJson();
                //更新界面属性
                UpdateStationConfigs();

            }
        }

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
        public AutoVisualCalibrationContentVM(IRepository repository,
                                      IRegionManager regionManager, IMotionController motionController, IDeviceEngine deviceEngine, FlowBus _flowBus, ICommonBus commonBus,
                                        CSVHelper cSVHelper) : base(repository, regionManager, commonBus, cSVHelper, _flowBus)
        {
            flowBus = _flowBus;
            _deviceEngine = deviceEngine;
            _mController = motionController;
            Pages = new ObservableCollection<CommonPageModel>();
            Pages.Add(new CommonPageModel() { Name = "AutoVisualCalibration", IsSelected = true, Region = "", ViewType = typeof(AssTbAutoVisualCalibration) });
            SelectedReportPage = Pages.Where(x => x.IsSelected).FirstOrDefault();
            InitModels();

            EndCommand = new DelegateCommand(OnEnd);
            OneKeyCheckCommand = new DelegateCommand<object>(OnOneKeyCheck);
            UpdateItemsCommand = new DelegateCommand(OnUpdateItems);

            ConfigKey = "AutoVisualCalibration";
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
            base.OnOneKeyCheck(obj);
            // 子界面的一键点检逻辑
            try
            {
                var stations = _mController.MotionEngine.GetStations();
                ProgressValue = 0; // 进度
                if (SelectedReportPage.ViewType == typeof(AssTbAutoVisualCalibration))
                {
                    if (_deviceEngine.GetMachineStatus() == EngineStatus.Ready)
                    {                       
                        var stat = stations.FirstOrDefault(s => s.Alias == "手眼标定");
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
                                if (ItemModels[i] is AssTbAutoVisualCalibration PressRepe)
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

                            // 绘制曲线
                            DrawPressureRepetitionChart();
                        }
                        else
                        {
                            throw new FriendlyException("未找到手眼标定站点");
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
                _commonbus.OnLog(new LogInfo() { LogType = LogType.Info, LogMessage = $"" });
                throw;
            }
            finally
            {

            }
        }

        public void FillTableContent(AssTb visualCali)
        {
            if (string.IsNullOrEmpty(visualCali.实测))
            {
                visualCali.状态 = "未完成";
            }
            else if (visualCali.标准 == visualCali.实测)
            {
                visualCali.状态 = "OK";
            }
            else if (visualCali.标准.Contains('~'))
            {
                var range = ParseColumnRange(visualCali.标准);
                double.TryParse(visualCali.实测, out double 实测浮点值);
                if (实测浮点值 >= range.lower && 实测浮点值 <= range.upper)
                {
                    visualCali.状态 = "OK";
                }
                else
                {
                    visualCali.状态 = "NG";
                }
            }
            else
            {
                visualCali.状态 = "NG";
            }
        }

        private void OnUpdateItems()
        {
            //if (ItemModels.Any(x => ((AssTb)x).项次 == null)) // AssTbAutoVisualCalibration
            //{
            //    ItemModels.Clear();
            //}
            //try
            //{
            //    switch (SelectedReportPage.Name)
            //    {
            //        case "AutoVisualCalibration":
            //            var existingItem = ItemModels.OfType<AssTbAutoVisualCalibration>()
            //                                                   .FirstOrDefault(item => item.项次 == "自动视觉调试");
            //            if (existingItem == null)
            //            {
            //                // 填入平台默认的项次信息和对应的标准
            //                AssTbAutoVisualCalibration item0 = new AssTbAutoVisualCalibration()
            //                {
            //                    项序 = 0,
            //                    项次 = "自动视觉调试",
            //                    标准 = "0.9~1.1", // 默认标准值为1+-0.1
            //                    实测 = "",        // 默认实测值为空
            //                    完成时间 = DateTime.Now
            //                };
            //                AssTbAutoVisualCalibration item1 = new AssTbAutoVisualCalibration()
            //                {
            //                    项序 = 1,
            //                    项次 = "吸嘴1焦距：锐利度",
            //                    标准 = "0.9~1.1", // 默认标准值为1+-0.1
            //                    实测 = "",        // 默认实测值为空
            //                    完成时间 = DateTime.Now
            //                };
            //                AssTbAutoVisualCalibration item2 = new AssTbAutoVisualCalibration()
            //                {
            //                    项序 = 2,
            //                    项次 = "吸嘴1视野：X",
            //                    标准 = "90~110", // 默认标准值为95+-0.5
            //                    实测 = "",          // 默认实测值为空
            //                    完成时间 = DateTime.Now
            //                };
            //                AssTbAutoVisualCalibration item3 = new AssTbAutoVisualCalibration()
            //                {
            //                    项序 = 3,
            //                    项次 = "吸嘴1视野：Y",
            //                    标准 = "90~110",
            //                    实测 = "",
            //                    完成时间 = DateTime.Now
            //                };
            //                AssTbAutoVisualCalibration item4 = new AssTbAutoVisualCalibration()
            //                {
            //                    项序 = 4,
            //                    项次 = "吸嘴1视野：Z",
            //                    标准 = "90~110",
            //                    实测 = "",
            //                    完成时间 = DateTime.Now
            //                };
            //                AssTbAutoVisualCalibration item5 = new AssTbAutoVisualCalibration()
            //                {
            //                    项序 = 5,
            //                    项次 = "相机1灰度：灰度值",
            //                    标准 = "0.9~1.1",
            //                    实测 = "",
            //                    完成时间 = DateTime.Now
            //                };
            //                ItemModels.Add(item0);
            //                ItemModels.Add(item1);
            //                ItemModels.Add(item2);
            //                ItemModels.Add(item3);
            //                ItemModels.Add(item4);
            //                ItemModels.Add(item5);
            //            }
            //            break;
            //        case "AutoFocusing":

            //            break;

            //        case "AutoFieldOfView":

            //            break;
            //        case "AutoGrayScale":

            //            break;
            //    }
            //}
            //catch (Exception)
            //{
            //    // 异常处理逻辑
            //}
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
