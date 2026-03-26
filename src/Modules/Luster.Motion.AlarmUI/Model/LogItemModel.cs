#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LogItemVM
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.AlarmUI.ViewModel
* 文 件 名:       LogItemVM.cs
* 创建时间:       2022/7/15 10:47:31
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      e59c39a0-65a8-4403-ba08-75c9028c6584
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/15 10:47:31
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.AlarmUI.Model
{
    public class LogItemModel
    {
        /// <summary>
        /// log编号
        /// </summary>
        public long Id
        {
            get; set;
        }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName
        {
            get; set;
        }

        /// <summary>
        /// 操作描述
        /// </summary>
        public string Operation
        {
            get; set;
        }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime
        {
            get; set;
        }
    }
}
