using Prism.Commands;
using System;

namespace DC.Authorization.WPF
{
    /// <summary>
    /// 带权限校验的 Command。
    /// 执行时自动检查当前用户是否拥有指定权限。
    /// 无权限则弹出提示，不执行业务逻辑。
    /// </summary>
    public class AuthCommand : DelegateCommand
    {
        private readonly IAuthorizationFacade _auth;
        private readonly AuthItem _rightItem;

        /// <param name="auth">权限门面，通过构造函数注入</param>
        /// <param name="rightItem">权限项，必须与全局定义的 AuthItem 一致</param>
        /// <param name="executeMethod">业务逻辑方法</param>
        public AuthCommand(IAuthorizationFacade auth, AuthItem rightItem, Action executeMethod)
            : base(executeMethod)
        {
            _auth = auth;
            _rightItem = rightItem;
        }

        protected override void Execute(object parameter)
        {
            if (!_auth.CheckAuth(_rightItem)) return;
            base.Execute(parameter);
        }
    }

    /// <summary>
    /// 带参数版本的 AuthCommand
    /// </summary>
    public class AuthCommand<T> : DelegateCommand<T>
    {
        private readonly IAuthorizationFacade _auth;
        private readonly AuthItem _rightItem;

        public AuthCommand(IAuthorizationFacade auth, AuthItem rightItem, Action<T> executeMethod)
            : base(executeMethod)
        {
            _auth = auth;
            _rightItem = rightItem;
        }

        protected override void Execute(object parameter)
        {
            if (!_auth.CheckAuth(_rightItem)) return;
            base.Execute(parameter);
        }
    }
}
