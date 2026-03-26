#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       OverTimeFunction
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Motion.Functions
* 文 件 名:       OverTimeFunction.cs
* 创建时间:       2022/6/14 15:43:57
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      02da0d4c-3b8b-4ff5-b7df-f2ba6c30a9a7
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/14 15:43:57
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.TaskFlow.Motion
{
    /// <summary>
    /// 超时功能
    /// </summary>
    public class OverTimeFunction : MotionFunction, IOverTime
    {
        [Limit(0, 1000000)]
        [Parameter("超时时间,单位ms", 20, CN = "超时时间", DefaultV = 5000)]
        public virtual int OverTime { get; set; }
    }
}