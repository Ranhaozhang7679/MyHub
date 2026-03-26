#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       CameraDialogVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.SubSystem.ViewModel.Dialog
* 文 件 名:       CameraDialogVM.cs
* 创建时间:       2022/4/15 11:05:54
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      9ec1c17c-9510-4fc9-bcfc-ea08712349f9
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/15 11:05:54
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Extensions;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.Real;
using Luster.SimDevice.Adapter;
using Luster.SimDevice.Engine;
using Luster.SimDevice.EngineUI;
using Luster.SimDevice.EngineUI.Models;
using Luster.SimDevice.MotionCards;
using Luster.SimDevice.Real;
using Luster.SimDevice.SubSystem.ViewModel.Device;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.SubSystem.ViewModel.Dialog
{
    public abstract class DeviceDialogVM : DialogVM
    {
        public virtual bool IsLightController => false;

        protected ISimDeviceEngineUI _engineUI;

        protected DeviceDialogVM(ISimDeviceEngineUI _engine, Type type) : base(_engine)
        {
            _engineUI = _engine;
            InitModel(type);
        }

        private void InitModel(Type type)
        {
            var adapters = _engineUI.Engine.GetAdapters();
            // 厂商
            BrandList = deviceEngine.GetBrands(type);
            ConnectionTypes = new List<KeyValue>();
            foreach (var item in adapters)
            {
                ConnectionTypes.Add(new KeyValue()
                {
                    Desc = item.Name,
                    Value = item
                });
            }

            // 插槽
            SlotList = new List<KeyValue>();
            for (int i = 0; i < 10; i++)
            {
                SlotList.Add(new KeyValue() { Desc = $"Slot {i}", Value = i });
            }

            // Com
            //Coms = Com.GetComs();//com类中获取所有com端口
            Coms = new List<KeyValue>();
            for (int i = 0; i < 10; i++)
            {
                Coms.Add(new KeyValue() { Desc = $"Com{i}", Value = $"Com{i}" });
            }

            // Usb
            Usbs = new List<KeyValue>();
            for (int i = 0; i < 10; i++)
            {
                Usbs.Add(new KeyValue() { Desc = $"USB{i + 1}", Value = $"USB{i + 1}" });
            }

            BaudRateList = new List<int>() { 4800, 9600, 19200, 38400, 57600, 115200, 230400 };
            StopBitsList = typeof(StopBits).EnumToDataSource();
            ParityList = typeof(Parity).EnumToDataSource();

            IsCom = false;
            IsPCIe = false;
            IsTcp = false;
            IsUsb = false;
        }

        public List<KeyValue> StopBitsList { get; set; }

        public List<KeyValue> ParityList { get; set; }

        /// <summary>
        /// 波特率
        /// </summary>
        private int _baudRate = 9600;
        public int BaudRate { get => _baudRate; set => SetProperty(ref _baudRate, value); }

        /// <summary>
        /// 数据位
        /// </summary>
        private int _dataBits = 8;
        public int DataBits { get => _dataBits; set => SetProperty(ref _dataBits, value); }

        /// <summary>
        /// 奇偶校验位
        /// </summary>
        private Parity _parity;
        public Parity Parity { get => _parity; set => SetProperty(ref _parity, value); }

        /// <summary>
        /// 停止位
        /// </summary>
        private StopBits _stopBits = StopBits.One;
        public StopBits StopBits { get => _stopBits; set => SetProperty(ref _stopBits, value); }

        private List<int> _baudRateList;
        public List<int> BaudRateList 
        {
            get => _baudRateList;
            set => SetProperty(ref _baudRateList, value);
        }

        /// <summary>
        /// TCP通讯方式
        /// </summary>
        private bool _isTcp;
        public bool IsTcp { get => _isTcp; set => SetProperty(ref _isTcp, value); }

        /// <summary>
        /// 串口通讯方式
        /// </summary>
        private bool _isCom;
        public bool IsCom { get => _isCom; set => SetProperty(ref _isCom, value); }

        /// <summary>
        /// PCI通讯方式
        /// </summary>
        private bool _isPCIe;
        public bool IsPCIe { get => _isPCIe; set => SetProperty(ref _isPCIe, value); }

        /// <summary>
        /// TCP通讯方式
        /// </summary>
        private bool _isUsb;
        public bool IsUsb { get => _isUsb; set => SetProperty(ref _isUsb, value); }

        /// <summary>
        /// IP地址
        /// </summary>
        private string _ip;
        public string IpAddress { get => _ip; set => SetProperty(ref _ip, value); }

        /// <summary>
        /// 端口号
        /// </summary>
        private int _port;
        public int Port { get => _port; set => SetProperty(ref _port, value); }

        /// <summary>
        /// PCIe插槽
        /// </summary>
        private int _pcie;
        public int PCIe { get => _pcie; set => SetProperty(ref _pcie, value); }

        /// <summary>
        /// 插槽池
        /// </summary>
        public List<KeyValue> SlotList { get; private set; }

        /// <summary>
        /// 通讯方式次
        /// </summary>
        public List<KeyValue> ConnectionTypes { get; private set; }

        /// <summary>
        /// 设配器类型
        /// </summary>
        private Type _adapter;
        [Required]
        public Type Adapter
        {
            get => _adapter;
            set
            {
                SetProperty(ref _adapter, value);
                AdapterTypeChanged();
            }
        }

        /// <summary>
        /// Com端口
        /// </summary>
        private string _comName;
        public string ComName { get => _comName; set => SetProperty(ref _comName, value); }

        /// <summary>
        /// 参数信息
        /// </summary>
        public List<KeyValue> Coms { get; private set; }

        /// <summary>
        /// Usb
        /// </summary>
        private string _usb;
        public string Usb { get => _usb; set => SetProperty(ref _usb, value); }

        /// <summary>
        /// 参数信息
        /// </summary>
        public List<KeyValue> Usbs { get; private set; }

        /// <summary>
        /// 唯一标识
        /// </summary>
        public Guid ID { get; set; } = Guid.Empty;

        /// <summary>
        /// 厂商信息
        /// </summary>
        public List<KeyValue> BrandList { get; private set; }

        /// <summary>
        /// 品牌
        /// </summary>
        private Type _type;
        [Required]
        public Type DeviceType { get => _type; set => SetProperty(ref _type, value); }

        /// <summary>
        /// 设备名称
        /// </summary>
        private string _name;
        [Required]
        public string Name { get => _name; set => SetProperty(ref _name, value); }

        /// <summary>
        /// 设备名称占位字符
        /// </summary>
        private string _namePlaceholder;

        public string NamePlaceholder { get => _namePlaceholder; set => SetProperty(ref _namePlaceholder, value); }

        /// <summary>
        /// 厂商占位字符
        /// </summary>
        private string _brandPlaceholder;
        public string BrandPlaceholder { get => _brandPlaceholder; set => SetProperty(ref _brandPlaceholder, value); }

        private int _channelCount;
        public int ChannelCount
        {
            get => _channelCount;
            set => SetProperty(ref _channelCount, value);
        }

        /// <summary>
        /// 对话框打开
        /// </summary>
        /// <param name="parameters"></param>
        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);

            if (parameters.TryGetValue<DeviceModel>("DeviceModel", out var dModel) && dModel != null)
            {
                Name = dModel.Name;
                DeviceType = dModel.DeviceType;
                ID = dModel.ID;
                SetDeviceModel(dModel);
            }
        }

        /// <summary>
        /// 窗口关闭
        /// </summary>
        /// <param name="parameter"></param>
        protected override void Ok(IDialogResult result)
        {
            base.Ok(result);
            if (IsTcp && string.IsNullOrEmpty(IpAddress))
            {
                throw new DeviceException("请输入IP地址!");
            }
            if (IsCom && string.IsNullOrEmpty(ComName))
            {
                throw new DeviceException("请选择Com口!");
            }
            if (IsPCIe && PCIe < 0)
            {
                throw new DeviceException("请选择PCIe插槽号!");
            }
            if (IsUsb && string.IsNullOrEmpty(Usb))
            {
                throw new DeviceException("请输入Usb端口号!");
            }
            result.Parameters.Add(BaseDeviceVM.DeviceParamKey, BuildDeviceModel());
        }

        /// <summary>
        /// 构建
        /// </summary>
        /// <returns></returns>
        protected virtual DeviceModel BuildDeviceModel()
        {
            var model = new DeviceModel(Name, DeviceType)
            {
                ID = ID,
                AdapterType = Adapter
            };
            if (Adapter == typeof(Network))
            {
                model.Adapter = new Network
                {
                    Ip = IpAddress,
                    Port = Port
                };
            }
            else if (Adapter == typeof(PCIe))
            {
                model.Adapter = new PCIe
                {
                    Slot = PCIe
                };
            }
            else if (Adapter == typeof(Com))
            {
                model.Adapter = new Com
                {
                    Name = _comName, BaudRate = BaudRate,StopBits = StopBits, Databits = DataBits, Parity=Parity
                    
                };
            }
            else
            {
                model.Adapter = new Usb
                {
                    Name = Usb
                };
            }
            model.AdapterAddress = model.Adapter.GetMethod();
            model.ChannelCount = ChannelCount;
            return model;
        }

        /// <summary>
        /// 更新值
        /// </summary>
        /// <param name="deviceModel"></param>
        protected virtual void SetDeviceModel(DeviceModel deviceModel)
        {
            Adapter = deviceModel.AdapterType;
            ChannelCount = deviceModel.ChannelCount;
            // 网口类型
            if (deviceModel.Adapter is Network network)
            {
                IpAddress = network.Ip;
                Port = network.Port;
            }

            // USB类型
            if (deviceModel.Adapter is Usb usb)
            {
                Usb = usb.Name;
            }

            // Com类型
            if (deviceModel.Adapter is Com com)
            {
                ComName = com.Name;
                DataBits = com.Databits;
                BaudRate = com.BaudRate;
                StopBits = com.StopBits;
                Parity = com.Parity;
            }

            // PCIe类型
            if (deviceModel.Adapter is PCIe pcie)
            {
                PCIe = pcie.Slot;
            }
        }

        private void AdapterTypeChanged()
        {
            if (Adapter == typeof(Network))
            {
                IsTcp = true;
                IsCom = false;
                IsPCIe = false;
                IsUsb = false;
            }
            else if (Adapter == typeof(Com))
            {
                IsTcp = false;
                IsCom = true;
                IsPCIe = false;
                IsUsb = false;
            }
            else if (Adapter == typeof(PCIe))
            {
                IsTcp = false;
                IsCom = false;
                IsPCIe = true;
                IsUsb = false;
            }
            else
            {
                IsTcp = false;
                IsCom = false;
                IsPCIe = false;
                IsUsb = true;
            }
        }
    }

    /// <summary>
    /// 控制卡VM
    /// </summary>
    public class MotionCardDialogVM : DeviceDialogVM
    {
        protected MotionCardDialogVM(ISimDeviceEngineUI _engine) : base(_engine, typeof(IMotionCard))
        {
            NamePlaceholder = $"{Langs.LangProvider.GetLang("MotionCard")}{Langs.LangProvider.GetLang("Name")}";
            BrandPlaceholder = $"{Langs.LangProvider.GetLang("MotionCard")}{Langs.LangProvider.GetLang("Brand")}";
        }
    }

    /// <summary>
    /// 相机对象
    /// </summary>
    public class CameraDialogVM : DeviceDialogVM
    {
        public CameraDialogVM(ISimDeviceEngineUI _engine) : base(_engine, typeof(ICamera))
        {
            NamePlaceholder = $"{Langs.LangProvider.GetLang("Camera")}{Langs.LangProvider.GetLang("Name")}";
            BrandPlaceholder = $"{Langs.LangProvider.GetLang("Camera")}{Langs.LangProvider.GetLang("Brand")}";
        }

        public CameraDialogVM(ISimDeviceEngineUI _engine, Type type) : base(_engine, type)
        {

        }
    }

    /// <summary>
    /// 线激光
    /// </summary>
    public class LineLaserDialogVM : DeviceDialogVM
    {
        public LineLaserDialogVM(ISimDeviceEngineUI _engine) : base(_engine, typeof(ILineLaser))
        {
           
            NamePlaceholder = $"{Langs.LangProvider.GetLang("LineLaser")}{Langs.LangProvider.GetLang("Name")}";
            BrandPlaceholder = $"{Langs.LangProvider.GetLang("LineLaser")}{Langs.LangProvider.GetLang("Brand")}";
        }
    }

    public class LightDialogVM : DeviceDialogVM
    {
        public override bool IsLightController => true;
        protected LightDialogVM(ISimDeviceEngineUI _engine) : base(_engine, typeof(ILightController))
        {
            NamePlaceholder = $"{Langs.LangProvider.GetLang("LightController")}{Langs.LangProvider.GetLang("Name")}";
            BrandPlaceholder = $"{Langs.LangProvider.GetLang("LightController")}{Langs.LangProvider.GetLang("Brand")}";
            ChannelCount = 1;
        }
    }

    public class PrinterDialogVM : DeviceDialogVM
    {
        public override bool IsLightController => false;
        protected PrinterDialogVM(ISimDeviceEngineUI _engine) : base(_engine, typeof(IPrinter))
        {
            NamePlaceholder = $"{Langs.LangProvider.GetLang("Printer")}{Langs.LangProvider.GetLang("Name")}";
            BrandPlaceholder = $"{Langs.LangProvider.GetLang("Printer")}{Langs.LangProvider.GetLang("Brand")}";
        }
    }

    public class RobotDialogVM : DeviceDialogVM
    {
        public override bool IsLightController => false;
        protected RobotDialogVM(ISimDeviceEngineUI _engine) : base(_engine, typeof(IRobot))
        {
            NamePlaceholder = $"{Langs.LangProvider.GetLang("Robot")}{Langs.LangProvider.GetLang("Name")}";
            BrandPlaceholder = $"{Langs.LangProvider.GetLang("Robot")}{Langs.LangProvider.GetLang("Brand")}";
        }
    }

}