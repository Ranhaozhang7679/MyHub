#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ModuleModel
* 机器名称:       L05590
* 命名空间:       Luster.Motion.CommonUI.Models
* 文 件 名:       ModuleModel.cs
* 创建时间:       2022/12/1 14:48:53
* 作    者:       L05590
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       fanlu5590@lusterinc.com 
* 唯一标识：      7fdd88db-ac72-4b9b-86b8-67af1f757d3b
* 登录用户:       fanlu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/12/1 14:48:53
* 修 改 人:		  L05590
************************************************************************************/
#endregion

using Luster.Control.Wpf.Motion.Flow;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Common.Module;
using Luster.TaskFlow.Motion;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Luster.Motion.CommonUI.Models
{
    public class ModuleModel : BindableBase
    {
        public IMotionModule Tag { get; set; }

        private int _ct;
        public int CT
        {
            get { return _ct; }
            set
            {
                int src = _ct;
                SetProperty(ref _ct, value);
                if (src != value)
                {
                    Tag.CT = value;
                }
            }
        }

        /// <summary>
        /// 数据ID
        /// </summary>
        private string _dataID;
        public string DataID
        {
            get { return _dataID; }
            set { SetProperty(ref _dataID, value); }
        }

        /// <summary>
        /// 名称
        /// </summary>
        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        /// <summary>
        /// 图标
        /// </summary>
        private string _icon;
        public string Icon
        {
            get { return _icon; }
            set { SetProperty(ref _icon, value); }
        }

        /// <summary>
        /// 展开
        /// </summary>
        private bool _isExpanded;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set { SetProperty(ref _isExpanded, value); }
        }

        private Brush _statusColor;
        public Brush StatusColor
        {
            get { return _statusColor; }
            set { SetProperty(ref _statusColor, value); }
        }

        /// <summary>
        /// 是否选中
        /// </summary>
        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        /// <summary>
        /// 孩集合
        /// </summary>
        private List<ModuleModel> _children;
        public List<ModuleModel> Children { get => _children; set => SetProperty(ref _children, value); }


        public ModuleModel()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="module"></param>
        public ModuleModel(IMotionModule module)
        {
            Tag = module;
            Name = module.Alias;
            //module.GetModuleAlias(module, ref _name);
            CT = module.CT;
            Icon = module.TaskFunction.Icon;
            IsExpanded = false;

            // 状态监控
            module.PropertyChangedEvent -= Module_PropertyChangedEvent;
            module.PropertyChangedEvent += Module_PropertyChangedEvent;
            Module_PropertyChangedEvent(module, "Status", RunStatus.Default, Tag.Status);
        }

        /// <summary>
        /// 状态变更事件
        /// </summary>
        /// <param name="module"></param>
        /// <param name="propName"></param>
        /// <param name="srcVal"></param>
        /// <param name="newVal"></param>
        private void Module_PropertyChangedEvent(IModule module, string propName, object srcVal, object newVal)
        {
            // 状态变更
            if (propName == "Status")
            {
                switch (module.Status)
                {
                    case RunStatus.Default:
                        StatusColor = Brushes.Black;
                        break;
                    case RunStatus.Running:
                        StatusColor = FlowItem.RunningBrush;
                        break;
                    case RunStatus.Skip:
                        StatusColor = FlowItem.SkipBrush;
                        break;
                    case RunStatus.Success:
                        StatusColor = FlowItem.SuccessBrush;
                        break;
                    case RunStatus.Alarmed:
                        StatusColor = FlowItem.TimeoutBrush;
                        break;
                    case RunStatus.Error:
                        StatusColor = FlowItem.FailBrush;
                        break;
                }
            }
            else if (propName == "DataID")
            {
                // 更新对应的DataID
                Name = $"{Tag.Alias} {newVal}";
            }
        }
    }
}