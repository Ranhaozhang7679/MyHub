#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       HerarMonitorVM
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.CommonUI.ViewModel
* 文 件 名:       HerarMonitorVM.cs
* 创建时间:       2022/12/22 10:13:59
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      6d75cec9-e80d-4d9d-9484-b7ee2a0e8288
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/12/22 10:13:59
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using Luster.Common.Tools;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Dock;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.Interfaces;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;

namespace Luster.Motion.SubSystem.ViewModel
{
    public class HeartItem : BindableBase
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
        /// 连接状态
        /// </summary>
        private bool isConnected;
        public bool IsConnected
        {
            get { return isConnected; }
            set { SetProperty(ref isConnected, value); }
        }

        public IHeartbeat Tag { get; set; }

        /// <summary>
        /// 连接字符串
        /// </summary>
        public string ConnectStr { get; set; }
    }


    public class HeartMonitorContentVM : MotionPageVM, IDockContent
    {
        /// <summary>
        ///  心跳监控设备列列表
        /// </summary>
        private ObservableCollection<HeartItem> _monitorItems;
        public ObservableCollection<HeartItem> MonitorItems
        {
            get { return _monitorItems; }
            set { SetProperty(ref _monitorItems, value); }
        }

        // 需要监控的设备
        private List<IHeartbeat> _heartDevice;

        /// <summary>
        /// 主线程渲染
        /// </summary>
        private Dispatcher _dispatcher;

        /// <summary>
        /// 对应的Key
        /// </summary>
        public string Name => "DeviceMonitor";

        /// <summary>
        /// 对应的区域
        /// </summary>
        public string RegionName { get; set; } = "HeartMonitorContent";

        /// <summary>
        /// 循环
        /// </summary>
        WhileTool whileTool = null;
        public HeartMonitorContentVM() { }

        public HeartMonitorContentVM(ICommonBus _commonBus, IDeviceEngine deviceEngine, Dispatcher dispatcher) : base(_commonBus)
        {
            _dispatcher = dispatcher;
            _heartDevice = deviceEngine.GetVDevices<IHeartbeat>();
            MonitorItems = new ObservableCollection<HeartItem>();
            foreach (var device in _heartDevice)
            {
                if (!MonitorItems.Any(u => u.ConnectStr == device.ConnectStr))
                {
                    MonitorItems.Add(new HeartItem()
                    {
                        Name = device.Name,
                        IsConnected = false,
                        Tag = device,
                        ConnectStr = device.ConnectStr,
                    });
                }
            }

            // 启动while 循环检查
            whileTool = new WhileTool();

            StartMonitorDeviceHealth();
        }

        private void StartMonitorDeviceHealth()
        {
            Task.Run(() =>
            {
                whileTool.While(() =>
                {
                    foreach (var device in MonitorItems)
                    {
                        bool isConn = device.Tag.IsHealth(out string errMsg);

                        if (isConn != device.IsConnected)
                        {
                            _dispatcher.Invoke(() =>
                            {
                                device.IsConnected = isConn;
                            });
                        }
                    }
                }, 2000);
            });
        }

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            base.OnNavigatedFrom(navigationContext);
            whileTool.Stop();
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
            StartMonitorDeviceHealth();
        }
    }
}
