namespace DC.Authorization.Models
{
    /// <summary>
    /// 用户账号
    /// </summary>
    public class Account
    {
        public int Id { get; set; }
        /// <summary>账户名</summary>
        public string AccName { get; set; } = string.Empty;
        /// <summary>密码（哈希后）</summary>
        public string AccPassword { get; set; } = string.Empty;
        /// <summary>角色ID</summary>
        public int RoleId { get; set; }
        /// <summary>角色名称（关联查询填充）</summary>
        public string RoleName { get; set; } = string.Empty;
        /// <summary>是否管理员</summary>
        public bool IsAdmin { get; set; }
        /// <summary>真实姓名</summary>
        public string RealName { get; set; } = string.Empty;
        /// <summary>卡号/工号</summary>
        public string TelNo { get; set; } = string.Empty;
        /// <summary>所在部门</summary>
        public string Department { get; set; } = string.Empty;
        /// <summary>Session 过期时间（分钟）</summary>
        public int SessionExpireMin { get; set; } = 3;
        /// <summary>状态：1=正常, 0=已停止, -1=已删除</summary>
        public int Status { get; set; }

        public string HiveLevel { get; set; } = string.Empty;
    }
}
