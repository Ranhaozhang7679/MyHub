#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LogicFunction
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Motion.Logic
* 文 件 名:       LogicFunction.cs
* 创建时间:       2022/5/30 18:50:17
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      28eda5b5-1e1b-4434-93a8-a91b7df11f29
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/30 18:50:17
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.TaskFlow.Motion.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.TaskFlow.Motion.Logic
{
    public class LogicFunction : MotionFunction, IGroup
    {
        /// <summary>
        /// 运动引擎
        /// </summary>
        protected MotionRunEngine motionRunEngine;

        public LogicFunction()
        {
            motionRunEngine = new MotionRunEngine();
        }

        /// <summary>
        /// 忽略等待
        /// </summary>
        /// <param name="isSkip"></param>
        public void SetSkipWait(bool isSkip = false)
        {
            motionRunEngine.SetSkipWait(isSkip);
        }
    }
}