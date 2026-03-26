#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       CameraModel
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.EngineUI.Models
* 文 件 名:       CameraModel.cs
* 创建时间:       2022/4/18 9:20:48
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      4c4b0051-a448-43d7-a031-561ce23baa81
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/18 9:20:48
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.SimDevice.Adapter;
using Luster.SimDevice.Engine;
using Luster.SimDevice.MotionCards;
using Luster.SimDevice.Real;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Prism.Mvvm;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Real;

namespace Luster.SimDevice.EngineUI.Models
{
    /// <summary>
    /// 设备模型
    /// </summary>
    public class DeviceModel : BindableBase
    {
        /// <summary>
        /// 唯一标识符号
        /// </summary>
        public Guid ID { get; set; }


        /// <summary>
        /// 是否被选择
        /// </summary>
        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                bool src = _isSelected;
                SetProperty(ref _isSelected, value);
                if (src != value)
                {
                    OnSelectedChanged(value);
                }
            }
        }

        /// <summary>
        /// 光源通道数
        /// </summary>
        private int _channelCount;

        public int ChannelCount
        {
            get { return _channelCount; } 
            set { SetProperty(ref _channelCount, value); }
        }

        /// <summary>
        /// 设备序列号
        /// </summary>
        private string _serialNumber;       
        public string SerialNumber
        {
            get => _serialNumber;
            set => SetProperty(ref _serialNumber, value);
        }
        /// <summary>
        /// 品牌
        /// </summary>
        private string _brand;
        public string Brand { get => _brand; set => SetProperty(ref _brand, value); }

        /// <summary>
        /// 设备名称
        /// </summary>
        private string _name;
        public string Name { get => _name; set => SetProperty(ref _name, value); }

        /// <summary>
        /// 适配器类型
        /// </summary>
        private Type _adapterType;        
        public Type AdapterType
        {
            get => _adapterType;
            set => SetProperty(ref _adapterType, value);
        }

        /// <summary>
        /// 适配器
        /// </summary>
        public IAdapter Adapter { get; set; }

        /// <summary>
        /// 适配器地址
        /// </summary>
        private string _adapterAddress;     
        public string AdapterAddress
        {
            get => _adapterAddress;
            set => SetProperty(ref _adapterAddress, value);
        }

        /// <summary>
        /// 设备状态，该字段不需要进行编辑修改
        /// </summary>
        private DeviceStatus _status = DeviceStatus.Offline;
        public DeviceStatus Status { get => _status; set => SetProperty(ref _status, value); }

        /// <summary>
        /// 设备类型
        /// </summary>
        public Type DeviceType { get; set; }

        /// <summary>
        /// 对象构造
        /// </summary>
        /// <param name="brand"></param>
        /// <param name="name"></param>
        public DeviceModel(string name, Type type)
        {
            Name = name;
            DeviceType = type;          
        }

        private void OnSelectedChanged(bool valk)
        {
            SelectedChangedEvent?.Invoke(valk);
        }

        public event Action<bool> SelectedChangedEvent;

        /// <summary>
        /// 通过设备更新对象
        /// </summary>
        /// <param name="device">设备对象</param>
        public void SetModel(IDevice device)
        {
            // ID 标识
            ID = device.ID;
            Name = device.Name;
            DeviceType = device.GetType();           
            Brand = device.Brand;
            if (device is CameraBase camera)
            {
                SerialNumber = camera.SerialNumber;
            }
            if (device is LineLaserBase lineLaser)
            {
                SerialNumber = lineLaser.SN;
            }
            if (device is ILightController light)
            {
                ChannelCount = light.Channel;
            }
            AdapterType = device.Adapter?.GetType();
            Adapter = device.Adapter;
            AdapterAddress = device.Adapter?.GetMethod();
        }

        /// <summary>
        /// 转换为设备对象
        /// </summary>
        /// <returns></returns>
        public IDevice ToDevice()
        {
            // 通过设备类型转换为类别对象
            var device = Activator.CreateInstance(DeviceType) as IDevice;
            device.ID = ID;
            device.Name = Name;
            Brand = device.Brand;
            device.Adapter = Adapter;
            if (device is CameraBase camera)
            {
                camera.SerialNumber = SerialNumber;
            }
            if (device is LineLaserBase lineLaser)
            {
                lineLaser.SN = SerialNumber;
            }
            if (device is ILightController light)
            {
                light.Channel = ChannelCount;
            }
            return device;
        }
    }
}