using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Stations.Functions
{
    public class StartStation:StationFunction, IStartStation
    {


        public StartStation() 
        {
            this.Icon = "\xe6de";
            this.Tips = "开始工站，用于启动后首先执行，且执行一次";
        }

        public override bool DoExcute(out string errMsg)
        {
            bool isSuccess = false;
            errMsg = string.Empty;

            // 2.运行子模块
            if(MyOwner.Children.Count>0)
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
