using Luster.Common.Assets;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Extensions;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.DataStruct.Network;
using Luster.Motion.DataStruct.VDevice;
using Luster.SimDevice.EngineUI;
using Luster.SimDevice.EngineUI.Models;
using Luster.SimDevice.SubSystem.Extension;
using Prism.Commands;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using static FreeSql.Internal.GlobalFilter;

namespace Luster.SimDevice.SubSystem.ViewModel.Virtual
{
    public class VCommuncationContentVM : PageVM
    {
        /// <summary>
        /// 显示添加按钮
        /// </summary>
        public override bool IsShowAdd => true;
        public DelegateCommand<object> EditCommuncationCommand { get; set; }
        /// <summary>
        /// 
        /// </summary>
        private IDialogService _dialogService;

        protected VCommuncationContentVM(ISimDeviceEngineUI _engine, IDialogService dialogService) : base(_engine)
        {
            Sockets = new ObservableCollection<VCommuncation>();
            Protocols = typeof(Luster.Motion.DataStruct.Enums.ProtocolType).EnumToDataSource();
            Brands =typeof(BrandType).EnumToDataSource();
            DataTypes = typeof(DataType).EnumToDataSource();
            ActionTypes = typeof(Luster.Motion.DataStruct.Enums.ActionType).EnumToDataSource();
            EditCommuncationCommand = new DelegateCommand<object>(EditCommuncation);

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
        /// 品牌
        /// </summary>
        public List<KeyValue> Brands { get; set; }

        /// <summary>
        /// 数据类型
        /// </summary>
        public List<KeyValue> DataTypes { get; set; }

        /// <summary>
        /// 数据类型
        /// </summary>
        public List<KeyValue> ActionTypes { get; set; }

        /// <summary>
        /// 对应的集合
        /// </summary>
        private ObservableCollection<SocketActionModel> _actionModels;
        public ObservableCollection<SocketActionModel> ActionModels
        {
            get { return _actionModels; }
            set { SetProperty(ref _actionModels, value); }
        }

        /// <summary>
        /// 对应MoudbusRTU的从站集合
        /// </summary>
        private ObservableCollection<SocketAction> _slaveModels;
        public ObservableCollection<SocketAction> SlaveModels
        {
            get { return _slaveModels; }
            set { SetProperty(ref _slaveModels, value); }
        }

        /// <summary>
        /// 输入列的名称
        /// </summary>
        private string _headerName = "从站名称";
        public string HeaderName
        {
            get { return _headerName; }
            set { SetProperty(ref _headerName, value); }
        }

        /// <summary>
        /// 输入列的名称
        /// </summary>
        private string _headerValue = "从站站号";
        public string HeaderValue
        {
            get { return _headerValue; }
            set { SetProperty(ref _headerValue, value); }
        }

        /// <summary>
        /// 数据类型
        /// </summary>
        private DataType dataType = DataType.Bool;
        public DataType DataType
        {
            get { return dataType; }
            set { SetProperty(ref dataType, value); }
        }

        /// <summary>
        /// 是否Modbus
        /// </summary>
        private bool _isModbus;
        public bool IsModbus
        {
            get { return _isModbus; }
            set { SetProperty(ref _isModbus, value); }
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
        /// 是否连接
        /// </summary>
        private int _stationNo;
        public int StationNo
        {
            get { return _stationNo; }
            set { SetProperty(ref _stationNo, value); }
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
        /// 加载所有的设备信息
        /// </summary>
        private void LoadDevices(string key = "")
        {
            var devices = deviceEngine.GetDevices(typeof(VCommuncation));
            Sockets = new ObservableCollection<VCommuncation>();
            foreach (var device in devices)
            {
                var VCommuncation = device as VCommuncation;
                if (string.IsNullOrEmpty(key))
                {
                    Sockets.Add(VCommuncation);
                }
                else
                {
                    if (device.Module.Contains(key) || device.Name.Contains(key))
                    {
                        Sockets.Add(VCommuncation);
                    }
                }
            }
        }

        /// <summary>
        /// 通信
        /// </summary>
        private ObservableCollection<VCommuncation> _sockets;
        public ObservableCollection<VCommuncation> Sockets
        {
            get { return _sockets; }
            set { SetProperty(ref _sockets, value); }
        }

        protected override void Subscribe(ISimDeviceEngineUI engineUI)
        {
            base.Subscribe(engineUI);
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
        private VCommuncation curSocket;
        public VCommuncation Current
        {
            get => curSocket;
            set
            {
                SetProperty(ref curSocket, value);
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
        /// 当前品牌
        /// </summary>
        private BrandType _brand;
        public BrandType Brand
        {
            get => _brand;
            set
            {
                SetProperty(ref _brand, value);
                if (Current != null)
                {
                    Current.SetBrand(value);
                }
            }
        }
      
        private Visibility _isViewDgAction;
        public Visibility IsViewDgAction
        {
            get => _isViewDgAction;
            set { SetProperty(ref _isViewDgAction, value); }
        }

        private Visibility _isViewDgSlave;
        public Visibility IsViewDgSlave
        {
            get => _isViewDgSlave;
            set { SetProperty(ref _isViewDgSlave, value); }
        }

        /// <summary>
        /// 选中设备
        /// </summary>
        private DelegateCommand<object> _selectedCommand;
        public DelegateCommand<object> SelectedCommand => _selectedCommand ?? (_selectedCommand = new DelegateCommand<object>((obj) =>
        {
            Current = obj as VCommuncation;
            if (Current == null) 
            {
                Communication = null;
                return;
            }
            Communication = Current.Communication;
            IsConnected = false;
            if (Current.Communication != null)
            {
                IsConnected = Current.Communication.IsConnected;
            }

            // 更新为字符串默认协议
            ProtocolType = Current.ProtocolType;

            // 更新品牌
            Brand = Current.Brand;

            // 集合
            if (Current.ProtocolType == ProtocolType.ModbusRTU || Current.ProtocolType == ProtocolType.StringDefault)
            {
                if (Current.ProtocolType == ProtocolType.ModbusRTU)
                {
                    HeaderName = "从站名称";
                    HeaderValue = "从站站号";
                }
                else if (Current.ProtocolType == ProtocolType.StringDefault)
                {
                    HeaderName = "补偿名称";
                    HeaderValue = "补偿值";
                }
                Current.UserParam = new List<string> { HeaderName, HeaderValue };
                SlaveModels = new ObservableCollection<SocketAction>();
                foreach (var item in Current.Actions)
                {
                    SlaveModels.Add(item);
                }
                IsViewDgAction = Visibility.Hidden;
                IsViewDgSlave = Visibility.Visible;
            }
            else
            {
                ActionModels = new ObservableCollection<SocketActionModel>();
                foreach (var item in Current.Actions)
                {
                    ActionModels.Add(new SocketActionModel(item));
                }
                IsViewDgAction = Visibility.Visible;
                IsViewDgSlave = Visibility.Hidden;
            }
        }));

        /// <summary>
        /// 操作-删除
        /// </summary>
        private DelegateCommand<object> _removeCommand;
        public DelegateCommand<object> RemoveCommand => _removeCommand ?? (_removeCommand = new DelegateCommand<object>((obj) =>
        {
            var model = obj as VCommuncation;
            if (model != null)
            {
                dialogService.ShowConfirm($"确认删除:{model.Name}?", r =>
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        var device = deviceEngine.GetVirtualByID(model.ID);
                        deviceEngine.ReomoveVirtual(device.ID);
                        Sockets.Remove(model);
                    }
                });
            }
        }));

        /// <summary>
        /// 操作-编辑
        /// </summary>
        private DelegateCommand<object> _editCommand;
        public DelegateCommand<object> EditCommand => _editCommand ?? (_editCommand = new DelegateCommand<object>((obj) =>
        {
            var model = obj as VCommuncation;
            if (model != null)
            {
                dialogService.ShowSocketDialog(model, r =>
                {
                    LoadDevices();
                });
            }
        }));

        /// <summary>
        /// 操作-删除从站
        /// </summary>
        private DelegateCommand<object> _removeSlaveCommand;
        public DelegateCommand<object> RemoveSlaveCommand => _removeSlaveCommand ?? (_removeSlaveCommand = new DelegateCommand<object>((obj) =>
        {
            var model = obj as SocketAction;
            if (model != null)
            {
                dialogService.ShowConfirm($"确认删除:{model.Name}?", r =>
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        curSocket.Actions.Remove(model);
                        SlaveModels.Remove(model);
                    }
                });
            }
        }));


        /// <summary>
        /// 增加新的传感器
        /// </summary>
        public override void AddNewItem()
        {
            dialogService.ShowSocketDialog(null, r =>
            {
                LoadDevices();
            });
        }

        /// <summary>
        /// 打开串口
        /// </summary>
        private DelegateCommand<VCommuncation> _connectCommand;
        public DelegateCommand<VCommuncation> ConnectCommand => _connectCommand ?? (_connectCommand = new DelegateCommand<VCommuncation>((socket) =>
        {
            Current.Open();

            IsConnected = true;
        }));

        /// <summary>
        /// 关闭串口
        /// </summary>
        private DelegateCommand<VCommuncation> _closeCommand;
        public DelegateCommand<VCommuncation> CloseCommand => _closeCommand ?? (_closeCommand = new DelegateCommand<VCommuncation>((socket) =>
        {
            Current.Close();
            IsConnected = false;
        }));

        /// <summary>
        /// 读取消息
        /// </summary>
        private string _readMsg;
        public string ReadMsg
        {
            get { return _readMsg; }
            set { _readMsg = value; }
        }

        /// <summary>
        /// 写入的消息
        /// </summary>
        private string _writeMsg;
        public string WriteMsg
        {
            get { return _writeMsg; }
            set { _writeMsg = value; }
        }

        /// <summary>
        /// 返回结果
        /// </summary>
        private string _resultMsg;
        public string ResultMsg
        {
            get { return _resultMsg; }
            set { SetProperty(ref _resultMsg, value); }
        }

        /// <summary>
        /// 写入值
        /// </summary>
        private string _writeValue;
        public string WriteValue
        {
            get { return _writeValue; }
            set { SetProperty(ref _writeValue, value); }
        }

        /// <summary>
        /// 读指令
        /// </summary>
        private DelegateCommand<VCommuncation> _readCommand;
        public DelegateCommand<VCommuncation> ReadCommand => _readCommand ?? (_readCommand = new DelegateCommand<VCommuncation>((socket) =>
        {
            if (DataType == DataType.Bool)
            {
                var result = Current.Read<bool>(ReadMsg);
                ResultMsg = "结果：" + String.Join(",", result);
            }
            else if (DataType == DataType.Int)
            {
                var result = Current.Read<int>(ReadMsg);
                ResultMsg = "结果：" + String.Join(",", result);
            }
            else if (DataType == DataType.Float)
            {
                var result = Current.Read<float>(ReadMsg);
                ResultMsg = "结果：" + String.Join(",", result);
            }
            else if (DataType == DataType.Double)
            {
                var result = Current.Read<double>(ReadMsg);
                ResultMsg = "结果：" + String.Join(",", result);
            }
            else if (DataType == DataType.String)
            {
                var result = Current.Read<string>(ReadMsg);
                ResultMsg = "结果：" + String.Join(",", result);
            }
            else if (DataType == DataType.Short)
            {
                var result = Current.Read<short>(ReadMsg);
                ResultMsg = "结果：" + String.Join(",", result);
            }
        }));

        /// <summary>
        /// 读指令
        /// </summary>
        private DelegateCommand _waitCommand;
        public DelegateCommand WaitCommand => _waitCommand ?? (_waitCommand = new DelegateCommand(() =>
        {
            if (DataType == DataType.Bool)
            {
                Current.Wait<bool>(Guid.Empty, true, OpRule.Equal, ReadMsg);
                //ResultMsg = "结果：" + String.Join(",", result);
            }
            else if (DataType == DataType.Int)
            {
                int val = int.Parse(WriteValue);
                Current.Wait<int>(Guid.Empty, val, OpRule.Equal, ReadMsg);
                ResultMsg = $"阈值>={val}";
            }
            else if (DataType == DataType.Float)
            {
                var result = Current.Read<float>(ReadMsg);
                ResultMsg = "结果：" + String.Join(",", result);
            }
            else if (DataType == DataType.Double)
            {
                var result = Current.Read<double>(ReadMsg);
                ResultMsg = "结果：" + String.Join(",", result);
            }
            else if (DataType == DataType.String)
            {
                var result = Current.Read<string>(ReadMsg);
                ResultMsg = "结果：" + String.Join(",", result);
            }
            else if (DataType == DataType.Short)
            {
                var result = Current.Read<short>(ReadMsg);
                ResultMsg = "结果：" + String.Join(",", result);
            }
        }));

        /// <summary>
        /// 写指令
        /// </summary>
        private DelegateCommand<VCommuncation> _writeCommand;
        public DelegateCommand<VCommuncation> WriteCommand => _writeCommand ?? (_writeCommand = new DelegateCommand<VCommuncation>((socket) =>
        {
            if (DataType == DataType.Bool)
            {
                if (bool.TryParse(WriteValue, out var v))
                {
                    Current.Write<bool>(v, WriteMsg);
                }
            }
            else if (DataType == DataType.Int)
            {
                var vals = WriteValue.Split<int>();
                Current.Write<int>(vals.ToList(), WriteMsg);
            }
            else if (DataType == DataType.Float)
            {
                if (float.TryParse(WriteValue, out var v))
                {
                    Current.Write<float>(v, WriteMsg);
                }
            }
            else if (DataType == DataType.Double)
            {
                if (double.TryParse(WriteValue, out var v))
                {
                    Current.Write<double>(v, WriteMsg);
                }
            }
            else if (DataType == DataType.String)
            {
                Current.Write<string>(WriteValue, WriteMsg);
            }
            else if (DataType == DataType.Short)
            {
                var vals = WriteValue.Split<short>().ToList();
                Current.Write<short>(vals, WriteMsg);
            }
        }));

        /// <summary>
        /// 清空发送区
        /// </summary>
        private DelegateCommand<string> _clearTxtCommand;
        public DelegateCommand<string> ClearTxtCommand => _clearTxtCommand ?? (_clearTxtCommand = new DelegateCommand<string>((type) =>
        {
            if (type == "read")
            {
                ReadMsg = "";
            }
            else
            {
                WriteMsg = "";
            }
        }));

        /// <summary>
        /// 添加Action
        /// </summary>
        private DelegateCommand<object> _addActionCommand;
        public DelegateCommand<object> AddActionCommand => _addActionCommand ?? (_addActionCommand = new DelegateCommand<object>((obj) =>
        {
            if (obj is VCommuncation) 
            {
                var vcommucation = (VCommuncation)obj;
                if(vcommucation.ProtocolType==ProtocolType.ModbusRTU || vcommucation.ProtocolType == ProtocolType.StringDefault)
                {
                    _dialogService.ShowSlaveActionDialog(Current, null, r =>
                    {
                        if (r.Parameters.TryGetValue<SocketAction>("Action", out var action))
                        {
                            if (Current != null)
                            {
                                SlaveModels.Add(action);
                            }
                        }
                    });
                }
                else
                {
                    _dialogService.ShowSocketActionDialog(Current, null, r =>
                    {
                        if (r.Parameters.TryGetValue<SocketAction>("Action", out var action))
                        {
                            if (Current != null)
                            {
                                ActionModels.Add(new SocketActionModel(action));
                            }
                        }
                    });
                }
            }
        }));

        /// <summary>
        /// 添加Action
        /// </summary>
        private DelegateCommand<object> _removeActionCommand;
        public DelegateCommand<object> RemoveActionCommand => _removeActionCommand ?? (_removeActionCommand = new DelegateCommand<object>((obj) =>
        {
            SocketActionModel model = obj as SocketActionModel;

            dialogService.ShowConfirm($"确认删除动作:{model.Name}?", r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    Current.Actions.RemoveAll(u => u.Name == model.Name);
                    ActionModels.Remove(model);
                }
            });
        }));

        /// <summary>
        /// 添加Action
        /// </summary>
        private DelegateCommand<object> _editActionCommand;
        public DelegateCommand<object> EditActionCommand => _editActionCommand ?? (_editActionCommand = new DelegateCommand<object>((obj) =>
        {
            SocketActionModel model = obj as SocketActionModel;
            _dialogService.ShowSocketActionDialog(Current, model);
        }));

        //// CurrentModels - 当前显示的数据源
        //private object _currentModels;
        //public object CurrentModels
        //{
        //    get => _currentModels;
        //    set { SetProperty(ref _currentModels, value); }
        //}

        //// 模式枚举
        //public enum DisplayMode
        //{
        //    ACTIONModels,
        //    SLAVEModels
        //}
    }

   
}
