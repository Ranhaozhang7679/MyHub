#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       HistoryTrendModel
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.ReportUI.Model
* 文 件 名:       HistoryTrendModel.cs
* 创建时间:       2022/7/19 11:09:22
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      4c466d91-9b2f-451b-b738-fd02b57b3cda
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/19 11:09:22
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.ReportUI.Model
{
    public class HistoryTrendModel
    {
        /// <summary>
        /// 点位日期
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// OK数量
        /// </summary>
        public int OkCount { get; set; }

        /// <summary>
        /// NG数量
        /// </summary>
        public int NgCount { get; set; }

    }
}
