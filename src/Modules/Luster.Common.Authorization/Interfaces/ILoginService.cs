using DC.Authorization.Models;
using System;

namespace DC.Authorization
{
    /// <summary>
    /// 登录服务接口
    /// </summary>
    public interface ILoginService
    {
        /// <summary>是否已登录</summary>
        bool HasLogin { get; }

        /// <summary>当前登录账号（未登录时为 null）</summary>
        Account? Current { get; }

        /// <summary>密码登录</summary>
        (bool Succeeded, string Message, string HiveLevel) Login(string username, string password, int targetLevel = 4);

        /// <summary>注销</summary>
        void Logout();

        event EventHandler<EventArgs>? OnLogout;

        /// <summary>允许登录标志（控制刷卡登录是否生效）</summary>
        bool LoginAllowed { get; set; }

        /// <summary>
        /// 用户在UI上选择的目标权限等级（Role.Level）。
        /// 刷卡验证时传给 HiveAuthProvider 做权限矩阵校验。
        /// </summary>
        int? TargetRoleLevel { get; set; }


        string LastCardNo { get; }
        // ─── 刷卡结果展示字段 ────────────────────────────────────────────────
        /// <summary>最后一次刷卡—Hive 返回的用户姓名</summary>
        string LastCardUserName { get; }
        /// <summary>最后一次刷卡—Hive 返回的厂商/部门</summary>
        string LastCardVendor { get; }
        /// <summary>最后一次刷卡—Hive 返回的设备等级（如 "L8"）</summary>
        string LastCardDeviceLevel { get; }
        /// <summary>最后一次认证的消息文本（用于MES状态框）</summary>
        string LastAuthMessage { get; }

        // ─── 事件 ────────────────────────────────────────────────────────────
        /// <summary>刷卡登录成功事件</summary>
        event EventHandler<EventArgs> OnCardLogin;

        /// <summary>密码登录成功事件</summary>
        event EventHandler<EventArgs> OnPasswordLogin;

        /// <summary>
        /// 刷卡状态更新事件（验证中或验证失败时触发）。
        /// ViewModel 订阅此事件以实时更新 MES 状态文本框和用户信息字段。
        /// </summary>
        event EventHandler<EventArgs> OnCardStatusUpdated;
    }
}
