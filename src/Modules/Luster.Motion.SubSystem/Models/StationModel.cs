#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       StationModel
* 机器名称:       L05590
* 命名空间:       Luster.Motion.SubSystem.Models
* 文 件 名:       StationModel.cs
* 创建时间:       2022/9/8 9:37:12
* 作    者:       L05590
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       fanlu5590@lusterinc.com 
* 唯一标识：      2c173d1e-3efd-44b9-9a42-895e8e171110
* 登录用户:       fanlu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/8 9:37:12
* 修 改 人:		  L05590
************************************************************************************/
#endregion

using Luster.TaskFlow.Common.Functions;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Luster.Motion.SubSystem.Models
{
    public class StationModel: BindableBase
    {
        /// <summary>
        /// 名称
        /// </summary>
        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        /// <summary>
        /// 颜色
        /// </summary>
        private Brush _backColor;
        public Brush BackColor
        {
            get { return _backColor; }
            set { SetProperty(ref _backColor, value); }
        }

        /// <summary>
        /// 最后一站
        /// </summary>
        private bool _isLastStation;
        public bool IsLastStation
        {
            get { return _isLastStation; }
            set { SetProperty(ref _isLastStation, value); }
        }

        /// <summary>
        /// 是否选择
        /// </summary>
        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        /// <summary>
        /// 是否可用
        /// </summary>
        private bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { SetProperty(ref _isEnabled, value); }
        }
        /// <summary>
        /// 图标
        /// </summary>
        private string _icon;
        public string Icon
        {
            get { return _icon; }
            set { SetProperty(ref _icon, value); }
        }

    }
}