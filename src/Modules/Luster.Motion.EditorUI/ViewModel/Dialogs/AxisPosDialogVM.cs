#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       AxisMDialogVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.EditorUI.ViewModel.Dialogs
* 文 件 名:       AxisMDialogVM.cs
* 创建时间:       2022/6/21 10:57:59
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      91290ea7-0eaf-4124-8b91-5a0271cd788a
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/21 10:57:59
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using LiveCharts;
using LiveCharts.Wpf;
using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Extensions;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.EditorUI.Models;
using Luster.Motion.TaskFlow.Engine;
using Luster.TaskFlow.Common.Attributes;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.Motion.EditorUI.ViewModel.Dialogs
{
    /// <summary>
    /// 多轴调试界面
    /// </summary>
    public class AxisPosDialogVM : MotionDialogVM
    {
        private IDeviceEngine _deviceEngine;

        private IMotionController _mController;

        public AxisPosDialogVM(IDeviceEngine deviceEngine, IMotionController mController)
        {
            _deviceEngine = deviceEngine;
            _mController = mController;
            Priorities = typeof(Priority).EnumToDataSource();
        }

        public List<KeyValue> Priorities { get; set; }

        /// <summary>
        /// 是否支持单轴
        /// </summary>
        private bool _isSingleAxis;
        public bool IsSingleAxis
        {
            get { return _isSingleAxis; }
            set { SetProperty(ref _isSingleAxis, value); }
        }

        /// <summary>
        /// 轴名称
        /// </summary>
        private string _tipName;
        public string TipName
        {
            get { return _tipName; }
            set { SetProperty(ref _tipName, value); }
        }

        /// <summary>
        /// 轴点位列表
        /// </summary>
        public List<LNode> _axisPostions;
        public List<LNode> AxisPostions
        {
            get { return _axisPostions; }
            set { SetProperty(ref _axisPostions, value); }
        }

        /// <summary>
        /// 点位列表
        /// </summary>
        public List<VAxisPosGroup> _positions;
        public List<VAxisPosGroup> Positions
        {
            get { return _positions; }
            set { SetProperty(ref _positions, value); }
        }

        /// <summary>
        /// 点位列表
        /// </summary>
        private ObservableCollection<AxisPosItemModel> _axisPosItems;
        public ObservableCollection<AxisPosItemModel> AxisPosItems
        {
            get { return _axisPosItems; }
            set { SetProperty(ref _axisPosItems, value); }
        }

        /// <summary>
        /// 当前选中项
        /// </summary>
        private LNode _selectItem;
        public LNode SelectedItem
        {
            get { return _selectItem; }
            set { SetProperty(ref _selectItem, value); }
        }


        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);

            // 1.初始化轴列表
            var axises = _deviceEngine.GetVDevices<VAxis>();
            AxisPostions = axises.Select(u => u.GetAxisPosNodes()).ToList();

            // 2.当前点位列表
            AxisPosItems = new ObservableCollection<AxisPosItemModel>();

            // 3.点位列表
            Positions = _deviceEngine.PosGroup;

            // 3.是否单轴
            if (parameters.TryGetValue<ParameterAttribute>("Parameter", out var parameter))
            {
                IsSingleAxis = !parameter.MultiValues;
                TipName = IsSingleAxis ? "单点位" : "多点位";
            }

            // 4.初始化已有的轴
            if (parameters.TryGetValue<VAxisPos>("VAxisPos", out var device))
            {
                if (device == null) return;
                foreach (var item in device)
                {
                    item.UpdatePosition(_deviceEngine);
                    if (item.Axis == null) continue;

                    var axis = new AxisPosItemModel(item);
                    axis.AxisPropertyChanged -= Axis_AxisPropertyChanged;
                    axis.AxisPropertyChanged += Axis_AxisPropertyChanged;
                    AxisPosItems.Add(axis);
                }
            }
        }

        /// <summary>
        /// 轴参数变更
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        private void Axis_AxisPropertyChanged(string arg1, object arg2, object arg3)
        {
            _mController.OnPropertyChanged(arg1, arg2, arg3);
        }

        /// <summary>
        /// 双击添加点位
        /// </summary>
        private DelegateCommand<object> _selectPosCommand;
        public DelegateCommand<object> SelectPosCommand => _selectPosCommand ?? (_selectPosCommand = new DelegateCommand<object>((vm) =>
        {
            var pNode = vm as LNode;
            if (pNode != null && pNode.Parent != null)
            {
                var vAxis = pNode.Parent.Tag as VAxis;


                var axisPos = pNode.Tag as DataStruct.DataModels.AxisPosition;
                var vPos = new AxisPosItem()
                {
                    AxisPostion = axisPos,
                    Acc = vAxis.Acc,
                    Dec = vAxis.Dec,
                    Speed = vAxis.MoveSpeed,
                    MovePriority = axisPos.MovePriority,
                    PosKey = axisPos.Key
                };

                // 如果是单轴清理历史记录
                if (IsSingleAxis)
                {
                    AxisPosItems.Clear();
                }

                // 同一个轴，被添加了多个点位，就没法运动了.........
                if (AxisPosItems.Any(u => u.Tag.Axis.ID == vAxis.ID))
                {
                    throw new FriendlyException($"轴:{vAxis.Name} 已经有点位添加!");
                }

                AxisPosItems.Add(new AxisPosItemModel(vPos));
            }
        }));

        /// <summary>
        /// 双击添加点位
        /// </summary>
        private DelegateCommand<object> _selectedCommand;
        public DelegateCommand<object> SelectedCommand => _selectedCommand ?? (_selectedCommand = new DelegateCommand<object>((vm) =>
        {
            var pGroup = vm as VAxisPosGroup;
            AxisPosItems?.Clear();
            foreach (var axisPos in pGroup)
            {
                var vAxis = axisPos.Axis;
                var vPos = new AxisPosItem()
                {
                    AxisPostion = axisPos,
                    Acc = vAxis.Acc,
                    Dec = vAxis.Dec,
                    Speed = vAxis.MoveSpeed,
                    MovePriority = axisPos.MovePriority,
                    PosKey = axisPos.Key
                };

                // 同一个轴，被添加了多个点位，就没法运动了.........
                if (AxisPosItems.Any(u => u.Tag.Axis.ID == vAxis.ID))
                {
                    throw new FriendlyException($"轴:{vAxis.Name} 已经有点位添加!");
                }

                AxisPosItems.Add(new AxisPosItemModel(vPos));
            }
        }));

        /// <summary>
        /// 删除点位
        /// </summary>
        private DelegateCommand<object> _removeCommand;
        public DelegateCommand<object> RemoveCommand => _removeCommand ?? (_removeCommand = new DelegateCommand<object>((obj) =>
        {
            var pNode = obj as AxisPosItemModel;
            if (pNode != null)
            {
                AxisPosItems.Remove(pNode);
            }
        }));

        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="result"></param>
        protected override void Ok(IDialogResult result)
        {
            // 每次都重新构建，确保参数调整能够及时生效
            var vAxisPos = new VAxisPos();
            foreach (var item in AxisPosItems)
            {
                vAxisPos.Add(item.Tag);
            }

            // 添加到对象
            result.Parameters.Add("VAxisPos", vAxisPos);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private VAxisPos GetNewVAxisPos()
        {
            var vAxisPos = new VAxisPos();
            foreach (var item in AxisPosItems)
            {
                vAxisPos.Add(item.Tag);
            }

            return vAxisPos;
        }
    }
}