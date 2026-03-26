#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       NewPressurize
* 机器名称:       F04846
* 命名空间:       Luster.Module.Motion.Business.Functions
* 文 件 名:       NewPressurize.cs
* 创建时间:       2022/9/8 22:33:45
* 作    者:       房晶鹏
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       jingpengfang@lusterinc.com 
* 唯一标识：      e822fa45-94d6-417b-980f-8d8a6393ce44
* 登录用户:       fangjingpeng
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/8 22:33:45
* 修 改 人:		  F04846
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.VDevice;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Functions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Business.Functions
{
    public class NewPressurize : GoToFunction
    {
        [NotEmpty]
        [Parameter("通信设备", 1, CN = "通信设备", EditorType = typeof(VCommuncation))]
        public VDevice CommDevice { get; set; }

        //[Range(1, 100)]
        //[Parameter("设备地址", 2, CN = "设备地址", DefaultV = 1)]
        public int PressureAddress { get; set; }

        [Parameter("从站设备", 2, CN = "设备地址", CanRef = ParamRef.NoRef)]
        public SocketAction Slave { get; set; }


        [NotEmpty]
        [Parameter("IO类型，通过关键字搜索", 3, CN = "保压气缸", EditorType = typeof(VCylinder))]
        public VDevice Cylinder { get; set; }

        [NotEmpty]
        [Parameter("气缸动作超时设定", 4, CN = "气缸超时", DefaultV = 5000)]
        public int CylinderTime { get; set; }

        [NotEmpty]
        [Parameter("气缸保压时间", 5, CN = "保压时间", DefaultV = 3000)]
        public int PressureTime { get; set; }

        [NotEmpty]
        [DependOn("HasCylinder1", true)]
        [Parameter("气缸保压压力值", 6, CN = "保压压力值", DefaultV = 1000)]
        public int PressureValue { get; set; }

        [NotEmpty]
        [Parameter("偏差值", 7, CN = "偏差值", DefaultV = 200)]
        public int OffsetValue { get; set; }

        [Parameter("延时读取压力值", 8, CN = "延时读取", DefaultV = 200)]
        public int DelayTime { get; set; }

        [NotEmpty]
        [Parameter("读取值x缩放值/1000 = 真实保压值", 9, CN = "缩放值", DefaultV = 1)]
        public double Scale { get; set; }

        [NotEmpty]
        [Parameter("压力超限提示", 9, CN = "超限提示", DefaultV = true)]
        public bool IsShowAlarm { get; set; }


        [Parameter("真实保压值", 10, CN = "真实保压值", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public double PressurizeVal { get; set; }

        private static object lockRW = new object();

        public NewPressurize()
        {
            this.Tips = "保压";
            this.Icon = "\xe652";
        }
        /// <summary>
        /// 通信清零 、保压
        /// </summary>
        [Step(2, "压力放大器清零")]
        public void Pressure()
        {
            // 0.连接压力传感器通讯
            GetVDevice<VCommuncation>(CommDevice, out var communcation);
            communcation.Open();
            communcation.SetProtocol(ProtocolType.ModbusRTU);
            PressureAddress = getSlaveNum(communcation);
            // 1.获取硬件设备
            GetVDevice<VCylinder>(Cylinder, out var cylinder);

            // 2.压力放大器清零
            lock (lockRW)
            {
                // 单通道清零-01 10 00 5E 00 01 02 00 01 6A EE
                communcation.Write<int>(1, $"{PressureAddress} 10 94 1");
            }
        }

        /// <summary>
        /// 通信读取值、缩回气缸
        /// </summary>
        [Step(3, "保压")]
        public void Retract()
        {
            // 0.连接压力传感器通讯
            GetVDevice<VCommuncation>(CommDevice, out var communcation);
            communcation.Open();
            communcation.SetProtocol(ProtocolType.ModbusRTU);
            PressureAddress = getSlaveNum(communcation);


            // 1.获取硬件设备
            GetVDevice<VCylinder>(Cylinder, out var cylinder);

            // 2.保压气缸下降
            cylinder.Extend();
            cylinder.InExtendSensorCheck(CylinderTime);

            // 3.延时读取
            Thread.Sleep(DelayTime);

            // 4.读值
            int pressurizeVal = 0;
            lock (lockRW)
            {
                var coilVs = communcation.Read<int>($"{PressureAddress} 03 80 1");
                if (coilVs.Count > 0)
                {
                    pressurizeVal = coilVs[0];
                }
                else
                {
                    pressurizeVal = 0;
                }
            }

            PressurizeVal = Math.Round(pressurizeVal * 1.0 * Scale / 1000, 3);
            if (Math.Abs(PressureValue - pressurizeVal) > OffsetValue && !IsEmptyMode)
            {
                cylinder.Retract();
                cylinder.InRetractSensorCheck(CylinderTime);

                // 此处报警后，由于复位后不再进行清零操作，所以此处需要做清零处理
                Pressure();
                if(IsShowAlarm)
                {
                    OnAlarm(AlarmType.InfoTip, $"保压设备{PressureAddress}读值为{PressurizeVal}超过误差值！");
                }
                return;
            }

            if (IsEmptyMode)
            {
                PressurizeVal = Math.Round(PressureValue * 1.0 / 1000, 3);
            }

            // 5.保压
            Thread.Sleep(PressureTime - DelayTime);

            // 6.保压气缸上升
            cylinder.Retract();
            cylinder.InRetractSensorCheck(CylinderTime);
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