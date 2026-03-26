using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luster.Motion.DataStruct.Enums;

namespace Luster.Module.Motion.Logic.Functions
{
    public class NG : MotionFunction
    {
        /// <summary>
        /// NG内容
        /// </summary>
        [Parameter("NG内容", 0, CN = "NG内容")]
        public string Message { get; set; }

        /// <summary>
        /// 触发报警上限
        /// </summary>
        [Parameter("次数", 1, CN = "次数", DefaultV = "3")]
        public int NGCount { get; set; }


        /// <summary>
        /// 复位
        /// </summary>
        [Parameter("复位", 2, CN = "复位",CanRef = ParamRef.Ref, DefaultV = false)]
        public bool Reset { get; set; }

        /// <summary>
        /// 报警内容，次数
        /// 用于统计对应报警的次数
        /// </summary>
        public static Dictionary<string, int> NGDic = new Dictionary<string, int>();

        public NG()
        {
            this.Tips = "NG多次报警";
            this.Icon = "\xe6a3";
        }

        public override bool DoExcute(out string errMsg)
        {
            if (!NGDic.ContainsKey(Message))
            {
                NGDic[Message] = 1;
            }
            else
            {
                NGDic[Message]++;
                if (NGDic[Message] >= NGCount)
                {
                    OnAlarm(AlarmType.InfoTip, Message);
                    NGDic[Message] = 0;
                }
            }
            if (Reset)
            {
                NGDic[Message] = 0;
            }
            return base.DoExcute(out errMsg);
        }
    }
}
