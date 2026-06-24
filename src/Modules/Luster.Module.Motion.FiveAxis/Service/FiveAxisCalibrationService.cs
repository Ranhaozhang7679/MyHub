using Luster.Motion.FiveAxis.Data.Calibration;
using Luster.Motion.FiveAxis.Kinematics;
using Luster.Motion.FiveAxis.Position;
using Luster.Motion.FiveAxis.Utils;
using Luster.Motion.DataStruct.Real;
using System;
using System.Collections.Generic;
using System.Linq;

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
    /// - <see cref="AccurateCalibrate"/>:精标,卡端 FrameCal(ADR-TES-110 已就位)——经 <see cref="IFiveAxisFrame"/> 旁路接口
    ///   编排 Frame 生命周期(ExitFrame→Frame(粗标)→FrameCal→ExitFrame,try/finally 保证必退 R-F2)。算法在卡端 Basic 固件,
    ///   PC 侧无可剥离 C#;同卡同固件输入相同采样点 → 输出一致,diff≤1e-6 自然满足。精标 diff 真机验收 ⚠️ 待人类现场验证(R-F4)。
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
        /// 精标:球心采样点经卡端 FrameCal 解算,写入 <c>accurate.Accurate5Para</c> + <c>ZeroRx</c>。
        /// 严格对齐源端 <c>Form5Cali.frameCal()</c>(Form5Cali.cs:1322-1376)编排顺序:
        /// <c>ExitFrame</c>(清残留)→ <c>Frame(粗标 Rough5Para)</c> → <c>FrameCal</c> → <c>ExitFrame</c>(必退)。
        /// Frame 生命周期由 try/finally 保证 <c>ExitFrame</c>(R-F2 缓解,接口不自动清理)。
        /// 卡端解算经 <see cref="IFiveAxisFrame"/> 旁路接口(ADR-TES-110);非五轴卡(ctx.Frame 为 null)优雅退出返回 false。
        /// </summary>
        public bool AccurateCalibrate(AccurateCaliResult accurate, AccurateCalibrateContext ctx)
        {
            if (accurate == null) throw new ArgumentNullException(nameof(accurate));
            if (ctx == null) throw new ArgumentNullException(nameof(ctx));
            // 非五轴卡(无 IFiveAxisFrame 能力)优雅退出,不抛异常(对齐 ADR 骨架 motionCard is not IFiveAxisFrame return false)
            if (ctx.Frame == null) return false;

            var frame = ctx.Frame;
            var realLis = ctx.RealAxisList ?? new List<int>();
            var virLis = ctx.VirAxisList ?? new List<int>();

            // 采样点 = ResultFirstPosi + ResultRxPosiLis + ResultRzPosiLis,转 5 轴脉冲数组(源端 :1315-1318 + MotorPosiHelper.To5AxisLis)
            var totalLis = new List<double[]> { To5Axis(accurate.ResultFirstPosi) };
            foreach (var p in accurate.ResultRxPosiLis) totalLis.Add(To5Axis(p));
            foreach (var p in accurate.ResultRzPosiLis) totalLis.Add(To5Axis(p));

            var roughPara = ToFramePara(ctx.Rough5Para);
            try
            {
                if (!frame.ExitFrame(realLis, virLis)) return false;                              // 先退(防残留,源端 :1322)
                if (!frame.Frame(ctx.CrdIndex, realLis, virLis, roughPara)) return false;         // 进粗标逆解模式(源端 :1328)
                if (!frame.FrameCal(ctx.CrdIndex, realLis.Take(3).ToList(), totalLis, out var aZero, out var accurateFramePara))
                    return false;                                                                  // 卡端解算(源端 :1334,实轴取前 3 轴)

                // 回填:ZeroRx + Accurate5Para(源端 :1340-1343),CirPulses 由 getCirPulse(MRx/MRz) 覆盖
                accurate.ZeroRx = aZero;
                var ap = FromFramePara(accurateFramePara);
                ap.ACirPulses = ctx.MrxPulses;
                ap.CCirPulses = ctx.MrzPulses;
                accurate.Accurate5Para.CopyFrom(ap);
            }
            finally
            {
                // 必退:保卡端 Connframe 状态干净(R-F2,源端 :1371)
                frame.ExitFrame(realLis, virLis);
            }
            return true;
        }

        /// <summary>PositionXYZRxRyRz → 5 轴脉冲数组 {X,Y,Z,RX,RZ}(原样迁自源端 MotorPosiHelper.To5AxisLis)。</summary>
        private static double[] To5Axis(PositionXYZRxRyRz posi)
        {
            if (posi == null) return new double[5];
            return new double[] { posi.X, posi.Y, posi.Z, posi.RX, posi.RZ };
        }

        /// <summary>Coord5Axis → FiveAxisFramePara(接口落点 DataStruct 不能引用 Coord5Axis,Service 侧互转)。</summary>
        private static FiveAxisFramePara ToFramePara(Coord5Axis para)
        {
            if (para == null) return new FiveAxisFramePara();
            return new FiveAxisFramePara
            {
                ACenterX = para.ACenter.X, ACenterY = para.ACenter.Y, ACenterZ = para.ACenter.Z,
                ADirX = para.ADir.X, ADirY = para.ADir.Y, ADirZ = para.ADir.Z,
                ACirPulses = para.ACirPulses,
                CCenterX = para.CCenter.X, CCenterY = para.CCenter.Y, CCenterZ = para.CCenter.Z,
                CDirX = para.CDir.X, CDirY = para.CDir.Y, CDirZ = para.CDir.Z,
                CCirPulses = para.CCirPulses,
            };
        }

        /// <summary>FiveAxisFramePara → Coord5Axis(卡端 FrameCal 输出回填 FiveAxis 数据模型)。</summary>
        private static Coord5Axis FromFramePara(FiveAxisFramePara para)
        {
            return new Coord5Axis
            {
                ACenter = new PositionXYZ(para.ACenterX, para.ACenterY, para.ACenterZ),
                ADir = new PositionXYZ(para.ADirX, para.ADirY, para.ADirZ),
                ACirPulses = para.ACirPulses,
                CCenter = new PositionXYZ(para.CCenterX, para.CCenterY, para.CCenterZ),
                CDir = new PositionXYZ(para.CDirX, para.CDirY, para.CDirZ),
                CCirPulses = para.CCirPulses,
            };
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
