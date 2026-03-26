#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ProjectInfoEvent
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.CommonUI.Events
* 文 件 名:       ProjectInfoEvent.cs
* 创建时间:       2022/6/27 20:47:02
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      d287de78-2a55-44e0-9d5b-978ad82f7cac
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/27 20:47:02
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Motion.CommonUI.Models;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.CommonUI.Events
{
    /// <summary>
    /// 新建解决方案
    /// </summary>
    public class SolutionNewEvent : PubSubEvent<ProjectInfo>
    {
    }

    /// <summary>
    /// 新建解决方案
    /// </summary>
    public class SolutionUpdateEvent : PubSubEvent<ProjectInfo>
    {
    }

    /// <summary>
    /// 打开项目/默认为当前项目
    /// </summary>
    public class SolutionOpenEvent : PubSubEvent<ProjectInfo>
    {
    }
}