#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       HyperTrain
* 机器名称:       L05123-NB
* 命名空间:       Luster.Module.Motion.Business.Functions
* 文 件 名:       HyperTrain.cs
* 创建时间:       2022/9/27 21:28:05
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      28ede871-44b3-4024-8317-8aa37333921b
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/27 21:28:05
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.Tools;
using Luster.Motion.TaskFlow.Engine;
using Luster.Motion.TaskFlow.Engine.Models;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Business.Functions
{
    public enum MachineMode
    {
        [Description("生产模式")]
        Product,

        [Description("空跑模式")]
        Empty,
    }

    public class SetMachineMode : MotionFunction
    {
        /// <summary>
        /// 机台模式
        /// </summary>
        [Parameter("机台模式", 0, CN = "机台模式")]
        public MachineMode MachineMode { get; set; }

        /// <summary>
        /// 机台模式
        /// </summary>
        public SetMachineMode()
        {
            this.Tips = "模式配置";
            this.Icon = "\xe61c";
        }

        public override bool DoExcute(out string errMsg)
        {
            errMsg = "";
            var mController = MyOwner.Ioc.Resolve<IMotionController>();
            if (mController != null)
            {
                mController.SetCurrentMode(MachineMode.GetDescription().ToString());
            }

            return base.DoExcute(out errMsg);
        }
    }
}