#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       Repeat
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Motion.Logic
* 文 件 名:       Repeat.cs
* 创建时间:       2022/9/29 20:11:01
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      c50ad4d4-48b6-40d8-b277-975c9f60c34f
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/29 20:11:01
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.TaskFlow.Motion.Logic
{
    public interface ILoop
    {
        int LoopNum { get; set; }

        int OutLoop { get; set; }
    }
    /// <summary>
    /// 重复逻辑
    /// </summary>
    public class Loop : Group, ILoop
    {
        /// <summary>
        /// 隐藏父类表达式
        /// </summary>
        public new LExpression Express { get; set; }

        [Limit(0, 1000)]
        [Parameter("循环次数", 0, CN = "循环次数", DefaultV = 1, CanRef = ParamRef.Ref)]
        public int LoopNum { get; set; }

        [Parameter("用于中断循环的条件", 1, CN = "中断条件")]
        public LExpression BreakExp { get; set; }

        [Parameter("当前循环次数", 2, CN = "运行次数", ParamType = Common.Enums.ParamType.OUT, DefaultV = 0)]
        public int OutLoop { get; set; }

        public Loop()
        {
            this.Icon = "\xe6f0";
            this.Tips = "循环";
        }

        public override bool DoExcute(out string errMsg)
        {
            errMsg = "";

            //循环次数清0
            OutLoop = 0;

            for (int i = 0; i < LoopNum; i++)
            {
                // 最终的循环次数，输出到其他模块使用
                OutLoop = i + 1;

                // 零时更新循环次数，用于表达式引用
                //MyOwner.SetInParameter(nameof(LoopNum), OutLoop);

                bool isSuccess = base.DoExcute(out errMsg);

                // 程序运行失败,退出循环
                if (!isSuccess)
                {
                    break;
                }

                if (BreakExp != null && BreakExp.ToString() != string.Empty)
                {
                    if (BreakExp.GetResult(MyOwner))
                    {
                        MyOwner.OnLog(LogType.Debug, $"Loop 执行到第 {i} 次满足条件");
                        break;
                    }
                }
            }
            while (LoopNum < 1)
            {
                Thread.Sleep(30);
                // 最终的循环次数，输出到其他模块使用
                OutLoop++;

                // 零时更新循环次数，用于表达式引用
                //MyOwner.SetInParameter(nameof(LoopNum), OutLoop);

                bool isSuccess = base.DoExcute(out errMsg);

                // 程序运行失败,退出循环
                if (!isSuccess)
                {
                    break;
                }

                if (BreakExp != null && BreakExp.ToString() != string.Empty)
                {
                    if (BreakExp.GetResult(MyOwner))
                    {
                        MyOwner.OnLog(LogType.Debug, $"Loop 执行到第 {OutLoop} 次满足条件");
                        break;
                    }
                }
            }//20250514 FPL要求添加，当次数小于1时，无限循环
            // 零时更新循环次数，用于表达式引用
            //MyOwner.SetInParameter(nameof(LoopNum), LoopNum);

            return string.IsNullOrEmpty(errMsg);
        }

        /// <summary>
        /// 获取引用模块
        /// </summary>
        /// <returns></returns>
        public override Dictionary<Guid, string> GetRefModule()
        {
            Dictionary<Guid, string> rs = new Dictionary<Guid, string>();

            var value = MyOwner.Parameters[nameof(BreakExp)].Value;
            if (value == null || value is not LExpression lExp) return rs;

            foreach (var vItem in lExp.Variables)
            {
                if (!rs.ContainsKey(vItem.ID))
                {
                    rs.Add(vItem.ID, nameof(BreakExp));
                }
            }

            return rs;
        }
    }
}