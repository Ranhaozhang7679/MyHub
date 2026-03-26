using DC.Authorization;
using DC.Authorization.Models;
using System.Collections.Generic;
using System.Linq;

namespace DC.Authorization.WPF.Infrastructure
{
    /// <summary>
    /// HiveApi 等级 → 本地角色映射
    /// <para>当 HiveApi 返回用户等级后，用此类找到对应的本地角色</para>
    /// </summary>
    public class RoleLevelMapper
    {
        private readonly IRoleRepository _roleRepository;

        public RoleLevelMapper(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        /// <summary>
        /// 根据 HiveApi 返回的等级，找到最接近的本地角色
        /// <para>规则：找 role_level 等于 hiveLevel 的角色；找不到则取 ≥ hiveLevel 中 level 最小的</para>
        /// </summary>
        /// <param name="hiveLevel">HiveApi 返回的等级（1=最高, 5=最低）</param>
        /// <returns>匹配的本地角色，找不到则返回最低权限角色</returns>
        public Role? MapToLocalRole(int hiveLevel)
        {
            var allRoles = _roleRepository.Load();
            if (allRoles == null || allRoles.Count == 0) return null;

            // 精确匹配
            var exact = allRoles.FirstOrDefault(r => r.Level == hiveLevel);
            if (exact != null) return exact;

            // 向上取最近的（权限稍高）
            var higher = allRoles
                .Where(r => r.Level <= hiveLevel)
                .OrderByDescending(r => r.Level)
                .FirstOrDefault();
            if (higher != null) return higher;

            // 兜底：返回最低权限角色
            return allRoles.OrderByDescending(r => r.Level).First();
        }

        /// <summary>
        /// 根据角色创建/查找本地临时账号（用于 HiveApi 刷卡用户）
        /// </summary>
        public Account CreateOrFindHiveAccount(string cardNo, Role role, IAccountRepository accountRepository)
        {
            // 先按卡号（存在 TelNo 字段）查找是否已有本地账号
            var allAccounts = accountRepository.Load(false);
            var existing = allAccounts.FirstOrDefault(a => a.TelNo == cardNo);
            if (existing != null)
            {
                // 更新角色（可能 HiveApi 等级有变化）
                if (existing.RoleId != role.Id)
                {
                    existing.RoleId = role.Id;
                    existing.RoleName = role.Name;
                    existing.IsAdmin = role.IsAdmin;
                    accountRepository.Update(existing);
                }
                return existing;
            }

            // 创建新的 HiveApi 账号
            var newAccount = new Account
            {
                AccName = $"Hive_{cardNo}",
                AccPassword = "", // HiveApi 用户无密码
                RoleId = role.Id,
                RoleName = role.Name,
                IsAdmin = role.IsAdmin,
                RealName = $"HiveApi用户({cardNo})",
                TelNo = cardNo,
                Department = "HiveApi",
                SessionExpireMin = 10,
                Status = 1
            };
            accountRepository.Create(newAccount);

            // 重新读取以获得 Id
            allAccounts = accountRepository.Load(false);
            return allAccounts.First(a => a.TelNo == cardNo);
        }
    }
}
