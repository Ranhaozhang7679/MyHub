#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       RunMode
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Motion.Enums
* 文 件 名:       RunMode.cs
* 创建时间:       2022/6/13 10:32:51
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      384a3688-528d-4ffe-8b10-502bc37de6fd
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/13 10:32:51
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.TaskFlow.Motion.Enums
{
    /// <summary>
    /// 运行模式
    /// </summary>
    public enum MotionMode
    {
        [Description("正常模式")]
        Normal,

        [Description("首件模式")]
        FAI,

        [Description("CPK模式")]
        CPK,

        [Description("固定次数空跑")]
        ARR,

        [Description("空跑模式")]
        Empty,

        [Description("重复跑")]
        GRR
    }
}