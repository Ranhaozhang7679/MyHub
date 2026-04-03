using Luster.Common.DataStruct.Attributes;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Motion.interfaces;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Luster.Common.DataStruct.DataModels;
using Luster.TaskFlow.Common.Models;
using System.IO;
using Luster.Common.DataStruct.Extensions;

namespace Luster.Module.Motion.DataProc.Functions
{
    /// <summary>
    /// 处理类型
    /// </summary>
    public enum KBActionType
    {
        [Description("插入数据")]
        InsertData,

        [Description("获取最小二乘法斜率截距")]
        GetLeastSquaresKB,

        [Description("清空数据")]
        Clear,
    }

    /// <summary>
    /// 求取平均值
    /// </summary>
    public class GetSlopeIntercept : MotionFunction, IHomeFunction
    {
        /// <summary>
        /// 存放平均值样本的字典
        /// </summary>
        private static Dictionary<string, Queue<(double x, double y)>> DicAverage = new Dictionary<string, Queue<(double, double)>>();

        private static readonly Object _lock = new Object();

        [Parameter("处理方式", 0, CN = "处理方式")]
        public KBActionType excuteType { get; set; }

        [Parameter("保留多少个数据", 1, CN = "个数", DefaultV = 5)]
        public int Number { get; set; }

        [Parameter("数据名称,不可重复", 2, CN = "数据名称")]
        public string DataName { get; set; }

        [NotEmpty]
        [DependOn("excuteType", KBActionType.InsertData)]
        [Parameter("参考值", 3, CN = "参考值", CanRef = ParamRef.Ref)]
        public double ReferenceValue { get; set; }

        [NotEmpty]
        [DependOn("excuteType", KBActionType.InsertData)]
        [Parameter("实测值", 3, CN = "实测值", CanRef = ParamRef.Ref)]
        public double MeasuredValue { get; set; }

        [DependOn("excuteType", KBActionType.InsertData)]
        [Parameter("校验数据，低于最小值则不插入", 4, CN = "最小值", DefaultV = -10)]
        public double MinValue { get; set; }

        [DependOn("excuteType", KBActionType.InsertData)]
        [Parameter("校验数据，高于最大值则不插入", 4, CN = "最大值", DefaultV = 10)]
        public double MaxValue { get; set; }

        [DependOn("excuteType", KBActionType.GetLeastSquaresKB)]
        [Parameter("斜率", 5, CN = "斜率", ParamType = ParamType.OUT)]
        public double OutK { get; set; }

        [DependOn("excuteType", KBActionType.GetLeastSquaresKB)]
        [Parameter("截距", 5, CN = "截距", ParamType = ParamType.OUT)]
        public double OutB { get; set; }

        [DependOn("excuteType", KBActionType.GetLeastSquaresKB)]
        [Parameter("拟合度", 5, CN = "拟合度", ParamType = ParamType.OUT)]
        public double OutR2 { get; set; }


        public GetSlopeIntercept()
        {
            this.Icon = "\xe622";
            this.Tips = "斜率截距";
        }

        public override bool DoExcute(out string errMsg)
        {
            errMsg = string.Empty;

            ///加锁防止同时读写
            lock (_lock)
            {
                ///插入数据
                if (excuteType == KBActionType.InsertData)
                {
                    if (!CheckDataIsValid(MinValue, MaxValue, ReferenceValue))
                    {
                        //errMsg = "数据异常,不纳入样本";
                        MyOwner.OnLog(Common.DataStruct.Enums.LogType.Warning, $"最大值:{MaxValue},最小值:{MinValue}," +
                            $"当前值:{ReferenceValue},数据非法不计入样本范围");
                        return true;
                    }

                    if (!CheckDataIsValid(MinValue, MaxValue, MeasuredValue))
                    {
                        //errMsg = "数据异常,不纳入样本";
                        MyOwner.OnLog(Common.DataStruct.Enums.LogType.Warning, $"最大值:{MaxValue},最小值:{MinValue}," +
                            $"当前值:{MeasuredValue},数据非法不计入样本范围");
                        return true;
                    }

                    ///判断是否存在key不存在则创建
                    if (!DicAverage.ContainsKey(DataName))
                    {
                        DicAverage.Add(DataName, new Queue<(double x, double y)>());
                    }
                    ///超过样本容量移除开头的数据
                    else if (DicAverage[DataName].Count >= Number)
                    {
                        DicAverage[DataName].Dequeue();
                    }
                    ///末尾插入最新的数据
                    DicAverage[DataName].Enqueue((MeasuredValue, ReferenceValue));
                }
                else if (excuteType == KBActionType.Clear)
                {
                    if (!DicAverage.ContainsKey(DataName))
                    {
                        ///第一次没有值,返回0
                        OutK = 0;
                        OutB = 0;
                        OutR2 = 0;
                    }
                    else
                    {
                        DicAverage[DataName].Clear();
                    }
                }
                else if (excuteType == KBActionType.GetLeastSquaresKB)
                {
                    if (!DicAverage.ContainsKey(DataName))
                    {
                        ///第一次没有值,返回0
                        OutK = 0;
                        OutB = 0;
                        OutR2 = 0;
                    }
                    else
                    {
                        var slopeIntercept = LinearFit(DicAverage[DataName].ToList());
                        OutK = slopeIntercept.K;
                        OutB = slopeIntercept.B;
                        OutR2 = slopeIntercept.R2;
                    }
                }
            }
            return base.DoExcute(out errMsg);
        }

        private bool CheckDataIsValid(double MinValue, double MaxValue, double CurValue)
        {
            if (MinValue >= MaxValue)
            {
                throw new Exception("数据非法!最大值不应小于最小值!");
            }
            if (MinValue <= CurValue && CurValue <= MaxValue)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 最小二乘法线性拟合 y = kx + b
        /// </summary>
        private (double K, double B, double R2) LinearFit(List<(double, double)> data)
        {
            int n = data.Count;
            double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;

            if (n < 2)
            {
                return (0, 0, 0);
            }

            foreach (var (x, y) in data)
            {
                sumX += x;
                sumY += y;
                sumXY += x * y;
                sumX2 += x * x;
            }

            double denominator = n * sumX2 - sumX * sumX;
            if (Math.Abs(denominator) < 1e-10) return (0, 0, 0);  // 防止除零

            double K = (n * sumXY - sumX * sumY) / denominator;
            double B = (sumY - K * sumX) / n;
            double R2 = CalculateR2(data, K, B);

            return (K, B, R2);
        }

        /// <summary>
        /// 计算R²
        /// </summary>
        private double CalculateR2(List<(double meas, double reference)> data, double K, double B)
        {
            double meanY = data.Average(p => p.reference);

            double ssTot = 0;
            double ssRes = 0;

            foreach (var (x, y) in data)
            {
                double yFit = K * x + B;

                ssTot += Math.Pow(y - meanY, 2);
                ssRes += Math.Pow(y - yFit, 2);
            }

            return 1 - (ssRes / ssTot);
        }

    }
}
