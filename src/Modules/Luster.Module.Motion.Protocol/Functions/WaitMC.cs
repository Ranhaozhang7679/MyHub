using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.VDevice;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Protocol.Functions
{
    /// <summary>
    /// MC 等待
    /// </summary>
    public class WaitMC:MotionFunction,IWait
    {

        [NotEmpty]
        [Parameter("通信设备", 0, CN = "通信设备", EditorType = typeof(VCommuncation))]
        public VDevice CommDevice { get; set; }

        [Parameter("设备地址", 2, CN = "设备地址", DefaultV = 1)]
        public string Address { get; set; }

        [Parameter("寄存器类型", 3, CN = "寄存器类型")]
        public PLCDataType DataType { get; set; }

        [Parameter("阈值规则", 4, CN = "阈值规则", DefaultV = OpRule.Equal)]
        public OpRule OpRule { get; set; }


        [DependOn("DataType", PLCDataType.X, PLCDataType.Y,PLCDataType.M,PLCDataType.L,PLCDataType.F,PLCDataType.V,PLCDataType.B)]
        [Parameter("持续等待线圈值", 5, CN = "线圈值", ParamType = ParamType.IN, CanRef = ParamRef.Ref)]
        public bool bVal { get; set; }

        [DependOn("DataType", PLCDataType.W, PLCDataType.D)]
        [Parameter("持续等寄存器值", 6, CN = "寄存器值", ParamType = ParamType.IN, CanRef = ParamRef.Ref)]
        public short iVal { get; set; }

        [Limit(-1, 1000000)]
        [Parameter("超时时间，单位为s", 7, CN = "超时时间", DefaultV = -1)]
        public int OverTime { get; set; }




        public WaitMC() 
        {
            this.Tips = "等待MC";
            this.Icon = "\xe6d6";
        }

        private VCommuncation comm = null;

        public override bool DoExcute(out string errMsg)
        {
            int waitOverTime = OverTime * 1000;
            GetVDevice<VCommuncation>(CommDevice, out var cDevice);
            if(comm==null)
            {
                comm=cDevice.Clone() as VCommuncation;
            }
            comm.Open();

            if (DataType == PLCDataType.X|| DataType == PLCDataType.Y|| DataType == PLCDataType.M|| DataType == PLCDataType.L|| DataType == PLCDataType.F|| DataType == PLCDataType.V|| DataType == PLCDataType.B)
            {
                comm.Wait<bool>(MyOwner.ID, bVal, OpRule, $"{DataType} {Address} 1 ", waitOverTime);
            }
            else if (DataType == PLCDataType.W|| DataType == PLCDataType.D)
            {            
                comm.Wait<short>(MyOwner.ID, iVal, OpRule, $"{DataType} {Address} 1 ", waitOverTime);
            }
            return base.DoExcute(out errMsg);   
        }

        public override void Stop()
        {
            if (comm == null) return;
            comm?.Stop();
            comm?.Close();
            comm = null;
        }
    }
}
