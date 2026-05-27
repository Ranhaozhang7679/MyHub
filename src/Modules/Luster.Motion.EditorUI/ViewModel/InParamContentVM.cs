#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       InParamContentVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.EditorUI.ViewModel
* 文 件 名:       InParamContentVM.cs
* 创建时间:       2022/6/8 11:13:34
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      349ea7f1-ba71-48c2-a828-5fbfdc5a6882
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/8 11:13:34
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.Assets;
using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using Luster.Control.Wpf.Motion;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Real;
using Luster.Motion.DataStruct.Virtual;
using Luster.Motion.EditorUI.Events;
using Luster.Motion.EditorUI.Extensions;
using Luster.Motion.TaskFlow.Engine;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Common.Models;
using Luster.TaskFlow.Common.Module;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Interfaces;
using Luster.TaskFlow.Motion.Logic;
using Luster.TaskFlow.Motion.Models;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Events;
using Prism.Services.Dialogs;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Luster.Motion.DataStruct;
using System.Threading;
using Luster.Motion.DataStruct.VDevice;
using Prism.Regions;
using System.Windows;

namespace Luster.Motion.EditorUI.ViewModel
{
    /// <summary>
    /// 模块参数设置对话框
    /// </summary>
    public class InParamContentVM : BaseFlowVM
    {
        /// <summary>
        /// 对话框服务
        /// </summary>
        private IDialogService _dialogService;

        /// <summary>
        /// 引擎
        /// </summary>
        private IMotionEngine _engine;

        /// <summary>
        /// 
        /// </summary>
        private Dispatcher _dispatcher;
        private readonly IRegionManager _regionManager;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="_eventBus"></param>
        /// <param name="dialogService"></param>

        public InParamContentVM(FlowBus _eventBus,
            IMotionEngine mEngine,
            IDialogService dialogService,
            ICommonBus cBus,
            Dispatcher dispatcher,IRegionManager regionManager) : base(_eventBus, cBus)
        {
            _dialogService = dialogService;
            _engine = mEngine;
            _dispatcher = dispatcher;
            _regionManager = regionManager;
        }


        /// <summary>
        /// 事件订阅
        /// </summary>
        /// <param name="bus"></param>
        protected override void RegisterEvent(IEventAggregator bus)
        {
            base.RegisterEvent(bus);
            //
            bus.GetEvent<ModuleSelectedEvent>().Subscribe(ms =>
            {
                if (ms.Length == 1)
                {
                    var motionModule = ms[0];

                    SetParamReadOnly(motionModule);

                    ModuleAlias = motionModule.Alias;
                }
            });
            //
            bus.GetEvent<ModuleUpdateEvent>().Subscribe(m =>
            {
                if (m.UpdateType == ModuleUpdate.ParameterNum)
                {
                    ModuleObj = null;
                    ModuleObj = m.Module;
                }
            });
            // 预处理
            bus.GetEvent<ModulePrevAddEvent>().Subscribe(module =>
            {
                // 仅在未设置 Parent 时才使用 GetCurrent()（兼容弹窗场景预设置 Parent 的情况）
                if (module.Parent == null)
                {
                    var pModule = eventBus.GetCurrent();
                    module.Parent = pModule;
                }

                if (module.TaskFunction is ISwitch iSwitch)
                {
                    var parameter = module.Parameters[nameof(iSwitch.Condition)];

                    _dialogService.ShowCondition(parameter, GetVarNodes(parameter), "MotionIconSmall", r =>
                    {
                        if (r.Result == ButtonResult.OK)
                        {
                            if (r.Parameters.TryGetValue<LCondition>("Condition", out var condition))
                            {
                                BuildDynCondtion(parameter, condition, module);
                            }
                        }
                    });
                }
                else if (module.TaskFunction is IBranch branch)
                {
                    var parameter = module.Parameters[nameof(branch.Express)];

                    _dialogService.ShowExpression(parameter, GetVarNodes(parameter), "MotionIconSmall", true, r =>
                    {
                        if (r.Result == ButtonResult.OK)
                        {
                            if (r.Parameters.TryGetValue<LExpression>("Express", out var expression))
                            {
                                parameter.Owner.UpdateReferences();
                                BuildDynBranch(parameter, expression, module);
                            }
                        }
                    });
                }
            });
            //
            bus.GetEvent<ModulePostRunEvent>().Subscribe(modules =>
            {
                if (modules.Length == 1)
                {
                    _dispatcher.Invoke(() =>
                    {
                        ModuleObj = null;
                        ModuleObj = modules[0];
                    });
                }
            });
            // 软件运行中不能进行编辑
            bus.GetEvent<OperationEvent>().Subscribe(op =>
            {
                IsReadOnly = !(op.Dst == EngineStatus.Idle || op.Dst == EngineStatus.Ready || op.Dst == EngineStatus.Stop);
                SetParamReadOnly(ModuleObj as IMotionModule);
            });

            bus.GetEvent<UserLoginEvent>().Subscribe(role =>
            {
                ViewVisible = role.UserRole == SystemRole.Admin ? Visibility.Visible : Visibility.Hidden;
            });
        }

