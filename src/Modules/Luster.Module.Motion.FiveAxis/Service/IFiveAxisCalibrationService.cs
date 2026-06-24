using Luster.Motion.FiveAxis.Data.Calibration;
using Luster.Motion.FiveAxis.Kinematics;
using Luster.Motion.FiveAxis.Position;
using Luster.Motion.DataStruct.Real;
using System.Collections.Generic;

namespace Luster.Motion.FiveAxis.Service
{
    /// <summary>
    /// 五轴标定 Service 契约(P2-B,接口定义先行)。
    /// 对应源端 Form5Cali 四阶段标定算法(粗标/精标/激光标定/工件原点示教),算法从 UI 事件抽离为可被算子节点调用的 Service。
    ///
    /// 范围与阻塞说明(2026-06-24 源端核实):
    /// - 粗标 RoughCalibrate:纯 C# 几何(AngleHelper.CalculateRoateCenter 由三点示教结果算旋转中心),软件可验。
    /// - 激光标定 LaserCalibrate:纯 C# 计算(两点激光读数+Z 高度 → LinearConverter + CameraOffset),软件可验。
    /// - 工件原点示教 CalibrateWorkOrigin:纯 C# 几何(见 TeachWorkOriginResult.CalculateOriginOffset),软件可验。
    /// - 精标 AccurateCalibrate:卡端 FrameCal(ADR-TES-110 已就位),经 IFiveAxisFrame 旁路接口编排 Frame 生命周期。
    ///   算法在卡端 Basic 固件,PC 侧无可剥离 C#;同卡同固件输入相同采样点 → 输出一致,diff≤1e-6 自然满足。
    ///   精标 diff 真机验收(卡端 FrameCal + 硬件采点)⚠️ 待人类现场验证(ADR R-F4),软件层不阻塞。
    /// </summary>
    public interface IFiveAxisCalibrationService
    {
        /// <summary>
        /// 粗标:由三点示教结果(rough.ResultFirstPosi/ResultRxPosi/ResultRzPosi)+ 示教角度 + 脉冲,
        /// 计算 rough.Rough5Para(ACenter/ADir/CCenter/CDir/ACirPulses/CCirPulses)。
        /// 对应源端 Form5Cali.RoughBtnClick.btnRoughCalculate(:831)。
        /// </summary>
        /// <param name="mrxPulses">Rx 轴一圈脉冲数(源端 getCirPulse(MRx))</param>
        /// <param name="mrzPulses">Rz 轴一圈脉冲数(源端 getCirPulse(MRz))</param>
        bool RoughCalibrate(RoughCaliResult rough, double mrxPulses, double mrzPulses);

        /// <summary>
        /// 精标:由采样球心点列表(accurate.ResultFirstPosi + ResultRxPosiLis + ResultRzPosiLis)+ 粗标参数,
        /// 经卡端 FrameCal 解算,写入 accurate.Accurate5Para + ZeroRx。对应源端 Form5Cali.frameCal(:1312)。
        /// 编排严格对齐源端 :1322-1376 顺序:ExitFrame(清残留)→ Frame(粗标 Rough5Para)→ FrameCal → ExitFrame(必退)。
        /// Frame 生命周期由 try/finally 保证 ExitFrame(R-F2 缓解)。卡端解算经 IFiveAxisFrame 旁路接口(ADR-TES-110)。
        /// </summary>
        /// <param name="accurate">精标结果(含采样球心点列表,调用方需先采点填 ResultFirstPosi/ResultRxPosiLis/ResultRzPosiLis)</param>
        /// <param name="ctx">精标编排上下文(卡端 Frame 接口 + 坐标系 + 实/虚轴列表 + 粗标参数 + Rx/Rz 一圈脉冲)</param>
        bool AccurateCalibrate(AccurateCaliResult accurate, AccurateCalibrateContext ctx);

        /// <summary>
        /// 激光标定:由两点激光读数+Z 高度 + 标准值 + 激光/相机示教位置,
        /// 计算 laser.LaserMap(LinearConverter)与 CameraOffset。对应源端 Form5Cali.laserCaliApply(:281)。
        /// </summary>
        bool LaserCalibrate(LaserCaliResult laser, double laser1, double z1, double laser2, double z2,
            double laserStandard, PositionXYZ laserPosi, PositionXYZ cameraPosi);

        /// <summary>
        /// 工件原点示教:由三点示教(OriginPosi/LongSidePosi/DiagonalPosi)+ 原点类型,
        /// 计算 origin.RltTool2Work 与原点偏移。对应源端 TeachWorkOriginProfile.CalculateOriginOffset。
        /// </summary>
        bool CalibrateWorkOrigin(TeachWorkOriginResult origin);
    }
}
