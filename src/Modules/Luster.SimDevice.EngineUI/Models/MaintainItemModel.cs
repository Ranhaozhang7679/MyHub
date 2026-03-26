#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       MaintainItemModel
* 机器名称:       Z05592
* 命名空间:       Luster.SimDevice.EngineUI.Models
* 文 件 名:       MaintainItemModel.cs
* 创建时间:       2022/12/9 10:55:36
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      19f317a9-8d7b-4435-80a5-4bbfaa5dc9cb
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/12/9 10:55:36
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using Luster.Motion.DataStruct.Interfaces;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Luster.SimDevice.EngineUI.Models
{
    public class MaintainItemModel : BindableBase
    {
        /// <summary>
        /// 设备名称
        /// </summary>
        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        /// <summary>
        /// 设备最大寿命
        /// </summary>
        private double _maxHP;
        public double MaxHP
        {
            get { return _maxHP; }
            set
            {
                var src = _maxHP;
                SetProperty(ref _maxHP, value);

                if (DeviceTag != null && src != value)
                {
                    DeviceTag.MaxHP = (float)value;
                }
            }
        }

        /// <summary>
        /// 需要维护的里程
        /// </summary>
        private double _maintainHP;
        public double MaintainHP
        {
            get { return _maintainHP; }
            set
            {
                if (_maintainHP != value)
                {
                    SetProperty(ref _maintainHP, value);
                    DeviceTag.MaintainHP = (float)value;
                }
            }
        }

        /// <summary>
        /// 已使用寿命
        /// </summary>
        private double _usedHP;
        public double UsedHP
        {
            get { return _usedHP; }
            set
            {
                if (_usedHP != value)
                {
                    SetProperty(ref _usedHP, value);
                    DeviceTag.UsedHP = (float)value;
                }
            }
        }

        /// <summary>
        /// 当前使用寿命，每次保养后重置
        /// </summary>
        private double _currentHP;
        public double CurrentHP
        {
            get { return _currentHP; }
            set
            {
                if (_currentHP != value)
                {
                    SetProperty(ref _currentHP, value);
                    DeviceTag.CurrentHP = (float)value;
                }
            }
        }

        /// <summary>
        /// 使用百分比
        /// </summary>
        private double _percent;
        public double Percent
        {
            get { return _percent; }
            set { SetProperty(ref _percent, value); }
        }

        /// <summary>
        /// 百分比文字提示
        /// </summary>
        private string _actualStr;
        public string ActualStr
        {
            get { return _actualStr; }
            set { SetProperty(ref _actualStr, value); }
        }

        /// <summary>
        /// 进度条色彩
        /// </summary>
        private Brush _foreColor;

        public Brush ForeColor
        {
            get => _foreColor;
            set => SetProperty(ref _foreColor, value);
        }

        /// <summary>
        /// 附加设备
        /// </summary>
        public IMaintain DeviceTag { get; set; }

        public MaintainItemModel(IMaintain device)
        {
            DeviceTag = device;
            Name = device.Name;
            MaxHP = device.MaxHP;
            MaintainHP = device.MaintainHP;
            UsedHP = device.UsedHP;
            CurrentHP = device.CurrentHP;
            Percent = device.GetPercent();
            ActualStr = device.GetActual();
        }

    }
}
