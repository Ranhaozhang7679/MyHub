using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace DC.Authorization.WPF.Infrastructure
{
    /*
    注意：
        如果运行中出现SetWindowsHookEx的返回值为0，这是因为.net 调试模式的问题，具体的做法是禁用宿主进程，在 Visual Studio 中打开项目。
        在"项目"菜单上单击"属性"。
        单击"调试"选项卡。
        清除"启用 Visual Studio 宿主进程(启用windows承载进程)"复选框 或 勾选启用非托管代码调试
    */

    [StructLayout(LayoutKind.Sequential)]
    internal class POINT
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal class MouseHookStruct
    {
        public POINT pt;
        public int hwnd;
        public int wHitTestCode;
        public int dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal class KeyboardHookStruct
    {
        public int vkCode;
        public int scanCode;
        public int flags;
        public int time;
        public int dwExtraInfo;
    }

    [Flags]
    internal enum HookType
    {
        Keyboard = 0, Mouse = 1
    }

    internal class KeyEventArgs : EventArgs
    {
        public KeyEventArgs(byte key) { Key = key; }
        public byte Key { get; private set; }
    }

    /// <summary>
    /// 全局键盘/鼠标钩子（用于刷卡输入检测和活动状态跟踪）
    /// </summary>
    internal class GlobalHook
    {
        public delegate int HookProc(int nCode, Int32 wParam, IntPtr lParam);
        public delegate int GlobalHookProc(int nCode, Int32 wParam, IntPtr lParam);

        public GlobalHook() { }

        public event EventHandler<KeyEventArgs>? KeyDown;
        public event EventHandler? MouseMove;
        public event EventHandler<KeyEventArgs>? KeyUp;

        private static int _hMouseHook = 0;
        private static int _hKeyboardHook = 0;

        public int HMouseHook => _hMouseHook;
        public int HKeyboardHook => _hKeyboardHook;

        public const int WH_MOUSE_LL = 14;
        public const int WH_KEYBOARD_LL = 13;

        private GlobalHookProc? MouseHookProcedure;
        private GlobalHookProc? KeyboardHookProcedure;

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        internal static extern int SetWindowsHookEx(int idHook, GlobalHookProc lpfn, IntPtr hInstance, int threadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        internal static extern bool UnhookWindowsHookEx(int idHook);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        internal static extern int CallNextHookEx(int idHook, int nCode, Int32 wParam, IntPtr lParam);

        internal bool Start(HookType hookType = HookType.Keyboard)
        {
            if (hookType.HasFlag(HookType.Keyboard) && _hKeyboardHook == 0)
            {
                KeyboardHookProcedure = new GlobalHookProc(KeyboardHookProc);
                try
                {
                    var entryDll = Assembly.GetEntryAssembly();
                    Module[] entryModule = entryDll!.GetModules();
                    _hKeyboardHook = SetWindowsHookEx(WH_KEYBOARD_LL,
                        KeyboardHookProcedure,
                        Marshal.GetHINSTANCE(entryModule[0]),
                        0);
                }
                catch { }
            }
            if (hookType.HasFlag(HookType.Mouse) && _hMouseHook == 0)
            {
                MouseHookProcedure = new GlobalHookProc(MouseHookProc);
                try
                {
                    var entryDll = Assembly.GetEntryAssembly();
                    Module[] entryModule = entryDll!.GetModules();
                    _hMouseHook = SetWindowsHookEx(WH_MOUSE_LL,
                        MouseHookProcedure,
                        Marshal.GetHINSTANCE(entryModule[0]),
                        0);
                }
                catch { }
            }
            return true;
        }

        private volatile bool _isActive;
        public bool IsActive => Volatile.Read(ref _isActive);
        public void ResetActive() => _isActive = false;

        private DateTime _lastTime = DateTime.Now;
        private int MouseHookProc(int nCode, Int32 wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                var now = DateTime.Now;
                if (now - _lastTime > TimeSpan.FromMilliseconds(100))
                {
                    MouseMove?.Invoke(this, EventArgs.Empty);
                }
                _lastTime = now;
                _isActive = true;
            }
            return CallNextHookEx(_hMouseHook, nCode, wParam, lParam);
        }

        internal void Stop(HookType hookType)
        {
            bool retMouse = true;
            bool retKeyboard = true;
            if (_hKeyboardHook != 0 && hookType.HasFlag(HookType.Keyboard))
            {
                retKeyboard = UnhookWindowsHookEx(_hKeyboardHook);
                _hKeyboardHook = 0;
            }
            if (_hMouseHook != 0 && hookType.HasFlag(HookType.Mouse))
            {
                retMouse = UnhookWindowsHookEx(_hMouseHook);
                _hMouseHook = 0;
            }
        }

        internal void Stop(int hMouseHook, int hKeyboardHook)
        {
            if (hKeyboardHook != 0) UnhookWindowsHookEx(hKeyboardHook);
        }

        private const int WM_KEYDOWN = 0x100;
        private const int WM_KEYUP = 0x101;
        private const int WM_SYSKEYDOWN = 0x104;
        private const int WM_SYSKEYUP = 0x105;

        [DllImport("user32")]
        internal static extern int ToAscii(int uVirtKey, int uScanCode, byte[] lpbKeyState, byte[] lpwTransKey, int fuState);

        [DllImport("user32")]
        internal static extern int GetKeyboardState(byte[] pbKeyState);

        private int KeyboardHookProc(int nCode, Int32 wParam, IntPtr lParam)
        {
            if ((nCode >= 0) && (KeyDown != null || KeyUp != null))
            {
                KeyboardHookStruct MyKeyboardHookStruct =
                    (KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardHookStruct))!;
                var key = (byte)(MyKeyboardHookStruct.vkCode);
                if (KeyDown != null && (wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN))
                {
                    KeyDown(this, new KeyEventArgs(key));
                }
                if (KeyUp != null && (wParam == WM_KEYUP || wParam == WM_SYSKEYUP))
                {
                    KeyUp(this, new KeyEventArgs(key));
                }
                _isActive = true;
            }
            return CallNextHookEx(_hKeyboardHook, nCode, wParam, lParam);
        }
    }
}
