using Luster.Common.Assets;
using Luster.Common.Tools;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.DataStruct.VDevice;
using Luster.SimDevice.Engine;
using Luster.SimDevice.EngineUI;
using Luster.SimDevice.EngineUI.Models;
using Luster.SimDevice.SubSystem.Extension;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;


namespace Luster.SimDevice.SubSystem.ViewModel
{
    public class ErrorContentCustomVM : PageVM
    {
        public override bool IsShowAdd => false;

        private IDialogService _dialogService;
        private readonly IDeviceEngine deviceEngine;

        /// <summary>
        /// 当前 VM 绑定的配方路径，用于防止跨配方写入
        /// </summary>
        private string _boundRecipePath;

        public override bool IsShowRemove => false;

        private ObservableCollection<ErrorItemCustomModel> errorList;
        public ObservableCollection<ErrorItemCustomModel> ErrorList
        {
            get { return errorList; }
            set { SetProperty(ref errorList, value); }
        }

        /// <summary>
        /// 搜索关键字
        /// </summary>
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    // 只在清空时自动刷新，正常输入字符不触发以免卡顿
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        RaisePropertyChanged(nameof(FilteredErrorList));
                    }
                }
            }
        }

        /// <summary>
        /// 根据搜索文本过滤后的列表
        /// </summary>
        public ObservableCollection<ErrorItemCustomModel> FilteredErrorList
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SearchText))
                    return ErrorList;

                var filtered = ErrorList.Where(item =>
                    (item.AlarmCode?.Contains(SearchText) == true) ||
                    (item.AlarmContent?.Contains(SearchText) == true) ||
                    (item.AlarmEnglish?.Contains(SearchText) == true) ||
                    (item.ModuleName?.Contains(SearchText) == true)
                ).ToList();
                return new ObservableCollection<ErrorItemCustomModel>(filtered);
            }
        }

        public DelegateCommand SearchCommand { get; set; }
        public DelegateCommand BatchExportCommand { get; set; }
        public DelegateCommand BatchImportCommand { get; set; }
        public DelegateCommand GenerateErrorCodeListCommand { get; set; }
        public DelegateCommand ScanRecipeCommand { get; set; }
        public DelegateCommand<ErrorItemCustomModel> EditItemCommand { get; set; }

        bool isDeleteFinish = true;

        protected ErrorContentCustomVM(ISimDeviceEngineUI _engine, IDialogService dialogService, IDeviceEngine deviceEngine) : base(_engine)
        {
            _dialogService = dialogService;
            this.deviceEngine = deviceEngine;
            ErrorList = new ObservableCollection<ErrorItemCustomModel>();

            SearchCommand = new DelegateCommand(() => RaisePropertyChanged(nameof(FilteredErrorList)));
            BatchExportCommand = new DelegateCommand(ExportTotalCommand);
            BatchImportCommand = new DelegateCommand(ImportTotalCommand);
            GenerateErrorCodeListCommand = new DelegateCommand(GenerateErrorCodeList);
            ScanRecipeCommand = new DelegateCommand(ScanRecipe);
            EditItemCommand = new DelegateCommand<ErrorItemCustomModel>(EditItem);

            ErrorList.CollectionChanged += (s, e) => RaisePropertyChanged(nameof(FilteredErrorList));

            _boundRecipePath = deviceEngine.RecipeConfigPath;
            LoadFromCsvFile();

            // 使用命名方法，便于取消订阅，防止跨配方写入
            deviceEngine.SaveEvent += OnSaveEvent;

            // 监听引擎初始化完成事件（配方切换时会触发）
            deviceEngine.InitializedEvent += OnEngineInitialized;
        }

        /// <summary>
        /// 保存事件处理：只保存当前配方的数据
        /// </summary>
        private void OnSaveEvent()
        {
            // 检查配方路径是否仍然匹配，防止旧VM写入新配方
            if (_boundRecipePath == deviceEngine.RecipeConfigPath)
            {
                SaveToCsvFile(ErrorList);
                // 引擎保存后，后处理 .recipe XML：更新 Alarm 参数并移除可能的 Global 引用
                UpdateRecipeXmlForAlarms();
            }
        }

        /// <summary>
        /// 引擎初始化完成（配方切换）时，重新加载数据
        /// </summary>
        private void OnEngineInitialized(IDeviceEngine engine, string deviceTask)
        {
            string newPath = deviceEngine.RecipeConfigPath;
            if (newPath != _boundRecipePath)
            {
                _boundRecipePath = newPath;
                // 需要在 UI 线程执行，因为会修改 ObservableCollection
                System.Windows.Application.Current?.Dispatcher?.Invoke(() =>
                {
                    LoadFromCsvFile();
                });
            }
        }

        private void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is ErrorItemCustomModel item && deviceEngine != null)
            {
                string searchKey = (e.PropertyName == nameof(item.AlarmCode)) ? item.OldAlarmCode : item.AlarmCode;
                var allAlarms = deviceEngine.GetVDevices<VAlarm>()?.ToList() ?? new List<VAlarm>();

                // 按 ModuleId (DeviceID) 查找 VAlarm，有 ModuleId 时严格匹配，不回退到 AlarmKey
                // 排除 ProEvent VAlarm，避免跨页面交叉污染
                VAlarm alarm = null;
                if (!string.IsNullOrEmpty(item.ModuleId) && Guid.TryParse(item.ModuleId, out var moduleIdGuid) && moduleIdGuid != Guid.Empty)
                {
                    alarm = allAlarms.FirstOrDefault(a => a.DeviceID == moduleIdGuid && a.Name != "ProEvent");
                }
                else
                {
                    alarm = allAlarms.FirstOrDefault(a => a.AlarmKey == searchKey && a.Name != "ProEvent");
                }

                if (alarm != null)
                {
                    if (e.PropertyName == nameof(item.AlarmContent))
                        alarm.AlarmCN = item.AlarmContent;
                    else if (e.PropertyName == nameof(item.AlarmEnglish))
                        alarm.AlarmEn = item.AlarmEnglish;
                    else if (e.PropertyName == nameof(item.AlarmCode))
                    {
                        alarm.AlarmKey = item.AlarmCode;
                        deviceEngine.RaiseAlarmCodeChangedEvent(searchKey, item.AlarmCode);
                    }

                    deviceEngine.RaiseVDeviceChangedEvent();
                }
            }
        }

        /// <summary>
        /// 扫描配方：从工作流 XML 中提取报警工具，忽略标记为 Skip 的模块。
        /// 使用 ModuleId (GUID) 作为唯一标识，支持同名模块、模块重命名和新模块添加。
        /// </summary>
        private void ScanRecipe()
        {
            _dialogService.ShowConfirm("扫描配方前，请确认当前配方已保存至最新？", r =>
            {
                if (r.Result != ButtonResult.OK)
                    return;

                DoScanRecipe();
            });
        }

        private void DoScanRecipe()
        {
            try
            {
                // 先保存配方，确保 .recipe 文件与内存中的最新状态同步
                // 用户在工作流编辑器中的修改（重命名模块、新增报警工具等）
                // 只有保存后才会写入 .recipe 文件
                try { deviceEngine.Save(); }
                catch { /* 新配方可能尚未完全初始化，忽略保存失败 */ }

                // 尝试从配方路径找到工作流 XML 文件
                string recipePath = deviceEngine.RecipeConfigPath;
                if (string.IsNullOrEmpty(recipePath) || !Directory.Exists(recipePath))
                {
                    SimEngineUI.OnLog(Common.DataStruct.Enums.LogType.Warning, "配方路径不存在，无法扫描");
                    return;
                }

                // 查找工作流文件：RecipeDataPath 对应的 XML 文件
                string recipeDataPath = deviceEngine.RecipeDataPath;
                string workflowFile = FindWorkflowFile(recipePath, recipeDataPath);
                if (string.IsNullOrEmpty(workflowFile) || !File.Exists(workflowFile))
                {
                    SimEngineUI.OnLog(Common.DataStruct.Enums.LogType.Warning, $"未找到工作流文件: {workflowFile}");
                    return;
                }

                var xRoot = XElement.Load(workflowFile);
                var scannedAlarms = new List<(string moduleId, string moduleName, string alarmCode, string message, string detail)>();

                // 递归扫描 Module 节点
                ScanModules(xRoot, scannedAlarms);

                if (scannedAlarms.Count == 0)
                {
                    SimEngineUI.OnLog(Common.DataStruct.Enums.LogType.Info, "未在工作流中找到报警工具");
                    return;
                }

                // 扫描结果集合：ModuleId 作为唯一标识
                var scanIds = new HashSet<string>(
                    scannedAlarms.Select(a => a.moduleId).Where(id => !string.IsNullOrEmpty(id)));

                // 保存被移除条目的用户编辑字段（用于模块重命名后恢复 AlarmCategory、RepairAction）
                var preservedEdits = new Dictionary<string, (string category, string repair)>();
                var preservedEditsByCode = new Dictionary<string, (string category, string repair)>();

                // 移除不再匹配的旧条目：ModuleId 不再存在于扫描结果中，或者根本没有 ModuleId（历史遗留数据/重复项）
                var toRemove = ErrorList.Where(e =>
                    (!string.IsNullOrEmpty(e.ModuleId) && !scanIds.Contains(e.ModuleId)) ||
                    string.IsNullOrEmpty(e.ModuleId)).ToList();

                foreach (var item in toRemove)
                {
                    if (!string.IsNullOrEmpty(item.ModuleId))
                    {
                        preservedEdits[item.ModuleId] = (item.AlarmCategory ?? "", item.RepairAction ?? "");
                    }
                    if (!string.IsNullOrEmpty(item.AlarmCode))
                    {
                        preservedEditsByCode[item.AlarmCode] = (item.AlarmCategory ?? "", item.RepairAction ?? "");
                    }
                    item.PropertyChanged -= Item_PropertyChanged;
                    ErrorList.Remove(item);
                }

                // 当前 ErrorList 中的 ModuleId 集合
                var currentIds = new HashSet<string>(
                    ErrorList.Select(e => e.ModuleId).Where(id => !string.IsNullOrEmpty(id)));

                int addedCount = 0;
                int updatedCount = toRemove.Count; // 重命名或升级条目算更新

                foreach (var (moduleId, moduleName, alarmCode, message, detail) in scannedAlarms)
                {
                    if (string.IsNullOrEmpty(alarmCode)) continue;

                    if (!string.IsNullOrEmpty(moduleId) && currentIds.Contains(moduleId))
                    {
                        // 已存在相同 ModuleId，更新 moduleName/alarmCode/message/detail
                        var existing = ErrorList.FirstOrDefault(e => e.ModuleId == moduleId);
                        if (existing != null)
                        {
                            existing.ModuleName = moduleName ?? existing.ModuleName;
                            existing.AlarmCode = alarmCode;
                            existing.AlarmContent = message ?? existing.AlarmContent;
                            existing.AlarmEnglish = detail ?? existing.AlarmEnglish;
                            updatedCount++;
                        }
                    }
                    else
                    {
                        // 新条目（可能是重命名后的模块，也可能是全新添加的报警工具，或是从旧版无ID数据升级而来）
                        string category = "";
                        string repair = "";
                        if (!string.IsNullOrEmpty(moduleId) && preservedEdits.TryGetValue(moduleId, out var edits))
                        {
                            category = edits.category;
                            repair = edits.repair;
                        }
                        else if (!string.IsNullOrEmpty(alarmCode) && preservedEditsByCode.TryGetValue(alarmCode, out var codeEdits))
                        {
                            category = codeEdits.category;
                            repair = codeEdits.repair;
                        }

                        var newModel = new ErrorItemCustomModel(
                            alarmCode, message ?? "", detail ?? "", category, repair, moduleName, moduleId);
                        newModel.PropertyChanged += Item_PropertyChanged;
                        ErrorList.Add(newModel);
                        if (!string.IsNullOrEmpty(moduleId))
                            currentIds.Add(moduleId);
                        addedCount++;

                        // 同步到设备引擎
                        if (deviceEngine != null)
                        {
                            // 按 ModuleId 查找是否已有对应的 VAlarm（排除 ProEvent VAlarm）
                            // 仅按 DeviceID 匹配，不按 AlarmKey 回退，确保每个模块都有独立的 VAlarm
                            var moduleIdGuid = Guid.TryParse(moduleId, out var midGuid) ? midGuid : Guid.Empty;
                            var existingAlarm = deviceEngine.GetVDevices<VAlarm>()
                                ?.FirstOrDefault(a => a.DeviceID == moduleIdGuid && moduleIdGuid != Guid.Empty && a.Name != "ProEvent");
                            if (existingAlarm == null)
                            {
                                deviceEngine.AddVirtual(new VAlarm()
                                {
                                    AlarmKey = alarmCode,
                                    AlarmCN = message ?? "",
                                    AlarmEn = detail ?? "",
                                    ID = Guid.NewGuid(),
                                    DeviceID = moduleIdGuid
                                });
                            }
                        }
                    }
                }

                RaisePropertyChanged(nameof(FilteredErrorList));
                SimEngineUI.OnLog(Common.DataStruct.Enums.LogType.Info,
                    $"扫描配方完成：新增 {addedCount} 条，更新 {updatedCount} 条");
            }
            catch (Exception ex)
            {
                SimEngineUI.OnLog(Common.DataStruct.Enums.LogType.Error, $"扫描配方失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 查找工作流 XML 文件（.recipe 文件）
        /// RecipeDataPath 指向 .data 文件（参数配置），同名的 .recipe 文件才是工作流 XML
        /// </summary>
        private string FindWorkflowFile(string recipeConfigPath, string recipeDataPath)
        {
            // 优先从 RecipeDataPath（.data）推导出 .recipe 文件路径
            if (!string.IsNullOrEmpty(recipeDataPath))
            {
                string recipeFile = Path.ChangeExtension(recipeDataPath, ".recipe");
                if (File.Exists(recipeFile))
                    return recipeFile;
            }

            // 备用：在 .data 文件同级目录查找同名 .recipe 文件
            if (!string.IsNullOrEmpty(recipeDataPath))
            {
                string dataDir = Path.GetDirectoryName(recipeDataPath);
                if (!string.IsNullOrEmpty(dataDir) && Directory.Exists(dataDir))
                {
                    foreach (var file in Directory.GetFiles(dataDir, "*.recipe"))
                    {
                        return file;
                    }
                }
            }

            // 最后尝试搜索配方目录下的 .recipe 文件
            if (!string.IsNullOrEmpty(recipeConfigPath) && Directory.Exists(recipeConfigPath))
            {
                var parentDir = Directory.GetParent(recipeConfigPath);
                if (parentDir != null)
                {
                    foreach (var file in Directory.GetFiles(parentDir.FullName, "*.recipe"))
                    {
                        return file;
                    }

                    // 检查子目录中的 .recipe 文件
                    foreach (var dir in Directory.GetDirectories(parentDir.FullName))
                    {
                        foreach (var file in Directory.GetFiles(dir, "*.recipe"))
                        {
                            return file;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 递归扫描 Module 节点，提取报警函数信息
        /// </summary>
        private void ScanModules(XElement parent, List<(string moduleId, string moduleName, string alarmCode, string message, string detail)> results)
        {
            foreach (var moduleEl in parent.Elements("Module"))
            {
                // 检查是否被标记为忽略 (IsSkip)
                var isSkipAttr = moduleEl.Attribute("IsSkip");
                if (isSkipAttr != null && bool.TryParse(isSkipAttr.Value, out bool isSkip) && isSkip)
                    continue;

                // 跳过测试工站（其内部报警工具不应纳入自定义报警配置）
                if (moduleEl.Elements("Function").Any(f => f.Attribute("Name")?.Value == "TestStation"))
                    continue;

                // 跳过回零工站（其内部报警工具不应纳入自定义报警配置）
                if (moduleEl.Elements("Function").Any(f => f.Attribute("Name")?.Value == "HomeStation"))
                    continue;

                // 模块唯一 ID（GUID）
                string moduleId = moduleEl.Attribute("ID")?.Value ?? "";

                // 优先使用 Alias（用户显示名称，如"报警1"），其次使用 Name（内部名称）
                string moduleName = moduleEl.Attribute("Alias")?.Value
                    ?? moduleEl.Attribute("Name")?.Value ?? "";

                // 查找 Alarm 函数
                foreach (var funcEl in moduleEl.Elements("Function"))
                {
                    var funcNameAttr = funcEl.Attribute("Name");
                    if (funcNameAttr != null && funcNameAttr.Value == "Alarm")
                    {
                        string alarmCode = GetParamValue(funcEl, "AlarmCode");
                        string message = GetParamValue(funcEl, "Message");
                        string detail = GetParamValue(funcEl, "Detail");
                        string alarmType = GetParamValue(funcEl, "AlarmType");

                        // 过滤掉报警类型为“信息提示”、“报警断点”和“人工介入提示”相关的模块
                        if (!string.IsNullOrEmpty(alarmType) && 
                            (alarmType == "InfoTip" || alarmType == "RetryAlarm" || alarmType == "ManuOperationAlarm"))
                        {
                            continue;
                        }

                        if (!string.IsNullOrEmpty(alarmCode))
                        {
                            results.Add((moduleId, moduleName, alarmCode, message, detail));
                        }
                    }
                }

                // 递归扫描子模块
                var childrenEl = moduleEl.Element("Children");
                if (childrenEl != null)
                {
                    ScanModules(childrenEl, results);
                }

                // 也检查 Modules 容器
                var modulesEl = moduleEl.Element("Modules");
                if (modulesEl != null)
                {
                    ScanModules(modulesEl, results);
                }
            }

            // 检查 Modules 容器
            var topModules = parent.Element("Modules");
            if (topModules != null && parent.Name != "Modules")
            {
                ScanModules(topModules, results);
            }
        }

        /// <summary>
        /// 从 Function 元素中提取参数值
        /// </summary>
        private string GetParamValue(XElement funcEl, string paramName)
        {
            var paramEl = funcEl.Element(paramName);
            if (paramEl != null)
            {
                var valAttr = paramEl.Attribute("Value");
                if (valAttr != null)
                    return valAttr.Value;
            }
            return "";
        }

        /// <summary>
        /// 后处理 .recipe 和 .data XML 文件：将 Alarm 函数的 AlarmCode/Message/Detail 参数更新为配置页面的值，
        /// 移除可能存在的 Global 引用（RefID/RefName 属性），并同步更新内嵌的 AlarmC/VDevice 数据。
        /// </summary>
        private void UpdateRecipeXmlForAlarms()
        {
            try
            {
                string recipePath = deviceEngine.RecipeConfigPath;
                if (string.IsNullOrEmpty(recipePath) || !Directory.Exists(recipePath))
                    return;

                string recipeDataPath = deviceEngine.RecipeDataPath;
                string workflowFile = FindWorkflowFile(recipePath, recipeDataPath);
                if (string.IsNullOrEmpty(workflowFile) || !File.Exists(workflowFile))
                    return;

                // === 1. 后处理 .recipe 文件 ===
                var xRoot = XElement.Load(workflowFile);
                bool recipeModified = false;

                foreach (var item in ErrorList)
                {
                    if (string.IsNullOrEmpty(item.ModuleId)) continue;

                    var moduleEl = FindModuleById(xRoot, item.ModuleId);
                    if (moduleEl == null) continue;

                    foreach (var funcEl in moduleEl.Elements("Function"))
                    {
                        var funcNameAttr = funcEl.Attribute("Name");
                        if (funcNameAttr == null || funcNameAttr.Value != "Alarm")
                            continue;

                        recipeModified |= UpdateAlarmFunctionParams(funcEl, item);
                    }
                }

                if (recipeModified)
                {
                    xRoot.Save(workflowFile);
                    SimEngineUI?.OnLog(Common.DataStruct.Enums.LogType.Info,
                        "已更新 .recipe 文件：Alarm 参数已同步至配置页面的值");
                }

                // === 2. 后处理 .data 文件 ===
                // .data 文件中 Alarm 函数为扁平结构：<Function Name="Alarm" ID="moduleId" ...>
                // 其中 ID 属性对应 .recipe 中 Module 的 ID
                string dataFile = recipeDataPath;
                if (!string.IsNullOrEmpty(dataFile) && File.Exists(dataFile))
                {
                    var xData = XElement.Load(dataFile);
                    bool dataModified = false;

                    foreach (var item in ErrorList)
                    {
                        if (string.IsNullOrEmpty(item.ModuleId)) continue;

                        // 在 .data 中按 Function Name="Alarm" + ID=ModuleId 查找
                        var alarmFuncs = xData.Descendants("Function")
                            .Where(f => f.Attribute("Name")?.Value == "Alarm"
                                     && f.Attribute("ID")?.Value == item.ModuleId)
                            .ToList();

                        foreach (var funcEl in alarmFuncs)
                        {
                            dataModified |= UpdateAlarmFunctionParams(funcEl, item);
                        }
                    }

                    if (dataModified)
                    {
                        xData.Save(dataFile);
                        SimEngineUI?.OnLog(Common.DataStruct.Enums.LogType.Info,
                            "已更新 .data 文件：Alarm 参数已同步至配置页面的值");
                    }
                }
            }
            catch (Exception ex)
            {
                SimEngineUI?.OnLog(Common.DataStruct.Enums.LogType.Warning, $"更新配方XML失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新单个 Alarm Function 元素中的 AlarmCode/Message/Detail 参数及 AlarmC VDevice 子数据。
        /// 同时适用于 .recipe 和 .data 两种 XML 结构。
        /// </summary>
        private bool UpdateAlarmFunctionParams(XElement funcEl, ErrorItemCustomModel item)
        {
            bool changed = false;

            // 更新 AlarmCode/Message/Detail 参数：设置 Value，移除可能的 RefID/RefName
            changed |= UpdateAlarmParamInXml(funcEl, "AlarmCode", item.AlarmCode);
            changed |= UpdateAlarmParamInXml(funcEl, "Message", item.AlarmContent);
            changed |= UpdateAlarmParamInXml(funcEl, "Detail", item.AlarmEnglish);

            // 同步更新 AlarmC 内嵌的 VDevice 数据
            var alarmCEl = funcEl.Element("AlarmC");
            if (alarmCEl != null)
            {
                var vDeviceEl = alarmCEl.Element("VDevice");
                if (vDeviceEl != null)
                {
                    var alarmKeyEl = vDeviceEl.Element("AlarmKey");
                    if (alarmKeyEl != null && alarmKeyEl.Value != (item.AlarmCode ?? ""))
                    {
                        alarmKeyEl.Value = item.AlarmCode ?? "";
                        changed = true;
                    }
                    var alarmCNEl = vDeviceEl.Element("AlarmCN");
                    if (alarmCNEl != null && alarmCNEl.Value != (item.AlarmContent ?? ""))
                    {
                        alarmCNEl.Value = item.AlarmContent ?? "";
                        changed = true;
                    }
                    var alarmEnEl = vDeviceEl.Element("AlarmEn");
                    if (alarmEnEl != null && alarmEnEl.Value != (item.AlarmEnglish ?? ""))
                    {
                        alarmEnEl.Value = item.AlarmEnglish ?? "";
                        changed = true;
                    }
                }
            }

            return changed;
        }

        /// <summary>
        /// 更新 Function 元素中指定参数：设置 Value 属性，移除 RefID/RefName 引用属性
        /// </summary>
        private bool UpdateAlarmParamInXml(XElement funcEl, string paramName, string newValue)
        {
            var paramEl = funcEl.Element(paramName);
            if (paramEl == null) return false;

            bool changed = false;

            // 移除 Global 引用属性
            var refIdAttr = paramEl.Attribute("RefID");
            var refNameAttr = paramEl.Attribute("RefName");
            if (refIdAttr != null) { refIdAttr.Remove(); changed = true; }
            if (refNameAttr != null) { refNameAttr.Remove(); changed = true; }

            // 移除可能存在的 <Ref> 子元素
            foreach (var r in paramEl.Elements("Ref").ToList()) { r.Remove(); changed = true; }

            // 设置/更新 Value 属性
            var valAttr = paramEl.Attribute("Value");
            if (valAttr != null)
            {
                if (valAttr.Value != (newValue ?? "")) { valAttr.Value = newValue ?? ""; changed = true; }
            }
            else
            {
                paramEl.SetAttributeValue("Value", newValue ?? "");
                changed = true;
            }

            return changed;
        }

        /// <summary>
        /// 递归在 XML 中查找指定 ID 的 Module 元素
        /// </summary>
        private XElement FindModuleById(XElement parent, string targetId)
        {
            foreach (var moduleEl in parent.Elements("Module"))
            {
                string id = moduleEl.Attribute("ID")?.Value ?? "";
                if (id == targetId) return moduleEl;

                var childrenEl = moduleEl.Element("Children");
                if (childrenEl != null)
                {
                    var found = FindModuleById(childrenEl, targetId);
                    if (found != null) return found;
                }

                var modulesEl = moduleEl.Element("Modules");
                if (modulesEl != null)
                {
                    var found = FindModuleById(modulesEl, targetId);
                    if (found != null) return found;
                }
            }

            var topModules = parent.Element("Modules");
            if (topModules != null && parent.Name != "Modules")
            {
                var found = FindModuleById(topModules, targetId);
                if (found != null) return found;
            }

            return null;
        }

        /// <summary>
        /// 编辑条目：打开编辑对话框
        /// </summary>
        private void EditItem(ErrorItemCustomModel item)
        {
            if (item == null) return;

            var existingCodes = ErrorList.Select(e => e.AlarmCode).ToList();

            dialogService.ShowAlarmConfigEditDialog(
                item.AlarmCode,
                item.AlarmContent,
                item.AlarmEnglish,
                item.AlarmCategory,
                item.RepairAction,
                existingCodes,
                r =>
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        r.Parameters.TryGetValue<string>("AlarmCode", out var alarmCode);
                        r.Parameters.TryGetValue<string>("AlarmContent", out var alarmContent);
                        r.Parameters.TryGetValue<string>("AlarmEnglish", out var alarmEnglish);
                        r.Parameters.TryGetValue<string>("AlarmCategory", out var alarmCategory);
                        r.Parameters.TryGetValue<string>("RepairAction", out var repairAction);

                        string oldCode = item.AlarmCode;
                        item.AlarmCode = alarmCode ?? item.AlarmCode;
                        item.AlarmContent = alarmContent ?? item.AlarmContent;
                        item.AlarmEnglish = alarmEnglish ?? item.AlarmEnglish;
                        item.AlarmCategory = alarmCategory ?? "";
                        item.RepairAction = repairAction ?? "";

                        // 同步更新 VAlarm
                        if (deviceEngine != null)
                        {
                            var allAlarms = deviceEngine.GetVDevices<VAlarm>()?.ToList() ?? new List<VAlarm>();

                            // 按 ModuleId (DeviceID) 查找 VAlarm，有 ModuleId 时严格匹配
                            // 排除 ProEvent VAlarm，避免跨页面交叉污染
                            VAlarm alarm = null;
                            if (!string.IsNullOrEmpty(item.ModuleId) && Guid.TryParse(item.ModuleId, out var moduleIdGuid) && moduleIdGuid != Guid.Empty)
                            {
                                alarm = allAlarms.FirstOrDefault(a => a.DeviceID == moduleIdGuid && a.Name != "ProEvent");
                            }
                            else
                            {
                                alarm = allAlarms.FirstOrDefault(a => a.AlarmKey == oldCode && a.Name != "ProEvent");
                            }

                            if (alarm != null)
                            {
                                alarm.AlarmKey = item.AlarmCode;
                                alarm.AlarmCN = item.AlarmContent;
                                alarm.AlarmEn = item.AlarmEnglish;
                                if (oldCode != item.AlarmCode)
                                    deviceEngine.RaiseAlarmCodeChangedEvent(oldCode, item.AlarmCode);
                            }
                            deviceEngine.RaiseVDeviceChangedEvent();
                        }

                        // 立即保存引擎并后处理 XML，将配置页面的值写入配方文件
                        try { deviceEngine.Save(); } catch { }
                        UpdateRecipeXmlForAlarms();
                        UpdateRuntimeAlarmModule(item.ModuleId, item.AlarmCode, item.AlarmContent, item.AlarmEnglish);

                        RaisePropertyChanged(nameof(FilteredErrorList));
                    }
                });
        }

        /// <summary>
        /// UnLoaded
        /// </summary>
        private DelegateCommand<ObservableCollection<ErrorItemCustomModel>> _unLoadedCommand;
        public DelegateCommand<ObservableCollection<ErrorItemCustomModel>> UnLoadedCommand => _unLoadedCommand ?? (_unLoadedCommand = new DelegateCommand<ObservableCollection<ErrorItemCustomModel>>((items) =>
        {
            SaveToCsvFile(ErrorList);
        }));

        private void SaveToCsvFile(ObservableCollection<ErrorItemCustomModel> items)
        {
            try
            {
                // 使用绑定路径而非引擎当前路径，防止跨配方污染
                string savePath = _boundRecipePath ?? deviceEngine.RecipeConfigPath;
                string csvPath = Path.Combine(savePath, "CustomErrors.csv");
                var csvLines = new List<string>();

                // 添加CSV表头
                csvLines.Add("AlarmCode,AlarmContent,AlarmEnglish,AlarmCategory,RepairAction,ModuleName,ModuleId");

                // 添加数据行
                foreach (var item in items)
                {
                    var content = EscapeCsvField(item.AlarmContent ?? "");
                    var english = EscapeCsvField(item.AlarmEnglish ?? "");
                    var category = EscapeCsvField(item.AlarmCategory ?? "");
                    var repairAction = EscapeCsvField(item.RepairAction ?? "");
                    var moduleName = EscapeCsvField(item.ModuleName ?? "");
                    var moduleId = EscapeCsvField(item.ModuleId ?? "");

                    csvLines.Add($"{item.AlarmCode},{content},{english},{category},{repairAction},{moduleName},{moduleId}");
                }

                File.WriteAllLines(csvPath, csvLines, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                if (SimEngineUI != null)
                {
                    SimEngineUI.OnLog(Common.DataStruct.Enums.LogType.Error, $"保存CSV文件失败: {ex.Message}");
                }
            }
        }

        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field)) return "\"\"";

            // 如果字段包含逗号、换行或引号，需要用引号包围并转义内部引号
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }

            return field;
        }

        private void LoadFromCsvFile()
        {
            if (string.IsNullOrEmpty(deviceEngine.RecipeConfigPath))
            {
                ErrorList.Clear();
                RaisePropertyChanged(nameof(FilteredErrorList));
                return;
            }

            string csvPath = Path.Combine(deviceEngine.RecipeConfigPath, "CustomErrors.csv");

            // 清空现有 UI 数据
            ErrorList.Clear();

            // 1. 读取 CSV 数据（CSV 是唯一真相源）
            var csvEntries = new List<(string code, string content, string english, string category, string repair, string module, string moduleId)>();

            if (File.Exists(csvPath))
            {
                try
                {
                    var lines = File.ReadAllLines(csvPath, Encoding.UTF8);
                    for (int i = 1; i < lines.Length; i++)
                    {
                        var line = lines[i];
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        var fields = ParseCsvLine(line);
                        if (fields.Length >= 3)
                        {
                            csvEntries.Add((
                                code: fields[0],
                                content: UnescapeCsvField(fields[1]),
                                english: UnescapeCsvField(fields[2]),
                                category: fields.Length > 3 ? UnescapeCsvField(fields[3]) : "",
                                repair: fields.Length > 4 ? UnescapeCsvField(fields[4]) : "",
                                module: fields.Length > 5 ? UnescapeCsvField(fields[5]) : "",
                                moduleId: fields.Length > 6 ? UnescapeCsvField(fields[6]) : ""
                            ));
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"读取CSV文件失败: {ex.Message}");
                }
            }

            // 2. 填充 UI 列表
            foreach (var entry in csvEntries)
            {
                var newModel = new ErrorItemCustomModel(
                    alarmCode: entry.code,
                    alarmContent: entry.content,
                    alarmEnglish: entry.english,
                    alarmCategory: entry.category,
                    repairAction: entry.repair,
                    moduleName: entry.module,
                    moduleId: entry.moduleId
                );
                newModel.PropertyChanged += Item_PropertyChanged;
                ErrorList.Add(newModel);
            }

            // 3. 以 CSV 为唯一真相源，同步引擎中的 VAlarm
            SyncEngineVAlarms(csvEntries.Select(e => (e.code, e.content, e.english, e.category, e.repair, e.moduleId)).ToList());

            RaisePropertyChanged(nameof(FilteredErrorList));
        }

        /// <summary>
        /// 以 CSV 数据为唯一真相源，同步引擎中的 VAlarm：
        /// - 按 ModuleId (DeviceID) 独立管理每个报警模块的 VAlarm
        /// - 删除不在 CSV 中的孤立 VAlarm
        /// - 补建 CSV 中有但引擎中缺失的 VAlarm
        /// </summary>
        private void SyncEngineVAlarms(List<(string code, string content, string english, string category, string repair, string moduleId)> csvEntries)
        {
            if (deviceEngine == null) return;

            // 构建查找集合
            var csvModuleIds = new HashSet<Guid>(
                csvEntries.Select(e => e.moduleId)
                          .Where(id => !string.IsNullOrEmpty(id))
                          .Select(id => Guid.TryParse(id, out var g) ? g : Guid.Empty)
                          .Where(g => g != Guid.Empty));
            var csvCodes = new HashSet<string>(csvEntries.Select(e => e.code).Where(c => !string.IsNullOrEmpty(c)));

            var allVAlarms = deviceEngine.GetVDevices<VAlarm>()?.ToList() ?? new List<VAlarm>();

            // 删除不在 CSV 中的孤立 VAlarm（跳过产品事件报警页面创建的 VAlarm）
            foreach (var alarm in allVAlarms)
            {
                // 跳过由产品事件报警配置页面创建的 VAlarm
                if (alarm.Name == "ProEvent") continue;

                bool shouldRemove = false;
                if (alarm.DeviceID != Guid.Empty)
                {
                    // 有 ModuleId 的 VAlarm：仅当 ModuleId 不在 CSV 中时移除
                    if (!csvModuleIds.Contains(alarm.DeviceID))
                        shouldRemove = true;
                }
                else
                {
                    // 无 ModuleId 的旧 VAlarm：仅当 AlarmKey 不在 CSV 中时移除
                    if (!csvCodes.Contains(alarm.AlarmKey))
                        shouldRemove = true;
                }

                if (shouldRemove)
                {
                    try { deviceEngine.ReomoveVirtual(alarm.ID); }
                    catch { }
                }
            }

            // 补建 CSV 中有但引擎中缺失的 VAlarm（按 ModuleId 或 AlarmKey 判断）
            var remaining = deviceEngine.GetVDevices<VAlarm>()?.ToList() ?? new List<VAlarm>();
            var existingModuleIds = new HashSet<Guid>(remaining.Where(a => a.DeviceID != Guid.Empty && a.Name != "ProEvent").Select(a => a.DeviceID));
            var existingKeys = new HashSet<string>(remaining.Where(a => a.DeviceID == Guid.Empty && a.Name != "ProEvent").Select(a => a.AlarmKey));

            foreach (var entry in csvEntries)
            {
                if (string.IsNullOrEmpty(entry.code)) continue;

                var moduleIdGuid = Guid.TryParse(entry.moduleId, out var g) ? g : Guid.Empty;

                if (moduleIdGuid != Guid.Empty)
                {
                    // 有 ModuleId 的条目：按 DeviceID 判断是否需要新建
                    if (!existingModuleIds.Contains(moduleIdGuid))
                    {
                        deviceEngine.AddVirtual(new VAlarm()
                        {
                            AlarmKey = entry.code,
                            AlarmCN = entry.content,
                            AlarmEn = entry.english,
                            ID = Guid.NewGuid(),
                            DeviceID = moduleIdGuid
                        });
                        existingModuleIds.Add(moduleIdGuid);
                    }
                }
                else
                {
                    // 无 ModuleId 的旧条目：按 AlarmKey 判断
                    if (!existingKeys.Contains(entry.code))
                    {
                        deviceEngine.AddVirtual(new VAlarm()
                        {
                            AlarmKey = entry.code,
                            AlarmCN = entry.content,
                            AlarmEn = entry.english,
                            ID = Guid.NewGuid()
                        });
                        existingKeys.Add(entry.code);
                    }
                }
            }
        }

        private string[] ParseCsvLine(string line)
        {
            var fields = new List<string>();
            var currentField = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        // 转义引号
                        currentField.Append('"');
                        i++; // 跳过下一个引号
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    fields.Add(currentField.ToString());
                    currentField.Clear();
                }
                else
                {
                    currentField.Append(c);
                }
            }

            fields.Add(currentField.ToString());
            return fields.ToArray();
        }

        private string UnescapeCsvField(string field)
        {
            if (field.StartsWith("\"") && field.EndsWith("\""))
            {
                field = field.Substring(1, field.Length - 2);
                field = field.Replace("\"\"", "\"");
            }
            return field;
        }

        /// <summary>
        /// 批量导出选中项
        /// </summary>
        private void BatchExport(List<ErrorItemCustomModel> items)
        {
            if (items == null || items.Count == 0)
            {
                SimEngineUI.OnLog(Common.DataStruct.Enums.LogType.Warning, "列表数据为空");
                return;
            }

            try
            {
                // 创建保存文件对话框
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "CSV文件 (*.csv)|*.csv|所有文件 (*.*)|*.*";
                saveFileDialog.FileName = $"CustomErrors_Export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                saveFileDialog.DefaultExt = ".csv";

                if (saveFileDialog.ShowDialog() == true)
                {
                    string filePath = saveFileDialog.FileName;
                    var csvLines = new List<string>();

                    // 添加CSV表头
                    csvLines.Add("AlarmCode,AlarmContent,AlarmEnglish,AlarmCategory,RepairAction,ModuleName,ModuleId");

                    // 添加数据行
                    foreach (var item in items)
                    {
                        var content = EscapeCsvField(item.AlarmContent ?? "");
                        var english = EscapeCsvField(item.AlarmEnglish ?? "");
                        var category = EscapeCsvField(item.AlarmCategory ?? "");
                        var repairAction = EscapeCsvField(item.RepairAction ?? "");
                        var moduleName = EscapeCsvField(item.ModuleName ?? "");
                        var moduleId = EscapeCsvField(item.ModuleId ?? "");

                        csvLines.Add($"{item.AlarmCode},{content},{english},{category},{repairAction},{moduleName},{moduleId}");
                    }

                    // 写入文件
                    File.WriteAllLines(filePath, csvLines, Encoding.UTF8);

                    SimEngineUI.OnLog(Common.DataStruct.Enums.LogType.Info, $"成功导出 {items.Count} 条错误信息到 {filePath}");
                }
            }
            catch (Exception ex)
            {
                SimEngineUI.OnLog(Common.DataStruct.Enums.LogType.Error, $"导出失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 导出选中项的命令
        /// </summary>
        private void ExportTotalCommand()
        {
            var itemsToExport = ErrorList.ToList();
            BatchExport(itemsToExport);
        }

        /// <summary>
        /// 批量导入信息
        /// </summary>
        private void BatchImport(List<ErrorItemCustomModel> items)
        {
            if (items == null || items.Count == 0)
            {
                SimEngineUI.OnLog(Common.DataStruct.Enums.LogType.Warning, "没有可导入的数据");
                return;
            }

            try
            {
                // 统计新增和更新的数量
                int addedCount = 0;
                int updatedCount = 0;

                foreach (var importedItem in items)
                {
                    // 按 ModuleId 判断重复；有 ModuleId 时严格按 ModuleId 匹配，不回退到 AlarmCode
                    ErrorItemCustomModel existingItem = null;
                    if (!string.IsNullOrEmpty(importedItem.ModuleId))
                    {
                        existingItem = ErrorList.FirstOrDefault(e => e.ModuleId == importedItem.ModuleId);
                    }
                    else
                    {
                        existingItem = ErrorList.FirstOrDefault(e => e.AlarmCode == importedItem.AlarmCode);
                    }

                    if (existingItem == null)
                    {
                        // 添加新项
                        var newModel = new ErrorItemCustomModel(
                            importedItem.AlarmCode,
                            importedItem.AlarmContent,
                            importedItem.AlarmEnglish,
                            importedItem.AlarmCategory,
                            importedItem.RepairAction,
                            importedItem.ModuleName,
                            importedItem.ModuleId
                        );
                        newModel.PropertyChanged += Item_PropertyChanged;
                        ErrorList.Add(newModel);
                        addedCount++;
                    }
                    else
                    {
                        // 更新现有项
                        existingItem.AlarmContent = importedItem.AlarmContent;
                        existingItem.AlarmEnglish = importedItem.AlarmEnglish;
                        existingItem.AlarmCategory = importedItem.AlarmCategory;
                        existingItem.RepairAction = importedItem.RepairAction;
                        existingItem.ModuleName = importedItem.ModuleName;
                        updatedCount++;
                    }
                }

                // 同步到设备引擎（先检查是否已存在，避免重复创建）
                foreach (var item in items)
                {
                    if (deviceEngine == null || string.IsNullOrEmpty(item.AlarmCode)) continue;

                    var moduleIdGuid = Guid.TryParse(item.ModuleId, out var midGuid) ? midGuid : Guid.Empty;

                    // 有 ModuleId 时严格按 DeviceID 查找，避免误匹配同 AlarmCode 的其他模块
                    VAlarm existing = null;
                    if (moduleIdGuid != Guid.Empty)
                    {
                        existing = deviceEngine.GetVDevices<VAlarm>()
                            ?.FirstOrDefault(a => a.DeviceID == moduleIdGuid);
                    }
                    else
                    {
                        // 无 ModuleId 的旧条目：按 AlarmKey 查找
                        existing = deviceEngine.GetVDevices<VAlarm>()
                            ?.FirstOrDefault(a => a.AlarmKey == item.AlarmCode);
                    }

                    if (existing != null)
                    {
                        existing.AlarmCN = item.AlarmContent;
                        existing.AlarmEn = item.AlarmEnglish;
                    }
                    else
                    {
                        deviceEngine.AddVirtual(new VAlarm()
                        {
                            AlarmKey = item.AlarmCode,
                            AlarmCN = item.AlarmContent,
                            AlarmEn = item.AlarmEnglish,
                            ID = Guid.NewGuid(),
                            DeviceID = moduleIdGuid
                        });
                    }
                }

                // 通知所有 Alarm 实例更新 VAlarm 数据
                deviceEngine.RaiseVDeviceChangedEvent();

                SimEngineUI.OnLog(Common.DataStruct.Enums.LogType.Info, $"导入完成：新增 {addedCount} 条，更新 {updatedCount} 条");
            }
            catch (Exception ex)
            {
                SimEngineUI.OnLog(Common.DataStruct.Enums.LogType.Error, $"导入失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 导入文件命令
        /// </summary>
        private void ImportTotalCommand()
        {
            try
            {
                // 创建打开文件对话框
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "CSV文件 (*.csv)|*.csv|所有文件 (*.*)|*.*";
                openFileDialog.DefaultExt = ".csv";

                if (openFileDialog.ShowDialog() == true)
                {
                    string filePath = openFileDialog.FileName;

                    // 读取CSV文件
                    var lines = File.ReadAllLines(filePath, Encoding.UTF8);
                    if (lines.Length < 2) // 至少包含表头和数据行
                    {
                        SimEngineUI.OnLog(Common.DataStruct.Enums.LogType.Warning, "CSV文件格式不正确，至少需要包含表头和数据行");
                        return;
                    }

                    var importedItems = new List<ErrorItemCustomModel>();

                    // 跳过表头（第一行）
                    for (int i = 1; i < lines.Length; i++)
                    {
                        var line = lines[i];
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        var fields = ParseCsvLine(line);
                        if (fields.Length >= 3)
                        {
                            importedItems.Add(new ErrorItemCustomModel(
                                alarmCode: fields[0],
                                alarmContent: UnescapeCsvField(fields[1]),
                                alarmEnglish: UnescapeCsvField(fields[2]),
                                alarmCategory: fields.Length > 3 ? UnescapeCsvField(fields[3]) : "",
                                repairAction: fields.Length > 4 ? UnescapeCsvField(fields[4]) : "",
                                moduleName: fields.Length > 5 ? UnescapeCsvField(fields[5]) : "",
                                moduleId: fields.Length > 6 ? UnescapeCsvField(fields[6]) : ""
                            ));
                        }
                        else
                        {
                            SimEngineUI.OnLog(Common.DataStruct.Enums.LogType.Warning, $"跳过第 {i + 1} 行：字段数量不足");
                        }
                    }

                    if (importedItems.Count > 0)
                    {
                        CleanupUnusedVAlarms(importedItems);
                        ErrorList.Clear();
                        BatchImport(importedItems);
                        RaisePropertyChanged(nameof(FilteredErrorList));
                    }
                    else
                    {
                        SimEngineUI.OnLog(Common.DataStruct.Enums.LogType.Warning, "CSV文件中没有有效数据");
                    }
                }
            }
            catch (Exception ex)
            {
                SimEngineUI.OnLog(Common.DataStruct.Enums.LogType.Error, $"导入文件失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 同步清理设备引擎中未使用的 VAlarm 设备 (按需调用)
        /// </summary>
        private void CleanupUnusedVAlarms(List<ErrorItemCustomModel> validItems)
        {
            if (deviceEngine == null) return;

            var importedModuleIds = new HashSet<Guid>(
                validItems.Where(x => !string.IsNullOrEmpty(x.ModuleId))
                          .Select(x => Guid.TryParse(x.ModuleId, out var g) ? g : Guid.Empty)
                          .Where(g => g != Guid.Empty));
            var importedCodes = validItems.Where(x => string.IsNullOrEmpty(x.ModuleId))
                                          .Select(x => x.AlarmCode).ToHashSet();
            var allVAlarms = deviceEngine.GetVDevices<VAlarm>()?.ToList() ?? new List<VAlarm>();

            foreach (var alarm in allVAlarms)
            {
                bool shouldKeep = false;
                if (alarm.DeviceID != Guid.Empty)
                {
                    // 有 ModuleId 的 VAlarm：按 DeviceID 判断
                    shouldKeep = importedModuleIds.Contains(alarm.DeviceID);
                }
                else
                {
                    // 无 ModuleId 的旧 VAlarm：按 AlarmKey 判断
                    shouldKeep = importedCodes.Contains(alarm.AlarmKey);
                }
                if (!shouldKeep)
                {
                    deviceEngine.ReomoveVirtual(alarm.ID);
                }
            }
        }

        #region 生成 ErrorCodeList

        /// <summary>
        /// 错误类型翻译映射表
        /// </summary>
        private static readonly Dictionary<string, string> ErrorTransMap = new Dictionary<string, string>
        {
            { "SensorFail", "信号异常" },
            { "SafeFail", "非安全位" },
            { "ZeroFail", "回零失败" },
            { "SerOnFail", "使能失败" },
            { "PelFail", "正极限报警" },
            { "MelFail", "负极限报警" },
            { "MoveTimeFail", "运行超时" },
            { "ExtendFail", "伸出异常" },
            { "RetractFail", "缩回异常" },
            { "BlowFail", "真空吹失败" },
            { "SuckFail", "真空吸失败" },
            { "ConnectTimeFail", "通讯异常" },
            { "HPMatain", "维护提醒" },
            { "SafeDoorFail", "安全门报警" }
        };

        /// <summary>
        /// 预定义默认行
        /// </summary>
        private static readonly string[][] DefaultErrorCodeRows = new string[][]
        {
            new[] { "Emergency stop button", "F01ESOO-01", "Actual Downtime", "急停按下", "顺时针方向旋转被按下的急停按钮，使其弹起复位。或者急停按钮损坏", "F01ESOO-01" },
            new[] { "Safety door open", "F02SCOO-01", "Waiting for OP", "安全门打开", "1、检查所有安全门是否完全关闭。2、检查门锁电磁锁/机械锁是否正常啮合。如果门已关但信号未通，手动检查门锁传感器（如磁性开关、行程开关）是否到位或损坏。", "F02SCOO-01" },
            new[] { "Barcode scan error", "T01OOOO-01", "Actual downtime", "扫描码异常", "清洁：使用无尘布二维码表面及扫码器的玻璃窗口。调整扫码器：微扫码器的角度、距离和焦距，确保扫描线能完整覆盖条码。对于固定式扫码器，可能需要调整安装支架。调整光照：检查环境光是否过强或过弱导致反光/阴影。调整扫码器自带的照明或外部光源。检查参数：登录扫码器配置软件，确认扫码模式（一维/二维）、触发模式、解码算法设置正确。", "T01OOOO-01" },
            new[] { "PDCA communication error", "N03OOOO-01", "Actual downtime", "PDCA通讯异常", "检查设备工控机与PDCA服务器之间的网络连接。在工控机命令行使用 ping 命令测试网络是否通畅。检查网线是否松动，网口指示灯是否正常闪烁。", "N03OOOO-01" },
            new[] { "Hive communication error", "N03OOOO-02", "Actual downtime", "Hive通讯异常", "检查设备工控机与Hive服务器之间的网络连接。在工控机命令行使用 ping 命令测试网络是否通畅。检查网线是否松动，网口指示灯是否正常闪烁。", "N03OOOO-02" },
            new[] { "Efficiency loss due to own process", "E99OOOO-90", "Waiting for OP", "设备本身的效率损失", "无", "E99OOOO-90" },
            new[] { "Downstream blocked", "E99OOOO-99", "Waiting for OP", "下游导致的效率损失", "无", "E99OOOO-99" },
            new[] { "PC/SW crashed", "N99PCSW-01", "Actual downtime", "电脑死机或软件崩溃", "无", "N99PCSW-01" },
            new[] { "Undefined error", "TBD", "Actual downtime", "未定义的异常", "无", "TBD" },
            new[] { "Manual triggered DT", "F99OOOO-20", "Actual downtime", "手动触发的停机", "无", "F99OOOO-20" },
            new[] { "1st tier vendor", "F99OOOO-01", "Actual downtime", "一级供应商", "无", "F99OOOO-01" },
            new[] { "1st and 2nd tier vendor", "F99OOOO-02", "Actual downtime", "一级供应商和二级供应商", "无", "F99OOOO-02" },
            new[] { "Cognex", "F99OOOO-03", "Actual downtime", "康耐视", "无", "F99OOOO-03" },
            new[] { "Keyence", "F99OOOO-04", "Actual downtime", "基恩士", "无", "F99OOOO-04" },
            new[] { "Repaired by contract manufacturer", "F99OOOO-11", "Actual downtime", "工厂人员维修", "无", "F99OOOO-11" },
            new[] { "Consumable material replenishment", "F99OOOO-08", "Waiting for OP", "耗材 / 物料补充", "无", "F99OOOO-08" },
            new[] { "Fine-tuning during non-production time", "F99OOOO-09", "Waiting for OP", "非生产时间优化设备", "无", "F99OOOO-09" },
            new[] { "Weekly Maintenance", "F99OOOO-05", "Waiting for OP", "周保养", "无", "F99OOOO-05" },
            new[] { "Monthly Maintenance", "F99OOOO-06", "Waiting for OP", "月保养", "无", "F99OOOO-06" },
            new[] { "Planned shutdown/idle", "F99OOOO-07", "Waiting for OP", "计划关机/待机", "无", "F99OOOO-07" },
            new[] { "N / A", "F99OOOO-10", "Actual downtime", "N / A", "无", "F99OOOO-10" }
        };

        /// <summary>
        /// 生成 ErrorCodeList CSV 文件
        /// </summary>
        private void GenerateErrorCodeList()
        {
            _dialogService.ShowConfirm("生成ErrorCodeList前，请确认当前配方已保存至最新？", r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    DoGenerateErrorCodeList();
                }
            });
        }

        private void DoGenerateErrorCodeList()
        {
            try
            {
                // 先将当前界面的报警配置数据（包括报警种类和维修动作）保存到CSV，确保生成时使用最新数据
                SaveToCsvFile(ErrorList);

                string configPath = deviceEngine.RecipeConfigPath;
                if (string.IsNullOrEmpty(configPath))
                {
                    SimEngineUI.OnLog(Common.DataStruct.Enums.LogType.Error, "配方路径为空，无法生成ErrorCodeList");
                    return;
                }

                string customErrorsPath = Path.Combine(configPath, "CustomErrors.csv");
                string dprojPath = Path.Combine(configPath, "Hardware.dproj");

                // 输出目录
                string outputDir = @"D:\Hive";
                if (!Directory.Exists(outputDir))
                {
                    try { Directory.CreateDirectory(outputDir); } catch { }
                }

                // 获取配置中的 机种 和 工站名称
                string machineModel = "V6x";
                string stationName = "CGx";
                try
                {
                    string webConfigPath = "";
                    DirectoryInfo dir = new DirectoryInfo(deviceEngine.RecipeConfigPath);
                    while (dir != null)
                    {
                        string testPath = Path.Combine(dir.FullName, "WebConfig.xml");
                        if (File.Exists(testPath))
                        {
                            webConfigPath = testPath;
                            break;
                        }
                        dir = dir.Parent;
                    }

                    if (!string.IsNullOrEmpty(webConfigPath))
                    {
                        var xml = System.Xml.Linq.XElement.Load(webConfigPath);
                        var pNameElem = xml.Element("Product");
                        if (pNameElem != null && !string.IsNullOrWhiteSpace(pNameElem.Value))
                        {
                            machineModel = pNameElem.Value.Trim();
                        }
                        var sNameElem = xml.Element("StationName");
                        if (sNameElem != null && !string.IsNullOrWhiteSpace(sNameElem.Value))
                        {
                            stationName = sNameElem.Value.Trim();
                        }
                    }
                }
                catch (Exception ex)
                {
                    SimEngineUI.OnLog(Common.DataStruct.Enums.LogType.Warning, $"获取文件名配置字段失败: {ex.Message}");
                }

                string targetFileName = $"{machineModel} {stationName} LUSTER ERROR LIST.csv";
                string outputPath = Path.Combine(outputDir, targetFileName);

                // CSV 表头
                string[] headers = { "Error Description", "Code", "Category", "Error Description (Chinese)", "Repair Actions", "Local Alarm Code" };

                var rows = new List<string[]>();

                // 1. 添加预定义默认行
                string defaultXmlPath = Path.Combine(configPath, "DefaultErrorCodes.xml");
                var defaultRows = GetOrUpdateDefaultErrorCodeRows(defaultXmlPath);
                rows.AddRange(defaultRows);

                // 2. 读取并添加 CustomErrors.csv（自定义报警配置）
                // CSV 列顺序：AlarmCode, AlarmContent, AlarmEnglish, AlarmCategory, RepairAction, ModuleName, ModuleId
                // 输出列映射：AlarmCode→Code+LocalAlarmCode, AlarmContent→ErrorDescription(Chinese),
                //            AlarmEnglish→ErrorDescription, AlarmCategory→Category, RepairAction→RepairActions
                if (File.Exists(customErrorsPath))
                {
                    try
                    {
                        var customLines = File.ReadAllLines(customErrorsPath, Encoding.UTF8);
                        bool isFirstLine = true;
                        foreach (var line in customLines)
                        {
                            if (isFirstLine) { isFirstLine = false; continue; }
                            if (string.IsNullOrWhiteSpace(line)) continue;

                            var parts = ParseCsvLine(line);
                            if (parts.Length >= 5)
                            {
                                string alarmCode = parts[0];
                                string alarmContent = UnescapeCsvField(parts[1]);
                                string alarmEnglish = UnescapeCsvField(parts[2]);
                                string alarmCategory = parts.Length > 3 ? UnescapeCsvField(parts[3]) : "";
                                string repairAction = parts.Length > 4 ? UnescapeCsvField(parts[4]) : "";
                                rows.Add(new[] { alarmEnglish, alarmCode, alarmCategory, alarmContent, repairAction, alarmCode });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        SimEngineUI.OnLog(Common.DataStruct.Enums.LogType.Warning, $"读取 CustomErrors.csv 失败: {ex.Message}");
                    }
                }

                //// 3. 读取并添加 ProductEventAlarms.csv（产品事件报警配置）
                //string productEventAlarmsPath = Path.Combine(configPath, "ProductEventAlarms.csv");
                //if (File.Exists(productEventAlarmsPath))
                //{
                //    try
                //    {
                //        var peLines = File.ReadAllLines(productEventAlarmsPath, Encoding.UTF8);
                //        bool isFirstLine = true;
                //        foreach (var line in peLines)
                //        {
                //            if (isFirstLine) { isFirstLine = false; continue; }
                //            if (string.IsNullOrWhiteSpace(line)) continue;

                //            var parts = ParseCsvLine(line);
                //            if (parts.Length >= 5)
                //            {
                //                string alarmCode = parts[0];
                //                string alarmContent = UnescapeCsvField(parts[1]);
                //                string alarmEnglish = UnescapeCsvField(parts[2]);
                //                string alarmCategory = parts.Length > 3 ? UnescapeCsvField(parts[3]) : "";
                //                string repairAction = parts.Length > 4 ? UnescapeCsvField(parts[4]) : "";
                //                rows.Add(new[] { alarmEnglish, alarmCode, alarmCategory, alarmContent, repairAction, alarmCode });
                //            }
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        SimEngineUI.OnLog(Common.DataStruct.Enums.LogType.Warning, $"读取 ProductEventAlarms.csv 失败: {ex.Message}");
                //    }
                //}

                // 4. 读取 Hardware.dproj
                if (File.Exists(dprojPath))
                {
                    try
                    {
                        var xDoc = System.Xml.Linq.XElement.Load(dprojPath);
                        foreach (var vDevice in xDoc.Descendants("VDevice"))
                        {
                            var nameNode = vDevice.Element("Name");
                            string deviceName = nameNode?.Value ?? "";

                            var catNode = vDevice.Element("AlarmCategory");
                            string alarmCategory = catNode?.Value ?? "";

                            var repairNode = vDevice.Element("RepairAction");
                            string repairAction = repairNode?.Value ?? "";

                            var errorsNode = vDevice.Element("Errors");
                            if (errorsNode != null)
                            {
                                foreach (var errorEl in errorsNode.Elements())
                                {
                                    string tagName = errorEl.Name.LocalName;
                                    string content = errorEl.Value ?? "";

                                    // 优先从错误元素属性读取 AlarmCategory 和 RepairAction，为空则回退到 VDevice 级别
                                    string errCategory = errorEl.Attribute("AlarmCategory")?.Value ?? "";
                                    if (string.IsNullOrEmpty(errCategory))
                                        errCategory = alarmCategory;

                                    string errRepairAction = errorEl.Attribute("RepairAction")?.Value ?? "";
                                    if (string.IsNullOrEmpty(errRepairAction))
                                        errRepairAction = repairAction;

                                    string translatedSuffix = ErrorTransMap.ContainsKey(tagName) ? ErrorTransMap[tagName] : tagName;
                                    string errDescCn = $"{deviceName}{translatedSuffix}";

                                    string codePart, descPart;
                                    if (content.Contains("@"))
                                    {
                                        var parts2 = content.Split(new[] { '@' }, 2);
                                        codePart = parts2[0];
                                        descPart = parts2[1];
                                    }
                                    else
                                    {
                                        codePart = content;
                                        descPart = "";
                                    }

                                    rows.Add(new[] { descPart, codePart, errCategory, errDescCn, errRepairAction, codePart });
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        SimEngineUI.OnLog(Common.DataStruct.Enums.LogType.Warning, $"解析 Hardware.dproj 时遇到异常: {ex.Message}");
                    }
                }

                // 5. 过滤无效数据：报警代码/报警内容/报警英文任一为空则过滤，报警代码为 10000 也过滤
                //    DefaultErrorCodeRows 不过滤 TBD，其他来源过滤报警代码包含 TBD 的项
                int defaultRowCount = defaultRows.Count;
                rows = rows.Where((row, index) =>
                {
                    if (row.Length < 4) return false;
                    string code = row[1]?.Trim() ?? "";
                    string descCn = row[3]?.Trim() ?? "";
                    string descEn = row[0]?.Trim() ?? "";
                    if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(descCn) || string.IsNullOrEmpty(descEn))
                        return false;
                    if (code == "10000") return false;
                    if (index >= defaultRowCount && code.Contains("TBD"))
                        return false;
                    return true;
                }).ToList();

                // 6. 生成目标 CSV
                using (var sw = new StreamWriter(outputPath, false, new System.Text.UTF8Encoding(true)))
                {
                    sw.WriteLine(string.Join(",", headers));
                    foreach (var row in rows)
                    {
                        for (int i = 0; i < row.Length; i++)
                        {
                            if (row[i].Contains(","))
                            {
                                row[i] = "\"" + row[i] + "\"";
                            }
                        }
                        sw.WriteLine(string.Join(",", row));
                    }
                }

                SimEngineUI.OnLog(Common.DataStruct.Enums.LogType.Info, $"ErrorCodeList 已生成: {outputPath}");

                // 弹窗提示
                _dialogService.ShowInfoTip($"ErrorCodeList 已生成: {outputPath}");
            }
            catch (Exception ex)
            {
                SimEngineUI.OnLog(Common.DataStruct.Enums.LogType.Error, $"生成ErrorCodeList时发生错误: {ex.Message}");
            }
        }

        private List<string[]> GetOrUpdateDefaultErrorCodeRows(string xmlPath)
        {
            var rows = new List<string[]>();

            if (!File.Exists(xmlPath))
            {
                try
                {
                    var xDoc = new System.Xml.Linq.XDocument(
                        new System.Xml.Linq.XDeclaration("1.0", "utf-8", "yes"),
                        new System.Xml.Linq.XElement("ErrorCodes")
                    );

                    foreach (var row in DefaultErrorCodeRows)
                    {
                        var el = new System.Xml.Linq.XElement("Error",
                            new System.Xml.Linq.XElement("ErrorDescription", row[0]),
                            new System.Xml.Linq.XElement("Code", row[1]),
                            new System.Xml.Linq.XElement("Category", row[2]),
                            new System.Xml.Linq.XElement("ErrorDescriptionChinese", row[3]),
                            new System.Xml.Linq.XElement("RepairActions", row[4]),
                            new System.Xml.Linq.XElement("LocalAlarmCode", row[5])
                        );
                        xDoc.Root.Add(el);
                    }
                    xDoc.Save(xmlPath);
                }
                catch (Exception ex)
                {
                    SimEngineUI?.OnLog(Common.DataStruct.Enums.LogType.Warning, $"生成默认配置 XML 失败: {ex.Message}");
                }

                return DefaultErrorCodeRows.ToList();
            }

            try
            {
                var xDoc = System.Xml.Linq.XDocument.Load(xmlPath);
                foreach (var element in xDoc.Descendants("Error"))
                {
                    string p0 = element.Element("ErrorDescription")?.Value ?? "";
                    string p1 = element.Element("Code")?.Value ?? "";
                    string p2 = element.Element("Category")?.Value ?? "";
                    string p3 = element.Element("ErrorDescriptionChinese")?.Value ?? "";
                    string p4 = element.Element("RepairActions")?.Value ?? "";
                    string p5 = element.Element("LocalAlarmCode")?.Value ?? "";
                    rows.Add(new[] { p0, p1, p2, p3, p4, p5 });
                }
            }
            catch (Exception ex)
            {
                SimEngineUI?.OnLog(Common.DataStruct.Enums.LogType.Warning, $"解析 DefaultErrorCodes.xml 失败, 请检查文件格式是否正确。错误: {ex.Message}");
                rows = DefaultErrorCodeRows.ToList();
            }
            return rows;
        }

        /// <summary>
        /// 同步更新运行时 Alarm 模块参数，确保流程图中的模块信息即时更新。
        /// 通过 MotionEngine.Get(id) 直接定位模块，替代原有的基于匿名投影对象的 BFS 遍历。
        /// </summary>
        private void UpdateRuntimeAlarmModule(string moduleId, string code, string message, string detail)
        {
            if (string.IsNullOrEmpty(moduleId) || deviceEngine == null) return;

            try
            {
                deviceEngine.RaiseUpdateAlarmModuleParams(moduleId, code, message, detail);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateRuntimeAlarmModule Error: {ex.Message}");
            }
        }

        #endregion
    }
}
