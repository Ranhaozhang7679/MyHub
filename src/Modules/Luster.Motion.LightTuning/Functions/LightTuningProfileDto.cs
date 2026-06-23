using System.Collections.Generic;
using System.Xml.Serialization;

namespace Luster.Motion.LightTuning.Functions
{
    /// <summary>
    /// 光调配置持久化 DTO（保存/加载 XML）。
    /// 与 <see cref="LightTuningParam"/>（ParamGrid 数据契约）一一映射，但用纯 POCO，
    /// 避免序列化 <c>OverTimeFunction</c> 基类的引擎状态（非侵入 + 可还原）。
    /// 对齐源端持久化路径：源端走 recipe XML（<c>PluginComponent.Instance().Settings.Save()</c>，
    /// <c>FormSTLs.cs:1281</c>）+ 算子 CSV 逻辑表（<c>FormSTLs.cs:3573 Export()</c>）；
    /// 目标端 P8-A 配方体系未就位前，先用独立 XML 落盘，P8-A 就位后切换到平台配方。
    /// </summary>
    [XmlRoot("LightTuningProfile")]
    public class LightTuningProfileDto
    {
        public int LightGroup { get; set; } = 1;
        public int TriggerMode { get; set; } = 0;
        public int ChannelCount { get; set; } = 8;

        public string ChanelR { get; set; } = "1,6,8,12,14";
        public string ChanelG { get; set; } = "2,5,7,9,11,13,15";
        public string ChanelB { get; set; } = "3,4,10";
        public string ChanelMono { get; set; } = "16";

        public int GrayTargetMono { get; set; } = 120;
        public int BelongScreen { get; set; } = 0;
        public string LinkEnable { get; set; } = "0";
        public string LinkIntervalTime { get; set; } = "255";

        /// <summary>每通道脉宽/延时表</summary>
        [XmlArray("Channels")]
        [XmlArrayItem("Channel")]
        public List<LightChannelDto> Channels { get; set; } = new List<LightChannelDto>();
    }

    /// <summary>单通道持久化项</summary>
    public class LightChannelDto
    {
        [XmlAttribute("Index")]
        public int Channel { get; set; }
        public int Delay { get; set; } = 80;
        public int Width { get; set; } = 50;
    }
}
