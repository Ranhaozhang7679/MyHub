using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Real;
using Luster.SimDevice.EngineUI;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;

namespace Luster.SimDevice.SubSystem.ViewModel.Dialog
{
    // 用于下拉框显示的 KV 结构
    public class ComboOption
    {
        public string Header { get; set; }
        public string Value { get; set; }
        public override string ToString() => Header;
    }

    public class ErrorCustomDialogVM : DialogVM
    {
        /// <summary>
        /// 报警代码
        /// </summary>
        private string _alarmCode;
        [Required]
        public string AlarmCode
        {
            get => _alarmCode;
            set => SetProperty(ref _alarmCode, value);
        }

        /// <summary>
        /// 报警内容
        /// </summary>
        private string _alarmContent;
        [Required]
        public string AlarmContent
        {
            get => _alarmContent;
            set => SetProperty(ref _alarmContent, value);
        }

        /// <summary>
        /// 报警英文
        /// </summary>
        private string _alarmEnglish;
        [Required]
        public string AlarmEnglish
        {
            get => _alarmEnglish;
            set => SetProperty(ref _alarmEnglish, value);
        }

        #region 向导生成器相关属性
        private AlarmWizardConfigModel _wizardConfig;
        
        /// <summary>
        /// 已有的报警代码列表，用于序号自增判断
        /// </summary>
        private List<string> _existingCodes = new List<string>();

        /// <summary>
        /// 防止 UpdatePreview 递归调用的重入保护标志
        /// </summary>
        private bool _isUpdatingPreview = false;
        
        public ObservableCollection<ComboOption> ErrorTypes { get; set; } = new ObservableCollection<ComboOption>();
        public ObservableCollection<ComboOption> ErrorSubTypes { get; set; } = new ObservableCollection<ComboOption>();
        public ObservableCollection<ComboOption> Components { get; set; } = new ObservableCollection<ComboOption>();
        public ObservableCollection<ComboOption> SubComponents { get; set; } = new ObservableCollection<ComboOption>();
        public ObservableCollection<ComboOption> RepairActions { get; set; } = new ObservableCollection<ComboOption>();

        private ComboOption _selectedErrorType;
        public ComboOption SelectedErrorType
        {
            get => _selectedErrorType;
            set
            {
                if (SetProperty(ref _selectedErrorType, value))
                {
                    RefreshSubTypes();
                    UpdatePreview();
                }
            }
        }

        private ComboOption _selectedErrorSubType;
        public ComboOption SelectedErrorSubType
        {
            get => _selectedErrorSubType;
            set { SetProperty(ref _selectedErrorSubType, value); UpdatePreview(); }
        }

        private string _selectedComponent;
        public string SelectedComponent
        {
            get => _selectedComponent;
            set { SetProperty(ref _selectedComponent, value); UpdatePreview(); }
        }

        private string _selectedSubComponent;
        public string SelectedSubComponent
        {
            get => _selectedSubComponent;
            set { SetProperty(ref _selectedSubComponent, value); UpdatePreview(); }
        }

        private string _errorIndex = "01";
        public string ErrorIndex
        {
            get => _errorIndex;
            set { SetProperty(ref _errorIndex, value); UpdatePreview(); }
        }

        private ComboOption _selectedRepairAction;
        public ComboOption SelectedRepairAction
        {
            get => _selectedRepairAction;
            set { SetProperty(ref _selectedRepairAction, value); UpdatePreview(); }
        }

        private string _previewCode;
        public string PreviewCode
        {
            get => _previewCode;
            set => SetProperty(ref _previewCode, value);
        }

        public DelegateCommand ApplyCodeCommand { get; private set; }

