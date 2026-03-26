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
    public class ReadFins:MotionFunction,IPauseFunction
    {
        [NotEmpty]
        [Parameter("通信设备", 0, CN = "通信设备", EditorType = typeof(VCommuncation))]
        public VDevice CommDevice { get; set; }

        [Parameter("寄存器地址", 1, CN = "寄存器地址", DefaultV = 1)]
        public string Address { get; set; }

        [Parameter("数据类型", 2, CN = "数据类型", DefaultV = OmronPLCDataType.DW)]
        public OmronPLCDataType DataType { get; set; }

        [DependOn("DataType", OmronPLCDataType.CB, OmronPLCDataType.WB, OmronPLCDataType.HB, OmronPLCDataType.AB, OmronPLCDataType.DB)]
        [Parameter("读取布尔值", 10, CN = "布尔值", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool bVal { get; set; }


        [DependOn("DataType", OmronPLCDataType.CW, OmronPLCDataType.WW, OmronPLCDataType.HW, OmronPLCDataType.AW, OmronPLCDataType.DW)]
        [Parameter("读取值", 10, CN = "值", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public int iVal { get; set; }

        public ReadFins()
        {
            this.Tips = "读取Fins";
            this.Icon = "\xe6d4";
        }

        public override bool DoExcute(out string errMsg)
        {

            GetVDevice<VCommuncation>(CommDevice, out var communcation);
            communcation.Open();

            if(DataType == OmronPLCDataType.CB || DataType == OmronPLCDataType.WB || DataType == OmronPLCDataType.HB || DataType == OmronPLCDataType.AB || DataType == OmronPLCDataType.DB)
            {
                var bVals = communcation.Read<bool>($"{DataType} {Address} 1 ");
                if (bVals.Count > 0)
                {
                    bVal = bVals[0];
                }
                else
                {
                    bVal = false;
                }
            }
            else
            {
                var iVals = communcation.Read<short>($"{DataType} {Address} 1 ");
                if (iVals.Count > 0)
                {
                    iVal = iVals[0];
                }
                else
                {
                    iVal = 0;
                }
            }

            return base.DoExcute(out errMsg);   

        }
    }
}
