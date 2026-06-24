using Luster.Motion.DataStruct.Real;
using Luster.Motion.FiveAxis.Kinematics;
using System.Collections.Generic;

namespace Luster.Motion.FiveAxis.Service
{
    /// <summary>
    /// 精标 AccurateCalibrate 编排上下文(P5-5b,ADR-TES-110)。
    /// 承载卡端 Frame 旁路接口 + 坐标系 + 实/虚轴列表 + 粗标参数 + Rx/Rz 一圈脉冲,
    /// 供 <see cref="IFiveAxisCalibrationService.AccurateCalibrate"/> 编排 Frame 生命周期(ExitFrame→Frame→FrameCal→ExitFrame)。
    /// </summary>
    public class AccurateCalibrateContext
    {
        /// <summary>卡端 Frame 旁路接口(由运动卡实现,非五轴卡传 null 则 AccurateCalibrate 优雅退出返回 false)。</summary>
        public IFiveAxisFrame Frame { get; set; }

        /// <summary>坐标系编号(对应卡端 CrdProfile,源端 virAxesGroup.ChanelIndex)。</summary>
        public int CrdIndex { get; set; }

        /// <summary>实轴(关节轴)编号列表(源端 realAxesGroup.MotorLis.ChanelIndex)。</summary>
        public IReadOnlyList<int> RealAxisList { get; set; }

        /// <summary>虚轴(工件坐标轴)编号列表(源端 virAxesGroup.MotorLis.ChanelIndex)。</summary>
        public IReadOnlyList<int> VirAxisList { get; set; }

        /// <summary>粗标五轴参数(进逆解模式用,源端 objrough.Rough5Para)。</summary>
        public Coord5Axis Rough5Para { get; set; }

        /// <summary>Rx 轴一圈脉冲数(源端 getCirPulse(MRx)=360*PulseUnitRateAlpha/PulseUnitRateBeta,回填 Accurate5Para.ACirPulses)。</summary>
        public double MrxPulses { get; set; }

        /// <summary>Rz 轴一圈脉冲数(源端 getCirPulse(MRz),回填 Accurate5Para.CCirPulses)。</summary>
        public double MrzPulses { get; set; }
    }
}
