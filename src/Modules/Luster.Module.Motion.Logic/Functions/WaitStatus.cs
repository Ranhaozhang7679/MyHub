#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       UpdateHave
* 机器名称:       L05123-NB
* 命名空间:       Luster.Module.Motion.Stations.Functions
* 文 件 名:       UpdateHave.cs
* 创建时间:       2022/8/9 15:30:23
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      dabfd248-f4f2-4677-9556-41ea7bba5480
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/8/9 15:30:23
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Extensions;
using Luster.Motion.DataStruct.Enums;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Interfaces;
using Luster.TaskFlow.Common.Models;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Interfaces;
using Luster.TaskFlow.Motion.Logic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Logic.Functions
{
    /// <summary>
    /// 更新工站有无料
    /// </summary>
    public class WaitStatus : OverTimeFunction, IWait, IRefFunction
    {
        [Parameter("等待类型", 1, CN = "等待类型", DefaultV = WaitType.Single)]
        public WaitType WaitType { get; set; }

        [DependOn("WaitType", WaitType.Single)]
        [Parameter("等待模块", 2, CN = "等待模块")]
        public LStatus InStatus { get; set; }
        /// <summary>
        /// 等待条件条件表达式
        /// </summary>
        [DependOn("WaitType", WaitType.Multi)]
        [Parameter("等待条件条件表达式", 3, CN = "等待条件")]
        public LExpression Express { get; set; }

        [Limit(-1, 1000000)]
        [Parameter("超时时间，单位为s", 4, CN = "超时时间", DefaultV = -1)]
        public override int OverTime { get; set; }


        [Parameter("数据转移方式,传递", 5, CN = "数据转移")]
        public DataTransType TransType { get; set; }

        [Limit(1, 100)]
        [DependOn("TransType", DataTransType.Get)]
        [Parameter("获取数据时上一工站对应的产品数量", 6, CN = "产品数量", DefaultV = 1)]
        public int ProductCount { get; set; }


        [Parameter("超时状态", 22, CN = "是否超时", ParamType = TaskFlow.Common.Enums.ParamType.OUT, DefaultV = false)]
        public bool IsTimeOut { get; set; }

        public override string[] NoteParams => new string[] { "等待", nameof(InStatus) };

        /// <summary>
        /// 引用模块引导
        /// </summary>
        public string PropName
        {
            get
            {
                if (MyOwner != null)
                {
                    var curWaitType = MyOwner.Parameters[nameof(WaitType)].Value;
                    if (curWaitType != null)
                    {
                        var waitType = curWaitType.To<WaitType>();

                        if (waitType == WaitType.Single)
                        {
                            return nameof(InStatus);
                        }
                        else
                        {
                            return nameof(Express);
                        }
                    }
                    else
                    {
                        return nameof(InStatus);
                    }
                }
                else
                {
                    return nameof(InStatus);
                }
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public WaitStatus()
        {
            this.Tips = "等待模块，用于等待某个模块运行完成";
            this.Icon = "\xe6b4";
            InStatus = new LStatus();
        }

        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="errMsg">错误消息</param>
        /// <returns></returns>
        public override bool DoExcute(out string errMsg)
        {
            errMsg = "";

            IsTimeOut = false;
            int minuTime = OverTime * 1000;
            if (WaitType == WaitType.Single)
            {
                var parameter = MyOwner.Parameters[nameof(InStatus)];
                if (parameter.RefOut == null)
                {
                    errMsg = "等待模块为空!";
                    return false;
                }

                var refModuleAlias = parameter.RefOut.Owner.Alias;

                string msg = "";
                WaitFunc(() =>
                {
                    var refVal = parameter.GetValue(out msg);
                    if (refVal != null && refVal is LStatus s)
                    {
                        return s.GetStatusAndDefault();
                    }
                    else { return false; }

                }, $"等待模块:{refModuleAlias} 运行结束", 20, minuTime,
                () =>
                {
                    IsTimeOut = true;
                    return;
                });

                var srcStation = parameter.RefOut.Owner as IMotionModule;
                DataProcess(MyOwner, srcStation, TransType, ProductCount);
                MyOwner.OnLog(LogType.Debug, $"数据获取:{MyOwner.Alias} {TransType} {ProductCount},ID值获取:{srcStation.DataID}");
            }
            else
            {

                WaitFunc(() =>
                {
                    var isResult = Express.GetResult(MyOwner, (m) =>
                    {
                        DataProcess(MyOwner, m as IMotionModule, TransType, ProductCount);
                    });

                    return isResult;
                }, $"等待 {Express}", 50, minuTime, () =>
                {
                    IsTimeOut = true;
                });
            }

            return base.DoExcute(out errMsg);
        }

        /// <summary>
        /// 获取引用模块
        /// </summary>
        /// <returns></returns>
        public Dictionary<Guid, string> GetRefModule()
        {
            Dictionary<Guid, string> rs = new Dictionary<Guid, string>();

            var waitType = GetParameterVal(nameof(WaitType));
            if ((WaitType)waitType == WaitType.Single)
            {
                if (MyOwner.Parameters.ContainsKey(nameof(InStatus)))
                {
                    var pStatus = MyOwner.Parameters[nameof(InStatus)];
                    if (pStatus.RefOut != null)
                    {
                        Guid mId = pStatus.RefOut.Owner.ID;
                        if (!rs.ContainsKey(mId))
                        {
                            rs.Add(mId, nameof(InStatus));
                        }
                    }
                }
            }
            else
            {
                var exp = GetParameterVal(nameof(Express)) as LExpression;
                if (exp != null)
                {
                    foreach (var vItem in exp.Variables)
                    {
                        if (!rs.ContainsKey(vItem.ID))
                        {
                            rs.Add(vItem.ID, nameof(Express));
                        }
                    }
                }
            }


            return rs;
        }

        public virtual List<LReference> GetReferences()
        {
            var waitType = GetParameterVal(nameof(WaitType));
            if ((WaitType)waitType != WaitType.Single)
            {
                return GetReferences(nameof(Express));
            }
            else
            {
                return new List<LReference>();
            }

        }
    }

    /// <summary>
    /// 等待类型
    /// </summary>
    public enum WaitType
    {
        [Description("单模块")]
        Single,

        [Description("多模块")]
        Multi
    }
}