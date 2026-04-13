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
        public override bool IsShowAdd => true;

        private IDialogService _dialogService;
        private readonly IDeviceEngine deviceEngine;

        public override bool IsShowRemove => true;

        private ObservableCollection<ErrorItemCustomModel> selectedList;
        public ObservableCollection<ErrorItemCustomModel> SelectedList
        {
            get { return selectedList; }
            set { SetProperty(ref selectedList, value); }
        }

        private ObservableCollection<ErrorItemCustomModel> errorList;
        public ObservableCollection<ErrorItemCustomModel> ErrorList
        {
            get { return errorList; }
            set { SetProperty(ref errorList, value); }
        }

        public DelegateCommand BatchExportCommand { get; set; }
        public DelegateCommand BatchImportCommand { get; set; }
        public DelegateCommand GenerateErrorCodeListCommand { get; set; }

        bool isDeleteFinish = true;

        protected ErrorContentCustomVM(ISimDeviceEngineUI _engine, IDialogService dialogService, IDeviceEngine deviceEngine) : base(_engine)
        {
            _dialogService = dialogService;
            this.deviceEngine = deviceEngine;
            ErrorList = new ObservableCollection<ErrorItemCustomModel>();
            SelectedList = new ObservableCollection<ErrorItemCustomModel>();

            BatchExportCommand = new DelegateCommand(ExportTotalCommand);
            BatchImportCommand = new DelegateCommand(ImportTotalCommand);
            GenerateErrorCodeListCommand = new DelegateCommand(GenerateErrorCodeList);

            LoadFromCsvFile();

            // 监听全局保存事件，点击全局保存按钮时自动保存自定义报警配置CSV
            deviceEngine.SaveEvent += () => SaveToCsvFile(ErrorList);
        }

        private void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is ErrorItemCustomModel item && deviceEngine != null)
            {
                string searchKey = (e.PropertyName == nameof(item.AlarmCode)) ? item.OldAlarmCode : item.AlarmCode;
                var alarm = deviceEngine.GetVDevices<VAlarm>()
                    .FirstOrDefault(a => a.AlarmKey == searchKey);
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

        public override void AddNewItem()
        {
            string alarmCode = "";
            string alarmContent = "";
            string alarmEnglish = "";

            // 收集已有的所有报警代码，传给向导用于序号自增
            var existingCodes = ErrorList.Select(e => e.AlarmCode).Where(c => !string.IsNullOrEmpty(c)).ToList();

            dialogService.ShowAlarmConfigCustomDialog(alarmCode, alarmContent, alarmEnglish, existingCodes, r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    r.Parameters.TryGetValue<string>("AlarmCode", out var alarmCode);
                    r.Parameters.TryGetValue<string>("AlarmContent", out var alarmContent);
                    r.Parameters.TryGetValue<string>("AlarmEnglish", out var alarmEnglish);
                    r.Parameters.TryGetValue<string>("AlarmCategory", out var alarmCategory);
                    r.Parameters.TryGetValue<string>("RepairAction", out var repairAction);

                    // 判断报警代码是否已存在
                    if (!string.IsNullOrEmpty(alarmCode) && ErrorList.Any(e => e.AlarmCode == alarmCode))
                    {
                        dialogService.ShowConfirm($"报警代码 \"{alarmCode}\" 已存在，请勿重复添加！", _ => { });
                        return;
                    }

                    var newModel = new ErrorItemCustomModel(alarmCode, alarmContent, alarmEnglish, alarmCategory ?? "", repairAction ?? "")
                    {
                        AlarmCode = alarmCode,
                        AlarmContent = alarmContent,
                        AlarmEnglish = alarmEnglish,
                        AlarmCategory = alarmCategory ?? "",
                        RepairAction = repairAction ?? "",
                    };
                    newModel.PropertyChanged += Item_PropertyChanged;
                    ErrorList.Add(newModel);
                    deviceEngine?.AddVirtual(new VAlarm()
                    {
                        AlarmKey = alarmCode,
                        AlarmCN = alarmContent,
                        AlarmEn = alarmEnglish,
                        ID = Guid.NewGuid()
                    });
                }


            });
        }

        public override void RemoveItem()
        {
            if (SelectedList.Count == 0)
            {
                SimEngineUI.OnLog(Common.DataStruct.Enums.LogType.Error, "请先选择要进行删除的项");
            }

            // 删除确认
            dialogService.ShowConfirm($"确认删除{SelectedList.Count}项?", (r) =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    foreach (var item in SelectedList)
                    {
                        if (deviceEngine != null)
                        {
                            // 获取所有 VAlarm 类型的虚拟设备
                            var virtualAlarms = deviceEngine.GetVDevices<VAlarm>();

                            // 查找并删除对应的 VAlarm 设备
                            var alarmToRemove = virtualAlarms
                                .FirstOrDefault(alarm => alarm.AlarmKey == item.AlarmCode);

                            if (alarmToRemove != null)
                            {
                                try
                                {
                                    deviceEngine.ReomoveVirtual(alarmToRemove.ID);
                                }
                                catch (Exception ex)
                                {
                                    SimEngineUI.OnLog(Common.DataStruct.Enums.LogType.Warning, $"删除底层虚拟设备时遇到异常（可能被其它模块引用），但将从界面列表中移除: {ex.Message}");
                                }
                            }
                        }

                        ErrorList.Remove(item);
                    }

                    SelectedList.Clear();
                }
            });
        }

        /// <summary>
        /// UnLoaded
        /// </summary>
        private DelegateCommand<ObservableCollection<ErrorItemCustomModel>> _unLoadedCommand;
        public DelegateCommand<ObservableCollection<ErrorItemCustomModel>> UnLoadedCommand => _unLoadedCommand ?? (_unLoadedCommand = new DelegateCommand<ObservableCollection<ErrorItemCustomModel>>((items) =>
        {
            SaveToCsvFile(items);
        }));

        public ICommand SelectionChangedCommand => new DelegateCommand<IList>(selectedItems =>
        {
            if (selectedItems.Count == 0) return;
            if (selectedItems is IList items)
            {
                SelectedList.Clear();
                foreach (var item in items)
                {
                    if (item is ErrorItemCustomModel errorItem)
                    {
                        SelectedList.Add(errorItem);
                    }
                }
            }
        });

        private void SaveToCsvFile(ObservableCollection<ErrorItemCustomModel> items)
        {
            //if (items == null || items.Count == 0) return;

            try
            {
                string csvPath = Path.Combine(deviceEngine.RecipeConfigPath, "CustomErrors.csv");
                var csvLines = new List<string>();

                // 添加CSV表头
                csvLines.Add("AlarmCode,AlarmContent,AlarmEnglish,AlarmCategory,RepairAction");

                // 添加数据行
                foreach (var item in items)
                {
                    // 处理可能包含逗号的内容（用引号包围）
                    var content = EscapeCsvField(item.AlarmContent ?? "");
                    var english = EscapeCsvField(item.AlarmEnglish ?? "");
                    var category = EscapeCsvField(item.AlarmCategory ?? "");
                    var repairAction = EscapeCsvField(item.RepairAction ?? "");

                    csvLines.Add($"{item.AlarmCode},{content},{english},{category},{repairAction}");
                }

                // 写入文件
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
            string csvPath = Path.Combine(deviceEngine.RecipeConfigPath, "CustomErrors.csv");
            if (!File.Exists(csvPath))
            {
                System.Diagnostics.Debug.WriteLine("CSV文件不存在，使用空数据");
                ErrorList.Clear();
                return;
            }

            try
            {
                var lines = File.ReadAllLines(csvPath, Encoding.UTF8);

                // 清空现有数据
                ErrorList.Clear();

                // 跳过表头（第一行）
                for (int i = 1; i < lines.Length; i++)
                {
                    var line = lines[i];
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var fields = ParseCsvLine(line);
                    if (fields.Length >= 3)
                    {
                        var alarmCode = fields[0];
                        var alarmContent = UnescapeCsvField(fields[1]);
                        var alarmEnglish = UnescapeCsvField(fields[2]);
                        var alarmCategory = fields.Length > 3 ? UnescapeCsvField(fields[3]) : "";
                        var repairAction = fields.Length > 4 ? UnescapeCsvField(fields[4]) : "";

                        var newModel = new ErrorItemCustomModel(
                            alarmCode: alarmCode,
                            alarmContent: alarmContent,
                            alarmEnglish: alarmEnglish,
                            alarmCategory: alarmCategory,
                            repairAction: repairAction
                        );
                        newModel.PropertyChanged += Item_PropertyChanged;
                        ErrorList.Add(newModel);

                        // CSV中有数据但deviceEngine中没有对应的VAlarm时，自动补建
                        if (deviceEngine != null && !string.IsNullOrEmpty(alarmCode))
                        {
                            var existing = deviceEngine.GetVDevices<VAlarm>()
                                ?.FirstOrDefault(a => a.AlarmKey == alarmCode);
                            if (existing == null)
                            {
                                deviceEngine.AddVirtual(new VAlarm()
                                {
                                    AlarmKey = alarmCode,
                                    AlarmCN = alarmContent,
                                    AlarmEn = alarmEnglish,
                                    ID = Guid.NewGuid()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"读取CSV文件失败: {ex.Message}");
                ErrorList.Clear();
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
        /// <param name="items">要导出的项列表</param>
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
                    csvLines.Add("AlarmCode,AlarmContent,AlarmEnglish,AlarmCategory,RepairAction");

                    // 添加数据行
                    foreach (var item in items)
                    {
                        var content = EscapeCsvField(item.AlarmContent ?? "");
                        var english = EscapeCsvField(item.AlarmEnglish ?? "");
                        var category = EscapeCsvField(item.AlarmCategory ?? "");
                        var repairAction = EscapeCsvField(item.RepairAction ?? "");

                        csvLines.Add($"{item.AlarmCode},{content},{english},{category},{repairAction}");
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
        /// <param name="items">要导入的项列表</param>
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
                    // 检查是否已存在相同的AlarmCode
                    var existingItem = ErrorList.FirstOrDefault(e => e.AlarmCode == importedItem.AlarmCode);

                    if (existingItem == null)
                    {
                        // 添加新项
                        var newModel = new ErrorItemCustomModel(
                            importedItem.AlarmCode,
                            importedItem.AlarmContent,
                            importedItem.AlarmEnglish,
                            importedItem.AlarmCategory,
                            importedItem.RepairAction
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
                        updatedCount++;
                    }
                }

                // 同步到设备引擎
                foreach (var item in items)
                {
                    deviceEngine?.AddVirtual(new VAlarm()
                    {
                        AlarmKey = item.AlarmCode,
                        AlarmCN = item.AlarmContent,
                        AlarmEn = item.AlarmEnglish,
                        ID = Guid.NewGuid()
                    });
                }

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
                                repairAction: fields.Length > 4 ? UnescapeCsvField(fields[4]) : ""
                            ));
                        }
                        else
                        {
                            SimEngineUI.OnLog(Common.DataStruct.Enums.LogType.Warning, $"跳过第 {i + 1} 行：字段数量不足");
                        }
                    }

                    if (importedItems.Count > 0)
                    {
                        // 移除无差别删除全部报警的逻辑 (会导致 TaskFlow 中的报警工具断开连接并清空配置)
                        CleanupUnusedVAlarms(importedItems);
                        
                        // 先清空绑定的列表
                        ErrorList.Clear();
                        
                        // 执行导入（自动更新底层的对应报警，而不删除有效的）
                        BatchImport(importedItems);
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
        /// 删除设备引擎中所有 VAlarm 设备
        /// </summary>
        private void RemoveAllVAlarms()
        {
            if (deviceEngine == null) return;

            try
            {
                // 获取所有 VAlarm 类型的虚拟设备
                var allVAlarms = deviceEngine.GetVDevices<VAlarm>();
                int totalCount = allVAlarms.Count;

                if (totalCount > 0)
                {
                    // 记录要删除的所有设备
                    var alarmCodes = allVAlarms.Select(a => a.AlarmKey).ToList();

                    // 删除所有 VAlarm 设备
                    deviceEngine.ReomoveVirtual(typeof(VAlarm));
                }
            }
            catch (Exception ex)
            {
                SimEngineUI.OnLog(Common.DataStruct.Enums.LogType.Error,
                    $"删除所有 VAlarm 设备失败: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// 同步清理设备引擎中未使用的 VAlarm 设备 (按需调用)
        /// </summary>
        private void CleanupUnusedVAlarms(List<ErrorItemCustomModel> validItems)
        {
            if (deviceEngine == null) return;
            var importedCodes = validItems.Select(x => x.AlarmCode).ToHashSet();
            var allVAlarms = deviceEngine.GetVDevices<VAlarm>().ToList();
            
            foreach (var alarm in allVAlarms)
            {
                if (!importedCodes.Contains(alarm.AlarmKey))
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
        /// 数据来源：
        ///   1. 预定义默认行
        ///   2. 当前配方目录下的 CustomErrors.csv
        ///   3. 当前配方目录下的 Hardware.dproj
        /// 输出：D:/Hive/xxx LUSTER ERROR LIST.csv
        /// </summary>
        private void GenerateErrorCodeList()
        {
            dialogService.ShowConfirm("请确保已保存当前配方，否则生成的 ErrorCodeList 可能不包含最新修改。\n\n是否继续生成？", (r) =>
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

                // 2. 读取并添加 CustomErrors.csv
                if (File.Exists(customErrorsPath))
                {
                    try
                    {
                        var customLines = File.ReadAllLines(customErrorsPath, System.Text.Encoding.UTF8);
                        bool isFirstLine = true;
                        foreach (var line in customLines)
                        {
                            if (isFirstLine)
                            {
                                isFirstLine = false;
                                continue;
                            }
                            if (string.IsNullOrWhiteSpace(line)) continue;

                            var parts = line.Split(',');
                            if (parts.Length >= 5)
                            {
                                rows.Add(new string[] { parts[0], parts[1], parts[2], parts[3], parts[4], parts[1] });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        SimEngineUI.OnLog(Common.DataStruct.Enums.LogType.Warning, $"读取 CustomErrors.csv 失败: {ex.Message}");
                    }
                }

                // 3. 读取 Hardware.dproj
                if (File.Exists(dprojPath))
                {
                    try
                    {
                        var xDoc = System.Xml.Linq.XElement.Load(dprojPath);
                        foreach (var vDevice in xDoc.Descendants("VDevice"))
                        {
                            var nameNode = vDevice.Element("Name");
                            string deviceName = nameNode?.Value ?? "";

                            // 获取设备级的 AlarmCategory 和 RepairAction
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

                                    // 翻译
                                    string translatedSuffix = ErrorTransMap.ContainsKey(tagName) ? ErrorTransMap[tagName] : tagName;
                                    string errDescCn = $"{deviceName}{translatedSuffix}";

                                    // 按 '@' 切分
                                    string codePart, descPart;
                                    if (content.Contains("@"))
                                    {
                                        var parts = content.Split(new[] { '@' }, 2);
                                        codePart = parts[0];
                                        descPart = parts[1];
                                    }
                                    else
                                    {
                                        codePart = content;
                                        descPart = "";
                                    }

                                    //// 过滤 10000
                                    //if (codePart == "10000") continue;

                                    rows.Add(new[] { descPart, codePart, alarmCategory, errDescCn, repairAction, codePart });
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        SimEngineUI.OnLog(Common.DataStruct.Enums.LogType.Warning, $"解析 Hardware.dproj 时遇到异常: {ex.Message}");
                    }
                }

                // 4. 生成目标 CSV
                using (var sw = new StreamWriter(outputPath, false, new System.Text.UTF8Encoding(true)))
                {
                    sw.WriteLine(string.Join(",", headers));
                    foreach (var row in rows)
                    {
                        //if (row[1] == "TBD") continue; // 过滤 TBD 错误

                        // 简单的 CSV 格式化, 处理包含逗号的字段
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
                dialogService.ShowConfirm($"ErrorCodeList 生成成功！\n输出路径: {outputPath}", _ => { });
            }
            catch (Exception ex)
            {
                SimEngineUI.OnLog(Common.DataStruct.Enums.LogType.Error, $"生成ErrorCodeList时发生错误: {ex.Message}");
            }
        }

        private List<string[]> GetOrUpdateDefaultErrorCodeRows(string xmlPath)
        {
            var rows = new List<string[]>();
            
            // 如果不存在，使用代码内置默认行先生成一个配置并保存
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
                
                // 返回默认行
                return DefaultErrorCodeRows.ToList();
            }

            // 如果存在，从 XML 解析
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
                // 解析失败时，回退到默认
                rows = DefaultErrorCodeRows.ToList();
            }
            return rows;
        }

        #endregion
    }
}

