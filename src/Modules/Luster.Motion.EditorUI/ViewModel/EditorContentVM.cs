#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       FlowContentVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.EditorUI.ViewModel
* 文 件 名:       FlowContentVM.cs
* 创建时间:       2022/5/26 13:48:57
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      681b88fd-2238-4112-9f48-ec7b675fe41b
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/26 13:48:57
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using HandyControl.Controls;
using Luster.Common.Assets;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.Tools.FlowChart;
using Luster.Common.Tools.FlowTreeChart;
using Luster.Control.Wpf.Motion;
using Luster.Control.Wpf.Motion.Flow;
using Luster.Motion.Assests.Langs;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.CommonUI.Models;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.EditorUI.Events;
using Luster.Motion.EditorUI.Extensions;
using Luster.Motion.EditorUI.Models;
using Luster.Motion.EditorUI.Views;
using Luster.Motion.TaskFlow.Engine;
using Luster.TaskFlow.Common;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Functions;
using Luster.TaskFlow.Common.Logics;
using Luster.TaskFlow.Common.Models;
using Luster.TaskFlow.Common.Module;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Enums;
using Luster.TaskFlow.Motion.Interfaces;
using Luster.TaskFlow.Motion.Logic;
using Microsoft.Win32;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Events;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
//using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Luster.Motion.EditorUI.ViewModel
{
    public class EditorContentVM : BaseFlowVM
    {
        private Dispatcher _dispatcher;

        /// <summary>
        /// 弹窗服务
        /// </summary>
        private IDialogService _dialogService;

        /// <summary>
        /// 流程编辑器
        /// </summary>
        private FlowEditor flowItem = null;
   

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="_eventBus"></param>
        /// <param name="moduleFactory"></param>
        /// <param name="dispatcher"></param>
        public EditorContentVM(FlowBus _eventBus, Dispatcher dispatcher, IDialogService dialogService, ICommonBus bus) : base(_eventBus, bus)
        {
            _dispatcher = dispatcher;
            _dialogService = dialogService;
            FlowItems = new List<FlowItem>();

            // 初始化要进行刷新
            RefreshUI();

            IsBtnEnabled = true;
        }


        protected override void RegisterEvent(IEventAggregator bus)
        {
            base.RegisterEvent(bus);

            // 按钮事件
            bus.GetEvent<OperationEvent>().Subscribe(op =>
            {
                IsBtnEnabled = (op.Dst == EngineStatus.Idle || op.Dst == EngineStatus.Ready || op.Dst == EngineStatus.Stop);
            });

            // 模块添加
            bus.GetEvent<ModuleAddEvent>().Subscribe((m) =>
            {
                RefreshUI("", () =>
                {
                    m.ModuleValid();

                    foreach (var item in FlowItems)
                    {
                        item.Select(item.Motion == m);
                    }
                });
            });

            // 模块移动
            bus.GetEvent<ModuleMovedEvent>().Subscribe((ms) =>
            {
                RefreshUI();
            });

            // 模块删除
            bus.GetEvent<ModuleRemoveEvent>().Subscribe((ms) => { RefreshUI(); });

            // 模块粘贴
            bus.GetEvent<ModulePastedEvent>().Subscribe((ms) => { RefreshUI(); });

            // 模块屏蔽
            bus.GetEvent<ModuleSkipEvent>().Subscribe((ms) => { RefreshUI(); });

            // 模块加载
            bus.GetEvent<ModuleLoadedEvent>().Subscribe((ms) =>
            {
                RefreshUI("", () =>
                {
                    // 默认加载第一个分支
                    if (IsMultiChild(ms) && ms.Children.Count > 0)
                    {
                        LoadedCommand.Execute(ms.Children[0]);
                    }
                });
            });

            // 模块模块
            bus.GetEvent<ModuleUpdateEvent>().Subscribe((ms) =>
            {
                RefreshUI();
            });

            // 配方加载
            bus.GetEvent<RecipeOpenEvent>().Subscribe(r =>
            {
                RefreshUI();
                Luster.Motion.CommonUI.GlobalProperty.IsEnabeld = true;
            });

            // 触发了回退或者前进按钮
            bus.GetEvent<TaskChangedEvent>().Subscribe(() =>
            {
                RefreshUI();
            });

            // 提取完成
            bus.GetEvent<ModuleCompositionEvent>().Subscribe((m) =>
            {
                RefreshUI();
            });

            bus.GetEvent<LangChangedEvent>().Subscribe((langName) =>
            {
                RefreshUI();
            });
        }

        /// <summary>
        /// 全局搜索
        /// </summary>
        protected override void BindGlobalCommnads()
        {
            GlobalCommands.CopyCommand.RegisterCommand(CopyCommand);
            GlobalCommands.PasteCommand.RegisterCommand(PasteCommand);
            GlobalCommands.DeleteCommand.RegisterCommand(RemoveCommand);
            GlobalCommands.RunCommand.RegisterCommand(RunAllCommand);
            GlobalCommands.RunOneCommand.RegisterCommand(RunOneCommand);
            GlobalCommands.RunNextCommand.RegisterCommand(RunNextCommand);
        }

        /// <summary>
        /// UI 界面刷新功能
        /// </summary>
        private void RefreshUI(string switchName = "", Action action = null)
        {
            //异步线程刷新功能
            Task.Run(() =>
            {
                //commonBus.EventBus.GetEvent<LoadingEvent>().Publish(true);
                var curModule = eventBus.GetCurrent();

                if (curModule == null) return;

                // 如果是根节点，那么就是数据流
                _dispatcher.Invoke(() =>
                {
                    IsDataFlow = curModule.Name == "Root";
                    FlowItems = BuildFlowItems(curModule, switchName);
                });

                // 构建当前所处层级
                Stack<IMotionModule> modules = new Stack<IMotionModule>();
                BuildMenus(curModule, modules);
                var breadList = new List<BreadItem>();
                while (true)
                {
                    if (modules.Count == 0)
                    {
                        break;
                    }
                    var module = modules.Pop();
                    string text = module.Alias;
                    if (module.Name == "Root")
                    {
                        text = Luster.Motion.Assests.Langs.LangProvider.GetLang("Home");
                    }

                    breadList.Add(new BreadItem() { Text = text, Value = module, IsLast = module == curModule, IsFirst = module.Name == "Root" });
                }

                _dispatcher.Invoke(() =>
                {
                    Breads = breadList;
                });


                // 函数调用
                action?.Invoke();
                // 移动会有动画造成闪烁
                // commonBus.EventBus.GetEvent<LoadingEvent>().Publish(false);
            });
        }

        private bool IsMultiChild(IModule module)
        {
            return module.TaskFunction is ISwitch || module.TaskFunction is IBranch;
        }

        /// <summary>
        /// 加载Switch
        /// </summary>
        /// <param name="module"></param>
        private List<FlowItem> BuildFlowItems(IMotionModule module, string defaultCondtion)
        {
            var list = new List<FlowItem>();
            IsSwitch = false;

            var mParent = module.Parent;
            if (mParent != null && IsMultiChild(mParent))
            {
                IsSwitch = true;
                SwitchButtons = new ObservableCollection<SwitchModel>();
                if (mParent.Children.Count > 0)
                {
                    foreach (var item in mParent.Children)
                    {
                        SwitchButtons.Add(new SwitchModel(item) { IsCurrent = item == module });
                    }
                }
            }

            // 如果是引用的模块，则更新对应的模块对象
            if (module.TaskFunction is IRefComposition refComp)
            {
                module = refComp.GetRefModule();
            }

            foreach (var item in module.Children)
            {
                var flowItem = new FlowItem(item);
                list.Add(flowItem);
            }

            return list;
        }

        /// <summary>
        /// 构建菜单
        /// </summary>
        /// <param name="motionModule"></param>
        /// <param name="modules"></param>
        private void BuildMenus(IMotionModule motionModule, Stack<IMotionModule> modules)
        {
            if (motionModule is ILogic)
            {
                modules.Push(motionModule);
                if (motionModule.Parent != null)
                {
                    BuildMenus(motionModule.Parent, modules);
                }
            }
        }

        /// <summary>
        /// 支持粘贴功能
        /// </summary>
        private bool _canPaste = false;
        public bool CanPaste
        {
            get { return _canPaste; }
            set { SetProperty(ref _canPaste, value); }
        }


        /// <summary>
        /// 是否数据流
        /// </summary>
        private bool _isDataFlow = false;
        public bool IsDataFlow
        {
            get { return _isDataFlow; }
            set
            {
                SetProperty(ref _isDataFlow, value);
            }
        }


        /// <summary>
        /// 按钮是否禁用
        /// </summary>
        private bool _isBtnEnabled = false;
        public bool IsBtnEnabled
        {
            get { return _isBtnEnabled; }
            set
            {
                SetProperty(ref _isBtnEnabled, value);
            }
        }


        #region 切换列表
        /// <summary>
        /// 是否分支功能
        /// </summary>
        private bool _isSwitch = false;
        public bool IsSwitch
        {
            get { return _isSwitch; }
            set { SetProperty(ref _isSwitch, value); }
        }

        /// <summary>
        /// Button 切换列表
        /// </summary>
        private ObservableCollection<SwitchModel> _switchButtons;
        public ObservableCollection<SwitchModel> SwitchButtons
        {
            get { return _switchButtons; }
            set { SetProperty(ref _switchButtons, value); }
        }

        #endregion
        /// <summary>
        /// 所处层级
        /// </summary>
        private List<BreadItem> _breads;
        public List<BreadItem> Breads
        {
            get { return _breads; }
            set { SetProperty(ref _breads, value); }
        }

        /// <summary>
        /// 流程
        /// </summary>
        private List<FlowItem> _flowItems;
        public List<FlowItem> FlowItems
        {
            get { return _flowItems; }
            set { SetProperty(ref _flowItems, value); }
        }



        /// <summary>
        /// 模块选择
        /// </summary>
        private DelegateCommand<FlowItemDownArgs> _selectedCommand;
        public DelegateCommand<FlowItemDownArgs> SelectedCommand => _selectedCommand ?? (_selectedCommand = new DelegateCommand<FlowItemDownArgs>((args) =>
        {
            if(!IsAdmin) return;
            var selectItems = args.SelectItems.Select(u => u.Tag as IMotionModule).ToArray();
            eventBus.OnSelected(selectItems);
        }));

        /// <summary>
        /// 模块移动
        /// </summary>
        private DelegateCommand<FlowItemMoveArgs> _movedCommand;
        public DelegateCommand<FlowItemMoveArgs> MovedCommand => _movedCommand ?? (_movedCommand = new DelegateCommand<FlowItemMoveArgs>((args) =>
        {
            if (!IsAdmin) return;

            if (IsDataFlow)
            {
                eventBus.OnStationMove(args.MoveItems[0].Tag as IMotionModule);
            }
            else
            {
                var moveNames = string.Join(",", args.MoveItems.Select(u => u.Name).ToArray());
                _dialogService.ShowConfirm($"确认将模块:{moveNames} 移动到:{args.MoveIndex + 1}?", r =>
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        var selectItems = args.MoveItems.Select(u => u.Tag as IMotionModule).ToList();
                        eventBus.OnMoveModule(selectItems, args.MoveIndex);
                    }
                });
            }
        }, (obj) => IsBtnEnabled).ObservesCanExecute(() => IsBtnEnabled));

        /// <summary>
        /// 模块添加输入参数
        /// </summary>
        private DelegateCommand<object> _addInCommand;
        public DelegateCommand<object> AddInCommand => _addInCommand ?? (_addInCommand = new DelegateCommand<object>((obj) =>
        {
            if (!IsAdmin) return;

            if (obj is IMotionModule module)
            {
                _dialogService.ShowAddInParam(module, null, r =>
                {
                    module.OnUpdate(Luster.TaskFlow.Common.Enums.ModuleUpdate.ParameterNum);
                });
            }
        }, (obj) => IsBtnEnabled).ObservesCanExecute(() => IsBtnEnabled));

        /// <summary>
        /// 打开3D点云
        /// </summary>
        private DelegateCommand<object> _openHoloCommand;
        public DelegateCommand<object> OpenHolo3DCommand => _openHoloCommand ?? (_openHoloCommand = new DelegateCommand<object>((obj) =>
        {
            var module = obj as IMotionModule;
            if (module.TaskFunction is IHolo3D holo3D)
            {
                var taskPath = module.Parameters[nameof(holo3D.TaskPath)].EditorValue as LPath;
                if (taskPath != null && !string.IsNullOrEmpty(taskPath.Path))
                {
                    // 直接进入
                    commonBus.NewOrOpenHolo3D(module, taskPath.Path, false);
                }
                else
                {
                    // 构建点云任务
                    _dialogService.ShowText("工程名称", "", r =>
                    {
                        if (r.Result == ButtonResult.OK)
                        {
                            var text = r.Parameters.GetValue<string>("Text");
                            commonBus.NewOrOpenHolo3D(module, text);
                        }
                    });
                }
            }
        }));


        /// <summary>
        /// 模块进入详情
        /// </summary>
        private DelegateCommand<object> _loadedCommand;
        public DelegateCommand<object> LoadedCommand => _loadedCommand ?? (_loadedCommand = new DelegateCommand<object>((obj) =>
        {
            IMotionModule motionModule = null;
            if (obj is IMotionModule module)
            {
                if (IsMultiChild(module))
                {
                    motionModule = module.Parent;
                }
                else
                {
                    motionModule = module;
                }
            }
            else if (obj is FlowDoubleClickArgs args)
            {
                if (args != null)
                {
                    motionModule = args.FlowItem.Tag as IMotionModule;

                    // 如果是引用模块，临时更新对应的分组模块的父节点，否则为空，无法找到对应的父节点
                    if (motionModule.TaskFunction is IRefComposition refComp)
                    {
                        var refModule = refComp.GetRefModule();
                        refModule.Parent = motionModule;
                    }
                }
            }

            // 新增：判断是否为Script模块
            if (motionModule != null && motionModule.TaskFunction?.Alias == "Script")
            {
                _dialogService.ShowCSharpEditorDialog(motionModule, motionModule.ID.ToString(), r => {});
                return;
            }

            if (motionModule != null && motionModule.TaskFunction?.Alias == "PythonScript")
            {
                _dialogService.ShowPythonEditorDialog(motionModule, motionModule.ID.ToString(), r => {});
                return;
            }

            if (motionModule != null && motionModule != eventBus.GetCurrent())
            {
                eventBus.OnLoaded(motionModule);
            }
        }));


        /// <summary>
        /// 模块进入详情
        /// </summary>
        private DelegateCommand<object> _editorLoadedCommand;
        public DelegateCommand<object> EditorLoadedCommand => _editorLoadedCommand ?? (_editorLoadedCommand = new DelegateCommand<object>((obj) =>
        {
            System.Windows.RoutedEventArgs args = obj as System.Windows.RoutedEventArgs;
            flowItem = args.Source as FlowEditor;
        }));


        private List<IMotionModule> GetSelectModules()
        {
            return FlowItems.Where(u => u.IsSelected)
                        .Select(u => u.Motion)
                        .OrderBy(u => u.Sort)
                        .ToList();
        }

        /// <summary>
        /// 模块移除
        /// </summary>
        private DelegateCommand _removeCommand;
        public DelegateCommand RemoveCommand => _removeCommand ?? (_removeCommand = new DelegateCommand(() =>
        {
            if (!IsAdmin) return;

            List<IMotionModule> modules = GetSelectModules();

            var removeItems = modules.ToArray();
            _dialogService.ShowConfirm($"确认删除模块:{string.Join(",", removeItems.Select(u => u.Alias).ToArray())}?", r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    eventBus.OnRemoveModule(removeItems.ToArray());
                }
            });
        }, () => IsBtnEnabled).ObservesCanExecute(() => IsBtnEnabled));

        /// <summary>
        /// 模块移除
        /// </summary>
        private DelegateCommand<object> _removeLineCommand;
        public DelegateCommand<object> RemoveLineCommand => _removeLineCommand ?? (_removeLineCommand = new DelegateCommand<object>((obj) =>
        {
            if (obj is FlowLine flowLine)
            {
                eventBus.OnRemovePrevStation(flowLine.StartItem.ID, flowLine.EndItem.ID);
            }
        }, (obj) => IsBtnEnabled).ObservesCanExecute(() => IsBtnEnabled));

        /// <summary>
        /// 模块提取
        /// </summary>
        private DelegateCommand<string> _extractCommand;
        public DelegateCommand<string> ExtractCommand => _extractCommand ?? (_extractCommand = new DelegateCommand<string>((typeStr) =>
        {
            var sModules = GetSelectModules();
            var num = sModules.Count();
            if (typeStr == "Switch")
            {
                _dialogService.ShowSwitchConfig(num, r =>
                {
                    if (r.Result != ButtonResult.OK) return;
                    if (r.Parameters.TryGetValue<LCondition>("Condition", out var condition))
                    {
                        string subGroup = r.Parameters.GetValue<String>("SubGroup");
                        string switchText = r.Parameters.GetValue<String>("SwitchText");
                        eventBus.OnExtractSwitch(condition, switchText, subGroup, sModules.ToArray());
                    }
                });
            }
            else
            {
                _dialogService.ShowText(typeStr, typeStr, r =>
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        string alias = r.Parameters.GetValue<string>("Text");
                        eventBus.OnExtract(typeStr, alias, sModules.ToArray());
                    }
                });
            }
        }, (obj) => IsBtnEnabled).ObservesCanExecute(() => IsBtnEnabled));

        /// <summary>
        /// 模块提取
        /// </summary>
        private DelegateCommand<string> _compositionCommand;
        public DelegateCommand<string> CompositionCommand => _compositionCommand ?? (_compositionCommand = new DelegateCommand<string>((typeStr) =>
        {
            var sModules = GetSelectModules();
            var num = sModules.Count();

            _dialogService.ShowText(typeStr, typeStr, r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    string alias = r.Parameters.GetValue<string>("Text");
                    eventBus.OnAddCompostion(alias, sModules.ToArray());
                }
            });
        }, (obj) => IsBtnEnabled).ObservesCanExecute(() => IsBtnEnabled));

        /// <summary>
        /// 模块复制
        /// </summary>
        private DelegateCommand _copyCommand;
        public DelegateCommand CopyCommand => _copyCommand ?? (_copyCommand = new DelegateCommand(() =>
        {
            if (!IsAdmin) return;

            var _copyModules = FlowItems.Where(u => u.IsSelected).Select(u => u.Motion).ToList();
            CanPaste = _copyModules != null && _copyModules.Count > 0;
            if (_copyModules.Count() == 0)
            {
                Clipboard.Clear();
                _dialogService.ShowInfoTip("复制需要选中模块[根节点不支持复制]");
                return;
            }

            XElement xCopy = new XElement("XCopy");
            foreach (var item in _copyModules)
            {
                xCopy.Add(item.ExportXml());
            }

            IDataObject data = new DataObject(DataFormats.Text, xCopy.ToString());
            try
            {
                Clipboard.SetDataObject(data, true);
            }
            catch (System.Runtime.InteropServices.COMException)
            {
            }

        }, () => IsBtnEnabled).ObservesCanExecute(() => IsBtnEnabled));

        /// <summary>
        /// 模块粘贴
        /// </summary>
        private DelegateCommand _pasteCommand;
        public DelegateCommand PasteCommand => _pasteCommand ?? (_pasteCommand = new DelegateCommand(() =>
        {
            if (!IsAdmin) return;

            string text = Clipboard.GetText();
            if (text.Contains("Name=\"Stations\""))
            {
                IMotionModule parent = eventBus.GetCurrent();
                if (parent == null || parent.Alias != "Group")
                {
                    _dialogService.ShowErrorTip("站点只能粘贴到数据流根节点下");
                    return;
                }
            }
            

            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            int insert = -1;

            var selectModule = GetSelectModules().LastOrDefault();
            if (selectModule != null)
            {
                insert = selectModule.Sort + 1;
            }

            var xCopy = XElement.Parse(text);
            eventBus.OnPaste(xCopy, null, insert);

        }, () => CanPaste).ObservesProperty(() => CanPaste));

        /// <summary>
        /// 模块粘贴
        /// </summary>
        private DelegateCommand<string> _stationCommand;
        public DelegateCommand<string> StationCommand => _stationCommand ?? (_stationCommand = new DelegateCommand<string>((type) =>
        {
            var module = eventBus.OnAddModule("Logic", "StationSet");
            var sType = (StationType)Enum.Parse(typeof(StationType), type);
            module.SetInParameter("StationType", sType);
            module.Alias = L(type);
        }));

        /// <summary>
        /// 模块忽略
        /// </summary>
        private DelegateCommand<object> _skipCommand;
        public DelegateCommand<object> SkipCommand => _skipCommand ?? (_skipCommand = new DelegateCommand<object>((obj) =>
        {
            if (!IsAdmin) return;
            _dialogService.ShowConfirm($"确认要忽略当前模块?", r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    var selectModules = obj as IEnumerable<IMotionModule>;
                    eventBus.OnSkip(selectModules.ToArray());
                }
            });
           
        }, (obj) => IsBtnEnabled).ObservesCanExecute(() => IsBtnEnabled));


        /// <summary>
        /// 跳转到模块引用
        /// </summary>
        private DelegateCommand<IMotionModule> _goToRefCommand;
        public DelegateCommand<IMotionModule> GoToRefCommand => _goToRefCommand ?? (_goToRefCommand = new DelegateCommand<IMotionModule>((m) =>
        {
            // 切换到模块所在层级
            eventBus.OnLoaded(m.Parent, m);
        }));

        /// <summary>
        /// 模块单步运行
        /// </summary>
        private DelegateCommand _runOneCommand;
        public DelegateCommand RunOneCommand => _runOneCommand ?? (_runOneCommand = new DelegateCommand(() =>
        {
            if (!IsAdmin) return;

            var selectIds = GetSelectModules().Select(u => u.ID).ToArray();
            if (selectIds != null && selectIds.Count() > 0)
            {
                eventBus.OnRunOne(selectIds);
            }
            else
            {
                // 默认运行第一个
                if (FlowItems.Count > 0)
                {
                    eventBus.OnRunOne(FlowItems[0].ID);
                }
            }
        }, () => IsBtnEnabled).ObservesCanExecute(() => IsBtnEnabled));

        /// <summary>
        /// 模块单步运行
        /// </summary>
        private DelegateCommand _runAllCommand;
        public DelegateCommand RunAllCommand => _runAllCommand ?? (_runAllCommand = new DelegateCommand(() =>
        {
            if (true) return;
            if (!IsAdmin) return;

            var module = eventBus.GetCurrent();
            eventBus.OnRun(module.ID);
        }, () => IsBtnEnabled).ObservesCanExecute(() => IsBtnEnabled));

        /// <summary>
        /// 模块单步运行
        /// </summary>
        private DelegateCommand _openEditorCommand;
        public DelegateCommand OpenEditorCommand => _openEditorCommand ?? (_openEditorCommand = new DelegateCommand(() =>
        {
            if (!IsAdmin) return;

            // 打开脚本编辑器
            string editorPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tools", "RoslynPad", $"RoslynPad.exe");
            if (!System.IO.File.Exists(editorPath))
            {
                return;
            }

            var process = System.Diagnostics.Process.GetProcessesByName("RoslynPad");
            if (process.Count() == 0)
            {
                var startInfo = new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = editorPath,
                };

                System.Diagnostics.Process.Start(startInfo);
            }
        }, () => IsBtnEnabled).ObservesCanExecute(() => IsBtnEnabled));

        /// <summary>
        /// 运行下一步
        /// </summary>
        private DelegateCommand _runNextCommand;
        public DelegateCommand RunNextCommand => _runNextCommand ?? (_runNextCommand = new DelegateCommand(() =>
        {
            if (!IsAdmin) return;

            if (IsDataFlow) return;
            var curItem = FlowItems.LastOrDefault(u => u.IsSelected);
            if (curItem != null)
            {
                // 将当前选中项目关闭
                curItem.Select(false);

                var next = curItem.Motion.NextModule;
                if (next != null)
                {
                    var nextItem = FlowItems.FirstOrDefault(u => u.ID == next.ID);
                    nextItem.Select(true);
                    eventBus.OnRunOne(next.ID);
                }
            }
        }, () => IsBtnEnabled).ObservesCanExecute(() => IsBtnEnabled));

        /// <summary>
        /// 运行下一步
        /// </summary>
        private DelegateCommand _stopCommand;
        public DelegateCommand StopCommand => _stopCommand ?? (_stopCommand = new DelegateCommand(() =>
        {
            if (!IsAdmin) return;

            eventBus.OnStop();
        }, () => IsBtnEnabled).ObservesCanExecute(() => IsBtnEnabled));

        /// <summary>
        /// 运行下一步
        /// </summary>
        private DelegateCommand<object> _showLogCommand;
        public DelegateCommand<object> ShowLogCommand => _showLogCommand ?? (_showLogCommand = new DelegateCommand<object>((obj) =>
        {
            var m = obj as IMotionModule;
            if (m != null)
            {
                eventBus.Bus.GetEvent<LogMonitorEvent>().Publish(m);
            }
        }));

        /// <summary>
        /// 拖拽模块
        /// </summary>
        private DelegateCommand<object> _dropCommand;
        public DelegateCommand<object> DropCommand => _dropCommand ?? (_dropCommand = new DelegateCommand<object>((obj) =>
        {
            if (!IsAdmin) return;

            ItemDropArgs args = obj as ItemDropArgs;
            if (args != null)
            {
                var node = args.Args.Data.GetData(typeof(ModuleNode)) as ModuleNode;
                var module = node.Tag1 as IModule;
                var function = node.Tag as IFunction;
                IMotionModule newModule = null;
                if (IsDataFlow)
                {
                    newModule = eventBus.OnAddModule(module.Name, function.Name, -1, args.Row, args.Column);
                }
                else
                {
                    if (function == null && node.Tag.ToString() == "Composition")
                    {
                        newModule = eventBus.OnAddModule("Logic", "RefGroup", args.Index);
                        if (newModule.TaskFunction is IRefComposition comp)
                        {
                            comp.SetRefModule(module as IMotionModule);
                            newModule.Alias = node.Text;
                        }
                    }
                    else
                    {
                        // 计算位置
                        newModule = eventBus.OnAddModule(module.Name, function.Name, args.Index);
                    }
                }

                eventBus.OnSelected(newModule);
            }

        }, (obj) => IsBtnEnabled).ObservesCanExecute(() => IsBtnEnabled));

        /// <summary>
        /// 多视图查看
        /// </summary>
        private DelegateCommand _compareLookCommand;
        public DelegateCommand CompareLookCommand => _compareLookCommand ?? (_compareLookCommand = new DelegateCommand(() =>
        {
            if (!IsDataFlow) return;
            var selectModules = GetSelectModules();
            eventBus.OnCompareLook(selectModules);
        }));


        /// <summary>
        /// 导出视图
        /// </summary>
        private DelegateCommand _exportImgCommand;
        public DelegateCommand ExportImageCommand => _exportImgCommand ?? (_exportImgCommand = new DelegateCommand(() =>
        {
            if (!IsDataFlow) return;

            var rootMotion = eventBus.GetCurrent();
            var rootNode = rootMotion.GetTreeNode();
            //string json = JsonConvert.SerializeObject(rootNode);
            var bmp = Luster.Common.Tools.ImageTool.SaveNodesToImage(commonBus.ProjInfo.ProjName, 60, rootNode.Children.ToArray());
            SaveFileDialog saveDialog = new SaveFileDialog();

            saveDialog.Filter = "图片|*.png";
            if (saveDialog.ShowDialog() == true)
            {
                bmp.Save(saveDialog.FileName);
            }
        }));

        /// <summary>
        /// 导出流程树
        /// </summary>
        private DelegateCommand _exportFlowCommand;
        public DelegateCommand ExportFlowCommand => _exportFlowCommand ?? (_exportFlowCommand = new DelegateCommand(() =>
        {
            #region 流程树相关
            //if (!IsDataFlow)
            //{
            //    return;
            //}

            //var rootMotion = eventBus.GetCurrent();
            //var rootNode = rootMotion.GetTreeNode().Children.ToArray()[0];
            //rootNode.Text = commonBus.ProjInfo.ProjName+"_"+ rootNode.Text;
            //SaveFileDialog saveDialog = new SaveFileDialog();
            //saveDialog.Filter = "图片|*.png";
            //if (saveDialog.ShowDialog() == true)
            //{
            //    FlowTreeImageRenderer flowTreeImageRenderer = new FlowTreeImageRenderer();
            //    flowTreeImageRenderer.Renderer(rootNode, saveDialog.FileName);
            //}

            //using (var folderDialog = new System.Windows.Forms.FolderBrowserDialog())
            //{
            //    folderDialog.RootFolder = Environment.SpecialFolder.MyComputer;
            //    if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //    {
            //        string selectedFolderPath = folderDialog.SelectedPath;
            //        if (!Directory.Exists(selectedFolderPath))
            //        {
            //            Directory.CreateDirectory(selectedFolderPath);
            //        }
            //        else
            //        {
            //            var rootMotion = eventBus.GetCurrent();

            //            LNode root =rootMotion.GetTreeNode();
            //            //string json = JsonConvert.SerializeObject(root);

            //            System.Threading.Tasks.Parallel.ForEach(rootMotion.GetTreeNode().Children, r => {

            //                //string json = JsonConvert.SerializeObject(r);

            //                FlowTreeImageRenderer flowTreeImageRenderer = new FlowTreeImageRenderer();
            //                string fileName= System.IO.Path.Combine(folderDialog.SelectedPath, commonBus.ProjInfo.ProjName+"_" +r.Text+ ".png");
            //                flowTreeImageRenderer.Renderer(r, fileName);
            //            });
            //        }
            //    }
            //}

            #endregion

            #region 流程图相关

            if (!IsDataFlow)
            {
                return;
            }

            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "PDF files(*.PDF)|*.pdf" +
            "|JPG Files(*.JPG)|*.jgp" +
            "|BMP Files(*.BMP)|*.bmp" +
            "|GIF Files(*.GIF)|*.gif" +
            "|Png Files(*.PNG)|*.png" +
            "|SVG Files (*.SVG)|*.svg" +
            "|MSAGL Files (*.MSAGL)|*.msagl";

            string projName = commonBus.ProjInfo.ProjName + DateTime.Now.ToString("yyyyMMddHHmmss");
            saveDialog.FileName = projName;

            if (saveDialog.ShowDialog() == true)
            {
                Task.Factory.StartNew(() =>
                {
                    var rootMotion = eventBus.GetCurrent();

                    //根节点处理
                    var rootNode = rootMotion.GetTreeNodeForFlow();
                    string fileName = saveDialog.FileName;
                    rootNode.FunctionName = "Parallel";
                    rootNode.Text = "开始";

                    //选择工站处理
                    if (rootNode.Children.Count > 0)
                    {
                        List<LNode> nodes = new List<LNode>();
                        foreach (var child in rootNode.Children)
                        {
                            if (child.IsSelected)
                            {
                                nodes.Add(child);
                            }
                        }

                        //替换掉孩子节点
                        if (nodes.Count > 0)
                        {
                            rootNode.Children = nodes;
                        }
                    }

                    FlowChartData flowChartData = new FlowChartData();
                    flowChartData.InitFlowChartData(rootNode);

                    string ext = System.IO.Path.GetExtension(fileName);
                    if (ext.Equals(".svg", StringComparison.OrdinalIgnoreCase))
                    {
                        FlowChartData.SaveToSVG(fileName, flowChartData);
                    }
                    else if (ext.Equals(".msagl", StringComparison.OrdinalIgnoreCase))
                    {
                        FlowChartData.SaveToMsagl(fileName, flowChartData);
                    }
                    else
                    {
                        FlowChartData.SaveToImage(fileName, flowChartData);
                    }
                });
            }

            #region 开发调试
            //rootNode.FunctionName = "Parallel";
            //rootNode.Text = "开始";
            //FlowChartData flowChartData = new FlowChartData();
            //flowChartData.InitFlowChartData(rootNode);
            //string path = @"C:\FlowChartData.bin";
            //FlowChartData.SaveBinaryLegacy(path, flowChartData);
            //FlowChartData flowChartData2= FlowChartData.LoadBinaryLegacy(path);

            //rootNode.Text = commonBus.ProjInfo.ProjName;
            //SaveFileDialog saveDialog = new SaveFileDialog();
            //saveDialog.Filter = "图片|*.png";
            //if (saveDialog.ShowDialog() == true)
            //{
            //    FlowTreeImageRenderer flowTreeImageRenderer = new FlowTreeImageRenderer();
            //    flowTreeImageRenderer.Renderer(rootNode, saveDialog.FileName);
            //}

            //using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            //{
            //    dialog.Description = "请选择导出的流程保存位置";
            //    dialog.SelectedPath = AppDomain.CurrentDomain.BaseDirectory;
            //    string projName = commonBus.ProjInfo.ProjName;

            //    if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //    {
            //        string folderPath = dialog.SelectedPath;
            //        foreach (var node in rootNode.Children.ToArray())
            //        {
            //            try
            //            {
            //                string path = Path.Combine(folderPath, $"{projName}_{node.Text}.png");
            //                FlowTreeImageRenderer flowTreeImageRenderer = new FlowTreeImageRenderer();
            //                flowTreeImageRenderer.Renderer(node, path);
            //            }
            //            catch
            //            {

            //            }
            //        }
            //    }
            //}
            #endregion

            #endregion

        }));
    }
}