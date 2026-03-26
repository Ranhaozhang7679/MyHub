using DC.Authorization.WPF.Infrastructure;
using System.Data.SQLite;

namespace DC.Authorization.WPF.Infrastructure
{
    /// <summary>
    /// 数据库初始化：创建表结构和初始数据
    /// </summary>
    public class DbInitializer
    {
        public static void Initialize()
        {
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
VALUES(4, 'Administrator', 1, 1, 50);

CREATE TABLE IF NOT EXISTS right_info(
    id integer not null primary key autoincrement,
    right_name nvarchar(50) not null,
    module_name nvarchar(50) null,
    view_name nvarchar(50) null,
    description nvarchar(200) null,
    right_type TINYINT NOT NULL DEFAULT 0,
    create_time DATETIME NOT NULL DEFAULT (datetime('now')),
    update_time DATETIME NULL
);

-- Drop the old single-column unique index if it exists to allow duplicate operations across modules
DROP INDEX IF EXISTS name_right_uq;

-- Create the new composite unique index
CREATE UNIQUE INDEX IF NOT EXISTS uq_module_view_right ON right_info(module_name, view_name, right_name);

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

            // 尝试在旧版本数据库中增加列，如果已存在会直接抛异常但不影响后续运行
            try
            {
                var altCmd = conn.CreateCommand();
                altCmd.CommandText = "ALTER TABLE right_info ADD COLUMN right_type TINYINT NOT NULL DEFAULT 0;";
                altCmd.ExecuteNonQuery();
            }
            catch { }
        }
    }
}
