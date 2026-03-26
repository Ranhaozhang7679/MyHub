using Luster.Common.DataStruct.Attributes;
using Luster.Motion.DataStruct.DataModels;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Device.Functions
{

    public enum DataSizeSDO
    {
        [Description("1")]
        One = 1,

        [Description("2")]
        Two = 2,

        [Description("4")]
        Four = 4
    }

    /// <summary>
    /// SDO行为
    /// </summary>
    public enum SDOBehavior
    {
        /// <summary>
        /// 写入
        /// </summary>
        [Description("写入")]
        Write,

        /// <summary>
        /// 读取
        /// </summary>
        [Description("读取")]
        Read,
    }

    public class SDOAction : MotionFunction
    {
        [NotEmpty]
        [Parameter("轴名称选择", 0, CN = "轴名称", EditorType = typeof(VAxis))]
        public VDevice Axis { get; set; }

        [Parameter("读写选择", 1, CN = "读写")]
        public SDOBehavior SDOBehavior { get; set; }

        [Parameter("索引（16进制）", 3, CN = "索引")]
        public string Index { get; set; }

        [Parameter("子索引", 4, CN = "子索引")]
        public short SubIndex { get; set; }

        [DependOn("SDOBehavior", SDOBehavior.Write)]
        [Parameter("写入值", 5, CN = "写入值")]
        public int Data { get; set; }

        [Parameter("数据大小", 6, CN = "数据大小")]
        public DataSizeSDO DataSize { get; set; } = DataSizeSDO.One;

        [DependOn("SDOBehavior", SDOBehavior.Write)]
        [Parameter("比例值", 7, CN = "比例值", DefaultV = 1.0)]
        public double Scale { get; set; }

        [DependOn("SDOBehavior", SDOBehavior.Read)]
        [Parameter("数值", 12, CN = "数值", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public int Value { get; set; }

        public SDOAction()
        {
            this.Tips = "SDO读写";
            this.Icon = "\xe6ce";
        }

        public override bool DoExcute(out string errMsg)
        {
            GetVDevice<VAxis>(Axis, out var axis);
           
            short index = Convert.ToInt16(Index, 16);

            if (SDOBehavior == SDOBehavior.Write)
            {
                switch (DataSize)
                {
                    case DataSizeSDO.One:
                        axis.SDOWrite(index, SubIndex, (int)((byte)Data * Scale), 1);
                        break;

                    case DataSizeSDO.Two:
                        axis.SDOWrite(index, SubIndex, (int)((Int16)Data * Scale), 2);
                        break;

                    default:
                        axis.SDOWrite(index, SubIndex, (int)(Data * Scale), 4);
                        break;
                }

            }
            else
            {
                int value = 0;
                axis.SDORead(index, SubIndex, (short)DataSize, out value, 1);

                switch (DataSize)
                {
                    case DataSizeSDO.One:

                        Value = (int)((sbyte)value);
                        break;

                    case DataSizeSDO.Two:
                        Value = (int)((Int16)value);
                        break;

                    default:
                        Value = value;
                        break;
                }

            }

            return base.DoExcute(out errMsg);
        }


    }
}
