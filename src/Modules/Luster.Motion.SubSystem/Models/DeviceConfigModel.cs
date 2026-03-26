using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.EditorUI.Models;
using Luster.Motion.TaskFlow.Engine.Models;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Luster.Motion.SubSystem.Models
{
    public class DeviceConfigModel : BindableBase
    {
        /// <summary>
        /// 名称
        /// </summary>
        private string _name = "";
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        /// <summary>
        /// 设备
        /// </summary>
        private string _selectDevice = "";
        public string SelectDevice
        {
            get { return _selectDevice; }
            set { SetProperty(ref _selectDevice, value); }
        }

        /// <summary>
        /// 颜色
        /// </summary>
        private SolidColorBrush _btnColor;
        public SolidColorBrush BtnColor
        {
            get { return _btnColor; }
            set { SetProperty(ref _btnColor, value); }
        }

        /// <summary>
        /// 名称是否可见
        /// </summary>
        private Visibility _isNameVisible;
        public Visibility IsNameVisible
        {
            get { return _isNameVisible; }
            set { SetProperty(ref _isNameVisible, value); }
        }

        /// <summary>
        /// 设备是否可见
        /// </summary>
        private Visibility _isDeviceVisible;
        public Visibility IsDeviceVisible
        {
            get { return _isDeviceVisible; }
            set { SetProperty(ref _isDeviceVisible, value); }
        }

        public static string SysConfingPath { set; get; }

        /// <summary>
        /// 绿色背景
        /// </summary>
        static SolidColorBrush greenColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4FCB03"));
        /// <summary>
        /// 灰色背景
        /// </summary>
        static SolidColorBrush grayColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d7d8d9"));

        private static ObservableCollection<DeviceConfigModel> _btnCollections;
        public static ObservableCollection<DeviceConfigModel> BtnCollections
        {
            get
            {
                //if (_btnCollections == null)
                //{
                SystemConfig SysConfig = new SystemConfig();
                SysConfig.LoadSysConfig(SysConfingPath);
                _btnCollections = new ObservableCollection<DeviceConfigModel>()
                    {
                          new DeviceConfigModel() { Name = "Start",SelectDevice=(SysConfig.StartButton==null ? "":SysConfig.StartButton.Name)},
                          new DeviceConfigModel() { Name = "Pause",SelectDevice=(SysConfig.PauseButton==null ? "":SysConfig.PauseButton.Name)},
                          new DeviceConfigModel() { Name = "Recoverey",SelectDevice=(SysConfig.RecoveryButton==null ? "":SysConfig.RecoveryButton.Name)},
                          new DeviceConfigModel() { Name = "Stop",SelectDevice=(SysConfig.StopButton==null ? "":SysConfig.StopButton.Name)},
                          new DeviceConfigModel() { Name = "EmergencyStop" ,SelectDevice=(SysConfig.EmergencyButton==null ? "":SysConfig.EmergencyButton.Name)},
                          new DeviceConfigModel() { Name = "Ignore",SelectDevice=(SysConfig.IgnoreButton==null ? "":SysConfig.IgnoreButton.Name)},
                          new DeviceConfigModel() { Name = "Retry" ,SelectDevice=(SysConfig.RetryButton==null ? "":SysConfig.RetryButton.Name)},
                          new DeviceConfigModel() { Name = "ManualSwitch" ,SelectDevice=(SysConfig.ManualSwitchButton==null ? "":SysConfig.ManualSwitchButton.Name)},
                          new DeviceConfigModel() { Name = "SmokeAlarmDevice" ,SelectDevice=(SysConfig.SmokeAlarmDevice==null ? "":SysConfig.SmokeAlarmDevice.Name)},
                    };
                SetVDeviceInfo(_btnCollections);
                //}
                return _btnCollections;
            }
        }

        private static ObservableCollection<DeviceConfigModel> _tricolorLamp;
        public static ObservableCollection<DeviceConfigModel> TricolorLamp
        {
            get
            {
                //if (_tricolorLamp == null)
                //{
                SystemConfig SysConfig = new SystemConfig();
                SysConfig.LoadSysConfig(SysConfingPath);
                _tricolorLamp = new ObservableCollection<DeviceConfigModel>()
                    {
                          new DeviceConfigModel() { Name = "RedLamp",SelectDevice=(SysConfig.LightRed==null ? "":SysConfig.LightRed.Name)},
                          new DeviceConfigModel() { Name = "GreenLamp",SelectDevice=(SysConfig.LightGreen==null ? "":SysConfig.LightGreen.Name)},
                          new DeviceConfigModel() { Name = "YellowLamp",SelectDevice=(SysConfig.LightYellow==null ? "":SysConfig.LightYellow.Name)},
                          new DeviceConfigModel() { Name = "Buzzer",SelectDevice=(SysConfig.Buzzer==null ? "":SysConfig.Buzzer.Name)},
                          new DeviceConfigModel() { Name = "StartLamp",SelectDevice=(SysConfig.LightStart==null ? "":SysConfig.LightStart.Name)},
                          new DeviceConfigModel() { Name = "PauseLamp",SelectDevice=(SysConfig.LightPause==null ? "":SysConfig.LightPause.Name)},
                          new DeviceConfigModel() { Name = "RecoveryLamp",SelectDevice=(SysConfig.LightRecovery==null ? "":SysConfig.LightRecovery.Name)},
                          new DeviceConfigModel() { Name = "StopLamp",SelectDevice=(SysConfig.LightStop==null ? "":SysConfig.LightStop.Name)},
                    };
                SetVDeviceInfo(_tricolorLamp);
                //}
                return _tricolorLamp;
            }
        }


        /// <summary>
        /// 设置按钮
        /// </summary>
        /// <param name="model"></param>
        public void SetSelectBtnDevice(DeviceConfigModel model)
        {
            model.BtnColor = greenColor;
            model.IsNameVisible = Visibility.Collapsed;
            model.IsDeviceVisible = Visibility.Visible;

            foreach (var item in DeviceConfigModel.BtnCollections)
            {
                SetBtnStatus(item, model);
            }
            foreach (var item in DeviceConfigModel.TricolorLamp)
            {
                SetBtnStatus(item, model);
            }
        }

        /// <summary>
        /// 设置状态
        /// </summary>
        /// <param name="item"></param>
        /// <param name="model"></param>
        private void SetBtnStatus(DeviceConfigModel item, DeviceConfigModel model)
        {
            if (item.Name == model.Name)
            {
                item.SelectDevice = model.SelectDevice;
                item.BtnColor = greenColor;
                item.IsNameVisible = Visibility.Collapsed;
                item.IsDeviceVisible = Visibility.Visible;
            }
        }
        /// <summary>
        /// 取消状态
        /// </summary>
        /// <param name="item"></param>
        /// <param name="model"></param>
        private void CancelBtnStatus(DeviceConfigModel item, DeviceConfigModel model)
        {
            if (item.Name == model.Name)
            {
                item.SelectDevice = "";
                item.BtnColor = grayColor;
                item.IsNameVisible = Visibility.Visible;
                item.IsDeviceVisible = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 设置设备信息
        /// </summary>
        /// <param name="listDevice"></param>
        private static void SetVDeviceInfo(ObservableCollection<DeviceConfigModel> listDevice)
        {
            foreach (var item in listDevice)
            {
                if (item.SelectDevice != null & item.SelectDevice != "")
                {
                    item.BtnColor = greenColor;
                    item.IsDeviceVisible |= Visibility.Visible;
                    item.IsNameVisible = Visibility.Collapsed;
                }
                else
                {
                    item.BtnColor = grayColor;
                    item.IsDeviceVisible |= Visibility.Collapsed;
                    item.IsNameVisible = Visibility.Visible;
                }
            }
        }

        /// <summary>
        /// 取消选中按钮
        /// </summary>
        /// <param name="model"></param>
        public void CancelSelectBtnDevice(DeviceConfigModel model)
        {
            foreach (var item in DeviceConfigModel.BtnCollections)
            {
                CancelBtnStatus(item, model);
            }
            foreach (var item in DeviceConfigModel.TricolorLamp)
            {
                CancelBtnStatus(item, model);
            }
        }

    }
}
