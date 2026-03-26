#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       SetWorkFlowDialogVM
* 机器名称:       L05590
* 命名空间:       Luster.Motion.EditorUI.ViewModel.Dialogs
* 文 件 名:       SetWorkFlowDialogVM.cs
* 创建时间:       2022/12/2 10:11:43
* 作    者:       L05590
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       fanlu5590@lusterinc.com 
* 唯一标识：      a30f1fa5-fd23-4dcf-9281-de7b2d1261b5
* 登录用户:       fanlu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/12/2 10:11:43
* 修 改 人:		  L05590
************************************************************************************/
#endregion

using Luster.Common.DataStruct.DataModels;
using Luster.Motion.CommonUI.Models;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.EditorUI.Extensions;
using Luster.Motion.EditorUI.Models;
using Luster.Motion.TaskFlow.Engine;
using Luster.Motion.TaskFlow.Engine.Models;
using Luster.TaskFlow.Motion;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Luster.Motion.EditorUI.ViewModel.Dialogs
{
    public class SetWorkFlowDialogVM : MotionDialogVM
    {
        /// <summary>
        /// 模块引擎
        /// </summary>
        private IMotionEngine _engine;

        private IDialogService _dialogService;

        public SetWorkFlowDialogVM(IMotionEngine motionEngine, IDialogService dialogService)
        {
            _engine = motionEngine;
            _dialogService = dialogService;
            StationList = new ObservableCollection<LNode>();
            _stations = new List<LNode>();
            BuildStation();
        }

        #region 变量
        private List<LNode> _stations;

        /// <summary>
        /// 工站集合
        /// </summary>
        private ObservableCollection<LNode> _stationList;
        public ObservableCollection<LNode> StationList
        {
            get { return _stationList; }
            set { SetProperty(ref _stationList, value); }
        }

        /// <summary>
        /// 开始模块
        /// </summary>
        private IMotionModule _startModule;
        public IMotionModule StartModule
        {
            get { return _startModule; }
            set { SetProperty(ref _startModule, value); }
        }

        /// <summary>
        /// 开始模块内容
        /// </summary>
        private string _startModuleText;
        public string StartModuleText
        {
            get { return _startModuleText; }
            set { SetProperty(ref _startModuleText, value); }
        }

        /// <summary>
        /// 结束模块
        /// </summary>
        private IMotionModule _stopModule;
        public IMotionModule StopModule
        {
            get { return _stopModule; }
            set { SetProperty(ref _stopModule, value); }
        }

        /// <summary>
        /// 结束模块内容
        /// </summary>
        private string _stopModuleText;
        public string StopModuleText
        {
            get { return _stopModuleText; }
            set { SetProperty(ref _stopModuleText, value); }
        }


        /// <summary>
        /// 当前节点
        /// </summary>
        private IMotionModule _currentModule;
        public IMotionModule CurrentModule
        {
            get { return _currentModule; }
            set { SetProperty(ref _currentModule, value); }
        }

        private bool _isVisible;
        public bool IsVisible
        {
            get { return _isVisible; }
            set { SetProperty(ref _isVisible, value); }
        }

        /// <summary>
        /// 工作流名称
        /// </summary>
        private string _flowName;
        [Required]
        public string FlowName
        {
            get { return _flowName; }
            set { SetProperty(ref _flowName, value); }
        }
        #endregion
        private void BuildStation()
        {
            var listStation = _engine.GetStations();
            if (listStation != null)
            {
                if (listStation.Count > 0)
                {
                    foreach (var station in listStation)
                    {
                        _stations.Add(station.GetTreeNode(false));
                    }
                    StationList = new ObservableCollection<LNode>(_stations);
                }
            }
        }

        /// <summary>
        /// 查询方法
        /// </summary>
        private DelegateCommand<string> _searchCommand;
        public DelegateCommand<string> SearchCommand => _searchCommand ?? (_searchCommand = new DelegateCommand<string>((str) =>
        {
            IsVisible = !IsVisible;
            if (str == "Start")
            {
                CurrentModule = StartModule;
            }
            else
            {
                CurrentModule = StopModule;
            }
        }));

        /// <summary>
        /// 双击选择
        /// </summary>
        private DelegateCommand<object> _doubleClickCommand;
        public DelegateCommand<object> DoubleClickCommand => _doubleClickCommand ?? (_doubleClickCommand = new DelegateCommand<object>((obj) =>
        {
            var args = obj as MouseButtonEventArgs;
            if (args != null)
            {
                var source = args.OriginalSource as TextBlock;
                if (source == null) return;
                var modulect = source.DataContext as LNode;
                if (modulect != null)
                {
                    if (CurrentModule == StartModule)
                    {
                        StartModule = modulect.Tag as IMotionModule;
                        if (StartModule == null) return;
                        StartModuleText = StartModule.Alias;
                    }
                    else
                    {
                        StopModule = modulect.Tag as IMotionModule;
                        if (StopModule == null) return;
                        StopModuleText = StopModule.Alias;
                    }
                }
                IsVisible = !IsVisible;
            }
        }));


        public override void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.TryGetValue<string>("Title", out var tvalue))
            {
                Title = tvalue;

            }
            else
            {
                Title = string.Empty;
            }

            if (parameters.TryGetValue<WorkItem>("WorkItem", out var wItem))
            {
                if (wItem != null)
                {
                    FlowName = wItem.Name;

                    StartModuleText = wItem.StartItem?.Alias;
                    StartModule = wItem.StartItem;

                    StopModuleText = wItem.EndItem?.Alias;
                    StopModule = wItem.EndItem;
                }
            }
        }

        /// <summary>
        /// 确认
        /// </summary>
        /// <param name="result"></param>
        protected override void Ok(IDialogResult result)
        {
            base.Ok(result);
            result.Parameters.Add("FlowName", FlowName);
            result.Parameters.Add("StopModule", StopModule);
            result.Parameters.Add("StartModule", StartModule);
        }
    }
}