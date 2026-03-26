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

namespace Luster.Module.Motion.DataProc.Functions
{
    /// <summary>
    /// 处理类型
    /// </summary>
    public enum ExcuteType
    {
        [Description("插入数据")]
        InsertData,

        [Description("获取平均值")]
        GetAverage,

        [Description("获取最大值")]
        GetMax,

        [Description("获取最小值")]
        GetMin,

        [Description("获取标准差")]
        GetStandardDeviation,

        [Description("清空数据")]
        Clear,
    }

    /// <summary>
    /// 求取平均值
    /// </summary>
    public class GetAverage: MotionFunction, IHomeFunction
    {
        /// <summary>
        /// 存放平均值样本的字典
        /// </summary>
        private static Dictionary<string,Queue<double>> DicAverage=new Dictionary<string, Queue<double>>();

        private static readonly Object _lock=new Object();

        [Parameter("处理方式",0,CN ="处理方式")]
        public ExcuteType excuteType { get; set; }

        [Parameter("保留多少个数据",1,CN ="个数",DefaultV =5)]
        public int Number { get; set; }

        [Parameter("数据名称,不可重复",2,CN ="数据名称")]
        public string DataName { get; set; }

        [NotEmpty]
        [DependOn("excuteType",ExcuteType.InsertData)]
        [Parameter("数据来源",3,CN ="数据来源",CanRef =ParamRef.Ref)]
        public double DataSource { get; set; }

        [DependOn("excuteType", ExcuteType.InsertData)]
        [Parameter("校验数据，低于最小值则不插入", 4, CN = "最小值",DefaultV =-0.2)]
        public double MinValue { get; set; }

        [DependOn("excuteType", ExcuteType.InsertData)]
        [Parameter("校验数据，高于最大值则不插入", 4, CN = "最大值",DefaultV =0.2)]
        public double MaxValue { get; set; }

        [Parameter("数据输出", 5, CN = "数据输出", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public double OutData { get; set; }


        public GetAverage() 
        {
            this.Icon = "\xe622";
            this.Tips = "平均值";
        }

        public override bool DoExcute(out string errMsg)
        {
            errMsg = string.Empty;

            ///加锁防止同时读写
            lock (_lock)
            {
                ///插入数据
                if (excuteType == ExcuteType.InsertData)
                {
                    if (!CheckDataIsValid(MinValue, MaxValue, DataSource))
                    {
                        //errMsg = "数据异常,不纳入样本";
                        MyOwner.OnLog(Common.DataStruct.Enums.LogType.Warning, $"最大值:{MaxValue},最小值:{MinValue}," +
                            $"当前值:{DataSource},数据非法不计入样本范围");
                        return true;
                    }

                    ///判断是否存在key不存在则创建
                    if (!DicAverage.ContainsKey(DataName))
                    {
                        DicAverage.Add(DataName, new Queue<double>());
                    }
                    ///超过样本容量移除开头的数据
                    else if (DicAverage[DataName].Count >= Number)
                    {
                        DicAverage[DataName].Dequeue();
                    }
                    ///末尾插入最新的数据
                    DicAverage[DataName].Enqueue(DataSource);
                }
                else if (excuteType == ExcuteType.GetAverage)
                {
                    if (!DicAverage.ContainsKey(DataName))
                    {
                        ///第一次没有值,返回0
                        OutData = 0;
                    }
                    else
                    {
                        double sum = DicAverage[DataName].Sum(x => x);
                        double average = OutData = DicAverage[DataName].Average();
                    }
                }
                else if (excuteType == ExcuteType.GetMax)
                {
                    if (!DicAverage.ContainsKey(DataName))
                    {
                        ///第一次没有值,返回0
                        OutData = 0;
                    }
                    else
                    {
                        OutData = DicAverage[DataName].Max();
                    }
                }
                else if (excuteType == ExcuteType.GetMin)
                {
                    if (!DicAverage.ContainsKey(DataName))
                    {
                        ///第一次没有值,返回0
                        OutData = 0;
                    }
                    else
                    {
                        OutData = DicAverage[DataName].Min();
                    }
                }
                else if (excuteType == ExcuteType.GetStandardDeviation)
                {
                    if (!DicAverage.ContainsKey(DataName))
                    {
                        ///第一次没有值,返回0
                        OutData = 0;
                    }
                    else
                    {
                        OutData = CalculateStandardDeviation(DicAverage[DataName].ToArray());
                    }
                }
                else if (excuteType == ExcuteType.Clear)
                {
                    if (!DicAverage.ContainsKey(DataName))
                    {
                        ///第一次没有值,返回0
                        OutData = 0;
                    }
                    else
                    {
                        DicAverage[DataName].Clear();
                    }
                }
            }
            return base.DoExcute(out errMsg);
        }

        private bool CheckDataIsValid(double MinValue,double MaxValue,double CurValue)
        {
            if (MinValue >= MaxValue)
            {
                throw new Exception("数据非法!最大值不应小于最小值!");
            }
            if(MinValue<=CurValue&&CurValue<=MaxValue)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        static double CalculateStandardDeviation(double[] data)
        {
            int n = data.Length;
            if (n <= 1)
            {
                throw new InvalidOperationException("数量数量必须大于1");
            }

            // 计算平均值
            double mean = data.Average();

            // 计算平方差和
            double sumOfSquares = data.Sum(u => Math.Pow(u - mean, 2));

            // 方差除以(n - 1)
            double variance = sumOfSquares / (n - 1);

            // 返回标准差（方差的平方根）
            return Math.Sqrt(variance);
        }

    }
}
