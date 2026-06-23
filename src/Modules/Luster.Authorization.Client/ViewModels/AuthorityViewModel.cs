using DC.Authorization;
using DC.Authorization.Models;
using DC.Authorization.WPF.Models;
using DC.Authorization.WPF.Services;
using DC.Authorization.WPF.Views;
using DC.Authorization.WPF.Helper;
using Prism.Commands;
using Serilog;
using System.Collections.ObjectModel;
using System.Data.SqlTypes;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Luster.Authorization.Client.Helper;

namespace DC.Authorization.WPF.ViewModels
{
    public class AuthorityViewModel : BindableBase, IDialogAware
    {
        private ObservableCollection<Role> _roles = new ObservableCollection<Role>();
        private ObservableCollection<Role> _rolesForRights = new ObservableCollection<Role>();
        private IAuthorizationFacade _authorizationFacade;
        private IRightRepository _rightRepository;
        private ILoginService _loginService;
        private IRoleRepository _roleRepository;
        private IAccountRepository _accountRepository;
        private ObservableCollection<Account> _accounts = new ObservableCollection<Account>();
        private int _logCurrentPage = 1;
        private IDialogService _dialogService;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IAuthSettingRepository _authSettingRepository;
        //private DialogCloseListener _requestClose = new DialogCloseListener();
        private ObservableCollection<AuditLogListModel> _logs = new ObservableCollection<AuditLogListModel>();
        private List<Right> _rights;
        private List<RightTreeNode> _rightTree = new List<RightTreeNode>();
        private List<RightTreeNode> _visibilityRightTree = new List<RightTreeNode>();
        private readonly ITradDialogService _tradDialogService;
        private readonly AccountImportService _accountImportService;
        private readonly ILogger _logger;
        public AuthorityViewModel(IRoleRepository roleRepository,
            IDialogService dialogService,
            IAccountRepository accountRepository,
            ILoginService loginService,
            IAuditLogRepository auditLogRepository,
            IRightRepository rightRepository,
            IAuthorizationFacade authorizationFacade,
            IAuthSettingRepository authSettingRepository,
            ITradDialogService tradDialogService,
            AccountImportService accountImportService,
            ILogger logger
            )
        {
            _authorizationFacade = authorizationFacade;
            _rightRepository = rightRepository;
            _loginService = loginService;
            _roleRepository = roleRepository;
            _accountRepository = accountRepository;
            _roles.AddRange(_roleRepository.Load()/*.Where(x => !x.IsAdmin)*/);
            _rolesForRights.AddRange(_roles.Where(r => !r.IsAdmin));
            _accounts.AddRange(_accountRepository.Load(false).Where(x => !x.IsAdmin));
            _dialogService = dialogService;
            _auditLogRepository = auditLogRepository;
            _logs.AddRange(_auditLogRepository.Query(new QueryModel()));
            _rights = _rightRepository.Load() ?? throw new SqlNullValueException("查不到任何权限！");
            _authSettingRepository = authSettingRepository;
            _tradDialogService = tradDialogService;
            _accountImportService = accountImportService;
            _logger = logger;

            // 初始构建权限树（无选中状态）
            _rightTree = RightTreeBuilder.Build(_rights.Where(x => x.Type == RightType.Operation).ToList(), new List<int>());
            _visibilityRightTree = RightTreeBuilder.Build(_rights.Where(x => x.Type == RightType.Visibility).ToList(), new List<int>());
            CurrentLogin = _loginService.Current;
            SelectedTabIndex = CurrentLogin == null ? 0 : 1;
        }

        public event Action<IDialogResult> RequestClose;

        private DelegateCommand _cancelCommand;
        /// <summary>右上角关闭按钮 → 关闭权限管理对话框</summary>
        public ICommand CancelCommand => _cancelCommand ??= new DelegateCommand(() =>
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
        });

        public string Title { get; set; } = "权限管理";
        public List<RightTreeNode> RightTree { get => _rightTree; set { SetProperty(ref _rightTree, value); } }
        public List<RightTreeNode> VisibilityRightTree { get => _visibilityRightTree; set { SetProperty(ref _visibilityRightTree, value); } }
        public ObservableCollection<Role> Roles
        {
            get => _roles; set { SetProperty(ref _roles, value); }
        }
        /// <summary>权限配置下拉框使用的角色列表（排除超级管理员）</summary>
        public ObservableCollection<Role> RolesForRights => _rolesForRights;
        public ObservableCollection<Account> Accounts
        {
            get { return _accounts; }
            set { SetProperty(ref _accounts, value); }
        }
        private Account _selectedAccount;

