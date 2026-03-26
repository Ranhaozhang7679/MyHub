#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       StringExtension
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.Extensions
* 文 件 名:       StringExtension.cs
* 创建时间:       2021/12/17 8:53:08
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      df7c8d99-5a32-499f-97e4-89d929285b39
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2021
* 修改时间:		  2021/12/17 8:53:08
* 修 改 人:		  L05123
************************************************************************************/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Reflection;
using System.IO;

namespace Luster.Common.DataStruct.Extensions
{
    public static class StringExtension
    {
        /// <summary>
        /// 将字符串转换为指定类型的枚举值
        /// </summary>
        /// <typeparam name="T">枚举变量类型</typeparam>
        /// <param name="str"></param>
        /// <returns></returns>
        public static T ToEnum<T>(this string str)
        {
            bool result = true;
            return str.ToEnum<T>(out result);
        }

        /// <summary>
        ///  将字符串转换为指定类型的枚举值
        /// </summary>
        /// <typeparam name="T">枚举变量类型</typeparam>
        /// <param name="str"></param>
        /// <param name="isOk">转换结果</param>
        /// <returns></returns>
        public static T ToEnum<T>(this string str, out bool isOk)
        {
            if (Enum.IsDefined(typeof(T), str))
            {
                isOk = true;
                return (T)Enum.Parse(typeof(T), str);
            }
            else
            {
                isOk = false;
                return default(T);
            }
        }

        /// <summary>
        /// 通过字符对字符串进行功能拆分
        /// </summary>
        /// <typeparam name="T">要拆分的类型</typeparam>
        /// <param name="str">要拆分的字符串</param>
        /// <param name="spitChars">对应的字符</param>
        /// <returns>拆分后的数组</returns>
        public static T[] Split<T>(this string str, params char[] spitChars) where T : struct
        {
            List<T> retArr = new List<T>();
            string[] splitStrs = str.Split(spitChars);
            foreach (var item in splitStrs)
            {
                if (string.IsNullOrEmpty(item)) continue;
                retArr.Add((T)Convert.ChangeType(item, typeof(T)));
            }

            return retArr.ToArray();
        }

        /// <summary>
        /// 通过字符对字符串进行功能拆分
        /// </summary>
        /// <typeparam name="T">要拆分的类型</typeparam>
        /// <param name="str">要拆分的字符串</param>
        /// <param name="spitChars">对应的字符</param>
        /// <returns>拆分后的数组</returns>
        public static string[] Split(this string str, params char[] spitChars)
        {
            List<string> retArr = new List<string>();
            string[] splitStrs = str.Split(spitChars);
            foreach (var item in splitStrs)
            {
                retArr.Add(item);
            }

            return retArr.ToArray();
        }

        /// <summary>
        /// 字符串中多个连续空格转为一个空格
        /// </summary>
        /// <param name="str">待处理的字符串</param>
        /// <returns>合并空格后的字符串</returns>
        public static string MergeSpace(this string str)
        {
            if ((!string.IsNullOrEmpty(str)) && (str.Length > 0))
            {
                str = new System.Text.RegularExpressions.Regex("[\\s]+").Replace(str, " ");
            }
            return str;
        }

        private static readonly ConcurrentDictionary<string, Type> _typeCache = new ConcurrentDictionary<string, Type>(StringComparer.Ordinal);

        /// <summary>
        /// 转换为类型（避免触发递归 AssemblyResolve）
        /// </summary>
        /// <param name="typeStr">类型名，支持完全限定名或程序集限定名。若仅为简单名，将在限定集合内扫描。</param>
        /// <returns></returns>
        public static Type ConvertToType(this string typeStr)
        {
            if (string.IsNullOrWhiteSpace(typeStr)) return null;

            if (_typeCache.TryGetValue(typeStr, out var cached))
            {
                return cached;
            }

            // 1) 优先尝试 Type.GetType（支持完全限定名/程序集限定名，不触发全量枚举）
            var t = Type.GetType(typeStr, false, true);
            if (t != null) return _typeCache[typeStr] = t;

            // 2) 若包含命名空间，优先在已加载程序集精确查找（不枚举所有类型）
            if (typeStr.Contains("."))
            {
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (!IsSafeAssembly(asm)) continue;
                    t = SafeGetType(asm, typeStr);
                    if (t != null) return _typeCache[typeStr] = t;
                }
            }

