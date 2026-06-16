using DC.Authorization;
using DC.Authorization.Models;
using Prism.Ioc;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Data;

namespace DC.Authorization.WPF.Helper
{
    /// <summary>
    /// 根据权限项名称（AuthDictionary 字段名）判断可见性。
    /// 用于 DataTemplate 中动态绑定权限，与 AuthBehavior 互补。
    /// </summary>
    public class AuthVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // value 为 AuthItemName 字符串，parameter 可作为 fallback
            if (value == null) return Visibility.Visible;

            string itemName = value.ToString();
            if (string.IsNullOrEmpty(itemName)) return Visibility.Visible;

            try
            {
                // 通过反射从 AuthDictionary 获取 AuthItem
                var field = typeof(AuthDictionary).GetField(itemName, BindingFlags.Public | BindingFlags.Static);
                if (field == null || field.FieldType != typeof(AuthItem)) return Visibility.Visible;

                var authItem = (AuthItem)field.GetValue(null);

                var authFacade = ContainerLocator.Current.Resolve<IAuthorizationFacade>();
                if (authFacade == null) return Visibility.Visible;

                return authFacade.HasAuth(authItem, RightType.Visibility) ? Visibility.Visible : Visibility.Collapsed;
            }
            catch
            {
                // 容器未初始化或设计时模式，默认显示
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
