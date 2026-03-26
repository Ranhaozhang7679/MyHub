#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ModuleDifferContentVM
* 机器名称:       L05590
* 命名空间:       Luster.Motion.EditorUI.ViewModel
* 文 件 名:       ModuleDifferContentVM.cs
* 创建时间:       2022/11/23 13:43:13
* 作    者:       L05590
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       fanlu5590@lusterinc.com 
* 唯一标识：      14f36945-c2da-49c6-93de-4719948ccac1
* 登录用户:       fanlu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/11/23 13:43:13
* 修 改 人:		  L05590
************************************************************************************/
#endregion

using Luster.Common.Assets;
using Luster.Control.Wpf.Motion.Flow;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.CommonUI.Models;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.EditorUI.Events;
using Luster.Motion.EditorUI.Extensions;
using Luster.Motion.EditorUI.Models;
using Luster.Motion.TaskFlow.Engine.Models;
using Luster.TaskFlow.Motion;
using Prism.Commands;
using Prism.Events;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Xml.Linq;

namespace Luster.Motion.EditorUI.ViewModel
{
    public class FlowViewContentVM : MotionPageVM
    {

        private Dispatcher _dispatcher;

        /// <summary>
        /// 流程Bus
        /// </summary>
        private FlowBus flowBus;

        /// <summary>
        /// 弹窗
        /// </summary>
        private IDialogService _dialogService;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="commonBus"></param>
        /// <param name="_flowBus"></param>
        /// <param name="Dispatcher"></param>
        public FlowViewContentVM(ICommonBus commonBus, IDialogService dialogService, FlowBus _flowBus, Dispatcher Dispatcher) : base(commonBus)
        {
            flowBus = _flowBus;
            _dispatcher = Dispatcher;
            _dialogService = dialogService;
            StationList = new ObservableCollection<CompareModuleModel>();
            IsBtnEnabled = true;
        }

        /// <summary>
        /// 工站模块
        /// </summary>
        private List<IMotionModule> _stationModules = null;

        /// <summary>
        /// CT集合
        /// </summary>
        private ObservableCollection<CompareModuleModel> _stationList;
        public ObservableCollection<CompareModuleModel> StationList
        {
            get { return _stationList; }
            set { SetProperty(ref _stationList, value); }
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

        protected override void RegisterEvent(IEventAggregator bus)
        {
            // 按钮事件
            bus.GetEvent<OperationEvent>().Subscribe(op =>
            {
                IsBtnEnabled = op.Dst != EngineStatus.Running;
            });


            bus.GetEvent<CompareLookEvent>().Subscribe(arrayModules =>
            {
                _stationModules = arrayModules.ToList();
                RefreshUI();
            });

            bus.GetEvent<RecipeOpenEvent>().Subscribe(recipe =>
            {
                _dispatcher.Invoke(() =>
                {
                    StationList.Clear();
                });
            });

            // 模块新增
            bus.GetEvent<ModuleAddEvent>().Subscribe(r =>
            {
                RefreshUI();
            });

            // 移除
            bus.GetEvent<ModuleRemoveEvent>().Subscribe(ms =>
            {
                RefreshUI();
            });

            // 忽略
            bus.GetEvent<ModuleSkipEvent>().Subscribe(ms =>
            {
                RefreshUI();
            });
        }

        /// <summary>
        /// 刷新
        /// </summary>
        private void RefreshUI()
        {
            _dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
            {
                StationList?.Clear();

                if (_stationModules != null)
                {
                    foreach (var item in _stationModules)
                    {
                        var moduleCT = new ModuleCT(item);
                        moduleCT.IsExpanded = true;

                        RefreshChildren(item, moduleCT);
                        var compareModule = new CompareModuleModel()
                        {
                            Name = item.Alias,
                            MotionChildList = new List<ModuleCT>() { moduleCT }
                        };

                        StationList.Add(compareModule);
                    }
                }
            }));
        }

        /// <summary>
        /// 递归构建树
        /// </summary>
        /// <param name="m"></param>
        /// <param name="p"></param>
        private void RefreshChildren(IMotionModule m, ModuleCT p)
        {
            foreach (var item in m.Children)
            {
                var moduleCT = new ModuleCT(item);
                p.Children.Add(moduleCT);

                RefreshChildren(item, moduleCT);
            }
        }

        /// <summary>
        /// 模块选择
        /// </summary>
        private DelegateCommand<object> _loadedCommand;
        public DelegateCommand<object> LoadedCommand => _loadedCommand ?? (_loadedCommand = new DelegateCommand<object>((obj) =>
        {
            if (obj is ModuleCT mCT)
            {
                flowBus.OnLoaded(mCT.Tag.Parent, mCT.Tag);
            }
        }));

