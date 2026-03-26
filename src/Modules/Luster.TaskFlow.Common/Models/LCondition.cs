#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LCondition
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Common.Models
* 文 件 名:       LCondition.cs
* 创建时间:       2022/10/9 9:46:58
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      3a1db506-65ef-45ca-b3c2-a3e70f4b36db
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/10/9 9:46:58
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Interfaces;
using Luster.Common.Tools;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.TaskFlow.Common.Models
{
    public class LCondition : LArray<LExpression>, IXMLParser, IEmptyObj, IName
    {
        /// <summary>
        /// 条件名称
        /// </summary>
        public string Name
        {
            get
            {
                if (IsEmpty())
                {
                    return "Condition";
                }

                return string.Join(",", this.Select(u => u.Name));
            }
            set { }
        }

        public bool IsEmpty()
        {
            return Count == 0;
        }

        /// <summary>
        /// 获取满足的条件分支
        /// </summary>
        /// <param name="module"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        /// <exception cref="FriendlyException"></exception>
        public bool GetCondition(IModule module, out string condition)
        {
            condition = string.Empty;
            foreach (var item in this)
            {
                bool isTrue = item.GetResult(module);

                if (isTrue)
                {
                    condition = item.Name;
                    break;
                }
            }

            return !string.IsNullOrEmpty(condition);
        }
    }
}