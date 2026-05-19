using System.ComponentModel;

namespace Luster.SimDevice.EngineUI.Models
{
    /// <summary>
    /// PLC Bool IO 导出数据模型
    /// </summary>
    public class ExportPlcIOModel
    {
        [DisplayName("序号")]
        public int Index { get; set; }

        [DisplayName("地址")]
        public string Address { get; set; }

        [DisplayName("名称")]
        public string Name { get; set; }

        [DisplayName("类型(0输入1输出)")]
        public int Type { get; set; }
    }
}
