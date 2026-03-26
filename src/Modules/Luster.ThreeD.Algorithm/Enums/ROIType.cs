#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ROIType
* 机器名称:       L05123-NB
* 命名空间:       Luster.ThreeD.Algorithm.Enums
* 文 件 名:       ROIType.cs
* 创建时间:       2022/3/15 10:13:19
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      28928714-d7a7-48eb-9c6c-3487359b323e
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/3/15 10:13:19
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.ThreeD.Algorithm.Enums
{
    /// <summary>
    /// ROI的类型
    /// </summary>
    public enum ROIType
    {
        /// <summary>
        /// 立方体
        /// </summary>
        [Description("长方体")]
        VCuboid,

        /// <summary>
        /// 球
        /// </summary>
        [Description("球体")]
        VSphere,

        /// <summary>
        /// 圆柱
        /// </summary>
        [Description("圆柱")]
        VCylinder,
    }
}