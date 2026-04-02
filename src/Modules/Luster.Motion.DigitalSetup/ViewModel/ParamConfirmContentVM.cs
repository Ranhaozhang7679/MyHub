using HandyControl.Data;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using LiveChartsCore.Kernel;
using Luster.Common.DataAccess.Repositories;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Common.Tools;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.VDevice;
using Luster.Motion.DigitalSetup.AssTables;
using Luster.Motion.DigitalSetup.Datas;
using Luster.Motion.DigitalSetup.Helpers;
using Luster.Motion.DigitalSetup.Services;
using Luster.Motion.EditorUI;
using Luster.Motion.Integration.Web;
using Luster.Motion.TaskFlow.Engine.HyperTrain;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using static FreeSql.Internal.GlobalFilter;
using static SkiaSharp.HarfBuzz.SKShaper;

namespace Luster.Motion.DigitalSetup.ViewModel
{
    /// <summary>
    /// 参数导入确认
    /// </summary>
    public class ParamConfirmContentVM : BaseAss    //BindableBase
    {
        // 新增3个按钮和1个进度条的定义
        private double _progressValue;
        private readonly IWebService _webService;

        public ICommand EndCommand { get; private set; }
        public ICommand OneKeyCheckCommand { get; private set; }
        public ICommand UpdateItemsCommand { get; private set; }
        public double ProgressValue
        {
            get { return _progressValue; }
            set { SetProperty(ref _progressValue, value); }
        }

