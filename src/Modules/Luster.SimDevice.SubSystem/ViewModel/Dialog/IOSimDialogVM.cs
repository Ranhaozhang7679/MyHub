#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       IODialogVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.SubSystem.ViewModel.Dialog
* 文 件 名:       IODialogVM.cs
* 创建时间:       2022/4/26 14:20:45
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      6e74db86-a141-462c-8c56-f72e64505019
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/26 14:20:45
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Extensions;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Real;
using Luster.SimDevice.Adapter;
using Luster.SimDevice.EngineUI;
using Luster.SimDevice.EngineUI.Models;
using Luster.SimDevice.MotionCards;
using Luster.SimDevice.Real;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.SubSystem.ViewModel.Dialog
{
    public class IOSimDialogVM : DialogVM
    {
        /// <summary>
        /// 当前对象
        /// </summary>
        private VIOSimulationModel _current;
        public VIOSimulationModel Current
        {
            get { return _current; }
            set { SetProperty(ref _current, value); }
        }

        protected IOSimDialogVM(ISimDeviceEngineUI _engine) : base(_engine)
        {
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);
            if (parameters.TryGetValue<VIOSimulationModel>("Model", out _current))
            {
                if (_current == null)
                {
                    _current = new VIOSimulationModel();
                }

                Current = _current;
            }
        }

        protected override void Ok(IDialogResult result)
        {
            if (Current.Tag == null)
            {
                VIOSimulation vNew = new VIOSimulation()
                {
                    ID = Guid.NewGuid(),
                    Name = Current.Name,
                    Module = Current.Module,
                };

                // 构建新对象
                deviceEngine.AddVirtual(vNew);
            }
        }
    }

    /// <summary>
    /// 新增Action
    /// </summary>
    public class IOActionDialogVM : DialogVM
    {
        private VIOSimulationModel current;

        /// <summary>
        /// 动作model
        /// </summary>
        private SimActionModel simActionModel;
        public SimActionModel SimActionModel
        {
            get { return simActionModel; }
            set { SetProperty(ref simActionModel, value); }
        }

        /// <summary>
        /// 当前对象
        /// </summary>
        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        /// <summary>
        /// 是否回零动作
        /// </summary>
        private bool _isHome;
        public bool IsHome
        {
            get { return _isHome; }
            set { SetProperty(ref _isHome, value); }
        }

        /// <summary>
        /// 是否用于检查动作完成的对象
        /// </summary>
        private bool _isCheck;
        public bool IsCheck
        {
            get { return _isCheck; }
            set
            {
                SetProperty(ref _isCheck, value);
                if (value)
                {
                    IOList = deviceEngine.GetVDevices<VIO>(current?.Module).Where(u => u.Behavior == IOBehavior.Input).ToList();
                }
            }
        }

        private int ioNum = 2;
        public int IoNum
        {
            get { return ioNum; }
            set
            {
                int srcV = ioNum;
                SetProperty(ref ioNum, value);
                if (srcV > value)
                {
                    for (int i = ioNum + 1; i <= srcV; i++)
                    {
                        var prop = GetType().GetProperty($"IO{i}");
                        if (prop != null)
                        {
                            prop.SetValue(this, null);
                        }
                    }
                }
            }
        }

        private VIO io1;
        public VIO IO1
        {
            get { return io1; }
            set { SetProperty(ref io1, value); }
        }

        private VIO io2;
        public VIO IO2
        {
            get { return io2; }
            set { SetProperty(ref io2, value); }
        }

        private VIO io3;
        public VIO IO3
        {
            get { return io3; }
            set { SetProperty(ref io3, value); }
        }

        private VIO io4;
        public VIO IO4
        {
            get { return io4; }
            set { SetProperty(ref io4, value); }
        }

        private VIO io5;
        public VIO IO5
        {
            get { return io5; }
            set { SetProperty(ref io5, value); }
        }

        private VIO io6;
        public VIO IO6
        {
            get { return io6; }
            set { SetProperty(ref io6, value); }
        }

        private VIO io7;
        public VIO IO7
        {
            get { return io7; }
            set { SetProperty(ref io7, value); }
        }

        private VIO io8;
        public VIO IO8
        {
            get { return io8; }
            set { SetProperty(ref io8, value); }
        }

        /// <summary>
        /// IO 数据源
        /// </summary>
        private List<VIO> _ioList;
        public List<VIO> IOList { get => _ioList; set => SetProperty(ref _ioList, value); }

        protected IOActionDialogVM(ISimDeviceEngineUI _engine) : base(_engine)
        {
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);
            io1 = null;
            io2 = null;
            io3 = null;
            io4 = null;
            io5 = null;
            io6 = null;
            io7 = null;
            io8 = null;
            //IOList = deviceEngine.GetVDevices<VIO>(current?.Module).Where(u => u.Behavior == IOBehavior.Output).ToList();

            if (parameters.TryGetValue<VIOSimulationModel>("Model", out var model))
            {
                current = model;
            }

            IOList = deviceEngine.GetVDevices<VIO>(current?.Module).Where(u => u.Behavior == IOBehavior.Output).ToList();

            if (parameters.TryGetValue<SimActionModel>("SimModel", out var simModel))
            {
                if (simModel == null) return;
                SimActionModel = simModel;
                Name = simModel.Action;
                IsHome = simModel.IsHome;
                IsCheck = simModel.IsCheck;
                IoNum = simModel.Actions.Count;

                for (int i = 0; i < ioNum; i++)
                {
                    this.GetType().GetProperty($"IO{i + 1}").SetValue(this, simModel.Actions[i].IO);
                }
            }
        }

        protected override void Ok(IDialogResult result)
        {
            // 构建动作对象
            if (current == null) return;

            // 新增的Action的情况
            if (SimActionModel == null)
            {
                if (current.Actions.Any(u => u.Action == Name))
                {
                    throw new DeviceException($"动作:{Name}已经存在!");
                }


                SimAction simAction = new SimAction();
                simAction.Action = Name;
                simAction.IsHome = IsHome;
                simAction.IsCheck = IsCheck;

                var ioActions = GetIOActions();
                simAction.IOActions = ioActions;

                current.Tag.Actions.Add(simAction);
                current.Actions.Add(new SimActionModel(simAction));
            }
            else
            {
                SimActionModel.Action = Name;
                SimActionModel.IsHome = IsHome;
                SimActionModel.IsCheck = IsCheck;
                SimActionModel.Actions.Clear();
                var ioActions = GetIOActions();
                foreach (var item in ioActions)
                {
                    SimActionModel.Actions.Add(new IOActionModel(item));
                }

                // 删除历史数据
                SimActionModel.Tag.IOActions.RemoveAll(u => true);
                SimActionModel.Tag.IOActions = ioActions;
            }

            deviceEngine.IsNeedSave = true;
        }

        private List<IOAction> GetIOActions()
        {
            List<IOAction> actions = new List<IOAction>();
            if (io1 != null)
            {
                IOAction ioAction = new IOAction() { IO = io1 };
                actions.Add(ioAction);
            }

            if (io2 != null)
            {
                IOAction ioAction = new IOAction() { IO = io2 };
                actions.Add(ioAction);
            }

            if (io3 != null)
            {
                IOAction ioAction = new IOAction() { IO = io3 };
                actions.Add(ioAction);
            }

            if (io4 != null)
            {
                IOAction ioAction = new IOAction() { IO = io4 };
                actions.Add(ioAction);
            }

            if (io5 != null)
            {
                IOAction ioAction = new IOAction() { IO = io5 };
                actions.Add(ioAction);
            }


            if (io6 != null)
            {
                IOAction ioAction = new IOAction() { IO = IO6 };
                actions.Add(ioAction);
            }


            if (io7 != null)
            {
                IOAction ioAction = new IOAction() { IO = IO7 };
                actions.Add(ioAction);
            }


            if (io8 != null)
            {
                IOAction ioAction = new IOAction() { IO = IO8 };
                actions.Add(ioAction);
            }

            return actions;
        }
    }
}