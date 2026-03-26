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
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Extensions;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.SimDevice.EngineUI;
using Luster.SimDevice.EngineUI.Models;
using Luster.SimDevice.SubSystem.Extension;
using Prism.Commands;
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
    public class VAxisMContentVM : PageVM
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
        /// 轴列表
        /// </summary>
        private ObservableCollection<AxisModel> _axisList;
        public ObservableCollection<AxisModel> SubAxisList
        {
            get { return _axisList; }
            set { SetProperty(ref _axisList, value); }
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
        /// 优先级
        /// </summary>
        public List<KeyValue> Priorities { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="_engine">引擎</param>
        protected VAxisMContentVM(ISimDeviceEngineUI _engine, IDialogService dialogService) : base(_engine)
        {
            _dialogService = dialogService;
            MAxisList = new ObservableCollection<VAxisModels>();
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
            if (currentMVAxis != null)
            {
                var hasAxises = currentMVAxis.Tag.Axises.Select(u => u.AxisID).ToList();
                AxisDatas = deviceEngine.GetVDevices<VAxis>()
                       .Where(u => !hasAxises.Contains(u.ID))
                       .ToList();
            }
            else
            {
                AxisDatas = deviceEngine.GetVDevices<VAxis>()
                        .Where(u => u.ByUse == null)
                        .ToList();
            }
        }

        /// <summary>
        /// 加载所有的设备信息
        /// </summary>
        private void LoadDevices(string key = "")
        {
            var devices = deviceEngine.GetDevices(typeof(VAxisM));
            MAxisList = new ObservableCollection<VAxisModels>();
            foreach (var device in devices)
            {
                var vAxis = device as VAxisM;
                var model = new VAxisModels(vAxis);
                if (string.IsNullOrEmpty(key))
                {
                    MAxisList.Add(model);
                }
                else
                {
                    if (vAxis.Module.Contains(key) || vAxis.Name.Contains(key))
                    {
                        MAxisList.Add(model);
                    }
                }
            }
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

        private VAxisModels currentMVAxis;

        /// <summary>
        /// 选中设备
        /// </summary>
        private DelegateCommand<object> _selectedCommand;
        public DelegateCommand<object> SelectedCommand => _selectedCommand ?? (_selectedCommand = new DelegateCommand<object>((obj) =>
        {
            currentMVAxis = obj as VAxisModels;
            SubAxisList = new ObservableCollection<AxisModel>();
            if (currentMVAxis != null)
            {
                foreach (var item in currentMVAxis.Tag.Axises)
                {
                    AddSubItem(item);
                }

                RefreshAxisStatus();
                LoadCanUseAxis();
            }
        }));

        private void AddSubItem(AxisItem item)
        {
            var axisItem = new AxisModel(item.Axis, false);
            //axisItem.AxisItem = item;
            axisItem.MovePriority = item.MovePriority;
            axisItem.MoveSpeed = item.Speed;
            axisItem.CurrentPos = item.Position;

            SubAxisList.Add(axisItem);
        }

        /// <summary>
        /// 双击添加
        /// </summary>
        private DelegateCommand<object> _doubleAddCommand;
        public DelegateCommand<object> DoubleAddCommand => _doubleAddCommand ?? (_doubleAddCommand = new DelegateCommand<object>((obj) =>
        {
            if (currentMVAxis != null)
            {
                var axis = obj as VAxis;
                var newItem = new AxisItem() { Axis = axis, AxisID = axis.ID, MovePriority = axis.MovePriority, Speed = axis.MoveSpeed };

                // 1.添加到记录中
                currentMVAxis.Tag.Axises.Add(newItem);
                AddSubItem(newItem);
                axis.ByUse = currentMVAxis.Tag;

                deviceEngine.IsNeedSave = true;
                LoadCanUseAxis();
            }
        }));


        /// <summary>
        /// 刷新状态
        /// </summary>
        private void RefreshAxisStatus()
        {
            // 尝试刷新轴的状态
            foreach (var item in SubAxisList)
            {
                UpdateAxis(item);
            }
        }

        /// <summary>
        /// 搜索命名
        /// </summary>
        private DelegateCommand<object> _removeCommand;
        public DelegateCommand<object> RemoveCommand => _removeCommand ?? (_removeCommand = new DelegateCommand<object>((obj) =>
        {
            var model = obj as VAxisModels;
            if (model != null)
            {
                dialogService.ShowConfirm($"确认删除多轴:{model.Name}", r =>
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        var device = deviceEngine.GetVirtualByName(model.Name);
                        deviceEngine.ReomoveVirtual(device.ID);
                        MAxisList.Remove(model);
                    }
                });
            }
        }));



        /// <summary>
        /// 轴列表
        /// </summary>
        private ObservableCollection<VAxisModels> mAxisList;
        public ObservableCollection<VAxisModels> MAxisList
        {
            get { return mAxisList; }
            set { SetProperty(ref mAxisList, value); }
        }

        /// <summary>
        /// 移除
        /// </summary>
        public override void RemoveItem()
        {
            base.RemoveItem();
        }

        /// <summary>
        /// 绝对运动
        /// </summary>
        private DelegateCommand<AxisModel> _moveAbsCommand;
        public DelegateCommand<AxisModel> MoveAbsCommand => _moveAbsCommand ?? (_moveAbsCommand = new DelegateCommand<EngineUI.Models.AxisModel>((axis) =>
        {
            Task.Run(() =>
            {
                axis.Tag.MoveAbs(axis.CurrentPos, axis.MoveSpeed);
                UpdateAxis(axis);
            });
        }));

        /// <summary>
        /// 相对运动
        /// </summary>
        private DelegateCommand<AxisModel> _moveRelCommand;
        public DelegateCommand<AxisModel> MoveRelCommand => _moveRelCommand ?? (_moveRelCommand = new DelegateCommand<EngineUI.Models.AxisModel>((axis) =>
        {
            axis.Tag.MoveRel(axis.CurrentPos, axis.MoveSpeed);
            UpdateAxis(axis);
        }));

        private void UpdateAxis(AxisModel axis)
        {
            // 更新状态信息
            var dictStatus = axis.Tag.GetAxisStatus(false);
            foreach (var sItem in axis.StatusList)
            {
                if (dictStatus.ContainsKey(sItem.Name))
                {
                    sItem.Status = dictStatus[sItem.Name];
                }
                else
                {
                    sItem.Status = false;
                }
            }

            // 当前位置'
            UpdatePos(axis);
        }

        private void UpdatePos(AxisModel aModel)
        {
            if (whileToken != null)
            {
                whileToken.Cancel();
            }

            whileToken = new CancellationTokenSource();

            Task.Run(() =>
            {
                while (true)
                {
                    if (whileToken.IsCancellationRequested)
                    {
                        aModel.CurrentPos = aModel.Tag.GetCurrentPos();
                        break;
                    }

                    aModel.CurrentPos = aModel.Tag.GetCurrentPos();
                    Thread.Sleep(50);
                }
            }, whileToken.Token);

            Task.Run(() =>
            {
                aModel.Tag.CheckMotionDone();
                whileToken?.Cancel();
            });
        }


        /// <summary>
        /// 回零
        /// </summary>
        private DelegateCommand<AxisModel> _homeCommand;
        public DelegateCommand<AxisModel> HomeCommand => _homeCommand ?? (_homeCommand = new DelegateCommand<EngineUI.Models.AxisModel>((axis) =>
        {
            Task.Run(() => axis.Tag.Home());
            UpdateAxis(axis);
        }));

        #region JOG运动模式
        /// <summary>
        /// JOG+点动
        /// </summary>
        private DelegateCommand<AxisModel> _jogCommandAdd;
        public DelegateCommand<AxisModel> JogCommandAdd => _jogCommandAdd ?? (_jogCommandAdd = new DelegateCommand<EngineUI.Models.AxisModel>((axis) =>
        {
            Task.Run(() =>
            {
                axis.Tag.Jog(true, axis.MoveSpeed);
                UpdateAxis(axis);
            });
        }));

        /// <summary>
        /// JOG-点动
        /// </summary>
        private DelegateCommand<AxisModel> _jogCommandSub;
        public DelegateCommand<AxisModel> JogCommandSub => _jogCommandSub ?? (_jogCommandSub = new DelegateCommand<EngineUI.Models.AxisModel>((axis) =>
        {
            Task.Run(() =>
            {
                axis.Tag.Jog(false, axis.MoveSpeed);
                UpdateAxis(axis);
            });
        }));

        /// <summary>
        /// 停止运动
        /// </summary>
        private DelegateCommand<AxisModel> _stopCommand;
        public DelegateCommand<AxisModel> StopCommand => _stopCommand ?? (_stopCommand = new DelegateCommand<EngineUI.Models.AxisModel>((axis) =>
        {
            // 显示对象
            axis.Tag.Stop();
            whileToken?.Cancel();
        }));

        #endregion

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
        /// 清理错误信息
        /// </summary>
        private DelegateCommand<AxisModel> _removeAxisCommand;
        public DelegateCommand<AxisModel> RemoveAxisCommand => _removeAxisCommand ?? (_removeAxisCommand = new DelegateCommand<AxisModel>((axis) =>
        {
            dialogService.ShowConfirm($"确认删除轴:{axis.Name}", r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    axis.Tag.ByUse = null;
                    currentMVAxis.Tag.Axises.RemoveAll(u => u.AxisID == axis.ID);
                    SubAxisList.Remove(axis);
                    LoadCanUseAxis();
                }
            });
        }));

        /// <summary>
        /// 多轴移动
        /// </summary>
        private DelegateCommand<string> _axisCommand;
        public DelegateCommand<string> AxisCommand => _axisCommand ?? (_axisCommand = new DelegateCommand<string>((str) =>
        {
            if (currentMVAxis != null)
            {
                VAxisMDevice vMDevice = new VAxisMDevice();
                foreach (var item in SubAxisList)
                {
                    AxisItem newItem = new AxisItem() { Axis = item.Tag, Speed = item.MoveSpeed, Position = item.CurrentPos };
                    vMDevice.Items.Add(newItem);
                }

                if (str == "Move")
                {
                    currentMVAxis.Tag.Move(vMDevice);
                }
                else if (str == "LineMove")
                {
                    currentMVAxis.Tag.MoveLineAbs(vMDevice);
                }
                else if (str == "JOG")
                {
                    currentMVAxis.Tag.Jog(vMDevice);
                }
                else if (str == "Home")
                {
                    currentMVAxis.Tag.Home();
                }
                else if (str == "Stop")
                {
                    currentMVAxis.Tag.Stop();
                }
            }
        }));
    }
}