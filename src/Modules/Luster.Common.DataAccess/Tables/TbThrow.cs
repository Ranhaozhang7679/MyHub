#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       TbThrow
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.CommonUI.Tables
* 文 件 名:       TbThrow.cs
* 创建时间:       2022/10/14 10:18:36
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      fe3a846c-44ab-4813-be34-9b045dcf9904
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/10/14 10:18:36
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using FreeSql.DataAnnotations;
using Luster.Common.DataAccess.Tables;
using Luster.Common.DataStruct.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.DataAccess.Tables
{
    /// <summary>
    /// 抛料
    /// </summary>
    [Table(Name = "ThrowTable")]
    public class TbThrow : BaseTable
    {
        /// <summary>
        /// 物料名称
        /// </summary>
        public string Material { get; set; }

        /// <summary>
        /// 抛料所属站
        /// </summary>
        public string Station { get; set; }

        /// <summary>
        /// 抛料原因
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// 机台运行模式
        /// </summary>
        public int Mode { get; set; }

        /// <summary>
        /// 条码信息
        /// </summary>
        public string SNCode { get; set; }
        public string Wip { get; set; }

    }
}
