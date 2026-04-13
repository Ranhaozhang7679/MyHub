using System.Collections.Generic;

namespace Luster.Motion.DigitalSetup.Datas
{
    /// <summary>
    /// BUSOP 配置根模型
    /// </summary>
    public class BusopConfig
    {
        /// <summary>
        /// xlsx 文件路径（支持绝对路径或配方相对路径）
        /// </summary>
        public string ExcelFilePath { get; set; } = "";

        /// <summary>
        /// 子界面配置列表
        /// </summary>
        public List<BusopSubItemConfig> SubItems { get; set; } = new List<BusopSubItemConfig>();
    }

    /// <summary>
    /// 单个 BUSOP 子界面配置
    /// </summary>
    public class BusopSubItemConfig
    {
        /// <summary>
        /// 子界面名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 对应的 xlsx Sheet 页名称
        /// </summary>
        public string SheetName { get; set; } = "";
    }
}