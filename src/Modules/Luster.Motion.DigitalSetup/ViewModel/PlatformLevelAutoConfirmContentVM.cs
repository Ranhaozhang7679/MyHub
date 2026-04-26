using LiveCharts;
using Luster.Common.DataAccess.Repositories;
using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.CommonUI;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
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
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// 平台水平自动确认
    /// </summary>
    public class PlatformLevelAutoConfirmContentVM : BaseAss
    {
        // 新增3个按钮和1个进度条的定义

        private const string PageName = "Horizontal";
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

        /// <summary>
        /// 进度条
        /// </summary>
        private double _progressValue;
        public double ProgressValue
        {
            get { return _progressValue; }
            set { SetProperty(ref _progressValue, value); }
        }

        public PlatformLevelAutoConfirmContentVM(IRepository repository,
                                                 IRegionManager regionManager, IMotionController motionController,
                                                 IDeviceEngine deviceEngine, FlowBus _flowBus, ICommonBus commonBus, CSVHelper cSVHelper, IDialogService dialogService,
                                                 CheckStatusService checkStatusService)
                                                    : base(repository, regionManager, commonBus, cSVHelper, _flowBus, dialogService, checkStatusService)
        {
            flowBus = _flowBus;
            _deviceEngine = deviceEngine;
            _mController = motionController;
            _parentRegionName = "PlatformLevelAutoConfirmContent";
            Pages = new ObservableCollection<CommonPageModel>();
            Pages.Add(new CommonPageModel() { Name = "AutomaticPosAndLeveling", IsSelected = false, Region = "", ViewType = typeof(AssTbAutomaticPosAndLeveling) });

            // 注册子页面到DigitalAssPageModel
            DigitalAssPageModel.RegisterSubPages("PlatformLevelAutoConfirmContent", Pages);

            SelectedReportPage = Pages.Where(x => x.IsSelected).FirstOrDefault();
            InitModels();

            EndCommand = new DelegateCommand(OnEnd);
            //OneKeyCheckCommand = new DelegateCommand(OnOneKeyCheck);
            OneKeyCheckCommand = new DelegateCommand<object>(OnOneKeyCheck);
            UpdateItemsCommand = new DelegateCommand(OnUpdateItems);

            ConfigKey = "AutomaticPosAndLevelingConfig";
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
                        page.ParentRegion = "PlatformLevelAutoConfirmContent";
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
                if (item is AssTbAutomaticPosAndLeveling posAndLevel)
                {
                    if (posAndLevel.状态 != "OK")
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
                ProgressValue = 0; // 进度
                if (SelectedReportPage.ViewType == typeof(AssTbAutomaticPosAndLeveling))
                {
                    if (_deviceEngine.GetMachineStatus() == EngineStatus.Ready)
                    {
                        var stations = _mController.MotionEngine.GetStations();
                        var stat = stations.FirstOrDefault(s => s.Alias == "自动定位与水平");
                        if (stat != null)
                        {
                            flowBus.OnRunOne(stat.ID);

                            // 异步等待stat.Status == RunStatus.Success，增加超时机制
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
                                if (ItemModels[i] is AssTbAutomaticPosAndLeveling caliTable)
                                {
                                    if (string.IsNullOrWhiteSpace(caliTable.标准))
                                    {
                                        caliTable.状态 = "格式错误";
                                        continue;
                                    }
                                    FillTableContent(caliTable);
                                }
                                ProgressValue = (i + 1) * 100 / ItemModels.Count; // 进度
                            }

                            // 绘制曲线
                            DrawPressureRepetitionChart();
                            string overallStatus = GetOverallStatus();
                            PageStatusService.Instance.UpdateStatus(PageName, overallStatus);
                        }
                        else
                        {
                            throw new Exception("未找到自动定位与水平站点，请检查流程配置。");
                        }
                    }
                    else
                    {
                        //throw new FriendlyException("回零完成后方可运行测试流程");
                    }
                }


            }
            catch (OperationCanceledException)
            {
                _commonbus.OnLog(new LogInfo() { LogType = LogType.Info, LogMessage = "平台水平点检被用户中止" });
                throw;
            }
            catch (Exception ex)
            {
                _commonbus.OnLog(new LogInfo() { LogType = LogType.Info, LogMessage = $"获取平台水平相关数据失败: {ex.Message}" });
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

        public void FillTableContent(AssTbAutomaticPosAndLeveling posAndLevel)
        {
            if (string.IsNullOrEmpty(posAndLevel.实测))
            {
                posAndLevel.状态 = "未完成";
            }
            else if (posAndLevel.标准 == posAndLevel.实测)
            {
                posAndLevel.状态 = "OK";
            }
            else if (posAndLevel.标准.Contains('~'))
            {
                var range = ParseColumnRange(posAndLevel.标准);
                bool bIsOk = double.TryParse(posAndLevel.实测, out double 实测浮点值);
                if (!bIsOk)
                {
                    posAndLevel.实测 = 实测浮点值.ToString();//给默认值0
                }
                if (实测浮点值 >= range.lower && 实测浮点值 <= range.upper)
                {
                    posAndLevel.状态 = "OK";
                }
                else
                {
                    posAndLevel.状态 = "NG";
                }
            }
            else
            {
                posAndLevel.状态 = "NG";
            }
        }

        private void OnUpdateItems()
        {
            if (ItemModels.Any(x => ((AssTbAutomaticPosAndLeveling)x).项次 == null))
            {
                ItemModels.Clear();
            }
            try
            {
                //switch (SelectedReportPage.Name)
                //{
                //    case "AutomaticPosAndLeveling":
                //        var existingItem = ItemModels.OfType<AssTbAutomaticPosAndLeveling>()
                //                                               .FirstOrDefault(item => item.项次 == "机台摆放定位");
                //        if (existingItem == null)
                //        {
                //            // 填入平台默认的项次信息和对应的标准
                //            AssTbAutomaticPosAndLeveling item0 = new AssTbAutomaticPosAndLeveling()
                //            {
                //                项序 = 0,
                //                项次 = "机台摆放定位",
                //                标准 = "/", // 默认标准值为空，无需填写
                //                实测 = "/", // 默认实测值为空，无需填写
                //                完成时间 = DateTime.Now
                //            };
                //            AssTbAutomaticPosAndLeveling item1 = new AssTbAutomaticPosAndLeveling()
                //            {
                //                项序 = 1,
                //                项次 = "高度调整：载具表面离地面高度(cm)",
                //                标准 = "94.5~95.5", // 默认标准值为95+-0.5
                //                实测 = "",          // 默认实测值为0
                //                完成时间 = DateTime.Now
                //            };
                //            AssTbAutomaticPosAndLeveling item2 = new AssTbAutomaticPosAndLeveling()
                //            {
                //                项序 = 2,
                //                项次 = "水平调整：四个角水平仪气泡居中(cm)",
                //                标准 = "4.5~5.5",
                //                实测 = "",
                //                完成时间 = DateTime.Now
                //            };
                //            AssTbAutomaticPosAndLeveling item3 = new AssTbAutomaticPosAndLeveling()
                //            {
                //                项序 = 3,
                //                项次 = "水平调整：中心点水平仪气泡居中(cm)",
                //                标准 = "4.5~5.5",
                //                实测 = "",
                //                完成时间 = DateTime.Now
                //            };
                //            AssTbAutomaticPosAndLeveling item4 = new AssTbAutomaticPosAndLeveling()
                //            {
                //                项序 = 4,
                //                项次 = "前后机台流道基准：流道载具水平，计算公式为：|(h1+h2+h3+h4)/4-h5|<0.05",
                //                标准 = "-0.05~0.05",
                //                实测 = "",
                //                完成时间 = DateTime.Now
                //            };
                //            ItemModels.Add(item0);
                //            ItemModels.Add(item1);
                //            ItemModels.Add(item2);
                //            ItemModels.Add(item3);
                //            ItemModels.Add(item4);
                //        }
                //        break; // 添加 break 以避免 CS8070 错误
                //}
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
