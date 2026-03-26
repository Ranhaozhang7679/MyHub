#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       CacheDataContentVM
* 机器名称:       Z05592
* 命名空间:       Luster.Motion.CommonUI.ViewModel
* 文 件 名:       CacheDataContentVM.cs
* 创建时间:       2022/11/22 16:32:52
* 作    者:       Z05592
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       pangpangzhang@lusterinc.com 
* 唯一标识：      aa57282a-9bdd-4f40-98b5-8840b9e6b35c
* 登录用户:       张庞庞
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/11/22 16:32:52
* 修 改 人:		  Z05592
************************************************************************************/
#endregion
using Luster.Motion.CommonUI.Events;
using Luster.Motion.CommonUI.Models;
using Luster.Motion.TaskFlow.Engine;
using Luster.Motion.TaskFlow.Engine.Models;
using Luster.TaskFlow.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Luster.Motion.CommonUI.ViewModel
{
    public class CacheDataContentVM : MotionVM
    {
        private List<MapData> _mapDatas;
        private IMotionEngine _motionEngine;
        private ICacheManager _cacheManager;
        private Dispatcher _dispatcher;
        /// <summary>
        /// 列表头动态变化事件
        /// </summary>
        public event Action DataHeaderChangedEvent;

        private List<string> _dataheaders;
        /// <summary>
        /// 输出数据名称
        /// </summary>
        public List<string> DataHeaders
        {
            get { return _dataheaders; }
            set
            {
                if (_dataheaders != value)
                {
                    _dataheaders = value;
                    DataHeaderChangedEvent?.Invoke();
                }
            }
        }

        /// <summary>
        /// 缓存数据items
        /// </summary>
        private ObservableCollection<CacheDataItem> _cacheDatas;
        public ObservableCollection<CacheDataItem> CacheDatas
        {
            get => _cacheDatas;
            set => SetProperty(ref _cacheDatas, value);
        }

        public CacheDataContentVM(ICommonBus _commonBus, Dispatcher dispatcher, ICacheManager cacheManager,
            IMotionEngine engine) : base(_commonBus)
        {
            _cacheManager = cacheManager;
            _motionEngine = engine;

            _cacheManager.ItemAddEvent -= CacheManager_ItemAddEvent;
            _cacheManager.ItemAddEvent += CacheManager_ItemAddEvent;

            _cacheManager.ItemRemoveEvent -= CacheManager_ItemRemoveEvent;
            _cacheManager.ItemRemoveEvent += CacheManager_ItemRemoveEvent;

            _cacheManager.ItemUpdateEvent -= CacheManager_ItemUpdateEvent;
            _cacheManager.ItemUpdateEvent += CacheManager_ItemUpdateEvent;

            _cacheManager.RemoveAllEvent -= CacheManager_RemoveAllEvent;
            _cacheManager.RemoveAllEvent += CacheManager_RemoveAllEvent;

            _dispatcher = dispatcher;
            commonBus.EventBus.GetEvent<MapDataEvent>().Subscribe(MapDataChanged);
            CacheDatas = new ObservableCollection<CacheDataItem>();
            SetDataHeader();
            InitModel();
        }

        /// <summary>
        /// 清错页面缓存数据
        /// </summary>
        private void CacheManager_RemoveAllEvent()
        {
            _dispatcher.Invoke(() =>
            {
                CacheDatas.Clear();
            });
        }

        /// <summary>
        /// 获取缓存下所有数据
        /// </summary>
        private void InitModel()
        {
            var cacheDatas = _cacheManager.GetAllData();
            foreach (var data in cacheDatas)
            {
                if (data is CacheItem cacheData)
                {
                    var cacheDataItem = CreateItem(cacheData);
                    CacheDatas.Add(cacheDataItem);
                }
            }
        }

        /// <summary>
        /// 初始化数据字典
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, object> GetInitDataDic()
        {
            // 不管Data是否为空，都构建表头数据,空数据只构建一次
            var dic = new Dictionary<string, object>();
            if (DataHeaders != null && DataHeaders.Count > 0)
            {
                foreach (var header in DataHeaders)
                {
                    if (!dic.ContainsKey(header))
                    {
                        dic.Add(header, "");
                    }
                }
            }
            return dic;
        }
        /// <summary>
        /// 缓存数据更新
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cacheItem"></param>
        private void CacheManager_ItemUpdateEvent(string id, object cacheItem)
        {
            _dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                if (cacheItem is CacheItem cacheData)
                {
                    var newItem = CreateItem(cacheData);
                    var item = CacheDatas.FirstOrDefault(x => x.DataId == cacheData.DataID);
                    if (item != null)
                    {
                        var index = CacheDatas.IndexOf(item);
                        CacheDatas.RemoveAt(index);
                        CacheDatas.Insert(index, newItem);
                    }
                }
            }));
        }

        /// <summary>
        /// 新建数据对象
        /// </summary>
        /// <param name="cacheData"></param>
        /// <returns></returns>
        private CacheDataItem CreateItem(CacheItem cacheData)
        {
            var initDataDic = GetInitDataDic();
            foreach (var item in cacheData)
            {
                var mapData = _mapDatas.FirstOrDefault(x => x.ID == item.ID && x.Key == item.ParamKey);
                if (mapData != null)
                {
                    if (initDataDic.ContainsKey(mapData.DataSource))
                    {
                        initDataDic[mapData.DataSource] = item.Value;
                    }
                }
            }
            var cacheDataItem = new CacheDataItem()
            {
                CreateTime = cacheData.CreateTime,
                Data = initDataDic,
                DataId = cacheData.DataID,
                IsRemoved = false
            };
            return cacheDataItem;
        }

        /// <summary>
        /// 移除缓存数据
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cacheItem"></param>
        private void CacheManager_ItemRemoveEvent(string id, object cacheItem)
        {
            _dispatcher.Invoke(() =>
            {
                if (cacheItem is CacheItem cacheData)
                {
                    var item = CacheDatas.FirstOrDefault(x => x.DataId == cacheData.DataID);
                    if (item != null)
                    {
                        item.IsRemoved = true;
                    }
                }
            });
        }

        /// <summary>
        /// 新增缓存数据
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cacheItem"></param>
        private void CacheManager_ItemAddEvent(string id, object cacheItem)
        {
            _dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                if (cacheItem is CacheItem cacheData)
                {
                    var cItem = cacheData.Clone() as CacheItem;
                    var cacheDataItem = CreateItem(cItem);
                    if (CacheDatas.Count >= 100)
                    {
                        CacheDatas.RemoveAt(CacheDatas.Count - 1);
                    }
                    // 倒序显示 新数据插入第一位
                    CacheDatas.Insert(0, cacheDataItem);
                }
            }));
        }

        /// <summary>
        /// 输出项变化
        /// </summary>
        /// <param name="obj"></param>
        private void MapDataChanged(MapData obj)
        {
            SetDataHeader();
        }

        /// <summary>
        /// 设置表头
        /// </summary>
        private void SetDataHeader()
        {
            _dispatcher.Invoke(() =>
            {
                _mapDatas = commonBus.GetMapDatas();
                DataHeaders = commonBus.GetMapDatas().Select(x => x.DataSource).ToList();
            });
        }

    }
}
