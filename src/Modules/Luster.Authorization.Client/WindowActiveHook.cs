using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DC.Authorization
{


    public class WindowActiveHook : IActiveHook
    {
        private volatile bool _active;
        public bool IsActive => _active;

        public void Reset()
        {
            _active = false;
        }

        public async void Start()
        {
            await Task.Delay(5000);
            //var mainWin = Application.Current.MainWindow;
            //mainWin.KeyUp += Active;
            //mainWin.MouseMove += Active;
            //mainWin.MouseUp += Active;
            EventManager.RegisterClassHandler(typeof(Window), Window.PreviewMouseUpEvent, new MouseButtonEventHandler(Active));
            EventManager.RegisterClassHandler(typeof(Window), Window.MouseMoveEvent, new MouseEventHandler(Active));
            EventManager.RegisterClassHandler(typeof(Window), Window.KeyUpEvent, new KeyEventHandler(Active));
        }

        public void Active(object sender, EventArgs e) => _active = true;
    }
}