        private Visibility _viewVisible = Visibility.Visible;
        public Visibility ViewVisible
        {
            get { return _viewVisible; }
            set { SetProperty(ref _viewVisible, value); }
        }

        /// <summary>
        /// 是否可以进行编辑
        /// </summary>
        private bool _isReadOnly;
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set { SetProperty(ref _isReadOnly, value); }
        }

        private void SetParamReadOnly(IMotionModule m)
        {
            if (m == null) return;
            // 配置参数是否只读
            foreach (var item in m.Parameters)
            {
                if (typeof(VDevice).IsAssignableFrom(item.Value.Type))
                {
                    item.Value.IsReadOnly = IsReadOnly;
                }
            }

            ModuleObj = null;
            ModuleObj = m;

            NoteVisible = false;

            if (m.TaskFunction is IMotionFunction func && func is INote n && n.NoteParams?.Length > 0)
            {
                NoteVisible = true;
                ModuleNote = func.GetNote(n);
                func.NoteChangedEvent -= Func_NoteChangedEvent;
                func.NoteChangedEvent += Func_NoteChangedEvent;
            }
        }

        /// <summary>
        /// 注册事件
        /// </summary>
        /// <param name="obj"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void Func_NoteChangedEvent(string note)
        {
            ModuleNote = note;
        }

        /// <summary>
        /// 构造动态条件
        /// </summary>
        private void BuildDynCondtion(ParameterAttribute p, LCondition newCondition, IMotionModule module)
        {
            // 只有是Swith才进行添加子节点
            if (module.TaskFunction is not ISwitch) return;

            if (p.Type != typeof(LCondition)) return;
            var parameters = p.Owner.Parameters;
            if (newCondition.Count == 0)
            {
                throw new FriendlyException($"{Luster.Motion.Assests.Langs.LangProvider.GetLang("Branch Does Not Exist")}");
            }
            p.Value = newCondition;
            p.Owner.UpdateReferences();

            int existNum = module.Children.Count;

            if (existNum == 0)
            {
                // 全新新增
                foreach (var item in newCondition)
                {
                    // 构建默认子节点
                    var group = _engine.CreateByName("Logic", "Group");
                    group.Alias = item.Name;
                    _engine.Insert(group, existNum, module);
                    existNum++;
                }
            }
            else if (existNum == newCondition.Count)
            {
                // 条件数量不变：按名称匹配并重排子节点
                var childByAlias = module.Children.ToDictionary(c => c.Alias);
                var reordered = new List<IMotionModule>();
                var matchedAliases = new HashSet<string>();

                for (int j = 0; j < newCondition.Count; j++)
                {
                    var condName = newCondition[j].Name;
                    if (childByAlias.TryGetValue(condName, out var matchedChild) && matchedAliases.Add(condName))
                    {
                        reordered.Add(matchedChild);
                    }
                    else
                    {
                        // 改名的情况：按位置匹配未使用的子节点
                        var unmatched = module.Children.FirstOrDefault(c => !reordered.Contains(c));
                        if (unmatched != null)
                        {
                            unmatched.Alias = condName;
                            reordered.Add(unmatched);
                        }
                    }
                }

                module.Children.Clear();
                foreach (var child in reordered)
                {
                    module.Children.Add(child);
                }

                eventBus.SortModule(module);
                eventBus.Bus.GetEvent<ModuleUpdateEvent>().Publish(new ModuleUpdateModule { Module = module });
            }
            else if (existNum < newCondition.Count)
            {
                // 更新或者添加
                for (int j = 0; j < newCondition.Count; j++)
                {
                    var item = newCondition[j];
                    if (existNum > j)
                    {
                        // 名称更新
                        module.Children[j].Alias = item.Name;
                    }
                    else
                    {
                        // 新增构建默认子节点
                        var group = _engine.CreateByName("Logic", "Group");
                        group.Alias = item.Name;
                        _engine.Insert(group, existNum, module);
                    }
                }
            }
            else if (existNum > newCondition.Count)
            {
                // 删除已有条件
                var names = newCondition.Select(u => u.Name).ToList();
                var removeModules = module.Children.Where(u => !names.Contains(u.Alias)).ToArray();
                eventBus.OnRemoveModule(removeModules);
            }
        }

