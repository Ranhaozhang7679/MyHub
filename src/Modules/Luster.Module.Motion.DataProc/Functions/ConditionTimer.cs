using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Interfaces;
using Luster.TaskFlow.Common.Models;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Luster.Motion.DataStruct.Enums;

namespace Luster.Module.Motion.DataProc.Functions
{
    public class ConditionTimer : MotionFunction
    {
        [Required]
        [Limit(1, 1000000)]
        [Parameter("计时时长，单位s", 0, CN = "计时时长", DefaultV = 1, CanRef = ParamRef.Ref)]
        public double Duration { get; set; }

        [Parameter("用于中断计时的条件", 1, CN = "中断条件")]
        public LExpression BreakExp { get; set; }

        [Parameter("对哪个灯进行开启", 8, CN = "开灯类型", DefaultV = LightType.None)]
        public LightType LightType { get; set; }

        [DependOn("LightType", true)]
        //[Parameter("中断灯闪烁的Io数量", 10, CN = "灯闪烁间隔", DefaultV = 200)]
        //public int IntervalLight { get; set; }

        [Parameter("是否开启开启蜂鸣器,如果为否，则关闭蜂鸣器", 9, CN = "开启蜂鸣器", DefaultV = false)]
        public bool IsBuzzer { get; set; }

        [DependOn("IsBuzzer", true)]
        [Parameter("中断灯闪烁的Io数量", 10, CN = "蜂鸣器间隔", DefaultV = 200)]
        public int Interval { get; set; }

        [Parameter("计时结果", 2, CN = "计时结果", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public double ElapsedTime { get; set; }
        [Parameter("是否中断", 2, CN = "是否提前中断", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool IsBerak { get; set; }

        public ConditionTimer()
        {
            this.Tips = "条件定时器";
            this.Icon = "\xe69c";
        }

        public override bool DoExcute(out string errMsg)
        {
            errMsg = "";
            ElapsedTime = 0;
            IsBerak = false;

            double elapsed = 0;
            int stepMs = 100;
            int loopNum = 0;
            while (elapsed < Duration*1000)
            {
                // 每100ms检查一次中断条件
                loopNum++;
                if (BreakExp != null && BreakExp.ToString() != string.Empty)
                {
                    if (BreakExp.GetResult(MyOwner))
                    {
                        MyOwner.OnLog(LogType.Debug, $"ConditionTimer 执行到第 {loopNum} 次满足条件");
                        IsBerak = true;
                        break;
                    }
                }
                Thread.Sleep(stepMs);
                elapsed += stepMs;
            }

            if (elapsed/1000 >= Duration)
            {
                ElapsedTime = Duration;
                // 计时时间到，可以报警，开启灯和蜂鸣器
                MyOwner.LightManager.OpenLight(LightType, IsBuzzer, Interval);
            }

            else
                ElapsedTime = elapsed / 1000;                
            return base.DoExcute(out errMsg);
        }

    }
}
