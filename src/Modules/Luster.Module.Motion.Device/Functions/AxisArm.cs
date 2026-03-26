#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       AxisArm
* 机器名称:       L05123-NB
* 命名空间:       Luster.Module.Motion.Device.Functions
* 文 件 名:       AxisArm.cs
* 创建时间:       2022/7/8 14:45:24
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      438f159f-f3b3-4b5d-92d9-9276213f6be6
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/8 14:45:24
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Device.Functions
{
    /// <summary>
    /// 
    /// </summary>
    public class AxisArm : OverTimeFunction
    {
        [NotEmpty]
        [Parameter("多轴仿真设备", 1, CN = "多轴")]
        public VAxisMDevice AxisM { get; set; }

        [NotEmpty]
        [Parameter("用于吸取物体的设备", 2, CN = "夹爪类型", DefaultV = PawType.Vacuum)]
        public PawType PawType { get; set; }

        #region 真空参数

        [DependOn("PawType", PawType.Vacuum)]
        [Parameter("用于吸取物体的设备", 3, CN = "真空", EditorType = typeof(VVacuum))]
        public VDevice Vacuum { get; set; }

        [DependOn("PawType", PawType.Vacuum)]
        [Parameter("用于吸取物体的设备", 4, CN = "吸取类别")]
        public VacuumType VacuumType { get; set; }
        #endregion

        #region 气缸参数

        [DependOn("PawType", PawType.Cylinder)]
        [Parameter("用于吸取物体的设备", 5, CN = "气缸", EditorType = typeof(VCylinder))]
        public VDevice Cylinder { get; set; }

        [DependOn("PawType", PawType.Cylinder)]
        [Parameter("用于吸取物体的设备", 6, CN = "伸缩类型")]
        public CylinderType CylinderType { get; set; }
        #endregion

        #region IO设备
        [DependOn("PawType", PawType.IODevice)]
        [Parameter("用于吸取物体的设备", 7, CN = "仿真设备", EditorType = typeof(VIOSimulation))]
        public VDevice IOSimulation { get; set; }

        /// <summary>
        /// 动作对象
        /// </summary>
        [DependOn("PawType", PawType.IODevice)]
        [Parameter("用于吸取物体的设备", 8, CN = "动作")]
        public string Action { get; set; }

        /// <summary>
        /// 感知
        /// </summary>
        [DependOn("PawType", PawType.IODevice)]
        [Parameter("用于吸取物体的设备", 9, CN = "动作完成")]
        public string Check { get; set; }
        #endregion

        #region 仿真电缸
        [DependOn("PawType", PawType.EleCylinder)]
        [Parameter("用于吸取物体的设备", 10, CN = "仿真设备", EditorType = typeof(VPCylinder))]
        public VDevice EleCylinder { get; set; }

        /// <summary>
        /// 动作对象
        /// </summary>
        [Range(1, 1000)]
        [DependOn("PawType", PawType.EleCylinder)]
        [Parameter("用于吸取物体的设备", 11, CN = "动作编号")]
        public int PointNo { get; set; }
        #endregion

        public AxisArm()
        {
            this.Tips = "多轴配合真空抓取物体";
            this.Icon = "\xe611";
            this.DynParam = true;
        }

        public override bool DoExcute(out string errMsg)
        {
            GetVDevice<VAxisM>(AxisM, out var axisM);

            // 更新轴位置和速度
            SetDynamicAxisM(AxisM);

            // 轴启动到位置
            axisM.Move(AxisM);

            if (PawType == PawType.Vacuum)
            {
                GetVDevice<VVacuum>(Vacuum, out var vacuum);
                if (VacuumType == VacuumType.Sucking)
                {
                    // 3、吸真空
                    vacuum.Suck();

                    // 真空检
                    vacuum.InVacuumSensorCheck(OverTime, true);
                }
                else if (VacuumType == VacuumType.Blow)
                {
                    // 8、破真空
                    vacuum.Blow();

                    // 真空检
                    vacuum.InVacuumSensorCheck(OverTime, false);

                    vacuum.Close();
                }
            }
            else if (PawType == PawType.Cylinder)
            {
                GetVDevice<VCylinder>(Cylinder, out var cylinder);
                if (CylinderType == CylinderType.Extend)
                {
                    // 3、伸出
                    cylinder.Extend();

                    // 伸出完成检查
                    cylinder.InExtendSensorCheck(OverTime);

                }
                else if (VacuumType == VacuumType.Blow)
                {
                    // 3、吸真空
                    cylinder.Retract();

                    // 真空检
                    cylinder.InExtendSensorCheck(OverTime);
                }
            }
            else if (PawType == PawType.IODevice)
            {
                GetVDevice<VIOSimulation>(IOSimulation, out var ioDevice);

                if (string.IsNullOrEmpty(Action))
                {
                    errMsg = "动作是必填的";
                    return false;
                }

                ioDevice.Action(Action);

                // 感知动作是否完成
                if (!string.IsNullOrEmpty(Check))
                {
                    ioDevice.Check(Check, OverTime);
                }
            }
            else if (PawType == PawType.EleCylinder)
            {
                GetVDevice<VPCylinder>(EleCylinder, out var cDevice);

                // 点位
                cDevice.Move(PointNo);
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
            if (parameter.Name == "IOSimulation")
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
            else if (parameter.Name == "AxisM")
            {
                BuildDynamicAxisM(newV as VAxisMDevice);
            }
        }
    }
}