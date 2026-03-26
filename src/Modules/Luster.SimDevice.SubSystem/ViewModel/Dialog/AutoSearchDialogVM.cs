
using Luster.Common.DataStruct.DataModels;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.Real;
using Luster.SimDevice.EngineUI;
using Luster.SimDevice.EngineUI.Models;
using Luster.SimDevice.Real;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Luster.SimDevice.SubSystem.ViewModel.Dialog
{
    public class AutoSearchDialogVM : DialogVM
    {
        /// <summary>
        /// 自动添加设备参数关键字
        /// </summary>
        public static string DeviceParamKey = "DiveceInfo";

        private List<IDevice> _devices;

        /// <summary>
        /// 设备列表
        /// </summary>
        public List<IDevice> Devices
        {
            get => _devices ?? (_devices = new List<IDevice>());
            set => SetProperty(ref _devices, value);
        }

        private IDevice _selectDevice;

        public IDevice SelectDevice
        {
            get { return _selectDevice; }
            set { SetProperty(ref _selectDevice, value); }
        }

        private List<IDevice> _existDevices;
        /// <summary>
        /// 已存在设备列表
        /// </summary>
        public List<IDevice> ExistDevices
        {
            get => _existDevices;
            set => SetProperty(ref _existDevices, value);
        }


        /// <summary>
        /// 厂商列表
        /// </summary>
        public List<KeyValue> BrandList { get; set; }

        /// <summary>
        /// 设备类型
        /// </summary>
        private Type _type;
        public Type DeviceType { get => _type; set => SetProperty(ref _type, value); }

        /// <summary>
        /// 搜索命令
        /// </summary>
        public DelegateCommand SearchCommad { get; set; }

        public AutoSearchDialogVM(ISimDeviceEngineUI _engine, Type type) : base(_engine)
        {
            // 厂商
            BrandList = deviceEngine.GetBrands(type);
            SearchCommad = new DelegateCommand(Search);
        }


        /// <summary>
        /// 查找设备列表
        /// </summary>
        protected virtual void Search()
        {

        }
    }

    public class SearchCameraDialogVM : AutoSearchDialogVM
    {
        public SearchCameraDialogVM(ISimDeviceEngineUI _engine) : base(_engine, typeof(ICamera))
        {
            ExistDevices = deviceEngine.GetRealDevices(typeof(ICamera));
        }

        /// <summary>
        /// 查找设备列表
        /// </summary>
        protected override void Search()
        {
            if (DeviceType == null)
            {
                throw new Exception("没有选择设备厂商！");
            }
               
            var camera = Activator.CreateInstance(DeviceType) as ICamera;
            var cameraList = new List<IDevice>();
            camera.CameraListRead(out cameraList);

            if (cameraList.Count > 0)
            {
                Devices = cameraList;
            }
            else 
            {
                throw new DeviceException("没有发现相机！");
            }
            //读取所有相机参数
            foreach (var item in cameraList)
            {
                var camera1 = item as ICamera;
                var errMessage = string.Empty;
                if (camera1.CameraOpen(out errMessage))
                {
                    float frameRate, exposureTime, gain, gamma;

                    camera1.CameraParaRead(out frameRate, out exposureTime, out gain, out gamma);

                    camera1.CloseCamera();
                }
            }
        }

        protected override void Ok(IDialogResult result)
        {          
            base.Ok(result);
            result.Parameters.Add(DeviceParamKey, Devices);
        }

        /// <summary>
        /// 搜索命名
        /// </summary>
        private DelegateCommand<object> _editCommand;
        public DelegateCommand<object> EditCommand => _editCommand ?? (_editCommand = new DelegateCommand<object>((obj) =>
        {

            var editArgs = obj as DataGridCellEditEndingEventArgs;
            if (editArgs != null)
            {
                var row = editArgs.Row;
                var device = row.DataContext as IDevice;
                object curVal = null;
                if (editArgs.EditingElement is TextBox txt)
                {
                    return;
                }
                else if (editArgs.EditingElement is ComboBox comb)
                {
                    curVal = comb.SelectedItem;
                }

                if (curVal != null) 
                {
                    var existCamera = curVal as IDevice;
                    if (existCamera != null) 
                    {
                        device.ID = existCamera.ID;
                        device.Name = existCamera.Name;
                    }
                }
            }
        }));

    }

    public class SearchLineLaserDialogVM : AutoSearchDialogVM
    {
        public SearchLineLaserDialogVM(ISimDeviceEngineUI _engine) : base(_engine, typeof(ILineLaser))
        {
            ExistDevices = deviceEngine.GetRealDevices(typeof(ILineLaser));
        }

        /// <summary>
        /// 查找设备列表
        /// </summary>
        protected override void Search()
        {
            if (DeviceType == null)
            {
                throw new Exception("没有选择设备厂商！");
            }

            var lineLaser = Activator.CreateInstance(DeviceType) as ILineLaser;
           
            var laserList = new List<IDevice>();

            if (lineLaser.GetType().Name == "Focal12Spectrum")
            {
                lineLaser.LaserListRead(out laserList);
            }
            else
            {
                throw new DeviceException("改类型相机不需匹配！");
            }

            if (laserList.Count > 0)
            {
                Devices = laserList;
            }
            else
            {
                throw new DeviceException("没有发现线扫相机！");
            }
        }

        protected override void Ok(IDialogResult result)
        {
            base.Ok(result);
            result.Parameters.Add(DeviceParamKey, Devices);
        }

        /// <summary>
        /// 搜索命名
        /// </summary>
        private DelegateCommand<object> _editCommand;
        public DelegateCommand<object> EditCommand => _editCommand ?? (_editCommand = new DelegateCommand<object>((obj) =>
        {

            var editArgs = obj as DataGridCellEditEndingEventArgs;
            if (editArgs != null)
            {
                var row = editArgs.Row;
                var device = row.DataContext as IDevice;
                object curVal = null;
                if (editArgs.EditingElement is TextBox txt)
                {
                    return;
                }
                else if (editArgs.EditingElement is ComboBox comb)
                {
                    curVal = comb.SelectedItem;
                }

                if (curVal != null)
                {
                    var existCamera = curVal as IDevice;
                    if (existCamera != null)
                    {
                        device.ID = existCamera.ID;
                        device.Name = existCamera.Name;
                    }
                }
            }
        }));

    }
}
