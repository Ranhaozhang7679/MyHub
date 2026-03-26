#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       FeedStation
* 机器名称:       L05123-NB
* 命名空间:       Luster.Module.Motion.Stations.Functions
* 文 件 名:       FeedStation.cs
* 创建时间:       2022/7/6 8:39:24
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      1a63fd7b-a82a-4a57-8e2d-039e3060cd1f
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/6 8:39:24
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Motion.DataStruct.DataModels;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Interfaces;
using Luster.TaskFlow.Motion.Logic;
using Luster.TaskFlow.Motion.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Stations
{
    public class TestStation : StationFunction, ITestStation
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public TestStation()
        {
            this.Icon = "\xe6be";
            this.Tips = "测试工站，自动运行时不运动";
        }

        public override bool DoExcute(out string errMsg)
        {
            bool isSuccess = false;
            errMsg = string.Empty;

            // 2.运行子模块
            if (MyOwner.Children.Count > 0)
            {
                // 2.1 获取第一个子模块
                var startModule = MyOwner.Children[0];

                // 2.2 程序运行（递归运行）
                motionRunEngine.Run(startModule, ref isSuccess);

                // 2.3 运行失败，获取错误消息
                if (!isSuccess)
                {
                    errMsg = motionRunEngine.ErrorMessage;
                    return false;
                }
            }

            return string.IsNullOrEmpty(errMsg);
        }
    }
}