        private void InitWizardDicts()
        {
            string configDir = deviceEngine?.RecipeConfigPath;
            if (string.IsNullOrEmpty(configDir))
            {
                configDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config");
            }
            
            if (!Directory.Exists(configDir))
            {
                Directory.CreateDirectory(configDir);
            }
            string configPath = Path.Combine(configDir, "AlarmWizardConfig.xml");
            
            XmlSerializer serializer = new XmlSerializer(typeof(AlarmWizardConfigModel));

            // 保存目前选中的项的值，以便在重新加载后尝试恢复选项
            string oldType = SelectedErrorType?.Value;
            string oldSubType = SelectedErrorSubType?.Value;
            string oldComp = SelectedComponent;
            string oldSubComp = SelectedSubComponent;
            string oldAction = SelectedRepairAction?.Value;

            if (File.Exists(configPath))
            {
                try
                {
                    using (FileStream fs = new FileStream(configPath, FileMode.Open, FileAccess.Read))
                    {
                        _wizardConfig = (AlarmWizardConfigModel)serializer.Deserialize(fs);
                    }
                }
                catch (Exception ex)
                {
                    // 若解析失败，退回使用默认
                    _wizardConfig = AlarmWizardConfigModel.GetDefaultConfig();
                    SimEngineUI?.OnLog(LogType.Warning, $"解析报警字典配置文件失败，使用默认配置。异常: {ex.Message}");
                }
            }
            else
            {
                _wizardConfig = AlarmWizardConfigModel.GetDefaultConfig();
                try
                {
                    using (StreamWriter sw = new StreamWriter(configPath, false, Encoding.UTF8))
                    {
                        serializer.Serialize(sw, _wizardConfig);
                    }
                }
                catch (Exception ex)
                {
                    SimEngineUI?.OnLog(LogType.Warning, $"保存缺省报警字典 XML 配置文件失败。异常: {ex.Message}");
                }
            }
            
            // 如果未能成功初始化，生成空的防御
            if (_wizardConfig == null)
            {
                _wizardConfig = new AlarmWizardConfigModel();
            }

            // 初始化界面的选项集合
            ErrorTypes.Clear();
            foreach (var type in _wizardConfig.ErrorTypes) ErrorTypes.Add(type);

            Components.Clear();
            foreach (var comp in _wizardConfig.Components) Components.Add(comp);

            SubComponents.Clear();
            foreach (var sc in _wizardConfig.SubComponents) SubComponents.Add(sc);

            RepairActions.Clear();
            foreach (var ra in _wizardConfig.RepairActions) RepairActions.Add(ra);

            // 尝试恢复选择
            if (!string.IsNullOrEmpty(oldType)) SelectedErrorType = ErrorTypes.FirstOrDefault(x => x.Value == oldType);
            if (!string.IsNullOrEmpty(oldSubType)) SelectedErrorSubType = ErrorSubTypes.FirstOrDefault(x => x.Value == oldSubType);
            
            if (!string.IsNullOrEmpty(oldComp)) SelectedComponent = oldComp;
            if (!string.IsNullOrEmpty(oldSubComp)) SelectedSubComponent = oldSubComp;
            
            if (!string.IsNullOrEmpty(oldAction)) SelectedRepairAction = RepairActions.FirstOrDefault(r => r.Value == oldAction);

            if (SelectedRepairAction == null) SelectedRepairAction = RepairActions.FirstOrDefault(r => r.Value == "");
            if (SelectedRepairAction == null) SelectedRepairAction = RepairActions.FirstOrDefault();
        }

        private DelegateCommand _reloadConfigCommand;
        public DelegateCommand ReloadConfigCommand => 
            _reloadConfigCommand ?? (_reloadConfigCommand = new DelegateCommand(InitWizardDicts));

        private void RefreshSubTypes()
        {
            ErrorSubTypes.Clear();
            if (SelectedErrorType == null) return;

            string typeVal = SelectedErrorType.Value;
            if (_wizardConfig != null)
            {
                var mapping = _wizardConfig.ErrorSubTypes.FirstOrDefault(x => x.ErrorType == typeVal);
                if (mapping != null && mapping.Options != null)
                {
                    foreach (var sub in mapping.Options)
                    {
                        ErrorSubTypes.Add(sub);
                    }
                }
            }
            else
            {
                ErrorSubTypes.Add(new ComboOption { Value = "99", Header = "99 - Other" });
            }

            SelectedErrorSubType = ErrorSubTypes.FirstOrDefault();
        }

