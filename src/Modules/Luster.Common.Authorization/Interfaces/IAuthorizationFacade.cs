using DC.Authorization.Models;

namespace DC.Authorization
{
    /// <summary>
    /// 权限系统门面接口（对外暴露的核心 API）
    /// </summary>
    public interface IAuthorizationFacade
    {
        /// <summary>注册功能权限项（自动注册时调用）</summary>
        void RegisterRights(Right[] rights);

        /// <summary>权限检查：无权限则会尝试弹出提示窗</summary>
        bool CheckAuth(AuthItem authItem, RightType rightType = RightType.Operation);

        /// <summary>静默权限检查：仅返回布尔值，不弹窗（用于UI控件绑定）</summary>
        bool HasAuth(AuthItem authItem, RightType rightType = RightType.Operation);

        /// <summary>弹出登录窗口（由 UI 层实现）</summary>
        void PopLoginWindow();

        /// <summary>登录弹窗动作委托，允许外部（如ViewModel）注入自定义弹窗逻辑</summary>
        Action PopLoginWindowAction { get; set; }

        /// <summary>弹出无权限提示（由 UI 层实现）</summary>
        void PopNoAuthNotification(AuthItem authItem);

        /// <summary>审计日志</summary>
        void Audit<T>(string operation, string detail, T? before, T? after);

        /// <summary>登录/注销/等级切换时触发，订阅者应刷新权限相关状态</summary>
        event EventHandler? AuthChanged;
    }
}
