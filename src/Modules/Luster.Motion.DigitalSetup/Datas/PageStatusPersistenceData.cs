using System;
using System.Collections.Generic;

namespace Luster.Motion.DigitalSetup.Datas
{
    /// <summary>
    /// 页面状态持久化数据
    /// </summary>
    public class PageStatusPersistenceData
    {
        /// <summary>页面状态缓存</summary>
        public Dictionary<string, string> StatusCache { get; set; } = new Dictionary<string, string>();

        /// <summary>最后保存时间</summary>
        public DateTime LastSavedTime { get; set; }
    }
}