            // 3) 需要按简单名匹配时，仅在安全的、应用目录内的程序集里做“受控枚举”
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies().Where(IsSafeAssembly))
            {
                foreach (var at in SafeGetTypes(asm))
                {
                    if (at != null && string.Equals(at.Name, typeStr, StringComparison.Ordinal))
                    {
                        return _typeCache[typeStr] = at;
                    }
                }
            }

            return null;
        }

        private static bool IsSafeAssembly(Assembly assembly)
        {
            try
            {
                if (assembly == null) return false;
                if (assembly.IsDynamic) return false;
                // 避免扫描 GAC 程序集，减少依赖解析
                bool inGac = false;
                try { inGac = assembly.GlobalAssemblyCache; } catch { }
                if (inGac) return false;

                // 跳过资源与 Costura 自身程序集，避免触发递归解析
                var name = assembly.GetName().Name;
                if (string.Equals(name, "Luster.Common.Assets", StringComparison.OrdinalIgnoreCase)) return false;
                var modName = assembly.ManifestModule?.Name;
                if (!string.IsNullOrEmpty(modName) && modName.EndsWith(".resources.dll", StringComparison.OrdinalIgnoreCase)) return false;

                // 优先仅扫描应用目录内的程序集
                string location = null;
                try { location = assembly.Location; } catch { }
                if (string.IsNullOrEmpty(location)) return true;

                var baseDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                return location.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private static Type SafeGetType(Assembly asm, string fullName)
        {
            try
            {
                return asm.GetType(fullName, false, true);
            }
            catch
            {
                return null;
            }
        }

        private static IEnumerable<Type> SafeGetTypes(Assembly asm)
        {
            try
            {
                return asm.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(x => x != null);
            }
            catch
            {
                return Array.Empty<Type>();
            }
        }

        /// <summary>
        /// 安全转换为字符串，去除两端空格，当值为null时返回""
        /// </summary>
        /// <param name="input">输入值</param>
        public static string SafeString(this object input)
        {
            return input?.ToString().Trim() ?? string.Empty;
        }

        /// <summary>
        /// 校验IP地址的正确性，同时支持IPv4和IPv6
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <param name="isMatch">是否匹配成功，若返回true，则会得到一个Match对象，否则为null</param>
        /// <returns>匹配对象</returns>
        public static IPAddress MatchInetAddress(this string s, out bool isMatch)
        {
            isMatch = IPAddress.TryParse(s, out var ip);
            return ip;
        }

        /// <summary>
        /// 校验IP地址的正确性，同时支持IPv4和IPv6
        /// </summary>
        /// <param name="s">源字符串</param>
        /// <returns>是否匹配成功</returns>
        public static bool MatchInetAddress(this string s)
        {
            MatchInetAddress(s, out var success);
            return success;
        }

        /// <summary>
        /// IP地址转换成数字
        /// </summary>
        /// <param name="addr">IP地址</param>
        /// <returns>数字,输入无效IP地址返回0</returns>
        public static uint IPToID(this string addr)
        {
            if (!IPAddress.TryParse(addr, out var ip))
            {
                return 0;
            }

            byte[] bInt = ip.GetAddressBytes();
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bInt);
            }

            return BitConverter.ToUInt32(bInt, 0);
        }

        /// <summary>
        /// 判断IP地址在不在某个IP地址段
        /// </summary>
        /// <param name="input">需要判断的IP地址</param>
        /// <param name="begin">起始地址</param>
        /// <param name="ends">结束地址</param>
        /// <returns></returns>
        public static bool IpAddressInRange(this string input, string begin, string ends)
        {
            uint current = input.IPToID();
            return current >= begin.IPToID() && current <= ends.IPToID();
        }
    }
}