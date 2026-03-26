#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       HomePriority
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Motion.Enums
* 文 件 名:       HomePriority.cs
* 创建时间:       2022/5/18 17:52:29
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      7b03d074-039e-4eed-a164-8738febd6b6a
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/18 17:52:29
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
    /// 回零优先级
    /// </summary>
    public enum HomePriority
    {
        [Description("最高")]
        High,

        [Description("中级")]
        Median,

        [Description("低级")]
        Low
    }
}