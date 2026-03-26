using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.DataStruct.Enums
{
    public enum OmronPLCDataType
    {
        [Description("CW")]
        CW,
        [Description("WW")]
        WW,
        [Description("HW")]
        HW,
        [Description("AW")]
        AW,
        [Description("DW")]
        DW,
        [Description("CB")]
        CB,
        [Description("WB")]
        WB,
        [Description("HB")]
        HB,
        [Description("AB")]
        AB,
        [Description("DB")]
        DB
    }
}
