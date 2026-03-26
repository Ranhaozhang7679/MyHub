#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       MotionRun
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Motion.Modules
* 文 件 名:       MotionRun.cs
* 创建时间:       2022/5/24 8:41:09
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      620a3313-0451-4cbe-aeca-ea0efbf93f14
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/24 8:41:09
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Common.Module;
using Luster.TaskFlow.Motion.Enums;
using Luster.TaskFlow.Motion.Functions;
using Luster.TaskFlow.Motion.interfaces;
using Luster.TaskFlow.Motion.Interfaces;
using Luster.TaskFlow.Motion.Logic;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.TaskFlow.Motion.Modules
{
    /// <summary>
    /// 模块运行处理器
    /// </summary>
    public class MotionRunEngine: MotionFunction
    {
        /// <summary>
        /// 错误消息
        /// </summary>
        public string ErrorMessage = string.Empty;

        /// <summary>
        /// 上一站结果
        /// </summary>
        public StationResult Result { get; set; }

        /// <summary>
        /// 耗时统计
        /// </summary>
        private float timeConsuming = 0;

        /// <summary>
        /// 当前运行的Module
        /// </summary>
        private IMotionModule runningModule = null;

        /// <summary>
        /// 进度一下
        /// </summary>
        public event Action<IMotionModule> ProgressEvent;


        /// <summary>
        /// 运控控制
        /// </summary>
        private MotionFunction _mController;

        /// <summary>
        /// 对数据进行汇总
        /// </summary>
        public MotionRunEngine()
        {
            Result = new StationResult();
        }

        /// <summary>
        /// 获取当前还在运行的模块
        /// </summary>
        /// <returns></returns>
        public IMotionModule GetRunning()
        {
            return runningModule;
        }

        /// <summary>
        /// 全局变量
        /// </summary>
        private IMotionModule gModule = null;

        /// <summary>
        /// 注册报警事件
        /// </summary>
        /// <param name="runModule"></param>
        private void RegisterAlarm(IMotionModule runModule)
        {
            if (runModule.AlarmInfo != null)
            {
                runModule.AlarmInfo.AlarmProcEvent -= AlarmInfo_AlarmProcEvent;
                runModule.AlarmInfo.AlarmProcEvent += AlarmInfo_AlarmProcEvent;
                runModule.OnAlarm_Module();

                // 信息提示，继续执行
                if (runModule.AlarmInfo != null && (runModule.AlarmInfo.AlarmType == AlarmType.InfoTip ||
                    runModule.AlarmInfo.AlarmType == AlarmType.PopInfoTip))
                {
                    runModule.Status = Common.Enums.RunStatus.Success;
                }
            }
        }

        /// <summary>
        /// 对外暴露的报警挂载与上抛入口，统一由运行器处理
        /// </summary>
        /// <param name="runModule">产生报警的模块</param>
        public void RegisterAndRaiseAlarm(IMotionModule runModule)
        {
            RegisterAlarm(runModule);
        }

        /// <summary>
        /// 报警处理，等待报警完成
        /// </summary>
        /// <param name="runModule"></param>
        private void ProcessAlarm(IMotionModule runModule)
        {
            RegisterAlarm(runModule);
            //if (runModule.BrokenOff != null && !runModule.BrokenOff.IsSet && runModule.Parent.TaskFunction is not IStartStation&& runModule.Parent.Parent.TaskFunction is not IStartStation)
            if (runModule.BrokenOff != null && !runModule.BrokenOff.IsSet &&!IsStartStation(runModule))
            {
                if (!_isSkipWait)
                {
                    runModule.BrokenOff.Wait();
                }
            }
        }

        private bool AlarmInfo_AlarmProcEvent(object sender, Luster.Motion.DataStruct.Enums.AlarmProc alarmProc)
        {
            bool procResult = true;
            if (sender is not IMotionModule module) return procResult;

            switch (alarmProc)
            {
                case AlarmProc.Retry:

                    procResult = module.DoFunction();

                    if (!procResult)
                    {
                        Task.Run(() => { RegisterAlarm(module); });
                    }

                    break;
                case AlarmProc.Ng:

                    var nextModule = module.NextModule;
                    if (nextModule != null && nextModule.TaskFunction is IActionNG actionNG)
                    {
                        nextModule.OnLog(LogType.Debug, $"NG模块:{nextModule.Alias} 开始执行!");

                        actionNG.SetNg(nextModule, true);
                        nextModule.DoFunction();
                        actionNG.SetNg(nextModule, false);

                        // 如果为错误的情况，运行会中断
                        module.Status = Common.Enums.RunStatus.Success;
                        nextModule.OnLog(LogType.Warning, $"NG模块:{nextModule.Alias} 执行完成!");
                    }
                    break;
                case AlarmProc.Contine:
                    procResult = true;

                    // 清理报警
                    module.AlarmInfo = null;
                    module.Status = Common.Enums.RunStatus.Success;
                    module.OnLog(LogType.Warning, $"模块:{module.Alias} 异常:{module.StatusMsg}被忽略，模块状态调整为成功!");
                    break;
            }

            return procResult;
        }


        /// <summary>
        /// 参数
        /// </summary>
        private ParameterAttribute gParameter = null;

        /// <summary>
        /// 递归运行
        /// </summary>
        /// <param name="runModule"></param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public void Run(IMotionModule runModule, ref bool isSuccess)
        {
            // 没有中间站或者末尾节点
            if (runModule == null || runModule.IsBreak) return;

            // 针对回零超时
            if (_isTimeout)
            {
                isSuccess = false;
                ErrorMessage = "回零模块运行超时!";

                // 停止状态还原，防止下次无法运行
                _isTimeout = false;
                return;
            }

            // 流程要等待
            //if (runModule.BrokenOff != null && !runModule.BrokenOff.IsSet&&runModule.Parent.TaskFunction is not IStartStation&&
            //    (runModule.Parent.Parent.TaskFunction is not IStartStation))
            if (runModule.BrokenOff != null && !runModule.BrokenOff.IsSet &&!IsStartStation(runModule))
            {
                if (!_isSkipWait)
                {
                    runModule.OnLog(LogType.Debug, $"模块:{runModule.Alias} 被暂停!");
                    runModule.BrokenOff.Wait();
                    runModule.OnLog(LogType.Debug, $"模块:{runModule.Alias} 恢复运行!");
                }
            }

            if (runModule.Status != Common.Enums.RunStatus.Skip)
            {
                runningModule = runModule;

                // 模块运行
                isSuccess = runModule.DoFunction();
                if(runModule.Alias=="复检工站")
                {
                    runModule.OnLog(LogType.Debug, $"模块:{runModule.Alias} runModule.DoFunction");
                }
                ProcessAlarm(runModule);
                if (runModule.Alias == "复检工站")
                {
                    runModule.OnLog(LogType.Debug, $"模块:{runModule.Alias} ProcessAlarm");
                }
                // 等待处理完成后，再次获取
                isSuccess = runModule.Status == Common.Enums.RunStatus.Success;

                // 回零工站需要知道模块运行的进度信息
                ProgressEvent?.Invoke(runModule);
            }
            else
            {
                // 如果时忽略模块，更新当前结果为true
                isSuccess = true;
            }

            // 耗时统计
            timeConsuming += runModule.TimeConsuming;

            // 运行成功后，运行下一模块
            if (isSuccess)
            {
                if (runModule.Alias == "复检工站")
                {
                    runModule.OnLog(LogType.Debug, $"运行下一模块");
                }
                // 如果return 模块，直接结束后续的动作
                if (runModule.TaskFunction is IReturn r && r.IsReturn())
                {
                    return;
                }

                //暂停启动后，第一次需要跳转至第一步
                //获取全局模块

                if(runModule.Parent.TaskFunction is IFreeStation )
                {
                    if (gModule == null)
                    {
                        gModule = runModule.Parent.TaskModules[GlobalModule.GlobalID] as IMotionModule;
                    }
                    var freeStation = runModule.Parent.TaskFunction as IFreeStation;

                    //调转初始步启用
                    if (freeStation.IsReturnEnabled&&!string.IsNullOrEmpty(freeStation.IsReturn))
                    {
                        if (gParameter == null)
                        {
                            gParameter = gModule.Parameters[freeStation.IsReturn];
                        }
                        //实时拿到全局对象的值
                        object pVal = gParameter?.Value;
                        if (pVal.Equals(true))
                        {
                            gParameter.Value = gParameter.DefaultV;
                            return;
                        }
                    }
                }
                



                // 如果是跳转模块,直接跳转过去
                if (runModule.TaskFunction is IGoToFunction goToFunc && goToFunc.GetJump(out var jumpModule))
                {
                    //if (((GoToModule)(runModule.TaskFunction).))

                    Run(jumpModule, ref isSuccess);

                    return;
                }

                if (runModule.ModuleType == ModuleType.Start)
                {
                    Run(runModule.NextModule, ref isSuccess);
                }
                else if (runModule.ModuleType == ModuleType.Middle)
                {
                    Run(runModule.NextModule, ref isSuccess);
                }
                else
                {

                }
            }
            else
            {
                // 记录当前的错误信息
                ErrorMessage = runModule.StatusMsg;
                isSuccess = string.IsNullOrWhiteSpace(runModule.StatusMsg);
            }
        }

        private bool _isTimeout = false;

        /// <summary>
        /// 流程超时
        /// </summary>
        public void Timeout()
        {
            _isTimeout = true;
            if (runningModule.TaskFunction is IStopFunction stopFunction)
            {
                stopFunction.Stop();
            }
        }

        /// <summary>
        /// 获取耗时统计
        /// </summary>
        /// <returns></returns>
        public float GetTimeConsuming()
        {
            return timeConsuming;
        }

        /// <summary>
        /// 是否忽略等待，报警模块重试不能进行等待
        /// </summary>
        private bool _isSkipWait = false;
        public void SetSkipWait(bool isSkip = false)
        {
            _isSkipWait = isSkip;
        }

        private bool IsStartStation(IMotionModule runMoudule)
        {
            if (runMoudule.Parent.TaskFunction is IStartStation)
            {
                return true;
            }
            else if(runMoudule.Parent.TaskFunction is IStation)
            {
                return false;
            }
            else
            {
                runMoudule = runMoudule.Parent;
                return IsStartStation(runMoudule);
            }

        }
    }
}