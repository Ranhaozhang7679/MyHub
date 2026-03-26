using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.VDevice;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Protocol.Functions
{
    public class WriteMC: MotionFunction,IPauseFunction
    {
        [NotEmpty]
        [Parameter("通信设备", 0, CN = "通信设备", EditorType = typeof(VCommuncation))]
        public VDevice CommDevice { get; set; }

        [Parameter("设备地址", 1, CN = "设备地址", DefaultV = 1)]
        public string Address { get; set; }

        [Parameter("数据类型", 2, CN = "数据类型", DefaultV = PLCDataType.M)]
        public PLCDataType DataType { get; set; }

        [DependOn("DataType", PLCDataType.Y, PLCDataType.M, PLCDataType.L, PLCDataType.F, PLCDataType.V, PLCDataType.B)]
        [Parameter("写入布尔值", 3, CN = "布尔值",DefaultV =true, ParamType = ParamType.IN, CanRef = ParamRef.Ref)]
        public bool bVal { get; set; }

        [DependOn("DataType", PLCDataType.D, PLCDataType.W)]
        [Parameter("写入值",4 , CN = "值", ParamType = ParamType.IN, CanRef = ParamRef.Ref)]
        public short iVal { get; set; }


        public WriteMC()
        {
            this.Tips = "写MC";
            this.Icon = "\xe6d2";
        }

        public override bool DoExcute(out string errMsg)
        {
            GetVDevice<VCommuncation>(CommDevice, out var communcation);
            communcation.Open();
            if (DataType == PLCDataType.Y || DataType == PLCDataType.M || DataType == PLCDataType.L || DataType == PLCDataType.F || DataType == PLCDataType.V || DataType == PLCDataType.B)
            {
                communcation.Write<bool>(bVal, $"{DataType} {Address} 1 ");
            }
            else
            {
                communcation.Write<short>(iVal, $"{DataType} {Address} 1 ");
            }

            return base.DoExcute(out errMsg);
        }
    }
}
