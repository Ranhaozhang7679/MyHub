using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion.Logic;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Stations.Functions
{
    public class BackgroundStation:StationFunction, IBackGroundStation
    {


        /// <summary>
        /// 工站启用
        /// </summary>
        [Parameter("是否启用工站", 0, CN = "工站启用", DefaultV = true, CanRef = ParamRef.Ref)]
        public bool IsEnabled { get; set; }



        [Range(0, 100000)]
        [Parameter("休息时间，防止资源占用,默认为10ms,单位ms", 1, CN = "休息时间", DefaultV = 10, CanRef = ParamRef.Ref)]
        public int SleepTime { get; set; }
        /// <summary>
        /// 构造函数
        /// </summary>
        public BackgroundStation()
        {
            this.Icon = "\xe6dd";
            this.Tips = "后台工站，一直运行";
        }


        public override bool DoExcute(out string errMsg)
        {
            bool isSuccess = false;
            errMsg = string.Empty;

            // 工站是否启用
            if (!IsEnabled)
            {
                //MyOwner.OnLog(Common.DataStruct.Enums.LogType.Debug, $"Station : {MyOwner.Alias} is disable!");
                Thread.Sleep(SleepTime);
                return true;
            }

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
            Thread.Sleep(SleepTime);
            return string.IsNullOrEmpty(errMsg);
        }

    }
}
