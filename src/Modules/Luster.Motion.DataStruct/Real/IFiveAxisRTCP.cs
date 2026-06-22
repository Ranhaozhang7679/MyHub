using System;
using System.Collections.Generic;

namespace Luster.Motion.DataStruct.Real
{
    /// <summary>
    /// 五轴 RTCP 旁路接口。
    /// 不扩展 IMotionCard 主契约，仅由具备卡侧 RTCP 能力的板卡实现。
    /// </summary>
    public interface IFiveAxisRTCP
    {
        /// <summary>
        /// 当前 RTCP 配置。
        /// </summary>
        FiveAxisRtcpConfig RtcpConfig { get; }

        /// <summary>
        /// RTCP 是否已启用。
        /// </summary>
        bool RtcpEnabled { get; }

        /// <summary>
        /// 配置卡侧 RTCP 坐标系参数。
        /// </summary>
        bool ConfigureRtcp(FiveAxisRtcpConfig config);

        /// <summary>
        /// 开关卡侧 RTCP。
        /// </summary>
        bool SetRtcpEnabled(bool enabled);
    }

    /// <summary>
    /// 五轴 RTCP 配置参数。
    /// </summary>
    [Serializable]
    public class FiveAxisRtcpConfig
    {
        public int CoordinateSystem { get; set; }

        public List<int> VirtualAxisIds { get; set; } = new List<int>();

        public List<int> RealAxisIds { get; set; } = new List<int>();

        public double RotationCenterX { get; set; }

        public double RotationCenterY { get; set; }

        public double RotationCenterZ { get; set; }

        public double ToolOffsetX { get; set; }

        public double ToolOffsetY { get; set; }

        public double ToolOffsetZ { get; set; }
    }
}