        public Account SelectedAccount { get => _selectedAccount; set => SetProperty(ref _selectedAccount, value); }

        private DelegateCommand accountAddCommand;
        public ICommand AccountAddCommand => accountAddCommand ??= new DelegateCommand(AccountAdd);

        private void AccountAdd()
        {
            Account account = new Account();
            var parameters = new DialogParameters
            {
                { "param", account },
                { "edit", false },
            };
            _dialogService.ShowDialog(nameof(CreateAccountView), parameters, (res) =>
            {
                if (res.Result == ButtonResult.OK)
                {
                    Accounts.Clear();
                    Accounts.AddRange(_accountRepository.Load(false).Where(x => !x.IsAdmin));
                }
            });
        }

        private DelegateCommand accountDeleteCommand;
        public ICommand AccountDeleteCommand => accountDeleteCommand ??= new DelegateCommand(AccountDelete);

        private void AccountDelete()
        {
            var warnings = AccountDeleteValidate();
            if (warnings.Count != 0)
            {
                MessageBox.Show(string.Join("\r\n", warnings), "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var result = MessageBox.Show("确认选中账户删除吗？", "询问", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                _accountRepository.Delete(SelectedAccount);
                MessageBox.Show("删除成功", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                Accounts.Remove(SelectedAccount);
            }
        }

        private List<string> AccountDeleteValidate()
        {
            List<string> warnings = new List<string>();
            if (SelectedAccount.IsAdmin)
            {
                warnings.Add("管理员账户无法删除");
            }
            return warnings;
        }

        private DelegateCommand _accountEditCommand;
        public ICommand AccountEditCommand => _accountEditCommand ??= new DelegateCommand(AccountEdit);

        private void AccountEdit()
        {

            var parameters = new DialogParameters
            {
                { "param", SelectedAccount },
                { "edit", true },
            };
            _dialogService.ShowDialog(nameof(CreateAccountView), parameters, (res) =>
            {
                if (res.Result == ButtonResult.OK)
                {
                    Accounts.Clear();
                    Accounts.AddRange(_accountRepository.Load(false).Where(x => !x.IsAdmin));
                }
            });
        }

        private Role selectedRole;

        public Role SelectedRole { get => selectedRole; set => SetProperty(ref selectedRole, value); }

        private DelegateCommand _roleAddCommand;
        public ICommand RoleAddCommand => _roleAddCommand ??= new DelegateCommand(RoleAdd);

        private void RoleAdd()
        {
            var role = new Role();
            var parameters = new DialogParameters
            {
                { "param", role },
            };
            _dialogService.ShowDialog(nameof(CreateRoleView), parameters, (res) =>
            {
                if (res.Result == ButtonResult.OK)
                {
                }
            });
        }

        private DelegateCommand _roledeleteCommand;
        public ICommand RoledeleteCommand => _roledeleteCommand ??= new DelegateCommand(RoleDelete);

        private void RoleDelete()
        {
            _roleRepository.Delete(SelectedRole.Id);
            _roles.Remove(SelectedRole);
        }

        private DelegateCommand _roleEditCommand;
        public ICommand RoleEditCommand => _roleEditCommand ??= new DelegateCommand(RoleEdit);

        private void RoleEdit()
        {
        }

        private DelegateCommand _saveNewPasswordCommand;
        public ICommand SaveNewPasswordCommand => _saveNewPasswordCommand ??= new DelegateCommand(SaveNewPassword);

        private void SaveNewPassword()
        {
        }

        private string _oldPassword;

        public string OldPassword { get => _oldPassword; set => SetProperty(ref _oldPassword, value); }

        private string _newPassword;

        public string NewPassword { get => _newPassword; set => SetProperty(ref _newPassword, value); }

        private string _checkedNewPassword;

        public string CheckedNewPassword { get => _checkedNewPassword; set => SetProperty(ref _checkedNewPassword, value); }



        public ObservableCollection<AuditLogListModel> Logs { get => _logs; set => SetProperty(ref _logs, value); }

        private DelegateCommand _previousPageCommand;
        public ICommand PreviousPageCommand => _previousPageCommand ??= new DelegateCommand(PreviousPage);

        private void PreviousPage()
        {
            if (LogCurrentPage == 1)
            {
                return;
            }
            QueryModel queryModel = new QueryModel()
            {
                StartTime = _startDate,
                EndTime = endDate,
                PageIndex = LogCurrentPage - 2
            };
            var result = _auditLogRepository.Query(queryModel);
            if (result != null && result.Count > 0)
            {
                Logs.Clear();
                LogCurrentPage--;
                Logs.AddRange(result);
            }
        }

        private DelegateCommand _nextPageCommand;
        public ICommand NextPageCommand => _nextPageCommand ??= new DelegateCommand(NextPage);

        private void NextPage()
        {
            if (Logs.Count < 30)
            {
                return;
            }
            QueryModel queryModel = new QueryModel()
            {
                StartTime = _startDate,
                EndTime = endDate,
                PageIndex = LogCurrentPage
            };
            var result = _auditLogRepository.Query(queryModel);
            if (result != null && result.Count > 0)
            {
                Logs.Clear();
                LogCurrentPage++;
                Logs.AddRange(result);
            }
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {

        }

        public void OnDialogOpened(IDialogParameters parameters)
        {

        }



        public int LogCurrentPage { get => _logCurrentPage; set => SetProperty(ref _logCurrentPage, value); }

        private string _jumpPageText;
        /// <summary>
        /// 页码跳转输入
        /// </summary>
        public string JumpPageText { get => _jumpPageText; set => SetProperty(ref _jumpPageText, value); }

        private DelegateCommand _jumpPageCommand;
        public ICommand JumpPageCommand => _jumpPageCommand ??= new DelegateCommand(JumpPage);

        private void JumpPage()
        {
            if (int.TryParse(JumpPageText, out int targetPage) && targetPage >= 1)
            {
                QueryModel queryModel = new QueryModel()
                {
                    StartTime = _startDate,
                    EndTime = endDate,
                    PageIndex = targetPage - 1
                };
                var result = _auditLogRepository.Query(queryModel);
                if (result == null || result.Count == 0)
                {
                    MessageBox.Show($"第 {targetPage} 页不存在", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    Logs.Clear();
                    LogCurrentPage = targetPage;
                    Logs.AddRange(result);
                }
            }
            JumpPageText = string.Empty;
        }

        //public DialogCloseListener RequestClose => _requestClose;

        private DelegateCommand searchCommand;
        public ICommand SearchCommand => searchCommand ??= new DelegateCommand(Search);

        private void Search()
        {
            var warnings = DateValidate();
            if (warnings.Count != 0)
            {
                MessageBox.Show(string.Join("\r\n", warnings), "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            Logs.Clear();
            QueryModel queryModel = new QueryModel()
            {
                StartTime = _startDate,
                EndTime = endDate,
            };
            Logs.AddRange(_auditLogRepository.Query(queryModel));
            LogCurrentPage = 1;
        }

        private List<string> DateValidate()
        {
            List<string> warning = new List<string>();
            if (_startDate == null)
            {
                warning.Add("开始日期不能为空！");
            }
            if (endDate == null)
            {
                warning.Add("结束日期不能为空！");
            }
            return warning;
        }
        private DateTime? _startDate;

        public DateTime? StartDate { get => _startDate; set => SetProperty(ref _startDate, value); }

        private DateTime? endDate;

        public DateTime? EndDate { get => endDate; set => SetProperty(ref endDate, value); }

        private DelegateCommand changeLoginCommand;
        public ICommand ChangeLoginCommand => changeLoginCommand ??= new DelegateCommand(ChangeLogin);

        private void ChangeLogin()
        {
            _dialogService.ShowDialog(nameof(LoginView), null, (res) =>
            {

            });
        }

        private DelegateCommand saveRightsCommand;
        public ICommand SaveRightsCommand => saveRightsCommand ??= new DelegateCommand(SaveRights);

        private void SaveRights()
        {
            var warnings = RightsValidate();
            if (warnings.Count != 0)
            {
                MessageBox.Show(string.Join("\r\n", warnings), "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var selectedRights = RightTreeBuilder.CollectCheckedRightIds(_rightTree).Concat(RightTreeBuilder.CollectCheckedRightIds(_visibilityRightTree)).ToArray();
            _rightRepository.DeleteRoleRights(_selectedRightRole.Id);
            _roleRepository.Assign(_selectedRightRole.Id, selectedRights);
            MessageBox.Show("保存成功", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private List<string> RightsValidate()
        {
            List<string> warnings = new List<string>();
            if (SelectedRightRole == null)
            {
                warnings.Add("请先选择角色！");
            }

            return warnings;
        }

        private Role? _selectedRightRole;

        public Role? SelectedRightRole
        {
            get => _selectedRightRole; set
            {
                SetProperty(ref _selectedRightRole, value);
                if (SelectedRightRole != null)
                {
                    var roleRights = _roleRepository.LoadRights(SelectedRightRole.Id);
                    RightTree = RightTreeBuilder.Build(_rights.Where(x => x.Type == RightType.Operation).ToList(), roleRights);
                    VisibilityRightTree = RightTreeBuilder.Build(_rights.Where(x => x.Type == RightType.Visibility).ToList(), roleRights);
                }
            }
        }

        private DelegateCommand changePasswordCommand;
        public ICommand ChangePasswordCommand => changePasswordCommand ??= new DelegateCommand(ChangePassword);

        private void ChangePassword()
        {
            if (!_authorizationFacade.CheckAuth(default)) { return; }
            _dialogService.ShowDialog(nameof(ChangePasswordView), null, (res) =>
            {

            });
        }

        private DelegateCommand logoutCommand;
        public ICommand LogoutCommand => logoutCommand ??= new DelegateCommand(Logout);

        private void Logout()
        {
            if (_loginService.Current == null)
            {
                MessageBox.Show("当前尚未登录！"); return;
            }
            _loginService.Logout();
        }

        private bool _rightEditEnable;

        public bool RightEditEnable { get => _rightEditEnable; set => SetProperty(ref _rightEditEnable, value); }

        private int _selectedTabIndex;

        public int SelectedTabIndex
        {
            get => _selectedTabIndex; set
            {
                if (SetProperty(ref _selectedTabIndex, value))
                {
                    if (_selectedTabIndex == 3)//权限分配
                    {
                        //RightEditEnable = _authorizationFacade.CheckAuth(AuthItems.EditRight.Name);
                    }
                }
            }
        }

        private Account _currentLogin;

        public Account CurrentLogin { get => _currentLogin; set => SetProperty(ref _currentLogin, value); }

        private string username;

        public string Username { get => username; set => SetProperty(ref username, value); }

        private string password;

        public string Password { get => password; set => SetProperty(ref password, value); }

        private DelegateCommand loginCommand;
        public ICommand LoginCommand => loginCommand ??= new DelegateCommand(Login);

        private void Login()
        {
            try
            {
                var result = _loginService.Login(Username, Password);
                if (!result.Item1)
                {
                    MessageBox.Show(result.Item2);
                    return;
                }
                CurrentLogin = _loginService.Current;
                SelectedTabIndex = 1;
            }
            catch (ArgumentNullException e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private DelegateCommand importCommand;
        public ICommand ImportCommand => importCommand ??= new DelegateCommand(Import);

        private async void Import()
        {
            var fullPath = _tradDialogService.OpenFileDialog("选择导入文件");
            if (string.IsNullOrEmpty(fullPath)) { return; }
            var message = await _accountImportService.Import(fullPath);
            if (message != null && message.Any())
            { MessageBox.Show($"导入时遇到错误!\r\n{string.Join("\r\n", message)}", "错误", MessageBoxButton.OK, MessageBoxImage.Error); }
            else
            {
                MessageBox.Show("导入成功!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                Accounts.Clear();
                Accounts.AddRange(_accountRepository.Load(false).Where(x => !x.IsAdmin));
            }
        }

        private DelegateCommand exportCommand;
        public ICommand ExportCommand => exportCommand ??= new DelegateCommand(Export);

        private void Export()
        {
            var fullPath = _tradDialogService.SaveFileDialog("选择账户导出目录");
            if (string.IsNullOrEmpty(fullPath)) { return; }
            try
            {
                _accountImportService.Export(fullPath);
            }
            catch (IOException ex)
            {
                _logger.Error(ex, $"导出账号时遇到错误, {ex.Message}");
                MessageBox.Show("导出时遇到错误，请检查文件是否被占用!", "错误");
            }
        }
    }
}
