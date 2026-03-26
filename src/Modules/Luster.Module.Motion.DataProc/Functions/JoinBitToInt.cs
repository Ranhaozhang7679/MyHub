using Luster.Common.DataStruct.DataModels;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Interfaces;
using Luster.TaskFlow.Common.Models;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.DataProc.Functions
{
    public class JoinBitToInt:MotionFunction
    {
        [Parameter("Bit0", 0, CN = "位0", DefaultV = false,CanRef =ParamRef.Ref, ParamType = TaskFlow.Common.Enums.ParamType.IN)]
        public bool Bit0 { get; set; }

        [Parameter("Bit1", 1, CN = "位1", DefaultV = false, CanRef = ParamRef.Ref, ParamType = TaskFlow.Common.Enums.ParamType.IN)]
        public bool Bit1 { get; set; }

        [Parameter("Bit2", 2, CN = "位2", DefaultV = false, CanRef = ParamRef.Ref,ParamType =TaskFlow.Common.Enums.ParamType.IN)]
        public bool Bit2 { get; set; }

        [Parameter("Bit3", 3, CN = "位3", DefaultV = false, CanRef = ParamRef.Ref, ParamType = TaskFlow.Common.Enums.ParamType.IN)]
        public bool Bit3 { get; set; }

        [Parameter("Bit4", 4, CN = "位4", DefaultV = false, CanRef = ParamRef.Ref, ParamType = TaskFlow.Common.Enums.ParamType.IN)]
        public bool Bit4 { get; set; }

        [Parameter("Bit5", 5, CN = "位5", DefaultV = false, CanRef = ParamRef.Ref, ParamType = TaskFlow.Common.Enums.ParamType.IN)]
        public bool Bit5 { get; set; }

        [Parameter("Bit6", 6, CN = "位6", DefaultV = false, CanRef = ParamRef.Ref, ParamType = TaskFlow.Common.Enums.ParamType.IN)]
        public bool Bit6 { get; set; }

        [Parameter("Bit7", 7, CN = "位7", DefaultV = false, CanRef = ParamRef.Ref, ParamType = TaskFlow.Common.Enums.ParamType.IN)]
        public bool Bit7 { get; set; }

        [Parameter("Bit8", 8, CN = "位8", DefaultV = false, CanRef = ParamRef.Ref, ParamType = TaskFlow.Common.Enums.ParamType.IN)]
        public bool Bit8 { get; set; }

        [Parameter("Bit9", 9, CN = "位9", DefaultV = false, CanRef = ParamRef.Ref, ParamType = TaskFlow.Common.Enums.ParamType.IN)]
        public bool Bit9 { get; set; }

        [Parameter("Bit10", 10, CN = "位10", DefaultV = false, CanRef = ParamRef.Ref, ParamType = TaskFlow.Common.Enums.ParamType.IN)]
        public bool Bit10 { get; set; }

        [Parameter("Bit11", 11, CN = "位11", DefaultV = false, CanRef = ParamRef.Ref, ParamType = TaskFlow.Common.Enums.ParamType.IN)]
        public bool Bit11 { get; set; }

        [Parameter("Bit12", 12, CN = "位12", DefaultV = false, CanRef = ParamRef.Ref,ParamType =TaskFlow.Common.Enums.ParamType.IN)]
        public bool Bit12 { get; set; }

        [Parameter("Bit13", 13, CN = "位13", DefaultV = false, CanRef = ParamRef.Ref, ParamType = TaskFlow.Common.Enums.ParamType.IN)]
        public bool Bit13 { get; set; }

        [Parameter("Bit14", 14, CN = "位14", DefaultV = false, CanRef = ParamRef.Ref, ParamType = TaskFlow.Common.Enums.ParamType.IN)]
        public bool Bit14 { get; set; }

        [Parameter("Bit15", 15, CN = "位15", DefaultV = false, CanRef = ParamRef.Ref, ParamType = TaskFlow.Common.Enums.ParamType.IN)]
        public bool Bit15 { get; set; }

        [Parameter("拼接整数", 20, CN = "拼接整数", ParamType =TaskFlow.Common.Enums.ParamType.OUT)]
        public int JoinInt {  get; set; }

        public JoinBitToInt() 
        {
            this.Tips = "按位拼接";
            this.Icon= "\xe6da;";
        }

        public override bool DoExcute(out string errMsg)
        {

            JoinInt = 0x0001 * (Bit0 ? 1 : 0)  + 0x0002 * (Bit1 ? 1 : 0) +
                      0x0004 * (Bit2 ? 1 : 0)  + 0x0008 * (Bit3 ? 1 : 0) +
                      0x0010 * (Bit4 ? 1 : 0)  + 0x0020 * (Bit5 ? 1 : 0) +
                      0x0040 * (Bit6 ? 1 : 0)  + 0x0080 * (Bit7 ? 1 : 0) +
                      0x0100 * (Bit8 ? 1 : 0)  + 0x0200 * (Bit9 ? 1 : 0) +
                      0x0400 * (Bit10 ? 1 : 0) + 0x0800 * (Bit11 ? 1 : 0) +
                      0x1000 * (Bit12 ? 1 : 0) + 0x2000 * (Bit13 ? 1 : 0) +
                      0x4000 * (Bit14 ? 1 : 0) + 0x8000 * (Bit15 ? 1 : 0);
            return base.DoExcute(out errMsg);
        }


    }
}
