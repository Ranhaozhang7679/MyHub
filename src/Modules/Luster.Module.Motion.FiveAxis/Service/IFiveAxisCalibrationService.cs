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
    /// - 精标 AccurateCalibrate:⚠️ 源端核心 = station.FrameCal(...) 为**运动卡 SDK 卡端调用**(需先 Frame 进逆解模式),
    ///   非纯 C#,软件不可复现。实现待 P5-5b:需决策"卡端 FrameCal 接口如何暴露(P0-A ZMotion 适配器?新 IFiveAxisFrameCal?)"。
    ///
    /// 「标定输出参数 diff(源端 vs 迁移后)」验收需源端参考标定输出做基准,且精标依赖卡端 FrameCal ——
    /// 两者均待项目经理/人类决策,故本接口仅定义契约,实现随 P5-5b 落地。
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
        /// 计算 accurate.Accurate5Para + ZeroRx。对应源端 Form5Cali.frameCal(:1312)。
        /// ⚠️ 源端核心计算 station.FrameCal 为运动卡 SDK 卡端调用,实现待 P5-5b 卡端接口决策。
        /// </summary>
        bool AccurateCalibrate(AccurateCaliResult accurate, Coord5Axis rough5Para, double ballRadius, double mrxPulses, double mrzPulses);

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
