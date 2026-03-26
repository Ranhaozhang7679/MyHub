#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       LeftMenuViewModel
* 机器名称:       L05123-NB
* 命名空间:       Luster.SimDevice.SubSystem.ViewModel
* 文 件 名:       LeftMenuViewModel.cs
* 创建时间:       2022/4/13 15:21:42
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      4a25919e-7292-42f4-bb1b-a8703d78699b
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/4/13 15:21:42
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.Assets;
using Luster.Common.DataStruct.Extensions;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Real;
using Luster.Motion.DataStruct.Virtual;
using Luster.SimDevice.Engine;
using Luster.SimDevice.EngineUI;
using Luster.SimDevice.EngineUI.Events;
using Luster.SimDevice.EngineUI.Models;
using Luster.SimDevice.Real;
using Luster.SimDevice.SubSystem.Config;
using Luster.SimDevice.SubSystem.Events;
using Luster.SimDevice.SubSystem.Views;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Luster.SimDevice.SubSystem.ViewModel
{
    public class LeftMenuVM : BaseVM
    {
        /// <summary>
        /// 区域视图管理
        /// </summary>
        private IRegionManager _regionManager;

        private Dispatcher _dispatcher;

        private PageModel _selectedPage;
        public PageModel SelectedPage
        {
            get => _selectedPage;
            set => SetProperty(ref _selectedPage, value);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="deviceEngine">设备引擎</param>
        /// <param name="regionManager">区域视图管理器</param>
        protected LeftMenuVM(ISimDeviceEngineUI deviceEngine, IRegionManager regionManager, Dispatcher dispatcher) : base(deviceEngine)
        {
            _regionManager = regionManager;

            // 构建左侧菜单
            DeviceList = new ObservableCollection<PageModel>();
            _dispatcher = dispatcher;

            BuildMenus();

            HomeCommand.Execute();
        }

        protected override void Subscribe(ISimDeviceEngineUI engineUI)
        {
            engineUI.Subscribe<ProjOpenEvent, ProjectInfo>((proj) =>
             {
                 // 构建左侧菜单
                 DeviceList = new ObservableCollection<PageModel>();
                 BuildMenus();
             });

            // 模式切换
            engineUI.Subscribe<DeviceModeEvent, DeviceMode>((m) =>
            {
                IsReal = SimEngineUI.DeviceMode == DeviceMode.Real;
                ModeTitle = SimEngineUI.DeviceMode.GetDescription();
            });

            engineUI.Subscribe<NavigationEvent, PageModel>(p =>
            {
                IsHomePage = p.Name == "Home";
            });

            // 设备发生变更
            engineUI.Subscribe<DeviceChangedEvent>(() =>
            {
                BuildMenus();
            });
        }

        /// <summary>
        /// 构造菜单
        /// </summary>
        private void BuildMenus()
        {
            _dispatcher.Invoke(() =>
            {
                DeviceList.Clear();

                DeviceList.Add(new PageModel()
                {
                    Group = "Management",
                    Icon = "\xe6d7",
                    Name = "AlarmConfig",
                    PageView = "ErrorContent",
                    DeviceType = DeviceType.Printer
                });

                DeviceList.Add(new PageModel()
                {
                    Group = "Management",
                    Icon = "\xe6d7",
                    Name = "Maintain",
                    PageView = "MaintainContent",
                    DeviceType = DeviceType.Printer
                });

                DeviceList.Add(new PageModel()
                {
                    Group = "Management",
                    Icon = "\xe6d7",
                    Name = "ModuleName",
                    PageView = "ModuleNameContent",
                    DeviceType = DeviceType.Printer
                });

                //DeviceList.Add(new PageModel()
                //{
                //    Group = "Management",
                //    Icon = "\xe6d7",
                //    Name = "AxisPosConfig",
                //    PageView = "AxisPosContent",
                //    DeviceType = DeviceType.Printer
                //});

                DeviceList.Add(new PageModel()
                {
                    Group = "Management",
                    Icon = "\xe6d7",
                    Name = "AxisIODebug",
                    PageView = "AxisIODebugContent",
                    DeviceType = DeviceType.Printer
                });

                DeviceList.Add(new PageModel()
                {
                    Group = "Management",
                    Icon = "\xe6d7",
                    Name = "ModuleConfig",
                    PageView = "VModuleContent",
                    DeviceType = DeviceType.Printer
                });
                
                DeviceList.Add(new PageModel()
                {
                    Group = "Management",
                    Icon = "\xe6d7",
                    Name = "AlarmConfigCustom",
                    PageView = "ErrorContentCustom",
                    DeviceType = DeviceType.Printer
                });
                

                DeviceList.Add(new PageModel()
                {
                    Group = "Management",
                    Icon = "\xe6d7",
                    Name = "PositionParameterConfig",
                    PageView = "PositionParameterContent",
                    DeviceType = DeviceType.Printer
                });

                DeviceList.Add(new PageModel()
                {
                    Group = "Management",
                    Icon = "\xe6d7",
                    Name = "KeyParameterConfig",
                    PageView = "KeyParameterContent",
                    DeviceType = DeviceType.Printer
                });

                var realTypes = deviceEngine.GetRealTypes(); //realDevices
                var vTypes = deviceEngine.GetVirtualTypes();//virtualDevices
                var v = deviceEngine.GetDevice_Type();

                List<PageModel> vDevices = new List<PageModel>();

                foreach (var item in v)
                {

                    if (realTypes.Contains(item.Key))
                    {
                        var d = item.Value as IDevice;
                        if (d.DeviceType == DeviceType.LineLaser)
                        {
                            continue;
                        }
                        var deviceView = new PageModel()
                        {
                            Group = "Device",
                            Icon = d.Icon,
                            Name = d.DeviceType.ToString(),
                            PageView = $"{d.DeviceType}Content",
                            DeviceType = d.DeviceType,
                            ItemNum = deviceEngine.GetRealDevices(item.Key).Count(),
                        };
                        DeviceList.Add(deviceView);
                    }
                    else if (vTypes.Contains(item.Key))
                    {
                        if (item.Key.Name == "VAlarm") continue;

                        var d = item.Value as IVirtualDevice;
                        var deviceView = new PageModel()
                        {
                            Group = "VDevice",
                            Icon = d.Icon,
                            Name = d.GetType().Name,
                            PageView = $"{item.Key.Name}Content",
                            ItemNum = deviceEngine.GetDevices(item.Key).Count(),
                            Sort = d.Sort,
                        };
                        vDevices.Add(deviceView);
                    }

                }
                vDevices.OrderBy(u => u.Sort).ToList().ForEach(vDevice => DeviceList.Add(vDevice));



                //foreach (var realType in realTypes)
                //{
                //    var device = Activator.CreateInstance(realType) as IDevice;
                //    // 添加
                //    if (!DeviceList.Any(u => u.DeviceType == device.DeviceType))
                //    {
                //        if (device.DeviceType == DeviceType.Printer) continue;
                //        var deviceView = new PageModel()
                //        {
                //            Group = "Device",
                //            Icon = device.Icon,
                //            Name = device.DeviceType.ToString(),
                //            PageView = $"{device.DeviceType}Content",
                //            DeviceType = device.DeviceType,
                //            ItemNum = deviceEngine.GetRealDevices(realType).Count(),
                //        };

                //        DeviceList.Add(deviceView);
                //    }
                //    else
                //    {
                //        var deviceView = DeviceList.FirstOrDefault(u => u.DeviceType == device.DeviceType);
                //        deviceView.ItemNum += deviceEngine.GetRealDevices(realType).Count();
                //    }
                //}

                //List<PageModel> vDevices = new List<PageModel>();
                //var vTypes = deviceEngine.GetVirtualTypes();
                //foreach (var vType in vTypes)
                //{
                //    var vDevice = Activator.CreateInstance(vType) as IVirtualDevice;
                //    if (vType.Name == "VPrinter" || vType.Name == "VAxisM" || vType.Name == "VAlarm") continue;

                //    var deviceView = new PageModel()
                //    {
                //        Group = "VDevice",
                //        Icon = vDevice.Icon,
                //        Name = vDevice.GetType().Name,
                //        PageView = $"{vType.Name}Content",
                //        ItemNum = deviceEngine.GetDevices(vType).Count(),
                //        Sort = vDevice.Sort,
                //    };

                //    vDevices.Add(deviceView);
                //}

                //vDevices.OrderBy(u => u.Sort).ToList().ForEach(vDevice => DeviceList.Add(vDevice));

            });
        }

        /// <summary>
        ///  by zhujie
        /// </summary>
        private void BuildMenusNew()
        {
            Task.Run(() =>
            {
                var newDeviceList = new List<PageModel>();
                // 管理菜单
                newDeviceList.AddRange(new[]
                {
                    new PageModel { Group = "Management", Icon = "\xe6d7", Name = "AlarmConfig", PageView = "ErrorContent", DeviceType = DeviceType.Printer },
                    new PageModel { Group = "Management", Icon = "\xe6d7", Name = "Maintain", PageView = "MaintainContent", DeviceType = DeviceType.Printer },
                    new PageModel { Group = "Management", Icon = "\xe6d7", Name = "AxisPosConfig", PageView = "AxisPosContent", DeviceType = DeviceType.Printer },
                    new PageModel { Group = "Management", Icon = "\xe6d7", Name = "ModuleConfig", PageView = "VModuleContent", DeviceType = DeviceType.Printer }
                });

                // 真实设备
                var realTypes = deviceEngine.GetRealTypes();
                var realDeviceDict = new Dictionary<DeviceType, PageModel>();
                foreach (var realType in realTypes)
                {
                    var device = Activator.CreateInstance(realType) as IDevice;
                    if (device.DeviceType == DeviceType.Printer) continue;

                    if (!realDeviceDict.TryGetValue(device.DeviceType, out var deviceView))
                    {
                        deviceView = new PageModel
                        {
                            Group = "Device",
                            Icon = device.Icon,
                            Name = device.DeviceType.ToString(),
                            PageView = $"{device.DeviceType}Content",
                            DeviceType = device.DeviceType,
                            ItemNum = 0
                        };
                        realDeviceDict[device.DeviceType] = deviceView;
                        newDeviceList.Add(deviceView);
                    }
                    deviceView.ItemNum += deviceEngine.GetRealDevices(realType).Count();
                }

                // 虚拟设备
                var vTypes = deviceEngine.GetVirtualTypes();
                var vDevices = new List<PageModel>();
                foreach (var vType in vTypes)
                {
                    if (vType.Name == "VPrinter" || vType.Name == "VAxisM" || vType.Name == "VAlarm") continue;
                    var vDevice = Activator.CreateInstance(vType) as IVirtualDevice;
                    var deviceView = new PageModel
                    {
                        Group = "VDevice",
                        Icon = vDevice.Icon,
                        Name = vDevice.GetType().Name,
                        PageView = $"{vType.Name}Content",
                        ItemNum = deviceEngine.GetDevices(vType).Count(),
                        Sort = vDevice.Sort,
                    };
                    vDevices.Add(deviceView);
                }
                vDevices = vDevices.OrderBy(u => u.Sort).ToList();
                newDeviceList.AddRange(vDevices);

                // UI线程更新
                _dispatcher.Invoke(() =>
                {
                    DeviceList.Clear();
                    DeviceList.AddRange(newDeviceList);
                    //foreach (var item in newDeviceList)
                    //    DeviceList.Add(item);
                });
            });
        }

        /// <summary>
        /// 设备信息
        /// </summary>
        private ObservableCollection<PageModel> _deviceList;
        public ObservableCollection<PageModel> DeviceList
        {
            get { return _deviceList; }
            set { SetProperty(ref _deviceList, value); }
        }

        /// <summary>
        /// 菜单切换功能
        /// </summary>
        private DelegateCommand<PageModel> _selectedCommand;
        public DelegateCommand<PageModel> SelectedCommand => _selectedCommand ?? (_selectedCommand = new DelegateCommand<PageModel>((item) =>
        {
            if (item == null) return;
            PageNavigation(item);
        }));

        private void PageNavigation(PageModel pModel)
        {
            NavigationParameters parameters = new NavigationParameters();
            parameters.Add("Page", pModel);

            // 视图切换
            _regionManager.RequestNavigate("ContentRegion", pModel.PageView, (result) =>
            {
                if (result.Result == false)
                {
                    SimEngineUI.OnAlert(Common.DataStruct.Enums.LogType.Error, result.Error.Message);
                }

            }, parameters);

            // 通知切换事件
            SimEngineUI.OnPublish<NavigationEvent, PageModel>(pModel);
        }

        /// <summary>
        /// 菜单切换功能
        /// </summary>
        private DelegateCommand _homeCommand;
        public DelegateCommand HomeCommand => _homeCommand ?? (_homeCommand = new DelegateCommand(() =>
        {
            var homePage = new PageModel()
            {
                Sort = 0,
                Group = "Home",
                Name = "Home",
                PageView = $"HomeContent",
                ItemNum = 0,
            };
            if (SelectedPage != null)
            {
                SelectedPage = null;
            }
            PageNavigation(homePage);
        }));


        /// <summary>
        /// 切页分组调试功能
        /// </summary>
        private DelegateCommand _groupDebugCommand;
        public DelegateCommand GroupDebugCommand => _groupDebugCommand ?? (_groupDebugCommand = new DelegateCommand(() =>
        {
            var groupPage = new PageModel()
            {
                Sort = 0,
                Group = "GroupDebug",
                Name = "GroupDebug",
                PageView = $"GroupDebugContent",
                ItemNum = 0,
            };
            if (SelectedPage != null)
            {
                SelectedPage = null;
            }
            PageNavigation(groupPage);
        }));



        /// <summary>
        /// 切页-控制参数
        /// </summary>
        private DelegateCommand _controlParaCommand;
        public DelegateCommand ControlParaCommand => _controlParaCommand ?? (_controlParaCommand = new DelegateCommand(() =>
        {
            dialogService.ShowText("密码", "1996", r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    if (r.Parameters.TryGetValue("Text", out string password))
                    {
                        if (password == "Luster@1996")
                        {
                            var controlParaPage = new PageModel()
                            {
                                Sort = 0,
                                Group = "ControlPara",
                                Name = "ControlPara",
                                PageView = $"ControlParaContent",
                                ItemNum = 0,
                            };
                            if (SelectedPage != null)
                            {
                                SelectedPage = null;
                            }
                            PageNavigation(controlParaPage);
                        }
                    }
                }
            }
            );
        }));

        /// <summary>
        /// 模式标题
        /// </summary>
        private string _modeTitle = DeviceMode.Virtual.GetDescription();
        public string ModeTitle
        {
            get { return _modeTitle; }
            set { SetProperty(ref _modeTitle, value); }
        }

        /// <summary>
        /// 是否联机状态
        /// </summary>
        private bool _isReal;
        public bool IsReal
        {
            get { return _isReal; }
            set { SetProperty(ref _isReal, value); }
        }

        /// <summary>
        /// 是否联机状态
        /// </summary>
        private bool _isHomePage;
        public bool IsHomePage
        {
            get { return _isHomePage; }
            set { SetProperty(ref _isHomePage, value); }
        }

        /// <summary>
        /// 菜单切换功能
        /// </summary>
        private DelegateCommand _changeModeCommand;
        public DelegateCommand ChangeModeCommand => _changeModeCommand ?? (_changeModeCommand = new DelegateCommand(() =>
        {
            // 切换到硬件
            if (IsReal)
            {
                SimEngineUI.SetDeviceModeAsync(DeviceMode.Real);
            }
            else
            {
                SimEngineUI.SetDeviceModeAsync(DeviceMode.Virtual);
            }
        }));
    }
}


