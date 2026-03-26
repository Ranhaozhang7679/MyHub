#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       CommuncationAddressDialogVM
* 机器名称:       Z05592
* 命名空间:       Luster.SimDevice.SubSystem.ViewModel.Dialog
* 文 件 名:       CommuncationAddressDialogVM.cs
* 创建时间:       2022/11/24 10:37:02
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      4e0243b3-f98c-4989-8457-e3bf103a76a1
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/11/24 10:37:02
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using Luster.Common.DataStruct.DataModels;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.Network;
using Luster.SimDevice.EngineUI;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.SubSystem.ViewModel.Dialog
{
    public class CommuncationAddressDialogVM : SocketDialogVM
    {
        private ICommunication _communication;
        protected CommuncationAddressDialogVM(ISimDeviceEngineUI _engine) : base(_engine)
        {
            //  [Required]属性，此处不需要，随意赋值
            Module = "system";
            Name = "system";
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.TryGetValue("Title", out string title))
            {
                Title = title.Trim();
            }

            if (parameters.TryGetValue("Model", out _communication))
            {
                if (_communication != null)
                {
                    if (_communication is CommTCP tcp)
                    {
                        IsSocket = true;
                        CommType = "网口通信";
                        IpAddress = tcp.Network.Ip;
                        Port = tcp.Network.Port;
                    }
                    else if (_communication is CommSerial serial)
                    {
                        IsSocket = false;
                        CommType = "串口通信";
                        COMName = serial.PortName;
                        BaudRate = serial.BaudRate;
                        DataBits = serial.Databits.ToString();
                        StopBits = serial.StopBits;
                        Parity = serial.Parity;
                    }
                }
            }
        }

        protected override void Ok(IDialogResult result)
        {
            if (_communication != null)
            {
                if (IsSocket)
                {
                    if (string.IsNullOrEmpty(IpAddress))
                    {
                        throw new DeviceException($" IpAddress 不能为空!");
                    }

                    _communication = new CommTCP()
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
                    _communication = new CommSerial()
                    {
                        Databits = dBit,
                        BaudRate = BaudRate,
                        Parity = Parity,
                        PortName = COMName,
                        StopBits = StopBits
                    };
                }
                result.Parameters.Add("Model", _communication);
            }
        }
    }
}
