#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       TbAccessory
* 机器名称:       Z05592
* 命名空间:       Luster.Common.DataAccess.Tables
* 文 件 名:       TbAccessory.cs
* 创建时间:       2022/12/5 9:42:20
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      20cc036f-4e15-46fd-baad-2381037f2148
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/12/5 9:42:20
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
    [Table(Name = "AccessoryTable")]
    public class TbAccessory : BaseTable
    {
        /// <summary>
        /// 辅料名称
        /// </summary>
        [PropSort(1), DisplayName("辅料名称")]
        public string Name { get; set; }

        /// <summary>
        /// 预警值，物料数量小于设置预警值时报警，百分比
        /// </summary>
        [PropSort(2), DisplayName("预警值")]
        public int AlarmThreshold { get; set; }

        /// <summary>
        /// 单piece使用数量，一个产品使用几个辅料
        /// </summary>
        [PropSort(3), DisplayName("单piece使用数量")]
        public int PieceUsedCount { get; set; }
    }
}
