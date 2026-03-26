#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       VIOSimulationVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.SubSystem.ViewModel.Virtual
* 文 件 名:       VIOSimulationVM.cs
* 创建时间:       2022/7/15 21:53:39
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      782359f9-63a1-4b85-bb24-3161bac6c634
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/15 21:53:39
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.Assets;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.SimDevice.EngineUI;
using Luster.SimDevice.EngineUI.Models;
using Luster.SimDevice.SubSystem.Extension;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.SubSystem.ViewModel.Virtual
{
    public class VIOSimulationContentVM : PageVM
    {
        /// <summary>
        /// 
        /// </summary>
        public override bool IsShowAdd => true;

        /// <summary>
        /// 仿真模型集合
        /// </summary>
        private ObservableCollection<VIOSimulationModel> _simModels;
        public ObservableCollection<VIOSimulationModel> SimModels
        {
            get { return _simModels; }
            set { SetProperty(ref _simModels, value); }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="_engine"></param>
        protected VIOSimulationContentVM(ISimDeviceEngineUI _engine) : base(_engine)
        {
            LoadDevices();
        }


        private void LoadDevices(string strKey = "")
        {
            SimModels = new ObservableCollection<VIOSimulationModel>();
            var devices = deviceEngine.GetDevices(typeof(VIOSimulation));
            foreach (var item in devices)
            {
                if (!string.IsNullOrEmpty(strKey))
                {
                    if (item.Module.Contains(strKey) || item.Name.Contains(strKey))
                    {
                        SimModels.Add(new VIOSimulationModel(item as VIOSimulation));
                    }
                }
                else
                {
                    SimModels.Add(new VIOSimulationModel(item as VIOSimulation));
                }
            }
        }

        /// <summary>
        /// 按钮切换
        /// </summary>
        private DelegateCommand<string> _searchCommand;
        public DelegateCommand<string> SearchCommand => _searchCommand ?? (_searchCommand = new DelegateCommand<string>((key) =>
        {
            string trimKey = key.Trim();

            LoadDevices(trimKey);
        }));


        /// <summary>
        /// 添加对象
        /// </summary>
        public override void AddNewItem()
        {
            base.AddNewItem();
            dialogService.ShowIOSimDialog(null, r =>
            {
                LoadDevices();
            });
        }

        /// <summary>
        /// 对象编辑
        /// </summary>
        private DelegateCommand<object> _editCommand;
        public DelegateCommand<object> EditCommand => _editCommand ?? (_editCommand = new DelegateCommand<object>((obj) =>
        {
            if (obj is VIOSimulationModel model)
            {
                dialogService.ShowIOSimDialog(model);
            }
        }));

        /// <summary>
        /// 对象删除
        /// </summary>
        private DelegateCommand<object> _removeCommand;
        public DelegateCommand<object> RemoveCommand => _removeCommand ?? (_removeCommand = new DelegateCommand<object>((obj) =>
        {
            if (obj is VIOSimulationModel model)
            {
                dialogService.ShowConfirm($"确认删除设备:{model.Name}?", (r) =>
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        deviceEngine.ReomoveVirtual(model.Tag.ID);
                        SimModels.Remove(model);
                    }
                });
            }
        }));

        /// <summary>
        /// 仿真模型集合
        /// </summary>
        private ObservableCollection<SimActionModel> _actionModels;
        public ObservableCollection<SimActionModel> ActionModels
        {
            get { return _actionModels; }
            set { SetProperty(ref _actionModels, value); }
        }

        internal VIOSimulationModel Current { get; set; }

        /// <summary>
        /// 对象选择
        /// </summary>
        private DelegateCommand<object> _selectedCommand;
        public DelegateCommand<object> SelectedCommand => _selectedCommand ?? (_selectedCommand = new DelegateCommand<object>((obj) =>
        {
            if (obj is VIOSimulationModel model)
            {
                Current = model;
                ActionModels = model.Actions;

                // 获取当前模式的所有IO
                IOList = deviceEngine.GetDevices(typeof(VIO)).Where(u => u.Module == model.Module).Select(u => u as VIO).ToList();
            }
        }));

        /// <summary>
        /// 所有IOlist
        /// </summary>
        public List<VIO> IOList { get; set; }

        /// <summary>
        /// 动作添加
        /// </summary>
        private DelegateCommand _addActionCommand;
        public DelegateCommand AddActionCommand => _addActionCommand ?? (_addActionCommand = new DelegateCommand(() =>
        {
            if (ActionModels != null)
            {
                dialogService.ShowIOActionDialog(Current, null);
            }
        }));

        /// <summary>
        /// 动作编辑
        /// </summary>
        private DelegateCommand<SimActionModel> _editActionCommand;
        public DelegateCommand<SimActionModel> EditActionCommand => _editActionCommand ?? (_editActionCommand = new DelegateCommand<SimActionModel>((simModel) =>
        {
            if (ActionModels != null)
            {
                dialogService.ShowIOActionDialog(Current, simModel);
            }
        }));

        /// <summary>
        /// 对象选择
        /// </summary>
        private DelegateCommand<object> _removeActionCommand;
        public DelegateCommand<object> RemoveActionCommand => _removeActionCommand ?? (_removeActionCommand = new DelegateCommand<object>((obj) =>
        {
            if (obj is SimActionModel action)
            {
                dialogService.ShowConfirm($"确认删除动作:{action.Action}?", r =>
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        // 删除模型对象
                        ActionModels?.Remove(action);

                        // 删除真实对象
                        Current.Tag.Actions.RemoveAll(u => u.Action == action.Action);
                    }
                });
            }
        }));

        /// <summary>
        /// 执行动作
        /// </summary>
        private DelegateCommand<object> _runActionCommand;
        public DelegateCommand<object> RunActionCommand => _runActionCommand ?? (_runActionCommand = new DelegateCommand<object>((obj) =>
        {
            // 支持动作
            if (obj is SimActionModel action)
            {
                Current.Tag.Action(action.Action);
            }
        }));

        /// <summary>
        /// 执行动作
        /// </summary>
        private DelegateCommand<object> _topActionCommand;
        public DelegateCommand<object> TopActionCommand => _topActionCommand ?? (_topActionCommand = new DelegateCommand<object>((obj) =>
        {
            // 向上移动
            if (obj is IOActionModel ioAction && ioAction.Sort > 0)
            {
                int newIndex = ioAction.Sort - 1;
                ioAction.Owner.Actions.RemoveAt(ioAction.Sort);
                ioAction.Owner.Actions.Insert(newIndex, ioAction);
                ioAction.Owner.UpdateSort();
            }
        }));

        /// <summary>
        /// 执行动作
        /// </summary>
        private DelegateCommand<object> _bottomActionCommand;
        public DelegateCommand<object> BottomActionCommand => _bottomActionCommand ?? (_bottomActionCommand = new DelegateCommand<object>((obj) =>
        {
            // 向下移动
            if (obj is IOActionModel ioAction && ioAction.Sort < (Current.Actions.Count - 1))
            {
                int newIndex = ioAction.Sort + 2;
                ioAction.Owner.Actions.Insert(newIndex, ioAction);
                ioAction.Owner.Actions.RemoveAt(ioAction.Sort);
                ioAction.Owner.UpdateSort();
            }
        }));

    }
}