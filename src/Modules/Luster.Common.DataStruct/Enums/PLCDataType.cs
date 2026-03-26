using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.DataStruct.Enums
{
    public enum PLCDataType
    {
        [Description("X")]
        X,
        [Description("Y")]
        Y,
        [Description("M")]
        M,
        [Description("L")]
        L,
        [Description("F")]
        F,
        [Description("V")]
        V,
        [Description("B")]
        B,
        [Description("D")]
        D,
        [Description("W")]
        W
    }
}
