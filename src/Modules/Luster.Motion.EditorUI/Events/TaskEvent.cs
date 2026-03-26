#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       TaskEvent
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.EditorUI.Events
* 文 件 名:       TaskEvent.cs
* 创建时间:       2022/5/31 16:17:56
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      35d30edb-8e1c-40bb-a43e-3b487ecc8a23
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/31 16:17:56
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.EditorUI.Events
{
    public class TaskPrevEvent : PubSubEvent
    {

    }

    public class TaskPostEvent : PubSubEvent
    {

    }

    /// <summary>
    /// 任务变更
    /// </summary>
    public class TaskChangedEvent : PubSubEvent
    {

    }
}