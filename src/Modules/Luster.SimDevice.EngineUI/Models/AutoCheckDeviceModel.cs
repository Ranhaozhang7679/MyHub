using Luster.Motion.DataStruct.Interfaces;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Luster.SimDevice.EngineUI.Models
{
    public class AutoCheckDeviceModel : BindableBase
    {
        /// <summary>
        /// 设备名称
        /// </summary>
        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        /// <summary>
        /// 错误信息
        /// </summary>
        private string _errorMessage;

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { SetProperty(ref _errorMessage, value); }
        }

        /// <summary>
        /// 点检时间
        /// </summary>
        private string _checkTime;
        public string CheckTime
        {
            get { return _checkTime; }
            set
            { SetProperty(ref _checkTime, value); }
        }

        /// <summary>
        /// 点检结果
        /// </summary>
        private string _result;
        public string Result
        {
            get { return _result; }
            set { SetProperty(ref _result, value); }
        }

        /// <summary>
        /// 点检结果显示色
        /// </summary>
        private SolidColorBrush _foreColor;

        public SolidColorBrush ForeColor
        {
            get { return _foreColor; }
            set { SetProperty(ref _foreColor, value); }
        }


        public IAutoCheck DeviceTag { get; set; }

        public AutoCheckDeviceModel(IAutoCheck device)
        {
            DeviceTag = device;
            Name = device.Name;
        }

        private int _index;

        /// <summary>
        /// 归零顺序
        /// </summary>
        public int Index
        {
            get => _index;
            set => SetProperty(ref _index, value);
        }
    }
}
