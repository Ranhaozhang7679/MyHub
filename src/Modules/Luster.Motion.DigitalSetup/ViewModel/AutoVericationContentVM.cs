using System;
using System.Collections.Generic;
using System.IO;
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
using Luster.Motion.DigitalSetup.Services;
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
                // 加载工站点检状态
                LoadStationCheckStatus();
            }
        }

        public AutoVericationContentVM(IRepository repository,
                                        IRegionManager regionManager,
                                        ICommonBus commonBus,
                                        IMotionController motionController,
                                        IDeviceEngine deviceEngine,
                                        FlowBus _flowBus,
                                        CSVHelper cSVHelper,IDialogService dialogService, CheckStatusService checkStatusService)
                                        : base(repository, regionManager, commonBus, cSVHelper, _flowBus, dialogService, checkStatusService)
        {
            _parentRegionName = "AutoVericationContent";

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
            // 加载工站点检状态
            LoadStationCheckStatus();

            // 订阅状态服务的更新事件，实时获取状态变化
            PageStatusService.Instance.StatusChanged += OnPageStatusChanged;

            InitializePageStatus();

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
                        page.ParentRegion = "AutoVericationContent";
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
            OnUpdateItems();
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
                ProgressValue = 0;

                // 读取最新 CSV 数据（包含实测值）
                var latestRows = LoadAllSubPagesLatestCsv();

                if (latestRows.Count == 0)
                {
                    ProgressValue = 100;
                    return;
                }

                // 清空并逐行刷新，每行间隔 10ms
                ItemModels.Clear();

                for (int i = 0; i < latestRows.Count; i++)
                {
                    var row = latestRows[i];

                    // 设置完成时间
                    row.完成时间 = DateTime.Now;

                    // 如果状态为空但有实测值，执行状态判断
                    if (string.IsNullOrEmpty(row.状态) && !string.IsNullOrEmpty(row.实测))
                    {
                        FillTableContent(row);
                    }

                    ItemModels.Add(row);
                    ProgressValue = (i + 1.0) * 100 / latestRows.Count;
                    RaisePropertyChanged(nameof(ItemModels));
                    await Task.Delay(10);
                }
            }
            catch (Exception ex)
            {
                _commonbus.OnLog(new LogInfo() { LogType = LogType.Info, LogMessage = $"一键点检刷新失败: {ex.Message}" });
                ProgressValue = 0;
            }
            finally
            {
                ProgressValue = 100;

                // 保存点检状态
                SaveCheckStatus(CheckStatus.CheckedOK, "自动验证页面数据读取完成");
                SyncOverallStatusToPageStatusService();
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
                ItemModels.Clear();

                var rows = LoadAllSubPagesLatestCsv();
                foreach (var row in rows)
                {
                    // 初次加载只显示项序、项次、标准，实测和状态留空
                    row.实测 = "";
                    row.状态 = "未点检";
                    ItemModels.Add(row);
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
        /// 获取子界面对应的 CSV 类别名列表
        /// </summary>
        private Dictionary<string, List<string>> GetSubPageCategoryMapping()
        {
            return new Dictionary<string, List<string>>
            {
                { "MainParameters", new List<string> { "SwVersion" } },
                { "Communications", new List<string> { "ConfigSoftwareCom", "ConfigSoftwareNet" } },
                { "IOConform", new List<string> { "DigitalInSingle", "DigitalOutSingle" } },
                { "Horizontal", new List<string> { "AutomaticPosAndLeveling" } },
                { "LoadCell", new List<string> { "CalibrationTable", "SuctionNozzle", "PressureRepetition" } },
                { "Embossing", new List<string> { "AutomaticEmbossing" } },
                { "DigitalVision", new List<string> { "AutoFocusing", "AutoFieldOfView", "AutoGrayScale" } },
                { "AutoVisualCalibration", new List<string> { "AutoVisualCalibration" } },
            };
        }

        /// <summary>
        /// 判断一级页面是否为多工站界面
        /// </summary>
        private bool IsMultiStationPage(string pageName)
        {
            var multiStationPages = new HashSet<string>
            {
                "Horizontal", "LoadCell", "Embossing", "DigitalVision",
                "PointTeaching", "AutoVisualCalibration"
            };
            return multiStationPages.Contains(pageName);
        }

        /// <summary>
        /// 从单个 CSV 文件读取数据行，转换为 AssTbAutoVerication 列表
        /// </summary>
        private List<AssTbAutoVerication> ReadCsvRows(string csvPath, string sourceLabel)
        {
            var result = new List<AssTbAutoVerication>();
            if (string.IsNullOrEmpty(csvPath) || !File.Exists(csvPath)) return result;

            try
            {
                var lines = File.ReadAllLines(csvPath, Encoding.Default);
                if (lines.Length < 2) return result;

                var headers = lines[0].Split(',');
                int xiangciIdx = Array.IndexOf(headers, "项次");
                int standardIdx = Array.IndexOf(headers, "标准");
                int measuredIdx = Array.IndexOf(headers, "实测");
                int statusIdx = Array.IndexOf(headers, "状态");

                for (int i = 1; i < lines.Length; i++)
                {
                    var cols = lines[i].Split(',');
                    if (cols.Length < 2) continue;

                    string xiangci = xiangciIdx >= 0 && xiangciIdx < cols.Length ? cols[xiangciIdx] : "";
                    if (string.IsNullOrEmpty(xiangci)) continue;

                    result.Add(new AssTbAutoVerication
                    {
                        项序 = 0,
                        项次 = $"{sourceLabel}-{xiangci}",
                        标准 = standardIdx >= 0 && standardIdx < cols.Length ? cols[standardIdx] : "",
                        实测 = measuredIdx >= 0 && measuredIdx < cols.Length ? cols[measuredIdx] : "",
                        状态 = statusIdx >= 0 && statusIdx < cols.Length ? cols[statusIdx] : "未完成",
                        完成时间 = DateTime.Now
                    });
                }
            }
            catch (Exception ex)
            {
                _commonbus.OnLog(new LogInfo() { LogType = LogType.Info, LogMessage = $"读取CSV失败[{csvPath}]: {ex.Message}" });
            }
            return result;
        }

        /// <summary>
        /// 加载所有子界面的 Latest CSV，展平合并到一张列表
        /// </summary>
        private List<AssTbAutoVerication> LoadAllSubPagesLatestCsv()
        {
            var allRows = new List<AssTbAutoVerication>();
            var recipeDir = _commonbus.CurrentRecipe?.GetRecipePath();
            if (string.IsNullOrEmpty(recipeDir)) return allRows;

            var assDir = Path.Combine(recipeDir, "db", "Ass_Data");
            var categoryMapping = GetSubPageCategoryMapping();
            var pages = DigitalAssPageModel.Pages;

            foreach (var page in pages)
            {
                if (!page.IsVisible || page.Name == "AutoVerication") continue;

                string displayName = GetLocalizedPageName(page.Name);

                // 特殊处理：PointTeaching 按轴维度读取
                if (page.Name == "PointTeaching")
                {
                    var axisNames = LoadAxisNamesFromGodLineJson(assDir);
                    foreach (var axisName in axisNames)
                    {
                        var csvPath = Path.Combine(assDir, $"AssTbOriginLimit_{axisName}_Latest.csv");
                        var axisDisplayName = GetLocalizedPageName(axisName);
                        var rows = ReadCsvRows(csvPath, $"{displayName}-{axisDisplayName}");
                        allRows.AddRange(rows);
                    }
                    continue;
                }

                // 获取该页面的类别列表
                if (!categoryMapping.TryGetValue(page.Name, out var categories) || categories.Count == 0)
                    continue;

                foreach (var category in categories)
                {
                    if (IsMultiStationPage(page.Name))
                    {
                        var stationConfigs = LoadStationConfigsFromFile(page.Name, recipeDir);
                        if (stationConfigs != null && stationConfigs.Count > 0)
                        {
                            foreach (var station in stationConfigs)
                            {
                                var csvPath = Path.Combine(assDir, $"AssTb{category}_{station}_Latest.csv");
                                var rows = ReadCsvRows(csvPath, $"{displayName}-{station}");
                                allRows.AddRange(rows);
                            }
                        }
                        else
                        {
                            var csvPath = Path.Combine(assDir, $"AssTb{category}_Latest.csv");
                            var rows = ReadCsvRows(csvPath, displayName);
                            allRows.AddRange(rows);
                        }
                    }
                    else
                    {
                        var csvPath = Path.Combine(assDir, $"AssTb{category}_Latest.csv");
                        var rows = ReadCsvRows(csvPath, displayName);
                        allRows.AddRange(rows);
                    }
                }
            }

            // 统一编排项序
            for (int i = 0; i < allRows.Count; i++)
            {
                allRows[i].项序 = i;
            }

            return allRows;
        }

        /// <summary>
        /// 从 StationConfig.json 文件中读取指定页面的工站名称列表
        /// </summary>
        private List<string> LoadStationConfigsFromFile(string pageName, string recipeDir)
        {
            var stationNames = new List<string>();
            try
            {
                var configFile = Path.Combine(recipeDir, "db", "Ass_Data", "StationConfig.json");
                if (!File.Exists(configFile)) return stationNames;

                var json = File.ReadAllText(configFile);
                var allConfigs = Newtonsoft.Json.Linq.JObject.Parse(json);

                var configKey = $"{pageName}Config";
                var configObj = allConfigs[configKey];
                if (configObj == null)
                {
                    // 遍历所有配置项查找 StationConfigs 数组
                    foreach (var child in allConfigs.Children())
                    {
                        var obj = child as Newtonsoft.Json.Linq.JProperty;
                        if (obj != null)
                        {
                            var inner = obj.Value as Newtonsoft.Json.Linq.JObject;
                            var stations = inner?.Property("StationConfigs");
                            if (stations != null)
                            {
                                var arr = stations.Value as Newtonsoft.Json.Linq.JArray;
                                if (arr != null)
                                {
                                    foreach (var item in arr)
                                    {
                                        var name = item["Name"]?.ToString();
                                        if (!string.IsNullOrEmpty(name))
                                            stationNames.Add(name);
                                    }
                                    if (stationNames.Count > 0) return stationNames;
                                    stationNames.Clear();
                                }
                            }
                        }
                    }
                    return stationNames;
                }

                var stationArray = configObj["StationConfigs"] as Newtonsoft.Json.Linq.JArray;
                if (stationArray != null)
                {
                    foreach (var item in stationArray)
                    {
                        var name = item["Name"]?.ToString();
                        if (!string.IsNullOrEmpty(name))
                            stationNames.Add(name);
                    }
                }
            }
            catch (Exception ex)
            {
                _commonbus.OnLog(new LogInfo() { LogType = LogType.Info, LogMessage = $"读取工站配置失败[{pageName}]: {ex.Message}" });
            }
            return stationNames;
        }

        /// <summary>
        /// 从 AxisPositions.json 读取所有轴名称，用于不依赖 PointTeachingContentVM 加载即可获取轴列表
        /// </summary>
        private List<string> LoadAxisNamesFromGodLineJson(string assDir)
        {
            var axisNames = new List<string>();
            try
            {
                var godLineDir = Path.Combine(assDir, "Ass_GodLine");
                var jsonPath = Path.Combine(godLineDir, "AxisPositions.json");
                if (!File.Exists(jsonPath))
                {
                    // 兼容带前导空格的文件名
                    jsonPath = Path.Combine(godLineDir, " AxisPositions.json");
                    if (!File.Exists(jsonPath)) return axisNames;
                }

                var json = File.ReadAllText(jsonPath);
                var jObj = Newtonsoft.Json.Linq.JObject.Parse(json);
                foreach (var prop in jObj.Properties())
                {
                    if (!string.IsNullOrEmpty(prop.Name))
                        axisNames.Add(prop.Name);
                }
            }
            catch (Exception ex)
            {
                _commonbus.OnLog(new LogInfo() { LogType = LogType.Info, LogMessage = $"读取轴配置失败: {ex.Message}" });
            }
            return axisNames;
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
                ItemModels.Clear();

                var latestRows = LoadAllSubPagesLatestCsv();
                foreach (var row in latestRows)
                {
                    // 查询时显示完整数据（包含实测）
                    if (string.IsNullOrEmpty(row.状态) && !string.IsNullOrEmpty(row.实测))
                    {
                        FillTableContent(row);
                    }
                    ItemModels.Add(row);
                }
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