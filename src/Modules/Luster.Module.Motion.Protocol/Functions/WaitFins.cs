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
    public class WaitFins:MotionFunction,IPauseFunction
    {
        [NotEmpty]
        [Parameter("通信设备", 0, CN = "通信设备", EditorType = typeof(VCommuncation))]
        public VDevice CommDevice { get; set; }

        [Parameter("寄存器地址", 1, CN = "寄存器地址", DefaultV = 1)]
        public string Address { get; set; }

        [Parameter("数据类型", 2, CN = "数据类型", DefaultV = OmronPLCDataType.DW)]
        public OmronPLCDataType DataType { get; set; }

        [Parameter("阈值规则", 3, CN = "阈值规则", DefaultV = OpRule.Equal)]
        public OpRule OpRule { get; set; }

        [DependOn("DataType", OmronPLCDataType.CB, OmronPLCDataType.WB, OmronPLCDataType.HB, OmronPLCDataType.AB, OmronPLCDataType.DB)]
        [Parameter("布尔值", 10, CN = "布尔值",ParamType =ParamType.IN,CanRef =ParamRef.Ref)]
        public bool bVal { get; set; }

        [DependOn("DataType", OmronPLCDataType.CW, OmronPLCDataType.WW, OmronPLCDataType.HW, OmronPLCDataType.AW, OmronPLCDataType.DW)]
        [Parameter("值", 10, CN = "值", ParamType = ParamType.IN, CanRef = ParamRef.Ref)]
        public short iVal { get; set; }


        [Limit(-1, 1000000)]
        [Parameter("超时时间，单位为s", 10, CN = "超时时间", DefaultV = -1)]
        public int OverTime { get; set; }

        public WaitFins()
        {
            this.Tips = "等待Fins";
            this.Icon = "\xe6d1";
        }
        private VCommuncation comm = null;


        public override bool DoExcute(out string errMsg)
        {
            int waitOverTime = OverTime * 1000;
            GetVDevice<VCommuncation>(CommDevice, out var cDevice);
            if (comm == null)
            {
                comm = cDevice.Clone() as VCommuncation;
            }
            comm.Open();

            if (DataType == OmronPLCDataType.CB || DataType == OmronPLCDataType.WB || DataType == OmronPLCDataType.HB || DataType == OmronPLCDataType.AB || DataType == OmronPLCDataType.DB)
            {
                comm.Wait<bool>(MyOwner.ID, bVal, OpRule, $"{DataType} {Address} 1 ", waitOverTime);
            }
            else
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
