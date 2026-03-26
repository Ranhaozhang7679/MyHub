using Luster.Common.DataStruct.Attributes;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Device.Functions
{
    public enum DataSizePDO
    {
        [Description("1")]
        One = 1,

        [Description("2")]
        Two = 2,

        [Description("4")]
        Four = 4

    }

    /// <summary>
    /// PDO行为
    /// </summary>
    public enum PDOBehavior
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

    public class PDOAction : MotionFunction
    {
        [NotEmpty]
        [Parameter("轴名称选择", 0, CN = "轴名称", EditorType = typeof(VAxis))]
        public VDevice Axis { get; set; }

        [Parameter("读写选择", 1, CN = "读写")]
        public PDOBehavior PDOBehavior { get; set; }

        [Parameter("索引", 3, CN = "索引")]
        public string Index { get; set; }

        [Parameter("子索引", 4, CN = "子索引")]
        public short SubIndex { get; set; }

        [DependOn("PDOBehavior", PDOBehavior.Write)]
        [Parameter("写入值", 5, CN = "写入值")]
        public int Data { get; set; }

        [Parameter("数据大小", 6, CN = "数据大小")]
        public DataSizePDO DataSize { get; set; } = DataSizePDO.One;


        [DependOn("PDOBehavior", PDOBehavior.Write)]
        [Parameter("比例值", 7, CN = "比例值", DefaultV = 1.0)]
        public double Scale { get; set; }


        [DependOn("PDOBehavior", PDOBehavior.Read)]
        [Parameter("数值", 12, CN = "数值", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public int Value { get; set; }


        public PDOAction()
        {
            this.Tips = "PDO读写";
            this.Icon = "\xe6d0"; //&#xe6d0;
        }

        public override bool DoExcute(out string errMsg)
        {
            GetVDevice<VAxis>(Axis, out var axis);

            short index = Convert.ToInt16(Index, 16);

            short axisNo = (short)axis.AxisNo;

            if (PDOBehavior == PDOBehavior.Write)
            {
                switch (DataSize)
                {
                    case DataSizePDO.One:
                        axis.PDOWrite(axisNo, index, SubIndex, (int)((byte)Data * Scale), 1);
                        break;

                    case DataSizePDO.Two:
                        axis.PDOWrite(axisNo, index, SubIndex, (int)((Int16)Data * Scale), 2);
                        break;

                    default:
                        axis.PDOWrite(axisNo, index, SubIndex, (int)(Data * Scale), 4);
                        break;
                }

            }
            else
            {
                int value = 0;
                axis.PDORead(axisNo, index, SubIndex, (short)DataSize, ref value, 1);

                switch (DataSize)
                {
                    case DataSizePDO.One:

                        Value = (int)((sbyte)value);
                        break;

                    case DataSizePDO.Two:
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
