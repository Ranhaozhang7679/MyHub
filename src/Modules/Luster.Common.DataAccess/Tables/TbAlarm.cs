#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       TbAlarm
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.CommonUI.Tables
* 文 件 名:       TbAlarm.cs
* 创建时间:       2022/10/14 10:07:31
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      a7340468-c9a2-4013-9967-224c2aa54714
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/10/14 10:07:31
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using FreeSql.DataAnnotations;
using Luster.Common.DataAccess.Tables;
using Luster.Common.DataStruct.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.DataAccess.Tables
{
    /// <summary>
    /// 报警信息
    /// </summary>
    [Table(Name = "AlarmTable")]
    public class TbAlarm : BaseTable
    {
        /// <summary>
        /// 线体
        /// </summary>
        [Ignore]
        public string Line { get; set; }

        /// <summary>
        /// 机台
        /// </summary>
        [Ignore]
        public string Machine { get; set; }

        /// <summary>
        /// 报警模块
        /// </summary>
        [PropSort(1), DisplayName("报警模块")]
        public string Module { get; set; }

        /// <summary>
        /// 报警代码
        /// </summary>
        [PropSort(2), DisplayName("报警代码")]
        public string AlarmCode { get; set; }

        /// <summary>
        /// 报警类型
        /// </summary>
        [PropSort(3), DisplayName("报警类型")]
        public string AlarmType { get; set; }

        /// <summary>
        /// 报警原因
        /// </summary>
        [PropSort(4), DisplayName("报警原因")]
        public string Reason { get; set; }

        /// <summary>
        /// 处理方式
        /// </summary>
        [PropSort(5), DisplayName("处理方式"), StringLength(100)]
        public string ProcMethod { get; set; }

        /// <summary>
        /// 报警时长 秒单位
        /// </summary>
        [PropSort(6), DisplayName("报警时长"), StringLength(100)]
        public int AlarmLongTime { get; set; }

        /// <summary>
        /// 处理人
        /// </summary>
        [PropSort(7), DisplayName("处理人"), StringLength(100)]
        public string ProcUser { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        [PropSort(8), DisplayName("开始时间")]
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        [PropSort(9), DisplayName("结束时间")]
        public DateTime EndTime { get; set; }
    }
}
