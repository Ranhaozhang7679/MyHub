#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       WorkFlowContentVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.CommonUI.ViewModel
* 文 件 名:       WorkFlowContentVM.cs
* 创建时间:       2022/6/27 20:56:20
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      8434699f-e027-4e87-acdc-0acaf0a39ecf
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/27 20:56:20
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.Assets;
using Luster.Control.Wpf.Motion.Flow;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Dock;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.CommonUI.Models;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.EditorUI.Extensions;
using Luster.Motion.TaskFlow.Engine;
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
using System.Windows.Threading;

namespace Luster.Motion.SubSystem.ViewModel
{
    public class WorkFlowContentVM : MotionPageVM, IDockContent
    {
        /// <summary>
        /// 工作流程
        /// </summary>
        private WorkFlow _workFlow;

        /// <summary>
        /// 流程
        /// </summary>
        private Dispatcher _dispatcher;

        /// <summary>
        /// 对话框
        /// </summary>
        private IDialogService _dialogService;

        /// <summary>
        /// 引擎
        /// </summary>
        private IMotionEngine _mEngine = null;

        /// <summary>
        /// 工艺流页面
        /// </summary>
        /// <param name="_commonBus"></param>
        /// <param name="mEngine"></param>
        /// <param name="dispatcher"></param>
        public WorkFlowContentVM(ICommonBus _commonBus,
            IMotionEngine mEngine,
            Dispatcher dispatcher,
            IDialogService dialogService) : base(_commonBus)
        {
            _dialogService = dialogService;
            _workFlow = mEngine.WorkFlow;
            _mEngine = mEngine;
            _dispatcher = dispatcher;
            WorkItems = new List<WorkItemModel>();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public WorkFlowContentVM() { }

        protected override void RegisterEvent(IEventAggregator bus)
        {
            base.RegisterEvent(bus);

            // 添加工艺
            bus.GetEvent<WorkItemAddedEvent>().Subscribe(w =>
            {
                _workFlow.AddItem(w);
                RefreshUI();
            });

            // 配方加载
            bus.GetEvent<RecipeOpenEvent>().Subscribe(w =>
            {
                RefreshUI();
            });
        }


        private void RefreshUI()
        {
            _dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
            {
                WorkItems = _workFlow.Select(u => new WorkItemModel(u)).ToList();
            }));
        }

        /// <summary>
        /// 对应的Key
        /// </summary>
        public string Name => "WorkFlow";

        /// <summary>
        /// 对应的区域
        /// </summary>
        public string RegionName { get; set; } = "WorkFlowContent";

        /// <summary>
        /// 对象集合
        /// </summary>
        private List<WorkItemModel> _workItems;
        public List<WorkItemModel> WorkItems
        {
            get { return _workItems; }
            set { SetProperty(ref _workItems, value); }
        }

        /// <summary>
        /// 模块进入详情
        /// </summary>
        private DelegateCommand<object> _loadedCommand;
        public DelegateCommand<object> LoadedCommand => _loadedCommand ?? (_loadedCommand = new DelegateCommand<object>((obj) =>
        {
            // 加载界面
            RefreshUI();
        }));

        /// <summary>
        /// 模块删除
        /// </summary>
        private DelegateCommand<object> _removeCommand;
        public DelegateCommand<object> RemoveCommand => _removeCommand ?? (_removeCommand = new DelegateCommand<object>((obj) =>
        {
            var list = obj as List<WorkItem>;
            if (list != null)
            {
                var names = string.Join(",", list.Select(u => u.Name).ToArray());
                _dialogService.ShowConfirm($"{Luster.Motion.Assests.Langs.LangProvider.GetLang("ConfirmDelete")}:{names}?", r =>
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        _workFlow.RemoveAll(u => list.Contains(u));
                        RefreshUI();
                    }
                });
            }
        }));

        /// <summary>
        /// 模块编辑
        /// </summary>
        private DelegateCommand<object> _editCommand;
        public DelegateCommand<object> EditCommand => _editCommand ?? (_editCommand = new DelegateCommand<object>((obj) =>
        {
            var list = obj as List<WorkItem>;
            if (list != null && list.Count > 0)
            {
                var wItem = list[0];
                _dialogService.ShowWorkFlowSet(list[0], (r) =>
                {
                    if (r.Result == ButtonResult.OK)
                    {
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
                    }
                });
            }
        }));

        /// <summary>
        /// 模块编辑
        /// </summary>
        private DelegateCommand _addCommand;
        public DelegateCommand AddCommand => _addCommand ?? (_addCommand = new DelegateCommand(() =>
        {
            _dialogService.ShowWorkFlowSet(null, (r) =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    WorkItem wItem = new WorkItem();
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
        /// 删除连接线
        /// </summary>
        private DelegateCommand<object> _removeLineCommand;
        public DelegateCommand<object> RemoveLineCommand => _removeLineCommand ?? (_removeLineCommand = new DelegateCommand<object>((obj) =>
        {
            var line = obj as FlowLine;
            if (line != null)
            {
                var wItem = line.EndItem.Tag as WorkItem;
                if (wItem.PrevItems.Count > 0)
                {
                    var prevItem = line.StartItem.Tag as WorkItem;
                    wItem.PrevItems.RemoveAll(u => u.ID == prevItem.ID);
                    wItem.PrevIds.RemoveAll(u => u == prevItem.ID);
                    RefreshUI();
                }
            }
        }));
    }
}