        /// <summary>
        /// 构造动态条件
        /// </summary>
        private void BuildDynBranch(ParameterAttribute p, LExpression expression, IMotionModule module)
        {
            // 只有是Swith才进行添加子节点
            if (module.TaskFunction is not IBranch) return;

            if (p.Type != typeof(LExpression)) return;
            var parameters = p.Owner.Parameters;

            p.Value = expression;

            int existNum = module.Children.Count;
            string[] branches = new string[] { "True", "False" };

            // 全新新增
            foreach (var item in branches)
            {
                // 构建默认子节点
                var group = _engine.CreateByName("Logic", "Group");
                group.Alias = item;
                _engine.Insert(group, existNum, module);
                existNum++;
            }
        }

        /// <summary>
        /// 别名
        /// </summary>
        private string _moduleAlias;
        public string ModuleAlias
        {
            get { return _moduleAlias; }
            set
            {
                SetProperty(ref _moduleAlias, value);

                if (ModuleObj != null && ModuleObj is IMotionModule module)
                {
                    module.Alias = value;
                    //var count = _engine.Count(u => u.Alias == module.Alias);
                    //if (count > 1)
                    //{
                    //    var motionModules = _engine.Where(u => u.Alias == module.Alias).ToList();
                    //    string alias = "";
                    //    foreach (var item in motionModules)
                    //    {
                    //        if (item != module)
                    //        {
                    //            item.GetModuleAlias(item, ref alias);
                    //        }
                    //    }
                    //    _engine.OnLog(Common.DataStruct.Enums.LogType.Error, $"跟模块:{alias}命名相同,请重新命名!");
                    //    //throw new Exception($"跟模块:{alias}命名相同,请重新命名!");
                    //    //Jack-20240206必须用Task异常来触发
                    //    var task = new Task(() =>
                    //    {
                    //        throw new Exception($"跟模块:{alias}命名相同,请重新命名!");
                    //    });
                    //    task.Start();
                    //}
                }
            }
        }

        /// <summary>
        /// 模块
        /// </summary>
        private object _moduleObj;
        public object ModuleObj
        {
            get { return _moduleObj; }
            set
            {
                SetProperty(ref _moduleObj, value);
            }
        }

        /// <summary>
        /// 模块注释
        /// </summary>
        private string _moduleNote;
        public string ModuleNote
        {
            get { return _moduleNote; }
            set
            {
                SetProperty(ref _moduleNote, value);
            }
        }

        /// <summary>
        /// 注释是否可见
        /// </summary>
        private bool _noteVisible = false;
        public bool NoteVisible
        {
            get { return _noteVisible; }
            set
            {
                SetProperty(ref _noteVisible, value);
            }
        }

        /// <summary>
        /// 一键注释
        /// </summary>
        private DelegateCommand _noteCommand;
        public DelegateCommand NoteCommand => _noteCommand ?? (_noteCommand = new DelegateCommand(() =>
        {
            if (ModuleObj != null && ModuleObj is IMotionModule module)
            {
                ModuleAlias = ModuleNote;
            }
        }));

        /// <summary>
        /// 模块运行
        /// </summary>
        private DelegateCommand _applyCommand;
        public DelegateCommand ApplyCommand => _applyCommand ?? (_applyCommand = new DelegateCommand(() =>
        {
            IModule module = ModuleObj as IModule;
            if (module != null)
            {
                eventBus.OnRunOne(module.ID);
            }
        }));

        private string previewKey = "";
        /// <summary>
        /// 模块选择
        /// </summary>
        private DelegateCommand<object> _itemConfigCommand;

