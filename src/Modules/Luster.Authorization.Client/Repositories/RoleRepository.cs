using DC.Authorization;
using DC.Authorization.Models;
using DC.Authorization.WPF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace DC.Authorization.WPF.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        public List<Role> Load()
        {
            using var conn = new SQLiteConnection(DbConfig.ConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT role_id,role_name,role_level,role_admin FROM role_info";
            using var reader = cmd.ExecuteReader();
            var res = new List<Role>();
            while (reader.Read())
            {
                res.Add(new Role
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Level = reader.GetInt32(2),
                    IsAdmin = Convert.ToBoolean(reader.GetInt32(3)),
                });
            }
            return res;
        }

        public int Create(Role role)
        {
            using var conn = new SQLiteConnection(DbConfig.ConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText =
                $@"insert into role_info(role_name,role_level,role_admin,insert_time) 
VALUES (@name,@level,@admin,datetime('now')) returning role_id;";
            cmd.Parameters.AddRange(new SQLiteParameter[]
            {
                new SQLiteParameter("@name", role.Name),
                new SQLiteParameter("@level", role.Level),
                new SQLiteParameter("@admin", role.IsAdmin ? 1 : 0),
            });
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public void Update(Role role)
        {
            using var conn = new SQLiteConnection(DbConfig.ConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText =
                $@"UPDATE role_info SET role_name=@name,role_level=@level,role_admin=@admin,update_time=datetime('now') WHERE role_id=@id";
            cmd.Parameters.AddRange(new SQLiteParameter[]
            {
                new SQLiteParameter("@name", role.Name),
                new SQLiteParameter("@level", role.Level),
                new SQLiteParameter("@admin", role.IsAdmin ? 1 : 0),
                new SQLiteParameter("@id", role.Id),
            });
            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var conn = new SQLiteConnection(DbConfig.ConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM role_info WHERE role_id=@id";
            cmd.Parameters.Add(new SQLiteParameter("id", id));
            cmd.ExecuteNonQuery();
        }

        public void Assign(int roleId, int[] rights)
        {
            using var conn = new SQLiteConnection(DbConfig.ConnectionString);
            conn.Open();
            var tran = conn.BeginTransaction();
            foreach (var item in rights)
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT OR IGNORE INTO role_right(role_id,right_id,insert_time) VALUES (@roleId,@rightId,datetime('now'))";
                cmd.Parameters.AddRange(new SQLiteParameter[]
                {
                    new SQLiteParameter("@roleId", roleId),
                    new SQLiteParameter("@rightId", item),
                });
                cmd.ExecuteNonQuery();
            }
            tran.Commit();
        }

        public List<int> LoadRights(int roleId)
        {
            using var conn = new SQLiteConnection(DbConfig.ConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT right_id FROM role_right WHERE role_id = {roleId}";
            using var reader = cmd.ExecuteReader();
            var rightIds = new List<int>();
            while (reader.Read())
            {
                rightIds.Add(reader.GetInt32(0));
            }
            return rightIds;
        }
    }
}
