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
using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Virtual;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Common.Functions;
using Luster.TaskFlow.Common.Interfaces;
using Luster.TaskFlow.Common.Logics;
using Luster.TaskFlow.Common.Models;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Enums;
using Luster.TaskFlow.Motion.Interfaces;
using Luster.TaskFlow.Motion.Logic;
using Luster.TaskFlow.Motion.Models;
using Luster.TaskFlow.Motion.Modules;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Logic.Functions
{
    /// <summary>
    /// 第一个工站
    /// </summary>
    public class CheckVariable : MotionFunction, IWait, IRefFunction, IPauseFunction
    {
        /// <summary>
        /// 变量的值
        /// </summary>
        [NotEmpty]
        [Parameter("全局变量对象", 1, CN = "全局变量", EditorType = typeof(IGlobalBool))]
        public string GlobalVar { get; set; }

        [Parameter("超时了是否蜂鸣并重试", 2, CN = "超时警告重试", DefaultV = false)]
        public bool IsNeedRetry { get; set; }

        /// <summary>
        /// 等待值
        /// </summary>
        [Parameter("等待全局变量的值和等待值保持一致", 2, CN = "等待值")]
        public bool Value { get; set; }

        [Parameter("等待成功后复位为原始值", 3, CN = "自动复位")]
        public bool IsResetValue { get; set; }

        [Limit(-1, 1000000)]
        [Parameter("超时时间，单位为s", 4, CN = "超时时间", DefaultV = -1)]
        public int OverTime { get; set; }


        [Parameter("将原始工站获取", 5, CN = "数据转移", DefaultV = DataTransType.None)]
        public DataTransType TransType { get; set; }

        [Parameter("数据源工站", 6, CN = "源工站")]
        public LStation SrcStation { get; set; }


        [Parameter("等待时间，单位为s", 12, CN = "等待时间", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public int WaitTime { get; set; } = 0;

        public override string[] NoteParams => new string[] { "等待", nameof(GlobalVar) };

        /// <summary>
        /// 全局变量
        /// </summary>
        //private IMotionModule gModule = null;

        /// <summary>
        /// 参数
        /// </summary>
        //private ParameterAttribute gParameter = null;

        //private static object lockObj = new object();

        /// <summary>
        /// 构造函数
        /// </summary>
        public CheckVariable()
        {
            this.Tips = "变量检查";
            this.Icon = "\xe6a6";
        }



        /// <summary>
        /// 对全局变量进行检查
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public override bool DoExcute(out string errMsg)
        {
            int waitOveTime = OverTime * 1000;

            DateTime begin = DateTime.Now;

            IMotionModule gModule = MyOwner.TaskModules[GlobalModule.GlobalID] as IMotionModule;
            if (!gModule.Parameters.ContainsKey(GlobalVar))
            {
                errMsg = $"全局变量:{GlobalVar}不存在!";
                return false;
            }

            ParameterAttribute gParameter = gModule.Parameters[GlobalVar];

            //if (gModule == null)
            //{
            //    gModule = MyOwner.TaskModules[GlobalModule.GlobalID] as IMotionModule;
            //}

            //if (gParameter == null)
            //{
            //    if (!gModule.Parameters.ContainsKey(GlobalVar))
            //    {
            //        errMsg = $"全局变量:{GlobalVar}不存在!";
            //        return false;
            //    }

            //    gParameter = gModule.Parameters[GlobalVar];
            //}

            //var pArray = GetParametersByType<string>();

            object pVal = gParameter?.Value;
            Action retryAction = null;
            if (IsNeedRetry)
            {
                retryAction = () =>
                {
                    MyOwner.LightManager.SetBuzzer(true, 500, 500);
                    OnAlarm(AlarmType.WarningTip, $"{MyOwner.Alias},等待变量超时,蜂鸣提示，再次等待");
                    WaitFunc(() =>
                    {
                        if (gParameter != null)
                        {
                            pVal = gParameter.Value;
                        }
                        else
                        {
                            if (gModule == null)
                            {
                                gModule = MyOwner.TaskModules[GlobalModule.GlobalID] as IMotionModule;
                            }

                            gParameter = gModule.Parameters[GlobalVar];
                            MyOwner.OnLog(Common.DataStruct.Enums.LogType.Warning, $"变量:{gParameter.CN} 丢失!");
                            pVal = gParameter?.Value;
                        }

                        return pVal.Equals(Value);

                    }, $"等待变量:{GlobalVar} == {Value}", 20, -1);
                };
            }
            try
            {
                // 等待
                WaitFunc(() =>
                {
                    if (gParameter != null)
                    {
                        pVal = gParameter.Value;
                    }
                    else
                    {
                        if (gModule == null)
                        {
                            gModule = MyOwner.TaskModules[GlobalModule.GlobalID] as IMotionModule;
                        }

                        gParameter = gModule.Parameters[GlobalVar];
                        MyOwner.OnLog(Common.DataStruct.Enums.LogType.Warning, $"变量:{gParameter.CN} 丢失!");
                        pVal = gParameter?.Value;
                    }

                    return pVal.Equals(Value);

                }, $"等待变量:{GlobalVar} == {Value}", 20, waitOveTime, retryAction);
            }
            catch (DeviceTimeoutException ex)
            {
                OnAlarm(AlarmType.WarningTip, $"等待变量{GlobalVar.Replace("Extend_", "")} == {Value}超时!");
            }

            // 数据转移
            if (SrcStation != null)
            {
                if (SrcStation.Tag == null)
                {
                    SrcStation.Tag = MyOwner.TaskModules[SrcStation.ID] as IMotionModule;
                }

                // 数据转移
                IMotionModule dstModule = SrcStation.Tag as IMotionModule;
                DataProcess(MyOwner, dstModule, TransType, 1);
            }

            // 等待成功后，复位变量
            if (IsResetValue)
            {
                // 防止复位失败
                //lock (lockObj)
                //{
                if (gParameter == null)
                {
                    gParameter = gModule.Parameters[GlobalVar];
                }

                gParameter.Value = gParameter.DefaultV;

                MyOwner.OnLog(Common.DataStruct.Enums.LogType.Debug, $"全局变量:{GlobalVar} 复位,Src={gParameter.Value} new value={gParameter.DefaultV}");
                // }
            }

            DateTime end = DateTime.Now;

            WaitTime = (int)(end - begin).TotalSeconds;

            return base.DoExcute(out errMsg);
        }

        public override void OnNotifyPropertyUIChanged(ParameterAttribute parameter, object newV)
        {
            base.OnNotifyPropertyUIChanged(parameter, newV);
            if (parameter.Name == "GlobalVar")
            {
                SetParameterVal(nameof(GlobalVar), newV);

                //gModule = null;
                //gParameter = null;
            }
        }

        /// <summary>
        /// 添加引用
        /// </summary>
        /// <returns></returns>
        public List<LReference> GetReferences()
        {
            List<LReference> lRefs = new List<LReference>();

            var paramVal = MyOwner.Parameters[nameof(GlobalVar)].Value;
            if (paramVal != null)
            {
                var refName = paramVal.ToString();
                lRefs.Add(new LReference()
                {
                    RefID = GlobalModule.GlobalID,
                    RefName = refName,
                    ByRefName = nameof(GlobalVar),
                });
            }

            return lRefs;
        }
    }
}