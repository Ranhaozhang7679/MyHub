#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       VPlcDialogVM
* 机器名称:       Z05592
* 命名空间:       Luster.SimDevice.SubSystem.ViewModel.Dialog
* 文 件 名:       VPlcDialogVM.cs
* 创建时间:       2022/11/18 17:00:40
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      9c6a0281-0658-47cc-a843-7f33833eb75b
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/11/18 17:00:40
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using HandyControl.Controls;
using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Extensions;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Network;
using Luster.Motion.DataStruct.VDevice;
using Luster.SimDevice.EngineUI;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Luster.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;

namespace Luster.SimDevice.SubSystem.ViewModel.Dialog
{
    internal class VPlcDialogVM : SocketDialogVM
    {
        /// <summary>
        /// 当前的协议
        /// </summary>
        private ProtocolType _protocol = ProtocolType.ModbusTCP;
        public ProtocolType ProtocolType
        {
            get => _protocol;
            set
            {
                SetProperty(ref _protocol, value);
            }
        }


        /// <summary>
        /// 字节顺序
        /// </summary>
        private EndianType _endianType = EndianType.ABCD;
        public EndianType EndianType
        {
            get => _endianType;
            set
            {
                SetProperty(ref _endianType, value);
            }
        }

        /// <summary>
        /// 协议
        /// </summary>
        public List<KeyValue> Protocols { get; set; }

        /// <summary>
        /// 协议对象
        /// </summary>
        public List<KeyValue> EndianTypes { get; set; }

        /// <summary>
        /// 刷新频率
        /// </summary>
        private int _frequency = 50;
        public int Frequency
        {
            get => _frequency;
            set => SetProperty(ref _frequency, value);
        }

        protected VPlcDialogVM(ISimDeviceEngineUI _engine) : base(_engine)
        {
            Protocols = typeof(ProtocolType).EnumToDataSource();
            EndianTypes = typeof(EndianType).EnumToDataSource();
        }


        protected override void Ok(IDialogResult result)
        {
            var existDevice = deviceEngine.GetDevices(typeof(VPlc)).FirstOrDefault(x => x.Name == Name);
            if (existDevice != null)
            {
                throw new FriendlyException($"PLC 设备已存在，名称；{Name}!");
            }

            ICommunication comm = null;

            if (IsSocket)
            {

                if (string.IsNullOrEmpty(IpAddress))
                {
                    throw new DeviceException($" IpAddress 不能为空!");
                }

                comm = new CommTCP()
                {
                    Network = new LNetwork() { Ip = IpAddress, Port = Port }
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

            //构建虚拟设备对象
            VPlc vPlc = new VPlc()
            {
                Address = new List<PlcAddr>(),
                ID = Guid.NewGuid(),
                Module = Module,
                Name = Name,
                ProtocolType = ProtocolType,
                EndianType = EndianType,
                PlcComm = comm,
                Frequency = Frequency
            };

            // 添加对象
            deviceEngine.AddVirtual(vPlc);
        }
    }
}
