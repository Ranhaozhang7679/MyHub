using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.EditorUI.Models;
using Luster.TaskFlow.Common.Attributes;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Models;
using Luster.Motion.CommonUI.Models;
using Luster.Motion.CommonUI;
using Luster.Motion.EditorUI.Events;
using Luster.Motion.TaskFlow.Engine;
using Prism.Events;
using Prism.Regions;
using System.Windows.Threading;
using Luster.TaskFlow.Motion.Enums;

namespace Luster.Motion.EditorUI.ViewModel.Dialogs
{
    public class ModuleDialogVM : MotionDialogVM
    {
        private IMotionEngine _motionEngine;
        private SubscriptionToken loadedToken;
        private FlowBus _flowBus;

        public ModuleDialogVM(FlowBus flowBus, ICommonBus commonBus, IMotionEngine motionEngine, Dispatcher dispatcher)
        {
            _flowBus = flowBus;
            _motionEngine = motionEngine;
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
        /// 弹窗打开
        /// </summary>
        /// <param name="parameters"></param>
        /// <exception cref="FriendlyException"></exception>
        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);
            if (parameters.TryGetValue<IMotionModule>("Module", out var module))
            {
                // 加载数据
                MotionChildList = new ObservableCollection<ModuleCT>();
                if (module.Station == null)
                {
                    module.UpdateStation();
                }
                BuildDataList(module.Station);
            }
        }

        /// <summary>
        /// 选择
        /// </summary>
        private DelegateCommand<object> _selectCommand;
        public DelegateCommand<object> SelectCommand => _selectCommand ?? (_selectCommand = new DelegateCommand<object>((obj) =>
        {
            if (obj != null && obj is ModuleCT m)
            {
                LModule lModule = new LModule()
                {
                    Name = m.Name,
                    ID = m.Tag.ID
                };

                IDialogResult r = new DialogResult(ButtonResult.OK);
                r.Parameters.Add("Module", lModule);

                RaiseRequestClose(r);
            }
        }));

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
    }
}
