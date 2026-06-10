using DC.Authorization.WPF.Infrastructure;
using System.Data.SQLite;
using System.IO;
using System.Reflection;

namespace DC.Authorization.WPF.Infrastructure
{
    /// <summary>
    /// 数据库初始化：创建表结构和初始数据
    /// </summary>
    public class DbInitializer
    {
        private const string TemplateResourceName = "Luster.Authorization.Client.Resources.authorization_template0514.db";

        public static void Initialize()
        {
            EnsureDatabase();

            using var conn = new SQLiteConnection(DbConfig.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = $@"
CREATE TABLE IF NOT EXISTS account_info (
id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
auth_name VARCHAR(32) NOT NULL,
auth_pwd VARCHAR(32) NOT NULL,
status TINYINT NOT NULL DEFAULT 1,
role_id INTEGER NOT NULL,
real_name VARCHAR(32),
tel_no VARCHAR(16),
department VARCHAR(32),
session_expire_min INTEGER DEFAULT 3,
create_time DATETIME NOT NULL,
update_time DATETIME  NULL);

CREATE UNIQUE INDEX IF NOT EXISTS uq_authName_accountInfo ON account_info(auth_name);

INSERT OR IGNORE INTO account_info(id,auth_name,auth_pwd,role_id,real_name,tel_no,department,session_expire_min,create_time)
VALUES(1,'Administrator','E2159C7EC2CBF34802DFE67A72AE2726',4,'管理员A','1433223','Luster',30,datetime('now'));

INSERT OR IGNORE INTO account_info(id,auth_name,auth_pwd,role_id,real_name,tel_no,department,session_expire_min,create_time)
VALUES(2,'Integrator','E2159C7EC2CBF34802DFE67A72AE2726',3,'B','114514','Luster',30,datetime('now'));

INSERT OR IGNORE INTO account_info(id,auth_name,auth_pwd,role_id,real_name,tel_no,department,session_expire_min,create_time)
VALUES(3,'Maintenance','E2159C7EC2CBF34802DFE67A72AE2726',2,'C','114515','Luster',30,datetime('now'));

INSERT OR IGNORE INTO account_info(id,auth_name,auth_pwd,role_id,real_name,tel_no,department,session_expire_min,create_time)
VALUES(4,'OP ReadOnly','E2159C7EC2CBF34802DFE67A72AE2726',1,'D','114516','Luster',1440,datetime('now'));

INSERT OR IGNORE INTO account_info(id,auth_name,auth_pwd,role_id,real_name,tel_no,department,session_expire_min,create_time)
VALUES(5,'SuperAdmin','E2159C7EC2CBF34802DFE67A72AE2726',5,'超级管理员','202611111111','Luster',30,datetime('now'));

CREATE TABLE IF NOT EXISTS role_info(
[role_id] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
[role_name] VARCHAR(64) NOT NULL,
[role_level] TINYINT NOT NULL,
[role_admin] TINYINT NOT NULL DEFAULT 0,
[role_volume] INTEGER DEFAULT 0
);

INSERT OR IGNORE INTO role_info (role_id, role_name, role_level, role_admin, role_volume) 
VALUES(1, 'OP ReadOnly', 4, 0, 0);
INSERT OR IGNORE INTO role_info (role_id, role_name, role_level, role_admin, role_volume) 
VALUES(2, 'Maintenance', 3, 0, 0);
INSERT OR IGNORE INTO role_info (role_id, role_name, role_level, role_admin, role_volume) 
VALUES(3, 'Integrator', 2, 0, 0);
INSERT OR IGNORE INTO role_info (role_id, role_name, role_level, role_admin, role_volume)
VALUES(4, 'Administrator', 1, 0, 50);
INSERT OR IGNORE INTO role_info (role_id, role_name, role_level, role_admin, role_volume)
VALUES(5, '超级管理员', 0, 1, 50);

CREATE TABLE IF NOT EXISTS right_info(
    id integer not null primary key autoincrement,
    right_name nvarchar(50) not null,
    module_name nvarchar(50) null,
    view_name nvarchar(50) null,
    description nvarchar(200) null,
    right_type TINYINT NOT NULL DEFAULT 0,
    sort_order INTEGER NOT NULL DEFAULT 0,
    create_time DATETIME NOT NULL DEFAULT (datetime('now')),
    update_time DATETIME NULL
);
";
            cmd.ExecuteNonQuery();

            var idxCmd = conn.CreateCommand();
            idxCmd.CommandText = @"
-- Drop the old single-column unique index if it exists to allow duplicate operations across modules
DROP INDEX IF EXISTS name_right_uq;

-- Create the new composite unique index
CREATE UNIQUE INDEX IF NOT EXISTS uq_module_view_right ON right_info(module_name, view_name, right_name, right_type);
";
            idxCmd.ExecuteNonQuery();

            cmd = conn.CreateCommand();
            cmd.CommandText = @"

CREATE TABLE IF NOT EXISTS role_right(
    role_id integer not null,
    right_id integer not null,
    insert_time datetime not null
);
CREATE UNIQUE INDEX IF NOT EXISTS unique_roleId_rightId_RoleRight ON role_right(role_id,right_id);

CREATE TABLE IF NOT EXISTS audit_log(
    id integer not null primary key autoincrement,
    account_id integer not null,
    operation nvarchar(50) not null,
    operation_detail nvarchar(500) not null,
    insert_time datetime not null
);
CREATE INDEX IF NOT EXISTS IX_insertTime_AuditLog on audit_log (insert_time);

CREATE TABLE IF NOT EXISTS settings (
    key VARCHAR(128) UNIQUE NOT NULL,
    value VARCHAR(512) NULL,
    [create_time] DATETIME NOT NULL, 
    [update_time] DATETIME DEFAULT NULL
)
";
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// 确保数据库文件存在且为新版：
        /// - 无库 → 从嵌入资源提取模板
        /// - 旧库（无 Level 0 角色）→ 备份 + 提取模板
        /// - 新库 → 不处理
        /// </summary>
        private static void EnsureDatabase()
        {
            string dbPath = GetDbPath();

            if (!File.Exists(dbPath))
            {
                ExtractTemplate(dbPath);
                return;
            }

            bool isOld = false;
            try
            {
                using var conn = new SQLiteConnection($"Data Source={dbPath};Version=3;");
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM role_info WHERE role_level=0";
                isOld = Convert.ToInt32(cmd.ExecuteScalar()) == 0;
            }
            catch
            {
                isOld = true;
            }

            if (!isOld) return;

            string backup = dbPath + $".{DateTime.Now:yyyyMMdd_HHmmss}.bak";
            File.Copy(dbPath, backup, overwrite: true);
            File.Delete(dbPath);
            ExtractTemplate(dbPath);
        }

        /// <summary>
        /// 从程序集嵌入资源提取模板数据库到目标路径；
        /// 资源不存在或为空时跳过，由后续 CREATE TABLE 补全
        /// </summary>
        private static void ExtractTemplate(string targetPath)
        {
            var dir = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(TemplateResourceName);
            if (stream == null || stream.Length == 0)
                return;

            using var fs = new FileStream(targetPath, FileMode.Create, FileAccess.Write);
            stream.CopyTo(fs);
        }

        private static string GetDbPath()
        {
            const string prefix = "Data Source=";
            return DbConfig.ConnectionString.Substring(prefix.Length);
        }
    }
}
