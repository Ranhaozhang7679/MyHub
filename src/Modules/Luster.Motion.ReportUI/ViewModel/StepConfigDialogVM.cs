using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Luster.Motion.ReportUI.Model;

namespace Luster.Motion.ReportUI.ViewModel
{
    /// <summary>
    /// 步骤配置对话框 ViewModel
    /// </summary>
    public class StepConfigDialogVM : BindableBase, IDialogAware
    {
        public string Title => "步骤配置";

        /// <summary>
        /// 可选颜色列表
        /// </summary>
        public List<ColorOption> ColorOptions { get; } = new List<ColorOption>
        {
            new ColorOption { Name = "红色", Value = "#F44336" },
            new ColorOption { Name = "蓝色", Value = "#2196F3" },
            new ColorOption { Name = "绿色", Value = "#4CAF50" },
            new ColorOption { Name = "黄色", Value = "#FF9800" },
            new ColorOption { Name = "紫色", Value = "#9C27B0" },
            new ColorOption { Name = "灰色", Value = "#9E9E9E" },
        };

        public ObservableCollection<StepAnnotationConfigModel> Steps { get; }
            = new ObservableCollection<StepAnnotationConfigModel>();

        private string _recipePath;
        public string RecipePath
        {
            get => _recipePath;
            set => SetProperty(ref _recipePath, value);
        }

        private string _csvFileName;
        /// <summary>
        /// 当前面板对应的 CSV 文件名（不含扩展名），用于配置字典映射
        /// </summary>
        public string CsvFileName
        {
            get => _csvFileName;
            set => SetProperty(ref _csvFileName, value);
        }

        private StepAnnotationConfigModel _selectedStep;
        public StepAnnotationConfigModel SelectedStep
        {
            get => _selectedStep;
            set => SetProperty(ref _selectedStep, value);
        }

        public event Action<IDialogResult> RequestClose;

        #region 命令

        private DelegateCommand _addStepCommand;
        public DelegateCommand AddStepCommand =>
            _addStepCommand ?? (_addStepCommand = new DelegateCommand(() =>
            {
                Steps.Add(new StepAnnotationConfigModel
                {
                    Name = $"步骤 {Steps.Count + 1}",
                    StartTimeMs = 0,
                    EndTimeMs = 100,
                    Color = "#4CAF50"
                });
            }));

        private DelegateCommand _removeStepCommand;
        public DelegateCommand RemoveStepCommand =>
            _removeStepCommand ?? (_removeStepCommand = new DelegateCommand(() =>
            {
                if (SelectedStep != null)
                {
                    Steps.Remove(SelectedStep);
                }
            }));

        private DelegateCommand _loadCommand;
        public DelegateCommand LoadCommand =>
            _loadCommand ?? (_loadCommand = new DelegateCommand(() =>
            {
                var config = StepAnnotationConfig.LoadByCsvName(CsvFileName, RecipePath);
                Steps.Clear();
                foreach (var step in config.Steps)
                {
                    Steps.Add(new StepAnnotationConfigModel
                    {
                        Name = step.Name,
                        StartTimeMs = step.StartTimeMs,
                        EndTimeMs = step.EndTimeMs,
                        Color = step.Color
                    });
                }
                MessageBox.Show($"已加载 {Steps.Count} 个步骤配置（键: {CsvFileName}）");
            }));

        private DelegateCommand _saveCommand;
        public DelegateCommand SaveCommand =>
            _saveCommand ?? (_saveCommand = new DelegateCommand(() =>
            {
                var config = new StepAnnotationConfig();
                foreach (var step in Steps)
                {
                    config.Steps.Add(new StepAnnotationConfigModel
                    {
                        Name = step.Name,
                        StartTimeMs = step.StartTimeMs,
                        EndTimeMs = step.EndTimeMs,
                        Color = step.Color
                    });
                }
                StepAnnotationConfig.SaveByCsvName(CsvFileName, RecipePath, config);
                MessageBox.Show($"已保存 {config.Steps.Count} 个步骤配置（键: {CsvFileName}）");
            }));

        private DelegateCommand _confirmCommand;
        public DelegateCommand ConfirmCommand =>
            _confirmCommand ?? (_confirmCommand = new DelegateCommand(() =>
            {
                var resultSteps = Steps.Select(s => new StepAnnotationConfigModel
                {
                    Name = s.Name,
                    StartTimeMs = s.StartTimeMs,
                    EndTimeMs = s.EndTimeMs,
                    Color = s.Color
                }).ToList();

                var parameters = new DialogParameters();
                parameters.Add("Steps", resultSteps);
                RequestClose?.Invoke(new DialogResult(ButtonResult.OK, parameters));
            }));

        private DelegateCommand _cancelCommand;
        public DelegateCommand CancelCommand =>
            _cancelCommand ?? (_cancelCommand = new DelegateCommand(() =>
            {
                RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
            }));

        #endregion

        #region IDialogAware

        public bool CanCloseDialog() => true;
        public void OnDialogClosed() { }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.ContainsKey("Steps"))
            {
                var steps = parameters.GetValue<List<StepAnnotationConfigModel>>("Steps");
                if (steps != null)
                {
                    foreach (var step in steps)
                    {
                        Steps.Add(new StepAnnotationConfigModel
                        {
                            Name = step.Name,
                            StartTimeMs = step.StartTimeMs,
                            EndTimeMs = step.EndTimeMs,
                            Color = step.Color
                        });
                    }
                }
            }

            if (parameters.ContainsKey("RecipePath"))
            {
                RecipePath = parameters.GetValue<string>("RecipePath") ?? AppDomain.CurrentDomain.BaseDirectory;
            }
            else
            {
                RecipePath = AppDomain.CurrentDomain.BaseDirectory;
            }

            if (parameters.ContainsKey("CsvFileName"))
            {
                CsvFileName = parameters.GetValue<string>("CsvFileName") ?? "_default";
            }
            else
            {
                CsvFileName = "_default";
            }
        }

        #endregion
    }

    /// <summary>
    /// 颜色选项
    /// </summary>
    public class ColorOption
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
