using System;
using System.Collections.Generic;

namespace Luster.Motion.DigitalSetup.Datas
{
    /// <summary>
    /// 页面启用设置数据模型
    /// </summary>
    public class PageEnableSettings
    {
        /// <summary>
        /// 一级页面设置列表
        /// </summary>
        public List<PageEnableItem> PageSettings { get; set; } = new List<PageEnableItem>();

        /// <summary>
        /// 最后保存时间
        /// </summary>
        public DateTime LastSavedTime { get; set; }
    }

    /// <summary>
    /// 页面启用项数据模型
    /// </summary>
    public class PageEnableItem
    {
        /// <summary>
        /// 页面名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 页面区域名称
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 子页面设置列表（二级按钮）
        /// </summary>
        public List<SubPageEnableItem> SubPages { get; set; } = new List<SubPageEnableItem>();
    }

    /// <summary>
    /// 子页面启用项数据模型（二级按钮）
    /// </summary>
    public class SubPageEnableItem
    {
        /// <summary>
        /// 子页面名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 子页面区域名称
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; } = true;
    }
}
