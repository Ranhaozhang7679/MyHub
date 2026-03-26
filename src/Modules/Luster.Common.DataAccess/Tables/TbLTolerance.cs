#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       TbLTolerance
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.CommonUI.Tables
* 文 件 名:       TbLTolerance.cs
* 创建时间:       2022/10/14 10:13:37
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      7669b416-cba7-46cf-892b-7c42052e6ff4
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/10/14 10:13:37
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
    [Table(Name = "LToleranceTable")]
    public class TbLTolerance : BaseTable
    {
        /// <summary>
        /// 所属配方名称
        /// </summary>
        public string RecipeName { get; set; }
        /// <summary>
        /// 匹配标识
        /// </summary>
        public Guid MapId { get; set; }

        /// <summary>
        /// 匹配名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 标准值
        /// </summary>
        public double Standard { get; set; }

        /// <summary>
        /// 公差下限
        /// </summary>
        public double ToleranceMin { get; set; }

        /// <summary>
        /// 公差下限
        /// </summary>
        public double ToleranceMax { get; set; }

        /// <summary>
        /// 补偿值
        /// </summary>
        public double Compensate { get; set; }
    }
}
