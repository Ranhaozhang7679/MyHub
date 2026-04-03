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
using Luster.Motion.DigitalSetup.Datas;
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

        /// <summary>
        /// 点检状态
        /// </summary>
        private CheckStatus _checkStatus = CheckStatus.NotChecked;
        public CheckStatus CheckStatus
        {
            get => _checkStatus;
            set => SetProperty(ref _checkStatus, value);
        }

        /// <summary>
        /// 上次点检时间
        /// </summary>
        private DateTime? _lastCheckTime;
        public DateTime? LastCheckTime
        {
            get => _lastCheckTime;
            set => SetProperty(ref _lastCheckTime, value);
        }

        /// <summary>
        /// 上次点检时间显示文本（格式化后的字符串）
        /// </summary>
        public string LastCheckTimeDisplay
        {
            get
            {
                if (!LastCheckTime.HasValue)
                    return "未点检";

                var time = LastCheckTime.Value;
                var now = DateTime.Now;

                // 如果是今天
                if (time.Date == now.Date)
                {
                    return $"今天 {time:HH:mm}";
                }
                // 如果是昨天
                else if (time.Date == now.AddDays(-1).Date)
                {
                    return $"昨天 {time:HH:mm}";
                }
                // 如果是今年
                else if (time.Year == now.Year)
                {
                    return time.ToString("MM-dd HH:mm");
                }
                // 其他情况
                else
                {
                    return time.ToString("yyyy-MM-dd HH:mm");
                }
            }
        }

        /// <summary>
        /// 上次点检人员
        /// </summary>
        private string _lastCheckOperator;
        public string LastCheckOperator
        {
            get => _lastCheckOperator;
            set => SetProperty(ref _lastCheckOperator, value);
        }

        /// <summary>
        /// 点检备注信息
        /// </summary>
        private string _checkRemark;
        public string CheckRemark
        {
            get => _checkRemark;
            set => SetProperty(ref _checkRemark, value);
        }

        /// <summary>
        /// 父页面Region (用于构建PageKey)
        /// </summary>
        public string ParentRegion { get; set; }

        /// <summary>
        /// 页面唯一标识 (格式: ParentRegion_Name)
        /// </summary>
        public string PageKey
        {
            get
            {
                if (string.IsNullOrEmpty(ParentRegion))
                    return Name;
                return $"{ParentRegion}_{Name}";
            }
        }
    }
}
