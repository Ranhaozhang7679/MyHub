#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       Repository
* 机器名称:       Z05592
* 命名空间:       Luster.Common.DataAccess.Repositories
* 文 件 名:       Repository.cs
* 创建时间:       2022/10/12 14:52:13
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      84906f15-6ead-44db-b6b8-db04bd111596
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/10/12 14:52:13
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using Luster.Common.DataAccess.Factory;
using Luster.Common.DataAccess.Tables;
using Luster.Common.DataStruct;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.DataAccess.Repositories
{
    public class Repository : IRepository
    {
        private IDBFactory _dBFactory;
        public Repository(IDBFactory dBFactory)
        {
            _dBFactory = dBFactory;
        }

        /// <summary>
        /// 新增插入实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public long Insert<T>(T entity) where T : class
        {
            try
            {
                return _dBFactory.GetDbConnection().Insert<T>(entity).ExecuteIdentity();
            }
            catch
            {
                throw new FriendlyException($"实体 {entity.GetType().Name}添加失败，请检查数据库连接！");
            }
        }


        /// <summary>
        /// 批量插入
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <returns></returns>
        public int BatchInsert<T>(IEnumerable<T> entities) where T : class
        {
            try
            {
                return _dBFactory.GetDbConnection().Insert(entities).ExecuteAffrows();
            }
            catch
            {
                throw new FriendlyException($"实体 {entities.FirstOrDefault().GetType().Name}批量添加失败，请检查数据库连接！");
            }
        }

        /// <summary>
        /// 异步执行批量插入
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <returns></returns>
        public async Task<int> BatchInsertAsync<T>(IEnumerable<T> entities) where T : class
        {
            try
            {
                return await _dBFactory.GetDbConnection().Insert(entities).ExecuteAffrowsAsync();
            }
            catch
            {
                throw new FriendlyException($"实体 {entities.FirstOrDefault().GetType().Name}批量添加失败，请检查数据库连接！");
            }
        }

        /// <summary>
        /// 删除指定实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public int Delete<T>(T entity) where T : class
        {
            try
            {
                return _dBFactory.GetDbConnection().Delete<T>(entity).ExecuteAffrows();
            }
            catch
            {
                throw new FriendlyException($"实体 {entity.GetType().Name}删除失败，请检查数据库连接！");
            }
        }

        /// <summary>
        /// 根据Id删除实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public int DeleteById<T>(long id) where T : class
        {
            try
            {
                return _dBFactory.GetDbConnection().Delete<T>(new { Id = id }).ExecuteAffrows();
            }
            catch
            {
                throw new FriendlyException("根据ID数据删除失败，请检查数据库连接！");
            }
        }

        /// <summary>
        /// 批量删除
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        public void BatchDelete<T>(IEnumerable<T> entities) where T : class
        {
            try
            {
                _dBFactory.GetDbConnection().Delete<T>(entities.ToArray()).ExecuteAffrows();
            }
            catch
            {
                throw new FriendlyException($"实体{entities.FirstOrDefault().GetType().Name}批量删除失败，请检查数据库连接！");
            }
        }


        /// <summary>
        /// 异步批量删除实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        public async void BatchDeleteAsync<T>(IEnumerable<T> entities) where T : class
        {
            try
            {
                await _dBFactory.GetDbConnection().Delete<T>(entities.ToArray()).ExecuteAffrowsAsync();
            }
            catch
            {
                throw new FriendlyException($"实体{entities.FirstOrDefault().GetType().Name}批量删除失败，请检查数据库连接！");
            }
        }

        /// <summary>
        /// 更新单个实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public int Update<T>(T entity) where T : class
        {
            try
            {
                return _dBFactory.GetDbConnection().Update<T>().SetSource(entity).ExecuteAffrows();
            }
            catch
            {
                throw new FriendlyException($"实体{entity.GetType().Name}更新失败，请检查数据库连接！");
            }
        }

        /// <summary>
        /// 更新单个实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public int Update<T>(T entity, params string[] columns) where T : class
        {
            try
            {
                return _dBFactory.GetDbConnection().Update<T>().
                    SetSource(entity).
                    UpdateColumns(columns).
                    ExecuteAffrows();
            }
            catch
            {
                throw new FriendlyException($"实体{entity.GetType().Name}更新失败，请检查数据库连接！");
            }
        }

        /// <summary>
        /// 批量更新
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <returns></returns>
        public int BatchUpdate<T>(IEnumerable<T> entities) where T : class
        {
            try
            {
                return _dBFactory.GetDbConnection().Update<T>().SetSource(entities).ExecuteAffrows();
            }
            catch
            {
                throw new FriendlyException($"实体{entities.FirstOrDefault().GetType().Name}批量更新失败，请检查数据库连接！");
            }
        }

        /// <summary>
        /// 批量更新插入
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <returns></returns>
        /// <exception cref="FriendlyException"></exception>
        [Obsolete("用本地CSV进行存储")]
        public int InsertOrUpdate<T>(IEnumerable<T> entities) where T : class
        {
            var db = _dBFactory.GetDbConnection_Ass();
            using (var uow = db.CreateUnitOfWork())
            {
                try
                {
                    //foreach (var item in entities)
                    //{
                    //    IEntity entity = (IEntity)item;
                    //    var re = db
                    //         .Delete<T>()
                    //         .WhereDynamic(new { entity.ID }) // 参数化SQL
                    //         .ExecuteAffrows();
                    //}
                    // 清空整个表
                    db.Delete<T>().Where("1=1").ExecuteAffrows();

                    db.Insert(entities).ExecuteAffrows();
                    //_dBFactory.GetDbConnection().InsertOrUpdate<T>().SetSource(entities).ExecuteAffrows();
                    uow.Commit();
                    return 1;
                }
                catch
                {
                    uow.Rollback();
                    throw new FriendlyException($"实体{entities.FirstOrDefault().GetType().Name}批量更新失败，请检查数据库连接！");
                }
            }
        }
        public IEnumerable<T> GetAllDataNew<T>(int pageIndex, int perPageCount, out long totalCount) where T : class, new()
        {
            var path = _dBFactory.GetCsvDir();
            if (string.IsNullOrEmpty(path))
                throw new FriendlyException("CSV目录未配置，请检查！");

            var type = typeof(T);
            var fileName = Path.Combine(path, $"{type.Name}.csv");
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite && p.GetIndexParameters().Length == 0)
                .ToArray();

            // 构建 DisplayName 到 PropertyInfo 的映射
            var displayNameToProp = props.ToDictionary(
                p =>
                {
                    var displayAttr = p.GetCustomAttribute<System.ComponentModel.DataAnnotations.DisplayAttribute>();
                    return displayAttr != null && !string.IsNullOrEmpty(displayAttr.Name) ? displayAttr.Name : p.Name;
                },
                p => p
            );

            if (!File.Exists(fileName))
            {
                // 创建空表（仅写入表头）
                Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                using (var writer = new StreamWriter(fileName, false, Encoding.UTF8))
                {
                    writer.WriteLine(string.Join(",", displayNameToProp.Keys));
                    // 默认增加一行空数据
                    writer.WriteLine(string.Join(",", displayNameToProp.Keys.Select(_ => "")));
                }
                totalCount = 1;
                var defaultObj = new T();
                return new List<T> { defaultObj };
            }

            var result = new List<T>();
            using (var reader = new StreamReader(fileName))
            {
                var header = reader.ReadLine();
                if (header == null)
                {
                    // 文件存在但无表头，补写表头和一行空数据
                    using (var writer = new StreamWriter(fileName, false, Encoding.UTF8))
                    {
                        writer.WriteLine(string.Join(",", displayNameToProp.Keys));
                        writer.WriteLine(string.Join(",", displayNameToProp.Keys.Select(_ => "")));
                    }
                    totalCount = 1;
                    var defaultObj = new T();
                    return new List<T> { defaultObj };
                }
                var columns = header.Split(',');

                string line;
                bool hasData = false;
                while ((line = reader.ReadLine()) != null)
                {
                    hasData = true;
                    var values = ParseCsvLine(line, columns.Length);
                    var obj = new T();
                    for (int i = 0; i < columns.Length && i < values.Length; i++)
                    {
                        if (displayNameToProp.TryGetValue(columns[i], out var prop) && !string.IsNullOrEmpty(values[i]))
                        {
                            try
                            {
                                var val = Convert.ChangeType(values[i], Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType, CultureInfo.InvariantCulture);
                                prop.SetValue(obj, val);
                            }
                            catch { /* 忽略转换失败 */ }
                        }
                    }
                    result.Add(obj);
                }
                // 若没有数据行，则默认增加一行
                if (!hasData)
                {
                    reader.Close();
                    using (var writer = new StreamWriter(fileName, true, Encoding.UTF8))
                    {
                        writer.WriteLine(string.Join(",", displayNameToProp.Keys.Select(_ => "")));
                    }
                    var defaultObj = new T();
                    result.Add(defaultObj);
                }
            }
            totalCount = result.Count;
            return result;
        }
        // 简单CSV行解析（支持逗号和引号转义）
        private string[] ParseCsvLine(string line, int columnCount)
        {
            var values = new List<string>();
            bool inQuotes = false;
            var value = new StringBuilder();
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '\"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '\"')
                    {
                        value.Append('\"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    values.Add(value.ToString());
                    value.Clear();
                }
                else
                {
                    value.Append(c);
                }
            }
            values.Add(value.ToString());
            // 补齐列数
            while (values.Count < columnCount) values.Add("");
            return values.ToArray();
        }

        public int InsertOrUpdateNew<T>(IEnumerable<T> entities) where T : class
        {
            var path = _dBFactory.GetCsvDir();
            if (string.IsNullOrEmpty(path))
            {
                throw new FriendlyException("CSV目录未配置，请检查！");
            }

            var type = typeof(T);
            var fileName = System.IO.Path.Combine(path, $"{type.Name}.csv");

            try
            {
                var props = type.GetProperties()
                    .Where(p => p.CanRead && p.PropertyType.IsSerializable && p.GetIndexParameters().Length == 0)
                    .ToArray();

                // 获取DisplayName或属性名
                var headers = props.Select(p =>
                {
                    var displayAttr = p.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.DisplayAttribute), false)
                        .FirstOrDefault() as System.ComponentModel.DataAnnotations.DisplayAttribute;
                    return displayAttr != null && !string.IsNullOrEmpty(displayAttr.Name) ? displayAttr.Name : p.Name;
                }).ToArray();

                using (var writer = new System.IO.StreamWriter(fileName, false, Encoding.UTF8))
                {
                    // 写入表头（DisplayName）
                    writer.WriteLine(string.Join(",", headers));

                    // 写入数据
                    foreach (var entity in entities)
                    {
                        var values = props.Select(p =>
                        {
                            var val = p.GetValue(entity, null);
                            if (val == null) return "";
                            var str = val.ToString();
                            // 转义逗号和引号
                            if (str.Contains(",") || str.Contains("\""))
                            {
                                str = "\"" + str.Replace("\"", "\"\"") + "\"";
                            }
                            return str;
                        });
                        writer.WriteLine(string.Join(",", values));
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                throw new FriendlyException($"实体{typeof(T).Name}批量写入CSV失败: {ex.Message}");
            }

        }

        /// <summary>
        /// 批量更新实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Ts"></param>
        public async Task<int> BatchUpdateAsync<T>(IEnumerable<T> entities) where T : class
        {
            try
            {
                return await _dBFactory.GetDbConnection().Update<T>().SetSource(entities).ExecuteAffrowsAsync();
            }
            catch
            {
                throw new FriendlyException($"实体{entities.FirstOrDefault().GetType().Name}批量更新失败，请检查数据库连接！");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fun"></param>
        /// <returns></returns>
        public long Count<T>(Expression<Func<T, bool>> where) where T : class
        {
            try
            {
                return _dBFactory.GetDbConnection().Select<T>().Where(where).Count();
            }
            catch
            {
                throw new FriendlyException($"查询数据总数失败，请检查数据库连接！");
            }
        }


        /// <summary>
        /// 根据Id查询数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public T Get<T>(long id) where T : class
        {
            try
            {
                return _dBFactory.GetDbConnection().Select<T>(id).ToOne();
            }
            catch
            {
                throw new FriendlyException($"查询数据失败，请检查数据库连接！");
            }
        }

        /// <summary>
        /// 根据筛选条件获取数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where"></param>
        /// <param name="sortFun"></param>
        /// <param name="desc"></param>
        /// <returns></returns>
        public IEnumerable<T> GetList<T>(Expression<Func<T, bool>> where, Expression<Func<T, object>> sortFun, bool desc = true) where T : class
        {
            try
            {
                if (desc)
                {
                    return _dBFactory.GetDbConnection().Select<T>().Where(where).OrderByDescending(sortFun).ToList();
                }
                else
                {
                    return _dBFactory.GetDbConnection().Select<T>().Where(where).OrderBy(sortFun).ToList();
                }
            }
            catch
            {
                throw new FriendlyException($"查询数据失败，请检查数据库连接！");
            }
        }

        /// <summary>
        /// 获取分页数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where"></param>
        /// <param name="sortFun"></param>
        /// <param name="pageIndex"></param>
        /// <param name="perPageCount"></param>
        /// <param name="totalCount"></param>
        /// <param name="desc"></param>
        /// <returns></returns>
        public IEnumerable<T> GetPage<T>(Expression<Func<T, bool>> where, Expression<Func<T, object>> sortFun, int pageIndex, int perPageCount, out long totalCount, bool desc = true) where T : class
        {
            try
            {
                if (desc)
                {
                    return _dBFactory.GetDbConnection().Select<T>().Where(where).OrderByDescending(sortFun).Count(out totalCount).Page(pageIndex, perPageCount).ToList();
                }
                else
                {
                    return _dBFactory.GetDbConnection().Select<T>().Where(where).OrderBy(sortFun).Count(out totalCount).Page(pageIndex, perPageCount).ToList();
                }
            }
            catch
            {
                throw new FriendlyException($"查询数据失败，请检查数据库连接！");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where"></param>
        /// <param name="sortFun"></param>
        /// <param name="pageIndex"></param>
        /// <param name="perPageCount"></param>
        /// <param name="totalCount"></param>
        /// <param name="desc"></param>
        /// <returns></returns>
        /// <exception cref="FriendlyException"></exception>
        public IEnumerable<T> GetPage_Ass<T>(Expression<Func<T, bool>> where, Expression<Func<T, object>> sortFun, int pageIndex, int perPageCount, out long totalCount, bool desc = false) where T : class
        {
            try
            {
                return _dBFactory.GetDbConnection_Ass().Select<T>().Where(where).OrderBy(sortFun).Count(out totalCount).Page(pageIndex, perPageCount).ToList();
            }
            catch
            {
                throw new FriendlyException($"查询数据失败，请检查数据库连接！");
            }
        }

        // 查询全部数据
        public IEnumerable<T> GetAllData<T>(int pageIndex, int perPageCount, out long totalCount) where T : class
        {
            var re = _dBFactory.GetDbConnection_Ass().Select<T>().Count(out totalCount).Page(pageIndex, perPageCount).ToList();
            return re;

        }

        /// <summary>
        /// 执行SQL语句
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <returns></returns>
        public IEnumerable<dynamic> Query<T>(string sql) where T : class
        {
            try
            {
                return _dBFactory.GetDbConnection().Queryable<T>().WithSql(sql).ToList();
            }
            catch
            {
                throw new FriendlyException($"查询数据失败，请检查数据库连接！");
            }
        }

        /// <summary>
        /// 保存数据
        /// </summary>
        /// <param name="aoiData"></param>
        public void SaveAOIResult(TbAOIResult aoiData)
        {
            //var id = Insert(aoiData);
            //foreach (var img in aoiData.Images)
            //{
            //    img.ParentID = id;
            //    var tbImg = Insert(img);

            //    foreach (var defect in img.Defects)
            //    {
            //        defect.ImageID = tbImg;
            //        Insert(defect);
            //    }
            //}

            //return;



            var freeSql = _dBFactory.GetDbConnection();


            var aoiRepository = freeSql.GetRepository<TbAOIResult, long>();
            var imgRepository = freeSql.GetRepository<TbAOIImage, long>();
            var defectRepository = freeSql.GetRepository<TbAOIDefect, long>();

            // 创建单元
            using (var uow = freeSql.CreateUnitOfWork())
            {
                // 1.AOI Data
                aoiRepository.UnitOfWork = uow;

                // 2.图像数据
                imgRepository.UnitOfWork = uow;

                // 3.瑕疵数据
                defectRepository.UnitOfWork = uow;

                var tbAOIData = aoiRepository.Insert(aoiData);
                foreach (var img in aoiData.Images)
                {
                    img.ParentID = tbAOIData.ID;
                    var tbImg = imgRepository.Insert(img);

                    foreach (var defect in img.Defects)
                    {
                        defect.ImageID = tbImg.ID;
                        defectRepository.Insert(defect);
                    }
                }

                // 统一提交事务
                uow.Commit();
            }
        }


        /// <summary>
        /// 保存载具信息
        /// </summary>
        /// <param name="carrierId"></param>
        public void SaveNGCarrierId(TbNGCarrirerData carrierData)
        {
            var freeSql = _dBFactory.GetDbConnection();
            var carrierRepository = freeSql.GetRepository<TbNGCarrirerData, long>();
            // 创建单元
            using (var uow = freeSql.CreateUnitOfWork())
            {
                // Carrier Data
                carrierRepository.UnitOfWork = uow;
                carrierRepository.Insert(carrierData);
                // 提交事务
                uow.Commit();
            }
        }

        /// <summary>
        /// 清空表并批量插入新数据（全量覆盖）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        public void OverwriteAll<T>(IEnumerable<T> entities) where T : class
        {
            var db = _dBFactory.GetDbConnection_Ass();
            using (var uow = db.CreateUnitOfWork())
            {
                try
                {
                    // 清空表
                    db.Delete<T>().Where("1=1").ExecuteAffrows();
                    // 插入新数据
                    db.Insert(entities).ExecuteAffrows();
                    uow.Commit();
                }
                catch
                {
                    uow.Rollback();
                    throw new FriendlyException($"实体{typeof(T).Name}全量覆盖失败，请检查数据库连接！");
                }
            }
        }
    }
}
