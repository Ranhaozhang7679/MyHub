#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       WaitPlc
* 机器名称:       L05123-NB
* 命名空间:       Luster.Module.Motion.Protocol.Functions
* 文 件 名:       ModbusCheck.cs
* 创建时间:       2022/7/25 13:40:15
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      7f4d76d4-422d-4c5b-bea2-f43ce8fc99a5
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/25 13:40:15
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct;
using Luster.Motion.DataStruct;
using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Extensions;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.VDevice;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Common.Interfaces;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Functions;
using Luster.TaskFlow.Motion.Interfaces;
using Luster.TaskFlow.Motion.Logic;
using Luster.TaskFlow.Motion.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Protocol.Functions
{
    public class WaitPlc : PlcBase, IWait, IRefFunction, IGoToFunction
    {
        [Parameter("阈值规则", 3, CN = "阈值规则", DefaultV = OpRule.Equal)]
        public OpRule OpRule { get; set; }

        [Parameter("超时了是否蜂鸣并重试", 4, CN = "超时警告重试", DefaultV = false)]
        public bool IsNeedRetry { get; set; }

        [DependOn("IsAlarm", false)]
        [Parameter("超时结果为false,成功结果为true", 12, CN = "是否成功", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool Result { get; set; }

        [DependOn("DataType", DataType.Bool)]
        [Parameter("持续等待线圈值", 6, CN = "布尔值", DefaultV = false, ParamType = ParamType.IN, CanRef = ParamRef.Ref)]
        public bool CoilVal { get; set; }

        [DependOn("DataType", DataType.Int, DataType.Short, DataType.Float, DataType.Double)]
        [Parameter("持续等寄存器值", 7, CN = "数值", DefaultV = 0, ParamType = ParamType.IN, CanRef = ParamRef.Ref)]
        public double RegisterVal { get; set; }

        [Limit(-1, 1000000)]
        [Parameter("超时时间,单位s", 7, CN = "等待超时", DefaultV = -1)]
        public int OverTime { get; set; }

        [Parameter("启用PLC监控", 8, CN = "PLC监控", DefaultV = false, Group = "PLC")]
        public bool PlcMonitor { get; set; }

        [Parameter("PLC初始化地址", 9, CN = "初始地址", DefaultV = 520, Group = "PLC")]
        public int PlcInitAddr { get; set; }

        [Parameter("如果PLC初始化", 10, CN = "跳转模块", Group = "PLC", CanRef = ParamRef.None)]
        public LModule GotoModule { get; set; }

        [Parameter("PLC发生了初始化", 11, CN = "是否初始化", ParamType = TaskFlow.Common.Enums.ParamType.OUT, DefaultV = false)]
        public bool IsPlcInit { get; set; }


        public WaitPlc()
        {
            this.Tips = "持续等待地址上的值是否和当前值相等";
            this.Icon = "\xe68c";
        }

        public override bool DoExcute(out string errMsg)
        {
            IsPlcInit = false;
            GetVDevice<VPlc>(PlcDevice, out var plc);
            plc.Open();

            double v = RegisterVal;
            int waitOveTime = OverTime * 1000;

            if (DataType == DataType.Bool)
            {
                v = CoilVal ? 1 : 0;
            }

            var plcAddr = plc.Address.FirstOrDefault(p => p.Address == Address);
            if (plcAddr == null)
            {
                errMsg = $"Plc:{Name} 不存在地址:{plcAddr.Address}";

                return false;
            }

            var monitorAddr = plc.Address.FirstOrDefault(u => u.Address == PlcInitAddr.ToString());
            if (PlcMonitor && monitorAddr == null)
            {
                errMsg = $"PLC 未添加对地址:{PlcInitAddr} 监控";
                return false;
            }
            Result = true;
            bool isInit = false;

            Action retryAction = null;
            if (IsNeedRetry)
            {
                retryAction = () =>
                {
                    MyOwner.LightManager.SetBuzzer(true, 500, 500);
                    OnAlarm(AlarmType.InfoTip, $"{MyOwner.Alias},等待PLC超时,蜂鸣提示，再次等待");
                    WaitFunc(() =>
                    {
                        // 是否监控PLC地址
                        if (PlcMonitor && monitorAddr != null)
                        {
                            if (monitorAddr.Value != 2)
                            {
                                isInit = true;
                            }
                            // 状态->2 此时PLC正在发生初始化，将中断循环的条件激活 
                            if (isInit && monitorAddr.Value == 2)
                            {
                                // 中断循环
                                IsPlcInit = true;
                                MyOwner.IsBreak = true;
                                MyOwner.OnLog(LogType.Error, $"模块:{MyOwner.Alias} PLC发生了初始化");
                                Thread.Sleep(200);
                            }
                        }
                        bool isTrue = Luster.Motion.DataStruct.Network.CompareVal<double>.Compare(OpRule, v, plcAddr.Value);
                        return isTrue;
                    }, $"Wait等待值 {OpRule.GetDescription()} {v}", 50, -1);
                };
            }
            try
            {
                WaitFunc(() =>
            {
                // 是否监控PLC地址
                if (PlcMonitor && monitorAddr != null)
                {
                    //// 状态->2 此时init
                    //if (monitorAddr.Value == 2)
                    //{
                    //    isInit = true;
                    //}

                    //// 状态->3 此时Inited PLC发生初始化，将中断循环的条件激活 
                    //if (isInit && monitorAddr.Value == 3)
                    //{
                    //    // 中断循环
                    //    IsPlcInit = true;
                    //    MyOwner.IsBreak = true;
                    //    MyOwner.OnLog(LogType.Error, $"模块:{MyOwner.Alias} PLC发生了初始化");
                    //    Thread.Sleep(200);
                    //}
                    // 状态->2 此时非初始化的状态
                    if (monitorAddr.Value != 2)
                    {
                        isInit = true;
                    }

                    // 状态->2 此时PLC正在发生初始化，将中断循环的条件激活 
                    if (isInit && monitorAddr.Value == 2)
                    {
                        // 中断循环
                        IsPlcInit = true;
                        MyOwner.IsBreak = true;
                        MyOwner.OnLog(LogType.Error, $"模块:{MyOwner.Alias} PLC发生了初始化");
                        Thread.Sleep(200);
                    }
                }

                bool isTrue = Luster.Motion.DataStruct.Network.CompareVal<double>.Compare(OpRule, v, plcAddr.Value);
                return isTrue;
            }, $"Wait等待值 {OpRule.GetDescription()} {v}", 50, waitOveTime, retryAction);
            }
            catch (DeviceTimeoutException ex)
            {
                OnAlarm(AlarmType.WarningTip, $"等待变量{OpRule.GetDescription()} == {v}超时!");
            }
            // PLC发生初始化，将中断循环的条件关闭
            if (IsPlcInit)
            {
                GoTo();

                // PLC 初始化后，将中断
                MyOwner.IsBreak = false;
            }

            return base.DoExcute(out errMsg);
        }

        /// <summary>
        /// 跳转
        /// </summary>
        private void GoTo()
        {
            var goTo = MyOwner.Parameters[nameof(GotoModule)];
            if (goTo == null) return;

            var lModule = goTo.Value as LModule;
            if (lModule != null)
            {
                var module = MyOwner.TaskModules[lModule.ID] as IMotionModule;
                if (module != null)
                {
                    if (module.Station == null)
                    {
                        module.UpdateStation();
                    }

                    if (module.Station != MyOwner.Station)
                    {
                        // 模块跳转
                        MyOwner.OnLog(LogType.Error, $"跳转模块工站:{module.Station.Name} 和当前工站:{MyOwner.Station} 不匹配!");
                        return;
                    }

                    MyOwner.OnLog(LogType.Info, $"工站跳转开始：{MyOwner.Alias}->{module.Alias}");
                }
            }
        }

        /// <summary>
        /// 跳转引用
        /// </summary>
        /// <returns></returns>
        public virtual List<LReference> GetReferences()
        {
            List<LReference> list = new List<LReference>();
            var p = MyOwner.Parameters[nameof(GotoModule)];
            if (p != null && p.Value != null && p.Value is LModule m)
            {
                list.Add(new LReference()
                {
                    ByRefName = nameof(GotoModule),
                    RefID = m.ID,
                    RefName = m.Name,
                });
            }

            return list;
        }

        /// <summary>
        /// 跳转成功
        /// </summary>
        public bool GetJump(out IMotionModule jumpModule)
        {
            jumpModule = null;

            // 未启用监控，则直接返回
            if (!PlcMonitor)
            {
                return false;
            }

            if (GotoModule == null) return false;

            // 如果PLC 未初始化 不跳转
            if (!IsPlcInit) return false;

            if (MyOwner.TaskModules.Contains(GotoModule.ID))
            {
                jumpModule = MyOwner.TaskModules[GotoModule.ID] as IMotionModule;
            }

            return jumpModule != null;
        }
    }
}
