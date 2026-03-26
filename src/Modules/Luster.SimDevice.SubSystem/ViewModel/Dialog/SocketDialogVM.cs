using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Real;
using Luster.SimDevice.EngineUI;
using Luster.SimDevice.MotionCards;
using Luster.SimDevice.Real;
using Prism.Services.Dialogs;
using System.ComponentModel.DataAnnotations;
using Luster.SimDevice.EngineUI.Models;
using System.IO.Ports;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.VDevice;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.DataModels;
using Luster.Motion.DataStruct.Network;
using System.Net.Sockets;

namespace Luster.SimDevice.SubSystem.ViewModel.Dialog
{
    public class SocketDialogVM : ModuleDialogVM
    {
        protected SocketDialogVM(ISimDeviceEngineUI _engine) : base(_engine)
        {
            CommTypes = new List<string>() { "串口通信", "网口通信" };
            COMNameList = new List<string>() { "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7",
                "COM8", "COM9", "COM10", "COM11", "COM12" ,"COM13","COM14" ,"COM15" ,"COM16" ,"COM17" ,"COM18" ,"COM19",
                "COM20", "COM21", "COM22", "COM23", "COM24" ,"COM25","COM26" ,"COM27" ,"COM28" ,"COM29" ,"COM30"};
            BaudRateList = new List<int>() { 4800, 9600, 19200, 38400, 57600, 115200, 230400 };
            StopBitsList = typeof(StopBits).EnumToDataSource();
            ParityList = typeof(Parity).EnumToDataSource();
            SocketRoleList = typeof(SocketRole).EnumToDataSource();
        }

        public List<string> CommTypes { get; set; }


        /// <summary>
        /// 通信类型
        /// </summary>
        private string _commType = "网口通信";
        public string CommType
        {
            get { return _commType; }
            set { SetProperty(ref _commType, value); IsSocket = value == "网口通信"; }
        }

        /// <summary>
        /// 是否是Socket通信
        /// </summary>
        private bool _isSocket = true;
        public bool IsSocket
        {
            get { return _isSocket; }
            set { SetProperty(ref _isSocket, value); }
        }

        /// <summary>
        /// 名称
        /// </summary>
        private string _name;
        [Required]
        public string Name { get => _name; set => SetProperty(ref _name, value); }

        #region 网口通信
        /// <summary>
        /// Ip地址
        /// </summary>
        private string _ipAddress = "127.0.0.1";
        public string IpAddress { get => _ipAddress; set => SetProperty(ref _ipAddress, value); }

        /// <summary>
        /// 端口
        /// </summary>
        private int _port = 502;
        public int Port { get => _port; set => SetProperty(ref _port, value); }


        /// <summary>
        /// socket 类型
        /// </summary>
        private SocketRole _socketRole = SocketRole.Client;
        public SocketRole SocketRole { get => _socketRole; set => SetProperty(ref _socketRole, value); }

        /// <summary>
        /// socket 类型列表
        /// </summary>
        public List<KeyValue> SocketRoleList { get; set; }

        #endregion

        #region 串口通信
        /// <summary>
        /// 串口列表
        /// </summary>
        private List<string> _comNameList;
        public List<string> COMNameList { get => _comNameList; set => SetProperty(ref _comNameList, value); }

        /// <summary>
        /// 串口名称
        /// </summary>
        private string _comName = "COM3";
        public string COMName { get => _comName; set => SetProperty(ref _comName, value); }

        /// <summary>
        /// 波特率列表
        /// </summary>
        private List<int> _baudRateList;
        public List<int> BaudRateList { get => _baudRateList; set => SetProperty(ref _baudRateList, value); }

        /// <summary>
        /// 波特率
        /// </summary>
        private int _baudRate = 9600;
        public int BaudRate { get => _baudRate; set => SetProperty(ref _baudRate, value); }

        /// <summary>
        /// 数据位
        /// </summary>
        private string _dataBits = "8";
        public string DataBits { get => _dataBits; set => SetProperty(ref _dataBits, value); }

