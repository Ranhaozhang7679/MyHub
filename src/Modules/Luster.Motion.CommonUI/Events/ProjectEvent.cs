#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ProjectEvent
* 机器名称:       L05590
* 命名空间:       Luster.Motion.CommonUI.Events
* 文 件 名:       ProjectEvent.cs
* 创建时间:       2022/8/25 13:38:02
* 作    者:       L05590
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       fanlu5590@lusterinc.com 
* 唯一标识：      f09847e6-d1a3-4825-a8f5-2a4968700ce8
* 登录用户:       fanlu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/8/25 13:38:02
* 修 改 人:		  L05590
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
    /// 打开工程
    /// </summary>
    public class ProjectOpenEvent : PubSubEvent<ProjectInfo>
    {

    }

    /// <summary>
    /// 工程数量变换
    /// </summary>
    public class ProjectListChangeEvent : PubSubEvent
    {

    }

    /// <summary>
    /// 工程名
    /// </summary>
    public class ProjectNameEvent : PubSubEvent<string>
    {

    }

    public class ProjectChangeEvent : PubSubEvent<ProjectInfo>
    {

    }

}