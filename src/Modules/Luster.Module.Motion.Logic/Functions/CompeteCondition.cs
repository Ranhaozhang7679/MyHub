using Luster.Common.DataStruct.DataModels;
using Luster.Motion.DataStruct.DataModels;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Interfaces;
using Luster.TaskFlow.Common.Models;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Logic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Logic.Functions
{
    public class CompeteCondition: LogicFunction
    {

        [Range(1, 100)]
        [Parameter("有多少个条件", 1, CN = "条件数量", DefaultV = 1)]
        public int ConditionNum { get; set; }

        /// <summary>
        /// 条件表达式
        /// </summary>
        [Parameter("条件表达式", 1, CN = "条件")]
        public LExpression Condition { get; set; }

        /// <summary>
        /// 条件表达式
        /// </summary>
        [Parameter("满足条件后且满足时间才会输出，单位为s,-1代表不需要满足时间", 1, CN = "竞争时间", DefaultV = -1)]
        public int CompeteTime { get; set; }


        [Parameter("满足第几个条件，输出为int", 10, CN = "满足条件号", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public int Result { get; set; }



        public CompeteCondition()
        {
            this.Tips = "竞争条件，有时间的条件进入";
            this.Icon = "\xe6df";
        }

        public override bool DoExcute(out string errMsg)
        {
            int count = 0;
            var ioDevices = GetParametersByType<LExpression>();
            var times = GetParametersByType<int>();
            Dictionary<LExpression,int> dicExpressTime= new Dictionary<LExpression,int>();
            List<LExpression> conditions = new List<LExpression>();
            List<int> ts= new List<int>();
            foreach (var item in ioDevices)
            {
                conditions.Add(item);
            }
            foreach (var time in times)
            {
                ts.Add(time);
            }

            while (true)
            {
                foreach (var item in conditions)
                {
                    if(item.GetResult(MyOwner))
                    {
                        count++;
                        int index = conditions.FindIndex(x => x == item) + 1;
                        if (count*5> ts[index] *1000)
                        {
                            Result = index;
                            return base.DoExcute(out errMsg);
                        }
                    }
                }

                Thread.Sleep(3);
            }
        }



        public override void OnNotifyPropertyUIChanged(ParameterAttribute parameter, object newV)
        {
            base.OnNotifyPropertyUIChanged(parameter, newV);
            if (parameter.Name == "ConditionNum" && int.TryParse(newV?.ToString(), out var num) && num >= 1)
            {

                BuildExternParameters(1, num, "CompeteTime", "竞争时间", typeof(int), (p) =>
                {
                    p.EditorType = typeof(int);
                    p.DefaultV = -1;
                });

                BuildExternParameters(1, num, "Condition", "条件", typeof(LExpression), (p) =>
                {
                    p.EditorType = typeof(LExpression);
                });
            }
        }
    }
}
