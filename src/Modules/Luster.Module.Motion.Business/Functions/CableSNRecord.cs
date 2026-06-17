using System.ComponentModel;

namespace Luster.Module.Motion.Business.Functions
{
    /// <summary>
    /// 排线SN记录数据模型
    /// </summary>
    public class CableSNRecord
    {
        [DisplayName("时间")]
        public string Time { get; set; }

        [DisplayName("排线SN")]
        public string SN { get; set; }

        [DisplayName("是否使用")]
        public string IsUsed { get; set; }
    }
}
