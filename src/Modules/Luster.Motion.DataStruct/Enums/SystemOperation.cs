#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       SystemOperation
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataStruct.Enums
* 文 件 名:       SystemOperation.cs
* 创建时间:       2022/7/28 9:50:08
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      f7a1a45c-82b2-4543-9a48-69e9d8284826
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/28 9:50:08
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
    /// 系统操作
    /// </summary>
    public enum SystemOperation
    {
        [Description("软件启动")]
        SoftStart,

        [Description("开始运行")]
        Start,

        [Description("停止运行")]
        Stop,

        [Description("暂停运行")]
        Pause,

        [Description("报警复位")]
        Recovery,

        [Description("设备回零")]
        Home,

        [Description("软件关闭")]
        SoftClose,

        [Description("程序继续")]
        Contine,

        /// <summary>
        /// 设备保养
        /// </summary>
        [Description("设备保养")]
        Repair,

        [Description("设备报警")]
        Alarm,
    }
}