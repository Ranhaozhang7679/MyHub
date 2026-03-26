using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.DigitalSetup.ViewModel.Validations.Configs
{
    /// <summary>
    /// LoadCell校准配置 ViewModel
    /// </summary>
    public class LoadCellCalibrationConfigVM : BindableBase
    {
        private string _loadCellType = "Standard";
        /// <summary>
        /// LoadCell类型
        /// </summary>
        public string LoadCellType
        {
            get => _loadCellType;
            set => SetProperty(ref _loadCellType, value);
        }

        private double _calibrationValue = 100;
        /// <summary>
        /// 校准值 (N)
        /// </summary>
        public double CalibrationValue
        {
            get => _calibrationValue;
            set => SetProperty(ref _calibrationValue, value);
        }

        private double _tolerance = 5;
        /// <summary>
        /// 容差范围 (%)
        /// </summary>
        public double Tolerance
        {
            get => _tolerance;
            set => SetProperty(ref _tolerance, value);
        }

        /// <summary>
        /// 配置变化事件
        /// </summary>
        public event EventHandler ConfigChanged;

        public LoadCellCalibrationConfigVM()
        {
            // 监听属性变化
            this.PropertyChanged += (s, e) => ConfigChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
