using Luster.Motion.FiveAxis.Data.Calibration;
using Luster.Motion.FiveAxis.Kinematics;
using Luster.Motion.FiveAxis.Position;
using Luster.Motion.FiveAxis.Utils;
using System;

namespace Luster.Motion.FiveAxis.Service
{
    /// <summary>
    /// 五轴标定 Service 实现(P5-5b,承接 TES-100 接口定义)。
    /// 把源端 <c>Form5Cali</c> 四阶段标定算法从 UI 事件抽离为可被算子节点调用的 Service,算法本体原样迁自源端,保证可还原性。
    /// </summary>
    /// <remarks>
    /// <b>源端对照</b>(核实于 SP-2025140 <c>Form5Cali.cs</c>):
    /// - <see cref="RoughCalibrate"/>:粗标,纯 C# 几何,对应 <c>btnRoughCalculate</c>(Form5Cali.cs:831)——
    ///   <c>AngleHelper.CalculateRoateCenter</c> 由三点示教结果算 A/C 轴旋转中心,写入 <c>Rough5Para</c>(ACenter/ADir/ACirPulses/CCenter/CDir/CCirPulses)。
    /// - <see cref="LaserCalibrate"/>:激光标定,纯 C# 计算,对应 <c>laserCaliApply</c>(Form5Cali.cs:281)——
    ///   两点激光读数+Z 高度填 <c>LinearConverter</c>,记录激光/相机示教位置;CameraOffset = CameraPosi - LaserPosi
    ///   由下游 <see cref="CalibratedCoord5Axis"/> 按需派生(源端存 BaseProfile.CameraOffset,本数据模型存 LaserPosi/CameraPosi)。
    /// - <see cref="CalibrateWorkOrigin"/>:工件原点示教,纯 C# 几何,对应 <c>btnWorkCalculateFromTeach</c>(Form5Cali.cs:1757)——
    ///   调 <see cref="TeachWorkOriginResult.CalculateOriginOffset"/> 算原点偏移,写入 <c>RltTool2Work.Trans</c>。
    /// - <see cref="AccurateCalibrate"/>:精标,⚠️ <b>阻塞</b>——源端核心 <c>frameCal</c>(Form5Cali.cs:1312)的
    ///   <c>station.FrameCal(...)</c> 为运动卡 SDK 卡端调用(需先 Frame 进逆解模式),非纯 C#,软件层无法复现。
    ///   实现待 FrameCal ADR 落地(倾向 P0-A ZMotion 适配器暴露新 <c>IFiveAxisFrameCal</c>,R1 非侵入),此处抛
    ///   <see cref="NotSupportedException"/> 明确阻塞,不猜测/不编造卡端算法。
    ///
    /// <b>R1 非侵入</b>:改动全在 FiveAxis 叶子模块,平台主干(IMotionCard/Luster.Prism/Luster.TaskFlow.Common)零改动。
    /// </remarks>
    public class FiveAxisCalibrationService : IFiveAxisCalibrationService
    {
        /// <summary>
        /// 粗标:由三点示教结果(<c>rough.ResultFirstPosi/ResultRxPosi/ResultRzPosi</c>)+ 示教角度 + 脉冲,
        /// 计算 <c>rough.Rough5Para</c>。原样迁自源端 <c>Form5Cali.btnRoughCalculate</c>(Form5Cali.cs:831-867)。
        /// </summary>
        /// <param name="rough">粗标结果(调用方需先 <see cref="RoughCaliResult.GeneratePosi"/> 填好 RxPosi/RzPosi)</param>
        /// <param name="mrxPulses">Rx 轴一圈脉冲数(源端 getCirPulse(MRx) = 360 * PulseUnitRateAlpha / PulseUnitRateBeta)</param>
        /// <param name="mrzPulses">Rz 轴一圈脉冲数(源端 getCirPulse(MRz))</param>
        /// <returns>计算成功返回 true(几何计算无失败分支,与源端一致)</returns>
        public bool RoughCalibrate(RoughCaliResult rough, double mrxPulses, double mrzPulses)
        {
            if (rough == null) throw new ArgumentNullException(nameof(rough));

            // A 轴旋转中心:取 YZ 平面投影,旋转角 = RxPosi.RX - FirstPosi.RX(= rough.Rx)
            // 源端 pf/pa 用 Result*.Y/Result*.Z + FirstPosi.RX/RxPosi.RX
            var pfA = new PositionXYRz
            {
                X = rough.ResultFirstPosi.Y,
                Y = rough.ResultFirstPosi.Z,
                RZ = rough.FirstPosi.RX,
            };
            var pa = new PositionXYRz
            {
                X = rough.ResultRxPosi.Y,
                Y = rough.ResultRxPosi.Z,
                RZ = rough.RxPosi.RX,
            };
            var centerA = AngleHelper.CalculateRoateCenter(pfA, pa);

            // C 轴旋转中心:取 XY 平面投影,旋转角 = RzPosi.RZ - FirstPosi.RZ(= rough.Rz)
            var pfC = new PositionXYRz
            {
                X = rough.ResultFirstPosi.X,
                Y = rough.ResultFirstPosi.Y,
                RZ = rough.FirstPosi.RZ,
            };
            var pc = new PositionXYRz
            {
                X = rough.ResultRzPosi.X,
                Y = rough.ResultRzPosi.Y,
                RZ = rough.RzPosi.RZ,
            };
            var centerC = AngleHelper.CalculateRoateCenter(pfC, pc);

            // 五轴结构参数:A 轴方向沿 X,C 轴方向沿 Z(3+2 构型约定,与源端一致)
            rough.Rough5Para.ACenter = new PositionXYZ(0, centerA.X, centerA.Y);
            rough.Rough5Para.ADir = new PositionXYZ(1, 0, 0);
            rough.Rough5Para.CCenter = new PositionXYZ(centerC.X, centerC.Y, 0);
            rough.Rough5Para.CDir = new PositionXYZ(0, 0, 1);

            rough.Rough5Para.ACirPulses = mrxPulses;
            rough.Rough5Para.CCirPulses = mrzPulses;

            return true;
        }

