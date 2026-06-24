using System;
using System.Globalization;
using System.IO;
using System.Numerics;
using Luster.Tools.DiffRegression.Differ;

namespace Luster.Tools.DiffRegression
{
    /// <summary>
    /// 工具自测：用 Coord5Axis 不变式数据验证 matrix diff 可用性。
    /// <para>
    /// 不变式：<c>GetOrg2DestMatrix</c>（正解，源→目标）与 <c>GetDest2OrgMatrix</c>（逆解，目标→源）
    /// 互为逆矩阵，二者相乘应回到 4x4 单位阵 I，误差 ≤1e-6（Coord5Axis 级联欧拉 + 旋转中心的数值不变式）。
    /// </para>
    /// <para>
    /// 本自测构造一个 3+2 五轴典型位姿矩阵 M（绕 V 轴旋转 + 平移），令 baseline = I，actual = M * M⁻¹，
    /// 用 <see cref="MatrixDiffer"/> 比对应 PASS；再对 actual 注入 1e-3 扰动应 FAIL。
    /// 不引用主干 Coord5Axis 程序集，保持工具工程非侵入。
    /// </para>
    /// </summary>
    internal static class SelfTest
    {
        public static bool Run()
        {
            Console.WriteLine("=== Luster.Tools.DiffRegression 自测（Coord5Axis 不变式）===");

            // 3+2 构型典型位姿：绕 Z（V 旋转轴）转 30°，平移 (100, 50, 200) mm
            Matrix4x4 pose = Matrix4x4.CreateRotationZ((float)(30.0 * Math.PI / 180.0)) *
                             Matrix4x4.CreateTranslation(100f, 50f, 200f);
            if (!Matrix4x4.Invert(pose, out Matrix4x4 poseInv))
            {
                Console.WriteLine("[fatal] 位姿矩阵不可逆，自测无法继续。");
                return false;
            }
            Matrix4x4 product = pose * poseInv; // 应为 I

            string tmpDir = Path.Combine(Path.GetTempPath(), "LusterDiffSelfTest");
            Directory.CreateDirectory(tmpDir);
            string identityPath = Path.Combine(tmpDir, "baseline_identity.csv");
            string productPath = Path.Combine(tmpDir, "actual_product.csv");
            string perturbedPath = Path.Combine(tmpDir, "actual_perturbed.csv");

            WriteMatrixCsv(identityPath, Matrix4x4.Identity);
            WriteMatrixCsv(productPath, product);
            WriteMatrixCsv(perturbedPath, product, perturb: 1e-3);

            bool ok = true;

            // 用例 1：M * M⁻¹ vs I —— 应 PASS（≤1e-6）
            var pass1 = DiffRunner.Run(DiffMode.Matrix, identityPath, productPath, 1e-6);
            Console.WriteLine($"[case1] M*M^-1 == I (threshold=1e-6): {pass1.Result}, maxError={pass1.MaxError:G6}, pass={pass1.Passed}/{pass1.Total}");
            ok &= pass1.Result == "PASS" && pass1.MaxError <= 1e-6;

            // 用例 2：扰动 1e-3 vs I —— 应 FAIL（>1e-6），证明工具能检出差异
            var fail1 = DiffRunner.Run(DiffMode.Matrix, identityPath, perturbedPath, 1e-6);
            Console.WriteLine($"[case2] perturbed(1e-3) vs I (threshold=1e-6): {fail1.Result}, maxError={fail1.MaxError:G6}, fail={fail1.Failed}/{fail1.Total}");
            ok &= fail1.Result == "FAIL" && fail1.Failed > 0;

            // 清理临时文件
            try { Directory.Delete(tmpDir, true); } catch { /* 忽略 */ }

            Console.WriteLine(ok ? "=== 自测通过 ✅ ===" : "=== 自测失败 ❌ ===");
            return ok;
        }

        private static void WriteMatrixCsv(string path, Matrix4x4 m, double perturb = 0)
        {
            // 按行写入 4x4；可选扰动以模拟差异
            float[,] a = ToArray(m);
            using (var sw = new StreamWriter(path))
            {
                for (int r = 0; r < 4; r++)
                {
                    var cells = new string[4];
                    for (int c = 0; c < 4; c++)
                    {
                        double v = a[r, c] + (r == 0 && c == 0 ? perturb : 0);
                        cells[c] = v.ToString("G17", CultureInfo.InvariantCulture);
                    }
                    sw.WriteLine(string.Join(",", cells));
                }
            }
        }

        private static float[,] ToArray(Matrix4x4 m)
        {
            // System.Numerics.Matrix4x4 是行主序，按行展开
            return new float[,]
            {
                { m.M11, m.M12, m.M13, m.M14 },
                { m.M21, m.M22, m.M23, m.M24 },
                { m.M31, m.M32, m.M33, m.M34 },
                { m.M41, m.M42, m.M43, m.M44 }
            };
        }
    }
}
