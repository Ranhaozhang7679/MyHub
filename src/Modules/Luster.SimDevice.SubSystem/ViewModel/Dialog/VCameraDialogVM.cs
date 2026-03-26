using Luster.Common.DataStruct.Enums;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Real;
using Luster.SimDevice.EngineUI;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.SubSystem.ViewModel.Dialog
{
    public class VCameraDialogVM : DialogVM
    {
        /// <summary>
        /// 设备名称
        /// </summary>
        private string _name;
        [Required]
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        /// <summary>
        /// 光源通道数
        /// </summary>
        private int _lightCount;
        [Range(1, 8)]
        public int LightCount
        {
            get => _lightCount;
            set => SetProperty(ref _lightCount, value);
        }

        /// <summary>
        /// 真实相机
        /// </summary>
        private ICamera _mCamera;
        [Required]
        public ICamera MCamera
        {
            get => _mCamera;
            set => SetProperty(ref _mCamera, value);
        }

        /// <summary>
        /// 隶属模组
        /// </summary>
        private string _module;
        [Required]
        public string Module
        {
            get => _module;
            set
            {
                SetProperty(ref _module, value);
            }
        }

        /// <summary>
        /// 真实控制器
        /// </summary>
        private ILightController _mLightController;
        [Required]
        public ILightController MLightController
        {
            get => _mLightController;
            set => SetProperty(ref _mLightController, value);
        }

        /// <summary>
        /// 真实相机列表
        /// </summary>
        public List<ICamera> Cameras { get; set; }

        /// <summary>
        /// 真实控制器列表
        /// </summary>
        public List<ILightController> LightControllers { get; set; }


        protected VCameraDialogVM(ISimDeviceEngineUI _engine) : base(_engine)
        {
            Cameras = _engine.Engine.GetRealDevices(typeof(ICamera)).Select(u => u as ICamera).ToList();
            LightControllers = _engine.Engine.GetRealDevices(typeof(ILightController)).Select(u => u as ILightController).ToList();
            LightCount = 1;
        }

        protected override void Ok(IDialogResult result)
        {
            base.Ok(result);
            //构建虚拟设备对象
            var vCamera = new VCamera()
            {
                ID = Guid.NewGuid(),
                Name = Name,
                Module = Module,
                DeviceID = (MCamera as IDevice).ID,
                CameraName = (MCamera as IDevice).Name,
                CameraSerialNumber = MCamera.SerialNumber,
                LightControllerName = (MLightController as IDevice).Name,
                LightController = MLightController as IDevice,
                ChannelCount = LightCount,
                ChannelInfos = new Dictionary<int, int>(),
                ImagePath = new Common.DataStruct.DataModels.LPath(),
            };

            result.Parameters.Add("VCamera", vCamera);
        }
    }
}
