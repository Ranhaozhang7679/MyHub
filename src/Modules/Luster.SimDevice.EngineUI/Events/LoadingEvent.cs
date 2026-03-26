#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LoadingEvent
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.EngineUI.Events
* 文 件 名:       LoadingEvent.cs
* 创建时间:       2022/5/13 13:46:50
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      626bc9c2-a099-4a9e-a531-f00d684bab71
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/13 13:46:50
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
    /// 
    /// </summary>
    public class LoadingEvent : PubSubEvent<LoadingModel>
    {
    }
}