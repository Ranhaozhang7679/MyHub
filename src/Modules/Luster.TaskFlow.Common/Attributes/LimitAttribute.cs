#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LRangeAttribute
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Common.Attributes
* 文 件 名:       LRangeAttribute.cs
* 创建时间:       2022/5/15 16:21:46
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      a91a156f-54e7-42af-9265-47249b827d8e
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/15 16:21:46
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.TaskFlow.Common.Attributes
{
    /// <summary>
    /// 范围限制
    /// </summary>
    public class LimitAttribute : RangeAttribute
    {
        public LimitAttribute(double minimum, double maximum) : base(minimum, maximum)
        {
        }

        public override bool IsValid(object value)
        {
            double minVal = Convert.ToDouble(Minimum);
            double maxVal = Convert.ToDouble(Maximum);
            if (value is LRange range)
            {
                return range.Min >= minVal && range.Max <= maxVal;
            }
            else
            {
                if (value != null)
                {
                    var props = value.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
                    bool isValid = true;
                    foreach (var prop in props)
                    {
                        if (prop.PropertyType.IsNumeric())
                        {
                            var curVal = Convert.ToDouble(prop.GetValue(value, null));
                            isValid = curVal >= minVal && curVal <= maxVal;
                            if (!isValid)
                            {
                                break;
                            }
                        }
                    }

                    return isValid;
                }

            }

            return true;
        }
    }
}