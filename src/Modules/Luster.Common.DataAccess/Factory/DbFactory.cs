#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       DbFactory
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.DataAccess.Factory
* 文 件 名:       DbFactory.cs
* 创建时间:       2022/7/4 8:50:15
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      aca884cf-7683-4211-8d0a-10cc24c80242
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/7/4 8:50:15
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using FreeSql.DataAnnotations;
using System.Data.SQLite;
using Luster.Common.DataAccess.Tables;
using Luster.Common.DataAccess.Repositories;

namespace Luster.Common.DataAccess.Factory
{
    public class DBFactory : IDBFactory
    {
        private string _connStr = "";
        private string _connStr_Ass = "";
        Lazy<IFreeSql> sqliteLazy;
        Lazy<IFreeSql> sqliteLazy_Ass;
        private string digitalAssCsvDir = "";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="types"></param>
        public void DbInit(Type[] types)
        {
            //同步实体
            GetDbConnection().CodeFirst.SyncStructure(types);
        }

        public void DbInit_Ass(Type[] types)
        {
            // 同步实体（Ass 库需要先初始化连接）
            var db = GetDbConnection_Ass();
            if (db == null)
            {
                throw new InvalidOperationException("Ass 数据库未初始化：请在调用 DbInit_Ass 前，通过 SetConnectionStr 传入 conn_Ass（Ass 库连接字符串）。");
            }

            db.CodeFirst.SyncStructure(types);
        }

        public IFreeSql GetDbConnection()
        {
            return sqliteLazy?.Value;
        }

        public IFreeSql GetDbConnection_Ass()
        {
            return sqliteLazy_Ass?.Value;
        }

        public void SetConnectionStr(string conn, string conn_Ass, string csvDir)
        {
            _connStr = conn;
            sqliteLazy = new Lazy<IFreeSql>(() => new FreeSql.FreeSqlBuilder()
              .UseConnectionString(FreeSql.DataType.Sqlite, _connStr)
              .UseAutoSyncStructure(true)// 建议开发期间为true 发布时改为False 功能为自动同步实体到数据库
              .UseLazyLoading(true)
              //.UseMonitorCommand(
              //    cmd => Trace.WriteLine("\r\n线程" + Thread.CurrentThread.ManagedThreadId + ": " + cmd.CommandText) //监听SQL命令对象，在执行前 输出执行前的SQL语句
              //                                                                                                     //, (cmd, traceLog) => Console.WriteLine(traceLog)
              //    )
              .Build());

            var types = Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => !x.IsAbstract && !x.IsInterface && x.Name.StartsWith("Tb"))
                .ToArray();
            DbInit(types);

            // 兼容：空字符串等同于未传入
            if (!string.IsNullOrWhiteSpace(conn_Ass))
            {
                //数字架线
                _connStr_Ass = conn_Ass;
                sqliteLazy_Ass = new Lazy<IFreeSql>(() => new FreeSql.FreeSqlBuilder()
                  .UseConnectionString(FreeSql.DataType.Sqlite, _connStr_Ass)
                  .UseAutoSyncStructure(true)// 建议开发期间为true 发布时改为False 功能为自动同步实体到数据库
                  .UseLazyLoading(true)
                  //.UseMonitorCommand(
                  //    cmd => Trace.WriteLine("\r\n线程" + Thread.CurrentThread.ManagedThreadId + ": " + cmd.CommandText) //监听SQL命令对象，在执行前 输出执行前的SQL语句
                  //                                                                                                     //, (cmd, traceLog) => Console.WriteLine(traceLog)
                  //    )
                  .Build());

                var types_ass = Assembly.GetExecutingAssembly().GetTypes()
                    .Where(x => !x.IsAbstract && !x.IsInterface && x.Name.StartsWith("AssTb"))
                    .ToArray();
                DbInit_Ass(types_ass);
            }
            else
            {
                // 显式清空，避免误用旧连接（如多配方切换）
                _connStr_Ass = "";
                sqliteLazy_Ass = null;
            }

            digitalAssCsvDir = csvDir;
        }

        /// <summary>
        /// 删除指定的数据表
        /// </summary>
        /// <param name="removeTypes"></param>
        public void DbDrop(Type[] removeTypes)
        {
            var conn = GetDbConnection();
            foreach (var type in removeTypes)
            {
                var tableName = type.Name;
                var tableAttribute = type.GetCustomAttribute<TableAttribute>();
                if (tableAttribute != null)
                {
                    tableName = tableAttribute.Name;
                }
                ;
                var existtable = conn.Ado.Query<int>("select count(*) from  sqlite_master where name=@name", new { name = tableName }).FirstOrDefault();
                if (existtable > 0)
                {
                    conn.Ado.ExecuteNonQuery("Drop Table " + tableName);
                }
            }

        }

        /// <summary>
        /// 彻底关闭数据库连接
        /// </summary>
        public void DbClose()
        {
            if (sqliteLazy != null)
            {
                sqliteLazy.Value.Dispose();
                SQLiteConnection.ClearAllPools();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                sqliteLazy = null;
            }
            if (sqliteLazy_Ass != null)
            {
                sqliteLazy_Ass.Value.Dispose();
                SQLiteConnection.ClearAllPools();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                sqliteLazy_Ass = null;
            }
        }

        public string GetCsvDir()
        {
            return digitalAssCsvDir ?? "E:\\DigitalCsv\\";
        }
    }
}