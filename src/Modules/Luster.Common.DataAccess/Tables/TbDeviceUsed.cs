#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       TbDeviceUsed
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.CommonUI.Tables
* 文 件 名:       TbDeviceUsed.cs
* 创建时间:       2022/10/14 10:12:56
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      9acc24bb-2dd0-4fbe-ab36-f5906a4167d0
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/10/14 10:12:56
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using FreeSql.DataAnnotations;
using Luster.Common.DataAccess.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.DataAccess.Tables
{
    /// <summary>
    /// 设备使用次数
    /// </summary>
    [Table(Name = "DeviceUsedTable")]
    public class TbDeviceUsed : BaseTable
    {
        /// <summary>
        /// 设备名称
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// 设备动作
        /// </summary>
        public string DeviceAction { get; set; }
    }
}
