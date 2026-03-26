#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       MaintainDeviceTypeItem
* 机器名称:       Z05592
* 命名空间:       Luster.SimDevice.EngineUI.Models
* 文 件 名:       MaintainDeviceTypeItem.cs
* 创建时间:       2022/12/9 13:29:20
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      94398b66-0a9a-4d3f-b324-b0ce5dbc41a5
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/12/9 13:29:20
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.EngineUI.Models
{
    public class MaintainDeviceTypeItem
    {
        /// <summary>
        /// 设备类型名称
        /// </summary>
        public string ItemName { get; set; }

        /// <summary>
        /// 设备类型类型
        /// </summary>
        public Type ItemType { get; set; }
    }
}
