#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       IRepository
* 机器名称:       Z05592
* 命名空间:       Luster.Common.DataAccess.Repositories
* 文 件 名:       IRepository.cs
* 创建时间:       2022/10/14 10:02:46
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      8d313371-d3a3-4b6a-9920-ef08f3fb7b38
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/10/14 10:02:46
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using FreeSql;
using Luster.Common.DataAccess.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.DataAccess.Repositories
{
    /// <summary>
    /// 数据仓储
    /// </summary>
    public interface IRepository 
    {
        /// <summary>
        /// 新增插入实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        long Insert<T>(T entity) where T : class;

        /// <summary>
        /// 批量新增插入实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        int BatchInsert<T>(IEnumerable<T> entities) where T : class;


        /// <summary>
        /// 异步批量新增插入实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task<int> BatchInsertAsync<T>(IEnumerable<T> entities) where T : class;

        /// <summary>
        /// 删除单个实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        int Delete<T>(T entity) where T : class;

        /// <summary>
        /// 根据Id删除单个实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="long"></param>
        int DeleteById<T>(long id) where T : class;

        /// <summary>
        /// 批量删除实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        void BatchDelete<T>(IEnumerable<T> entities) where T : class;

        /// <summary>
        /// 异步批量删除实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        void BatchDeleteAsync<T>(IEnumerable<T> entities) where T : class;


        /// <summary>
        /// 更新单个实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        int Update<T>(T entity) where T : class;

        /// <summary>
        /// 通过条件更新
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="whereObj"></param>
        /// <returns></returns>
        int Update<T>(T entity, params string[] columns) where T : class;

        /// <summary>
        /// 批量更新实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Ts"></param>
        int BatchUpdate<T>(IEnumerable<T> entities) where T : class;

        /// <summary>
        /// 批量更新实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Ts"></param>
        Task<int> BatchUpdateAsync<T>(IEnumerable<T> entities) where T : class;

        /// <summary>
        /// 更加Id查询实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        T Get<T>(long id) where T : class;

        /// <summary>
        /// 根据筛选条件获取实体数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where">限制条件</param>
        /// <param name="sortFun">排序条件</param>
        /// <param name="desc">倒序</param>
        /// <returns></returns>
        IEnumerable<T> GetList<T>(Expression<Func<T, bool>> where, Expression<Func<T, object>> sortFun, bool desc = true) where T : class;

        /// <summary>
        /// 获取分页数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fun">限制条件</param>
        /// <param name="where">排序条件</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="perPageCount">每页数量</param>
        /// <param name="totalCount">总数</param>
        /// <param name="desc">降序</param>
        /// <returns></returns>
        IEnumerable<T> GetPage<T>(Expression<Func<T, bool>> where, Expression<Func<T, object>> sortFun, int pageIndex, int perPageCount, out long totalCount, bool desc = true) where T : class;

        /// <summary>
        ///  获取分页数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where"></param>
        /// <param name="sortFun"></param>
        /// <param name="pageIndex"></param>
        /// <param name="perPageCount"></param>
        /// <param name="totalCount"></param>
        /// <param name="desc"></param>
        /// <returns></returns>
        IEnumerable<T> GetPage_Ass<T>(Expression<Func<T, bool>> where, Expression<Func<T, object>> sortFun, int pageIndex, int perPageCount, out long totalCount, bool desc = true) where T : class;

        
        /// <summary>
        /// 读取数据数量
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fun">限制条件</param>
        /// <returns></returns>
        long Count<T>(Expression<Func<T, bool>> fun) where T : class;

        /// <summary>
        /// 使用SQL语句查询实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="fun">条件</param>
        /// <param name="sortFun">排序条件</param>
        /// <param name="desc">降序</param>
        /// <returns></returns>
        IEnumerable<dynamic> Query<T>(string sql) where T : class;

        /// <summary>
        /// 保存数据
        /// </summary>
        /// <param name="aoiData"></param>
        void SaveAOIResult(TbAOIResult aoiData);


        /// <summary>
        /// 保存载具数量
        /// </summary>
        /// <param name="carrierData"></param>
        void SaveNGCarrierId(TbNGCarrirerData carrierData);
    }
}
