#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       MotionCardContentVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.SubSystem.ViewModel.Device
* 文 件 名:       MotionCardContentVM.cs
* 创建时间:       2022/4/15 9:28:38
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      4be72eb4-f11f-4c34-b9f4-53836a449ed5
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/15 9:28:38
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Motion.DataStruct.Real;
using Luster.SimDevice.EngineUI;
using Luster.SimDevice.MotionCards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.SubSystem.ViewModel.Device
{
    public class MotionCardContentVM : BaseDeviceVM
    {
        protected MotionCardContentVM(ISimDeviceEngineUI _engine) : base(_engine, typeof(IMotionCard))
        {
        }

        
    }
}