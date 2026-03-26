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

        // 状态更新事件
        public event Action<string, string> StatusChanged;

        private PageStatusService() { }

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