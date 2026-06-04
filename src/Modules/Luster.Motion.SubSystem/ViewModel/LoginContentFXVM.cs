using DC.Authorization;
using DC.Authorization.Models;
using DC.Authorization.WPF;
using DC.Authorization.WPF.Infrastructure;
using DC.Authorization.WPF.Providers;
using DocumentFormat.OpenXml.Spreadsheet;
using HandyControl.Controls;
using LiveCharts.Dtos;
using Luster.Authorization.Client.Models;
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
using Prism.Services.Dialogs;
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
using UserInfo = Luster.Motion.Integration.WorkCardVerify.UserInfo;

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
        private readonly ILoginService _loginService;
        private readonly IDialogService _dialogService;
        private readonly DC.Authorization.WPF.Providers.HiveAuthProvider _hiveAuthProvider;
        private readonly LoginConfig _loginConfig;

        private UserInfo _userInfo;
        private UserModel _userModel;

        public LoginContentFXVM(ICommonBus commonBus,
                                IMotionController mController,
                                IDeviceEngine deviceEngine,
                                ILoginService loginService,
                                IDialogService dialogService,
                                IAuthorizationFacade facade,
                                HiveAuthProvider hiveAuthProvider) : base(commonBus, facade)
        {
            this._commonBus = commonBus;
            this._hiveAuthProvider = hiveAuthProvider;
            this._mController = mController;
            this._deviceEngine = deviceEngine;
            this._loginService = loginService;
            this._dialogService = dialogService;
            ModeList = typeof(DeviceMode).EnumToDataSource();
            LoginModeList = typeof(LoginMode).EnumToDataSource();
            LoginLevelList = typeof(SystemRole).EnumToDataSource();
            BindProject();
            RegisterMuliLang(nameof(ModeList));
            SelectUrl = ListUrl[0];
            if (_hiveAuthProvider != null)
            {
                _hiveAuthProvider.HiveApiBaseUrl = SelectUrl;
            }
            _userInfo = new UserInfo();
            _userModel = new UserModel()
            {
                UserName = _userInfo.Role.ToString(),
                UserRole = _userInfo.Role,
            };

            // 加载登录配置（记忆上次的登录模式和等级）
            _loginConfig = new LoginConfig();
            _loginConfig.Load();
            SelectLoginMode = (LoginMode)_loginConfig.LoginMode;
            SelectLoginLevel = (SystemRole)_loginConfig.LoginLevel;
            _loginService.TargetRoleLevel = (int)SelectLoginLevel + 1;

            // 订阅刷卡登录成功事件 → 自动完成登录并广播用户信息
            _loginService.OnCardLogin += LoginService_OnCardLogin;

            // 订阅刷卡状态更新（验证中/失败）→ 刷新 FXRecv 显示
            _loginService.OnCardStatusUpdated += LoginService_OnCardStatusUpdated;
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (navigationContext.Parameters.ContainsKey("ProjectEnable"))
            {
                ProjectEnable = navigationContext.Parameters.GetValue<bool>("ProjectEnable");
            }
            // 进入登录页：开启刷卡监听
            _loginService.LoginAllowed = SelectLoginMode == LoginMode.FXCard;            
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            // 离开登录页：关闭刷卡监听，防止其他页面下误触发
            _loginService.LoginAllowed = false;
        }

        protected override void RegisterEvent(IEventAggregator bus)
        {
            base.RegisterEvent(bus);
            bus.GetEvent<UserInfoEvent>().Subscribe(userInfo =>
            {
                //_authService?.OnRoleChanged(userInfo);
            });
        }

        // ─── 新 Auth 事件处理 ─────────────────────────────────────────────

        /// <summary>
        /// 双击标题 → 打开权限管理界面
        /// </summary>
        private AuthCommand _openAuthorViewCommand;
        public AuthCommand OpenAuthorViewCommand => _openAuthorViewCommand
            ?? (_openAuthorViewCommand = new AuthCommand(Auth, AuthDictionary.ModifyRight, OnOpenAuthorViewCommand));


        [AuthRight(nameof(AuthDictionary.ModifyRight))]
        private void OnOpenAuthorViewCommand()
        {
            if (!Auth.CheckAuth(default)) return;
            _dialogService.ShowDialog(nameof(AuthorityView));
        }

        /// <summary>
        /// 刷卡登录成功 → 构造 UserInfo/UserModel，驱动 SendUserToOther
        /// </summary>
        private void LoginService_OnCardLogin(object? sender, EventArgs e)
        {
            var acc = _loginService.Current;
            if (acc == null) return;

            _userInfo = BuildUserInfoFromAccount(acc);
            _userInfo.Level = UserInfo.ParseDeviceLevel(acc.HiveLevel);
            _userModel = new UserModel
            {
                UserName = _userInfo.Name,
                UserRole = _userInfo.Role,
            };

            FXCardID = _loginService.LastCardNo;
            FXRecv = _loginService.LastAuthMessage;
            IsOffline = System.Windows.Visibility.Collapsed;

            Application.Current.Dispatcher.Invoke(SendUserToOther);
        }

        /// <summary>
        /// 刷卡状态更新（验证中 / 失败）→ 刷新 FXRecv
        /// </summary>
        private void LoginService_OnCardStatusUpdated(object? sender, EventArgs e)
        {
            FXCardID = _loginService.LastCardNo;
            FXRecv = _loginService.LastAuthMessage;
        }

        /// <summary>
        /// 将新 Account 模型映射到旧 UserInfo（供 commonBus 广播使用）
        /// Level 映射：1=Admin, 2=Integrator, 3=Maintenance, 4=Operator
        /// </summary>
        private static UserInfo BuildUserInfoFromAccount(Account acc)
        {
            // 按 RoleName 映射 SystemRole（与 DbInitializer 预设角色名一致）
            var role = acc.RoleName switch
            {
                "Administrator" => SystemRole.Admin,
                "Integrator" => SystemRole.Integrator,
                "Maintenance" => SystemRole.Maintenance,
                _ => SystemRole.Operator   // OP ReadOnly 及未知角色
            };

            return new UserInfo
            {
                Name = acc.RealName ?? acc.AccName,
                Company = acc.Department ?? string.Empty,
                CardId = acc.TelNo,
                Role = role,
                LogionMsg = $"{acc.RoleName} 登录成功"
            };
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

        private Visibility _fXCardModeVisual = Visibility.Visible;
        public Visibility FXCardModeVisual
        {
            get { return _fXCardModeVisual; }
            set { SetProperty(ref _fXCardModeVisual, value); }
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

        private static ObservableCollection<string> GetUrlsFromConfig()
        {
            var urls = new ObservableCollection<string>();
            try
            {
                // 读取 app.config 中的 HiveApiUrls 配置
                var configStr = System.Configuration.ConfigurationManager.AppSettings["HiveApiUrls"];
                if (!string.IsNullOrWhiteSpace(configStr))
                {
                    // 支持使用逗号或分号分割多个URL
                    var items = configStr.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var item in items)
                    {
                        urls.Add(item.Trim());
                    }
                }
            }
            catch
            {
                // 忽略异常，防止在没有System.Configuration引用时的崩溃
            }

            // 如果没配置或者配置为空，给一个默认值
            if (urls.Count == 0)
            {
                urls.Add("http://172.25.3.168/fatpal");
            }
            return urls;
        }

        private ObservableCollection<string> _listUrl = GetUrlsFromConfig();
        /// <summary>
        /// 不同专案的URL
        /// </summary>
        public ObservableCollection<string> ListUrl
        {
            get { return _listUrl; }
            set { SetProperty(ref _listUrl, value); }
        }

        ///// <summary>
        ///// 切换离线
        ///// </summary>
        //public DelegateCommand ToggleOfflineCommand => new DelegateCommand(() =>
        //{
        //    IsOffline = IsOffline == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        //});

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



        private LoginMode _selectLoginMode = LoginMode.FXCard;
        /// <summary>
        /// 当前模式
        /// </summary>
        public LoginMode SelectLoginMode
        {
            get { return _selectLoginMode; }
            set
            {
                SetProperty(ref _selectLoginMode, value);
                // 同步视觉状态
                IsOffline = value == LoginMode.Offline ? Visibility.Visible : Visibility.Collapsed;
                FXCardModeVisual = value == LoginMode.FXCard ? Visibility.Visible : Visibility.Collapsed;
                _loginService.LoginAllowed = value == LoginMode.FXCard;
                // 保存配置
                _loginConfig.LoginMode = (int)value;
                _loginConfig.Save();
            }
        }
        private List<KeyValue> _loginModeList;

        public List<KeyValue> LoginModeList
        {
            get => _loginModeList; set
            {
                SetProperty(ref _loginModeList, value);
            }
        }


        private SystemRole _selectLoginLevel;
        /// <summary>
        /// 当前登录等级
        /// </summary>
        public SystemRole SelectLoginLevel
        {
            get { return _selectLoginLevel; }
            set
            {
                if (!SetProperty(ref _selectLoginLevel, value)) return;
                //int targetLevel = 4 - (int)SelectLoginLevel;
                int targetLevel = (int)SelectLoginLevel + 1;
                _loginService.TargetRoleLevel = targetLevel;
                // 保存配置
                _loginConfig.LoginLevel = (int)value;
                _loginConfig.Save();
            }
        }
        private List<KeyValue> _loginLevelList;

        public List<KeyValue> LoginLevelList
        {
            get => _loginLevelList; set
            {
                SetProperty(ref _loginLevelList, value);
            }
        }


        private DelegateCommand<object> _changeLoginModeCommand;
        public DelegateCommand<object> ChangeLoginModeCommand => _changeLoginModeCommand ?? (_changeLoginModeCommand = new DelegateCommand<object>((arg) =>
        {
            // 视觉状态同步已移至 SelectLoginMode setter，此处仅保留绑定兼容
        }));


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

                if (_hiveAuthProvider != null)
                {
                    _hiveAuthProvider.HiveApiBaseUrl = keyVal;
                }
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

        protected void Login()
        {
            if (IsOffline == Visibility.Visible)
            {
                if (string.IsNullOrEmpty(FXCardID))
                {
                    FXRecv = "账户名不能为空 / Account cannot be null";
                    return;
                }
                if (string.IsNullOrEmpty(FXPassword))
                {
                    FXRecv = "密码不能为空 / Password cannot be null";
                    return;
                }

                int targetLevel = 4 - (int)SelectLoginLevel;
                // 使用新 LoginService 进行本地密码验证
                var (succeeded, message, hiveLevel) = _loginService.Login(FXCardID, FXPassword, targetLevel);
                FXRecv = message;

                if (!succeeded) return;

                // 登录成功：构建 UserInfo/UserModel
                var acc = _loginService.Current!;
                _userInfo = BuildUserInfoFromAccount(acc);
                _userInfo.Level = UserInfo.ParseDeviceLevel(hiveLevel); // 记录 HiveAuth 返回的实际权限等级
                _userModel = new UserModel
                {
                    UserName = _userInfo.Name,
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
            // 通知新权限模块注销
            _loginService.Logout();

            // 重置为默认 Operator 状态，广播到 commonBus
            _userInfo = new UserInfo()
            {
                Name = "None",
                Company = "None",
                CardId = "None",
                Role = SystemRole.Operator,
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
        private string tempCardID = string.Empty; // 已弃用，保留防止子类引用

        private DelegateCommand<object> _scanCommand;
        /// <summary>
        /// 刷卡响应事件（保留绑定，内部逻辑已迁移至 LoginService.GlobalHook）
        /// 新流程：GlobalHook(WH_KEYBOARD_LL) → ProcessCardNoAsync → OnCardLogin 事件 → LoginService_OnCardLogin
        /// </summary>
        public DelegateCommand<object> ScanCommand => _scanCommand ?? (_scanCommand = new DelegateCommand<object>((obj) =>
        {
            // 已由 GlobalHook 全局键盘钩子接管，此处不再处理键盘解析逻辑
            // 保留此命令仅为防止 LoginContentFX.xaml 中的 EventToCommand 绑定报错
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
