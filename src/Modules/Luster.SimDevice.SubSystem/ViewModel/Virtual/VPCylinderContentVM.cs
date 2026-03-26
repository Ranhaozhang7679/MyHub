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
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Network;
using Luster.Motion.DataStruct.VDevice;
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
    public class VPCylinderContentVM : PageVM
    {
        /// <summary>
        /// 显示添加按钮
        /// </summary>
        public override bool IsShowAdd => true;

        /// <summary>
        /// 
        /// </summary>
        private IDialogService _dialogService;

        public DelegateCommand<object> EditCommuncationCommand { get; set; }

        protected VPCylinderContentVM(ISimDeviceEngineUI _engine, IDialogService dialogService) : base(_engine)
        {
            EditCommuncationCommand = new DelegateCommand<object>(EditCommuncation);
            Cylinders = new ObservableCollection<VPCylinder>();
            Protocols = typeof(Luster.Motion.DataStruct.Enums.ProtocolType).EnumToDataSource();
            CmdTypes = typeof(PClinderCmdType).EnumToDataSource();
            _dialogService = dialogService;
            LoadDevices();
        }

        private void EditCommuncation(object obj)
        {
            if (Current == null) return;

            _dialogService.ShowCommuncationDialog(Current.Communication, r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    if (r.Parameters.TryGetValue<ICommunication>("Model", out var comm))
                    {
                        Current.Communication = comm;
                        Communication = Current.Communication;
                        LoadDevices();
                    }
                }
            });
        }

        /// <summary>
        /// 协议
        /// </summary>
        public List<KeyValue> Protocols { get; set; }

        /// <summary>
        /// 数据类型
        /// </summary>
        public List<KeyValue> CmdTypes { get; set; }

        /// <summary>
        /// 对应的集合
        /// </summary>
        private ObservableCollection<PointPosModel> _pointPosModels;
        public ObservableCollection<PointPosModel> PointPosModels
        {
            get { return _pointPosModels; }
            set { SetProperty(ref _pointPosModels, value); }
        }


        /// <summary>
        /// 是否连接
        /// </summary>
        private bool _isConnect;
        public bool IsConnected
        {
            get { return _isConnect; }
            set { SetProperty(ref _isConnect, value); }
        }


        /// <summary>
        /// 加载所有的设备信息
        /// </summary>
        private void LoadDevices(string key = "")
        {
            var devices = deviceEngine.GetDevices(typeof(VPCylinder));
            Cylinders = new ObservableCollection<VPCylinder>();
            foreach (var device in devices)
            {
                var VCommuncation = device as VPCylinder;
                if (string.IsNullOrEmpty(key))
                {
                    Cylinders.Add(VCommuncation);
                }
                else
                {
                    if (device.Module.Contains(key) || device.Name.Contains(key))
                    {
                        Cylinders.Add(VCommuncation);
                    }
                }
            }
        }

        /// <summary>
        /// 通信
        /// </summary>
        private ObservableCollection<VPCylinder> vPCylinders;
        public ObservableCollection<VPCylinder> Cylinders
        {
            get { return vPCylinders; }
            set { SetProperty(ref vPCylinders, value); }
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
        /// 当前Socket
        /// </summary>
        private VPCylinder _curCylinder;
        public VPCylinder Current
        {
            get => _curCylinder;
            set
            {
                SetProperty(ref _curCylinder, value);
            }
        }

        /// <summary>
        /// 当前的协议
        /// </summary>
        private ProtocolType _protocol;
        public ProtocolType ProtocolType
        {
            get => _protocol;
            set
            {
                SetProperty(ref _protocol, value);
                if (Current != null)
                {
                    Current.SetProtocol(value);
                }
            }
        }

        /// <summary>
        /// 通讯地址
        /// </summary>
        private ICommunication _communication;
        public ICommunication Communication
        {
            get => _communication;
            set => SetProperty(ref _communication, value);
        }

        /// <summary>
        /// 选中设备
        /// </summary>
        private DelegateCommand<object> _selectedCommand;
        public DelegateCommand<object> SelectedCommand => _selectedCommand ?? (_selectedCommand = new DelegateCommand<object>((obj) =>
        {
            Current = obj as VPCylinder;
            if (Current == null) 
            {
                Communication = null;
                return;
            }
            Communication =Current.Communication;
            IsConnected = false;
            if (Current.Communication != null)
            {
                IsConnected = Current.Communication.IsConnected;
            }

            // 更新为字符串默认协议
            ProtocolType = Current.ProtocolType;

            // 集合
            PointPosModels = new ObservableCollection<PointPosModel>();
            if (Current != null)
            {
                foreach (var item in Current.Positions)
                {
                    PointPosModels.Add(new PointPosModel(item));
                }
            }
        }));

        /// <summary>
        /// 操作-删除
        /// </summary>
        private DelegateCommand<object> _removeCommand;
        public DelegateCommand<object> RemoveCommand => _removeCommand ?? (_removeCommand = new DelegateCommand<object>((obj) =>
        {
            var model = obj as VPCylinder;
            if (model != null)
            {
                dialogService.ShowConfirm($"确认删除:{model.Name}?", r =>
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        var device = deviceEngine.GetVirtualByID(model.ID);
                        deviceEngine.ReomoveVirtual(device.ID);
                        Cylinders.Remove(model);
                    }
                });
            }
        }));

        /// <summary>
        /// 操作-删除
        /// </summary>
        private DelegateCommand<object> _editCommand;
        public DelegateCommand<object> EditCommand => _editCommand ?? (_editCommand = new DelegateCommand<object>((obj) =>
        {
            var model = obj as VPCylinder;
            if (model != null)
            {
                dialogService.ShowPCylinder(model, r =>
                {
                    LoadDevices();
                });
            }
        }));

        /// <summary>
        /// 增加新的传感器
        /// </summary>
        public override void AddNewItem()
        {
            dialogService.ShowPCylinder(null, r =>
            {
                LoadDevices();
            });
        }

        /// <summary>
        /// 打开串口
        /// </summary>
        private DelegateCommand<VPCylinder> _connectCommand;
        public DelegateCommand<VPCylinder> ConnectCommand => _connectCommand ?? (_connectCommand = new DelegateCommand<VPCylinder>((socket) =>
        {
            var result = Current.Communication.Open();
            if (result.IsSuccess)
            {
                IsConnected = true;
            }
            else
            {
                throw new FriendlyException(result.ErrorMsg);
            }
        }));

        /// <summary>
        /// 关闭串口
        /// </summary>
        private DelegateCommand<VPCylinder> _closeCommand;
        public DelegateCommand<VPCylinder> CloseCommand => _closeCommand ?? (_closeCommand = new DelegateCommand<VPCylinder>((socket) =>
        {
            Current.Communication.Close();
            IsConnected = false;
        }));

        /// <summary>
        /// 添加
        /// </summary>
        private DelegateCommand<object> _addPointPosCommand;
        public DelegateCommand<object> AddPointPosCommand => _addPointPosCommand ?? (_addPointPosCommand = new DelegateCommand<object>((obj) =>
        {
            int index = 0;
            if (Current.Positions.Count > 0)
            {
                index = Current.Positions.Max(u => u.PointNo) + 1;
            }

            var pos = new PointPos() { PointNo = index, CmdType = PClinderCmdType.Abs, Speed = 50, Acc = 30, Dec = 30, Pos = 0, Pressure = 30, PreDistance = 10, Range = 0.1, Next = -1, Delay = 100 };
            Current.Positions.Add(pos);
            PointPosModels.Add(new PointPosModel(pos));
        }));

        /// <summary>
        /// 添加Action
        /// </summary>
        private DelegateCommand<object> _removePointPosCommand;
        public DelegateCommand<object> RemovePointPosCommand => _removePointPosCommand ?? (_removePointPosCommand = new DelegateCommand<object>((obj) =>
        {
            PointPosModel model = obj as PointPosModel;

            dialogService.ShowConfirm($"确认删除点位:{model.PointNo}?", r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    Current.Positions.RemoveAll(u => u.PointNo == model.PointNo);
                    PointPosModels.Remove(model);
                }
            });
        }));

        /// <summary>
        /// 保存配置
        /// </summary>
        private DelegateCommand _saveCommand;
        public DelegateCommand SaveCommand => _saveCommand ?? (_saveCommand = new DelegateCommand(() =>
        {
            if (Current == null) return;

            dialogService.ShowConfirm($"确认保存电缸:{Current.Name}?", r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    foreach (var item in Current.Positions)
                    {
                        Current.SetParam(item.PointNo);
                    }
                }
            });
        }));

        private T CommonRead<T>(string readMsg)
        {
            CommResult<T> cResult = Current.Protocol.Read<T>(Current.Communication, readMsg, 1);
            if (!cResult.IsSuccess)
            {
                throw new DeviceException(Current.AlarmCode, cResult.ErrorMsg);
            }

            return cResult.Datas[0];
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        private DelegateCommand _loadCommand;
        public DelegateCommand LoadCommand => _loadCommand ?? (_loadCommand = new DelegateCommand(() =>
        {
            if (Current == null) return;

            if (!IsConnected)
            {
                throw new FriendlyException($"通讯失败:{Current.Communication}");
            }

            Current.Positions.Clear();

            // 11 是默认重置力
            for (int i = 0; i <= 11; i++)
            {
                int v = CommonRead<int>($"01 3 {5000 + 20 * i} 1");
                if (v > 0)
                {
                    var pos = new PointPos() { PointNo = i };
                    Current.Positions.Add(pos);
                }
            }

            foreach (var item in Current.Positions)
            {
                Current.ReadParam(item.PointNo);
            }

            // 重新构建点位信息
            PointPosModels.Clear();
            foreach (var item in Current.Positions)
            {
                PointPosModels.Add(new PointPosModel(item));
            }
        }));

        /// <summary>
        /// 加载配置
        /// </summary>
        private DelegateCommand _homeCommand;
        public DelegateCommand HomeCommand => _homeCommand ?? (_homeCommand = new DelegateCommand(() =>
        {
            if (Current == null) return;

            Current.Home();
        }));


        #region 力缸动作
        /// <summary>
        /// 绝对运动
        /// </summary>
        private DelegateCommand<PointPosModel> _moveCommand;
        public DelegateCommand<PointPosModel> MoveCommand => _moveCommand ?? (_moveCommand = new DelegateCommand<PointPosModel>((pos) =>
        {
            System.Threading.Tasks.Task.Run(() =>
             {
                 //Current.ReadParam(pos.PointNo);
                 ////Current.SetParam(pos.PointNo);
                 Current.Move(pos.PointNo);
             });
        }));

        #region JOG运动模式
        /// <summary>
        /// JOG+点动
        /// </summary>
        private DelegateCommand<PointPosModel> _jogCommandAdd;
        public DelegateCommand<PointPosModel> JogCommandAdd => _jogCommandAdd ?? (_jogCommandAdd = new DelegateCommand<PointPosModel>((pos) =>
        {
            Task.Run(() =>
            {
                //axis.Tag.Jog(true, axis.MoveSpeed);

            });
        }));

        /// <summary>
        /// JOG-点动
        /// </summary>
        private DelegateCommand<PointPosModel> _jogCommandSub;
        public DelegateCommand<PointPosModel> JogCommandSub => _jogCommandSub ?? (_jogCommandSub = new DelegateCommand<PointPosModel>((pos) =>
        {
            Task.Run(() =>
            {
                //axis.Tag.Jog(false, axis.MoveSpeed);

            });
        }));

        /// <summary>
        /// 停止运动
        /// </summary>
        private DelegateCommand _stopCommand;
        public DelegateCommand StopCommand => _stopCommand ?? (_stopCommand = new DelegateCommand(() =>
        {
            Current.Stop();
        }));

        #endregion

        /// <summary>
        /// 清理错误信息
        /// </summary>
        private DelegateCommand _clearErrorCommand;
        public DelegateCommand ClearErrorCommand => _clearErrorCommand ?? (_clearErrorCommand = new DelegateCommand(() =>
        {
            Current.Reset();
        }));
        #endregion
    }
}