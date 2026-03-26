#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       PawType
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataStruct.Enums
* 文 件 名:       PawType.cs
* 创建时间:       2022/7/12 16:29:17
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      f9849098-54ca-4523-a4f0-9cb9928a8fab
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/12 16:29:17
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
    /// 夹爪类型
    /// </summary>
    public enum PawType
    {
        [Description("真空")]
        Vacuum,

        [Description("气缸")]
        Cylinder,

        [Description("IO仿真")]
        IODevice,

        /// <summary>
        /// 电缸
        /// </summary>
        [Description("电缸")]
        EleCylinder
    }
}