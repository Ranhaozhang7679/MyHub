#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       CameraBase
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.Camera
* 文 件 名:       CameraBase.cs
* 创建时间:       2022/4/11 14:36:59
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      a604d775-d627-4a4e-9736-e55d898434e7
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/11 14:36:59
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Motion.DataStruct.Enums;
using Luster.SimDevice.Real;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice
{
    public abstract class CameraBase : DeviceBase
    {
        /// <summary>
        /// 序列号
        /// </summary>
        [PropItem(0, DisplayName = "序列号")]
        public string SerialNumber { get; set; } = "";

        /// <summary>
        /// 帧率
        /// </summary>
        [PropItem(1, DisplayName = "帧率", DecimalPlaces = 1)]
        public float FrameRate { get; set; } = 10;

        /// <summary>
        /// 曝光时间
        /// </summary>
        [PropItem(2, DisplayName = "曝光时间")]
        public float ExposureTime { get; set; } = 15000;

        /// <summary>
        /// 增益
        /// </summary>
        [PropItem(3, DisplayName = "增益")]
        public float Gain { get; set; } = 10;

        ///// <summary>
        ///// Gamma值
        ///// </summary>
        //[PropItem(4)]
        //public float Gamma { get; set; }

        /// <summary>
        /// 品牌
        /// </summary>
        public override string Brand => "";

        /// <summary>
        /// 图标
        /// </summary>
        public override string Icon => "\xe6d8";

        /// <summary>
        /// 设备类型
        /// </summary>
        public override DeviceType DeviceType => DeviceType.Camera;
    }
}