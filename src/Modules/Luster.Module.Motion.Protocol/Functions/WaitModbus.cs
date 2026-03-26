#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ModbusCheck
* 机器名称:       L05123-NB
* 命名空间:       Luster.Module.Motion.Protocol.Functions
* 文 件 名:       ModbusCheck.cs
* 创建时间:       2022/7/25 13:40:15
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      7f4d76d4-422d-4c5b-bea2-f43ce8fc99a5
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/25 13:40:15
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Motion.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.VDevice;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luster.Motion.DataStruct.Enums;
using Luster.TaskFlow.Motion.Interfaces;
using Luster.Motion.DataStruct.Network;
using Luster.TaskFlow.Common.Enums;

namespace Luster.Module.Motion.Protocol.Functions
{
    /// <summary>
    /// 值等待
    /// </summary>
    public class WaitModbus : MotionFunction, IWait
    {
        private const int WaitSleep = 50;

        [NotEmpty]
        [Parameter("通信设备", 0, CN = "通信设备", EditorType = typeof(VCommuncation))]
        public VDevice CommDevice { get; set; }

        [Parameter("设备地址", 1, CN = "协议类型", DefaultV = ProtocolType.ModbusTCP)]
        public ProtocolType ProtocolType { get; set; }

        [Parameter("设备地址", 2, CN = "设备地址", DefaultV = 1)]
        public int Address { get; set; }

        [Parameter("设备地址", 3, CN = "数据类型")]
        public DataType DataType { get; set; }

        [Parameter("阈值规则", 3, CN = "阈值规则", DefaultV = OpRule.Equal)]
        public OpRule OpRule { get; set; }

        [Parameter("内存地址,十进制值", 4, CN = "内存地址")]
        public int StartAddress { get; set; }

        [DependOn("DataType", DataType.Bool)]
        [Parameter("持续等待线圈值", 5, CN = "线圈值", ParamType = ParamType.IN, CanRef = ParamRef.Ref)]
        public bool CoilVal { get; set; }

        [DependOn("DataType", DataType.Int, DataType.Short)]
        [Parameter("持续等寄存器值", 6, CN = "寄存器值", ParamType = ParamType.IN, CanRef = ParamRef.Ref)]
        public int RegisterVal { get; set; }

        [Limit(-1, 1000000)]
        [Parameter("超时时间，单位为s", 7, CN = "超时时间", DefaultV = -1)]
        public  int OverTime { get; set; }


        public WaitModbus()
        {
            this.Tips = "持续等待地址上的值是否和当前值相等";
            this.Icon = "\xe692";
        }

        private VCommuncation comm = null;

        public override bool DoExcute(out string errMsg)
        {

            int waitOveTime = OverTime * 1000;
            GetVDevice<VCommuncation>(CommDevice, out var cDevice);

            if (comm == null)
            {
                comm = cDevice.Clone() as VCommuncation;
            }

            comm.Open();
            comm.SetProtocol(ProtocolType);

            if (DataType == DataType.Bool)
            {
                comm.Wait<bool>(MyOwner.ID, CoilVal, OpRule, $"{Address} 01 {StartAddress} 1", waitOveTime);
            }
            else if (DataType == DataType.Short)
            {
                short val = (short)RegisterVal;
                comm.Wait<short>(MyOwner.ID, val, OpRule, $"{Address} 03 {StartAddress} 1", waitOveTime);
            }
            else if (DataType == DataType.Int)
            {
                comm.Wait<int>(MyOwner.ID, RegisterVal, OpRule, $"{Address} 03 {StartAddress} 1", waitOveTime);
            }
            else if (DataType == DataType.Double)
            {
                comm.Wait<double>(MyOwner.ID, RegisterVal, OpRule, $"{Address} 03 {StartAddress} 1", waitOveTime);
            }

            return base.DoExcute(out errMsg);
        }

        /// <summary>
        /// 模块关闭
        /// </summary>
        public override void Stop()
        {
            if (comm == null) return;
            comm?.Stop();
            comm?.Close();
            comm = null;
        }
    }
}