using System.ComponentModel;

namespace Luster.Module.Motion.Device.Functions
{
    /// <summary>
    /// 音圈电机动作类型
    /// </summary>
    public enum VCMActionType
    {
        [Description("使能")]
        ServoOn,

        [Description("复位")]
        Reset,

        [Description("失能")]
        ServoOff,

        [Description("回零")]
        Home,

        [Description("非标回零")]
        HomeNonStandard,

        [Description("硬着陆")]
        HardLanding,

        [Description("软着陆")]
        SoftLanding
    }
}