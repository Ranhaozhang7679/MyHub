using DC.Authorization;
using DC.Authorization.Models;
using DC.Authorization.WPF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace DC.Authorization.WPF.Repositories
{
    public class RightRepository : IRightRepository
    {
        public List<Right> Load()
        {
            using var conn = new SQLiteConnection(DbConfig.ConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id,right_name,module_name,view_name,description,right_type FROM right_info";
            using var reader = cmd.ExecuteReader();
            var res = new List<Right>();
            while (reader.Read())
            {
                res.Add(new Right
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    ModuleName = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    ViewName = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    Description = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    Type = (RightType) reader.GetByte(5),
                });
            }
            return res;
        }

        public void Upsert(Right[] rights)
        {
            using var conn = new SQLiteConnection(DbConfig.ConnectionString);
            conn.Open();
            var tran = conn.BeginTransaction();
            foreach (var right in rights)
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"INSERT INTO right_info(right_name, module_name, view_name, description, right_type) 
VALUES (@name, @moduleName, @viewName, @description, @rightType) 
ON CONFLICT(module_name, view_name, right_name) DO UPDATE SET description=@description, right_type=@rightType;";
                cmd.Parameters.AddRange(new SQLiteParameter[]
                {
                    new SQLiteParameter("@name", right.Name),
                    new SQLiteParameter("@moduleName", right.ModuleName),
                    new SQLiteParameter("@viewName", right.ViewName),
                    new SQLiteParameter("@description", right.Description),
                    new SQLiteParameter("@rightType", (byte)right.Type),
                });
                cmd.ExecuteNonQuery();
            }
            tran.Commit();
        }

        public bool HasRight(int accountId, string moduleName, string viewName, string rightName)
        {
            using var conn = new SQLiteConnection(DbConfig.ConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT 1 FROM role_right RR 
JOIN account_info ACC ON rr.role_id=acc.role_id 
JOIN right_info RI ON rr.right_id=ri.id 
WHERE acc.id=@accountId AND ri.module_name=@moduleName AND ri.view_name=@viewName AND ri.right_name=@rightName";
            cmd.Parameters.Add(new SQLiteParameter("@accountId", accountId));
            cmd.Parameters.Add(new SQLiteParameter("@moduleName", moduleName));
            cmd.Parameters.Add(new SQLiteParameter("@viewName", viewName));
            cmd.Parameters.Add(new SQLiteParameter("@rightName", rightName));
            return cmd.ExecuteScalar() != null;
        }

        public void DeleteRoleRights(int roleId)
        {
            using var conn = new SQLiteConnection(DbConfig.ConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM role_right WHERE role_id=@roleId";
            cmd.Parameters.Add(new SQLiteParameter("@roleId", roleId));
            cmd.ExecuteNonQuery();
        }
    }
}
