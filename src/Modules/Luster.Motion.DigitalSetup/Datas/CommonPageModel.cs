#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ReportType
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.ReportUI.Model
* 文 件 名:       ReportType.cs
* 创建时间:       2022/10/18 9:02:15
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      cacb98ce-67d3-40f2-8ed6-9917dc5ee69c
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/10/18 9:02:15
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.DigitalSetup.Datas
{
    public class CommonPageModel : BindableBase
    {
        /// <summary>
        /// 名称
        /// </summary>
        private string _name;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        /// <summary>
        /// 是否被选择
        /// </summary>
        private bool _isSelected;

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        private string _region;

        public string Region
        {
            get => _region;
            set => SetProperty(ref _region, value);
        }

        private Type _viewType;
        public Type ViewType
        {
            get => _viewType;
            set => SetProperty(ref _viewType, value);
        }

        /// <summary>
        /// 一键点检确认弹窗消息
        /// </summary>
        private string _checkConfirmMessage;
        public string CheckConfirmMessage
        {
            get => _checkConfirmMessage;
            set => SetProperty(ref _checkConfirmMessage, value);
        }

        /// <summary>
        /// 是否启用
        /// </summary>
        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }
    }
}
