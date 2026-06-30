using HandyControl.Tools;
using Luster.Common.Assets;
using Luster.Common.Assets.Views;
using Luster.Common.DataAccess.Factory;
using Luster.Common.DataAccess.Repositories;
using Luster.Common.Tools;
using Luster.Motion.CommonUI;
using Luster.Motion.DataStruct;
using Luster.Motion.DataStruct.Interfaces;
using Luster.Motion.DigitalSetup.Services;
using Luster.Motion.FiveAxis.Calibration;
using Luster.Motion.Integration.Web;
using Luster.Motion.Integration.WorkCardVerify;
using Luster.Motion.SubSystem.ViewModel;
using Luster.Motion.SubSystem.Views;
using Luster.Motion.TaskFlow.Engine;
using Luster.Motion.TaskFlow.Engine.Engine;
using Luster.Motion.TaskFlow.Engine.HyperTrain;
using Luster.Motion.TaskFlow.Engine.Models;
using Luster.SimDevice.Engine;
using Luster.TaskFlow.Common;
using Luster.TaskFlow.Common.Interfaces;
using Luster.TaskFlow.Motion.Interfaces;
using LusterMotion.Config;
using LusterMotion.ViewModel;
using NLog;
using NLog.Config;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace LusterMotion
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        /// <summary>
        /// 弹窗
        /// </summary>
        private IDialogService _dialogService;

        /// <summary>
        /// 防止多开
        /// </summary>
        private static System.Threading.Mutex mutex;

        /// <summary>
        /// 默认任务路径
        /// </summary>
        private string _defaultTask = "";

        /// <summary>
        /// 构造函数
        /// </summary>
        public App()
        {

            // UI线程捕获异常
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // Task捕获未处理异常
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }


        /// <summary>
        /// 注册所有类型
        /// </summary>
        /// <param name="containerRegistry"></param>
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<Dispatcher>(() => Application.Current.Dispatcher);
            containerRegistry.RegisterSingleton<IIocManager, IocManager>();
            containerRegistry.RegisterSingleton<IDBFactory, DBFactory>();
            containerRegistry.RegisterSingleton<IConfigManager, ConfigManager>();
            containerRegistry.RegisterSingleton<ICacheManager, CacheManager>();
            containerRegistry.RegisterSingleton<IOrderManager, OrderManager>();
            containerRegistry.RegisterSingleton<IRollManager, RollManager>();
            containerRegistry.RegisterSingleton<IErrorManager, ErrorManager>();

            containerRegistry.RegisterSingleton<IModuleFactory, ModuleFactory>();

            containerRegistry.RegisterSingleton<IMotionEngine, MotionEngine>();
            containerRegistry.RegisterSingleton<IDeviceEngine, DeviceEngine>();
            containerRegistry.RegisterSingleton<ILightManager, LightManager>();
            containerRegistry.RegisterSingleton<IWebService, WebService>();

            containerRegistry.Register<IRepository, Repository>();
            containerRegistry.Register<IDbHelper, DbHelper>();
            containerRegistry.RegisterSingleton<IMotionController, MotionController>();

            containerRegistry.RegisterSingleton(typeof(LoadModules));
            //containerRegistry.RegisterSingleton(typeof(ProductStat));


            //containerRegistry.RegisterSingleton<Luster.TaskFlow.Engine.ITaskFlowEngine, Luster.TaskFlow.Engine.TaskFlowEngine>();

            // 通用配置
            containerRegistry.RegisterSingleton<IDbManager, DbManager>();
            containerRegistry.RegisterSingleton<ICommonBus, CommonBus>();

            // 点检状态服务
            containerRegistry.RegisterSingleton<CheckStatusPersistenceService>();
            containerRegistry.RegisterSingleton<CheckStatusService>();

            // 弹窗对话框
            containerRegistry.RegisterDialogWindow<Luster.Common.Assets.Views.DialogWindow>();

            // 驾驶舱和Vision系统上报
            containerRegistry.RegisterSingleton<HyperTrainAPI>();
            containerRegistry.RegisterSingleton<VisionAPI>();
            containerRegistry.RegisterSingleton<HiveAPI>();
            // 作业模式
            containerRegistry.RegisterSingleton<IAuthService, AuthService>();

            // TES-190 P2-B: 五轴标定 Service 化（接口+实现在 Luster.Module.Motion.FiveAxis，Shell 容器统一注册，FiveAxisCaliParam.DoExcute 经 Ioc 服务定位）
            containerRegistry.RegisterSingleton<IFiveAxisCalibrationService, FiveAxisCalibrationService>();
        }

        /// <summary>
        /// 系统
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStartup(StartupEventArgs e)
        {
            FrameworkCompatibilityPreferences.KeepTextBoxDisplaySynchronizedWithTextProperty = false;

            // 查看加载后的App.config中的NLog配置
            //LogManager.Configuration = new XmlLoggingConfiguration(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

            SetAdminLogin(e);

            mutex = new System.Threading.Mutex(true, "LusterMotion");

            if (mutex.WaitOne(0, false))
            {
                base.OnStartup(e);
            }
            else
            {
                // 读取配置：是否允许程序多开
                var allowMultiple = System.Configuration.ConfigurationManager.AppSettings["AllowMultipleInstances"];
                if (string.Equals(allowMultiple, "True", StringComparison.OrdinalIgnoreCase))
                {
                    // 允许多开，继续启动
                    MessageBox.Show("程序已经多开", "警告", MessageBoxButton.OK);
                    base.OnStartup(e);
                }
                else
                {
                    MessageBox.Show("程序已经在运行，禁止多开！\n如需多开请在App.config中设置AllowMultipleInstances为True", "警告", MessageBoxButton.OK);
                    this.Shutdown();
                }
            }

            if (e.Args.Length > 0 && File.Exists(e.Args[0]))
            {
                // 通过任务路径启动
                _defaultTask = e.Args[0].Trim();
            }


            HandyControl.Tools.ConfigHelper.Instance.SetLang(AppConfig.Lang);
            //Luster.Common.Assets.Langs.LangProvider.Culture = new CultureInfo(AppConfig.Lang);
            //Luster.Motion.Assests.Langs.LangProvider.Culture = new CultureInfo(AppConfig.Lang);
        }

        /// <summary>
        /// 基于条件编译自动使用Admin登录
        /// </summary>
        /// <param name="e"></param>
        private void SetAdminLogin(StartupEventArgs e)
        {
#if DEBUG
            if (e.Args.Length > 0)
            {
                Luster.Common.Tools.CommonTool.SetOffline(e.Args[0], DateTime.Now.ToString("yyyy-MM-dd"));
            }

            if (e.Args.Length >= 2)
            {
                GlobalProperty.AutoLogin = e.Args[1];
            }
#endif
        }

        //Shell(壳)是加载模块的宿主应用程序
        protected override Window CreateShell()
        {
            // 记录详细错误信息
            //LogTool.SetLogPath(AppDomain.CurrentDomain.BaseDirectory);
            //LogTool.Info("驾驶舱软件启动中...", AppConfig.System);
            var window = Container.Resolve<MainWindow>();
            window.SetDefaultTask(_defaultTask);
            return window;
        }

        protected override void ConfigureViewModelLocator()
        {
            base.ConfigureViewModelLocator();

        }
        /// <summary>
        /// 配置依赖包- module catalog(模块目录)
        /// </summary>
        /// <param name="moduleCatalog"></param>
        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            base.ConfigureModuleCatalog(moduleCatalog);


            moduleCatalog.AddModule<DC.Authorization.WPF.WpfAuthorizationModule>();

            // 注册UI模块
            moduleCatalog.AddModule<Luster.Motion.AlarmUI.AlarmModule>();// 报警
            moduleCatalog.AddModule<Luster.Motion.ReportUI.ReportModule>();// 报表

            // 3D页面
            //moduleCatalog.AddModule<Luster.SubSystem.ThreeD.Luster3DModule>();

            // 添加依赖模块
            moduleCatalog.AddModule<Luster.Motion.EditorUI.EditorModule>();
            moduleCatalog.AddModule<Luster.Motion.CommonUI.CommonModule>();
            moduleCatalog.AddModule<Luster.SimDevice.SubSystem.SimDeviceModule>(); // 设备

            moduleCatalog.AddModule<Luster.Motion.DigitalSetup.DigitalSetupModule>(); //数字架线

            // P6-A：五轴 AOI UI 模块（可还原，移除本行 + LusterMotion.csproj 的 ProjectReference 即可，平台标准 UI 不受影响）
            moduleCatalog.AddModule<Luster.Motion.FiveAxis.UI.FiveAxisUIModule>(); //五轴AOI UI

            // 注册动态页面
            var commBus = Container.Resolve<ICommonBus>();
            commBus.RegisterSystemDll();
            var moduleType = commBus.GetUiModuleType();
            moduleCatalog.AddModule(new ModuleInfo(moduleType, moduleType.Name, InitializationMode.WhenAvailable));

            // 动态加载UI模块
            // 注册标题栏
            //var typeName = commBus.GetToolbarContentType().ToString();
            //ViewModelLocationProvider.Register(typeName, typeof(ToolBarContentVM));
            //ViewModelLocationProvider.Register<Luster.Motion.SubSystem.FX.Views.ToolBarContent, ToolBarContentVM>();

            //Luster.Motion.CommonUI.ConfigHelper.IsIPhone(() =>
            //{
            //    ViewModelLocationProvider.Register<ToolBarContent, ToolBarContentVM>();
            //}, () =>
            //{
            //    ViewModelLocationProvider.Register<Luster.Motion.SubSystem.LuXshare.Views.ToolBarContent, ToolBarContentVM>();
            //});
            // 提前注册
            ViewModelLocationProvider.Register<MainWindow, MainWindowVM>();
        }

        /// <summary>
        /// 未处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // 记录详细错误信息
            var ex = e.ExceptionObject as Exception;
            if (ex != null)
            {
                GetErrorMsg(ex);
            }
        }

        /// <summary>
        /// 未处理的任务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            string errMsg = GetErrorMsg(e.Exception);

            if (_dialogService == null)
            {
                _dialogService = Container.Resolve<IDialogService>();
            }

            Dispatcher.Invoke(() =>
            {
                try
                {
                    _dialogService.ShowErrorTip(errMsg);
                }
                catch (Exception ex)
                { }
            });
        }

        /// <summary>
        /// 构建错误提示
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        private string GetErrorMsg(Exception exception)
        {
            StringBuilder sb = new StringBuilder();

            // 获取最终错误
            GetErrorMessage(sb, ref exception);

            // 前端错误
            string message = sb.ToString();

            // 记录详细错误信息
            LogTool.Error(message, exception);

            return message;
        }

        /// <summary>
        /// 未处理异常抛出
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">e</param>
        private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // 针对剪切板被占用的问题处理
            var comException = e.Exception as System.Runtime.InteropServices.COMException;
            if (comException != null && comException.ErrorCode == -2147221040)
                e.Handled = true;

            string errMsg = GetErrorMsg(e.Exception);

            if (_dialogService == null)
            {
                _dialogService = Container.Resolve<IDialogService>();
            }

            Dispatcher.Invoke(() =>
            {
                try
                {
                    _dialogService.ShowErrorTip(errMsg);
                }
                catch { }
            });

            // 结束异常往上外抛
            e.Handled = true;
        }

        /// <summary>
        /// 获取错误消息
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        private StringBuilder GetErrorMessage(StringBuilder sb, ref Exception exception)
        {
            if (exception != null)
            {
                if (exception.InnerException != null)
                {
                    exception = exception.InnerException;
                    GetErrorMessage(sb, ref exception).ToString();
                }
                else if (exception is ReflectionTypeLoadException rEx)
                {
                    foreach (var loadEx in rEx.LoaderExceptions)
                    {
                        sb.AppendLine($"{loadEx.Message}");
                    }
                }
                else
                {
                    sb.AppendLine($"{exception.Message}");
                }
            }

            return sb;
        }
    }
}
