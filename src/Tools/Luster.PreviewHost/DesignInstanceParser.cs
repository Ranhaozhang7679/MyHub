using System;
using System.Text.RegularExpressions;

namespace Luster.PreviewHost
{
    /// <summary>从源 XAML 解析 d:DesignInstance 设计时实例类型</summary>
    public sealed class DesignInstanceInfo
    {
        public string TypeName { get; set; }
        public bool IsDesignDataCreatable { get; set; } = true;
    }

    public static class DesignInstanceParser
    {
        // 匹配 d:DesignInstance="ClrNs.Type" 或 d:DesignInstance="{Type ClrNs.Type}"
        private static readonly Regex Pattern = new Regex(
            @"d:DesignInstance\s*=\s*\""(?:\{Type\s+)?(?<type>[\w.]+)(?:\})?\""",
            RegexOptions.Compiled);

        /// <summary>从 XAML 文本解析 d:DesignInstance;无则返回 null</summary>
        public static DesignInstanceInfo Parse(string xaml)
        {
            if (string.IsNullOrEmpty(xaml)) return null;
            var m = Pattern.Match(xaml);
            if (!m.Success) return null;
            return new DesignInstanceInfo
            {
                TypeName = m.Groups["type"].Value,
                IsDesignDataCreatable = true
            };
        }
    }
}
