using System.Collections.Generic;

namespace Luster.Module.Motion.AOI.Core.Models
{
    /// <summary>
    /// <see cref="IAoiAxisMap"/> 的可写 POCO 实现，由 XML 解析器或测试构造。
    /// </summary>
    public sealed class AoiAxisMap : IAoiAxisMap
    {
        public string XAxisName { get; set; } = string.Empty;
        public string YAxisName { get; set; } = string.Empty;
        public string ZAxisName { get; set; } = string.Empty;
        public string UAxisName { get; set; } = string.Empty;
        public string VAxisName { get; set; } = string.Empty;

        public Dictionary<string, int> Channels { get; } = new Dictionary<string, int>();

        IReadOnlyDictionary<string, int> IAoiAxisMap.AxisChannelMap => Channels;
    }
}
