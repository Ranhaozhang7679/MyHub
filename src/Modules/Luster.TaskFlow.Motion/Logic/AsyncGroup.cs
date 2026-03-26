#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 接口名称:       IAsync
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Motion.Logic
* 文 件 名:       IAsync.cs
* 创建时间:       2022/5/20 9:07:44
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      0f2920fe-52d7-417a-b20b-27d70501d948
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/20 9:07:44
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.TaskFlow.Common.Logics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.TaskFlow.Motion.Logic
{
    /// <summary>
    /// 异步模
    /// </summary>
    public interface IAsync
    {
    }

    public class AsyncGroup : LogicFunction, IAsync
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public AsyncGroup() : base()
        {
            this.Tips = "异步模组";
            this.Icon = "\xe6a2";
        }

        /// <summary>
        /// 分组模块处理
        /// </summary>
        /// <param name="errMsg">错误消息</param>
        /// <returns>是否运行成功</returns>
        public override bool DoExcute(out string errMsg)
        {
            bool isSuccess = true;

            errMsg = String.Empty;

            if (MyOwner.Children.Count > 0)
            {
                Task.Run(() =>
                {
                    // 1.获取Start
                    var startModule = MyOwner.Children[0];

                    // 2.程序运行
                    motionRunEngine.Run(startModule, ref isSuccess);
                });
            }

            return isSuccess;
        }
    }
}