using Luster.Common.DataStruct.Enums;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.CommonUI.Models;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.CommonUI.Views.Dialogs;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.EditorUI.Models;
using Luster.Motion.SubSystem.Models;
using Luster.Motion.SubSystem.ViewModels;
using Luster.Motion.SubSystem.Views;
using Luster.Motion.TaskFlow.Engine;
using Luster.TaskFlow.Motion;
using Microsoft.CodeAnalysis.Differencing;
using Prism.Commands;
using Prism.Events;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace Luster.Motion.SubSystem.ViewModel
{
    public class FunctionEnableContentVM : MotionPageVM
    {
        /// <summary>
        /// 对话框服务
        /// </summary>
        private IDialogService _dialogService;

        /// <summary>
        /// 运动控制器
        /// </summary>
        private IMotionController _mController;

        /// <summary>
        /// 运动引擎
        /// </summary>
        private IMotionEngine _engine;

        /// <summary>
        /// UI调度器
        /// </summary>
        private Dispatcher _dispatcher;

        /// <summary>
        /// 自动刷新定时器
        /// </summary>
        private DispatcherTimer _autoRefreshTimer;

        /// <summary>
        /// 是否允许编辑启用/禁用功能（非生产模式下允许编辑）
        /// </summary>
        public bool IsEnableDisableEdit
        {
            get
            {
                return !IsInProductionMode();
            }
        }

        /// <summary>
        /// 检查是否处于生产模式
        /// </summary>
        private bool IsInProductionMode()
        {
            try
            {
                var currentMode = _mController.GetCurrentMode();
                return currentMode != null && currentMode.Contains("生产");
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 变量列表
        /// </summary>
        private ObservableCollection<GlobalVar> _enableDisableVars;
        public ObservableCollection<GlobalVar> EnableDisableVars
        {
            get { return _enableDisableVars; }
            set { SetProperty(ref _enableDisableVars, value); }
        }

        public FunctionEnableContentVM(ICommonBus commonBus, IDialogService dialogService, IMotionController mController,
            IDeviceEngine deviceEngine, Dispatcher dispatcher, IMotionEngine engine)
            : base(commonBus)
        {
            _dialogService = dialogService;
            _mController = mController;
            _dispatcher = dispatcher;
            _engine = engine;

            // 初始化启用/屏蔽变量列表
            EnableDisableVars = GetEnableDisableGlobals();

            // 初始化定时器
            InitializeAutoRefreshTimer();
        }

        /// <summary>
        /// 注册事件
        /// </summary>
        /// <param name="bus">事件聚合器</param>
        protected override void RegisterEvent(IEventAggregator bus)
        {
            base.RegisterEvent(bus);

            bus.GetEvent<RecipeOpenEvent>().Subscribe(r =>
            {
                // 刷新启用/屏蔽变量列表
                EnableDisableVars = GetEnableDisableGlobals();
            });
        }

        #region 功能启用功能

        /// <summary>
        /// 添加变量命令
        /// </summary>
        private DelegateCommand _addEnableDisableCommand;
        public DelegateCommand AddEnableDisableCommand =>
            _addEnableDisableCommand ??= new DelegateCommand(ExecuteAddEnableDisableCommand);

        /// <summary>
        /// 执行添加变量命令
        /// </summary>
        private void ExecuteAddEnableDisableCommand()
        {
            // 获取所有bool变量
            var allBoolGlobalVars = GetAllBoolGlobalVariables().ToList();

            // 获取当前功能启用列表中已有的变量Key（用于过滤）
            var existingVarKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (EnableDisableVars != null && EnableDisableVars.Any())
            {
                foreach (var existingVar in EnableDisableVars)
                {
                    // 优先使用Key，因为Key是唯一的
                    var key = !string.IsNullOrEmpty(existingVar.Key) ? existingVar.Key : existingVar.Name;
                    if (!string.IsNullOrEmpty(key))
                    {
                        existingVarKeys.Add(key);
                    }
                }
            }

            // 关键修改：过滤掉已经在列表中的变量
            var availableVars = allBoolGlobalVars
                .Where(v =>
                {
                    var varKey = !string.IsNullOrEmpty(v.Key) ? v.Key : v.Name;
                    return !string.IsNullOrEmpty(varKey) && !existingVarKeys.Contains(varKey);
                })
                .ToList();

            // 如果已经没有可添加的变量，显示提示信息
            if (!availableVars.Any())
            {
                MessageBox.Show("所有bool类型变量都已添加到功能启用列表中", "提示",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new DeviceDialogWindow();
            var viewModel = new DeviceDialogViewModel();

            // 设置过滤后的变量数据（不包含已在列表中的变量）
            viewModel.SetGlobalVariables(availableVars);

            dialog.DataContext = viewModel;
            dialog.Owner = Application.Current.MainWindow;

            bool? result = dialog.ShowDialog();

            if (result == true && dialog.SelectedGlobalVariables != null && dialog.SelectedGlobalVariables.Any())
            {
                foreach (var selectedVar in dialog.SelectedGlobalVariables)
                {
                    AddVariableToEnableDisableList(selectedVar);
                }
            }
        }

        /// <summary>
        /// 添加变量到一键屏蔽列表
        /// </summary>
        private void AddVariableToEnableDisableList(GlobalVar selectedVar)
        {
            try
            {
                if (selectedVar == null) return;

                bool exists = EnableDisableVars?.Any(v =>
                    (!string.IsNullOrEmpty(v.Name) && v.Name == selectedVar.Name) ||
                    (!string.IsNullOrEmpty(v.Key) && v.Key == selectedVar.Key)) == true;

                if (exists)
                {
                    MessageBox.Show($"变量: {selectedVar.Name}已在列表存在", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (EnableDisableVars == null)
                {
                    EnableDisableVars = new ObservableCollection<GlobalVar>();
                }

                if (selectedVar.Parameter != null)
                {
                    var newVar = new GlobalVar(selectedVar.Parameter)
                    {
                        Value = selectedVar.Parameter.Value is bool val ? val : false
                    };
                    EnableDisableVars.Add(newVar);
                }
                else
                {
                    EnableDisableVars.Add(selectedVar);
                }

                SaveEnableDisableVarsToConfig();
                commonBus.IsNeedSave = true;
            }
            catch (Exception ex)
            {
                
            }
        }

        /// <summary>
        /// 获取所有 bool 类型的全局变量
        /// </summary>
        private IEnumerable<GlobalVar> GetAllBoolGlobalVariables()
        {
            var result = new List<GlobalVar>();
            try
            {
                var gModule = GetGlobal();
                if (gModule?.Parameters == null)
                {
                    return result;
                }

                foreach (var item in gModule.Parameters)
                {
                    if (item.Value == null || !item.Value.Visible)
                        continue;

                    if (item.Value.Type == typeof(bool))
                    {
                        result.Add(new GlobalVar(item.Value));
                    }
                }

                result = result.OrderBy(v => v.Name).ToList();
            }
            catch (Exception ex)
            {
                
            }

            return result;
        }

        /// <summary>
        /// 获取启用/屏蔽变量列表
        /// </summary>
        public ObservableCollection<GlobalVar> GetEnableDisableGlobals()
        {
            var collection = new ObservableCollection<GlobalVar>();

            try
            {
                // 从系统配置中加载保存的变量名称
                if (_mController.SysConfig?.EnableDisableVarNames != null &&
                    _mController.SysConfig.EnableDisableVarNames.Any())
                {
                    // 获取所有可用的bool类型全局变量
                    var allBoolGlobalVars = GetAllBoolGlobalVariables().ToList();

                    foreach (var varName in _mController.SysConfig.EnableDisableVarNames)
                    {
                        // 从当前全局变量中查找对应的变量
                        var globalVar = allBoolGlobalVars.FirstOrDefault(v => v.Name == varName || v.Key == varName);
                        if (globalVar != null)
                        {
                            var newVar = new GlobalVar(globalVar.Parameter)
                            {
                                // 如果参数不为空，使用参数的当前值
                                Value = globalVar.Parameter?.Value is bool val ? val : (globalVar.Value is bool v ? v : false)
                            };

                            collection.Add(newVar);
                        }
                        else
                        {
                            MessageBox.Show($"无法找到一键屏蔽变量: {varName}", "该变量可能已被删除或重命名", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                commonBus.OnLog(LogType.Warning, $"加载一键屏蔽列表失败: {ex.Message}");
            }

            return collection;
        }

        /// <summary>
        /// 保存一键屏蔽列表到系统配置
        /// </summary>
        private void SaveEnableDisableVarsToConfig()
        {
            try
            {
                if (_mController.SysConfig == null) return;

                _mController.SysConfig.EnableDisableVarNames.Clear();
                _mController.SysConfig.EnableDisableVarValues.Clear();

                if (EnableDisableVars != null && EnableDisableVars.Any())
                {
                    foreach (var varItem in EnableDisableVars)
                    {
                        string key = !string.IsNullOrEmpty(varItem.Name) ? varItem.Name : varItem.Key;
                        if (!string.IsNullOrEmpty(key))
                        {
                            _mController.SysConfig.EnableDisableVarNames.Add(key);
                            _mController.SysConfig.EnableDisableVarValues[key] = varItem.Value is bool val && val;
                        }
                    }
                }

                commonBus.IsNeedSave = true;
            }
            catch (Exception ex)
            {
                commonBus.OnLog(LogType.Error, $"保存一键屏蔽列表失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 切换状态命令
        /// </summary>
        private DelegateCommand<GlobalVar> _toggleEnableDisableCommand;
        public DelegateCommand<GlobalVar> ToggleEnableDisableCommand => _toggleEnableDisableCommand ??
            (_toggleEnableDisableCommand = new DelegateCommand<GlobalVar>((globalVar) =>
            {
                if (IsInProductionMode()) { return; }
                if (globalVar != null && globalVar.Parameter != null)
                {
                    try
                    {
                        if (globalVar.Value is bool newValue)
                        {
                            globalVar.Parameter.Value = newValue;
                            SaveEnableDisableVarsToConfig();
                            commonBus.IsNeedSave = true;
                            commonBus.EventBus.GetEvent<GlobalVarEvent>().Publish();

                            commonBus.OnLog(LogType.Info,
                                $"变量 {globalVar.Name} 状态已切换为: {newValue}");
                        }
                    }
                    catch (Exception ex)
                    {
                        commonBus.OnLog(LogType.Error, $"切换状态失败: {ex.Message}");
                    }
                }
            }));

        /// <summary>
        /// 删除一键屏蔽变量命令
        /// </summary>
        private DelegateCommand<GlobalVar> _deleteEnableDisableCommand;
        public DelegateCommand<GlobalVar> DeleteEnableDisableCommand =>
            _deleteEnableDisableCommand ?? (_deleteEnableDisableCommand = new DelegateCommand<GlobalVar>((globalVar) =>
            {
                if (IsInProductionMode()) { return; }
                if (globalVar != null && EnableDisableVars != null)
                {
                    try
                    {
                        EnableDisableVars.Remove(globalVar);
                        SaveEnableDisableVarsToConfig();
                        commonBus.IsNeedSave = true;

                        // 触发集合更新
                        var tempList = EnableDisableVars.ToList();
                        EnableDisableVars = new ObservableCollection<GlobalVar>(tempList);
                    }
                    catch (Exception ex)
                    {
                        commonBus.OnLog(LogType.Error, $"删除变量失败: {ex.Message}");
                    }
                }
            }));

        /// <summary>
        /// 刷新列表命令
        /// </summary>
        private DelegateCommand _refreshEnableDisableCommand;
        public DelegateCommand RefreshEnableDisableCommand => _refreshEnableDisableCommand ??
            (_refreshEnableDisableCommand = new DelegateCommand(() =>
            {
                try
                {
                    if (EnableDisableVars == null || !EnableDisableVars.Any())
                    {
                        return;
                    }

                    // 手动执行一次刷新
                    ExecuteAutoRefresh();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"刷新失败: {ex.Message}", "错误",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }));

        /// <summary>
        /// 获取全局模块
        /// </summary>
        private IMotionModule GetGlobal()
        {
            var id = Luster.TaskFlow.Motion.Logic.GlobalModule.GlobalID;
            return _engine.Get(id);
        }

        #endregion

        #region 自动刷新功能

        /// <summary>
        /// 初始化自动刷新定时器
        /// </summary>
        private void InitializeAutoRefreshTimer()
        {
            _autoRefreshTimer = new DispatcherTimer();
            _autoRefreshTimer.Interval = TimeSpan.FromSeconds(3);
            _autoRefreshTimer.Tick += AutoRefreshTimer_Tick;
            // 启动定时器
            _autoRefreshTimer.Start();
        }

        /// <summary>
        /// 定时器触发事件 - 每2秒执行一次
        /// </summary>
        private void AutoRefreshTimer_Tick(object sender, EventArgs e)
        {
            if (EnableDisableVars != null && EnableDisableVars.Any())
            {
                ExecuteAutoRefresh();
            }
        }

        /// <summary>
        /// 执行自动刷新逻辑
        /// </summary>
        private void ExecuteAutoRefresh()
        {
            try
            {
                var latestBoolGlobalVars = GetAllBoolGlobalVariables().ToList();
                bool hasChanges = false;
                _dispatcher.Invoke(() =>
                {
                    foreach (var existingVar in EnableDisableVars)
                    {
                        // 根据名称或Key查找最新的变量
                        var latestVar = latestBoolGlobalVars.FirstOrDefault(v =>
                            (!string.IsNullOrEmpty(v.Name) && v.Name == existingVar.Name) ||
                            (!string.IsNullOrEmpty(v.Key) && v.Key == existingVar.Key));

                        if (latestVar != null && latestVar.Parameter != null)
                        {
                            bool currentValue = existingVar.Value is bool val && val;
                            bool latestValue = latestVar.Value is bool latestVal && latestVal;

                            if (currentValue != latestValue)
                            {
                                existingVar.Value = latestValue;
                                existingVar.IsChecked = latestValue;

                                if (existingVar.Parameter != null)
                                {
                                    existingVar.Parameter.Value = latestValue;
                                }
                                SaveEnableDisableVarsToConfig();
                                hasChanges = true;
                            }
                        }
                    }
                    if (EnableDisableVars != null && EnableDisableVars.Any())
                    {
                        // 即使hasChanges为false，也要重新赋值集合
                        var tempList = EnableDisableVars.ToList();
                        EnableDisableVars = new ObservableCollection<GlobalVar>(tempList);
                    }
                    if (hasChanges)
                    {
                        // 触发全局变量变更事件
                        commonBus.EventBus.GetEvent<GlobalVarEvent>().Publish();
                    }
                });
            }
            catch (Exception ex)
            {

            }
        }

        #endregion

        /// <summary>
        /// 析构函数，清理资源
        /// </summary>
        ~FunctionEnableContentVM()
        {
            if (_autoRefreshTimer != null)
            {
                _autoRefreshTimer.Stop();
                _autoRefreshTimer.Tick -= AutoRefreshTimer_Tick;
            }
        }
    }
}