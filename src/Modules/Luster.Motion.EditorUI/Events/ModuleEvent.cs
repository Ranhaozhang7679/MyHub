#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ModuleEvent
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.EditorUI.Events
* 文 件 名:       ModuleEvent.cs
* 创建时间:       2022/5/31 14:54:14
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      2d29c47c-dd98-4e20-ad7a-8daf4bc3805c
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/31 14:54:14
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Motion.TaskFlow.Engine;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Motion;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.EditorUI.Events
{
    /// <summary>
    /// 新增之前
    /// </summary>
    public class ModulePrevAddEvent : PubSubEvent<IMotionModule>
    {

    }

    /// <summary>
    /// 模块新增
    /// </summary>
    public class ModuleAddEvent : PubSubEvent<IMotionModule>
    {

    }

    /// <summary>
    /// 模块移动
    /// </summary>
    public class ModuleMovedEvent : PubSubEvent<IMotionModule[]>
    {

    }

    /// <summary>
    /// 模块选择
    /// </summary>
    public class ModuleSelectedEvent : PubSubEvent<IMotionModule[]>
    {

    }

    /// <summary>
    /// 模块删除
    /// </summary>
    public class ModuleRemoveEvent : PubSubEvent<IMotionModule[]>
    {

    }

    /// <summary>
    /// 模块忽略
    /// </summary>
    public class ModuleSkipEvent : PubSubEvent<IMotionModule[]>
    {

    }

    /// <summary>
    /// 模块运行
    /// </summary>
    public class ModuleRunOneEvent : PubSubEvent<IMotionModule>
    {

    }

    /// <summary>
    /// 模块运行前
    /// </summary>
    public class ModulePrevRunEvent : PubSubEvent
    {

    }

    /// <summary>
    /// 模块运行后
    /// </summary>
    public class ModulePostRunEvent : PubSubEvent<IMotionModule[]>
    {

    }

    /// <summary>
    /// 进入子模块
    /// </summary>
    public class ModuleLoadedEvent : PubSubEvent<IMotionModule>
    {

    }


    public class ModulePastedEvent : PubSubEvent<IMotionModule[]>
    {

    }

    public class CompareLookEvent : PubSubEvent<IMotionModule[]>
    {

    }

    /// <summary>
    /// 模块参数校验
    /// </summary>
    public class ModuleValidEvent : PubSubEvent<IMotionModule>
    {

    }

    /// <summary>
    /// 模块更新
    /// </summary>
    public class ModuleUpdateEvent : PubSubEvent<ModuleUpdateModule>
    {

    }

    /// <summary>
    /// 模块组合
    /// </summary>
    public class ModuleCompositionEvent : PubSubEvent<IMotionModule>
    {

    }

    public class ModuleUpdateModule
    {
        public IMotionModule Module { get; set; }

        public ModuleUpdate UpdateType { get; set; }
    }
}