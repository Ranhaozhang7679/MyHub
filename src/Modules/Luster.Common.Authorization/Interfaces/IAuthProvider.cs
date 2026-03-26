using DC.Authorization.Models;
using System.Threading.Tasks;

namespace DC.Authorization
{
    /// <summary>
    /// 认证方式
    /// </summary>
    public enum AuthMethod
    {
        /// <summary>用户名 + 密码（本地数据库验证）</summary>
        Password,
        /// <summary>刷卡（HiveApi 在线验证）</summary>
        CardSwipe
    }

    /// <summary>
    /// 认证凭据（统一封装不同登录方式的输入）
    /// </summary>
    public class AuthCredential
    {
        /// <summary>用户名（密码登录时使用）</summary>
        public string? Username { get; set; }
        /// <summary>密码（密码登录时使用）</summary>
        public string? Password { get; set; }
        /// <summary>卡号（刷卡登录时使用）</summary>
        public string? CardNo { get; set; }
        /// <summary>认证方式</summary>
        public AuthMethod Method { get; set; }
        /// <summary>
        /// 用户在UI上选择的目标权限等级（刷卡登录时使用）。
        /// Hive 认证时会校验此等级是否在该卡号的可授等级范围内。
        /// </summary>
        public int? TargetRoleLevel { get; set; }
    }

    /// <summary>
    /// 认证结果
    /// </summary>
    public class AuthResult
    {
        /// <summary>是否认证成功</summary>
        public bool Success { get; set; }
        /// <summary>描述信息（成功/失败原因）</summary>
        public string Message { get; set; } = string.Empty;
        /// <summary>HiveApi 返回的等级（仅刷卡登录时有值，用于映射本地角色）</summary>
        public int? RoleLevel { get; set; }
        /// <summary>匹配到的本地账号（登录成功时填充）</summary>
        public Account? Account { get; set; }
        /// <summary>Hive SFC 接口返回的原始设备等级字符串（如 "L8"），供界面展示</summary>
        public string HiveDeviceLevel { get; set; } = string.Empty;
        /// <summary>Hive SFC 接口返回的用户姓名，供界面展示</summary>
        public string CardUserName { get; set; } = string.Empty;
        /// <summary>Hive SFC 接口返回的公司/厂商，供界面展示</summary>
        public string CardVendor { get; set; } = string.Empty;

    }

    /// <summary>
    /// 认证提供者接口（策略模式）
    /// <para>本地验证和 HiveApi 验证各自实现此接口</para>
    /// </summary>
    public interface IAuthProvider
    {
        /// <summary>验证凭据</summary>
        Task<AuthResult> AuthenticateAsync(AuthCredential credential);

        /// <summary>此 Provider 当前是否可用（如网络断开时 HiveApi 不可用）</summary>
        bool IsAvailable { get; }
    }
}
