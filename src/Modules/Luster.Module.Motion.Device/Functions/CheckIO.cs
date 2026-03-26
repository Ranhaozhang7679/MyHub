#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       CheckIO
* 机器名称:       F04846
* 命名空间:       Luster.Module.Motion.Device.Functions
* 文 件 名:       CheckIO.cs
* 创建时间:       2022/8/4 10:21:08
* 作    者:       房晶鹏
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       jingpengfang@lusterinc.com 
* 唯一标识：      e40ab161-6146-4a5c-9213-5a2971a12e01
* 登录用户:       fangjingpeng
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/8/4 10:21:08
* 修 改 人:		  F04846
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Device.Functions
{

    public enum CheckMode
    {
        [Description("单次检查")]
        One,

        [Description("连续检查")]
        Wait
    }

    public enum JudgeMode
    {
        [Description("所有满足")]
        All,

        [Description("任意满足")]
        Any,

        [Description("每个不同")]
        Every
    }

    public enum OutResultType
    {
        [Description("输出结果")]
        Result,

        [Description("报警")]
        Alarm
    }

    public class CheckIO : MotionFunction,IPauseFunction
    {
        [Range(1, 100)]
        [Parameter("要对多少个IO进行检查", 1, CN = "IO数量", DefaultV = 1)]
        public int IONum { get; set; }

        [Parameter("IO检查模式", 1, CN = "检查模式", DefaultV = CheckMode.One)]
        public CheckMode CheckMode { get; set; }

        [Parameter("IO检查模式", 2, CN = "判断模式", DefaultV = JudgeMode.All)]
        public JudgeMode JudgeMode { get; set; }

        [Parameter("输出模式", 3, CN = "输出模式", DefaultV = OutResultType.Result)]
        public OutResultType OutResultType { get; set; }

        [NotEmpty]
        [Parameter("IO类型，通过关键字搜索", 4, CN = "IO名称", EditorType = typeof(VIO))]
        public VDevice Device { get; set; }

        [DependOn("JudgeMode", JudgeMode.All, JudgeMode.Any)]
        [Parameter("检查信号值", 20, CN = "IO判断值")]
        public bool DigitalVal { get; set; }

        [DependOn("JudgeMode", JudgeMode.Every)]
        [Parameter("必须和IO设备数量匹配", 21, CN = "IO数组")]
        public LArray<bool> Values { get; set; }

        [DependOn("CheckMode", CheckMode.Wait)]
        [Parameter("规定时间内检查信号是否满足", 21, CN = "超时时间")]
        public int TimeOut { get; set; } = -1;


        [Parameter("结果是否满足", 40, CN = "是否满足", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public bool Result { get; set; }

        public CheckIO()
        {
            this.Tips = "检查IO信号是否满足条件，满足为真，不满足为假";
            this.Icon = "\xe67f";

        }

        public override bool DoExcute(out string errMsg)
        {
            // 初始值
            Result = false;
            errMsg = "";
            var ioDevices = GetParametersByType<VDevice>();
            List<VIO> ios = new List<VIO>();
            foreach (var item in ioDevices)
            {
                GetVDevice<VIO>(item, out var io);
                ios.Add(io);
            }

            if (JudgeMode == JudgeMode.All)
            {
                if (CheckMode == CheckMode.One)
                {
                    bool isResult = true;
                    ios.ForEach(u =>
                    {
                        isResult = isResult && u.GetDigitalIn() == DigitalVal;
                    });

                    Result = isResult;
                }
                else
                {
                    WaitFunc(() =>
                    {
                        var isResult = true;
                        ios.ForEach(u =>
                        {
                            isResult = isResult && u.GetDigitalIn() == DigitalVal;
                        });
                        return isResult;
                    }, $"等待:{MyOwner.Alias} 检查值满足!",timeout:TimeOut);

                    Result = true;
                }
            }
            else if (JudgeMode == JudgeMode.Any)
            {
                if (CheckMode == CheckMode.One)
                {
                    bool isResult = false;
                    ios.ForEach(u =>
                    {
                        isResult = isResult || u.GetDigitalIn() == DigitalVal;
                    });

                    Result = isResult;
                }
                else
                {
                    WaitFunc(() =>
                    {
                        var isResult = false;
                        ios.ForEach(u =>
                        {
                            isResult = isResult || u.GetDigitalIn() == DigitalVal;
                        });

                        return isResult;
                    }, $"等待:{MyOwner.Alias} 检查值满足!", timeout: TimeOut);

                    Result = true;
                }
            }
            else if (JudgeMode == JudgeMode.Every)
            {
                if (ios.Count != Values.Count)
                {
                    errMsg = "IO设备数量和IO数组的数量必须相同";
                    return false;
                }

                if (CheckMode == CheckMode.One)
                {
                    bool isResult = true;
                    int count = Values.Count;
                    for (int i = 0; i < count; i++)
                    {
                        isResult = isResult && ios[i].GetDigitalIn() == Values[i];
                    }

                    Result = isResult;
                }
                else
                {
                    WaitFunc(() =>
                    {
                        bool isResult = true;
                        int count = Values.Count;
                        for (int i = 0; i < count; i++)
                        {
                            isResult = isResult && ios[i].GetDigitalIn() == Values[i];
                        }

                        return isResult;

                    }, $"等待:{MyOwner.Alias} 检查值满足!", timeout: TimeOut);

                    Result = true;
                }
            }

            // 如果结果是false，并且报警
            if (!Result && OutResultType == OutResultType.Alarm)
            {
                OnAlarm(AlarmType.PopInfoTip, $"结果不满足条件", ios[0].Module);
            }

            return base.DoExcute(out errMsg);
        }


        /// <summary>
        /// 暂停
        /// </summary>
        public override void Pause()
        {
        }
        public override void OnNotifyPropertyUIChanged(ParameterAttribute parameter, object newV)
        {
            base.OnNotifyPropertyUIChanged(parameter, newV);
            if (parameter.Name == "IONum" && int.TryParse(newV?.ToString(), out var num) && num >= 1)
            {
                BuildExternParameters(1, num, "Device", "IO名称", typeof(VDevice), (p) =>
                {
                    p.EditorType = typeof(VIO);
                });
            }
        }
    }
}