#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       CloseEvent
* 机器名称:       L05590
* 命名空间:       Luster.Motion.CommonUI.Events
* 文 件 名:       CloseEvent.cs
* 创建时间:       2022/8/23 15:25:36
* 作    者:       L05590
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       fanlu5590@lusterinc.com 
* 唯一标识：      eb99f0f4-4b36-4e45-9c2f-1e05d290e657
* 登录用户:       fanlu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/8/23 15:25:36
* 修 改 人:		  L05590
************************************************************************************/
#endregion

using Prism.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.CommonUI.Events
{
    public class CloseEvent:PubSubEvent
    {
    }

    public class WinMinEvent : PubSubEvent
    {
    }

    public class WinMaxEvent : PubSubEvent
    {
    }
}