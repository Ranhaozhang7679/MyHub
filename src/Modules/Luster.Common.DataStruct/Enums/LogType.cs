#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LogType
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.DataStruct.Enums
* 文 件 名:       LogType.cs
* 创建时间:       2022/5/12 9:13:48
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      f50f6d96-ad51-4058-84ee-04fa285ef691
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/12 9:13:48
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
    /// 日志消息类型
    /// </summary>
    public enum LogType
    {
        [Description("Debug")]
        Debug,

        [Description("Info")]
        Info,

        [Description("Warning")]
        Warning,

        [Description("Error")]
        Error,
    }
}