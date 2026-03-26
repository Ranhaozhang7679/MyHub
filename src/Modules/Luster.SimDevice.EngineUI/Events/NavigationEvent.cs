#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       SelectDeviceEvent
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.SubSystem.Events
* 文 件 名:       SelectDeviceEvent.cs
* 创建时间:       2022/4/15 17:54:28
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      1301653a-e234-41ce-8184-eebc59576ede
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/15 17:54:28
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

namespace Luster.SimDevice.SubSystem.Events
{
    /// <summary>
    /// 切换设备事件
    /// </summary>
    public class NavigationEvent : PubSubEvent<PageModel>
    {

    }
}