using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct.Enums
{
    public enum  CoordType
    {
        [Description("笛卡尔")]
        Cartesian,

        [Description("关节")]
        Joint
    }
}
