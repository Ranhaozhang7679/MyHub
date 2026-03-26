#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       NotEmptyAttribute
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Common.Attributes
* 文 件 名:       NotEmptyAttribute.cs
* 创建时间:       2022/1/21 11:27:26
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      466c39bf-3e57-4b3b-8cc9-a1fa8974aa16
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/1/21 11:27:26
* 修 改 人:		  L05123
************************************************************************************/

#endregion

using Luster.Common.DataStruct.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.TaskFlow.Common.Attributes
{
    /// <summary>
    /// 标记该特性的属性不能为空
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class NotEmptyAttribute : RequiredAttribute
    {
        public override bool IsValid(object value)
        {
            // 值为空
            if (value == null)
            {
                return false;
            }

            // 如果是非空对象
            if (value is IEmptyObj empty && empty.IsEmpty())
            {
                return false;
            }

            string text = value as string;
            if (text != null && !AllowEmptyStrings)
            {
                return text.Trim().Length != 0;
            }

            return true;
        }
    }
}