using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.TaskFlow.Motion.Enums
{
    public enum InteractiveType
    {
        [Description("继续")]
        Continue,

        [Description("重试")]
        Retry,

        [Description("停止")]
        Stop
    }
}
