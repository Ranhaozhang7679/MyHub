using HandyControl.Controls;
using LiveCharts.Dtos;
using Luster.Common.DataStruct;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Extensions;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.CommonUI.Models;
using Luster.Motion.CommonUI.ViewModel;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.Enums;
using Luster.Motion.Integration.SFC;
using Luster.Motion.Integration.WorkCardVerify;
using Luster.Motion.TaskFlow.Engine;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Luster.Motion.SubSystem.ViewModel
{
    public class LoginContentFXVM : MotionPageVM, INavigationAware
    {
        /// <summary>
        /// 总线,用于传递解决方案的目录
        /// </summary>
        private readonly ICommonBus _commonBus;
        /// <summary>
        /// 用于读取配置,以及切换平台在线离线模式
        /// </summary>
        private readonly IMotionController _mController;
        /// <summary>
        /// 切换平台在线离线模式
        /// </summary>
        private readonly IDeviceEngine _deviceEngine;
        private readonly IAuthService _authService;

        private UserInfo _userInfo;
        private UserModel _userModel;

        public LoginContentFXVM(ICommonBus commonBus,
                                IMotionController mController,
                                IDeviceEngine deviceEngine,
                                IAuthService authService) : base(commonBus)
        {
            this._commonBus = commonBus;
            this._mController = mController;
            this._deviceEngine = deviceEngine;
            this._authService = authService;
            ModeList = typeof(DeviceMode).EnumToDataSource();
            BindProject();
            RegisterMuliLang(nameof(ModeList));
            SelectUrl = ListUrl[0];
            _userInfo = new UserInfo();
            _userModel = new UserModel()
            {
                UserName = _userInfo.Role.ToString(),
                UserRole = _userInfo.Role,
            };
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (navigationContext.Parameters.ContainsKey("ProjectEnable"))
            {
                ProjectEnable = navigationContext.Parameters.GetValue<bool>("ProjectEnable");
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;
        public void OnNavigatedFrom(NavigationContext navigationContext) { }

        protected override void RegisterEvent(IEventAggregator bus)
        {
            base.RegisterEvent(bus);
            bus.GetEvent<UserInfoEvent>().Subscribe(userInfo =>
            {
                //_authService?.OnRoleChanged(userInfo);
            });
        }

        
        private bool _projectEnable = true;
        /// <summary>
        /// 选择工程是否可用
        /// </summary>
        public bool ProjectEnable
        {
            get { return _projectEnable; }
            set { SetProperty(ref _projectEnable, value); }
        }

        private string _fXCardID;
        /// <summary>
        /// 登录ID
        /// </summary>
        public string FXCardID
        {
            get { return _fXCardID; }
            set { SetProperty(ref _fXCardID, value); }
        }

        private Visibility _isOffline = Visibility.Collapsed;
        /// <summary>
        /// 是否在线模式,绑定[CardId是否可以编辑,密码是否显示,离线登录勾选是否显示]
        /// </summary>
        public Visibility IsOffline
        {
            get { return _isOffline; }
            set { SetProperty(ref _isOffline, value); }
        }

        private string _fXPassword;
        /// <summary>
        /// 离线登录密码
        /// </summary>
        public string FXPassword
        {
            get { return _fXPassword; }
            set { SetProperty(ref _fXPassword, value); }
        }

        private string _fXRecv;
        /// <summary>
        /// SFC接口返回内容
        /// </summary>
        public string FXRecv
        {
            get { return _fXRecv; }
            set { SetProperty(ref _fXRecv, value); }
        }

        private ProjectInfo _selectProject;
        /// <summary>
        /// 选择的工程
        /// </summary>
        public ProjectInfo SelectProject
        {
            get { return _selectProject; }
            set { SetProperty(ref _selectProject, value); }
        }

        private List<ProjectInfo> _listProject;
        /// <summary>
        /// 所有工程
        /// </summary>
        public List<ProjectInfo> ListProject
        {
            get { return _listProject; }
            set { SetProperty(ref _listProject, value); }
        }

        private string _selectUrl;
        /// <summary>
        /// 当前选择的URL
        /// </summary>
        private string SelectUrl
        {
            get { return _selectUrl; }
            set { SetProperty(ref _selectUrl, value); }
        }

        private ObservableCollection<string> _listUrl = new ObservableCollection<string>() { "http://172.18.240.28/fatpvk", "11.0.0.1" };
        /// <summary>
        /// 不同专案的URL
        /// </summary>
        public ObservableCollection<string> ListUrl
        {
            get { return _listUrl; }
            set { SetProperty(ref _listUrl, value); }
        }

        /// <summary>
        /// 切换离线
        /// </summary>
        public DelegateCommand ToggleOfflineCommand => new DelegateCommand(() =>
        {
            IsOffline = IsOffline == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        });

        /// <summary>
        /// 切换工程
        /// </summary>
        private DelegateCommand<object> _changeProjCommand;
        public DelegateCommand<object> ChangeProjCommand => _changeProjCommand ?? (_changeProjCommand = new DelegateCommand<object>((arg) =>
        {
            var cArgs = arg as SelectionChangedEventArgs;
            if (cArgs.AddedItems.Count > 0)
            {
                SelectProject = cArgs.AddedItems[0] as ProjectInfo;
                _commonBus.ProjInfo = SelectProject;
                _commonBus.EventBus.GetEvent<ProjectNameEvent>().Publish(SelectProject.ProjName);
                ModeChangeByProj();
            }
        }));


        private DeviceMode _selectMode = DeviceMode.Virtual;
        /// <summary>
        /// 当前模式
        /// </summary>
        public DeviceMode SelectMode
        {
            get { return _selectMode; }
            set
            {
                SetProperty(ref _selectMode, value);
            }
        }
        private List<KeyValue> _modeList;
        /// <summary>
        /// 模式列表
        /// </summary>
        public List<KeyValue> ModeList
        {
            get => _modeList; set
            {
                SetProperty(ref _modeList, value);
            }
        }

        /// <summary>
        /// 切换模式
        /// </summary>
        private DelegateCommand<object> _changeModeCommand;
        public DelegateCommand<object> ChangeModeCommand => _changeModeCommand ?? (_changeModeCommand = new DelegateCommand<object>((arg) =>
        {
            var cArgs = arg as SelectionChangedEventArgs;

            if (cArgs.AddedItems.Count > 0)
            {
                var keyVal = cArgs.AddedItems[0] as KeyValue;
                DeviceMode mode = (DeviceMode)keyVal.Value;
                _commonBus.EventBus.GetEvent<DeviceModeChangeEvent>().Publish(mode);
            }
        }));


        private DelegateCommand<object> _changeUrlCommand;
        public DelegateCommand<object> ChangeUrlCommand => _changeUrlCommand ?? (_changeUrlCommand = new DelegateCommand<object>((arg) =>
        {
            var cArgs = arg as SelectionChangedEventArgs;

            if (cArgs.AddedItems.Count > 0)
            {
                var keyVal = cArgs.AddedItems[0].ToString();
                SelectUrl = keyVal;
            }
        }));


        /// <summary>
        /// 登录
        /// </summary>
        private DelegateCommand _loginCommand;

        public DelegateCommand LoginCommand => _loginCommand ?? (_loginCommand = new DelegateCommand(() =>
        {
            Login();
        }));

        protected async void Login()
        {
            if (IsOffline == Visibility.Visible)
            {
                if (string.IsNullOrEmpty(FXCardID))
                {
                    FXRecv = "CardID cannot be null !!!\r\n";
                    return;
                }
                if (string.IsNullOrEmpty(FXPassword))
                {
                    FXRecv = "CardID cannot be null !!!\r\n";
                    return;
                }
                _userInfo = await _authService.OfflineVerifCardyAsync(FXCardID, FXPassword);
                _userModel = new UserModel()
                {
                    UserName = _userInfo.Role.ToString(),
                    UserRole = _userInfo.Role,
                };
            }
            SendUserToOther();
        }

        /// <summary>
        /// 登出
        /// </summary>
        private DelegateCommand _logoutCommand;

        public DelegateCommand LogoutCommand => _logoutCommand ?? (_logoutCommand = new DelegateCommand(() =>
        {
            _userInfo = new UserInfo()
            {
                Name = "None",
                Company = "None",
                CardId = "None",
                Role = SystemRole.Operator,
                //Level = DeviceLevel.L1,
                Level = DeviceLevel.None,
                LogionMsg = "Logout"
            };  
            _userModel = new UserModel()
            {
                UserName = SystemRole.Operator.ToString(),
                UserRole = SystemRole.Operator
            };
            SendUserToOther();
        }));

        private void SendUserToOther()
        {
            commonBus.CurrentUser = _userModel;
            if (SelectProject != null && SelectProject.RecipeList != null)
            {
                _commonBus.ProjInfo = SelectProject;
                var recipe = SelectProject.RecipeList.FirstOrDefault(u => u.IsActive);
                if (recipe != null)
                {
                    commonBus.OnActiveRecipe(recipe);
                }
                else
                {
                    if (SelectProject.RecipeList.Count > 0)
                    {
                        commonBus.OnActiveRecipe(SelectProject.RecipeList[0]);
                    }
                }
                // 记录当前的模式
                _userModel.CurrentMode = SelectMode;
                UpdateMode();
            }
            else
            {
                var proj = PageModel.Pages.FirstOrDefault(u => u.Name == "Project");
                if (proj == null)
                {
                    proj = PageModel.Pages.FirstOrDefault(u => u.Name == "Configure");
                }
                commonBus.OnNavigate(proj);
            }
            commonBus.OnUserLogin(_userModel);
            commonBus.OnUserRoleChange(_userInfo);
            FXCardID = string.Empty;
            FXPassword = string.Empty;
            FXRecv = string.Empty;
        }

        protected virtual void UpdateMode()
        {
            // 保存配置信息
            if (_deviceEngine.DeviceMode == DeviceMode.Empty)
            {
                _mController.SysConfig.RunMode = DeviceMode.Real;
            }
            else
            {
                _mController.SysConfig.RunMode = _deviceEngine.DeviceMode;
            }
        }

        /// <summary>
        /// 临时用于接受界面卡号的字符串
        /// </summary>
        private string tempCardID = string.Empty;

        private DelegateCommand<object> _scanCommand;
        /// <summary>
        /// 刷卡响应事件
        /// </summary>
        public DelegateCommand<object> ScanCommand => _scanCommand ?? (new DelegateCommand<object>(async (obj) =>
        {
            if (IsOffline == Visibility.Visible)
            {
                return;
            }
            //接收刷卡响应消息
            var e = obj as KeyEventArgs;
            if (e != null)
            {
                if (e.Key != Key.Enter)
                {
                    if (e.Key.ToString().Contains('D'))
                    {
                        tempCardID += e.Key.ToString()[1];
                    }
                }
                else
                {
                    if (tempCardID.Length > 10 || tempCardID.Length < 6)
                    {
                        tempCardID = "";
                        return;
                    }
                    //进行报文上报
                    var currentID = tempCardID;
                    if (tempCardID.Substring(0, 1) == "0")
                        tempCardID = tempCardID.Substring(1, tempCardID.Length - 1);
                    FXCardID = tempCardID;
                    _userInfo = await _authService.OnlineVerifyCardAsync(SelectUrl, FXCardID);
                    FXRecv = _userInfo.LogionMsg;
                    _userModel = new UserModel()
                    {
                        UserName = _userInfo.Role.ToString(),
                        UserRole = _userInfo.Role,
                    };
                    if (_userInfo.Level == DeviceLevel.None)
                    {
                        IsOffline = Visibility.Visible;
                    }
                    
                    if (IsOffline != Visibility.Visible)
                    {
                        SendUserToOther();
                    }
                    tempCardID = string.Empty; //清空临时变量 
                }
            }
        }));

        /// <summary>
        /// 获取绑定工程
        /// </summary>
        private void BindProject()
        {
            if (commonBus.ProjInfo != null)
            {
                ListProject = new List<ProjectInfo>(commonBus.ProjectList.ToArray());
                if (ListProject != null && ListProject.Count > 0)
                {
                    var projectInfo = ListProject.FirstOrDefault(u => u.IsActive == true);
                    if (projectInfo != null)
                    {
                        SelectProject = projectInfo;

                    }
                    else
                    {
                        SelectProject = ListProject[0];
                    }
                    ModeChangeByProj();
                }
            }
        }

        private void ModeChangeByProj()
        {
            if (SelectProject != null)
            {
                string sysConfig = Path.Combine(Path.GetDirectoryName(SelectProject.FullName), "Config", "SystemConfig.xml");
                _mController.SysConfig.LoadSysConfig(sysConfig);//加载配置
                if (_mController.SysConfig != null)
                {
                    SelectMode = _mController.SysConfig.RunMode;
                    _commonBus.EventBus.GetEvent<DeviceModeChangeEvent>().Publish(SelectMode);
                }
            }
        }


    }
}
