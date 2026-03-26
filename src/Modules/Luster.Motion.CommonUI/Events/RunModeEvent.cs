#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       RunModeEvent
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.CommonUI.Events
* 文 件 名:       RunModeEvent.cs
* 创建时间:       2022/9/8 9:04:50
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      10e87958-006f-46ef-b422-e9e255c5c933
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/8 9:04:50
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Motion.DataStruct.Enums;
using Luster.Motion.TaskFlow.Engine.Models;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.CommonUI.Events
{
    public class RunModeEvent : PubSubEvent<DeviceMode>
    {
        public object Sender { get; set; }
    }

    /// <summary>
    /// 运行模式改变
    /// </summary>
    public class RunModeChangeEvent : PubSubEvent<RunModeModel>
    {
    }

    /// <summary>
    /// 机台模式变换
    /// </summary>
    public class MachineModeEvent : PubSubEvent<MachineMode>
    {

    }

    /// <summary>
    /// 生产模式变换
    /// </summary>
    public class ProductModeEvent : PubSubEvent<string>
    {

    }

    public class MachineMode
    {
        /// <summary>
        /// 旧模式
        /// </summary>
        public string OldMode { get; set; }

        /// <summary>
        /// 新模式
        /// </summary>
        public string NewMode { get; set; }
    }
}