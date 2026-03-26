namespace DC.Authorization.Models
{
    public enum RightType
    {
        /// <summary>操作权限（用于禁用控件或弹窗提示）</summary>
        Operation = 0,
        
        /// <summary>可见性权限（专门用于控制界面的显示与隐藏）</summary>
        Visibility = 1
    }
}
