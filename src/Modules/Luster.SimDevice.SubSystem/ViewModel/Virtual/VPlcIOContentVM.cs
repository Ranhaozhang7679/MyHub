using Luster.Common.Assets;
using Luster.Common.Tools;
using Luster.Motion.CommonUI;
using Luster.Motion.DataStruct.VDevice;
using Luster.SimDevice.EngineUI;
using Luster.SimDevice.EngineUI.Models;
using Microsoft.Win32;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Luster.SimDevice.SubSystem.ViewModel.Virtual
{
    public class VPlcIOContentVM : PageVM
    {
        private readonly Dispatcher _dispatcher;
        private readonly ICommonBus _commonBus;

        #region 仿真通信设备

        private ObservableCollection<VCommuncation> _commDevices;
        public ObservableCollection<VCommuncation> CommDevices
        {
            get => _commDevices;
            set => SetProperty(ref _commDevices, value);
        }

        private VCommuncation _currentComm;
        public VCommuncation CurrentComm
        {
            get => _currentComm;
            set => SetProperty(ref _currentComm, value);
        }

        #endregion

        #region Tab 管理

        private ObservableCollection<PlcIOTabData> _tabs;
        public ObservableCollection<PlcIOTabData> Tabs
        {
            get => _tabs;
            set => SetProperty(ref _tabs, value);
        }

        private PlcIOTabData _selectedTab;
        public PlcIOTabData SelectedTab
        {
            get => _selectedTab;
            set
            {
                if (SetProperty(ref _selectedTab, value))
                {
                    bool wasMonitoring = IsMonitorOn;
                    if (wasMonitoring) StopMonitor();

                    InputPageIndex = 1;
                    OutputPageIndex = 1;
                    RefreshPages();

                    if (wasMonitoring) StartMonitor();
                }
            }
        }

        #endregion

        #region 数据集合

        private ObservableCollection<PlcIOModel> _inputDatas;
        public ObservableCollection<PlcIOModel> InputDatas
        {
            get => _inputDatas;
            set => SetProperty(ref _inputDatas, value);
        }

        private ObservableCollection<PlcIOModel> _outputDatas;
        public ObservableCollection<PlcIOModel> OutputDatas
        {
            get => _outputDatas;
            set => SetProperty(ref _outputDatas, value);
        }

        #endregion

        #region 监控控制

        private int _frequency = 500;
        public int Frequency
        {
            get => _frequency;
            set => SetProperty(ref _frequency, value);
        }

        private bool _isMonitorOn;
        public bool IsMonitorOn
        {
            get => _isMonitorOn;
            set
            {
                SetProperty(ref _isMonitorOn, value);
                if (value)
                    StartMonitor();
                else
                    StopMonitor();
            }
        }

        private bool _isConnected;
        public bool IsConnected
        {
            get => _isConnected;
            set => SetProperty(ref _isConnected, value);
        }

        #endregion

        #region 输入分页

        private int _inputPerPageCount = 20;
        public int InputPerPageCount
        {
            get => _inputPerPageCount;
            set => SetProperty(ref _inputPerPageCount, value);
        }

        private int _inputPageCount;
        public int InputPageCount
        {
            get => _inputPageCount;
            set => SetProperty(ref _inputPageCount, value);
        }

        private int _inputPageIndex = 1;
        public int InputPageIndex
        {
            get => _inputPageIndex;
            set => SetProperty(ref _inputPageIndex, value);
        }

        #endregion

        #region 输出分页

        private int _outputPerPageCount = 20;
        public int OutputPerPageCount
        {
            get => _outputPerPageCount;
            set => SetProperty(ref _outputPerPageCount, value);
        }

        private int _outputPageCount;
        public int OutputPageCount
        {
            get => _outputPageCount;
            set => SetProperty(ref _outputPageCount, value);
        }

        private int _outputPageIndex = 1;
        public int OutputPageIndex
        {
            get => _outputPageIndex;
            set => SetProperty(ref _outputPageIndex, value);
        }

        #endregion

        #region 选中项

        private PlcIOModel _selectedInput;
        public PlcIOModel SelectedInput
        {
            get => _selectedInput;
            set => SetProperty(ref _selectedInput, value);
        }

        private PlcIOModel _selectedOutput;
        public PlcIOModel SelectedOutput
        {
            get => _selectedOutput;
            set => SetProperty(ref _selectedOutput, value);
        }

        #endregion

        #region 命令

        public DelegateCommand CommSelectionChangedCommand { get; set; }
        public DelegateCommand ConnectCommand { get; set; }
        public DelegateCommand DisconnectCommand { get; set; }
        public DelegateCommand BatchImportCommand { get; set; }
        public DelegateCommand BatchExportCommand { get; set; }
        public DelegateCommand ExportTemplateCommand { get; set; }
        public DelegateCommand<string> PageUpdatedCommand { get; set; }
        public DelegateCommand AddInputCommand { get; set; }
        public DelegateCommand AddOutputCommand { get; set; }
        public DelegateCommand DeleteInputCommand { get; set; }
        public DelegateCommand DeleteOutputCommand { get; set; }
        public DelegateCommand<PlcIOModel> WriteOutputCommand { get; set; }
        public DelegateCommand<PlcIOModel> ToggleOutputCommand { get; set; }
        public DelegateCommand CellEditFinishedCommand { get; set; }
        public DelegateCommand AddTabCommand { get; set; }
        public DelegateCommand RemoveTabCommand { get; set; }
        public DelegateCommand RenameTabCommand { get; set; }

        #endregion

        private List<PlcIOModel> _allInputs = new List<PlcIOModel>();
        private List<PlcIOModel> _allOutputs = new List<PlcIOModel>();

        private CancellationTokenSource _monitorToken;
        private bool _isInitialized;
        private string _lastReadError;
        private int _cycleReadErrors;

        public VPlcIOContentVM(ISimDeviceEngineUI _engine, Dispatcher dispatcher, ICommonBus commonBus) : base(_engine)
        {
            _dispatcher = dispatcher;
            _commonBus = commonBus;

            CommSelectionChangedCommand = new DelegateCommand(OnCommSelectionChanged);
            ConnectCommand = new DelegateCommand(OnConnect);
            DisconnectCommand = new DelegateCommand(OnDisconnect);
            BatchImportCommand = new DelegateCommand(OnBatchImport);
            BatchExportCommand = new DelegateCommand(OnBatchExport);
            ExportTemplateCommand = new DelegateCommand(OnExportTemplate);
            PageUpdatedCommand = new DelegateCommand<string>(OnPageUpdated);
            AddInputCommand = new DelegateCommand(OnAddInput);
            AddOutputCommand = new DelegateCommand(OnAddOutput);
            DeleteInputCommand = new DelegateCommand(OnDeleteInput);
            DeleteOutputCommand = new DelegateCommand(OnDeleteOutput);
            WriteOutputCommand = new DelegateCommand<PlcIOModel>(OnWriteOutput);
            ToggleOutputCommand = new DelegateCommand<PlcIOModel>(OnToggleOutput);
            CellEditFinishedCommand = new DelegateCommand(OnCellEditFinished);
            AddTabCommand = new DelegateCommand(OnAddTab);
            RemoveTabCommand = new DelegateCommand(OnRemoveTab);
            RenameTabCommand = new DelegateCommand(OnRenameTab);

            InputDatas = new ObservableCollection<PlcIOModel>();
            OutputDatas = new ObservableCollection<PlcIOModel>();
            Tabs = new ObservableCollection<PlcIOTabData>();
        }

        public override bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
            _isInitialized = false;
            LoadCommDevices();

            if (Tabs.Count == 0)
            {
                LoadFromFile();
            }
            _isInitialized = true;
        }

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            base.OnNavigatedFrom(navigationContext);
            StopMonitor();
        }

        public override void Leave()
        {
            base.Leave();
            StopMonitor();
        }

        #region 持久化

        private string GetConfigFilePath()
        {
            var recipePath = _commonBus?.CurrentRecipe?.GetRecipePath();
            if (string.IsNullOrEmpty(recipePath)) return null;

            var configDir = Path.Combine(recipePath, "Config");
            if (!Directory.Exists(configDir))
            {
                Directory.CreateDirectory(configDir);
            }
            return Path.Combine(configDir, "PlcIOConfig.json");
        }

        private void LoadFromFile()
        {
            try
            {
                var configPath = GetConfigFilePath();
                if (configPath == null || !File.Exists(configPath)) return;

                var json = File.ReadAllText(configPath);
                if (string.IsNullOrWhiteSpace(json)) return;

                string savedCommName = null;

                // 尝试最新格式（PlcIOConfigDto 包装结构）
                var config = JsonConvert.DeserializeObject<PlcIOConfigDto>(json);
                if (config?.Tabs != null)
                {
                    savedCommName = config.SelectedCommName;
                    Tabs.Clear();
                    foreach (var tab in config.Tabs)
                    {
                        var tabData = new PlcIOTabData { TabName = tab.TabName };
                        if (tab.Addresses != null)
                        {
                            foreach (var addr in tab.Addresses)
                            {
                                tabData.Addresses.Add(new PlcIOModel(addr.Address, addr.Name, addr.IsOutput));
                            }
                        }
                        Tabs.Add(tabData);
                    }
                }
                else
                {
                    // 兼容旧格式1：Tab 数组
                    var tabDtos = JsonConvert.DeserializeObject<List<PlcIOTabDto>>(json);
                    if (tabDtos?.Count > 0 && tabDtos[0].Addresses != null)
                    {
                        Tabs.Clear();
                        foreach (var tab in tabDtos)
                        {
                            var tabData = new PlcIOTabData { TabName = tab.TabName };
                            if (tab.Addresses != null)
                            {
                                foreach (var addr in tab.Addresses)
                                {
                                    tabData.Addresses.Add(new PlcIOModel(addr.Address, addr.Name, addr.IsOutput));
                                }
                            }
                            Tabs.Add(tabData);
                        }
                    }
                    else
                    {
                        // 兼容旧格式2：平面地址列表
                        var oldItems = JsonConvert.DeserializeObject<List<PlcIODto>>(json);
                        var tabData = new PlcIOTabData { TabName = "默认" };
                        if (oldItems != null)
                        {
                            foreach (var dto in oldItems)
                            {
                                tabData.Addresses.Add(new PlcIOModel(dto.Address, dto.Name, dto.IsOutput));
                            }
                        }
                        Tabs.Clear();
                        Tabs.Add(tabData);
                    }
                }

                SelectedTab = Tabs.FirstOrDefault();

                // 恢复通信设备选择
                if (!string.IsNullOrEmpty(savedCommName) && CommDevices != null)
                {
                    var savedComm = CommDevices.FirstOrDefault(d => d.Name == savedCommName);
                    if (savedComm != null)
                    {
                        CurrentComm = savedComm;
                        IsConnected = CurrentComm.Communication?.IsConnected ?? false;
                    }
                }
            }
            catch { }
        }

        private void SaveToFile()
        {
            try
            {
                var configPath = GetConfigFilePath();
                if (configPath == null) return;

                var config = new PlcIOConfigDto
                {
                    SelectedCommName = CurrentComm?.Name,
                    Tabs = Tabs.Select(t => new PlcIOTabDto
                    {
                        TabName = t.TabName,
                        Addresses = t.Addresses.Select(a => new PlcIODto
                        {
                            Address = a.Address,
                            Name = a.Name,
                            IsOutput = a.IsOutput
                        }).ToList()
                    }).ToList()
                };

                var json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(configPath, json);
            }
            catch { }
        }

        private class PlcIOConfigDto
        {
            public string SelectedCommName { get; set; }
            public List<PlcIOTabDto> Tabs { get; set; }
        }

        private class PlcIODto
        {
            public string Address { get; set; }
            public string Name { get; set; }
            public bool IsOutput { get; set; }
        }

        private class PlcIOTabDto
        {
            public string TabName { get; set; }
            public List<PlcIODto> Addresses { get; set; }
        }

        #endregion

        #region 加载设备

        private void LoadCommDevices()
        {
            var devices = deviceEngine.GetVDevices<VCommuncation>();
            CommDevices = new ObservableCollection<VCommuncation>(devices);

            if (CurrentComm == null || !CommDevices.Contains(CurrentComm))
            {
                CurrentComm = CommDevices.FirstOrDefault();
            }

            IsConnected = CurrentComm?.Communication?.IsConnected ?? false;
        }

        private void RefreshPages()
        {
            _allInputs.Clear();
            _allOutputs.Clear();

            if (SelectedTab == null) return;

            int inIdx = 1, outIdx = 1;
            foreach (var addr in SelectedTab.Addresses)
            {
                if (addr.IsOutput)
                {
                    addr.Index = outIdx++;
                    _allOutputs.Add(addr);
                }
                else
                {
                    addr.Index = inIdx++;
                    _allInputs.Add(addr);
                }
            }

            UpdateInputPage();
            UpdateOutputPage();
        }

        #endregion

        #region 分页

        private void UpdateInputPage()
        {
            _dispatcher.BeginInvoke(DispatcherPriority.Render, () =>
            {
                InputDatas?.Clear();
                var count = _allInputs.Count;
                InputPageCount = count / InputPerPageCount + (count % InputPerPageCount == 0 ? 0 : 1);
                if (InputPageCount == 0) InputPageCount = 1;

                foreach (var item in _allInputs.Skip((InputPageIndex - 1) * InputPerPageCount).Take(InputPerPageCount))
                {
                    InputDatas.Add(item);
                }
            });
        }

        private void UpdateOutputPage()
        {
            _dispatcher.BeginInvoke(DispatcherPriority.Render, () =>
            {
                OutputDatas?.Clear();
                var count = _allOutputs.Count;
                OutputPageCount = count / OutputPerPageCount + (count % OutputPerPageCount == 0 ? 0 : 1);
                if (OutputPageCount == 0) OutputPageCount = 1;

                foreach (var item in _allOutputs.Skip((OutputPageIndex - 1) * OutputPerPageCount).Take(OutputPerPageCount))
                {
                    OutputDatas.Add(item);
                }
            });
        }

        private void OnPageUpdated(string side)
        {
            if (side == "Input")
                UpdateInputPage();
            else
                UpdateOutputPage();
        }

        #endregion

        #region 监控

        private void StartMonitor()
        {
            if (CurrentComm == null) return;

            StopMonitor();

            if (!IsConnected)
            {
                try
                {
                    CurrentComm.Open();
                    IsConnected = true;
                }
                catch (Exception ex)
                {
                    IsMonitorOn = false;
                    dialogService.ShowErrorTip($"连接失败: {ex.Message}");
                    return;
                }
            }

            _monitorToken = new CancellationTokenSource();

            Task.Run(() =>
            {
                int consecutiveFailures = 0;

                while (_monitorToken != null && !_monitorToken.IsCancellationRequested)
                {
                    try
                    {
                        if (CurrentComm?.Communication == null || !CurrentComm.Communication.IsConnected)
                        {
                            _dispatcher.BeginInvoke(() =>
                            {
                                IsConnected = false;
                                IsMonitorOn = false;
                                dialogService.ShowErrorTip("通信连接已断开，监控已停止");
                            });
                            break;
                        }

                        var inputSnapshot = InputDatas?.ToList() ?? new List<PlcIOModel>();
                        var outputSnapshot = OutputDatas?.ToList() ?? new List<PlcIOModel>();
                        int readCount = inputSnapshot.Count + outputSnapshot.Count;

                        _cycleReadErrors = 0;

                        var inputVals = new List<KeyValuePair<PlcIOModel, bool>>();
                        foreach (var item in inputSnapshot)
                        {
                            inputVals.Add(new KeyValuePair<PlcIOModel, bool>(item, ReadInput(item.Address)));
                        }

                        var outputVals = new List<KeyValuePair<PlcIOModel, bool>>();
                        foreach (var item in outputSnapshot)
                        {
                            outputVals.Add(new KeyValuePair<PlcIOModel, bool>(item, ReadOutput(item.Address)));
                        }

                        // 仅当全部读取都抛异常时才判定为通信失败
                        if (readCount > 0 && _cycleReadErrors >= readCount)
                        {
                            consecutiveFailures++;
                            if (consecutiveFailures >= 3)
                            {
                                string errorDetail = _lastReadError ?? "未知错误";
                                _dispatcher.BeginInvoke(() =>
                                {
                                    IsMonitorOn = false;
                                    dialogService.ShowErrorTip($"通信读取连续失败，监控已停止\n最近错误: {errorDetail}");
                                });
                                break;
                            }
                        }
                        else
                        {
                            consecutiveFailures = 0;
                        }

                        _dispatcher.BeginInvoke(() =>
                        {
                            foreach (var kv in inputVals)
                            {
                                kv.Key.BoolValue = kv.Value;
                            }
                            foreach (var kv in outputVals)
                            {
                                kv.Key.BoolValue = kv.Value;
                            }
                        });
                    }
                    catch { }

                    if (_monitorToken == null || _monitorToken.IsCancellationRequested) break;
                    Thread.Sleep(Frequency);
                }
            }, _monitorToken.Token);
        }

        private void StopMonitor()
        {
            _monitorToken?.Cancel();
            _monitorToken = null;
        }

        private bool ReadInput(string address)
        {
            if (CurrentComm?.Protocol == null || CurrentComm.Communication == null || !CurrentComm.Communication.IsConnected)
                return false;

            try
            {
                string readMsg = $"01 01 {address} 1";
                var result = CurrentComm.Read<bool>(readMsg, 1000);
                return result != null && result.Count > 0 && result[0];
            }
            catch (Exception ex)
            {
                _lastReadError = $"读取输入 {address} 失败: {ex.Message}";
                _cycleReadErrors++;
                return false;
            }
        }

        private bool ReadOutput(string address)
        {
            if (CurrentComm?.Protocol == null || CurrentComm.Communication == null || !CurrentComm.Communication.IsConnected)
                return false;

            try
            {
                string readMsg = $"01 01 {address} 1";
                var result = CurrentComm.Read<bool>(readMsg, 1000);
                return result != null && result.Count > 0 && result[0];
            }
            catch (Exception ex)
            {
                _lastReadError = $"读取输出 {address} 失败: {ex.Message}";
                _cycleReadErrors++;
                return false;
            }
        }

        public void WriteOutput(PlcIOModel model, bool value)
        {
            if (CurrentComm?.Protocol == null || CurrentComm.Communication == null)
                return;

            if (!CurrentComm.Communication.IsConnected)
            {
                dialogService.ShowErrorTip("通信未连接，请先连接设备");
                return;
            }

            try
            {
                string writeMsg = $"01 05 {model.Address} {(value ? 1 : 0)}";
                CurrentComm.Write(value, writeMsg);
                model.BoolValue = value;
            }
            catch (Exception ex)
            {
                dialogService.ShowErrorTip($"写入失败: {ex.Message}");
            }
        }

        #endregion

        #region Tab 管理

        private void OnAddTab()
        {
            int count = Tabs.Count + 1;
            string name = $"模块{count}";
            while (Tabs.Any(t => t.TabName == name))
            {
                count++;
                name = $"模块{count}";
            }

            var newTab = new PlcIOTabData { TabName = name };
            Tabs.Add(newTab);
            SelectedTab = newTab;
            SaveToFile();
        }

        private void OnRemoveTab()
        {
            if (SelectedTab == null) return;
            if (Tabs.Count <= 1)
            {
                dialogService.ShowErrorTip("至少保留一个模块");
                return;
            }

            int idx = Tabs.IndexOf(SelectedTab);
            Tabs.Remove(SelectedTab);

            if (idx >= Tabs.Count) idx = Tabs.Count - 1;
            SelectedTab = idx >= 0 ? Tabs[idx] : null;

            SaveToFile();
        }

        private void OnRenameTab()
        {
            if (SelectedTab == null) return;

            dialogService.ShowInfoInput("重命名模块", SelectedTab.TabName, result =>
            {
                if (result.Result == ButtonResult.OK)
                {
                    var newName = result.Parameters.GetValue<string>("Text");
                    if (!string.IsNullOrWhiteSpace(newName))
                    {
                        SelectedTab.TabName = newName.Trim();
                        SaveToFile();
                    }
                }
            });
        }

        #endregion

        #region 增删改

        private void OnAddInput()
        {
            if (SelectedTab == null) return;
            SelectedTab.Addresses.Add(new PlcIOModel("", "", false));
            RefreshPages();
            SaveToFile();
        }

        private void OnAddOutput()
        {
            if (SelectedTab == null) return;
            SelectedTab.Addresses.Add(new PlcIOModel("", "", true));
            RefreshPages();
            SaveToFile();
        }

        private void OnDeleteInput()
        {
            if (SelectedInput == null || SelectedTab == null) return;
            SelectedTab.Addresses.Remove(SelectedInput);
            SelectedInput = null;
            RefreshPages();
            SaveToFile();
        }

        private void OnDeleteOutput()
        {
            if (SelectedOutput == null || SelectedTab == null) return;
            SelectedTab.Addresses.Remove(SelectedOutput);
            SelectedOutput = null;
            RefreshPages();
            SaveToFile();
        }

        private void OnWriteOutput(PlcIOModel model)
        {
            if (model == null) return;
            WriteOutput(model, model.WriteValue != 0);
        }

        private void OnToggleOutput(PlcIOModel model)
        {
            if (model == null) return;
            WriteOutput(model, !model.BoolValue);
        }

        private void OnCellEditFinished()
        {
            SaveToFile();
        }

        #endregion

        #region 连接

        private void OnConnect()
        {
            if (CurrentComm == null) return;

            try
            {
                CurrentComm.Open();
                IsConnected = true;
            }
            catch (Exception ex)
            {
                dialogService.ShowErrorTip($"连接失败: {ex.Message}");
            }
        }

        private void OnDisconnect()
        {
            if (CurrentComm == null) return;

            StopMonitor();
            IsMonitorOn = false;
            CurrentComm.Close();
            IsConnected = false;
        }

        private void OnCommSelectionChanged()
        {
            if (CurrentComm != null)
            {
                InputPageIndex = 1;
                OutputPageIndex = 1;
                IsConnected = CurrentComm.Communication?.IsConnected ?? false;
                if (_isInitialized) SaveToFile();
            }
        }

        #endregion

        #region 导入导出

        private void OnBatchImport()
        {
            if (SelectedTab == null) return;

            var openFile = new OpenFileDialog();
            openFile.Filter = "XLS|*.xls";
            if (!openFile.ShowDialog().Value) return;

            try
            {
                var excel = new ExcelTool(openFile.FileName, false);
                var table = excel.GetTableBySheet(0, 1, 0);

                SelectedTab.Addresses.Clear();

                for (int i = 0; i < table.Rows.Count; i++)
                {
                    var row = table.Rows[i];
                    if (row.ItemArray.All(x => string.IsNullOrEmpty(x?.ToString()))) continue;

                    var address = row[1]?.ToString()?.Trim();
                    if (string.IsNullOrEmpty(address)) continue;

                    var name = row[2]?.ToString()?.Trim() ?? address;
                    var typeStr = row[3]?.ToString()?.Trim() ?? "0";
                    bool isOutput = typeStr == "1";

                    SelectedTab.Addresses.Add(new PlcIOModel(address, name, isOutput));
                }

                RefreshPages();
                SaveToFile();
            }
            catch (Exception ex)
            {
                dialogService.ShowErrorTip($"导入失败: {ex.Message}");
            }
        }

        private void OnBatchExport()
        {
            if (SelectedTab == null || SelectedTab.Addresses.Count == 0)
            {
                dialogService.ShowErrorTip("没有数据可导出");
                return;
            }

            var saveFile = new SaveFileDialog();
            saveFile.Filter = "XLS|*.xls";
            if (!saveFile.ShowDialog().Value) return;

            var filename = saveFile.FileName;
            var excel = new ExcelTool(filename);

            var prop = typeof(ExportPlcIOModel).GetProperties()
                .Where(x => x.GetCustomAttribute<DisplayNameAttribute>() != null).ToList();
            var header = prop.Select(x => x.GetCustomAttribute<DisplayNameAttribute>().DisplayName).ToArray();
            excel.SetHeaders(0, 0, header);

            var data = new object[header.Length];

            for (int i = 0; i < SelectedTab.Addresses.Count; i++)
            {
                var item = SelectedTab.Addresses[i];
                var exportModel = new ExportPlcIOModel
                {
                    Index = i + 1,
                    Address = item.Address,
                    Name = item.Name,
                    Type = item.IsOutput ? 1 : 0
                };

                for (int j = 0; j < header.Length; j++)
                {
                    data[j] = prop.FirstOrDefault(x => x.GetCustomAttribute<DisplayNameAttribute>().DisplayName == header[j])?.GetValue(exportModel) ?? "";
                }
                excel.WriteRowDatas(i + 1, 0, data);
            }

            excel.Save(filename);
        }

        private void OnExportTemplate()
        {
            var saveFile = new SaveFileDialog();
            saveFile.Filter = "XLS|*.xls";
            if (!saveFile.ShowDialog().Value) return;

            var filename = saveFile.FileName;
            var excel = new ExcelTool(filename);

            var prop = typeof(ExportPlcIOModel).GetProperties()
                .Where(x => x.GetCustomAttribute<DisplayNameAttribute>() != null).ToList();
            var header = prop.Select(x => x.GetCustomAttribute<DisplayNameAttribute>().DisplayName).ToArray();
            excel.SetHeaders(0, 0, header);
            excel.Save(filename);
        }

        #endregion
    }
}
