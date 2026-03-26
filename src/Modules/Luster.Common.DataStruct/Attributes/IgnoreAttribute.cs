#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       IgnoreAttribute
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.DataStruct.Attributes
* 文 件 名:       IgnoreAttribute.cs
* 创建时间:       2022/4/7 20:09:54
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      d68ec47e-e5a2-4381-a79e-c1c8be863505
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/7 20:09:54
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
    /// 对某个属性进行忽略
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Enum | AttributeTargets.Class | AttributeTargets.Field)]
    public class IgnoreAttribute : Attribute
    {
    }
}