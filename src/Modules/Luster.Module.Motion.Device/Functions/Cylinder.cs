#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       Cylinder
* 机器名称:       L05123-NB
* 命名空间:       Luster.Module.Motion.Device.Functions
* 文 件 名:       Cylinder.cs
* 创建时间:       2022/6/13 14:26:36
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      188f6319-4724-4515-bf1e-770ee9d25a50
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/13 14:26:36
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Extensions;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.VDevice;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace Luster.Module.Motion.Device.Functions
{



    public class Cylinder : OverTimeFunction, IPauseFunction, IGetExtResult
    {
        public enum CylinderType
        {
            [Description("伸出")]
            Extend,

            [Description("缩回")]
            Retract
        }

        public static List<string> ActionTypes = new List<string> { "伸出", "缩回" };

        [NotEmpty]
        [Parameter("气缸类型，通过关键字搜索", 0, CN = "气缸名称", EditorType = typeof(VCylinder))]
        public VDevice Device { get; set; }

        [Parameter("气缸要做的动作", 1, CN = "气缸动作", DefaultV = CylinderType.Extend)]
        public CylinderType CType { get; set; }


        [Parameter("确认气缸是否伸出或缩回完成", 3, CN = "感知检查", DefaultV = true)]
        public bool CheckEnd { get; set; }

        [Parameter("空跑是否禁用", 3, CN = "空跑是否禁用", DefaultV = false, CanRef = ParamRef.Ref)]
        public bool IsUse { get; set; }

        [DependOn("CheckEnd", true)]
        [Parameter("动作时间是否在合理区间", 10, CN = "是否合理", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool IsInVaildRange { get; set; }

        [DependOn("CheckEnd", true)]
        [Parameter("标准时间偏差", 10, CN = "标准时间偏差", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public int Deviation { get; set; }

        /// <summary>
        /// 自动注释
        /// </summary>
        //public override string[] NoteParams => new string[] { nameof(Device), nameof(CType) };

        public override string[] NoteParams { get; set; } = new string[2] { nameof(Device), nameof(CType) };

        public static string ExtendName = "伸出";
        public static string RetractName = "缩回";


        public Cylinder()
        {
            this.Tips = "气缸运动";
            this.Icon = "\xe666";
        }

        public override bool DoExcute(out string errMsg)
        {
            if (!IsUse)
            {
                GetVDevice<VCylinder>(Device, out var cylinder);
                DateTime beginTime = DateTime.Now;
                if (CType == CylinderType.Extend)
                {
                    cylinder.Extend();

                    if (CheckEnd)
                    {
                        cylinder.InExtendSensorCheck(OverTime);
                    }
                }
                else
                {
                    cylinder.Retract();

                    if (CheckEnd)
                    {
                        cylinder.InRetractSensorCheck(OverTime);
                    }
                }
                DateTime endTime = DateTime.Now;
                IsInVaildRange = (endTime - beginTime).TotalMilliseconds >= cylinder.MinValue && (endTime - beginTime).TotalMilliseconds <= cylinder.MaxValue;
                Deviation = (int)(endTime - beginTime).TotalMilliseconds - cylinder.TargetValue;
            }
            return base.DoExcute(out errMsg);
        }

        /// <summary>
        /// 气缸速度
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> GetExtResult()
        {
            Dictionary<string, object> result = new Dictionary<string, object>();

            GetVDevice<VCylinder>(Device, out var cylinder);
            result.Add("Distance", cylinder.Distance);
            result.Add("MaxSpeed", cylinder.MaxSpeed);
            result.Add("Speed", cylinder.Speed);

            return result;
        }

        public override void OnNotifyPropertyUIChanged(ParameterAttribute parameter, object newV)
        {
            base.OnNotifyPropertyUIChanged(parameter, newV);
            if (parameter.Name == "Device")
            {
                GetVDevice<VCylinder>(newV as VDevice, out var cylinder);

                if (cylinder != null)
                {
                    //var abc = Enum.GetValues(typeof(CylinderType)).Cast<Enum>().Select(x => x.GetDescription()).ToList();

                    NoteParams[0] = "伸出代表:" + cylinder.ExtendName;
                    NoteParams[1] = "缩回代表:" + cylinder.RetractName;
                }

            }
        }
    }
}