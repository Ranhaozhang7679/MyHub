#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       GoToFunction
* 机器名称:       L05123-02
* 命名空间:       Luster.TaskFlow.Motion.Functions
* 文 件 名:       GoToFunction.cs
* 创建时间:       2022/12/16 11:41:25
* 作    者:       刘克志
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      872f3aa5-a552-4667-823e-dc75dfab40c7
* 登录用户:       刘克志
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/12/16 11:41:25
* 修 改 人:		  刘克志
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Motion.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;

namespace Luster.TaskFlow.Motion.Functions
{

    public interface IGoTo
    {
        void InitStatus();
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class StepAttribute : Attribute
    {
        /// <summary>
        /// 步骤名称
        /// </summary>
        public string StepName { get; set; }

        /// <summary>
        /// 执行的步序列
        /// </summary>
        public int StepSort { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public RunStatus Status { get; set; }

        /// <summary>
        /// 是否需要暂停
        /// </summary>
        public bool IsNeedPause { get; set; } = false;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="sort">排序</param>
        /// <param name="stepName">步骤名称</param>
        public StepAttribute(int sort, string stepName = "")
        {
            StepName = stepName;
            StepSort = sort;
        }

        /// <summary>
        /// 定义方法
        /// </summary>
        private MethodInfo methodInfo;
        public void SetMethod(MethodInfo methodInfo)
        {
            this.methodInfo = methodInfo;
            if (string.IsNullOrEmpty(StepName))
            {
                StepName = methodInfo.Name;
            }
        }

        public void Invoke(IMotionFunction function)
        {
            // Default 代表未运行，所以需要运行
            // Running 代表函数报警了，所以状态还是Running，下次进来还需要再次运行
            // Pause 代表主动暂停了，下次进来还需要再次运行该模块
            if (Status == RunStatus.Default || Status == RunStatus.Running || Status == RunStatus.Pause)
            {
                // 运行中
                Status = RunStatus.Running;

                methodInfo.Invoke(function, null);

                // 如果运行成功，就更新
                if (Status == RunStatus.Running)
                {
                    Status = RunStatus.Success;
                }
            }
        }

        public void SetDefault()
        {
            Status = RunStatus.Default;
        }
    }

    /// <summary>
    /// 基于步序进行跳转
    /// </summary>
    public class GoToFunction : MotionFunction, IPauseFunction, IGoTo
    {
        /// <summary>
        /// 步序列对象
        /// </summary>
        private readonly List<StepAttribute> StepMethods = new List<StepAttribute>();

        /// <summary>
        /// 初始化
        /// </summary>
        protected override void Init()
        {
            base.Init();

            StepMethods.Clear();

            // 方法列表
            var methods = GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public);

            List<StepAttribute> stepMethods = new List<StepAttribute>();
            foreach (var method in methods)
            {
                var methodAttr = method.GetCustomAttribute<StepAttribute>();
                if (methodAttr != null)
                {
                    methodAttr.SetMethod(method);

                    stepMethods.Add(methodAttr);
                }
            }

            foreach (var item in stepMethods.OrderBy(u => u.StepSort))
            {
                StepMethods.Add(item);
            }
        }

        /// <summary>
        /// 回零后，需要初始化状态
        /// </summary>
        public void InitStatus()
        {
            foreach (var item in StepMethods)
            {
                item.SetDefault();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public override bool DoExcute(out string errMsg)
        {
            foreach (var item in StepMethods)
            {
                MyOwner.OnLog(LogType.Debug, $"{MyOwner.Alias}:开始执行步骤:{item.StepName}");
                try
                {
                    item.Invoke(this);
                }
                catch (Exception e)
                {
                    // 返回真实的异常类型
                    if (e.InnerException != null)
                    {
                        throw e.InnerException;
                    }
                }

                MyOwner.OnLog(LogType.Debug, $"{MyOwner.Alias}:结束执行步骤:{item.StepName}");

                // 暂停跳出循环，并更新状态
                if (item.IsNeedPause && MyOwner.Status == RunStatus.Pause)
                {
                    item.Status = RunStatus.Pause;
                    break;
                }

                // 停止直接跳出循环
                if (MyOwner.Status == RunStatus.Stop)
                {
                    break;
                }
            }

            // 正常运行中，才将每个步骤的状态进行重置
            if (MyOwner.Status == RunStatus.Running || MyOwner.Status == RunStatus.Stop)
            {
                InitStatus();
            }

            return base.DoExcute(out errMsg);
        }

        public override bool IsNeedPause => base.IsNeedPause;
        /// <summary>
        /// 暂停
        /// </summary>
        //public override void Pause()
        //{
        //    // 自动解析参数，并调用设备
        //}

        //public override void Stop()
        //{

        //}
    }
}