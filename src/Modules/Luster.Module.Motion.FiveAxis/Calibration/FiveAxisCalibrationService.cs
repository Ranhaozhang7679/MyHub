using System;

namespace Luster.Motion.FiveAxis.Calibration
{
    /// <summary>
    /// <see cref="IFiveAxisCalibrationService"/> 默认实现（TES-190 P2-B Service 化骨架）。
    /// </summary>
    /// <remarks>
    /// ⚠️ 本实现是“诚实失败壳”：<see cref="Calibrate"/> 抛 <see cref="NotImplementedException"/>，
    /// 真正的标定数值求解（粗标球采样拟合/精标多角度最小二乘/激光约束/原点示教）仍在旧程序
    /// Form5Cali/FrameCal/ZFrameCali，留待后续 issue 迁移（D2 精度 + 硬件验证）——
    /// 不产出 fake 中间结果，避免错误标定值流入下游 recipe 造成精度事故。
    /// </remarks>
    public class FiveAxisCalibrationService : IFiveAxisCalibrationService
    {
        /// <inheritdoc />
        public CalibrationResult Calibrate(CalibrationInput input)
        {
            throw new NotImplementedException("标定算法本体待迁移（独立 issue，D2 精度+硬件验证）");
        }
    }
}
