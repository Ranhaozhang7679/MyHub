using DC.Authorization;
using DC.Authorization.Models;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Luster.Authorization.Client.Helper
{
    public enum AuthBehaviorAction
    {
        Hide,
        Disable
    }

    /// <summary>
    /// 界面权限展示附加属性，可直接在 XAML 中用于控制任何 UIElement。
    /// 当权限不足时，根据 Action 的配置，自动隐藏 (Hide) 或者是禁用 (Disable)。
    /// 用户登录状态或刷卡切换用户时，界面会自动即时响应变更。
    /// </summary>
    public static class AuthBehavior
    {
        // 弱引用集合，用于跟踪所有绑定了权限属性的控件
        private static readonly List<WeakReference<UIElement>> _trackedElements = new List<WeakReference<UIElement>>();
        private static bool _isEventSubscribed = false;

        public static readonly DependencyProperty RightItemProperty =
            DependencyProperty.RegisterAttached("RightItem", typeof(AuthItem), typeof(AuthBehavior), new PropertyMetadata(default(AuthItem), OnRightItemChanged));

        public static readonly DependencyProperty ActionProperty =
            DependencyProperty.RegisterAttached("Action", typeof(AuthBehaviorAction), typeof(AuthBehavior), new PropertyMetadata(AuthBehaviorAction.Hide, OnActionChanged));

        public static AuthItem GetRightItem(DependencyObject obj) => (AuthItem)obj.GetValue(RightItemProperty);
        public static void SetRightItem(DependencyObject obj, AuthItem value) => obj.SetValue(RightItemProperty, value);

        public static AuthBehaviorAction GetAction(DependencyObject obj) => (AuthBehaviorAction)obj.GetValue(ActionProperty);
        public static void SetAction(DependencyObject obj, AuthBehaviorAction value) => obj.SetValue(ActionProperty, value);

        private static void EnsureSubscribed()
        {
            if (_isEventSubscribed) return;

            try
            {
                var loginService = ContainerLocator.Current.Resolve<ILoginService>();
                if (loginService != null)
                {
                    loginService.OnCardLogin += GlobalAuthRefresh;
                    loginService.OnPasswordLogin += GlobalAuthRefresh;
                    loginService.OnLogout += GlobalAuthRefresh;
                    _isEventSubscribed = true;
                }
            }
            catch
            {
                // 忽略注入失败或设计时错误（Prism 容器可能在最初尚未准备完毕）
            }
        }

        private static void GlobalAuthRefresh(object? sender, EventArgs e)
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                _trackedElements.RemoveAll(weakRef =>
                {
                    if (weakRef.TryGetTarget(out var element))
                    {
                        UpdateElementState(element);
                        return false; // 保留
                    }
                    return true; // 元素已被回收，移除弱引用
                });
            });
        }

        private static void OnRightItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                // 防止重复添加
                _trackedElements.RemoveAll(w => !w.TryGetTarget(out var target) || target == element);
                _trackedElements.Add(new WeakReference<UIElement>(element));

                EnsureSubscribed();
                
                // 确保异步更新，避免在窗口刚开始反序列化并且 Prism Container 还未 Ready 时报错
                Application.Current?.Dispatcher.BeginInvoke(new Action(()=> 
                {
                    // 如果由于某些原因尚未订阅成功（比如之前容器没准备好），再尝试一次
                    EnsureSubscribed();
                    UpdateElementState(element);
                }), System.Windows.Threading.DispatcherPriority.Loaded);
            }
        }

        private static void OnActionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                UpdateElementState(element);
            }
        }

        private static void UpdateElementState(UIElement element)
        {
            var rightItem = GetRightItem(element);
            if (string.IsNullOrEmpty(rightItem.Operation)) return;

            try
            {
                var authFacade = ContainerLocator.Current.Resolve<IAuthorizationFacade>();
                if (authFacade != null)
                {
                    bool hasRight = authFacade.HasAuth(rightItem, RightType.Visibility);
                    var action = GetAction(element);

                    if (!hasRight)
                    {
                        if (action == AuthBehaviorAction.Hide)
                        {
                            element.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            element.IsEnabled = false;
                        }
                    }
                    else
                    {
                        element.Visibility = Visibility.Visible;
                        element.IsEnabled = true;
                    }
                }
            }
            catch (Exception)
            {
                // 设计器模式下或者容器尚未初始化时忽略
            }
        }
    }
}
