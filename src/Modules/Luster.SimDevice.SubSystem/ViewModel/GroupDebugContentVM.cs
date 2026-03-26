using Luster.Common.DataStruct.DataModels;
using Luster.Motion.DataStruct.DataModels;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.DataStruct.Virtual;
using Luster.SimDevice.EngineUI;
using Luster.SimDevice.EngineUI.Models;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace Luster.SimDevice.SubSystem.ViewModel
{
    public class GroupDebugContentVM : PageVM
    {

        /// <summary>
        /// 输入组合
        /// </summary>
        private ObservableCollection<AxisModel> _axisList;
        public ObservableCollection<AxisModel> AxisList
        {
            get => _axisList;
            set => SetProperty(ref _axisList, value);
        }

        /// <summary>
        /// 相机列表
        /// </summary>
        public List<IVirtualDevice> CameraList { get; set; }



        /// <summary>
        /// 数字量输入组合
        /// </summary>
        private ObservableCollection<IOModel> _diDatas;
        public ObservableCollection<IOModel> DIDatas
        {
            get => _diDatas;
            set => SetProperty(ref _diDatas, value);
        }

        /// <summary>
        /// 数字量输出组合
        /// </summary>
        private ObservableCollection<IOModel> _doDatas;
        public ObservableCollection<IOModel> DODatas
        {
            get => _doDatas;
            set => SetProperty(ref _doDatas, value);
        }


        /// <summary>
        /// 模拟量输入组合
        /// </summary>
        private ObservableCollection<IOModel> _aiDatas;
        public ObservableCollection<IOModel> AIDatas
        {
            get => _aiDatas;
            set => SetProperty(ref _aiDatas, value);
        }

        /// <summary>
        /// 模拟量输出组合
        /// </summary>
        private ObservableCollection<IOModel> _aoDatas;
        public ObservableCollection<IOModel> AODatas
        {
            get => _aoDatas;
            set => SetProperty(ref _aoDatas, value);
        }



        /// <summary>
        /// 气缸组合
        /// </summary>
        private ObservableCollection<CylinderModel> _cylinderLists;
        public ObservableCollection<CylinderModel> CylinderLists
        {
            get => _cylinderLists;
            set => SetProperty(ref _cylinderLists, value);
        }


        /// <summary>
        /// 真空组合
        /// </summary>
        private ObservableCollection<VacuumModel> _vacuumLists;
        public ObservableCollection<VacuumModel> VacuumLists
        {
            get => _vacuumLists;
            set => SetProperty(ref _vacuumLists, value);
        }


        /// <summary>
        /// 模块集合
        /// </summary>
        private List<string> _modules;
        public List<string> Modules
        {
            get { return _modules; }
            set
            {
                SetProperty(ref _modules, value);
            }
        }

        /// <summary>
        /// 模块集合
        /// </summary>
        private string _module;
        public string   Module
        {
            get { return _module; }
            set
            {
                SetProperty(ref _module, value);
                GetIOList("");
                if(AxisList!=null)
                {
                    BuildAxisLists();
                }
                if(CylinderLists!=null)
                {
                    BuildCylinderLists();
                }
                if (VacuumLists != null)
                {
                    BuildVacuumLists();
                }
                CameraList = deviceEngine.GetDevices(typeof(VCamera), Module);
            }
        }


        /// <summary>
        /// 实时刷新标志
        /// </summary>
        private bool isRefresh = false;

        private Dispatcher _dispatcher;


        protected GroupDebugContentVM(ISimDeviceEngineUI _engine,Dispatcher dispatcher) : base(_engine)
        {
            DIDatas = new ObservableCollection<IOModel>();
            DODatas = new ObservableCollection<IOModel>();
            AIDatas = new ObservableCollection<IOModel>();
            AODatas = new ObservableCollection<IOModel>();
            CylinderLists = new ObservableCollection<CylinderModel>();
            AxisList = new ObservableCollection<AxisModel>();
            VacuumLists =new ObservableCollection<VacuumModel>();
            Modules = deviceEngine.GetModules();
            if (Modules != null)
                Module = Modules[0];
            CameraList = deviceEngine.GetDevices(typeof(VCamera),Module);     
            BuildVacuumLists();
            BuildCylinderLists();
            BuildAxisLists();
            GetIOList("", IOBehavior.Input, IOType.Digital);
            _dispatcher = dispatcher;
        }

        #region 控制指令


        /// <summary>
        /// 气缸伸出
        /// </summary>
        private DelegateCommand<CylinderModel> _cylinderExtendCommand;
        public DelegateCommand<CylinderModel> CylinderExtendCommand => _cylinderExtendCommand ?? (_cylinderExtendCommand = new DelegateCommand<CylinderModel>((cylinder) =>
        {
            cylinder.Tag.Extend();
        }));

        /// <summary>
        /// 气缸缩回
        /// </summary>
        private DelegateCommand<CylinderModel> _cylinderRetractCommand;
        public DelegateCommand<CylinderModel> CylinderRetractCommand => _cylinderRetractCommand ?? (_cylinderRetractCommand = new DelegateCommand<CylinderModel>((cylinder) =>
        {
            cylinder.Tag.Retract();
        }));


        /// <summary>
        /// 真空吸
        /// </summary>
        private DelegateCommand<VacuumModel> _vacuumSuckCommand;
        public DelegateCommand<VacuumModel> VacuumSuckCommand => _vacuumSuckCommand ?? (_vacuumSuckCommand = new DelegateCommand<VacuumModel>((vacuum) =>
        {
            vacuum.Tag.Suck();
        }));

        /// <summary>
        /// 真空破
        /// </summary>
        private DelegateCommand<VacuumModel> _vacuumBlowCommand;
        public DelegateCommand<VacuumModel> VacuumBlowCommand => _vacuumBlowCommand ?? (_vacuumBlowCommand = new DelegateCommand<VacuumModel>((vacuum) =>
        {
            vacuum.Tag.Blow();
        }));

        /// <summary>
        /// 真空关
        /// </summary>
        private DelegateCommand<VacuumModel> _vacuumCloseCommand;
        public DelegateCommand<VacuumModel> VacuumCloseCommand => _vacuumCloseCommand ?? (_vacuumCloseCommand = new DelegateCommand<VacuumModel>((vacuum) =>
        {
            vacuum.Tag.Close();
        }));


        /// <summary>
        /// 真空破+关
        /// </summary>
        private DelegateCommand<VacuumModel> _vacuumBlowCloseCommand;
        public DelegateCommand<VacuumModel> VacuumBlowCloseCommand => _vacuumBlowCloseCommand ?? (_vacuumBlowCloseCommand = new DelegateCommand<VacuumModel>((vacuum) =>
        {
            vacuum.Tag.BlowClose();
        }));



        #region 轴相关
        /// <summary>
        /// 使能
        /// </summary>
        private DelegateCommand<AxisModel> _enableCommand;
        public DelegateCommand<AxisModel> EnableCommand => _enableCommand ?? (_enableCommand = new DelegateCommand<AxisModel>((axis) =>
        {
            if (axis != null)
            {
                axis.Tag.ServOn(true);
            }
        }));

        /// <summary>
        /// 点动
        /// </summary>
        private DelegateCommand<AxisModel> _jogCommand;
            public DelegateCommand<AxisModel> JogCommand => _jogCommand ?? (_jogCommand = new DelegateCommand<AxisModel>((axis) =>
            {
                if (axis != null)
                {
                    axis.Tag.ServOn(true);
                }
            }));


        #endregion


        #endregion










        /// <summary>
        /// 获取数据
        /// </summary>
        /// <param name="key"></param>
        /// <param name="behavior"></param>
        /// <param name="ioType"></param>
        /// <returns></returns>
        private IEnumerable<VIO> GetIOList(string key = "", IOBehavior behavior = IOBehavior.Input, IOType ioType = IOType.Digital)
        {
            var list = deviceEngine.GetDevices(typeof(VIO))
                .Select(u => u as VIO)
                .Where(u => u.CardName == "LC")
                //.Where(u => u.IOType == ioType)
                //.Where(u => u.Behavior == behavior)
                .Where(u => u.Module == Module);

            if (!string.IsNullOrEmpty(key))
            {
                list = list.Where(u => u.CardName.Contains(key) || u.Name.Contains(key));
            }
            DIDatas?.Clear();
            DODatas?.Clear();
            AIDatas?.Clear();
            AODatas?.Clear();

            foreach (var item in list)
            {
                var ioModel = new IOModel(item);
                //ioModel.SelectedChanged -= IoModel_SelectedChanged;
                //ioModel.SelectedChanged += IoModel_SelectedChanged;

                if(DIDatas!=null)
                {
                    if(item.IOType == IOType.Digital && item.Behavior==IOBehavior.Input)
                    {
                        DIDatas.Add(ioModel);
                    }
                }


                if (DODatas != null)
                {
                    if (item.IOType == IOType.Digital && item.Behavior == IOBehavior.Output)
                    {
                        DODatas.Add(ioModel);
                    }
                }



                if (AIDatas != null)
                {
                    if (item.IOType == IOType.Analog && item.Behavior == IOBehavior.Input)
                    {
                        AIDatas.Add(ioModel);
                    }
                }

                if (AODatas != null)
                {
                    if (item.IOType == IOType.Analog && item.Behavior == IOBehavior.Output)
                    {
                        AODatas.Add(ioModel);
                    }
                }
            }
            return list;
        }


        //刷新主界面显示状态
        private void RefreshStatus()
        {

            Task.Run(async () =>
            {
                while (true)
                {
                    if (!isRefresh)
                        break;

                    //更新DI
                    if(DIDatas.Count>0)
                    {
                        _dispatcher.Invoke(() =>
                        {
                            foreach (var item in DIDatas)
                            {
                               item.DigitalValue = item.Tag.GetDigitalIn();
                            }
                        });
                    }


                    //更新DO
                    if (DODatas.Count > 0)
                    {
                        _dispatcher.Invoke(() =>
                        {
                            foreach (var item in DODatas)
                            {
                                item.DigitalValue = item.Tag.GetDigitalOut();
                            }
                        });
                    }



                    //更新气缸
                    if (CylinderLists.Count > 0)
                    {
                        _dispatcher.Invoke(() =>
                        {
                            foreach (var item in CylinderLists)
                            {
                                item.StatusExtend = item.Tag.InExtendSensor.GetDigitalIn();
                                item.StatusRetract = item.Tag.InRetractSensor.GetDigitalIn();
                            }
                        });
                    }


                    //更新真空
                    if (VacuumLists.Count > 0)
                    {
                        // 状态更新，涉及到UI显示，需要在主线程调用
                        _dispatcher.Invoke(() =>
                        {
                            foreach (var item in VacuumLists)
                            {
                                item.Status = item.Tag.InVacuumSensor.GetDigitalIn();
                            }
                        });
                    }


                    // 等待时长
                    await Task.Delay(500);
                }
            });
        }




        /// <summary>
        /// 生成轴列表
        /// </summary>
        private void BuildAxisLists()
        {
            AxisList.Clear();
            var devices = deviceEngine.GetDevices(typeof(VAxis));
            foreach (var device in devices)
            {
                var vAxis= device as VAxis;
                var axisModel = new AxisModel(vAxis);
                if (string.IsNullOrEmpty(Module))
                {
                    AxisList.Add(axisModel);
                }
                else
                {
                    if (vAxis.Module.Contains(Module) || vAxis.Name.Contains(Module))
                    {
                        AxisList.Add(axisModel);
                    }
                }
            }

        }



        /// <summary>
        /// 生成真空列表
        /// </summary>
        private void BuildVacuumLists()
        {
            VacuumLists.Clear();
            var devices = deviceEngine.GetDevices(typeof(VVacuum));
            foreach (var device in devices)
            {
                var vVacuum = device as VVacuum;
                var vacuumModel = new VacuumModel(vVacuum);
                if (string.IsNullOrEmpty(Module))
                {
                    VacuumLists.Add(vacuumModel);
                }
                else
                {
                    if (vVacuum.Module.Contains(Module) || vVacuum.Name.Contains(Module))
                    {
                        VacuumLists.Add(vacuumModel);
                    }
                }
            }
        }

        /// <summary>
        /// 生成气缸列表
        /// </summary>
        private void BuildCylinderLists()
        {
            CylinderLists.Clear();
            var devices = deviceEngine.GetDevices(typeof(VCylinder));
            foreach (var device in devices)
            {
                var vCylinder = device as VCylinder;
                var cylinderModel = new CylinderModel(vCylinder);
                if (string.IsNullOrEmpty(Module))
                {
                    CylinderLists.Add(cylinderModel);
                }
                else
                {
                    if (vCylinder.Module.Contains(Module) || vCylinder.Name.Contains(Module))
                    {
                        CylinderLists.Add(cylinderModel);
                    }
                }
            }
        }





        #region 页面

        //进入当前页面
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            isRefresh = true;
            RefreshStatus();
            base.OnNavigatedTo(navigationContext);
        }


        //离开当前页面
        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            isRefresh = false;

            base.OnNavigatedFrom(navigationContext);
        }

        #endregion


    }
}
