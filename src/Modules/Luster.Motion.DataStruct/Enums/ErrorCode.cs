using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct.Enums
{
    /// <summary>
    /// 设备
    /// </summary>
    public enum DeviceError
    {
        [Description("未知异常")]
        None,

        [Description("非安全位")]
        SafeFail,

        [Description("回零失败")]
        ZeroFail,

        [Description("使能失败")]
        SerOnFail,

        [Description("正极限报警")]
        PelFail,

        [Description("负极限报警")]
        MelFail,

        [Description("运动超时")]
        MoveTimeFail,

        [Description("伸出异常")]
        ExtendFail,

        [Description("缩回异常")]
        RetractFail,

        [Description("真空吹失败")]
        BlowFail,

        [Description("信号异常")]
        SuckFail,

        [Description("通讯异常")]
        ConnectTimeFail,

        [Description("信号异常")]
        SensorFail,

        [Description("维护提醒")]
        HPMatain,

        [Description("安全门报警")]
        SafeDoorFail
    }

    /// <summary>
    /// 系统错误
    /// </summary>
    public enum SystemError
    {
        [Description("急停")]
        EmgFail,

        [Description("安全光栅")]
        Lightcurtain,

        [Description("连接失败")]
        ConnectFail,

        [Description("等待超时")]
        WaitFail,

        [Description("系统警告")]
        Warnning,

        [Description("PLC报警")]
        PlcWarnFail,

        [Description("PLC初始化")]
        PlcInitFail
    }
}
