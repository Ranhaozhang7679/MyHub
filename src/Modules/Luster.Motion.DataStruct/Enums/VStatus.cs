#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       VStatus
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataStruct.Enums
* 文 件 名:       VStatus.cs
* 创建时间:       2022/8/5 15:56:28
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      8ba19206-2c8b-4ea3-9b37-efc437c74812
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/8/5 15:56:28
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
    /// 虚拟设备的状态
    /// </summary>
    public enum VStatus
    {
        [Description("空闲中")]
        Idle,

        [Description("运动中")]
        Running,

        [Description("暂停")]
        Pause,

        [Description("停止")]
        Stop,

        [Description("回零中")]
        Home
    }
}