using DC.Authorization;
using DC.Authorization.Models;
using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DC.Authorization.WPF.Providers
{
    /// <summary>
    /// 组合认证提供者：按优先级尝试多个 Provider
    /// <para>刷卡 → 优先 HiveApi；密码 → 本地验证</para>
    /// </summary>
    public class CompositeAuthProvider : IAuthProvider
    {
        private readonly LocalAuthProvider _localProvider;
        private readonly HiveAuthProvider _hiveProvider;
        private readonly ILogger _logger;

        public CompositeAuthProvider(LocalAuthProvider localProvider,
            HiveAuthProvider hiveProvider, ILogger logger)
        {
            _localProvider = localProvider;
            _hiveProvider = hiveProvider;
            _logger = logger;
        }

        public bool IsAvailable => _localProvider.IsAvailable || _hiveProvider.IsAvailable;

        public async Task<AuthResult> AuthenticateAsync(AuthCredential credential)
        {
            // 密码登录 → 直接走本地
            if (credential.Method == AuthMethod.Password)
            {
                _logger.Information("密码登录 → 使用 LocalAuthProvider");
                return await _localProvider.AuthenticateAsync(credential);
            }

            // 刷卡登录 → 优先 HiveApi，失败则回退本地
            if (credential.Method == AuthMethod.CardSwipe)
            {
                if (_hiveProvider.IsAvailable)
                {
                    _logger.Information("刷卡登录 → 先尝试 HiveAuthProvider");
                    var hiveResult = await _hiveProvider.AuthenticateAsync(credential);
                    if (hiveResult.Success) return hiveResult;

                    _logger.Warning("HiveApi 认证失败({Message})，回退到本地验证", hiveResult.Message);
                }

                _logger.Information("刷卡登录 → 使用 LocalAuthProvider");
                return await _localProvider.AuthenticateAsync(credential);
            }

            return new AuthResult { Success = false, Message = "不支持的认证方式" };
        }
    }
}
