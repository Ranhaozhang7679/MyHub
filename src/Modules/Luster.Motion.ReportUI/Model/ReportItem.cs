#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ReportItemVM
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.ReportUI.ViewModel
* 文 件 名:       ReportItemVM.cs
* 创建时间:       2022/7/5 9:58:57
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      4d38eaf2-1146-41bd-9d80-7802b3b18271
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/5 9:58:57
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.ReportUI.Model
{
    public class ReportItem
    {
        public long Id
        {
            get; set;
        }

        /// <summary>
        /// 条码信息
        /// </summary>

        public string BarCode
        {
            get; set;
        }

        /// <summary>
        /// 扫码时间
        /// </summary>

        public DateTime OutTime
        {
            get; set;
        }

        /// <summary>
        /// 扫码时间
        /// </summary>

        public DateTime EnterTime
        {
            get; set;
        }

        /// <summary>
        /// 字据号
        /// </summary>

        public string Jig
        {
            get; set;
        }

        /// <summary>
        /// 结果
        /// </summary>

        public string Result
        {
            get; set;
        }


        /// <summary>
        /// CT
        /// </summary>

        public Double CT
        {
            get; set;
        }
        /// 结果
        /// </summary>

        public string ImagePath
        {
            get; set;
        }

        /// <summary>
        /// ng原因
        /// </summary>
        public string NgReason
        {
            get; set;
        }


        public Dictionary<string, object> Data { get; set; }
    }
}
