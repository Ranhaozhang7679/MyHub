#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       StationType
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Motion.Enums
* 文 件 名:       StationType.cs
* 创建时间:       2022/6/29 17:22:52
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      bee1aa23-5160-4aac-a221-bb530c7e22aa
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/29 17:22:52
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.TaskFlow.Motion.Enums
{
    public enum StationType
    {
        [Description("本站有料")]
        ThisHave,

        [Description("本站要料")]
        ThisGet,
    }
}