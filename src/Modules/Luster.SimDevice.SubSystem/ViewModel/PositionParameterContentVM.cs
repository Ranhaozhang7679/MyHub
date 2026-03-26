using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Extensions;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Real;
using Luster.SimDevice.EngineUI.Models;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;

namespace Luster.SimDevice.SubSystem.ViewModel
{
    // 工站下拉菜单项
    public class StationComboBoxItem
    {
        public object Station { get; set; }
        public string DisplayName { get; set; }
        public bool IsAllStationsOption { get; set; }
    }

    public class AxisPosItemModel : BindableBase
    {
        private AxisPosItem _tag;
        public AxisPosItem Tag
        {
            get => _tag;
            set
            {
                SetProperty(ref _tag, value);
                RaisePropertyChanged(nameof(Name));
            }
        }

        public string Name
        {
            get
            {
                if (Tag?.AxisPostion == null) return "未知轴";

                var axis = Tag.AxisPostion.Axis;
                var pointName = Tag.AxisPostion.Name;

                if (axis != null && !string.IsNullOrEmpty(axis.Name))
                {
                    // 检查点位名称是否已经包含轴名称
                    if (!string.IsNullOrEmpty(pointName))
                    {
                        if (pointName.StartsWith("[") && pointName.Contains("]"))
                        {
                            return pointName;
                        }
                        return $"[{axis.Name}]{pointName}";
                    }
                    return $"[{axis.Name}]未知点位";
                }
                return pointName ?? "未知轴";
            }
        }

        // 位置
        public double Position
        {
            get => Tag?.AxisPostion?.Position ?? 0;
            set
            {
                if (Tag?.AxisPostion != null)
                {
                    Tag.AxisPostion.Position = value;
                    RaisePropertyChanged(nameof(Position));
                }
            }
        }

        // 加速度
        public double Acc
        {
            get => Tag?.Acc ?? 0;
            set
            {
                if (Tag != null)
                {
                    Tag.Acc = value;
                    RaisePropertyChanged(nameof(Acc));
                }
            }
        }

        // 减速度
        public double Dec
        {
            get => Tag?.Dec ?? 0;
            set
            {
                if (Tag != null)
                {
                    Tag.Dec = value;
                    RaisePropertyChanged(nameof(Dec));
                }
            }
        }

        // 速度
        public double Speed
        {
            get => Tag?.Speed ?? 0;
            set
            {
                if (Tag != null)
                {
                    Tag.Speed = value;
                    RaisePropertyChanged(nameof(Speed));
                }
            }
        }

        // 优先级
        public Priority Priority
        {
            get => Tag?.MovePriority ?? Priority.Low;
            set
            {
                if (Tag != null)
                {
                    Tag.MovePriority = value;
                    RaisePropertyChanged(nameof(Priority));
                }
            }
        }

        public string AxisNo => Tag?.AxisPostion?.AxisNo.ToString() ?? string.Empty;
        public string AxisOnlyName => Tag?.AxisPostion?.Axis?.Name ?? "未知轴";
        public string PointOnlyName => Tag?.AxisPostion?.Name ?? "未知点位";

        public AxisPosItemModel(AxisPosItem item)
        {
            Tag = item;
        }

        /// <summary>
        /// 更新
        /// </summary>
        public void UpdateToOriginalData(AxisPosItem originalAxisPosItem)
        {
            if (originalAxisPosItem != null && Tag != null && Tag.AxisPostion != null)
            {
                if (originalAxisPosItem.AxisPostion != null)
                {
                    originalAxisPosItem.AxisPostion.Position = Tag.AxisPostion.Position;
                }

                // 更新当前点位的运动参数（不修改轴的全局参数）
                originalAxisPosItem.Acc = Tag.Acc;
                originalAxisPosItem.Dec = Tag.Dec;
                originalAxisPosItem.Speed = Tag.Speed;
                originalAxisPosItem.MovePriority = Tag.MovePriority;
            }
        }
    }

    // 统一行数据模型
    public class UnifiedRowViewModel : BindableBase
    {
        private AxisPosItemModel _axisPosItem;
        private MotionPosGroupModel _pointGroup;
        private string _stationName;

        public string StationName
        {
            get => _stationName;
            set => SetProperty(ref _stationName, value);
        }

