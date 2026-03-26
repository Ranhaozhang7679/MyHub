#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       IModuleCollection
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Common.Module
* 文 件 名:       IModuleCollection
* 创建时间:       2021/10/29 16:29:24
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      0b94b09a-6188-49fd-9b63-d0a930118fa5
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/10/29 16:29:24
* 修 改 人:		  luster
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.TaskFlow.Common.Module
{
    public interface IModuleCollection
    {
        /// <summary>
        /// 通过字符串来获取模块
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>IModule</returns>
        IModule this[string id] { get; }

        /// <summary>
        /// 通过Guid来获取模块
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>IModule</returns>
        IModule this[Guid id] { get; }

        /// <summary>
        /// 通过Id来判断是否包含该模块
        /// </summary>
        /// <param name="moduleID">moduleID object id</param>
        /// <returns>True if contains, false if not contains</returns>
        bool Contains(Guid moduleID);

        /// <summary>
        /// 隶属工程
        /// </summary>
        IModuleFactory Factory { get; }
    }
}
