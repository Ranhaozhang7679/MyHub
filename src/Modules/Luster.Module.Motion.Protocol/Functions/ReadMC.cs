using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.VDevice;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Protocol.Functions
{
      public class ReadMC:MotionFunction,IPauseFunction
    {
        [NotEmpty]
        [Parameter("通信设备", 0, CN = "通信设备", EditorType = typeof(VCommuncation))]
        public VDevice CommDevice { get; set; }

        [Parameter("设备地址", 1, CN = "设备地址", DefaultV = 1)]
        public string Address { get; set; }

        [Parameter("数据类型", 2, CN = "数据类型", DefaultV=PLCDataType.M)]
        public PLCDataType DataType { get; set; }


        [DependOn("DataType", PLCDataType.X,PLCDataType.Y, PLCDataType.M, PLCDataType.L, PLCDataType.F, PLCDataType.V, PLCDataType.B)]
        [Parameter("读取布尔值", 10, CN = "布尔值", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool bVal { get; set; }


        [DependOn("DataType", PLCDataType.D, PLCDataType.W)]
        [Parameter("读取值", 10, CN = "值", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public int iVal { get; set; }

        public ReadMC()
        {
            this.Tips = "读取MC";
            this.Icon = "\xe6d5";
        }

        public override bool DoExcute(out string errMsg)
        {
            GetVDevice<VCommuncation>(CommDevice, out var communcation);
            communcation.Open();
            //bool类型
            if(DataType == PLCDataType.X|| DataType == PLCDataType.Y || DataType == PLCDataType.M || DataType == PLCDataType.L || DataType == PLCDataType.F || DataType == PLCDataType.V || DataType == PLCDataType.B)
            {
                var bVals = communcation.Read<bool>($"{DataType} {Address} 1 ");
                if(bVals.Count>0)
                {
                    bVal = bVals[0];
                }
                else
                {
                    bVal = false;
                }
            }
            //数值类型
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
