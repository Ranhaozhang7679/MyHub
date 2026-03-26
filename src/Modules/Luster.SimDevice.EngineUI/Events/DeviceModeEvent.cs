#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       DeviceModeEvent
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.EngineUI.Events
* 文 件 名:       DeviceModeEvent.cs
* 创建时间:       2022/5/11 10:20:43
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      9744af1c-b2d8-4706-a579-87aa59d212fd
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/5/11 10:20:43
* 修 改 人:		  L05123
************************************************************************************/

#endregion

using Luster.Motion.DataStruct.Enums;
using Prism.Events;

namespace Luster.SimDevice.EngineUI.Events
{
    /// <summary>
    /// 模式切换
    /// </summary>
    public class DeviceModeEvent : PubSubEvent<DeviceMode>
    {
    }
}