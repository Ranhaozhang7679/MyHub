using DC.Authorization;
using DC.Authorization.Models;
using DC.Authorization.WPF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace DC.Authorization.WPF.Repositories
{
    public class AuthSettingRepository : IAuthSettingRepository
    {
        public AuthSetting Query()
        {
            using var conn = new SQLiteConnection(DbConfig.ConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT key, value FROM settings";
            using var reader = cmd.ExecuteReader();
            var map = new Dictionary<string, string>();
            while (reader.Read())
            {
                map.Add(reader.GetString(0), reader.IsDBNull(1) ? string.Empty : reader.GetString(1));
            }
            var result = new AuthSetting();
            result.Deserialize(map);
            return result;
        }

        public void Update(AuthSetting config)
        {
            using var conn = new SQLiteConnection(DbConfig.ConnectionString);
            conn.Open();
            var tran = conn.BeginTransaction();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO settings (key,value,create_time) values(@key,@value,datetime('now')) " +
                "ON CONFLICT (key) DO UPDATE SET value=@value,update_time=datetime('now')";
            var map = config.Serialize();
            foreach (var pair in map)
            {
                cmd.Parameters.Clear();
                cmd.Parameters.Add(new SQLiteParameter("key", pair.Key));
                cmd.Parameters.Add(new SQLiteParameter("value", pair.Value));
                cmd.ExecuteNonQuery();
            }
            tran.Commit();
            SettingChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler<EventArgs>? SettingChanged;
    }
}
