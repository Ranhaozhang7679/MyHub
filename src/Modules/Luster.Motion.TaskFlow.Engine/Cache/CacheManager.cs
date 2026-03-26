#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       PlcConfig
* 机器名称:       L05123-NB
* 命名空间:       Luster.Motion.TaskFlow.Engine.Models
* 文 件 名:       PlcConfig.cs
* 创建时间:       2022/9/1 14:37:58
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      22b038f0-c587-4bc6-a16b-ec454a8e21d1
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/1 14:37:58
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using Luster.Common.DataStruct;
using Luster.Common.DataStruct.Attributes;
using Luster.Common.DataStruct.DataModels;
using Luster.Common.DataStruct.Extensions;
using Luster.TaskFlow.Common;
using Luster.TaskFlow.Common.Attributes;
using Luster.TaskFlow.Common.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Luster.Motion.TaskFlow.Engine.Models
{
    /// <summary>
    /// 缓存管理
    /// </summary>
    public class CacheManager : ICacheManager
    {
        /// <summary>
        /// 缓存Keys
        /// </summary>
        private List<string> keys = new List<string>();

        #region 事件
        /// <summary>
        /// 新增Item
        /// </summary>
        public event Action<string, object> ItemAddEvent;

        /// <summary>
        /// 删除Item
        /// </summary>
        public event Action<string, object> ItemRemoveEvent;

        /// <summary>
        /// 删除更新
        /// </summary>
        public event Action<string, object> ItemUpdateEvent;

        /// <summary>
        /// 删除所有事件
        /// </summary>
        public event Action RemoveAllEvent;
        #endregion

        /// <summary>
        /// 缓存数据
        /// </summary>
        private ConcurrentDictionary<string, CacheItem> cacheDatas;

        /// <summary>
        /// 构造函数
        /// </summary>
        public CacheManager()
        {
            cacheDatas = new ConcurrentDictionary<string, CacheItem>();
            keys = new List<string>();
        }

        /// <summary>
        /// 新增数据列,入料事件时触发
        /// </summary>
        /// <param name="dataID"></param>
        public void AddItem(string dataID)
        {
            var newItem = new CacheItem(dataID);

            // 如果存在相同产品问题，但是被标记删除，就删除该缓存
            if (cacheDatas.TryGetValue(dataID, out var cItem) && cItem.IsDeleted)
            {
                cacheDatas.TryRemove(dataID, out cItem);
            }

            if (cacheDatas.TryAdd(dataID, newItem))
            {
                ItemAddEvent?.Invoke(dataID, newItem);
            }
        }

        public void RemoveItem(string dataID)
        {
            if (cacheDatas.TryGetValue(dataID, out var newItem))
            {
                newItem.IsDeleted = true;
                keys.Add(dataID);

                ItemRemoveEvent?.Invoke(dataID, newItem);
            }

            if (keys.Count > 50)
            {
                var removeID = keys[0];
                if (cacheDatas.TryRemove(removeID, out var cItem))
                {
                    keys.RemoveAt(0);
                }
            }
        }

        /// <summary>
        /// 获取缓存的对象
        /// </summary>
        /// <param name="dataID"></param>
        /// <param name="moduleID"></param>
        /// <param name="paramKey"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public object GetCache(string dataID, Guid moduleID, string paramKey)
        {
            if (cacheDatas.TryGetValue(dataID, out var cItem))
            {
                var pItem = cItem.FirstOrDefault(u => u.ID == moduleID && u.ParamKey == paramKey);
                if (pItem is null)
                {
                    return null;
                }
                else
                {
                    return pItem.Value;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dataID">数据列</param>
        /// <param name="moduleID">模块ID</param>
        /// <param name="datas">数据对象</param>
        public void AddItemData(string dataID, Guid moduleID, List<LColumn> datas)
        {
            // 防止多线程上传数据的同时，又对List集合做了变更，造成程序异常
            var cDatas = new List<LColumn>(datas);
            if (cacheDatas.TryGetValue(dataID, out var cItems))
            {
                foreach (var cItem in cDatas)
                {
                    if (cItems.Any(u => u == cItem))
                    {
                        continue;
                    }

                    cItems.Add(cItem);
                }

                ItemUpdateEvent?.Invoke(dataID, cItems);
            }
        }

        /// <summary>
        /// 获取当前的所有数据
        /// </summary>
        /// <param name="dataID"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public object GetItem(string dataID)
        {
            if (cacheDatas.TryGetValue(dataID, out var cItem))
            {
                return cItem;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 删除所有
        /// </summary>
        public void RemoveAll()
        {
            cacheDatas.Clear();
            RemoveAllEvent?.Invoke();
        }

        /// <summary>
        /// 获取所有数据
        /// </summary>
        /// <returns></returns>
        public IEnumerable<object> GetAllData()
        {
            return cacheDatas.Values;
        }

        /// <summary>
        /// 通过别名获取缓存信息
        /// </summary>
        /// <param name="dataID"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        public object GetCacheByAlias(string dataID, string alias)
        {
            if (cacheDatas.TryGetValue(dataID, out var cItem))
            {
                var pItem = cItem.FirstOrDefault(u => !string.IsNullOrEmpty(u.Alias) && u.Alias == alias);
                if (pItem is null)
                {
                    return null;
                }
                else
                {
                    return pItem.Value;
                }
            }
            else
            {
                return null;
            }
        }
    }
}