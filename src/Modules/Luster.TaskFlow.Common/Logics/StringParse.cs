#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       StringParse
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Common.Logics
* 文 件 名:       StringParse.cs
* 创建时间:       2022/10/19 15:17:15
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      dc280dc5-f300-431e-b57b-c74dedb2c38c
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/10/19 15:17:15
* 修 改 人:		  L05123
************************************************************************************/
#endregion

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
    /// <summary>
    /// 字符解析
    /// </summary>
    public class StringParse : Function
    {
        [NotEmpty]
        [Parameter("要进行解析的字符", 0, CN = "字符串", CanRef = ParamRef.Ref)]
        public string Text { get; set; }

        [NotEmpty]
        [Parameter("字符串拼接", 1, CN = "字符模板")]
        public LStringMatch StringMatch { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public StringParse()
        {
            this.Icon = "\xe713";
            this.Tips = "对字符串进行解析";
            this.DynParam = true;
        }

        public override bool DoExcute(out string errMsg)
        {
            // 进行匹配验证
            StringMatch.Match(Text);

            // 更新对应的Value值
            foreach (var item in StringMatch.Variables)
            {
                SetParameterVal(item.Name, item.Value);
            }

            return base.DoExcute(out errMsg);
        }

        public override void OnNotifyPropertyUIChanged(ParameterAttribute parameter, object newV)
        {
            base.OnNotifyPropertyUIChanged(parameter, newV);
            if (parameter.Name == nameof(StringMatch))
            {
                var strMatch = newV as LStringMatch;
                int sort = Owner.Parameters.Count;
                List<string> removeKeys = new List<string>();
                foreach (var item in Owner.Parameters)
                {
                    if (item.Value.Property == null)
                    {
                        removeKeys.Add(item.Key);
                    }
                }

                foreach (var item in removeKeys)
                {
                    Owner.Parameters.Remove(item);
                }

                foreach (var item in strMatch.Variables)
                {
                    Owner.Parameters.Add(item.Name, item.CreateParameter(Owner, sort));
                }

                Owner.OnUpdate(Enums.ModuleUpdate.ParameterNum);
            }
        }
    }
}