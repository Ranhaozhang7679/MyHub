using DC.Authorization;
using DC.Authorization.Models;
using DC.Authorization.WPF.Infrastructure;
using DC.Authorization.WPF.Repositories;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Threading.Tasks;

namespace DC.Authorization.WPF.Providers
{
    /// <summary>
    /// 本地认证提供者：基于 SQLite 数据库验证用户名+密码 或 卡号
    /// </summary>
    public class LocalAuthProvider : IAuthProvider
    {
        private readonly IAccountRepository _accountRepository;

        public LocalAuthProvider(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        /// <summary>本地 Provider 始终可用</summary>
        public bool IsAvailable => true;

        public Task<AuthResult> AuthenticateAsync(AuthCredential credential)
        {
            AuthResult result;

            switch (credential.Method)
            {
                case AuthMethod.Password:
                    {
                        var (account, succeeded, message) = _accountRepository.Login(
                            credential.CardNo!, credential.Password!);
                        if (!succeeded)
                        {
                            return Task.FromResult(new AuthResult
                            {
                                Success = false,
                                Message = message
                            });
                        }

                        // ─── Step 3: 权限矩阵校验（四级权限表）──────────────────────────
                        int targetRoleLevel = credential.TargetRoleLevel ?? 4;

                        // ─── Step 5: 取该角色的默认账号（Hive 已完成身份认证，本地只需角色对应的占位账号）
                        var allAccounts = _accountRepository.Load(false);
                        var accountInfo = allAccounts.FirstOrDefault(a => a.RoleId == targetRoleLevel);

                        if (accountInfo == null)
                        {
                            return Task.FromResult(new AuthResult
                            {
                                Success = false,
                                Message = message
                            });
                        }

                        result = new AuthResult
                        {
                            Success = succeeded,
                            Message = message,
                            Account = accountInfo
                        };
                        break;
                    }
                case AuthMethod.CardSwipe:
                    {
                        var (account, succeeded, message) = _accountRepository.Login(credential.CardNo!);
                        result = new AuthResult
                        {
                            Success = succeeded,
                            Message = message,
                            Account = account
                        };
                        break;
                    }
                default:
                    result = new AuthResult { Success = false, Message = "不支持的认证方式" };
                    break;
            }

            return Task.FromResult(result);
        }

        /// <summary>
        /// DeviceLevel 字符串 → 该等级可授权的最高 Role.Level
        /// Role.Level 约定：数值越小权限越高。
        ///   1 = Administrator
        ///   2 = Integrator
        ///   3 = Maintenance
        ///   4 = OP Read only
        /// 因此此处存储"最低的 Level 数值"即"最高可授权限"。
        /// </summary>
        private static readonly Dictionary<string, int> DeviceLevelToMaxRoleLevel =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                { "L1", 2 },  // OP / Maintenance / Integrator（不含 Administrator）
                { "L2", 2 },  // OP / Maintenance / Integrator
                { "L3", 2 },  // 全部四级均可（含 Administrator）
                { "L6", 3 },  // OP / Maintenance
                { "L7", 4 },  // 仅 OP Read only
                { "L8", 4 },  // 仅 OP Read only（Line leader）
                { "L9", 3 },  // OP / Maintenance（MFG Technician）
            };

        /// <summary>
        /// 校验 Hive DeviceLevel 是否允许所选 Role.Level（四级权限矩阵）
        /// </summary>
        /// <param name="hiveDeviceLevel">Hive 返回的等级字符串（如 "L8"）</param>
        /// <param name="targetRoleLevel">本地 Role.Level（1=Admin, 2=Integrator, 3=Maintenance, 4=OP）</param>
        /// <param name="message">校验结果说明</param>
        /// <returns>是否有权限</returns>
        private bool CheckPermission(string hiveDeviceLevel, int targetRoleLevel, out string message)
        {
            if (!DeviceLevelToMaxRoleLevel.TryGetValue(hiveDeviceLevel, out int maxGrantableLevel))
            {
                message = $"未知的 Hive 等级: {hiveDeviceLevel}，拒绝访问";
                return false;
            }

            // Role.Level 越小权限越高，因此：
            //   targetRoleLevel >= maxGrantableLevel 表示目标权限不高于可授权上限 → 允许
            //   targetRoleLevel <  maxGrantableLevel 表示目标权限超出可授范围 → 拒绝
            if (targetRoleLevel >= maxGrantableLevel)
            {
                message = $"权限校验通过 ({hiveDeviceLevel} 可授予 Level {targetRoleLevel})";
                return true;
            }
            else
            {
                message = $"权限不足 / Insufficient permission\n" +
                          $"您的卡片等级 {hiveDeviceLevel} 最高可授予 Level {maxGrantableLevel}，" +
                          $"无法获得 Level {targetRoleLevel} 权限";
                return false;
            }
        }
    }
}
