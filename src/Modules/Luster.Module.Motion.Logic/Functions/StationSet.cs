using Luster.Common.DataStruct.Extensions;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Interfaces;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Enums;
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
    public class StationSet : MotionFunction, INote, IStationSet
    {
        [Parameter("产品的最终结果", 0, CN = "本站状态", DefaultV = StationType.ThisHave)]
        public StationType StationType { get; set; }


        [Parameter("产品的最终结果", 1, CN = "信号关闭", DefaultV = false)]
        public bool IsClose { get; set; }

        /// <summary>
        /// 是否有料
        /// </summary>
        public override string[] NoteParams => new string[] { nameof(StationType), nameof(IsClose) };

        /// <summary>
        /// 
        /// </summary>
        public StationSet()
        {
            this.Tips = "本站有料/或者要料";
            this.Icon = "\xe79e";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public override bool DoExcute(out string errMsg)
        {
            if (MyOwner.Station == null)
            {
                MyOwner.UpdateStation();
            }

            var stationType = StationType.ToString();

            // 更新有料
            if (MyOwner.Station.Parameters.ContainsKey(stationType))
            {
                var parameter = MyOwner.Station.Parameters[stationType];
                bool isResult = IsClose ? false : true;

                // 属性更新
                if (parameter.Property != null)
                {
                    parameter.Property.SetValue(MyOwner.Station.TaskFunction, isResult);
                }

                parameter.Value = isResult;
            }

            return base.DoExcute(out errMsg);
        }

        /// <summary>
        /// 重写注释
        /// </summary>
        /// <param name="note"></param>
        /// <returns></returns>
        public override string GetNote(INote note)
        {
            StringBuilder sb = new StringBuilder();
            var p = MyOwner.Parameters[nameof(StationType)];
            sb.Append($"{p.Value.GetDescription()}");

            var cP = MyOwner.Parameters[nameof(IsClose)];
            if (bool.TryParse(cP.Value?.ToString(), out var isClose) && isClose)
            {
                sb.Append($"_关闭");
            }

            return sb.ToString();
        }
    }
}
