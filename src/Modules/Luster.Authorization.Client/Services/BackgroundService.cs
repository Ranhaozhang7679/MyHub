using DC.Authorization;
using Serilog;
using System;

namespace DC.Authorization.WPF.Services
{
    /// <summary>
    /// 后台服务（定时清理审计日志 + 启动会话管理）
    /// </summary>
    public class BackgroundService
    {
        private readonly IAuditLogRepository _auditLogRepo;
        private readonly ILogger _logger;
        private Timer? _rmHistoryAuditLogTimer;
        private readonly ISessionManager _sessionManager;

        public BackgroundService(IAuditLogRepository auditLogRepository, ILogger logger, ISessionManager sessionManager)
        {
            _auditLogRepo = auditLogRepository;
            _logger = logger;
            _sessionManager = sessionManager;
        }

        public void Start()
        {
            if (_rmHistoryAuditLogTimer != null) return;
            _rmHistoryAuditLogTimer = new Timer((_) =>
            {
                var oneYearAgo = DateTime.Now.AddDays(-366);
                var rowsCount = _auditLogRepo.Delete(oneYearAgo);
                if (rowsCount > 0)
                {
                    _logger.Information($"移除一年前的历史审讯日志, 共有条数:{rowsCount}");
                }
            }, null, TimeSpan.Zero, TimeSpan.FromDays(1));
            _sessionManager.Start();
        }
    }
}