        private void UpdatePreview()
        {
            // 防止递归：设置 ErrorIndex 会再次触发 UpdatePreview
            if (_isUpdatingPreview) return;
            _isUpdatingPreview = true;

            try
            {
            string t = SelectedErrorType?.Value ?? "";
            string st = SelectedErrorSubType?.Value ?? "";
            
            string comp = "";
            if (!string.IsNullOrEmpty(SelectedComponent))
            {
                var match = Components.FirstOrDefault(c => c.Header == SelectedComponent || c.Value == SelectedComponent);
                comp = match != null ? match.Value : SelectedComponent;
            }

            string subComp = "";
            if (!string.IsNullOrEmpty(SelectedSubComponent))
            {
                var match = SubComponents.FirstOrDefault(c => c.Header == SelectedSubComponent || c.Value == SelectedSubComponent);
                subComp = match != null ? match.Value : SelectedSubComponent;
            }

            // 计算前缀（不含序号和维修动作部分）
            string prefix = $"{t}{st}{comp}{subComp}";

            // 当前缀非空时，自动计算下一个可用序号
            if (!string.IsNullOrEmpty(prefix) && _existingCodes != null && _existingCodes.Count > 0)
            {
                string searchPrefix = prefix + "-";
                int maxIndex = 0;
                foreach (var code in _existingCodes)
                {
                    if (!string.IsNullOrEmpty(code) && code.StartsWith(searchPrefix))
                    {
                        // 提取分隔符之后的序号部分，例如 "ABCD-03" -> "03"，"ABCD-03-R" -> "03"
                        string afterPrefix = code.Substring(searchPrefix.Length);
                        string indexPart = afterPrefix.Split('-')[0];
                        if (int.TryParse(indexPart, out int existingIdx))
                        {
                            if (existingIdx > maxIndex)
                                maxIndex = existingIdx;
                        }
                    }
                }
                // 序号 = 已有最大值 + 1
                int nextIndex = maxIndex + 1;
                ErrorIndex = nextIndex.ToString("D2");
            }

            string idx = string.IsNullOrWhiteSpace(ErrorIndex) ? "01" : ErrorIndex.PadLeft(2, '0');
            if (idx.Length > 2) idx = idx.Substring(0, 2);

            string ra = SelectedRepairAction?.Value ?? "";

            string baseCode = $"{t}{st}{comp}{subComp}-{idx}";
            if (!string.IsNullOrEmpty(ra))
            {
                baseCode += $"-{ra}";
            }

            PreviewCode = baseCode;
            }
            finally
            {
                _isUpdatingPreview = false;
            }
        }

        private void OnApplyCode()
        {
            if (!string.IsNullOrEmpty(PreviewCode))
            {
                AlarmCode = PreviewCode;
            }
        }
        #endregion

        protected ErrorCustomDialogVM(ISimDeviceEngineUI _engine) : base(_engine)
        {
            InitWizardDicts();
            ApplyCodeCommand = new DelegateCommand(OnApplyCode);
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);

            if (parameters.TryGetValue<string>("AlarmCode", out var code))
                AlarmCode = code;
            if (parameters.TryGetValue<string>("AlarmContent", out var content))
                AlarmContent = content;
            if (parameters.TryGetValue<string>("AlarmEnglish", out var english))
                AlarmEnglish = english;
            if (parameters.TryGetValue<List<string>>("ExistingCodes", out var codes))
                _existingCodes = codes ?? new List<string>();
        }

        protected override void Ok(IDialogResult result)
        {
            result.Parameters.Add("AlarmCode", AlarmCode);
            result.Parameters.Add("AlarmContent", AlarmContent);
            result.Parameters.Add("AlarmEnglish", AlarmEnglish);
        }
    }
}
