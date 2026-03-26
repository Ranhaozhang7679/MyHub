#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ProLoaded
* 机器名称:       L05123-NB
* 命名空间:       Luster.Module.Motion.Logic.Functions
* 文 件 名:       ProLoaded.cs
* 创建时间:       2022/8/4 14:36:24
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      01454307-87e3-489b-a051-a06c05729b91
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/8/4 14:36:24
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Motion.DataStruct.DataModels;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Common.Models;
using Luster.TaskFlow.Motion;
using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luster.Motion.DataStruct.VDevice;
using Luster.Motion.DataStruct.Enums;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Extensions;
using System.Net;

namespace Luster.Module.Motion.Business.Functions
{
    [Description("时间类型")]
    public enum TimeType
    {
        [Description("流线入料时间记录")]
        LoadedTimeLog,

        [Description("流线出料时间记录")]
        UnLoadedTimeLog,
    }

    /// <summary>
    /// 时间记录
    /// </summary>
    public class TimeLogEvent : MotionFunction
    {
        [NotEmpty]
        [Parameter("入料/出料", 0, CN = "时间类型", DefaultV = TimeType.LoadedTimeLog)]
        public TimeType TimeType { get; set; }

        [NotEmpty]
        [Parameter("PLC设备", 0, CN = "PLC设备", EditorType = typeof(VPlc))]
        public VDevice PlcDevice { get; set; }

        [NotEmpty]
        [Parameter("计时开始对应的PLC地址", 1, CN = "计时开始地址")]
        public string StartTimeAddress { get; set; }

        [NotEmpty]
        [Parameter("计时结束对应的PLC地址", 2, CN = "计时开始地址")]
        public string EndTimeAddress { get; set; }

        [NotEmpty]
        [Parameter("超时时间,在阻塞一定时间后输出状态", 3, CN = "超时时间")]
        public int OverTime { get; set; } = 5000;

        [NotEmpty]
        [Parameter("入料/出料耗时时间", 4, CN = "运行耗时", ParamType = ParamType.OUT)]
        public double ActualTime { get; set; } = 0;

        public TimeLogEvent()
        {
            this.Tips = "任务延时等待";
            this.Icon = "\xe69c";
        }

        public override bool DoExcute(out string errMsg)
        {
            GetVDevice<VPlc>(PlcDevice, out var plc);
            ActualTime = -1;
            errMsg = string.Empty;
            DateTime sTime = DateTime.Now;
            DateTime eTime = DateTime.Now;

            bool WaitStart = false;
            bool WaitEnd = false;

            plc.Open();

            var startAddr = plc.Address.FirstOrDefault(p => p.Address == StartTimeAddress.ToString());
            if (startAddr == null)
            {
                errMsg = $"Plc:{Name} 不存在计时开始地址:{startAddr.Address}";
                return false;
            }

            var endAddr = plc.Address.FirstOrDefault(p => p.Address == EndTimeAddress.ToString());
            if (endAddr == null)
            {
                errMsg = $"Plc:{Name} 不存在计时结束地址:{endAddr.Address}";
                return false;
            }

            WaitFunc(() =>
            {
                WaitStart = Luster.Motion.DataStruct.Network.CompareVal<double>.Compare(OpRule.Equal, 1, startAddr.Value);
                return WaitStart;
            }, $"Wait等待值为1", 50);

            ///更新开始时间,无需判断开始信号，开始信号为持续等待
            sTime = DateTime.Now;
            MyOwner.OnLog(LogType.Debug, $"更新开始时间为:{sTime.ToString("HH:mm:ss:fff")}");

            bool isBlocked = false;
            WaitFunc(() =>
            {
                eTime = DateTime.Now;
                WaitEnd = Luster.Motion.DataStruct.Network.CompareVal<double>.Compare(OpRule.Equal, 1, endAddr.Value);
                double SpendTime = (eTime - sTime).TotalSeconds;
                if (!WaitEnd && !isBlocked && SpendTime * 1000 > OverTime)
                {
                    isBlocked = true;
                    if (TimeType == TimeType.LoadedTimeLog)
                    {
                        MyOwner.OnProBlock(SpendTime, "Block", "入料阻塞");
                    }
                    else
                    {
                        MyOwner.OnProBlock(SpendTime, "Block", "出料阻塞");
                    }
                }
                else if (WaitEnd)
                {
                    MyOwner.OnLog(LogType.Debug, $"更新结束时间为:{eTime.ToString("HH:mm:ss:fff")}");
                    ActualTime = SpendTime;
                    MyOwner.OnLog(LogType.Debug, $"更新耗时时间为:{ActualTime}s");
                    MyOwner.OnProBlock(ActualTime, "UnBlock", "阻塞恢复");
                }
                return WaitEnd;
            }, $"Wait等待值为1", 50);

            return base.DoExcute(out errMsg);
        }

        public override void OnNotifyPropertyUIChanged(ParameterAttribute parameter, object newV)
        {
            base.OnNotifyPropertyUIChanged(parameter, newV);
            GetVDevice<VPlc>(newV as VDevice, out var plcDevice);
            if (plcDevice != null)
            {
                var addr = GetAddress(plcDevice, DataType.Bool);
                OnDataSource(nameof(StartTimeAddress), addr.ToArray());
                OnDataSource(nameof(EndTimeAddress), addr.ToArray());
            }
        }

        protected virtual List<string> GetAddress(VPlc plc, DataType dataType)
        {
            return plc.GetAddress(dataType, PlcAddrType.Read);
        }
    }
}


