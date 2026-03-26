#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       PropSortAttribute
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.DataStruct.Attributes
* 文 件 名:       PropSortAttribute.cs
* 创建时间:       2022/9/6 14:54:36
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      2ff88f96-cb41-456d-b737-2cbcc776411d
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/6 14:54:36
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.DataStruct.Attributes
{
    /// <summary>
    /// 属性排序
    /// </summary>
    public class PropSortAttribute : Attribute
    {
        /// <summary>
        /// 属性顺序
        /// </summary>
        public int Sort { get; set; } = 0;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="sort">顺序编号</param>
        public PropSortAttribute(int sort)
        {
            Sort = sort;
        }
    }

    /// <summary>
    /// 基于属性进行排序
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PropSortCompare<T> : IComparer<T>
    {
        public int Compare(T x, T y)
        {
            if (x is not MemberInfo xMem || y is not MemberInfo yMem) return 0;

            var xAttr = xMem.GetCustomAttribute<PropSortAttribute>();
            var yAttr = yMem.GetCustomAttribute<PropSortAttribute>();
            if (xAttr != null && yAttr != null)
            {
                if (xAttr.Sort > yAttr.Sort)
                {
                    return 1;
                }
                else if (xAttr.Sort < yAttr.Sort)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }
    }
}