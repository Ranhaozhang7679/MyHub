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
    public class RobotConfigModel2 : BindableBase
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

        /// <summary>
        /// 系统配置路径
        /// </summary>
        public static string SysConfigPath { get; set; }

        /// <summary>
        /// 绿色背景
        /// </summary>
        static SolidColorBrush greenColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4FCB03"));
        /// <summary>
        /// 灰色背景
        /// </summary>
        static SolidColorBrush grayColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d7d8d9"));

        /// <summary>
        /// 机器人DO
        /// 机器人动作
        /// </summary>
        private static ObservableCollection<RobotConfigModel2> _robotDOCollections;
        public static ObservableCollection<RobotConfigModel2> RobotDOCollections
        {
            get
            {
                if (_robotDOCollections == null)
                {
                    SystemConfig SysConfig = new SystemConfig();
                    SysConfig.LoadSysConfig(SysConfigPath);
                    _robotDOCollections = new ObservableCollection<RobotConfigModel2>()
                    {
                        new RobotConfigModel2(){Name="Start",SelectDevice=(SysConfig.RobotStart2==null?"":SysConfig.RobotStart2.Name)},
                        new RobotConfigModel2(){Name="Pause",SelectDevice=(SysConfig.RobotPause2==null?"":SysConfig.RobotPause2.Name)},
                        new RobotConfigModel2(){Name="Stop",SelectDevice=(SysConfig.RobotStop2==null?"":SysConfig.RobotStop2.Name)},
                        new RobotConfigModel2(){Name="Reset",SelectDevice=(SysConfig.RobotReset2==null?"":SysConfig.RobotReset2.Name)},
                        new RobotConfigModel2(){Name="ClearFault",SelectDevice=(SysConfig.RobotClearFault2==null?"":SysConfig.RobotClearFault2.Name)}
                    };
                    SetVDeviceInfo(_robotDOCollections);
                }
                return _robotDOCollections;
            }
        }

        /// <summary>
        /// 机器人DI
        /// 机器人状态
        /// </summary>
        private static ObservableCollection<RobotConfigModel2> _robotDICollections;
        public static ObservableCollection<RobotConfigModel2> RobotDICollections
        {
            get
            {
                if (_robotDICollections == null)
                {
                    SystemConfig SysConfig = new SystemConfig();
                    SysConfig.LoadSysConfig(SysConfigPath);
                    _robotDICollections = new ObservableCollection<RobotConfigModel2>()
                    {
                      new RobotConfigModel2(){Name="Runing",SelectDevice=(SysConfig.RobotRuning2==null?"":SysConfig.RobotRuning2.Name)},
                      new RobotConfigModel2(){Name="Paused",SelectDevice=(SysConfig.RobotPaused2==null?"":SysConfig.RobotPaused2.Name)},
                      new RobotConfigModel2(){Name="Idle",SelectDevice=(SysConfig.RobotPause2==null?"":SysConfig.RobotIdle2.Name)},
                      new RobotConfigModel2(){Name="Error",SelectDevice=(SysConfig.RobotError2==null?"":SysConfig.RobotError2.Name)},

                    };
                    SetVDeviceInfo(_robotDICollections);
                }
                return _robotDICollections;
            }

        }
        /// <summary>
        /// 设置按钮
        /// </summary>
        /// <param name="model"></param>
        public void SetSelectBtnDevice(RobotConfigModel2 model)
        {
            //设置输入
            foreach (var item in RobotConfigModel2.RobotDICollections)
            {
                SetBtnStatus(item, model);
            }
            //设置输出
            foreach (var item in RobotConfigModel2.RobotDOCollections)
            {
                SetBtnStatus(item, model);
            }
        }

        /// <summary>
        /// 设置按钮状态
        /// </summary>
        /// <param name="item"></param>
        /// <param name="model"></param>
        public void SetBtnStatus(RobotConfigModel2 item, RobotConfigModel2 model)
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
        private void CancelBtnStatus(RobotConfigModel2 item, RobotConfigModel2 model)
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
        private static void SetVDeviceInfo(ObservableCollection<RobotConfigModel2> listDevice)
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
        public void CancelSelectBtnDevice(RobotConfigModel2 model)
        {
            foreach (var item in RobotConfigModel2.RobotDICollections)
            {
                CancelBtnStatus(item, model);
            }
            foreach (var item in RobotConfigModel2.RobotDOCollections)
            {
                CancelBtnStatus(item, model);
            }
        }

    }
}

