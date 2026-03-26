#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       DataType
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.DataStruct.Enums
* 文 件 名:       DataType.cs
* 创建时间:       2022/10/18 18:23:53
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      e9a41052-8c42-4309-9c77-b852c9135a77
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/10/18 18:23:53
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.DataStruct.Enums
{
    /// <summary>
    /// 基础数据类型
    /// </summary>
    public enum DataType
    {
        [Description("Boolean")]
        Bool,

        [Description("Short")]
        Short,

        [Description("Int")]
        Int,

        [Description("Long")]
        Long,

        [Description("Float")]
        Float,

        [Description("Double")]
        Double,

        [Description("String")]
        String,
    }
}