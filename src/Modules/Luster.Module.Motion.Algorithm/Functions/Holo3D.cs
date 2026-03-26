#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       Station
* 机器名称:       L05123-NB
* 命名空间:       Luster.Module.Motion.Logic.Functions
* 文 件 名:       Station.cs
* 创建时间:       2022/5/30 8:50:12
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      88c51280-b8cb-42ce-9f4e-7af2cccd305b
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/30 8:50:12
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.DataModels;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Functions;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Enums;
using Luster.TaskFlow.Motion.Interfaces;
using Luster.TaskFlow.Motion.Logic;
using Luster.TaskFlow.Motion.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Luster.TaskFlow.Engine;
using System.IO;
using Luster.Common.Tools;

namespace Luster.Module.Motion.Logic.Functions
{
    /// <summary>
    /// 点云对象
    /// </summary>
    public class Holo3D : MotionFunction, IHolo3D
    {
        [Parameter("3D点云任务", 0, CN = "3D任务", EditorType = typeof(LPath), Filter = "(点云)|*.3dProj")]
        public string TaskPath { get; set; }

        /// <summary>
        /// 3D的引擎
        /// </summary>
        public object Holo3DEngine { get; set; }

        /// <summary>
        /// 引擎对象
        /// </summary>
        private ITaskFlowEngine hEngine = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        public Holo3D()
        {
            this.Icon = "\xe65a";
            this.Tips = "3D点云算法平台";

            // 支持动态参数
            this.DynParam = true;
        }

        /// <summary>
        /// 流程运行
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public override bool DoExcute(out string errMsg)
        {
            errMsg = "";
            if (hEngine == null)
            {
                errMsg = "3D引擎未配置";
                return false;
            }

            // 构建点云对象
            foreach (var item in MyOwner.Parameters)
            {
                if (item.Value.Property != null) continue;
                hEngine.SetInput(item.Key, item.Value.Value);
            }

            bool isResult = hEngine.RunAll();
            //获取3D结果
            GetMeasureOutParams();
            return isResult;
        }

        public override void OnNotifyPropertyUIChanged(ParameterAttribute parameter, object newV)
        {
            base.OnNotifyPropertyUIChanged(parameter, newV);
            if (parameter.Name == nameof(TaskPath))
            {
                if (hEngine == null)
                {
                    hEngine = new TaskFlowEngine(MyOwner.TaskModules.Factory);
                    hEngine.LogEvent -= HEngine_LogEvent;
                    hEngine.LogEvent += HEngine_LogEvent;

                    // 参数变更
                    hEngine.ParameterUpdateEvent -= HEngine_ParameterUpdateEvent;
                    hEngine.ParameterUpdateEvent += HEngine_ParameterUpdateEvent;
                }

                // 加载任务
                if (newV is LPath p)
                {
                    // 第一次创建的场景
                    if (!File.Exists(p.Path))
                    {
                        var newXML = hEngine.ExportXml();
                        newXML.Save(p.Path);
                    }

                    hEngine.LoadProjfile(p.Path);

                    // 动态构建参数
                    BuildDynInParams(hEngine);
                }

                // 配置临时变量，供外面使用
                Holo3DEngine = hEngine;
            }
        }

        /// <summary>
        /// 参数变更,需要重新动态构建参数
        /// </summary>
        /// <param name="obj"></param>
        private void HEngine_ParameterUpdateEvent(ITaskFlowEngine obj)
        {
            BuildDynInParams(obj);
        }

        /// <summary>
        /// 构建动态输入和输出参数
        /// </summary>
        /// <param name="taskFlow"></param>
        private void BuildDynInParams(ITaskFlowEngine taskFlow)
        {
            var ps = taskFlow.GetParameterInfo();
            int sort = 1;
            bool isChange = false;

            List<string> removeItems = new List<string>();

            // 如果不存在新增
            foreach (var item in ps)
            {
                if (!MyOwner.Parameters.ContainsKey(item.Label))
                {
                    var newP = item.ToParameter(MyOwner, sort);
                    newP.CanRef = ParamRef.Ref;
                    Owner.Parameters.Add(item.Label, newP);
                    isChange = true;
                }
            }

            // 如果有删除也要进行动态检查
            foreach (var item in Owner.Parameters)
            {
                if (item.Value.Property != null) continue;

                if (!ps.Any(u => u.Label == item.Key))
                {
                    removeItems.Add(item.Key);
                }
            }

            if (removeItems.Count > 0)
            {
                foreach (var item in removeItems)
                {
                    MyOwner.Parameters.Remove(item);
                }

                isChange = true;
            }

            // 如果参数变更了，那么就通知程序刷新熟悉
            if (isChange)
            {
                MyOwner.OnUpdate(TaskFlow.Common.Enums.ModuleUpdate.ParameterNum);
            }
        }

        /// <summary>
        /// 日志注册
        /// </summary>
        /// <param name="logType">日志类型</param>
        /// <param name="message">错误消息</param>
        /// <param name="arg3"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void HEngine_LogEvent(Common.DataStruct.Enums.LogType logType, string message, string arg3)
        {
            LogTool.Log(logType, message, "Holo3D");
        }

        private void GetMeasureOutParams()
        {
            //获取结果
            List<ParameterInfo> parameterInfos = hEngine.GetOutputs();
            if (parameterInfos.Count > 0)
            {
                for (int i = 0; i < parameterInfos.Count; i++)
                {
                    foreach (var item in Owner.Parameters)
                    {
                        if (item.Key == parameterInfos[i].Label)
                        {
                            //测量结果输出
                            if (parameterInfos[i].Value is LTolerance tolerance)
                            {
                                Owner.Parameters[item.Key].Value = tolerance.Measure;
                            }
                            else
                            {
                                Owner.Parameters[item.Key].Value = parameterInfos[i].Value.ToString();
                            }
                        }
                    }
                }
            }
        }
    }
}