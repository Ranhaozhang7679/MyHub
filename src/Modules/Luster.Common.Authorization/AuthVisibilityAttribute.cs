using System;
using System.Reflection;

namespace DC.Authorization
{
    /// <summary>
    /// 标注在 ViewModel 的字段或空方法上，声明该操作仅用于“界面可见性”管控。
    /// 可以与 AuthBehavior 的 Hide 或 Disable 一同结合生效。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class AuthVisibilityAttribute : Attribute
    {
        public AuthItem AuthItem { get; private set; }

        /// <summary>
        /// 接收 AuthItem 对象在枚举/静态类中的名称（如 nameof(AuthDictionary.VizAdvancedConfigTab)）。
        /// 通过反射动态获取对应的结构体实例对象。
        /// </summary>
        public AuthVisibilityAttribute(string itemName)
        {
            var field = typeof(AuthDictionary).GetField(itemName, BindingFlags.Public | BindingFlags.Static);
            if (field != null && field.FieldType == typeof(AuthItem))
            {
                AuthItem = (AuthItem)field.GetValue(null);
            }
            else
            {
                AuthItem = new AuthItem("未分类", "未分类", "无效可见性定义: " + itemName, "在 AuthDictionary 中找不到该定义");
            }
        }
    }
}
