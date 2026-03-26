using Luster.Common.DataStruct.Attributes;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Stations.Functions
{
    public class NGStation : StationFunction, INGStation
    {
        [Parameter("机台状态", 0, CN = "机台状态", DefaultV = EngineStatus.Stop, MultiValues=true)]
        public EngineStatus MachineStatus { get; set; }

        /// <summary>
        /// 报警类型
        /// </summary>
        [DependOn("MachineStatus", EngineStatus.Alarm)]
        [Parameter("报警类型", 1, CN = "报警类型")]
        public AlarmType AlarmType { get; set; }


        public NGStation()
        {
            this.Tips = "机台异常后,做的特殊处理";
            this.Icon = "\xe6a3";
        }

        /// <summary>
        /// 流程处理
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public override bool DoExcute(out string errMsg)
        {
            bool isSuccess = false;
            errMsg = string.Empty;

            // 2.运行子模块
            if (MyOwner.Children.Count > 0)
            {
                // 2.1 获取第一个子模块
                var startModule = MyOwner.Children[0];

                // 2.2 程序运行（递归运行）
                //startModule.BrokenOff = breakoff;
                motionRunEngine.Run(startModule, ref isSuccess);

                // 2.3 运行失败，获取错误消息
                if (!isSuccess)
                {
                    errMsg = motionRunEngine.ErrorMessage;
                    return false;
                }
            }

            return string.IsNullOrEmpty(errMsg);
        }

        /// <summary>
        /// 更新当前报警
        /// </summary>
        /// <param name="alarmInfo"></param>
        public void SetAlarm(params AlarmInfo[] alarmInfos)
        {

        }
    }
}
