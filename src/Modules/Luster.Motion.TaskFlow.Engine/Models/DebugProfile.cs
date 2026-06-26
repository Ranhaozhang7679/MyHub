using Luster.Common.DataStruct.Extensions;
using Luster.Common.DataStruct.Interfaces;
using System.Xml.Linq;

namespace Luster.Motion.TaskFlow.Engine.Models
{
    /// <summary>
    /// 调机模式参数表（TES-34 P9-A，迁移自源端 SP-2025140 Plugin.CommonPlugin\Model\Settings\DebugProfile.cs）。
    /// 对齐源端 7 个开关：SingleMode/RunWithProduct/RunWithICW/IsCalibritionMode/IsCalibritionSave/
    /// EnableHandShakeSafe/LoadMCEnable。挂载于 <see cref="SystemConfig.DebugSetting"/>，
    /// 与 RunMode/EnableSaftyDoor 等同属软件配置区（IXMLParser 自动持久化）。
    /// </summary>
    /// <remarks>
    /// <b>源端语义</b>：DebugProfile 仅在“调机模式”下被流程层读取（源端 CheckDebugAction 重写 4 个谓词
    /// RunWithProduct/RunWithICW/IsCalibritionMode/IsCalibritionSave）。生产模式下这些值为硬编码
    /// (true/true/false/false)，不读本表。模式本身（生产/调机/空跑）属运行时状态，不在本表。
    /// <b>死字段</b>：EnableHandShakeSafe/LoadMCEnable 在源端零读取点（仅 ctor/CopyFrom 赋值），
    /// 此处保留以维持配置兼容，标 Legacy 注释，不接逻辑。
    /// <b>非侵入</b>：新增独立配置模型，不改既有 SystemConfig 字段语义。
    /// </remarks>
    public class DebugProfile : IXMLParser
    {
        /// <summary>单机模式：true 时单站采图，ICW BG 面检测走手动触发(RequestCheckMode=2)</summary>
        public bool SingleMode { get; set; } = true;

        /// <summary>是否带产品：false 时跳过人工上下料真空吸/破及 ManualFeed/ManualLeave 等待</summary>
        public bool RunWithProduct { get; set; }

        /// <summary>是否带 ICW：false 时跳过 ICW 入料/检测全部异步交互</summary>
        public bool RunWithICW { get; set; }

        /// <summary>校准模式：true 时累积锁存偏差 AddLatchedOffset()</summary>
        public bool IsCalibritionMode { get; set; }

        /// <summary>校准模式后保存数据：true 时一轮完成触发 Settings.Save() 持久化</summary>
        public bool IsCalibritionSave { get; set; }

        /// <summary>[Legacy 死字段] 轴互锁中启用上下游互锁信号。源端零读取点，保留兼容不接逻辑</summary>
        public bool EnableHandShakeSafe { get; set; }

        /// <summary>[Legacy 死字段] 强制上料站为 MC 协议服务器。源端零读取点，保留兼容不接逻辑</summary>
        public bool LoadMCEnable { get; set; }

        public DebugProfile() { }

        public DebugProfile(DebugProfile other)
        {
            if (other != null) CopyFrom(other);
        }

        /// <summary>逐字段复制（对齐源端 CopyFrom）</summary>
        public void CopyFrom(DebugProfile other)
        {
            if (other == null) return;
            SingleMode = other.SingleMode;
            RunWithProduct = other.RunWithProduct;
            RunWithICW = other.RunWithICW;
            IsCalibritionMode = other.IsCalibritionMode;
            IsCalibritionSave = other.IsCalibritionSave;
            EnableHandShakeSafe = other.EnableHandShakeSafe;
            LoadMCEnable = other.LoadMCEnable;
        }

        /// <summary>导出 XML（bool 字段经 ToXml 自动序列化）</summary>
        public XElement ExportXml()
        {
            return this.ToXml();
        }

        /// <summary>解析 XML（bool 字段经 FromXml 自动反序列化）</summary>
        public void ParserXml(XElement xElement)
        {
            this.FromXml(xElement);
        }
    }
}
