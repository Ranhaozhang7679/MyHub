using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Device.Functions
{
    public class GenAxisPos:MotionFunction
    {
        [NotEmpty]
        [Parameter("轴", 1, CN = "轴")]
        public VAxisDevice SelectedAxis { get; set; }

        [Parameter("位置偏移", 2, CN = "位置偏移")]
        public double Offset { get; set; }


        [Parameter("计算位置", 10, CN = "计算位置", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public double CalPos { get; set; }

        public GenAxisPos()
        {
            this.Tips = "获取轴位置";
            this.Icon = "\xe6bd";
        }

        public override bool DoExcute(out string errMsg)
        {
            double currentPos=0;
            GetVDevice<VAxis>(SelectedAxis, out var axis);
            if (axis == null)
            {
                errMsg = $"设备:{SelectedAxis.Name}未找到";
                return false;
            }
            currentPos= axis.GetCurrentPos();
            CalPos = currentPos + Offset;
            return base.DoExcute(out errMsg);
        }
    }
}
