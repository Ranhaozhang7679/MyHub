using Luster.Motion.DigitalSetup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.DigitalSetup.ViewModel
{
    /// <summary>
    /// 页面状态存储服务（单例）
    /// </summary>
    public class PageStatusService
    {
        private static PageStatusService _instance;
        private static readonly object _lock = new object();

        // 存储各页面的状态
        private Dictionary<string, string> _statusCache = new Dictionary<string, string>();

        // 持久化服务
        private PageStatusPersistenceService _persistenceService;

        // 标记是否已开始加载数据
        private bool _hasStartedLoading = false;

        // 状态更新事件
        public event Action<string, string> StatusChanged;

        private PageStatusService()
        {
            // 初始化持久化服务
            _persistenceService = new PageStatusPersistenceService();

            // 异步加载持久化数据，不阻塞构造函数
            StartLoadPersistedStatus();
        }

        public static PageStatusService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new PageStatusService();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// 设置配置文件路径（基于配方路径）
        /// </summary>
        /// <param name="recipePath">配方路径</param>
        public void SetConfigPath(string recipePath)
        {
            lock (_lock)
            {
                _persistenceService?.SetConfigPath(recipePath);
            }
            // 异步重新加载数据，不阻塞调用线程
            StartLoadPersistedStatus();
        }

        /// <summary>
        /// 启动异步加载持久化状态
        /// </summary>
        private void StartLoadPersistedStatus()
        {
            // 防止重复加载
            if (_hasStartedLoading)
                return;

            _hasStartedLoading = true;

            // 使用后台线程异步加载，不阻塞 UI
            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    var data = _persistenceService?.Load();
                    if (data != null && data.StatusCache != null)
                    {
                        lock (_lock)
                        {
                            // 合并持久化数据到缓存（不覆盖已存在的状态）
                            foreach (var kvp in data.StatusCache)
                            {
                                if (!_statusCache.ContainsKey(kvp.Key))
                                {
                                    _statusCache[kvp.Key] = kvp.Value;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"加载持久化状态失败: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// 保存状态到持久化存储
        /// </summary>
        private void SavePersistedStatus()
        {
            try
            {
                Dictionary<string, string> snapshot;
                lock (_lock)
                {
                    snapshot = new Dictionary<string, string>(_statusCache);
                }

                var data = new Datas.PageStatusPersistenceData
                {
                    StatusCache = snapshot
                };
                _persistenceService?.Save(data);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存持久化状态失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 更新页面状态
        /// </summary>
        public void UpdateStatus(string pageName, string status)
        {
            lock (_lock)
            {
                _statusCache[pageName] = status;
            }
            // 触发事件通知订阅者
            StatusChanged?.Invoke(pageName, status);

            // 保存到持久化存储（异步执行，不阻塞调用）
            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    SavePersistedStatus();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"异步保存状态失败: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// 获取页面状态
        /// </summary>
        public string GetStatus(string pageName)
        {
            lock (_lock)
            {
                if (_statusCache.ContainsKey(pageName))
                {
                    return _statusCache[pageName];
                }
                return "未点检";
            }
        }

        /// <summary>
        /// 获取所有页面状态
        /// </summary>
        public Dictionary<string, string> GetAllStatus()
        {
            lock (_lock)
            {
                return new Dictionary<string, string>(_statusCache);
            }
        }
    }
}
