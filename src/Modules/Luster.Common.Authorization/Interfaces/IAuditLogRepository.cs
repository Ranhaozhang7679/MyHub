using DC.Authorization.Models;
using System;
using System.Collections.Generic;

namespace DC.Authorization
{
    /// <summary>
    /// 审计日志仓储接口
    /// </summary>
    public interface IAuditLogRepository
    {
        /// <summary>按条件分页查询日志</summary>
        List<AuditLogListModel> Query(QueryModel model);

        /// <summary>插入一条日志</summary>
        void Insert(AuditLog log);

        /// <summary>删除指定时间之前的日志，返回删除行数</summary>
        int Delete(DateTime timeBefore);
    }
}
