#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       CylinderType
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataStruct.Enums
* 文 件 名:       CylinderType.cs
* 创建时间:       2022/7/8 14:41:18
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      4af2f62e-cecb-441b-a0cd-cdc3773e0c5d
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/8 14:41:18
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct.Enums
{
    /// <summary>
    /// 气缸类型
    /// </summary>
    public enum CylinderType
    {
        [Description("伸出")]
        Extend,

        [Description("缩回")]
        Retract
    }

    /// <summary>
    /// 气缸分类
    /// </summary>
    public enum CylinderCategory
    {
        [Description("单控气缸")]
        Single,

        [Description("双控气缸")]
        Double
    }
}