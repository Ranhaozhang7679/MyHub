using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.TaskFlow.Motion.Enums
{
    public enum HomeType
    {
        [Description("回零")]
        Home,

        [Description("复位")]
        Reset,

        [Description("取消")]
        Cancel
    }
}
