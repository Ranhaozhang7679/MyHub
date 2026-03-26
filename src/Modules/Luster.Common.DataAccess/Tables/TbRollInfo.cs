#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       TbWorkOrder
* 机器名称:       L05123-02
* 命名空间:       Luster.Common.DataAccess.Tables
* 文 件 名:       TbWorkOrder.cs
* 创建时间:       2023/2/21 8:15:25
* 作    者:       刘克志
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      33e9ac14-4813-4eb5-8d36-34dfb1fc53cf
* 登录用户:       刘克志
* 所 属 域:       LUSTERINC
* 创建年份:       2023
* 修改时间:		  2023/2/21 8:15:25
* 修 改 人:		  刘克志
************************************************************************************/
#endregion

using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.DataAccess.Tables
{
    /// <summary>
    /// 工单数据
    /// </summary>
    [Table(Name = "RollInfo")]
    public class TbRollInfo : BaseTable
    {
        /// <summary>
        /// 工单编号
        /// </summary>
        public string RollNo { get; set; }

        /// <summary>
        /// 总卷料数量
        /// </summary>
        public int RollNum { get; set; }

        /// <summary>
        /// 已使用卷料数量
        /// </summary>
        public int UseNum { get; set; }

        /// <summary>
        /// 工单是否完结
        /// </summary>
        public bool IsEnd { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 工站
        /// </summary>
        public string Sation { get; set; }
    }
}