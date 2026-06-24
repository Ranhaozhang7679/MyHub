using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using Luster.Tools.DiffRegression.Differ;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Luster.Tools.DiffRegression
{
    /// <summary>
    /// 系统级 diff 回归工具入口。
    /// <para>用法：<c>--baseline &lt;基线文件&gt; --actual &lt;实际输出&gt; --threshold &lt;阈值&gt; [--mode matrix|cali|detect|ct] [--self-test]</c></para>
    /// <para>用途：迁移后 lmv-2026 输出与源端 SP-2025140 基线比对，验证五轴正逆解 / 标定 / 检测 / CT 节拍一致性。</para>
    /// <para>非侵入：独立工具工程，不引用主干节点/Service，仅挂载到 sln。</para>
    /// </summary>
    internal static class Program
    {
        private const int ExitPass = 0;
        private const int ExitFail = 1;
        private const int ExitError = 2;

        private static int Main(string[] args)
        {
            try
            {
                var opts = Options.Parse(args);
                if (opts.ShowHelp)
                {
                    PrintHelp();
                    return ExitPass;
                }

                if (opts.SelfTest)
                {
                    return SelfTest.Run() ? ExitPass : ExitFail;
                }

                if (string.IsNullOrEmpty(opts.Baseline) || string.IsNullOrEmpty(opts.Actual))
                {
                    Console.Error.WriteLine("[error] 缺少必填参数 --baseline / --actual（或使用 --self-test 自测）。");
                    PrintHelp();
                    return ExitError;
                }

                DiffMode mode = ParseMode(opts.Mode);
                double threshold = opts.Threshold ?? DefaultThreshold(mode);

                var report = DiffRunner.Run(mode, opts.Baseline, opts.Actual, threshold);
                Console.WriteLine(report.ToJson(Formatting.Indented));
                return report.Result == "PASS" ? ExitPass : ExitFail;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[error] {ex.Message}");
                return ExitError;
            }
        }

        /// <summary>各 mode 默认阈值：matrix 绝对 1e-6，ct 相对 5%，cali/detect 绝对 1e-6。</summary>
        private static double DefaultThreshold(DiffMode mode)
        {
            switch (mode)
            {
                case DiffMode.Matrix: return 1e-6;
                case DiffMode.Ct: return 0.05; // ±5%
                case DiffMode.Cali: return 1e-6;
                case DiffMode.Detect: return 1e-6;
                default: throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }

        private static DiffMode ParseMode(string mode)
        {
            if (string.IsNullOrWhiteSpace(mode))
            {
                throw new ArgumentException("缺少 --mode 参数（matrix|cali|detect|ct）。");
            }
            switch (mode.ToLowerInvariant())
            {
                case "matrix": return DiffMode.Matrix;
                case "cali": return DiffMode.Cali;
                case "detect": return DiffMode.Detect;
                case "ct": return DiffMode.Ct;
                default:
                    throw new ArgumentException($"不支持的 mode: {mode}（应为 matrix|cali|detect|ct）。");
            }
        }

        private static void PrintHelp()
        {
            Console.WriteLine("Luster.Tools.DiffRegression — 系统级 diff 回归工具");
            Console.WriteLine("用法:");
            Console.WriteLine("  Luster.Tools.DiffRegression --baseline <基线> --actual <实际> --threshold <阈值> --mode matrix|cali|detect|ct");
            Console.WriteLine("  Luster.Tools.DiffRegression --self-test");
            Console.WriteLine("选项:");
            Console.WriteLine("  --baseline <file>   基线文件路径（.csv / .json）");
            Console.WriteLine("  --actual   <file>   实际输出文件路径（.csv / .json）");
            Console.WriteLine("  --threshold <num>   比对阈值：matrix/cali/detect 为绝对误差，ct 为相对误差");
            Console.WriteLine("  --mode <m>          比对模式：matrix(正逆解矩阵) | cali(标定参数) | detect(检测判定) | ct(节拍)");
            Console.WriteLine("  --self-test         使用 Coord5Axis 不变式数据自测工具可用性");
            Console.WriteLine("  --help              显示帮助");
            Console.WriteLine("退出码: 0=PASS, 1=FAIL, 2=ERROR");
        }
    }

    /// <summary>命令行选项。</summary>
    internal sealed class Options
    {
        public string Baseline { get; private set; }
        public string Actual { get; private set; }
        public double? Threshold { get; private set; }
        public string Mode { get; private set; }
        public bool SelfTest { get; private set; }
        public bool ShowHelp { get; private set; }

        public static Options Parse(string[] args)
        {
            var o = new Options();
            for (int i = 0; i < args.Length; i++)
            {
                string a = args[i];
                switch (a)
                {
                    case "--baseline":
                    case "-b":
                        o.Baseline = Next(args, ref i, a);
                        break;
                    case "--actual":
                    case "-a":
                        o.Actual = Next(args, ref i, a);
                        break;
                    case "--threshold":
                    case "-t":
                        o.Threshold = double.Parse(Next(args, ref i, a), CultureInfo.InvariantCulture);
                        break;
                    case "--mode":
                    case "-m":
                        o.Mode = Next(args, ref i, a);
                        break;
                    case "--self-test":
                        o.SelfTest = true;
                        break;
                    case "--help":
                    case "-h":
                        o.ShowHelp = true;
                        break;
                    default:
                        throw new ArgumentException($"未知参数: {a}");
                }
            }
            return o;
        }

        private static string Next(string[] args, ref int i, string flag)
        {
            if (i + 1 >= args.Length)
            {
                throw new ArgumentException($"参数 {flag} 缺少值。");
            }
            return args[++i];
        }
    }

    /// <summary>比对模式。</summary>
    public enum DiffMode
    {
        /// <summary>正逆解矩阵（元素绝对误差）。</summary>
        Matrix,
        /// <summary>标定参数（字段绝对误差）。</summary>
        Cali,
        /// <summary>检测判定（分类字段精确匹配 + 数值字段绝对误差）。</summary>
        Detect,
        /// <summary>CT 节拍（数值相对误差）。</summary>
        Ct
    }
}
