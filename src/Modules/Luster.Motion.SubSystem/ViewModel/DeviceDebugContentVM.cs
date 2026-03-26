#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       FileConfigContentVM
* 机器名称:       L05590
* 命名空间:       Luster.Motion.SubSystem.ViewModel
* 文 件 名:       FileConfigContentVM.cs
* 创建时间:       2023/6/19 15:34:14
* 作    者:       L05590
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       fanlu5590@lusterinc.com 
* 唯一标识：      3447a0c9-b933-404c-a2c7-e7563a365f8f
* 登录用户:       fanlu
* 所 属 域:       LUSTERINC
* 创建年份:       2023
* 修改时间:		  2023/6/19 15:34:14
* 修 改 人:		  L05590
************************************************************************************/
#endregion

using HandyControl.Controls;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.CommonUI.Models;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Virtual;
using Luster.Motion.TaskFlow.Engine;
using Luster.TaskFlow.Motion;
using Luster.TaskFlow.Motion.Logic;
using Luster.WindowsAPICodePack.Dialogs;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.SubSystem.ViewModel
{
    public class DeviceDebugContentVM : MotionPageVM
    {
        private readonly IMotionEngine _mEngine = null;

        private readonly IDeviceEngine _deviceEngine = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commonBus"></param>
        /// <param name="mEngine"></param>
        public DeviceDebugContentVM(ICommonBus commonBus, IMotionEngine mEngine, IDeviceEngine deviceEngine) : base(commonBus)
        {
            _mEngine = mEngine;
            _deviceEngine = deviceEngine;

            // 构建工站
            Init();
        }

        protected override void RegisterEvent(IEventAggregator bus)
        {
            base.RegisterEvent(bus);
            bus.GetEvent<RecipeOpenEvent>().Subscribe(r =>
            {
                Init();
            });
        }

        private void Init()
        {
            // 构建工站
            Stations = _mEngine.GetStations().Where(u => u.TaskFunction is IFreeStation).Select(u => new StationLog(u)).ToList();
        }

        #region 属性
        /// <summary>
        /// 是否保存
        /// </summary>
        private List<StationLog> _stations;
        public List<StationLog> Stations
        {
            get { return _stations; }
            set
            {
                SetProperty(ref _stations, value);
            }
        }

        /// <summary>
        /// 选择的工站
        /// </summary>
        private StationLog _selectStation;
        public StationLog SelectedStation
        {
            get { return _selectStation; }
            set
            {
                SetProperty(ref _selectStation, value);
            }
        }

        /// <summary>
        /// 设备模型
        /// </summary>
        private List<DeviceModel> _deviceTypes;
        public List<DeviceModel> DeviceTypes
        {
            get { return _deviceTypes; }
            set
            {
                SetProperty(ref _deviceTypes, value);
            }
        }

        /// <summary>
        /// 是否保存
        /// </summary>
        private List<DeviceModel> _devices;
        public List<DeviceModel> Devices
        {
            get { return _devices; }
            set
            {
                SetProperty(ref _devices, value);
            }
        }

        #endregion

        /// <summary>
        /// 选择工站
        /// </summary>
        private DelegateCommand<object> _SelectedCommand;
        public DelegateCommand<object> SelectedCommand => _SelectedCommand ?? (_SelectedCommand = new DelegateCommand<object>((obj) =>
        {
            if (obj is StationLog sLog)
            {
                // 解析所有设备
                List<IVirtualDevice> devices = new List<IVirtualDevice>();
                LoadDevicesByStation(sLog.Module, devices);

                // 对硬件设备做类别区分，生成类别信息
                Devices = devices.OrderBy(u => u.Sort).Select(u => new DeviceModel(u)).ToList();

                // 设备类型
                DeviceTypes = Devices.GroupBy(u => u.DeviceType).Select(u => new DeviceModel()
                {
                    DeviceType = u.Key,
                }).ToList();
            }
        }));


        /// <summary>
        /// 选择类型
        /// </summary>
        private DelegateCommand<object> _selectTypeCommand;
        public DelegateCommand<object> SelectTypeCommand => _selectTypeCommand ?? (_selectTypeCommand = new DelegateCommand<object>((obj) =>
        {
            if (obj is DeviceModel s)
            {
                switch (s.DeviceType)
                {
                    case "VIO":
                        UpdateIOs(Devices);
                        break;
                }
            }
        }));

        /// <summary>
        /// 通过工站选择对应的设备
        /// </summary>
        private void LoadDevicesByStation(IMotionModule m, List<IVirtualDevice> vDevices)
        {
            foreach (var item in m.Parameters)
            {
                var p = item.Value;
                if (p.Value == null || p.ParamType == Luster.TaskFlow.Common.Enums.ParamType.OUT) continue;

                // IO 气缸 真空
                if (p.Value is VDevice vDevice)
                {
                    if (vDevice.Virtual == null)
                    {
                        vDevice.Virtual = _deviceEngine.GetVirtualByID(vDevice.DeviceID);
                    }

                    if (vDevice != null)
                    {
                        vDevices.Add(vDevice.Virtual);
                    }
                }
                else if (p.Value is VAxisPos vPos)
                {
                    // 添加轴设备
                    foreach (var vAxis in vPos)
                    {
                        vAxis.UpdatePosition(_deviceEngine);
                        vDevices.Add(vAxis.Axis);
                    }
                }
            }

            foreach (var item in m.Children)
            {
                LoadDevicesByStation(item, vDevices);
            }
        }

        #region 输入IO/输出IO
        /// <summary>
        /// 输入的IOs
        /// </summary>
        private ObservableCollection<VIO> _inputIOs;
        public ObservableCollection<VIO> InputIOs
        {
            get { return _inputIOs; }
            set
            {
                SetProperty(ref _inputIOs, value);
            }
        }

        /// <summary>
        /// 输出的IOs
        /// </summary>
        private ObservableCollection<VIO> _outputIOs;
        public ObservableCollection<VIO> OutputIOs
        {
            get { return _outputIOs; }
            set
            {
                SetProperty(ref _outputIOs, value);
            }
        }

        /// <summary>
        /// 更新IO 数据源
        /// </summary>
        private void UpdateIOs(List<DeviceModel> devices)
        {
            InputIOs = new ObservableCollection<VIO>();
            OutputIOs = new ObservableCollection<VIO>();

            foreach (var device in devices.OrderBy(u => u.Sort))
            {
                if (device.Tag is VIO inputIO && inputIO.Behavior == IOBehavior.Input)
                {
                    if (!InputIOs.Contains(inputIO))
                    {
                        InputIOs.Add(inputIO);
                    }
                }

                if (device.Tag is VIO outputIO && outputIO.Behavior == IOBehavior.Output)
                {
                    if (!InputIOs.Contains(outputIO))
                    {
                        OutputIOs.Add(outputIO);
                    }
                }
            }
        }
        #endregion
    }

    /// <summary>
    /// 设备模块
    /// </summary>
    public class DeviceModel : BindableBase
    {
        private string _device;
        public string Device
        {
            get
            {
                return _device;
            }
            set
            {
                SetProperty(ref _device, value);
            }
        }

        public string DeviceType { get; set; }

        public int Sort
        {
            get
            {
                if (Tag is VIO s)
                {
                    return s.Sort;
                }

                return Tag.Sort;
            }
        }

        public IVirtualDevice Tag { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public DeviceModel() { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="device"></param>
        public DeviceModel(IVirtualDevice device)
        {
            DeviceType = device.GetType().Name;
            Device = device.Name;
            Tag = device;
        }
    }
}