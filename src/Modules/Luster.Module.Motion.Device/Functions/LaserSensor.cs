using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.DataModels;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Device.Functions
{
    public class LaserSensor : MotionFunction
    {
        [Range(1, 100)]
        [Parameter("要对多少个激光传感器进行检查", 0, CN = "数量", DefaultV = 1)]
        public int LaserSensorNum { get; set; }

        [NotEmpty]
        [Parameter("传感器1黄金位置对应的读值", 1, CN = "偏移值1", DefaultV = 0)]
        public double Offset1 { get; set; }

        [NotEmpty]
        [Parameter("传感器2黄金位置对应的读值", 1, CN = "偏移值2", DefaultV = 0)]
        public double Offset2 { get; set; }


        [NotEmpty]
        [Parameter("传感器3黄金位置对应的读值", 1, CN = "偏移值3", DefaultV = 0)]
        public double Offset3 { get; set; }

        [NotEmpty]
        [Range(-100000, 100000)]
        [Parameter("1mm对应的增量值,不能设置0", 2, CN = "比例值", DefaultV = 1)]
        public double Ratio { get; set; }


        [NotEmpty]
        [Parameter("超差上限值", 3, CN = "阈值", DefaultV = 1)]
        public double ThresholdValue { get; set; }

        [NotEmpty]
        [Parameter("是否使用原始值", 4, CN = "原始模拟量", DefaultV = true)]
        public bool IsOriginal { get; set; }

        [NotEmpty]
        [Parameter("IO类型，通过关键字搜索", 4, CN = "IO名称", EditorType = typeof(VIO))]
        public VDevice Device { get; set; }

        [NotEmpty]
        [Limit(1, 5)]
        [Parameter("多次读取次数", 15, CN = "多次读取", DefaultV =1)]
        public int Times { get; set; }

        [NotEmpty]
        [Parameter("多次读取时间间隔", 16, CN = "间隔", DefaultV = 100)]
        public int Interval { get; set; }

        [Parameter("判定结果", 20, CN = "结果", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool Result { get; set; }

        [Parameter("高度值1，单位为mm", 20, CN = "高度值1", ParamType = TaskFlow.Common.Enums.ParamType.OUT, DefaultV = 0)]
        public double Height1 { get; set; }

        [Parameter("高度值2，单位为mm", 21, CN = "高度值2", ParamType = TaskFlow.Common.Enums.ParamType.OUT, DefaultV = 0)]
        public double Height2 { get; set; }

        [Parameter("高度值3，单位为mm", 22, CN = "高度值3", ParamType = TaskFlow.Common.Enums.ParamType.OUT, DefaultV = 0)]
        public double Height3 { get; set; }

        [Parameter("最大值-最小值的绝对值 mm", 23, CN = "高度差值", ParamType = TaskFlow.Common.Enums.ParamType.OUT, DefaultV = 0)]
        public double HeightDiff { get; set; }

        [Parameter("实际次数", 24, CN = "实际测高次数", ParamType = TaskFlow.Common.Enums.ParamType.OUT, DefaultV = 1)]
        public int ActualTime { get; set; }

        public LaserSensor()
        {
            this.Tips = "激光测高传感器";
            this.Icon = "\xe67f";
        }

        public override bool DoExcute(out string errMsg)
        {
            errMsg = "";
            Result = true;
            Height1 = 0;
            Height2 = 0;
            Height3 = 0;
            HeightDiff = 0;

            // 空跑模式直接返回
            if (IsEmptyMode)
            {
                return true;
            }

            //定义变量
            //模拟量值
            errMsg = "";
            double analogValue;
            var ioDevices = GetParametersByType<VDevice>();

            //0.获取设备
            List<VIO> ios = new List<VIO>();
            foreach (var item in ioDevices)
            {
                GetVDevice<VIO>(item, out var io);
                ios.Add(io);
            }

            // 1.读取模拟量值
            Result = true;
            for(int j=0; j<Times; j++)
            {
                Result = true;
                for (int i = 0; i < ios.Count; i++)
                {
                    analogValue = ios[i].GetAnglogIn();
                    if (i == 0)
                    {
                        Height1 = Math.Round((analogValue - Offset1) / Ratio, 3);
                        if (!IsOriginal)
                        {
                            Height1 = Math.Round(ios[i].GetAnalogExpVal(), 3);
                        }

                        if (Math.Abs(Height1) >= ThresholdValue)
                        {
                            Result = false;
                        }

                        MyOwner.OnLog(LogType.Debug, $"高度检测传感器1 Height1({Height1})>={ThresholdValue} Result={Result}");
                    }
                    if (i == 1)
                    {
                        Height2 = Math.Round((analogValue - Offset2) / Ratio, 3);
                        if (!IsOriginal)
                        {
                            Height2 = Math.Round(ios[i].GetAnalogExpVal(), 3);
                        }
                        if (Math.Abs(Height2) >= ThresholdValue)
                        {
                            Result = false;
                        }

                        MyOwner.OnLog(LogType.Debug, $"高度检测传感器2 Height2({Height2})>={ThresholdValue} Result={Result}");
                    }
                    if (i == 2)
                    {
                        Height3 = Math.Round((analogValue - Offset3) / Ratio, 3);
                        if (!IsOriginal)
                        {
                            Height3 = Math.Round(ios[i].GetAnalogExpVal(), 3);
                        }
                        if (Math.Abs(Height3) >= ThresholdValue)
                        {
                            Result = false;
                        }

                        MyOwner.OnLog(LogType.Debug, $"高度检测传感器3 Height3({Height3})>={ThresholdValue} Result={Result}");
                    }
                }

                MyOwner.OnLog(LogType.Info, $"第{j+1}次读取高度值");
                ActualTime = j + 1;
                //如果成功，直接跳出
                if (Result)
                {
                    break;
                }
                else if(j+1<Times)
                {
                    Thread.Sleep(100);
                }
            }
            

            // 2.高度差
            HeightDiff = Math.Abs(Math.Max(Math.Max(Height1, Height2), Height3) - Math.Min(Math.Min(Height1, Height2), Height3));
            Result= HeightDiff<=ThresholdValue? true : false;

            //3.输出结果
            return base.DoExcute(out errMsg);
        }


        public override void OnNotifyPropertyUIChanged(ParameterAttribute parameter, object newV)
        {
            base.OnNotifyPropertyUIChanged(parameter, newV);
            if (parameter.Name == "LaserSensorNum" && int.TryParse(newV?.ToString(), out var num) && num >= 1)
            {
                BuildExternParameters(1, num, "Device", "IO设备", typeof(VDevice), (p) =>
                {
                    p.EditorType = typeof(VIO);
                });
            }
        }
    }
}
