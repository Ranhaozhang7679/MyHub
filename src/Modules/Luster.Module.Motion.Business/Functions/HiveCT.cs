using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Real;
using Luster.Motion.DataStruct.VDevice;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Models;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Interfaces;
using Luster.TaskFlow.Motion.Logic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Business.Functions
{
    public class HiveCT : MotionFunction
    {
        /// <summary>
        /// WIP
        /// </summary>
        //[NotEmpty]
        [Parameter("WIP", 1, CN = "WIP", CanRef = ParamRef.Ref)]
        public string WIP { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        [Parameter("开始时间", 2, CN = "开始时间", CanRef = ParamRef.Ref)]
        public string InputTime { get; set; }

        /// <summary>
        /// 结果
        /// </summary>
        [Parameter("结果", 3, CN = "结果", CanRef = ParamRef.Ref, DefaultV = true)]
        public bool Result { get; set; }




        public HiveCT()
        {
            this.Tips = "HiveCT";
            this.Icon = "\xe6a8";
        }
        public override bool DoExcute(out string errMsg)
        {
            string OutputTime = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.ff+0800");
            if (string.IsNullOrEmpty(WIP))
            {
                errMsg = "";
                return true;
            }

            MyOwner.OnStationTime(WIP, InputTime, OutputTime, Result);
            return base.DoExcute(out errMsg);
        }

    }
}
