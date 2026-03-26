#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       VPlcContentVM
* 机器名称:       Z05592
* 命名空间:       Luster.SimDevice.SubSystem.ViewModel.Virtual
* 文 件 名:       VPlcContentVM.cs
* 创建时间:       2022/11/18 15:35:39
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      86f29d8e-e20e-448a-8bd8-89d333522868
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/11/18 15:35:39
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using Luster.Common.Assets;
using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Enums;
using Luster.Common.DataStruct.Extensions;
using Luster.Common.Tools;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.DataStruct.Network;
using Luster.Motion.DataStruct.VDevice;
using Luster.SimDevice.EngineUI;
using Luster.SimDevice.SubSystem.Extension;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Luster.SimDevice.SubSystem.ViewModel.Virtual
{
    public class VPlcContentVM : PageVM
    {
        /// <summary>
        /// 数据类型
        /// </summary>
        public List<KeyValue> DataTypes { get; set; }

        /// <summary>
        /// PLC读取类型
        /// </summary>
        public List<KeyValue> PlcAddrTypes { get; set; }

        /// <summary>
        /// 协议对象
        /// </summary>
        public List<KeyValue> Protocols { get; set; }

        /// <summary>
        /// 协议对象
        /// </summary>
        public List<KeyValue> EndianTypes { get; set; }

        /// <summary>
        /// PLC设备
        /// </summary>
        private ObservableCollection<PlcModel> _vPlcDevices;
        public ObservableCollection<PlcModel> VPlcDevices
        {
            get => _vPlcDevices;
            set => SetProperty(ref _vPlcDevices, value);
        }

        /// <summary>
        /// 单个PLC设备地址列表
        /// </summary>
        private ObservableCollection<AddressItemVM> _addressItems;
        public ObservableCollection<AddressItemVM> AddressItems
        {
            get => _addressItems;
            set => SetProperty(ref _addressItems, value);
        }

        #region PLc设备相关命令
        public DelegateCommand<PlcModel> EditCommand { get; set; }

        public DelegateCommand<PlcModel> RemoveCommand { get; set; }

        public DelegateCommand<PlcModel> SelectCommand { get; set; }

        public DelegateCommand<string> SearchCommand { get; set; }

        public DelegateCommand<PlcModel> ReadAllCommand { get; set; }
        #endregion

        #region 地址相关命令
        /// <summary>
        /// 移除地址配置
        /// </summary>
        public DelegateCommand<AddressItemVM> RemoveAddressCommand { get; set; }

        /// <summary>
        /// 批量导入
        /// </summary>
        public DelegateCommand<PlcModel> BatchImportCommand { get; set; }

        /// <summary>
        /// 批量导出
        /// </summary>
        public DelegateCommand<PlcModel> BatchExportCommand { get; set; }

        /// <summary>
        /// 添加地址
        /// </summary>
        public DelegateCommand<PlcModel> AddAddressCommand { get; set; }
        /// <summary>
        /// 连接PLC设备
        /// </summary>
        private DelegateCommand<PlcModel> _conectCommand;
        public DelegateCommand<PlcModel> ConnectCommand
        {
            get => _conectCommand;
            set => SetProperty(ref _conectCommand, value);
        }

        #endregion

        public DelegateCommand<PlcModel> EditCommuncationCommand { get; set; }


        /// <summary>
        /// 显示添加按钮
        /// </summary>
        public override bool IsShowAdd => true;

        private PlcModel _currentPlc;
        public PlcModel CurrentPlc
        {
            get => _currentPlc;
            set => SetProperty(ref _currentPlc, value);
        }

        protected VPlcContentVM(ISimDeviceEngineUI _engine) : base(_engine)
        {
            DataTypes = typeof(DataType).EnumToDataSource();
            PlcAddrTypes = typeof(PlcAddrType).EnumToDataSource();
            Protocols = typeof(ProtocolType).EnumToDataSource();
            EndianTypes = typeof(EndianType).EnumToDataSource();
            RemoveCommand = new DelegateCommand<PlcModel>(Remove);
            SelectCommand = new DelegateCommand<PlcModel>(Select);
            SearchCommand = new DelegateCommand<string>(Search);
            AddAddressCommand = new DelegateCommand<PlcModel>(AddAddress);
            RemoveAddressCommand = new DelegateCommand<AddressItemVM>(RemoveAddress);
            ReadAllCommand = new DelegateCommand<PlcModel>(ReadAll);
            ConnectCommand = new DelegateCommand<PlcModel>(Connect);
            BatchImportCommand = new DelegateCommand<PlcModel>(BatchImport);
            BatchExportCommand = new DelegateCommand<PlcModel>(BatchExport);
            EditCommuncationCommand = new DelegateCommand<PlcModel>(EditCommuncation);
            LoadDevices();
        }

        private void EditCommuncation(PlcModel vPlc)
        {
            dialogService.ShowCommuncationDialog(vPlc.PlcComm, r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    if (r.Parameters.TryGetValue<ICommunication>("Model", out var comm))
                    {
                        vPlc.PlcComm = comm;
                    }
                }
            });
        }

        private void Connect(PlcModel plcM)
        {
            plcM?.Tag?.Close();
            Thread.Sleep(50);
            plcM.Tag.Open();
            plcM.IsConnect = true;
            if (plcM.IsConnect)
            {
                RealRefresh(plcM.Tag);
            }
        }
        /// <summary>
        /// 导出PLC地址配置
        /// </summary>
        private void BatchExport(PlcModel model)
        {
            var saveFile = new SaveFileDialog();
            saveFile.Filter = "XLS|*.xls";
            var result = saveFile.ShowDialog().Value;
            if (result)
            {
                var filename = saveFile.FileName;
                var excel = new ExcelTool(filename);
                var prop = typeof(PlcAddr).GetProperties().Where(x => x.GetCustomAttribute<IgnoreAttribute>() == null).ToList();
                var header = prop.Select(x => x.Name).ToArray();
                var data = new object[header.Length];
                excel.SetHeaders(0, 0, header);
                for (int i = 0; i < model.Tag.Address.Count(); i++)
                {
                    for (int j = 0; j < header.Length; j++)
                    {
                        data[j] = prop.Where(x => x.Name == header[j]).FirstOrDefault()?.GetValue(model.Tag.Address[i]);
                    }
                    excel.WriteRowDatas(i + 1, 0, data);
                }
                excel.Save(filename);
            }
        }

        /// <summary>
        /// 导入PLC地址配置
        /// </summary>
        private void BatchImport(PlcModel model)
        {
            if (model == null) return;
            if (model.Tag.Address != null && model.Tag.Address.Count > 0)
            {
                dialogService.ShowConfirm("确认要导入PLC地址，一旦导入会清空已有数据!", r =>
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        dialogService.ShowImportPlcAddress(model.Tag, r =>
                        {
                            var plcModel = new PlcModel(model.Tag);
                            LoadPlcAddress(plcModel);
                        });
                    }
                });
            }
            else
            {
                dialogService.ShowImportPlcAddress(model.Tag, r =>
                {
                    var plcModel = new PlcModel(model.Tag);
                    LoadPlcAddress(plcModel);
                });
            }
        }

        private void RemoveAddress(AddressItemVM obj)
        {
            if (obj != null)
            {
                dialogService.ShowConfirm($"确认删除Plc地址:{obj.Address}", r =>
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        AddressItems.Remove(obj);
                        var address = obj.Owner.Address.FirstOrDefault(x => x.Address == obj.Address);
                        if (address != null)
                            obj.Owner.Address.Remove(address);
                    }
                });
            }
        }

        private void AddAddress(PlcModel vPlc)
        {
            dialogService.ShowPlcAddressDialog(vPlc.Tag, r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    LoadPlcAddress(vPlc);
                }
            });
        }

        private void Search(string key)
        {
            string trimKey = key.Trim();

            LoadDevices(trimKey);
        }

        private void Select(PlcModel plc)
        {
            if (plc != null)
            {
                CurrentPlc = plc;
                LoadPlcAddress(plc);
            }
        }

        private void Remove(PlcModel plc)
        {
            if (plc != null)
            {
                dialogService.ShowConfirm($"确认删除PLC设备:{plc.Name}", r =>
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        var device = deviceEngine.GetVirtualByName(plc.Name);
                        deviceEngine.ReomoveVirtual(device.ID);
                        VPlcDevices.Remove(plc);
                        plc.Tag.UpdateAddr();
                    }
                    if (CurrentPlc.Name == plc.Name)
                    {
                        CurrentPlc = VPlcDevices.FirstOrDefault();
                        if (CurrentPlc != null)
                            LoadPlcAddress(CurrentPlc);
                    }
                });
            }
        }

        /// <summary>
        /// 添加PLC设备
        /// </summary>
        public override void AddNewItem()
        {
            dialogService.ShowPlcDialog(null, r =>
            {
                if (r.Result == ButtonResult.OK)
                    LoadDevices();
            });
        }

        /// <summary>
        /// 加载设备信息
        /// </summary>
        private void LoadDevices(string key = "")
        {
            VPlcDevices = new ObservableCollection<PlcModel>();
            var vPlcs = deviceEngine.GetVDevices<VPlc>();

            foreach (var item in vPlcs)
            {
                var plcModel = new PlcModel(item);
                VPlcDevices.Add(plcModel);
            }
            if (CurrentPlc == null)
            {
                CurrentPlc = VPlcDevices.FirstOrDefault();
                if (CurrentPlc != null)
                    LoadPlcAddress(CurrentPlc);
            }
        }


        private void LoadPlcAddress(PlcModel plc)
        {
            AddressItems = new ObservableCollection<AddressItemVM>();
            foreach (var address in plc.Tag.Address)
            {
                AddressItems.Add(new AddressItemVM(address) { Owner = plc.Tag });
            }

            plc.Tag.UpdateAddr();

            ReadAll(plc);
        }

        /// <summary>
        /// 一次性读取多次值
        /// </summary>
        /// <param name="plc"></param>
        private void ReadAll(PlcModel plc)
        {
            if (plc.IsConnect)
            {
                // 监控过程中不需要主动读取，因为软件在持续刷新
                if (!plc.Tag.IsMonitor)
                {
                    plc.Tag.ReadBatch();
                }

                RealRefresh(plc.Tag);
            }
        }

        /// <summary>
        /// 真实值刷新
        /// </summary>
        private CancellationTokenSource token;
        private void RealRefresh(VPlc plc)
        {
            if (plc == null || !plc.PlcComm.IsConnected) return;
            token?.Cancel();
            Thread.Sleep(400);
            token = new CancellationTokenSource();
            Task.Run(() =>
            {
                while (true)
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    foreach (var item in AddressItems)
                    {
                        item.RefreashVal();
                    }

                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    Thread.Sleep(200);
                }

            }, token.Token);
        }

        /// <summary>
        /// 切换到当前页面
        /// </summary>
        /// <param name="navigationContext"></param>
        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            base.OnNavigatedFrom(navigationContext);
            token?.Cancel();
        }

        public override void Leave()
        {
            base.Leave();
            token?.Cancel();
        }
    }

    public class PlcModel : BindableBase
    {
        /// <summary>
        /// 当前的协议
        /// </summary>
        private string _module;
        public string Module
        {
            get => _module;
            set
            {
                SetProperty(ref _module, value);
                if (Tag != null)
                {
                    Tag.Module = value;
                }
            }
        }

        /// <summary>
        /// 当前的协议
        /// </summary>
        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                SetProperty(ref _name, value);
                if (Tag != null)
                {
                    Tag.Name = value;
                }
            }
        }

        /// <summary>
        /// 当前的协议
        /// </summary>
        private ICommunication _plcComm;
        public ICommunication PlcComm
        {
            get => _plcComm;
            set
            {
                SetProperty(ref _plcComm, value);
                if (Tag != null)
                {
                    Tag.PlcComm = value;
                }
            }
        }

        /// <summary>
        /// 当前的协议
        /// </summary>
        private ProtocolType _protocol;
        public ProtocolType ProtocolType
        {
            get => _protocol;
            set
            {
                SetProperty(ref _protocol, value);
                if (Tag != null)
                {
                    Tag.SetProtocol(value);
                    Tag.ProtocolType = value;
                }
            }
        }

        /// <summary>
        /// 字节顺序
        /// </summary>
        private EndianType _endianType;
        public EndianType EndianType
        {
            get => _endianType;
            set
            {
                SetProperty(ref _endianType, value);
                if (Tag != null && Tag.Protocol is IModbus m)
                {
                    m.EndianType = value;
                }
            }
        }


        /// <summary>
        /// 刷新频率
        /// </summary>
        private int _frequency = 50;
        public int Frequency
        {
            get => _frequency;
            set
            {
                SetProperty(ref _frequency, value);

                if (Tag != null)
                {
                    Tag.Frequency = value;
                }
            }
        }

        private bool _isConnect = false;
        public bool IsConnect
        {
            get => _isConnect;
            set
            {
                SetProperty(ref _isConnect, value);
            }
        }

        private bool _isMonitor = false;
        public bool IsMonitor
        {
            get => _isMonitor;
            set
            {
                SetProperty(ref _isMonitor, value);
                if (Tag != null)
                {
                    Tag.IsMonitor = value;
                }
            }
        }

        public VPlc Tag { get; set; }

        public PlcModel(VPlc plc)
        {
            Tag = plc;
            Module = plc.Module;
            Name = plc.Name;
            Frequency = plc.Frequency;
            PlcComm = plc.PlcComm;
            ProtocolType = plc.ProtocolType;
            IsMonitor = plc.IsMonitor;
            IsConnect = plc.PlcComm.IsConnected;
            if (plc.Protocol is IModbus m)
            {
                EndianType = m.EndianType;
            }
        }

        /// <summary>
        /// 选择当前
        /// </summary>
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                SetProperty(ref _isSelected, value);
            }
        }
    }

    /// <summary>
    /// PLC地址信息
    /// </summary>
    public class AddressItemVM : BindableBase
    {
        /// <summary>
        /// 地址
        /// </summary>
        private string _address;
        public string Address
        {
            get => _address;
            set
            {
                SetProperty(ref _address, value);
                if (Tag != null && Tag.Address != value)
                {
                    Tag.Address = value;
                }
            }
        }

        /// <summary>
        /// 数据类型
        /// </summary>
        private DataType _dataType;
        public DataType DataType
        {
            get => _dataType;
            set
            {
                SetProperty(ref _dataType, value);
                if (Tag != null && Tag.DataType != value)
                {
                    Tag.DataType = value;
                }
            }
        }

        /// <summary>
        /// PLC地址读取类型
        /// </summary>
        private PlcAddrType _addrType;
        public PlcAddrType AddrType
        {
            get => _addrType;
            set
            {
                SetProperty(ref _addrType, value);
                if (Tag != null && Tag.AddrType != value)
                {
                    Tag.AddrType = value;
                }
            }
        }

        /// <summary>
        /// 值
        /// </summary>
        private double _value;
        public double Value
        {
            get => _value;
            set
            {
                SetProperty(ref _value, value);
                if (Tag != null && Tag.Value != value)
                {
                    Tag.Value = value;
                }
            }
        }

        /// <summary>
        /// 描述
        /// </summary>
        private string _desc;
        public string Desc
        {
            get => _desc;
            set
            {
                SetProperty(ref _desc, value);
                if (Tag != null && Tag.Desc != value)
                {
                    Tag.Desc = value;
                }
            }
        }

        /// <summary>
        /// 附加设备
        /// </summary>
        public PlcAddr Tag { get; set; }

        /// <summary>
        /// 隶属Owner
        /// </summary>
        public VPlc Owner { get; set; }

        public AddressItemVM(PlcAddr plcAddr)
        {
            Tag = plcAddr;
            Address = Tag.Address;
            DataType = Tag.DataType;
            AddrType = Tag.AddrType;
            Value = Tag.Value;
            Desc = Tag.Desc;
        }

        public void RefreashVal()
        {
            Value = Tag.Value;
        }
    }
}
