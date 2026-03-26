#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       IOType
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataStruct._6.Enums
* 文 件 名:       IOType.cs
* 创建时间:       2022/4/26 11:04:12
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      3e8c4d37-b0c2-4dcc-9f0c-3aae712a2623
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/26 11:04:12
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
    /// IO类型
    /// </summary>
    public enum IOType
    {
        [Description("数字IO")]
        Digital,

        [Description("模拟IO")]
        Analog
    }

    /// <summary>
    /// IO行为
    /// </summary>
    public enum IOBehavior
    {
        /// <summary>
        /// 输入
        /// </summary>
        [Description("输入")]
        Input,

        /// <summary>
        /// 输出
        /// </summary>
        [Description("输出")]
        Output,
    }

    /// <summary>
    /// IO设备监控
    /// </summary>
    public enum IOMonitor
    {
        [Description("位处于监控")]
        None,

        [Description("False")]
        False,

        [Description("True")]
        True
    }
}