        //将 ItemConfigCommand 改为异步 lambda，前置耗时操作放到后台线程，减少UI卡顿
        public DelegateCommand<object> ItemConfigCommand => _itemConfigCommand ?? (_itemConfigCommand = new DelegateCommand<object>(async (obj) =>
        {
            var args = obj as ItemConfigArgs;
            if (args == null) return;

            if (args.Paramter.Type == typeof(VDevice))
            {
                _dialogService.ShowDeviceConfig(args.Paramter, previewKey, r =>
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        if (r.Parameters.TryGetValue("Device", out IVirtualDevice vDevice))
                        {
                            args.Paramter.Value = new VDevice() { DeviceID = vDevice.ID, Name = vDevice.Name };
                        }
                        r.Parameters.TryGetValue<string>("SearchKey", out previewKey);
                    }
                    else if (r.Result == ButtonResult.Cancel)
                    {
                        args.Paramter.Value = null;
                    }
                    var src = ModuleObj; ModuleObj = null; ModuleObj = src;
                });
            }
            else if (args.Paramter.Type == typeof(VAxisMDevice))
            {
                _dialogService.ShowAxisMConfig(args.Paramter, r =>
                {
                    if (r.Result == ButtonResult.OK && r.Parameters.TryGetValue("VAxisMDevice", out VAxisMDevice vAxisM))
                    {
                        args.Paramter.Value = vAxisM;
                    }
                    else if (r.Result == ButtonResult.No)
                    {
                        args.Paramter.Value = null;
                    }
                    var src = ModuleObj; ModuleObj = null; ModuleObj = src;
                });
            }
            else if (args.Paramter.Type == typeof(VAxisDevice))
            {
                _dialogService.ShowAxisConfig(args.Paramter, r =>
                {
                    if (r.Result == ButtonResult.OK && r.Parameters.TryGetValue("VAxisDevice", out VAxisDevice vAxis))
                    {
                        args.Paramter.Value = vAxis;
                    }
                    else if (r.Result == ButtonResult.No)
                    {
                        args.Paramter.Value = null;
                    }
                    var src = ModuleObj; ModuleObj = null; ModuleObj = src;
                });
            }
            else if (args.Paramter.Type == typeof(VAxisPos))
            {
                _dialogService.ShowAxisPosDialog(args.Paramter, r =>
                {
                    if (r.Result == ButtonResult.OK && r.Parameters.TryGetValue("VAxisPos", out VAxisPos vAxisPos) && vAxisPos.Count > 0)
                    {
                        args.Paramter.Value = vAxisPos;
                    }
                    else if (r.Result == ButtonResult.No)
                    {
                        args.Paramter.Value = null;
                    }
                    var src = ModuleObj; ModuleObj = null; ModuleObj = src;
                });
            }
            else if (args.Paramter.Type == typeof(LCondition))
            {
                var module = args.Paramter.Owner as IMotionModule;
                // 异步准备可引用节点
                var varNodes = await Task.Run(() => GetVarNodes(args.Paramter));
                _dialogService.ShowCondition(args.Paramter, varNodes, "MotionIconSmall", r =>
                {
                    if (r.Result == ButtonResult.OK && r.Parameters.TryGetValue<LCondition>("Condition", out var con))
                    {
                        args.Paramter.Value = con;
                        args.Paramter.Owner.UpdateReferences();
                        BuildDynCondtion(args.Paramter, con, module);
                    }
                    var src = ModuleObj; ModuleObj = null; ModuleObj = src;
                });
            }
            else if (args.Paramter.Type.HasImplementedRawGeneric(typeof(LArray<>)))
            {
                _dialogService.ShowArrayConfig(args.Paramter, r =>
                {
                    var src = ModuleObj; ModuleObj = null; ModuleObj = src;
                });
            }
            else if (args.Paramter.Type == typeof(LStringEx))
            {
                // 异步获取变量，记录耗时
                var pVars = await Task.Run(() => eventBus.GetVariables(args.Paramter));
                var varNodes = await Task.Run(() => GetVarNodes(args.Paramter));
                _dialogService.ShowStringEx(args.Paramter, varNodes, "MotionIconSmall", r =>
                {
                    if (r.Parameters.TryGetValue<LStringEx>("StringEx", out var strEx))
                    {
                        args.Paramter.Value = strEx;
                        args.Paramter.Owner.UpdateReferences();
                    }
                    var src = ModuleObj; ModuleObj = null; ModuleObj = src;
                });
            }
            else if (args.Paramter.Type == typeof(LStringMatch))
            {
                _dialogService.ShowStringMatch(args.Paramter, r =>
                {
                    if (r.Parameters.TryGetValue<LStringMatch>("StringMatch", out var strEx))
                    {
                        args.Paramter.Value = strEx;
                        args.Paramter.Owner.UpdateReferences();
                    }
                    var src = ModuleObj; ModuleObj = null; ModuleObj = src;
                });
            }
            else if (args.Paramter.Type == typeof(LExpression))
            {
                // 异步获取变量，记录耗时
                var pVars = await Task.Run(() => eventBus.GetVariables(args.Paramter));
                var varNodes = await Task.Run(() => GetVarNodes(args.Paramter));
                _dialogService.ShowExpression(args.Paramter, varNodes, "MotionIconSmall", true, r =>
                {
                    if (r.Parameters.TryGetValue<LExpression>("Express", out var strEx))
                    {
                        args.Paramter.Value = strEx;
                        args.Paramter.Owner.UpdateReferences();
                    }
                    var src = ModuleObj; ModuleObj = null; ModuleObj = src;
                });
            }
            else if (args.Paramter.Type == typeof(LStation))
            {
                var modules = _engine.GetStations();
                _dialogService.ShowStationDialog(modules, r =>
                {
                    if (r.Result == ButtonResult.OK && r.Parameters.TryGetValue<LStation>("Station", out var stationEx))
                    {
                        args.Paramter.Value = stationEx;
                    }
                    else if (r.Result == ButtonResult.Cancel)
                    {
                        args.Paramter.Value = null;
                    }
                    var src = ModuleObj; ModuleObj = null; ModuleObj = src;
                });
            }
            else if (args.Paramter.Type == typeof(LModule))
            {
                var m = args.Paramter.Owner as IMotionModule;
                _dialogService.ShowSelectModule(m, r =>
                {
                    if (r.Result == ButtonResult.OK && r.Parameters.TryGetValue<LModule>("Module", out var lModule))
                    {
                        args.Paramter.Value = lModule;
                    }
                    else if (r.Result == ButtonResult.No)
                    {
                        args.Paramter.Value = null;
                    }
                    var src = ModuleObj; ModuleObj = null; ModuleObj = src;
                });
            }
            else if (args.Paramter.Type == typeof(SocketAction))
            {
                _dialogService.ShowSlaveConfig(args.Paramter, previewKey, r =>
                {
                    if (r.Result == ButtonResult.OK && r.Parameters.TryGetValue("Slave", out SocketAction vSlave))
                    {
                        args.Paramter.Value = new SocketAction() { Name = vSlave.Name, Value = vSlave.Value };
                    }
                    r.Parameters.TryGetValue<string>("SearchKey", out previewKey);
                    if (r.Result == ButtonResult.Cancel)
                    {
                        args.Paramter.Value = null;
                    }
                    var src = ModuleObj; ModuleObj = null; ModuleObj = src;
                });
            }
            else if (args.Paramter.Type == typeof(VAlarm))
            {
                _dialogService.ShowAlarmConfig(args.Paramter, previewKey, r =>
                {
                    if (r.Result == ButtonResult.OK && r.Parameters.TryGetValue("VAlarm", out VAlarm vAlarm))
                    {
                        args.Paramter.Value = vAlarm;
                        var module = args.Paramter.Owner as IMotionModule;
                        if (module != null)
                        {
                            var parameters = module.Parameters;
                            if (parameters.ContainsKey("AlarmCode") && !string.IsNullOrEmpty(vAlarm.AlarmKey))
                                parameters["AlarmCode"].Value = vAlarm.AlarmKey;
                            if (parameters.ContainsKey("Message") && !string.IsNullOrEmpty(vAlarm.AlarmCN))
                                parameters["Message"].Value = vAlarm.AlarmCN;
                            if (parameters.ContainsKey("Detail") && !string.IsNullOrEmpty(vAlarm.AlarmEn))
                                parameters["Detail"].Value = vAlarm.AlarmEn;
                        }
                    }
                    else if (r.Result == ButtonResult.Cancel)
                    {
                        args.Paramter.Value = null;
                    }
                    var src = ModuleObj; ModuleObj = null; ModuleObj = src;
                });
            }
        }));

        [Obsolete]
        public DelegateCommand<object> ItemConfigCommand1 => _itemConfigCommand ?? (_itemConfigCommand = new DelegateCommand<object>((obj) =>
        {
            ItemConfigArgs args = obj as ItemConfigArgs;

            if (args.Paramter.Type == typeof(VDevice))
            {
                _dialogService.ShowDeviceConfig(args.Paramter, previewKey, r =>
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        if (r.Parameters.TryGetValue("Device", out IVirtualDevice vDevice))
                        {
                            args.Paramter.Value = new VDevice() { DeviceID = vDevice.ID, Name = vDevice.Name };
                        }

                        r.Parameters.TryGetValue<string>("SearchKey", out previewKey);
                    }
                    else if (r.Result == ButtonResult.Cancel)
                    {
                        args.Paramter.Value = null;
                    }

                    // 刷新参数列表
                    var src = ModuleObj;
                    ModuleObj = null;
                    ModuleObj = src;
                });
            }          
            else if (args.Paramter.Type == typeof(VAxisMDevice))
            {
                _dialogService.ShowAxisMConfig(args.Paramter, (r) =>
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        if (r.Parameters.TryGetValue("VAxisMDevice", out VAxisMDevice vAxisM))
                        {
                            args.Paramter.Value = vAxisM;
                        }
                    }
                    else if (r.Result == ButtonResult.No)
                    {
                        args.Paramter.Value = null;
                    }

                    // 刷新参数列表
                    var src = ModuleObj;
                    ModuleObj = null;
                    ModuleObj = src;
                });
            }
            else if (args.Paramter.Type == typeof(VAxisDevice))
            {
                _dialogService.ShowAxisConfig(args.Paramter, (r) =>
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        if (r.Parameters.TryGetValue("VAxisDevice", out VAxisDevice vAxisM))
                        {
                            args.Paramter.Value = vAxisM;
                        }
                    }
                    else if (r.Result == ButtonResult.No)
                    {
                        args.Paramter.Value = null;
                    }

                    // 刷新参数列表
                    var src = ModuleObj;
                    ModuleObj = null;
                    ModuleObj = src;
                });
            }
            else if (args.Paramter.Type == typeof(VAxisPos))
            {
                _dialogService.ShowAxisPosDialog(args.Paramter, (r) =>
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        if (r.Parameters.TryGetValue("VAxisPos", out VAxisPos vAxisPos))
                        {
                            if (vAxisPos.Count > 0)
                            {
                                args.Paramter.Value = vAxisPos;
                            }
                        }
                    }
                    else if (r.Result == ButtonResult.No)
                    {
                        args.Paramter.Value = null;
                    }

                    // 刷新参数列表
                    var src = ModuleObj;
                    ModuleObj = null;
                    ModuleObj = src;
                });
            }
            else if (args.Paramter.Type == typeof(LCondition))
            {
                var module = args.Paramter.Owner as IMotionModule;

                _dialogService.ShowCondition(args.Paramter, GetVarNodes(args.Paramter), "MotionIconSmall", r =>
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        if (r.Parameters.TryGetValue<LCondition>("Condition", out var con))
                        {
                            args.Paramter.Value = con;
                            args.Paramter.Owner.UpdateReferences();
                            BuildDynCondtion(args.Paramter, con, module);
                        }
                    }

                    // 刷新参数列表
                    var src = ModuleObj;
                    ModuleObj = null;
                    ModuleObj = src;
                });
            }
            else if (args.Paramter.Type.HasImplementedRawGeneric(typeof(LArray<>)))
            {
                _dialogService.ShowArrayConfig(args.Paramter, r =>
                {
                    // 刷新参数列表
                    var src = ModuleObj;
                    ModuleObj = null;
                    ModuleObj = src;
                });
            }
            else if (args.Paramter.Type == (typeof(LStringEx)))
            {
                var pVars = eventBus.GetVariables(args.Paramter);

                _dialogService.ShowStringEx(args.Paramter, GetVarNodes(args.Paramter), "MotionIconSmall", r =>
                {
                    if (r.Parameters.TryGetValue<LStringEx>("StringEx", out var strEx))
                    {
                        args.Paramter.Value = strEx;
                        args.Paramter.Owner.UpdateReferences();
                    }

                    // 刷新参数列表
                    var src = ModuleObj;
                    ModuleObj = null;
                    ModuleObj = src;
                });
            }
            else if (args.Paramter.Type == (typeof(LStringMatch)))
            {
                _dialogService.ShowStringMatch(args.Paramter, r =>
                {
                    if (r.Parameters.TryGetValue<LStringMatch>("StringMatch", out var strEx))
                    {
                        args.Paramter.Value = strEx;
                        args.Paramter.Owner.UpdateReferences();
                    }

                    // 刷新参数列表
                    var src = ModuleObj;
                    ModuleObj = null;
                    ModuleObj = src;
                });
            }
            else if (args.Paramter.Type == (typeof(LExpression)))
            {
                // 记录GetVariables耗时
                var pVars = eventBus.GetVariables(args.Paramter);
                _dialogService.ShowExpression(args.Paramter, GetVarNodes(args.Paramter), "MotionIconSmall", true, r =>
                {
                    if (r.Parameters.TryGetValue<LExpression>("Express", out var strEx))
                    {
                        args.Paramter.Value = strEx;
                        args.Paramter.Owner.UpdateReferences();
                    }

                    // 刷新参数列表
                    var src = ModuleObj;
                    ModuleObj = null;
                    ModuleObj = src;
                });
            }
            else if (args.Paramter.Type == typeof(LStation))
            {
                var modules = _engine.GetStations();
                _dialogService.ShowStationDialog(modules, r =>
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        if (r.Parameters.TryGetValue<LStation>("Station", out var stationEx))
                        {
                            args.Paramter.Value = stationEx;
                        }
                    }
                    else if (r.Result == ButtonResult.Cancel)
                    {
                        args.Paramter.Value = null;
                    }

                    // 刷新参数列表
                    var src = ModuleObj;
                    ModuleObj = null;
                    ModuleObj = src;
                });
            }
            else if (args.Paramter.Type == typeof(LModule))
            {
                var m = args.Paramter.Owner as IMotionModule;
                _dialogService.ShowSelectModule(m, r =>
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        if (r.Parameters.TryGetValue<LModule>("Module", out var lModule))
                        {
                            args.Paramter.Value = lModule;
                        }
                    }
                    else if (r.Result == ButtonResult.No)
                    {
                        args.Paramter.Value = null;
                    }

                    // 刷新参数列表
                    var src = ModuleObj;
                    ModuleObj = null;
                    ModuleObj = src;
                });
            }
            else if (args.Paramter.Type == typeof(SocketAction))
            {
                _dialogService.ShowSlaveConfig(args.Paramter, previewKey, r =>
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        if (r.Parameters.TryGetValue("Slave", out SocketAction vSlave))
                        {
                            args.Paramter.Value = new SocketAction() { Name = vSlave.Name, Value = vSlave.Value };
                        }
                        r.Parameters.TryGetValue<string>("SearchKey", out previewKey);
                    }
                    else if (r.Result == ButtonResult.Cancel)
                    {
                        args.Paramter.Value = null;
                    }
                    // 刷新参数列表
                    var src = ModuleObj;
                    ModuleObj = null;
                    ModuleObj = src;
                });
            }
            else if (args.Paramter.Type == typeof(VAlarm))
            {
                _dialogService.ShowAlarmConfig(args.Paramter, previewKey, r =>
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        if (r.Parameters.TryGetValue("VAlarm", out VAlarm vAlarm))
                        {
                            args.Paramter.Value = vAlarm;

                            var module = args.Paramter.Owner as IMotionModule;
                            if (module != null)
                            {
                                var parameters = module.Parameters;

                                if (parameters.ContainsKey("AlarmCode") && !string.IsNullOrEmpty(vAlarm.AlarmKey))
                                {
                                    parameters["AlarmCode"].Value = vAlarm.AlarmKey;
                                }

                                if (parameters.ContainsKey("Message") && !string.IsNullOrEmpty(vAlarm.AlarmCN))
                                {
                                    parameters["Message"].Value = vAlarm.AlarmCN;
                                }

                                if (parameters.ContainsKey("Detail") && !string.IsNullOrEmpty(vAlarm.AlarmEn))
                                {
                                    parameters["Detail"].Value = vAlarm.AlarmEn;
                                }
                            }
                        }
                        else if (r.Parameters.TryGetValue("VAlarm", out IVirtualDevice vDevice))
                        {

                        }

                        r.Parameters.TryGetValue<string>("SearchKey", out previewKey);
                    }
                    else if (r.Result == ButtonResult.Cancel)
                    {
                        args.Paramter.Value = null;
                    }

                    var src = ModuleObj;
                    ModuleObj = null;
                    ModuleObj = src;
                });
            }
        }));

      
        

        /// <summary>
        /// 可以选择的节点几何信息
        /// </summary>
        private ObservableCollection<LNode> _refNodes;
        public ObservableCollection<LNode> RefNodes
        {
            get { return _refNodes; }
            set { SetProperty(ref _refNodes, value); }
        }

        /// <summary>
        /// 参数变更执行应用
        /// </summary>
        private DelegateCommand<MouseButtonEventArgs> _showlinkCommand;
        public DelegateCommand<MouseButtonEventArgs> ShowLinkCommand =>
            _showlinkCommand ?? (_showlinkCommand = new DelegateCommand<MouseButtonEventArgs>(ExcuteShowLinkCommand));

        private void ExcuteShowLinkCommand(MouseButtonEventArgs e)
        {
            var pGrid = e.Source as ParamGrid;

            if (!pGrid.HasPermission)
            {
                return;
            }

            // 表示菜单是否显示
            pGrid.Tag = null;

            if (e.OriginalSource is TextBlock block && block.Tag != null && block.Tag is ParameterAttribute p)
            {
                // 如果是输入参数，则jin
                if (p.ParamType == ParamType.IN)
                {
                    // 设置可以引用的节点信息
                    RefNodes = new ObservableCollection<LNode>();
                    foreach (var item in eventBus.GetRefNodes(p))
                    {
                        RefNodes.Add(item);
                    }

                    if (RefNodes.Count > 0 && p.RefOut != null)
                    {
                        RefNodes.Add(new LNode() { Text = "-" });
                        RefNodes.Add(new LNode() { Text = "Clear", Tag1 = p, Icon = "\ue625" });
                    }

                    if (RefNodes.Count > 0)
                    {
                        pGrid.Tag = "True";
                    }
                }
                else
                {
                    // 另存文件
                    SaveFileDialog saveDialog = new SaveFileDialog();

                    if (typeof(ISave).IsAssignableFrom(p.Type) && p.Value != null)
                    {
                        pGrid.Tag = "True";

                        var save = p.Value as ISave;
                        saveDialog.DefaultExt = save.SaveFormat[0];

                        StringBuilder sb = new StringBuilder();
                        foreach (var item in save.SaveFormat)
                        {
                            sb.Append($"{Luster.Motion.Assests.Langs.LangProvider.GetLang("SaveFile")}|{item}|");
                        }
                        saveDialog.Filter = sb.Remove(sb.Length - 1, 1).ToString();
                        if (saveDialog.ShowDialog() == true)
                        {
                            save.Save(saveDialog.FileName);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 参数变更执行应用
        /// </summary>
        private DelegateCommand<LNode> _linkCommand;

        public DelegateCommand<LNode> LinkCommand =>
            _linkCommand ?? (_linkCommand = new DelegateCommand<LNode>(ExcuteLinkCommand));

        private void ExcuteLinkCommand(LNode lNode)
        {
            var dst = lNode.Tag1 as ParameterAttribute;
            if (dst == null) return;

            if (lNode.Text == "Clear")
            {
                dst.RefOut = null;
            }
            else
            {
                var src = lNode.Tag as ParameterAttribute;
                dst.RefOut = src;
            }
        }

        /// <summary>
        /// 获取全局根节点
        /// </summary>
        /// <param name="icon"></param>
        /// <returns></returns>
        public LNode GetGlobalNode(string icon)
        {
            var gModel = eventBus.GetModule(GlobalModule.GlobalID);
            LNode gNode = new LNode()
            {
                Text = Luster.Motion.Assests.Langs.LangProvider.GetLang("GlobalVar"),
                Key = gModel.ID.ToString(),
                Tag = gModel,
                Icon = gModel.TaskFunction.Icon
            };

            foreach (var item in gModel.Parameters)
            {
                var pNode = new LNode()
                {
                    Key = item.Key,
                    Text = item.Value.CN,
                    Tag = item.Value,
                    Icon = icon
                };
                gNode.Children.Add(pNode);
            }

            return gNode;
        }

        public List<LNode> GetVarNodes(ParameterAttribute p)
        {
            var pRoot = eventBus.GetRootModule().GetTreeNode(true, "\xe699");

            var curModule = p.Owner as IMotionModule;
            if (curModule.Station == null)
            {
                curModule.UpdateStation();
            }

            // 如果工站是回零站，则只能引用回零站参数
            if (curModule.Station != null)
            {
                if (curModule.Station.TaskFunction is IHomeStation)
                {
                    pRoot.Children.RemoveAll(u => (u.Tag is IModule m && m.TaskFunction is not IHomeStation));
                }
                else if (curModule.Station.TaskFunction is ITestStation)
                {
                    pRoot.Children.RemoveAll(u => (u.Tag is IModule m && m.TaskFunction is not ITestStation));
                }
                else
                {
                    pRoot.Children.RemoveAll(u => u.Tag is IModule m && (m.TaskFunction is IHomeStation || m.TaskFunction is ITestStation));
                }
            }

            pRoot.Children.Insert(0, GetGlobalNode("\xe699"));
            return pRoot.Children;
        }
    }
}