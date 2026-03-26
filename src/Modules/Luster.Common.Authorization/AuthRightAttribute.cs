using System;
using System.Reflection;

namespace DC.Authorization
{
    /// <summary>
    /// 标注在 ViewModel 的命令方法上，声明该操作所需的权限。
    /// 应用启动时会自动扫描并注册到权限表中。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class AuthRightAttribute : Attribute
    {
        public AuthItem AuthItem { get; private set; }

        /// <summary>
        /// 接收 AuthItem 对象在枚举/静态类中的名称（如 nameof(AuthDictionary.ModifyRight)）。
        /// 通过反射动态获取对应的结构体实例对象。
        /// </summary>
        public AuthRightAttribute(string itemName)
        {
            var field = typeof(AuthDictionary).GetField(itemName, BindingFlags.Public | BindingFlags.Static);
            if (field != null && field.FieldType == typeof(AuthItem))
            {
                AuthItem = (AuthItem)field.GetValue(null);
            }
            else
            {
                AuthItem = new AuthItem("未分类", "未分类", "无效权限定义: " + itemName, "在 AuthDictionary 中找不到该定义");
            }
        }
    }
}
