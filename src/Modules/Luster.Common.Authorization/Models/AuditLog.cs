using System;

namespace DC.Authorization.Models
{
    /// <summary>
    /// 审计日志
    /// </summary>
    public class AuditLog
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public string Operation { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
    }

    /// <summary>
    /// 审计日志列表展示模型（含关联的账户名和时间）
    /// </summary>
    public class AuditLogListModel : AuditLog
    {
        public string AccountName { get; set; } = string.Empty;
        public DateTime When { get; set; }
    }

    /// <summary>
    /// 审计日志查询条件
    /// </summary>
    public class QueryModel
    {
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int PageIndex { get; set; } = 0;
        public int PageSize { get; set; } = 30;
    }
}
