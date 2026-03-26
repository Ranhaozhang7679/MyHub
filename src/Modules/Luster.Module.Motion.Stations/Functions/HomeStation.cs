#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       HomeStation
* 机器名称:       L05123-NB
* 命名空间:       Luster.Module.Motion.Stations.Functions
* 文 件 名:       HomeStation.cs
* 创建时间:       2022/8/18 18:51:30
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      910bd5c9-294b-498d-83a7-6ec7b7de0edc
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/8/18 18:51:30
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Interfaces;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion.Enums;
using Luster.TaskFlow.Motion.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Stations.Functions
{
    public class HomeStation : StationFunction, IHomeStation
    {
        /// <summary>
        /// 永远有料
        /// </summary>
        public override bool Haved
        {
            get
            {
                return true;
            }
            set
            {
            }
        }

        /// <summary>
        /// 属于类型
        /// </summary>
        public HomeType HomeType => HomeType.Home;

        [Parameter("回零超时时间，单位s", 1, CN = "回零超时", DefaultV = -1)]
        public int Overtime { get; set; }

        /// <summary>
        /// 当前进度信息
        /// </summary>
        private int _currentVal = 0;

        /// <summary>
        /// 回零进度
        /// <para>总数</para>
        /// <para>当前运行</para>
        /// <para>描述信息</para>
        /// </summary>
        public event ProgressDelegateHandler HomeProgressEvent;

        /// <summary>
        /// 构造函数
        /// </summary>
        public HomeStation()
        {
            this.Icon = "\xe642";
            this.Tips = "回零工站，用于回零特殊任务;该站存在，不再对硬件进行回零";
        }

        private int time = 0;

        /// <summary>
        /// 结束异步线程
        /// </summary>
        private CancellationTokenSource _tokenS = null;

        /// <summary>
        /// 方法运行即时
        /// </summary>
        private void TimeCount()
        {
          
            time = 0;
            _tokenS = new CancellationTokenSource();
            Task.Run(() =>
            {
                while (true)
                {
                    if (_tokenS.IsCancellationRequested)
                    {
                        break;
                    }

                    time += 1000;
                    Thread.Sleep(1000);

                    if (time > (Overtime * 1000))
                    {
                        motionRunEngine.Timeout();
                        MyOwner.StatusMsg = $"{MyOwner.Alias}运行超时:{time}>{Overtime}";
                        time = 0;
                        break;
                    }
                }
            }, _tokenS.Token);
        }

        public override bool DoExcute(out string errMsg)
        {
            bool isSuccess = false;
            errMsg = string.Empty;
            _currentVal = 0;

            if (Overtime > 0)
            {
                TimeCount();
            }

            motionRunEngine.ProgressEvent -= MotionRunEngine_ProgressEvent;
            motionRunEngine.ProgressEvent += MotionRunEngine_ProgressEvent;




            // 2.运行子模块
            if (MyOwner.Children.Count > 0)
            {
                // 2.1 获取第一个子模块
                var startModule = MyOwner.Children[0];

                // 2.2 程序运行（递归运行）
                motionRunEngine.Run(startModule, ref isSuccess);

                // 2.3 结束耗时监控
                _tokenS?.Cancel();

                // 2.3 运行失败，获取错误消息
                if (!isSuccess)
                {
                    errMsg = motionRunEngine.ErrorMessage;
                    return false;
                }
            }

            return string.IsNullOrEmpty(errMsg);
        }

        /// <summary>
        /// 回零进度
        /// </summary>
        /// <param name="obj"></param>
        private void MotionRunEngine_ProgressEvent(TaskFlow.Motion.IMotionModule obj)
        {
            int total = MyOwner.Children.Count;

            if (MyOwner.Children.Contains(obj))
            {
                _currentVal++;
                HomeProgressEvent?.Invoke(total, ref _currentVal, $"模块:{obj.Alias}回零完成");
            }
        }
    }
}