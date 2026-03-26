#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       DeviceStatus
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataStruct.Enums
* 文 件 名:       DeviceStatus.cs
* 创建时间:       2022/4/6 16:03:58
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      fac3f22a-1721-491c-8494-a07d93c595aa
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/6 16:03:58
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
    /// 设备状态
    /// </summary>
    public enum DeviceStatus
    {
        [Description("离线")]
        Offline,

        [Description("准备")]
        Ready,

        [Description("在线")]
        Online,

        [Description("运行中")]
        Running,

        [Description("忙碌")]
        Busy,

        [Description("停止")]
        Stop
    }
}