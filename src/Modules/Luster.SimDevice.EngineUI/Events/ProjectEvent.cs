#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ProjectEvent
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.EngineUI.Events
* 文 件 名:       ProjectEvent.cs
* 创建时间:       2022/4/26 9:36:43
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      8c7907fd-9af0-4825-bf7c-81d70e122ab5
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/26 9:36:43
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.SimDevice.EngineUI.Models;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.EngineUI.Events
{
    /// <summary>
    /// 项目新增事件
    /// </summary>
    public class ProjNewEvent : PubSubEvent<ProjectInfo>
    {

    }

    /// <summary>
    /// 项目打开事件
    /// </summary>
    public class ProjOpenEvent : PubSubEvent<ProjectInfo>
    {

    }
}