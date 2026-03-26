#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       RunStatus
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Common.Enums
* 文 件 名:       RunStatus
* 创建时间:       2021/10/30 10:52:42
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      67f8e594-b166-4af7-8027-96ca6eee3d67
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/10/30 10:52:42
* 修 改 人:		  luster
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.TaskFlow.Common.Enums
{
    /// <summary>
    /// 运行状态
    /// </summary>
    public enum RunStatus
    {
        /// <summary>
        /// 模块初始化
        /// </summary>
        [Description("空闲")]
        Default,

        /// <summary>
        /// 运行中
        /// </summary>
        [Description("运行中")]
        Running,

        /// <summary>
        /// 运行成功
        /// </summary>
        [Description("成功")]
        Success,

        /// <summary>
        /// 初始失败状态
        /// </summary>
        [Description("失败")]
        Error,

        /// <summary>
        /// 报警状态
        /// </summary>
        [Description("报警")]
        Alarmed,

        /// <summary>
        /// 超时
        /// </summary>
        [Description("超时")]
        TimeOut,

        /// <summary>
        /// 忽略状态
        /// </summary>
        [Description("忽略")]
        Skip,

        /// <summary>
        /// 暂停状态
        /// </summary>
        [Description("暂停")]
        Pause,

        /// <summary>
        /// 停止状态
        /// </summary>
        [Description("停止")]
        Stop
    }
}