        /// <summary>
        /// 停止位列表
        /// </summary>
        public List<KeyValue> StopBitsList { get; set; }

        /// <summary>
        /// 停止位
        /// </summary>
        private StopBits _stopBits = StopBits.One;
        public StopBits StopBits { get => _stopBits; set => SetProperty(ref _stopBits, value); }

        /// <summary>
        /// 奇偶校验位列表
        /// </summary>
        public List<KeyValue> ParityList { get; set; }

        /// <summary>
        /// 奇偶校验位
        /// </summary>
        private Parity _parity;
        [Required]
        public Parity Parity { get => _parity; set => SetProperty(ref _parity, value); }
        #endregion

        private int _minValue;
        /// <summary>
        /// 最小
        /// </summary>
        [Required]
        public int MinValue { get => _minValue; set => SetProperty(ref _minValue, value); }

        private int _maxValue;
        /// <summary>
        /// 最大值
        /// </summary>
        [Required]
        public int MaxValue { get => _maxValue; set => SetProperty(ref _maxValue, value); }

        private int _targetValue;

        /// <summary>
        /// 目标值
        /// </summary>
        [Required]
        public int TargetValue { get => _targetValue; set => SetProperty(ref _targetValue, value); }



        private VCommuncation _current = null;
        /// <summary>
        /// 覆写对话框打开方法
        /// </summary>
        /// <param name="parameters"></param>
        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);
            if (parameters.TryGetValue<VCommuncation>("Model", out _current))
            {
                if (_current != null)
                {
                    Module = _current.Module;
                    Name = _current.Name;
                    if (_current.Communication is CommTCP tcp)
                    {
                        IsSocket = true;
                        CommType = "网口通信";
                        IpAddress = tcp.Network.Ip;
                        Port = tcp.Network.Port;
                        SocketRole = (SocketRole)Enum.Parse(typeof(SocketRole), tcp.Network.SocketRole);
                    }
                    else if (_current.Communication is CommSerial serial)
                    {
                        IsSocket = false;
                        CommType = "串口通信";
                        COMName = serial.PortName;
                        BaudRate = serial.BaudRate;
                        DataBits = serial.Databits.ToString();
                        StopBits = serial.StopBits;
                        Parity = serial.Parity;
                    }
                    MinValue = _current.MinValue;
                    MaxValue = _current.MaxValue;
                    TargetValue = _current.TargetValue;
                }
                else
                {
                    MinValue = 5;
                    MaxValue = 200;
                    TargetValue = 10;
                }
            }
        }

        protected override void Ok(IDialogResult result)
        {
            string newName = Name.Trim().ToLower();

            ICommunication comm = null;

            if (IsSocket)
            {

                if (string.IsNullOrEmpty(IpAddress))
                {
                    throw new DeviceException($" IpAddress 不能为空!");
                }

                comm = new CommTCP()
                {
                    Network = new LNetwork() { Ip = IpAddress, Port = Port, SocketRole = this.SocketRole.ToString() }
                };
            }
            else
            {
                if (string.IsNullOrEmpty(DataBits) || !int.TryParse(DataBits, out var dBit))
                {
                    throw new DeviceException("数据位格式不正确!");
                }

                comm = new CommSerial()
                {
                    Databits = dBit,
                    BaudRate = BaudRate,
                    Parity = Parity,
                    PortName = COMName,
                    StopBits = StopBits
                };
            }

            if (_current == null)
            {
                if (deviceEngine.GetVDevices<VCommuncation>().Any(u => u.Name.ToLower() == newName))
                {
                    throw new DeviceException($"名称:{newName}已经存在!");
                }

                _current = new VCommuncation()
                {
                    ID = Guid.NewGuid(),
                    Module = Module,
                    Name = newName,
                    Communication = comm
                };

                deviceEngine.AddVirtual(_current);
            }
            else
            {
                _current.Name = newName;
                _current.Module = Module;
                _current.Communication = comm;
            }
        }
    }
}
