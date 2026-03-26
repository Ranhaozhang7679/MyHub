#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ILogic
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Common.Logics
* 文 件 名:       ILogic
* 创建时间:       2021/11/6 21:02:27
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      c65ce6c9-0a99-4d61-bdda-86fe62c16d06
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/11/6 21:02:27
* 修 改 人:		  luster
************************************************************************************/
#endregion

using Luster.TaskFlow.Common.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.TaskFlow.Common.Logics
{
    public interface ILogic
    {
        /// <summary>
        /// 别名
        /// </summary>
        string Alias { get; }
    }
}
