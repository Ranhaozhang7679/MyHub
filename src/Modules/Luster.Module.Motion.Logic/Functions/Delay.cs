#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       DelayTime
* 机器名称:       L05123-NB
* 命名空间:       Luster.Module.Motion.Logic.Functions
* 文 件 名:       DelayTime.cs
* 创建时间:       2022/7/19 16:59:46
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      a7bde420-1918-477b-b626-f95b18e36b9e
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/19 16:59:46
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.DataModels;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Interfaces;
using Luster.TaskFlow.Common.Models;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.Logic.Functions
{
    public class Delay : MotionFunction, IGetExtResult, IRefFunction
    {
        [Range(0, 100000)]
        [Parameter("延时时间,单位毫秒", 1, CN = "等待时间", DefaultV = 100, CanRef = ParamRef.Ref, IsSensitive = true)]
        public int Time { get; set; }

        /// <summary>
        /// 条件报警
        /// </summary>
        [Parameter("是否延时，如果为true,就延时警，如果为false，不延时", 2, CN = "延时条件", IsSensitive = true)]
        public LExpression Express { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public Delay()
        {
            this.Tips = "任务延时等待";
            this.Icon = "\xe69c";
        }


        public override bool DoExcute(out string errMsg)
        {
            bool isDelay = true;
            if (Express != null)
            {
                isDelay = Express.GetResult(MyOwner);
            }

            if (isDelay)
            {
                System.Threading.Thread.Sleep(Time);
            }

            return base.DoExcute(out errMsg);
        }


        public Dictionary<string, object> GetExtResult()
        {
            return new Dictionary<string, object>
            {
                { "Delayed", Time },
            };
        }

        public virtual List<LReference> GetReferences()
        {
            return GetReferences(nameof(Express));
        }
    }
}