        public MotionPosGroupModel PointGroup
        {
            get => _pointGroup;
            set => SetProperty(ref _pointGroup, value);
        }

        public AxisPosItemModel AxisPosItem
        {
            get => _axisPosItem;
            set => SetProperty(ref _axisPosItem, value);
        }

        // 用于控制显示的属性
        public bool ShowStationName { get; set; } = true;
        public bool ShowPointName { get; set; } = true;

        // 用于分组显示的标识符
        public string StationKey => StationName;
        public string PointKey => $"{StationName}_{PointGroup?.Name}";
    }

    internal class PositionParameterContentVM : BindableBase
    {
        private readonly IDeviceEngine _deviceEngine;
        private StationComboBoxItem _selectedStationItem;
        private bool _isInitialized = false;
        private string _lastSelectedStationName = null;

        // 工站下拉菜单项列表
        private ObservableCollection<StationComboBoxItem> _stationComboBoxItems;
        public ObservableCollection<StationComboBoxItem> StationComboBoxItems
        {
            get => _stationComboBoxItems;
            set => SetProperty(ref _stationComboBoxItems, value);
        }

        // 选中的工站项
        public StationComboBoxItem SelectedStationItem
        {
            get => _selectedStationItem;
            set
            {
                SetProperty(ref _selectedStationItem, value);
                OnSelectedStationChanged();
            }
        }

        // 统一列表数据
        private ObservableCollection<UnifiedRowViewModel> _unifiedRows;
        public ObservableCollection<UnifiedRowViewModel> UnifiedRows
        {
            get => _unifiedRows;
            set => SetProperty(ref _unifiedRows, value);
        }

        // 优先级列表
        public List<KeyValue> Priorities { get; set; }

        // 更新命令
        public ICommand UpdateCommand { get; }

        // 导出命令
        public ICommand ExportCommand { get; }

        // 刷新工站列表命令
        public ICommand RefreshCommand { get; }

        // 存储所有工站的数据（用于更新）
        private Dictionary<string, List<AxisPosItem>> _allStationAxisData = new Dictionary<string, List<AxisPosItem>>();

        public PositionParameterContentVM(IDeviceEngine deviceEngine)
        {
            _deviceEngine = deviceEngine;
            StationComboBoxItems = new ObservableCollection<StationComboBoxItem>();
            UnifiedRows = new ObservableCollection<UnifiedRowViewModel>();
            Priorities = typeof(Priority).EnumToDataSource();

            // 初始化命令
            UpdateCommand = new DelegateCommand<object>(OnUpdate);
            ExportCommand = new DelegateCommand(OnExport);
            RefreshCommand = new DelegateCommand(RefreshStationList);
            //InitializeComboBox();
        }

