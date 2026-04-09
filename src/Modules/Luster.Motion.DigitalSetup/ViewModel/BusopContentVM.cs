using Aspose.Cells;
using Aspose.Cells.Rendering;
using Luster.Common.DataAccess.Repositories;
using Luster.Motion.CommonUI;
using Luster.Motion.DigitalSetup.Datas;
using Luster.Motion.DigitalSetup.Helpers;
using Luster.Motion.DigitalSetup.Services;
using Luster.Motion.EditorUI;
using Luster.Motion.Integration.Web;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Diagnostics;

namespace Luster.Motion.DigitalSetup.ViewModel
{
    /// <summary>
    /// BUSOP 一级页面 ViewModel
    /// 管理18个子界面，集成子界面功能
    /// </summary>
    public class BusopContentVM : BaseAss
    {
        private readonly BusopConfigService _busopConfigService;
        private BusopConfig _busopConfig;
        private bool _dataLoadRequested = false;

        /// <summary>
        /// 当前选中的子界面配置
        /// </summary>
        private BusopSubItemConfig _currentSubItemConfig;
        public BusopSubItemConfig CurrentSubItemConfig
        {
            get => _currentSubItemConfig;
            private set => SetProperty(ref _currentSubItemConfig, value);
        }

        /// <summary>
        /// 当前子界面名称
        /// </summary>
        public string CurrentSubItemName => CurrentSubItemConfig?.Name ?? "";

        /// <summary>
        /// Excel 文件完整路径
        /// </summary>
        private string _excelFilePath;
        public string ExcelFilePath
        {
            get => _excelFilePath;
            private set => SetProperty(ref _excelFilePath, value);
        }

        /// <summary>
        /// 当前 Sheet 页渲染的图片
        /// </summary>
        private BitmapImage _sheetImage;
        public BitmapImage SheetImage
        {
            get => _sheetImage;
            private set => SetProperty(ref _sheetImage, value);
        }

        /// <summary>
        /// 右侧提示文本（无配置/加载中/错误等状态）
        /// </summary>
        private string _statusMessage = "请先在设置中配置 Sheet 页";
        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        /// <summary>
        /// 是否有 Sheet 图片可显示
        /// </summary>
        private bool _hasSheetImage;
        public bool HasSheetImage
        {
            get => _hasSheetImage;
            private set => SetProperty(ref _hasSheetImage, value);
        }

        /// <summary>
        /// 缩放比例（1.0 = 100%）
        /// </summary>
        private double _zoomScale = 1.0;
        public double ZoomScale
        {
            get => _zoomScale;
            set => SetProperty(ref _zoomScale, value);
        }

        /// <summary>
        /// 缩放百分比显示
        /// </summary>
        public int ZoomPercent => (int)(ZoomScale * 100);

        /// <summary>
        /// 图片水平偏移（拖拽用）
        /// </summary>
        private double _offsetX;
        public double OffsetX
        {
            get => _offsetX;
            set => SetProperty(ref _offsetX, value);
        }

        /// <summary>
        /// 图片垂直偏移（拖拽用）
        /// </summary>
        private double _offsetY;
        public double OffsetY
        {
            get => _offsetY;
            set => SetProperty(ref _offsetY, value);
        }

        /// <summary>
        /// 打开 BUSOP 按钮 command
        /// </summary>
        public ICommand OpenBusopCommand { get; private set; }

        /// <summary>
        /// 设置按钮 command
        /// </summary>
        public ICommand OpenSettingsCommand { get; private set; }

        /// <summary>
        /// 放大 command
        /// </summary>
        public ICommand ZoomInCommand { get; private set; }

        /// <summary>
        /// 缩小 command
        /// </summary>
        public ICommand ZoomOutCommand { get; private set; }

        /// <summary>
        /// 适应窗口 command
        /// </summary>
        public ICommand ZoomFitCommand { get; private set; }

        /// <summary>
        /// 添加子界面 command
        /// </summary>
        public ICommand AddSubItemCommand { get; private set; }

        /// <summary>
        /// 删除子界面 command
        /// </summary>
        public ICommand DeleteSubItemCommand { get; private set; }

