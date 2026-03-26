using DC.Authorization;
using DC.Authorization.WPF.Helper;
using DC.Authorization.WPF.Infrastructure;
using DC.Authorization.WPF.Repositories;
using DC.Authorization.WPF.Services;
using DC.Authorization.WPF.ViewModels;
using DC.Authorization.WPF.Views;
using Luster.Authorization.Client.Helper;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Serilog;
using System.ComponentModel;

namespace DC.Authorization.WPF
{
    public class WpfAuthorizationModule : IModule
    {
        public WpfAuthorizationModule() { }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            // 确保数据库目录存在并初始化
            var settingsDir = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "..", "settings");
            if (!System.IO.Directory.Exists(settingsDir))
                System.IO.Directory.CreateDirectory(settingsDir);
            Infrastructure.DbInitializer.Initialize();
            var backgroundService = containerProvider.Resolve<BackgroundService>();
            backgroundService.Start();
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // 确保这一行在 OnInitialized 之前执行
            containerRegistry.RegisterSingleton<Serilog.ILogger>(() =>
                new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.File(
                        path: "logs/account-.txt",           // 文件路径
                        rollingInterval: RollingInterval.Day,  // 按天滚动
                        retainedFileCountLimit: 7,       // 保留7天
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}")
                    .CreateLogger());

            // ---- Repositories（接口 → 实现） ----
            containerRegistry.RegisterSingleton<IAccountRepository, AccountRepository>();
            containerRegistry.RegisterSingleton<IRoleRepository, RoleRepository>();
            containerRegistry.RegisterSingleton<IRightRepository, RightRepository>();
            containerRegistry.RegisterSingleton<IAuditLogRepository, AuditLogRepository>();
            containerRegistry.RegisterSingleton<IAuthSettingRepository, AuthSettingRepository>();

            // ---- Auth Providers（双认证） ----
            containerRegistry.RegisterSingleton<Providers.LocalAuthProvider>();
            containerRegistry.RegisterSingleton<Providers.HiveAuthProvider>();
            containerRegistry.RegisterSingleton<Providers.CompositeAuthProvider>();
            containerRegistry.RegisterSingleton<IAuthProvider, Providers.CompositeAuthProvider>();

            // ---- Infrastructure ----
            containerRegistry.RegisterSingleton<RoleLevelMapper>();

            // ---- Services ----
            containerRegistry.RegisterSingleton<ILoginService, LoginService>();
            containerRegistry.RegisterSingleton<ISessionManager, SessionManager>();
            containerRegistry.RegisterSingleton<IActiveHook, WindowActiveHook>();
            containerRegistry.RegisterSingleton<BackgroundService>();
            containerRegistry.RegisterSingleton<IAuthorizationFacade, WpfAuthorizationFacade>();
            containerRegistry.Register<AccountImportService>();

            // ---- Helper ----
            containerRegistry.RegisterSingleton<ITradDialogService, TradDialogService>();

            // ---- Dialogs ----
            containerRegistry.RegisterDialog<LoginView, LoginViewModel>();
            containerRegistry.RegisterDialog<CreateRoleView, CreateRoleViewModel>();
            containerRegistry.RegisterDialog<CreateAccountView, CreateAccountViewModel>();
            containerRegistry.RegisterDialog<ChangePasswordView, ChangePasswordViewModel>();
            containerRegistry.RegisterDialog<AuthorityView, AuthorityViewModel>();
            ViewModelLocationProvider.Register<LoginView, LoginViewModel>();
        }
    }
}
