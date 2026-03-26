using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luster.Common.DataStruct;
using Luster.Motion.DataStruct;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.Motion.DataStruct.Enums;
using Luster.Common.DataStruct.Attributes;

namespace Luster.Module.Motion.Business.Functions
{
    public class ManualGetBarcode : MotionFunction
    {

        [NotEmpty]
        [Parameter("获取二维码失败是否输出报警", 0, CN = "是否报警", DefaultV = false)]
        public bool IsAlaram { get; set; }

        [DependOn("IsAlaram", true)]
        [Parameter("报警类型", 1, CN = "报警类型")]
        public AlarmType AlarmType { get; set; }

        [DependOn("IsAlaram", true)]
        [Parameter("报警内容", 1, CN = "报警内容")]
        public string AlarmContent { get; set; }

        [NotEmpty]
        [Parameter("二维码", 5, CN = "二维码", ParamType = ParamType.OUT)]
        public string BarCode { get; set; }

        public ManualGetBarcode()
        {
            Tips = "手动获取条码";
            Icon = "\xe683";
        }

        public override bool DoExcute(out string errMsg)
        {
            errMsg = string.Empty;
            BarCode = DateTime.Now.ToString("yyyy-MM-dd-HH:mm:ss");
            if (IsEmptyMode)
            {
                return false;
            }

            if (MyOwner.OrderManager.HasOrder(out var orderNum))
            {
                BarCode = orderNum.ToString();
                MyOwner.OrderManager.UpdateOrder(orderNum, 0);
            }
            else
            {
                if (IsAlaram)
                {
                    MyOwner.OnAlarm(AlarmType, AlarmContent, "");
                }
            }

            return base.DoExcute(out errMsg);
        }
    }
}