        /// <summary>
        /// 重命名子界面 command
        /// </summary>
        public ICommand RenameSubItemCommand { get; private set; }

        public BusopContentVM(
            IRepository repository,
            IRegionManager regionManager,
            ICommonBus commonBus,
            CSVHelper csvHelper,
            FlowBus flowBus,
            IDialogService dialogService,
            CheckStatusService checkStatusService,
            BusopConfigService busopConfigService)
            : base(repository, regionManager, commonBus, csvHelper, flowBus, dialogService, checkStatusService)
        {
            _busopConfigService = busopConfigService;
            _parentRegionName = "BusopContent";

            // 初始化命令
            OpenBusopCommand = new DelegateCommand(OnOpenBusop, CanOpenBusop);
            OpenSettingsCommand = new DelegateCommand(OnOpenSettings);
            ZoomInCommand = new DelegateCommand(() => ApplyZoom(0.2));
            ZoomOutCommand = new DelegateCommand(() => ApplyZoom(-0.2));
            ZoomFitCommand = new DelegateCommand(() => { ZoomScale = 1.0; OffsetX = 0; OffsetY = 0; RaisePropertyChanged(nameof(ZoomPercent)); });
            AddSubItemCommand = new DelegateCommand(OnAddSubItem);
            DeleteSubItemCommand = new DelegateCommand(OnDeleteSubItem, CanModifySubItem);
            RenameSubItemCommand = new DelegateCommand(OnRenameSubItem, CanModifySubItem);

            // 加载配置
            _busopConfig = _busopConfigService.LoadConfig();

            // 初始化18个子页面
            Pages = new ObservableCollection<CommonPageModel>();
            for (int i = 0; i < _busopConfig.SubItems.Count; i++)
            {
                Pages.Add(new CommonPageModel
                {
                    Name = _busopConfig.SubItems[i].Name,
                    IsSelected = i == 0,
                    Region = "",
                    ViewType = null
                });
            }

            // 注册子页面到 DigitalAssPageModel
            DigitalAssPageModel.RegisterSubPages("BusopContent", Pages);

            // 默认选中第一个
            SelectedReportPage = Pages.FirstOrDefault(p => p.IsSelected) ?? Pages.FirstOrDefault();

            // 初始化第一个子界面
            if (SelectedReportPage != null)
            {
                SelectSubItem(SelectedReportPage);
            }

            // 延迟加载点检状态 - 使用较低的优先级
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                LoadCheckStatusForAllPages();
            }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);

