using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Logic.Functions
{
    /// <summary>
    /// 定时判断
    /// </summary>
    public class TimerJudge : MotionFunction
    {
        [Range(0, 23)]
        [Parameter("计时器小时数", 1, CN = "时钟", DefaultV = 23, CanRef = ParamRef.Ref, IsSensitive = true)]
        public int Time { get; set; }

        [Range(0, 59)]
        [Parameter("计时器分钟数", 1, CN = "分钟", DefaultV = 59, CanRef = ParamRef.Ref, IsSensitive = true)]
        public int Minute { get; set; }

        [Range(0, 59)]
        [Parameter("计时器秒钟数", 1, CN = "秒钟", DefaultV = 59, CanRef = ParamRef.Ref, IsSensitive = true)]
        public int Second { get; set; }

        [Parameter("定时判断结果", 20, CN = "结果", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool Result { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public TimerJudge()
        {
            this.Tips = "定时判断";
            this.Icon = "\xe6e1"; 
        }

        public override bool DoExcute(out string errMsg)
        {

            errMsg = string.Empty;

            Result = false;
            if (DateTime.Now.Hour == Time && DateTime.Now.Minute == Minute && DateTime.Now.Second == Second)
            {
                Result = true;
            }

            return base.DoExcute(out errMsg);
        }

    }
}
