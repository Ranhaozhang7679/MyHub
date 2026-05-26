using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.VDevice;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Logics;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Functions;
using Luster.TaskFlow.Motion.Logic;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TaiKeCommon;

namespace Luster.Module.Motion.Business.Functions
{
    public class PressurizeChart : GoToFunction
    {
        [NotEmpty]
        [Parameter("轴的点位及对应的参数配置", 0, CN = "点位参数", MultiValues = false)]
        public VAxisPos AxisPos { get; set; }

        [NotEmpty]
        [Parameter("带轴运动参数配置", 1, CN = "是否带轴运动", DefaultV = true)]
        public bool HasAxisPos { get; set; }

        [NotEmpty]
        [Parameter("通信设备", 2, CN = "通信设备", EditorType = typeof(VCommuncation))]
        public VDevice CommDevice { get; set; }

        public int PressureAddress { get; set; }

        [Parameter("从站设备", 3, CN = "设备地址", CanRef = ParamRef.NoRef)]
        public SocketAction Slave { get; set; }

        [NotEmpty]
        [Parameter("选择对应的模拟量IO", 4, CN = "模拟量IO", EditorType = typeof(VIO))]
        public VDevice Device { get; set; }

        [Parameter("乘比例", 5, CN = "乘的比例", DefaultV = 1.0)]
        public double RatioCG { get; set; }

        [Parameter("除比例", 6, CN = "除的比例", DefaultV = 1.0)]
        public double RatioCU { get; set; }

        [Parameter("采集时间间隔(ms)", 7, CN = "采集时间间隔", DefaultV = 100)]
        public int InstallTime { get; set; }

        [Parameter("SN", 8, CN = "变量值", CanRef = ParamRef.Ref, DefaultV = "")]
        public string GStringVal { get; set; }


        [NotEmpty]
        [Parameter("IO类型，通过关键字搜索", 9, CN = "保压气缸", EditorType = typeof(VCylinder))]
        public VDevice Cylinder { get; set; }

        [NotEmpty]
        [Parameter("气缸动作超时设定", 10, CN = "气缸超时", DefaultV = 5000)]
        public int CylinderTime { get; set; }

        [NotEmpty]
        [Parameter("气缸保压时间", 11, CN = "保压时间", DefaultV = 3000)]
        public int PressureTime { get; set; }

        [NotEmpty]
        [Parameter("最大压力值", 12, CN = "最大压力值", DefaultV = 200)]
        public double OffsetValue { get; set; }

        [Parameter("延时读取压力值", 13, CN = "延时读取", DefaultV = 0.2)]
        public int DelayTime { get; set; }

        [NotEmpty]
        [Parameter("压力超限提示", 14, CN = "超限提示", DefaultV = false)]
        public bool IsShowAlarm { get; set; }

