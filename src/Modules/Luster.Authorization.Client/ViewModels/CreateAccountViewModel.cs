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

namespace DC.Authorization.WPF.ViewModels
{
    public class CreateAccountViewModel : BindableBase, IDialogAware
    {
        private int? _limitation;
        private ObservableCollection<Role> _roles = new ObservableCollection<Role>();
        //private DialogCloseListener _requestClose = new DialogCloseListener();
        private IAccountRepository _accountRepository;
        private Role? _selectedRole;
        private DelegateCommand _saveCommand;
        private string _name; 
        private string _department; 
        private string _cardID; 
        private string _username; 
        private string _password; 
        private string _checkedPassword;
        private bool _isEdit;
        public CreateAccountViewModel(IAccountRepository accountRepository, IRoleRepository roleRepository)
        {
            _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
            _roles.AddRange(roleRepository.Load());
        }

        public event Action<IDialogResult> RequestClose;

        public string Title { get; set; } = "新建账户";
        public ICommand SaveCommand => _saveCommand ??= new DelegateCommand(Save);
        public Role? SelectedRole { get => _selectedRole; set { SetProperty(ref _selectedRole, value); } }
        
        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public string Department { get => _department; set => SetProperty(ref _department, value); }
        public string CardID { get => _cardID; set => SetProperty(ref _cardID, value); }
        public int? Limitation { get => _limitation; set => SetProperty(ref _limitation, value); }

        private Account _inputAccount;

        public string Username { get => _username; set => SetProperty(ref _username, value); }
        public string Password { get => _password; set => SetProperty(ref _password, value); }
        public string CheckedPassword { get => _checkedPassword; set => SetProperty(ref _checkedPassword, value); }
        public ObservableCollection<Role> Roles { get => _roles; set => SetProperty(ref _roles, value); }
        //public DialogCloseListener RequestClose => _requestClose;

        private void Save()
        {
            if (_isEdit) Edit();
            else Create();
        }

        private void Create()
        {
            var result = Validate();
            if (!result.Item1)
            {
                MessageBox.Show(string.Join("\r\n", result.Item2), "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (_accountRepository.AccountNameExists(Username))
            {
                MessageBox.Show($"用户名 \"{Username}\" 已存在，请更换！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            SetControlValues(_inputAccount);
            _accountRepository.Create(_inputAccount);
            MessageBox.Show("创建成功", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            RequestClose.Invoke(new DialogResult(ButtonResult.OK));
        }

        private void Edit()
        {
            var result = Validate();
            if (!result.Item1)
            {
                MessageBox.Show(string.Join("\r\n", result.Item2), "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (_accountRepository.AccountNameExists(Username, _inputAccount.Id))
            {
                MessageBox.Show($"用户名 \"{Username}\" 已存在，请更换！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            SetControlValues(_inputAccount);
            _accountRepository.Update(_inputAccount);
            MessageBox.Show("保存成功", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            RequestClose.Invoke(new DialogResult(ButtonResult.OK));
        }
        private Account SetControlValues(Account account)
        {
            account.RoleId = SelectedRole == null ? throw new ArgumentNullException("角色不能为空") : SelectedRole.Id;
            account.RoleName = SelectedRole.Name;
            account.AccName = Username;
            account.RealName = Name;
            account.AccPassword = Password;
            account.Department = Department;
            account.TelNo = CardID;
            account.SessionExpireMin = Limitation ?? 10;
            return account;
        }
        
        private (bool, List<string>) Validate()
        {
            List<string> infos = new List<string>();
            bool result = true;
            if (string.IsNullOrEmpty(Name?.Trim())) { infos.Add("姓名不能为空！"); result = false; }
            if (SelectedRole == null) { infos.Add("角色不能为空！"); result = false; }
            if (string.IsNullOrEmpty(Username?.Trim())) { infos.Add("用户名不能为空！"); result = false; }
            if (string.IsNullOrEmpty(Password?.Trim())) { infos.Add("密码不能为空！"); result = false; }
            if (string.IsNullOrEmpty(Department?.Trim())) { infos.Add("部门不能为空！"); result = false; }
            if (string.IsNullOrEmpty(CardID?.Trim())) { infos.Add("刷卡ID不能为空！"); result = false; }
            if (Limitation == null) { infos.Add("时限不能为空！"); result = false; }
            if (Limitation == 0) { infos.Add("时限不能为0！"); result = false; }
            if (result && !IsEdit)
            {
                if (Password != CheckedPassword) { infos.Add("两次输入密码不一致！"); result = false; }
            }
            return (result, infos);
        }

        public bool CanCloseDialog() => true;
        public void OnDialogClosed() { }
        public void OnDialogOpened(IDialogParameters parameters)
        {
            Account account = parameters.GetValue<Account>("param") ?? throw new ArgumentException($"请传入一个不为null的{nameof(Account)}对象！");
            IsEdit = parameters.GetValue<bool>("edit");
            SelectedRole = account.AccName == null ? null : Roles.Where(x => x.Id == account.RoleId).FirstOrDefault();
            Username = account.AccName;
            Name = account.RealName;
            Password = account.AccPassword;
            Department = account.Department;
            CardID = account.TelNo;
            Limitation = account.SessionExpireMin;
            _inputAccount = account;
        }
        public bool IsEdit { get => _isEdit; set => SetProperty(ref _isEdit, value); }


        private DelegateCommand _cancelCommand;
        /// <summary>右上角关闭按钮 → 取消对话框</summary>
        public ICommand CancelCommand => _cancelCommand ??= new DelegateCommand(() =>
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
        });
    }
}
