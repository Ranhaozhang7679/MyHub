using DC.Authorization;
using DC.Authorization.Models;
using DC.Authorization.WPF.Infrastructure;
using Luster.Authorization.Client.Helper;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace DC.Authorization.WPF.ViewModels
{
    public class ChangePasswordViewModel : BindableBase, IDialogAware
    {
        private DelegateCommand saveCommand;
        //private DialogCloseListener _requestClose = new DialogCloseListener();
        private ILoginService _loginService;
        private IAccountRepository _accountRepository;

        public ChangePasswordViewModel(ILoginService loginService, IAccountRepository accountRepository)
        {
            _loginService = loginService;
            _accountRepository = accountRepository;
        }

        public event Action<IDialogResult> RequestClose;

        public string Title { get; set; } = "修改密码";
        //public DialogCloseListener RequestClose => _requestClose;

        public bool CanCloseDialog() => true;
        public void OnDialogClosed() { }
        public void OnDialogOpened(IDialogParameters parameters) { }

        private string _oldPassword;
        public string OldPassword { get => _oldPassword; set => SetProperty(ref _oldPassword, value); }

        private string _newPassword;
        public string NewPassword { get => _newPassword; set => SetProperty(ref _newPassword, value); }

        private string _checkedNewPassword;
        public string CheckedNewPassword { get => _checkedNewPassword; set => SetProperty(ref _checkedNewPassword, value); }

        private DelegateCommand _cancelCommand;
        /// <summary>右上角关闭按钮 → 取消对话框</summary>
        public ICommand CancelCommand => _cancelCommand ??= new DelegateCommand(() =>
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
        });

        public ICommand SaveCommand => saveCommand ??= new DelegateCommand(Save);

        private void Save()
        {
            var warnings = Validate();
            if (warnings.Count > 0)
            {
                MessageBox.Show(string.Join("\r\n", warnings), "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            _loginService.Current!.AccPassword = _newPassword;
            _accountRepository.Update(_loginService.Current);
            _loginService.Current.AccPassword = NewPassword;
            MessageBox.Show("保存成功", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            RequestClose.Invoke(new DialogResult(ButtonResult.OK));
        }
        private List<string> Validate()
        {
            var warnings = new List<string>();
            if (string.IsNullOrEmpty(OldPassword?.Trim()))
            {
                warnings.Add("旧密码不能为空！");
            }
            if (string.IsNullOrEmpty(NewPassword?.Trim()))
            {
                warnings.Add("新密码不能为空！");
            }
            if (string.IsNullOrEmpty(CheckedNewPassword?.Trim()))
            {
                warnings.Add("确认密码不能为空！");
            }
            if (warnings.Count == 0)
            {
                if (DbConfig.CalcPwdMd5(OldPassword) != _loginService.Current?.AccPassword)
                {
                    warnings.Add("旧密码输入错误！");
                }
                if (NewPassword != CheckedNewPassword)
                {
                    warnings.Add("确认密码不一致！");
                }
            }
            return warnings;
        }
    }
}
