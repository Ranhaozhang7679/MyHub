using Luster.Motion.FiveAxis.Data.Calibration;
using Luster.Motion.FiveAxis.Kinematics;
using Luster.Motion.FiveAxis.Position;

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
    /// - 精标 AccurateCalibrate:卡端 FrameCal 接入(ADR-TES-110 已落地)。严格按源端 frameCal(Form5Cali.cs:1312-1376)
    ///   顺序编排 IFiveAxisFrame 生命周期(ExitFrame→Frame(粗标)→FrameCal→ExitFrame,try/finally 保证 ExitFrame)。
    ///   精标 diff ⚠️ 待人类现场验证(卡端 FrameCal + 硬件采点,软件层无法 diff)。
    ///
    /// 「标定输出参数 diff(源端 vs 迁移后)」验收:精标依赖卡端 FrameCal + 硬件采点,⚠️ 待人类现场验证;
    /// 纯 C# 三阶段软件可 diff,但需源端标定输出基准(基准来源待人类决策)。
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
        /// 精标:由采样球心点列表(accurate.ResultFirstPosi + ResultRxPosiLis + ResultRzPosiLis)+ 粗标参数 + 球半径,
        /// 计算 accurate.Accurate5Para + ZeroRx。对应源端 Form5Cali.frameCal(:1312-1376)。
        /// 卡端 FrameCal 接入见 ADR-TES-110(IFiveAxisFrame 旁路接口);严格 Frame 生命周期编排,try/finally 保证 ExitFrame。
        /// 精标 diff ⚠️ 待人类现场验证(卡端 + 硬件采点)。
        /// </summary>
        bool AccurateCalibrate(FiveAxisFrameProfile frameProfile, AccurateCaliResult accurate, Coord5Axis rough5Para,
            double ballRadius, double mrxPulses, double mrzPulses);

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
