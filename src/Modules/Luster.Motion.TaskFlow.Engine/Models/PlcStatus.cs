using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.TaskFlow.Engine.Models
{
    /// <summary>
    /// Plc的状态
    /// </summary>
    public enum PlcStatus
    {
        [Description("空闲")]
        None = 0,

        [Description("手动")]
        Manual = 1,

        [Description("待初始化")]
        Init = 2,

        [Description("初始完成")]
        Inited = 3,

        [Description("运行中")]
        AutoRun = 4,

        [Description("暂停中")]
        Pause = 5,

        [Description("报警中")]
        Alarm = 6
    }
}
