using Luster.Motion.CommonUI.ViewModel.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Luster.Motion.CommonUI.Views.Dialogs
{
    /// <summary>
    /// WinFormDialog.xaml 的交互逻辑
    /// </summary>
    public partial class WinFormDialog : UserControl
    {
        public WinFormDialog()
        {
            InitializeComponent();

            //this.finControl.Load += FinControl_Load;
            //this.finControl.Load -= FinControl_Load;

            //this.finControl.LoginEvent -= FinControl_LoginEvent;
            //this.finControl.LoginEvent += FinControl_LoginEvent;

            //WindowsFormsHost winHost = new WindowsFormsHost();
            //this.Content = winHost;

            //FinSensor.FingerControl finSensor = new FinSensor.FingerControl();
            //finSensor.Dock = System.Windows.Forms.DockStyle.Fill;
            //winHost.Child = finSensor;

            this.Loaded += WinFormDialog_Loaded;
        }

        private void WinFormDialog_Loaded(object sender, RoutedEventArgs e)
        {
            var child = this.winHost.Child;
            AddHandler(child, "LoginEvent");
        }

        public void AddHandler<T>(T instance, string eventName)
        {
            var eventInfo = instance.GetType().GetEvent(eventName);
            var methodInfo = GetType().GetMethod(nameof(FinControl_LoginEvent));
            var @delegate = Delegate.CreateDelegate(eventInfo.EventHandlerType, this, methodInfo);
            eventInfo.AddEventHandler(instance, @delegate);

            var initMethod = instance.GetType().GetMethod("Initlize");
            initMethod.Invoke(instance, null);
        }

        private void FinControl_Load(object sender, EventArgs e)
        {
            // 初始化指纹锁
            //this.finControl.Initlize();
        }

        public void FinControl_LoginEvent(object arg1, bool isOK)
        {
            if (this.DataContext is WinFormDialogVM vM)
            {
                vM.FormEvent(this, isOK);
            }
        }
    }
}
