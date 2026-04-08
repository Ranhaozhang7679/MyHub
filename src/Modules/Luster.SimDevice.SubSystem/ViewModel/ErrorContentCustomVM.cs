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

        bool isDeleteFinish = true;

        protected ErrorContentCustomVM(ISimDeviceEngineUI _engine, IDialogService dialogService, IDeviceEngine deviceEngine) : base(_engine)
        {
            _dialogService = dialogService;
            this.deviceEngine = deviceEngine;
            ErrorList = new ObservableCollection<ErrorItemCustomModel>();
            SelectedList = new ObservableCollection<ErrorItemCustomModel>();

            BatchExportCommand = new DelegateCommand(ExportTotalCommand);
            BatchImportCommand = new DelegateCommand(ImportTotalCommand);

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
    }
}

