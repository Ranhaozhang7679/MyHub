#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       MapDataEvent
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.CommonUI.Events
* 文 件 名:       MapDataEvent.cs
* 创建时间:       2022/7/27 13:57:29
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      03b91747-fb6e-45a1-964b-5eb972d4d3c3
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/27 13:57:29
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using Luster.Motion.TaskFlow.Engine.Models;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.CommonUI.Events
{
    public class MapDataEvent : PubSubEvent<MapData>
    {
      
    }

    public class MapUpdateEvent : PubSubEvent<List<MapData>>
    {

    }
}
