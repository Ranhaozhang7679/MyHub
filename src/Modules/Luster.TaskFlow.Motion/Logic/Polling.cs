#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       GroupFunction
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Motion.Functions
* 文 件 名:       GroupFunction.cs
* 创建时间:       2022/5/24 9:52:41
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      9beb6bda-d09f-483b-b30c-bd5c0611cd88
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/24 9:52:41
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.TaskFlow.Common.Logics;
using Luster.TaskFlow.Motion.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.TaskFlow.Motion.Logic
{
    /// <summary>
    /// 轮询模组
    /// </summary>
    public interface IPolling
    {
    }

    public class Polling : LogicFunction, IPolling
    {
        /// <summary>
        /// 索引信息
        /// </summary>
        public static int PollingIndex = 0;

        /// <summary>
        /// 构造函数
        /// </summary>
        public Polling() : base()
        {
        }

        /// <summary>
        /// 分组模块处理
        /// </summary>
        /// <param name="errMsg">错误消息</param>
        /// <returns>是否运行成功</returns>
        public override bool DoExcute(out string errMsg)
        {
            bool isSuccess = false;

            errMsg = String.Empty;

            if (Owner.Children.Count == 0)
            {
                errMsg = "模块未包含要轮巡的子模块";
                return false;
            }

            int mCount = MyOwner.Children.Count;
            if (mCount <= PollingIndex)
            {
                errMsg = $"轮巡的索引:{PollingIndex} 大于子模块数量:{Owner.Children}";
                return true;
            }

            // 1.获取Start
            var startModule = MyOwner.Children[PollingIndex];

            // 2.程序运行
            motionRunEngine.Run(startModule, ref isSuccess);

            // 3.运行失败，获取错误消息
            if (!isSuccess)
            {
                errMsg = motionRunEngine.ErrorMessage;
            }

            // 4.更新索引信息
            PollingIndex++;
            if (PollingIndex == mCount)
            {
                // 防止数组越界
                PollingIndex = 0;
            }

            return isSuccess;
        }
    }
}