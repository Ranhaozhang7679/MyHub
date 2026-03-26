#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LogContentVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.EditorUI.ViewModel
* 文 件 名:       LogContentVM.cs
* 创建时间:       2022/8/7 15:33:38
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      9b5d0f31-a0d4-4ea9-bbfd-4cf5313146b0
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/8/7 15:33:38
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Extensions;
using Luster.Control.Wpf.Motion.Controls;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.CommonUI.Models;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.EditorUI.Events;
using Luster.Motion.EditorUI.Models;
using Luster.Motion.TaskFlow.Engine;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Logic;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Luster.Motion.EditorUI.ViewModel
{
    /// <summary>
    /// 轴配置
    /// </summary>
    public class CTConfigContentVM : MotionPageVM
    {
        private IMotionEngine _motionEngine;
        private SubscriptionToken loadedToken;
        private FlowBus _flowBus;
        public CTConfigContentVM(FlowBus flowBus, ICommonBus commonBus, IMotionEngine motionEngine, Dispatcher dispatcher) : base(commonBus)
        {
            _flowBus = flowBus;
            _motionEngine = motionEngine;
        }

        protected override void RegisterEvent(IEventAggregator bus)
        {
            base.RegisterEvent(bus);
            loadedToken = bus.GetEvent<ModuleLoadedEvent>().Subscribe(m =>
            {
                if (m.TaskFunction is IStation s)
                {
                    BuildStations();
                }
            });
        }

        private void BuildStations()
        {
            var stations = _motionEngine.GetStations();
            Stations = new ObservableCollection<StationLog>();
            foreach (var item in stations)
            {
                Stations.Add(new StationLog(item));
            }
        }

        /// <summary>
        /// 重新加载
        /// </summary>
        /// <param name="navigationContext"></param>
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            BuildStations();
        }

        /// <summary>
        /// 当前的工站
        /// </summary>
        private ObservableCollection<StationLog> _stations;
        public ObservableCollection<StationLog> Stations
        {
            get { return _stations; }
            set { SetProperty(ref _stations, value); }
        }

        /// <summary>
        /// SelectedItem
        /// </summary>
        private ModuleCT _selectedItem;
        public ModuleCT SelectedItem
        {
            get { return _selectedItem; }
            set { SetProperty(ref _selectedItem, value); }
        }

        /// <summary>
        /// CT集合
        /// </summary>
        private ObservableCollection<ModuleCT> _motionChildList;
        public ObservableCollection<ModuleCT> MotionChildList
        {
            get { return _motionChildList; }
            set { SetProperty(ref _motionChildList, value); }
        }


        /// <summary>
        /// 模块选择
        /// </summary>
        private DelegateCommand<object> _changeCommand;
        public DelegateCommand<object> ChangeCommand => _changeCommand ?? (_changeCommand = new DelegateCommand<object>((obj) =>
        {
            var stationLog = obj as StationLog;
            if (stationLog != null)
            {
                // 加载数据
                MotionChildList = new ObservableCollection<ModuleCT>();
                BuildDataList(stationLog.Module);
            }
        }));

        /// <summary>
        /// 模块选择
        /// </summary>
        private DelegateCommand<object> _loadedCommand;
        public DelegateCommand<object> LoadedCommand => _loadedCommand ?? (_loadedCommand = new DelegateCommand<object>((obj) =>
        {
            if (Stations != null && Stations.Count > 0)
            {
                ChangeCommand.Execute(Stations[0]);
            }
        }));

        /// <summary>
        /// 构建数据表
        /// </summary>
        private void BuildDataList(IMotionModule sMotion)
        {
            var moduleCT = new ModuleCT(sMotion);
            moduleCT.IsExpanded = true;
            List<ModuleCT> childList = new List<ModuleCT>();
            foreach (var item in sMotion.Children)
            {
                childList.Add(new ModuleCT(item));
            }
            moduleCT.Children = childList;
            MotionChildList.Add(moduleCT);
            BuildChildList(moduleCT.Children);
            SelectedItem = MotionChildList[0];
        }

        /// <summary>
        /// 循环构建子级数据
        /// </summary>
        /// <param name="listChild"></param>
        private void BuildChildList(List<ModuleCT> listChild)
        {
            if (listChild.Count > 0)
            {
                foreach (var item in listChild)
                {
                    if (item.Tag.Children.Count > 0)
                    {
                        var moduleCT = new ModuleCT(item.Tag);
                        List<ModuleCT> childList = new List<ModuleCT>();
                        foreach (var itemchild in item.Tag.Children)
                        {
                            childList.Add(new ModuleCT(itemchild));
                        }
                        item.Children = childList;
                        BuildChildList(childList);
                    }
                }
            }
        }

        /// <summary>
        /// 右键添加等待模块
        /// </summary>
        private DelegateCommand<object> _toWaitCommand;
        public DelegateCommand<object> ToWaitCommand => _toWaitCommand ?? (_toWaitCommand = new DelegateCommand<object>((obj) =>
        {
            if (obj is ModuleCT mCT)
            {
                // 添加新模块
                var newModule = _flowBus.OnAddModule("Logic", "WaitStatus");
                if (mCT.Tag.Parameters.ContainsKey("Status"))
                {
                    var statusParam = mCT.Tag.Parameters["Status"];
                    newModule.Parameters["InStatus"].RefOut = statusParam;
                    newModule.Alias = $"{Luster.Motion.Assests.Langs.LangProvider.GetLang("Wait")} {mCT.Tag.Alias} {Luster.Motion.Assests.Langs.LangProvider.GetLang("Done")}";
                }
            }
        }));
    }
}