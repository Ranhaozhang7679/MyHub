#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ModuleUpdate
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Common.Enums
* 文 件 名:       ModuleUpdate.cs
* 创建时间:       2021/11/18 13:09:51
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      808ea1ee-e3c9-4861-b03f-d671db338dd0
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/11/18 13:09:51
* 修 改 人:		  luster
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.TaskFlow.Common.Enums
{
    /// <summary>
    /// 模块更新的状态
    /// </summary>
    public enum ModuleUpdate
    {
        /// <summary>
        /// 位置变更
        /// </summary>
        Move,

        /// <summary>
        /// 活动颜色变更
        /// </summary>
        Color,

        /// <summary>
        /// 参数数量发送变更
        /// </summary>
        ParameterNum,

        /// <summary>
        /// 参数值发生变更
        /// </summary>
        ParameterVal,
    }
}
