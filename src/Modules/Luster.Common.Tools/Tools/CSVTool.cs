#region 作者和版权

/*************************************************************************************
* CLR 版本:       4.0.30319.42000
* 类 名 称:       CSVTool
* 机器名称:       L05123-NB
* 命名空间:       Luster.Common.Tools
* 文 件 名:       CSVTool.cs
* 创建时间:       2022/2/9 10:10:39
* 作    者:       L05123
* 所属部门：      系统集成部
* 版    权:    	  <copyright company="凌云光工业">
* 签    名:       Luster Technology Co.,Ltd.
* 网    站:       https://www.lusterinc.com/
* 邮    箱:       darkliu@lusterinc.com
* 唯一标识：      37a1b01a-5345-4096-96b9-8a129aeabfd4
* 登录用户:       darkliu
* 所 属 域:       LUSTERINC
* 创建年份:       2022
* 修改时间:		  2022/2/9 10:10:39
* 修 改 人:		  L05123
************************************************************************************/

#endregion

using Luster.Common.DataStruct.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.Tools
{
    /// <summary>
    /// CSV 工具类
    /// </summary>
    public class CSVTool
    {
        /// <summary>
        /// 是否显示中文标题
        /// </summary>
        public static bool IsCN = true;

        /// <summary>
        /// 写入CSV文件，支持ExpandoObject
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="datas">List数据对象</param>
        /// <param name="fullFileName">文件全名</param>
        public static void SaveCSV(ICollection<ExpandoObject> datas, string fullFileName, Encoding encoding)
        {
            using (var fs = new FileStream(fullFileName, FileMode.Create, FileAccess.Write))
            {
                using (var sw = new StreamWriter(fs, encoding))
                {
                    // 构建标题信息
                    List<string> titles = new List<string>();
                    ExpandoObject first = datas.FirstOrDefault();
                    foreach (var item in first)
                    {
                        titles.Add(item.Key);
                    }

                    // 写出列名称
                    var tContent = string.Join(",", titles);
                    sw.WriteLine(tContent);
                    StringBuilder sb = new StringBuilder();
                    foreach (ExpandoObject exObj in datas)
                    {
                        IDictionary<string, object> data = exObj;
                        sb.Clear();

                        foreach (var item in titles)
                        {
                            var itemVal = data[item];
                            sb.Append($"{itemVal},");
                        }
                        sb.Remove(sb.Length - 1, 1);

                        sw.WriteLine(sb);
                    }
                }
            }
        }

        /// <summary>
        /// 写入CSV文件
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="data">List数据对象</param>
        /// <param name="fullFileName">文件全名</param>
        /// <param name="multiRow">多行表头</param>
        public static void SaveCSV<T>(ICollection<T> datas, string fullFileName, Encoding encoding, string dateFormat = "yyyy/MM/dd HH:mm:ss") where T : class
        {
            string DirectoryName = fullFileName.Substring(0, fullFileName.LastIndexOf('\\'));
            if (!Directory.Exists(DirectoryName))
            {
                Directory.CreateDirectory(DirectoryName);            
            }

            using (var fs = new FileStream(fullFileName, FileMode.Create, FileAccess.Write))
            {
                using (var sw = new StreamWriter(fs, encoding))
                {
                    SaveCSV(sw, datas, encoding, dateFormat);
                    sw.Close();
                }
                fs.Close();
            }
        }

        /// <summary>
        /// 每小时一个文件版
        /// 调用示例：
        /// CsvHelper.SaveCSVHourly(datas, folderName2, fileName, Encoding.UTF8, "yyyy-MM-dd HH:mm:ss.fff");
        /// </summary>
        public static void SaveCSVHourly(ICollection<ExpandoObject> datas, string Folder, string csvBaseName, Encoding encoding, string dateFormat = "yyyy/MM/dd HH:mm:ss")
        {
            if (datas == null || datas.Count == 0) return;

            var now = DateTime.Now;
            var fileName = $"{csvBaseName}_{now:HH}.csv";
            var fullPath = Path.Combine(Folder, fileName);
            // 确保目录存在
            //Directory.CreateDirectory(Folder);
            // 复用单文件逻辑
            SaveCSV(datas, fullPath, encoding, dateFormat);
        }

        /// <summary>
        /// 提供外部控制
        /// </summary>
        /// <typeparam name="T">泛型对象</typeparam>
        /// <param name="sw">写入对象</param>
        /// <param name="datas">数据对象</param>
        /// <param name="srcTitle">使用默认标题 false为DisplayName标题</param>
        public static void SaveCSV<T>(StreamWriter sw, ICollection<T> datas, Encoding encoding, string dateFormat = "yyyy/MM/dd HH:mm:ss") where T : class
        {
            var properties = GetProperties<T>();

            // 支持中文标题
            List<string> titles = new List<string>();
            List<string> srcTitle = new List<string>();

            foreach (var item in properties)
            {
                // 原始表头
                var disAttr = item.GetCustomAttribute<DisplayNameAttribute>();
                if (disAttr != null)
                {
                    titles.Add(disAttr.DisplayName);
                    srcTitle.Add(item.Name);
                }
                else
                {
                    // 支持字典类型
                    if (datas.Count > 0 && item.PropertyType == typeof(Dictionary<string, object>))
                    {
                        var fisrt = GetNotNullDictionary<T>(item, datas);
                        if (fisrt != null && fisrt is Dictionary<string, object> dict)
                        {
                            foreach (var dictKey in dict.Keys)
                            {
                                titles.Add(dictKey);
                                srcTitle.Add(dictKey);
                            }
                        }
                    }
                    else
                    {
                        titles.Add(item.Name);
                    }
                }
            }

            if (IsCN)
            {
                // 中文表头
                var tContent = string.Join(",", titles);
                sw.WriteLine(tContent);
            }
            else
            {
                // 原始表头
                sw.WriteLine(string.Join(",", srcTitle));
            }

            // 保存数据
            foreach (var item in datas)
            {
                AppendCSV<T>(sw, item, properties, dateFormat);
            }
        }

        private static object GetNotNullDictionary<T>(PropertyInfo propInfo, ICollection<T> datas)
        {
            foreach (var item in datas)
            {
                var rowVal = propInfo.GetValue(item);
                if (rowVal != null)
                {
                    return rowVal;
                }
            }

            return null;
        }

        /// <summary>
        /// 数据追加
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sw"></param>
        /// <param name="item"></param>
        public static void AppendCSV<T>(StreamWriter sw, T item, List<PropertyInfo> properties, string dateFormat = "yyyy/MM/dd HH:mm:ss")
        {
            StringBuilder sb = new StringBuilder();
            foreach (var pItem in properties)
            {
                var pVal = pItem.GetValue(item);

                if (pVal == null)
                {
                    sb.Append(",");
                }
                else if (pVal is Dictionary<string, object> dict)
                {
                    foreach (var kVal in dict)
                    {
                        if (kVal.Value is string s)
                        {
                            s = s.Replace("\r", "").Replace("\n", "");
                            sb.Append($"{s},");
                        }
                        else
                        {
                            sb.Append($"{kVal.Value},");
                        }
                    }
                }
                else
                {
                    if (pItem.PropertyType == typeof(DateTime))
                    {
                        if (string.IsNullOrEmpty(dateFormat))
                        {
                            sb.Append($"\t{pVal},");
                        }
                        else
                        {
                            var date = Convert.ToDateTime(pVal);
                            sb.Append($"{date.ToString(dateFormat)},");
                        }
                    }
                    else
                    {
                        if (pVal is string s)
                        {
                            sb.Append($"{s.Replace("\r", "").Replace("\n", "")},");
                        }
                        else
                        {
                            sb.Append($"{pVal},");
                        }
                    }
                }
            }

            sb.Remove(sb.Length - 1, 1);
            sw.WriteLine(sb.ToString());
        }

        /// <summary>
        /// 获取类型所有属性
        /// </summary>
        /// <typeparam name="T">待处理的对象类型</typeparam>
        /// <returns>目标对象所有属性集合</returns>
        public static List<PropertyInfo> GetProperties<T>()
        {
            var listProps = typeof(T).GetProperties()
                            .Where(u => u.GetCustomAttribute<IgnoreAttribute>() == null)
                            .ToList();

            // 对属性进行排序
            listProps.Sort(new PropSortCompare<PropertyInfo>());

            return listProps;
        }

        public static List<string> GetTitles<T>(out List<PropertyInfo> properties)
        {
            properties = GetProperties<T>();

            List<string> titles = new List<string>();

            foreach (var item in properties)
            {
                // 原始表头
                var disAttr = item.GetCustomAttribute<DisplayNameAttribute>();
                if (disAttr != null)
                {
                    titles.Add(disAttr.DisplayName);
                }
                else
                {
                    // 支持字典类型
                    titles.Add(item.Name);
                }
            }

            return titles;
        }
        /// <summary>
        /// 从CSV文件中读取数据并转化为目标类型的集合
        /// </summary>
        /// <typeparam name="T">目标对象类型</typeparam>
        /// <param name="fileName">文件名</param>
        /// <returns>获取的目标集合</returns>
        public static List<T> OpenCSV<T>(string fileName, int dataStartRow = 1) where T : class
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException("file:" + fileName + " is not exist!");

            // 读取标题，获取顺序
            var properties = GetProperties<T>();
            var list = new List<T>();
            using (var reader = new StreamReader(fileName, Encoding.UTF8))
            {
                // 读取标题
                string line = string.Empty;
                for (int i = 0; i < dataStartRow; i++)
                {
                    line = reader.ReadLine();
                }

                // 读取内容
                while ((line = reader.ReadLine()) != null)
                {
                    var data = line.Split(',');
                    var obj = Activator.CreateInstance<T>();
                    for (var i = 0; i < properties.Count; i++)
                    {
                        var vaule = Convert.ChangeType(data[i], properties[i].PropertyType);
                        properties[i].SetValue(obj, vaule);
                    }

                    list.Add(obj);
                }
            }
            return list;
        }

        public static int GetCSVColNum(string fileName, int dataStartRow = 1)
        {
            int num = 0;
            if (!File.Exists(fileName))
                throw new FileNotFoundException("file:" + fileName + " is not exist!");

            // 读取标题，获取顺序
            using (var reader = new StreamReader(fileName, Encoding.UTF8))
            {
                // 读取标题
                string line = string.Empty;
                for (int i = 0; i < dataStartRow; i++)
                {
                    line = reader.ReadLine();
                }

                // 读取内容
                while ((line = reader.ReadLine()) != null)
                {
                    var data = line.Split(',');
                    num = data.Length;
                }
                reader.Close();
            }
            return num;
        }

    }
}