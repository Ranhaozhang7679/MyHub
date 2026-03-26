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

using Luster.Common.DataStruct.Attributes;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Logics;
using Luster.TaskFlow.Common.Module;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Logic;
using Luster.TaskFlow.Motion.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Stations
{
    public class FreeStation : StationFunction, IFreeStation
    {

        /// <summary>
        /// 所处行
        /// </summary>
        [Parameter("是否启用工站", 0, CN = "工站启用", DefaultV = true, CanRef = ParamRef.Ref)]
        public bool IsEnabled { get; set; }


        /// <summary>
        /// 所处行
        /// </summary>
        [Parameter("启动后，跳转至第一步", 0, CN = "工站跳转启用", DefaultV = false, CanRef = ParamRef.Ref)]
        public bool IsReturnEnabled { get; set; }

        [DependOn("IsReturnEnabled", true)]
        [Parameter("启动后，跳转至第一步", 1, CN = "工站跳转", EditorType = typeof(IGlobal))]
        public String IsReturn { get; set; }

        /// <summary>
        /// 隶属工站
        /// </summary>
        [Parameter("所属模块", 1, CN = "所属模块",DefaultV ="System")]
        public string Module { get; set; }


        /// <summary>
        /// 本站要料
        /// </summary>
        [Parameter("本站有料", 11, CN = "本站要料", ParamType = TaskFlow.Common.Enums.ParamType.OUT, DefaultV = false)]
        public bool ThisGet { get; set; }

        /// <summary>
        /// 本站有料
        /// </summary>
        [Parameter("本站有料", 12, CN = "本站有料", ParamType = TaskFlow.Common.Enums.ParamType.OUT, DefaultV = false)]
        public bool ThisHave { get; set; }


        /// <summary>
        /// 构造函数
        /// </summary>
        public FreeStation()
        {
            this.Icon = "\xe696";
            this.Tips = "自由工站，不检查有料状态，需要自己通过全局变量控制有无料";

        }


        public override bool DoExcute(out string errMsg)
        {
            bool isSuccess = false;
            errMsg = string.Empty;
 

            // 工站是否启用
            if (!IsEnabled)
            {
                MyOwner.OnLog(Common.DataStruct.Enums.LogType.Debug, $"Station : {MyOwner.Alias} is disable!");
                return true;
            }

            // 1.如果有上游工站，那么等待上游有料
            // WaitFunc(() => CheckPrevHaved(out var havedStation), "等待上游有料");

            // 2.更新当前的缓存数据
            if (TryPeek(out var dataID))
            {
                MyOwner.DataID = dataID;
            }
            else
            {
                MyOwner.DataID = "";
            }

            // 3.运行子模块
            if (MyOwner.Children.Count > 0)
            {
                // 3.1 获取第一个子模块
                var startModule = MyOwner.Children[0];

                // 3.2 程序运行（递归运行）
                motionRunEngine.Run(startModule, ref isSuccess);

                // 3.3 运行失败，获取错误消息
                if (!isSuccess)
                {
                    errMsg = motionRunEngine.ErrorMessage;
                    return false;
                }
            }

            OnStationTime();

            return string.IsNullOrEmpty(errMsg);
        }


        /// <summary>
        /// 获取模块是否可以使用
        /// </summary>
        /// <returns></returns>
        public bool GetIsEnabled()
        {
            var p = MyOwner.Parameters[nameof(IsEnabled)];
            if (p != null && bool.TryParse(p.GetValue(out var errMsg)?.ToString(), out var v))
            {
                return v;
            }

            return true;
        }

        /// <summary>
        /// 获取模块跳转对应的全局变量名称
        /// </summary>
        /// <returns></returns>
        public string GetReturnValName()
        {
            return IsReturn;
        }


        #region 模块跳转
        /// <summary>
        /// 触发GoTo
        /// </summary>
        public bool IsGoTo(IMotionModule curModule)
        {
            // 需要跳转
            bool isGoTo = goModule != null;

            if (isGoTo)
            {
                if (curModule == goModule)
                {
                    isGoTo = false;
                    goModule = null;
                    MyOwner.OnLog(Common.DataStruct.Enums.LogType.Info, $"工站跳转结束->{curModule.Alias}");
                }
            }

            return isGoTo;
        }

        private IMotionModule goModule = null;

        /// <summary>
        /// 模块跳转
        /// </summary>
        /// <param name="goToModule">模块跳转</param>
        public void GoTo(IMotionModule goToModule)
        {
            // 中断当前Break
            goModule = goToModule;
        }
        #endregion

        /// <summary>
        /// 递归中断循环
        /// </summary>
        /// <param name="m">模块</param>
        /// <param name="isBreak">是否中断</param>
        private void SetBreak(IMotionModule m, bool isBreak)
        {
            foreach (var item in m.Children)
            {
                item.IsBreak = isBreak;
                foreach (var sub in item.Children)
                {
                    SetBreak(m, isBreak);
                }
            }
        }

        /// <summary>
        /// 清理临时变量
        /// </summary>
        public override void ClearDatas()
        {
            base.ClearDatas();
            goModule = null;
        }
    }
}