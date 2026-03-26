#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       BeltCarry
* 机器名称:       F04846
* 命名空间:       Luster.Module.Motion.Business.Functions
* 文 件 名:       BeltCarry.cs
* 创建时间:       2022/7/4 18:14:59
* 作    者:       房晶鹏
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       jingpengfang@lusterinc.com 
* 唯一标识：      e34c0d2a-b4f1-48c1-870b-27fe4f07980c
* 登录用户:       fangjingpeng
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/4 18:14:59
* 修 改 人:		  F04846
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Enums;
using Luster.TaskFlow.Motion.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Business.Functions
{
    /// <summary>
    /// 搬运皮带
    /// </summary>
    public class BeltCarry : MotionFunction
    {
        [Parameter("搬运皮带", 0, CN = "站别")]
        public StationPos StationPos { get; set; }

        [DependOn("StationPos", StationPos.Middle)]
        [Parameter("搬运皮带", 0, CN = "上站皮带")]
        public VAxisMDevice PrevBelt { get; set; }


        [NotEmpty]
        [Parameter("搬运皮带", 0, CN = "本站皮带")]
        public VAxisMDevice ThisBelt { get; set; }

        [Parameter("选择是否需要阻挡气缸", 0, CN = "有无气缸", DefaultV = true)]
        public bool HasCylinder { get; set; }

        [DependOn("HasCylinder", true)]
        [Parameter("IO类型，通过关键字搜索", 1, CN = "阻挡气缸", EditorType = typeof(VCylinder))]
        public VDevice Cylinder { get; set; }

        [NotEmpty]
        [Parameter("IO类型，通过关键字搜索", 2, CN = "来料检测", EditorType = typeof(VIO))]
        public VDevice ComeIO { get; set; }

        [NotEmpty]
        [Parameter("IO类型，通过关键字搜索", 3, CN = "到料检测", EditorType = typeof(VIO))]
        public VDevice PlaceIO { get; set; }


        #region 首站特有的参数
        //[DependOn("StationPos", StationPos.First)]
        //[Parameter("IO类型，通过关键字搜索", 3, CN = "本站要料", EditorType = typeof(VIO))]
        //public VDevice RequireIO { get; set; }

        //[DependOn("StationPos", StationPos.First)]
        //[Parameter("IO类型，通过关键字搜索", 3, CN = "上游有料", EditorType = typeof(VIO))]
        //public VDevice HavedIO { get; set; }
        #endregion

        #region 尾站特有的参数
        [DependOn("StationPos", StationPos.Last)]
        [Parameter("本站有料信号", 3, CN = "本站有料", EditorType = typeof(VIO))]
        public VDevice ThisHaveIO { get; set; }

        [DependOn("StationPos", StationPos.Last)]
        [Parameter("下游要料信号", 3, CN = "下游要料", EditorType = typeof(VIO))]
        public VDevice DownStreamRequireIO { get; set; }
        #endregion


        [NotEmpty]
        [Parameter("IO类型，通过关键字搜索", 5, CN = "到料延时", DefaultV = 500)]
        public int DelayTime { get; set; }

        public BeltCarry()
        {
            this.Tips = "搬运皮带";
            this.Icon = "\xe662";
        }

        public override bool DoExcute(out string errMsg)
        {
            errMsg = "";

            // 0.获取硬件设备
            GetVDevice<VIO>(PlaceIO, out var placeIO);

            GetVDevice<VIO>(ComeIO, out var comeIO);
            GetVDevice<VCylinder>(Cylinder, out var cylinder);
            GetVDevice<VAxisM>(ThisBelt, out var thisBelt);

            GetVDevice<VAxisM>(PrevBelt, out var prevBelt);

            //GetVDevice<VIO>(HavedIO, out var haved);
            //GetVDevice<VIO>(RequireIO, out var require);

            GetVDevice<VIO>(ThisHaveIO, out var thisHaveIO);
            GetVDevice<VIO>(DownStreamRequireIO, out var downStreamRequireIO);

            //SetDynamicAxisM(thisBelt); //皮带没有补偿值不需要

            if (StationPos == StationPos.First)
            {
                // 发送要料信号
                //require.SetDigital(true);

                // 等待上游
                //haved.WaitIO(true, -1);

                // 转皮带 使用JOG
                thisBelt.Jog(ThisBelt);
                MyOwner.OnLog(Common.DataStruct.Enums.LogType.Debug, $"首战皮带启动!");
            }
            else if (StationPos == StationPos.Middle)
            {
                // 转皮带 使用JOG
                prevBelt.Jog(PrevBelt);
                thisBelt.Jog(ThisBelt);
            }
            else
            {
                // 本站有料信号 ON
                thisHaveIO.SetDigital(true);

                // 等下游要料信号 ON
                downStreamRequireIO.WaitIO(true, -1);

                // 转皮带 使用JOG
                thisBelt.Jog(ThisBelt);

                // 等下游要料信号 OFF
                downStreamRequireIO.WaitIO(false, -1);

                // 本站有料信号 OFF
                thisHaveIO.SetDigital(false);

                // 关闭皮带 
                thisBelt.Stop();

                return string.IsNullOrEmpty(errMsg);
            }

            // 等来料
            comeIO.WaitIO(true, -1);

            // 有阻挡气缸，就抬起顶升气缸
            if (HasCylinder)
            {
                if (cylinder == null)
                {
                    errMsg = "阻挡气缸不能为空!";
                    return false;
                }

                // 升阻挡气缸
                cylinder.Extend();

                // 伸出检查
                cylinder.InExtendSensorCheck(1000);
            }

            // 等到料，等待时间要通过轴计算
            placeIO.WaitIO(true, 5000);

            // 延时确保气缸到位         
            Thread.Sleep(DelayTime);

            if (StationPos == StationPos.First)
            {
                // 关闭要料信号
                //require.SetDigital(false);
            }
            else if (StationPos == StationPos.Middle)
            {
                // 上游皮带停止
                prevBelt.Stop();
                MyOwner.OnLog(Common.DataStruct.Enums.LogType.Debug, $"中间站关闭上游皮带!");
            }

            // 关闭皮带 
            thisBelt.Stop();

            // 降气缸
            if (HasCylinder)
            {
                cylinder.Retract();
                // 缩回检查
                cylinder.InRetractSensorCheck(1000);
            }

            return string.IsNullOrEmpty(errMsg);
        }
    }
}