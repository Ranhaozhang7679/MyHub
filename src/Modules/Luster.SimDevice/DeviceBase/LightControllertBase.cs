#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       Light
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.Light
* 文 件 名:       Light.cs
* 创建时间:       2022/4/15 9:31:12
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      57df183f-4edc-4b52-a7d1-feec80e55e53
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/15 9:31:12
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Motion.DataStruct.Enums;
using Luster.SimDevice.Real;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice
{
   
    /// <summary>
    /// 光源
    /// </summary>
    public abstract class LightControllertBase : DeviceBase
    {
        public override DeviceType DeviceType => DeviceType.LightController;

        public override string Brand => "";

        public override string Icon => "\xe6d8";
       
    }
}