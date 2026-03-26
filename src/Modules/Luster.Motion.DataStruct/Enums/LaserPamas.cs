#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LaserPamas
* 机器名称:       L05014-NB
* 命名空间:       Luster.Motion.DataStruct.Enums
* 文 件 名:       LaserPamas.cs
* 创建时间:       2022/10/12 22:27:17
* 作    者:       L05014
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       yongli@lusterinc.com
* 唯一标识：      b708b7f6-b321-4612-9a24-9c0e8a8a1d1b
* 登录用户:       liyong
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/10/12 22:27:17
* 修 改 人:		  L05014
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct.Enums
{
    public enum LaserPamas
    {
        [Description("触发开关（软触发、硬件触发）")]
        StartTriggerMode,

        [Description("数据触发模式（时间、硬件IO、编码器）")]
        DataTriggerMode,

        [Description("帧率")]
        Frame,

        [Description("曝光")]
        Exposure,

        [Description("增益")]
        Gain,

        [Description("数据模式（点云、轮廓）")]
        DataMode,

        [Description("单次回调获取的轮廓数")]
        RowNum,

        [Description("轮廓间隔距离")]
        RowDis,

        [Description("一条轮廓线的点数")]
        ColNum,

        [Description("一条轮廓线的点间距")]
        ColDis,

    }

    public enum LaserStartTrigger
    {
        [Description("软触发")]
        SoftWare,

        [Description("硬触发")]
        Hardware,
    }

    public enum LaserDataTrigger
    {
        [Description("编码器")]
        Encode,
        [Description("硬件IO")]
        IO,
        [Description("时间")]
        Time,
    }

    public enum LaserDataMode
    {
        [Description("点云")]
        PointCloud,
        [Description("轮廓")]
        Profile,
    }

    public enum SoftTriggerMode
    {
        [Description("单次")]
        Single,
        [Description("持续")]
        Continuous,
    }
}