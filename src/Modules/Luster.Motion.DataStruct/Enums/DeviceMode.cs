#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       DeviceMode
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataStruct.Enums
* 文 件 名:       DeviceMode.cs
* 创建时间:       2022/4/2 15:20:50
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      3ea7ef78-d815-4b20-a565-e3b03822936c
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/2 15:20:50
* 修 改 人:		  L05123
************************************************************************************/

#endregion

using Luster.Common.DataStruct.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.DataStruct.Enums
{
    public enum DeviceMode
    {
        /// <summary>
        /// 虚拟装填
        /// </summary>
        [Description("离线模式")]
        Virtual,

        /// <summary>
        /// 真实设备
        /// </summary>
        [Description("在线模式")]
        Real,

        [Ignore]
        [Description("空跑模式")]
        Empty,

        [Ignore]
        [Description("生产模式")]
        Project,

        [Ignore]
        [Description("调试模式")]
        Debug
    }
}