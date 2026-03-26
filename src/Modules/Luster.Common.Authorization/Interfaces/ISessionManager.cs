using System;

namespace DC.Authorization
{
    /// <summary>
    /// 会话管理接口（超时自动注销）
    /// </summary>
    public interface ISessionManager
    {
        /// <summary>倒计时秒数（距离会话过期）</summary>
        int CountDown { get; }

        /// <summary>启动会话监控</summary>
        void Start();

        /// <summary>停止会话监控</summary>
        void Stop();

        /// <summary>会话过期事件</summary>
        event EventHandler<EventArgs> SessionExpired;
    }
}
