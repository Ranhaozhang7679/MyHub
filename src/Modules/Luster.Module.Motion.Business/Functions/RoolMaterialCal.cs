using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.VDevice;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Business.Functions
{

    /// <summary>
    /// 卷料工站
    /// </summary>
    public enum RollStation
    {
        //[Description("左")]
        左,

        //[Description("右")]
        右,
    }


    public class RoolMaterialCal:MotionFunction, IPauseFunction
    {
        /// <summary>
        /// 卷料名称
        /// </summary>
        [NotEmpty]
        [Parameter("卷料名称", 0, CN = "卷料名称",DefaultV = RollStation.左)]
        public RollStation RoolName { get; set; }

        /// <summary>
        /// 预警百分比
        /// </summary>
        [NotEmpty]
        [Parameter("预警百分比%", 1, CN = "预警百分比", DefaultV = 20)]
        public int WarnPercent { get; set; }

        /// <summary>
        /// 达到限制
        /// </summary>
        [Parameter("达到下限后预警", 11, CN = "预警", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool BelowLimit { get; set; }
        /// <summary>
        /// 输出当前剩余百分比
        /// </summary>
        [Parameter("剩余百分比%", 12, CN = "剩余百分比", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public int Surplus { get; set; }


        public RoolMaterialCal()
        {
            this.Tips = "卷料计算";
            this.Icon = "\xe691";
        }

        public override bool DoExcute(out string errMsg)
        {
            BelowLimit = false;
            Surplus=MyOwner.RollManager.UpdateUseNum(RoolName.ToString());
            //if (Surplus < WarnPercent)
            //{
            //    BelowLimit = true;
            //    MyOwner.OnAlarm(Luster.Motion.DataStruct.Enums.AlarmType.InfoTip, $"卷料剩余{Surplus}%");
            //}
            //if(Surplus==999)
            //{
            //    MyOwner.OnAlarm(Luster.Motion.DataStruct.Enums.AlarmType.InfoTip, $"未找到对应卷料{this.RoolName}");
            //}
            return base.DoExcute(out errMsg);
        }
    }
}
