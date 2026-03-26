using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.Integration.Web
{
    /// <summary>
    /// 运行模式
    /// </summary>
    public enum TrainRunMode
    {
        [Description("运行中")]
        Running = 1,

        [Description("待料")]
        Idle = 2,

        [Description("维护")]
        Engineering = 3,

        [Description("通讯失败")]
        CommErr = 4,

        [Description("自动停机")]
        Down = 5,

        [Description("手动停机")]
        ManualDown = 6,

        [Description("未注册")]
        OffBoard = 7
    }
}
