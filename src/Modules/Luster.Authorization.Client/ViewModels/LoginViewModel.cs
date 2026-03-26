using DC.Authorization;
using DC.Authorization.Models;
using Luster.Authorization.Client.Helper;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace DC.Authorization.WPF.ViewModels
{
    public class LoginViewModel : BindableBase, IDialogAware
    {
        private readonly ILoginService _loginService;
        private string _password;
        private readonly IAuthSettingRepository _authSettingRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;

        public LoginViewModel(ILoginService loginService, IAuthSettingRepository authSettingRepository,
            IRoleRepository roleRepository, IAccountRepository accountRepository)
        {
            _loginService = loginService;
            _loginService.OnCardLogin += LoginService_OnCardLogin;
            _loginService.OnCardStatusUpdated += LoginService_OnCardStatusUpdated;
            _authSettingRepository = authSettingRepository;
            _roleRepository = roleRepository;
            _accountRepository = accountRepository;
        }

        public event Action<IDialogResult> RequestClose;

        // ─── 刷卡成功 → 更新信息并关闭 ──────────────────────────────────────
        private void LoginService_OnCardLogin(object? sender, EventArgs e)
        {
            _dispatcher.Invoke(async () =>
            {
                CardNo           = _loginService.LastCardNo;
                CardUserName     = _loginService.LastCardUserName;
                CardVendor       = _loginService.LastCardVendor;
                CardLevel        = _loginService.LastCardDeviceLevel;
                MesStatusMessage = _loginService.LastAuthMessage;
                IsCardVerified   = true;
                IsProgressActive = false;

                await Task.Delay(3000);
                RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
            });
            _loginService.OnCardLogin -= LoginService_OnCardLogin;
        }

        // ─── 刷卡状态更新（验证中 / 失败）→ 仅刷新文本，不关闭 ─────────────
        private void LoginService_OnCardStatusUpdated(object? sender, EventArgs e)
        {
            _dispatcher.Invoke(() =>
            {
                CardNo           = _loginService.LastCardNo;
                CardUserName     = _loginService.LastCardUserName;
                CardVendor       = _loginService.LastCardVendor;
                CardLevel        = _loginService.LastCardDeviceLevel;
                MesStatusMessage = _loginService.LastAuthMessage;
                // 验证失败时不关闭，不设 IsCardVerified = true
            });
        }

        public bool CanCloseDialog() => true;

        public void OnDialogOpened(IDialogParameters parameters)
        {
            _loginService.LoginAllowed = true;
            var setting = _authSettingRepository.Query();
            _authSettingRepository.SettingChanged += AuthSettingRepository_SettingChanged;
            OnlyScan = setting.IsOnlyScanType;

            _roles.AddRange(_roleRepository.Load());
            _accountList = _accountRepository.Load(false);

            // 初始化显示状态
            MesStatusMessage = "请选择权限等级后刷卡 / Select level then swipe badge";
            IsCardVerified   = false;
            IsProgressActive = false;
        }

        private List<Account> _accountList = new List<Account>();

        // ─── 角色列表 ────────────────────────────────────────────────────────
        private ObservableCollection<Role> _roles = new ObservableCollection<Role>();
        public ObservableCollection<Role> Roles { get => _roles; }

        private Role _selectedRole;
        public Role SelectedRole
        {
            get => _selectedRole;
            set
            {
                if (!SetProperty(ref _selectedRole, value)) return;

                // 同步 TargetRoleLevel 到 LoginService，刷卡时使用
                _loginService.TargetRoleLevel = value?.Level;

                Accounts = _accountList.Where(acc => acc.RoleId == value.Id).ToList();

                // 切换等级时重置验证状态并提示重新刷卡
                IsCardVerified   = false;
                IsProgressActive = true;
                CardUserName     = string.Empty;
                CardVendor       = string.Empty;
                CardLevel        = string.Empty;
                MesStatusMessage = $"已选择: {value?.Name} — 请刷卡 / Swipe badge to verify";
            }
        }

        private List<Account> _accounts = new List<Account>();
        public List<Account> Accounts { get => _accounts; set => SetProperty(ref _accounts, value); }

        private Account _selectedAccount;
        public Account SelectedAccount { get => _selectedAccount; set => SetProperty(ref _selectedAccount, value); }

        public void OnDialogClosed()
        {
            _loginService.LoginAllowed = false;
            _loginService.OnCardLogin -= LoginService_OnCardLogin;
            _loginService.OnCardStatusUpdated -= LoginService_OnCardStatusUpdated;
            _authSettingRepository.SettingChanged -= AuthSettingRepository_SettingChanged;
        }

        private void AuthSettingRepository_SettingChanged(object? sender, EventArgs e)
        {
            var setting = _authSettingRepository.Query();
            OnlyScan = setting.IsOnlyScanType;
        }

        private bool _onlyScan = false;
        public bool OnlyScan { get => _onlyScan; set => SetProperty(ref _onlyScan, value); }

        // ─── 刷卡后显示的 Hive 用户信息 ─────────────────────────────────────
        private string _cardUserName = string.Empty;
        /// <summary>Hive 返回的姓名（刷卡后显示）</summary>
        public string CardUserName { get => _cardUserName; set => SetProperty(ref _cardUserName, value); }

        private string _cardVendor = string.Empty;
        /// <summary>Hive 返回的厂商/部门（刷卡后显示）</summary>
        public string CardVendor { get => _cardVendor; set => SetProperty(ref _cardVendor, value); }

        private string _cardLevel = string.Empty;
        /// <summary>Hive 返回的设备等级字符串，如 "L8"（刷卡后显示）</summary>
        public string CardLevel { get => _cardLevel; set => SetProperty(ref _cardLevel, value); }

        private string _cardNo = string.Empty;
        /// <summary>刷卡读到的卡号（去前导零后），只读显示。</summary>
        public string CardNo { get => _cardNo; set => SetProperty(ref _cardNo, value); }

        private bool _isCardVerified = false;
        /// <summary>刷卡验证成功标志</summary>
        public bool IsCardVerified { get => _isCardVerified; set => SetProperty(ref _isCardVerified, value); }

        // ─── MES 通信状态 ────────────────────────────────────────────────────
        private string _mesStatusMessage = string.Empty;
        /// <summary>MES 通信状态文本（显示在状态框中）</summary>
        public string MesStatusMessage { get => _mesStatusMessage; set => SetProperty(ref _mesStatusMessage, value); }

        private bool _isProgressActive = false;
        /// <summary>进度条激活（等待 MES 响应时为 true）</summary>
        public bool IsProgressActive { get => _isProgressActive; set => SetProperty(ref _isProgressActive, value); }

        // ─── 离线登录（密码方式）────────────────────────────────────────────
        private bool _isOfflineLogin = false;
        /// <summary>是否启用离线密码登录（显示密码输入框）</summary>
        public bool IsOfflineLogin
        {
            get => _isOfflineLogin;
            set
            {
                if (!SetProperty(ref _isOfflineLogin, value)) return;
                if (value)
                {
                    MesStatusMessage = "离线模式: 请输入登录密码 / Offline mode: Enter password";
                    IsProgressActive = false;
                }
                else
                {
                    MesStatusMessage = SelectedRole != null
                        ? $"已选择: {SelectedRole.Name} — 请刷卡 / Swipe badge to verify"
                        : "请选择权限等级后刷卡 / Select level then swipe badge";
                    IsProgressActive = SelectedRole != null;
                    Password = string.Empty;
                }
            }
        }

        private string _title = "Settings Login";
        public string Title { get => _title; set => SetProperty(ref _title, value); }
        //public DialogCloseListener RequestClose => _requestClose;

        private DelegateCommand _loginCommand;
        public ICommand LoginCommand => _loginCommand ??= new DelegateCommand(Login);

        private DelegateCommand _cancelCommand;
        /// <summary>右上角关闭按钮 → 取消对话框</summary>
        public ICommand CancelCommand => _cancelCommand ??= new DelegateCommand(() =>
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
        });

        private List<string> Validate()
        {
            var res = new List<string>();
            if (SelectedRole == null)
            {
                res.Add("请先选择权限等级 / Please select a permission level");
                return res;
            }
            if (IsOfflineLogin)
            {
                // 离线密码登录
                if (SelectedAccount == null || string.IsNullOrWhiteSpace(SelectedAccount.AccName))
                    res.Add("请选择账户 / Please select an account");
                if (string.IsNullOrWhiteSpace(Password))
                    res.Add("密码不能为空 / Password cannot be empty");
            }
            else
            {
                // 刷卡登录
                if (!IsCardVerified)
                    res.Add("请先刷卡验证身份 / Please swipe badge first");
            }
            return res;
        }

        private void Login()
        {
            var valRes = Validate();
            if (valRes.Count > 0)
            {
                MessageBox.Show(string.Join("\r\n", valRes), "验证提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (IsOfflineLogin)
            {
                var result = _loginService.Login(SelectedAccount.AccName, Password);
                if (!result.Succeeded)
                {
                    MessageBox.Show(result.Message, "登录失败", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            // 刷卡成功已由 LoginService_OnCardLogin 自动处理关闭
            RequestClose.Invoke(new DialogResult());
        }

        public string Password { get => _password; set => SetProperty(ref _password, value); }
    }
}
