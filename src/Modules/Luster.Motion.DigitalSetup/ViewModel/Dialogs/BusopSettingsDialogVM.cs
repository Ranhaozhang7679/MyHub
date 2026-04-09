using Luster.Motion.CommonUI;
using Luster.Motion.DigitalSetup.Datas;
using Luster.Motion.DigitalSetup.Services;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Luster.Motion.DigitalSetup.ViewModel.Dialogs
{
    /// <summary>
    /// BUSOP 设置对话框 ViewModel
    /// </summary>
    public class BusopSettingsDialogVM : BindableBase, IDialogAware
    {
        private readonly BusopConfigService _configService;
        private readonly ICommonBus _commonBus;

        private string _excelFilePath;
        /// <summary>
        /// xlsx 文件路径（支持绝对路径或配方相对路径）
        /// </summary>
        public string ExcelFilePath
        {
            get => _excelFilePath;
            set => SetProperty(ref _excelFilePath, value);
        }

        private string _currentSheetName;
        /// <summary>
        /// 当前子界面的 Sheet 页名称
        /// </summary>
        public string CurrentSheetName
        {
            get => _currentSheetName;
            set => SetProperty(ref _currentSheetName, value);
        }

        private string _currentSubItemName;
        /// <summary>
        /// 当前子界面名称（显示用）
        /// </summary>
        public string CurrentSubItemName
        {
            get => _currentSubItemName;
            set => SetProperty(ref _currentSubItemName, value);
        }

        private ObservableCollection<string> _availableSheets;
        /// <summary>
        /// 可用的 Sheet 页名称列表
        /// </summary>
        public ObservableCollection<string> AvailableSheets
        {
            get => _availableSheets;
            set => SetProperty(ref _availableSheets, value);
        }

        public string Title => "BUSOP 设置";

        public event Action<IDialogResult> RequestClose;

        public ICommand SaveCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }
        public ICommand BrowseFileCommand { get; private set; }
        public ICommand RefreshSheetsCommand { get; private set; }

        private BusopConfig _originalConfig;

        public BusopSettingsDialogVM(BusopConfigService configService, ICommonBus commonBus)
        {
            _configService = configService;
            _commonBus = commonBus;
            AvailableSheets = new ObservableCollection<string>();

            SaveCommand = new DelegateCommand(OnSave);
            CancelCommand = new DelegateCommand(OnCancel);
            BrowseFileCommand = new DelegateCommand(OnBrowseFile);
            RefreshSheetsCommand = new DelegateCommand(OnRefreshSheets);
        }

        public bool CanCloseDialog() => true;

        public void OnDialogClosed() { }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            // 从参数中获取配置和当前子界面名称
            _originalConfig = parameters.GetValue<BusopConfig>("Config");
            var subItemName = parameters.GetValue<string>("CurrentSubItemName");

            if (_originalConfig != null)
            {
                ExcelFilePath = _originalConfig.ExcelFilePath;

                // 找到当前子界面的配置
                CurrentSubItemName = subItemName;
                if (_originalConfig.SubItems != null)
                {
                    var subItem = _originalConfig.SubItems.FirstOrDefault(s => s.Name == subItemName);
                    if (subItem != null)
                    {
                        CurrentSheetName = subItem.SheetName;
                    }
                }
            }

            // 自动加载可用 Sheet 列表
            OnRefreshSheets();
        }

        /// <summary>
        /// 浏览选择 xlsx 文件
        /// </summary>
        private void OnBrowseFile()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Excel文件|*.xlsx;*.xls|所有文件|*.*",
                Title = "选择 BUSOP 文件"
            };

            if (dialog.ShowDialog() == true)
            {
                // 如果文件在配方目录下，自动转为相对路径
                var recipeDir = _commonBus?.CurrentRecipe?.GetRecipePath() ?? "";
                var fullPath = dialog.FileName;
                if (!string.IsNullOrEmpty(recipeDir) && fullPath.StartsWith(recipeDir, StringComparison.OrdinalIgnoreCase))
                {
                    ExcelFilePath = fullPath.Substring(recipeDir.Length).TrimStart('\\', '/');
                }
                else
                {
                    ExcelFilePath = fullPath;
                }
                OnRefreshSheets();
            }
        }

        /// <summary>
        /// 从 xlsx 文件中刷新可用 Sheet 列表
        /// </summary>
        private void OnRefreshSheets()
        {
            AvailableSheets.Clear();
            if (string.IsNullOrWhiteSpace(ExcelFilePath))
                return;

            var fullPath = _configService.GetExcelFullPath(ExcelFilePath);
            var sheets = _configService.GetSheetNames(fullPath);
            foreach (var sheet in sheets)
            {
                AvailableSheets.Add(sheet);
            }
        }

        private void OnSave()
        {
            if (_originalConfig == null)
                return;

            // 更新全局 xlsx 路径
            _originalConfig.ExcelFilePath = ExcelFilePath;

            // 更新当前子界面的 SheetName
            if (_originalConfig.SubItems != null)
            {
                var subItem = _originalConfig.SubItems.FirstOrDefault(s => s.Name == CurrentSubItemName);
                if (subItem != null)
                {
                    subItem.SheetName = CurrentSheetName;
                }
            }

            var parameters = new DialogParameters
            {
                { "Config", _originalConfig }
            };
            RequestClose?.Invoke(new DialogResult(ButtonResult.OK, parameters));
        }

        private void OnCancel()
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
        }
    }
}