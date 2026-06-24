using System;

namespace Luster.Module.Motion.TestToolchain.Simulation
{
    /// <summary>
    /// 光学模拟器（TES-34 P9-C，迁移自源端 FormSTLs.cs 内部类 OpticalSimulator）。
    /// 模拟 Bayer 相机脉宽→灰度响应：光源强度(gamma 1.2 非线性) → 串扰矩阵 → 颜色校正矩阵 → 钳位取整。
    /// 用于无硬件环境下离线调参验证，不依赖真实相机。
    /// </summary>
    /// <remarks>
    /// <b>源端状态</b>：源端 OpticalSimulator.SimulateCamera 为死代码（全仓无调用方），
    /// 但算法完整（gamma 1.2 + 3x3 串扰矩阵 + 3x3 颜色校正矩阵）。本类等价移植算法，
    /// 作为虚拟相机光学预测器，接入 VCamera 后即可在纯虚拟环境跑通自动光调闭环。
    /// 噪声项源端已注释，此处保持无噪声（确定性，便于单测）。
    /// </remarks>
    public class OpticalSimulator
    {
        /// <summary>RGB 灰度值（0-255）</summary>
        public struct RGBValues
        {
            public int R;
            public int G;
            public int B;

            public RGBValues(int r, int g, int b)
            {
                R = r;
                G = g;
                B = b;
            }
        }

        /// <summary>RGB 脉宽值</summary>
        public struct PulseValues
        {
            public int R;
            public int G;
            public int B;

            public PulseValues(int r, int g, int b)
            {
                R = r;
                G = g;
                B = b;
            }
        }

        /// <summary>Bayer 相机串扰矩阵（行=受影响通道 R/G/B，列=光源 R/G/B）</summary>
        private static readonly double[,] CrosstalkMatrix =
        {
            { 0.92, 0.08, 0.05 },
            { 0.15, 0.85, 0.12 },
            { 0.03, 0.15, 0.88 },
        };

        /// <summary>相机颜色校正矩阵</summary>
        private static readonly double[,] ColorCorrectionMatrix =
        {
            {  1.15, -0.12, -0.08 },
            { -0.15,  1.25, -0.15 },
            { -0.05, -0.12,  1.18 },
        };

        /// <summary>LED 非线性响应指数（源端 gamma 1.2）</summary>
        private const double GammaExponent = 1.2;

        /// <summary>脉宽归一基准（源端以 100 为基准）</summary>
        private const double PulseNormalizeBase = 100.0;

        /// <summary>灰度上限</summary>
        private const int GrayMax = 255;

        /// <summary>
        /// 模拟相机：RGB 脉宽 + 光源亮度权重 → 灰度值。
        /// </summary>
        /// <param name="rPulse">R 通道脉宽</param>
        /// <param name="gPulse">G 通道脉宽</param>
        /// <param name="bPulse">B 通道脉宽</param>
        /// <param name="lightWeightR">R 光源亮度权重（0-1）</param>
        /// <param name="lightWeightG">G 光源亮度权重</param>
        /// <param name="lightWeightB">B 光源亮度权重</param>
        /// <returns>RGB 灰度值（0-255，钳位取整）</returns>
        public RGBValues SimulateCamera(int rPulse, int gPulse, int bPulse,
                                        float lightWeightR, float lightWeightG, float lightWeightB)
        {
            // 1. 光源强度（gamma 1.2 非线性 + 亮度权重 + 归一到 255）
            double lightR = Math.Pow(rPulse / PulseNormalizeBase, GammaExponent) * GrayMax * lightWeightR;
            double lightG = Math.Pow(gPulse / PulseNormalizeBase, GammaExponent) * GrayMax * lightWeightG;
            double lightB = Math.Pow(bPulse / PulseNormalizeBase, GammaExponent) * GrayMax * lightWeightB;

            // 2. 串扰：raw = crosstalkMatrix × light
            double[] light = { lightR, lightG, lightB };
            double[] raw = MultiplyMatrixVector(CrosstalkMatrix, light);

            // 3. 颜色校正：corrected = colorCorrectionMatrix × raw
            double[] corrected = MultiplyMatrixVector(ColorCorrectionMatrix, raw);

            // 4. 钳位 0-255 + 取整
            return new RGBValues(
                ClampRound(corrected[0]),
                ClampRound(corrected[1]),
                ClampRound(corrected[2]));
        }

        private static double[] MultiplyMatrixVector(double[,] m, double[] v)
        {
            double[] result = new double[3];
            for (int i = 0; i < 3; i++)
            {
                double sum = 0;
                for (int j = 0; j < 3; j++)
                {
                    sum += m[i, j] * v[j];
                }
                result[i] = sum;
            }
            return result;
        }

        private static int ClampRound(double value)
        {
            return (int)Math.Round(Math.Max(0, Math.Min(GrayMax, value)));
        }
    }
}
