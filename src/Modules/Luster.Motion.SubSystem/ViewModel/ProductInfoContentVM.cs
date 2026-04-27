using HandyControl.Controls;
using Luster.Common.Assets;
using Luster.Controls.Wpf.Commands;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.CommonUI.ViewModel.Dialogs;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.EditorUI.Extensions;
using Luster.Motion.Integration.SFC;
using Luster.Motion.Integration.Web;
using Luster.Motion.TaskFlow.Engine;
using Luster.Motion.TaskFlow.Engine.HyperTrain;
using Luster.TaskFlow.Common.Interfaces;
using Microsoft.VisualBasic;
using Prism.Commands;
using Prism.Events;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

using Microsoft.Win32;
using Luster.Motion.DataStruct.DataModels;
//using System.Windows;

namespace Luster.Motion.SubSystem.ViewModel
{
    public class ProductInfoContentVM : MotionVM
    {
        /// <summary>
        /// 对话框服务
        /// </summary>
        private IDialogService _dialogService;

        /// <summary>
        /// 控制器
        /// </summary>
        private IMotionController _mController;

        /// <summary>
        /// 驾驶舱API
        /// </summary>
        private HyperTrainAPI _hTrainAPI;

        /// <summary>
        /// VisionAPI
        /// </summary>
        private VisionAPI _visionAPI;

        /// <summary>
        /// Hive 系统
        /// </summary>
        private HiveAPI _hiveAPI;

        /// <summary>
        /// 首件 系统
        /// </summary>
        private WholeLineAPI _wholeAPI;


        /// <summary>
        /// 弹窗
        /// </summary>
        private Dispatcher _dispatcher;

        /// <summary>
        /// SFC
        /// </summary>
        private SFCHelper _sfcHelper;



        /// <summary>
        /// Web系统配置
        /// </summary>
        private readonly IWebService _webService;

        /// <summary>
        /// 配置文件
        /// </summary>
        private WebConfig webConfig = null;

        private const string LusterPwd = "Luster@1996";

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="commonBus"></param>
        /// <param name="dialogService"></param>
        /// <param name="mController"></param>
        /// <param name="hTrainAPI"></param>
        /// <param name="dispatcher"></param>
        public ProductInfoContentVM(ICommonBus commonBus,
            IDialogService dialogService,
            IMotionController mController,
            IWebService webService,
            HyperTrainAPI hTrainAPI,
            VisionAPI vApi,
            HiveAPI hApi,
            WholeLineAPI wholeAPI,
            Dispatcher dispatcher) : base(commonBus)
        {
            _dialogService = dialogService;
            _mController = mController;
            _hTrainAPI = hTrainAPI;
            _visionAPI = vApi;
            _hiveAPI = hApi;
            _wholeAPI = wholeAPI;

            _webService = webService;

            _hTrainAPI.IsConnectEvent -= HyTrainAPI_IsConnectEvent;
            _hTrainAPI.IsConnectEvent += HyTrainAPI_IsConnectEvent;
            _visionAPI.IsConnectEvent -= HyTrainAPI_IsConnectEvent;
            _visionAPI.IsConnectEvent += HyTrainAPI_IsConnectEvent;
            _hiveAPI.IsConnectEvent -= HyTrainAPI_IsConnectEvent;
            _hiveAPI.IsConnectEvent += HyTrainAPI_IsConnectEvent;
            _wholeAPI.IsConnectEvent -= HyTrainAPI_IsConnectEvent;
            _wholeAPI.IsConnectEvent += HyTrainAPI_IsConnectEvent;
            _dispatcher = dispatcher;

            PwdConfirmCommand = new RelayCommand(HandleEnterKey);
            InitData();


        }


        protected override void RegisterEvent(IEventAggregator bus)
        {
            base.RegisterEvent(bus);
            bus.GetEvent<RecipeOpenEvent>().Subscribe(r =>
            {
                InitData();
            });
        }

