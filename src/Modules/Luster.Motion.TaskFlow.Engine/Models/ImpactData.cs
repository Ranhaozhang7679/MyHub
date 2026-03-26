#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ImpactData
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.TaskFlow.Engine.Models
* 文 件 名:       ImpactData.cs
* 创建时间:       2022/7/27 9:29:19
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      930be909-b714-4896-be44-51e74cefc656
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/27 9:29:19
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.TaskFlow.Engine.Models
{
    /// <summary>
    /// 稼动率统计
    /// </summary>
    public class ImpactData
    {
        /// <summary>
        /// 总时间 当前班别第一次启动的时间到目前时间 s
        /// </summary>
        public double RunTime { get; set; }

        /// <summary>
        /// 维护时间 设备异常的花费时间 s
        /// </summary>
        public double PMTime { get; set; }

        /// <summary>
        /// 待机时间
        /// </summary>
        public double IdleTime { get; set; }

        /// <summary>
        /// 停机时间 当前班别的暂停到启动的时间s
        /// </summary>
        public double StopTime { get; set; }

        /// <summary>
        /// 抛料时间 固定值
        /// </summary>
        public double TossingTime { get; set; }

        /// <summary>
        /// 实际运行时间
        /// </summary>
        public double ActualRunTimeTime { get; set; }

        /// <summary>
        /// 软件启动时间
        /// </summary>
        private DateTime _softStartTime = DateTime.Now;

        public ImpactData()
        {
            _softStartTime = DateTime.MinValue;
        }

        /// <summary>
        /// 启动时间
        /// </summary>
        /// <param name="startTime">软件启动时间</param>
        public ImpactData(DateTime softStartTime)
        {
            ClearAllTime();
            _softStartTime = softStartTime;
        }

        /// <summary>
        /// 获取稼动率 理想状态100
        /// </summary>
        /// <returns></returns>
        public double GetImpactPer()
        {
            RunTime = GetRunTime();

            // 记录当前停止时间
            var stopTime = StopTime + GetStopingTime();
            if (RunTime > 0 && _softStartTime != DateTime.MinValue)
            {
                var actualTime = RunTime - stopTime - PMTime - TossingTime - IdleTime;
                if (actualTime < 0)
                {
                    actualTime = 1;
                }

                return Math.Round(actualTime / RunTime * 100, 2);
            }
            else
            {
                return 100;
            }
        }

        /// <summary>
        /// 计算实际运行时间
        /// </summary>
        /// <returns></returns>
        public double GetActualRunTime()
        {
            RunTime = GetRunTime();

            // 记录当前停止时间
            var stopTime = StopTime + GetStopingTime();
            if (RunTime > 0 && _softStartTime != DateTime.MinValue)
            {
                var actualTime = RunTime - stopTime - PMTime - IdleTime - TossingTime;
                if (actualTime < 0)
                {
                    actualTime = 1;
                }

                return Math.Round(actualTime, 2);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// 清除当前时间统计
        /// </summary>
        public void ClearAllTime()
        {
            RunTime = 0;
            PMTime = 0;
            IdleTime = 0;
            StopTime = 0;
            TossingTime = 0;
            _softStartTime = DateTime.MinValue;
            _softPauseTime = DateTime.MinValue;
            _idleTime = DateTime.MinValue;
        }

        /// <summary>
        /// 获取软件停止的时间
        /// </summary>
        /// <returns></returns>
        public double GetStopingTime()
        {
            if (_softPauseTime == DateTime.MinValue) return 0;

            return (DateTime.Now - _softPauseTime).TotalSeconds;
        }

        private DateTime _idleTime = DateTime.MinValue;
        public void UpdateIdleTime()
        {
            _idleTime = DateTime.Now;
        }

        #region 停止和恢复时间
        /// <summary>
        /// 记录暂停时间
        /// </summary>
        private DateTime _softPauseTime = DateTime.MinValue;
        public void UpdatePauseTime()
        {
            // 程序如果先暂停->再点停止，此时只记录第一次的暂停时间
            if (_softPauseTime != DateTime.MinValue) return;

            _softPauseTime = DateTime.Now;
        }

        /// <summary>
        /// 更新恢复时间
        /// </summary>
        /// <param name="now"></param>
        public void UpdateRecoveryTime()
        {
            if (_softPauseTime == DateTime.MinValue) return;
            var timeSpan = DateTime.Now - _softPauseTime;

            // 记录停止时间
            StopTime += timeSpan.TotalSeconds;

            _softPauseTime = DateTime.MinValue;
        }
        #endregion

        /// <summary>
        /// 获取运行时间
        /// </summary>
        /// <returns></returns>
        public double GetRunTime()
        {
            if (_softStartTime == DateTime.MinValue) return 0;
            var timeSpan = DateTime.Now - _softStartTime;
            return timeSpan.TotalSeconds;
        }
    }
}