            // 延迟加载数据，避免界面切换时卡顿
            _dataLoadRequested = false;
        }        

        /// <summary>
        /// 选择子界面
        /// </summary>
        private void SelectSubItem(CommonPageModel page)
        {
            if (page == null) return;

            var subItemConfig = _busopConfig.SubItems.FirstOrDefault(s => s.Name == page.Name);
            if (subItemConfig == null) return;

            CurrentSubItemConfig = subItemConfig;

            // 确保 ExcelFilePath 始终从配置中获取
            if (_busopConfig != null)
            {
                ExcelFilePath = _busopConfigService.GetExcelFullPath(_busopConfig.ExcelFilePath);
            }

            // 刷新按钮状态
            (OpenBusopCommand as DelegateCommand)?.RaiseCanExecuteChanged();

            // 加载当前 Sheet 页图片
            LoadSheetImage();
        }

        /// <summary>
        /// 将指定 Sheet 页渲染为图片并显示在右侧
        /// </summary>
        private void LoadSheetImage()
        {
            HasSheetImage = false;

            // 检查文件和 Sheet 配置
            if (string.IsNullOrWhiteSpace(ExcelFilePath) || !File.Exists(ExcelFilePath))
            {
                StatusMessage = "Excel 文件未配置或不存在";
                SheetImage = null;
                return;
            }

            var sheetName = CurrentSubItemConfig?.SheetName;
            if (string.IsNullOrWhiteSpace(sheetName))
            {
                StatusMessage = "当前子界面未配置 Sheet 页，请在设置中配置";
                SheetImage = null;
                return;
            }

            StatusMessage = "加载中...";

            // 异步加载，避免阻塞 UI
            Task.Run(() =>
            {
                try
                {
                    var workbook = new Workbook(ExcelFilePath);
                    Worksheet worksheet = null;

                    // 查找目标 Sheet
                    foreach (Worksheet ws in workbook.Worksheets)
                    {
                        if (string.Equals(ws.Name, sheetName, StringComparison.OrdinalIgnoreCase))
                        {
                            worksheet = ws;
                            break;
                        }
                    }

                    if (worksheet == null)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            StatusMessage = $"未找到 Sheet 页: {sheetName}";
                            SheetImage = null;
                        });
                        return;
                    }

                    // 用 SheetRender 渲染为图片
                    var imgOptions = new ImageOrPrintOptions
                    {
                        ChartImageType = System.Drawing.Imaging.ImageFormat.Png,
                        OnePagePerSheet = true,
                        OnlyArea = true
                    };

                    var render = new SheetRender(worksheet, imgOptions);
                    var ms = new System.Drawing.Bitmap(render.ToImage(0));

                    // 转换为 WPF BitmapImage
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var bmp = new BitmapImage();
                        bmp.BeginInit();
                        var stream = new MemoryStream();
                        ms.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                        stream.Position = 0;
                        bmp.StreamSource = stream;
                        bmp.CacheOption = BitmapCacheOption.OnLoad;
                        bmp.EndInit();
                        bmp.Freeze();

                        SheetImage = bmp;
                        HasSheetImage = true;
                        StatusMessage = "";
                    });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"加载 Sheet 图片失败: {ex.Message}");
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        StatusMessage = $"加载失败: {ex.Message}";
                        SheetImage = null;
                    });
                }
            });
        }

        /// <summary>
        /// 选中子页面时的回调（覆盖 BaseAss 的 SelectedCommand）
        /// </summary>
        protected override void Selected(CommonPageModel page)
        {
            // 直接调用 SelectSubItem，避免 BaseAss 中的额外操作
            if (page != null)
            {
                SelectSubItem(page);
            }
        }

        /// <summary>
        /// 加载所有子页面的点检状态
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
                        page.ParentRegion = "BusopContent";
                        var record = _checkStatusService.GetRecord(page.PageKey);
                        if (record != null)
                        {
                            page.CheckStatus = record.Status;
                            page.LastCheckTime = record.CheckTime;
                            page.LastCheckOperator = record.Operator;
                            page.CheckRemark = record.Remark;
                        }
                        else
                        {
                            page.CheckStatus = CheckStatus.NotChecked;
                            page.LastCheckTime = null;
                            page.LastCheckOperator = null;
                            page.CheckRemark = null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载 BUSOP 点检状态失败: {ex.Message}");
            }
        }

        protected override void RefreshCheckStatus()
        {
            LoadCheckStatusForAllPages();
        }

        /// <summary>
        /// 检查是否可以打开 BUSOP
        /// </summary>
        private bool CanOpenBusop()
        {
            return !string.IsNullOrWhiteSpace(ExcelFilePath) && File.Exists(ExcelFilePath);
        }

        /// <summary>
        /// 用 Excel COM 互操作打开 xlsx 文件并跳转到指定 Sheet 页
        /// 如果 COM 不可用则回退为系统默认程序打开
        /// </summary>
        private void OnOpenBusop()
        {
            try
            {
                var fullPath = ExcelFilePath;
                if (!File.Exists(fullPath))
                    return;

                var sheetName = CurrentSubItemConfig?.SheetName;

                // 如果没有配置 Sheet 页名称，直接打开文件
                if (string.IsNullOrWhiteSpace(sheetName))
                {
                    Process.Start(new ProcessStartInfo(fullPath) { UseShellExecute = true });
                    return;
                }

                // 使用 Excel COM 互操作打开并激活指定 Sheet
                OpenExcelToSheet(fullPath, sheetName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"打开 BUSOP 文件失败: {ex.Message}，回退为默认程序打开");
                try
                {
                    Process.Start(new ProcessStartInfo(ExcelFilePath) { UseShellExecute = true });
                }
                catch { }
            }
        }

        /// <summary>
        /// 通过 COM 互操作打开文件并激活指定 Sheet 页
        /// 支持 Microsoft Excel 和 WPS 表格
        /// 使用反射调用，兼容不同 COM 接口
        /// </summary>
        private void OpenExcelToSheet(string filePath, string sheetName)
        {
            // 按优先级尝试不同的 COM ProgID
            string[] progIds = { "Excel.Application", "et.Application", "Kwps.Application" };
            Type appType = null;
            foreach (var progId in progIds)
            {
                appType = Type.GetTypeFromProgID(progId);
                if (appType != null)
                    break;
            }

            if (appType == null)
            {
                Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                return;
            }

            object app = null;
            object workbooks = null;
            object workbook = null;
            try
            {
                app = Activator.CreateInstance(appType);
                app.GetType().InvokeMember("Visible", System.Reflection.BindingFlags.SetProperty, null, app, new object[] { true });
                app.GetType().InvokeMember("DisplayAlerts", System.Reflection.BindingFlags.SetProperty, null, app, new object[] { false });

                workbooks = app.GetType().InvokeMember("Workbooks", System.Reflection.BindingFlags.GetProperty, null, app, null);
                workbook = workbooks.GetType().InvokeMember("Open", System.Reflection.BindingFlags.InvokeMethod, null, workbooks, new object[] { filePath });

                // 查找并激活指定 Sheet
                object sheets = workbook.GetType().InvokeMember("Worksheets", System.Reflection.BindingFlags.GetProperty, null, workbook, null);
                int sheetCount = (int)sheets.GetType().InvokeMember("Count", System.Reflection.BindingFlags.GetProperty, null, sheets, null);
                for (int i = 1; i <= sheetCount; i++)
                {
                    object sheet = sheets.GetType().InvokeMember("Item", System.Reflection.BindingFlags.GetProperty, null, sheets, new object[] { i });
                    string name = (string)sheet.GetType().InvokeMember("Name", System.Reflection.BindingFlags.GetProperty, null, sheet, null);
                    if (string.Equals(name, sheetName, StringComparison.OrdinalIgnoreCase))
                    {
                        sheet.GetType().InvokeMember("Activate", System.Reflection.BindingFlags.InvokeMethod, null, sheet, null);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"COM 打开失败: {ex.Message}，回退为默认程序打开");
                try
                {
                    Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                }
                catch { }
            }
        }

        /// <summary>
        /// 应用缩放，步进 0.2，范围 0.2 ~ 5.0
        /// </summary>
        private void ApplyZoom(double delta)
        {
            var newScale = ZoomScale + delta;
            if (newScale < 0.2) newScale = 0.2;
            if (newScale > 5.0) newScale = 5.0;
            ZoomScale = newScale;
            RaisePropertyChanged(nameof(ZoomPercent));
        }

        /// <summary>
        /// 判断是否可以修改子界面（需要选中一个）
        /// </summary>
        private bool CanModifySubItem()
        {
            return SelectedReportPage != null;
        }

        /// <summary>
        /// 添加子界面
        /// </summary>
        private void OnAddSubItem()
        {
            var dialogParams = new DialogParameters
            {
                { "Title", "添加子界面" },
                { "DefaultValue", $"BUSOP{Pages.Count + 1:D2}" }
            };

            _dialogService.ShowDialog("TextInputDialog", dialogParams, result =>
            {
                if (result.Result == ButtonResult.OK)
                {
                    var name = result.Parameters.GetValue<string>("InputText")?.Trim();
                    if (string.IsNullOrEmpty(name)) return;
                    if (Pages.Any(p => p.Name == name))
                    {
                        HandyControl.Controls.MessageBox.Show($"名称 \"{name}\" 已存在", "提示");
                        return;
                    }

                    // 添加到配置
                    var newConfig = new BusopSubItemConfig { Name = name, SheetName = "" };
                    _busopConfig.SubItems.Add(newConfig);
                    _busopConfigService.SaveConfig(_busopConfig);

                    // 添加到界面
                    var newPage = new CommonPageModel
                    {
                        Name = name,
                        IsSelected = false,
                        Region = "",
                        ViewType = null,
                        ParentRegion = "BusopContent"
                    };
                    Pages.Add(newPage);

                    // 选中新添加的项
                    SelectedReportPage = newPage;
                    SelectSubItem(newPage);
                }
            });
        }

        /// <summary>
        /// 删除子界面
        /// </summary>
        private void OnDeleteSubItem()
        {
            if (SelectedReportPage == null) return;

            var name = SelectedReportPage.Name;
            if (HandyControl.Controls.MessageBox.Show($"确定删除 \"{name}\" 吗？", "确认删除",
                System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question) != System.Windows.MessageBoxResult.Yes)
                return;

            // 从配置中删除
            var configItem = _busopConfig.SubItems.FirstOrDefault(s => s.Name == name);
            if (configItem != null)
            {
                _busopConfig.SubItems.Remove(configItem);
                _busopConfigService.SaveConfig(_busopConfig);
            }

            // 从界面中删除
            var pageIndex = Pages.IndexOf(SelectedReportPage);
            Pages.Remove(SelectedReportPage);

            // 选中相邻项
            if (Pages.Count > 0)
            {
                var newIndex = pageIndex < Pages.Count ? pageIndex : Pages.Count - 1;
                SelectedReportPage = Pages[newIndex];
                SelectSubItem(SelectedReportPage);
            }
            else
            {
                SelectedReportPage = null;
                CurrentSubItemConfig = null;
                HasSheetImage = false;
                SheetImage = null;
                StatusMessage = "请添加子界面";
            }

            (DeleteSubItemCommand as DelegateCommand)?.RaiseCanExecuteChanged();
            (RenameSubItemCommand as DelegateCommand)?.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// 重命名子界面
        /// </summary>
        private void OnRenameSubItem()
        {
            if (SelectedReportPage == null) return;

            var dialogParams = new DialogParameters
            {
                { "Title", "重命名子界面" },
                { "DefaultValue", SelectedReportPage.Name }
            };

            _dialogService.ShowDialog("TextInputDialog", dialogParams, result =>
            {
                if (result.Result == ButtonResult.OK)
                {
                    var newName = result.Parameters.GetValue<string>("InputText")?.Trim();
                    if (string.IsNullOrEmpty(newName)) return;
                    if (newName == SelectedReportPage.Name) return;
                    if (Pages.Any(p => p.Name == newName))
                    {
                        HandyControl.Controls.MessageBox.Show($"名称 \"{newName}\" 已存在", "提示");
                        return;
                    }

                    var oldName = SelectedReportPage.Name;

                    // 更新配置
                    var configItem = _busopConfig.SubItems.FirstOrDefault(s => s.Name == oldName);
                    if (configItem != null)
                    {
                        configItem.Name = newName;
                        _busopConfigService.SaveConfig(_busopConfig);
                    }

                    // 更新界面
                    SelectedReportPage.Name = newName;
                    if (CurrentSubItemConfig?.Name == oldName)
                    {
                        CurrentSubItemConfig = configItem;
                    }

                    RaisePropertyChanged(nameof(CurrentSubItemName));
                }
            });
        }

        /// <summary>
        /// 打开设置对话框
        /// </summary>
        private void OnOpenSettings()
        {
            var config = _busopConfigService.LoadConfig();
            var parameters = new DialogParameters
            {
                { "Config", config },
                { "CurrentSubItemName", CurrentSubItemConfig?.Name ?? "" }
            };

            _dialogService.ShowDialog("BusopSettingsDialog", parameters, result =>
            {
                if (result.Result == ButtonResult.OK)
                {
                    var savedConfig = result.Parameters.GetValue<BusopConfig>("Config");
                    if (savedConfig != null)
                    {
                        _busopConfigService.SaveConfig(savedConfig);
                        // 同步更新内存中的配置引用
                        _busopConfig = savedConfig;
                        // 刷新当前子界面路径
                        ExcelFilePath = _busopConfigService.GetExcelFullPath(savedConfig.ExcelFilePath);
                        // 更新当前子界面的 SheetName
                        if (CurrentSubItemConfig != null)
                        {
                            foreach (var item in savedConfig.SubItems)
                            {
                                if (item.Name == CurrentSubItemConfig.Name)
                                {
                                    CurrentSubItemConfig.SheetName = item.SheetName;
                                    break;
                                }
                            }
                        }
                        (OpenBusopCommand as DelegateCommand)?.RaiseCanExecuteChanged();
                    }
                }
            });
        }
    }
}