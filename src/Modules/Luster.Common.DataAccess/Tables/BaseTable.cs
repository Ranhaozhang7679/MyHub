#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       BaseTable
* 机器名称:       Z05592
* 命名空间:       Luster.Common.DataAccess.Tables
* 文 件 名:       BaseTable.cs
* 创建时间:       2022/10/14 10:09:15
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      e5f986e0-3ca1-4ecd-89f1-267028f36743
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/10/14 10:09:15
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using FreeSql.DataAnnotations;
using Luster.Common.DataStruct.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.DataAccess.Tables
{
    /// <summary>

    /// 基础数据表
    /// </summary>
    public abstract class BaseTable
    {
        /// <summary>
        /// 唯一ID号
        /// </summary>
        [Ignore]
        [Column(IsIdentity = true, IsPrimary = true)]
        public long ID { get; set; }

        /// <summary>
        /// 记录时间
        /// </summary>
        [Ignore]
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public BaseTable()
        {
            CreateTime = DateTime.Now;
        }
    }
}
