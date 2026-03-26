#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       EleCylinder
* 机器名称:       L05123-NB
* 命名空间:       Luster.Module.Motion.Device.Functions
* 文 件 名:       EleCylinder.cs
* 创建时间:       2022/8/11 18:18:23
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      95a7df16-6d97-449b-9d34-f046bb16e4e7
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/8/11 18:18:23
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.VDevice;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Device.Functions
{
    /// <summary>
    /// 电缸
    /// electric cylinder
    /// </summary>
    public class EleCylinder : OverTimeFunction
    {
        [NotEmpty]
        [Parameter("气缸类型，通过关键字搜索", 0, CN = "电缸设备", EditorType = typeof(VPCylinder))]
        public VDevice Device { get; set; }

        [Range(0, 10)]
        [Parameter("电缸的点位", 1, CN = "执行点位", DefaultV = 0)]
        public int PointNo { get; set; }

        [Parameter("检查电缸运动是否完成", 2, CN = "检查完成", DefaultV = true)]
        public bool CheckDone { get; set; }

        [Parameter("检查电缸运动是否完成", 3, CN = "力值监控", DefaultV = false)]
        public bool IsNMonitor { get; set; }

        /// <summary>
        /// 是否力监控
        /// </summary>
        [DependOn("IsNMonitor", true)]
        [Limit(-100, 500)]
        [Parameter("力值范围，单位是N", 4, CN = "力值范围")]
        public LRange NRange { get; set; }

        /// <summary>
        /// 对应的力
        /// </summary>
        [Parameter("电缸的力值,单位N", 8, CN = "力值", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public double N { get; set; }

        public EleCylinder()
        {
            this.Tips = "执行电缸运行点位，接着等待力完成";
            this.Icon = "\xe666";
            NRange = new LRange(0, 10);
        }

        public override bool DoExcute(out string errMsg)
        {
            GetVDevice<VPCylinder>(Device, out var eCylinder);

            N = eCylinder.Move(PointNo, CheckDone, OverTime);

            if (!IsEmptyMode && IsNMonitor && !NRange.IsSatisfy(N))
            {
                OnAlarm(AlarmType.WarningTip, $"力值:{N}不在范围{NRange.Min} - {NRange.Max}", "01");
            }

            return base.DoExcute(out errMsg);
        }
    }
}