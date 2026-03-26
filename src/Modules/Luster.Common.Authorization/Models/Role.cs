namespace DC.Authorization.Models
{
    /// <summary>
    /// 角色
    /// </summary>
    public class Role
    {
        public int Id { get; set; }
        /// <summary>角色名称</summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>角色等级（数值越小权限越高）</summary>
        public int Level { get; set; }
        /// <summary>是否管理员角色</summary>
        public bool IsAdmin { get; set; }
    }
}
