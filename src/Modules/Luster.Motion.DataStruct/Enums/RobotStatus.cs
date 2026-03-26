using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct.Enums
{
    public enum RobotStatus
    {
        [Description("报警")]
        Alarm = 0,

        [Description("连接")]
        Connect = 1,

        [Description("使能")]
        SvOn = 2,

        [Description("拖拽")]
        Drag = 3,

        [Description("停止")]
        Stop =4,

        [Description("暂停")]
        Pause = 5,

        [Description("运行")]
        Moving = 6,

        [Description("手动运行中")]
        ManualMoving = 7,

        [Description("急停")]
        Emg = 8,

        [Description("到位")]
        OnPos = 9,

    }

    /// <summary>
    /// 机器人的运动模式
    /// </summary>
    public enum RobotMoveMode
    {
        [Description("点位模式")]
        Move,

        [Description("Jump模式")]
        Jump,

        [Description("回零模式")]
        Home,

        [Description("读取位置")]
        GetPosition
    }
}
