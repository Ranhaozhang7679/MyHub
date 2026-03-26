using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.Enums;
using Luster.Common.Tools;
using Luster.Motion.DataStruct.Enums;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Enums;
using Luster.TaskFlow.Motion;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.DataProc.Functions
{
    public enum JudgeType
    {
        [Description("长度")]
        Length,

        [Description("包含")]
        Contain
    }

    /// <summary>
    /// 字符串解析
    /// </summary>
    public class JudgeString : MotionFunction
    {
        /// <summary>
        /// 
        /// </summary>
        [Parameter("原始字符串", 0, CN = "原始字符", CanRef = ParamRef.Ref, DefaultV = "")]
        public string Source { get; set; }

        [Parameter("通过哪种类型进行字符拆分", 1, CN = "判断方式", DefaultV = JudgeType.Length)]
        public JudgeType JudgeType { get; set; }

        [DependOn("JudgeType", JudgeType.Length)]
        [Parameter("字符长度", 2, CN = "长度规则", DefaultV = OpRule.Equal)]
        public OpRule OpRule { get; set; }

        [DependOn("JudgeType", JudgeType.Length)]
        [Parameter("字符长度", 3, CN = "字符长度")]
        public int Length { get; set; }

        [DependOn("JudgeType", JudgeType.Contain)]
        [Parameter("原始字符包含的字符", 4, CN = "包含字符")]
        public string ContainStr { get; set; }

        [Parameter("通过字符拆分", 7, CN = "判断结果", ParamType = ParamType.OUT)]
        public bool OutResult { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public JudgeString()
        {
            this.Icon = "\xe6af";
            this.Tips = "字符串判断";
        }

        /// <summary>
        /// 运动
        /// </summary>
        /// <param name="errMsg">errMsg</param>
        /// <returns></returns>
        public override bool DoExcute(out string errMsg)
        {
            if (string.IsNullOrEmpty(Source))
            {
                Source = "";
            }

            switch (JudgeType)
            {
                case JudgeType.Contain:
                    OutResult = Source.Contains(ContainStr);
                    break;
                case JudgeType.Length:
                    switch (OpRule)
                    {
                        case OpRule.Equal:
                            OutResult = Source.Length == Length;
                            break;
                        case OpRule.GatherEqual:
                            OutResult = Source.Length >= Length;
                            break;
                        case OpRule.LessEqual:
                            OutResult = Source.Length <= Length;
                            break;
                    }
                    break;
            }


            return base.DoExcute(out errMsg);
        }
    }
}
