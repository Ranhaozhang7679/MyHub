
#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       TbVersion
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.CommonUI.Tables
* 文 件 名:       TbVersion.cs
* 创建时间:       2022/10/14 10:19:44
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      e15cb022-c2e2-4ce1-b213-33e30de94666
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/10/14 10:19:44
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
    [Table(Name = "VersionTable")]
    public class TbVersion : BaseTable
    {
        /// <summary>
        /// 数据库班别号
        /// </summary>
        public string Version { get; set; }
    }
}
