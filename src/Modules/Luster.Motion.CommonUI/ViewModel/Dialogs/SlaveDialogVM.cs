using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Extensions;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.VDevice;
using Luster.Motion.DataStruct.Virtual;
using Luster.Motion.TaskFlow.Engine;
using Luster.TaskFlow.Common.Attributes;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.CommonUI.ViewModel.Dialogs
{
    public sealed class SlaveDialogVM : MotionDialogVM
    {
        private IDeviceEngine _deviceEngine;

        public SlaveDialogVM(IDeviceEngine deviceEngine)
        {
            _deviceEngine = deviceEngine;
        }

        /// <summary>
        /// 当前Socket
        /// </summary>
        private VCommuncation currentDevice;

        /// <summary>
        /// 当前类型
        /// </summary>
        private Type _currentType;
        public Type CurrentType
        {
            get { return _currentType; }
            set { SetProperty(ref _currentType, value); }
        }

        /// <summary>
        /// 设备列表 IVirtualDevice
        /// </summary>
        private List<IVirtualDevice> _deviceList;
        public List<IVirtualDevice> DeviceList
        {
            get { return _deviceList; }
            set { SetProperty(ref _deviceList, value); }
        }

        /// <summary>
        /// 从站列表 SocketAction
        /// </summary>
        private ObservableCollection<SocketAction> _slaveList;
        public ObservableCollection<SocketAction> SlaveList
        {
            get { return _slaveList; }
            set { SetProperty(ref _slaveList, value); }
        }

        /// <summary>
        /// 是否可以选
        /// </summary>
        private bool _canSelected;
        public bool CanSelected
        {
            get { return _canSelected; }
            set { SetProperty(ref _canSelected, value); }
        }

        /// <summary>
        /// 查询关键字
        /// </summary>
        private string _searchKey;
        public string SearchKey
        {
            get { return _searchKey; }
            set { SetProperty(ref _searchKey, value); }
        }

        /// <summary>
        /// 选择对象
        /// </summary>
        private DelegateCommand<SocketAction> _selectCommand;
        public DelegateCommand<SocketAction> SelectCommand => _selectCommand ?? (_selectCommand = new DelegateCommand<SocketAction>(slave =>
        {
            IDialogResult r = new DialogResult(ButtonResult.OK);
            r.Parameters.Add("Slave", slave);
            r.Parameters.Add("SearchKey", SearchKey);
            RaiseRequestClose(r);
        }));

        /// <summary>
        /// 选中设备
        /// </summary>
        private DelegateCommand<object> _selectedDeviceCommand;
        public DelegateCommand<object> SelectedDeviceCommand => _selectedDeviceCommand ?? (_selectedDeviceCommand = new DelegateCommand<object>((obj) =>
        {
            currentDevice = obj as VCommuncation;
            if (currentDevice == null)
            {
                return;
            }

            SlaveList = new ObservableCollection<SocketAction>();
            foreach (var item in currentDevice.Actions)
            {
                SlaveList.Add(item);
            }
            
        }));

        /// <summary>
        /// 查找对象
        /// </summary>
        private DelegateCommand _searchCommand;
        public DelegateCommand SearchCommand => _searchCommand ?? (_searchCommand = new DelegateCommand(() =>
        {
            if (CurrentType != null)
            {
                //FilterItems(CurrentType);
            }
        }));


        /// <summary>
        /// 对话框打开
        /// </summary>
        /// <param name="parameters">参数列表</param>
        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);
            List<IVirtualDevice> devices = _deviceEngine.GetDevices();
            List<IVirtualDevice> newDevices= new List<IVirtualDevice>();
            for (int i = devices.Count - 1; i >= 0; i--)
            {
                if (devices[i] is VCommuncation vComm)
                {
                    if (vComm.Actions != null && vComm.Actions.Count != 0  && 
                        (vComm.ProtocolType==ProtocolType.ModbusRTU || vComm.ProtocolType == ProtocolType.StringDefault))
                    {
                        newDevices.Add(devices[i]);
                    }
                }
            }
            DeviceList = newDevices;
        }
    }
}
