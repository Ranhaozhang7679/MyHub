using Luster.Motion.TaskFlow.Engine;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Business.Functions
{
    public class GetMachineStatus : MotionFunction
    {

        [Parameter("当前机台状态", 1, CN = "机台状态", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string StatusMsg { get; set; }

        /// <summary>
        /// 获取机台状态
        /// </summary>
        public GetMachineStatus()
        {
            this.Tips = "获取机台状态";
            this.Icon = "\xe664";
        }

        public override bool DoExcute(out string errMsg)
        {
            errMsg = "";
            var mController = MyOwner.Ioc.Resolve<IMotionController>();
            if (mController != null)
            {
                StatusMsg = mController.MachineStatus.ToString();
            }

            return base.DoExcute(out errMsg);
        }

    }
}
