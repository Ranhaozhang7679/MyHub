using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using System.Collections.ObjectModel;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static System.Windows.Forms.AxHost;
using Luster.Motion.Assests.Langs;
using Prism.Services.Dialogs;

namespace Luster.Motion.DigitalSetup.ViewModel
{
    /// <summary>
    /// AutoVerication
    /// </summary>
    public class AutoVericationContentVM : BaseAss
    {
        // 进度条定义
        private double _progressValue;
        private string _paramConfirmStatus = "未点检";

        public ICommand EndCommand { get; private set; }
        public ICommand OneKeyCheckCommand { get; private set; }
        public ICommand UpdateItemsCommand { get; private set; }
        public ICommand QueryCommand { get; private set; }
        public ICommand PageUpdatedCommand { get; private set; }

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

                // 设置配置键
                if (_seletedReportPage.ViewType == typeof(AssTbAutoVerication))
                {
                    ConfigKey = "AutoVericationConfig";
                }

                // 加载界面属性
                LoadStationConfigFromJson();
                //更新界面属性
                UpdateStationConfigs();
            }
        }

        public AutoVericationContentVM(IRepository repository,
                                        IRegionManager regionManager,
                                        ICommonBus commonBus,
                                        IMotionController motionController,
                                        IDeviceEngine deviceEngine,
                                        FlowBus _flowBus,
                                        CSVHelper cSVHelper,IDialogService dialogService)
                                        : base(repository, regionManager, commonBus, cSVHelper, _flowBus, dialogService)
        {
            flowBus = _flowBus;
            _deviceEngine = deviceEngine;
            _mController = motionController;

            Pages = new ObservableCollection<CommonPageModel>();
            Pages.Add(new CommonPageModel() { Name = "AutoVerication", IsSelected = true, Region = "", ViewType = typeof(AssTbAutoVerication) });

            SelectedReportPage = Pages.Where(x => x.IsSelected).FirstOrDefault();
            InitModels();

            EndCommand = new DelegateCommand(OnEnd);
            OneKeyCheckCommand = new DelegateCommand<object>(OnOneKeyCheck);
            UpdateItemsCommand = new DelegateCommand(OnUpdateItems);
            QueryCommand = new DelegateCommand(OnQuery);
            PageUpdatedCommand = new DelegateCommand<object>(OnPageUpdated);

            ConfigKey = "AutoVericationConfig";
            // 加载界面属性
            LoadStationConfigFromJson();
            //更新界面属性
            UpdateStationConfigs();

            // 订阅状态服务的更新事件，实时获取状态变化
            PageStatusService.Instance.StatusChanged += OnPageStatusChanged;

            InitializePageStatus();
        }

        /// <summary>
        /// 页面状态变更事件处理
        /// </summary>
        private void OnPageStatusChanged(string pageName, string status)
        {
            // 更新表格中对应行的状态
            var item = ItemModels.OfType<AssTbAutoVerication>()
                .FirstOrDefault(x => x.项次 == GetLocalizedPageName(pageName));

            if (item != null)
            {
                item.状态 = status;
                item.完成时间 = DateTime.Now;
                RaisePropertyChanged(nameof(ItemModels));
            }
        }

        private void InitializePageStatus()
        {
            // 先初始化表格
            OnUpdateItems();

            // 刷新所有页面的状态
            foreach (var item in ItemModels)
            {
                if (item is AssTbAutoVerication autoVer)
                {
                    string pageName = GetEnglishKeyFromDisplayName(autoVer.项次);
                    string status = PageStatusService.Instance.GetStatus(pageName);
                    autoVer.状态 = status;
                    autoVer.完成时间 = DateTime.Now;
                }
            }

            RaisePropertyChanged(nameof(ItemModels));
        }

        public override void OnEnd()
        {
            // 子界面的结束逻辑
            ProgressValue = 0; 
            base.OnEnd();
        }

        public override async void OnOneKeyCheck(object obj)
        {
            base.OnOneKeyCheck(obj);

            try
            {
                ProgressValue = 50;
                await Task.Delay(500);

                // 刷新所有页面的状态
                foreach (var item in ItemModels)
                {
                    if (item is AssTbAutoVerication autoVer)
                    {
                        string pageName = GetEnglishKeyFromDisplayName(autoVer.项次);
                        string status = PageStatusService.Instance.GetStatus(pageName);
                        autoVer.状态 = status;
                        autoVer.完成时间 = DateTime.Now;
                    }
                }

                RaisePropertyChanged(nameof(ItemModels));
                ProgressValue = 100;
            }
            catch (Exception ex)
            {
                _commonbus.OnLog(new LogInfo() { LogType = LogType.Info, LogMessage = $"读取状态失败: {ex.Message}" });
                ProgressValue = 0;
            }
        }

        public void FillTableContent(AssTb autoVer)
        {
            if (string.IsNullOrEmpty(autoVer.实测))
            {
                autoVer.状态 = "未完成";
            }
            else if (autoVer.标准 == autoVer.实测)
            {
                autoVer.状态 = "OK";
            }
            else if (autoVer.标准.Contains('~'))
            {
                var range = ParseColumnRange(autoVer.标准);
                double.TryParse(autoVer.实测, NumberStyles.Float, CultureInfo.InvariantCulture, out double 实测浮点值);
                if (实测浮点值 >= range.lower && 实测浮点值 <= range.upper)
                {
                    autoVer.状态 = "OK";
                }
                else
                {
                    autoVer.状态 = "NG";
                }
            }
            else
            {
                autoVer.状态 = "NG";
            }
        }

        private void OnUpdateItems()
        {
            try
            {
                if (SelectedReportPage.Name == "AutoVerication")
                {
                    ItemModels.Clear();

                    var pages = DigitalAssPageModel.Pages;
                    int order = 0;

                    foreach (var page in pages)
                    {
                        if (page.IsVisible && page.Name != "AutoVerication")
                        {
                            string displayName = GetLocalizedPageName(page.Name);

                            // 从服务读取每个页面的状态
                            string status = PageStatusService.Instance.GetStatus(page.Name);

                            var autoVerItem = new AssTbAutoVerication()
                            {
                                项序 = order,
                                项次 = displayName,
                                标准 = "",
                                实测 = "",
                                状态 = status,
                                完成时间 = DateTime.Now
                            };

                            ItemModels.Add(autoVerItem);
                            order++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _commonbus.OnLog(new LogInfo()
                {
                    LogType = LogType.Info,
                    LogMessage = $"更新列表失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 获取页面名称的本地化中文文本
        /// </summary>
        /// <param name="pageName">页面英文键值</param>
        /// <returns>本地化后的中文名称</returns>
        private string GetLocalizedPageName(string pageName)
        {
            try
            {
                var langType = typeof(Lang);
                var propertyInfo = langType.GetProperty(pageName);

                if (propertyInfo != null)
                {
                    var localizedValue = propertyInfo.GetValue(null) as string;
                    if (!string.IsNullOrEmpty(localizedValue))
                    {
                        return localizedValue;
                    }
                }

                return pageName;
            }
            catch
            {
                return pageName;
            }
        }

        /// <summary>
        /// 将中文显示名称映射回英文键值
        /// </summary>
        private string GetEnglishKeyFromDisplayName(string displayName)
        {
            var langType = typeof(Lang);
            var properties = langType.GetProperties();

            foreach (var prop in properties)
            {
                var value = prop.GetValue(null) as string;
                if (value == displayName)
                {
                    return prop.Name;
                }
            }
            return displayName;
        }

        /// <summary>
        /// 查询命令
        /// </summary>
        private void OnQuery()
        {
            try
            {
                // 刷新数据
                UpdateItemsFromCsv();

                // 刷新所有页面的状态
                foreach (var item in ItemModels)
                {
                    if (item is AssTbAutoVerication autoVer)
                    {
                        string pageName = GetEnglishKeyFromDisplayName(autoVer.项次);
                        string status = PageStatusService.Instance.GetStatus(pageName);
                        autoVer.状态 = status;
                        autoVer.完成时间 = DateTime.Now;
                    }
                }
                RaisePropertyChanged(nameof(ItemModels));
            }
            catch (Exception ex)
            {
                _commonbus.OnLog(new LogInfo() { LogType = LogType.Info, LogMessage = $"查询失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 分页更新命令
        /// </summary>
        private void OnPageUpdated(object obj)
        {
            try
            {
                // 处理分页逻辑
                UpdateItemsFromCsv();
            }
            catch (Exception ex)
            {
                _commonbus.OnLog(new LogInfo() { LogType = LogType.Info, LogMessage = $"分页更新失败: {ex.Message}" });
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

            // 2) 含 ~ ：必须是"下限~上限"且仅出现一次 ~
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