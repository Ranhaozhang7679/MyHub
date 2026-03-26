namespace DC.Authorization.Models
{
    /// <summary>
    /// 权限项
    /// </summary>
    public class Right
    {
        public int Id { get; set; }
        /// <summary>权限唯一名称</summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>所属模块名（一级分组）</summary>
        public string ModuleName { get; set; } = string.Empty;
        /// <summary>所属界面名（二级分组）</summary>
        public string ViewName { get; set; } = string.Empty;
        /// <summary>描述</summary>
        public string Description { get; set; } = string.Empty;
        /// <summary>权限类型（操作或显示）</summary>
        public RightType Type { get; set; } = RightType.Operation;
    }
}
