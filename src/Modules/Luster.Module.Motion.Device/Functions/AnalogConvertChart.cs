using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Interfaces;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Logics;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Logic;
using Microsoft.Msagl.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TaiKeCommon;

namespace Luster.Module.Motion.Device.Functions
{
    public class AnalogConvertChart : MotionFunction
    {
        [NotEmpty]
        [Parameter("选择对应的模拟量IO", 1, CN = "模拟量IO", EditorType = typeof(VIO))]
        public VDevice Device { get; set; }

        [Parameter("乘比例", 4, CN = "乘的比例", DefaultV = 1.0)]
        public double RatioCG { get; set; }

        [Parameter("除比例", 4, CN = "除的比例", DefaultV = 1.0)]
        public double RatioCU { get; set; }

        [Parameter("停止采集全局变量", 15, CN = "停止采集变量", EditorType = typeof(IGlobal))]
        public string GlobalVar { get; set; }

        [Parameter("采集时间间隔(ms)", 14, CN = "采集时间间隔", DefaultV = 100)]
        public int InstallTime { get; set; }

        [Parameter("SN", 16, CN = "变量值", CanRef = ParamRef.Ref, DefaultV = "")]
        public string GStringVal { get; set; }

        private volatile bool _isBreak;

        private IMotionModule gModule;
        private ParameterAttribute gParameter;
        //按压数据
        private System.Collections.Generic.List<double> pressureSamples;

        public AnalogConvertChart()
        {
            this.Tips = "设备模拟量获取并记录进csv文件";
            this.Icon = "\xe67f";
        }

        public void SaveFile()
        {
            DateTime now = DateTime.Now;
            string dateStr = now.ToString("yyyyMMdd");
            string timeStr = now.ToString("HHmmss");
            string FileDir = @"D:\力控数据存储\" + dateStr + "\\"+ timeStr + "\\";
            string filename = GStringVal + ".csv";
            string picName = GStringVal;
            CRecordValue recordValuePress = new CRecordValue();
            string title = "No" + "," + "Time" + "," + "Press" + "," + "Position";
            string value = "";
            //PM要求最后抬起来要添加点数据，好看
            //for (int i = 0; i < 5; i++)
            //{
            //    Double presstemp = OutPressure- Convert.ToInt32(OutPressure/5*i);
            //    pressureSamples.Add(presstemp);
            //}
            int pressindex = 0;
            for (int i = 0; i < pressureSamples.Count; i++)
            {
                int num = i + 1;
                int timenum = num * InstallTime;
                double press = pressureSamples[i]>=0? pressureSamples[i]:0;
                double position = 0;
                value = num + "," + timenum + "," + press + "," + position;
                recordValuePress.RecordValue(FileDir, filename, title, value);
                pressindex = i;
            }
            try
            {
                if (pressureSamples.Count > 0)
                {
                    double[] timeArr = new double[pressureSamples.Count];
                    double[] pressArr = new double[pressureSamples.Count];
                    double[] posArr = new double[pressureSamples.Count];
                    for (int i = 0; i < pressureSamples.Count; i++)
                    {
                        timeArr[i] = (i + 1) * InstallTime;
                        pressArr[i] = pressureSamples[i] >= 0 ? pressureSamples[i] : 0;
                        posArr[i] = 0;
                    }
                    TorqueChart torqueChart = new TorqueChart();
                    torqueChart.SavePressureCurveImage(timeArr, pressArr, posArr, FileDir, picName);
                }
            }
            catch (Exception ex)
            {
                MyOwner.OnLog(LogType.Debug, $"模块 {MyOwner.Alias} 曲线图生成失败: {ex.Message}");
            }
        }
        public override bool DoExcute(out string errMsg)
        {
            _isBreak = false;

            if (gModule == null)
            {
                gModule = MyOwner.TaskModules[GlobalModule.GlobalID] as IMotionModule;
            }

            pressureSamples = new System.Collections.Generic.List<double>();
            // 空跑模式直接返回
            if (IsEmptyMode)
            {
                errMsg = "";
                return true;
            }

            var io = MyOwner.DeviceEngine.GetVirtualByID(Device.DeviceID) as VIO;
            while (true)
            {
                if (_isBreak) break;

                if (gParameter == null)
                {
                    if (gModule != null && gModule.Parameters.ContainsKey(GlobalVar))
                    {
                        gParameter = gModule.Parameters[GlobalVar];
                    }
                    else
                    {
                        MyOwner.OnLog(LogType.Debug, $"全局变量:{GlobalVar}不存在!");
                        break;
                    }
                }

                object pVal = gParameter?.Value;
                if (pVal != null && pVal.Equals(true))
                {
                    MyOwner.OnLog(LogType.Debug, $"模块 {MyOwner.Alias} 压力采集: 全局变量触发停止");
                    break;
                }
                // 实时采集压力
                double CurentCardAnalogValue = io.GetAnglogIn();
                double CurrentDeviceRealValue = Math.Round(CurentCardAnalogValue * RatioCG / RatioCU, 3);
                pressureSamples.Add(CurrentDeviceRealValue);    
                Thread.Sleep(InstallTime);
            }
            //结束后写入csv
            SaveFile();
            return base.DoExcute(out errMsg);
        }

        #region 停止/暂停

        public override void Stop()
        {
            _isBreak = true;
        }
        #endregion
    }
}