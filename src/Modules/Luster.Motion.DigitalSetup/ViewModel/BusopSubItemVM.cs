using Luster.Motion.DigitalSetup.Datas;
using Luster.Motion.DigitalSetup.Services;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;

namespace Luster.Motion.DigitalSetup.ViewModel
{
    /// <summary>
    /// BUSOP 子界面 ViewModel
    /// </summary>
    public class BusopSubItemVM : BindableBase
    {
        private readonly BusopConfigService _configService;
        private readonly IDialogService _dialogService;

        private BusopSubItemConfig _currentConfig;
        private string _excelFilePath;

        /// <summary>
        /// 当前子界面的配置
        /// </summary>
        public BusopSubItemConfig CurrentConfig
        {
            get => _currentConfig;
            set => SetProperty(ref _currentConfig, value);
        }

        /// <summary>
        /// xlsx 文件完整路径
        /// </summary>
        public string ExcelFilePath
        {
            get => _excelFilePath;
            set => SetProperty(ref _excelFilePath, value);
        }

        /// <summary>
        /// 当前子界面名称
        /// </summary>
        public string SubItemName => CurrentConfig?.Name ?? "";

        /// <summary>
        /// 打开 BUSOP 按钮 command
        /// </summary>
        public ICommand OpenBusopCommand { get; private set; }

        /// <summary>
        /// 设置按钮 command
        /// </summary>
        public ICommand OpenSettingsCommand { get; private set; }

        public BusopSubItemVM(BusopConfigService configService, IDialogService dialogService)
        {
            _configService = configService;
            _dialogService = dialogService;

            OpenBusopCommand = new DelegateCommand(OnOpenBusop, CanOpenBusop);
            OpenSettingsCommand = new DelegateCommand(OnOpenSettings);
        }

        /// <summary>
        /// 初始化子界面配置
        /// </summary>
        public void Initialize(BusopSubItemConfig config, string excelFilePath)
        {
            CurrentConfig = config;
            ExcelFilePath = excelFilePath;
            RaisePropertyChanged(nameof(SubItemName));
            (OpenBusopCommand as DelegateCommand)?.RaiseCanExecuteChanged();
        }

        private bool CanOpenBusop()
        {
            return !string.IsNullOrWhiteSpace(ExcelFilePath) && File.Exists(ExcelFilePath);
        }

        /// <summary>
        /// 用系统默认程序打开 xlsx 文件
        /// </summary>
        private void OnOpenBusop()
        {
            try
            {
                var fullPath = ExcelFilePath;
                if (File.Exists(fullPath))
                {
                    Process.Start(new ProcessStartInfo(fullPath) { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"打开 BUSOP 文件失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 打开设置对话框
        /// </summary>
        private void OnOpenSettings()
        {
            var config = _configService.LoadConfig();
            var parameters = new DialogParameters
            {
                { "Config", config },
                { "CurrentSubItemName", CurrentConfig?.Name ?? "" }
            };

            _dialogService.ShowDialog("BusopSettingsDialog", parameters, result =>
            {
                if (result.Result == ButtonResult.OK)
                {
                    var savedConfig = result.Parameters.GetValue<BusopConfig>("Config");
                    if (savedConfig != null)
                    {
                        _configService.SaveConfig(savedConfig);
                        // 刷新当前子界面路径
                        ExcelFilePath = _configService.GetExcelFullPath(savedConfig.ExcelFilePath);
                        // 更新当前子界面的 SheetName
                        if (CurrentConfig != null)
                        {
                            foreach (var item in savedConfig.SubItems)
                            {
                                if (item.Name == CurrentConfig.Name)
                                {
                                    CurrentConfig.SheetName = item.SheetName;
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