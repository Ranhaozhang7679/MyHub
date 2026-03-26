#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       DeviceType
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataStruct._6.Enums
* 文 件 名:       DeviceType.cs
* 创建时间:       2022/4/13 16:48:24
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      93c39a70-3777-4446-9952-4925728d0802
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/13 16:48:24
* 修 改 人:		  L05123
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
    /// <summary>
    /// 硬件设备
    /// </summary>
    public enum DeviceType
    {
        [Description("相机")]
        Camera,

        [Description("控制卡")]
        MotionCard,

        [Description("激光")]
        LineLaser,

        [Description("光源控制器")]
        LightController,

        [Description("打印机")]
        Printer,

        [Description("机器人")]
        Robot
    }
}