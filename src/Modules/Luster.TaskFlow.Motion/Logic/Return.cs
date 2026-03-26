#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       Return
* 机器名称:       L05123-02
* 命名空间:       Luster.TaskFlow.Motion.Logic
* 文 件 名:       Return.cs
* 创建时间:       2023/1/29 17:55:06
* 作    者:       刘克志
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      6764974d-df98-47d4-a8ce-27663cddc09b
* 登录用户:       刘克志
* 所 属 域:       LUSTERINC
* 创建年份:       2023
* 修改时间:		  2023/1/29 17:55:06
* 修 改 人:		  刘克志
************************************************************************************/
#endregion

using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Models;
using Luster.TaskFlow.Motion.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.TaskFlow.Motion.Logic
{
    public class Return : LogicFunction, IReturn
    {
        /// <summary>
        /// 条件表达式
        /// </summary>
        [NotEmpty]
        [Parameter("条件表达式", 0, CN = "中断条件")]
        public LExpression Express { get; set; }

        [Parameter("是否终止当前循环", 1, CN = "是否终止", ParamType = Common.Enums.ParamType.OUT)]
        public bool IsEnd { get; set; }

        /// <summary>
        /// 条件中断
        /// </summary>
        public Return()
        {
            this.Icon = "\xe75f";
            this.Tips = "终止条件";
        }

        /// <summary>
        /// 产品执行
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public override bool DoExcute(out string errMsg)
        {
            // 初始化变量
            IsEnd = false;

            // 通过条件判断循环
            IsEnd = Express.GetResult(MyOwner);

            if (IsEnd)
            {
                MyOwner.OnLog(Luster.Common.DataStruct.Enums.LogType.Debug, $"终止条件被触发:{Express}");
            }

            return base.DoExcute(out errMsg);
        }

        /// <summary>
        /// 是否退出循环
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool IsReturn()
        {
            return IsEnd;
        }
    }
}