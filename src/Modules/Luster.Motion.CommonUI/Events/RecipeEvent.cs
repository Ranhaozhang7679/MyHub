#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       RecipeEvent
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.EditorUI.Events
* 文 件 名:       RecipeEvent.cs
* 创建时间:       2022/5/31 15:14:31
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      41f5e0fa-9901-449f-ab1e-2567a53e83e4
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/31 15:14:31
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
    /// 新建配方
    /// </summary>
    public class ProjectNewEvent : PubSubEvent<Recipe>
    {
    }

    /// <summary>
    /// 打开配方之前
    /// </summary>
    public class RecipePrevOpenEvent : PubSubEvent<Recipe>
    {

    }

    /// <summary>
    /// 打开某一个配方事件
    /// </summary>
    public class RecipeOpenEvent : PubSubEvent<Recipe>
    {

    }

    /// <summary>
    /// 配方改变
    /// </summary>
    public class RecipeChangedEvent : PubSubEvent<Recipe>
    {

    }
}