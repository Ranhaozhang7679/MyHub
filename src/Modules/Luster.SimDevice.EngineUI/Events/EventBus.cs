#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       EventBus
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.SubSystem.Events
* 文 件 名:       EventBus.cs
* 创建时间:       2022/4/15 18:06:39
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      98a186f5-48bd-438d-9ee3-d9089c664cdc
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/15 18:06:39
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.SubSystem.Events
{
    /// <summary>
    /// 事件总线
    /// </summary>
    public class EventBus
    {
        /// <summary>
        /// 事件总线
        /// </summary>
        private IEventAggregator eventBus;

        private EventBus(IEventAggregator eventBus)
        {
        }
    }
}