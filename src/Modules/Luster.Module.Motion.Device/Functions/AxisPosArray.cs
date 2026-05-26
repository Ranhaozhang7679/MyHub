using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Models;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Device.Functions
{
    public class AxisPosArray : MotionFunction, IPauseFunction, IStopFunction, INote
    {
        /// <summary>
        /// 不需要显示行轴名称
        /// </summary>
        
        [Parameter("行轴", 1, CN = "行轴")]
        public VAxisPos RowAxis { get; set; }

        /// <summary>
        /// 不需要显示列轴名称
        /// </summary>
        
        [Parameter("列轴", 2, CN = "列轴")]
        public VAxisPos ColAxis { get; set; }

        [Range(0, 100)]
        [Parameter("行数", 3, CN = "行数", DefaultV = 1)]
        public int Row { get; set; }

        [Range(0, 100)]
        [Parameter("列数", 4, CN = "列数", DefaultV = 1)]
        public int Column { get; set; }

        [Parameter("行增量，单位mm", 5, CN = "行增量")]
        public double RowAxisIncrement { get; set; }

       
        [Parameter("列增量，单位mm", 6, CN = "列增量")]
        public double ColAxisIncrement { get; set; }

        [Parameter("点位号", 7, CN = "点位号")]
        public LStringEx PointNumber { get; set; }

        [Parameter("极限值，算出来值大于极限值，直接报警", 8, CN = "极限值", DefaultV = 10000)]
        public int LimitValue { get; set; }

        [Parameter("左右上下/上下左右", 9, CN = "左右上下", DefaultV = true)]
        public bool IsLeftRightOrder { get; set; }

        [Parameter("是否超限", 20, CN = "是否超限", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool IsExceed { get; set; }

        [Parameter("行偏移", 21, CN = "行偏移", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public double RowOutPos { get; set; }

        [Parameter("列偏移", 22, CN = "列偏移", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public double ColOutPos { get; set; }

        public override string[] NoteParams => new string[] { nameof(RowAxis), nameof(ColAxis) };

        public AxisPosArray()
        {
            this.Tips = "双轴点位阵列";
            this.Icon = "\xe678";
            this.DynParam = true;
        }

        public override bool DoExcute(out string errMsg)
        {
            int inPoint = int.Parse(PointNumber.GetString(MyOwner));
            int actRow, actCol;

            if (IsLeftRightOrder)
            {
                // 左右上下顺序：先从左到右，再换行
                actRow = inPoint / Column + 1;
                actCol = inPoint % Column;
                if (inPoint % Column == 0)
                {
                    actCol = Column;
                    actRow--;
                }
            }
            else
            {
                // 上下左右顺序：先从上到下，再换列
                actRow = inPoint % Row;
                actCol = inPoint / Row + 1;
                if (inPoint % Row == 0)
                {
                    actRow = Row;
                    actCol--;
                }
            }

            RowOutPos = (actRow - 1) * RowAxisIncrement;
            ColOutPos = (actCol - 1) * ColAxisIncrement;

            // 是否超限
            IsExceed = RowOutPos >= LimitValue || ColOutPos >= LimitValue;
            return base.DoExcute(out errMsg);
        }

        /// <summary>
        /// 暂停相当于停止
        /// </summary>
        public override void Pause()
        {
            Stop();
        }

        public override void OnNotifyPropertyUIChanged(ParameterAttribute parameter, object newV)
        {
            base.OnNotifyPropertyUIChanged(parameter, newV);
        }
    }
}
