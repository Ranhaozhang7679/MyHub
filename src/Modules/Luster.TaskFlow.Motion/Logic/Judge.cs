#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       OKProcess
* 机器名称:       L05123-NB
* 命名空间:       Luster.Module.Motion.Logic.Functions
* 文 件 名:       OKProcess.cs
* 创建时间:       2022/8/7 16:42:07
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      bdf4e885-6ce1-4246-b534-0e1741058a39
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/8/7 16:42:07
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.Tools;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Models;
using Luster.TaskFlow.Motion.Interfaces;
using Luster.TaskFlow.Motion.Logic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.TaskFlow.Motion.Logic
{
    /// <summary>
    /// If else 分支
    /// </summary>
    public interface IBranch
    {
        LExpression Express { get; set; }
    }

    /// <summary>
    /// 对结果进行判断
    /// </summary>
    public class Judge : Group, IBranch, IWait
    {
        /// <summary>
        /// 条件表达式
        /// </summary>
        [NotEmpty]
        [Parameter("条件表达式", 0, CN = "条件")]
        public new LExpression Express { get; set; }

        /// <summary>
        /// 是否持续等待结果为真
        /// </summary>
        [Parameter("是否持续等待", 1, CN = "等待为真", DefaultV = false)]
        public bool IsWaitTrue { get; set; }

        /// <summary>
        /// 是否满足
        /// </summary>
        [Parameter("表达式是否满足条件", 3, CN = "是否满足", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool IsTrue { get; set; }

        public Judge()
        {
            this.Tips = "等价于If Else，支持对条件为真结果进行等待";
            this.Icon = "\xe6a5";
        }

        /// <summary>
        /// 函数执行
        /// </summary>
        /// <param name="errMsg">错误消息</param>
        /// <returns>运行结果</returns>
        public override bool DoExcute(out string errMsg)
        {
            errMsg = "";

            if (MyOwner.Children.Count < 2)
            {
                errMsg = "表达式必须包含两个节点";
                return false;
            }

            if (IsWaitTrue)
            {
                WaitFunc(() => Express.GetResult(MyOwner), $"等待 {Express}", 30);
            }
            else
            {
                // 表达式是否满足结果
                IsTrue = Express.GetResult(MyOwner);
            }

            // 最终运行结果
            bool isSuccess = true;

            // 如果条件满足，执行子节点
            int excuteNum = IsTrue ? 0 : 1;

            if (MyOwner.Children.Count > 0)
            {
                // 1.获取Start
                var startModule = MyOwner.Children[excuteNum];

                // 2.程序运行
                motionRunEngine.Run(startModule, ref isSuccess);

                // 3.运行失败，获取错误消息
                if (!isSuccess)
                {
                    errMsg = motionRunEngine.ErrorMessage;
                }
            }

            return isSuccess;
        }
    }
}