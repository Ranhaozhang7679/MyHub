using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.DigitalSetup.ViewModel.Validations.Configs
{
    /// <summary>
    /// CCD校准配置 ViewModel
    /// </summary>
    public class CCDCalibrationConfigVM : BindableBase
    {
        private string _ccdType = "AreaScan";
        /// <summary>
        /// CCD类型
        /// </summary>
        public string CCDType
        {
            get => _ccdType;
            set => SetProperty(ref _ccdType, value);
        }

        private double _pixelSize = 5.5;
        /// <summary>
        /// 像素尺寸 (μm)
        /// </summary>
        public double PixelSize
        {
            get => _pixelSize;
            set => SetProperty(ref _pixelSize, value);
        }

        private int _xResolution = 2048;
        /// <summary>
        /// X分辨率 (px)
        /// </summary>
        public int XResolution
        {
            get => _xResolution;
            set => SetProperty(ref _xResolution, value);
        }

        private int _yResolution = 1536;
        /// <summary>
        /// Y分辨率 (px)
        /// </summary>
        public int YResolution
        {
            get => _yResolution;
            set => SetProperty(ref _yResolution, value);
        }

        /// <summary>
        /// 配置变化事件
        /// </summary>
        public event EventHandler ConfigChanged;

        public CCDCalibrationConfigVM()
        {
            // 监听属性变化
            this.PropertyChanged += (s, e) => ConfigChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
