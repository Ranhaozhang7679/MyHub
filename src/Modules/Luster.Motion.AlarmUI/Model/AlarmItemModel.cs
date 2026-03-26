#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       AlarmItemModel
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.AlarmUI.ViewModel
* 文 件 名:       AlarmItemModel.cs
* 创建时间:       2022/7/12 10:15:53
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      6b9f93ff-2a21-44b6-823e-c97687cce5ca
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/12 10:15:53
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using Luster.Motion.CommonUI.ViewModel;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.AlarmUI.Model
{
    public class AlarmItemModel
    {
        /// <summary>
        /// 报警ID号
        /// </summary>
        public long Id
        {
            get; set;
        }

        /// <summary>
        /// 报警模块
        /// </summary>
        public string Module
        {
            get; set;
        }

        /// <summary>
        /// 报警类型
        /// </summary>
        public string AlarmType
        {
            get; set;
        }

        /// <summary>
        /// 报警原因
        /// </summary>
        public string Reason
        {
            get; set;
        }

        /// <summary>
        /// 处理方式
        /// </summary>
        public string ProcMethod
        {
            get; set;
        }

        /// <summary>
        /// 报警时长 毫秒单位
        /// </summary>
        public int AlarmLongTime
        {
            get; set;
        }

        /// <summary>
        /// 处理人
        /// </summary>
        public string ProcUser
        {
            get; set;
        }

        /// <summary>
        /// 开始时间
        /// </summary>
        public string StartTime
        {
            get; set;
        }

        /// <summary>
        /// 结束时间
        /// </summary>
        public string EndTime
        {
            get; set;
        }

        /// <summary>
        /// 报警代码
        /// </summary>
        public string AlarmCode 
        {
            get; set;
        }
    }
}
