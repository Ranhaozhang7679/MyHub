#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       AlarmAnalyzeModel_
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.AlarmUI.ViewModel
* 文 件 名:       AlarmAnalyzeModel_.cs
* 创建时间:       2022/7/14 13:37:12
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      b0fcc47d-96c7-4448-8d54-76ceeaad65f0
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/14 13:37:12
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
    public class AlarmAnalyzeModel : BindableBase
    {
        /// <summary>
        /// 位置
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 大小
        /// </summary>
        public int Weight { get; set; }
    }
}
