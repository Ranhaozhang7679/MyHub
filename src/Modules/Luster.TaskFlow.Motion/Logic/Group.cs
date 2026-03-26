#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       GroupFunction
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Motion.Functions
* 文 件 名:       GroupFunction.cs
* 创建时间:       2022/5/24 9:52:41
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      9beb6bda-d09f-483b-b30c-bd5c0611cd88
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/24 9:52:41
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.DataModels;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Interfaces;
using Luster.TaskFlow.Common.Models;
using Luster.TaskFlow.Motion.Interfaces;
using Luster.TaskFlow.Motion.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.TaskFlow.Motion.Logic
{
    /// <summary>
    /// 分组接口
    /// </summary>
    public interface IGroup
    {
    }

    /// <summary>
    /// 分组
    /// </summary>
    public class Group : LogicFunction, IRefFunction
    {
        /// <summary>
        /// 条件表达式
        /// </summary>
        [Parameter("表达式判断，如果为空默认执行，True->执行，False->不执行", 0, CN = "条件")]
        public virtual LExpression Express { get; set; }

        public string PropName => nameof(Express);

        /// <summary>
        /// 构造函数
        /// </summary>
        public Group() : base()
        {
            this.Tips = "将多个模块打包成一个模块";
            this.Icon = "\xe65e";
        }

        /// <summary>
        /// 分组模块处理
        /// </summary>
        /// <param name="errMsg">错误消息</param>
        /// <returns>是否运行成功</returns>
        public override bool DoExcute(out string errMsg)
        {
            bool isSuccess = true;

            errMsg = String.Empty;

            bool isRun = true;
            if (Express != null)
            {
                isRun = Express.GetResult(MyOwner);
            }

            if (isRun && MyOwner.Children.Count > 0)
            {
                // 1.获取Start
                var startModule = MyOwner.Children[0];

                // 2.程序运行
                motionRunEngine.Run(startModule, ref isSuccess);

                // 3.运行失败，获取错误消息
                if (!isSuccess)
                {
                    errMsg = motionRunEngine.ErrorMessage;
                }
            }

            return isSuccess;
        }

        public virtual Dictionary<Guid, string> GetRefModule()
        {
            Dictionary<Guid, string> rs = new Dictionary<Guid, string>();

            var value = MyOwner.Parameters[nameof(Express)].Value;
            if (value == null || value is not LExpression lExp) return rs;

            foreach (var vItem in lExp.Variables)
            {
                if (!rs.ContainsKey(vItem.ID))
                {
                    rs.Add(vItem.ID, nameof(Express));
                }
            }

            return rs;
        }

        public virtual List<LReference> GetReferences()
        {
            return GetReferences(nameof(Express));
        }
    }
}