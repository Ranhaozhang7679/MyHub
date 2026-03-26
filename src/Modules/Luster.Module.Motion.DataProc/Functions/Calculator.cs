#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       Calculator
* 机器名称:       L05123-NB
* 命名空间:       Luster.Module.Motion.Logic.Functions
* 文 件 名:       Calculator.cs
* 创建时间:       2022/9/30 19:45:30
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      b6327b94-d48d-405d-93c0-891852d50913
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/30 19:45:30
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.DataProc.Functions
{
    public class Calculator : MotionFunction, IRefFunction
    {
        [NotEmpty]
        [Parameter("计算器表达式", 1, CN = "表达式")]
        public LExpression CalcExp { get; set; }

        [Parameter("计算值", 2, CN = "计算值", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public double Value { get; set; }

        public string PropName => nameof(CalcExp);

        public Calculator()
        {
            this.Icon = "\xe622";
            this.Tips = "数字计算器";
        }

        public override bool DoExcute(out string errMsg)
        {
            errMsg = "";

            Value =Math.Round(CalcExp.GetValue(MyOwner),3);
            MyOwner.OnLog(Common.DataStruct.Enums.LogType.Info,$"{MyOwner.Alias}:{Value}" );
            return string.IsNullOrEmpty(errMsg);
        }

        public virtual List<LReference> GetReferences()
        {
            return GetReferences(nameof(CalcExp));
        }
    }
}