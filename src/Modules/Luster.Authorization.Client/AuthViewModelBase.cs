using DC.Authorization.Models;
using Prism.Mvvm;
using System.Linq;
using System.Reflection;

namespace DC.Authorization.WPF
{
    /// <summary>
    /// ViewModel 基类。
    /// 继承此类后，子类中标注了 [AuthRight] 的方法会在构造时自动注册到权限表。
    /// </summary>
    public abstract class AuthViewModelBase : BindableBase
    {
        protected readonly IAuthorizationFacade? Auth;

        protected AuthViewModelBase(IAuthorizationFacade auth)
        {
            Auth = auth;
            AutoRegisterRights();
        }

        /// <summary>
        /// 扫描子类所有方法上的 [AuthRight] 特性，自动注册权限项
        /// </summary>
        private void AutoRegisterRights()
        {
            var methods = GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var properties = GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            var operationRights = methods
                .SelectMany(m => m.GetCustomAttributes<AuthRightAttribute>())
                .Select(attr => new Right
                {
                    Name = attr.AuthItem.Operation,
                    ModuleName = attr.AuthItem.Module,
                    ViewName = attr.AuthItem.View,
                    Description = attr.AuthItem.Description,
                    Type = RightType.Operation,
                    SortOrder = attr.AuthItem.Order
                });

            var visibilityRightsFromMethods = methods.SelectMany(m => m.GetCustomAttributes<AuthVisibilityAttribute>());
            var visibilityRightsFromProps = properties.SelectMany(m => m.GetCustomAttributes<AuthVisibilityAttribute>());
            var visibilityRightsFromFields = fields.SelectMany(m => m.GetCustomAttributes<AuthVisibilityAttribute>());

            var visibilityRights = visibilityRightsFromMethods
                .Concat(visibilityRightsFromProps)
                .Concat(visibilityRightsFromFields)
                .Select(attr => new Right
                {
                    Name = attr.AuthItem.Operation,
                    ModuleName = attr.AuthItem.Module,
                    ViewName = attr.AuthItem.View,
                    Description = attr.AuthItem.Description,
                    Type = RightType.Visibility,
                    SortOrder = attr.AuthItem.Order
                });

            var rights = operationRights.Concat(visibilityRights).ToArray();

            if (rights.Any())
            {
                Auth?.RegisterRights(rights);
            }
        }
    }
}
