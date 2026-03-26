#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       BaseDeviceVM
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.SubSystem.ViewModel.Device
* 文 件 名:       BaseDeviceVM.cs
* 创建时间:       2022/4/21 13:58:03
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      865584db-9d65-40de-a5de-2d8e7e3066b0
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/21 13:58:03
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.Assets;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Real;
using Luster.SimDevice.EngineUI;
using Luster.SimDevice.EngineUI.Events;
using Luster.SimDevice.EngineUI.Models;
using Luster.SimDevice.Laser.SmartRay;
using Luster.SimDevice.Real;
using Luster.SimDevice.SubSystem.Extension;
using Luster.SimDevice.SubSystem.ViewModel.Dialog;
using Luster.SimDevice.SubSystem.Views.Dialog;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Luster.SimDevice.SubSystem.ViewModel.Device
{
    public class BaseDeviceVM : PageVM
    {
        /// <summary>
        /// 显示添加按钮
        /// </summary>
        public override bool IsShowAdd => true;

        /// <summary>
        /// 
        /// </summary>
        public static string DeviceParamKey = "DeviceModel";

        /// <summary>
        /// 是否为相机设备
        /// </summary>
        public virtual bool IsCameraDevice => false;

        /// <summary>
        /// 是否为相机设备
        /// </summary>
        public virtual bool IsLightController => false;

        /// <summary>
        /// 
        /// </summary>
        private Type deviceType = typeof(ICamera);

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="_engine"></param>
        /// <param name="dialogService"></param>
        protected BaseDeviceVM(ISimDeviceEngineUI _engine, Type _deviceType) : base(_engine)
        {
            deviceType = _deviceType;
            LoadDevices(deviceType);
        }


        /// <summary>
        /// 页面加载过来后，进行设备初始化
        /// </summary>
        /// <param name="mode"></param>
        protected override void ModeChanged(DeviceMode mode)
        {
            base.ModeChanged(mode);
            foreach (var item in DeviceList)
            {
                if (mode == DeviceMode.Real)
                {
                    var device = deviceEngine.GetDeviceByID(item.ID);
                    if (device != null)
                    {
                        // 设备状态变更事件
                        device.StatusChangedEvent -= Device_StatusChangedEvent;
                        device.StatusChangedEvent += Device_StatusChangedEvent;
                    }
                }
                else
                {
                    item.Status = DeviceStatus.Offline;
                }
            }
        }

        /// <summary>
        /// //特例：SmartRay线扫就要删除SmartRay管理线扫类中对应的对象
        /// </summary>
        /// <param name="model"></param>
        private void RemoveSmartRay(DeviceModel model)
        {
            if (model.Brand == "SmartRay")
            {
                if (model.ToDevice() is ILineLaser lineLaser)
                {
                    NativeMethod.RemoveSensor((SRLineLaser)lineLaser);
                }
            }
        }

        /// <summary>
        /// 实时更新状态
        /// </summary>
        /// <param name="obj"></param>
        private void Device_StatusChangedEvent(IDevice device, DeviceStatus newStatus)
        {
            var deviceModel = DeviceList.FirstOrDefault(u => u.ID == device.ID);
            if (deviceModel != null)
            {
                deviceModel.Status = newStatus;
            }
        }

        /// <summary>
        /// 加载所有的设备信息
        /// </summary>
        private void LoadDevices(Type deviceType)
        {
            var devices = SimEngineUI.Engine.GetRealDevices(deviceType);
            DeviceList = new ObservableCollection<DeviceModel>();
            foreach (var device in devices)
            {
                var deviceModel = new DeviceModel(device.Name, deviceType);
                deviceModel.SelectedChangedEvent += DeviceModel_SelectedChangedEvent;
                deviceModel.SetModel(device);
                DeviceList.Add(deviceModel);
            }
        }

        /// <summary>
        /// 注册事件
        /// </summary>
        /// <param name="obj"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void DeviceModel_SelectedChangedEvent(bool obj)
        {
            if (!isOnSelectAll)
                IsSelectAll = DeviceList.Count(u => u.IsSelected) == DeviceList.Count;
        }

        /// <summary>
        /// 设备信息
        /// </summary>
        private ObservableCollection<DeviceModel> _deviceList;
        public ObservableCollection<DeviceModel> DeviceList
        {
            get { return _deviceList; }
            set { SetProperty(ref _deviceList, value); }
        }

        /// <summary>
        /// 选中行事件
        /// </summary>
        private DelegateCommand<DeviceModel> _selectedCommand;
        public DelegateCommand<DeviceModel> SelectedCommand => _selectedCommand ?? (_selectedCommand = new DelegateCommand<DeviceModel>((item) =>
        {
            if (item != null)
            {
                var device = deviceEngine.GetDeviceByID(item.ID);
                // 更新配置
                PropertyObj = device;
            }
        }));


        /// <summary>
        /// 检查设备是否已存在
        /// </summary>
        /// <returns>true 已存在，false 不存在</returns>
        public virtual bool CheckDeviceIsExist(IDevice device)
        {
            return false;
        }

        /// <summary>
        /// 弹窗新增对话框
        /// </summary>
        public override void AddNewItem()
        {
            dialogService.ShowDeviceConfig(deviceType, null, (Action<IDialogResult>)(r =>
              {
                  if (r.Result == ButtonResult.OK)
                  {
                      if (r.Parameters.TryGetValue<DeviceModel>(DeviceParamKey, out var deviceModel))
                      {
                          var newDevice = deviceModel.ToDevice();
                          if (!CheckDeviceIsExist(newDevice))
                          {
                              // 初始化ID
                              deviceModel.ID = Guid.NewGuid();
                              deviceModel.SelectedChangedEvent -= this.DeviceModel_SelectedChangedEvent;
                              deviceModel.SelectedChangedEvent += this.DeviceModel_SelectedChangedEvent;

                              // 添加到引擎中
                              newDevice.ID = deviceModel.ID;
                              deviceEngine.AddDevice(newDevice);

                              // 添加到集合列表
                              DeviceList.Add(deviceModel);

                              SimEngineUI.OnPublish<DeviceChangedEvent>();
                          }
                      }
                  }
              }));
        }

        /// <summary>
        /// 自动搜索硬件信息
        /// </summary>
        public override void AutoSearchDevice()
        {
            var dialogParameters = new DialogParameters();
            dialogParameters.Add("Title", Langs.LangProvider.GetLang("Device"));
            if (base.GetType().Name == nameof(LineLaserContentVM))
            {
                dialogService.ShowDialog(nameof(SearchLineLaserDialog), dialogParameters, r => {
                    if (r.Result == ButtonResult.OK)
                    {
                        if (r.Parameters.TryGetValue<List<IDevice>>(AutoSearchDialogVM.DeviceParamKey, out var devices))
                        {
                            
                            foreach (var device in devices)
                            {
                                if (CheckDeviceIsExist(device))
                                {
                                    continue;
                                }
                                var matchDevice = deviceEngine.GetDeviceByID(device.ID);
                                if (matchDevice == null)
                                {
                                    // 给设备添加ID
                                    device.ID = Guid.NewGuid();

                                    // 添加到引擎中
                                    deviceEngine.AddDevice(device);

                                    // 添加到集合列表
                                    var deviceModel = new DeviceModel(device.Name, deviceType);
                                    deviceModel.SelectedChangedEvent += DeviceModel_SelectedChangedEvent;
                                    deviceModel.SetModel(device);
                                    DeviceList.Add(deviceModel);
                                }
                                else
                                {
                                    // 删除原有设备
                                    deviceEngine.ReomoveDevice(matchDevice.ID);
                                    // 删除原有视图Model
                                    var model = DeviceList.Where(x => x.ID == matchDevice.ID).FirstOrDefault();
                                    DeviceList.Remove(model);
                                    // 添加到引擎中
                                    deviceEngine.AddDevice(device);

                                    // 添加到集合列表
                                    var deviceModel = new DeviceModel(device.Name, deviceType);
                                    deviceModel.SelectedChangedEvent += DeviceModel_SelectedChangedEvent;
                                    deviceModel.SetModel(device);
                                    DeviceList.Add(deviceModel);
                                }
                            }
                        }
                    }

                });
            }
            else
            {
                dialogService.ShowDialog(nameof(SearchCameraDialog), dialogParameters, r =>
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        if (r.Parameters.TryGetValue<List<IDevice>>(AutoSearchDialogVM.DeviceParamKey, out var devices))
                        {
                            foreach (var device in devices)
                            {
                                if (CheckDeviceIsExist(device))
                                {
                                    continue;
                                }
                                var matchDevice = deviceEngine.GetDeviceByID(device.ID);
                                if (matchDevice == null)
                                {
                                    // 给设备添加ID
                                    device.ID = Guid.NewGuid();

                                    // 添加到引擎中
                                    deviceEngine.AddDevice(device);

                                    // 添加到集合列表
                                    var deviceModel = new DeviceModel(device.Name, deviceType);
                                    deviceModel.SelectedChangedEvent += DeviceModel_SelectedChangedEvent;
                                    deviceModel.SetModel(device);
                                    DeviceList.Add(deviceModel);
                                }
                                else
                                {
                                    // 删除原有设备
                                    deviceEngine.ReomoveDevice(matchDevice.ID);
                                    // 删除原有视图Model
                                    var model = DeviceList.Where(x => x.ID == matchDevice.ID).FirstOrDefault();
                                    DeviceList.Remove(model);
                                    // 添加到引擎中
                                    deviceEngine.AddDevice(device);

                                    // 添加到集合列表
                                    var deviceModel = new DeviceModel(device.Name, deviceType);
                                    deviceModel.SelectedChangedEvent += DeviceModel_SelectedChangedEvent;
                                    deviceModel.SetModel(device);
                                    DeviceList.Add(deviceModel);
                                }
                            }
                        }
                    }
                });
            }
        }

        /// <summary>
        /// 选中行事件
        /// </summary>
        private DelegateCommand<object> _editCommand;
        public DelegateCommand<object> EditCommand => _editCommand ?? (_editCommand = new DelegateCommand<object>((item) =>
        {
            dialogService.ShowDeviceConfig(deviceType, item as DeviceModel, r =>
             {
                 if (r.Result == ButtonResult.OK)
                 {
                     if (r.Parameters.TryGetValue<DeviceModel>(DeviceParamKey, out var deviceModel))
                     {
                         // update device
                         var device = deviceEngine.GetDeviceByID(deviceModel.ID);
                         if (device != null)
                         {
                             device.Name = deviceModel.Name;
                             device.Adapter = deviceModel.Adapter;
                             if (deviceType == typeof(ILightController))
                             {
                                 var light = device as ILightController;
                                 light.Channel = deviceModel.ChannelCount;
                             }
                         }
                         // update deviceList
                         var dModel = DeviceList.FirstOrDefault(u => u.ID == deviceModel.ID);
                         if (dModel != null)
                         {
                             dModel.SetModel(device);
                         }
                     }
                 }
             });
        }));

        /// <summary>
        /// 选中行事件
        /// </summary>
        private DelegateCommand<DeviceModel> _removeCommand;
        public DelegateCommand<DeviceModel> RemoveCommand => _removeCommand ?? (_removeCommand = new DelegateCommand<DeviceModel>((item) =>
        {            
                dialogService.ShowConfirm($"确认删除设备,设备:{item.Name}", r =>
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        deviceEngine.ReomoveDevice(item.ID);
                        var model = DeviceList.FirstOrDefault(u => u.ID == item.ID);
                        if (model != null)
                        {
                            DeviceList.Remove(model);
                            RemoveSmartRay(model);

                            SimEngineUI.OnPublish<DeviceChangedEvent>();
                        }
                    }
                });
        }));

        /// <summary>
        /// 判断是否由SelectAll触发
        /// </summary>
        private bool isOnSelectAll = false;

        /// <summary>
        /// 选中行事件
        /// </summary>
        private DelegateCommand<object> _selectAllCommand;
        public DelegateCommand<object> SelectAllCommand => _selectAllCommand ?? (_selectAllCommand = new DelegateCommand<object>((chkBox) =>
        {
            isOnSelectAll = true;
            CheckBox cbk = chkBox as CheckBox;
            foreach (var item in DeviceList)
            {
                item.IsSelected = cbk.IsChecked.Value;
            }

            isOnSelectAll = false;
        }));

        /// <summary>
        /// 是否选择所有
        /// </summary>
        private bool _isSelectAll;
        public bool IsSelectAll
        {
            get { return _isSelectAll; }
            set { SetProperty(ref _isSelectAll, value); }
        }
    }
}