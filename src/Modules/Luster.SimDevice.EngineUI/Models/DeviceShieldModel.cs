#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       DeviceShieldModel
* 机器名称:       Z05592
* 命名空间:       Luster.SimDevice.EngineUI.Models
* 文 件 名:       DeviceShieldModel.cs
* 创建时间:       2022/8/12 9:29:20
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      3c83ffe8-21d1-443c-9214-fca6b9d69b71
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/8/12 9:29:20
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.DataStruct.Real;
using Luster.Motion.DataStruct.Virtual;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.EngineUI.Models
{
    public class DeviceShieldModel : BindableBase
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
        /// 是否屏蔽
        /// </summary>
        private bool _isShield;
        public bool IsShield
        {
            get { return _isShield; }
            set
            {
                if (_isShield != value)
                {
                    SetProperty(ref _isShield, value);
                    Tag.IsShield = value;
                }
            }
        }
        private int _index;
        public int Index 
        {
            get => _index;
            set { SetProperty(ref _index, value); }
        }

        /// <summary>
        /// 附加设备
        /// </summary>
        public IDeviceShield Tag { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="tag"></param>
        public DeviceShieldModel(IDeviceShield tag)
        {
            Tag = tag;

            if (tag is IVirtualDevice virtualDevice)
            {
                Name = virtualDevice.Name;
            }

            IsShield = tag.IsShield;
         
        }
    }
}
