#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       SingelAxisFlyShot
* 机器名称:       F05852
* 命名空间:       Luster.Module.Motion.Protocol.Functions
* 文 件 名:       SingelAxisFlyShot.cs
* 创建时间:       2022/11/1 9:54:04
* 作    者:       F05852
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       leifan@lusterinc.com 
* 唯一标识：      2d65d0b3-6fdb-445e-874f-b7fc5e930b19
* 登录用户:       樊磊
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/11/1 9:54:04
* 修 改 人:		  F05852
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Motion.Interfaces;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luster.Common.Tools;
using System.ComponentModel.DataAnnotations;

namespace Luster.Module.Motion.Business.Functions
{
    public class SingelAxisFlyShot : MotionFunction, IPauseFunction
    {
        [NotEmpty]
        [Parameter("轴类型，通过关键字搜索", 0, CN = "轴起点")]
        public VAxisDevice DeviceStart { get; set; }

        [NotEmpty]
        [Parameter("轴类型，通过关键字搜索", 0, CN = "轴终点")]
        public VAxisDevice DeviceEnd { get; set; }

        [NotEmpty]
        [Parameter("轴类型，通过关键字搜索", 0, CN = "轴喷码点")]
        public VAxisDevice DeviceShot { get; set; }

        [NotEmpty]
        [Parameter("IO类型，通过关键字搜索", 1, CN = "喷码IO点", EditorType = typeof(VIO))]
        public VDevice IODevice { get; set; }

        /// <summary>
        /// 正负10m
        /// </summary>
        [Parameter("位置,移动位置单位毫米", 5, CN = "喷码点位置补偿值", CanRef = ParamRef.Ref)]
        public double Compensate { get; set; }

        // 信号只触发一次，记录触发状态
        private bool bShotState = false;

        /// <summary>
        /// 构造函数
        /// </summary>
        public SingelAxisFlyShot()
        {
            this.Tips = "单轴喷码";
            this.Icon = "\xe678";
        }

        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="errMsg">错误消息</param>
        /// <returns></returns>
        public override bool DoExcute(out string errMsg)
        {
            GetVDevice<VAxis>(DeviceStart, out var axisStart);
            GetVDevice<VAxis>(DeviceEnd, out var axisEnd);
            GetVDevice<VIO>(IODevice, out var io);
            if (axisStart == null)
            {
                errMsg = $"设备:{DeviceStart.Name}未找到";
                return false;
            }
            if (axisEnd == null)
            {
                errMsg = $"设备:{DeviceStart.Name}未找到";
                return false;
            }

            // 走到喷墨起始点
            axisStart.MoveAbs(DeviceStart);
            axisStart.CheckMotionDone();

            // 喷码点位置获取
            double positionShot = DeviceShot.Position + Compensate;

            // 走到喷墨终点，同时在过程中触发喷码
            axisEnd.MoveAbs(DeviceEnd);
            axisEnd.CheckMotionDone(DeviceEnd, 45, curPosition =>
            {
                if (curPosition >= positionShot && !bShotState)
                {
                    io.SetDigital(true);
                    bShotState = true;
                }
            });

            // 关闭喷墨触发信号
            if (bShotState)
            {
                io.SetDigital(false);
                bShotState = false;
            }

            return base.DoExcute(out errMsg);
        }

        public override bool IsNeedPause => true;
    }

}