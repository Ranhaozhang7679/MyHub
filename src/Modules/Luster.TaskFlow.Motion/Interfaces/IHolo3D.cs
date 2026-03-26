#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       IHolo3D
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Motion.Interfaces
* 文 件 名:       IHolo3D.cs
* 创建时间:       2022/10/15 21:44:45
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      1dbb63a2-3ed5-4bcf-9bb7-cb23fd8e8e62
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/10/15 21:44:45
* 修 改 人:		  L05123
************************************************************************************/
#endregion
using Luster.Common.DataStruct.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.TaskFlow.Motion.Interfaces
{
    public interface IHolo3D
    {
        /// <summary>
        /// 任务路径
        /// </summary>
        string TaskPath { get; set; }

        /// <summary>
        /// 3D的引擎
        /// </summary>
        object Holo3DEngine { get; set; }
    }
}
