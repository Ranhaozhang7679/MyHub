#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LineLaserBase
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.LineLasers
* 文 件 名:       LineLaserBase.cs
* 创建时间:       2022/4/11 14:38:51
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      b6e0085a-8599-4e50-9e68-b12d133cdbc5
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/11 14:38:51
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Motion.DataStruct.Enums;
using Luster.SimDevice.Real;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice
{
    public abstract class LineLaserBase : DeviceBase
    {
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
        public override DeviceType DeviceType => DeviceType.LineLaser;

        [Description("序列号")]
        public string SN { get; set; }

        [Description("帧率")]
        public int Frame { get; set; }

        [Description("当前配置ID")]
        public string CurJobID { get; set; }

        [Description("激光线亮度")]
        public int LineBrightness { get; set; }

        [Description("增益")]
        public float Gain { get; set; }

        [Description("曝光时间")]
        public double Exposure { get; set; }

        [Description("采集行数")]
        public int RowNum { get; set; }

        [Description("点云行间隔距离")]
        public double RowDis { get; set; }

        [Description("一条点云线的点数")]
        public int ColNum { get; set; }

        [Description("一条点云线的点间距")]
        public double ColDis { get; set; }

        [Description("触发开关（软触发、硬件触发）")]
        public LaserStartTrigger StartTriggerMode { get; set; }
        
        [Description("数据触发模式（时间、硬件IO、编码器）")]
        public LaserDataTrigger DataTriggerMode { get; set; }
              
    }
}