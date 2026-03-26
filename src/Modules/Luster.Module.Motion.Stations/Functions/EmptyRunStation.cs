using Luster.TaskFlow.Motion.Logic;
using Luster.TaskFlow.Motion.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Stations.Functions
{
    public class EmptyRunStation : StationFunction, IEmptyStation
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public EmptyRunStation()
        {
            this.Icon = "\xe642";
            this.Tips = "空跑工站，用于空跑任务";
        }

        /// <summary>
        /// 获取上游的有料站
        /// </summary>
        /// <param name="havedStation"></param>
        /// <returns></returns>
        public virtual bool CheckPrevHaved(out IProStation havedStation)
        {
            havedStation = null;
            return true;
        }

        public override bool DoExcute(out string errMsg)
        {
            bool isSuccess = false;
            errMsg = string.Empty;

            // 只有空跑才能够运行
            if (!IsEmptyMode) return true;

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
