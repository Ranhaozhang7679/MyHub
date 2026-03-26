#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       AnalyzeModelcs
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.ReportUI.ViewModel
* 文 件 名:       AnalyzeModelcs.cs
* 创建时间:       2022/7/13 13:00:10
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      b854a0f0-da67-4e26-9339-24948fdc5116
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/13 13:00:10
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
    public class AnalyzeModel
    {
        public DateTime Time { get; set; }

        /// <summary>
        ///良率
        /// </summary>
        public double OkRate { get; set; }

        /// <summary>
        /// NG率
        /// </summary>
        public double NgRate { get; set; }

        /// <summary>
        /// 良品数量
        /// </summary>
        public int OkCount { get; set; }

        /// <summary>
        /// NG数量
        /// </summary>       
        public int NgCount { get; set; }

        /// <summary>
        /// CT
        /// </summary>
        public double SumCT { get; set; }

    }
}
