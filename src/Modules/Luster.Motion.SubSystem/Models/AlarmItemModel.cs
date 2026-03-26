#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       AlarmItemModel
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.SubSystem.Models
* 文 件 名:       AlarmItemModel.cs
* 创建时间:       2022/9/9 10:20:49
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      61d67fd0-1303-44ed-aeeb-11f347103631
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/9 10:20:49
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using Luster.Common.DataStruct.Attributes;
using Luster.Motion.DataStruct.Virtual;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.SubSystem.Models
{
    public class AlarmItemModel : BindableBase
    {
        /// <summary>
        /// 序号
        /// </summary>
        private int _index;
        [Ignore]
        [PropSort(0), DisplayName("序号")]
        public int Index
        {
            get { return _index; }
            set { SetProperty(ref _index, value); }
        }

        /// <summary>
        /// 名称
        /// </summary>
        [PropSort(1), DisplayName("名称")]
        public string Name { get; set; }

        /// <summary>
        /// 报警代码
        /// </summary>
        private string _alarmCode;
        [PropSort(2), DisplayName("报警代码")]
        public string AlarmCode
        {
            get { return _alarmCode; }
            set
            {
                if (_alarmCode != value)
                {
                    SetProperty(ref _alarmCode, value);
                    if(Tag!=null)
                    {
                        Tag.AlarmCode = value;
                    }
      
                }
            }
        }

        private bool _isSelected;
        [Ignore]
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    SetProperty(ref _isSelected, value);
                    OnSelected(value);
                }
            }
        }

        /// <summary>
        /// 选择发生了变更
        /// </summary>
        public event Action<bool> SelectedChanged;

        public void OnSelected(bool isSelected)
        {
            SelectedChanged?.Invoke(isSelected);
        }
        [Ignore]
        public IVirtualDevice Tag { get;  set; }



        public AlarmItemModel(IVirtualDevice device)
        {
            Tag = device;
            AlarmCode = device.AlarmCode;
            Name = device.Name;
        }

        public AlarmItemModel()
        {
        }
    }
}
