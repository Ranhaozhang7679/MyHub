#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       DependOnAttribute
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.DataStruct.Attributes
* 文 件 名:       DependOnAttribute.cs
* 创建时间:       2022/4/18 17:51:05
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      b32b7f86-fb6c-4c30-830b-70372915c580
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/18 17:51:05
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.DataStruct.Attributes
{
    /// <summary>
    /// 参数依赖属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class DependOnAttribute : Attribute
    {

        public string DependProp { get; set; }

        public object[] DependValue { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="propName">依赖属性的名称</param>
        /// <param name="val">依赖的值</param>
        public DependOnAttribute(string propName, params object[] vals)
        {
            DependProp = propName;
            DependValue = vals;
        }
    }
}