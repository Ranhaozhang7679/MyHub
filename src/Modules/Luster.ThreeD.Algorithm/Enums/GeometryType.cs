#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       GeometryType
* 机器名称:       L05123-NB
* 命名空间:       Luster.ThreeD.Algorithm.Enums
* 文 件 名:       GeometryType.cs
* 创建时间:       2022/4/2 9:20:45
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      949f4039-78fa-4b0a-9371-d3ead02bf798
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/2 9:20:45
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
    public enum GeometryType
    {
        [Description("点")]
        Point,

        [Description("线")]
        Line,

        [Description("面")]
        Plane,

        [Description("圆")]
        Circle,
        [Description("拟合面")]
        FitPlane,
    }
}