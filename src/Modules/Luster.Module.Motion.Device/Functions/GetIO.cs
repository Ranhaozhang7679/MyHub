#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       GetIO
* 机器名称:       L05123-NB
* 命名空间:       Luster.Module.Motion.Device.Functions
* 文 件 名:       GetIO.cs
* 创建时间:       2022/5/30 9:35:33
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      dd4298e6-900c-45dc-81e4-8758470f2a3f
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/30 9:35:33
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Device.Functions
{
    /// <summary>
    /// 获取IO值
    /// </summary>
    public class GetIO : MotionFunction, IGetExtResult
    {
        [NotEmpty]
        [Parameter("IO类型，通过关键字搜索", 0, CN = "IO名称", EditorType = typeof(VIO))]
        public VDevice Device { get; set; }

        [Parameter("空跑模式直接返回\"空跑信号量\"", 1, CN = "启用空跑", DefaultV = false)]
        public bool IsEnableEmptyMode { get; set; }

        [DependOn("IsEnableEmptyMode", true)]
        [Parameter("空跑时，输出的值", 2, CN = "空跑信号量", DefaultV = false)]
        public bool EmptyDigital { get; set; }

        [Parameter("数字信号", 3, CN = "数字信号", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool DigitalVal { get; set; }

        [Parameter("模拟信号,类型为Double", 4, CN = "模拟信号", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public double AnglogVal { get; set; }

        /// <summary>
        /// 自动注释
        /// </summary>
        public override string[] NoteParams => new string[] { "获取", nameof(Device) };

        public GetIO()
        {
            this.Tips = "获取IO信号值";
            this.Icon = "\xe67f";
        }

        public override bool DoExcute(out string errMsg)
        {
            errMsg = "";
            GetVDevice<VIO>(Device, out var io);

            //空跑模式并且启用自带空跑模式判断的时候输出值Jack20230523
            if (IsEmptyMode && IsEnableEmptyMode)
            {
                DigitalVal = EmptyDigital;
                return true;
            }

            if (io.IOType == Luster.Motion.DataStruct.Enums.IOType.Digital)
            {
                if (io.Behavior == Luster.Motion.DataStruct.Enums.IOBehavior.Input)
                {
                    DigitalVal = io.GetDigitalIn();
                }
                else
                {
                    DigitalVal = io.GetDigitalOut();
                }
                MyOwner.OnLog(LogType.Info,$"{io.Name}:状态为{DigitalVal}");
            }
            else
            {
                if (!string.IsNullOrEmpty(io.AnglogExp))
                {
                    AnglogVal = io.GetAnalogExpVal();
                }
                else
                {
                    if (io.Behavior == Luster.Motion.DataStruct.Enums.IOBehavior.Input)
                    {
                        AnglogVal = io.GetAnglogIn();
                    }
                    else
                    {
                        AnglogVal = io.GetAnglogOut();
                    }
                }
                MyOwner.OnLog(LogType.Info, $"{io.Name}:值为{AnglogVal}");
            }

            return base.DoExcute(out errMsg);
        }


        public override void OnNotifyPropertyUIChanged(ParameterAttribute parameter, object newV)
        {
            base.OnNotifyPropertyUIChanged(parameter, newV);
        }

        public Dictionary<string, object> GetExtResult()
        {
            Dictionary<string, object> result = new Dictionary<string, object>();

            GetVDevice<VIO>(Device, out var io);
            if (io.IOType == IOType.Analog && io.Behavior == Luster.Motion.DataStruct.Enums.IOBehavior.Input)
            {
                result.Add("Pressure", io.GetAnglogIn());
                result.Add("RespondTime", MyOwner.TimeConsuming);
            }

            return result;
        }
    }
}