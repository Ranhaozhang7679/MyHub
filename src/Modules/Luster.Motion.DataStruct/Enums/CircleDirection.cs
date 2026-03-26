#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       CircleDirection
* 机器名称:       L05123-02
* 命名空间:       Luster.Motion.DataStruct.Enums
* 文 件 名:       CircleDirection.cs
* 创建时间:       2022/12/21 13:39:55
* 作    者:       刘克志
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      74424d70-7446-4e1d-86e0-82d15ae925f0
* 登录用户:       刘克志
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/12/21 13:39:55
* 修 改 人:		  刘克志
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
    public enum AngleDirection
    {
        [Description("顺时针")]
        Clockwise,

        [Description("逆时针")]
        AntiClockwise
    }
}