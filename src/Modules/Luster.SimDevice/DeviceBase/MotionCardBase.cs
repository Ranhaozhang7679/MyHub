#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       MotionCardBase
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.MotionCards
* 文 件 名:       MotionCardBase.cs
* 创建时间:       2022/4/22 18:34:01
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      66096040-470d-4fa8-bcb5-af7bfee3806c
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/22 18:34:01
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Motion.DataStruct.Enums;
using Luster.SimDevice.Real;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.MotionCards
{
    public abstract class MotionCardBase : DeviceBase
    {
        /// <summary>
        /// 设备名称
        /// </summary>
        public override DeviceType DeviceType => DeviceType.MotionCard;

        /// <summary>
        /// 厂商信息
        /// </summary>
        public override string Brand { get; }

        /// <summary>
        /// 图标
        /// </summary>
        public override string Icon => "\xe6d8";

        /// <summary>
        /// 设置轴当前位置
        /// </summary>
        /// <param name="axisNo">轴号</param>
        /// <param name="perPulse">脉冲当量</param>
        /// <param name="position">轴的位置，单位mm或角度</param>
        public virtual void SetCurrentPos(int axisNo, double perPulse, double position)
        {
        }

        /// <summary>
        /// 检测回零是否完成
        /// </summary>
        /// <returns></returns>
        public virtual bool CheckHomeDone(int axisNo = 0)
        {
            return true;
        }

        /// <summary>
        /// 清理急停信号
        /// </summary>
        public virtual void ClearEmg()
        { }
    }
}