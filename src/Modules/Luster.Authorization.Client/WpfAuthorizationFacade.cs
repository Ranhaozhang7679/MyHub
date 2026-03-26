using DC.Authorization;
using DC.Authorization.WPF.Services;
using DC.Authorization.WPF.Views;
using Prism.Services.Dialogs;
using System.Windows;

namespace DC.Authorization.WPF
{
    /// <summary>
    /// WPF 特化的权限门面：覆写弹窗为 WPF Dialog
    /// </summary>
    public class WpfAuthorizationFacade : AuthorizationFacade
    {
        private readonly IDialogService _dialogService;

        public WpfAuthorizationFacade(IRightRepository rightRepository, ILoginService loginService,
            IDialogService dialogService, IAuditLogRepository auditLogRepository)
            : base(rightRepository, loginService, auditLogRepository)
        {
            _dialogService = dialogService;
        }

        public override void PopNoAuthNotification(AuthItem authItem)
        {
            MessageBox.Show($"当前无 [{authItem.Module}]-[{authItem.View}]-'{authItem.Operation}' 权限!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public override void PopLoginWindow()
        {
            PopLoginWindowAction?.Invoke();
            //_dialogService.ShowDialog(nameof(LoginView));
        }
    }
}
