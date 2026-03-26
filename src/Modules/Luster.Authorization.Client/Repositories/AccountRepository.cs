using DC.Authorization;
using DC.Authorization.Models;
using DC.Authorization.WPF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace DC.Authorization.WPF.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        public List<Account> Load(bool skipDefaultAdmin = true)
        {
            using var conn = new SQLiteConnection(DbConfig.ConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText =
$@"SELECT acc.id,auth_name,auth_pwd,acc.role_id,real_name,tel_no,department,status,rl.role_name,rl.role_admin,acc.session_expire_min
FROM account_info ACC JOIN role_info RL ON acc.role_id = rl.role_id WHERE status<>-1 {(skipDefaultAdmin ? " AND id<>1" : string.Empty)}";
            using var reader = cmd.ExecuteReader();
            var res = new List<Account>();
            while (reader.Read())
            {
                var item = new Account();
                item.Id = reader.GetInt32(0);
                item.AccName = reader.GetString(1);
                item.AccPassword = reader.GetString(2);
                item.RoleId = reader.GetInt32(3);
                item.RealName = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);
                item.TelNo = reader.IsDBNull(5) ? string.Empty : reader.GetString(5);
                item.Department = reader.IsDBNull(6) ? string.Empty : reader.GetString(6);
                item.Status = reader.GetInt32(7);
                item.RoleName = reader.GetString(8);
                item.IsAdmin = Convert.ToBoolean(reader.GetInt32(9));
                item.SessionExpireMin = reader.GetInt32(10);
                res.Add(item);
            }
            return res;
        }

        public (Account?, bool, string) Login(string cardNum)
        {
            return LoginInternal(cardNum, null, null);
        }

        public (Account?, bool, string) Login(string cardNo, string password)
        {
            return LoginInternal(null, cardNo, password);
        }

        private (Account?, bool, string) LoginInternal(string? cardNum, string? cardNo, string? password)
        {
            using var conn = new SQLiteConnection(DbConfig.ConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = !string.IsNullOrEmpty(cardNum) ?
$@"SELECT id,auth_name,auth_pwd,acc.role_id,real_name,tel_no,department,status,session_expire_min,rl.role_name,rl.role_admin
FROM account_info ACC JOIN role_info rl on acc.role_id=rl.role_id
WHERE tel_no=@cardNum" :
$@"SELECT id,auth_name,auth_pwd,acc.role_id,real_name,tel_no,department,status,session_expire_min,rl.role_name,rl.role_admin
FROM account_info ACC JOIN role_info rl on acc.role_id=rl.role_id
WHERE tel_no=@cardNo and auth_pwd=@password";
            cmd.Parameters.AddRange(!string.IsNullOrEmpty(cardNum) ?
                new SQLiteParameter[] { new SQLiteParameter("@cardNum", cardNum) } :
                new SQLiteParameter[]
                {
                    new SQLiteParameter("@cardNo", cardNo),
                    new SQLiteParameter("@password", DbConfig.CalcPwdMd5(password)),
                });
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var item = new Account();
                item.Id = reader.GetInt32(0);
                item.AccName = reader.GetString(1);
                item.AccPassword = reader.GetString(2);
                item.RoleId = reader.GetInt32(3);
                item.RealName = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);
                item.TelNo = reader.IsDBNull(5) ? string.Empty : reader.GetString(5);
                item.Department = reader.IsDBNull(6) ? string.Empty : reader.GetString(6);
                item.Status = reader.GetInt32(7);
                item.SessionExpireMin = reader.GetInt32(8);
                item.RoleName = reader.GetString(9);
                item.IsAdmin = Convert.ToBoolean(reader.GetInt32(10));
                return (item, item.Status == 1, item.Status == 0 ? "该账户已注释!" : "该账户已被删除!");
            }
            return (null, false, "用户名或密码错误!\r\n请检查是否具备该等级权限!");
        }

        public int Create(Account account)
        {
            using var conn = new SQLiteConnection(DbConfig.ConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText =
                $@"insert into account_info(auth_name,auth_pwd,role_id,real_name,tel_no,department,session_expire_min,create_time) 
VALUES (@accName,@accPwd,@roleId,@realName,@telNo,@department,@sessionExpireMin,datetime('now')) returning id;";
            cmd.Parameters.AddRange(new SQLiteParameter[]
            {
                new SQLiteParameter("@accName", account.AccName),
                new SQLiteParameter("@accPwd", DbConfig.CalcPwdMd5(account.AccPassword)),
                new SQLiteParameter("@roleId", account.RoleId),
                new SQLiteParameter("@realName", account.RealName),
                new SQLiteParameter("@telNo", account.TelNo),
                new SQLiteParameter("@department", account.Department),
                new SQLiteParameter("@sessionExpireMin", account.SessionExpireMin),
            });
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public void Update(Account account)
        {
            using var conn = new SQLiteConnection(DbConfig.ConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $@"UPDATE account_info SET auth_name=@accName,role_id=@roleId,real_name=@realName,
tel_no=@telNo,department=@department,session_expire_min=@sessionExpireMin,update_time=datetime('now') WHERE id=@id;";
            cmd.Parameters.AddRange(new SQLiteParameter[]
            {
                new SQLiteParameter("@accName", account.AccName),
                new SQLiteParameter("@roleId", account.RoleId),
                new SQLiteParameter("@realName", account.RealName),
                new SQLiteParameter("@telNo", account.TelNo),
                new SQLiteParameter("@department", account.Department),
                new SQLiteParameter("@sessionExpireMin", account.SessionExpireMin),
                new SQLiteParameter("@id", account.Id),
            });
            cmd.ExecuteNonQuery();
        }

        public void ResetPassword(int id, string password)
        {
            using var conn = new SQLiteConnection(DbConfig.ConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $@"UPDATE account_info SET auth_pwd=@accPwd,update_time=datetime('now') WHERE id=@id;";
            cmd.Parameters.AddRange(new SQLiteParameter[]
            {
                new SQLiteParameter("@accPwd", DbConfig.CalcPwdMd5(password)),
                new SQLiteParameter("@id", id),
            });
            cmd.ExecuteNonQuery();
        }

        public void UpdateStatus(int id, int status)
        {
            using var conn = new SQLiteConnection(DbConfig.ConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE account_info SET status=@status WHERE id=@id";
            cmd.Parameters.Add(new SQLiteParameter("@id", id));
            cmd.Parameters.Add(new SQLiteParameter("@status", status));
            cmd.ExecuteNonQuery();
        }

        public void Delete(Account account)
        {
            using var conn = new SQLiteConnection(DbConfig.ConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $@"DELETE FROM account_info WHERE id=@id;";
            cmd.Parameters.Add(new SQLiteParameter("@id", account.Id));
            cmd.ExecuteNonQuery();
        }

        public void Import(List<Account> accounts)
        {
            using var conn = new SQLiteConnection(DbConfig.ConnectionString);
            conn.Open();
            var tran = conn.BeginTransaction();
            foreach (var account in accounts)
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText =
$@"insert into account_info(auth_name,auth_pwd,role_id,real_name,tel_no,department,session_expire_min,create_time) 
VALUES (@accName,@accPwd,@roleId,@realName,@telNo,@department,@sessionExpireMin,datetime('now'))
ON CONFLICT (auth_name) DO UPDATE SET auth_pwd=@accPwd,role_id=@roleId,real_name=@realName,tel_no=@telNo,department=@department,
session_expire_min=@sessionExpireMin,update_time=datetime('now');";
                cmd.Parameters.AddRange(new[]
                {
                    new SQLiteParameter("@accName", account.AccName),
                    new SQLiteParameter("@accPwd", DbConfig.CalcPwdMd5(account.AccPassword)),
                    new SQLiteParameter("@roleId", account.RoleId),
                    new SQLiteParameter("@realName", account.RealName),
                    new SQLiteParameter("@telNo", account.TelNo),
                    new SQLiteParameter("@department", account.Department),
                    new SQLiteParameter("@sessionExpireMin", account.SessionExpireMin),
                });
                cmd.ExecuteNonQuery();
            }
            tran.Commit();
        }

        public bool AccountNameExists(string accName, int? id = null)
        {
            if (string.IsNullOrEmpty(accName)) throw new ArgumentNullException(nameof(accName));
            using var conn = new SQLiteConnection(DbConfig.ConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $@"SELECT 1 FROM account_info WHERE auth_name=@accName {(id.HasValue ? "and id<>@id" : string.Empty)};";
            cmd.Parameters.AddRange(new[]
            {
                new SQLiteParameter("@accName", accName),
                new SQLiteParameter("@id", id),
            });
            return cmd.ExecuteScalar() != null;
        }
    }
}
