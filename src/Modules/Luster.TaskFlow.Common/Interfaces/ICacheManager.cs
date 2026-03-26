#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       IExpand
* 机器名称:       L05123-NB
* 命名空间:       Luster.TaskFlow.Common.Interfaces
* 文 件 名:       IExpand
* 创建时间:       2021/10/31 15:01:14
* 作    者:       luster
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      2155e45a-79bc-4743-bf1d-d91ccff7b08c
* 登录用户:       darkliu
* 所 属 域:       L05123-NB
* 创建年份:       2021
* 修改时间:		  2021/10/31 15:01:14
* 修 改 人:		  luster
************************************************************************************/
#endregion

using Luster.Common.DataStruct.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.TaskFlow.Common.Interfaces
{
    /// <summary>
    /// 需要将类型注册进
    /// </summary>
    public interface ICacheManager
    {
        /// <summary>
        /// 新增事件
        /// </summary>
        event Action<string, object> ItemAddEvent;

        /// <summary>
        /// 移除事件
        /// </summary>
        event Action<string, object> ItemRemoveEvent;

        /// <summary>
        /// 变更事件
        /// </summary>
        event Action<string, object> ItemUpdateEvent;

        /// <summary>
        /// 删除所有事件
        /// </summary>
        event Action RemoveAllEvent;

        /// <summary>
        /// 获取缓存数据
        /// </summary>
        /// <param name="dataID">数据行</param>
        /// <param name="moduleID">模块ID</param>
        /// <param name="paramKey">参数Key</param>
        /// <returns>缓存值</returns>
        public object GetCache(string dataID, Guid moduleID, string paramKey);

        /// <summary>
        /// 通过列名来获取
        /// </summary>
        /// <param name="dataId"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        public object GetCacheByAlias(string dataId, string alias);

        /// <summary>
        /// 新缓存
        /// </summary>
        /// <param name="dataID">唯一ID</param>
        void AddItem(string dataID);

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dataID">数据行</param>
        /// <param name="moduleID">模块ID</param>
        /// <param name="datas">数据对象</param>
        void AddItemData(string dataID, Guid moduleID, List<LColumn> datas);

        /// <summary>
        /// 移除数据行
        /// </summary>
        /// <param name="dataID">删除对象</param>
        void RemoveItem(string dataID);

        /// <summary>
        /// 获取完整记录
        /// </summary>
        /// <param name="dataID">唯一ID</param>
        /// <returns>完整对象</returns>
        object GetItem(string dataID);

        /// <summary>
        /// 删除所有
        /// </summary>
        void RemoveAll();

        /// <summary>
        /// 获取所有数据IDs
        /// </summary>
        /// <returns></returns>
        IEnumerable<object> GetAllData();
    }
}