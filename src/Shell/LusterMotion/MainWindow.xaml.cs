using HandyControl.Data;
using HandyControl.Tools;
using Luster.Common.Assets;
using Luster.Motion.CommonUI;
using Luster.Motion.CommonUI.Events;
using Luster.Motion.DataStruct;
using Luster.Motion.EditorUI;
using Luster.Motion.SubSystem;
using Luster.Motion.SubSystem.Views;
using Luster.Motion.TaskFlow.Engine;
using LusterMotion.ViewModel;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace LusterMotion
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        static MainWindow()
        {
            StyleProperty.OverrideMetadata(typeof(Window), new FrameworkPropertyMetadata(ResourceHelper.GetResource<Style>("MyWindow")));
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            WindowState = WindowState.Maximized;
            this.MinWidth = 1268;
            this.MinHeight = 600;

            var app = (Application.Current as App) as PrismApplication;
            var bus = app.Container.Resolve<ICommonBus>();
             var toolBarType = bus.GetToolbarContentType();

            if (toolBarType == null)
            {
                toolBarType = typeof(Luster.Motion.SubSystem.Views.ToolBarContent);
            }

            this.NonClientAreaContent = Activator.CreateInstance(toolBarType);
            this.Title = "LusterMotion";
            //if (Luster.Motion.CommonUI.ConfigHelper.GetAppKey("IsIPhone") == "False")
            //{
            //    this.NonClientAreaContent = new ToolBarContent();
            //}
            //else
            //{
            //    this.NonClientAreaContent = new Luster.Motion.SubSystem.LuXshare.Views.ToolBarContent();
            //}
#if DEBUG
            //AllowsTransparency = false;
            //WindowStyle = WindowStyle.ToolWindow;
#endif

           //CheckDongle();

            StartSplash((splashWindow) =>
            {
                splashWindow.Loading();
            });
        }

        /// <summary>
        /// 显示加密狗未识别的BUG
        /// </summary>
        /// <param name="msg"></param>
        private void ShowDongleError(string msg)
        {
            var app = (Application.Current as App) as PrismApplication;
            if (app != null)
            {
                var dialogService = app.Container.Resolve<IDialogService>();
                Dispatcher.Invoke(() =>
                {
                    try
                    {
                        dialogService.ShowErrorTip(msg);
                    }
                    catch (Exception ex)
                    { }
                    App.Current.Shutdown();
                });
            }
        }


        /// <summary>
        /// 加密狗检查
        /// </summary>
        private void CheckDongle()
        {
            try
            {
                Luster.Common.Dongle.DongleTool.Init();
                Luster.Common.Dongle.DongleTool.DongleError += str =>
                {
                    ShowDongleError(str);
                };

                Luster.Common.Dongle.DongleTool.Login();
            }
            catch (Exception ex)
            {
                var isExist = Luster.Common.Dongle.SentinelTool.Instance.IsExisted();
                if (!isExist)
                {
                    ShowDongleError($"加密狗异常:{ex.Message}");
                    Application.Current.Shutdown();
                }
            }
        }


        /// <summary>
        /// 关闭前
        /// </summary>
        /// <param name="e"></param>
        //protected override void OnClosing(CancelEventArgs e)
        //{
        //    base.OnClosing(e);

        //    var app = (Application.Current as App) as PrismApplication;
        //    if (app != null)
        //    {
        //        var deviceEngine = app.Container.Resolve<IDeviceEngine>();
        //        var commonBus = app.Container.Resolve<ICommonBus>();

        //        var dialog = app.Container.Resolve<IDialogService>();
        //        if (deviceEngine.IsNeedSave || commonBus.IsNeedSave)
        //        {
        //            dialog.ShowConfirm("任务要保存吗？", (result) =>
        //            {
        //                if (result.Result == ButtonResult.OK)
        //                {
        //                    StartSplash((sWin) =>
        //                    {
        //                        sWin.Unloading();
        //                    }, false);
        //                }
        //                else if (result.Result == ButtonResult.Cancel || result.Result == ButtonResult.None)
        //                {
        //                    e.Cancel = true;
        //                }
        //            });
        //        }
        //        if (commonBus.CurrentRecipe != null) 
        //        {
        //            var dbManager = app.Container.Resolve<IDbManager>();
        //            // 插入一条软件关闭记录
        //            dbManager.AddSysOperation(Luster.Motion.DataStruct.Enums.SystemOperation.SoftClose);
        //        }
        //    }
        //}

        /// <summary>
        /// 启动加载动画窗口
        /// </summary>
        /// <param name="action"></param>
        /// <param name="showMainWin"></param>
        private void StartSplash(Action<SplashWindow> action, bool showMainWin = true)
        {
            SplashWindow splashWindow = new SplashWindow();

            Thread t = new Thread(() =>
            {
                action(splashWindow);

                // 等500 ms
                Thread.Sleep(500);

                // 程序运行完成动画窗口关闭
                splashWindow.Dispatcher.Invoke(() =>
                {
                    splashWindow.Close();
                });

                if (showMainWin)
                {
                    // 启动当前Window
                    Dispatcher.Invoke(() =>
                    {
                        Show();
                        Activate();
                    });
                }
            });

            t.IsBackground = true;
            t.SetApartmentState(ApartmentState.STA);//设置单线程，在该模式下才能显示出来
            t.Start();
            splashWindow.ShowDialog();
        }

        public void SetDefaultTask(string defaultTask)
        {
            this.Tag = defaultTask;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            CloseFinSensor();
        }

        #region 消息接收
        private const int OKMsg = 0x100A;
        private const int NGMsg = 0x200A;
        private const int CloseMsg = 0x500A;
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(new HwndSourceHook(WndProc));
        }


        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            //System.Diagnostics.Debug.WriteLine($"msg:{msg},wParam:{wParam},lParam:{lParam}");
            switch (msg)
            {
                case OKMsg:
                    {

                        var vm = this.DataContext as MainWindowVM;
                        var bus = vm.GetBus();
                        bus.PublishEvent<SensorEvent, bool>(true);
                        break;

                    }
                case NGMsg:
                case CloseMsg:
                    {
                        var vm = this.DataContext as MainWindowVM;
                        var bus = vm.GetBus();
                        bus.PublishEvent<SensorEvent, bool>(false);

                        break;
                    }

                default:
                    break;
            }

            return hwnd;
        }

        [DllImport("User32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, IntPtr lParam);

        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        private const int HideMsg = 0x300A;
        private const int ShowMsg = 0x400A;

        private void CloseFinSensor()
        {
            IntPtr windowHandle = FindWindow(null, "指纹认证");
            if (windowHandle == IntPtr.Zero)
            {
                return;
            }

            SendMessage(windowHandle, CloseMsg, 0, IntPtr.Zero);
        }
        #endregion
    }
}
