using DC.Authorization.Models;
using System.Collections.Generic;

namespace DC.Authorization
{
    /// <summary>
    /// 角色仓储接口
    /// </summary>
    public interface IRoleRepository
    {
        /// <summary>加载所有角色</summary>
        List<Role> Load();

        /// <summary>创建角色，返回新角色ID</summary>
        int Create(Role role);

        /// <summary>更新角色信息</summary>
        void Update(Role role);

        /// <summary>删除角色</summary>
        void Delete(int id);

        /// <summary>给角色分配权限</summary>
        void Assign(int roleId, int[] rights);

        /// <summary>加载角色已分配的权限ID列表</summary>
        List<int> LoadRights(int roleId);
    }
}