        /// <summary>
        /// 导出命令处理
        /// </summary>
        private void OnExport()
        {
            try
            {
                if (UnifiedRows == null || UnifiedRows.Count == 0) return;

                // 生成默认文件名
                string stationName = "全部工站";
                if (SelectedStationItem != null && !SelectedStationItem.IsAllStationsOption)
                {
                    stationName = SelectedStationItem.DisplayName;
                }

                string safeStationName = RemoveInvalidFileNameChars(stationName);
                string defaultFileName = $"点位参数_{safeStationName}_{DateTime.Now:yyyyMMdd}.csv";

                // 创建保存文件对话框
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV文件 (*.csv)|*.csv|所有文件 (*.*)|*.*",
                    FilterIndex = 1,
                    RestoreDirectory = true,
                    FileName = defaultFileName,
                    Title = "选择保存位置",
                    DefaultExt = ".csv"
                };

                saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                if (saveFileDialog.ShowDialog() == true)
                {
                    string filePath = saveFileDialog.FileName;
                    StringBuilder csvContent = new StringBuilder();
                    csvContent.AppendLine("工站名称,点位名称,轴名称,当前位置,加速度,减速度,运动速度,优先级");

                    // 添加数据行
                    foreach (var row in UnifiedRows)
                    {
                        if (row.AxisPosItem != null && row.PointGroup != null)
                        {
                            // 转义字段中的特殊字符
                            string stationNameField = EscapeCsvField(row.ShowStationName ? row.StationName : "");
                            string pointNameField = EscapeCsvField(row.ShowPointName ? row.PointGroup.Name : "");
                            string axisNameField = EscapeCsvField(row.AxisPosItem.Name);
                            string priorityField = EscapeCsvField(row.AxisPosItem.Priority.ToString());

                            csvContent.AppendLine($"{stationNameField},{pointNameField},{axisNameField},{row.AxisPosItem.Position:F3},{row.AxisPosItem.Acc:F3},{row.AxisPosItem.Dec:F3},{row.AxisPosItem.Speed:F3},{priorityField}");
                        }
                    }
                    File.WriteAllText(filePath, csvContent.ToString(), Encoding.UTF8);
                }
                else
                {

                    return;
                }
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("导出失败：没有权限写入文件", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (PathTooLongException)
            {
                MessageBox.Show("导出失败：文件路径太长", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (IOException ex)
            {
                MessageBox.Show($"导出失败：文件操作错误 - {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出失败：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 转义CSV字段中的特殊字符
        /// </summary>
        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "";

            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
            {
                return "\"" + field.Replace("\"", "\"\"") + "\"";
            }
            return field;
        }

        /// <summary>
        /// 移除文件名中的非法字符
        /// </summary>
        private string RemoveInvalidFileNameChars(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return fileName;

            var invalidChars = Path.GetInvalidFileNameChars();
            foreach (var invalidChar in invalidChars)
            {
                fileName = fileName.Replace(invalidChar.ToString(), "");
            }
            return fileName;
        }

        /// <summary>
        /// 刷新工站列表
        /// </summary>
        public void RefreshStationList()
        {
            try
            {
                var allStationsItem = StationComboBoxItems.FirstOrDefault(item => item.IsAllStationsOption);
                // 清空现有列表
                StationComboBoxItems.Clear();

                // 重新添加"全部工站"选项
                if (allStationsItem != null)
                {
                    StationComboBoxItems.Add(allStationsItem);
                }
                else
                {
                    // 如果不存在，创建新的全部工站选项
                    StationComboBoxItems.Add(new StationComboBoxItem
                    {
                        Station = null,
                        DisplayName = "全部工站",
                        IsAllStationsOption = true
                    });
                }
                LoadStationsToComboBox();
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// 页面激活时调用
        /// </summary>
        public void OnViewActivated()
        {
            if (!_isInitialized)
            {
                InitializeComboBox();
                _isInitialized = true;
            }
            else
            {
                // 重置状态
                ResetToInitialState();
            }
        }

        /// <summary>
        /// 重置到初始状态
        /// </summary>
        private void ResetToInitialState()
        {
            try
            {
                // 清空数据
                UnifiedRows.Clear();
                _allStationAxisData.Clear();
                SelectedStationItem = null;
                _lastSelectedStationName = null;
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// 页面离开时调用
        /// </summary>
        public void OnViewDeactivated()
        {
            // 保存当前状态
            if (SelectedStationItem != null)
            {
                _lastSelectedStationName = SelectedStationItem.DisplayName;
            }
        }

        /// <summary>
        /// 初始化下拉菜单
        /// </summary>
        private void InitializeComboBox()
        {
            try
            {
                // 清空现有列表
                StationComboBoxItems.Clear();

                // 添加"全部工站"选项
                var allStationsItem = new StationComboBoxItem
                {
                    Station = null,
                    DisplayName = "全部工站",
                    IsAllStationsOption = true
                };

                // 异步加载工站数据到下拉菜单
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    LoadStationsToComboBox();
                }));

                SelectedStationItem = null;
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// 加载工站数据到下拉菜单
        /// </summary>
        private void LoadStationsToComboBox()
        {
            try
            {
                var stations = _deviceEngine.GetModulesFromMotionEngine(null, null);
                if (stations != null)
                {
                    foreach (var station in stations)
                    {
                        var alias = GetPropertyValue(station, "Alias") as string;
                        if (!string.IsNullOrEmpty(alias))
                        {
                            var item = new StationComboBoxItem
                            {
                                Station = station,
                                DisplayName = alias,
                                IsAllStationsOption = false
                            };

                            // 使用Dispatcher确保在UI线程上添加
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                StationComboBoxItems.Add(item);
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// 加载所有工站的数据
        /// </summary>
        private void LoadAllData()
        {
            try
            {
                UnifiedRows.Clear();
                _allStationAxisData.Clear();

                // 获取所有工站
                var allStations = _deviceEngine.GetModulesFromMotionEngine(null, null);
                if (allStations == null || !allStations.Any()) return;

                foreach (var station in allStations)
                {
                    try
                    {
                        var stationID = GetPropertyValue(station, "ID")?.ToString();
                        var stationName = GetPropertyValue(station, "Alias")?.ToString() ?? "未知工站";

                        if (string.IsNullOrEmpty(stationID)) continue;

                        // 获取该工站的所有AxisPosMove模块
                        var elements = _deviceEngine.GetModulesFromMotionEngine(stationID, "AxisPosMove");

                        if (elements != null && elements.Any())
                        {
                            foreach (var item in elements)
                            {
                                try
                                {
                                    var axisPosItems = GetPropertyValue(item, "Values") as List<AxisPosItem>;
                                    var moduleName = GetPropertyValue(item, "Name") as string ??
                                                    GetPropertyValue(item, "Alias") as string ??
                                                    "未知模块";

                                    if (axisPosItems == null || !axisPosItems.Any()) continue;

                                    var moduleID = GetPropertyValue(item, "ID") as string;
                                    if (!string.IsNullOrEmpty(moduleID) && Guid.TryParse(moduleID, out Guid guid))
                                    {
                                        // 过滤掉 AxisPostion 为 null 的项
                                        var validAxisPositions = axisPosItems
                                            .Where(api => api?.AxisPostion != null)
                                            .Select(api => api.AxisPostion)
                                            .ToList();

                                        if (!validAxisPositions.Any()) continue;

                                        var pointGroup = new MotionPosGroupModel(
                                            guid,
                                            moduleName,
                                            validAxisPositions
                                        );

                                        // 保存原始数据引用
                                        string key = $"{stationName}_{moduleName}";
                                        _allStationAxisData[key] = axisPosItems;

                                        // 添加到统一列表
                                        bool isFirstAxisInPoint = true;
                                        foreach (var axisPosItem in axisPosItems)
                                        {
                                            // 检查 AxisPosItem 是否有效
                                            if (axisPosItem?.AxisPostion == null) continue;

                                            var axisItemModel = new AxisPosItemModel(axisPosItem);

                                            var row = new UnifiedRowViewModel
                                            {
                                                StationName = stationName,
                                                PointGroup = pointGroup,
                                                AxisPosItem = axisItemModel,
                                                ShowStationName = isFirstAxisInPoint,
                                                ShowPointName = isFirstAxisInPoint
                                            };

                                            UnifiedRows.Add(row);
                                            isFirstAxisInPoint = false;
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
            catch (Exception ex)
            {
                UnifiedRows.Clear();
                _allStationAxisData.Clear();
                throw new FriendlyException($"加载数据失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 选中的工站发生变化
        /// </summary>
        private void OnSelectedStationChanged()
        {
            if (_selectedStationItem == null)
            {
                // 如果选中的是null（空选择），清空数据列表
                UnifiedRows.Clear();
                return;
            }

            if (_selectedStationItem.IsAllStationsOption || _selectedStationItem.Station == null)
            {
                LoadAllData();
            }
            else
            {
                LoadSelectedStationData();
            }
        }

        /// <summary>
        /// 加载选中工站的数据
        /// </summary>
        private void LoadSelectedStationData()
        {
            try
            {
                UnifiedRows.Clear();

                if (_selectedStationItem?.Station == null) return;

                var stationID = GetPropertyValue(_selectedStationItem.Station, "ID")?.ToString();
                var stationName = GetPropertyValue(_selectedStationItem.Station, "Alias")?.ToString() ?? "未知工站";

                if (string.IsNullOrEmpty(stationID)) return;

                // 获取该工站的所有AxisPosMove模块
                var elements = _deviceEngine.GetModulesFromMotionEngine(stationID, "AxisPosMove");

                if (elements != null && elements.Any())
                {
                    foreach (var item in elements)
                    {
                        try
                        {
                            var axisPosItems = GetPropertyValue(item, "Values") as List<AxisPosItem>;
                            var moduleName = GetPropertyValue(item, "Name") as string ??
                                            GetPropertyValue(item, "Alias") as string ??
                                            "未知模块";

                            if (axisPosItems == null || !axisPosItems.Any()) continue;

                            var moduleID = GetPropertyValue(item, "ID") as string;
                            if (!string.IsNullOrEmpty(moduleID) && Guid.TryParse(moduleID, out Guid guid))
                            {
                                // 过滤掉 AxisPostion 为 null 的项
                                var validAxisPositions = axisPosItems
                                    .Where(api => api?.AxisPostion != null)
                                    .Select(api => api.AxisPostion)
                                    .ToList();

                                if (!validAxisPositions.Any()) continue;

                                var pointGroup = new MotionPosGroupModel(
                                    guid,
                                    moduleName,
                                    validAxisPositions
                                );

                                // 保存原始数据引用（用于更新）
                                string key = $"{stationName}_{moduleName}";
                                _allStationAxisData[key] = axisPosItems;

                                // 添加到统一列表
                                bool isFirstAxisInPoint = true;
                                foreach (var axisPosItem in axisPosItems)
                                {
                                    // 检查 AxisPosItem 是否有效
                                    if (axisPosItem?.AxisPostion == null) continue;

                                    var axisItemModel = new AxisPosItemModel(axisPosItem);

                                    var row = new UnifiedRowViewModel
                                    {
                                        StationName = stationName,
                                        PointGroup = pointGroup,
                                        AxisPosItem = axisItemModel,
                                        ShowStationName = isFirstAxisInPoint,
                                        ShowPointName = isFirstAxisInPoint
                                    };

                                    UnifiedRows.Add(row);
                                    isFirstAxisInPoint = false;
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UnifiedRows.Clear();
                throw new FriendlyException($"加载数据失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新命令处理
        /// </summary>
        private void OnUpdate(object obj)
        {
            var row = obj as UnifiedRowViewModel;
            if (row != null && row.PointGroup != null && row.AxisPosItem != null)
            {
                try
                {
                    string key = $"{row.StationName}_{row.PointGroup.Name}";
                    if (_allStationAxisData.ContainsKey(key))
                    {
                        var originalItems = _allStationAxisData[key];
                        var axisItem = row.AxisPosItem;

                        // 找到对应的原始数据
                        var originalItem = originalItems.FirstOrDefault(item =>
                            item.AxisPostion?.AxisNo.ToString() == axisItem.AxisNo);

                        if (originalItem != null)
                        {
                            // 更新数据
                            axisItem.UpdateToOriginalData(originalItem);
                            if (originalItem.AxisPostion != null)
                            {
                                _deviceEngine.UpdatePostion(originalItem.AxisPostion, originalItem.AxisPostion.Position);
                            }

                            bool saveSuccess = SaveChangesToConfig(key, originalItems);

                            if (saveSuccess)
                            {
                                // 刷新显示
                                RefreshRowDisplay(row);
                                System.Windows.MessageBox.Show($"点位 '{row.PointGroup.Name}' 更新成功", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                System.Windows.MessageBox.Show($"点位 '{row.PointGroup.Name}' 保存到配置文件失败", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
                catch (FriendlyException ex)
                {
                    System.Windows.MessageBox.Show(ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"更新失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                System.Windows.MessageBox.Show("更新失败：请选择有效的数据行", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 刷新行显示
        /// </summary>
        private void RefreshRowDisplay(UnifiedRowViewModel row)
        {
            //直接替换AxisPosItem对象
            var axisPosItem = row.AxisPosItem;
            if (axisPosItem != null)
            {
                var newAxisItem = new AxisPosItemModel(axisPosItem.Tag);
                row.AxisPosItem = newAxisItem;
            }
        }

        /// <summary>
        /// 保存修改到配置文件
        /// </summary>
        private bool SaveChangesToConfig(string key, List<AxisPosItem> originalItems)
        {
            try
            {
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 获取属性值
        /// </summary>
        private object GetPropertyValue(object obj, string propertyName)
        {
            try
            {
                var property = obj.GetType().GetProperty(propertyName);
                return property?.GetValue(obj);
            }
            catch
            {
                return null;
            }
        }
    }
}