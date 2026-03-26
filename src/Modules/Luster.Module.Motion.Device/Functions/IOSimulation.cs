#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       IODevice
* 机器名称:       L05123-NB
* 命名空间:       Luster.Module.Motion.Device.Functions
* 文 件 名:       IODevice.cs
* 创建时间:       2022/7/19 10:46:17
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      748bcd7b-16ae-4aa5-8786-c1f4d88fc005
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/19 10:46:17
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Luster.Module.Motion.Device.Functions
{
    public class IOSimulation : OverTimeFunction, IPauseFunction
    {
        #region IO设备
        [NotEmpty]
        [Parameter("用于吸取物体的设备", 0, CN = "仿真设备", EditorType = typeof(VIOSimulation))]
        public VDevice IODevice { get; set; }

        /// <summary>
        /// 动作对象
        /// </summary>
        [Parameter("执行IO动作", 1, CN = "IO动作")]
        public string Action { get; set; }

        /// <summary>
        /// 感知
        /// </summary>
        [Parameter("检查IO动作是否完成", 2, CN = "IO检知")]
        public string Check { get; set; }

        [Parameter("是否检查IO仿真动作超时", 3, CN = "IO仿真检知", DefaultV = true)]
        public bool CheckAction { get; set; }

        /// <summary>
        /// 强制感知
        /// </summary>
        [Parameter("启用强制检知", 4, CN = "IO强制检知", CanRef = ParamRef.Ref, DefaultV = false)]
        public bool ForceCheck { get; set; }

        /// <summary>
        /// 多IO组合时，执行动作需要有间隔时间
        /// </summary>
        [Parameter("多IO组合时，执行动作需要有间隔时间,单位ms", 5, CN = "IO执行间隔", DefaultV = 10)]
        public int Interval { get; set; }
        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        public IOSimulation()
        {
            this.Tips = "IO仿真设备";
            this.Icon = "\xe69b";
        }

        /// <summary>
        /// 代码执行
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public override bool DoExcute(out string errMsg)
        {
            GetVDevice<VIOSimulation>(IODevice, out var ioDevice);

            // 动作检查
            if (!string.IsNullOrEmpty(Action))
            {
                ioDevice.Action(Action, Interval);
            }

            // 感知动作是否完成
            if (!string.IsNullOrEmpty(Check) && CheckAction && (ForceCheck || !IsEmptyMode))
            {
                ioDevice.Check(Check, OverTime);
            }

            return base.DoExcute(out errMsg);
        }

        /// <summary>
        /// 仿真设备动作
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="newV"></param>
        public override void OnNotifyPropertyUIChanged(ParameterAttribute parameter, object newV)
        {
            base.OnNotifyPropertyUIChanged(parameter, newV);
            if (parameter.Name == "IODevice")
            {
                try
                {
                    GetVDevice<VIOSimulation>(newV as VDevice, out var outIO);
                    if (outIO != null)
                    {
                        var actions = outIO.Actions.Where(u => !u.IsCheck).Select(u => u.Action).ToArray();
                        OnDataSource(nameof(Action), actions);

                        var checks = outIO.Actions.Where(u => u.IsCheck).Select(u => u.Action).ToArray();
                        OnDataSource(nameof(Check), checks);
                    }
                }
                catch (Exception ex)
                {
                    Owner.OnLog(Common.DataStruct.Enums.LogType.Error, ex.Message);
                }
            }
        }
    }
}