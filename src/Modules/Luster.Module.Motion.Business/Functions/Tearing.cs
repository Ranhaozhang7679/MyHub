#region 作者和版本
/*----------------------------------------------------------------
 * 版权所有 (c) 2022 P R C  保留所有权利。
 * CLR版本：4.0.30319.42000
 * 机器名称：D05149
 * 公司名称：P R C
 * 命名空间：Luster.Module.Motion.Business.Functions
 * 唯一标识：d1c65462-df5f-4227-897d-d901764bc2ad
 * 文件名：Tearing
 * 当前用户域：LUSTERINC
 * 
 * 创建者：D05149
 * 电子邮箱：huidong@lusterinc.com
 * 创建时间：2022/7/26 11:18:34
 * 版本：V1.0.0
 * 描述：
 *
 * ----------------------------------------------------------------
 * 修改人：
 * 时间：
 * 修改说明：
 *
 * 版本：V1.0.1
 *----------------------------------------------------------------*/
#endregion 作者和版本

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luster.TaskFlow.Motion;
using Luster.Common.DataStruct.DataModels;
using Luster.Motion.DataStruct.DataModels;
using Luster.TaskFlow.Common.Attributes;
using Luster.Common.DataStruct.Attributes;

namespace Luster.Module.Motion.Business.Functions
{
    /// <summary>
    /// 撕膜业务
    /// </summary>
    public class Tearing: MotionFunction
    {
        [Parameter("是否含有撕膜前后气缸", 0, CN = "有无气缸", DefaultV = false)]
        public bool IsHaveTearingFrontCyl { get; set; }

        [Parameter("仿真气缸类型",1,CN ="撕膜夹爪气缸",EditorType =typeof(VCylinder))]
        public VDevice TearingClawCyl { get; set; }

        [Parameter("仿真气缸类型", 1, CN = "撕膜升降气缸", EditorType = typeof(VCylinder))]
        public VDevice TearingUpCyl { get; set; }

        [DependOn("IsHaveTearingFrontCyl", true)]
        [Parameter("仿真气缸类型", 1, CN = "撕膜前后气缸", EditorType = typeof(VCylinder))]
        public VDevice TearingFrontCyl { get; set; }

        [Parameter("轴类型", 2, CN = "CG抬升Z轴", EditorType = typeof(VAxis))]
        public VDevice ZAxis { get; set; }

        [Parameter("仿真气缸类型", 3, CN = "CG抬升气缸", EditorType = typeof(VCylinder))]
        public VDevice CGUpCyl { get; set; }

        [Parameter("Z轴抬升高度", 2, CN = "Z轴抬升高度")]
        public double ZUpDistance { get; set;}

        [Parameter("Z轴抬升速度", 2, CN = "Z轴抬升速度")]
        public double ZUpSpeed { get; set; }

        public Tearing()
        {
            this.Icon = "\xe67e";
            this.Tips = "撕膜";
        }

        public override bool DoExcute(out string errMsg)
        {
            ///0.解析硬件
            GetVDevice<VCylinder>(TearingClawCyl, out var tearingClawCyl);
            GetVDevice<VCylinder>(TearingUpCyl, out var tearingUpCyl);
            GetVDevice<VCylinder>(TearingFrontCyl, out var tearingFrontCyl);
            GetVDevice<VAxis>(ZAxis, out var zAxis);
            GetVDevice<VCylinder>(CGUpCyl, out var cGUpCyl);

            ///1、撕膜模块气缸复位
            ///1.1升降气缸复位
            tearingUpCyl.Extend();
            tearingUpCyl.InExtendSensorCheck(3000);
            ///1.2前后气缸复位
            if (IsHaveTearingFrontCyl)
            {
                tearingFrontCyl.Retract();
                tearingFrontCyl.InRetractSensorCheck(3000);
            }
            ///1.3夹爪气缸复位
            tearingClawCyl.Extend();
            tearingClawCyl.InExtendSensorCheck(3000);

            ///1.4Cg夹取气缸复位
            cGUpCyl.Extend();
            cGUpCyl.InRetractSensorCheck(3000);

            ///2.Z轴下降、气缸夹紧CG
            zAxis.MoveRel(-ZUpDistance, ZUpSpeed);
            zAxis.CheckMotionDone();

            cGUpCyl.Retract();
            cGUpCyl.InRetractSensorCheck(3000);

            ///3.夹爪气缸夹紧膜
            tearingClawCyl.Retract();
            tearingClawCyl.InRetractSensorCheck(3000);

            ///4.Z轴抬升
            zAxis.MoveRel(ZUpDistance,ZUpSpeed);
            zAxis.CheckMotionDone();

            ///5.撕膜部分去抛料,可以是异步动作
            Task.Run(() =>
            {
                if (IsHaveTearingFrontCyl)
                {
                    ///5.1 撕膜前后气缸动作
                    tearingFrontCyl.Extend();
                    tearingFrontCyl.InExtendSensorCheck(3000);
                }

                ///5.2 撕膜上下气缸动作
                tearingUpCyl.Retract();
                tearingUpCyl.InRetractSensorCheck(3000);

                ///5.3 撕膜夹爪气缸动作
                tearingClawCyl.Extend();
                tearingClawCyl.InExtendSensorCheck(3000);
            });

            ///6.撕膜部分复位,可以是异步动作
            Task.Run(() =>
            {
                ///6.1 撕膜上下气缸动作
                tearingUpCyl.Extend();
                tearingUpCyl.InExtendSensorCheck(3000);

                if (IsHaveTearingFrontCyl)
                {
                    ///6.2 撕膜前后气缸动作
                    tearingFrontCyl.Retract();
                    tearingFrontCyl.InRetractSensorCheck(3000);
                }
            });

            return base.DoExcute(out errMsg);
        }
    }
}


