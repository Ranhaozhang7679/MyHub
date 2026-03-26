using Luster.Motion.DataStruct.Enums;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Logic;
using Luster.TaskFlow.Motion.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.DataProc.Functions
{
    /// <summary>
    /// 数据转移
    /// </summary>
    public class DataTransfer : MotionFunction
    {
        [NotEmpty]
        [Parameter("转移类型,分为拉取和转移", 0, CN = "转移类型")]
        public DataTransType TransType { get; set; }

        /// <summary>
        /// 原始工站
        /// </summary>
        [NotEmpty]
        [Parameter("数据拉取到的目标工站", 1, CN = "原始工站")]
        public LStation SrcStation { get; set; }

        /// <summary>
        /// 目标工站
        /// </summary>
        [NotEmpty]
        [Parameter("数据移动到的目标工站", 2, CN = "目标工站")]
        public LStation DstStation { get; set; }

        /// <summary>
        /// 数据转移
        /// </summary>
        public DataTransfer()
        {
            this.Tips = "将当前工站要进行上传的数据进行转移";
            this.Icon = "\xe6aa";
        }

        /// <summary>
        /// 将数据转移到任意工站中
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public override bool DoExcute(out string errMsg)
        {
            if (DstStation.Tag == null)
            {
                DstStation.Tag = MyOwner.TaskModules[DstStation.ID] as IMotionModule;
            }

            if (SrcStation.Tag == null)
            {
                SrcStation.Tag = MyOwner.TaskModules[SrcStation.ID] as IMotionModule;
            }

            DataProcess(SrcStation.Tag as IMotionModule, DstStation.Tag as IMotionModule, TransType);

            return base.DoExcute(out errMsg);
        }

        public override void Added()
        {
            if (MyOwner.Station == null)
            {
                MyOwner.UpdateStation();
            }

            if (MyOwner.Station != null)
            {
                var station = MyOwner.Parameters[nameof(SrcStation)].Value;
                if(station == null)
                {
                    station = new LStation() { ID = MyOwner.Station.ID, Tag = MyOwner.Station, Alias = MyOwner.Station.Alias };
                }
            }
        }
    }
}
