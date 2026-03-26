using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luster.Common.DataStruct.Extensions
{
    public static class DictionaryExtension
    {
        public static string ToSafeString<T, K>(this Dictionary<T, K> data)
        {
            if (data == null || data.Count == 0) return string.Empty;

            var sb = new StringBuilder();
            foreach (var item in data)
            {
                sb.Append($"{item.Key}={item.Value},");
            }

            if (sb.Length > 0)
            {
                sb.Remove(sb.Length - 1, 1);
            }

            return sb.ToString();
        }
    }
}