        [Parameter("真实保压值", 20, CN = "真实保压值", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public double PressurizeVal { get; set; }

        [Parameter("气缸保压时间输出,单位为S", 21, CN = "保压时间", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public double PressureTimeOut { get; set; }

        [Parameter("气缸伸出时间输出,单位为S", 22, CN = "气缸伸出时间", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public double CylinderTTimeOut { get; set; }

        [Parameter("气缸缩回时间输出,单位为S", 23, CN = "气缸缩回时间", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public double CylinderFTimeOut { get; set; }

        public override bool IsNeedPause => true;

        private static object lockRW = new object();
        private List<double> lstPressVal = new List<double>();
        //按压数据
        private System.Collections.Generic.List<double> pressureSamples;
        //用于数据采集的停止
        private bool _isBreak;

        public PressurizeChart()
        {
            this.Tips = "保压并生成曲线";
            this.Icon = "\ue652";
        }

        [Step(1, "移动到保压位", IsNeedPause = true)]
        public void Move()
        {
            _isBreak = false;
            pressureSamples = new System.Collections.Generic.List<double>();
            if (HasAxisPos)
            {
                AxisPos.MovePostion(MyOwner.DeviceEngine, false);
            }
        }

        [Step(2, "压力放大器清零")]
        public void Pressure()
        {
            GetVDevice<VCommuncation>(CommDevice, out var val);
            val.Open(5000, (byte[])null);
            val.SetProtocol(ProtocolType.ModbusRTU);
            PressureAddress = getSlaveNum(val);
            GetVDevice<VCylinder>(Cylinder, out var val2);
            lock (lockRW)
            {
                val.Write<int>(1, $"{PressureAddress} 10 17924 1", true);
            }
        }

        [Step(3, "保压")]
        public void Retract()
        {
            MyOwner.OnLog(LogType.Info, $"开始保压过程");
            press(); //异步进行压力采集
            GetVDevice<VCylinder>(Cylinder, out var val2);
            val2.InRetractSensorCheck(CylinderTime);
            DateTime startTime = DateTime.Now;
            val2.Extend();
            val2.InExtendSensorCheck(CylinderTime);
            DateTime endTime = DateTime.Now;
            TimeSpan timedec = endTime - startTime;
            CylinderTTimeOut = timedec.TotalSeconds;
            double num = 0.0;
            Thread.Sleep(DelayTime);
            double PressureValue = Singlepress();
            PressurizeVal = PressureValue;
            MyOwner.OnLog((LogType)1, $"保压设备{PressureAddress}读值为{PressurizeVal}", "");
            if (Math.Abs(PressureValue - num) > OffsetValue && !IsEmptyMode)
            {
                val2.Retract();
                val2.InRetractSensorCheck(CylinderTime);
                Pressure();
                if (IsShowAlarm)
                {
                    OnAlarm((AlarmType)0, $"保压设备{PressureAddress}读值为{PressurizeVal}超过误差值！", "");
                }
                _isBreak=true; //停止采集
                return;
            }
            if (IsEmptyMode)
            {
                PressurizeVal = Math.Round(PressureValue, 3);
            }
            Thread.Sleep(PressureTime - DelayTime);
            DateTime startTime1 = DateTime.Now;
            val2.Retract();
            val2.InRetractSensorCheck(CylinderTime);
            DateTime endTime1 = DateTime.Now;
            TimeSpan timedec1 = endTime1 - startTime1;
            CylinderFTimeOut = timedec1.TotalSeconds;
            _isBreak = true; //停止采集
            PressureTimeOut = PressureTime / 1000;
        }

        private void press()
        {
            Task.Run(() =>
            {
                var io = MyOwner.DeviceEngine.GetVirtualByID(Device.DeviceID) as VIO;
                while (true)
                {
                    if (_isBreak) break;
                    // 实时采集压力
                    double CurentCardAnalogValue = io.GetAnglogIn();
                    double CurrentDeviceRealValue = Math.Round(CurentCardAnalogValue * RatioCG / RatioCU, 3);
                    pressureSamples.Add(CurrentDeviceRealValue);
                    Thread.Sleep(InstallTime);
                }
                //有一些曲线图最后数据会没有，传感器在气缸运动时不改变
                for (int i = 0; i <= 10; i++)
                {
                    pressureSamples.Add(0);
                }
                //结束后写入csv
                SaveFile();
            });
        }
        //单次读取模拟量
        private double Singlepress()
        {
            var io = MyOwner.DeviceEngine.GetVirtualByID(Device.DeviceID) as VIO;
            // 实时采集压力
            double CurentCardAnalogValue = io.GetAnglogIn();
            double CurrentDeviceRealValue = Math.Round(CurentCardAnalogValue * RatioCG / RatioCU, 3);
            return CurrentDeviceRealValue;
        }

        public void SaveFile()
        {
            DateTime now = DateTime.Now;
            string dateStr = now.ToString("yyyyMMdd");
            string timeStr = now.ToString("HHmmss");
            string FileDir = @"D:\力控数据存储\" + dateStr + "\\" + timeStr + "\\";
            string filename = GStringVal + ".csv";
            string picName = GStringVal;
            CRecordValue recordValuePress = new CRecordValue();
            string title = "No" + "," + "Time" + "," + "Press" + "," + "Position";
            string value = "";
            int pressindex = 0;
            for (int i = 0; i < pressureSamples.Count; i++)
            {
                int num = i + 1;
                int timenum = num * InstallTime;
                double press = pressureSamples[i] >= 0 ? pressureSamples[i] : 0;
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
        public override void OnNotifyPropertyUIChanged(ParameterAttribute parameter, object newV)
        {
            base.OnNotifyPropertyUIChanged(parameter, newV);
            VAxisPos val = default(VAxisPos);
            int num;
            if (parameter.Name == "AxisPos")
            {
                val = (VAxisPos)((newV is VAxisPos) ? newV : null);
                num = ((val != null) ? 1 : 0);
            }
            else
            {
                num = 0;
            }
            if (num != 0)
            {
                val.UpdateAxis(MyOwner.DeviceEngine);
            }
        }

        public override void Stop()
        {
            if (AxisPos != null)
            {
                bool flag = false;
                AxisPos.Stop(MyOwner.DeviceEngine, flag);
            }
        }

        public override void Pause()
        {
            ((MotionFunction)this).Stop();
        }

        public Dictionary<string, object> GetExtResult()
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            foreach (AxisPosItem item in (List<AxisPosItem>)(object)AxisPos)
            {
                dictionary.Add($"{item.Axis.AxisType}_Spd", item.Speed);
                dictionary.Add($"{item.Axis.AxisType}_Acc", item.Acc);
                dictionary.Add($"{item.Axis.AxisType}_Dec", item.Dec);
                dictionary.Add($"{item.Axis.AxisType}_SpdTarget", item.Axis.MoveSpeed * 0.8);
                dictionary.Add($"{item.Axis.AxisType}_AccTarget", 0.1);
                dictionary.Add($"{item.Axis.AxisType}_Distance", item.Distance);
            }
            return dictionary;
        }

        private int getSlaveNum(VCommuncation comm)
        {
            int slaveNum = 1;
            for (int i = 0; i < comm.Actions.Count; i++)
            {
                if (comm.Actions[i].Name == Slave.Name)
                {
                    int.TryParse(comm.Actions[i].Value, out slaveNum);
                    break;
                }
            }
            return slaveNum;
        }
    }
}
