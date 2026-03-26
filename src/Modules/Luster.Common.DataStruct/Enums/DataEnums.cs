#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       Enums
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.DataStruct.Enums
* 文 件 名:       Enums.cs
* 创建时间:       2022/1/7 8:40:22
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      ed459f96-df50-4126-ac2b-1e6fff3af338
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/1/7 8:40:22
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
    /// 公差类型
    /// </summary>
    public enum ToleranceType
    {
        [Description("直线度")]
        Straightness,

        [Description("平面度")]
        Flatness,

        [Description("圆度")]
        Roundness,

        [Description("圆柱度")]
        Cylindricity,

        [Description("线轮廓度")]
        Profileanyline,

        [Description("面轮廓度")]
        Profileanysurface,

        [Description("平行度")]
        Parallelism,

        [Description("垂直度")]
        Perpendicularity,

        [Description("倾斜度")]
        Angularity,

        [Description("位置度")]
        Position,

        [Description("同轴度")]
        Concentricity,

        [Description("对称度")]
        Symmetry,

        [Description("圆跳动")]
        CircularrunOut,

        [Description("全跳动")]
        TotalrunOut
    }
}