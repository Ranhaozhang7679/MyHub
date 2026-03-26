using Luster.Motion.DataStruct;
using Luster.Motion.TaskFlow.Engine;
using Luster.TaskFlow.Motion;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

public class GlobalVarWrapper
{
    public string Name { get; set; }
    public string Key { get; set; }
    public string TypeName { get; set; }
    public Type Type { get; set; }
    public object Value { get; set; }
    public object DefaultV { get; set; }
    public bool IsSelected { get; set; }
}

public class KeyParameterGlobalVM : BindableBase
{
    private readonly IMotionEngine _engine;
    private readonly IEventAggregator _ea;
    private readonly IMotionController _mController;

    private ObservableCollection<GlobalVarWrapper> _globalVariables;
    private ObservableCollection<GlobalVarWrapper> _selectedGlobalVariables;

    public KeyParameterGlobalVM()
    {
    }

    public KeyParameterGlobalVM(IMotionEngine engine, IEventAggregator ea, IMotionController mController) 
    {
        _engine = engine;
        _ea = ea;
        _mController = mController; 

        GlobalVariables = new ObservableCollection<GlobalVarWrapper>();
        SelectedGlobalVariables = new ObservableCollection<GlobalVarWrapper>();

        RefreshCommand = new DelegateCommand(RefreshGlobalVariables);
        AddGlobalVariableCommand = new DelegateCommand(ExecuteAddGlobalVariable);
        DeleteGlobalVariableCommand = new DelegateCommand<GlobalVarWrapper>(ExecuteDeleteGlobalVariable);
        SaveDefaultValueCommand = new DelegateCommand<GlobalVarWrapper>(ExecuteSaveDefaultValue);

        LoadGlobalVariables();
        LoadSelectedVariablesFromConfig();
    }

    /// <summary>
    /// 所有全局变量
    /// </summary>
    public ObservableCollection<GlobalVarWrapper> GlobalVariables
    {
        get => _globalVariables;
        set => SetProperty(ref _globalVariables, value);
    }

    /// <summary>
    /// 已选择的全局变量列表
    /// </summary>
    public ObservableCollection<GlobalVarWrapper> SelectedGlobalVariables
    {
        get => _selectedGlobalVariables;
        set => SetProperty(ref _selectedGlobalVariables, value);
    }

    /// <summary>
    /// 刷新命令
    /// </summary>
    public ICommand RefreshCommand { get; private set; }

    /// <summary>
    /// 新增全局变量命令
    /// </summary>
    public ICommand AddGlobalVariableCommand { get; private set; }

    /// <summary>
    /// 删除全局变量命令
    /// </summary>
    public ICommand DeleteGlobalVariableCommand { get; private set; }

    /// <summary>
    /// 保存默认值命令
    /// </summary>
    public ICommand SaveDefaultValueCommand { get; private set; }

    /// <summary>
    /// 检查是否处于生产模式
    /// </summary>
    private bool IsInProductionMode()
    {
        try
        {
            var currentMode = _mController?.GetCurrentMode();
            return currentMode != null && currentMode.Contains("生产");
        }
        catch (Exception)
        {
            return false;
        }
    }


    /// <summary>
    /// 视图加载时调用
    /// </summary>
    public void OnViewLoaded()
    {
        RefreshGlobalVariables();
    }

    /// <summary>
    /// 加载全局变量
    /// </summary>
    private void LoadGlobalVariables()
    {
        try
        {
            var globalVars = GetAllGlobalVariables();
            GlobalVariables = new ObservableCollection<GlobalVarWrapper>(globalVars);
        }
        catch (Exception ex)
        {

        }
    }

    /// <summary>
    /// 从系统配置加载已选择的变量列表
    /// </summary>
    private void LoadSelectedVariablesFromConfig()
    {
        try
        {
            if (_mController?.SysConfig?.KeyParameterGlobalNames == null)
                return;

            var allGlobalVars = GetAllGlobalVariables().ToList();
            var selectedNames = _mController.SysConfig.KeyParameterGlobalNames;

            foreach (var varName in selectedNames)
            {
                var globalVar = allGlobalVars.FirstOrDefault(v => v.Name == varName || v.Key == varName);
                if (globalVar != null)
                {
                    var wrapper = new GlobalVarWrapper
                    {
                        Name = globalVar.Name,
                        Key = globalVar.Key,
                        TypeName = globalVar.TypeName,
                        Type = globalVar.Type,
                        Value = globalVar.Value,
                        DefaultV = globalVar.DefaultV,
                        IsSelected = true
                    };
                    SelectedGlobalVariables.Add(wrapper);
                }
            }
        }
        catch (Exception ex)
        {

        }
    }

    /// <summary>
    /// 保存已选择的变量列表到系统配置
    /// </summary>
    private void SaveSelectedVariablesToConfig()
    {
        try
        {
            if (_mController?.SysConfig == null) return;

            _mController.SysConfig.KeyParameterGlobalNames.Clear();

            if (SelectedGlobalVariables != null)
            {
                foreach (var varItem in SelectedGlobalVariables)
                {
                    string key = !string.IsNullOrEmpty(varItem.Name) ? varItem.Name : varItem.Key;
                    if (!string.IsNullOrEmpty(key))
                    {
                        _mController.SysConfig.KeyParameterGlobalNames.Add(key);
                    }
                }
            }
        }
        catch (Exception ex)
        {

        }
    }

