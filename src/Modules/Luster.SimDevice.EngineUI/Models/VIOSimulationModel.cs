#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       VIOSimulationModel
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.EngineUI.Models
* 文 件 名:       VIOSimulationModel.cs
* 创建时间:       2022/7/16 12:10:00
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      c8be44fe-4448-4202-bbf7-9a3754c7fd71
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/16 12:10:00
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.EngineUI.Models
{
    /// <summary>
    /// Prism Model
    /// </summary>
    public class VIOSimulationModel : BindableBase
    {
        /// <summary>
        /// 隶属模块
        /// </summary>
        private string _module;
        public string Module
        {
            get { return _module; }
            set { SetProperty(ref _module, value); if (Tag != null) Tag.Module = value; }
        }

        /// <summary>
        /// 设备名称
        /// </summary>
        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; if (Tag != null) Tag.Name = value; }
        }

        /// <summary>
        /// 对象
        /// </summary>
        public VIOSimulation Tag { get; set; }

        /// <summary>
        /// 所有的动作
        /// </summary>
        private ObservableCollection<SimActionModel> _actions;
        public ObservableCollection<SimActionModel> Actions
        {
            get { return _actions; }
            set { SetProperty(ref _actions, value); }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="vIO">IO对象</param>
        public VIOSimulationModel(VIOSimulation vIO)
        {
            Tag = vIO;
            Module = vIO.Module;
            Name = vIO.Name;

            Actions = new ObservableCollection<SimActionModel>();
            foreach (var item in vIO.Actions)
            {
                Actions.Add(new SimActionModel(item));
            }
        }

        public VIOSimulationModel()
        {
            Name = "";
            Module = "";
        }
    }

    /// <summary>
    /// action
    /// </summary>
    public class SimActionModel : BindableBase
    {
        /// <summary>
        /// 动作名称
        /// </summary>
        private string _action;
        public string Action
        {
            get { return _action; }
            set
            {
                SetProperty(ref _action, value);
                if (Tag != null)
                {
                    Tag.Action = value;
                }
            }
        }

        /// <summary>
        /// 是否回零
        /// </summary>
        private bool _isHome;
        public bool IsHome
        {
            get => _isHome; set
            {
                SetProperty(ref _isHome, value);
                if (Tag != null)
                {
                    Tag.IsHome = value;
                }
            }
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
                if (Tag != null)
                {
                    Tag.IsCheck = value;
                }
            }
        }

        /// <summary>
        /// 动作包含的IO
        /// </summary>
        public ObservableCollection<IOActionModel> Actions { get; set; }

        /// <summary>
        /// 对象
        /// </summary>
        public SimAction Tag { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="simAction"></param>
        public SimActionModel(SimAction simAction)
        {
            Tag = simAction;
            Action = simAction.Action;
            IsHome = simAction.IsHome;
            IsCheck = simAction.IsCheck;

            Actions = new ObservableCollection<IOActionModel>();

            foreach (var item in simAction.IOActions.OrderBy(u => u.Sort))
            {
                Actions.Add(new IOActionModel(item)
                {
                    Owner = this
                });
            }

            UpdateSort();
        }

        /// <summary>
        /// 更新排序
        /// </summary>
        public void UpdateSort()
        {
            int sort = 0;
            foreach (var item in Actions)
            {
                item.Sort = sort;
                item.Tag.Sort = sort;
                sort++;
            }
        }
    }

    /// <summary>
    /// IO对象
    /// </summary>
    public class IOActionModel : BindableBase
    {
        public SimActionModel Owner { get; set; }

        /// <summary>
        /// 是否回零
        /// </summary>
        private bool _value;
        public bool Value
        {
            get => _value; set
            {
                SetProperty(ref _value, value);
                if (Tag != null)
                {
                    Tag.Value = value;
                }
            }
        }

        /// <summary>
        /// IO对象
        /// </summary>
        private VIO vIO;
        public VIO IO
        {
            get => vIO; set
            {
                SetProperty(ref vIO, value);
            }
        }

        /// <summary>
        /// 顺序
        /// </summary>
        private int _sort;
        public int Sort
        {
            get { return _sort; }
            set { SetProperty(ref _sort, value); }
        }


        /// <summary>
        /// Object
        /// </summary>
        public IOAction Tag { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ioAction"></param>
        public IOActionModel(IOAction ioAction)
        {
            Tag = ioAction;
            Value = ioAction.Value;
            IO = ioAction.IO;
        }
    }
}