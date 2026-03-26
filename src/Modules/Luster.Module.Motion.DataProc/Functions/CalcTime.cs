using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Interfaces;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.DataProc.Functions
{

    public enum TimeMethod
    {
        [Description("单模块计时")]
        Single,

        [Description("双模块计时")]
        Multi
    }

    public enum TimeUnit
    {
        [Description("毫秒")]
        MS,

        [Description("秒")]
        S,

        [Description("分钟")]
        Min
    }

    public class CalcTime : MotionFunction
    {
        [Required]
        [Parameter("开始模块", 1, CN = "开始模块", DefaultV = TimeMethod.Multi)]
        public TimeMethod Method { get; set; }

        [Required]
        [Parameter("开始模块", 2, CN = "开始模块")]
        public LStatus StartModule { get; set; }

        [Parameter("结束模块", 3, CN = "结束模块")]
        public LStatus EndModule { get; set; }

        [Parameter("时间单位", 4, CN = "时间单位")]
        public TimeUnit TimeUnit { get; set; }

        [Parameter("模块耗时", 10, CN = "模块耗时", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public double Time { get; set; }

        public CalcTime()
        {
            this.Tips = "模块耗时计时器";
            this.Icon = "\xe69c";
        }

        public override bool DoExcute(out string errMsg)
        {
            errMsg = "";
            IMotionModule startModule = null;
            IMotionModule endModule = null;
            double ms = 0;

            var firstP = MyOwner.Parameters[nameof(StartModule)];
            if (firstP != null && firstP.RefOut != null)
            {
                startModule = firstP.RefOut.Owner as IMotionModule;
            }

            var secP = MyOwner.Parameters[nameof(EndModule)];
            if (secP != null && secP.RefOut != null)
            {
                endModule = secP.RefOut.Owner as IMotionModule;
            }

            switch (Method)
            {
                case TimeMethod.Single:
                    ms = startModule.TimeConsuming;
                    break;
                case TimeMethod.Multi:

                    if (endModule == null)
                    {
                        errMsg = "结束模块没有选择";
                        return false;
                    }

                    ms = (endModule.EndTime - startModule.StartTime).TotalMilliseconds;
                    break;
            }

            Time = Math.Round(GetTime(ms - MyOwner.PauseTime), 2);

            return base.DoExcute(out errMsg);
        }


        public double GetTime(double ms)
        {
            switch (TimeUnit)
            {
                case TimeUnit.MS:
                    return ms;
                case TimeUnit.S:

                    return ms / 1000;
                case TimeUnit.Min:

                    return ms / 60000;

                default:
                    return ms;
            }
        }
    }
}
