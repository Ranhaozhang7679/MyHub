#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LogEvent
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.EngineUI.Events
* 文 件 名:       LogEvent.cs
* 创建时间:       2022/4/22 11:17:30
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      2fe25055-6af2-4f7f-9b6e-083d67b31034
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/22 11:17:30
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.DataModels;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.EngineUI.Events
{
    
    public class AlertEvent : PubSubEvent<LogInfo>
    {
    }

    public class LogEvent : PubSubEvent<LogInfo>
    {

    }
}