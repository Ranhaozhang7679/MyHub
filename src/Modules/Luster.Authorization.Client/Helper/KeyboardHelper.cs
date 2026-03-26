using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Runtime.InteropServices;

namespace Luster.Authorization.Client.Helper
{

    public static class KeyboardHelper
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [StructLayout(LayoutKind.Sequential)]
        struct INPUT
        {
            public uint type;
            public KEYBDINPUT ki;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct KEYBDINPUT
        {
            public ushort wVk;      // 虚拟键码
            public ushort wScan;    // 扫描码
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        const uint INPUT_KEYBOARD = 1;
        const uint KEYEVENTF_KEYUP = 0x0002;
        const uint KEYEVENTF_SCANCODE = 0x0008;  // 使用扫描码

        // 虚拟键码
        const ushort VK_RETURN = 0x0D;
        const ushort VK_SHIFT = 0x10;

        /// <summary>
        /// 模拟键盘输入（使用虚拟键码，能被 LL 钩子捕获）
        /// </summary>
        public static void SimulateTyping(string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            var inputs = new List<INPUT>();

            foreach (char c in text)
            {
                // 获取虚拟键码和是否需要 Shift
                var (vkCode, needShift) = GetVirtualKeyCode(c);

                // 如果需要 Shift，先按下 Shift
                if (needShift)
                {
                    inputs.Add(CreateKeyInput(VK_SHIFT, false));
                }

                // 按键按下
                inputs.Add(CreateKeyInput(vkCode, false));

                // 按键释放
                inputs.Add(CreateKeyInput(vkCode, true));

                // 释放 Shift
                if (needShift)
                {
                    inputs.Add(CreateKeyInput(VK_SHIFT, true));
                }
            }

            // Enter 键
            inputs.Add(CreateKeyInput(VK_RETURN, false));
            inputs.Add(CreateKeyInput(VK_RETURN, true));

            SendInput((uint)inputs.Count, inputs.ToArray(), Marshal.SizeOf<INPUT>());
        }

        private static INPUT CreateKeyInput(ushort vkCode, bool keyUp)
        {
            return new INPUT
            {
                type = INPUT_KEYBOARD,
                ki = new KEYBDINPUT
                {
                    wVk = vkCode,
                    dwFlags = keyUp ? KEYEVENTF_KEYUP : 0
                }
            };
        }

        /// <summary>
        /// 字符转虚拟键码（简化版，支持基本英文字符和符号）
        /// </summary>
        private static (ushort vkCode, bool needShift) GetVirtualKeyCode(char c)
        {
            // 数字和符号（主键盘）
            if (c >= '0' && c <= '9')
            {
                return ((ushort)('0' + (c - '0')), false);
            }

            // 大写字母
            if (c >= 'A' && c <= 'Z')
            {
                return ((ushort)('A' + (c - 'A')), true); // 需要 Shift
            }

            // 小写字母
            if (c >= 'a' && c <= 'z')
            {
                return ((ushort)('A' + (c - 'a')), false);
            }

            // 空格
            if (c == ' ') return (0x20, false);

            // 常用符号（需要 Shift 的）
            var shiftSymbols = new Dictionary<char, ushort>
            {
                ['!'] = 0x31,
                ['@'] = 0x32,
                ['#'] = 0x33,
                ['$'] = 0x34,
                ['%'] = 0x35,
                ['^'] = 0x36,
                ['&'] = 0x37,
                ['*'] = 0x38,
                ['('] = 0x39,
                [')'] = 0x30,
                ['_'] = 0xBD,
                ['+'] = 0xBB,
                ['{'] = 0xDB,
                ['}'] = 0xDD,
                ['|'] = 0xDC,
                [':'] = 0xBA,
                ['"'] = 0xDE,
                ['<'] = 0xBC,
                ['>'] = 0xBE,
                ['?'] = 0xBF
            };

            if (shiftSymbols.TryGetValue(c, out var vkShift))
            {
                return (vkShift, true);
            }

            // 不需要 Shift 的符号
            var normalSymbols = new Dictionary<char, ushort>
            {
                ['-'] = 0xBD,
                ['='] = 0xBB,
                ['['] = 0xDB,
                [']'] = 0xDD,
                ['\\'] = 0xDC,
                [';'] = 0xBA,
                ['\''] = 0xDE,
                [','] = 0xBC,
                ['.'] = 0xBE,
                ['/'] = 0xBF,
                ['`'] = 0xC0
            };

            if (normalSymbols.TryGetValue(c, out var vkNormal))
            {
                return (vkNormal, false);
            }

            // 默认返回原字符的虚拟键码
            return ((ushort)c, false);
        }
    }
}
