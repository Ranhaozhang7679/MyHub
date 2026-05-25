using Luster.Common.DataAccess.Repositories;
using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.CommonUI;
using Luster.Motion.DataStruct;
using Luster.Motion.DigitalSetup.AssTables;
using Luster.Motion.DigitalSetup.Datas;
using Luster.Motion.DigitalSetup.Helpers;
using Luster.Motion.DigitalSetup.Services;
using Luster.Motion.EditorUI;
using Luster.Motion.TaskFlow.Engine;
using Luster.Motion.TaskFlow.Engine.HyperTrain;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Motion;
using Prism.Commands;
using Prism.Regions;
using Prism.Services.Dialogs;
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
        private const string PageName = "AutoVisualCalibration";

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
                if (_seletedReportPage == null) return;
                ConfigKey = _seletedReportPage.ViewType switch
                {
                    Type type when type == typeof(AssTbAutoFocusing) => "AutoVisualCalibration",
                    _ => ConfigKey
                };

                // 加载界面属性
                LoadStationConfigFromJson();
                //更新界面属性
                UpdateStationConfigs();
                // 加载工站点检状态
                LoadStationCheckStatus();

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
                                        CSVHelper cSVHelper, IDialogService dialogService, CheckStatusService checkStatusService) : base(repository, regionManager, commonBus, cSVHelper, _flowBus, dialogService, checkStatusService)
        {
            _parentRegionName = "AutoVisualCalibrationContent";

            flowBus = _flowBus;
            _deviceEngine = deviceEngine;
            _mController = motionController;
            Pages = new ObservableCollection<CommonPageModel>();
            Pages.Add(new CommonPageModel() { Name = "AutoVisualCalibration", IsSelected = false, Region = "", ViewType = typeof(AssTbAutoVisualCalibration) });

            // 注册子页面到DigitalAssPageModel
            DigitalAssPageModel.RegisterSubPages("AutoVisualCalibrationContent", Pages);

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
            // 加载工站点检状态
            LoadStationCheckStatus();
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
                        page.ParentRegion = "AutoVisualCalibrationContent";
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

        private string GetOverallStatus()
        {
            if (ItemModels == null || ItemModels.Count == 0)
                return "未点检";

            foreach (var item in ItemModels)
            {
                if (item is AssTbAutoVisualCalibration visualCalibration)
                {
                    if (visualCalibration.状态 != "OK")
                        return "NG";
                }
            }
            return "OK";
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
                                // 阶段1：等待流程启动（离开Default）
                                while (ReadStatus(stat) == RunStatus.Default)
                                {
                                    _cts.Token.ThrowIfCancellationRequested();
                                    await Task.Delay(100);
                                }
                                // 阶段2：等待流程完成（Running/Pause继续等待，其他状态视为结束）
                                while (true)
                                {
                                    var s = ReadStatus(stat);
                                    if (s != RunStatus.Running && s != RunStatus.Pause)
                                        break;
                                    _cts.Token.ThrowIfCancellationRequested();
                                    await Task.Delay(200);
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
                string overallStatus = GetOverallStatus();
            }
            catch (OperationCanceledException)
            {
                _commonbus.OnLog(new LogInfo() { LogType = LogType.Info, LogMessage = "手眼标定被用户中止" });
                throw;
            }
            catch (Exception ex)
            {
                _commonbus.OnLog(new LogInfo() { LogType = LogType.Info, LogMessage = $"{ex.Message}" });
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
            UpdateItemsFromCsv();
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

        /// <summary>
        /// 线程安全地读取模块状态，通过内存屏障确保读取到最新值
        /// </summary>
        private static RunStatus ReadStatus(IMotionModule module)
        {
            System.Threading.Thread.MemoryBarrier();
            return module.Status;
        }
    }
}
