#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       ReflectionUtility
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.Utility
* 文 件 名:       ReflectionUtility.cs
* 创建时间:       2022/1/4 16:08:39
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      0666d88e-8978-480e-ae73-72c219f6d8f8
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/1/4 16:08:39
* 修 改 人:		  L05123
************************************************************************************/

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.Tools
{
    public class ReflectionTool
    {
        public static List<Type> GetTypesByAssembly<T>(Assembly assembly) where T : class
        {
            List<Type> retTypes = new List<Type>();
            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                if (type.IsGenericType || type.IsAbstract) continue;

                if (typeof(T).IsAssignableFrom(type))
                {
                    retTypes.Add(type);
                }
            }

            return retTypes;
        }

        /// <summary>
        /// 获取当前程序集所在路径
        /// </summary>
        /// <returns>程序集所在路径</returns>
        public static string GetAssemblyPath(Assembly assembly = null)
        {
            if (assembly == null)
            {
                assembly = Assembly.GetExecutingAssembly();
            }
            string codeBase = assembly.CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }

        /// <summary>
        /// 解析程序集
        /// </summary>
        /// <typeparam name="T">要解析的对象</typeparam>
        /// <param name="assemblyName">assemblyName</param>
        /// <returns>T</returns>
        public static List<T> GetByAssembly<T>(string assemblyName) where T : class
        {
            Assembly assembly = Assembly.LoadFrom(assemblyName);

            return GetByAssembly<T>(assembly);
        }

        /// <summary>
        /// 通过程序集获取
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static List<T> GetByAssembly<T>(Assembly assembly) where T : class
        {
            List<T> ts = new List<T>();
            foreach (var item in assembly.GetTypes())
            {
                // 1.忽略抽象类
                if (item.IsAbstract || item.IsInterface) continue;

                // 2.类继承
                if (typeof(T).IsAssignableFrom(item))
                {
                    ts.Add(Activator.CreateInstance(item) as T);
                }
            }

            return ts;
        }
    }
}