using DC.Authorization;
using DC.Authorization.Models;
using DC.Authorization.WPF.Infrastructure;
using Serilog;
using System;
using System.Threading.Tasks;

namespace DC.Authorization.WPF.Services
{
    /// <summary>
    /// 会话管理实现（超时自动注销）
    /// </summary>
    public class SessionManager : ISessionManager
    {
        private readonly ILoginService _loginService;
        private readonly IActiveHook _activeHook;
        private readonly ILogger _logger;

        public SessionManager(ILoginService loginService, ILogger logger, IActiveHook activeHook)
        {
            _logger = logger;
            _activeHook = activeHook;
            _loginService = loginService;
        }

        public int CountDown { get; private set; }
        public event EventHandler<EventArgs>? SessionExpired;

        public void Start()
        {
            Task.Run(TrackSession);
            _activeHook.Start();
        }

        private async Task TrackSession()
        {
            DateTime inactiveStartTime = default;
            while (true)
            {
                if (!_loginService.HasLogin)
                {
                    await Task.Delay(500);
                    continue;
                }

                try
                {
                    if (_activeHook.IsActive)
                    {
                        _activeHook.Reset();
                        if (inactiveStartTime != default && Utility.IsForeground())
                        {
                            inactiveStartTime = default;
                            CountDown = _loginService.Current!.SessionExpireMin * 60;
                        }
                        if (inactiveStartTime == default) { CountDown = _loginService.Current!.SessionExpireMin * 60; }
                    }
                    else
                    {
                        if (inactiveStartTime == default) { inactiveStartTime = DateTime.Now; }
                        var elapsedSecs = (int)(DateTime.Now - inactiveStartTime).TotalSeconds;
                        CountDown = _loginService.Current!.SessionExpireMin * 60 - elapsedSecs;
                        if (_loginService.HasLogin && elapsedSecs > _loginService.Current!.SessionExpireMin * 60)
                        {
                            _logger.Information($"用户超过{_loginService.Current!.SessionExpireMin}min无动作，注销");
                            _loginService.Logout();
                            SessionExpired?.Invoke(this, EventArgs.Empty);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Session管理出现未经处理的异常");
                    break;
                }

                await Task.Delay(500);
            }
        }

        public void Stop() { }
    }
}
