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
using Luster.Motion.DigitalSetup.Services;

namespace Luster.Motion.DigitalSetup.ViewModel
{
    /// <summary>
    /// 视觉调试
    /// </summary>
    public class DigitalVisionContentVM : BaseAss
    {
        // 新增3个按钮和1个进度条的定义
        private double _progressValue;
        private const string PageName = "DigitalVision";

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
                    Type type when type == typeof(AssTbAutoFocusing) => "AutoFocusingConfig",
                    //Type type when type == typeof(AssTbAutoFieldOfView) => "AutoFieldOfViewConfig",
                    //Type type when type == typeof(AssTbAutoGrayScale) => "AutoGrayScaleConfig",
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
        public DigitalVisionContentVM(IRepository repository,
                                      IRegionManager regionManager, IMotionController motionController, IDeviceEngine deviceEngine, FlowBus _flowBus, ICommonBus commonBus,
                                        CSVHelper cSVHelper, IDialogService dialogService, CheckStatusService checkStatusService) : base(repository, regionManager, commonBus, cSVHelper, _flowBus, dialogService, checkStatusService)
        {
            flowBus = _flowBus;
            _deviceEngine = deviceEngine;
            _mController = motionController;
            _parentRegionName = "DigitalVisionContent";
            Pages = new ObservableCollection<CommonPageModel>();
            Pages.Add(new CommonPageModel() { Name = "AutoFocusing", IsSelected = false, Region = "", ViewType = typeof(AssTbAutoFocusing) });
            //Pages.Add(new CommonPageModel() { Name = "AutoGrayScale", IsSelected = false, Region = "", ViewType = typeof(AssTbAutoGrayScale) });
            //Pages.Add(new CommonPageModel() { Name = "AutoFieldOfView", IsSelected = false, Region = "", ViewType = typeof(AssTbAutoFieldOfView) });
            //Pages.Add(new CommonPageModel() { Name = "AutoVisualCalibration", IsSelected = true, Region = "", ViewType = typeof(AssTbAutoVisualCalibration) });

            // 注册子页面到DigitalAssPageModel
            DigitalAssPageModel.RegisterSubPages("DigitalVisionContent", Pages);

            SelectedReportPage = Pages.Where(x => x.IsSelected).FirstOrDefault();
            InitModels();

            EndCommand = new DelegateCommand(OnEnd);
            OneKeyCheckCommand = new DelegateCommand<object>(OnOneKeyCheck);
            UpdateItemsCommand = new DelegateCommand(OnUpdateItems);

            ConfigKey = "AutoFocusingConfig";
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
                        page.ParentRegion = "DigitalVisionContent";
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

        public async override void OnOneKeyCheck(object obj)
        {
            if (IsChecking) return;
            IsChecking = true;

            await base.OnOneKeyCheckAsync(obj);
            // 子界面的一键点检逻辑
            bool wasCancelled = false;

            try
            {
                ProgressValue = 0; // 进度
                var stations = _mController.MotionEngine.GetStations();
                if (SelectedReportPage.ViewType == typeof(AssTbAutoFocusing))
                {
                    if (_deviceEngine.GetMachineStatus() == EngineStatus.Ready)
                    {
                        var stat = stations.FirstOrDefault(s => s.Alias == "自动对焦");
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
                                if (ItemModels[i] is AssTbAutoFocusing PressRepe)
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
                            throw new Exception("未找到自动对焦站点");
                        }
                    }
                    else
                    {
                        throw new FriendlyException("回零完成后方可运行测试流程");
                    }
                }
                else if (SelectedReportPage.ViewType == typeof(AssTbAutoFieldOfView))
                {
                    if (_deviceEngine.GetMachineStatus() == EngineStatus.Ready)
                    {
                        var stat = stations.FirstOrDefault(s => s.Alias == "自动视野");
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
                                if (ItemModels[i] is AssTbAutoFieldOfView PressRepe)
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
                            throw new Exception("未找到自动视野站点");
                        }

                    }
                    else
                    {
                        throw new FriendlyException("回零完成后方可运行测试流程");
                    }

                }
                else if (SelectedReportPage.ViewType == typeof(AssTbAutoGrayScale))
                {
                    if (_deviceEngine.GetMachineStatus() == EngineStatus.Ready)
                    {
                        var stat = stations.FirstOrDefault(s => s.Alias == "自动灰度");
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
                                if (ItemModels[i] is AssTbAutoGrayScale PressRepe)
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
                            throw new Exception("未找到自动灰度站点");
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
                wasCancelled = true;
                _commonbus.OnLog(new LogInfo() { LogType = LogType.Info, LogMessage = "点检被用户中止" });
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

                if (wasCancelled && _cts.IsCancellationRequested)
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

        private string GetOverallStatus()
        {
            if (ItemModels == null || ItemModels.Count == 0)
                return "未点检";

            foreach (var item in ItemModels)
            {
                string status = "";
                if (item is AssTbAutoFocusing focusing)
                    status = focusing.状态;
                else if (item is AssTbAutoFieldOfView fieldOfView)
                    status = fieldOfView.状态;
                else if (item is AssTbAutoGrayScale grayScale)
                    status = grayScale.状态;
                else if (item is AssTbAutoVisualCalibration visualCalibration)
                    status = visualCalibration.状态;

                if (status == "NG")
                    return "NG";
            }
            return "OK";
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
            if (ItemModels.Any(x => ((AssTb)x).项次 == null)) // AssTbAutoVisualCalibration
            {
                ItemModels.Clear();
            }
            try
            {
                switch (SelectedReportPage.Name)
                {
                    case "AutoVisualCalibration":
                        var existingItem = ItemModels.OfType<AssTbAutoVisualCalibration>()
                                                               .FirstOrDefault(item => item.项次 == "自动视觉调试");
                        if (existingItem == null)
                        {
                            // 填入平台默认的项次信息和对应的标准
                            AssTbAutoVisualCalibration item0 = new AssTbAutoVisualCalibration()
                            {
                                项序 = 0,
                                项次 = "自动视觉调试",
                                标准 = "0.9~1.1", // 默认标准值为1+-0.1
                                实测 = "",        // 默认实测值为空
                                完成时间 = DateTime.Now
                            };
                            AssTbAutoVisualCalibration item1 = new AssTbAutoVisualCalibration()
                            {
                                项序 = 1,
                                项次 = "吸嘴1焦距：锐利度",
                                标准 = "0.9~1.1", // 默认标准值为1+-0.1
                                实测 = "",        // 默认实测值为空
                                完成时间 = DateTime.Now
                            };
                            AssTbAutoVisualCalibration item2 = new AssTbAutoVisualCalibration()
                            {
                                项序 = 2,
                                项次 = "吸嘴1视野：X",
                                标准 = "90~110", // 默认标准值为95+-0.5
                                实测 = "",          // 默认实测值为空
                                完成时间 = DateTime.Now
                            };
                            AssTbAutoVisualCalibration item3 = new AssTbAutoVisualCalibration()
                            {
                                项序 = 3,
                                项次 = "吸嘴1视野：Y",
                                标准 = "90~110",
                                实测 = "",
                                完成时间 = DateTime.Now
                            };
                            AssTbAutoVisualCalibration item4 = new AssTbAutoVisualCalibration()
                            {
                                项序 = 4,
                                项次 = "吸嘴1视野：Z",
                                标准 = "90~110",
                                实测 = "",
                                完成时间 = DateTime.Now
                            };
                            AssTbAutoVisualCalibration item5 = new AssTbAutoVisualCalibration()
                            {
                                项序 = 5,
                                项次 = "相机1灰度：灰度值",
                                标准 = "0.9~1.1",
                                实测 = "",
                                完成时间 = DateTime.Now
                            };
                            ItemModels.Add(item0);
                            ItemModels.Add(item1);
                            ItemModels.Add(item2);
                            ItemModels.Add(item3);
                            ItemModels.Add(item4);
                            ItemModels.Add(item5);
                        }
                        break;
                    case "AutoFocusing":

                        break;

                    case "AutoFieldOfView":

                        break;
                    case "AutoGrayScale":

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
