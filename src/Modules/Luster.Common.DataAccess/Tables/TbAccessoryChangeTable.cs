#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       TbAccessoryChangeTable
* 机器名称:       Z05592
* 命名空间:       Luster.Common.DataAccess.Tables
* 文 件 名:       TbAccessoryChangeTable.cs
* 创建时间:       2022/12/5 9:57:48
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      d88603c2-a64b-4bc7-90c5-66ea53e29e6f
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/12/5 9:57:48
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using FreeSql.DataAnnotations;
using Luster.Common.DataStruct.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.DataAccess.Tables
{
    [Table(Name = "AccessoryChangeTable")]
    public class TbAccessoryChangeTable : BaseTable
    {
        /// <summary>
        /// 跟换辅料名称
        /// </summary>
        [PropSort(1), DisplayName("辅料名称")]
        public string Name { get; set; }

        /// <summary>
        /// 辅料的批次号
        /// </summary>
        [PropSort(2), DisplayName("批号")]
        public string BatchNo { get; set; }

        /// <summary>
        /// 辅料数量
        /// </summary>
        [PropSort(3), DisplayName("数量")]
        public double Count { get; set; }

        /// <summary>
        /// 辅料所属工位，针对机台有多个工作工位
        /// </summary>
        [PropSort(4), DisplayName("工位名称")]
        public string StationName { get; set; }

    }
}
