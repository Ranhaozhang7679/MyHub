using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.VDevice;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Functions;
using Luster.TaskFlow.Common.Module;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Functions;

namespace Luster.Module.Motion.Business.Functions
{
    public class Pressurize : GoToFunction
    {
        private static object lockRW = new object();

        private List<double> lstPressVal = new List<double>();

        [NotEmpty]
        [Parameter("轴的点位及对应的参数配置", 0, CN = "点位参数", MultiValues = false)]
        public VAxisPos AxisPos { get; set; }

        [NotEmpty]
        [Parameter("带轴运动参数配置", 1, CN = "是否带轴运动", DefaultV = true)]
        public bool HasAxisPos { get; set; }

        [NotEmpty]
        [Parameter("通信设备", 2, CN = "通信设备", EditorType = typeof(VCommuncation))]
        public VDevice CommDevice { get; set; }

        //[Range(1, 100)]
        //[Parameter("设备地址", 3, CN = "设备地址", DefaultV = 1)]
        public int PressureAddress { get; set; }

        [Parameter("从站设备", 2, CN = "设备地址", CanRef = ParamRef.NoRef)]
        public SocketAction Slave { get; set; }


        [NotEmpty]
        [Parameter("IO类型，通过关键字搜索", 4, CN = "保压气缸", EditorType = typeof(VCylinder))]
        public VDevice Cylinder { get; set; }

        [NotEmpty]
        [Parameter("气缸动作超时设定", 5, CN = "气缸超时", DefaultV = 5000)]
        public int CylinderTime { get; set; }

        [NotEmpty]
        [Parameter("气缸保压时间", 6, CN = "保压时间", DefaultV = 3000)]
        public int PressureTime { get; set; }

        [NotEmpty]
        [DependOn("HasCylinder1", new object[] { true })]
        [Parameter("气缸保压压力值", 7, CN = "保压压力值", DefaultV = 1000)]
        public double PressureValue { get; set; }

        [NotEmpty]
        [Parameter("偏差值", 8, CN = "偏差值", DefaultV = 200)]
        public double OffsetValue { get; set; }

        [Parameter("延时读取压力值", 9, CN = "延时读取", DefaultV = 0.2)]
        public int DelayTime { get; set; }

        [Range(1, 100)]
        [Parameter("多次读取次数", 10, CN = "读取次数", DefaultV = 1)]
        public int Times { get; set; }

        [Parameter("多次读取间隔时间，单位为ms", 11, CN = "读取间隔", DefaultV = 1)]
        public int Interval { get; set; }

        [NotEmpty]
        [Parameter("压力超限提示", 12, CN = "超限提示", DefaultV = false)]
        public bool IsShowAlarm { get; set; }

        [Parameter("真实保压值", 20, CN = "真实保压值", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public double PressurizeVal { get; set; }

        [NotEmpty]
        [Parameter("气缸保压时间输出,单位为S", 21, CN = "保压时间", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public int PressureTimeOut { get; set; }

        public override bool IsNeedPause => true;

        public Pressurize()
        {
            this.Tips = "保压";
            this.Icon = "\ue652";
        }

        [Step(1, "移动到保压位", IsNeedPause = true)]
        public void Move()
        {
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
            GetVDevice<VCommuncation>(CommDevice, out var val);
            val.Open(5000, (byte[])null);
            val.SetProtocol(ProtocolType.ModbusRTU);
            PressureAddress = getSlaveNum(val);
            GetVDevice<VCylinder>(Cylinder, out var val2);
            val2.InRetractSensorCheck(CylinderTime);
            val2.Extend();
            val2.InExtendSensorCheck(CylinderTime);
            Thread.Sleep(DelayTime);
            double num = 0.0;
            lstPressVal.Clear();
            for (int i = 0; i < Times; i++)
            {
                lock (lockRW)
                {
                    List<float> list = val.Read<float>($"{PressureAddress} 04 02 01", 2000);
                    if (Times < 3)
                    {
                        num = ((list.Count <= 0) ? 0.0 : (num + (double)list[0]));
                    }
                    else
                    {
                        lstPressVal.Add(list[0]);
                    }
                }
                Thread.Sleep(Interval);
            }
            if (Times >= 3)
            {
                PressurizeVal = 0.0;
                lstPressVal.Remove(lstPressVal.Max());
                lstPressVal.Remove(lstPressVal.Min());
                for (int j = 0; j < Times - 2; j++)
                {
                    PressurizeVal += lstPressVal[j];
                }
                PressurizeVal = Math.Round(Math.Round(PressurizeVal, 3) / (double)(Times - 2), 3);
            }
            else
            {
                PressurizeVal = Math.Round(Math.Round(num, 3) / (double)Times, 3);
            }
            MyOwner.OnLog((LogType)1, $"保压设备{PressureAddress}读值为{PressurizeVal}", "");
            if (Math.Abs(PressureValue - num) > OffsetValue && !IsEmptyMode)
            {
                val2.Retract();
                val2.InRetractSensorCheck(CylinderTime);
                Pressure();
                if(IsShowAlarm)
                {
                    OnAlarm((AlarmType)0, $"保压设备{PressureAddress}读值为{PressurizeVal}超过误差值！", "");
                }
                return;
            }
            if (IsEmptyMode)
            {
                PressurizeVal = Math.Round(PressureValue, 3);
            }
            Thread.Sleep(PressureTime - DelayTime);
            val2.Retract();
            val2.InRetractSensorCheck(CylinderTime);
            PressureTimeOut = PressureTime / 1000;
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
            //IL_002e: Unknown result type (might be due to invalid IL or missing references)
            //IL_005a: Unknown result type (might be due to invalid IL or missing references)
            //IL_0086: Unknown result type (might be due to invalid IL or missing references)
            //IL_00b2: Unknown result type (might be due to invalid IL or missing references)
            //IL_00ed: Unknown result type (might be due to invalid IL or missing references)
            //IL_011c: Unknown result type (might be due to invalid IL or missing references)
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