        /// <summary>
        /// 连接事件
        /// </summary>
        /// <param name="obj"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void HyTrainAPI_IsConnectEvent(object baseMESAPI, bool obj)
        {
            _dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                if (baseMESAPI is HyperTrainAPI h)
                {
                    ConnectStatus = obj;
                }
                else if (baseMESAPI is VisionAPI)
                {
                    IsVisionConnect = obj;
                }
                else if (baseMESAPI is HiveAPI)
                {
                    IsHiveConnect = obj;
                }
                else if (baseMESAPI is WholeLineAPI)
                {
                    IsHiveConnect = obj;
                }
            }));
        }

        #region 属性
        /// <summary>
        /// 是否处于连接状态
        /// </summary>
        private bool _isConnectStatus = false;
        public bool ConnectStatus
        {
            get { return _isConnectStatus; }
            set
            {
                SetProperty(ref _isConnectStatus, value);
            }
        }

        /// <summary>
        /// 是否保存
        /// </summary>
        private bool _hyperTrainEnabled = false;
        public bool HyperTrainEnabled
        {
            get { return _hyperTrainEnabled; }
            set
            {
                SetProperty(ref _hyperTrainEnabled, value);
                webConfig.HyperTrainEnabled = value;
            }
        }

        private DelegateCommand<object> _changeCommand;
        public DelegateCommand<object> ChangeCommand => _changeCommand = new DelegateCommand<object>((obj) =>
        {
            if (obj is ToggleButton btn && btn.IsChecked == true)
            {
                try
                {
                    _hTrainAPI.StartService(_mController, ServerIP, ServerPort);
                }
                catch (Exception)
                {
                    btn.IsChecked = false;
                    throw;
                }

                webConfig.HyperTrainEnabled = btn.IsChecked.Value;
            }
            else
            {
                webConfig.HyperTrainEnabled = false;
                _hTrainAPI.StopService();

            }

            commonBus.IsNeedSave = true;
        });

        /// <summary>
        /// 是否保存
        /// </summary>
        private bool _isSave = false;
        public bool IsSave
        {
            get { return _isSave; }
            set
            {
                SetProperty(ref _isSave, value);
                commonBus.IsNeedSave = value;
            }
        }



        /// <summary>
        /// 设备SN
        /// </summary>
        private string _sfcURL;
        public string SFCUrl
        {
            get { return _sfcURL; }
            set
            {
                SetProperty(ref _sfcURL, value);
                webConfig.SFCUrl = value;
                SFCHelper.SetConfig(webConfig);
            }
        }

        /// <summary>
        /// subcmd
        /// </summary>
        private string _subcmd = "asy_4_sfc";
        public string subcmd
        {
            get { return _subcmd; }
            set
            {
                SetProperty(ref _subcmd, value);
                webConfig.subcmd = value;
                SFCHelper.SetConfig(webConfig);
            }
        }

        /// <summary>
        /// 文件路径
        /// </summary>
        private string _fileDirectory;
        public string FileDirectory
        {
            get { return _fileDirectory; }
            set
            {
                SetProperty(ref _fileDirectory, value);
                webConfig.FileDirectory = value;

            }
        }

        /// <summary>
        /// 设备SN
        /// </summary>
        private string _machineSn;
        public string MachineSn
        {
            get { return _machineSn; }
            set
            {
                SetProperty(ref _machineSn, value);
                webConfig.MachineSn = value;
            }
        }

        /// <summary>
        /// 设备名称
        /// </summary>
        public string _machineName;
        public string MachineName
        {
            get { return _machineName; }
            set
            {
                SetProperty(ref _machineName, value);
                webConfig.MachineName = value;
            }
        }

        /// <summary>
        /// 设备类型
        /// </summary>
        public string _machineType;
        public string MachineType
        {
            get { return _machineType; }
            set
            {
                SetProperty(ref _machineType, value);
                webConfig.MachineType = value;
            }
        }

        /// <summary>
        /// 机种
        /// </summary>
        public string _product;
        public string Product
        {
            get { return _product; }
            set
            {
                SetProperty(ref _product, value);
                webConfig.Product = value;
                SFCHelper.SetConfig(webConfig);
            }
        }

        /// <summary>
        /// Vision机种
        /// </summary>
        public string _product_Vision;
        public string Product_Vision
        {
            get { return _product_Vision; }
            set
            {
                SetProperty(ref _product_Vision, value);
                webConfig.Product_Vision = value;
            }
        }

        /// <summary>
        /// 厂区
        /// </summary>
        public string _iteCode;
        public string SiteCode
        {
            get { return _iteCode; }
            set
            {
                SetProperty(ref _iteCode, value);
                webConfig.SiteCode = value;
            }
        }
        /// <summary>
        /// 卷料批次名称
        /// </summary>
        public string _partName;
        public string PartName
        {
            get { return _partName; }
            set
            {
                SetProperty(ref _partName, value);
                webConfig.PartName = value;
                SFCHelper.SetConfig(webConfig);
            }
        }

        /// <summary>
        /// 卷料批次名称
        /// </summary>
        public string _category_key;
        public string category_key
        {
            get { return _category_key; }
            set
            {
                SetProperty(ref _category_key, value);
                webConfig.category_key = value;
                SFCHelper.SetConfig(webConfig);
            }
        }
        /// <summary>
        /// 区域
        /// </summary>
        public string _area;
        public string Area
        {
            get { return _area; }
            set
            {
                SetProperty(ref _area, value);
                webConfig.Area = value;
            }
        }

        /// <summary>
        /// 楼层
        /// </summary>
        public string _floor;
        public string Floor
        {
            get { return _floor; }
            set
            {
                SetProperty(ref _floor, value);
                webConfig.Floor = value;
            }
        }

        /// <summary>
        /// 线体
        /// </summary>
        public string _lineCode;
        public string LineCode
        {
            get { return _lineCode; }
            set
            {
                SetProperty(ref _lineCode, value);
                webConfig.LineCode = value;
                SFCHelper.SetConfig(webConfig);
            }
        }

        /// <summary>
        /// 单元
        /// </summary>
        public string _nniteCode;
        public string UniteCode
        {
            get { return _nniteCode; }
            set
            {
                SetProperty(ref _nniteCode, value);
                webConfig.UniteCode = value;
            }
        }

        /// <summary>
        /// 工站名称
        /// </summary>
        public string _stationName;
        public string StationName
        {
            get { return _stationName; }
            set
            {
                SetProperty(ref _stationName, value);
                webConfig.StationName = value;
                SFCHelper.SetConfig(webConfig);
            }
        }

        /// <summary>
        /// 工站类型
        /// </summary>
        public string _stationType;
        public string StationType
        {
            get { return _stationType; }
            set
            {
                SetProperty(ref _stationType, value);
                webConfig.StationType = value;
            }
        }

        /// <summary>
        /// 工站ID
        /// </summary>
        public string _stationId;
        public string StationId
        {
            get { return _stationId; }
            set
            {
                SetProperty(ref _stationId, value);
                webConfig.StationId = value;
                SFCHelper.SetConfig(webConfig);
            }
        }

        /// <summary>
        /// 设备厂商
        /// </summary>
        public string _vendorName;
        public string VendorName
        {
            get { return _vendorName; }
            set
            {
                SetProperty(ref _vendorName, value);
                webConfig.VendorName = value;
            }
        }

        /// <summary>
        /// IP地址
        /// </summary>
        public string _deviceIP;
        public string DeviceIP
        {
            get { return _deviceIP; }
            set
            {
                SetProperty(ref _deviceIP, value);
                webConfig.DeviceIP = value;
            }
        }

        /// <summary>
        /// Mac地址
        /// </summary>
        public string _deviceMac;
        public string DeviceMac
        {
            get { return _deviceMac; }
            set
            {
                SetProperty(ref _deviceMac, value);
                webConfig.DeviceMac = value;
                SFCHelper.SetConfig(webConfig);
            }
        }

        /// <summary>
        /// Insight工站
        /// </summary>
        public string _insightType;
        public string InsightType
        {
            get { return _insightType; }
            set
            {
                SetProperty(ref _insightType, value);
                webConfig.InsightType = value;
            }
        }


        /// <summary>
        /// 视觉版本
        /// </summary>
        public string _visionVersion;
        public string VisionVersion
        {
            get { return _visionVersion; }
            set
            {
                SetProperty(ref _visionVersion, value);
                webConfig.VisionVersion = value;
            }
        }

        /// <summary>
        /// 机器人版本
        /// </summary>
        public string _robotVersion;
        public string RobotVersion
        {
            get { return _robotVersion; }
            set
            {
                SetProperty(ref _robotVersion, value);
                webConfig.RobotVersion = value;
            }
        }


        public string _plcVersion;
        public string PlcVersion
        {
            get { return _plcVersion; }
            set
            {
                SetProperty(ref _plcVersion, value);
                webConfig.PlcVersion = value;
            }
        }

        public string _reciepeVersion;
        public string ReciepeVersion
        {
            get { return _reciepeVersion; }
            set
            {
                SetProperty(ref _reciepeVersion, value);
                webConfig.ReciepeVersion = value;
            }
        }

        /// <summary>
        /// 激光版本
        /// </summary>
        public string _laserVersion;
        public string LaserVersion
        {
            get { return _laserVersion; }
            set
            {
                SetProperty(ref _laserVersion, value);
                webConfig.LaserVersion = value;
            }
        }


        /// <summary>
        /// limits版本
        /// </summary>
        public string _limitsVersion;
        public string LimitsVersion
        {
            get { return _limitsVersion; }
            set
            {
                SetProperty(ref _limitsVersion, value);
                webConfig.LimitsVersion = value;
            }
        }



        /// <summary>
        /// Bali版本
        /// </summary>
        public string _baliVersion;
        public string BaliVersion
        {
            get { return _baliVersion; }
            set
            {
                SetProperty(ref _baliVersion, value);
                webConfig.BaliVersion = value;
            }
        }

        /// <summary>
        /// 最终软件版本
        /// </summary>
        public string _softVersion;
        public string SoftVersion
        {
            get { return _softVersion; }
            set
            {
                SetProperty(ref _softVersion, value);
                webConfig.SoftVersion = value;
            }
        }
        /// <summary>
        /// VisionApi版本
        /// </summary>
        public string _apiVersion;
        public string ApiVersion
        {
            get { return _apiVersion; }
            set
            {
                SetProperty(ref _apiVersion, value);
                webConfig.ApiVersion = value;
            }
        }




        /// <summary>
        /// 服务器IP地址
        /// </summary>
        public string _serverIP;
        public string ServerIP
        {
            get { return _serverIP; }
            set
            {
                SetProperty(ref _serverIP, value);
                webConfig.ServerIP = value;
            }
        }

        /// <summary>
        /// Hive App Id
        /// </summary>
        public string _hiveAppId;
        public string HiveAppId
        {
            get { return _hiveAppId; }
            set
            {
                SetProperty(ref _hiveAppId, value);
                webConfig.HiveAppId = value;
            }
        }

        /// <summary>
        /// 图片路径
        /// </summary>
        public string _imagePath;
        public string ImagePath
        {
            get { return _imagePath; }
            set
            {
                SetProperty(ref _imagePath, value);
                webConfig.ImagePath = value;
            }
        }



        /// <summary>
        /// 服务器Port
        /// </summary>
        public int _serverPort;
        public int ServerPort
        {
            get { return _serverPort; }
            set
            {
                SetProperty(ref _serverPort, value);
                webConfig.ServerPort = value;
            }
        }

        /// <summary>
        /// ftp用户名
        /// </summary>
        public string _ftpUser;
        public string FtpUser
        {
            get { return _ftpUser; }
            set
            {
                SetProperty(ref _ftpUser, value);
                webConfig.FtpUser = value;
            }
        }

        /// <summary>
        /// ftp用户密码
        /// </summary>
        public string _ftpPwd;
        public string FtpPwd
        {
            get { return _ftpPwd; }
            set
            {
                SetProperty(ref _ftpPwd, value);
                webConfig.FtpPwd = value;
            }
        }


        #endregion

        #region Vision系统

        /// <summary>
        /// 服务器IP地址
        /// </summary>
        public string _visionIP = "172.10.4.158";
        public string VisionIP
        {
            get { return _visionIP; }
            set
            {
                SetProperty(ref _visionIP, value);
                webConfig.VisionIP = value;
            }
        }

        /// <summary>
        /// 服务器Port
        /// </summary>
        public int _visionPort = 8088;
        public int VisionPort
        {
            get { return _visionPort; }
            set
            {
                SetProperty(ref _visionPort, value);
                webConfig.VisionPort = value;
            }
        }

        /// <summary>
        /// 是否处于连接状态
        /// </summary>
        private bool _isVisionConnect = false;
        public bool IsVisionConnect
        {
            get { return _isVisionConnect; }
            set
            {
                SetProperty(ref _isVisionConnect, value);
            }
        }

        /// <summary>
        /// 是否保存
        /// </summary>
        private bool _visionEnabled = false;
        public bool VisionEnabled
        {
            get { return _visionEnabled; }
            set
            {
                SetProperty(ref _visionEnabled, value);
                webConfig.VisionEnabled = value;
            }
        }

        /// <summary>
        /// vision日志打印
        /// </summary>
        private bool _visionLog = true;
        public bool VisionLog
        {
            get { return _visionLog; }
            set
            {
                SetProperty(ref _visionLog, value);
                webConfig.VisionLog = value;
            }
        }


        /// <summary>
        /// Vision StationId
        /// </summary>
        public string _visionStationId;
        public string VisionStationId
        {
            get { return _visionStationId; }
            set
            {
                SetProperty(ref _visionStationId, value);
                webConfig.VisionStationId = value;
            }
        }


        private DelegateCommand<object> _changeDoorCommand;
        public DelegateCommand<object> ChangeDoorCommand => _changeDoorCommand = new DelegateCommand<object>((obj) =>
        {
            if (obj is ToggleButton btn && btn.IsChecked == true)
            {
                try
                {
                }
                catch (Exception)
                {
                    btn.IsChecked = false;
                    throw;
                }
                finally
                {
                    webConfig.IsShieldDoor = btn.IsChecked.Value;
                }
            }
            else
            {
                webConfig.IsShieldDoor = false;
            }

            commonBus.IsNeedSave = true;
        });
        private DelegateCommand<object> _changeVisionCommand;
        public DelegateCommand<object> ChangeVisionCommand => _changeVisionCommand = new DelegateCommand<object>((obj) =>
        {
            if (obj is ToggleButton btn && btn.IsChecked == true)
            {
                try
                {
                    _visionAPI.StartService(_mController, VisionIP, VisionPort);
                    //注册一次，可能会有需要修改配置，配置完，重新注册一次
                    //_visionAPI.Register();
                }
                catch (Exception)
                {
                    btn.IsChecked = false;
                    throw;
                }
                finally
                {
                    webConfig.VisionEnabled = btn.IsChecked.Value;
                }
            }
            else
            {
                webConfig.VisionEnabled = false;
                _visionAPI.StopService();
            }

            commonBus.IsNeedSave = true;
        });


        public ICommand PwdConfirmCommand { get; set; }
        #endregion

        #region Hive系统

        /// <summary>
        /// 服务器IP地址
        /// </summary>
        public string _hiveIP = "10.0.0.2";
        public string HiveIP
        {
            get { return _hiveIP; }
            set
            {
                SetProperty(ref _hiveIP, value);
                webConfig.HiveIP = value;
            }
        }


        /// <summary>
        /// 是否忽略hive范阔
        /// </summary>
        private bool _isHiveIgnoreFeedBack = false;
        public bool IsHiveIgnoreFeedBack
        {
            get { return _isHiveIgnoreFeedBack; }
            set
            {
                SetProperty(ref _isHiveIgnoreFeedBack, value);
                webConfig.IsHiveIgnoreFeedBack = value;
            }
        }

        /// <summary>
        /// 服务器Port
        /// </summary>
        public int _hivePort;
        public int HivePort
        {
            get { return _hivePort; }
            set
            {
                SetProperty(ref _hivePort, value);
                webConfig.HivePort = value;
            }
        }



        /// <summary>
        /// 是否处于连接状态
        /// </summary>
        private bool _isHiveConnect = false;
        public bool IsHiveConnect
        {
            get { return _isHiveConnect; }
            set
            {
                SetProperty(ref _isHiveConnect, value);

            }
        }

        /// <summary>
        /// 是否保存
        /// </summary>
        private bool _hiveEnabled = false;
        public bool HiveEnabled
        {
            get { return _hiveEnabled; }
            set
            {
                SetProperty(ref _hiveEnabled, value);

                if (HiveIsVisibility == System.Windows.Visibility.Visible)
                    webConfig.HiveEnabled = value;
                HiveButtonEnabled = false;
            }
        }


        private System.Windows.Visibility hiveIsVisibility = System.Windows.Visibility.Hidden;
        public System.Windows.Visibility HiveIsVisibility
        {
            get { return hiveIsVisibility; }
            set
            {
                SetProperty(ref hiveIsVisibility, value);
            }
        }

        /// <summary>
        /// Vision补充
        /// </summary>
        private string _visionExtra;
        public string VisionExtra
        {
            get { return _visionExtra; }
            set
            {
                SetProperty(ref _visionExtra, value);
                webConfig.VisionExtra = value;
            }
        }

        /// <summary>
        /// Vision管理部门
        /// </summary>
        private string _manageDept_Vision;
        public string ManageDept_Vision
        {
            get { return _manageDept_Vision; }
            set
            {
                SetProperty(ref _manageDept_Vision, value);
                webConfig.ManageDept_Vision = value;
            }
        }


        /// <summary>
        /// Vision功能部门
        /// </summary>
        private string _functionId;
        public string FunctionId
        {
            get { return _functionId; }
            set
            {
                SetProperty(ref _functionId, value);
                webConfig.FunctionId = value;
            }
        }

        /// <summary>
        /// Vision注册类型汇总
        /// </summary>
        private List<string> _registerTypes;
        public List<string> RegisterTypes
        {
            get { return _registerTypes; }
            set
            {
                SetProperty(ref _registerTypes, value);

            }
        }


        /// <summary>
        /// Vision注册类型
        /// </summary>
        private string _registerType;
        public string RegisterType
        {
            get { return _registerType; }
            set
            {
                SetProperty(ref _registerType, value);
                webConfig.RegisterType = value;
            }
        }

        /// <summary>
        /// Vision工站名称
        /// </summary>
        private string _stationName_Vision;
        public string StationName_Vision
        {
            get { return _stationName_Vision; }
            set
            {
                SetProperty(ref _stationName_Vision, value);
                webConfig.StationName_Vision = value;
            }
        }


        /// <summary>
        /// 是否保存
        /// </summary>
        private bool _hiveButtonEnabled = false;
        public bool HiveButtonEnabled
        {
            get { return _hiveButtonEnabled; }
            set
            {
                SetProperty(ref _hiveButtonEnabled, value);
            }
        }


        /// <summary>
        /// 密码
        /// </summary>
        private string _password;
        public string Password
        {
            get { return _password; }
            set
            {
                SetProperty(ref _password, value);
                if (value == LusterPwd)
                {
                    HiveButtonEnabled = true;
                }
            }
        }


        /// <summary>
        /// Hive阀门
        /// </summary>
        private bool _isHiveValveEnabled = false;
        public bool IsHiveValveEnabled
        {
            get { return _isHiveValveEnabled; }
            set
            {
                SetProperty(ref _isHiveValveEnabled, value);
                webConfig.IsHiveValveEnabled = value;
                HiveButtonEnabled = value;
            }
        }
        /// <summary>
        /// Hive阀门
        /// </summary>
        private bool _isShieldDoor = false;
        public bool IsShieldDoor
        {
            get { return _isShieldDoor; }
            set
            {
                SetProperty(ref _isShieldDoor, value);
                webConfig.IsShieldDoor = value;
            }
        }


        //sfc 配置文件路径
        private string ghFilePath = "Config\\gh_station_info.json";




        private DelegateCommand<object> _changeHiveCommand;
        public DelegateCommand<object> ChangeHiveCommand => _changeHiveCommand = new DelegateCommand<object>((obj) =>
        {
            if (obj is ToggleButton btn && btn.IsChecked == true)
            {
                try
                {
                    _hiveAPI.StartService(_mController, HiveIP, HivePort);
                }
                catch (Exception)
                {
                    btn.IsChecked = false;
                    throw;
                }
                finally
                {
                    webConfig.HiveEnabled = btn.IsChecked.Value;
                }
            }
            ///不允许停止Hive
            else
            {
                webConfig.HiveEnabled = false;
                webConfig.HiveIsVisibility = false;
                HiveIsVisibility = System.Windows.Visibility.Hidden;
                IsHiveConnect = false;
                _hiveAPI.StopService();
            }

            commonBus.IsNeedSave = true;
        });



        private DelegateCommand<object> _updateSFCInfoCommand;
        public DelegateCommand<object> UpdateSFCInfoCommand => _updateSFCInfoCommand = new DelegateCommand<object>((obj) =>
        {

            string gh_stationPath = commonBus.ProjInfo.ProjPath;
            gh_stationPath = Path.Combine(gh_stationPath, ghFilePath);
            //1.判断文件是否存在，如果不存在，提示操作方式
            if (!File.Exists(gh_stationPath))
            {
                throw new Exception(gh_stationPath + "文件不存在" + ",请从macmini的valut/data_collection/test_station_config/gh_station_info.json路径下拷贝");
            }

            //2.读取对应文件中的信息
            using (FileStream fileStream = new FileStream(gh_stationPath, FileMode.Open, FileAccess.Read))
            using (StreamReader reader = new StreamReader(fileStream))
            {
                string strJson = reader.ReadToEnd();
                var ob = Common.Tools.JsonTool.ToObject<ghStationInfo>(strJson);

                if (ob != null)
                {
                    //3.截取文件中关键信息
                    //4.更新
                    if (!string.IsNullOrEmpty(ob.ghinfo.STATION_ID))
                    {
                        StationId = ob.ghinfo.STATION_ID;
                    }
                    if (!string.IsNullOrEmpty(ob.ghinfo.STATION_TYPE))
                    {
                        StationName = ob.ghinfo.STATION_TYPE;
                    }
                    if (!string.IsNullOrEmpty(ob.ghinfo.PRODUCT))
                    {
                        Product = ob.ghinfo.PRODUCT;
                    }
                    if (!string.IsNullOrEmpty(ob.ghinfo.SITE))
                    {
                        SiteCode = ob.ghinfo.SITE;
                    }
                }
            }
        });
        #endregion

        #region 整线系统
        /// <summary>
        /// 服务器IP地址
        /// </summary>
        public string _wholeIP = "127.0.0.1";
        public string WholeIP
        {
            get { return _wholeIP; }
            set
            {
                SetProperty(ref _wholeIP, value);
                webConfig.WholeIP = value;
            }
        }

        /// <summary>
        /// 服务器Port
        /// </summary>
        public int _wholePort;
        public int WholePort
        {
            get { return _wholePort; }
            set
            {
                SetProperty(ref _wholePort, value);
                webConfig.WholePort = value;
            }
        }

        /// <summary>
        /// 是否保存
        /// </summary>
        private bool _WholeEnabled = false;
        public bool WholeEnabled
        {
            get { return _WholeEnabled; }
            set
            {
                SetProperty(ref _WholeEnabled, value);
                webConfig.WholeLineEnabled = value;
            }
        }







        private DelegateCommand<object> _changeWholeCommand;
        public DelegateCommand<object> ChangeWholeCommand => _changeWholeCommand ?? (_changeWholeCommand = new DelegateCommand<object>((obj) =>
        {
            if (obj is ToggleButton btn && btn.IsChecked == true)
            {
                try
                {
                    _wholeAPI.StartService(_mController, WholeIP, WholePort);
                }
                catch (Exception)
                {
                    btn.IsChecked = false;
                    throw;
                }
                finally
                {
                    webConfig.WholeLineEnabled = btn.IsChecked.Value;
                }
            }
            else
            {
                webConfig.WholeLineEnabled = false;
                _wholeAPI.StopService();
            }

            commonBus.IsNeedSave = true;
        }));

        #endregion

        #region FFU相关
        /// <summary>
        /// FFU速度等级
        /// </summary>
        private string ffuSpeedLevel;
        public string FFUSpeedLevel
        {
            get { return ffuSpeedLevel; }
            set
            {
                SetProperty(ref ffuSpeedLevel, value);
                webConfig.FFUSpeedLevel = value;
            }
        }


        /// <summary>
        /// FFU速度种类汇总
        /// </summary>
        private List<string> ffuSpeedModes;
        public List<string> FFUSpeedModes
        {
            get { return ffuSpeedModes; }
            set
            {
                SetProperty(ref ffuSpeedModes, value);
            }
        }


        /// <summary>
        /// 低速度模式电流值下限
        /// </summary>
        private double lowCurrentLowLimit;
        public double LowCurrentLowLimit
        {
            get { return lowCurrentLowLimit; }
            set
            {
                SetProperty(ref lowCurrentLowLimit, value);
                webConfig.LowCurrentLowLimit = value;
            }
        }

        /// <summary>
        /// 低速度模式电流值上限
        /// </summary>
        private double lowCurrentUpperLimit;
        public double LowCurrentUpperLimit
        {
            get { return lowCurrentUpperLimit; }
            set
            {
                SetProperty(ref lowCurrentUpperLimit, value);
                webConfig.LowCurrentUpperLimit = value;
            }
        }

        /// <summary>
        /// 中速度模式电流值下限
        /// </summary>
        private double middleCurrentLowLimit;
        public double MiddleCurrentLowLimit
        {
            get { return middleCurrentLowLimit; }
            set
            {
                SetProperty(ref middleCurrentLowLimit, value);
                webConfig.MiddleCurrentLowLimit = value;
            }
        }


        /// <summary>
        /// 中速度模式电流值上限
        /// </summary>
        private double middleCurrentUpperLimit;
        public double MiddleCurrentUpperLimit
        {
            get { return middleCurrentUpperLimit; }
            set
            {
                SetProperty(ref middleCurrentUpperLimit, value);
                webConfig.MiddleCurrentUpperLimit = value;
            }
        }

        /// <summary>
        /// 高速度模式电流值下限
        /// </summary>
        private double highCurrentLowLimit;
        public double HighCurrentLowLimit
        {
            get { return highCurrentLowLimit; }
            set
            {
                SetProperty(ref highCurrentLowLimit, value);
                webConfig.HighCurrentLowLimit = value;
            }
        }
        /// <summary>
        /// 高速度模式电流值上限
        /// </summary>
        private double highCurrentUpperLimit;

        public double HighCurrentUpperLimit
        {
            get { return highCurrentUpperLimit; }
            set
            {
                SetProperty(ref highCurrentUpperLimit, value);
                webConfig.HighCurrentUpperLimit = value;
            }
        }


        #endregion


        private void InitData()
        {
            // 获取当前的配置信息
            webConfig = _webService.GetConfig() as WebConfig;
            if (webConfig == null || webConfig.IsEmpty())
            {
                HiveIP = "10.0.0.2";
                HivePort = 10001;
                VisionIP = "172.10.4.158";

                // 保持对旧版本兼容
                var sysConfig = Path.Combine(commonBus.ProjInfo.GetProjConfigDir(), "WebConfig.xml"); // SystemConfig.xml
                if (File.Exists(sysConfig))
                {
                    _webService.LoadConfig(sysConfig);

                    webConfig = _webService.GetConfig() as WebConfig;
                }
            }

            if (webConfig != null)
            {
                MachineSn = webConfig.MachineSn ?? "";
                MachineName = webConfig.MachineName ?? "";
                MachineType = webConfig.MachineType ?? "";
                Product = webConfig.Product ?? "";
                Product_Vision = webConfig.Product_Vision ?? "";
                SiteCode = webConfig.SiteCode ?? "";
                Area = webConfig.Area ?? "";
                Floor = webConfig.Floor ?? "";
                LineCode = webConfig.LineCode ?? "";
                UniteCode = webConfig.UniteCode ?? "1";
                StationName = webConfig.StationName ?? "";
                StationId = webConfig.StationId ?? "";
                StationType = webConfig.StationType ?? "";
                VendorName = webConfig.VendorName ?? "";
                DeviceIP = string.IsNullOrEmpty(GetVisionIPAddress(webConfig.StationName_Vision, webConfig.UniteCode)) ? webConfig.DeviceIP : GetVisionIPAddress(webConfig.StationName_Vision, webConfig.UniteCode);

                DeviceMac = string.IsNullOrEmpty(GetVisionMacAddress(webConfig.StationName_Vision, webConfig.UniteCode)) ? webConfig.DeviceMac : GetVisionMacAddress(webConfig.StationName_Vision, webConfig.UniteCode);
                ServerIP = webConfig.ServerIP ?? "";
                ServerPort = webConfig.ServerPort == 0 ? 0 : webConfig.ServerPort;
                HyperTrainEnabled = webConfig.HyperTrainEnabled;
                InsightType = webConfig.InsightType;
                VisionVersion = webConfig.VisionVersion != "" ? webConfig.VisionVersion : "10.01";
                RobotVersion = webConfig.RobotVersion != "" ? webConfig.RobotVersion : "NN.NN";
                PlcVersion = webConfig.PlcVersion != "" ? webConfig.PlcVersion : "NN.NN";
                ReciepeVersion = webConfig.ReciepeVersion != "" ? webConfig.ReciepeVersion : "NN.NN";
                LaserVersion = webConfig.LaserVersion != "" ? webConfig.LaserVersion : "NN.NN";
                LimitsVersion = webConfig.LimitsVersion != "" ? webConfig.LimitsVersion : "2.13";
                BaliVersion = webConfig.BaliVersion != "" ? webConfig.BaliVersion : "b4.12.1";
                SoftVersion = webConfig.SoftVersion != "" ? webConfig.SoftVersion : "Luster-03.07-10.01-NN.NN-NN.NN-250506";
                HiveAppId = webConfig.HiveAppId != "" ? webConfig.HiveAppId : "28b0e64d-e17a-4d2b-a6f6-6ff5b464ee22";
                ImagePath = webConfig.ImagePath != "" ? webConfig.ImagePath : "E://GVIMAGES";
                ApiVersion = webConfig.ApiVersion != "" ? webConfig.ApiVersion : "V1.0.1";
                ManageDept_Vision = webConfig.ManageDept_Vision != "" ? webConfig.ManageDept_Vision : "HC";
                FunctionId = webConfig.FunctionId != "" ? webConfig.FunctionId : "FATP";
                RegisterType = webConfig.RegisterType != "" ? webConfig.RegisterType : "ADD";
                StationName_Vision = webConfig.StationName_Vision;
                VisionStationId = webConfig.VisionStationId;

                IsHiveValveEnabled = webConfig.IsHiveValveEnabled;
                IsShieldDoor = webConfig.IsShieldDoor;
                PartName = webConfig.PartName;
                category_key = webConfig.category_key;

                // Vision 系统的端口
                VisionEnabled = webConfig.VisionEnabled;
                VisionIP = webConfig.VisionIP;
                VisionPort = webConfig.VisionPort;
                VisionExtra = webConfig.VisionExtra;
                VisionLog = webConfig.VisionLog;



                // Hive 系统的端口
                HiveIsVisibility = webConfig.HiveIsVisibility ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;
                HiveEnabled = webConfig.HiveEnabled;
                HiveIP = webConfig.HiveIP;
                HivePort = webConfig.HivePort;
                IsShieldDoor = webConfig.IsShieldDoor;
                IsHiveIgnoreFeedBack = webConfig.IsHiveIgnoreFeedBack;
                // FirstMode 系统的端口
                WholeEnabled = webConfig.WholeLineEnabled;
                WholeIP = webConfig.WholeIP;
                WholePort = webConfig.WholePort;

                FtpPwd = webConfig.FtpPwd;
                FtpUser = webConfig.FtpUser;
                commonBus.IsNeedSave = false;


                // 配置FileDirectory
                FileDirectory = webConfig.FileDirectory;

                // 配置SFC 的URL
                SFCUrl = webConfig.SFCUrl;
                subcmd = webConfig.subcmd;

                //注册类型添加
                RegisterTypes = new List<string>();
                RegisterTypes.Clear();
                RegisterTypes.Add("ADD");
                RegisterTypes.Add("UPDATE");



                //注册类型添加
                RegisterTypes = new List<string>();
                RegisterTypes.Clear();
                RegisterTypes.Add("ADD");
                RegisterTypes.Add("UPDATE");

                //FFU
                FFUSpeedModes = new List<string>();
                FFUSpeedModes.Clear();
                FFUSpeedModes.Add("Low");
                FFUSpeedModes.Add("Middle");
                FFUSpeedModes.Add("High");

                FFUSpeedLevel = webConfig.FFUSpeedLevel != "" ? webConfig.FFUSpeedLevel : "Low"; ;
                LowCurrentLowLimit = webConfig.LowCurrentLowLimit;
                LowCurrentUpperLimit = webConfig.LowCurrentUpperLimit;
                MiddleCurrentLowLimit = webConfig.MiddleCurrentLowLimit;
                MiddleCurrentUpperLimit = webConfig.MiddleCurrentUpperLimit;
                HighCurrentLowLimit = webConfig.HighCurrentLowLimit;
                HighCurrentUpperLimit = webConfig.HighCurrentUpperLimit;
            }
        }

        /// <summary>
        /// 获取Mac
        /// </summary>
        /// <returns></returns>
        private string GetMacAddress()
        {
            try
            {
                string mac = "";
                ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    if ((bool)mo["IPEnabled"] == true)
                    {
                        mac = mo["MacAddress"].ToString();
                        break;
                    }
                }
                moc = null;
                mc = null;
                return mac;
            }
            catch
            {
                return "unknow";
            }
            finally
            {
            }

        }



        /// <summary>
        /// 获取Vision Mac地址
        /// </summary>
        /// <returns></returns>
        private string GetVisionMacAddress(string stationName, string siteCode)
        {
            string macAddress = "";
            CsNetWorkParam NetWorkParameter = new CsNetWorkParam();
            List<Network> LNetworkInformation = NetWorkParameter.GetNetworkInformation();

            foreach (Network network in LNetworkInformation)
            {
                if (network.Name.ToUpper() == "VISION" + stationName + siteCode)
                {
                    macAddress = network.Mac;
                    break;
                }
            }
            return macAddress;
        }


        /// <summary>
        /// 获取Vision IP地址
        /// </summary>
        /// <returns></returns>
        private string GetVisionIPAddress(string stationName, string siteCode)
        {
            string ip = "";
            CsNetWorkParam NetWorkParameter = new CsNetWorkParam();
            List<Network> LNetworkInformation = NetWorkParameter.GetNetworkInformation();

            foreach (Network network in LNetworkInformation)
            {
                if (network.Name.ToUpper() == "VISION" + stationName + siteCode)
                {
                    ip = network.Ip.ToString();
                    break;
                }
            }
            return ip;
        }





        #region 保存
        protected override void BindGlobalCommnads()
        {
            GlobalCommands.SaveCommand.RegisterCommand(SaveCommand);
        }


        private DelegateCommand _saveCommand;
        public DelegateCommand SaveCommand => _saveCommand ?? (_saveCommand = new DelegateCommand(() =>
        {
            commonBus.OnSaveSystem();
        }));



        /// <summary>
        /// 设备注册
        /// </summary>
        private DelegateCommand<object> _machineRegisterCommand;
        public DelegateCommand<object> MachineRegisterCommand => _machineRegisterCommand ?? (_machineRegisterCommand = new DelegateCommand<object>((o) =>
        {
            if ((string)o == "Hive")
            {
                _hiveAPI.Register();
            }
            else if ((string)o == "HyperTrain")
            {

            }
            //郑州厂区采用ALink注册设备，根据Vision的SiteCode内容,判断是否启用Vision注册
            else if (((string)o == "Vision") && (Product != "D94"))
            {
                _visionAPI.StartService(_mController, webConfig.VisionIP, webConfig.VisionPort);
                _visionAPI.Register();
            }
            else if (((string)o == "Vision") && (SiteCode == "FXZZ") && (Product == "D94"))
            {
                MessageBox.Show("郑州厂区不用Vision注册!");
                //是否需要加return;
                return;
            }
        }));

        /// <summary>
        /// 选择ALink生成的CSV文件
        /// </summary>
        private DelegateCommand _selectFileCommand;
        public DelegateCommand SelectFileCommand => _selectFileCommand ?? (_selectFileCommand = new DelegateCommand(() =>
        {
            // 创建一个OpenFileDialog实例
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // 设置对话框的一些属性
            openFileDialog.Filter = "所有文件 (*.*)|*.*"; // 设置文件过滤器
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            // 打开对话框，并获取用户选择的文件
            if (openFileDialog.ShowDialog() == true)
            {
                // 获取选择的文件名
                string fileName = openFileDialog.FileName;
                // 处理文件，例如显示文件内容或进行其他操作
                FileDirectory = fileName;
            }
        }));

        /// <summary>
        /// 更新驾驶舱的配置内容
        /// </summary>
        private DelegateCommand _updateContentCommand;
        public DelegateCommand UpdateContentCommand => _updateContentCommand ?? (_updateContentCommand = new DelegateCommand(() =>
        {
            //存储CSV内容的中转列表
            List<string[]> csvList = new List<string[]>();

            //存储CSV内容的字典
            Dictionary<String, String> csvDictionary = new Dictionary<string, string>();

            //1、读取文件路径下的CSV文件
            if (FileDirectory != null)
            {
                String csvFilePath = FileDirectory;
                //2、解析CSV文件,暂存到列表数组中           
                using (var reader = new StreamReader(csvFilePath))
                {
                    while (!reader.EndOfStream)
                    {
                        var values = reader.ReadLine().Split(',');
                        csvList.Add(values);
                    }
                }
            }
            else
            {
                MessageBox.Show("文件路径为空，重新选择文件!");
                return;
            }

            //获取CSV数组行数和列数
            int csvFileRow = csvList.Count;
            int csvFileColum = csvList[0].Count();

            //将CSV数组中需要的元素放到字典中
            for (int i = 1; i < csvFileRow; i++)
            {
                //将CSV的索引枚舉名稱和Value放到字典中               
                csvDictionary.Add(csvList[i][1], csvList[i][4]);
            }

            //3、根据CSV文件更新驾驶舱Vision的配置内容           
            csvDictionary.TryGetValue("machineSn", out String tempMachineSn);
            MachineSn = tempMachineSn;
            csvDictionary.TryGetValue("machineName", out String tempMachineName);
            MachineName = tempMachineName;
            csvDictionary.TryGetValue("machineType", out String tempMachineType);
            MachineType = tempMachineType;
            csvDictionary.TryGetValue("functionId", out String tempFunctionId);
            FunctionId = tempFunctionId;
            csvDictionary.TryGetValue("manageDept", out String tempManageDept);
            ManageDept_Vision = tempManageDept;
            csvDictionary.TryGetValue("registerType", out String tempRegisterType);
            RegisterType = tempRegisterType;
            csvDictionary.TryGetValue("product", out String tempProduct);
            Product = Product_Vision = tempProduct;
            csvDictionary.TryGetValue("siteCode", out String tempSiteCode);
            SiteCode = tempSiteCode;
            csvDictionary.TryGetValue("area", out String tempArea);
            Area = tempArea;
            csvDictionary.TryGetValue("floor", out String tempFloor);
            Floor = tempFloor;
            csvDictionary.TryGetValue("lineCode", out String tempLineCode);
            LineCode = tempLineCode;
            csvDictionary.TryGetValue("uniteCode", out String tempUniteCode);
            UniteCode = string.IsNullOrEmpty(tempUniteCode) ? "1" : tempUniteCode;
            csvDictionary.TryGetValue("stationName", out String tempStationName);
            StationName = StationName_Vision = tempStationName;
            csvDictionary.TryGetValue("stationType", out String tempStationType);
            InsightType = StationType = tempStationType;
            csvDictionary.TryGetValue("stationId", out String tempStationId);
            StationId = VisionStationId = tempStationId;
            csvDictionary.TryGetValue("vendorName", out String tempVendorName);
            VendorName = tempVendorName;
            csvDictionary.TryGetValue("deviceIp", out String tempDeviceIp);
            DeviceIP = tempDeviceIp;
            csvDictionary.TryGetValue("deviceMac", out String tempDeviceMac);
            DeviceMac = tempDeviceMac;
            //csvDictionary.TryGetValue("product", out String tempOsVersion);
            //OsVersion = tempOsVersion;
            //csvDictionary.TryGetValue("siteCode", out String tempAppVersion);
            //AppVersion = tempAppVersion;
            //csvDictionary.TryGetValue("area", out String tempConfigVersion);
            //ConfigVersion = tempConfigVersion;
            //csvDictionary.TryGetValue("from", out String tempFrom);
            //From = tempFrom;
            csvDictionary.TryGetValue("apiVersion", out String tempApiVersion);
            ApiVersion = tempApiVersion;
            //csvDictionary.TryGetValue("addBy", out String tempAddBy);
            //AddBy = tempAddBy;
            //csvDictionary.TryGetValue("editBy", out String tempEditBy);
            //EditBy = tempEditBy;
            //csvDictionary.TryGetValue("addDate", out String tempAddDate);
            //AddDate = tempAddDate;
            //csvDictionary.TryGetValue("editDate", out String tempEditDate);
            //EditDate = tempEditDate;


            //把vision注册按钮屏蔽
            //if(tempSiteCode == "FXZZ")
            //{
            //    MachineRegisterCommand;
            //}

        }));

        #endregion


        /// <summary>
        /// 
        /// </summary>
        class ghStationInfo
        {
            public System_message system_Message { get; set; }
            public APP_HASH aPP_HASH { get; set; }
            public Ghinfo ghinfo { get; set; }
            public Parachute parachute { get; set; }
            public Dc_routesItem dc_RoutesItem { get; set; }
            public Controlbits controlbits { get; set; }
            public Groundhog_day groundhog_Day { get; set; }
            public Attr_dictionaryItem attr_DictionaryItem { get; set; }
            public Root root { get; set; }
        }

        public class System_message
        {
        }

        public class APP_HASH
        {
        }

        public class Ghinfo
        {
            /// <summary>
            /// 
            /// </summary>
            public string IMPORTANT { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string SITE { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string PRODUCT { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string BUILD_STAGE { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string BUILD_SUBSTAGE { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string REMOTE_ADDR { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string LOCATION { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string LINE_NUMBER { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string LINE_ID { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string LINE_NAME { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string LINE_TYPE { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string LINE_MANAGER_IP { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string STATION_NUMBER { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string STATION_TYPE { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string DISPLAY_NAME { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string FDR_STATION_TYPE { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string DEPARTMENT { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string STATION_TYPE_DISPLAY { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string SCREEN_COLOR { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string STATION_IP { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string SECURITY_OVERRIDE { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string DCS_IP { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string PDCA_IP { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string KOMODO_IP { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int KOMODO_ID { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string ACTIVATION_IP { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string NTP_IP { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string NTP2_IP { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string SPIDERCAB_IP { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string FUSING_IP { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string DROPBOX_IP { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string DEMO_IP { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string SFC_IP { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string SFC_URL { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string PROV_IP { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string RAT_IP { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string ALDER_IP { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string JMETDS_IP { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string MARINA_IP { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string LINECONTROLLER_IP { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string MATCHMAKER_IP { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string SKYNET_IP { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string TTRS_IP { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string DRS_IP { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string SCPAIR_IP { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string SCPROV_IP { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string THOR_IP { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string GARFIELD_IP { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string NYQUIST_IP { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string MSI_IP { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string DATE_TIME { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int TIMESTAMP { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string STATION_ID { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string URI_CONFIG_HOST { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string URI_CONFIG_PATH { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string GROUNDHOG_IP { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string MAC { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string GHLS_VERSION { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string SAVE_BRICKS { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string LOCAL_CSV { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string REALTIME_PARAMETRIC { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string SINGLE_CSV_UUT { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string BOBCAT_DIRECT { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string RAT_DIRECT { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string FIREWALL { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string USB_STORAGE { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string RAFT_LINE { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string CONTROL_RUN { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string CONTROL_RUN_CB { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string GDADMIN { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string SEND_BLOBS_ON_FAIL_ONLY { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string SEND_BLOBS_ON { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string TESTING_DISK_LIMIT_ON_OFF { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string BOBCAT_BLOBS { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string EXPORT_BLOB_FROM_FACTORY { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string TORTUGA_DIRECT { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string CRITICAL_TO_SAFETY { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int SFC_MAX_FAIL { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string MAX_PDATA_PRIORITY { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string SFC_QUERY_UNIT_ON_OFF { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string STATION_SET_CONTROL_BIT_ON_OFF { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string CONTROL_BITS_TO_CHECK_ON_OFF { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string FDR_TO_CHECK_ON_OFF { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string CONTROL_BITS_TO_CLEAR_ON_PASS_ON_OFF { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string CONTROL_BITS_TO_CLEAR_ON_FAIL_ON_OFF { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string FDR_REGISTRATION_ON_OFF { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int SFC_TIMEOUT { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int FERRET_NOT_RUNNING_TIMEOUT { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int NETWORK_NOT_OK_TIMEOUT { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int GHSI_LASTUPDATE_TIMEOUT { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int FERRET_TIMEOUT { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int TIME_NOT_OK_TIMEOUT { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int MAX_PDATA_TESTS_ALLOWED { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int MAX_BLOB_SIZE_ALLOWED_IN_BYTES { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int MAX_NUMBER_BLOBS { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int TESTING_DISK_LIMIT_IN_MB { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int OVERALL_BLOB_SIZE_ALLOWED_IN_MBS { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int SIGNATURE_EXPIRATION { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string SIGNATURE_VERSION { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string TIMEZONE { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string FDRUCA_IP { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string FDRDS_IP { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string FDRUSS_IP { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string FDR_REGISTRATION { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string GH_STATION_NAME { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string GROUNDHOG_NAT_IP { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string LAST_RESTORED { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string GROUNDHOG_STARTED { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string LIVE_VERSION { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string LIVE_CURRENT { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string LIVE_SCHEDULED { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string LIVE_VERSION_PATH { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string STATION_OVERLAY { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string STATION_OSX_IMAGE { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string STATION_BASE_OVERLAY { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string STATION_CORE_OVERLAY { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string STATION_SUPPLEMENTAL_OVERLAY { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string STATION_FDR_OVERLAY { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public System_message system_message { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public APP_HASH APP_HASH { get; set; }
        }

        public class Parachute
        {
            /// <summary>
            /// 
            /// </summary>
            public int IP_MSG_ERROR_NETWORK_NOT_RESPONDING { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int IP_MSG_ERROR_ETHERNET_NOT_RESPONDING { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int IP_MSG_ERROR_INVALID_STATION_TYPE { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int IP_MSG_ERROR_INVALID_SW_VERSION { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int IP_MSG_ERROR_INVALID_IP_VERSION { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int IP_MSG_ERROR_INVALID_FERRET_VERSION { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int IP_MSG_ERROR_FERRET_NOT_RUNNING { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int IP_MSG_ERROR_DCS_NOT_RESPONDING { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int IP_MSG_ERROR_PDCA_NOT_RESPONDING { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int IP_MSG_ERROR_SFC_QUERY_UNIT { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int IP_MSG_ERROR_INVALID_SERIAL_NUMBER { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int IP_MSG_ERROR_API_UNHANDLED { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int IP_MSG_ERROR_API_SYNTAX { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int IP_MSG_ERROR_FILESYSTEM { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int IP_MSG_ERROR_UNIT_OUT_OF_PROCESS { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int IP_MSG_ERROR_INVALID_DATA_FORMAT { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int IP_MSG_ERROR_INVALID_SIGNATURE { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int IP_MSG_ERROR_TIMESERVER_NOT_RESPONDING { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int IP_MSG_ERROR_SUPPLANT_APICALL { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int IP_MSG_ERROR_NOT_ENOUGH_HARDISK_SPACE { get; set; }
        }

        public class Dc_routesItem
        {
            /// <summary>
            /// 
            /// </summary>
            public string type { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int priority { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string URI { get; set; }
        }

        public class Controlbits
        {
            /// <summary>
            /// 
            /// </summary>
            public int version { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public List<string> CONTROL_BITS_TO_CHECK { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public List<string> CONTROL_BITS_TO_CLEAR_ON_PASS { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public List<string> CONTROL_BITS_TO_CLEAR_ON_FAIL { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public List<string> CONTROL_BITS_STATION_NAMES { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int STATION_FAIL_COUNT_ALLOWED { get; set; }
        }

        public class Groundhog_day
        {
            /// <summary>
            /// 
            /// </summary>
            public int event_timestamp { get; set; }
        }

        public class Attr_dictionaryItem
        {
            /// <summary>
            /// 
            /// </summary>
            public string bobcat_alias { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public int length { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string attr_name { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string attr_type { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string type { get; set; }
        }

        public class Root
        {
            /// <summary>
            /// 
            /// </summary>
            public Ghinfo ghinfo { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public Parachute parachute { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public List<Dc_routesItem> dc_routes { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public Controlbits controlbits { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public Groundhog_day groundhog_day { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public List<Attr_dictionaryItem> attr_dictionary { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string signature { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string digest { get; set; }
        }

        /// <summary>
        /// 处理密码回车
        /// </summary>
        private void HandleEnterKey()
        {
            if (Password == LusterPwd)
            {
                HiveButtonEnabled = true;
                Password = "";
            }
        }



        public struct Network
        {
            public string Name;
            public string Mac;
            public IPAddress Ip;
        };

        /// <summary>
        /// 获取网络适配器信息
        /// </summary>
        public class CsNetWorkParam
        {
            //使用方法：声明+调用


            public List<Network> GetNetworkInformation()
            {
                //存储集合声明
                List<Network> LNetwork = new List<Network>();

                NetworkInterface[] InterfacesInformation = NetworkInterface.GetAllNetworkInterfaces();//获取网络接口所有信息 包含未启用的网卡

                foreach (NetworkInterface networkInterface in InterfacesInformation)//遍历所有网络接口信息
                {
                    bool bIsOpen = networkInterface.OperationalStatus == OperationalStatus.Up;//判断网络连接状态
                    bool bIsLoopback = networkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback;//是否是 回环
                                                                                                              //Loopback       回环127.0.0.1
                                                                                                              //Wireless80211  无线网 
                                                                                                              //Ethernet       以太网
                    if (bIsOpen && !bIsLoopback)
                    {
                        //单个网卡声明
                        Network network = new Network();

                        //获取网卡名
                        network.Name = networkInterface.Name;

                        //获取ip
                        IPInterfaceProperties ipInterfaceProperties = networkInterface.GetIPProperties();//获取ip信息 包含ipv4和ipv6
                        foreach (UnicastIPAddressInformation ipAddressInformation in ipInterfaceProperties.UnicastAddresses)
                        {
                            if (ipAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork)//判断是否符合ipv4
                            {
                                network.Ip = ipAddressInformation.Address;
                            }
                        }

                        //获取MAC
                        PhysicalAddress mac = networkInterface.GetPhysicalAddress();
                        string result = Regex.Replace(mac.ToString(), ".{2}", "$0:");//添加字符 .{插入位置} ${数组位置 默认0}{插入字符}
                        //string strMac = result.Remove(result.Length - 1);
                        // 如果字符串只有冒号或为空，就不要 Remove
                        string strMac = result.Length > 0 ? result.Remove(result.Length - 1) : result;
                        network.Mac = strMac;

                        LNetwork.Add(network);
                    }
                }

                return LNetwork;
            }
        }

    }
}
