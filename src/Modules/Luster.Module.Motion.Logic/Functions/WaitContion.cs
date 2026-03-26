using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Interfaces;
using Luster.TaskFlow.Common.Models;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Logic.Functions
{
    /// <summary>
    /// 本站有料
    /// </summary>
    public class WaitCondition : OverTimeFunction, IWait, INote, IRefFunction
    {
        [Parameter("等待条件条件表达式", 1, CN = "等待条件")]
        public LExpression Express { get; set; }


        [Limit(-1, 1000000)]
        [Parameter("超时时间，单位为s,如果为-1,代表一直持续等待", 4, CN = "超时时间", DefaultV = -1)]
        public override int OverTime { get; set; }

        [Parameter("数据转移方式,传递", 5, CN = "数据转移", DefaultV = DataTransType.None)]
        public DataTransType TransType { get; set; }

        [Limit(1, 100)]
        [DependOn("TransType", DataTransType.Get)]
        [Parameter("获取数据时上一工站对应的产品数量", 6, CN = "产品数量", DefaultV = 1)]
        public int ProductCount { get; set; }

        [Parameter("超时状态", 22, CN = "是否超时", ParamType = TaskFlow.Common.Enums.ParamType.OUT, DefaultV = false)]
        public bool IsTimeOut { get; set; }

        public override string[] NoteParams => new string[] { "等待" };

        /// <summary>
        /// 构造函数
        /// </summary>
        public WaitCondition()
        {
            this.Tips = "等待条件满足";
            this.Icon = "\xe883";
        }

        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="errMsg">错误消息</param>
        /// <returns></returns>
        public override bool DoExcute(out string errMsg)
        {
            errMsg = "";

            IsTimeOut = false;
            int minuTime = OverTime * 1000;

            WaitFunc(() =>
            {
                var isResult = Express.GetResult(MyOwner);

                return isResult;
            }, $"等待 {Express}", 30, minuTime, () =>
            {
                IsTimeOut = true;
            });

            foreach (var item in Express.Variables)
            {
                if (MyOwner.TaskModules.Contains(item.ID))
                {
                    var module = MyOwner.TaskModules[item.ID] as IMotionModule;
                    DataProcess(MyOwner, module, TransType, ProductCount);
                }

                break;
            }

            return base.DoExcute(out errMsg);
        }

        public virtual List<LReference> GetReferences()
        {
            return GetReferences(nameof(Express));
        }
    }
}
