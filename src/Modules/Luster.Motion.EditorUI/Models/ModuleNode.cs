#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ModuleNode
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.EditorUI.Models
* 文 件 名:       ModuleNode.cs
* 创建时间:       2022/7/1 16:58:47
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      94f4d41e-15f7-407d-a185-2fe67529bc29
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/1 16:58:47
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Extensions;
using Luster.Control.Wpf.Motion.Flow;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Common.Module;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Logic;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Luster.Motion.EditorUI.Models
{
    public class ModuleNode : LNode, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _isExpand;
        /// <summary>
        /// 是否为最后一个分类（用于显示加号按钮）
        /// </summary>
        public bool IsLastCategory { get; set; }
        public bool IsExpand
        {
            get { return _isExpand; }
            set
            {
                _isExpand = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsExpand)));
            }
        }

        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsEnabled)));
            }
        }

        /// <summary>
        /// 是否选中
        /// </summary>
        private bool _isSelected = true;
        public new bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
            }
        }

        public new ModuleNode Parent { get; set; }

        public ObservableCollection<ModuleNode> Childrens { get; set; }

        public ModuleNode()
        {
        }

        public ModuleNode(LNode node)
        {
            Text = node.Text;
            Tips = node.Tips;
            Level = node.Level;
            Tag = node.Tag;
            Tag1 = node.Tag1;
            Icon = node.Icon;
            Childrens = new ObservableCollection<ModuleNode>();
        }

        public ModuleNode(IMotionModule m)
        {
            Text = m.Alias;
            Tips = m.Tips;
            Level = 1;
            Tag = "Composition";
            Tag1 = m;
            Icon = m.Icon;
            Childrens = new ObservableCollection<ModuleNode>();
        }
    }


    public class ModuleCT : BindableBase
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
        private List<ModuleCT> _children;
        public List<ModuleCT> Children { get => _children; set => SetProperty(ref _children, value); }


        public ModuleCT()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="module"></param>
        public ModuleCT(IMotionModule module)
        {
            Tag = module;
            Name = $"{module.Alias}";
            //module.GetModuleAlias(module, ref _name);
            CT = module.CT;
            Icon = module.TaskFunction.Icon;
            IsExpanded = false;
            Children = new List<ModuleCT>();
            DataID = module.DataID;

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

            if (module.TaskFunction is IStation)
            {
                if (propName == "Status" || propName == "DataID" || propName == "RunNum")
                {
                    // 更新对应的DataID
                    Name = $"{Tag.Alias} {Tag.DataID} {Tag.RunNum}";
                }
            }
        }
    }

    public class ModuleError : BindableBase
    {
        public IMotionModule Tag { get; set; }

        private string _errorCode;
        public string ErrorCode
        {
            get { return _errorCode; }
            set
            {
                string src = _errorCode;
                SetProperty(ref _errorCode, value);
                if (src != value)
                {
                    Tag.ErrorCode = value;
                }
            }
        }

        private string _errorContent;
        public string ErrorContent
        {
            get { return _errorContent; }
            set
            {
                string src = _errorContent;
                SetProperty(ref _errorContent, value);
                if (src != value)
                {
                    Tag.ErrorContent = value;
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
        private List<ModuleError> _children;
        public List<ModuleError> Children { get => _children; set => SetProperty(ref _children, value); }


        public ModuleError()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="module"></param>
        public ModuleError(IMotionModule module)
        {
            Tag = module;
            Name = module.Alias;
            //module.GetModuleAlias(module, ref _name);
            ErrorCode = module.ErrorCode;
            ErrorContent = module.ErrorContent;
            Icon = module.TaskFunction.Icon;
            IsExpanded = false;
            Children = new List<ModuleError>();

            // 状态监控
            //module.PropertyChangedEvent -= Module_PropertyChangedEvent;
            //module.PropertyChangedEvent += Module_PropertyChangedEvent;
            //Module_PropertyChangedEvent(module, "Status", RunStatus.Default, Tag.Status);
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
            else if (module.TaskFunction is IStation)
            {
                if (propName == "DataID" || propName == "RunNum")
                {
                    // 更新对应的DataID
                    Name = $"{Tag.Alias} {Tag.RunNum} {Tag.DataID}";
                }
            }
        }
    }

}