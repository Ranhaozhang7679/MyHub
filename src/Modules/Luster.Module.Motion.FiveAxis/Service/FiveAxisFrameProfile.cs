using System.Collections.Generic;

namespace Luster.Motion.FiveAxis.Service
{
    /// <summary>
    /// 精标 Frame 生命周期配置(ADR-TES-110)。
    /// 封装卡端 Frame 编排所需的坐标系编号 + 实轴/虚轴列表,对齐源端 <c>Check5AxisStationBase</c> 用
    /// <c>MotorGroupComponent.ChanelIndex</c>(crdIndex)+ <c>MotorLis.Select(m => m.ChanelIndex)</c>(实/虚轴列表)的语义。
    /// </summary>
    public class FiveAxisFrameProfile
    {
        /// <summary>坐标系编号(对齐源端 virAxesGroup.ChanelIndex / crdIndex)</summary>
        public int CrdIndex { get; set; }

        /// <summary>实轴(关节轴)编号列表(对齐源端 realAxesGroup.MotorLis 各 ChanelIndex)</summary>
        public IReadOnlyList<int> RealAxisIds { get; set; }

        /// <summary>虚轴(工件坐标轴)编号列表(对齐源端 virAxesGroup.MotorLis 各 ChanelIndex)</summary>
        public IReadOnlyList<int> VirtualAxisIds { get; set; }
    }
}
