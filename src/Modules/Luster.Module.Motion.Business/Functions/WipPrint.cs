#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       WipPrint
* 机器名称:       Z05150
* 命名空间:       Luster.Module.Motion.Business.Functions
* 文 件 名:       WipPrint.cs
* 创建时间:       2022/6/29 9:46:49
* 作    者:       张宇
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       yuzhang5150@lusterinc.com 
* 唯一标识：      a57e28f8-26c0-4439-919a-535e195403ad
* 登录用户:       zhangyu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/29 9:46:49
* 修 改 人:		  Z05150
************************************************************************************/
#endregion

using Luster.Motion.DataStruct.DataModels;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Business.Functions
{
    /// <summary>
    /// WIP打印
    /// </summary>
    public class WipPrint : MotionFunction
    {

        /// <summary>
        /// 打印机设备
        /// </summary>
        [NotEmpty]
        //[Parameter("打印机设备", 0, CN = "打印机", EditorType = typeof(VPrinter))]
        public VDevice WipPrinter { get; set; }

        /// <summary>
        /// 有无气缸
        /// </summary>
        [Parameter("选择是否需要气缸", 0, CN = "有无气缸", DefaultV = true)]
        public bool HasCylinder { get; set; }

        /// <summary>
        /// 伸缩气缸设备
        /// </summary>
        [Parameter("打印机伸缩气缸", 1, CN = "伸缩气缸", EditorType = typeof(VCylinder))]
        public VDevice Cylinder { get; set; }

        /// <summary>
        /// 有无真空
        /// </summary>
        [Parameter("选择是否需要真空", 0, CN = "有无真空", DefaultV = true)]
        public bool HasVaccum { get; set; }

        /// <summary>
        /// 真空设备
        /// </summary>
        [Parameter("打印机真空", 2, CN = "真空", EditorType = typeof(VVacuum))]
        public VDevice Vaccum { get; set; }

        /// <summary>
        /// Wip码
        /// </summary>
        [Parameter("Wip码", 3, CN = "Wip码", DefaultV = "", CanRef = ParamRef.Ref)]
        public string WipCode { get; set; }

        /// <summary>
        /// 到料延时
        /// </summary>
        [Parameter("到料延时", 4, CN = "到料延时", DefaultV = 500)]
        public int DelayTime { get; set; }

        public WipPrint()
        {
            this.Tips = "Wip打印";
            this.Icon = "\xe75d";
        }

        public override bool DoExcute(out string errMsg)
        {
            //GetVDevice<VPrinter>(WipPrinter, out var wipPrinter);
            GetVDevice<VCylinder>(Cylinder, out var cylinder);
            GetVDevice<VVacuum>(Vaccum, out var vaccum);

            string wip = "";

            // 1、获取WIP码
            if (!wip.Equals(WipCode))
            {
                wip = WipCode;
                if (wip == null)
                {
                    wip = "no print content";
                }
            }

            if (HasCylinder)
            {
                // 气缸缩回
                cylinder.Retract();
                // 检测
                cylinder.InRetractSensorCheck(100);
            }

            // 3、打印
            //wipPrinter.Print(wip, DelayTime);

            if (HasVaccum)
            {
                // 4、吸真空
                vaccum.Suck();
                // 真空检
                vaccum.InVacuumSensorCheck(DelayTime, true);
            }

            if (HasCylinder)
            {
                // 5、气缸伸出
                cylinder.Extend();
                // 检测
                cylinder.InExtendSensorCheck(DelayTime);
            }

            // 6、有料
            // 7、等待搬运
            // 8、破真空
            // 9、关真空
            // 10、无料

            return base.DoExcute(out errMsg);
        }
    }
}
