#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       WaitIO
* 机器名称:       L05123-NB
* 命名空间:       Luster.Module.Motion.Device.Functions
* 文 件 名:       WaitIO.cs
* 创建时间:       2022/5/30 9:35:58
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      2d6a3e06-819f-4849-942d-c4c3a67528cf
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/30 9:35:58
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.VDevice;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Device.Functions
{
    public class WaitIO : OverTimeFunction, IWait, IPauseFunction
    {
        [NotEmpty]
        [Parameter("IO类型，通过关键字搜索", 0, CN = "IO名称", EditorType = typeof(VIO))]
        public VDevice Device { get; set; }

        [Parameter("IO类型，通过关键字搜索", 1, CN = "IO类型", DefaultV = IOType.Digital)]
        public IOType IOType { get; set; }

        [DependOn("IOType", IOType.Digital)]
        [Parameter("等待数字信号值", 3, CN = "数值值")]
        public bool DigitalVal { get; set; }


        [DependOn("IOType", IOType.Analog)]
        [Parameter("等待模拟信号值", 4, CN = "模拟值")]
        public double AnlogVal { get; set; }

        [Parameter("超时了是否蜂鸣并重试", 5, CN = "超时警告重试", DefaultV = false)]
        public bool IsNeedRetry { get; set; }

        [Limit(-1, 1000000)]
        [Parameter("超时时间,单位s", 20, CN = "等待超时", DefaultV = -1)]
        public override int OverTime { get; set; }

        [Parameter("超时了是否报警，如果不报警并且结果吸取异常，则输出false", 21, CN = "超时报警", DefaultV = true)]
        public bool IsAlarm { get; set; }

        [DependOn("IsAlarm", false)]
        [Parameter("真空吸取的结果,吸取超时结果为false,吸取成功结果为true", 31, CN = "是否成功", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool Result { get; set; }

        /// <summary>
        /// 自动注释
        /// </summary>
        public override string[] NoteParams => new string[] { "等待", nameof(Device) };

        public WaitIO()
        {
            this.Tips = "等待信号值";
            this.Icon = "\xe658";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public override bool DoExcute(out string errMsg)
        {
            GetVDevice<VIO>(Device, out var io);

            // 默认成功
            Result = true;
            Action retryAction = null;
            if (IsNeedRetry)
            {
                retryAction = () =>
                {
                    MyOwner.LightManager.SetBuzzer(true, 500, 500);
                    OnAlarm(AlarmType.Timeout, $"{MyOwner.Alias}等待IO超时,蜂鸣提示，再次等待");
                    if (IOType == IOType.Digital)
                    {
                        io.WaitIO(DigitalVal, -1);
                    }
                    else
                    {
                        io.WaitIO(AnlogVal, -1);
                    }
                };
            }
            /////////
            try
            {

                if (IOType == IOType.Digital)
                {
                    io.WaitIO(DigitalVal, OverTime * 1000, retryAction);
                }
                else
                {
                    io.WaitIO(AnlogVal, OverTime * 1000, retryAction);
                }

            }
            catch (DeviceTimeoutException dte)
            {
                dte.Module = io.Module;
                if (IsAlarm)
                {
                    //throw  dte;
                    throw new DeviceTimeoutException(io.Errors[DeviceError.SensorFail], io.ID, io.ErrorMessage, io.Module, io.Name);
                }
                else
                {
                    // 超时了，不报警，结果赋值为True
                    Result = false;
                }
            }
            return base.DoExcute(out errMsg);
        }

        /// <summary>
        /// IO等待支持暂停,只有处于报警时并且超时时才支持复位
        /// </summary>
        //public override bool IsNeedPause => IsAlarm && OverTime > 0;

        /// <summary>
        /// 暂停
        /// </summary>
        public override void Pause()
        {
        }
    }
}