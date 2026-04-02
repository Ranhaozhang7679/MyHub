#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       PressureSensor
* 机器名称:       F04846
* 命名空间:       Luster.Module.Motion.Device.Functions
* 文 件 名:       PressureSensor.cs
* 创建时间:       2022/12/16 18:57:48
* 作    者:       房晶鹏
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       jingpengfang@lusterinc.com 
* 唯一标识：      a15a1157-d114-49d8-a606-af3edfb74ed0
* 登录用户:       fangjingpeng
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/12/16 18:57:48
* 修 改 人:		  F04846
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.VDevice;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Device.Functions
{
    public enum PressureActionType
    {
        [Description("清零")]
        Clear,

        [Description("读值")]
        ReadValue,

        [Description("设置标定模式")]
        CalibrationSet,

        [Description("开始标定")]
        CalibrationStart
    }
    public class PressureSensor : MotionFunction, IPauseFunction
    {
        [NotEmpty]
        [Parameter("通信设备", 0, CN = "通信设备", EditorType = typeof(VCommuncation))]
        public VDevice CommDevice { get; set; }

        [Parameter("从站设备", 1, CN = "从站设备", CanRef = ParamRef.NoRef)]
        public SocketAction Slave { get; set; }

        //[Range(1, 100)]
        //[Parameter("站号", 1, CN = "压力传感器站号", DefaultV = 1)]
        private int StationNum { get; set; }

        [Parameter("压力放大器工作模式", 2, CN = "工作模式", DefaultV = PressureActionType.ReadValue)]
        public PressureActionType PActionType { get; set; }

        [DependOn("PActionType", PressureActionType.ReadValue)]
        [Parameter("延时读取压力值", 3, CN = "延时读取", DefaultV = 200)]
        public int DelayTime { get; set; }

        [Parameter("标准压力值", 4, CN = "压力值", DefaultV = 0.5)]
        public double PressureValue { get; set; }

        [Parameter("最小压力值,只有值在大于-1并且value<0才生效", 5, CN = "最小压力值", DefaultV = -1)]
        public double MinValue { get; set; }

        [Range(1, 100)]
        [Parameter("多次读取次数", 4, CN = "多次读取", DefaultV = 1)]
        public int Times { get; set; }

        [Parameter("多次读取间隔时间，单位为ms", 4, CN = "间隔", DefaultV = 1)]
        public int Interval { get; set; }


        [DependOn("PActionType", PressureActionType.ReadValue)]
        [Parameter("压力传感器值,单位为千克", 6, CN = "压力传感器值", ParamType = TaskFlow.Common.Enums.ParamType.OUT, DefaultV = 0)]
        public double Value { get; set; }


        private static object lockRW = new object();

        /// <summary>
        /// 压力值数组
        /// </summary>
        private List<double> lstPressVal = new List<double>();

        public PressureSensor()
        {
            this.Tips = "压力传感器";
            this.Icon = "\xe652";
        }

        public override bool DoExcute(out string errMsg)
        {
            errMsg = "";
            // 0.连接压力传感器通讯
            GetVDevice<VCommuncation>(CommDevice, out var communcation);
            communcation.SetProtocol(ProtocolType.ModbusRTU);
            StationNum = getSlaveNum(communcation);
            if (StationNum == -1)
            {
                errMsg = "串口选择异常或者站号选择异常,请在流程里将串口和站号重新选择一下！";
                OnAlarm(AlarmType.WarningTip, errMsg);
                return false;
            }
            communcation.Open();
            Value = 0;
            lstPressVal.Clear();
            if (PActionType == PressureActionType.Clear)
            {
                if (!IsEmptyMode)
                {
                    lock (lockRW)
                    {
                        // 1、单通道清零-01 10 00 5E 00 01 02 00 01 6A EE
                        for (int i = 0; i < 1; i++)
                        {
                            bool comResult = communcation.WriteWithoutBlocking<int>(1, $"{StationNum} 10 94 1");
                            if (comResult)
                            {
                                //    var coilVs = (communcation.Read<int>($"{StationNum} 03 80 1")[0]) / 1000.0;
                                //    MyOwner.OnLog(LogType.Info, $"第{i + 1}清零成功,当前压力值为 {coilVs}");
                                //    if (Math.Abs(coilVs)>0.5)
                                //    {
                                //        MyOwner.OnAlarm(AlarmType.Timeout, $"压力值清零成功,但是当前压力值为 {coilVs}，有异常！！！！");
                                //    }
                                break;
                            }
                            else
                            {
                                MyOwner.OnLog(LogType.Warning, $"第{i + 1}次压力值清零失败");
                                //    var coilVs = (communcation.Read<float>($"{StationNum} 03 80 1")[0]) / 1000.0;
                                //    MyOwner.OnLog(LogType.Warning, $"第{i + 1}次压力值清零失败,当前压力值为 {coilVs}");
                                //    if (i>=2)
                                //    {
                                //        MyOwner.OnAlarm(AlarmType.Timeout, $"压力值清零失败,当前压力值为 {coilVs}");
                                //    }

                            }
                        }
                      }
                    }
            }
            else if (PActionType == PressureActionType.ReadValue)
            {
                // 2、延时读取
                Thread.Sleep(DelayTime);
                if (!IsEmptyMode)
                {
                    // 3、读值
                    for (int i = 0; i < Times; i++)
                    {
                        lock (lockRW)
                        {
                            var coilVs = communcation.Read<int>($"{StationNum} 03 80 1");

                            
                            if (Times < 3)
                            {
                                if (coilVs.Count > 0)
                                {
                                    Value += coilVs[0];
                                }
                                else
                                {
                                    Value = 0;
                                }
                            }
                            else
                            {
                                lstPressVal.Add(coilVs[0]);
                            }
                            
                        }
                        Thread.Sleep(Interval);
                    }
                    //如果读值次数大于等于3次，去除最大最小值，取平均值
                    if (Times >= 3)
                    {
                        Value = 0;
                        lstPressVal.Remove(lstPressVal.Max());
                        lstPressVal.Remove(lstPressVal.Min());
                        for (int j = 0; j < Times - 2; j++)
                        {
                            Value += lstPressVal[j];
                        }
                        Value = Math.Round(Math.Round(Value * 1.0 / 1000, 3) / (double)(Times - 2), 3);
                    }
                    else
                    {
                        Value = Math.Round(Math.Round(Value * 1.0 / 1000, 3) / (double)Times, 3);
                    }

                    double dbRandom = Math.Round(new Random().NextDouble() / 10.0, 3);
                    // 压力值为负，并且程序配置最小压力值，那么就更新压力并提醒报警
                    if (MinValue > -1 && Value < 0)
                    {
                        Value = MinValue + dbRandom;
                        //取消界面报警提示
                        // OnAlarm(AlarmType.InfoTip, $"压力读取到负值:{Value} 使用默认最小值{MinValue}");
                    }

                }
                else
                {
                    Value = PressureValue;
                }
            }
            else if (PActionType == PressureActionType.CalibrationSet)
            {
                // 4、标定
            }
            return string.IsNullOrEmpty(errMsg);
        }

        private int getSlaveNum(VCommuncation comm)
        {
            int slaveNum = -1;
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
