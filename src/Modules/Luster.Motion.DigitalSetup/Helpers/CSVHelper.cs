using Luster.Common.DataAccess.Factory;
using Luster.Common.DataStruct;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Motion.DigitalSetup.Helpers
{
    public  class CSVHelper
    {
        private IDBFactory _dBFactory;
        public CSVHelper(IDBFactory dBFactory)
        {
            _dBFactory = dBFactory;
        }

        public IEnumerable<T> GetAllDataNew<T>(int pageIndex, int perPageCount, out long totalCount) where T : class, new()
        {
            var path = _dBFactory.GetCsvDir();
            if (string.IsNullOrEmpty(path))
                throw new FriendlyException("CSV目录未配置，请检查！");

            var type = typeof(T);
            var fileName = Path.Combine(path,"Ass_Data", $"{type.Name}.csv");
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite && p.GetIndexParameters().Length == 0)
                .ToArray();

            // 构建 DisplayName 到 PropertyInfo 的映射
            var displayNameToProp = props.ToDictionary(
                p =>
                {
                    var displayAttr = p.GetCustomAttribute<System.ComponentModel.DataAnnotations.DisplayAttribute>();
                    return displayAttr != null && !string.IsNullOrEmpty(displayAttr.Name) ? displayAttr.Name : p.Name;
                },
                p => p
            );

            if (!File.Exists(fileName))
            {
                // 创建空表（仅写入表头）
                Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                using (var writer = new StreamWriter(fileName, false, Encoding.UTF8))
                {
                    writer.WriteLine(string.Join(",", displayNameToProp.Keys));
                    // 默认增加一行空数据
                    writer.WriteLine(string.Join(",", displayNameToProp.Keys.Select(_ => "")));
                }
                totalCount = 1;
                var defaultObj = new T();
                return new List<T> { defaultObj };
            }

            var result = new List<T>();
            using (var reader = new StreamReader(fileName))
            {
                var header = reader.ReadLine();
                if (header == null)
                {
                    // 文件存在但无表头，补写表头和一行空数据
                    using (var writer = new StreamWriter(fileName, false, Encoding.UTF8))
                    {
                        writer.WriteLine(string.Join(",", displayNameToProp.Keys));
                        writer.WriteLine(string.Join(",", displayNameToProp.Keys.Select(_ => "")));
                    }
                    totalCount = 1;
                    var defaultObj = new T();
                    return new List<T> { defaultObj };
                }
                var columns = header.Split(',');

                string line;
                bool hasData = false;
                while ((line = reader.ReadLine()) != null)
                {
                    hasData = true;
                    var values = ParseCsvLine(line, columns.Length);
                    var obj = new T();
                    for (int i = 0; i < columns.Length && i < values.Length; i++)
                    {
                        if (displayNameToProp.TryGetValue(columns[i], out var prop) && !string.IsNullOrEmpty(values[i]))
                        {
                            try
                            {
                                var val = Convert.ChangeType(values[i], Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType, CultureInfo.InvariantCulture);
                                prop.SetValue(obj, val);
                            }
                            catch { /* 忽略转换失败 */ }
                        }
                    }
                    result.Add(obj);
                }
                // 若没有数据行，则默认增加一行
                if (!hasData)
                {
                    reader.Close();
                    using (var writer = new StreamWriter(fileName, true, Encoding.UTF8))
                    {
                        writer.WriteLine(string.Join(",", displayNameToProp.Keys.Select(_ => "")));
                    }
                    var defaultObj = new T();
                    result.Add(defaultObj);
                }
            }
            totalCount = result.Count;
            return result;
        }

        // GetAllDataNew，去除默认增加空数据行的逻辑
        public IEnumerable<T> GetAllDataNew1<T>(int pageIndex, int perPageCount, out long totalCount) where T : class, new()
        {
            var path = _dBFactory.GetCsvDir();
            if (string.IsNullOrEmpty(path))
                throw new FriendlyException("CSV目录未配置，请检查！");

            var type = typeof(T);
            var fileName = Path.Combine(path, "Ass_Data", $"{type.Name}.csv");
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite && p.GetIndexParameters().Length == 0)
                .ToArray();

            // 构建 DisplayName 到 PropertyInfo 的映射
            var displayNameToProp = props.ToDictionary(
                p =>
                {
                    var displayAttr = p.GetCustomAttribute<System.ComponentModel.DataAnnotations.DisplayAttribute>();
                    return displayAttr != null && !string.IsNullOrEmpty(displayAttr.Name) ? displayAttr.Name : p.Name;
                },
                p => p
            );

            if (!File.Exists(fileName))
            {
                // 创建空表（仅写入表头）
                Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                using (var writer = new StreamWriter(fileName, false, Encoding.UTF8))
                {
                    writer.WriteLine(string.Join(",", displayNameToProp.Keys));
                }
                totalCount = 0;
                return new List<T>();
            }

            var result = new List<T>();
            using (var reader = new StreamReader(fileName))
            {
                var header = reader.ReadLine();
                if (header == null)
                {
                    // 文件存在但无表头，补写表头
                    using (var writer = new StreamWriter(fileName, false, Encoding.UTF8))
                    {
                        writer.WriteLine(string.Join(",", displayNameToProp.Keys));
                    }
                    totalCount = 0;
                    return new List<T>();
                }
                var columns = header.Split(',');

                string line;
                bool hasData = false;
                while ((line = reader.ReadLine()) != null)
                {
                    hasData = true;
                    var values = ParseCsvLine(line, columns.Length);
                    var obj = new T();
                    for (int i = 0; i < columns.Length && i < values.Length; i++)
                    {
                        if (displayNameToProp.TryGetValue(columns[i], out var prop) && !string.IsNullOrEmpty(values[i]))
                        {
                            try
                            {
                                var val = Convert.ChangeType(values[i], Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType, CultureInfo.InvariantCulture);
                                prop.SetValue(obj, val);
                            }
                            catch { /* 忽略转换失败 */ }
                        }
                    }
                    result.Add(obj);
                }
                // 若没有数据行，则不再增加一行空数据
            }
            totalCount = result.Count;
            return result;
        }


        // 简单CSV行解析（支持逗号和引号转义）
        private string[] ParseCsvLine(string line, int columnCount)
        {
            var values = new List<string>();
            bool inQuotes = false;
            var value = new StringBuilder();
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '\"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '\"')
                    {
                        value.Append('\"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    values.Add(value.ToString());
                    value.Clear();
                }
                else
                {
                    value.Append(c);
                }
            }
            values.Add(value.ToString());
            // 补齐列数
            while (values.Count < columnCount) values.Add("");
            return values.ToArray();
        }

        public int InsertOrUpdateNew<T>(IEnumerable<T> entities) where T : class
        {
            var path = _dBFactory.GetCsvDir();
            if (string.IsNullOrEmpty(path))
            {
                throw new FriendlyException("CSV目录未配置，请检查！");
            }

            var type = typeof(T);
            var fileName = System.IO.Path.Combine(path, "Ass_Data", $"{type.Name}.csv");

            try
            {
                var props = type.GetProperties()
                    .Where(p => p.CanRead && p.PropertyType.IsSerializable && p.GetIndexParameters().Length == 0)
                    .ToArray();

                // 获取DisplayName或属性名
                var headers = props.Select(p =>
                {
                    var displayAttr = p.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.DisplayAttribute), false)
                        .FirstOrDefault() as System.ComponentModel.DataAnnotations.DisplayAttribute;
                    return displayAttr != null && !string.IsNullOrEmpty(displayAttr.Name) ? displayAttr.Name : p.Name;
                }).ToArray();

                using (var writer = new System.IO.StreamWriter(fileName, false, Encoding.UTF8))
                {
                    // 写入表头（DisplayName）
                    writer.WriteLine(string.Join(",", headers));

                    // 转换为List以便判断数量和索引
                    var entityList = entities.ToList();
                    // 查找"项次"属性的索引（支持"项次"或"Index"），找不到默认为null
                    var indexProp = props.FirstOrDefault(p => p.Name.Equals("项次", StringComparison.OrdinalIgnoreCase));
                    // 写入数据
                    //foreach (var entity in entities)
                    for (int i=0; i < entityList.Count; i++)
                    {
                        var entity = entityList[i];
                        // 当csv中第一行数据的"项次"取值为空时，不保存
                        if (i == 0)
                        {
                            if (indexProp != null)
                            {
                                int indexPropIndex = Array.IndexOf(props, indexProp);
                                if (props.Length > indexPropIndex)
                                {
                                    var indexValue = props[indexPropIndex].GetValue(entity, null);
                                    if (indexValue == null)
                                    {
                                        continue;
                                    }
                                }
                            }
                        }
                        var values = props.Select(p =>
                        {
                            var val = p.GetValue(entity, null);
                            if (val == null) return "";
                            var str = val.ToString();
                            // 转义逗号和引号
                            if (str.Contains(",") || str.Contains("\""))
                            {
                                str = "\"" + str.Replace("\"", "\"\"") + "\"";
                            }
                            return str;
                        });
                        writer.WriteLine(string.Join(",", values));
                    }
                }
                return 1;
            }
            catch (Exception ex)
            {
                throw new FriendlyException($"实体{typeof(T).Name}批量写入CSV失败: {ex.Message}");
            }

        }

    }
}
