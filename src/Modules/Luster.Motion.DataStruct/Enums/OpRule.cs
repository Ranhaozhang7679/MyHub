#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       OpRule
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataStruct.Enums
* 文 件 名:       OpRule.cs
* 创建时间:       2022/8/15 20:42:36
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      9c41696e-8870-47bf-b0e1-73f10dd6d026
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/8/15 20:42:36
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
    /// 条件
    /// </summary>
    public enum OpRule
    {
        [Description("等于")]
        Equal,

        [Description("大于等于")]
        GatherEqual,

        [Description("小于等于")]
        LessEqual
    }
}