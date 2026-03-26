using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace DC.Authorization.WPF.Infrastructure
{
    /// <summary>
    /// 工具类：前台窗口判断 + 属性比较
    /// </summary>
    internal class Utility
    {
        [DllImport("User32.dll")]
        internal static extern IntPtr GetForegroundWindow();

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        internal static extern int GetWindowThreadProcessId(IntPtr hwnd, out int ID);

        /// <summary>判断当前进程是否在前台</summary>
        public static bool IsForeground()
        {
            string myProcessFileName = "";
            string ProcessFileName = "";
            try
            {
                IntPtr hWnd = GetForegroundWindow();
                GetWindowThreadProcessId(hWnd, out int calcID);
                Process myProcess = Process.GetProcessById(calcID);
                myProcessFileName = myProcess.MainModule?.FileName ?? "";
            }
            catch
            {
                // 32位进程无法访问64位进程的模块
            }
            ProcessFileName = Process.GetCurrentProcess().MainModule?.FileName ?? "";
            return myProcessFileName == ProcessFileName;
        }

        /// <summary>比较两个对象的属性差异</summary>
        public static List<PropDiff> CompareProperties<T>(T before, T after)
        {
            var res = new List<PropDiff>();
            foreach (var prop in typeof(T).GetProperties())
            {
                var beforeVal = prop.GetValue(before);
                var afterVal = prop.GetValue(after);
                if (beforeVal != null && !beforeVal.Equals(afterVal))
                {
                    var descpAttr = prop.GetCustomAttributes(typeof(DescriptionAttribute), false)
                        .FirstOrDefault() as DescriptionAttribute;
                    res.Add(new PropDiff
                    {
                        PropDesc = descpAttr?.Description ?? prop.Name,
                        Before = beforeVal,
                        After = afterVal,
                    });
                }
            }
            return res;
        }
    }

    internal class PropDiff
    {
        public string PropDesc { get; set; } = string.Empty;
        public object Before { get; set; } = null!;
        public object After { get; set; } = null!;
    }
}
