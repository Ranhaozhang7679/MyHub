#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 接口名称:       IWindowSubSystem
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.DataStruct.Interfaces
* 文 件 名:       IWindowSubSystem.cs
* 创建时间:       2022/3/29 16:29:51
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      426a3af9-5fe4-4b7d-87e3-9dff62a9550d
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/3/29 16:29:51
* 修 改 人:		  L05123
************************************************************************************/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.DataStruct.Interfaces
{
    /// <summary>
    /// 子系统接口
    /// </summary>
    public interface IWindowSubSystem
    {
        /// <summary>
        /// 事件保存
        /// </summary>
        event Action SaveEvent;
    }
}