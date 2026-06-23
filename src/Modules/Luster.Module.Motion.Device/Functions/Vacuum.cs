#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       Vacuum
* 机器名称:       L05123-NB
* 命名空间:       Luster.Module.Motion.Device.Functions
* 文 件 名:       Vacuum.cs
* 创建时间:       2022/6/8 14:55:27
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      50222f83-a9ee-49f2-a9c2-1fc69b832be9
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/8 14:55:27
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.SimDevice.Adapter;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Functions;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Device.Functions
{
    /// <summary>
    /// 真空
    /// </summary>
    public class Vacuum : OverTimeFunction, INote, IPauseFunction, IGetExtResult
    {
        [NotEmpty]
        [Parameter("真空设备", 1, CN = "真空", EditorType = typeof(VVacuum))]
        public VDevice Device { get; set; }

        [Parameter("真空类型", 2, CN = "真空类型")]
        public VacuumType VacuumType { get; set; }

        [DependOn("VacuumType", VacuumType.BlowClose)]
        [Parameter("真空破延时时间，单位是ms", 3, CN = "破关延时", DefaultV = 100)]
        public int DelayTime { get; set; }

        [Parameter("是否检查真空动作超时", 3, CN = "真空检知", DefaultV = true)]
        public bool CheckSuck { get; set; }

        [Limit(0, 1000000)]
        [Parameter("超时时间,单位ms", 19, CN = "吸取超时", DefaultV = 5000)]
        public override int OverTime { get; set; }

        [Parameter("超时了是否报警，如果不报警并且结果吸取异常，则输出false", 21, CN = "超时报警", DefaultV = true)]
        public bool IsAlarm { get; set; }

        [Parameter("是否有模拟量反馈", 21, CN = "模拟量反馈", DefaultV = false)]
        public bool IsHavePressureDevice { get; set; }

        [DependOn("IsHavePressureDevice", true)]
        [Parameter("IO类型，通过关键字搜索", 22, CN = "真空反馈", EditorType = typeof(VIO))]
        public VDevice PressureDevice { get; set; }

        [Parameter("负压值，单位为KPa", 30, CN = "负压值", ParamType = TaskFlow.Common.Enums.ParamType.OUT, DefaultV = 0)]
        public double PressureVal { get; set; }

        [DependOn("VacuumType", VacuumType.Sucking,VacuumType.BlowClose)]
        [Parameter("真空吸取的结果,吸取超时结果为false,吸取成功结果为true", 31, CN = "吸取结果", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool Result { get; set; }


        [DependOn("CheckSuck", true)]
        [Parameter("动作时间是否在合理区间", 10, CN = "是否合理", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool IsInVaildRange { get; set; }

        [DependOn("CheckSuck", true)]
        [Parameter("标准时间偏差", 10, CN = "标准时间偏差", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public int Deviation { get; set; }
        public override string[] NoteParams => new string[] { nameof(Device), nameof(VacuumType) };
        public Vacuum()
        {
            this.Tips = "真空";
            this.Icon = "\xe690";
        }

        /// <summary>
        /// 逻辑执行
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public override bool DoExcute(out string errMsg)
        {
            GetVDevice<VVacuum>(Device, out var vacuum);
            DateTime beginTime = DateTime.Now;
            Result = true;
            if (VacuumType == VacuumType.Sucking)
            {
                vacuum.Suck();
            }
            else if (VacuumType == VacuumType.Blow)
            {
                vacuum.Blow();
            }
            else if (VacuumType == VacuumType.BlowClose)
            {
                vacuum.BlowClose(DelayTime);
            }
            else
            {
                vacuum.Close();
            }

            if (CheckSuck && !IsEmptyMode)
            {
                try
                {
                    vacuum.InVacuumSensorCheck(OverTime, VacuumType == VacuumType.Sucking ? true : false);
                }
                catch (DeviceTimeoutException ex)
                {
                    if (IsAlarm)
                    {
                        //throw new DeviceTimeoutException(vacuum.Errors[DeviceError.SuckFail],$"{vacuum.Name} timeout>{DelayTime}!"); 
                        throw new DeviceTimeoutException(vacuum.Errors[DeviceError.SuckFail], vacuum.GetConfigMessage(DeviceError.SuckFail));
                    }
                    else
                    {
                        Result = false;
                    }
                }

                if (IsHavePressureDevice)
                {
                    GetVDevice<VIO>(PressureDevice, out var pressureDevice);
                    PressureVal = pressureDevice.GetAnglogIn();
                }
            }


            DateTime endTime = DateTime.Now;
            IsInVaildRange = (endTime - beginTime).TotalMilliseconds >= vacuum.MinValue && (endTime - beginTime).TotalMilliseconds <= vacuum.MaxValue;
            Deviation = (int)(endTime - beginTime).TotalMilliseconds - vacuum.TargetValue;
            return base.DoExcute(out errMsg);
        }

        public Dictionary<string, object> GetExtResult()
        {
            return new Dictionary<string, object>
            {
                { "Pressure", PressureVal },
                { "RespondTime", MyOwner.TimeConsuming },
            };
        }
    }
}