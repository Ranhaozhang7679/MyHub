using DC.Authorization;
using DC.Authorization.Models;
using DC.Authorization.WPF.Infrastructure;
using System;
using System.Text;
using System.Text.Json;

namespace DC.Authorization.WPF.Services
{
    /// <summary>
    /// 权限门面实现（基础实现，WpfAuthorizationFacade 继承此类覆写 UI 弹窗）
    /// </summary>
    public class AuthorizationFacade : IAuthorizationFacade
    {
        private readonly IRightRepository _rightRepository;
        private readonly ILoginService _loginService;
        private readonly IAuditLogRepository _auditLogRepo;

        public Action PopLoginWindowAction { get; set; }

        public event EventHandler? AuthChanged;

        public AuthorizationFacade(IRightRepository rightRepository, ILoginService loginService, IAuditLogRepository auditLogRepository)
        {
            _rightRepository = rightRepository;
            _loginService = loginService;
            _auditLogRepo = auditLogRepository;

            // 订阅登录/注销事件，转发为统一的 AuthChanged
            _loginService.OnCardLogin += (_, _) => AuthChanged?.Invoke(this, EventArgs.Empty);
            _loginService.OnPasswordLogin += (_, _) => AuthChanged?.Invoke(this, EventArgs.Empty);
            _loginService.OnLogout += (_, _) => AuthChanged?.Invoke(this, EventArgs.Empty);
        }

        public void RegisterRights(Right[] rights)
        {
            if (rights == null) throw new ArgumentNullException(nameof(rights));
            _rightRepository.Upsert(rights);
        }

        public virtual void PopLoginWindow() { }

        public virtual void PopNoAuthNotification(AuthItem authItem) { }

        public bool CheckAuth(AuthItem authItem, RightType rightType = RightType.Operation)
        {
            if (!_loginService.HasLogin)
            {
                PopLoginWindow();
                return false;
            }
            if (_loginService.Current!.IsAdmin) return true;
            if (string.IsNullOrEmpty(authItem.Operation)) return true;

            var auth = _rightRepository.HasRight(_loginService.Current.Id, authItem.Module, authItem.View, authItem.Operation, rightType);
            if (!auth)
            {
                PopNoAuthNotification(authItem);
                return false;
            }
            return true;
        }

        public bool HasAuth(AuthItem authItem, RightType rightType = RightType.Operation)
        {
            if (!_loginService.HasLogin) return false;
            if (_loginService.Current!.IsAdmin) return true;
            if (string.IsNullOrEmpty(authItem.Operation)) return true;

            return _rightRepository.HasRight(_loginService.Current.Id, authItem.Module, authItem.View, authItem.Operation, rightType);
        }

        public void Audit<T>(string operation, string detail, T? before, T? after)
        {
            if (!_loginService.HasLogin) throw new InvalidOperationException("未登录不允许调用此API");
            var sb = new StringBuilder();
            sb.Append(detail);

            if (before != null && after != null)
            {
                var diffList = Utility.CompareProperties(before, after);
                foreach (var diff in diffList)
                {
                    sb.Append($",将{diff.PropDesc}由{diff.Before}改成{diff.After}");
                }
            }
            if (before == null && after != null)
            {
                sb.Append($",修改后的数据{JsonSerializer.Serialize(after)}");
            }
            _auditLogRepo.Insert(new AuditLog
            {
                AccountId = _loginService.Current!.Id,
                Operation = operation,
                Detail = sb.ToString(),
            });
        }
    }
}