        public ParamConfirmContentVM(IRepository repository,
                                     IRegionManager regionManager,
                                     ICommonBus commonBus,
                                     CSVHelper cSVHelper,
                                     IWebService webService,
                                     FlowBus flowBus,
                                     IDialogService dialogService,
                                     CheckStatusService checkStatusService)
            : base(repository, regionManager, commonBus, cSVHelper, flowBus, dialogService, checkStatusService)
        {
            _parentRegionName = "ParamConfirmContent";

            Pages = new ObservableCollection<CommonPageModel>();
            Pages.Add(new CommonPageModel() { Name = "ObtainSwVersion", IsSelected = false, Region = "", ViewType = typeof(AssTbSwVersion) });

            // 注册子页面到DigitalAssPageModel
            DigitalAssPageModel.RegisterSubPages("ParamConfirmContent", Pages);

            SelectedReportPage = Pages.Where(x => x.IsSelected).FirstOrDefault();
            InitModels();

            EndCommand = new DelegateCommand(OnEnd);
            OneKeyCheckCommand = new DelegateCommand<object>(OnOneKeyCheck);
            UpdateItemsCommand = new DelegateCommand(OnUpdateItems);
            this._webService = webService;
            LoadCheckConfirmMessages();

            // 延迟加载点检状态，确保 UI 绑定已建立
            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                LoadCheckStatusForAllPages();
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        public override void OnEnd()
        {
            // 子界面的结束逻辑
            ProgressValue = 0; // 清空进度条
            base.OnEnd();
        }

        /// <summary>
        /// 加载所有子页面的历史点检状态
        /// </summary>
        private void LoadCheckStatusForAllPages()
        {
            if (_checkStatusService == null || Pages == null)
            {
                return;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"[ParamConfirmContent] 开始加载点检状态，页面数量: {Pages.Count}");

                foreach (var page in Pages)
                {
                    if (page != null)
                    {
                        page.ParentRegion = "ParamConfirmContent";
                        var record = _checkStatusService.GetRecord(page.PageKey);

                        System.Diagnostics.Debug.WriteLine($"[ParamConfirmContent] 加载页面 {page.PageKey} 的状态");

                        if (record != null)
                        {
                            page.CheckStatus = record.Status;
                            page.LastCheckTime = record.CheckTime;
                            page.LastCheckOperator = record.Operator;
                            page.CheckRemark = record.Remark;
                            System.Diagnostics.Debug.WriteLine($"[ParamConfirmContent] 页面 {page.Name} 状态: {record.Status}, 时间: {record.CheckTime}");
                        }
                        else
                        {
                            page.CheckStatus = CheckStatus.NotChecked;
                            page.LastCheckTime = null;
                            page.LastCheckOperator = null;
                            page.CheckRemark = null;
                            System.Diagnostics.Debug.WriteLine($"[ParamConfirmContent] 页面 {page.Name} 无历史记录");
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine($"[ParamConfirmContent] 点检状态加载完成");
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

        /// <summary>
        /// 获取页面整体状态
        /// </summary>
        private string GetOverallStatus()
        {
            try
            {
                if (ItemModels == null || ItemModels.Count == 0)
                    return "未点检";

                foreach (var item in ItemModels)
                {
                    if (item is AssTbSwVersion swVersion)
                    {
                        if (swVersion.状态 != "OK")
                            return "NG";
                    }
                }
                return "OK";
            }
            catch
            {
                return "获取失败";
            }
        }

        public override async void OnOneKeyCheck(object obj)
        {
            await base.OnOneKeyCheckAsync(obj);
            // 子界面的一键点检逻辑
            try
            {
                ProgressValue = 0; // 进度

                // 获取 WebConfig 信息
                var webConfig = _webService.GetConfig() as WebConfig;

                for (int i = 0; i < ItemModels.Count; i++)
                {
                    if (ItemModels[i] is AssTbSwVersion swVersion)
                    {
                        // 检查标准是否为空
                        if (string.IsNullOrWhiteSpace(swVersion.标准))
                        {
                            swVersion.状态 = "格式错误";
                            continue;
                        }

                        // 根据项次从 WebConfig 获取实测值
                        string actualVersion = GetVersionFromWebConfig(webConfig, swVersion.项次);
                        swVersion.实测 = actualVersion;

                        // 比较标准和实测
                        if (swVersion.标准.Trim() == swVersion.实测.Trim())
                        {
                            swVersion.状态 = "OK";
                        }
                        else
                        {
                            swVersion.状态 = "NG";
                        }

                        swVersion.完成时间 = DateTime.Now;
                        ProgressValue = (i + 1) * 100 / ItemModels.Count; // 进度
                    }
                }

                // 点检完成后，将结果存储到 PageStatusService
                string overallStatus = GetOverallStatus();
                var checkStatus = overallStatus switch
                {
                    "OK" => CheckStatus.CheckedOK,
                    "NG" => CheckStatus.CheckedFail,
                    _ => CheckStatus.NotChecked
                };
                SaveCheckStatus(checkStatus, overallStatus);

                // 同步一级界面整体状态到 PageStatusService
                SyncOverallStatusToPageStatusService();
            }
            catch (Exception ex)
            {
                _commonbus.OnLog(new LogInfo() { LogType = LogType.Info, LogMessage = $"获取软件版本失败" });
            }
            finally
            {
                ProgressValue = 100;
            }
        }

        /// <summary>
        /// 根据项次名称从 WebConfig 获取对应版本
        /// </summary>
        private string GetVersionFromWebConfig(WebConfig webConfig, string itemName)
        {
            if (webConfig == null) return "NN.NN";

            return itemName switch
            {
                "平台版本" => webConfig.SoftVersion ?? "NN.NN",
                "配方版本" => webConfig.ReciepeVersion ?? "NN.NN",
                "PLC版本" => webConfig.PlcVersion ?? "NN.NN",
                "视觉版本" => webConfig.VisionVersion ?? "NN.NN",
                "机器人版本" => webConfig.RobotVersion ?? "NN.NN",
                "激光版本" => webConfig.LaserVersion ?? "NN.NN",
                _ => "NN.NN"
            };
        }

        private void OnUpdateItems()
        {
            ItemModels.Clear();
            try
            {
                switch (SelectedReportPage.Name)
                {
                    case "ObtainSwVersion":
                        // 从 CSV 文件读取标准数据
                        long totalCount = 0;
                        var items = _csvHelper.GetAllDataNew1<AssTbSwVersion>(0, 0, out totalCount);
                        foreach (var item in items)
                        {
                            // 清理纯空壳数据：如果项次和标准都为空，说明是无意义的残留占位符，不应该被视作真实数据
                            if (string.IsNullOrWhiteSpace(item.项次) && string.IsNullOrWhiteSpace(item.标准))
                            {
                                continue;
                            }

                            item.实测 = "";
                            item.状态 = "";
                            ItemModels.Add(item);
                        }

                        // CSV读取失败或无实质数据时，进行默认兜底配置（备用逻辑）
                        if (ItemModels.Count == 0)
                        {
                            string[] defaultItems = { "平台版本", "配方版本", "PLC版本", "视觉版本", "机器人版本", "激光版本" };
                            for (int i = 0; i < defaultItems.Length; i++)
                            {
                                ItemModels.Add(new AssTbSwVersion()
                                {
                                    项序 = i,
                                    项次 = defaultItems[i],
                                    标准 = "NN.NN",
                                    实测 = "",
                                    状态 = "未点检",
                                    完成时间 = DateTime.Now
                                });
                            }
                        }

                        PageStatusService.Instance.UpdateStatus("MainParameters", "未点检");
                        break;
                }
            }
            catch (Exception)
            {

            }
        }
    }
}
