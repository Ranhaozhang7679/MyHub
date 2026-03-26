#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       StringMerge
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Common.Logics
* 文 件 名:       StringMerge.cs
* 创建时间:       2022/9/8 15:00:19
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      481c4ebc-c523-4459-a97a-146157dcd48b
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/8 15:00:19
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Functions;
using Luster.TaskFlow.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.TaskFlow.Common.Logics
{
    public class StringMerge : Function
    {
        [NotEmpty]
        [Parameter("字符串拼接,使用+或空格分隔", 0, CN = "字符表达式")]
        public LStringEx StringEx { get; set; }

        [Parameter("是否去除部分字符", 1, CN = "是否去除部分字符", DefaultV = false)]
        public bool IsIgnoreSeperator{ get; set; }

        [DependOn("IsIgnoreSeperator", true)]
        [Parameter("去除字符长度", 1, CN = "去除字符长度",DefaultV =1)]
        public int length { get; set; }

        [DependOn("IsIgnoreSeperator", true)]
        [Parameter("去除字符起始位", 1, CN = "去除字符起始位",DefaultV =0)]
        public int index { get; set; }



        [Parameter("真实字符串", 1, CN = "字符串", ParamType = TaskFlow.Common.Enums.ParamType.OUT)]
        public string OutString { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public StringMerge()
        {
            this.Icon = "\xe712";
            this.Tips = "对多个数据进行任意组合拼接成特定的字符串";
        }

        public override bool DoExcute(out string errMsg)
        {
            var str = StringEx.GetString(Owner);
            if (IsIgnoreSeperator && (index +length)<str.Length)
            {
                OutString = string.Join("", str.Replace("\r", "").Replace("\n", "").Substring(0,index))+ string.Join("", str.Replace("\r", "").Replace("\n", "").Substring(index+length));
            }
            else
            {
                OutString = str;
            }

                return base.DoExcute(out errMsg);
        }
    }
}