    /// <summary>
    /// 获取全局模块
    /// </summary>
    private IMotionModule GetGlobal()
    {
        try
        {
            var id = Luster.TaskFlow.Motion.Logic.GlobalModule.GlobalID;
            return _engine.Get(id);
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    /// <summary>
    /// 获取所有全局变量
    /// </summary>
    private IEnumerable<GlobalVarWrapper> GetAllGlobalVariables()
    {
        var result = new List<GlobalVarWrapper>();

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

                var wrapper = new GlobalVarWrapper
                {
                    Name = item.Value.CN ?? item.Value.Name ?? "",
                    Key = item.Value.Name ?? "",
                    TypeName = GetTypeName(item.Value.Type),
                    Type = item.Value.Type,
                    Value = item.Value.Value,
                    DefaultV = item.Value.DefaultV,
                    IsSelected = false
                };

                result.Add(wrapper);
            }

            result = result.OrderBy(v => v.Name).ToList();
        }
        catch (Exception ex)
        {

        }

        return result;
    }

    /// <summary>
    /// 获取类型名称
    /// </summary>
    private string GetTypeName(Type type)
    {
        if (type == typeof(bool)) return "Bool";
        if (type == typeof(int)) return "Int";
        if (type == typeof(double)) return "Double";
        if (type == typeof(string)) return "String";
        return type?.Name ?? "Object";
    }

    /// <summary>
    /// 执行新增全局变量
    /// </summary>
    private void ExecuteAddGlobalVariable()
    {
        try
        {
            var availableVars = GlobalVariables.Where(v =>
                !SelectedGlobalVariables.Any(s => s.Name == v.Name))
                .Select(v => new GlobalVarWrapper
                {
                    Name = v.Name,
                    Key = v.Key,
                    TypeName = v.TypeName,
                    Type = v.Type,
                    Value = v.Value,
                    DefaultV = v.DefaultV,
                    IsSelected = false
                })
                .ToList();

            if (!availableVars.Any())
            {
                MessageBox.Show("所有全局变量都已添加", "提示",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new Luster.SimDevice.SubSystem.Views.Dialog.GlobalSelectDialog(availableVars);
            dialog.Owner = Application.Current.MainWindow;

            var result = dialog.ShowDialog();

            if (result == true && dialog.SelectedVariables != null)
            {
                foreach (var var in dialog.SelectedVariables)
                {
                    if (!SelectedGlobalVariables.Any(v => v.Name == var.Name))
                    {
                        SelectedGlobalVariables.Add(var);
                    }
                }
                RaisePropertyChanged(nameof(SelectedGlobalVariables));
                SaveSelectedVariablesToConfig(); // 新增：保存到配置
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"新增失败: {ex.Message}", "错误",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 执行删除全局变量
    /// </summary>
    private void ExecuteDeleteGlobalVariable(GlobalVarWrapper globalVar)
    {
        try
        {
            if (IsInProductionMode())
            {
                throw new DeviceException("提示：", "生产模式下不能删除");
            }

            if (globalVar != null && SelectedGlobalVariables.Contains(globalVar))
            {
                SelectedGlobalVariables.Remove(globalVar);
                RaisePropertyChanged(nameof(SelectedGlobalVariables));
                SaveSelectedVariablesToConfig();
            }
        }
        catch (DeviceException ex)
        {
            throw;
        }
    }

    /// <summary>
    /// 执行保存默认值
    /// </summary>
    private void ExecuteSaveDefaultValue(GlobalVarWrapper globalVar)
    {
        try
        {
            if (globalVar == null) return;

            // 获取全局模块
            var gModule = GetGlobal();
            if (gModule?.Parameters == null)
            {
                MessageBox.Show("无法获取全局模块", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 查找对应的参数
            var parameter = gModule.Parameters.FirstOrDefault(p => p.Value != null && (p.Value.CN == globalVar.Name || p.Value.Name == globalVar.Key));

            if (parameter.Value == null)
            {
                MessageBox.Show($"未找到变量: {globalVar.Name}", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 更新默认值
            parameter.Value.DefaultV = globalVar.DefaultV;

            // 同时在所有变量列表中更新
            var sourceVar = GlobalVariables.FirstOrDefault(v => v.Name == globalVar.Name);
            if (sourceVar != null)
            {
                sourceVar.DefaultV = globalVar.DefaultV;
            }

            // 触发属性变更
            RaisePropertyChanged(nameof(SelectedGlobalVariables));
            RaisePropertyChanged(nameof(GlobalVariables));

            MessageBox.Show($"默认值保存成功！", "提示",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"保存失败: {ex.Message}", "错误",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 刷新全局变量
    /// </summary>
    private void RefreshGlobalVariables()
    {
        try
        {
            var latestVars = GetAllGlobalVariables().ToList();
            var selectedNames = SelectedGlobalVariables.Select(v => v.Name).ToHashSet();
            GlobalVariables = new ObservableCollection<GlobalVarWrapper>(latestVars);

            var newSelectedVars = latestVars
                .Where(v => selectedNames.Contains(v.Name))
                .Select(v => new GlobalVarWrapper
                {
                    Name = v.Name,
                    Key = v.Key,
                    TypeName = v.TypeName,
                    Type = v.Type,
                    Value = v.Value,
                    DefaultV = v.DefaultV,
                    IsSelected = true
                })
                .ToList();

            SelectedGlobalVariables = new ObservableCollection<GlobalVarWrapper>(newSelectedVars);

            RaisePropertyChanged(nameof(GlobalVariables));
            RaisePropertyChanged(nameof(SelectedGlobalVariables));
        }
        catch (Exception ex)
        {

        }
    }
}