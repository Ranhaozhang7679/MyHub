#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       RunType
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Engine.Enums
* 文 件 名:       RunType
* 创建时间:       2021/11/2 10:21:38
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      4805bd4a-abb5-4cb9-ac61-600a646fa0e0
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/11/2 10:21:38
* 修 改 人:		  luster
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.TaskFlow.Engine.Enums
{
    public enum RunType
    {
        /// <summary>
        /// 单步执行
        /// </summary>
        RunOne,

        /// <summary>
        /// 执行
        /// </summary>
        Run,
    }
}
