using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Interfaces;
using Luster.TaskFlow.Common.Logics;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Device.Functions
{
    public class DHCalcu : MotionFunction
    {
        [Parameter("停止添加并计算", 0, CN = "停止添加并计算", EditorType = typeof(IGlobal))]
        public string GlobalVar { get; set; }

        [Parameter("寻力扭矩设置", 1, CN = "寻力扭矩设置", CanRef = ParamRef.Ref, DefaultV = 0)]
        public int TorqueLimit { get; set; }

        [Parameter("实际压力", 2, CN = "实际压力", CanRef = ParamRef.Ref, DefaultV = 0)]
        public double Pressure { get; set; }

        [Parameter("执行结果", 10, CN = "执行结果", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool OutResult { get; set; }

        [Parameter("拟合K输出", 11, CN = "拟合K输出", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public double OutPutK { get; set; }

        [Parameter("拟合B输出", 12, CN = "拟合B输出", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public double OutPutB { get; set; }

        [Parameter("拟合系数输出(越靠近1越好)", 12, CN = "拟合系数输出", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public double OutPutX { get; set; }

        private IMotionModule gModule;
        private ParameterAttribute gParameter;

        private List<int> TorqueList = new List<int>();
        private List<double> PressList = new List<double>();
        public DHCalcu()
        {
            this.Icon = "\xe622";
            this.Tips = "拟合计算大寰K和B";
        }

        public override bool DoExcute(out string errMsg)
        {
            errMsg = "";
            OutResult = false;
            // 获取全局模块
            if (gModule == null)
            {
                gModule = MyOwner.TaskModules[GlobalModule.GlobalID] as IMotionModule;
            }
            if (gParameter == null)
            {
                if (gModule != null && gModule.Parameters.ContainsKey(GlobalVar))
                {
                    gParameter = gModule.Parameters[GlobalVar];
                }
                else
                {
                    MyOwner.OnLog(LogType.Debug, $"全局变量:{GlobalVar}不存在!");
                    return false;
                }
            }

            object pVal = gParameter?.Value;
            if (pVal != null && pVal.Equals(true))
            {
                double K = 0;
                double B = 0;
                double X = 0;
                TryFit(TorqueList.ToArray(), PressList.ToArray(),out K,out B,out X);
                OutResult = true;
                OutPutK = K;
                OutPutB=B;
                OutPutX = X;
                //计算结束后清除
                TorqueList = new List<int>();
                PressList = new List<double>();
            }
            else
            {
                TorqueList.Add(TorqueLimit);
                PressList.Add(Pressure);
            }
                return base.DoExcute(out errMsg);
        }

        /// <summary>
        /// 使用最小二乘法对 (I, F) 数据点进行线性拟合
        /// </summary>
        /// <param name="I">自变量数组</param>
        /// <param name="F">因变量数组</param>
        /// <param name="K">输出的斜率</param>
        /// <param name="B">输出的截距</param>
        /// <param name="rSquared">输出的决定系数 R²（越接近 1 拟合越好）</param>
        /// <returns>拟合是否成功</returns>
        public static bool TryFit(int[] I, double[] F,
                                  out double K, out double B, out double rSquared)
        {
            K = 0;
            B = 0;
            rSquared = 0;

            // 输入校验：非空、长度一致、至少 2 个点
            if (I == null || F == null || I.Length != F.Length || I.Length < 2)
                return false;

            int n = I.Length;

            // 计算累加和：ΣI、ΣF、Σ(I*F)、Σ(I²)
            double sumI = 0, sumF = 0, sumIF = 0, sumI2 = 0;
            for (int i = 0; i < n; i++)
            {
                sumI += I[i];
                sumF += F[i];
                sumIF += I[i] * F[i];
                sumI2 += I[i] * I[i];
            }

            // 分母：n*Σ(I²) - (ΣI)²
            double denominator = n * sumI2 - sumI * sumI;
            if (Math.Abs(denominator) < 1e-12)
            {
                // 所有 I 相同，无法计算斜率
                return false;
            }

            // 最小二乘法公式
            K = (n * sumIF - sumI * sumF) / denominator;
            B = (sumF - K * sumI) / n;

            // 计算 R²（决定系数）
            double meanF = sumF / n;
            double ssTot = 0, ssRes = 0;
            for (int i = 0; i < n; i++)
            {
                double predicted = K * I[i] + B;
                ssTot += (F[i] - meanF) * (F[i] - meanF);
                ssRes += (F[i] - predicted) * (F[i] - predicted);
            }
            rSquared = Math.Abs(ssTot) < 1e-12 ? 1.0 : 1.0 - ssRes / ssTot;

            return true;
        }
    }
}