        private DelegateCommand<object> _selectCommand;
        public DelegateCommand<object> SelectCommand => _selectCommand ?? (_selectCommand = new DelegateCommand<object>((obj) =>
        {
            var args = obj as SelectionChangedEventArgs;
            if (args.AddedItems.Count > 0 && args.AddedItems[0] is ModuleCT mCT)
            {
                flowBus.OnSelected(mCT.Tag);
            }
        }));

        /// <summary>
        /// 模块单步运行
        /// </summary>
        private DelegateCommand<IMotionModule> _runOneCommand;
        public DelegateCommand<IMotionModule> RunOneCommand => _runOneCommand ?? (_runOneCommand = new DelegateCommand<IMotionModule>((module) =>
        {
            flowBus.OnRunOne(module.ID);
        }, (obj) => IsBtnEnabled).ObservesCanExecute(() => IsBtnEnabled));

        /// <summary>
        /// 模块忽略
        /// </summary>
        private DelegateCommand<IMotionModule> _skipCommand;
        public DelegateCommand<IMotionModule> SkipCommand => _skipCommand ?? (_skipCommand = new DelegateCommand<IMotionModule>((module) =>
        {
            flowBus.OnSkip(module);
        }, (obj) => IsBtnEnabled).ObservesCanExecute(() => IsBtnEnabled));


        /// <summary>
        /// 设置工作流
        /// </summary>
        private DelegateCommand<IMotionModule> _setWorkFlowCommand;
        public DelegateCommand<IMotionModule> SetWorkFlowCommand => _setWorkFlowCommand ?? (_setWorkFlowCommand = new DelegateCommand<IMotionModule>((module) =>
        {
            var workItem = new WorkItem()
            {
                StartItem = module
            };
            _dialogService.ShowWorkFlowSet(workItem, (r) =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    WorkItem wItem = new WorkItem() { StartItem = module };
                    if (r.Parameters.TryGetValue("StopModule", out IMotionModule stopModule))
                    {
                        wItem.EndItem = stopModule;
                    }
                    if (r.Parameters.TryGetValue("StartModule", out IMotionModule startModule))
                    {
                        wItem.StartItem = startModule;
                    }
                    if (r.Parameters.TryGetValue("FlowName", out string flowName))
                    {
                        wItem.Name = flowName;
                    }

                    commonBus.EventBus.GetEvent<WorkItemAddedEvent>().Publish(wItem);
                }
            });
        }));

        /// <summary>
        /// 模块移除
        /// </summary>
        private DelegateCommand<IMotionModule> _removeCommand;
        public DelegateCommand<IMotionModule> RemoveCommand => _removeCommand ?? (_removeCommand = new DelegateCommand<IMotionModule>((module) =>
        {
            _dialogService.ShowConfirm($"{Luster.Motion.Assests.Langs.LangProvider.GetLang("ConfirmDeleteModule")}:{module.Alias}?", r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    flowBus.OnRemoveModule(module);
                }
            });
        }, (obj) => IsBtnEnabled).ObservesCanExecute(() => IsBtnEnabled));

        /// <summary>
        /// 右键添加等待模块
        /// </summary>
        private DelegateCommand<object> _toWaitCommand;
        public DelegateCommand<object> ToWaitCommand => _toWaitCommand ?? (_toWaitCommand = new DelegateCommand<object>((obj) =>
        {
            if (obj is ModuleCT mCT)
            {
                // 添加新模块
                var newModule = flowBus.OnAddModule("Logic", "WaitStatus");
                if (mCT.Tag.Parameters.ContainsKey("Status"))
                {
                    var statusParam = mCT.Tag.Parameters["Status"];
                    newModule.Parameters["InStatus"].RefOut = statusParam;
                    newModule.Alias = $"{Luster.Motion.Assests.Langs.LangProvider.GetLang("Wait")}_{mCT.Tag.Alias}";
                }
            }
        }));

        /// <summary>
        /// 模块复制
        /// </summary>
        private DelegateCommand<IMotionModule> _copyCommand;
        public DelegateCommand<IMotionModule> CopyCommand => _copyCommand ?? (_copyCommand = new DelegateCommand<IMotionModule>((m) =>
        {
           
            XElement xCopy = new XElement("XCopy");
            xCopy.Add(m.ExportXml());

            IDataObject data = new DataObject(DataFormats.Text, xCopy.ToString());
            try
            {
                Clipboard.SetDataObject(data, true);
            }
            catch (System.Runtime.InteropServices.COMException)
            {
            }
        }));
    }
}