#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       PrinterBase
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.DeviceBase
* 文 件 名:       PrinterBase.cs
* 创建时间:       2022/6/17 11:21:28
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      55705e08-3d9b-4bd7-9e3f-cf1972ddc1ac
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/6/17 11:21:28
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Motion.DataStruct.Enums;
using Luster.SimDevice.Real;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice
{
    public abstract class PrinterBase : DeviceBase
    {
        /// <summary>
        /// 品牌
        /// </summary>
        public override string Brand => "";

        /// <summary>
        /// 图标
        /// </summary>
        public override string Icon => "\xe6d8";

        /// <summary>
        /// 设备类型
        /// </summary>
        public override DeviceType DeviceType => DeviceType.Printer;
    }
}