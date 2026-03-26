#region 作者和版权
/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       CommonTool
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.Tools.Tools
* 文 件 名:       CommonTool.cs
* 创建时间:       2022/9/14 10:23:46
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com 
* 唯一标识：      530db607-93e4-4067-a4ed-235905ae7b85
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/9/14 10:23:46
* 修 改 人:		  L05123
************************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Proxies;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.Tools
{
    public static class CommonTool
    {
        /// <summary>
        /// 获取类型
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        public static Type GetType<T>()
        {
            return GetType(typeof(T));
        }

        /// <summary>
        /// 获取类型
        /// </summary>
        /// <param name="type">类型</param>
        public static Type GetType(Type type)
        {
            return Nullable.GetUnderlyingType(type) ?? type;
        }

        private static long Second = 1000;
        public static long Minute = Second * 60;
        private static long Hour = Minute * 60;
        private static long Day = Hour * 24;

        /// <summary>
        /// 将ms转换成对的字符串时间
        /// </summary>
        /// <param name="ms">毫秒</param>
        /// <returns></returns>
        public static string GetTimeByMS(float ms, int decplace = 1)
        {
            StringBuilder sb = new StringBuilder();

            if (ms > Day)
            {
                sb.Append($"{(long)ms / Day}d ");
                ms = ms % Day;
            }

            if (ms > Hour)
            {
                sb.Append($"{(long)ms / Hour}h ");
                ms = ms % Hour;
            }
            if (ms > Minute)
            {
                sb.Append($"{(long)ms / Minute}m ");
                ms = ms % Minute;
            }
            if (ms > Second)
            {
                sb.Append($"{(long)ms / Second}s ");
                ms = ms % Second;
            }

            if (ms > 0)
            {
                if (sb.Length > 0)
                {
                    sb.Append($"{(int)ms}ms ");
                }
                else
                {
                    sb.Append($"{ms}ms ");
                }
            }

            if (sb.Length > 0)
            {
                sb.Remove(sb.Length - 1, 1);
            }

            return sb.ToString();
        }

        /// <summary>
        /// 换行符
        /// </summary>
        public static string Line => Environment.NewLine;


        /// <summary>
        /// 是否Linux操作系统
        /// </summary>
        public static bool IsLinux => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        /// <summary>
        /// 是否Windows操作系统
        /// </summary>
        public static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        /// <summary>
        /// 是否苹果操作系统
        /// </summary>
        public static bool IsOsx => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        /// <summary>
        /// 当前操作系统
        /// </summary>
        public static string System => IsWindows ? "Windows" : IsLinux ? "Linux" : IsOsx ? "OSX" : string.Empty;

        internal static ApplicationMode ApplicationMode = ApplicationMode.Real;
        private static string NowDate = "";

        /// <summary>
        /// 支持离线配置
        /// </summary>
        public static void ApplicatinExit()
        {
            // 支持离线验证
            if (ApplicationMode == ApplicationMode.Offline && DateTime.Now.ToString("yyyy-MM-dd") == NowDate)
            {
                return;
            }

            Process[] processes = Process.GetProcesses();
            foreach (var item in processes)
            {
                if (item.ProcessName.IndexOf("Luster") == 0)
                {
                    item.Kill();
                }
            }
        }

        /// <summary>
        /// 更新Offline
        /// </summary>
        /// <param name="nowDate"></param>
        public static void SetOffline(string mode, string nowDate)
        {
            if (Enum.TryParse<ApplicationMode>(mode, out var aMode))
            {
                ApplicationMode = aMode;
                NowDate = nowDate;
            }
        }
    }

    enum ApplicationMode
    {
        Offline,
        Real,
    }
}