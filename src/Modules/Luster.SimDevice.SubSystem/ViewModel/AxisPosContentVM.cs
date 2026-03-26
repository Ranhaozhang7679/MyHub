#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       VAxisMContentVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.SubSystem.ViewModel.Virtual
* 文 件 名:       VAxisMContentVM.cs
* 创建时间:       2022/6/13 20:55:33
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      ca34e131-19a4-40c3-a4ff-f000a04478a3
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/13 20:55:33
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.Assets;
using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Extensions;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using Luster.SimDevice.EngineUI;
using Luster.SimDevice.EngineUI.Models;
using Luster.SimDevice.SubSystem.Extension;
using Prism.Commands;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Luster.SimDevice.SubSystem.ViewModel.Virtual
{
    public class AxisPosContentVM : PageVM
    {
        /// <summary>
        /// 是否添加
        /// </summary>
        public override bool IsShowAdd => true;

        /// <summary>
        /// 弹窗
        /// </summary>
        private IDialogService _dialogService;

        /// <summary>
        /// 点位列表
        /// </summary>
        private ObservableCollection<PosGroupModel> _Currents;
        public ObservableCollection<PosGroupModel> PosGroups
        {
            get { return _Currents; }
            set { SetProperty(ref _Currents, value); }
        }

        /// <summary>
        /// 轴列表
        /// </summary>
        private ObservableCollection<AxisModel> _SubAxisList;
        public ObservableCollection<AxisModel> SubAxisList
        {
            get { return _SubAxisList; }
            set { SetProperty(ref _SubAxisList, value); }
        }

        /// <summary>
        /// 轴列表
        /// </summary>
        private List<VAxis> axisDatas;
        public List<VAxis> AxisDatas
        {
            get => axisDatas; set => SetProperty(ref axisDatas, value);
        }

        /// <summary>
        /// 轴列表
        /// </summary>
        private PosGroupModel _current;
        public PosGroupModel Current
        {
            get => _current; set => SetProperty(ref _current, value);
        }

        /// <summary>
        /// 优先级
        /// </summary>
        public List<KeyValue> Priorities { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="_engine">引擎</param>
        protected AxisPosContentVM(ISimDeviceEngineUI _engine, IDialogService dialogService) : base(_engine)
        {
            _dialogService = dialogService;
            PosGroups = new ObservableCollection<PosGroupModel>();
            SubAxisList = new ObservableCollection<AxisModel>();

            AxisDatas = new List<VAxis>();
            Priorities = typeof(Priority).EnumToDataSource();
            LoadDevices();
        }

        /// <summary>
        /// 加载可用的轴
        /// </summary>
        private void LoadCanUseAxis()
        {
            if (SubAxisList != null && SubAxisList.Count > 0)
            {
                var hasAxises = SubAxisList.Select(u => u.ID).ToList();
                AxisDatas = deviceEngine.GetVDevices<VAxis>()
                       .Where(u => !hasAxises.Contains(u.ID) && u.AxisType != AxisType.None)
                       .ToList();
            }
            else
            {
                AxisDatas = deviceEngine.GetVDevices<VAxis>()
                        .Where(u => u.AxisType != AxisType.None)
                        .ToList();
            }
        }

        /// <summary>
        /// 加载所有的设备信息
        /// </summary>
        private void LoadDevices(string key = "")
        {
            PosGroups.Clear();
            var devices = deviceEngine.PosGroup;
            foreach (var device in devices)
            {
                var model = new PosGroupModel(device);
                PosGroups.Add(model);
            }

            LoadCanUseAxis();
        }

        /// <summary>
        /// 添加
        /// </summary>
        public override void AddNewItem()
        {
            _dialogService.ShowAxisMDialog(r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    if (r.Parameters.TryGetValue<VAxisM>("VAxisM", out var vM))
                    {
                        deviceEngine.AddVirtual(vM);
                        LoadDevices();
                    }
                }
            });
        }

        /// <summary>
        /// 搜索命名
        /// </summary>
        private DelegateCommand<string> _searchCommand;
        public DelegateCommand<string> SearchCommand => _searchCommand ?? (_searchCommand = new DelegateCommand<string>((key) =>
        {
            string trimKey = key.Trim();

            LoadDevices(trimKey);
        }));

        /// <summary>
        /// 选中设备
        /// </summary>
        private DelegateCommand<object> _selectedCommand;
        public DelegateCommand<object> SelectedCommand => _selectedCommand ?? (_selectedCommand = new DelegateCommand<object>((obj) =>
        {
            Current = obj as PosGroupModel;
            SubAxisList = new ObservableCollection<AxisModel>();
            if (Current != null)
            {
                foreach (var item in Current.Axises)
                {
                    var pos = Current.GetTeachPos(item.AxisType);
                    AddSubItem(item, pos.Position, pos.MovePriority);
                }
            }

            LoadCanUseAxis();
        }));

        /// <summary>
        /// 显示点位
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="pos"></param>
        private void AddSubItem(VAxis axis, double pos, Priority priority = Priority.Low)
        {
            var axisItem = new AxisModel(axis, false, whileToken);
            axisItem.TeachPos = pos;
            axisItem.MovePriority = priority;
            axisItem.PropertyChanged -= AxisItem_PropertyChanged;
            axisItem.PropertyChanged += AxisItem_PropertyChanged;
            SubAxisList.Add(axisItem);
        }

        private void AxisItem_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "MovePriority" && Current != null && sender is AxisModel m)
            {
                Current.UpdatePriority(m.AxisType, m.MovePriority);
            }
        }

        /// <summary>
        /// 双击添加
        /// </summary>
        private DelegateCommand<object> _doubleAddCommand;
        public DelegateCommand<object> DoubleAddCommand => _doubleAddCommand ?? (_doubleAddCommand = new DelegateCommand<object>((obj) =>
        {
            var axis = obj as VAxis;

            if (SubAxisList.Any(u => u.AxisNo == axis.AxisNo))
            {
                throw new FriendlyException($"轴:{axis.AxisNo}已经存在!");
            }

            if (SubAxisList.Any(u => u.AxisType == axis.AxisType))
            {
                throw new FriendlyException($"轴类型:{axis.AxisType}已经存在!");
            }

            // 1.添加到记录中
            AddSubItem(axis, double.NaN);

            if (Current != null)
            {
                // 2.添加轴列表
                Current.Axises.Add(axis);
            }

            LoadCanUseAxis();
        }));

        /// <summary>
        /// 双击添加
        /// </summary>
        private DelegateCommand<object> _deleteAxisCommand;
        public DelegateCommand<object> DeleteAxisCommand => _deleteAxisCommand ?? (_deleteAxisCommand = new DelegateCommand<object>((obj) =>
        {
            var axis = obj as AxisModel;

            // 1.添加到记录中
            SubAxisList.Remove(axis);

            if (Current != null)
            {
                // 2.将轴列表从集合中删除
                Current.RemovePos(axis.Tag);
            }

            LoadCanUseAxis();
        }));


        /// <summary>
        /// 搜索命名
        /// </summary>
        private DelegateCommand<object> _moveCommand;
        public DelegateCommand<object> MoveCommand => _moveCommand ?? (_moveCommand = new DelegateCommand<object>((obj) =>
        {
            var model = obj as PosGroupModel;
            if (model != null)
            {
                VAxisPos vPos = new VAxisPos();
                foreach (var item in model.Tag)
                {
                    vPos.Add(new AxisPosItem()
                    {
                        PosKey = Guid.NewGuid(),
                        AxisPostion = item,
                        MovePriority = item.MovePriority,
                        Acc = item.Axis.Acc,
                        Dec = item.Axis.Dec,
                        Speed = item.Axis.MoveSpeed,
                        MoveMode = MoveMode.Abs,
                    });
                }

                Task.Run(() =>
                {
                    // 开始运动
                    vPos.MovePostion(deviceEngine);
                });
            }
        }));

        /// <summary>
        /// 更新
        /// </summary>
        private DelegateCommand<object> _updateCommand;
        public DelegateCommand<object> UpdateCommand => _updateCommand ?? (_updateCommand = new DelegateCommand<object>((obj) =>
        {
            var model = obj as PosGroupModel;
            if (model != null)
            {
                dialogService.ShowConfirm($"点位:{model.Name}，确认将示教点更新为实时点位?", r =>
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        // 将当前的实时点位更新为示教点
                        model.Axises.ForEach(u =>
                        {
                            model.UpdatePos(u);
                        });
                    }
                });
            }
        }));


        /// <summary>
        /// 搜索命名
        /// </summary>
        private DelegateCommand<object> _removeCommand;
        public DelegateCommand<object> RemoveCommand => _removeCommand ?? (_removeCommand = new DelegateCommand<object>((obj) =>
        {
            var model = obj as PosGroupModel;
            if (model != null)
            {
                dialogService.ShowConfirm($"确认删除点位:{model.Name}", r =>
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        deviceEngine.RemovePosGroup(model.Name);
                        LoadDevices();
                    }
                });
            }
        }));

        /// <summary>
        /// 移除
        /// </summary>
        public override void RemoveItem()
        {
            base.RemoveItem();
        }

        /// <summary>
        /// 清理错误信息
        /// </summary>
        private DelegateCommand<AxisModel> _clearErrorCommand;
        public DelegateCommand<AxisModel> ClearErrorCommand => _clearErrorCommand ?? (_clearErrorCommand = new DelegateCommand<AxisModel>((axis) =>
        {
            // 显示对象
            axis.Tag.ResetStatus();
        }));

        /// <summary>
        /// 多轴移动
        /// </summary>
        private DelegateCommand<string> _axisCommand;
        public DelegateCommand<string> AxisCommand => _axisCommand ?? (_axisCommand = new DelegateCommand<string>((str) =>
        {
            // 统一功能
            foreach (var item in SubAxisList)
            {
                if (str == "ClearError")
                {
                    item.Tag.ResetStatus();
                }
                else if (str == "ServOn")
                {
                    item.Tag.ServOn(true);
                }
                else if (str == "ServOff")
                {
                    item.Tag.ServOn(false);
                }
                else if (str == "Stop")
                {
                    item.Tag.Stop();
                }
            }

            // 如果示教，就将数据更新到点位中
            if (str == "Teach")
            {
                if (Current == null)
                {
                    _dialogService.ShowTeachPositionDialog(0, "", true, r =>
                    {
                        if (r.Result == ButtonResult.OK)
                        {
                            if (r.Parameters.TryGetValue<string>("Name", out var name))
                            {
                                deviceEngine.TeachPosGroup(name, SubAxisList.Select(u => u.Tag).ToArray());
                                LoadDevices();
                            }
                        }
                    });
                }
            }
        }));

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
            Enter();
        }

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            base.OnNavigatedFrom(navigationContext);
            Leave();
        }

        public override void Enter()
        {
            if (whileToken != null)
            {
                whileToken?.Cancel();
                Thread.Sleep(300);
            }

            whileToken = new System.Threading.CancellationTokenSource();

        }

        public override void Leave()
        {
            whileToken?.Cancel();
            Thread.Sleep(200);
            whileToken = null;
        }
    }
}