        /// <summary>
        /// 精标:球心采样 + 卡端 FrameCal,写入 <c>accurate.Accurate5Para</c> + <c>ZeroRx</c>。
        /// ⚠️ <b>阻塞</b>:源端核心 <c>station.FrameCal(...)</c>(Form5Cali.cs:1334)为运动卡 SDK 卡端调用,非纯 C#,
        /// 软件层无法复现。实现待 FrameCal ADR(倾向 P0-A ZMotion 适配器暴露 <c>IFiveAxisFrameCal</c>)。
        /// 在 ADR 落地前抛 <see cref="NotSupportedException"/>,不猜测/不编造卡端算法。
        /// </summary>
        public bool AccurateCalibrate(AccurateCaliResult accurate, Coord5Axis rough5Para, double ballRadius, double mrxPulses, double mrzPulses)
        {
            throw new NotSupportedException(
                "精标 AccurateCalibrate 阻塞:源端核心 station.FrameCal(...) 为运动卡 SDK 卡端调用(Form5Cali.cs:1334)," +
                "非纯 C#,软件层无法复现。实现待 FrameCal ADR 落地(倾向 P0-A ZMotion 适配器暴露 IFiveAxisFrameCal,R1 非侵入)。" +
                "见 TES-111 决策 A / 验收「精标:⚠️ 待人类现场验证」。");
        }

        /// <summary>
        /// 激光标定:由两点激光读数+Z 高度 + 标准值 + 激光/相机示教位置,填 <c>laser.LaserMap</c>(LinearConverter)
        /// 与 <c>LaserPosi/CameraPosi</c>。原样迁自源端 <c>Form5Cali.laserCaliApply</c>(Form5Cali.cs:281-295)。
        /// CameraOffset = CameraPosi - LaserPosi 由下游 <see cref="CalibratedCoord5Axis"/> 派生(源端存 BaseProfile.CameraOffset)。
        /// </summary>
        public bool LaserCalibrate(LaserCaliResult laser, double laser1, double z1, double laser2, double z2,
            double laserStandard, PositionXYZ laserPosi, PositionXYZ cameraPosi)
        {
            if (laser == null) throw new ArgumentNullException(nameof(laser));

            laser.LaserStandard = laserStandard;
            // LinearConverter:DirectValue=激光测量值,UnitValue=Z 轴高度(见 LinearConverter 注释)
            laser.LaserMap.Map1.DirectValue = laser1;
            laser.LaserMap.Map1.UnitValue = z1;
            laser.LaserMap.Map2.DirectValue = laser2;
            laser.LaserMap.Map2.UnitValue = z2;
            laser.LaserPosi = laserPosi;
            laser.CameraPosi = cameraPosi;

            // CameraOffset = CameraPosi - LaserPosi(源端 getStation().BaseProfile.CameraOffset = obj.CameraPosi - obj.LaserPosi)。
            // 本数据模型不单独存 CameraOffset,下游 CalibratedCoord5Axis(accurate5Para, cameraPosi - laserPosi) 按需派生。
            return true;
        }

        /// <summary>
        /// 工件原点示教:由三点示教(<c>origin.OriginPosi/LongSidePosi/DiagonalPosi</c>)+ 原点类型,
        /// 计算 <c>origin.RltTool2Work.Trans</c>。原样迁自源端 <c>Form5Cali.btnWorkCalculateFromTeach</c>(Form5Cali.cs:1757-1774)。
        /// </summary>
        public bool CalibrateWorkOrigin(TeachWorkOriginResult origin)
        {
            if (origin == null) throw new ArgumentNullException(nameof(origin));

            var rltValue = origin.CalculateOriginOffset();
            origin.RltTool2Work.Trans = new PositionXYZRxRyRz
            {
                X = rltValue.X,
                Y = rltValue.Y,
                Z = rltValue.Z,
                RZ = rltValue.RZ,
            };
            return true;
        }
    }
}
