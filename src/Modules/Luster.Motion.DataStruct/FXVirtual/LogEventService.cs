using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace Luster.Motion.DataStruct.FXVirtual
{
    public class LogEventService
    {
        private static readonly Lazy<LogEventService> _instance =
            new Lazy<LogEventService>(() => new LogEventService());

        public static LogEventService Instance => _instance.Value;

        private readonly List<Action<string>> _logSubscribers =
            new List<Action<string>>();

        public event Action<string> LogEvent
        {
            add => _logSubscribers.Add(value);
            remove => _logSubscribers.Remove(value);
        }

        public void PublishLog(string message)
        {
            foreach (var subscriber in _logSubscribers)
            {
                try
                {
                    subscriber?.Invoke(message);
                }
                catch
                {
                    // 忽略订阅者处理错误
                }
            }
        }
    }
}
