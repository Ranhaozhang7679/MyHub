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
    /// AutoVerication — 展平显示所有子界面的点检数据
    /// 每个子界面对应一个 CommonPageModel，切换页面显示该界面的 CSV 数据
    /// </summary>
    public class AutoVericationContentVM : BaseAss
    {
        private double _progressValue;
        private string _paramConfirmStatus = "未点检";

        public ICommand EndCommand { get; private set; }
        public ICommand OneKeyCheckCommand { get; private set; }
        public ICommand UpdateItemsCommand { get; private set; }
        public ICommand QueryCommand { get; private set; }
        public ICommand PageUpdatedCommand { get; private set; }

        private FlowBus flowBus;
        private IDeviceEngine _deviceEngine = null;
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
                SetProperty(ref _seletedReportPage, value);
                base.SelectedReportPage = value;

                ConfigKey = $"{value?.Name}Config";
                LoadStationConfigFromJson();
                UpdateStationConfigs();
                LoadStationCheckStatus();

                // 切换页面时自动加载数据（不保存旧页面数据）
                if (value != null)
                    LoadCurrentPageData();
            }
        }

        public AutoVericationContentVM(IRepository repository,
                                        IRegionManager regionManager,
                                        ICommonBus commonBus,
                                        IMotionController motionController,
                                        IDeviceEngine deviceEngine,
                                        FlowBus _flowBus,
                                        CSVHelper cSVHelper, IDialogService dialogService, CheckStatusService checkStatusService)
                                        : base(repository, regionManager, commonBus, cSVHelper, _flowBus, dialogService, checkStatusService)
        {
            _parentRegionName = "AutoVericationContent";

            flowBus = _flowBus;
            _deviceEngine = deviceEngine;
            _mController = motionController;

            // 每个子界面对应一个 CommonPageModel
            Pages = new ObservableCollection<CommonPageModel>();
            Pages.Add(new CommonPageModel() { Name = "Communications", IsSelected = true, Region = "", ViewType = typeof(AssTbAutoVerication) });
            Pages.Add(new CommonPageModel() { Name = "IOConform", IsSelected = false, Region = "", ViewType = typeof(AssTbAutoVerication) });
            Pages.Add(new CommonPageModel() { Name = "Horizontal", IsSelected = false, Region = "", ViewType = typeof(AssTbAutoVerication) });
            Pages.Add(new CommonPageModel() { Name = "LoadCell", IsSelected = false, Region = "", ViewType = typeof(AssTbAutoVerication) });
            Pages.Add(new CommonPageModel() { Name = "Embossing", IsSelected = false, Region = "", ViewType = typeof(AssTbAutoVerication) });
            Pages.Add(new CommonPageModel() { Name = "DigitalVision", IsSelected = false, Region = "", ViewType = typeof(AssTbAutoVerication) });
            Pages.Add(new CommonPageModel() { Name = "PointTeaching", IsSelected = false, Region = "", ViewType = typeof(AssTbAutoVerication) });
            Pages.Add(new CommonPageModel() { Name = "AutoVisualCalibration", IsSelected = false, Region = "", ViewType = typeof(AssTbAutoVerication) });

            SelectedReportPage = Pages.Where(x => x.IsSelected).FirstOrDefault();
            InitModels();

            EndCommand = new DelegateCommand(OnEnd);
            OneKeyCheckCommand = new DelegateCommand<object>(OnOneKeyCheck);
            UpdateItemsCommand = new DelegateCommand(OnUpdateItems);
            QueryCommand = new DelegateCommand(OnQuery);
            PageUpdatedCommand = new DelegateCommand<object>(OnPageUpdated);

            ConfigKey = "AutoVericationConfig";
            LoadStationConfigFromJson();
            UpdateStationConfigs();
            LoadStationCheckStatus();

            PageStatusService.Instance.StatusChanged += OnPageStatusChanged;
            InitializePageStatus();

            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                LoadCheckStatusForAllPages();
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void LoadCheckStatusForAllPages()
        {
            if (_checkStatusService == null || Pages == null) return;
            try
            {
                foreach (var page in Pages)
                {
                    if (page != null)
                    {
                        page.ParentRegion = "AutoVericationContent";
                        var record = _checkStatusService.GetRecord(page.PageKey);
                        page.CheckStatus = record != null ? record.Status : CheckStatus.NotChecked;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载点检状态失败: {ex.Message}");
            }
        }

        protected override void RefreshCheckStatus()
        {
            LoadCheckStatusForAllPages();
        }

        private void OnPageStatusChanged(string pageName, string status)
        {
            // 更新侧边栏对应 CommonPageModel 的状态
            var page = Pages.FirstOrDefault(p => p.Name == pageName);
            if (page != null)
            {
                page.CheckStatus = status switch
                {
                    "OK" => CheckStatus.CheckedOK,
                    "NG" => CheckStatus.CheckedFail,
                    _ => CheckStatus.NotChecked
                };
            }
        }

        private void InitializePageStatus()
        {
            OnUpdateItems();
        }

        public override void OnEnd()
        {
            ProgressValue = 0;
            base.OnEnd();
        }

        public override async void OnOneKeyCheck(object obj)
        {
            base.OnOneKeyCheck(obj);
            try
            {
                ProgressValue = 0;
                var latestRows = LoadCurrentPageLatestCsv();
                if (latestRows.Count == 0)
                {
                    ProgressValue = 100;
                    return;
                }

                ItemModels.Clear();
                for (int i = 0; i < latestRows.Count; i++)
                {
                    var row = latestRows[i];
                    row.完成时间 = DateTime.Now;
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

                // 保存当前页面数据和状态
                SaveCurrentPageData();
                SaveCurrentPageCheckStatus();
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
                    autoVer.状态 = "OK";
                else
                    autoVer.状态 = "NG";
            }
            else
            {
                autoVer.状态 = "NG";
            }
        }

        /// <summary>
        /// 加载当前选中页面的 CSV 数据（初次加载只显示项序/项次/标准）
        /// </summary>
        private void OnUpdateItems()
        {
            try
            {
                ItemModels.Clear();
                var rows = LoadCurrentPageLatestCsv();
                foreach (var row in rows)
                {
                    row.实测 = "";
                    row.状态 = "未点检";
                    ItemModels.Add(row);
                }
            }
            catch (Exception ex)
            {
                _commonbus.OnLog(new LogInfo() { LogType = LogType.Info, LogMessage = $"更新列表失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 查询命令 — 显示完整数据
        /// </summary>
        private void OnQuery()
        {
            try
            {
                ItemModels.Clear();
                var latestRows = LoadCurrentPageLatestCsv();
                foreach (var row in latestRows)
                {
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
        /// 分页更新 — 切换 CommonPageModel 时重新加载
        /// </summary>
        private void OnPageUpdated(object obj)
        {
            try
            {
                LoadCurrentPageData();
            }
            catch (Exception ex)
            {
                _commonbus.OnLog(new LogInfo() { LogType = LogType.Info, LogMessage = $"分页更新失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 加载当前页面数据：优先读取本地持久化CSV，没有则从源CSV加载
        /// </summary>
        private void LoadCurrentPageData()
        {
            if (SelectedReportPage == null) return;
            try
            {
                ItemModels.Clear();

                // 优先加载本地持久化CSV（有上一次点检的完整数据）
                var persisted = ReadLocalPersistedCsv();
                if (persisted.Count > 0)
                {
                    foreach (var row in persisted)
                        ItemModels.Add(row);
                    return;
                }

                // 没有持久化数据则从源CSV加载（只显示项序/项次/标准）
                var rows = LoadCurrentPageLatestCsv();
                foreach (var row in rows)
                {
                    row.实测 = "";
                    row.状态 = "未点检";
                    ItemModels.Add(row);
                }
            }
            catch (Exception ex)
            {
                _commonbus.OnLog(new LogInfo() { LogType = LogType.Info, LogMessage = $"加载页面数据失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 保存当前页面的 ItemModels 到本地持久化 CSV
        /// </summary>
        private void SaveCurrentPageData()
        {
            try
            {
                if (SelectedReportPage == null || ItemModels == null || ItemModels.Count == 0) return;
                // 只有执行过一键点检（有实测值）才持久化
                if (!ItemModels.OfType<AssTbAutoVerication>().Any(r => !string.IsNullOrEmpty(r.实测))) return;
                var recipeDir = _commonbus.CurrentRecipe?.GetRecipePath();
                if (string.IsNullOrEmpty(recipeDir)) return;

                var csvPath = Path.Combine(recipeDir, "db", "Ass_Data", $"AutoVerication_{SelectedReportPage.Name}_Latest.csv");
                var type = typeof(AssTbAutoVerication);
                var props = type.GetProperties()
                    .Where(p => p.CanRead && p.PropertyType.IsSerializable && p.GetIndexParameters().Length == 0)
                    .ToArray();
                var headers = props.Select(p => p.Name).ToArray();

                using (var writer = new StreamWriter(csvPath, false, Encoding.UTF8))
                {
                    writer.WriteLine(string.Join(",", headers));
                    foreach (var item in ItemModels)
                    {
                        var values = props.Select(p => p.GetValue(item, null)?.ToString() ?? "");
                        writer.WriteLine(string.Join(",", values));
                    }
                }
            }
            catch (Exception ex)
            {
                _commonbus.OnLog(new LogInfo() { LogType = LogType.Info, LogMessage = $"保存页面数据失败: {ex.Message}" });
            }
        }

        /// <summary>
        /// 读取本地持久化CSV
        /// </summary>
        private List<AssTbAutoVerication> ReadLocalPersistedCsv()
        {
            var result = new List<AssTbAutoVerication>();
            try
            {
                if (SelectedReportPage == null) return result;
                var recipeDir = _commonbus.CurrentRecipe?.GetRecipePath();
                if (string.IsNullOrEmpty(recipeDir)) return result;

                var csvPath = Path.Combine(recipeDir, "db", "Ass_Data", $"AutoVerication_{SelectedReportPage.Name}_Latest.csv");
                if (!File.Exists(csvPath)) return result;

                var rows = ReadCsvRows(csvPath, "");
                if (rows.Count == 0) return result;

                // 验证数据有效性（至少有一行有实测值）
                bool hasValidData = rows.Any(r => !string.IsNullOrEmpty(r.实测));
                return hasValidData ? rows : result;
            }
            catch { return result; }
        }

        /// <summary>
        /// 保存当前页面的点检状态到持久化服务
        /// </summary>
        private void SaveCurrentPageCheckStatus()
        {
            try
            {
                if (SelectedReportPage == null) return;

                // 根据表格数据计算状态
                bool allOK = ItemModels.OfType<AssTbAutoVerication>().All(r => r.状态 == "OK");
                bool hasNG = ItemModels.OfType<AssTbAutoVerication>().Any(r => r.状态 == "NG");
                bool hasData = ItemModels.Count > 0;

                var status = !hasData ? CheckStatus.NotChecked
                    : hasNG ? CheckStatus.CheckedFail
                    : allOK ? CheckStatus.CheckedOK
                    : CheckStatus.NotChecked;

                // 更新 CommonPageModel
                SelectedReportPage.CheckStatus = status;
                SelectedReportPage.ParentRegion = _parentRegionName;

                // 持久化到服务
                _checkStatusService?.UpdateStatus(
                    SelectedReportPage.PageKey,
                    status,
                    _parentRegionName,
                    SelectedReportPage.Name,
                    _commonbus?.CurrentUser?.UserName ?? "Unknown",
                    $"一键点检完成"
                );
            }
            catch (Exception ex)
            {
                _commonbus.OnLog(new LogInfo() { LogType = LogType.Info, LogMessage = $"保存页面状态失败: {ex.Message}" });
            }
        }

        #region CSV 读取逻辑

        /// <summary>
        /// 子界面名 → CSV 类别名映射
        /// </summary>
        private static readonly Dictionary<string, List<string>> CategoryMapping = new Dictionary<string, List<string>>
        {
            { "Communications", new List<string> { "ConfigSoftwareCom", "ConfigSoftwareNet" } },
            { "IOConform", new List<string> { "DigitalInSingle", "DigitalOutSingle" } },
            { "Horizontal", new List<string> { "AutomaticPosAndLeveling" } },
            { "LoadCell", new List<string> { "CalibrationTable", "SuctionNozzle", "PressureRepetition" } },
            { "Embossing", new List<string> { "AutomaticEmbossing" } },
            { "DigitalVision", new List<string> { "AutoFocusing", "AutoFieldOfView", "AutoGrayScale" } },
            { "AutoVisualCalibration", new List<string> { "AutoVisualCalibration" } },
        };

        private static readonly HashSet<string> MultiStationPages = new HashSet<string>
        {
            "Horizontal", "LoadCell", "Embossing", "DigitalVision",
            "PointTeaching", "AutoVisualCalibration"
        };

        /// <summary>
        /// 加载当前选中页面的 Latest CSV
        /// </summary>
        private List<AssTbAutoVerication> LoadCurrentPageLatestCsv()
        {
            var allRows = new List<AssTbAutoVerication>();
            if (SelectedReportPage == null) return allRows;

            var recipeDir = _commonbus.CurrentRecipe?.GetRecipePath();
            if (string.IsNullOrEmpty(recipeDir)) return allRows;

            var assDir = Path.Combine(recipeDir, "db", "Ass_Data");
            string pageName = SelectedReportPage.Name;

            // PointTeaching 特殊处理：按轴读取
            if (pageName == "PointTeaching")
            {
                var axisNames = LoadAxisNamesFromGodLineJson(assDir);
                foreach (var axisName in axisNames)
                {
                    var csvPath = Path.Combine(assDir, $"AssTbOriginLimit_{axisName}_Latest.csv");
                    var rows = ReadCsvRows(csvPath, axisName);
                    allRows.AddRange(rows);
                }
            }
            else if (CategoryMapping.TryGetValue(pageName, out var categories))
            {
                foreach (var category in categories)
                {
                    if (MultiStationPages.Contains(pageName))
                    {
                        var stations = LoadStationConfigsFromFile(pageName, recipeDir);
                        if (stations != null && stations.Count > 0)
                        {
                            foreach (var station in stations)
                            {
                                var csvPath = Path.Combine(assDir, $"AssTb{category}_{station}_Latest.csv");
                                var rows = ReadCsvRows(csvPath, station);
                                allRows.AddRange(rows);
                            }
                        }
                        else
                        {
                            var csvPath = Path.Combine(assDir, $"AssTb{category}_Latest.csv");
                            var rows = ReadCsvRows(csvPath, "");
                            allRows.AddRange(rows);
                        }
                    }
                    else
                    {
                        var csvPath = Path.Combine(assDir, $"AssTb{category}_Latest.csv");
                        var rows = ReadCsvRows(csvPath, "");
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
        /// 从单个 CSV 文件读取数据行
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

                    // 如果有 sourceLabel（工站名/轴名），加前缀区分来源
                    string displayName = string.IsNullOrEmpty(sourceLabel) ? xiangci : $"{sourceLabel}-{xiangci}";

                    result.Add(new AssTbAutoVerication
                    {
                        项序 = 0,
                        项次 = displayName,
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
        /// 从 StationConfig.json 读取指定页面的工站名称列表
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
        /// 从 AxisPositions.json 读取所有轴名称
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

        #endregion

        #region 本地化

        private string GetLocalizedPageName(string pageName)
        {
            try
            {
                var propertyInfo = typeof(Lang).GetProperty(pageName);
                if (propertyInfo != null)
                {
                    var localizedValue = propertyInfo.GetValue(null) as string;
                    if (!string.IsNullOrEmpty(localizedValue))
                        return localizedValue;
                }
                return pageName;
            }
            catch { return pageName; }
        }

        private string GetEnglishKeyFromDisplayName(string displayName)
        {
            foreach (var prop in typeof(Lang).GetProperties())
            {
                if (prop.GetValue(null) as string == displayName)
                    return prop.Name;
            }
            return displayName;
        }

        #endregion

        private static (double lower, double upper) ParseColumnRange(string standardValue)
        {
            if (!standardValue.Contains('~'))
            {
                if (!double.TryParse(standardValue, NumberStyles.Float, CultureInfo.InvariantCulture, out double v))
                    throw new FormatException("第一列单值格式非法");
                return (v, v);
            }

            string[] tokens = standardValue.Split('~');
            if (tokens.Length != 2)
                throw new FormatException("第一列区间只能包含一个 '~'");

            if (!double.TryParse(tokens[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double lower) ||
                !double.TryParse(tokens[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double upper))
                throw new FormatException("第一列区间上下限格式非法");

            if (lower > upper)
                throw new FormatException("第一列区间下限不能大于上限");

            return (lower, upper);
        }
    }
}
