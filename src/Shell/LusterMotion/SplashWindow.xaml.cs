using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LusterMotion
{
    /// <summary>
    /// SplashWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SplashWindow : Window
    {
        /// <summary>
        /// 模块加载
        /// </summary>
        private LoadModules loadModules;


        public SplashWindow()
        {
            InitializeComponent();

            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            WindowStyle = WindowStyle.None;
            loadModules = new LoadModules();
            loadModules.LoadingEvent += LoadModules_LoadingEvent;
        }

        private void LoadModules_LoadingEvent(int arg1, string arg2)
        {
            Dispatcher.Invoke(() =>
            {
                ShowTips = arg2;
                ProgressVal = arg1;
            });
        }

        public void Loading()
        {
            loadModules.LoadModule();
        }

        public void Unloading()
        {
            loadModules.Unload();
        }

        public string ShowTips
        {
            get { return (string)GetValue(ShowTipsProperty); }
            set { SetValue(ShowTipsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowTips.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowTipsProperty =
            DependencyProperty.Register("ShowTips", typeof(string), typeof(SplashWindow), new PropertyMetadata(""));



        public int ProgressVal
        {
            get { return (int)GetValue(ProgressValProperty); }
            set { SetValue(ProgressValProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ProgressVal.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProgressValProperty =
            DependencyProperty.Register("ProgressVal", typeof(int), typeof(SplashWindow), new PropertyMetadata(0));

    }
}
