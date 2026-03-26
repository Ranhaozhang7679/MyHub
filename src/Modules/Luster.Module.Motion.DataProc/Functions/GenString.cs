#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       GenString
* 机器名称:       L05123-NB
* 命名空间:       Luster.Module.Motion.Logic.Functions
* 文 件 名:       GenString.cs
* 创建时间:       2022/9/15 22:06:04
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      6650f133-963d-4cc7-9eae-5b5998db8800
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/15 22:06:04
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Module.Motion.DataProc.Functions
{
    public enum GenStringType
    {
        [Description("日期")]
        DateTime,

        [Description("序列")]
        Sequence
    }


    public class GenString : MotionFunction, IHomeFunction
    {
        /// <summary>
        /// 条件表达式
        /// </summary>
        [NotEmpty]
        [Parameter("字符串生成的方式", 0, CN = "生成方式", DefaultV = GenStringType.DateTime)]
        public GenStringType StringType { get; set; }

        [Parameter("前置字符串", 1, CN = "前置字符串")]
        public string Prefix { get; set; }

        [DependOn("StringType", GenStringType.DateTime)]
        [Parameter("时间格式化", 2, CN = "格式化", DefaultV = "yyyy-MM-dd-HH-mm-ss")]
        public string DateFormat { get; set; }

        [DependOn("StringType", GenStringType.Sequence)]
        [Parameter("起始编号", 3, CN = "起始编号", DefaultV = "0")]
        public int Index { get; set; }

        [Parameter("后置字符串", 4, CN = "后置字符串")]
        public string Afterfix { get; set; }

        [DependOn("StringType", GenStringType.DateTime)]
        [Range(-1000, 1000)]
        [Parameter("时间偏移，单位为s", 5, CN = "时间偏移", DefaultV = 0.0)]
        public double TimeOffset { get; set; }

        /// <summary>
        /// 是否满足
        /// </summary>
        [Parameter("自动生成的字符串", 5, CN = "字符串", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string OutString { get; set; }

        /// <summary>
        /// 序列号
        /// </summary>
        int sequence = -1;

        public GenString()
        {
            this.Tips = "字符串生成器";
            this.Icon = "\xe6ac";
        }



        /// <summary>
        /// 函数执行
        /// </summary>
        /// <param name="errMsg">错误消息</param>
        /// <returns>运行结果</returns>
        public override bool DoExcute(out string errMsg)
        {
            errMsg = "";
            string str = "";
            if (!string.IsNullOrEmpty(Prefix))
            {
                str = Prefix;
            }

            switch (StringType)
            {
                case GenStringType.DateTime:
                    //增加0~1000ms内的随机数
                    Random random = new Random();
                    str = $"{str}{DateTime.Now.AddSeconds(TimeOffset).AddMilliseconds(random.Next(0,1000)).ToString(DateFormat)}";
                    break;
                case GenStringType.Sequence:
                    if (sequence == -1)
                    {
                        sequence = Index;
                    }

                    str = $"{str}{sequence}";
                    sequence++;

                    break;
            }

            if (!string.IsNullOrEmpty(Afterfix))
            {
                str += Afterfix;
            }

            OutString = str;
            return base.DoExcute(out errMsg);
        }

        /// <summary>
        /// 触发回零
        /// </summary>
        public override void Home()
        {
            sequence = Index;
        }
    }
}