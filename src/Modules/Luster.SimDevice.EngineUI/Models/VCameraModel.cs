using Luster.Common.DataStruct.DataModels;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Real;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.SimDevice.EngineUI.Models
{
    public class VCameraModel : BindableBase
    {
        /// <summary>
        /// 唯一标识符
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        /// <summary>
        /// 相机名称
        /// </summary>
        private string _cameraName;
        public string CameraName
        {
            get => _cameraName;
            set => SetProperty(ref _cameraName, value);
        }

        /// <summary>
        /// 相机序列号
        /// </summary>
        public string _cameraSerialNumber;
        public string CameraSerialNumber
        {
            get => _cameraSerialNumber;
            set => SetProperty(ref _cameraSerialNumber, value);
        }

        /// <summary>
        /// 隶属模组
        /// </summary>
        private string _module;
        public string Module
        {
            get => _module; 
            set { SetProperty(ref _module, value); }
        }

        /// <summary>
        /// 光源控制器名称
        /// </summary>
        private string _lightName;
        public string LightName
        {
            get => _lightName;
            set => SetProperty(ref _lightName, value);
        }

        /// <summary>
        /// 所需光源通道数
        /// </summary>
        private int _channelCount;
        public int ChannelCount
        {
            get => _channelCount;
            set => SetProperty(ref _channelCount, value);
        }

        /// <summary>
        /// 光源熄灭延迟时间
        /// </summary>
        private int _delayTime;

        public int DelayTime
        {
            get => _delayTime;
            set
            {
                SetProperty(ref _delayTime, value);
                DelayTimeChanged();
            }
        }

        private void DelayTimeChanged()
        {
            if (_vCamera != null)
            {
                _vCamera.LightOffDelayTime = DelayTime;
            }
        }

        /// <summary>
        /// 离线图片路径
        /// </summary>
        private string _imagePath;
        public string ImagePath
        {
            get => _imagePath;
            set
            {
                SetProperty(ref _imagePath, value);
                ImagePathChanged();
            }
        }

        private void ImagePathChanged()
        {
            if (_vCamera != null)
            {
                _vCamera.ImagePath = new LPath(ImagePath);
            }
        }

        private VCamera _vCamera;

        public VCameraModel(VCamera vCamera)
        {
            _vCamera = vCamera;
            Module = vCamera.Module;
            ID = vCamera.ID;
            Name = vCamera.Name;
            CameraName = vCamera.CameraName;
            CameraSerialNumber = vCamera.CameraSerialNumber;
            LightName = vCamera.LightControllerName;
            ChannelCount = vCamera.ChannelCount;
            DelayTime = vCamera.LightOffDelayTime;
            ImagePath = vCamera.ImagePath.Path ?? string.Empty;
        }
    }
}
