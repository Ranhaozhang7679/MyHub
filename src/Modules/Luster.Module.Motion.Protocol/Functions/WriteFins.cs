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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Protocol.Functions
{
    public class WriteFins:MotionFunction,IPauseFunction
    {
        [NotEmpty]
        [Parameter("通信设备", 0, CN = "通信设备", EditorType = typeof(VCommuncation))]
        public VDevice CommDevice { get; set; }

        [Parameter("寄存器地址", 1, CN = "寄存器地址", DefaultV = 1)]
        public string Address { get; set; }

        [Parameter("数据类型", 2, CN = "数据类型", DefaultV = OmronPLCDataType.DW)]
        public OmronPLCDataType DataType { get; set; }

        [Parameter("数量", 3, CN = "数量",DefaultV =1)]
        public short Num { get; set; }

        [DependOn("DataType", OmronPLCDataType.CB, OmronPLCDataType.WB, OmronPLCDataType.HB, OmronPLCDataType.AB, OmronPLCDataType.DB)]
        [Parameter("写入布尔值", 4, CN = "布尔值", DefaultV = true, ParamType = ParamType.IN, CanRef = ParamRef.Ref)]
        public bool bVal { get; set; }

        [DependOn("DataType", OmronPLCDataType.CW, OmronPLCDataType.WW, OmronPLCDataType.HW, OmronPLCDataType.AW, OmronPLCDataType.DW)]
        [Parameter("写入值", 5, CN = "值", ParamType = ParamType.IN, CanRef = ParamRef.Ref)]
        public short iVal { get; set; }

        public WriteFins()
        {
            this.Tips = "写Fins";
            this.Icon = "\xe6d3";
        }


        public override bool DoExcute(out string errMsg)
        {
            GetVDevice<VCommuncation>(CommDevice, out var communcation);
            communcation.Open();
            if (DataType == OmronPLCDataType.CB || DataType == OmronPLCDataType.WB || DataType == OmronPLCDataType.HB || DataType == OmronPLCDataType.AB || DataType == OmronPLCDataType.DB )
            {
                //类型 地址 数量
                communcation.Write<bool>(bVal, $"{DataType} {Address} {Num}");
            }
            else
            {
                communcation.Write<short>(iVal, $"{DataType} {Address} {Num}");
            }




            return base.DoExcute(out errMsg);

        }


    }
}
