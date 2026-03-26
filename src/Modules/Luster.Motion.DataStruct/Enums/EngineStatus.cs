#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       EngineStatus
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.TaskFlow.Engine
* 文 件 名:       EngineStatus.cs
* 创建时间:       2022/5/19 16:25:40
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      db732006-c419-4ff4-bce3-c4eb8e5762f8
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/19 16:25:40
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct
{
    /// <summary>
    /// 引擎运行状态
    /// </summary>
    [Flags]
    public enum EngineStatus
    {
        [Description("空闲中")]
        Idle = 0,

        [Description("回零中")]
        Homing = 1,

        [Description("待启动")]
        Ready = 2,

        [Description("运行中")]
        Running = 4,

        [Description("报警中")]
        Alarm = 8,

        [Description("暂停中")]
        Pause = 16,

        [Description("停止中")]
        Stop =32,

        /// <summary>
        /// 机台复位中，只能进行停止操作
        /// </summary>
        [Description("复位中")]
        Resetting = 64,

        /// <summary>
        /// 90s无料切idle，仅用于界面显示，不参与状态切换
        /// </summary>
        [Description("待料中")]
        MaterialPending = 128
    }
}