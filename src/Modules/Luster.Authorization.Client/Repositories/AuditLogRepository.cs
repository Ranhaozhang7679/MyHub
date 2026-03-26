using DC.Authorization;
using DC.Authorization.Models;
using DC.Authorization.WPF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace DC.Authorization.WPF.Repositories
{
    public class AuditLogRepository : IAuditLogRepository
    {
        public List<AuditLogListModel> Query(QueryModel model)
        {
            using var conn = new SQLiteConnection(DbConfig.ConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText =
$@"SELECT AL.id,ACC.id, ACC.auth_name,operation,operation_detail, AL.insert_time FROM audit_log AL
JOIN account_info ACC on al.account_id=acc.id
WHERE 1=1
{(model.StartTime.HasValue ? "AND insert_time>=@startTime" : string.Empty)}
{(model.EndTime.HasValue ? "AND insert_time<=@endTime" : string.Empty)}
ORDER BY insert_time DESC LIMIT @count OFFSET @offset;";
            cmd.Parameters.AddRange(new SQLiteParameter[]
            {
                new SQLiteParameter("@count", model.PageSize),
                new SQLiteParameter("@offset", model.PageSize * model.PageIndex),
                new SQLiteParameter("@startTime", model.StartTime),
                new SQLiteParameter("@endTime", model.EndTime),
            });
            var reader = cmd.ExecuteReader();
            var res = new List<AuditLogListModel>();
            while (reader.Read())
            {
                res.Add(new AuditLogListModel
                {
                    Id = reader.GetInt32(0),
                    AccountId = reader.GetInt32(1),
                    AccountName = reader.GetString(2),
                    Operation = reader.GetString(3),
                    Detail = reader.GetString(4),
                    When = reader.GetDateTime(5),
                });
            }
            return res;
        }

        public void Insert(AuditLog log)
        {
            using var conn = new SQLiteConnection(DbConfig.ConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText =
                $@"insert into audit_log(account_id,operation,operation_detail,insert_time) 
VALUES (@accId,@operation,@detail,datetime('now'));";
            cmd.Parameters.AddRange(new SQLiteParameter[]
            {
                new SQLiteParameter("@accId", log.AccountId),
                new SQLiteParameter("@operation", log.Operation),
                new SQLiteParameter("@detail", log.Detail),
            });
            cmd.ExecuteNonQuery();
        }

        public int Delete(DateTime timeBefore)
        {
            using var conn = new SQLiteConnection(DbConfig.ConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $@"DELETE FROM audit_log WHERE insert_Time<@timeBefore;";
            cmd.Parameters.Add(new SQLiteParameter("@timeBefore", timeBefore));
            return cmd.ExecuteNonQuery();
        }
    }
}
