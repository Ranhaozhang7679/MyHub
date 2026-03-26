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
    public class SplitIntToBit: MotionFunction
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public SplitIntToBit()
        {
            this.Tips = "按位读取";
            this.Icon = "\xe6d9";
        }

        [Range(0,25535)]
        [Parameter("运算数",0, CN = "按位读取的数字",ParamType =TaskFlow.Common.Enums.ParamType.IN,CanRef =ParamRef.Ref)]
        public int SplitInt {  get; set; }

        [Parameter("位0", 10, CN = "位0", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool Bit0 { get; set; }

        [Parameter("位1",11, CN = "位1", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool Bit1 { get; set; }

        [Parameter("位2", 12, CN = "位2", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool Bit2 { get; set; }

        [Parameter("位3",13, CN = "位3", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool Bit3 { get; set; }

        [Parameter("位4", 14, CN = "位4", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool Bit4 { get; set; }

        [Parameter("位5", 15, CN = "位5", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool Bit5 { get; set; }

        [Parameter("位6", 16, CN = "位6", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool Bit6 { get; set; }

        [Parameter("位7", 17, CN = "位7", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool Bit7 { get; set; }


        [Parameter("位8", 18, CN = "位8", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool Bit8 { get; set; }

        [Parameter("位9", 19, CN = "位9", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool Bit9 { get; set; }

        [Parameter("位10", 20, CN = "位10", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool Bit10 { get; set; }

        [Parameter("位11", 21, CN = "位11", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool Bit11 { get; set; }

        [Parameter("位12", 22, CN = "位12", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool Bit12{ get; set; }

        [Parameter("位13", 23, CN = "位13", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool Bit13 { get; set; }

        [Parameter("位14", 24, CN = "位14", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool Bit14{ get; set; }

        [Parameter("位15", 25, CN = "位15", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool Bit15 { get; set; }


        public override bool DoExcute(out string errMsg)
        {
            errMsg = string.Empty;
            Bit0  = (SplitInt & 0x01) == 0x01;
            Bit1  = (SplitInt & 0x02) == 0x02;
            Bit2  = (SplitInt & 0x04) == 0x04;
            Bit3  = (SplitInt & 0x08) == 0x08;
            Bit4  = (SplitInt & 0x10) == 0x10;
            Bit5  = (SplitInt & 0x20) == 0x20;
            Bit6  = (SplitInt & 0x40) == 0x40;
            Bit7  = (SplitInt & 0x80) == 0x80;
            Bit8  = (SplitInt & 0x100) == 0x100;
            Bit9  = (SplitInt & 0x200) == 0x200;
            Bit10 = (SplitInt & 0x400) == 0x400;
            Bit11 = (SplitInt & 0x800) == 0x800;
            Bit12 = (SplitInt & 0x1000) == 0x1000;
            Bit13 = (SplitInt & 0x2000) == 0x2000;
            Bit14 = (SplitInt & 0x4000) == 0x4000;
            Bit15 = (SplitInt & 0x8000) == 0x8000;
            return true;

        }

    }
}
