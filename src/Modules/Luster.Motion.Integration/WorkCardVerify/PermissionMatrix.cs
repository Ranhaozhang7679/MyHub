using Luster.Common.DataStruct;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.Integration.WorkCardVerify
{
    /// <summary>
    /// 用户定义与平台定义角色Mapper
    /// 内部类，不需要加载日志模块，写死就行
    /// </summary>
    public class PermissionMatrix
    {
        /// <summary>
        /// 根据开发文档确认的DeviceLevel和UserRole映射关系
        /// </summary>
        private static readonly Dictionary<DeviceLevel, HashSet<SystemRole>> Matrix =
            new Dictionary<DeviceLevel, HashSet<SystemRole>>
            {
                { DeviceLevel.L1,   new HashSet<SystemRole> { SystemRole.Operator, SystemRole.Maintenance, SystemRole.Integrator } },
                { DeviceLevel.L2,   new HashSet<SystemRole> { SystemRole.Operator, SystemRole.Maintenance, SystemRole.Integrator } },
                { DeviceLevel.L3,   new HashSet<SystemRole> { SystemRole.Operator, SystemRole.Maintenance, SystemRole.Integrator, SystemRole.Admin } },
                { DeviceLevel.L6,   new HashSet<SystemRole> { SystemRole.Operator, SystemRole.Maintenance } },
                { DeviceLevel.L7,   new HashSet<SystemRole> { SystemRole.Operator } },
                { DeviceLevel.L8,   new HashSet<SystemRole> { SystemRole.Operator } },
                { DeviceLevel.L9,   new HashSet<SystemRole> { SystemRole.Operator, SystemRole.Maintenance } },
            };

        /// <summary>
        /// 指定密码对应的离线角色
        /// </summary>
        private static readonly Dictionary<string, SystemRole> OfflineAccounts =
            new Dictionary<string, SystemRole>
            {
                { "Luster@1996",        SystemRole.Maintenance },
                { "Luster@1996_Inter",  SystemRole.Integrator },
                { "Luster@1996_Admin",  SystemRole.Admin },
                { "Luster@1996_OP",     SystemRole.Operator },
            };
        /// <summary>
        /// 内置管理员账号
        /// </summary>
        public readonly Dictionary<string, SystemRole> InsertedAdmin;
        public PermissionMatrix()
        {
            var keyvalue = ConfigurationManager.AppSettings["FXAdminCardID"] ?? string.Empty;
            InsertedAdmin = keyvalue
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Distinct()
                .ToDictionary(id => id, id => SystemRole.Admin);
        }

        /// <summary>
        /// 在线权限验证，使用预设的DeviceLevel和角色映射
        /// </summary>
        /// <param name="level">用户接口返回的等级</param>
        /// <param name="role">角色等级</param>
        /// <returns></returns>
        public void CheckOnlinePermission(DeviceLevel level, out SystemRole role)
        {
            role = SystemRole.Operator;
            if (Matrix.TryGetValue(level, out var roles))
            {
                role = roles.Min();
            }
        }
        /// <summary>
        /// 离线权限验证，使用预设的密码和角色映射
        /// </summary>
        /// <param name="password">离线登录密码</param>
        /// <param name="role">角色等级</param>
        /// <returns></returns>
        public void CheckOfflinePermission(string password, out SystemRole role)
        {
            role = SystemRole.Operator;
            if (OfflineAccounts.TryGetValue(password, out var roles))
            {
                role = roles;
            }
        }
    }
}
