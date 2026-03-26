#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 接口名称:       ISwitch
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Motion.Logic
* 文 件 名:       ISwitch.cs
* 创建时间:       2022/5/20 9:06:16
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      6bb90df1-27eb-4491-a5ad-40c6a9f6ca1a
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/20 9:06:16
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.Tools;
using Luster.Motion.DataStruct.Interfaces;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Interfaces;
using Luster.TaskFlow.Common.Logics;
using Luster.TaskFlow.Common.Models;
using Luster.TaskFlow.Motion.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.TaskFlow.Motion.Logic
{
    /// <summary>
    /// 多条件
    /// </summary>
    public interface ISwitch
    {
        /// <summary>
        /// 表达式对象
        /// </summary>
        LCondition Condition { get; set; }
    }

    /// <summary>
    /// 分支逻辑
    /// </summary>
    public class Switch : LogicFunction, ISwitch, IRefFunction
    {
        /// <summary>
        /// 条件表达式
        /// </summary>
        [Parameter("条件表达式", 0, CN = "条件")]
        public LCondition Condition { get; set; }

        [Parameter("是否持续等待条件满足", 1, CN = "满足等待", DefaultV = true)]
        public bool IsWait { get; set; }

        public string PropName => nameof(Condition);

        /// <summary>
        /// 构造函数
        /// </summary>
        public Switch()
        {
            this.Icon = "\xe657";
            this.Tips = "支持多个分支模块";
            this.DynParam = true;
            Condition = new LCondition();
        }

        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public override bool DoExcute(out string errMsg)
        {
            errMsg = "";
            if (MyOwner.Children.Count == 0)
            {
                errMsg = "分支节点分组";
                return false;
            }

            string group = "";

            // 减少变量模块判断的次数
            WaitFunc(() =>
            {
                return Condition.GetCondition(MyOwner, out group);
            }, $"Switch等待{MyOwner.Alias} : 条件满足");

            bool isSuccess = true;
            var runModule = MyOwner.Children.FirstOrDefault(u => u.Alias == group);
            if (runModule != null)
            {
                motionRunEngine.Run(runModule, ref isSuccess);

                // 3.运行失败，获取错误消息
                if (!isSuccess)
                {
                    errMsg = motionRunEngine.ErrorMessage;
                }
            }
            else
            {
                // 只有非正常停止才报错提醒
                if (!MyOwner.IsBreak)
                {
                    errMsg = $"模块:{MyOwner.Alias} 未找到子节点:{group}";
                    return false;
                }
            }

            return isSuccess;
        }


        public List<LReference> GetReferences()
        {
            List<LReference> refs = new List<LReference>();
            var value = MyOwner.Parameters[nameof(Condition)].Value;
            if (value == null || value is not LCondition lCond) return refs;

            foreach (var item in lCond)
            {
                foreach (var vItem in item.Variables)
                {
                    refs.Add(new LReference() { RefID = vItem.ID, RefName = vItem.ParamKey, ByRefName = nameof(Condition) });
                }
            }

            return refs;
        }
    }
}