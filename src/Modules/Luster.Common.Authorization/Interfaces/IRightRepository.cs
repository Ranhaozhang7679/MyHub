using DC.Authorization.Models;
using System.Collections.Generic;

namespace DC.Authorization
{
    /// <summary>
    /// 权限项仓储接口
    /// </summary>
    public interface IRightRepository
    {
        /// <summary>加载所有权限项</summary>
        List<Right> Load();

        /// <summary>批量新增或更新权限项（用于自动注册）</summary>
        void Upsert(Right[] rights);

        /// <summary>检查指定账号是否拥有某权限</summary>
        bool HasRight(int accountId, string moduleName, string viewName, string rightName);

        /// <summary>删除角色的所有权限</summary>
        void DeleteRoleRights(int roleId);
    }
}
