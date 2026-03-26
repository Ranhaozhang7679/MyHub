#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       MoveType
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Motion.Enums
* 文 件 名:       MoveType.cs
* 创建时间:       2022/5/18 18:17:21
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      2f62ebc5-37d7-407e-a01a-30c12bbf1810
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/18 18:17:21
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
    /// 运行流向
    /// </summary>
    public enum MoveType
    {
        [Description("下一步")]
        Next,

        /// <summary>
        /// 如果物料NG，则跳转到其他步骤
        /// </summary>
        [Description("来料NG")]
        NG,

        /// <summary>
        /// 抓取失败，进入重试
        /// </summary>
        [Description("失败")]
        Fail,

        /// <summary>
        /// 运行超时
        /// </summary>
        [Description("超时")]
        OverTime,
    }
}